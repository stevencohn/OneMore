//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Cli;
	using System;
	using System.Linq;
	using System.Threading.Tasks;


	/// <summary>
	/// Syncs one or all loaded notebooks to their storage location. Fire-and-forget;
	/// OneNote provides no way to detect when a sync actually completes.
	/// </summary>
	internal class SyncCommand : Command, ICliCommand
	{
		public SyncCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		#region CLI Implementation

		public string CommandName => "Sync";


		public string Description => "Sync one or all loaded notebooks to storage";


		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("notebook", "Name of notebook to sync, or * for all loaded notebooks",
				required: true);

		#endregion CLI Implementation


		public override async Task Execute(params object[] args)
		{
			var cliParams = args.Length > 0 ? args[0] as CliParameterSet : null;

			string notebookName = null;
			cliParams?.TryGet("notebook", out notebookName);

			await using var one = new OneNote();

			var notebooks = await one.GetNotebooks(OneNote.Scope.Notebooks);
			if (notebooks is null || !notebooks.HasElements)
			{
				return;
			}

			var ns = one.GetNamespace(notebooks);
			var all = notebooks.Elements(ns + "Notebook");

			var targets = string.IsNullOrWhiteSpace(notebookName) || notebookName == "*"
				? all
				: all.Where(n => string.Equals(
					n.Attribute("name")?.Value, notebookName,
					StringComparison.InvariantCultureIgnoreCase));

			var synced = targets.ToList();
			foreach (var notebook in synced)
			{
				var id = notebook.Attribute("ID")?.Value;
				if (!string.IsNullOrEmpty(id))
				{
					await one.Sync(id);
				}
			}

			CliOutput = synced.Count == 0
				? "No matching notebooks found"
				: $"Synced {synced.Count} notebook(s): " +
					string.Join(", ", synced.Select(n => n.Attribute("name")?.Value));
		}
	}
}
