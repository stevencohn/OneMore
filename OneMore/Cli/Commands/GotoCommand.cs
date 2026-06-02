//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Cli;


	[CommandService]
	internal class GotoCommand : Command, ICliInteractiveCommand
	{
		public GotoCommand()
		{
			IsCancelled = true;
		}


		#region CLI Implementation

		public string CommandName => "Goto";


		public string Description => "Navigate OneNote to a specific page or object";


		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("pageId", "ID of the page to navigate to", required: true)
			.AddString("objectId", "ID of the object on the page to navigate to",
				required: false, defaultValue: "");

		#endregion CLI Implementation


		public override async Task Execute(params object[] args)
		{
			string pageId = null;
			var objectId = string.Empty;

			if (args.Length > 0 && args[0] is CliParameterSet cliParams)
			{
				// CLI path: args[0] is a CliParameterSet
				cliParams.TryGet("pageId", out pageId);
				cliParams.TryGet("objectId", out objectId);
			}
			else if (args.Length > 0 && args[0] is string)
			{
				// Protocol path: onemore://GotoCommand/<pageId>[/<objectId>]
				pageId = args[0] as string;
				objectId = args.Length > 1 ? args[1] as string ?? string.Empty : string.Empty;
			}

			if (string.IsNullOrWhiteSpace(pageId))
			{
				CliOutput = "The --pageId argument is required.";
				await Task.Yield();
				return;
			}

			await using var one = new OneNote();
			var ok = await one.NavigateTo(pageId, objectId ?? string.Empty);

			CliOutput = ok ? string.Empty : "NavigateTo failed.";

			await Task.Yield();
		}
	}
}
