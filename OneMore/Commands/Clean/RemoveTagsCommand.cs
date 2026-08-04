//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Cli;
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Removes tags from the current page, except from the tag bank and those associated
	/// with reminders. If a range of content is selected then only tags within that range
	/// are removed; otherwise all eligible tags on the page are removed.
	/// </summary>
	internal class RemoveTagsCommand : Command, ICliPageCommand
	{
		public RemoveTagsCommand()
		{
		}


		#region CLI Implementation

		public string CommandName => "RemoveTags";

		public string Description => "Remove all tags from a page except reminder-linked tags";

		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("notebook", "Name of notebook", required: true)
			.AddString("section", "Path of section", required: false)
			.AddString("page", "Name of page", required: false);

		#endregion CLI Implementation


		public override async Task Execute(params object[] args)
		{
			var cliParams = args.Length > 0 ? args[0] as CliParameterSet : null;
			if (cliParams != null)
			{
				cliParams.TryGet("pageId", out string pageId);
				if (string.IsNullOrWhiteSpace(pageId)) { return; }
				await using var one = new OneNote();
				var page = await one.GetPage(pageId, OneNote.PageDetail.All);
				await Run(one, page, page.Namespace);
				return;
			}

			await using var ribbon = new OneNote(out var rpage, out var rns);
			await Run(ribbon, rpage, rns);
		}


		private async Task Run(OneNote one, Page page, XNamespace ns)
		{
			var tags = page.BodyOutlines.Descendants(ns + "Tag").ToList();
			if (!tags.Any())
			{
				return;
			}

			var range = new SelectionRange(page);
			var selections = range.GetSelections(anyElement: true).ToList();

			if (range.Scope == SelectionScope.Run ||
				range.Scope == SelectionScope.Range ||
				range.Scope == SelectionScope.Block)
			{
				// use the actual selected leaf elements (selected="all") to find their
				// directly containing OE; do not rely on the "selected" attribute alone
				// because OneNote also stamps ancestor OEs as selected="partial" when the
				// selection lies within a nested, indented paragraph under their OEChildren
				var selectedOEs = selections
					.Select(e => e.Name.LocalName == "T" ? e.Parent : e)
					.Where(e => e?.Name.LocalName == "OE")
					.Distinct()
					.ToList();

				tags = tags.Where(t => selectedOEs.Contains(t.Parent)).ToList();
				if (!tags.Any())
				{
					return;
				}
			}

			var updated = false;
			var reminders = new ReminderSerializer().LoadReminders(page);
			var tagdefs = TagMapper.GetTagDefs(page);

			foreach (var tag in tags)
			{
				// lookup the tagdef to cross-reference symbol
				var tagdef = tagdefs
					.FirstOrDefault(m => m.Index == tag.Attribute("index").Value);

				if (tagdef != null)
				{
					var objectId = tag.Parent.Attribute("objectID").Value;

					// ensure tag is not a reminder for its paragraph
					if (!reminders.Exists(r =>
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
