//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S1075 // URIs should not be hardcoded

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.IO;
	using System.IO.Compression;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class ShowKeyboardShortcutsCommand : Command
	{
		private const string TemplateUrl =
			"https://github.com/stevencohn/OneMore/raw/master/Templates/OneNote_Keyboard_Shortcuts.zip";

		private OneNote one;


		public ShowKeyboardShortcutsCommand()
		{
		}


		public override async void Execute(params object[] args)
		{
			using (one = new OneNote())
			{
				var context = SynchronizationContext.Current;

				var results = one.SearchMeta(string.Empty, "omKeyboardShortcuts");
				var ns = one.GetNamespace(results);

				var pageId = results?.Descendants(ns + "Meta")
					.Where(e =>
						e.Attribute("name").Value == "omKeyboardShortcuts" &&
						e.Parent.Attribute("isInRecycleBin") == null)
					.Select(e => e.Parent.Attribute("ID").Value)
					.FirstOrDefault();

				if (pageId == null)
				{
					var path = await DownloadTemplate().ConfigureAwait(false);
					if (path != null)
					{
						var page = ExtractTemplate(path);
						if (page != null)
						{
							pageId = ImportTemplate(page);
						}

						File.Delete(path);
					}
				}

				await context;

				if (pageId != null)
				{
					logger.WriteLine("navigating to page");
					one.Sync();
					one.NavigateTo(pageId);
				}
				else
				{
					UIHelper.ShowMessage("Could not download or import template, see log file for details");
				}
			}
		}


		private async Task<string> DownloadTemplate()
		{
			logger.Start();
			logger.WriteLine("downloading template");

			ServicePointManager.SecurityProtocol =
				SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

			var client = new HttpClient();
			client.DefaultRequestHeaders.Add("User-Agent", "OneMore");

			string path = null;

			try
			{
				using (var response = await client.GetAsync(TemplateUrl).ConfigureAwait(false))
				{
					path = Path.GetTempFileName();
					using (var output = File.Create(path))
					{
						await response.Content.CopyToAsync(output).ConfigureAwait(false);
					}

					return path;
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error downloading shortcuts template", exc);
			}

			return path;
		}


		private Page ExtractTemplate(string path)
		{
			logger.Start();
			logger.WriteLine($"extracting {path}");

			try
			{
				using (var stream = new FileStream(path, FileMode.Open))
				using (var archive = new ZipArchive(stream))
				{
					var entry = archive.Entries.First();

					using (var reader = new StreamReader(entry.Open()))
					{
						return new Page(XElement.Parse(reader.ReadToEnd()));
					}
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error extracting template", exc);
				return null;
			}
		}


		private string ImportTemplate(Page template)
		{
			try
			{
				logger.Start();
				logger.WriteLine("importing template");
				one.CreatePage(one.CurrentSectionId, out var pageId);

				// remove any objectID values and let OneNote generate new IDs
				template.Root.Descendants().Attributes("objectID").Remove();

				// set the page ID to the new page's ID
				template.Root.Attribute("ID").Value = pageId;

				if (string.IsNullOrEmpty(template.Title))
				{
					template.Title = "OneNote Keyboard Shortcuts";
				}

				one.Update(template);

				return pageId;
			}
			catch (Exception exc)
			{
				logger.WriteLine("error importing template", exc);
				return null;
			}
		}
	}
}
