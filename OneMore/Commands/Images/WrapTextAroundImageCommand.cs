//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Emulates square text wrapping for the selected left or right aligned image.
	/// </summary>
	/// <remarks>
	/// The image is moved into a one-row, two-column borderless table. The image column is sized
	/// tight to the image and the other column receives as many of the following paragraphs as
	/// will fit within the height of the image. Only whole paragraphs are moved.
	/// </remarks>
	internal class WrapTextAroundImageCommand : Command
	{
		/// <summary>
		/// Measured height of a candidate paragraph
		/// </summary>
		internal readonly struct Measure
		{
			public Measure(double height, bool isBlank, double lineHeight = 0)
			{
				Height = height;
				IsBlank = isBlank;
				LineHeight = lineHeight;
			}

			public double Height { get; }
			public bool IsBlank { get; }
			public double LineHeight { get; }
		}


		private enum Outcome
		{
			Wrapped,
			NoText,
			Rejected
		}


		// extra space between the image and the text, in points
		private const double Gutter = 12;

		// approximate left+right cell padding, in points
		private const double CellPadding = 8;

		// never make the text column narrower than this, in points
		private const double MinTextWidth = 72;

		// Fallback estimation constants for 11pt text, in points, used only if GDI measuring
		// fails; these mirror the values used by AddAnnotationCommand. Normally the line height
		// comes from the font itself since AddAnnotationCommand's constants overestimated.
		private const double FallbackLineHeight = 14.4;
		private const double AverageCharWidth = 5.5;
		private const double IndentWidth = 18.0;
		private const double BaselineFontSize = 11.0;

		private const double PixelsToPoints = 72.0 / 96.0;

		private const double DefaultOutlineWidth = 300;
		private const double DefaultFontSize = 11;
		private const string DefaultFontFamily = "Calibri";

		private XNamespace ns;


		public WrapTextAroundImageCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var guard = EnterOnce();
			if (guard is null)
			{
				return;
			}

			await using var one = new OneNote(out var page, out ns, OneNote.PageDetail.All);

			var images = page.Root.Descendants(ns + "Image")
				.Where(e => e.Attribute("selected")?.Value == "all")
				.ToList();

			if (images.Count == 0)
			{
				ShowError(Resx.Error_SelectImage);
				return;
			}

			// each image only claims the text between it and the next non-text paragraph, so
			// several selected images never compete for the same paragraphs
			var wrapped = 0;
			string message = null;
			var outcome = Outcome.Rejected;

			foreach (var image in images)
			{
				var result = Wrap(page, image, out var reason);
				if (result == Outcome.Wrapped)
				{
					wrapped++;
				}
				else
				{
					// remember the last failure to report if nothing at all was wrapped
					outcome = result;
					message = reason;
					logger.Verbose($"wrap skipped an image: {reason}");
				}
			}

			if (wrapped == 0)
			{
				if (outcome == Outcome.NoText)
				{
					ShowInfo(message);
				}
				else
				{
					ShowError(message);
				}

				return;
			}

			await one.Update(page);
		}


		// Wraps text around one image; leaves the page untouched unless it returns Wrapped
		private Outcome Wrap(Page page, XElement image, out string message)
		{
			message = null;

			if (image.Parent.Name.LocalName == "Page")
			{
				message = Resx.WrapTextAroundImageCommand_background;
				return Outcome.Rejected;
			}

			var imageOE = image.Parent;
			if (imageOE.Name.LocalName != "OE" ||
				imageOE.Ancestors(ns + "Cell").Any() ||
				!IsImageOnly(imageOE, image))
			{
				message = Resx.WrapTextAroundImageCommand_alreadyWrapped;
				return Outcome.Rejected;
			}

			var alignment = imageOE.Attribute("alignment")?.Value;
			if (alignment == "center")
			{
				message = Resx.WrapTextAroundImageCommand_centered;
				return Outcome.Rejected;
			}

			var isRight = alignment == "right";

			var oneImage = new OneImage(image);
			var sizeIsValid = oneImage.Width > 0 && oneImage.Height > 0;
			GetImageSize(oneImage, image, out var imageWidth, out var imageHeight);

			var outlineWidth = GetOutlineWidth(image);
			var imageColumn = imageWidth + Gutter;
			var textColumn = Math.Max(outlineWidth - imageColumn, MinTextWidth);

			var candidates = CollectCandidates(imageOE);
			var measures = MeasureCandidates(page, candidates, textColumn - CellPadding);
			// the text may end up to one line below the bottom of the image; that looks better
			// than leaving a gap under the image, and the estimate is slightly conservative
			var tolerance = 0.0;
			foreach (var measure in measures)
			{
				if (!measure.IsBlank)
				{
					tolerance = measure.LineHeight;
					break;
				}
			}

			var count = CountThatFit(measures, imageHeight, out var skip, tolerance);
			logger.Verbose(
				$"wrap image={imageHeight} candidates={candidates.Count} skip={skip} count={count} " +
				$"outlineWidth={outlineWidth} imageColumn={imageColumn} textColumn={textColumn}");

			if (count == 0)
			{
				message = Resx.WrapTextAroundImageCommand_noText;
				return Outcome.NoText;
			}

			if (!sizeIsValid && imageWidth > 0 && imageHeight > 0)
			{
				// A zero height leaves a stale width that OneNote still uses as the minimum
				// column width, so make the size match what is actually displayed
				oneImage.SetSize(
					(int)Math.Round(imageWidth), (int)Math.Round(imageHeight), true);
			}

			// leading blank lines that were ignored are discarded; the table provides the spacing
			var discarding = candidates.Take(skip).ToList();
			var moving = candidates.Skip(skip).Take(count).ToList();

			image.Attribute("selected")?.Remove();

			var table = MakeTable(imageColumn, textColumn, isRight, out var imageCell, out var textCell);

			imageOE.ReplaceWith(new XElement(ns + "OE", table.Root));
			foreach (var element in discarding.Concat(moving))
			{
				element.Remove();
			}

			imageCell.SetContent(new XElement(ns + "OEChildren", imageOE));
			textCell.SetContent(new XElement(ns + "OEChildren", moving));

			return Outcome.Wrapped;
		}


		private Table MakeTable(
			double imageColumn, double textColumn, bool isRight,
			out TableCell imageCell, out TableCell textCell)
		{
			var table = new Table(ns) { BordersVisible = false };

			// right aligned image: [text | image], otherwise [image | text]
			if (isRight)
			{
				table.AddColumn((float)textColumn, true);
				table.AddColumn((float)imageColumn, true);
			}
			else
			{
				table.AddColumn((float)imageColumn, true);
				table.AddColumn((float)textColumn, true);
			}

			var cells = table.AddRow().Cells.ToList();
			imageCell = isRight ? cells[1] : cells[0];
			textCell = isRight ? cells[0] : cells[1];

			return table;
		}


		// An image resized by width only is reported with a height of 0.0 (auto height), so
		// derive any missing dimension from the pixel aspect ratio. This reads the image data
		// itself rather than OneImage.ReadImage, which would write the unscaled pixel height
		// back into the page.
		private void GetImageSize(
			OneImage oneImage, XElement image, out double width, out double height)
		{
			width = oneImage.Width;
			height = oneImage.Height;

			if (width > 0 && height > 0)
			{
				return;
			}

			try
			{
				var data = image.Element(ns + "Data")?.Value;
				if (string.IsNullOrEmpty(data))
				{
					return;
				}

				var bytes = Convert.FromBase64String(data);
				using var stream = new MemoryStream(bytes);
				using var bitmap = Image.FromStream(stream);

				if (height > 0)
				{
					// only the width is missing
					width = height * bitmap.Width / bitmap.Height;
				}
				else
				{
					// A zero height (auto) makes OneNote ignore the whole Size, even a valid
					// width, and render the image at its natural size. It also ignores the
					// bitmap's DPI metadata and uses 96 dpi, so one pixel is 0.75 points.
					width = bitmap.Width * PixelsToPoints;
					height = bitmap.Height * PixelsToPoints;
				}

				logger.Verbose($"wrap image size derived from pixels: {width:0.0}x{height:0.0}");
			}
			catch (Exception exc)
			{
				logger.Verbose($"wrap cannot read image size: {exc.Message}");
			}
		}


		private double GetOutlineWidth(XElement image)
		{
			var width = image.Ancestors(ns + "Outline").FirstOrDefault()?
				.Element(ns + "Size")?.Attribute("width")?.Value;

			if (width is not null &&
				double.TryParse(width, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) &&
				value > 0)
			{
				return value;
			}

			return DefaultOutlineWidth;
		}


		// the paragraph must contain only the image, allowing for metadata and empty text runs
		private bool IsImageOnly(XElement paragraph, XElement image)
		{
			return paragraph.Elements().All(e =>
				e == image ||
				e.Name.LocalName == "Meta" ||
				(e.Name.LocalName == "T" && string.IsNullOrWhiteSpace(GetText(e))));
		}


		/// <summary>
		/// Collects the consecutive text paragraphs that follow the image paragraph
		/// </summary>
		private List<XElement> CollectCandidates(XElement imageOE)
		{
			var list = new List<XElement>();
			foreach (var sibling in imageOE.ElementsAfterSelf())
			{
				if (!IsTextParagraph(sibling))
				{
					break;
				}

				list.Add(sibling);
			}

			return list;
		}


		private bool IsTextParagraph(XElement element)
		{
			return element.Name == ns + "OE" &&
				element.Elements(ns + "T").Any() &&
				!element.Elements(ns + "OEChildren").Any() &&
				!element.Elements().Any(e =>
					e.Name.LocalName != "T" && e.Name.LocalName != "Meta" && e.Name.LocalName != "List");
		}


		private string GetText(XElement run)
		{
			var cdata = run.GetCData();
			return cdata is null || cdata.IsEmpty()
				? string.Empty
				: cdata.GetWrapper().Value;
		}


		private List<Measure> MeasureCandidates(
			Page page, List<XElement> candidates, double width)
		{
			var measures = new List<Measure>(candidates.Count);
			var analyzer = new StyleAnalyzer(page.Root);

			using var g = Graphics.FromHwnd(IntPtr.Zero);
			g.PageUnit = GraphicsUnit.Point;

			foreach (var paragraph in candidates)
			{
				var runs = paragraph.Elements(ns + "T").ToList();
				var text = string.Concat(runs.Select(GetText));
				var isBlank = string.IsNullOrWhiteSpace(text);

				var style = analyzer.CollectStyleFrom(runs[0]);

				var fontSize = ParsePoints(style.FontSize);
				if (fontSize <= 0)
				{
					fontSize = DefaultFontSize;
				}

				// list items are indented
				var available = Math.Max(
					AverageCharWidth,
					paragraph.Elements(ns + "List").Any() ? width - IndentWidth : width);

				var lines = CountLines(
					g, style, isBlank ? string.Empty : text, available, fontSize, out var lineHeight);

				var height = EstimateHeight(lines, lineHeight)
					+ ParsePoints(style.SpaceBefore) + ParsePoints(style.SpaceAfter);

				logger.Verbose(
					$"wrap paragraph blank={isBlank} size={fontSize} lines={lines} " +
					$"lineHeight={lineHeight:0.0} height={height:0.0}");

				measures.Add(new Measure(height, isBlank, lineHeight));
			}

			return measures;
		}


		// Counts wrapped lines using GDI for real word wrapping, honoring font size and weight,
		// and reports the font's own line height. Falls back to AddAnnotationCommand's
		// character-count formula if measuring fails. An empty text is one line.
		private int CountLines(
			Graphics g, Style style, string text, double width, double fontSize,
			out double lineHeight)
		{
			try
			{
				using var font = MakeFont(style);
				lineHeight = font.GetHeight(g);

				if (text.Length == 0)
				{
					return 1;
				}

				var measured = g.MeasureString(text, font, new SizeF((float)width, 10000f)).Height;
				return Math.Max(1, (int)Math.Round(measured / lineHeight));
			}
			catch (Exception exc)
			{
				logger.Verbose($"wrap measure failed, estimating by characters: {exc.Message}");

				lineHeight = FallbackLineHeight * fontSize / BaselineFontSize;
				var charWidth = AverageCharWidth * fontSize / BaselineFontSize;
				var charsPerLine = Math.Max(1.0, Math.Floor(width / charWidth));
				return (int)Math.Max(1.0, Math.Ceiling(text.Length / charsPerLine));
			}
		}


		/// <summary>
		/// Estimates the height of a paragraph, in points, from its line count and the
		/// font's line height.
		/// </summary>
		internal static double EstimateHeight(int lines, double lineHeight)
		{
			return Math.Max(1, lines) * lineHeight;
		}


		private static Font MakeFont(Style style)
		{
			var size = ParsePoints(style.FontSize);
			if (size <= 0)
			{
				size = DefaultFontSize;
			}

			var family = string.IsNullOrWhiteSpace(style.FontFamily)
				? DefaultFontFamily
				: style.FontFamily;

			var fontStyle = style.IsBold ? FontStyle.Bold : FontStyle.Regular;

			try
			{
				return new Font(family, (float)size, fontStyle, GraphicsUnit.Point);
			}
			catch (ArgumentException)
			{
				return new Font(DefaultFontFamily, (float)size, fontStyle, GraphicsUnit.Point);
			}
		}


		private static double ParsePoints(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return 0;
			}

			var text = value.Trim();
			if (text.EndsWith("pt", StringComparison.OrdinalIgnoreCase))
			{
				text = text.Substring(0, text.Length - 2);
			}

			return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)
				? result
				: 0;
		}


		/// <summary>
		/// Determines which paragraphs fit within the target height. Always includes the first
		/// non-blank paragraph, and never ends on a blank paragraph.
		/// </summary>
		/// <remarks>
		/// Leading blank paragraphs, those between the image and the first text, are kept unless
		/// ignoring them allows more text paragraphs to fit.
		/// </remarks>
		/// <param name="measures">Measured paragraphs, in page order</param>
		/// <param name="target">The height to fill, typically the image height</param>
		/// <param name="skip">
		/// The number of leading blank paragraphs to ignore (and discard); zero if they are kept
		/// </param>
		/// <param name="tolerance">
		/// How far past the target the text may extend, typically one line height
		/// </param>
		/// <returns>
		/// The number of paragraphs to move, starting after skip, or zero if there is no text
		/// </returns>
		internal static int CountThatFit(
			IReadOnlyList<Measure> measures, double target, out int skip, double tolerance = 0)
		{
			skip = 0;

			var leading = 0;
			while (leading < measures.Count && measures[leading].IsBlank)
			{
				leading++;
			}

			if (leading == measures.Count)
			{
				return 0;
			}

			var keep = CountFrom(measures, 0, target, tolerance);
			if (leading == 0)
			{
				return keep;
			}

			var drop = CountFrom(measures, leading, target, tolerance);

			// ignore the leading blanks only if doing so fits more text paragraphs
			if (CountText(measures, leading, drop) > CountText(measures, 0, keep))
			{
				skip = leading;
				return drop;
			}

			return keep;
		}


		private static int CountFrom(
			IReadOnlyList<Measure> measures, int start, double target, double tolerance)
		{
			var first = start;
			while (first < measures.Count && measures[first].IsBlank)
			{
				first++;
			}

			if (first == measures.Count)
			{
				return 0;
			}

			var total = 0.0;
			var count = 0;
			for (var i = start; i < measures.Count; i++)
			{
				total += measures[i].Height;
				if (total > target + tolerance && i > first)
				{
					break;
				}

				count = i - start + 1;
			}

			while (count > first - start + 1 && measures[start + count - 1].IsBlank)
			{
				count--;
			}

			return count;
		}


		private static int CountText(IReadOnlyList<Measure> measures, int start, int count)
		{
			var text = 0;
			for (var i = start; i < start + count; i++)
			{
				if (!measures[i].IsBlank)
				{
					text++;
				}
			}

			return text;
		}
	}
}
