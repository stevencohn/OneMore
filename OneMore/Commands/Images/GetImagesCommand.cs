//************************************************************************************************
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
	using System.Net;
	using System.Net.Http;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class GetImagesCommand : Command
	{
		private ImageDetector detector;
		private XNamespace ns;


		public GetImagesCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			detector = new ImageDetector();

			using (var one = new OneNote(out var page, out ns))
			{
				if (GetImages(page))
				{
					await one.Update(page);
				}
			}
		}


		private bool GetImages(Page page)
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

				var tasks = new List<Task<int>>();
				Parallel.ForEach(elements, (element) =>
				{
					tasks.Add(GetImage(element));
				});

				Task.WaitAll(tasks.ToArray());

				count = tasks.Sum(t => t.Result);
			}

			return count > 0;
		}


		private async Task<int> GetImage(XElement element)
		{
			var cdata = element.GetCData();

			var wrapper = cdata.GetWrapper();
			var a = wrapper.Element("a");
			if (a != null)
			{
				var href = a.Attribute("href")?.Value;
				if (href != null && href == a.Value)
				{
					Image image;
					var watch = new Stopwatch();
					watch.Start();

					try
					{
						image = await DownloadImage(href);
						watch.Stop();
					}
					catch
					{
						watch.Stop();
						logger.WriteLine($"cannot resolve {href} after {watch.ElapsedMilliseconds}ms");
						return 0;
					}

					if (image != null)
					{
						//logger.WriteLine($"resolved {href} in {watch.ElapsedMilliseconds}ms");

						// create a new OE/Image before current OE/T/A
						element.Parent.AddBeforeSelf(
							new XElement(ns + "OE",
							new XElement(ns + "Image",
								new XAttribute("format", "png"),
								new XElement(ns + "Size",
									new XAttribute("width", $"{image.Width}.0"),
									new XAttribute("height", $"{image.Height}.0")),
								new XElement(ns + "Data", image.ToBase64String())
							)));

						// remove A from CDATA since CDATA might contain more text than just <A/>
						a.Remove();
						cdata.ReplaceWith(wrapper.GetInnerXml());

						image.Dispose();

						return 1;
					}
					//else
					//{
					//	logger.WriteLine($"cannot resolve {href} as an image");
					//}
				}
			}

			return 0;
		}


		private async Task<Image> DownloadImage(string url)
		{
			using (var client = new HttpClient())
			{
				client.Timeout = TimeSpan.FromSeconds(12);

				using (var response = await client.GetAsync(new Uri(url, UriKind.Absolute)))
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
	}
}
