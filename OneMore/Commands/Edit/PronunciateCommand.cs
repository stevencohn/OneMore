﻿//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS0649  // never assigned to, will always be null
#pragma warning disable S1075   // URIs should not be hardcoded
#pragma warning disable S3459   // Unassigned members should be removed

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Web.Script.Serialization;
	using System.Windows.Forms;
	using System.Xml;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class PronunciateCommand : Command
	{
		private class Phonetics
		{
			public string text;
		}

		private class Definition
		{
			public Phonetics[] phonetics;
		}


		private const string DictionaryUrl = "https://api.dictionaryapi.dev/api/v2/entries/{0}/{1}";


		public PronunciateCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out var ns))
			{
				var element = page.Root.Descendants(ns + "T")
					.FirstOrDefault(e =>
						e.Attributes("selected").Any(a => a.Value.Equals("all")) &&
						e.FirstNode.NodeType == XmlNodeType.CDATA &&
						((XCData)e.FirstNode).Value.Length > 0);

				if (element == null)
				{
					UIHelper.ShowError(Resx.Pronunciate_FullWord);
					return;
				}

				var cdata = element.GetCData();
				var wrapper = cdata.GetWrapper();

				var word = wrapper.Value;
				var spaced = char.IsWhiteSpace(word[word.Length - 1]);
				word = word.Trim();

				if (string.IsNullOrEmpty(word))
				{
					UIHelper.ShowError(Resx.Pronunciate_EmptyWord);
					return;
				}

				string isoCode;
				using (var dialog = new PronunciateDialog())
				{
					dialog.Word = word;

					if (dialog.ShowDialog(owner) != DialogResult.OK)
					{
						return;
					}

					isoCode = dialog.Language;
				}

				var ruby = await LookupPhonetics(word, isoCode);
				if (!string.IsNullOrEmpty(ruby))
				{
					if (ruby[0] != '/' || ruby[ruby.Length - 1] != '/')
					{
						ruby = $"/{ruby}/";
					}

					ruby = spaced ? ruby + ' ' : $" {ruby} ";

					element.AddAfterSelf(
						new XElement(ns + "T",
							new XCData($"<span style='color:#8496B0'>{ruby}</span>"))
						);

					await one.Update(page);
				}
			}
		}


		private async Task<string> LookupPhonetics(string word, string isoCode)
		{
			var client = HttpClientFactory.Create();
			client.Timeout = new TimeSpan(0, 0, 10);

			var url = string.Format(DictionaryUrl, isoCode, word);

			string json = null;
			int retries = 0;

			while (json == null && retries < 2)
			{
				retries++;

				try
				{
					json = await client.GetStringAsync(url);
					//logger.WriteLine(json);
				}
				//catch (HttpRequestException exc) // when (..)
				//{
				//	logger.WriteLine("error fetching definition", exc);
				//	logger.WriteLine($"retrying {(200 & retries)}ms");
				//	await Task.Delay(200 * retries);
				//}
				catch (Exception exc)
				{
					logger.WriteLine("error fetching definition", exc);
					logger.WriteLine($"retrying {(200 & retries)}ms");
					await Task.Delay(200 * retries);
				}
			}

			if (string.IsNullOrEmpty(json))
			{
				UIHelper.ShowError(string.Format(Resx.Pronunciate_NetError, word));
				return null;
			}

			try
			{
				// use the .NET Framework serializer
				// it's not great but I don't want to pull in a nuget if I don't need to
				var serializer = new JavaScriptSerializer();
				var definition = serializer.Deserialize<Definition[]>(json);

				if (definition?.Length > 0 && definition[0].phonetics?.Length > 0)
				{
					return definition[0].phonetics[0].text;
				}

				UIHelper.ShowError(string.Format(Resx.Pronunciate_NoWord, word));
			}
			catch (Exception exc)
			{
				logger.WriteLine("error deserializing json", exc);
			}

			return null;
		}
	}
}
