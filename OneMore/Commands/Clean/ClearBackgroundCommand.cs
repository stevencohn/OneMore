//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Clear the background color or (table cell) shading of the selected text and reset the
	/// text color to add contrast with page background.
	/// </summary>
	internal class ClearBackgroundCommand : Command
	{
		private const string White = "#FFFFFF";
		private const string Black = "#000000";

		private OneNote one;
		private XNamespace ns;
		private Page page;
		private bool darkPage;
		private StyleAnalyzer analyzer;


		public ClearBackgroundCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (one = new OneNote(out page, out ns))
			{
				darkPage = page.GetPageColor(out _, out _).GetBrightness() < 0.5;
				analyzer = new StyleAnalyzer(page.Root);

				var updated = ClearTextBackground(page.GetSelectedElements(all: true));
				updated |= ClearCellBackground();

				if (updated)
				{
					await one.Update(page);
				}
			}
		}


		private bool ClearTextBackground(IEnumerable<XElement> runs, bool deep = false)
		{
			var updated = false;

			foreach (var run in runs)
			{
				var style = new Style(analyzer.CollectFrom(run, true));

				if (!string.IsNullOrEmpty(style.Highlight))
				{
					style.Highlight = Style.Automatic;
				}

				var darkText = !style.Color.Equals(Style.Automatic)
					&& ColorTranslator.FromHtml(style.Color).GetBrightness() < 0.5;

				// if dark-on-dark or light-on-light
				if (darkText == darkPage)
				{
					style.Color = darkText ? White : Black;
				}

				var stylizer = new Stylizer(style);
				stylizer.ApplyStyle(run);

				ClearHyperlinkBackground(run);
				updated = true;

				// deep prevents runs from being processed multiple times

				// NOTE sometimes OneNote will incorrectly set the collapsed attribute,
				// thinking it is set to 1 but is not visually collapsed!

				if (!deep && run.Parent.Attribute("collapsed")?.Value == "1")
				{
					updated |= ClearTextBackground(
						run.Parent.Descendants(ns + "T").Where(e => e != run),
						true);
				}
			}

			return updated;
		}


		private void ClearHyperlinkBackground(XElement run)
		{
			// hyperlinks are further styled with their own span, usually including both the
			// background and mso-highlight css attributes, but we only care about background
			// attribute since rebuilding it with a Style object will wipe out mso-highlight.
			// OneNote will manage the font color of hyperlinks so don't bother with that

			var cdata = run.GetCData();
			var wrapper = cdata.GetWrapper();

			var found = false;
			wrapper.Descendants("span")
				.Where(e => e.Attribute("style") != null &&
					e.Attribute("style").Value.Contains("background"))
				.Select(e => e.Attribute("style"))
				.ForEach(a =>
				{
					var style = new Style(a.Value)
					{
						Highlight = Style.Automatic
					};

					a.Value = style.ToCss();
					found = true;
				});

			if (found)
			{
				cdata.Value = wrapper.GetInnerXml();
			}
		}


		private bool ClearCellBackground()
		{
			IEnumerable<XElement> cells;

			if (page.SelectionScope == SelectionScope.Empty)
			{
				cells = page.Root.Descendants(ns + "Cell")
					.Where(e => e.Attribute("shadingColor") != null);
			}
			else
			{
				// only clear cell background if entire cell is selected

				cells = page.Root.Descendants(ns + "Cell")
					.Where(e => e.Attribute("shadingColor") != null
						&& e.Attribute("selected")?.Value == "all"
						);
			}

			var updated = false;

			foreach (var cell in cells)
			{
				var attr = cell.Attribute("shadingColor");
				var darkCell = ColorTranslator.FromHtml(attr.Value).GetBrightness() < 0.5;

				// if dark-on-light or light-on-dark
				if (darkCell != darkPage)
				{
					attr.Remove();
					updated = true;
				}
			}

			return updated;
		}
	}
}