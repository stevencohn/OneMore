//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Helpers.Office;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System.Drawing;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;


	/// <summary>
	/// Applies a custom background color to the page and optionally applies the currently
	/// loaded custom styles to all content on the page
	/// </summary>
	internal class PageColorCommand : Command
	{
		public PageColorCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			Color color;
			Page page;
			using (var one = new OneNote(out page, out _))
			{
				color = page.GetPageColor(out var automatic, out _);
				if (automatic)
				{
					color = Color.Transparent;
				}
			}

			using var dialog = new PageColorDialog(color, new ThemeProvider().Theme.Name);
			if (dialog.ShowDialog(owner) == DialogResult.OK)
			{
				UpdatePageColor(page, MakePageColor(dialog.Color));

				if (dialog.ApplyStyle)
				{
					ThemeProvider.RecordTheme(dialog.ThemeKey);
					new ApplyStylesCommand().Apply(page);
				}

				using var one = new OneNote();
				await one.Update(page);
			}
		}


		public string MakePageColor(Color color)
		{
			var dark = Office.IsBlackThemeEnabled();

			if (color.Equals(Color.Transparent) ||
				(color.Equals(Color.Black) && dark) ||
				(color.Equals(Color.White) && !dark))
			{
				return StyleBase.Automatic;
			}

			return color.ToRGBHtml();
		}


		public bool UpdatePageColor(Page page, string color)
		{
			var element = page.Root
				.Elements(page.Namespace + "PageSettings")
				.FirstOrDefault();

			// this SHOULD be impossible
			if (element == null)
			{
				logger.WriteLine("PageColor failed because PageSettings not found!");
				return false;
			}

			var changed = false;

			var attr = element.Attribute("color");
			if (attr != null)
			{
				if (attr.Value != color)
				{
					attr.Value = color;
					changed = true;
				}
			}
			else
			{
				element.Add(new XAttribute("color", color));
				changed = true;
			}

			if (changed)
			{
				// if light->dark or dark->light, apply appropriate theme...

				var dark = false;
				if (color != StyleBase.Automatic)
				{
					dark = ColorTranslator.FromHtml(color).IsDark();
				}

				logger.WriteLine($"color set to {color} (dark:{dark})");
			}
			else
			{
				logger.WriteLine($"page color unchanged");
			}

			return changed;
		}
	}
}
