//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Cli;
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Threading.Tasks;


	/// <summary>
	/// Removes "Authored by" annotations from a page, also removing related
	/// attributes from all content on the page
	/// </summary>
	internal class RemoveAuthorsCommand : Command, ICliPageCommand
	{
		public RemoveAuthorsCommand()
		{
		}


		#region CLI Implementation

		public string CommandName => "RemoveAuthors";

		public string Description => "Remove author annotations and editedBy attributes from a page";

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
				await Run(one, page);
				return;
			}

			await using var ribbon = new OneNote(out var rpage, out var _);
			logger.StartClock();
			await Run(ribbon, rpage);
		}


		private async Task Run(OneNote one, Page page)
		{
			var count = 0;

			// these are all the elements that might have editedByAttributes
			var elements = page.Root.Descendants().Where(d =>
				d.Name.LocalName == "Table" ||
				d.Name.LocalName == "Row" ||
				d.Name.LocalName == "Cell" ||
				d.Name.LocalName == "Outline" ||
				d.Name.LocalName == "OE")
				.ToList();

			var editedByAttributes = KnownSchemaAttributes.GetEditedByAttributes();

			foreach (var element in elements)
			{
				var attributes = element.Attributes()
					.Where(a => editedByAttributes.Contains(a.Name.LocalName))
					.ToList();

				count += attributes.Count;
				attributes.ForEach(a => a.Remove());
			}

			// TODO: This is removing authorship from OEs that wrap Images but
			// OneNote isn't saving those changes. I don't know why...

			if (count > 0)
			{
				logger.WriteTime($"removed {count} author-related attributes; saving...", true);
				await one.Update(page);
			}

			logger.WriteTime("removed authors completed");
		}
	}
}
