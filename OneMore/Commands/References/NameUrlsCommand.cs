//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
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


	#region wrappers
	internal class UnnameUrlsCommand : NameUrlsCommand
	{
		public UnnameUrlsCommand() { }
		public override Task Execute(params object[] args)
		{
			return base.Execute(false);
		}
	}
	#endregion


	internal class NameUrlsCommand : Command
	{

		public NameUrlsCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var friendly = args.Length == 0 || (bool)args[0];

			if (friendly && !HttpClientFactory.IsNetworkAvailable())
			{
				ShowInfo(Properties.Resources.NetwordConnectionUnavailable);
				return;
			}

			await using var one = new OneNote(out var page, out _);

			var updated = friendly
				? await NameUrls(page)
				: SimplifyUrls(page);

			if (updated)
			{
				await one.Update(page);
			}
		}


		private static bool SimplifyUrls(Page page)
		{
			var elements = GetCandiateElements(page);

			var total = 0;
			foreach (var element in elements)
			{
				var cdata = element.GetCData();
				var wrapper = cdata.GetWrapper();

				var count = 0;
				foreach (var anchor in wrapper.Elements("a"))
				{
					var href = anchor.Attribute("href")?.Value;
					if (ValidWebAddress(href))
					{
						if (anchor.TextValue() != href)
						{
							anchor.Value = href;
							count++;
						}
					}
				}

				if (count > 0)
				{
					cdata.Value = wrapper.GetInnerXml();
				}

				total += count;
			}

			return total > 0;
		}


		private static bool ValidWebAddress(string href)
		{
			return
				!string.IsNullOrWhiteSpace(href) &&
				href.StartsWith("http") &&
				!(
					href.StartsWith("https://onedrive.live.com/view.aspx") &&
					href.Contains("&id=documents") &&
					href.Contains(".one")
				);

		}


		private static List<XElement> GetCandiateElements(Page page)
		{
			List<XElement> elements;

			// OneNote XML will insert CR prior to 'href' in the CDATA
			var regex = new Regex(@"<a\s+href=", RegexOptions.Compiled);

			var range = new SelectionRange(page);
			range.GetSelection();

			if (range.Scope == SelectionScope.None ||
				range.Scope == SelectionScope.TextCursor)
			{
				// entire page
				elements = page.Root
					.DescendantNodes().OfType<XCData>()
					.Where(c => regex.IsMatch(c.Value))
					.Select(e => e.Parent)
					.ToList();
			}
			else
			{
				// only selections
				elements = page.Root
					.DescendantNodes().OfType<XCData>()
					.Where(c => regex.IsMatch(c.Value))
					.Select(e => e.Parent)
					.Where(e => e.Attributes("selected").Any(a => a.Value == "all"))
					.ToList();
			}

			return elements;
		}


		private async Task<bool> NameUrls(Page page)
		{
			var elements = GetCandiateElements(page);

			// parallelize internet access for all hyperlinks on page...

			int count = 0;
			if (elements.Any())
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

			var count = 0;
			foreach (var anchor in wrapper.Elements("a"))
			{
				var href = anchor.Attribute("href")?.Value;
				if (ValidWebAddress(href))
				{
					var title = await FetchPageTitle(href);
					if (!string.IsNullOrWhiteSpace(title))
					{
						anchor.Value = title;
						count++;
					}
				}
			}

			if (count > 0)
			{
				cdata.ReplaceWith(wrapper.GetInnerXml());
			}

			return count;
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage(
			"Minor Code Smell",
			"S1643:Strings should not be concatenated using '+' in a loop",
			Justification = "Need string functions that are not available in StringBuilder")]
		private async Task<string> FetchPageTitle(string url)
		{
			string title = null;

			var watch = new System.Diagnostics.Stopwatch();
			watch.Start();

			try
			{
				logger.WriteLine($"fetching {url}");
				var client = HttpClientFactory.Create();

				using var source = new CancellationTokenSource(TimeSpan.FromSeconds(5));
				using var response = await client
					.GetAsync(new Uri(url, UriKind.Absolute), source.Token).ConfigureAwait(false);

				watch.Stop();

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

					while ((title is null) && (length = await stream.ReadAsync(buffer, 0, chunkSize)) > 0)
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

					title = HttpUtility.HtmlDecode(title);
					logger.WriteLine($"resolved {url} to [{title}] in {watch.ElapsedMilliseconds}ms");
				}
				else
				{
					logger.WriteLine($"cannot resolve {url} after {watch.ElapsedMilliseconds}ms");
					logger.WriteLine($"- StatusCode [{response.StatusCode}]");
					logger.WriteLine($"- ReasonPhrase [{response.ReasonPhrase}]");
				}
			}
			catch (Exception exc)
			{
				watch.Stop();
				logger.WriteLine($"cannot resolve {url} after {watch.ElapsedMilliseconds}ms");
				logger.WriteLine($"ERROR: {exc.Message}");
			}

			return title;
		}
	}
}
