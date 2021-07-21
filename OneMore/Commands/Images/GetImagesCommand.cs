﻿//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
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
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class GetImagesCommand : Command
	{
		private readonly ImageDetector detector;
		private XNamespace ns;
		private Style citation;


		public GetImagesCommand()
		{
			detector = new ImageDetector();
		}


		public override async Task Execute(params object[] args)
		{
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

			using (var one = new OneNote(out var page, out ns))
			{
				if (result == DialogResult.Yes)
				{
					// ensure page contains the definition of the Citation style
					citation = page.GetQuickStyle(Styles.StandardStyles.Citation);
				}

				if (GetImages(page))
				{
					await one.Update(page);
				}
			}
		}


		public bool GetImages(Page page)
		{
			List<XElement> runs = null;
			var regex = new Regex(@"<a\s+href=", RegexOptions.Compiled);

			var selections = page.Root.Descendants(page.Namespace + "T")
				.Where(e =>
					e.Attributes("selected").Any(a => a.Value.Equals("all")));

			if ((selections.Count() == 1) &&
				(selections.First().DescendantNodes().OfType<XCData>().First().Value.Length == 0))
			{
				// single empty selection so affect entire page
				runs = page.Root.DescendantNodes().OfType<XCData>()
					.Where(c => regex.IsMatch(c.Value))
					.Select(e => e.Parent)
					.ToList();
			}
			else
			{
				// selected range so affect only within that
				runs = page.Root.DescendantNodes().OfType<XCData>()
					.Where(c => regex.IsMatch(c.Value))
					.Select(e => e.Parent)
					.Where(e => e.Attributes("selected").Any(a => a.Value == "all"))
					.ToList();
			}

			// parallelize internet access for all hyperlinks on page

			int count = 0;
			if (runs?.Count > 0)
			{
				// do not use await in the body of a Parallel.ForEach

				//<one:Meta name="om" content="caption" />

				var tasks = new List<Task<int>>();
				Parallel.ForEach(runs, (run) =>
				{
					// do not reprocess captioned images
					if (!run.Parent.Descendants().Any(m =>
						m.Name.LocalName == "Meta" &&
						m.Attribute("name").Value == "om" &&
						m.Attribute("content").Value == "caption"))
					{
						tasks.Add(GetImage(run));
					}
				});

				if (tasks.Any())
				{
					Task.WaitAll(tasks.ToArray());

					count = tasks.Sum(t => t == null ? 0 : t.Result);
				}
			}

			return count > 0;
		}


		private async Task<int> GetImage(XElement run)
		{
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
				logger.WriteLine($"cannot resolve {href} after {watch.ElapsedMilliseconds}ms");
				logger.WriteLine(exc);
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

			logger.WriteLine($"resolved {href} in {watch.ElapsedMilliseconds}ms");

			// split current OE into beforeOE/thisOE/afterOE
			// results in thisOE having only the T/anchor which we'll swap out for the Image...

			SplitElement(run, anchor);

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
				content = WrapWithCitation(content, href).Root;
			}

			run.ReplaceWith(content);

			image.Dispose();

			return 1;
		}


		private async Task<Image> DownloadImage(string url)
		{
			var client = HttpClientFactory.Create();

			using (var source = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
			{
				using (var response = await client.GetAsync(new Uri(url, UriKind.Absolute), source.Token))
				{
					if (response.IsSuccessStatusCode)
					{
						using (var stream = new MemoryStream())
						{
							await response.Content.CopyToAsync(stream);

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
				}
			}

			return null;
		}


		private void SplitElement(XElement run, XElement anchor)
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


		private Table WrapWithCitation(XElement image, string caption)
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
