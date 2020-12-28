//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S1075 // URIs should not be hardcoded

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.IO;
	using System.IO.Compression;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Threading.Tasks;

	internal class ShowKeyboardShortcutsCommand : Command
	{
		private const string TemplateUrl =
			"https://github.com/stevencohn/OneMore/raw/master/Templates/OneNote_Keyboard_Shortcuts.zip";


		public ShowKeyboardShortcutsCommand()
		{
		}


		public override async void Execute(params object[] args)
		{
			using (var one = new OneNote())
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
			}
		}


		private async Task<string> DownloadTemplatePage()
		{
			ServicePointManager.SecurityProtocol =
				SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

			var client = new HttpClient();
			client.DefaultRequestHeaders.Add("User-Agent", "OneMore");

			string pageId = null;

			try
			{
				using (var response = await client.GetAsync(TemplateUrl))
				{
					using (var dstream = new DeflateStream(
						await response.Content.ReadAsStreamAsync(),
						CompressionMode.Decompress))
					{
						var path = Path.GetTempFileName();
						using (var output = File.Create(path))
						{
							await dstream.CopyToAsync(output);
							logger.WriteLine(path);
						}
					}
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error downloading shortcuts template", exc);
				return null;
			}

			return pageId;
		}
	}
}