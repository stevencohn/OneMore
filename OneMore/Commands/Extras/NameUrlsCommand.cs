//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using System.Xml.Linq;
	using River.OneMoreAddIn.Models;


	internal class NameUrlsCommand : Command
	{

		public NameUrlsCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out var ns))
			{
				if (NameUrls(page))
				{
					one.Update(page);
				}
			}
		}


		private bool NameUrls(Page page)
		{
			List<XElement> elements = null;
			var regex = new Regex(@"<a\s+href=", RegexOptions.Compiled);

			var selections = page.Root.Descendants(page.Namespace + "T")
				.Where(e =>
					e.Attributes("selected").Any(a => a.Value.Equals("all")));

			if ((selections.Count() == 1) &&
				(selections.First().DescendantNodes().OfType<XCData>().First().Value.Length == 0))
			{
				// single empty selection so affect entire page
				elements = page.Root.DescendantNodes().OfType<XCData>()
					.Where(c => regex.IsMatch(c.Value))
					.Select(e => e.Parent)
					.ToList();
			}
			else
			{
				// selected range so affect only within that
				elements = page.Root.DescendantNodes().OfType<XCData>()
					.Where(c => regex.IsMatch(c.Value))
					.Select(e => e.Parent)
					.Where(e => e.Attributes("selected").Any(a => a.Value == "all"))
					.ToList();
			}

			// parallelize internet access for all hyperlinks on page

			int count = 0;
			if (elements?.Count > 0)
			{
				ServicePointManager.SecurityProtocol =
					SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

				Parallel.ForEach(elements, (element) =>
				{
					var c = ReplaceUrlText(element);
					Interlocked.Add(ref count, c);
				});
			}

			return count > 0;
		}


		private int ReplaceUrlText(XElement element)
		{
			var cdata = element.GetCData();

			var wrapper = cdata.GetWrapper();
			var a = wrapper.Element("a");
			if (a != null)
			{
				var href = a.Attribute("href")?.Value;
				if (href != null && href == a.Value)
				{
					string title;
					var watch = new Stopwatch();
					watch.Start();

					try
					{
						title = FetchPageTitle(href);
						watch.Stop();
					}
					catch
					{
						watch.Stop();
						logger.WriteLine($"cannot resolve {href} after {watch.ElapsedMilliseconds}ms");
						return 0;
					}

					if (title != null)
					{
						logger.WriteLine($"resolved {href} in {watch.ElapsedMilliseconds}ms");
						a.Value = HttpUtility.HtmlDecode(title);
						cdata.ReplaceWith(wrapper.GetInnerXml());
						return 1;
					}
				}
			}

			return 0;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage(
			"Minor Code Smell",
			"S1643:Strings should not be concatenated using '+' in a loop",
			Justification = "Need string functions that are not available in StringBuilder")]
		private static string FetchPageTitle(string url)
		{
			string title = null;

			using (var client = new HttpClient())
			{
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
								title = match.Groups[1].Value;
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
