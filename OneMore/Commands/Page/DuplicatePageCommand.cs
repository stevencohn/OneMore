//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Commands.Snippets.Toc;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.Styles;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Simple and direct duplication of current page, inserting the new page immediate
	/// after the current page in the section. Adds (#) after the page title.
	/// </summary>
	internal class DuplicatePageCommand : Command
	{

		public DuplicatePageCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			// each of these three features is off by default; enable them in
			// Settings > Page
			var settings = new SettingsProvider().GetCollection(nameof(PageSheet));
			var insertNote = settings.Get<bool>("insertNote");
			var insertBacklinks = settings.Get<bool>("insertBacklinks");
			var refreshToc = settings.Get<bool>("refreshToc");

			// capture page and section IDs before handing off to background thread
			await using var ctx = new OneNote();
			var originalId = ctx.CurrentPageId;
			var sectionId = ctx.CurrentSectionId;

			// set once known; read after phase 1 completes
			string newPageId = null;
			List<(string Text, string Link)> headingLinks = null;
			TocParameters tocParameters = null;
			var phase1Closed = new TaskCompletionSource<bool>();

			// PHASE 1: duplicate the page and add the top-of-page "Duplicated from" citation
			var progress1 = new UI.ProgressDialog(async (dialog, token) =>
			{
				dialog.SetMaximum(3);

				// need All detail to copy images and Ink
				dialog.SetMessage("Getting page content...");
				await using var one = new OneNote();
				var page = await one.GetPage(originalId, OneNote.PageDetail.All);
				dialog.Increment();

				if (token.IsCancellationRequested) { dialog.Close(); return; }

				// must run while page.PageId still refers to the original page so
				// hyperlinks resolve back to it rather than the not-yet-created copy
				if (insertNote)
				{
					dialog.SetMessage("Add duplicated citation...");
					InsertDuplicationCitation(page, one, originalId);
				}

				if (insertBacklinks)
				{
					dialog.SetMessage("Capturing heading links...");
					headingLinks = CaptureHeadingLinks(page, one);
				}

				if (refreshToc)
				{
					// the omToc meta content string is copied verbatim below regardless
					// of what else happens to the tree, so simplest to capture it now
					tocParameters = GetTocParameters(page);
				}

				dialog.SetMessage("Creating duplicate page...");

				// create a new page with new ID and update its title
				one.CreatePage(sectionId, out newPageId);
				var newId = newPageId;

				// set the page ID to the new page's ID
				page.Root.Attribute("ID").Value = newId;

				// ensure unique OneMore page ID; create or override always
				page.SetMeta(MetaNames.PageID, Guid.NewGuid().ToString("N"));

				// remove all objectID values and let OneNote generate new IDs
				page.Root.Descendants().Attributes("objectID").Remove();
				page = new Page(page.Root); // reparse to refresh PageId

				var section = await one.GetSection(sectionId);
				var editor = new SectionEditor(section);

				// restore Title if it's hidden; the Interop API doesn't let us delete Title!
				if (page.Title is null)
				{
					page.SetTitle(page.Root.Attribute("name").Value);
				}

				editor.SetUniquePageTitle(page);
				dialog.Increment();

				if (token.IsCancellationRequested) { dialog.Close(); return; }

				dialog.SetMessage("Uploading duplicate page...");
				await one.Update(page);
				dialog.Increment();

				dialog.SetMessage("Reorganizing section...");
				if (editor.MovePageAfterAnchor(page.PageId, originalId))
				{
					one.UpdateHierarchy(section);
				}

				dialog.Close();
			});

			// closedAction only signals that phase 1's dialog has closed; it does no
			// other work itself so NavigateTo/Delay below run as plain code in this
			// method's own continuation, not inside any ProgressDialog callback
			progress1.RunModeless((sender, e) => phase1Closed.TrySetResult(true));

			await phase1Closed.Task;

			if (string.IsNullOrEmpty(newPageId))
			{
				return;
			}

			// between phases: navigate to the duplicate and let it settle, isolated from
			// both dialogs' own callback/thread contexts, to avoid a crash that occurred
			// when navigation was attempted from within either dialog's own context
			await using (var one = new OneNote())
			{
				await one.NavigateTo(newPageId);
			}

			// let the navigation settle before treating newPageId as current
			// (see WhereAmIWindow.SelectSibling)
			await Task.Delay(150);

			// PHASE 2: add heading citations and refresh the TOC (if any) against a
			// freshly loaded copy of the duplicate, isolated from phase 1 since doing
			// this directly against the not-yet-uploaded page reliably crashed OneNote
			// on next restart - separately, GetHyperlinkToObject only resolves a TOC
			// self-link correctly once the target page is the one currently active in
			// OneNote's UI, which the NavigateTo above now ensures for this phase
			var progress2 = new UI.ProgressDialog(async (dialog, token) =>
			{
				dialog.SetMaximum(tocParameters is null ? 2 : 3);

				dialog.SetMessage("Loading duplicate page...");
				await using var two = new OneNote();
				var page = await two.GetPage(newPageId, OneNote.PageDetail.All);
				dialog.Increment();

				if (token.IsCancellationRequested) { dialog.Close(); return; }

				dialog.SetMessage("Add heading based-on citations...");
				InsertHeadingCitations(page, two, headingLinks);

				dialog.SetMessage("Saving duplicate page...");
				await two.Update(page);
				dialog.Increment();

				if (tocParameters is not null)
				{
					dialog.SetMessage("Refreshing table of contents...");
					var generator = new PageTocGenerator(tocParameters, newPageId);
					await generator.Build();
					dialog.Increment();
				}

				dialog.Close();
			});

			progress2.RunModeless();
		}


		// marks the "Duplicated from" citation OE so it can be found again regardless
		// of locale - the resx string is a reorderable format string, so its rendered
		// text can no longer be assumed to start with any particular fixed substring
		private const string DuplicatedFromMeta = "omDuplicatedFrom";


		/// <summary>
		/// Inserts a Citation-styled line at the top of the first outline linking back to
		/// the original page that this page was duplicated from, followed by a blank line.
		/// If the page already has such a line (because it was itself a duplicate), that
		/// line is replaced in place rather than stacking a new one on top, so each page
		/// always points only to its immediate predecessor and the full history can be
		/// found by clicking back through each page in turn.
		/// </summary>
		/// <remarks>
		/// Must be called before <paramref name="page"/>'s ID is changed to the new page's
		/// ID, since <paramref name="originalId"/> is used to build the link.
		/// </remarks>
		private static void InsertDuplicationCitation(Page page, OneNote one, string originalId)
		{
			var title = page.Title;
			var link = one.GetHyperlink(originalId, string.Empty);
			if (string.IsNullOrEmpty(link))
			{
				return;
			}

			var ns = page.Namespace;
			var anchor = $"<a href=\"{link}\">{WebUtility.HtmlEncode(title)}</a>";
			var content = string.Format(Resx.DuplicatePageCommand_duplicatedFrom, anchor);
			var container = page.EnsureContentContainer(last: false);

			var existing = container.Elements(ns + "OE").FirstOrDefault(e =>
				e.Elements(ns + "Meta").Any(m => m.Attribute("name")?.Value == DuplicatedFromMeta));

			if (existing is not null)
			{
				// keep the Meta marker; only replace the text
				existing.Elements(ns + "T").Remove();
				existing.Add(new XElement(ns + "T", new XCData(content)));
				return;
			}

			var index = page.GetQuickStyle(StandardStyles.Citation).Index;
			var citation = new Paragraph(ns, content).SetQuickStyle(index);
			citation.AddFirst(new Meta(ns, DuplicatedFromMeta, "1"));

			container.AddFirst(citation, new Paragraph(ns));
		}


		/// <summary>
		/// Captures each heading's text and hyperlink from the (pre-duplication)
		/// original page, for later use by <see cref="InsertHeadingCitations"/> once the
		/// duplicate exists. Must run while <paramref name="page"/>'s ID still refers to
		/// the original page, so each link resolves back to it.
		/// </summary>
		/// <returns>
		/// The original page's headings, in document order, as (Text, Link) pairs -
		/// Link may be null/empty for a heading whose hyperlink could not be resolved
		/// </returns>
		private static List<(string Text, string Link)> CaptureHeadingLinks(Page page, OneNote one)
		{
			return page.GetHeadings(one)
				.Select(h => (h.Text, h.Link))
				.ToList();
		}


		/// <summary>
		/// Inserts a Citation-styled line immediately after each heading in the
		/// (duplicate) page, linking back to the corresponding heading captured from the
		/// original page by <see cref="CaptureHeadingLinks"/>. Headings are matched to
		/// captured entries by document order, since duplication preserves heading order
		/// and count.
		/// </summary>
		/// <remarks>
		/// This runs against a freshly loaded copy of the duplicate, separately from the
		/// rest of duplication, because inserting these citations directly into the page
		/// content before it's first uploaded reliably crashed OneNote on next restart.
		/// </remarks>
		private static void InsertHeadingCitations(
			Page page, OneNote one, List<(string Text, string Link)> headingLinks)
		{
			if (headingLinks is null || headingLinks.Count == 0)
			{
				return;
			}

			var headings = page.GetHeadings(one, linked: false);
			var index = page.GetQuickStyle(StandardStyles.Citation).Index;
			var datestamp = DateTime.Now.ToShortFriendlyString();

			var count = Math.Min(headings.Count, headingLinks.Count);
			for (var i = 0; i < count; i++)
			{
				var (text, link) = headingLinks[i];
				if (string.IsNullOrEmpty(link))
				{
					continue;
				}

				var anchor = $"<a href=\"{link}\">{WebUtility.HtmlEncode(text)}</a>";
				var content = string.Format(Resx.DuplicatePageCommand_basedOn, anchor, datestamp);

				var citation = new Paragraph(page.Namespace, content)
					.SetQuickStyle(index);

				headings[i].Root.AddAfterSelf(citation);
			}
		}


		/// <summary>
		/// If the page carries a page-level table of contents, returns the parameters it
		/// was originally built with (same "page/links/level3/..." segments stored in the
		/// omToc meta's "content" attribute), for use with <see cref="PageTocGenerator"/>
		/// to rebuild it against the duplicate's own headings once they exist.
		/// </summary>
		/// <returns>The parameters, or null if the page has no table of contents</returns>
		private static TocParameters GetTocParameters(Page page)
		{
			var meta = page.BodyOutlines.Descendants(page.Namespace + "Meta")
				.FirstOrDefault(m => m.Attribute("name")?.Value == Toc.MetaName);

			var content = meta?.Attribute("content")?.Value;

			return !string.IsNullOrEmpty(content)
				? new TocParameters(content.Split(
					new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
				: null;
		}
	}
}
