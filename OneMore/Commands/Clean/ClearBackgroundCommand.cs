//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S2583 // Conditionally executed code should be reachable

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
		private Color pageColor;
		private string pcolor;
		private SelectionRange range;


		public ClearBackgroundCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out page, out ns);
			pageColor = page.GetPageColor(out var _, out var _);
			pcolor = page.GetQuickStyle(StandardStyles.Normal).Color;

			range = new SelectionRange(page);

			var runs = range.GetSelections(defaulToAnytIfNoRange: true);
			logger.WriteLine($"found {runs.Count()} runs, scope={range.Scope}");

			var updated = ClearTextBackground(runs);
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
#if LOG
				var original = cdata.Value;
				var parstyle = run.Parent.Attribute("style")?.Value;
#endif
				// remove CDATA 'background' and 'mso-highlight' CSS properties...

				var matches = regex.Matches(cdata.Value);

				for (int i = matches.Count - 1; i >= 0; i--)
				{
					for (int j = matches[i].Groups.Count - 1; j >= 1; j--)
					{
						cdata.Value = cdata.Value.Remove(
							matches[i].Groups[j].Index,
							matches[i].Groups[j].Length);

						updated = true;
					}
				}

				// correct for contrast...

				if (cdata.Value.Contains("<span"))
				{
					var rewrap = false;
					var wrapper = cdata.GetWrapper();
					wrapper.Elements("span").ForEach(e => rewrap = CheckContrast(e) || rewrap);

					// #pragma
					if (rewrap)
					{
						cdata.Value = wrapper.GetInnerXml();
						updated = true;
					}
				}

				updated = CheckContrast(run.Parent) || updated;

				// deep prevents runs from being processed multiple times

				// NOTE sometimes OneNote will incorrectly set the collapsed attribute,
				// thinking it is set to 1 but is not visually collapsed!

				if (!deep && run.Parent.Attribute("collapsed")?.Value == "1")
				{
					updated = ClearTextBackground(
						run.Parent.Descendants(ns + "T").Where(e => e != run),
						true) || updated;
				}
#if LOG
				if (cdata.Value != original)
				{
					logger.WriteLine("---------");
					logger.WriteLine($"original: [{original}]");
					logger.WriteLine($"modified: [{cdata.Value}]");
				}
				if (run.Parent.Attribute("style")?.Value != parstyle)
				{
					logger.WriteLine("---------");
					logger.WriteLine($"parentst: [{parstyle}]");
					logger.WriteLine($"modified: [{run.Parent.Attribute("style")?.Value}]");
				}
#endif
			}

			return updated;
		}


		private bool CheckContrast(XElement element)
		{
			var css = element.Attribute("style")?.Value;
			if (css != null)
			{
				var style = new Style(css, false) { ApplyColors = true };

				if (!string.IsNullOrWhiteSpace(style.Color) &&
					style.Color != StyleBase.Automatic)
				{
					var color = ColorHelper.FromHtml(style.Color);
					if (color.LowContrast(pageColor))
					{
						logger.WriteLine($"pageColor:{pageColor.ToRGBHtml()} (dark:{pageColor.Invert().ToRGBHtml()}) ~ {color.ToRGBHtml()} (dark:{color.Invert().ToRGBHtml()}) -> {pcolor}");

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

			if (range.Scope == SelectionScope.TextCursor)
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
				var shade = ColorTranslator.FromHtml(attr.Value);
				if (pageColor.IsDark() != shade.IsDark())
				{
					attr.Remove();
					updated = true;
				}
			}

			return updated;
		}
	}
}