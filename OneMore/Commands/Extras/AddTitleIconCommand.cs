//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class AddTitleIconCommand : Command
	{
		public AddTitleIconCommand() : base()
		{
		}


		public void Execute()
		{
			string[] codes = null;

			using (var dialog = new Dialogs.EmojiDialog())
			{
				if (dialog.ShowDialog(owner) == DialogResult.Cancel)
				{
					return;
				}

				codes = dialog.GetSelectedCodes();
			}

			try
			{
				AddIcons(codes);
			}
			catch (Exception exc)
			{
				logger.WriteLine("Error adding title icons", exc);
			}
		}

		private void AddIcons(string[] codes)
		{
			using (var manager = new ApplicationManager())
			{
				var page = new Page(manager.CurrentPage());

				var cdata = page.Root.Elements(page.Namespace + "Title")
					.Elements(page.Namespace + "OE")
					.Elements(page.Namespace + "T")
					.DescendantNodes().OfType<XCData>()
					.FirstOrDefault();

				if (cdata != null)
				{
					var wrapper = cdata.GetWrapper();

					var espan = wrapper.Elements("span")
						.FirstOrDefault(e =>
							e.Attributes("style").Any(a => a.Value.Contains("Segoe UI Emoji")));

					if (espan != null)
					{
						espan.Value = string.Join(string.Empty, codes) + espan.Value;
					}
					else
					{
						wrapper.AddFirst(new XElement("span",
							new XAttribute("style", "font-family:'Segoe UI Emoji';font-size:16pt;"),
							string.Join(string.Empty, codes)
							));
					}

					var decoded = string.Concat(wrapper.Nodes()
						.Select(x => x.ToString()).ToArray())
						.Replace("&amp;", "&");

					cdata.ReplaceWith(new XCData(decoded));

					manager.UpdatePageContent(page.Root);
				}
			}
		}
	}
}
