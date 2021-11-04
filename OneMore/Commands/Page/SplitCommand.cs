//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S125 // Sections of code should not be commented out

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml;
	using System.Xml.Linq;


	internal class SplitCommand : Command
	{
		// OneNote internal hyperlinks are represented in CDATA similar to:
		// <a href="onenote:#three&amp;
		//	section-id={A640CEA0-536E-4ED0-ACC1-428AAB96501F}&amp;
		//	page-id={C9709FD9-6044-4A82-BB2E-E829884B364A}&amp;
		//	end&amp;base-path=https://d..."

		private const string LinkPattern = @"\<a\s*?href=""onenote\:#";
		private const string IDPattern = @"section-id=(?<sid>{.*?}).*?page-id=(?<pid>{.*?})";

		private Dictionary<string, string> hyperlinks;
		private OneNote one;
		private Page page;
		private XNamespace ns;


		public SplitCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var dialog = new SplitDialog())
			{
				if (dialog.ShowDialog(owner) == DialogResult.OK)
				{
					using (one = new OneNote(out page, out ns, OneNote.PageDetail.All))
					{
						await SplitPage(dialog.SplitByHeading, dialog.Tagged ? dialog.TagSymbol : -1);
					}
				}
			}
		}


		private async Task SplitPage(bool byHeading, int tagSymbol)
		{
			var headers = GetHeaders(byHeading, tagSymbol);

			if (headers.Count == 0)
			{
				return;
			}

			if (headers.Any(h => Regex.IsMatch(h.Root.GetCData().Value, LinkPattern)))
			{
				BuildHyperlinkCache();
			}

			for (int i = 0; i < headers.Count; i++)
			{
				var header = headers[i];

				// find the target page
				var target = GetTargetPage(header);

				// copy content to the target page
				var content = header.Root.ElementsAfterSelf();

				if (i < headers.Count - 1)
				{
					// trim content up to the next header
					var next = headers[i + 1];
					var mark = next.Root;

					if (next.Outline == header.Outline && next.Level > header.Level)
					{
						var level = next.Level;
						while (level > header.Level)
						{
							mark = mark.Parent;
							level--;
						}
					}

					content = content.TakeWhile(e => e != mark);
				}

				// copy content along with related quick styles
				var container = target.AddContent(content);
				var map = target.MergeQuickStyles(page);
				target.ApplyStyleMapping(map, container);

				await one.Update(target);

				if (!header.IsHyper)
				{
					// remove existing runs
					header.Root.Elements(ns + "T").Remove();

					// add new hyperlinked run
					var link = one.GetHyperlink(target.PageId, string.Empty);
					var run = new XElement(ns + "T",
							new XCData($"<a href=\"{link}\">{header.Text}</a>"));

					var tags = header.Root.Elements(ns + "Tag");
					if (tags.Any())
					{
						// schema sequence, must follow Tag elements
						tags.Last().AddAfterSelf(run);
					}
					else
					{
						header.Root.AddFirst(run);
					}
				}

				// remove content from current page
				content.Remove();
			}

			await one.Update(page);
		}


		private List<Heading> GetHeaders(bool byHeading, int tagSymbol)
		{
			List<Heading> headers;

			if (byHeading)
			{
				// find all H1 headings
				headers = page.GetHeadings(one)
					.Where(h => h.Level == 1)
					.Select(h => new Heading
					{
						Root = h.Root,

						IsHyper = h.Root.FirstNode.NodeType == XmlNodeType.CDATA &&
								Regex.IsMatch(((XCData)h.Root.FirstNode).Value, LinkPattern)
					})
					.ToList();
			}
			else
			{
				// find all internal OneNote hyperlinks, regardless of syyle
				headers = page.Root.Descendants(ns + "T")
					.Where(e =>
						e.FirstNode.NodeType == XmlNodeType.CDATA &&
						Regex.IsMatch(((XCData)e.FirstNode).Value, LinkPattern))
					.Select(e => new Heading { Root = e.Parent, IsHyper = true })
					.ToList();
			}

			if (tagSymbol >= 0)
			{
				// find the index of the tagdef of tagSymbol
				var tagIndex = page.Root.Elements(ns + "TagDef")
					.Where(e => e.Attribute("symbol").Value == tagSymbol.ToString())
					.Select(e => e.Attribute("index").Value).FirstOrDefault();

				// filter tagged breaks
				headers = headers.Where(e =>
					e.Root.Elements(ns + "Tag").Any(t => t.Attribute("index").Value == tagIndex))
					.ToList();
			}

			foreach (var header in headers)
			{
				// get header text, possibly bisected by insertion cursor
				header.Text = header.Root.Elements(ns + "T")
					.Select(e => e.Value).Aggregate((x, y) => $"{x}{y}");

				// determine heading's document level, to be compare in relation to other headings
				var level = 0;
				var outline = header.Root.Parent;
				while (outline != null && outline.Name.LocalName != "Outline")
				{
					outline = outline.Parent;
					level++;
				}

				header.Outline = outline;
				header.Level = level;
			}

			return headers;
		}


		private void BuildHyperlinkCache()
		{
			// there's no direct way to map onenote:http URIs to page IDs so create a cache
			// of all pages in the current section and lookup the URI for each of them...

			hyperlinks = new Dictionary<string, string>();

			var section = one.GetSection();
			var pageIDs = section.Descendants(ns + "Page")
				.Select(e => e.Attribute("ID").Value);

			foreach (var pageID in pageIDs)
			{
				var link = one.GetHyperlink(pageID, string.Empty);
				var matches = Regex.Match(link, IDPattern);
				if (matches.Success)
				{
					var sid = matches.Groups["sid"].Value;
					var pid = matches.Groups["pid"].Value;
					hyperlinks.Add($"{sid}{pid}", pageID);
				}
			}
		}


		private Page GetTargetPage(Heading header)
		{
			// first test if heading is a link to a page in this section
			var cdata = header.Root.GetCData();

			if (header.IsHyper)
			{
				var matches = Regex.Match(cdata.Value, IDPattern);
				if (matches.Success)
				{
					var sid = matches.Groups["sid"].Value;
					var pid = matches.Groups["pid"].Value;
					var key = $"{sid}{pid}";

					if (hyperlinks.ContainsKey(key))
					{
						return one.GetPage(hyperlinks[key], OneNote.PageDetail.Basic);
					}
				}
			}

			// create a page in this section to capture heading content
			one.CreatePage(one.CurrentSectionId, out var pageId);
			var target = one.GetPage(pageId);

			target.Title = header.Text;

			return target;
		}
	}
}
