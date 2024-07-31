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


	/// <summary>
	/// Splits the current page at each Heading1 or page links. Also, these can be filtered by
	/// an optional tag.
	/// </summary>
	/// <remarks>
	/// This will create new pages in the current section. If splitting on page links, if the
	/// linked pages exists, it must exist in the current section or it will not be found and a
	/// new page will be created in the current section; if it is found then the content will be
	/// appended to that page.
	/// </remarks>
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
			using var dialog = new SplitDialog();
			if (dialog.ShowDialog(owner) == DialogResult.OK)
			{
				await using (one = new OneNote(out page, out ns, OneNote.PageDetail.All))
				{
					await SplitPage(dialog.SplitByHeading, dialog.Tagged ? dialog.TagSymbol : -1);
				}
			}
		}


		private async Task SplitPage(bool byHeading, int tagSymbol)
		{
			var headings = GetHeadings(byHeading, tagSymbol);

			if (headings.Count == 0)
			{
				return;
			}

			if (headings.Exists(h => Regex.IsMatch(h.Root.GetCData().Value, LinkPattern)))
			{
				await BuildHyperlinkCache();
			}

			for (int i = 0; i < headings.Count; i++)
			{
				var heading = headings[i];

				// find the target page
				var target = await GetTargetPage(heading);

				// copy content to the target page...

				// collect content related to this heading
				var next = i < headings.Count - 1 ? headings[i + 1] : null;
				var content = GetContent(heading, next);

				// target is a new blank page so we can blindly append
				var container = target.EnsureContentContainer();
				container.Add(content);

				// copy related quick styles
				var map = target.MergeQuickStyles(page);
				target.ApplyStyleMapping(map, container);

				await one.Update(target);

				if (!heading.IsHyper)
				{
					// remove existing runs
					heading.Root.Elements(ns + "T").Remove();

					// add new hyperlinked run
					var link = one.GetHyperlink(target.PageId, string.Empty);
					var run = new XElement(ns + "T",
							new XCData($"<a href=\"{link}\">{heading.Text}</a>"));

					var tags = heading.Root.Elements(ns + "Tag");
					if (tags.Any())
					{
						// schema sequence, must follow Tag elements
						tags.Last().AddAfterSelf(run);
					}
					else
					{
						heading.Root.AddFirst(run);
					}
				}

				// remove content from current page
				content.Remove();
			}

			// remove any empty OEChildren that may be left over
			page.Root.Descendants(ns + "OEChildren")
				.Where(e => !e.Elements().Any())
				.Remove();

			await one.Update(page);
		}


		private List<Heading> GetHeadings(bool byHeading, int tagSymbol)
		{
			List<Heading> headings;

			if (byHeading)
			{
				// find all H1 headings
				headings = page.GetHeadings(one)
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
				headings = page.Root.Descendants(ns + "T")
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
				headings = headings.Where(e =>
					e.Root.Elements(ns + "Tag").Any(t => t.Attribute("index").Value == tagIndex))
					.ToList();
			}

			foreach (var heading in headings)
			{
				// get heading text, possibly bisected by insertion cursor
				heading.Text = heading.Root.Elements(ns + "T")
					.Select(e => e.Value).Aggregate((x, y) => $"{x}{y}");

				// determine heading's document level, to be compare in relation to other headings
				var level = 0;
				var outline = heading.Root.Parent;
				while (outline != null && outline.Name.LocalName != "Outline")
				{
					outline = outline.Parent;
					level++;
				}

				heading.Outline = outline;
				heading.Level = level;
			}

			return headings;
		}


		private async Task BuildHyperlinkCache()
		{
			// there's no direct way to map onenote:http URIs to page IDs so create a cache
			// of all pages in the current section and lookup the URI for each of them...

			hyperlinks = new Dictionary<string, string>();

			var section = await one.GetSection();
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


		private async Task<Page> GetTargetPage(Heading header)
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
						return await one.GetPage(hyperlinks[key], OneNote.PageDetail.Basic);
					}
				}
			}

			// create a page in this section to capture heading content
			one.CreatePage(one.CurrentSectionId, out var pageId);
			var target = await one.GetPage(pageId);

			target.Title = header.Text;

			return target;
		}


		private IEnumerable<XElement> GetContent(Heading heading, Heading next)
		{
			var content = new List<XElement>();

			var children = heading.Root.Elements(ns + "OEChildren");
			if (children.Any())
			{
				content.AddRange(children.Elements());
			}

			var after = heading.Root.ElementsAfterSelf();
			if (next != null)
			{
				after = after.TakeWhile(e => e != next.Root);
			}

			if (after.Any())
			{
				content.AddRange(after);
			}

			if (!content.Any())
			{
				// provide default content - empty line - if header is not followed by anything
				content.Add(new XElement(ns + "OE",
					new XElement(ns + "T", new XCData(string.Empty))
					));
			}

			return content;
		}
	}
}
