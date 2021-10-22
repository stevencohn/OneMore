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

				var reminder = GetReminder(paragraph);
				if (reminder != null)
				{
					if (!string.IsNullOrEmpty(reminder.Symbol))
					{
						var result = UIHelper.ShowQuestion(
							Resx.DeleteReminderCommand_deleteTag, canCancel: true);

						if (result == DialogResult.Cancel)
						{
							return;
						}

						if (result == DialogResult.No)
						{
							reminder.Symbol = reminder.TagIndex = null;
						}
					}

					paragraph.Elements(ns + "Meta")
						.Where(e => e.Attribute("name").Value == MetaNames.Reminder)
						.Remove();

					if (!string.IsNullOrEmpty(reminder.TagIndex))
					{
						paragraph.Elements(ns + "Tag")
							.Where(e => e.Attribute("index").Value == reminder.TagIndex)
							.Remove();
					}

					paragraph.Attribute("objectID").Remove();

					await one.Update(page);
				}
			}
		}


		private Reminder GetReminder(XElement paragraph)
		{
			var meta = paragraph.Elements(ns + "Meta")
				.FirstOrDefault(e => e.Attribute("name").Value == MetaNames.Reminder);

			if (meta == null)
			{
				return null;
			}

			var reminder = new ReminderSerializer().Decode(meta.Attribute("content").Value);

			if (reminder != null &&
				!string.IsNullOrEmpty(reminder.Symbol) && reminder.Symbol != "0")
			{
				reminder.TagIndex = page.GetTagDefIndex(reminder.Symbol);
				if (reminder.TagIndex != null)
				{
					// confirm tag still exists
					var tag = paragraph.Elements(ns + "Tag")
						.FirstOrDefault(e => e.Attribute("index").Value == reminder.TagIndex);

					if (tag == null)
					{
						reminder.TagIndex = null;
						reminder.Symbol = null;
					}
				}
			}

			return reminder;
		}
	}
}
