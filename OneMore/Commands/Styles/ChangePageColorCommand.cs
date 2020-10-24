//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
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
				using (var dialog = new Dialogs.ChangePageColorDialog(page.GetPageColor(out _, out _)))
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
