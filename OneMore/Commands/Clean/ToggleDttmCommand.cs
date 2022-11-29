//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;


	/// <summary>
	/// Toggles the page date and time stamps under the title on the current page or all
	/// pages in the current section
	/// </summary>
	internal class ToggleDttmCommand : Command
	{

		public ToggleDttmCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var dialog = new ToggleDttmDialog();
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				await Toggle(dialog.PageOnly, dialog.ShowTimestamps);
			}
		}


		private async Task Toggle(bool pageOnly, bool showTimestamps)
		{
			using var one = new OneNote();
			if (pageOnly)
			{
				var page = one.GetPage();
				await SetTimestampVisibility(one, page, showTimestamps);
			}
			else
			{
				var section = one.GetSection();
				if (section != null)
				{
					var ns = one.GetNamespace(section);

					var pageIds = section.Elements(ns + "Page")
						.Select(e => e.Attribute("ID").Value)
						.ToList();

					using var progress = new UI.ProgressDialog();
					progress.SetMaximum(pageIds.Count);
					progress.Show();

					foreach (var pageId in pageIds)
					{
						var page = one.GetPage(pageId, OneNote.PageDetail.Basic);
						var name = page.Root.Attribute("name").Value;

						progress.SetMessage(name);
						progress.Increment();

						await SetTimestampVisibility(one, page, showTimestamps);
					}
				}
			}
		}


		private static async Task SetTimestampVisibility(OneNote one, Page page, bool visible)
		{
			var modified = false;
			var title = page.Root.Element(page.Namespace + "Title");
			if (title != null)
			{
				modified |= SetTimestampAttribute(title, "showDate", visible);
				modified |= SetTimestampAttribute(title, "showTime", visible);

				if (modified)
				{
					await one.Update(page);
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

