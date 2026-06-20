//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Favorites
{
	using Newtonsoft.Json;
	using River.OneMoreAddIn.Cli;
	using System;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Exports the current favorites and folders to a JSON file.
	/// </summary>
	internal class ExportFavoritesCommand : Command, ICliCommand
	{
		public ExportFavoritesCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		#region CLI Implementation

		public string CommandName => "ExportFavorites";


		public string Description => "Export favorites to a JSON file";


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

			using var provider = new FavoritesProvider();
			var collection = provider.ReadFavorites();
			var count = collection.Items.Count + collection.Folders.Sum(f => f.Items.Count);

			try
			{
				var json = JsonConvert.SerializeObject(collection, Formatting.Indented);
				File.WriteAllText(path, json);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error exporting favorites to {path}", exc);

				if (runningFromCli)
				{
					CliOutput = $"Error: {exc.Message}";
				}
				else
				{
					ShowError(Resx.ExportFavoritesCommand_error);
				}

				return;
			}

			if (runningFromCli)
			{
				CliOutput = string.Format(Resx.ExportFavoritesCommand_exported, count, path);
			}
			else
			{
				ShowInfo(string.Format(Resx.ExportFavoritesCommand_exported, count, path));
			}
		}


		private string ChooseFile()
		{
			using var dialog = new OpenFileDialog
			{
				AddExtension = true,
				CheckFileExists = false,
				DefaultExt = ".json",
				FileName = "Favorites.json",
				Filter = Resx.ExportFavoritesCommand_OpenFileFilter,
				Multiselect = false,
				Title = Resx.ExportFavoritesCommand_OpenFileTitle
			};

			return dialog.ShowDialog(owner) == DialogResult.OK ? dialog.FileName : null;
		}
	}
}
