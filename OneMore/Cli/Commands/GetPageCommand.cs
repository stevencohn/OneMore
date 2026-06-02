//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Cli;


	internal class GetPageCommand : Command, ICliCommand
	{
		public GetPageCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		#region CLI Implementation

		public string CommandName => "GetPage";


		public string Description => "Get the XML of a specific page";


		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("notebook", "Name of notebook", required: false)
			.AddString("section", "Path of section", required: false)
			.AddString("page", "Name of page", required: false)
			.AddBoolean("current", "Return the currently active OneNote page",
				required: false, defaultValue: false);

		#endregion CLI Implementation


		public override async Task Execute(params object[] args)
		{
			var cliParams = args.Length > 0 ? args[0] as CliParameterSet : null;

			string notebookName = null;
			string sectionPath = null;
			string pageName = null;
			var useCurrent = false;

			if (cliParams != null)
			{
				cliParams.TryGet("notebook", out notebookName);
				cliParams.TryGet("section", out sectionPath);
				cliParams.TryGet("page", out pageName);
				cliParams.TryGet("current", out useCurrent);
			}

			if (useCurrent && !string.IsNullOrWhiteSpace(pageName))
			{
				CliOutput = "Specify either --current or --page, not both.";
				await Task.Yield();
				return;
			}

			await using var one = new OneNote();

			if (useCurrent)
			{
				var currentPageId = one.CurrentPageId;
				if (string.IsNullOrEmpty(currentPageId))
				{
					CliOutput = "No page is currently active in OneNote.";
					await Task.Yield();
					return;
				}

				var currentPage = await one.GetPage(currentPageId, OneNote.PageDetail.All);
				CliOutput = currentPage?.Root?.ToString() ?? string.Empty;
				await Task.Yield();
				return;
			}

			if (string.IsNullOrWhiteSpace(pageName))
			{
				CliOutput = "Specify either --current or --page (with --notebook and --section).";
				await Task.Yield();
				return;
			}

			if (string.IsNullOrWhiteSpace(notebookName) || string.IsNullOrWhiteSpace(sectionPath))
			{
				CliOutput = "When using --page, --notebook and --section are also required.";
				await Task.Yield();
				return;
			}

			var path = string.Concat(notebookName, "/", sectionPath, "/", pageName);
			var pageIds = await one.FindPagesByPath(path);

			if (pageIds.Length == 0)
			{
				CliOutput = string.Empty;
				await Task.Yield();
				return;
			}

			var page = await one.GetPage(pageIds[0], OneNote.PageDetail.All);
			CliOutput = page?.Root?.ToString() ?? string.Empty;

			await Task.Yield();
		}
	}
}
