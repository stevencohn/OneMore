//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Dialogs;
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Microsoft.Office.Interop.OneNote;


	internal class ToggleDttmCommand : Command
	{

		public ToggleDttmCommand() : base()
		{
		}


		public void Execute()
		{
			using (var dialog = new TimestampDialog())
			{
				if (dialog.ShowDialog(owner) == DialogResult.OK)
				{
					using (var manager = new ApplicationManager())
					{
						if (dialog.PageOnly)
						{
							var page = new Page(manager.CurrentPage());
							SetTimestampVisibility(manager, page.Root, page.Namespace, dialog.ShowTimestamps);
						}
						else
						{
							var section = manager.CurrentSection();
							if (section != null)
							{
								var ns = section.GetNamespaceOfPrefix("one");

								var pageIds = section.Elements(ns + "Page")
									.Select(e => e.Attribute("ID").Value)
									.ToList();

								using (var progress = new ProgressDialog())
								{
									progress.SetMaximum(pageIds.Count);
									progress.Show(owner);

									foreach (var pageId in pageIds)
									{
										var page = manager.GetPage(pageId, PageInfo.piBasic);
										var name = page.Attribute("name").Value;

										progress.SetMessage(name);
										progress.Increment();

										SetTimestampVisibility(manager, page, ns, dialog.ShowTimestamps);
									}
								}
							}
						}
					}
				}
			}
		}


		private static void SetTimestampVisibility(
			ApplicationManager manager, XElement page, XNamespace ns, bool visible)
		{
			var modified = false;
			var title = page.Element(ns + "Title");
			if (title != null)
			{
				modified |= SetTimestampAttribute(title, "showDate", visible);
				modified |= SetTimestampAttribute(title, "showTime", visible);

				if (modified)
				{
					manager.UpdatePageContent(page);
				}
			}
		}


		private static bool SetTimestampAttribute(XElement title, string name, bool visible)
		{
			var attr = title.Attribute(name);
			if (visible)
			{
				if (attr != null)
				{
					attr.Remove();
					return true;
				}
			}
			else
			{
				if (attr == null || attr.Value == "true")
				{
					title.SetAttributeValue(name, "false");
					return true;
				}
			}

			return false;
		}
	}
}

