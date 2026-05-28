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
			.AddString("notebook", "Name of notebook", required: true)
			.AddString("section", "Path of section", required: true)
			.AddString("page", "Name of page", required: true);

		#endregion CLI Implementation


		public override async Task Execute(params object[] args)
		{
			var cliParams = args.Length > 0 ? args[0] as CliParameterSet : null;

			string notebookName = null;
			string sectionPath = null;
			string pageName = null;

			if (cliParams != null)
			{
				cliParams.TryGet<string>("notebook", out notebookName);
				cliParams.TryGet<string>("section", out sectionPath);
				cliParams.TryGet<string>("page", out pageName);
			}

			await using var one = new OneNote();

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
