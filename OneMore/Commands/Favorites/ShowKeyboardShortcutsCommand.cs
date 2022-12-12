//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S1075 // URIs should not be hardcoded

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.UI;
	using System;
	using System.IO;
	using System.IO.Compression;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class ShowKeyboardShortcutsCommand : Command
	{
		private const string TemplateUrl =
			"https://github.com/stevencohn/OneMore/raw/main/Templates/OneNote_Keyboard_Shortcuts.zip";

		private OneNote one;


		public ShowKeyboardShortcutsCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			if (!HttpClientFactory.IsNetworkAvailable())
			{
				UIHelper.ShowInfo(Properties.Resources.NetwordConnectionUnavailable);
				return;
			}

			using (one = new OneNote())
			{
				var context = SynchronizationContext.Current;

				var results = await one.SearchMeta(string.Empty, "omKeyboardShortcuts");
				if (results == null)
				{
					UIHelper.ShowInfo(one.Window, "Could not show page at this time. Restart OneNote");
					return;
				}

				var ns = one.GetNamespace(results);

				var pageId = results?.Descendants(ns + "Meta")
					.Where(e => e.Attribute("name").Value == "omKeyboardShortcuts")
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
							pageId = await ImportTemplate(page);
						}

						File.Delete(path);
					}
				}

				await context;

				if (pageId != null)
				{
					logger.WriteLine("navigating to page");
					await one.Sync();
					await one.NavigateTo(pageId);
				}
				else
				{
					MoreMessageBox.ShowErrorWithLogLink(owner,
						"Could not download or import template, see log file for details");
				}
			}
		}


		private async Task<string> DownloadTemplate()
		{
			logger.Start();
			logger.WriteLine("downloading template");

			var client = HttpClientFactory.Create();

			string path = null;

			try
			{
				using var response = await client.GetAsync(TemplateUrl).ConfigureAwait(false);
				if (response.IsSuccessStatusCode)
				{
					path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

					using var output = File.Create(path);
					await response.Content.CopyToAsync(output).ConfigureAwait(false);

					return path;
				}

				logger.WriteLine($"error download shortcuts template ({response.StatusCode}): {response.ReasonPhrase}, {TemplateUrl}");
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
				using var stream = new FileStream(path, FileMode.Open);
				using var archive = new ZipArchive(stream);
				var entry = archive.Entries.First();

				using var reader = new StreamReader(entry.Open());
				return new Page(XElement.Parse(reader.ReadToEnd()));
			}
			catch (Exception exc)
			{
				logger.WriteLine("error extracting template", exc);
				return null;
			}
		}


		private async Task<string> ImportTemplate(Page template)
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

				await one.Update(template);

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
