//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class CompleteReminderCommand : Command
	{
		private const string dateFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'000K";


		public CompleteReminderCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out var ns))
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

				var reminder = GetReminder(page, ns, paragraph, out var meta, out var tag);
				if (reminder == null)
				{
					UIHelper.ShowError(one.Window, Resx.RemindCommand_noReminder);
					return;
				}

				if (tag == null)
				{
					meta.Remove();

					// must delete OE objectID to clean up one:Meta tags
					paragraph.Attribute("objectID").Remove();

					await one.Update(page);

					UIHelper.ShowError(one.Window, Resx.RemindCommand_noReminder);
					return;
				}

				// complete the reminder...

				reminder.Status = ReminderStatus.Completed;
				reminder.Percent = 100;
				reminder.Completed = DateTime.UtcNow;
				meta.Attribute("content").Value = new ReminderSerializer().Encode(reminder);

				tag.Attribute("completed").Value = "true";
				tag.SetAttributeValue("completionDate", reminder.Completed.ToString(dateFormat));

				await one.Update(page);
			}
		}


		public static Reminder GetReminder(Page page, XNamespace ns,
			XElement paragraph, out XElement meta, out XElement tag)
		{
			tag = null;

			meta = paragraph.Elements(ns + "Meta")
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
					tag = paragraph.Elements(ns + "Tag")
						.FirstOrDefault(e => e.Attribute("index").Value == reminder.TagIndex);
				}
			}

			return reminder;
		}
	}
}
