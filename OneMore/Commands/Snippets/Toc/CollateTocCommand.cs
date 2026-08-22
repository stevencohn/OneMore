//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using Snippets.Toc;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Collates the on-page, section, and notebook tables of content found on hashtag-tagged
	/// pages across one or more selected notebooks into a single index page.
	/// </summary>
	internal class CollateTocCommand : Command
	{
		public override async Task Execute(params object[] args)
		{
			using var guard = EnterOnce();
			if (guard is null) { return; }

			await using var one = new OneNote();

			var books = await one.GetNotebooks();
			var bookNs = one.GetNamespace(books);

			List<string> notebookIds;
			List<string> hashtags;

			using (var dialog = new CollateTocDialog(books.Elements(bookNs + "Notebook")))
			{
				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}

				notebookIds = dialog.SelectedNotebookIds;
				hashtags = dialog.Hashtags;
			}

			var criteria = string.Join(" OR ", hashtags);

			var tags = new Hashtags();
			using (var provider = new HashtagProvider())
			{
				foreach (var notebookId in notebookIds)
				{
					tags.AddRange(provider.SearchTags(
						criteria, false, false, out _, notebookID: notebookId));
				}
			}

			var pages = tags
				.GroupBy(t => t.PageID)
				.Select(g => new HashtagContext(g.First()))
				.OrderBy(c => c.HierarchyPath).ThenBy(c => c.PageTitle)
				.ToList();

			if (pages.Count == 0)
			{
				ShowInfo(Resx.CollateTocCommand_noPagesFound);
				return;
			}

			var section = await one.GetSection();
			var sectionNs = one.GetNamespace(section);

			var existingId = section.Elements(sectionNs + "Page")
				.Where(e => e.Attribute("isInRecycleBin") is null)
				.Elements(sectionNs + "Meta")
				.Where(e =>
					e.Attribute("name").Value == MetaNames.TableOfContents &&
					e.Attribute("content").Value == "index")
				.Select(e => e.Parent.Attribute("ID").Value)
				.FirstOrDefault();

			Page dest;

			if (existingId is not null)
			{
				var result = UI.MoreMessageBox.ShowQuestion(
					owner, Resx.CollateTocCommand_replaceQuestion, true);

				if (result == DialogResult.Cancel)
				{
					return;
				}

				if (result == DialogResult.Yes)
				{
					dest = await one.GetPage(existingId);
					dest.EnsureContentContainer().Elements().Remove();
				}
				else
				{
					dest = await CreateDestinationPage(one, section.Attribute("ID").Value);
				}
			}
			else
			{
				dest = await CreateDestinationPage(one, section.Attribute("ID").Value);
			}

			PageNamespace.Set(dest.Namespace);

			dest.SetMeta(MetaNames.TableOfContents, "index");
			var container = dest.EnsureContentContainer();
			var pageTitleIndex = dest.GetQuickStyle(StandardStyles.PageTitle).Index;

			using (var progress = new UI.ProgressDialog())
			{
				progress.SetMaximum(pages.Count);
				progress.Show();

				foreach (var context in pages)
				{
					progress.SetMessage(context.PageTitle);
					progress.Increment();

					await CollateOnePage(one, dest, container, context, pageTitleIndex);
				}
			}

			await one.UpdateWithProgress(dest);
			await one.NavigateTo(dest.PageId);
		}


		private static async Task<Page> CreateDestinationPage(OneNote one, string sectionId)
		{
			one.CreatePage(sectionId, out var pageId);
			var page = await one.GetPage(pageId);
			page.Title = Resx.CollateTocCommand_indexTitle;
			return page;
		}


		private async Task CollateOnePage(
			OneNote one, Page dest, XElement container, HashtagContext context, int pageTitleIndex)
		{
			var source = await one.GetPage(context.PageID, OneNote.PageDetail.Basic);
			var ns = source.Namespace;

			var metas = source.BodyOutlines
				.Descendants(ns + "Meta")
				.Where(e => e.Attribute("name")?.Value == Toc.MetaName)
				.ToList();

			if (metas.Count == 0)
			{
				return;
			}

			var breadcrumb = BuildBreadcrumb(one, context);
			var fragments = new List<XElement>();

			foreach (var meta in metas)
			{
				var content = meta.Attribute("content")?.Value ?? string.Empty;

				if (content.StartsWith("section") || content.StartsWith("notebook"))
				{
					fragments.AddRange(ExtractHierarchyScopeFragment(meta, ns, breadcrumb, pageTitleIndex));
				}
				else
				{
					fragments.Add(ExtractPageScopeFragment(meta, ns, breadcrumb, pageTitleIndex));
				}
			}

			if (fragments.Count == 0)
			{
				return;
			}

			// remap this source page's quick styles onto the destination page BEFORE
			// appending any of its fragments, so paragraph quickStyleIndex references
			// point at the destination's own style list rather than the source's
			var map = dest.MergeQuickStyles(source);

			foreach (var fragment in fragments)
			{
				fragment.Attributes("objectID").Remove();
				fragment.Descendants().Attributes("objectID").Remove();
				dest.ApplyStyleMapping(map, fragment);
			}

			// visually separate each collated TOC, matching the single Horizontal Line
			// snippet (InsertSingleLineCommand)
			InsertLineCommand.AppendLine(one, dest, '─');

			foreach (var fragment in fragments)
			{
				container.Add(fragment);
			}

			container.Add(new Paragraph(string.Empty));
		}


		/// <summary>
		/// Extracts a page-scope on-page TOC: the omToc meta and its following Table
		/// sibling live under the same container OE. Clones that OE, drops the meta
		/// (so no live omToc marker survives on the destination page), and rewrites
		/// the table's title row.
		/// </summary>
		private XElement ExtractPageScopeFragment(
			XElement meta, XNamespace ns, string breadcrumb, int pageTitleIndex)
		{
			var container = new XElement(meta.Parent);

			container.Elements(ns + "Meta")
				.First(e => e.Attribute("name")?.Value == Toc.MetaName)
				.Remove();

			var table = container.Element(ns + "Table");
			if (table is not null)
			{
				RewriteTitleRun(table.Descendants(ns + "OE").FirstOrDefault(), breadcrumb, pageTitleIndex);
			}

			return container;
		}


		/// <summary>
		/// Extracts a section- or notebook-scope TOC: the omToc meta is the first child
		/// of the title OE itself, and the entire TOC content is the ordered set of
		/// sibling OEs following the title OE under their shared parent OEChildren.
		/// Clones the title OE and every following sibling, drops the meta from the
		/// cloned title, and rewrites the title's text.
		/// </summary>
		private List<XElement> ExtractHierarchyScopeFragment(
			XElement meta, XNamespace ns, string breadcrumb, int pageTitleIndex)
		{
			var title = new XElement(meta.Parent);

			title.Elements(ns + "Meta")
				.First(e => e.Attribute("name")?.Value == Toc.MetaName)
				.Remove();

			RewriteTitleRun(title, breadcrumb, pageTitleIndex);

			var fragments = new List<XElement> { title };
			fragments.AddRange(meta.Parent.ElementsAfterSelf().Select(e => new XElement(e)));

			return fragments;
		}


		/// <summary>
		/// Rewrites a TOC's title/heading OE: prepends the breadcrumb, strips the
		/// "[Refresh]" link, and normalizes the paragraph to the destination page's
		/// PageTitle quick style so every collated entry looks consistent
		/// </summary>
		private void RewriteTitleRun(XElement oe, string breadcrumb, int pageTitleIndex)
		{
			if (oe is null)
			{
				return;
			}

			// OneNote routinely splits a single logical line across multiple T runs once
			// saved and reloaded (e.g. at style boundaries or wherever the cursor last sat -
			// see Page.GetHeadings' identical concat-all-T-runs handling), so the "[Refresh]"
			// text/link may live in a run other than the first; gather them all before
			// stripping it, then collapse back into a single run
			var runs = oe.Elements(oe.Name.Namespace + "T").ToList();
			if (runs.Count == 0)
			{
				return;
			}

			var cdata = runs[0].GetCData();
			if (cdata is null)
			{
				return;
			}

			var html = string.Concat(runs.Select(t => t.Value));
			cdata.Value = $"{breadcrumb}: {Toc.StripRefreshLink(html)}";

			for (var i = 1; i < runs.Count; i++)
			{
				runs[i].Remove();
			}

			// force PageTitle so every collated heading renders consistently, regardless
			// of whatever style/formatting the source page's own title carried
			oe.SetAttributeValue("style", null);
			oe.SetAttributeValue("quickStyleIndex", pageTitleIndex);
		}


		private string BuildBreadcrumb(OneNote one, HashtagContext context)
		{
			var notebook = one.GetHierarchyNode(context.NotebookID);
			var section = one.GetHierarchyNode(context.SectionID);
			var pageLink = one.GetHyperlink(context.PageID,
				string.IsNullOrWhiteSpace(context.TitleID) ? string.Empty : context.TitleID);

			string Crumb(string link, string name) =>
				$"<a href=\"{link}\">{SecurityElement.Escape(name)}</a>";

			return $"{Crumb(notebook.Link, notebook.Name)} → {Crumb(section.Link, section.Name)} " +
				$"→ {Crumb(pageLink, context.PageTitle)}";
		}
	}
}
