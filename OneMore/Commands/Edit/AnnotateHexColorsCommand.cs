//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Globalization;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Prefaces each #rrggbb value with a swatch box of the same color, e.g. "■ #3366CC".
	/// Applies to the selected range or, if there is only a cursor, to the entire page.
	/// </summary>
	internal class AnnotateHexColorsCommand : Command
	{
		private const string Solid = "■"; // ■ black square, in Segoe UI
		private const string Outline = "□"; // □ white square, in Segoe UI
		private const string SwatchStyle = "font-family:'Segoe UI';font-size:10.0pt;color:";

		// a swatch glyph plus space is already an annotation
		private static readonly string[] Prefixes = { Solid + " ", Outline + " " };

		// minimum RGB distance (of a max ~441) between swatch and page to be considered visible
		private const double MinDistance = 40;

		private static readonly Regex HexPattern = new(
			@"(?<![0-9A-Fa-f#\w])#([0-9A-Fa-f]{6})(?![0-9A-Fa-f\w])", RegexOptions.Compiled);


		public AnnotateHexColorsCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out var ns);

			var range = new SelectionRange(page);
			_ = range.GetSelections(allowPageTitle: true);

			IEnumerable<XElement> runs;
			if (range.Scope == SelectionScope.TextCursor ||
				range.Scope == SelectionScope.SpecialCursor ||
				range.Scope == SelectionScope.None)
			{
				runs = page.Root.Elements(ns + "Outline").Descendants(ns + "T");
			}
			else
			{
				runs = page.Root.Descendants(ns + "T")
					.Where(e => e.Attributes("selected").Any(a => a.Value == "all"));
			}

			var background = page.GetPageColor(out _, out _);

			if (Annotate(runs.ToList(), background))
			{
				await one.Update(page);
			}
		}


		/// <summary>
		/// Annotates all #rrggbb values in the given T runs.
		/// </summary>
		/// <param name="runs">The T elements to scan</param>
		/// <param name="background">The page background color</param>
		/// <returns>True if any run was modified</returns>
		internal bool Annotate(IEnumerable<XElement> runs, Color background)
		{
			var updated = false;

			foreach (var run in runs)
			{
				if (run.GetCData() is not XCData cdata ||
					!HexPattern.IsMatch(cdata.Value))
				{
					continue;
				}

				var wrapper = cdata.GetWrapper();
				var changed = false;
				var tail = string.Empty;

				foreach (var node in wrapper.Nodes().ToList())
				{
					if (node is XText text)
					{
						var nodes = Split(text.Value, null, background, ref tail);
						if (nodes is not null)
						{
							text.ReplaceWith(nodes);
							changed = true;
						}
					}
					else if (node is XElement span && span.Name.LocalName == "span")
					{
						// only simple spans containing a single text node
						if (span.FirstNode is XText inner && span.FirstNode == span.LastNode)
						{
							var nodes = Split(inner.Value, span, background, ref tail);
							if (nodes is not null)
							{
								span.ReplaceWith(nodes);
								changed = true;
							}
						}
						else
						{
							tail = Tail(tail, span.Value);
						}
					}
				}

				if (changed)
				{
					// special case for <br> + EOL, as in PageEditor.EditSelected
					cdata.Value = wrapper.GetInnerXml().Replace("<br>", "<br>\n");
					updated = true;
				}
			}

			return updated;
		}


		/// <summary>
		/// Splits the text around each unannotated hex value, inserting a swatch span before it.
		/// </summary>
		/// <param name="value">The text content</param>
		/// <param name="span">The span owning the text, or null if it is bare text</param>
		/// <param name="background">Page color</param>
		/// <param name="tail">Trailing text emitted so far, used to detect existing swatches</param>
		/// <returns>New nodes, or null if nothing changed</returns>
		private static List<XNode> Split(
			string value, XElement span, Color background, ref string tail)
		{
			var matches = HexPattern.Matches(value);
			if (matches.Count == 0)
			{
				tail = Tail(tail, value);
				return null;
			}

			var nodes = new List<XNode>();
			var index = 0;
			var changed = false;

			foreach (Match match in matches)
			{
				var before = value.Substring(index, match.Index - index);
				var preceding = Tail(tail, before);

				if (Prefixes.Any(p => preceding.EndsWith(p, StringComparison.Ordinal)))
				{
					// already annotated
					tail = preceding;
					continue;
				}

				if (before.Length > 0)
				{
					nodes.Add(Wrap(before, span));
				}

				nodes.Add(new XElement("span",
					new XAttribute("style", SwatchStyle + SwatchColor(match.Value, background)),
					GlyphFor(match.Value, background) + " "));

				index = match.Index;
				tail = Solid + " ";
				changed = true;
			}

			if (!changed)
			{
				return null;
			}

			var rest = value.Substring(index);
			nodes.Add(Wrap(rest, span));
			tail = Tail(tail, rest);

			return nodes;
		}


		private static XNode Wrap(string text, XElement span)
		{
			return span is null
				? new XText(text)
				: new XElement(span.Name, span.Attributes(), text);
		}


		private static string Tail(string tail, string text)
		{
			var s = tail + text;
			return s.Length > 2 ? s.Substring(s.Length - 2) : s;
		}


		private static string GlyphFor(string hex, Color background)
		{
			return IsHidden(hex, background) ? Outline : Solid;
		}


		private static string SwatchColor(string hex, Color background)
		{
			if (!IsHidden(hex, background))
			{
				return hex;
			}

			// outline glyph in a color that contrasts with the page
			return background.GetBrightness() < 0.5 ? "#FFFFFF" : "#000000";
		}


		private static bool IsHidden(string hex, Color background)
		{
			var r = int.Parse(hex.Substring(1, 2), NumberStyles.HexNumber);
			var g = int.Parse(hex.Substring(3, 2), NumberStyles.HexNumber);
			var b = int.Parse(hex.Substring(5, 2), NumberStyles.HexNumber);

			var dr = r - background.R;
			var dg = g - background.G;
			var db = b - background.B;

			return Math.Sqrt((dr * dr) + (dg * dg) + (db * db)) < MinDistance;
		}
	}
}
