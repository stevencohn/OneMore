//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Xml.Linq;


	internal class NameUrlsCommand : Command
	{
		private const int LineCharCount = 100;


		public NameUrlsCommand() : base()
		{
		}


		public void Execute()
		{
			try
			{
				Page page;
				using (var manager = new ApplicationManager())
				{
					page = new Page(manager.CurrentPage());
				}

				if (NameUrls(page))
				{
					using (var manager = new ApplicationManager())
					{
						manager.UpdatePageContent(page.Root);
					}
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"Error executing {nameof(NameUrlsCommand)}", exc);
			}
		}


		private bool NameUrls(Page page)
		{
			int count = 0;
			var regex = new Regex(@"<a\s+href=", RegexOptions.Compiled);

			var elements = page.Root.DescendantNodes().OfType<XCData>()
				.Where(c => regex.IsMatch(c.Value))
				.Select(e => e.Parent)
				.ToList();

			if (elements?.Count > 0)
			{
				foreach (var element in elements)
				{
					var cdata = element.GetCData();

					var wrapper = cdata.GetWrapper();
					var a = wrapper.Element("a");
					if (a != null)
					{
						var href = a.Attribute("href")?.Value;
						if (href != null)
						{
							if (href == a.Value)
							{
								var title = FetchPageTitle(href);
								if (title != null)
								{
									a.Value = HttpUtility.HtmlDecode(title);
									cdata.ReplaceWith(wrapper.GetInnerXml());
									count++;
								}
							}
						}
					}
				}
			}

			return count > 0;
		}


		private string FetchPageTitle(string url)
		{
			string title = null;

			using (var client = new HttpClient())
			{
				ServicePointManager.SecurityProtocol = 
					SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

				// yes, yes, i know, never use Result....
				using (var response = client.GetAsync(new Uri(url, UriKind.Absolute)).Result)
				{
					using (var stream = response.Content.ReadAsStreamAsync().Result)
					{
						// compiled regex to check for <title></title> block
						var pattern = new Regex(@"<title>\s*(.+?)\s*</title>",
							RegexOptions.Compiled | RegexOptions.IgnoreCase);

						var chunkSize = 512;
						var buffer = new byte[chunkSize];
						var contents = "";
						var length = 0;

						while ((title == null) && (length = stream.Read(buffer, 0, chunkSize)) > 0)
						{
							// convert the byte-array to a string and add it to the rest of the
							// contents that have been downloaded so far
							contents += Encoding.UTF8.GetString(buffer, 0, length);

							var match = pattern.Match(contents);
							if (match.Success)
							{
								// we found a <title></title> match
								title = match.Groups[1].Value.ToString();
							}
							else if (contents.Contains("</head>"))
							{
								// reached end of head-block; no title found
								title = string.Empty;
							}
						}
					}
				}
			}

			return title;
		}
	}
}
