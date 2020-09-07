//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS0649 // never assigned to, will always be null
namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Dialogs;
	using System;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web.Script.Serialization;
	using System.Windows.Forms;
	using System.Xml;
	using System.Xml.Linq;


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


		private static HttpClient client;


		public PronunciateCommand()
		{
		}
		

		public async void Execute()
		{
			using (var manager = new ApplicationManager())
			{
				var page = manager.CurrentPage();
				var ns = page.GetNamespaceOfPrefix("one");

				var element = page.Descendants(ns + "T")
					.Where(e =>
						e.Attributes("selected").Any(a => a.Value.Equals("all")) &&
						e.FirstNode.NodeType == XmlNodeType.CDATA &&
						((XCData)e.FirstNode).Value.Length > 0)
					.FirstOrDefault();

				if (element == null)
				{
					UIHelper.ShowError("Must select a full word");
					return;
				}

				var cdata = element.GetCData();
				var wrapper = cdata.GetWrapper();

				var word = wrapper.Value;
				var spaced = char.IsWhiteSpace(word[word.Length - 1]);
				word = word.Trim();

				if (string.IsNullOrEmpty(word))
				{
					UIHelper.ShowError("Selected word cannot be empty");
					return;
				}

				string isoCode = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
				if (isoCode == "zh")
				{
					isoCode = "zh-CN";
				}

				using (var dialog = new PhoneticsDialog())
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
					//if (ruby[0] == '/' && ruby[ruby.Length - 1] == '/')
					//{
					//	ruby = $"({ruby.Substring(1, ruby.Length - 2)})";
					//}

					ruby = spaced ? ruby + ' ' : $" {ruby} ";

					element.AddAfterSelf(new XElement(ns + "T", new XCData(ruby)));
					manager.UpdatePageContent(page);
				}
			}
		}


		private async Task<string> LookupPhonetics(string word, string isoCode)
		{
			if (client == null)
			{
				ServicePointManager.SecurityProtocol =
					SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

				client = new HttpClient
				{
					Timeout = new TimeSpan(0, 0, 10)
				};

				trash.Add(client);
			}

			var url = string.Format(DictionaryUrl, isoCode, word);

			string json = null;
			try
			{
				var uri = new Uri(url);
				json = await client.GetStringAsync(uri);
				logger.WriteLine(json);
			}
			catch (Exception exc)
			{
				logger.WriteLine("Error fetching definition", exc);
			}

			if (string.IsNullOrEmpty(json))
			{
				return null;
			}

			try
			{
				// use the .NET Framework serializer;
				// it's not great but I don't want to pull in a nuget if I don't need to
				var serializer = new JavaScriptSerializer();
				var definition = serializer.Deserialize<Definition[]>(json);

				if (definition?.Length > 0 && definition[0].phonetics?.Length > 0)
				{
					return definition[0].phonetics[0].text;
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("Error deserializing json", exc);
			}

			return null;
		}
	}
}
