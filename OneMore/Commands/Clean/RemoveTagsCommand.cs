//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Removes all tags from the current page, except those that are associated with reminders.
	/// </summary>
	internal class RemoveTagsCommand : Command
	{
		public RemoveTagsCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var one = new OneNote(out var page, out var ns);
			var tags = page.Root.Elements(ns + "Outline").Descendants(ns + "Tag").ToList();
			if (!tags.Any())
			{
				return;
			}

			var updated = false;
			var reminders = new ReminderSerializer().LoadReminders(page);
			var tagdefs = page.GetTagDefMap().Select(m => m.TagDef);

			foreach (var tag in tags)
			{
				// lookup the tagdef to cross-reference symbol
				var tagdef = tagdefs
					.FirstOrDefault(m => m.Index == tag.Attribute("index").Value);

				if (tagdef != null)
				{
					var objectId = tag.Parent.Attribute("objectID").Value;

					// ensure tag is not a reminder for its paragraph
					if (!reminders.Any(r =>
						r.Symbol == tagdef.Symbol && r.ObjectId == objectId))
					{
						tag.Remove();
						updated = true;
					}
				}
			}

			if (updated)
			{
				await one.Update(page);
			}
		}
	}
}
