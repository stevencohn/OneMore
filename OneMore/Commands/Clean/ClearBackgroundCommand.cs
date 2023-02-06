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
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Clear the background color or table cell shading of the selected text and reset the
	/// text color to add contrast with page background.
	/// </summary>
	internal class ClearBackgroundCommand : Command
	{
		private XNamespace ns;
		private Page page;
		private bool darkPage;
		private string pcolor;


		public ClearBackgroundCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var one = new OneNote(out page, out ns);
			var pageColor = page.GetPageColor(out var automatic, out var black);
			darkPage = (automatic && black) || pageColor.IsDark();
			pcolor = page.GetQuickStyle(StandardStyles.Normal).Color;

			var updated = ClearTextBackground(page.GetSelectedElements(all: true));
			updated = ClearCellBackground() || updated;

			if (updated)
			{
				await one.Update(page);
			}
		}


		private bool ClearTextBackground(IEnumerable<XElement> runs, bool deep = false)
		{
			var updated = false;
			var regex = new Regex(@"<span[^>]*(background:[^;']+)(;mso-highlight:[^;']+)?");

			foreach (var run in runs)
			{
				var cdata = run.GetCData();
				if (cdata == null)
				{
					continue;
				}

				// remove CDATA 'background' and 'mso-highlight' CSS properties...

				var matches = regex.Matches(cdata.Value);
				if (matches.Count == 0)
				{
					continue;
				}

				for (int i = matches.Count - 1; i >= 0; i--)
				{
					for (int j = matches[i].Groups.Count - 1; j >= 1; j--)
					{
						cdata.Value = cdata.Value.Remove(
							matches[i].Groups[j].Index,
							matches[i].Groups[j].Length);
					}
				}

				// correct for contrast...

				if (cdata.Value.Contains("<span"))
				{
					var rewrap = false;
					var wrapper = cdata.GetWrapper();
					wrapper.Elements("span").ForEach(e => rewrap = CheckContrast(e) || rewrap);

					if (rewrap)
					{
						cdata.Value = wrapper.GetInnerXml();
					}
				}

				CheckContrast(run.Parent);

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


		private bool CheckContrast(XElement element)
		{
			var css = element.Attribute("style")?.Value;
			if (css != null)
			{
				var style = new Style(css) { ApplyColors = true };

				if (!string.IsNullOrWhiteSpace(style.Color))
				{
					var color = ColorHelper.FromHtml(style.Color);
					var ness = color.GetBrightness();


					if ((darkPage && ness < 0.4) || (!darkPage && ness > 0.6))
					{
						style.Color = pcolor;
						element.Attribute("style").Value = style.ToCss();
						return true;
					}
				}
			}

			return false;
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

				// if dark-on-light or light-on-dark
				if (darkPage != ColorTranslator.FromHtml(attr.Value).IsDark())
				{
					attr.Remove();
					updated = true;
				}
			}

			return updated;
		}
	}
}