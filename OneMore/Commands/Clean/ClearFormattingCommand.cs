//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Clears all text styling (bold, italic, colors, highlights, font family and size) from
	/// the selected text, or from the current paragraph if there is no selection range.
	/// Unlike the built-in Clear All Formatting, this preserves list bullets and numbers,
	/// indents, tables, images and paragraph separations.
	/// </summary>
	/// <remarks>
	/// Style precedence is QuickStyleDef, then OE/@style, then T/@style, then CDATA spans.
	/// Removing the explicit styles lets the text fall back to the page's Normal style.
	/// </remarks>
	internal class ClearFormattingCommand : Command
	{
		private XNamespace ns;
		private string defaultCss;
		private string markerFont;
		private string markerSize;


		public ClearFormattingCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out ns);
			if (!page.IsValid)
			{
				return;
			}

			var range = new SelectionRange(page);

			// Scope is not set until the selections have been read
			var selections = range.GetSelections().ToList();
			if (selections.Count == 0)
			{
				return;
			}

			var wholeParagraph = range.Scope is SelectionScope.TextCursor or SelectionScope.SpecialCursor;

			var paragraphs = selections
				.Select(t => t.Parent)
				.Where(e => e?.Name == ns + "OE")
				.Distinct()
				.ToList();

			SetDefaults(page);

			var selected = new HashSet<XElement>(selections);
			var updated = false;

			foreach (var oe in paragraphs)
			{
				updated |= ClearParagraph(oe, selected, wholeParagraph);
			}

			if (updated)
			{
				await one.Update(page);
			}
		}


		private void SetDefaults(Page page)
		{
			var normal = page.GetQuickStyle(StandardStyles.Normal);

			markerFont = string.IsNullOrWhiteSpace(normal.FontFamily)
				? StyleBase.DefaultFontFamily
				: normal.FontFamily;

			markerSize = string.IsNullOrWhiteSpace(normal.FontSize)
				? StyleBase.DefaultFontSize.ToString("0.0", CultureInfo.InvariantCulture)
				: normal.FontSize;

			defaultCss = $"font-family:'{markerFont}';font-size:{markerSize}pt;";
		}


		private bool ClearParagraph(XElement oe, HashSet<XElement> selected, bool wholeParagraph)
		{
			// only this paragraph's own runs; child paragraphs (OEChildren) are not affected
			var runs = oe.Elements(ns + "T").ToList();

			var targets = wholeParagraph
				? runs
				: runs.Where(selected.Contains).ToList();

			var updated = false;
			var entire = targets.Count == runs.Count;

			if (entire)
			{
				// the paragraph level styles apply to every run so clear them as well
				updated |= oe.Attribute("style") is not null;
				oe.Attribute("style")?.Remove();

				updated |= oe.Attribute("quickStyleIndex") is not null;
				oe.Attribute("quickStyleIndex")?.Remove();

				updated |= ListMarker.Reset(oe, markerFont, markerSize);
			}

			// when only some runs are selected, OE/@style would still apply to them, so
			// give them an explicit default style; OneNote normalizes the result on save
			var overrideOE = !entire && oe.Attribute("style") is not null;

			foreach (var run in targets)
			{
				updated |= ClearRun(run, overrideOE);
			}

			return updated;
		}


		private bool ClearRun(XElement run, bool overrideOE)
		{
			var updated = false;

			var style = run.Attribute("style");
			if (style is not null)
			{
				style.Remove();
				updated = true;
			}

			var cdata = run.GetCData();
			if (cdata is not null && cdata.Value.Contains("<span"))
			{
				updated |= ClearSpans(cdata);
			}

			if (overrideOE)
			{
				run.SetAttributeValue("style", defaultCss);
				updated = true;
			}

			return updated;
		}


		private static bool ClearSpans(XCData cdata)
		{
			// skip content that can't be parsed, such as math
			XElement wrapper;
			try
			{
				wrapper = cdata.GetWrapper();
			}
			catch
			{
				return false;
			}

			if (wrapper is null)
			{
				return false;
			}

			// reverse document order so nested spans are unwrapped before their parents
			foreach (var span in wrapper.Descendants("span").Reverse().ToList())
			{
				span.Attribute("style")?.Remove();

				// keep spans that still carry other attributes, e.g. lang
				if (!span.HasAttributes)
				{
					span.ReplaceWith(span.Nodes());
				}
			}

			var value = wrapper.GetInnerXml();
			if (value == cdata.Value)
			{
				return false;
			}

			cdata.Value = value;
			return true;
		}
	}
}
