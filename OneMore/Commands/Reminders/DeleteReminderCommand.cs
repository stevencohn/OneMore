//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class DeleteReminderCommand : Command
	{
		private Page page;
		private XNamespace ns;


		public DeleteReminderCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out page, out ns))
			{
				var paragraph = page.Root.Descendants(ns + "T")
					.Where(e => e.Attribute("selected")?.Value == "all")
					.Select(e => e.Parent)
					.FirstOrDefault();

				if (paragraph == null)
				{
					UIHelper.ShowInfo(one.Window, Resx.RemindCommand_noContext);
					return;
				}

				var reminder = CompleteReminderCommand.GetReminder(
					page, ns, paragraph, out var meta, out var tag);

				if (reminder == null)
				{
					UIHelper.ShowError(one.Window, Resx.RemindCommand_noReminder);
					return;
				}

				if (tag == null)
				{
					meta.Remove();
					await one.Update(page);

					UIHelper.ShowError(one.Window, Resx.RemindCommand_noReminder);
					return;
				}

				var result = UIHelper.ShowQuestion(
					Resx.DeleteReminderCommand_deleteTag, canCancel: true);

				if (result == DialogResult.Cancel)
				{
					return;
				}

				if (result == DialogResult.Yes)
				{
					tag.Remove();
				}

				meta.Remove();

				// must delete OE objectID to delete one:Meta tags
				paragraph.Attribute("objectID").Remove();

				await one.Update(page);
			}
		}
	}
}
