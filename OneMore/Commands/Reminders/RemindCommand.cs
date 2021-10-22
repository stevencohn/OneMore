//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class RemindCommand : Command
	{
		private Page page;
		private XNamespace ns;


		public RemindCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out page, out ns))
			{
				PageNamespace.Set(ns);

				var paragraph = page.Root.Descendants(ns + "T")
					.Where(e => e.Attribute("selected")?.Value == "all")
					.Select(e => e.Parent)
					.FirstOrDefault();

				if (paragraph == null)
				{
					UIHelper.ShowInfo(one.Window, "Cursor must be positioned on a paragraph");
					return;
				}

				var reminder = GetReminder(paragraph);

				using (var dialog = new ReminderDialog(reminder))
				{
					if (dialog.ShowDialog(owner) == DialogResult.OK)
					{
						if (SetReminder(paragraph, dialog.Reminder))
						{
							await one.Update(page);
						}
					}
				}
			}
		}


		private Reminder GetReminder(XElement paragraph)
		{
			Reminder reminder;

			var meta = paragraph.Elements(ns + "Meta")
				.FirstOrDefault(e => e.Attribute("name").Value == MetaNames.Reminder);

			if (meta != null)
			{
				reminder = new ReminderSerializer()
					.Decode(meta.Attribute("content").Value);

				if (reminder != null)
				{
					// check tag still exists
					if (paragraph.Elements(ns + "Tag")
						.Any(e => e.Attribute("index").Value == reminder.TagIndex))
					{
						return reminder;
					}
				}
			}

			var text = paragraph.Value;
			if (text.Length > 50)
			{
				text = text.Substring(0, 50) + "...";
			}

			reminder = new Reminder
			{
				ObjectId = paragraph.Attribute("objectID").Value,
				Subject = text
			};

			var tag = paragraph.Elements(ns + "Tag").FirstOrDefault();
			if (tag != null)
			{
				reminder.TagIndex = tag.Attribute("index").Value;
				reminder.Symbol = page.GetTagDefSymbol(reminder.TagIndex);

				reminder.Created = DateTime.Parse(tag.Attribute("creationDate").Value);

				var completionDate = tag.Attribute("creationDate");
				if (completionDate != null)
				{
					reminder.Completed = DateTime.Parse(completionDate.Value);
				}
			}

			return reminder;
		}


		private bool SetReminder(XElement paragraph, Reminder reminder)
		{
			var tag = paragraph.Elements(ns + "Tag")
				.FirstOrDefault(e => e.Attribute("index").Value == reminder.TagIndex);

			if (tag == null)
			{
				var index = page.GetTagDefIndex(reminder.Symbol);
				if (index == null)
				{
					index = page.AddTagDef(reminder.Symbol, "Reminder");
				}

				paragraph.AddFirst(new XElement(ns + "Tag",
					new XAttribute("index", index),
					new XAttribute("completed", "false")
					));
			}

			var encoded = new ReminderSerializer().Encode(reminder);
			if (encoded == null)
			{
				return false;
			}

			var meta = paragraph.Elements(ns + "Meta")
				.FirstOrDefault(e => e.Attribute("name").Value == MetaNames.Reminder);

			if (meta != null)
			{
				meta.Attribute("content").Value = encoded;
			}
			else
			{
				meta = new Meta(MetaNames.Reminder, encoded);
				paragraph.Elements(ns + "Tag").Last().AddAfterSelf(meta);
			}

			return true;
		}
	}
}
