//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
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
	internal class ArchiveCommand : Command
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


		public override async Task Execute(params object[] args)
		{
			var scope = args[0] as string;

			await using (one = new OneNote())
			{
				bookScope = scope == "notebook";

				hierarchy = bookScope
					? await one.GetNotebook(one.CurrentNotebookId, OneNote.Scope.Pages)
					: await one.GetSection(one.CurrentSectionId);

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

					progress.SetMessage($"Archiving {page.Title ?? Resx.phrase_QuickNote}");
					progress.Increment();

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

			var tpath = string.IsNullOrEmpty(path) ? tempdir : Path.Combine(tempdir, path);
			var filename = PathHelper.GetUniqueQualifiedFileName(tpath, ref name, ".htm");

			if (filename is not null)
			{
				filename = await archivist.ExportHTML(page, filename, path, bookScope);
				await ArchiveAssets(Path.GetDirectoryName(filename), path);
				pageCount++;
				return name;
			}

			logger.WriteLine($"archive path too long [{tpath}\\{name}.htm]");
			return null;
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