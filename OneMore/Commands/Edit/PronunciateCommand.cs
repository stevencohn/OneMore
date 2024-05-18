//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable CS0649  // never assigned to, will always be null
#pragma warning disable S1075   // URIs should not be hardcoded
#pragma warning disable S3459   // Unassigned members should be removed

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Linq;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web.Script.Serialization;
	using System.Windows.Forms;
	using System.Xml;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Inserts the "ruby text" pronunciation of selected words. This uses an online service
	/// that supports over a dozen languages.
	/// </summary>
	internal class PronunciateCommand : Command
	{
		private string isoCode;


		private sealed class Phonetics
		{
			public string text;
		}

		private sealed class Definition
		{
			public Phonetics[] phonetics;
		}


		private const string DictionaryUrl = "https://api.dictionaryapi.dev/api/v2/entries/{0}/{1}";


		public PronunciateCommand()
		{
			isoCode = null;
		}


		public override async Task Execute(params object[] args)
		{
			if (!HttpClientFactory.IsNetworkAvailable())
			{
				ShowInfo(Resx.NetwordConnectionUnavailable);
				return;
			}

			await using var one = new OneNote(out var page, out var ns);
			var element = page.Root.Descendants(ns + "T")
				.FirstOrDefault(e =>
					e.Attributes("selected").Any(a => a.Value.Equals("all")) &&
					e.FirstNode.NodeType == XmlNodeType.CDATA &&
					((XCData)e.FirstNode).Value.Length > 0);

			if (element == null)
			{
				ShowError(Resx.Pronunciate_FullWord);
				return;
			}

			var cdata = element.GetCData();
			var wrapper = cdata.GetWrapper();

			var word = wrapper.Value;
			var spaced = char.IsWhiteSpace(word[word.Length - 1]);
			word = word.Trim();

			if (string.IsNullOrEmpty(word))
			{
				ShowError(Resx.Pronunciate_EmptyWord);
				return;
			}

			// check for replay
			if (args is not null)
			{
				var index = args.IndexOf(a => a is XElement e && e.Name.LocalName == "isoCode");
				if (index >= 0 && args[index] is XElement isoElement &&
					!string.IsNullOrEmpty(isoElement.Value))
				{
					isoCode = isoElement.Value;
				}
			}

			if (isoCode is null)
			{
				using var dialog = new PronunciateDialog();
				dialog.Word = word;

				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					IsCancelled = true;
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


		private async Task<string> LookupPhonetics(string word, string isoCode)
		{
			var client = HttpClientFactory.Create();
			var url = string.Format(DictionaryUrl, isoCode, word);

			string json = null;
			int retries = 0;

			while (json == null && retries < 2)
			{
				retries++;

				try
				{
					using var source = new CancellationTokenSource(TimeSpan.FromSeconds(10));
					var response = await client.GetAsync(url, source.Token);
					response.EnsureSuccessStatusCode();
					json = await response.Content.ReadAsStringAsync();

					//logger.WriteLine(json);
				}
				catch (HttpRequestException exc) when (exc.HResult == -2146233088) //0x80131500)
				{
					logger.WriteLine($"could not fetch {url}");
					retries = int.MaxValue;
				}
				catch (Exception exc)
				{
					logger.WriteLine($"error fetching {url}", exc);
					logger.WriteLine($"retrying {(200 & retries)}ms");
					await Task.Delay(200 * retries);
				}
			}

			if (string.IsNullOrEmpty(json))
			{
				ShowError(string.Format(Resx.Pronunciate_NetError, isoCode, word));
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

				ShowError(string.Format(Resx.Pronunciate_NoWord, word));
			}
			catch (Exception exc)
			{
				logger.WriteLine("error deserializing json", exc);
			}

			return null;
		}


		public override XElement GetReplayArguments()
		{
			if (isoCode != null)
			{
				return new XElement("isoCode", isoCode);
			}

			return null;
		}
	}
}
