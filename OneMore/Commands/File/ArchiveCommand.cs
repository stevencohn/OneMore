//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Cli;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.IO.Compression;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Accessible from the context menu for both sections and notebooks, creates a zip file of
	/// all pages in the section or notebook, exported to HTML, including all images and file
	/// attachments on each page. Also fixes the hyperlinks between pages within the context of
	/// the archive so the archive can stand on its own as a working directory of HTML files with
	/// live hyperlinks.
	/// </summary>
	internal class ArchiveCommand : Command, ICliInteractiveCommand
	{
		private const string OrderFile = "__File_Order.txt";

		private OneNote one;
		private Archivist archivist;
		private ZipArchive archive;
		private XElement hierarchy;
		private string zipPath;
		private string tempdir;
		private int totalCount;
		private int pageCount = 0;
		private int quickCount = 0;
		private bool bookScope;

		private Exception exception = null;


		public ArchiveCommand()
		{
		}


		#region CLI Implementation

		public string CommandName => "Archive";


		public string Description => "Archive a notebook or section to a zip file";


		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("notebook", "Name of the notebook to archive", required: true)
			.AddString("section", "Path of section to archive; omit to archive the entire notebook",
				required: false)
			.AddString("outfile", "Path and filename of the output zip file", required: true);

		#endregion CLI Implementation


		public override async Task Execute(params object[] args)
		{
			if (runningFromCli)
			{
				await ExecuteCli(args[0] as CliParameterSet);
				return;
			}

			var scope = args[0] as string;

			await using (one = new OneNote())
			{
				bookScope = scope.In("notebook", "sectiongroup");

				if (bookScope)
				{
					string id;
					if (scope == "notebook")
					{
						id = one.CurrentNotebookId;
					}
					else
					{
						var sectionId = one.CurrentSectionId;
						if (string.IsNullOrEmpty(sectionId))
						{
							ShowError(Resx.ArchiveCommand_noContext);
							return;
						}
						id = one.GetParent(sectionId);
					}

					if (string.IsNullOrEmpty(id))
					{
						ShowError(Resx.ArchiveCommand_noContext);
						return;
					}

					hierarchy = await one.GetNotebook(id, OneNote.Scope.Pages);
				}
				else
				{
					var sectionId = one.CurrentSectionId;
					if (string.IsNullOrEmpty(sectionId))
					{
						ShowError(Resx.ArchiveCommand_noContext);
						return;
					}

					hierarchy = await one.GetSection(sectionId);
				}

				var ns = one.GetNamespace(hierarchy);

				totalCount = hierarchy.Descendants(ns + "Page").Count();
				if (totalCount == 0)
				{
					ShowError(Resx.ArchiveCommand_noPages);
					return;
				}

				var topName = hierarchy.Attribute("name").Value.Trim();
				zipPath = await SingleThreaded.Invoke(() =>
				{
					// OpenFileDialog must run in STA thread
					return ChooseLocation(topName);
				});

				if (zipPath == null)
				{
					return;
				}

				var progressDialog = new ProgressDialog(Execute);

				// report result is needed to show UI after Execute is completed on another thread
				progressDialog.RunModeless(ReportResult);
			}

			logger.WriteLine("done");
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private async Task ExecuteCli(CliParameterSet cliParams)
		{
			cliParams.TryGet("notebook", out string notebookName);
			cliParams.TryGet("section", out string sectionPath);
			cliParams.TryGet("outfile", out string outFilePath);

			await using (one = new OneNote())
			{
				bookScope = string.IsNullOrEmpty(sectionPath);

				var notebooks = await one.GetNotebooks(OneNote.Scope.Notebooks);
				for (int attempt = 1; attempt < 4 && (notebooks == null || !notebooks.HasElements); attempt++)
				{
					await Task.Delay(500 * attempt);
					notebooks = await one.GetNotebooks(OneNote.Scope.Notebooks);
				}

				if (notebooks == null)
				{
					CliOutput = "Cannot connect to OneNote";
					return;
				}

				var ns = one.GetNamespace(notebooks);
				var notebookEl = notebooks
					.Elements(ns + "Notebook")
					.FirstOrDefault(n => string.Equals(
						n.Attribute("name")?.Value, notebookName,
						StringComparison.InvariantCultureIgnoreCase));

				if (notebookEl == null)
				{
					CliOutput = $"Notebook not found: {notebookName}";
					return;
				}

				if (bookScope)
				{
					hierarchy = await one.GetNotebook(
						notebookEl.Attribute("ID").Value, OneNote.Scope.Pages);
				}
				else
				{
					var notebookTree = await one.GetNotebook(
						notebookEl.Attribute("ID").Value, OneNote.Scope.Sections);

					if (notebookTree == null)
					{
						CliOutput = $"Cannot load notebook: {notebookName}";
						return;
					}

					var node = notebookTree;
					var parts = sectionPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

					foreach (var part in parts)
					{
						node = node.Elements().FirstOrDefault(e =>
							(e.Name.LocalName == "Section" || e.Name.LocalName == "SectionGroup") &&
							string.Equals(e.Attribute("name")?.Value, part,
								StringComparison.InvariantCultureIgnoreCase));

						if (node == null)
						{
							CliOutput = $"Section not found: {sectionPath}";
							return;
						}
					}

					var sectionId = node.Attribute("ID")?.Value;
					if (string.IsNullOrEmpty(sectionId))
					{
						CliOutput = $"Section not found: {sectionPath}";
						return;
					}

					hierarchy = await one.GetSection(sectionId);
				}

				if (hierarchy == null)
				{
					CliOutput = bookScope
						? $"Cannot load notebook: {notebookName}"
						: $"Cannot load section: {sectionPath}";
					return;
				}

				var hns = one.GetNamespace(hierarchy);
				totalCount = hierarchy.Descendants(hns + "Page").Count();
				if (totalCount == 0)
				{
					CliOutput = "No pages found";
					return;
				}

				zipPath = outFilePath;

				var dir = Path.GetDirectoryName(zipPath);
				if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
				{
					CliOutput = $"Output directory does not exist: {dir}";
					return;
				}

				logger.Start();
				logger.StartClock();

				archivist = new Archivist(one, zipPath);

				// Always use Notebooks scope in CLI: Sections/Pages scope inside
				// HyperlinkProvider asks for the "current notebook" via GetNotebook(Pages),
				// which doesn't exist outside the OneNote UI context. Notebooks scope walks
				// all notebooks — slower than the ribbon path's narrowed scope, but produces
				// a superset map that still resolves every link in the archived pages.
				await archivist.BuildHyperlinkMap(
					OneNote.Scope.Notebooks,
					null,
					CancellationToken.None);

				var t = Path.GetRandomFileName();
				tempdir = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(t));
				PathHelper.EnsurePathExists(tempdir);
				logger.WriteLine($"building archive {zipPath}");

				try
				{
					using var stream = new FileStream(zipPath, FileMode.Create);
					using (archive = new ZipArchive(stream, ZipArchiveMode.Create))
					{
						await Archive(null, hierarchy, hierarchy.Attribute("name").Value.Trim());
					}

					CliOutput = $"Archived {pageCount} of {totalCount} pages to {zipPath}";
				}
				catch (Exception exc)
				{
					logger.WriteLine("cannot create archive", exc);
					CliOutput = $"Error: {exc.Message}";
				}

				try
				{
					Directory.Delete(tempdir, true);
				}
				catch (Exception exc)
				{
					logger.WriteLine($"cannot delete {tempdir}", exc);
				}

				logger.WriteTime("archive complete");
				logger.End();
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		// Invoked by the ProgressDialog OnShown callback
		private async Task Execute(ProgressDialog progress, CancellationToken token)
		{
			logger.Start();
			logger.StartClock();

			archivist = new Archivist(one, zipPath);
			await archivist.BuildHyperlinkMap(
				bookScope ? OneNote.Scope.Sections : OneNote.Scope.Pages,
				progress,
				token);

			// use this temp folder as a sandbox for each page
			var t = Path.GetRandomFileName();
			tempdir = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(t));
			PathHelper.EnsurePathExists(tempdir);
			logger.WriteLine($"building archive {zipPath}");

			progress.SetMaximum(totalCount);
			progress.SetMessage($"Archiving {totalCount} pages");

			try
			{
				using var stream = new FileStream(zipPath, FileMode.Create);
				using (archive = new ZipArchive(stream, ZipArchiveMode.Create))
				{
					await Archive(progress, hierarchy, hierarchy.Attribute("name").Value.Trim());
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"cannot create archive", exc);
				exception = exc;
			}

			try
			{
				Directory.Delete(tempdir, true);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"cannot delete {tempdir}", exc);
			}

			progress.Close();

			logger.WriteTime("archive complete");
			logger.End();
		}


		private void ReportResult(object sender, EventArgs e)
		{
			// report results back on the main UI thread...

			if (sender is ProgressDialog progress)
			{
				// otherwise ShowMessage window will appear behind progress dialog
				progress.Visible = false;
			}

			if (exception == null)
			{
				ShowMessage(string.Format(
					Resx.ArchiveCommand_archived, pageCount, totalCount, zipPath));
			}
			else
			{
				MoreMessageBox.ShowErrorWithLogLink(owner, exception.Message);
			}
		}


		private string ChooseLocation(string name)
		{
			string path;
			using (var dialog = new System.Windows.Forms.OpenFileDialog
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
				ShowError(Resx.ArchiveCommand_noDirectory);
				return null;
			}

			return path;
		}


		private async Task Archive(ProgressDialog progress, XElement root, string path)
		{
			// keep track of the order of pages and sections
			var order = new List<string>();

			foreach (var element in root.Elements())
			{
				if (element.Name.LocalName == "Page")
				{
					var page = await one.GetPage(
						element.Attribute("ID").Value, OneNote.PageDetail.BinaryData);

					progress?.SetMessage($"Archiving {page.Title ?? Resx.phrase_QuickNote}");
					progress?.Increment();

					var name = await ArchivePage(element, page, path);
					if (name is not null)
					{
						order.Add(name);
					}

					CleanupTemp();
				}
				else
				{
					// SectionGroup or Section

					element.GetAttributeValue("locked", out bool locked, false);
					if (locked) continue;

					element.GetAttributeValue("isRecycleBin", out bool recycle, false);
					if (recycle) continue;

					// append name of Section/Group to path to build zip folder path
					var name = element.Attribute("name").Value.Trim();

					await Archive(progress, element, Path.Combine(path, name));
				}
			}

			if (order.Any())
			{
				await ArchiveOrder(order, path);
			}
		}


		private async Task<string> ArchivePage(XElement element, Page page, string path)
		{
			if (page.Title == null)
			{
				page.SetTitle(quickCount == 0
					? Resx.phrase_QuickNote
					: $"{Resx.phrase_QuickNote} ({quickCount})");

				quickCount++;
			}

			var name = PathHelper.CleanFileName(page.Title).Trim();
			if (string.IsNullOrEmpty(name))
			{
				name = $"Unnamed__{pageCount}";
			}
			else
			{
				// ensure the page name is unique within the section
				var n = element.Parent.Elements()
					.Count(e => e.Attribute("name")?.Value ==
						PathHelper.CleanFileName(page.Title).Trim());

				if (n > 1)
				{
					name = $"{name}_{element.ElementsBeforeSelf().Count()}";
				}
			}

			var tempPath = string.IsNullOrEmpty(path) ? tempdir : Path.Combine(tempdir, path);
			var filename = PathHelper.GetUniqueQualifiedFileName(tempPath, name, ".htm");
			if (filename is null)
			{
				logger.WriteLine($"archive path too long [{tempPath}\\{name}.htm]");
				return null;
			}

			filename = await archivist.ExportHTML(page, filename, path, bookScope);
			await ArchiveAssets(Path.GetDirectoryName(filename), path);
			pageCount++;
			return name;
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
				using var reader = new FileStream(
					file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);

				var name = path == null
					? file.Name
					: Path.Combine(path, file.Name);

				var entry = archive.CreateEntry(name, CompressionLevel.Optimal);
				using var writer = new StreamWriter(entry.Open());
				await reader.CopyToAsync(writer.BaseStream);
			}

			foreach (DirectoryInfo dir in info.GetDirectories())
			{
				var dname = path == null
					? dir.Name
					: Path.Combine(path, dir.Name);

				await ArchiveAssets(dir.FullName, dname);
			}
		}


		private async Task ArchiveOrder(List<string> order, string path)
		{
			var filename = Path.Combine(tempdir, OrderFile);
			File.WriteAllLines(filename, order);

			var name = Path.Combine(path, OrderFile);
			var entry = archive.CreateEntry(name, CompressionLevel.Optimal);
			using var writer = new StreamWriter(entry.Open());

			using var reader = new FileStream(
				filename, FileMode.Open, FileAccess.Read, FileShare.Read);

			await reader.CopyToAsync(writer.BaseStream);
		}


		private void CleanupTemp()
		{
			var temp = new DirectoryInfo(tempdir);
			foreach (FileInfo file in temp.GetFiles())
			{
				try
				{
					file.Delete();
				}
				catch (Exception exc)
				{
					logger.WriteLine($"cannot delete {file.FullName}", exc);
				}
			}

			foreach (DirectoryInfo dir in temp.GetDirectories())
			{
				try
				{
					// this will unset the ReadOnly flag for all files/dirs in and below dir
					dir.Attributes = FileAttributes.Normal;
					foreach (var info in dir.GetFileSystemInfos("*", SearchOption.AllDirectories))
					{
						info.Attributes = FileAttributes.Normal;
					}

					// recursively delete del
					dir.Delete(true);
				}
				catch (Exception exc)
				{
					logger.WriteLine($"cannot delete {dir.FullName}", exc);
				}
			}
		}
	}
}