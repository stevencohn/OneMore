//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Helpers.Office;
	using River.OneMoreAddIn.Models;
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
				color = page.GetPageColor(out _, out _);
			}

			using var dialog = new PageColorDialog(color);
			if (dialog.ShowDialog(owner) == DialogResult.OK)
			{
				UpdatePageColor(page, MakePageColor(dialog.Color));

				//if (dialog.ApplyStyle)
				//{
				//	ThemeProvider.RecordTheme(dialog.ThemeKey);
				//	new ApplyStylesCommand().Apply(page);
				//}

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
				return "automatic";
			}

			return color.ToRGBHtml();
		}


		private void UpdatePageColor(Page page, string color)
		{
			var element = page.Root
				.Elements(page.Namespace + "PageSettings")
				.FirstOrDefault();

			// this SHOULD be impossible
			if (element == null)
			{
				logger.WriteLine("PageColor failed because PageSettings not found!");
				return;
			}

			var attr = element.Attribute("color");
			if (attr != null)
			{
				attr.Value = color;
			}
			else
			{
				element.Add(new XAttribute("color", color));
			}

			// if light->dark or dark->light, apply appropriate theme...

			var dark = false;
			if (color != "automatic")
			{
				dark = ColorTranslator.FromHtml(color).GetBrightness() < 0.5;
			}

			logger.WriteLine($"color set to {color} (dark:{dark})");
		}
	}
}
