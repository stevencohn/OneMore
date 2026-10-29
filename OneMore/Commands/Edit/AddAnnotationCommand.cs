//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Highlights the selected text with the next color of the rotating highlighter and adds
	/// an empty annotation outline, seeded with the same color, to the right of the selection.
	/// </summary>
	/// <remarks>
	/// OneNote only reports positions for top-level outlines, not for paragraphs within them,
	/// so the vertical offset of the selection is estimated from the paragraphs above it.
	/// Outline positions are page-relative so the result is independent of scrolling.
	/// </remarks>
	internal class AddAnnotationCommand : Command
	{
		// width of the annotation outline, in points
		private const double AnnotationWidth = 200.0;

		// space between the anchor outline and the annotation outline, in points
		private const double Gutter = 20.0;

		// estimation constants for a default 11pt paragraph, in points
		private const double LineHeight = 14.4;
		private const double ParagraphGap = 2.0;
		private const double AverageCharWidth = 5.5;
		private const double IndentWidth = 18.0;
		private const double DefaultOutlineWidth = 300.0;


		public AddAnnotationCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out var ns);

			var range = new SelectionRange(page);
			var selections = range.GetSelections().ToList();
			if (selections.Count == 0 ||
				range.Scope is not (SelectionScope.Run or SelectionScope.Range or SelectionScope.Block))
			{
				ShowInfo(Resx.AddAnnotationCommand_NoSelection);
				return;
			}

			var first = selections[0];
			var outline = first.Ancestors(ns + "Outline").FirstOrDefault();
			if (outline is null)
			{
				ShowError(Resx.Error_BodyContext);
				return;
			}

			// where the annotation goes; must be computed before highlighting alters the tree
			var x = GetDouble(outline.Element(ns + "Position"), "x", 0.0)
				+ GetDouble(outline.Element(ns + "Size"), "width", DefaultOutlineWidth)
				+ Gutter;

			var y = GetDouble(outline.Element(ns + "Position"), "y", 0.0)
				+ EstimateOffset(outline, first, ns);

			var (index, color) = HighlightRotator.GetColor(page, advance: true);

			if (!HighlightRotator.Apply(page, color))
			{
				return;
			}

			HighlightRotator.SaveIndex(page, index);

			// only one selection may exist so the cursor lands in the annotation
			foreach (var run in page.Root.Descendants(ns + "T")
				.Where(e => e.Attribute("selected") is not null).ToList())
			{
				run.Attribute("selected").Remove();
			}

			page.Root.Add(MakeAnnotation(page, ns, color, x, y));

			await one.Update(page);
		}


		private static double GetDouble(XElement element, string name, double fallback)
		{
			if (element is not null &&
				element.GetAttributeValue(name, out decimal value))
			{
				return (double)value;
			}

			return fallback;
		}


		// Sums the estimated heights of every paragraph preceding the one containing the
		// first selected run, in document order. Parents precede their children both in the
		// document and on the page so a simple descendant walk is sufficient.
		private static double EstimateOffset(XElement outline, XElement first, XNamespace ns)
		{
			var target = first.Ancestors(ns + "OE").FirstOrDefault();
			var width = GetDouble(outline.Element(ns + "Size"), "width", DefaultOutlineWidth);
			var offset = 0.0;

			foreach (var paragraph in outline.Descendants(ns + "OE"))
			{
				if (paragraph == target)
				{
					break;
				}

				offset += EstimateHeight(paragraph, width, ns) + ParagraphGap;
			}

			// never place beyond the outline's known extent
			var height = GetDouble(outline.Element(ns + "Size"), "height", double.MaxValue);
			return Math.Min(offset, height);
		}


		private static double EstimateHeight(XElement paragraph, double outlineWidth, XNamespace ns)
		{
			var runs = paragraph.Elements(ns + "T").ToList();
			if (runs.Count == 0)
			{
				// images report their own size
				var imageHeight = GetDouble(
					paragraph.Elements(ns + "Image").Elements(ns + "Size").FirstOrDefault(),
					"height", 0.0);

				if (imageHeight > 0.0)
				{
					return imageHeight;
				}

				// table cells are counted as their own paragraphs, so the table row itself adds
				// nothing; otherwise this is an empty paragraph
				return paragraph.Elements(ns + "Table").Any() ? 0.0 : LineHeight;
			}

			var depth = paragraph.Ancestors(ns + "OEChildren").Count() - 1;
			var available = Math.Max(AverageCharWidth, outlineWidth - (depth * IndentWidth));
			var charsPerLine = Math.Max(1.0, Math.Floor(available / AverageCharWidth));

			var length = runs.Sum(r => r.FirstNode is XCData cdata ? cdata.GetWrapper().Value.Length : 0);
			var lines = Math.Max(1.0, Math.Ceiling(length / charsPerLine));

			return lines * LineHeight;
		}


		private static XElement MakeAnnotation(
			Page page, XNamespace ns, string color, double x, double y)
		{
			var z = page.Root.Elements(ns + "Outline")
				.Select(o => o.Element(ns + "Position"))
				.Where(p => p is not null)
				.Select(p => GetDouble(p, "z", 0.0))
				.DefaultIfEmpty(-1.0)
				.Max() + 1;

			// empty paragraph that carries the highlight color so typing continues in it
			var cdata = new XCData($"<span style='background:{color};font-family:\"Calibri Light\";font-size:11.0pt;color:#0070C0'>→</span>");

			// the empty selected run after the arrow positions the text cursor there
			var paragraph = new XElement(ns + "OE",
				new XAttribute("style", $"background:{color}"),
				new XElement(ns + "T", cdata),
				new XElement(ns + "T",
					new XAttribute("selected", "all"),
					new XCData($"<span style='background:{color}'></span>")));

			return new XElement(ns + "Outline",
				new XElement(ns + "Position",
					new XAttribute("x", x.ToString("0.0###", System.Globalization.CultureInfo.InvariantCulture)),
					new XAttribute("y", y.ToString("0.0###", System.Globalization.CultureInfo.InvariantCulture)),
					new XAttribute("z", ((int)z).ToString(System.Globalization.CultureInfo.InvariantCulture))),
				new XElement(ns + "Size",
					new XAttribute("width", AnnotationWidth.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)),
					new XAttribute("height", "14.0")),
				new XElement(ns + "OEChildren", paragraph));
		}
	}
}
