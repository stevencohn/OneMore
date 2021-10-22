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

			//System.Diagnostics.Debugger.Launch();

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
			XElement tag;

			var meta = paragraph.Elements(ns + "Meta")
				.FirstOrDefault(e => e.Attribute("name").Value == MetaNames.Reminder);

			if (meta != null)
			{
				reminder = new ReminderSerializer()
					.Decode(meta.Attribute("content").Value);

				if (reminder != null && 
					!string.IsNullOrEmpty(reminder.Symbol) && reminder.Symbol != "0")
				{
					// check tag still exists
					var index = page.GetTagDefIndex(reminder.Symbol);
					tag = paragraph.Elements(ns + "Tag")
						.FirstOrDefault(e => e.Attribute("index").Value == index);

					if (tag != null)
					{
						if (tag.Attribute("completed").Value == "true" &&
							reminder.Status != ReminderStatus.Completed)
						{
							reminder.Status = ReminderStatus.Completed;
							reminder.Percent = 100;
							reminder.Completed = DateTime.Parse(tag.Attribute("completionDate").Value);
						}

						return reminder;
					}
				}
			}

			// either no meta or meta is orphaned from its tag so create a new one...

			var text = paragraph.Value;
			if (text.Length > 50)
			{
				text = text.Substring(0, 50) + "...";
			}

			reminder = new Reminder(paragraph.Attribute("objectID").Value)
			{
				Subject = text
			};

			tag = paragraph.Elements(ns + "Tag").FirstOrDefault();
			if (tag != null)
			{
				// use existing tag on paragraph
				reminder.TagIndex = tag.Attribute("index").Value;
				reminder.Symbol = page.GetTagDefSymbol(reminder.TagIndex);

				reminder.Created = DateTime.Parse(tag.Attribute("creationDate").Value);

				var completionDate = tag.Attribute("creationDate");
				if (completionDate != null)
				{
					reminder.Completed = DateTime.Parse(completionDate.Value);
				}
			}
			else
			{
				// use default symbol
				// if dialog is cancelled, OneNote will clean up the unused TagDef
				reminder.TagIndex = page.AddTagDef(reminder.Symbol,
					$"Reminder due {reminder.Due.ToFriendlyString()}");
			}

			return reminder;
		}


		private bool SetReminder(XElement paragraph, Reminder reminder)
		{
			var index = page.GetTagDefIndex(reminder.Symbol);
			if (index == null)
			{
				index = page.AddTagDef(reminder.Symbol,
					$"Reminder due {reminder.Due.ToFriendlyString()}");

				reminder.TagIndex = index;
			}

			var tag = paragraph.Elements(ns + "Tag")
				.FirstOrDefault(e => e.Attribute("index").Value == index);

			if (tag == null)
			{
				// tags must be ordered by index even within their containing paragraph
				// so take all, remove from paragraph, append, sort, re-add...

				var tags = paragraph.Elements(ns + "Tag").ToList();
				tags.ForEach(t => t.Remove());

				var completed = reminder.Status == ReminderStatus.Completed
					? "true" : "false";

				tag = new XElement(ns + "Tag",
					new XAttribute("index", index),
					new XAttribute("completed", completed),
					new XAttribute("disabled", "false")
					);

				tags.Add(tag);

				paragraph.AddFirst(tags.OrderBy(t => t.Attribute("index").Value));
			}
			else
			{
				var tcompleted = tag.Attribute("completed").Value == "true";
				var rcompleted = reminder.Status == ReminderStatus.Completed;
				if (tcompleted != rcompleted)
				{
					tag.Attribute("completed").Value = rcompleted ? "true" : "false";
				}
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
