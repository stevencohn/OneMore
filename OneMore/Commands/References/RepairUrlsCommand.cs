//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.Styles;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal class RepairUrlsCommand : Command
	{
		private OneNote one;
		private Page page;
		private List<XElement> candidates;
		private Dictionary<string, OneNote.HyperlinkInfo> map;

		private const string SettingsName = nameof(RepairUrlsCommand);
		private const string DefaultMarkColor = "#FF0000";

		private RepairUrlsScope scope;
		private string markColor = DefaultMarkColor;
		private List<string> pageIDs;
		private bool perLinkProgress;
		private bool repairLinks = true;
		private bool markLinks = true;
		private bool onenoteOnly;

		private ProgressDialog progressDialog;
		private readonly TaskCompletionSource<bool> progressClosed = new();

		// what happened to each link that was not fine, for the report
		private readonly List<RepairUrlsResult> results = new();
		private string sourceID;
		private string sourceTitle;
		private string sectionID;

		// names of the open notebooks, to tell a link to a closed or foreign notebook from a broken one
		private HashSet<string> openNotebooks;
		private int progressMax;
		private int progressValue;

		private OneNote.Scope mapScope;
		private bool mapBuilt;
		private Dictionary<string, OneNote.HyperlinkInfo> mapByGuid;
		private Workspaces.TargetResolver resolver;
		private Identity.PageIdentityProvider identity;

		// counts of links, updated while links are checked one page at a time
		private int badCount;           // marked
		private int repairedCount;
		private int changeCount;        // text elements changed
		private int unverifiedCount;
		private int offlineCount;
		private int ambiguousCount;
		private int paragraphCount;
		private Exception exception;


		public RepairUrlsCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			if (!HttpClientFactory.IsNetworkAvailable())
			{
				ShowInfo(Resx.NetwordConnectionUnavailable);
				return;
			}

			await using (one = new OneNote(out page, out _))
			{

				// the hierarchy of the current notebook tells which scopes make sense
				var hierarchy = await one.GetNotebook(OneNote.Scope.Pages);
				var ns = one.GetNamespace(hierarchy);
				var current = GetCurrentPageElement(hierarchy, ns);

				// kept for the results dialog, which copies its list to a page in this section
				sectionID = current?.Parent?.Attribute("ID")?.Value;

				using var dialog = new RepairUrlsDialog(
					GetPageGroup(current, ns, out _) is not null,
					GetSectionGroup(current, ns) is not null);

				var provider = new SettingsProvider();
				var settings = provider.GetCollection(SettingsName);
				try
				{
					dialog.MarkColor = ColorTranslator.FromHtml(
						settings.Get("markColor", DefaultMarkColor));
				}
				catch (Exception exc)
				{
					logger.WriteLine("invalid highlight color setting", exc);
				}

				dialog.Repair = settings.Get("repair", true);
				dialog.Mark = settings.Get("mark", true);
				dialog.OnenoteOnly = settings.Get("onenoteOnly", false);

				if (dialog.ShowDialog(owner) != System.Windows.Forms.DialogResult.OK)
				{
					return;
				}


				scope = dialog.Scope;
				markColor = dialog.MarkColor.ToRGBHtml();
				repairLinks = dialog.Repair;
				markLinks = dialog.Mark;
				onenoteOnly = dialog.OnenoteOnly;

				settings.Add("onenoteOnly", onenoteOnly);
				settings.Add("markColor", markColor);
				settings.Add("repair", repairLinks);
				settings.Add("mark", markLinks);
				provider.SetCollection(settings);
				provider.Save();

				if (scope == RepairUrlsScope.Page)
				{
					perLinkProgress = true;
					candidates = GetCandiateElements(page, true);
					if (candidates.Count == 0)
					{
						return;
					}
				}
				else
				{
					pageIDs = await GetPageIDs(hierarchy, ns, current);
					logger.WriteLine($"scope {scope} has {pageIDs.Count} pages");
					if (pageIDs.Count == 0)
					{
						return;
					}
				}

				var progress = new ProgressDialog(Execute);

				// RunModeless returns when the dialog closes if it runs its own message loop,
				// and returns at once if it joins an existing one, so wait either way
				progress.RunModeless(ReportResult);
				await progressClosed.Task;

				ShowReport();
			}
		}


		#region Scope
		private XElement GetCurrentPageElement(XElement hierarchy, XNamespace ns)
		{
			return hierarchy.Descendants(ns + "Page")
				.FirstOrDefault(e => e.Attribute("ID")?.Value == page.PageId);
		}


		/// <summary>
		/// Gets the section group that holds the section of the given page, or null if the
		/// section is directly in the notebook.
		/// </summary>
		private static XElement GetSectionGroup(XElement current, XNamespace ns)
		{
			var group = current?.Parent?.Parent;
			return group is not null &&
				group.Name == ns + "SectionGroup" &&
				group.Attribute("isRecycleBin") is null
				? group
				: null;
		}


		/// <summary>
		/// Gets the pages of the page group that includes the given page: the page and all
		/// of its subpages, or its parent and all of the parent's subpages if the page is
		/// itself a subpage. Returns null if the page is neither a parent nor a subpage.
		/// </summary>
		private static List<XElement> GetPageGroup(
			XElement current, XNamespace ns, out int currentIndex)
		{
			currentIndex = -1;
			if (current?.Parent is null)
			{
				return null;
			}

			var siblings = current.Parent.Elements(ns + "Page").ToList();
			var levels = siblings.Select(e =>
				int.TryParse(e.Attribute("pageLevel")?.Value, out var n) ? n : 1).ToList();

			currentIndex = siblings.IndexOf(current);
			if (!GetPageGroupRange(levels, currentIndex, out var first, out var last))
			{
				return null;
			}

			return siblings.GetRange(first, last - first + 1);
		}


		/// <summary>
		/// Finds the range of pages that form the group of the page at the given index.
		/// </summary>
		/// <param name="levels">The pageLevel of each page of a section, in order</param>
		/// <param name="index">The index of the current page</param>
		/// <param name="first">The index of the first page of the group, its parent</param>
		/// <param name="last">The index of the last page of the group</param>
		/// <returns>
		/// False if the page has no subpages and is not itself a subpage
		/// </returns>
		internal static bool GetPageGroupRange(
			IList<int> levels, int index, out int first, out int last)
		{
			first = last = index;
			if (index < 0 || index >= levels.Count)
			{
				return false;
			}

			if (index + 1 < levels.Count && levels[index + 1] > levels[index])
			{
				// the current page is a parent
				first = index;
			}
			else if (levels[index] > 1)
			{
				// the current page is a subpage; walk back to its parent
				first = index - 1;
				while (first > 0 && levels[first] >= levels[index])
				{
					first--;
				}

				if (levels[first] >= levels[index])
				{
					return false;
				}
			}
			else
			{
				return false;
			}

			last = first;
			while (last + 1 < levels.Count && levels[last + 1] > levels[first])
			{
				last++;
			}

			return true;
		}


		private async Task<List<string>> GetPageIDs(
			XElement hierarchy, XNamespace ns, XElement current)
		{
			IEnumerable<XElement> pages;

			switch (scope)
			{
				case RepairUrlsScope.PageGroup:
					pages = GetPageGroup(current, ns, out _);
					break;

				case RepairUrlsScope.Section:
					pages = current?.Parent.Elements(ns + "Page");
					break;

				case RepairUrlsScope.SectionGroup:
					pages = GetSectionGroup(current, ns)?.Descendants(ns + "Page");
					break;

				case RepairUrlsScope.Notebooks:
					var notebooks = await one.GetNotebooks(OneNote.Scope.Pages);
					pages = notebooks.Descendants(ns + "Page");
					break;

				default: // Notebook
					pages = hierarchy.Descendants(ns + "Page");
					break;
			}

			return (pages ?? Enumerable.Empty<XElement>())
				.Where(e => !e.AncestorsAndSelf().Any(a =>
					a.Attribute("isRecycleBin") is not null ||
					a.Attribute("isInRecycleBin") is not null))
				.Select(e => e.Attribute("ID")?.Value)
				.Where(id => !string.IsNullOrEmpty(id))
				.ToList();
		}
		#endregion Scope


		private static List<XElement> GetCandiateElements(Page page, bool useSelection)
		{
			List<XElement> elements;

			// OneNote XML will insert CR prior to 'href' in the CDATA
			var regex = new Regex(@"<a\s+href=", RegexOptions.Compiled);

			var range = useSelection ? new SelectionRange(page) : null;
			range?.GetSelection();

			if (range is null ||
				range.Scope == SelectionScope.None ||
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
			using var indent = logger.Indent();
			logger.StartClock();

			progressDialog = progress;

			// the map is the fallback for links whose page the identity catalog cannot place
			// yet; it covers the notebook of the current page unless all notebooks are checked
			mapScope = scope switch
			{
				RepairUrlsScope.Page => GetOneNoteScope(),
				RepairUrlsScope.Notebooks => OneNote.Scope.Notebooks,
				_ => OneNote.Scope.Sections
			};

			if (mapScope == OneNote.Scope.Self)
			{
				mapScope = OneNote.Scope.Sections;
			}

			try
			{
				// the catalog knows the link GUID of pages in every open notebook, so it can find
				// a page that moved to another section or notebook
				resolver = await Workspaces.WorkspaceResolver.ReadResolver(token);
				identity = new Identity.PageIdentityProvider();

				openNotebooks = await ReadOpenNotebooks();
			}
			catch (Exception exc)
			{
				logger.WriteLine("could not read the identity catalog", exc);
			}

			if (token.IsCancellationRequested)
			{
				return;
			}
			try
			{
				if (scope == RepairUrlsScope.Page)
				{
					sourceID = page.PageId;
					sourceTitle = page.Title;

					SetProgressMaximum(candidates.Count);
					progress.SetMessage(
						string.Format(Resx.RepairUrlsCommand_checkingMsg, candidates.Count));

					await ValidateUrls(progress, token);
					if (changeCount > 0 && !token.IsCancellationRequested)
					{
						await UpdatePage(page);
					}
				}
				else
				{
					await ValidatePages(progress, token);
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error validating URLs", exc);
				exception = exc;
			}

			identity?.Dispose();

			logger.WriteLine(
				$"links repaired:{repairedCount} marked:{badCount} unverified:{unverifiedCount} " +
				$"offline:{offlineCount} ambiguous:{ambiguousCount} paragraph:{paragraphCount}");

			progress.Close();

			logger.WriteTime("check complete");
		}


		private OneNote.Scope GetOneNoteScope()
		{
			/*
			 * Notebook reference will start with "onenote:https://...."
             * onenote:https://d.docs.live.net/6925.../&amp;section-id={...}&amp;page-id={...}&amp;end
			 * 
			 * Any pages within this notebook will have a base-path=https...
             * onenote:...&amp;section-id={...}&amp;page-id={...}&amp;end&amp;base-path=https://d...
             * 
             * Possible future optimization: collect all named notebooks/sections since the
             * notebook URI contains the exact names, here "OneMore Wiki" and "Get Started"
             * https://d.docs.live.net/.../Documents/OneMore%20Wiki/Get%20Started.one
			 */

			var scope = OneNote.Scope.Self;

			foreach (var candidate in candidates)
			{
				var data = candidates.DescendantNodes().OfType<XCData>();
				if (data.Any(d => d.Value.Contains("<a\nhref=\"onenote:http")))
				{
					return OneNote.Scope.Notebooks;
				}

				if (data.Any(d => d.Value.Contains("<a\nhref=\"onenote:")))
				{
					if (scope == OneNote.Scope.Self)
					{
						scope = OneNote.Scope.Sections;
					}
				}
			}

			return scope;
		}


		/// <summary>
		/// Builds the map of link GUIDs to pages the first time it is needed. It costs a call
		/// to OneNote for every page in its scope, so it is built only if the identity catalog
		/// could not place a link.
		/// </summary>
		private async Task EnsureMap(CancellationToken token)
		{
			if (mapBuilt)
			{
				return;
			}

			mapBuilt = true;

			// the progress bar is borrowed for the map and given back afterward
			var progress = progressDialog;

			map = await new HyperlinkProvider(one).BuildHyperlinkMap(mapScope, token,
				async (count) =>
				{
					progress.SetMaximum(count);
					progress.SetMessage(string.Format(Resx.RepairUrlsCommand_mappingMsg, count));
					await Task.Yield();
				},
				async () =>
				{
					progress.Increment();
					await Task.Yield();
				});

			mapByGuid = new Dictionary<string, OneNote.HyperlinkInfo>();
			foreach (var info in map.Values)
			{
				var guid = Identity.LinkGuids.PageGuid(info.Uri);
				if (guid is not null && !mapByGuid.ContainsKey(guid))
				{
					mapByGuid.Add(guid, info);
				}
			}

			// restore the progress bar
			var value = progressValue;
			SetProgressMaximum(progressMax);
			for (var i = 0; i < value; i++)
			{
				Step();
			}
		}


		private void SetProgressMaximum(int max)
		{
			progressMax = max;
			progressValue = 0;
			progressDialog.SetMaximum(max);
		}


		private void Step()
		{
			progressValue++;
			progressDialog.Increment();
		}

		private async Task ValidatePages(ProgressDialog progress, CancellationToken token)
		{
			SetProgressMaximum(pageIDs.Count);
			progress.SetMessage(string.Format(Resx.RepairUrlsCommand_pagesMsg, pageIDs.Count));

			foreach (var pageID in pageIDs)
			{
				if (token.IsCancellationRequested)
				{
					return;
				}

				Step();

				// a cheap look at the page before paying to load all of it
				if (pageID != page.PageId && !one.GetPageXml(pageID).Contains("href="))
				{
					continue;
				}

				var target = pageID == page.PageId
					? page
					: await one.GetPage(pageID, OneNote.PageDetail.All);

				try
				{
					sourceID = target.PageId;
					sourceTitle = target.Title;

					candidates = GetCandiateElements(target, false);
					if (candidates.Count == 0)
					{
						continue;
					}

					var before = changeCount;
					await ValidateUrls(progress, token);

					if (changeCount > before && !token.IsCancellationRequested)
					{
						await UpdatePage(target);
					}
				}
				catch (Exception exc)
				{
					logger.WriteLine($"error checking page [{target.Title}]", exc);
				}
			}
		}


		private async Task ValidateUrls(ProgressDialog progress, CancellationToken token)
		{
			// each text is parsed once and written back once, after both kinds of links are
			// checked, because writing it back replaces its CDATA node
			var items = candidates
				.Select(e => e.GetCData())
				.Where(c => c is not null)
				.Select(c => new LinkText { Cdata = c, Wrapper = c.GetWrapper() })
				.ToList();

			// links to pages are checked one at a time because they call into OneNote
			foreach (var item in items)
			{
				await ValidateUrl(item, true, token);
			}

			// parallelize internet access for all chosen hyperlinks on the page...

			// must use a thread-safe collection here
			var tasks = new ConcurrentBag<Task>();

			foreach (var item in items)
			{
				if (onenoteOnly)
				{
					// web links are neither resolved, repaired nor reported
					break;
				}

				// do not use await in the body loop; just build list of tasks
				tasks.Add(ValidateUrl(item, false, token));
			}

			await Task.WhenAll(tasks.ToArray());

			if (token.IsCancellationRequested)
			{
				return;
			}

			foreach (var item in items.Where(i => i.Changed))
			{
				Interlocked.Increment(ref changeCount);
				item.Cdata.ReplaceWith(item.Wrapper.GetInnerXml());
			}
		}


		private sealed class LinkText
		{
			public XCData Cdata;
			public XElement Wrapper;
			public bool Changed;
		}


		/// <summary>
		/// Checks the links of one text element: either only the links to OneNote pages
		/// or only the web links.
		/// </summary>
		private async Task ValidateUrl(LinkText item, bool onenote, CancellationToken token)
		{
			foreach (var anchor in item.Wrapper.Elements("a"))
			{
				if (token.IsCancellationRequested)
				{
					return;
				}

				var href = anchor.Attribute("href")?.Value;
				if (!ValidAddress(href) || IsOneNoteLink(href) != onenote)
				{
					continue;
				}

				if (perLinkProgress)
				{
					Step();
				}

				if (onenote)
				{
					var (status, fresh, target) = await ClassifyOneNoteLink(href, token);

					logger.WriteLine(
						$"link [{status}] text=\"{anchor.Value}\" href={href}" +
						(fresh is null ? string.Empty : $" fresh={fresh}"));

					switch (status)
					{
						case LinkStatus.Stale:
							if (href.Contains("object-id="))
							{
								// a link made from the page alone would lose the paragraph, and
								// OneNote can still follow this one, so leave it as it is
								Interlocked.Increment(ref paragraphCount);
								AddResult(RepairUrlsOutcome.Unchanged, anchor,
									string.Format(Resx.RepairUrlsCommand_detailParagraph, target));
							}
							else if (repairLinks)
							{
								anchor.SetAttributeValue("href", fresh);
								Interlocked.Increment(ref repairedCount);
								item.Changed = true;
								AddResult(RepairUrlsOutcome.Repaired, anchor,
									string.Format(Resx.RepairUrlsCommand_detailRepaired, target));
							}
							else if (markLinks)
							{
								Mark(anchor);
								item.Changed = true;
								AddResult(RepairUrlsOutcome.Highlighted, anchor,
									string.Format(Resx.RepairUrlsCommand_detailStale, target));
							}
							else
							{
								AddResult(RepairUrlsOutcome.Unchanged, anchor,
									string.Format(Resx.RepairUrlsCommand_detailStale, target));
							}
							break;

						case LinkStatus.Invalid:
							if (markLinks)
							{
								Mark(anchor);
								item.Changed = true;
								AddResult(RepairUrlsOutcome.Highlighted, anchor,
									Resx.RepairUrlsCommand_detailNotFound);
							}
							else
							{
								AddResult(RepairUrlsOutcome.Unchanged, anchor,
									Resx.RepairUrlsCommand_detailNotFound);
							}
							break;

						case LinkStatus.Unverified:
							Interlocked.Increment(ref unverifiedCount);
							AddResult(RepairUrlsOutcome.Unchanged, anchor,
								Resx.RepairUrlsCommand_detailUnverified);
							break;

						case LinkStatus.Offline:
							Interlocked.Increment(ref offlineCount);
							AddResult(RepairUrlsOutcome.Unchanged, anchor,
								Resx.RepairUrlsCommand_detailOffline);
							break;

						case LinkStatus.Ambiguous:
							Interlocked.Increment(ref ambiguousCount);
							AddResult(RepairUrlsOutcome.Unchanged, anchor,
								Resx.RepairUrlsCommand_detailAmbiguous);
							break;
					}
				}
				else if (await InvalidWebUrl(href))
				{
					if (markLinks)
					{
						Mark(anchor);
						item.Changed = true;
						AddResult(RepairUrlsOutcome.Highlighted, anchor, Resx.RepairUrlsCommand_detailWeb);
					}
					else
					{
						AddResult(RepairUrlsOutcome.Unchanged, anchor, Resx.RepairUrlsCommand_detailWeb);
					}
				}
			}
		}


		private void AddResult(RepairUrlsOutcome outcome, XElement anchor, string detail)
		{
			lock (results)
			{
				results.Add(new RepairUrlsResult
				{
					Outcome = outcome,
					LinkText = anchor.Value,
					PageID = sourceID,
					PageTitle = sourceTitle,
					Detail = detail
				});
			}
		}



		/// <summary>
		/// Highlights the text of a link that cannot be repaired.
		/// </summary>
		private void Mark(XElement anchor)
		{
			// as usual, make direct updates and let OneNote normalize

			// update the background style of all spans in value of anchor
			foreach (var span in anchor.Nodes().OfType<XElement>())
			{
				var style = new Style(span.Attribute("style")?.Value ?? string.Empty, false)
				{
					ApplyColors = true,
					Highlight = markColor
				};

				span.SetAttributeValue("style", style.ToCss());
			}

			// and then wrap the whole value in a span, even if there are sub-spans
			var texts = anchor.Nodes().OfType<XText>();
			if (texts.Any())
			{
				anchor.ReplaceNodes(
					new XElement("span",
						new XAttribute("style", $"background:{markColor}"),
					anchor.Nodes()
					)
				);
			}

			Interlocked.Increment(ref badCount);
		}


		private enum LinkStatus
		{
			/// <summary>The link names a page where it is now</summary>
			Valid,

			/// <summary>The link names a page that can be found, but the link is out of date</summary>
			Stale,

			/// <summary>The page cannot be found anywhere</summary>
			Invalid,

			/// <summary>The page cannot be found yet, but it may be</summary>
			Unverified,

			/// <summary>The page is probably in a notebook that is not open</summary>
			Offline,

			/// <summary>More than one page has the same link GUID, probably copies</summary>
			Ambiguous
		}


		/// <summary>
		/// Finds the page that a link names and says whether the link is still good. The GUIDs in
		/// a link are not the IDs of the hierarchy, so they are compared only with the GUIDs of
		/// links that OneNote makes for the pages it has now.
		/// </summary>
		/// <returns>
		/// The status of the link and, if the page was found, a new link to it and where it is
		/// </returns>
		private async Task<(LinkStatus Status, string Fresh, string Target)> ClassifyOneNoteLink(
			string href, CancellationToken token)
		{
			var guid = Identity.LinkGuids.PageGuid(href);
			if (guid is null)
			{
				// not a link to a page
				return (LinkStatus.Valid, null, null);
			}

			string pageID = null;
			string fresh = null;
			string target = null;
			var pending = false;

			if (resolver is not null)
			{
				var resolution = resolver.Resolve(
					new Workspaces.TargetQuery { PageID = "-", Uri = href });

				switch (resolution.Outcome)
				{
					case Workspaces.ResolveOutcome.Resolved:
						pageID = resolution.PageID;
						target = resolution.Location;
						break;

					case Workspaces.ResolveOutcome.Ambiguous:
						return (LinkStatus.Ambiguous, null, null);

					case Workspaces.ResolveOutcome.Offline:
						return (LinkStatus.Offline, null, null);

					case Workspaces.ResolveOutcome.Pending:
						pending = true;
						break;
				}
			}

			if (pageID is null)
			{
				// a page that was known before and is in no open notebook is not found because
				// its notebook is closed, or it was deleted a short while ago
				var known = identity?.ReadByGuid(guid);
				if (known is not null && known.Count > 0 && known.All(r => r.IsMissing))
				{
					return (LinkStatus.Offline, null, null);
				}

				// a link to a notebook that is not open cannot be judged, so it is not called broken
				if (IsOutsideOpenNotebooks(href))
				{
					return (LinkStatus.Offline, null, null);
				}

				await EnsureMap(token);
				if (mapByGuid is not null && mapByGuid.TryGetValue(guid, out var info))
				{
					pageID = info.PageID;
					fresh = info.Uri;
					target = $"{info.FullPath}/{info.Name}";
				}
				else
				{
					return (pending ? LinkStatus.Unverified : LinkStatus.Invalid, null, null);
				}
			}

			fresh ??= one.GetHyperlink(pageID, string.Empty);
			if (string.IsNullOrEmpty(fresh))
			{
				return (LinkStatus.Unverified, null, null);
			}

			return (IsStale(href, fresh) ? LinkStatus.Stale : LinkStatus.Valid, fresh, target);
		}


		private async Task<HashSet<string>> ReadOpenNotebooks()
		{
			var names = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

			var books = await one.GetNotebooks(OneNote.Scope.Notebooks);
			if (books is null)
			{
				return names;
			}

			foreach (var book in books.Elements())
			{
				// a notebook is identified by its folder and the folder that holds it, such as
				// Documents/Flux or Local Notebooks/Local; the folder alone is too common a name,
				// and a path with only one part cannot be told from the folders around it
				var path = book.Attribute("path")?.Value;
				if (!string.IsNullOrEmpty(path))
				{
					var parts = Uri.UnescapeDataString(path).Replace('\\', '/')
						.Split('/').Where(s => s.Length > 0).ToList();

					if (parts.Count >= 2)
					{
						names.Add($"{parts[parts.Count - 2]}/{parts[parts.Count - 1]}");
					}
				}
			}

			return names;
		}


		/// <summary>
		/// Determines whether the notebook that a link names is not open: none of the folders
		/// of the link's path is the name of an open notebook. The path of a link is written in
		/// several forms, such as a OneDrive address or a UNC path, so only names are compared.
		/// Returns false if that cannot be told, so that a doubt never hides a broken link.
		/// </summary>
		private bool IsOutsideOpenNotebooks(string href)
		{
			if (openNotebooks is null || openNotebooks.Count == 0)
			{
				return false;
			}

			var location = GetLinkLocation(href);
			if (string.IsNullOrEmpty(location))
			{
				return false;
			}

			// the last part is the .one file of the section
			var folders = location.Split('/').Where(s => s.Length > 0).ToList();
			if (folders.Count > 0)
			{
				folders.RemoveAt(folders.Count - 1);
			}

			for (var i = 0; i + 1 < folders.Count; i++)
			{
				if (openNotebooks.Contains($"{folders[i]}/{folders[i + 1]}"))
				{
					return false;
				}
			}

			return true;
		}


		/// <summary>
		/// Saves a page. If OneNote refuses, the links on the page were not changed after all,
		/// so they are reported that way.
		/// </summary>
		private async Task UpdatePage(Page target)
		{
			if (await one.Update(target))
			{
				return;
			}

			logger.WriteLine(
				$"could not update page [{sourceTitle}] {target.PageId} with " +
				$"{results.Count(r => r.PageID == target.PageId)} changed links");

			lock (results)
			{
				foreach (var result in results.Where(r =>
					r.PageID == target.PageId && r.Outcome != RepairUrlsOutcome.Unchanged))
				{
					if (result.Outcome == RepairUrlsOutcome.Repaired)
					{
						Interlocked.Decrement(ref repairedCount);
					}
					else
					{
						Interlocked.Decrement(ref badCount);
					}

					result.Outcome = RepairUrlsOutcome.Unchanged;
					result.Detail = Resx.RepairUrlsCommand_detailUpdateFailed;
				}
			}
		}


		/// <summary>
		/// Determines whether a link differs from the one OneNote makes for the same page in
		/// the section or the location of the notebook it names.
		/// </summary>
		internal static bool IsStale(string href, string fresh)
		{
			if (!string.Equals(
				Identity.LinkGuids.SectionGuid(href),
				Identity.LinkGuids.SectionGuid(fresh),
				StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			var linkPath = GetLinkLocation(href);
			var freshPath = GetLinkLocation(fresh);

			return linkPath is not null && freshPath is not null &&
				!string.Equals(linkPath, freshPath, StringComparison.OrdinalIgnoreCase);
		}


		/// <summary>
		/// Gets the .one location from a link, either the part before '#' or the base-path
		/// value of an intra-notebook link, decoded and with separators normalized.
		/// </summary>
		private static string GetLinkLocation(string uri)
		{
			var body = uri.Substring("onenote:".Length);
			var hash = body.IndexOf('#');
			var location = hash > 0 ? body.Substring(0, hash) : null;

			if (string.IsNullOrEmpty(location))
			{
				var match = Regex.Match(body, @"[&]base-path=(?<p>.+)$");
				location = match.Success ? match.Groups["p"].Value : null;
			}

			return location is null
				? null
				: Uri.UnescapeDataString(location).Replace('\\', '/').TrimStart('/');
		}


		private static bool IsOneNoteLink(string href)
		{
			return href.StartsWith("onenote:") && href.Contains("page-id=");
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


		// Called when the progress dialog closes. The report is not shown from here: this runs
		// while the dialog's message loop is being torn down, and a modal message box shown now
		// is ended at once by the loop's quit message (it returns Cancel in a few milliseconds),
		// which also leaves the foreground on whatever window is next in the z-order, not OneNote.
		private void ReportResult(object sender, EventArgs e)
		{
			progressClosed.TrySetResult(true);
		}


		private void ShowReport()
		{
			if (exception is not null)
			{
				MoreMessageBox.ShowErrorWithLogLink(owner, exception.Message);
				return;
			}

			if (results.Count == 0)
			{
				MoreMessageBox.Show(owner, Resx.RepairUrlsCommand_noneMsg);
				return;
			}

			// repaired and highlighted links first, in the order they were found
			var ordered = results.OrderBy(r => r.Outcome).ToList();

			// many links share a page, and each path is several calls into OneNote
			var paths = new Dictionary<string, string>();
			foreach (var result in ordered)
			{
				if (string.IsNullOrEmpty(result.PageID))
				{
					continue;
				}

				if (!paths.TryGetValue(result.PageID, out var path))
				{
					try
					{
						path = one.GetPageHierarchyInfo(result.PageID).Path?.TrimStart('/');
					}
					catch (Exception exc)
					{
						logger.WriteLine($"error reading path of page [{result.PageTitle}]", exc);
						path = string.Empty;
					}

					paths.Add(result.PageID, path);
				}

				result.PagePath = path;
			}

			// modeless, not ShowDialog(owner): the list stays open while the user goes to pages,
			// and navigating hangs OneNote if its window is disabled by a modal owner. The form
			// disposes itself when closed, so it must not be disposed here, because RunModeless
			// returns at once if it joins an existing message loop.
			var dialog = new RepairUrlsResultsDialog(ordered, sectionID);
			dialog.RunModeless();
		}	}
}
