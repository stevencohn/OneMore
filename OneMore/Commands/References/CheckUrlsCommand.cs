//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class CheckUrlsCommand : Command
	{

		public CheckUrlsCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			if (!HttpClientFactory.IsNetworkAvailable())
			{
				ShowInfo(Properties.Resources.NetwordConnectionUnavailable);
				return;
			}

			await using var one = new OneNote(out var page, out _);

			var count = await HasInvalidUrls(page);
			if (count > 0)
			{
				await one.Update(page);

				UI.MoreMessageBox.ShowWarning(owner, $"Found {count} invalid URLs on this page");
			}
		}


		private async Task<int> HasInvalidUrls(Page page)
		{
			var elements = GetCandiateElements(page);

			// parallelize internet access for all chosen hyperlinks on the page...

			var count = 0;
			if (elements.Any())
			{
				// must use a thread-safe collection here
				var tasks = new ConcurrentBag<Task<int>>();

				foreach (var element in elements)
				{
					// do not use await in the body loop; just build list of tasks
					tasks.Add(ValidateUrls(element));
				}

				await Task.WhenAll(tasks.ToArray());
				count = tasks.Sum(t => t.IsFaulted ? 0 : t.Result);
			}

			return count;
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


		private async Task<int> ValidateUrls(XElement element)
		{
			var cdata = element.GetCData();
			var wrapper = cdata.GetWrapper();

			var count = 0;
			foreach (var anchor in wrapper.Elements("a"))
			{
				var href = anchor.Attribute("href")?.Value;
				if (ValidWebAddress(href))
				{
					if (await InvalidUrl(href))
					{
						// as usual, make direct updates and let OneNote normalize

						// update the background style of all spans in value of anchor
						foreach (var span in anchor.Nodes().OfType<XElement>())
						{
							var style = new Style(span.Attribute("style").Value, false)
							{
								ApplyColors = true,
								Highlight = "red"
							};

							span.SetAttributeValue("style", style.ToCss());
						}

						// and then wrap the whole value in a span, even if there are sub-spans
						var texts = anchor.Nodes().OfType<XText>();
						if (texts.Any())
						{
							anchor.ReplaceNodes(
								new XElement("span",
									new XAttribute("style", "background:red"),
								anchor.Nodes()
								)
							);
						}

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


		private async Task<bool> InvalidUrl(string url)
		{
			var invalid = false;

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
					logger.WriteLine($"resolved {url} in {watch.ElapsedMilliseconds}ms");
				}
				else
				{
					invalid = true;
					logger.WriteLine($"cannot resolve {url} after {watch.ElapsedMilliseconds}ms");

					logger.WriteLine(
						$"- Status [{response.StatusCode}] Reason [{response.ReasonPhrase}]");
				}
			}
			catch (Exception exc)
			{
				watch.Stop();
				logger.WriteLine($"cannot resolve {url} after {watch.ElapsedMilliseconds}ms");
				logger.WriteLine($"ERROR: {exc.Message}");
				invalid = true;
			}

			return invalid;
		}
	}
}
