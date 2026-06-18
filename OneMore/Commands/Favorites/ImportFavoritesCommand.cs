//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Favorites
{
	using Newtonsoft.Json;
	using River.OneMoreAddIn.Cli;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Imports favorites and folders from a JSON file, merging them into the existing
	/// favorites by folder name; folders that don't already exist are created.
	/// </summary>
	internal class ImportFavoritesCommand : Command, ICliCommand
	{
		public ImportFavoritesCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		#region CLI Implementation

		public string CommandName => "ImportFavorites";


		public string Description => "Import favorites from a JSON file";


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
					ShowError(Resx.ImportFavoritesCommand_error);
				}

				return;
			}

			FavoritesCollection collection;
			try
			{
				var json = File.ReadAllText(path);
				collection = JsonConvert.DeserializeObject<FavoritesCollection>(json);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error reading favorites from {path}", exc);

				if (runningFromCli)
				{
					CliOutput = $"Error: {exc.Message}";
				}
				else
				{
					ShowError(Resx.ImportFavoritesCommand_error);
				}

				return;
			}

			if (collection is null)
			{
				if (runningFromCli)
				{
					CliOutput = $"File does not contain a valid favorites collection: {path}";
				}
				else
				{
					ShowError(Resx.ImportFavoritesCommand_error);
				}

				return;
			}

			var (imported, attempted) = MergeIntoDatabase(collection);
			ribbon?.InvalidateControl(FavoritesMenu.MenuID);

			if (runningFromCli)
			{
				CliOutput = $"{imported} of {attempted} favorites imported from {path}";
			}
			else
			{
				ShowInfo(string.Format(Resx.ImportFavoritesCommand_imported, imported, attempted));
			}
		}


		private (int imported, int attempted) MergeIntoDatabase(FavoritesCollection collection)
		{
			using var provider = new FavoritesProvider();
			var existing = provider.ReadFavorites();

			var folderIDs = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
			foreach (var folder in existing.Folders)
			{
				folderIDs[folder.Name] = folder.FolderID;
			}

			var imported = 0;
			var attempted = 0;

			foreach (var folder in collection.Folders)
			{
				if (!folderIDs.TryGetValue(folder.Name, out var folderID))
				{
					folderID = provider.CreateFolder(folder.Name);
					if (folderID == 0)
					{
						attempted += folder.Items.Count;
						continue;
					}

					folderIDs[folder.Name] = folderID;
				}

				foreach (var favorite in folder.Items)
				{
					attempted++;
					favorite.ID = 0;
					favorite.FolderID = folderID;

					if (provider.WriteFavorite(favorite, out _))
					{
						imported++;
					}
				}
			}

			foreach (var favorite in collection.Items)
			{
				attempted++;
				favorite.ID = 0;
				favorite.FolderID = 0;

				if (provider.WriteFavorite(favorite, out _))
				{
					imported++;
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
				Filter = Resx.ImportFavoritesCommand_OpenFileFilter,
				Multiselect = false,
				Title = Resx.ImportFavoritesCommand_OpenFileTitle
			};

			return dialog.ShowDialog(owner) == DialogResult.OK ? dialog.FileName : null;
		}
	}
}
