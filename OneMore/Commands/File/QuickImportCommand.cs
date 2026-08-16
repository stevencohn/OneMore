//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.UI;
	using System;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using Resx = Properties.Resources;


	/// <summary>
	/// Silently imports every recognized file from a preconfigured folder into a preconfigured
	/// section, using the folder/section set on the Quick Import group of the File Import
	/// settings sheet. Presents no UI of its own beyond warning/result message boxes.
	/// </summary>
	internal class QuickImportCommand : Command
	{
		private static readonly string[] KnownExtensions =
		{
			".docx", ".doc", ".pptx", ".ppt", ".pdf", ".md", ".one", ".txt", ".xml"
		};


		public override async Task Execute(params object[] args)
		{
			var settings = new SettingsProvider().GetCollection(nameof(FileImportSheet));
			var folder = settings.Get<string>("quickImportFolder");
			var sectionId = settings.Get<string>("quickImportSectionID");

			if (string.IsNullOrWhiteSpace(folder) || string.IsNullOrWhiteSpace(sectionId))
			{
				MoreMessageBox.ShowWarning(owner, Resx.QuickImportCommand_SettingsNotSet);
				return;
			}

			if (!Directory.Exists(folder))
			{
				MoreMessageBox.ShowWarning(owner, Resx.QuickImportCommand_InvalidPath);
				return;
			}

			await using (var one = new OneNote())
			{
				var info = await one.GetSectionInfo(sectionId);
				if (info == null)
				{
					MoreMessageBox.ShowWarning(owner, Resx.QuickImportCommand_SectionNotFound);
					return;
				}
			}

			var files = Directory.GetFiles(folder)
				.Where(f => KnownExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
				.ToArray();

			if (files.Length == 0)
			{
				MoreMessageBox.ShowWarning(owner, Resx.QuickImportCommand_NoFilesFound);
				return;
			}

			var importer = new ImportCommand();
			importer.SetLogger(logger);
			importer.SetOwner(owner);

			var importedFolder = Path.Combine(folder, Resx.QuickImportCommand_ImportedFolderName);

			var good = 0;
			var failed = 0;

			var timeout = 10 + (files.Length * 5);

			importer.RunWithProgress(timeout, folder, async (token) =>
			{
				foreach (var file in files)
				{
					if (token.IsCancellationRequested)
					{
						break;
					}

					var succeeded = Path.GetExtension(file).ToLowerInvariant() switch
					{
						".doc" or ".docx" =>
							await importer.ImportWordFile(file, false, token, sectionId),
						".ppt" or ".pptx" =>
							await importer.ImportPowerPointFile(file, false, false, token, sectionId),
						".pdf" =>
							await importer.ImportPdfFile(file, false, token, sectionId),
						".md" =>
							await importer.ImportMarkdownFile(file, token, sectionId),
						".txt" =>
							await importer.ImportTextFile(file, token, sectionId),
						".xml" =>
							await importer.ImportXml(file, sectionId),
						".one" =>
							await importer.ImportOneNote(file, sectionId),
						_ => false
					};

					if (succeeded)
					{
						good++;
						MoveImportedFile(file, importedFolder);
					}
					else
					{
						failed++;
					}
				}

				return !token.IsCancellationRequested;
			});

			ShowInfo(string.Format(
				Resx.QuickImportCommand_ResultFormat, good, files.Length, failed));
		}


		private void MoveImportedFile(string file, string importedFolder)
		{
			try
			{
				Directory.CreateDirectory(importedFolder);
				var target = Path.Combine(importedFolder, Path.GetFileName(file));

				if (File.Exists(target))
				{
					var name = Path.GetFileNameWithoutExtension(file);
					var ext = Path.GetExtension(file);
					var stamp = $"{DateTime.Now:yyyyMMddHHmmss}";

					target = Path.Combine(importedFolder, $"{name}_{stamp}{ext}");
				}

				File.Move(file, target);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error moving {file} to {importedFolder}", exc);
			}
		}
	}
}
