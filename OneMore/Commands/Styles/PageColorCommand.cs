//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
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
				color = page.GetPageColor(out _, out _);
			}

			using var dialog = new PageColorDialog(color);
			if (dialog.ShowDialog(owner) != DialogResult.OK)
			{
				return;
			}

			//UpdatePageColor(page, dialog.Color);
			//ThemeProvider.RecordTheme(dialog.ThemeKey);

			//if (dialog.ApplyStyle)
			//{
			//	new ApplyStylesCommand().Apply(page);
			//}

			//await one.Update(page);
		}


		private void UpdatePageColor(Page page, string color)
		{
			var element = page.Root
				.Elements(page.Namespace + "PageSettings")
				.FirstOrDefault();

			if (element == null)
			{
				logger.WriteLine("ChangePageColor failed because PageSettings not found");
				return;
			}

			ribbon.Invalidate();

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
