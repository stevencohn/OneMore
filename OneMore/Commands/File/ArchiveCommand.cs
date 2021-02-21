//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#define LogArc
#define _Choose

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
		private string tempdir;
		private int pageCount = 0;
		private bool bookScope;
		private CancellationTokenSource source;


		public ArchiveCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var scope = args[0] as string;
			using (one = new OneNote())
			{

				System.Diagnostics.Debugger.Launch();

				bookScope = scope == "notebook";

				XElement root = bookScope
					? one.GetNotebook(one.CurrentNotebookId, OneNote.Scope.Pages)
					: one.GetSection(one.CurrentSectionId);

				var ns = one.GetNamespace(root);

				if (!root.Descendants(ns + "Page").Any())
				{
					UIHelper.ShowMessage(Resx.ArchiveCommand_noPages);
					return;
				}

				var hierarchy = root.Attribute("name").Value;

				string path = await SingleThreaded.Invoke(() =>
				{
					// OpenFileDialog must run in STA thread
					return ChooseLocation(hierarchy);
				});

				if (path == null)
				{
					return;
				}

				source = new CancellationTokenSource();

				archivist = new Archivist(one, path);
				archivist.BuildHyperlinkMap(
					bookScope ? OneNote.Scope.Sections : OneNote.Scope.Pages,
					source.Token);

				// use this temp folder as a sandbox for each page
				var t = Path.GetTempFileName();
				tempdir = Path.Combine(Path.GetDirectoryName(t), Path.GetFileNameWithoutExtension(t));
				PathFactory.EnsurePathExists(tempdir);
#if LogArc
				logger.WriteLine("building archive");
				logger.WriteLine(root);
#endif
				using (var stream = new FileStream(path, FileMode.Create))
				{
					using (archive = new ZipArchive(stream, ZipArchiveMode.Create))
					{
						await Archive(root, root.Attribute("name").Value);
					}
				}

				Directory.Delete(tempdir, true);

				UIHelper.ShowMessage(string.Format(Resx.ArchiveCommand_archived, pageCount, path));
			}
		}


		private string ChooseLocation(string name)
		{
#if Choose
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
				if (dialog.ShowDialog() != DialogResult.OK)
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
#else
			return @"C:\Users\steven\Downloads\Foo\Bar.zip";
#endif
		}


		private async Task Archive(XElement root, string path)
		{
			logger.WriteLine($"ARC r00t:{root.Name.LocalName}={root.Attribute("name").Value} path:{path}");

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
#if LogArc
					logger.WriteLine($"ARC root:{root.Name.LocalName}={root.Attribute("name").Value} element:{element.Name.LocalName}={element.Attribute("name").Value} path:{path}");
#endif
					element.ReadAttributeValue("locked", out bool locked, false);
					if (locked) continue;

					element.ReadAttributeValue("isRecycleBin", out bool recycle, false);
					if (recycle) continue;

					// append name of Section/Group to path to build zip folder path
					var name = element.Attribute("name").Value;

					await Archive(element, Path.Combine(path, name));
				}
			}
		}


		private async Task ArchivePage(Page page, string path)
		{
			CleanupTemp();

			var name = PathFactory.CleanFileName(page.Title).Trim();
			if (string.IsNullOrEmpty(name))
			{
				name = $"Unnamed__{pageCount}";
			}

			var filename = string.IsNullOrEmpty(path)
				? Path.Combine(tempdir, $"{name}.htm")
				: Path.Combine(tempdir, Path.Combine(path, $"{name}.htm"));
#if LogArc
			logger.WriteLine($"ARC path:[{path}] filename:[{filename}]");
#endif
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