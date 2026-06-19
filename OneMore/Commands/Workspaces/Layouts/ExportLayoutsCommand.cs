//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Newtonsoft.Json;
	using River.OneMoreAddIn.Cli;
	using River.OneMoreAddIn.Commands.Layouts;
	using System;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Exports the current layouts and their windows to a JSON file.
	/// </summary>
	internal class ExportLayoutsCommand : Command, ICliCommand
	{
		public ExportLayoutsCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		#region CLI Implementation

		public string CommandName => "ExportLayouts";


		public string Description => "Export layouts to a JSON file";


		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("file", "Path of the JSON file to write", required: true);

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
					CliOutput = "Specify --file with the path of the JSON file to write.";
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

			using var provider = new LayoutsProvider();
			var collection = provider.ReadLayouts();
			var count = collection.Layouts.Sum(l => l.Windows.Count);

			try
			{
				var json = JsonConvert.SerializeObject(collection, Formatting.Indented);
				File.WriteAllText(path, json);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error exporting layouts to {path}", exc);

				if (runningFromCli)
				{
					CliOutput = $"Error: {exc.Message}";
				}
				else
				{
					ShowError(Resx.ExportLayoutsCommand_error);
				}

				return;
			}

			if (runningFromCli)
			{
				CliOutput = string.Format(Resx.ExportLayoutsCommand_exported, count, path);
			}
			else
			{
				ShowInfo(string.Format(Resx.ExportLayoutsCommand_exported, count, path));
			}
		}


		private string ChooseFile()
		{
			using var dialog = new OpenFileDialog
			{
				AddExtension = true,
				CheckFileExists = false,
				DefaultExt = ".json",
				FileName = "Layouts.json",
				Filter = Resx.ExportLayoutsCommand_OpenFileFilter,
				Multiselect = false,
				Title = Resx.ExportLayoutsCommand_OpenFileTitle
			};

			return dialog.ShowDialog(owner) == DialogResult.OK ? dialog.FileName : null;
		}
	}
}
