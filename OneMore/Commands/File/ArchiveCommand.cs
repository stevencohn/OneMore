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
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class ArchiveCommand : Command
	{
		private OneNote one;
		private ZipArchive archive;


		public ArchiveCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
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

				string path = ChooseLocation(root.Attribute("name").Value);
				if (path == null)
				{
					return;
				}

				using (var stream = new FileStream(path, FileMode.Create))
				{
					using (archive = new ZipArchive(stream, ZipArchiveMode.Create))
					{
						await Archive(root, string.Empty);
					}
				}
			}
		}


		private string ChooseLocation(string name)
		{
			string path;
			using (var dialog = new OpenFileDialog
			{
				AddExtension = true,
				CheckFileExists = true,
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
					await Archive(one.GetPage(
						element.Attribute("ID").Value, OneNote.PageDetail.BinaryData));
				}
				else
				{
					element.ReadAttributeValue("locked", out bool locked, false);
					element.ReadAttributeValue("isRecycleBin", out bool recycled, false);
					if (!locked && !recycled)
					{
						// SectionGroup or Section
						path = path == null
							? $"{element.Attribute("name").Value}/"
							: $"{path}/{element.Attribute("name").Value}/";

						archive.CreateEntry(path);

						await Archive(element, path);
					}
				}
			}
		}


		private async Task Archive(Page page)
		{
			var name = page.Title;

			MemoryStream reader = null;

			var entry = archive.CreateEntry(name);
			using (var writer = entry.Open())
			{
				await reader.CopyToAsync(writer);
			}
		}
	}
}