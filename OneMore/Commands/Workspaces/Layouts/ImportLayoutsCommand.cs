//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Newtonsoft.Json;
	using River.OneMoreAddIn.Cli;
	using River.OneMoreAddIn.Commands.Layouts;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Imports layouts and their windows from a JSON file, merging them into the existing
	/// layouts by layout name; layouts that don't already exist are created.
	/// </summary>
	internal class ImportLayoutsCommand : Command, ICliCommand
	{
		public ImportLayoutsCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		#region CLI Implementation

		public string CommandName => "ImportLayouts";


		public string Description => "Import layouts from a JSON file";


		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("file", "Path of the JSON file to import", required: true);

		#endregion CLI Implementation


		public override async Task Execute(params object[] args)
		{
			string path = null;

			if (runningFromCli)
			{
				var cliParams = args.Length > 0 ? args[0] as CliParameterSet : null;
				cliParams?.TryGet("file", out path);

				if (string.IsNullOrWhiteSpace(path))
				{
					CliOutput = "Specify --file with the path of the JSON file to import.";
					await Task.Yield();
					return;
				}
			}
			else
			{
				// OpenFileDialog must run in an STA thread
				path = await SingleThreaded.Invoke(ChooseFile);
				if (path == null)
				{
					return;
				}
			}

			if (!File.Exists(path))
			{
				if (runningFromCli)
				{
					CliOutput = $"File not found: {path}";
				}
				else
				{
					ShowError(Resx.ImportLayoutsCommand_error);
				}

				return;
			}

			LayoutsCollection collection;
			try
			{
				var json = File.ReadAllText(path);
				collection = JsonConvert.DeserializeObject<LayoutsCollection>(json);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error reading layouts from {path}", exc);

				if (runningFromCli)
				{
					CliOutput = $"Error: {exc.Message}";
				}
				else
				{
					ShowError(Resx.ImportLayoutsCommand_error);
				}

				return;
			}

			if (collection is null)
			{
				if (runningFromCli)
				{
					CliOutput = $"File does not contain a valid layouts collection: {path}";
				}
				else
				{
					ShowError(Resx.ImportLayoutsCommand_error);
				}

				return;
			}

			var (imported, attempted) = MergeIntoDatabase(collection);

			if (runningFromCli)
			{
				CliOutput = $"{imported} of {attempted} layout windows imported from {path}";
			}
			else
			{
				ShowInfo(string.Format(Resx.ImportLayoutsCommand_imported, imported, attempted));
			}
		}


		private (int imported, int attempted) MergeIntoDatabase(LayoutsCollection collection)
		{
			using var provider = new LayoutsProvider();
			var existing = provider.ReadLayouts();

			var layoutIDs = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
			foreach (var layout in existing.Layouts)
			{
				layoutIDs[layout.Name] = layout.LayoutID;
			}

			var imported = 0;
			var attempted = 0;

			foreach (var layout in collection.Layouts)
			{
				if (!layoutIDs.TryGetValue(layout.Name, out var layoutID))
				{
					layoutID = provider.CreateLayout(layout.Name);
					if (layoutID == 0)
					{
						attempted += layout.Windows.Count;
						continue;
					}

					layoutIDs[layout.Name] = layoutID;
				}

				foreach (var window in layout.Windows)
				{
					attempted++;
					window.ID = 0;
					window.LayoutID = layoutID;

					if (provider.WriteWindow(window, out _))
					{
						imported++;
					}
				}
			}

			return (imported, attempted);
		}


		private string ChooseFile()
		{
			using var dialog = new OpenFileDialog
			{
				AddExtension = true,
				CheckFileExists = true,
				DefaultExt = ".json",
				Filter = Resx.ImportLayoutsCommand_OpenFileFilter,
				Multiselect = false,
				Title = Resx.ImportLayoutsCommand_OpenFileTitle
			};

			return dialog.ShowDialog(owner) == DialogResult.OK ? dialog.FileName : null;
		}
	}
}
