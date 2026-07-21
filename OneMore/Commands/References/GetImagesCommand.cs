//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using Svg;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Replaces all or selected hyperlinked URLs that reference online images with those
	/// referenced images. If the image cannot be downloaded, no changes are made. This will
	/// only affect URLs where you can read the Web address on the page; it skips URLs where
	/// the text differs from the actual address.
	/// </summary>
	internal class GetImagesCommand : Command
	{
		private readonly bool forceful;
		private readonly Regex regex;
		private Style citation;


		/// <summary>
		/// Default constructor to examine all anchors on page or in selected region
		/// </summary>
		public GetImagesCommand()
			: this(new Regex(@"<a\s+href=", RegexOptions.Compiled))
		{
			forceful = false;
		}


		/// <summary>
		/// Specialized entry point for Import Web command, examines anchors with specific
		/// pattern on entire page
		/// </summary>
		/// <param name="regex"></param>
		public GetImagesCommand(Regex regex)
		{
			this.regex = regex;
			forceful = true;
		}


		public override async Task Execute(params object[] args)
		{
			if (!HttpClientFactory.IsNetworkAvailable())
			{
				ShowInfo(Resx.NetwordConnectionUnavailable);
				return;
			}

			var result = UI.MoreMessageBox.Show(owner,
				Resx.GetImagesCommand_Cite,
				MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

			if (result == DialogResult.Cancel)
			{
				return;
			}

			await using var one = new OneNote(out var page, out _);
			if (result == DialogResult.Yes)
			{
				// ensure page contains the definition of the Citation style
				citation = page.GetQuickStyle(StandardStyles.Citation);
			}

			if (await GetImages(page))
			{
				await one.Update(page);
			}
		}


		public async Task<bool> GetImages(Page page)
		{
			logger = Logger.Current;

			var runs = SelectRuns(page);
			if (!runs.Any())
			{
				return false;
			}

			// resolve and cache one Image per distinct href; this download/rasterization work
			// is safe to parallelize (unlike the DOM splicing below) and also means a href
			// repeated across many runs - e.g. a row of identical inline icon anchors - is
			// only ever fetched/rasterized once
			var hrefs = runs
				.Where(run => !IsCaptioned(run))
				.Select(GetEligibleHref)
				.Where(href => href != null)
				.Distinct()
				.ToList();

			var cache = new Dictionary<string, Image>();

			if (hrefs.Any())
			{
				var results = await Task.WhenAll(hrefs.Select(ResolveImage));
				foreach (var (href, image) in results)
				{
					if (image != null)
					{
						cache[href] = image;
					}
				}
			}

			logger.WriteLine(
				$"found {runs.Count} candidate runs, {hrefs.Count} distinct hrefs, " +
				$"{cache.Count} resolved to images");

			int count = 0;
			var attempts = 0;
			var failed = new HashSet<string>();

			try
			{
				// splice sequentially, re-querying the live page each pass. SplitElement
				// moves/clones a run's sibling elements when they share the same parent OE -
				// or even the same original CDATA, e.g. several icon anchors merged into one
				// run because the source HTML had no whitespace between them - so splitting
				// one run back apart can surface a *new* eligible run whose href was never
				// part of the up-front batch above. Resolve on demand here (falling back to
				// the pre-fetched cache when possible) instead of only matching pre-cached
				// hrefs, so these stragglers still get converted instead of being silently
				// left behind as raw anchor text
				while (true)
				{
					var candidates = SelectRuns(page)
						.Where(e => !IsCaptioned(e))
						.ToList();

					var run = candidates.FirstOrDefault(e =>
					{
						var eligibleHref = GetEligibleHref(e);
						return eligibleHref != null && !failed.Contains(eligibleHref);
					});

					if (run == null)
					{
						logger.WriteLine(
							$"splice loop ending: {candidates.Count} live candidates remain");
						break;
					}

					attempts++;
					var href = GetEligibleHref(run);

					if (!cache.TryGetValue(href, out var image))
					{
						(_, image) = await ResolveImage(href);
						if (image != null)
						{
							cache[href] = image;
						}
					}

					if (image != null && ApplyImage(run, image))
					{
						count++;
					}
					else
					{
						// permanently unresolvable or unsplice-able; blacklist so the next
						// pass doesn't loop on it forever
						failed.Add(href);
					}
				}
			}
			finally
			{
				foreach (var image in cache.Values)
				{
					image.Dispose();
				}
			}

			logger.WriteLine($"spliced {count} of {attempts} attempted images");

			return count > 0;
		}


		private List<XElement> SelectRuns(Page page)
		{
			IEnumerable<XElement> selections;

			if (forceful)
			{
				// force full page regardless of selections
				selections = new List<XElement>();
			}
			else
			{
				// determine if selected region or full page
				selections = page.Root
					.Elements(page.Namespace + "Outline")
					.Descendants(page.Namespace + "T")
					.Where(e =>
						e.Attributes("selected").Any(a => a.Value.Equals("all")));
			}

			if (forceful ||
				(selections.Count() == 1) &&
				(selections.First().DescendantNodes().OfType<XCData>().First().Value.Length == 0))
			{
				// single empty selection so affect entire page
				return page.Root
					.Elements(page.Namespace + "Outline")
					.DescendantNodes().OfType<XCData>()
					.Where(c => regex.IsMatch(c.Value))
					.Select(e => e.Parent)
					.ToList();
			}

			// selected range so affect only within that
			return page.Root
				.Elements(page.Namespace + "Outline")
				.DescendantNodes().OfType<XCData>()
				.Where(c => regex.IsMatch(c.Value))
				.Select(e => e.Parent)
				.Where(e => e.Attributes("selected").Any(a => a.Value == "all"))
				.ToList();
		}


		private static bool IsCaptioned(XElement run)
		{
			//<one:Meta name="om" content="caption" />
			return run.Parent.Descendants().Any(m =>
				m.Name.LocalName == "Meta" &&
				m.Attribute("name")?.Value == "om" &&
				m.Attribute("content")?.Value == "caption");
		}


		private static string GetEligibleHref(XElement run)
		{
			var wrapper = run.GetCData().GetWrapper();
			var anchor = wrapper.Element("a");
			if (anchor == null)
			{
				return null;
			}

			// only including URLs like <a href="URL">URL</a> where the text shows the URL
			// does not include "named" URLs like <a href="URL">name</a>

			var href = anchor.Attribute("href")?.Value;
			return href == anchor.Value ? href : null;
		}


		private async Task<(string href, Image image)> ResolveImage(string href)
		{
			var watch = Stopwatch.StartNew();

			try
			{
				var image = await DownloadImage(href);
				if (image == null)
				{
					logger.WriteLine($"cannot resolve {href} as an image");
				}
				else
				{
					logger.WriteLine($"resolved {href} in {watch.ElapsedMilliseconds}ms");
				}

				return (href, image);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"cannot resolve {href} after {watch.ElapsedMilliseconds}ms", exc);
				return (href, null);
			}
		}


		private bool ApplyImage(XElement run, Image image)
		{
			try
			{
				var wrapper = run.GetCData().GetWrapper();
				var anchor = wrapper.Element("a");
				var href = anchor.Attribute("href")?.Value;

				// split current OE into beforeOE/thisOE/afterOE
				// results in thisOE having only the T/anchor which we'll swap out for the Image...

				var ns = run.GetNamespaceOfPrefix(OneNote.Prefix);
				if (ns == null)
				{
					logger.WriteLine($"cannot replace {href}: run has no '{OneNote.Prefix}' " +
						"namespace in scope (detached from page?)");
					return false;
				}

				SplitElement(run, anchor, ns);

				// create Image element and swap with the current T...

				var content = new XElement(ns + "Image",
					new XAttribute("format", "png"),
					new XElement(ns + "Size",
						new XAttribute("width", $"{image.Width}.0"),
						new XAttribute("height", $"{image.Height}.0")),
					new XElement(ns + "Data", image.ToBase64String())
					);

				if (citation != null)
				{
					content = WrapWithCitation(content, href, ns).Root;
				}

				run.ReplaceWith(content);
				logger.WriteLine($"replaced {href}");
				return true;
			}
			catch (Exception exc)
			{
				logger.WriteLine("cannot replace image", exc);
				return false;
			}
		}


		private async Task<Image> DownloadImage(string url)
		{
			// special case for importing HTML command
			var index = url.IndexOf("://onemore.");
			if (index > 0)
			{
				url = url.Remove(index + 3, 8);
			}

			try
			{
				MemoryStream stream;
				string contentType = null;

				if (url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
				{
					// special case for inline <svg> elements captured by importing HTML command
					stream = DecodeDataUri(url, out contentType);
					if (stream == null)
					{
						return null;
					}
				}
				else
				{
					var client = HttpClientFactory.Create();
					using var source = new CancellationTokenSource(TimeSpan.FromSeconds(5));
					using var response = await client.GetAsync(new Uri(url, UriKind.Absolute), source.Token);
					if (!response.IsSuccessStatusCode)
					{
						return null;
					}

					contentType = response.Content.Headers.ContentType?.MediaType;

					stream = new MemoryStream();
					await response.Content.CopyToAsync(stream);
				}

				using (stream)
				{
					var detector = new ImageDetector();
					var signature = detector.GetSignature(stream);
					if (signature == ImageSignature.Unknown &&
						string.Equals(contentType, "image/svg+xml", StringComparison.OrdinalIgnoreCase))
					{
						// some servers don't set correct content-type on svg responses; the
						// converse (a mislabeled content-type that isn't backed by svg content)
						// is caught below since Svg.NET will throw on non-svg bytes
						signature = ImageSignature.SVG;
					}

					if (signature == ImageSignature.Unknown)
					{
						return null;
					}

					try
					{
						if (signature == ImageSignature.SVG)
						{
							stream.Position = 0;
							return SvgDocument.Open<SvgDocument>(stream).Draw();
						}

						return new Bitmap(Image.FromStream(stream));
					}
					catch
					{
						logger.WriteLine($"{url} does not appear to be an image");
					}
				}
			}
			catch (TaskCanceledException exc)
			{
				logger.WriteLine($"timeout fetching image from {url}", exc);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error fetching image from {url}", exc);
			}

			return null;
		}


		private static MemoryStream DecodeDataUri(string url, out string contentType)
		{
			contentType = null;

			// data:[<mediatype>][;base64],<data>
			var comma = url.IndexOf(',');
			if (comma < 0)
			{
				return null;
			}

			var header = url.Substring(5, comma - 5);
			if (!header.EndsWith(";base64", StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}

			contentType = header.Substring(0, header.Length - ";base64".Length);

			var bytes = Convert.FromBase64String(url.Substring(comma + 1));

			// use the publiclyVisible ctor so ImageDetector.GetSignature can call GetBuffer()
			return new MemoryStream(bytes, 0, bytes.Length, writable: false, publiclyVisible: true);
		}


		private void SplitElement(XElement run, XElement anchor, XNamespace ns)
		{
			var left = anchor.NodesBeforeSelf();
			var before = run.ElementsBeforeSelf();
			if (left.Any() || before.Any())
			{
				var oe = new XElement(ns + "OE", run.Parent.Attributes());

				if (before.Any())
				{
					oe.Add(before);
					before.Remove();
				}
				if (left.Any())
				{
					oe.Add(new XElement(ns + "T", new XCData(new XElement("w", left).GetInnerXml())));
					left.Remove();
				}
				run.Parent.AddBeforeSelf(oe);
			}

			var right = anchor.NodesAfterSelf();
			var after = run.ElementsAfterSelf();
			if (right.Any() || after.Any())
			{
				var oe = new XElement(ns + "OE", run.Parent.Attributes());
				if (right.Any())
				{
					oe.Add(new XElement(ns + "T", new XCData(new XElement("w", right).GetInnerXml())));
					right.Remove();
				}
				if (after.Any())
				{
					oe.Add(after);
					after.Remove();
				}
				run.Parent.AddAfterSelf(oe);
			}
		}


		private Table WrapWithCitation(XElement image, string caption, XNamespace ns)
		{
			var table = new Table(ns);
			table.AddColumn(0f); // OneNote will set width accordingly

			var cdata = new XCData(caption);

			var row = table.AddRow();
			var cell = row.Cells.First();

			cell.SetContent(
				new XElement(ns + "OEChildren",
					new XElement(ns + "OE",
						new XAttribute("alignment", "center"),
						image),
					new XElement(ns + "OE",
						new XAttribute("alignment", "center"),
						new XAttribute("quickStyleIndex", citation.Index.ToString()),
						new XElement(ns + "Meta",
							new XAttribute("name", "om"),
							new XAttribute("content", "caption")),
						new XElement(ns + "T", cdata)
					)
				));

			return table;
		}
	}
}
