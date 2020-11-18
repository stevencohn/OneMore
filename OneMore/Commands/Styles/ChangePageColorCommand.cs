//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class ChangePageColorCommand : Command
	{
		public ChangePageColorCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out _))
			{
				var currentColor = page.GetPageColor(out _, out _);
				var currentlydDark = currentColor.GetBrightness() < 0.5;

				using (var dialog = new Dialogs.ChangePageColorDialog(currentColor))
				{
					if (dialog.ShowDialog(owner) != DialogResult.OK)
					{
						return;
					}

					var element = page.Root
						.Elements(page.Namespace + "PageSettings")
						.FirstOrDefault();

					if (element != null)
					{
						var attr = element.Attribute("color");
						if (attr != null)
						{
							attr.Value = dialog.PageColor;
						}
						else
						{
							element.Add(new XAttribute("color", dialog.PageColor));
						}

						// if light->dark or dark->light, apply appropriate theme...

						var dark = false;
						if (dialog.PageColor != "automatic")
						{
							dark = ColorTranslator.FromHtml(dialog.PageColor).GetBrightness() < 0.5;
						}

						if (dark != currentlydDark)
						{
							//
						}

						one.Update(page);
					}
					else
					{
						logger.WriteLine("ChangePageColor failed because PageSettings not found");
					}
				}
			}
		}
	}
}
