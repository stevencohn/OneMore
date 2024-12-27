//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using River.OneMoreAddIn.UI;
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
		private OneNote one;
		private Page page;
		private List<XElement> candidates;
		private Dictionary<string, OneNote.HyperlinkInfo> map;

		private int badCount;
		private Exception exception;


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

			await using (one = new OneNote(out page, out _))
			{
				candidates = GetCandiateElements(page);
				if (candidates.Count > 0)
				{
					var progressDialog = new ProgressDialog(Execute);

					// report results on UI thread after execution
					progressDialog.RunModeless(ReportResult);
				}
			}
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


		// Invoked by the ProgressDialog OnShown callback
		private async Task Execute(ProgressDialog progress, CancellationToken token)
		{
			logger.Start();
			logger.StartClock();

			if (HasOneNoteReferences())
			{
				await BuildHyperlinkMap(progress, token);
			}

			progress.SetMaximum(candidates.Count);
			progress.SetMessage($"Checking {candidates.Count} URLs");

			try
			{
				await ValidateUrls(progress);
				if (badCount > 0)
				{
					await one.Update(page);
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error validating URLs", exc);
				exception = exc;
			}

			progress.Close();

			logger.WriteTime("check complete");
			logger.End();
		}


		private bool HasOneNoteReferences()
		{
			return candidates.Any(e =>
				e.Descendants().OfType<XCData>()
					.Any(c => c.Value.StartsWith("<a href=\"onenote:")));
		}


		private async Task BuildHyperlinkMap(ProgressDialog progress, CancellationToken token)
		{
			map = await new HyperlinkProvider(one).BuildHyperlinkMap(
				OneNote.Scope.Notebooks,
				token,
				async (count) =>
				{
					progress.SetMaximum(count);
					progress.SetMessage($"Mapping {count} page references");
					await Task.Yield();
				},
				async () =>
				{
					progress.Increment();
					await Task.Yield();
				});
		}


		private async Task ValidateUrls(ProgressDialog progress)
		{
			// parallelize internet access for all chosen hyperlinks on the page...

			// must use a thread-safe collection here
			var tasks = new ConcurrentBag<Task>();

			foreach (var candidate in candidates)
			{
				// do not use await in the body loop; just build list of tasks
				tasks.Add(ValidateUrl(candidate, progress));
			}

			await Task.WhenAll(tasks.ToArray());
			//var count = tasks.Sum(t => t.IsFaulted ? 0 : t.Result);
		}


		private async Task ValidateUrl(XElement element, ProgressDialog progress)
		{
			var cdata = element.GetCData();
			var wrapper = cdata.GetWrapper();

			var count = badCount;
			foreach (var anchor in wrapper.Elements("a"))
			{
				progress.Increment();

				var href = anchor.Attribute("href")?.Value;
				if (ValidAddress(href))
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

						Interlocked.Increment(ref badCount);
					}
				}
			}

			if (badCount > count)
			{
				cdata.ReplaceWith(wrapper.GetInnerXml());
			}
		}


		private static bool ValidAddress(string href)
		{
			if (string.IsNullOrWhiteSpace(href))
			{
				return false;
			}

			if (href.StartsWith("http") &&
				!(
					href.Contains("onedrive.live.com/view.aspx") &&
					href.Contains("&id=documents") &&
					href.Contains(".one")
				))
			{
				return true;
			}

			return
				href.StartsWith("onenote:") &&
				href.Contains("section-id=") &&
				href.Contains("page-id=");
		}


		private async Task<bool> InvalidUrl(string url)
		{
			if (url.StartsWith("onenote:") && url.Contains("page-id="))
			{
				return InvalidOneNoteUrl(url);
			}
			else
			{
				return await InvalidWebUrl(url);
			}
		}


		private bool InvalidOneNoteUrl(string url)
		{
			var match = Regex.Match(url, @"section-id=({[^}]+})&amp;page-id=({[^}]+})");
			if (match.Success)
			{
				return map.Any(m =>
					m.Value.SectionID == match.Groups[1].Value &&
					m.Value.PageID == match.Groups[2].Value);
			}

			return false;
		}


		private async Task<bool> InvalidWebUrl(string url)
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


		private void ReportResult(object sender, EventArgs e)
		{
			// report results back on the main UI thread...

			if (sender is ProgressDialog progress)
			{
				// otherwise ShowMessage window will appear behind progress dialog
				progress.Visible = false;
			}

			if (exception is null)
			{
				if (badCount > 0)
				{
					MoreMessageBox.ShowWarning(owner,
						$"Found {badCount} invalid URLs on this page");
				}
			}
			else
			{
				MoreMessageBox.ShowErrorWithLogLink(owner, exception.Message);
			}
		}

	}
}
