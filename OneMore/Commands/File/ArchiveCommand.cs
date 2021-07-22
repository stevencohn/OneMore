//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.IO;
	using System.IO.Compression;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class ArchiveCommand : Command
	{
		private OneNote one;
		private Archivist archivist;
		private ZipArchive archive;
		private XElement hierarchy;
		private string zipPath;
		private string tempdir;
		private int totalCount;
		private int pageCount = 0;
		private bool bookScope;


		public ArchiveCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var scope = args[0] as string;

			using (one = new OneNote())
			{
				bookScope = scope == "notebook";

				hierarchy = bookScope
					? one.GetNotebook(one.CurrentNotebookId, OneNote.Scope.Pages)
					: one.GetSection(one.CurrentSectionId);

				var ns = one.GetNamespace(hierarchy);

				totalCount = hierarchy.Descendants(ns + "Page").Count();
				if (totalCount == 0)
				{
					UIHelper.ShowMessage(Resx.ArchiveCommand_noPages);
					return;
				}

				var topName = hierarchy.Attribute("name").Value;
				zipPath = await SingleThreaded.Invoke(() =>
				{
					// OpenFileDialog must run in STA thread
					return ChooseLocation(topName);
				});

				if (zipPath == null)
				{
					return;
				}

				var progressDialog = new UI.ProgressDialog(Execute);
				await progressDialog.RunModeless();
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		// Invoked by the ProgressDialog OnShown callback
		private async Task Execute(UI.ProgressDialog progress, CancellationToken token)
		{
			logger.Start();
			logger.StartClock();

			archivist = new Archivist(one, zipPath);
			await archivist.BuildHyperlinkMap(
				bookScope ? OneNote.Scope.Sections : OneNote.Scope.Pages,
				progress,
				token);

			// use this temp folder as a sandbox for each page
			var t = Path.GetTempFileName();
			tempdir = Path.Combine(Path.GetDirectoryName(t), Path.GetFileNameWithoutExtension(t));
			PathFactory.EnsurePathExists(tempdir);
			logger.WriteLine($"building archive {zipPath}");

			progress.SetMaximum(totalCount);
			progress.SetMessage($"Archiving {totalCount} pages");

			using (var stream = new FileStream(zipPath, FileMode.Create))
			{
				using (archive = new ZipArchive(stream, ZipArchiveMode.Create))
				{
					await Archive(progress, hierarchy, hierarchy.Attribute("name").Value);
				}
			}

			Directory.Delete(tempdir, true);

			progress.Close();
			UIHelper.ShowMessage(string.Format(Resx.ArchiveCommand_archived, pageCount, zipPath));

			logger.WriteTime("archive complete");
			logger.End();
		}


		private string ChooseLocation(string name)
		{
			string path;
			using (var dialog = new OpenFileDialog
			{
				AddExtension = true,
				CheckFileExists = false,
				DefaultExt = ".zip",
				Filter = Resx.ArchiveCommand_OpenFileFilter,
				FileName = name,
				//InitialDirectory = path,
				Multiselect = false,
				Title = Resx.ArchiveCommand_OpenFileTitle
			})
			{
				// cannot use owner parameter here or it will hang! cross-threading
				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return null;
				}

				path = dialog.FileName;
			}

			var dir = Path.GetDirectoryName(path);
			if (!Directory.Exists(dir))
			{
				UIHelper.ShowMessage(Resx.ArchiveCommand_noDirectory);
				return null;
			}

			return path;
		}


		private async Task Archive(UI.ProgressDialog progress, XElement root, string path)
		{
			foreach (var element in root.Elements())
			{
				if (element.Name.LocalName == "Page")
				{
					var page = one.GetPage(
						element.Attribute("ID").Value, OneNote.PageDetail.BinaryData);

					progress.SetMessage($"Archiving {page.Title}");
					progress.Increment();

					await ArchivePage(element, page, path);
				}
				else
				{
					// SectionGroup or Section

					element.GetAttributeValue("locked", out bool locked, false);
					if (locked) continue;

					element.GetAttributeValue("isRecycleBin", out bool recycle, false);
					if (recycle) continue;

					// append name of Section/Group to path to build zip folder path
					var name = element.Attribute("name").Value;

					await Archive(progress, element, Path.Combine(path, name));
				}
			}
		}


		private async Task ArchivePage(XElement element, Page page, string path)
		{
			CleanupTemp();

			var name = PathFactory.CleanFileName(page.Title).Trim();
			if (string.IsNullOrEmpty(name))
			{
				name = $"Unnamed__{pageCount}";
			}
			else
			{
				// ensure the page name is unique within the section
				var n = element.Parent.Elements()
					.Count(e => e.Attribute("name")?.Value ==
						PathFactory.CleanFileName(page.Title).Trim());

				if (n > 1)
				{
					name = $"{name}_{element.ElementsBeforeSelf().Count()}";
				}
			}

			var filename = string.IsNullOrEmpty(path)
				? Path.Combine(tempdir, $"{name}.htm")
				: Path.Combine(tempdir, Path.Combine(path, $"{name}.htm"));

			archivist.ExportHTML(page, ref filename, path, bookScope);

			await ArchiveAssets(Path.GetDirectoryName(filename), path);

			pageCount++;
		}


		private async Task ArchiveAssets(string location, string path)
		{
			if (!Directory.Exists(location))
			{
				return;
			}

			// directory contains both the page HTML and attachment files
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

				await ArchiveAssets(dir.FullName, dname);
			}
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
	}
}