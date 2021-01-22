//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.IO;
	using System.IO.Compression;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class ArchiveCommand : Command
	{
		private OneNote one;
		private Archivist archivist;
		private ZipArchive archive;
		private string tempdir;


		public ArchiveCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{

			System.Diagnostics.Debugger.Launch();


			var scope = args[0] as string;
			using (one = new OneNote())
			{
				XElement root = scope == "notebook"
					? one.GetNotebook(one.CurrentNotebookId, OneNote.Scope.Pages)
					: one.GetSection(one.CurrentSectionId);

				var ns = one.GetNamespace(root);

				if (!root.Descendants(ns + "Page").Any())
				{
					// no pages!
					return;
				}

				string path = await SingleThreaded.Invoke(() =>
				{
					// OpenFileDialog must run in STA thread
					return ChooseLocation(root.Attribute("name").Value);
				});

				if (path == null)
				{
					return;
				}

				// use this temp folder as a sandbox for each page
				var t = Path.GetTempFileName();
				tempdir = Path.Combine(Path.GetDirectoryName(t), Path.GetFileNameWithoutExtension(t));
				PathFactory.EnsurePathExists(tempdir);

				using (var stream = new FileStream(path, FileMode.Create))
				{
					using (archive = new ZipArchive(stream, ZipArchiveMode.Create))
					{
						archivist = new Archivist(one);

						await Archive(root, null);
					}
				}

				Directory.Delete(tempdir, true);
			}
		}


		private string ChooseLocation(string name)
		{
			string path;
			using (var dialog = new OpenFileDialog
			{
				AddExtension = true,
				CheckFileExists = false,
				DefaultExt = ".zip",
				Filter = "Zip File (*.zip)|*.zip", // Resx.ArchiveCommand_OpenFileFilter,
				FileName = name,
				//InitialDirectory = path,
				Multiselect = false,
				Title = "Archive Location" // Resx.ArchiveCommand_OpenFileTitle
			})
			{
				// cannot use owner parameter here or it will hang! cross-threading
				if (dialog.ShowDialog() != DialogResult.OK)
				{
					return null;
				}

				path = dialog.FileName;
			}

			var dir = Path.GetDirectoryName(path);
			if (!Directory.Exists(dir))
			{
				// choose valid directory
				return null;
			}

			return path;
		}


		private async Task Archive(XElement root, string path)
		{
			foreach (var element in root.Elements())
			{
				if (element.Name.LocalName == "Page")
				{
					var page = one.GetPage(
						element.Attribute("ID").Value, OneNote.PageDetail.BinaryData);

					await ArchivePage(page, path);
				}
				else
				{
					// SectionGroup or Section
					element.ReadAttributeValue("locked", out bool locked, false);
					element.ReadAttributeValue("isRecycleBin", out bool recycled, false);
					if (!locked && !recycled)
					{
						// append name of Section/Group to path to build zip folder path
						path = path == null
							? element.Attribute("name").Value
							: Path.Combine(path, element.Attribute("name").Value);

						await Archive(element, path);
					}
				}
			}
		}


		private async Task ArchivePage(Page page, string path)
		{
			CleanupTemp();

			var name = PathFactory.CleanFileName(page.Title);

			var filename = path == null
				? Path.Combine(tempdir, $"{name}.htm")
				: Path.Combine(tempdir, Path.Combine(path, $"{name}.htm"));

			archivist.SaveAsHTML(page, ref filename, true);

			await ArchivePageFiles(Path.GetDirectoryName(filename), path);
		}


		private void CleanupTemp()
		{
			var temp = new DirectoryInfo(tempdir);
			foreach (FileInfo file in temp.GetFiles())
			{
				file.Delete();
			}

			foreach (DirectoryInfo dir in temp.GetDirectories())
			{
				dir.Delete(true);
			}
		}


		private async Task ArchivePageFiles(string location, string path)
		{
			if (!Directory.Exists(location))
			{
				return;
			}

			var info = new DirectoryInfo(location);

			foreach (FileInfo file in info.GetFiles())
			{
				using (var reader = new FileStream(file.FullName, FileMode.Open))
				{
					var name = path == null
						? file.Name
						: Path.Combine(path, file.Name);

					var entry = archive.CreateEntry(name, CompressionLevel.Optimal);
					using (var writer = new StreamWriter(entry.Open()))
					{
						await reader.CopyToAsync(writer.BaseStream);
					}
				}
			}

			foreach (DirectoryInfo dir in info.GetDirectories())
			{
				var dname = path == null
					? dir.Name
					: Path.Combine(path, dir.Name);

				await ArchivePageFiles(dir.FullName, dname);
			}
		}
	}
}