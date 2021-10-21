//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;

	internal class RemindCommand : Command
	{
		private XNamespace ns;


		public RemindCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out ns))
			{
				var paragraph = page.Root.Descendants(ns + "T")
					.Where(e => e.Attribute("selected")?.Value == "all")
					.Select(e => e.Parent)
					.FirstOrDefault();

				if (paragraph == null)
				{
					logger.WriteLine("cursor must be positioned on a paragraph");
					return;
				}

				var tag = paragraph.Elements(ns + "Tag").FirstOrDefault();
				var reminder = GetReminder(paragraph, tag);

				using (var dialog = new ReminderDialog(reminder))
				{
					dialog.ShowDialog(owner);
				}
			}

			await Task.Yield();
		}


		private Reminder GetReminder(XElement paragraph, XElement tag)
		{
			var meta = paragraph.Elements(ns + "Meta")
				.FirstOrDefault(e => e.Attribute("name").Value == MetaNames.Reminder);

			var text = paragraph.Value;
			if (text.Length > 40)
			{
				text = text.Substring(0, 40) + "...";
			}

			if (meta == null)
			{
				return new Reminder
				{
					ObjectId = paragraph.Attribute("objectID").Value,
					TagIndex = tag == null ? 0 : int.Parse(tag.Attribute("index").Value),
					Subject = text
				};
			}

			var decoder = new Meta(MetaNames.Reminder, meta.Attribute("content").Value);
			var reminder = decoder.DecodeContent<Reminder>();

			return reminder;
		}
	}
}
