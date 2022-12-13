//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System;
	using System.Collections.Concurrent;
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
				UIHelper.ShowInfo(Resx.NetwordConnectionUnavailable);
				return;
			}

			var result = MessageBox.Show(
				Resx.GetImagesCommand_Cite,
				Resx.OneMoreTab_Label,
				MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button2,
				MessageBoxOptions.DefaultDesktopOnly);

			if (result == DialogResult.Cancel)
			{
				return;
			}

			using var one = new OneNote(out var page, out _);
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
			List<XElement> runs = null;

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
				runs = page.Root
					.Elements(page.Namespace + "Outline")
					.DescendantNodes().OfType<XCData>()
					.Where(c => regex.IsMatch(c.Value))
					.Select(e => e.Parent)
					.ToList();
			}
			else
			{
				// selected range so affect only within that
				runs = page.Root
					.Elements(page.Namespace + "Outline")
					.DescendantNodes().OfType<XCData>()
					.Where(c => regex.IsMatch(c.Value))
					.Select(e => e.Parent)
					.Where(e => e.Attributes("selected").Any(a => a.Value == "all"))
					.ToList();
			}

			// parallelize internet access for all hyperlinks on page...

			int count = 0;
			if (runs?.Count > 0)
			{
				//<one:Meta name="om" content="caption" />

				// must use a thread-safe collection here
				var tasks = new ConcurrentBag<Task<int>>();

				foreach (var run in runs)
				{
					// do not use await in the body loop; just build list of tasks

					// do not reprocess captioned images
					if (!run.Parent.Descendants().Any(m =>
						m.Name.LocalName == "Meta" &&
						m.Attribute("name").Value == "om" &&
						m.Attribute("content").Value == "caption"))
					{
						tasks.Add(GetImage(run));
					}
				}

				if (tasks.Any())
				{
					await Task.WhenAll(tasks.ToArray());

					count = tasks.Sum(t => t.Result);
				}
			}

			return count > 0;
		}


		private async Task<int> GetImage(XElement run)
		{
			// get thread-local ref of logger
			logger = Logger.Current;

			var cdata = run.GetCData();

			var wrapper = cdata.GetWrapper();
			var anchor = wrapper.Element("a");
			if (anchor == null)
			{
				return 0;
			}

			// only including URLs like <a href="ULR">URL</a> where the text shows the URL
			// does not include "named" URLs like <a href="URL">name</a>

			var href = anchor.Attribute("href")?.Value;
			if (href != anchor.Value)
			{
				return 0;
			}

			Image image;
			var watch = new Stopwatch();
			watch.Start();

			try
			{
				image = await DownloadImage(href);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"cannot resolve {href} after {watch.ElapsedMilliseconds}ms", exc);
				return 0;
			}
			finally
			{
				watch.Stop();
			}

			if (image == null)
			{
				logger.WriteLine($"cannot resolve {href} as an image");
				return 0;
			}

			try
			{
				logger.WriteLine($"resolved {href} in {watch.ElapsedMilliseconds}ms");

				// split current OE into beforeOE/thisOE/afterOE
				// results in thisOE having only the T/anchor which we'll swap out for the Image...

				var ns = run.GetNamespaceOfPrefix(OneNote.Prefix);
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
			}
			catch (Exception exc)
			{
				logger.WriteLine("cannot replace image", exc);
				return 0;
			}
			finally
			{
				image.Dispose();
			}

			return 1;
		}


		private async Task<Image> DownloadImage(string url)
		{
			// special case for importing HTML command
			var index = url.IndexOf("://onemore.");
			if (index > 0)
			{
				url = url.Remove(index + 3, 8);
			}

			var client = HttpClientFactory.Create();

			try
			{
				using var source = new CancellationTokenSource(TimeSpan.FromSeconds(5));
				using var response = await client.GetAsync(new Uri(url, UriKind.Absolute), source.Token);
				if (response.IsSuccessStatusCode)
				{
					using var stream = new MemoryStream();
					await response.Content.CopyToAsync(stream);

					var detector = new ImageDetector();
					if (detector.GetSignature(stream) != ImageSignature.Unknown)
					{
						try
						{
							return new Bitmap(Image.FromStream(stream));
						}
						catch
						{
							logger.WriteLine($"{url} does not appear to be an image");
						}
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
