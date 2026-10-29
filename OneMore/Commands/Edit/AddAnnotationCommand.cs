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


		// Sums the estimated heights of everything preceding the paragraph containing the
		// first selected run. Tables are measured row by row, each row as tall as its tallest
		// cell, since cells within a row sit side by side rather than stacking.
		private static double EstimateOffset(XElement outline, XElement first, XNamespace ns)
		{
			var target = first.Ancestors(ns + "OE").FirstOrDefault();
			var width = GetDouble(outline.Element(ns + "Size"), "width", DefaultOutlineWidth);
			var offset = 0.0;

			Walk(outline, target, width, 0, ns, ref offset);

			// never place beyond the outline's known extent
			var height = GetDouble(outline.Element(ns + "Size"), "height", double.MaxValue);
			return Math.Min(offset, height);
		}


		// Accumulates the heights of content preceding target; returns true once it is found
		private static bool Walk(
			XElement parent, XElement target, double width, int depth, XNamespace ns, ref double offset)
		{
			foreach (var oe in parent.Elements(ns + "OEChildren").Elements(ns + "OE"))
			{
				if (oe == target)
				{
					return true;
				}

				if (!oe.Descendants(ns + "OE").Contains(target))
				{
					offset += EstimateHeight(oe, width, depth, ns) + ParagraphGap;
					continue;
				}

				// target is nested within this paragraph; add its own text first
				offset += EstimateTextHeight(oe, width, depth, ns) + ParagraphGap;

				foreach (var table in oe.Elements(ns + "Table"))
				{
					if (!table.Descendants(ns + "OE").Contains(target))
					{
						offset += EstimateTableHeight(table, width, ns);
						continue;
					}

					var cellWidth = GetCellWidth(table, width, ns);
					foreach (var row in table.Elements(ns + "Row"))
					{
						if (!row.Descendants(ns + "OE").Contains(target))
						{
							offset += EstimateRowHeight(row, cellWidth, ns);
							continue;
						}

						var cell = row.Elements(ns + "Cell")
							.FirstOrDefault(c => c.Descendants(ns + "OE").Contains(target));

						return cell is not null && Walk(cell, target, cellWidth, 0, ns, ref offset);
					}

					return true;
				}

				return Walk(oe, target, width, depth + 1, ns, ref offset);
			}

			return false;
		}


		private static double GetCellWidth(XElement table, double width, XNamespace ns)
		{
			var columns = table.Element(ns + "Columns")?.Elements(ns + "Column").Count() ?? 1;
			return width / Math.Max(1, columns);
		}


		private static double EstimateTableHeight(XElement table, double width, XNamespace ns)
		{
			var cellWidth = GetCellWidth(table, width, ns);
			return table.Elements(ns + "Row").Sum(r => EstimateRowHeight(r, cellWidth, ns));
		}


		private static double EstimateRowHeight(XElement row, double cellWidth, XNamespace ns)
		{
			return row.Elements(ns + "Cell")
				.Select(c => c.Elements(ns + "OEChildren").Elements(ns + "OE")
					.Sum(oe => EstimateHeight(oe, cellWidth, 0, ns) + ParagraphGap))
				.DefaultIfEmpty(LineHeight)
				.Max();
		}


		// Full height of a paragraph including its tables and nested children
		private static double EstimateHeight(XElement oe, double width, int depth, XNamespace ns)
		{
			var height = EstimateTextHeight(oe, width, depth, ns);

			height += oe.Elements(ns + "Table").Sum(t => EstimateTableHeight(t, width, ns));

			height += oe.Elements(ns + "OEChildren").Elements(ns + "OE")
				.Sum(c => EstimateHeight(c, width, depth + 1, ns) + ParagraphGap);

			return height;
		}


		private static double EstimateTextHeight(
			XElement paragraph, double outlineWidth, int depth, XNamespace ns)
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

				// a table adds no text line of its own; otherwise this is an empty paragraph
				return paragraph.Elements(ns + "Table").Any() ? 0.0 : LineHeight;
			}

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
