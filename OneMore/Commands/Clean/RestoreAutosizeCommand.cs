//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Cli;
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Restores the auto-sizing behavior of manually resized cotnainers on the page
	/// </summary>
	internal class RestoreAutosizeCommand : Command, ICliPageCommand
	{
		private readonly int MaxWidth = 600;


		public RestoreAutosizeCommand()
		{
		}


		#region CLI Implementation

		public string CommandName => "RestoreAutosize";

		public string Description => "Restore auto-sizing behavior of manually resized containers";

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
			var sizes = page.Root.Descendants(ns + "Outline")
				.Elements(ns + "Size")
				.Where(e =>
					e.Attribute("isSetByUser") != null &&
					e.Attribute("isSetByUser").Value == "true");

			if (sizes.Any())
			{
				var modified = false;

				foreach (var size in sizes)
				{
					size.SetAttributeValue("isSetByUser", "false");

					// must modify both width and height in order for this to take effect

					size.SetAttributeValue("width", $"{MaxWidth}.0");

					size.GetAttributeValue("height", out float height);
					size.SetAttributeValue("height", (height + 1).ToInvariantString());

					modified = true;
				}

				if (modified)
				{
					await one.Update(page);
				}
			}

			await Task.Yield();
		}
	}
}
