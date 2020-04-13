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


		public void Execute()
		{
			logger.WriteLine($"{nameof(ChangePageColorCommand)}.Execute()");

			using (var manager = new ApplicationManager())
			{
				var page = new Page(manager.CurrentPage());

				using (var dialog = new ChangePageColorDialog(page.GetPageColor()))
				{
					if (dialog.ShowDialog(owner) == DialogResult.OK)
					{

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

							manager.UpdatePageContent(page.Root);
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
}
