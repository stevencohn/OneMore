//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using System.Xml.Linq;


	internal class NameUrlsCommand : Command
	{

		public NameUrlsCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			if (!HttpClientFactory.IsNetworkAvailable())
			{
				UIHelper.ShowInfo(Properties.Resources.NetwordConnectionUnavailable);
				return;
			}

			using var one = new OneNote(out var page, out _);
			if (await NameUrls(page))
			{
				await one.Update(page);
			}
		}


		private async Task<bool> NameUrls(Page page)
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

			// parallelize internet access for all hyperlinks on page...

			int count = 0;
			if (elements?.Count > 0)
			{
				// must use a thread-safe collection here
				var tasks = new ConcurrentBag<Task<int>>();

				foreach (var element in elements)
				{
					// do not use await in the body loop; just build list of tasks
					tasks.Add(ReplaceUrlText(element));
				}

				await Task.WhenAll(tasks.ToArray());
				count = tasks.Sum(t => t.IsFaulted ? 0 : t.Result);
			}

			return count > 0;
		}


		private async Task<int> ReplaceUrlText(XElement element)
		{
			var cdata = element.GetCData();

			var wrapper = cdata.GetWrapper();
			var anchor = wrapper.Element("a");
			if (anchor == null)
			{
				return 0;
			}

			var href = anchor.Attribute("href")?.Value;
			if (string.IsNullOrWhiteSpace(href))
			{
				return 0;
			}

			string title;
			var watch = new System.Diagnostics.Stopwatch();
			watch.Start();

			try
			{
				title = await FetchPageTitle(href);
				watch.Stop();

				if (!string.IsNullOrWhiteSpace(title))
				{
					logger.WriteLine($"resolved {href} in {watch.ElapsedMilliseconds}ms");
					anchor.Value = HttpUtility.HtmlDecode(title);
					cdata.ReplaceWith(wrapper.GetInnerXml());
					return 1;
				}
			}
			catch
			{
				watch.Stop();
				logger.WriteLine($"cannot resolve {href} after {watch.ElapsedMilliseconds}ms");
			}

			return 0;
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage(
			"Minor Code Smell",
			"S1643:Strings should not be concatenated using '+' in a loop",
			Justification = "Need string functions that are not available in StringBuilder")]
		private async Task<string> FetchPageTitle(string url)
		{
			string title = null;

			try
			{
				logger.WriteLine($"fetching {url}");
				var client = HttpClientFactory.Create();

				using var source = new CancellationTokenSource(TimeSpan.FromSeconds(5));
				using var response = await client
					.GetAsync(new Uri(url, UriKind.Absolute), source.Token).ConfigureAwait(false);

				if (response.IsSuccessStatusCode)
				{
					using var stream = await response.Content
						.ReadAsStreamAsync()
						.ConfigureAwait(false);

					// compiled regex to check for <title></title> block
					var pattern = new Regex(@"<title>([^<]+)</title>",
						RegexOptions.Compiled | RegexOptions.IgnoreCase);

					var chunkSize = 512;
					var buffer = new byte[chunkSize];
					var contents = "";
					var length = 0;

					while ((title == null) && (length = stream.Read(buffer, 0, chunkSize)) > 0)
					{
						// convert the byte-array to a string and add it to the rest of the
						// contents that have been downloaded so far
						var line = Encoding.UTF8.GetString(buffer, 0, length);
						contents += line;

						var match = pattern.Match(contents);
						if (match.Success)
						{
							// we found a <title></title> match
							title = match.Groups[1].Value.Trim();
						}

						// must do this after appending content if either tag spans chunks
						if (contents.Contains("</head>") || contents.Contains("<body"))
						{
							// reached end of head-block; no title found
							break;
						}
					}
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"cannot retrieve title of {url}", exc);
			}

			return title;
		}
	}
}
