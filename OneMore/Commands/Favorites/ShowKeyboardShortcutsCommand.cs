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
				var results = one.SearchMeta(string.Empty, "omKeyboardShortcuts");
				var ns = one.GetNamespace(results);

				logger.WriteLine(results);

				var pageId = results?.Descendants(ns + "Meta")
					.Where(e =>
						e.Attribute("name").Value == "omKeyboardShortcuts" &&
						e.Parent.Attribute("isInRecycleBin") == null)
					.Select(e => e.Parent.Attribute("ID").Value)
					.FirstOrDefault();

				if (pageId == null)
				{
					pageId = await DownloadTemplatePage();
				}

				if (pageId != null)
				{
					one.NavigateTo(pageId);
				}
				else
				{
					UIHelper.ShowMessage("Could not download or import template, see log file for details");
				}
			}
		}


		private async Task<string> DownloadTemplatePage()
		{
			logger.WriteLine("downloading template");

			ServicePointManager.SecurityProtocol =
				SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

			var client = new HttpClient();
			client.DefaultRequestHeaders.Add("User-Agent", "OneMore");

			string pageId = null;

			try
			{
				using (var response = await client.GetAsync(TemplateUrl))
				{
					var path = Path.GetTempFileName();
					using (var output = File.Create(path))
					{
						await response.Content.CopyToAsync(output);
					}

					logger.WriteLine($"extracting {path}");

					using (var fs = new FileStream(path, FileMode.Open))
					using (var zip = new ZipArchive(fs))
					{
						var entry = zip.Entries.First();

						using (var sr = new StreamReader(entry.Open()))
						{
							pageId = ImportTemplate(new Page(XElement.Parse(sr.ReadToEnd())));
						}
					}

					if (File.Exists(path))
					{
						File.Delete(path);
					}
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error downloading shortcuts template", exc);
			}

			return pageId;
		}


		private string ImportTemplate(Page template)
		{
			try
			{
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