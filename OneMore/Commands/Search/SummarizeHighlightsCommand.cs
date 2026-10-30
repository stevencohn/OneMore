//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Globalization;
	using System.Linq;
	using System.Net;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Generates a report page of all paragraphs containing highlighted text within a scope:
	/// this page, section, section group, notebook, or all notebooks.
	/// </summary>
	[CommandService]
	internal class SummarizeHighlightsCommand : Command
	{
		public enum HighlightScope
		{
			Page,
			Section,
			SectionGroup,
			Notebook,
			Notebooks
		}


		private sealed class ParagraphHit
		{
			public string Html;
			public string Link;
			public DateTime Modified;
		}


		private sealed class PageHit
		{
			public string Notebook;
			public string SectionId;
			public string SectionPath;
			public string PageId;
			public string Title;
			public string Link;
			public List<ParagraphHit> Paragraphs = new();
		}


		// matches a span whose style sets a background color
		private static readonly Regex HighlightPattern = new(
			@"<span[^>]*?background:\s*([^;'""]+)",
			RegexOptions.Singleline | RegexOptions.IgnoreCase);

		private const string CiteCss = "font-size:8.0pt;color:#595959";

		private OneNote one;
		private HighlightScope scope;
		private string scopeName;
		private string currentPageId;
		private int highlightCount;
		private readonly List<PageHit> hits = new();


		public SummarizeHighlightsCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			string pageId;

			await using (one = new OneNote())
			{
				currentPageId = one.CurrentPageId;
				var sectionId = one.CurrentSectionId;

				// ask about an existing report first, before the scope dialog and while
				// OneNote is still the foreground window; after a long scan and closing the
				// progress dialog, a modal dialog can open behind OneNote and appear missing
				pageId = await FindReport();
				if (pageId == string.Empty)
				{
					return;
				}

				// a report page can't be summarized so don't offer "this page" on one
				var startPage = await one.GetPage(currentPageId, OneNote.PageDetail.Basic);
				var onReport = startPage?.GetMetaContent(MetaNames.HighlightsReport) is not null;

				using (var dialog = new SummarizeHighlightsDialog(!onReport))
				{
					if (dialog.ShowDialog(owner) != DialogResult.OK)
					{
						return;
					}

					scope = dialog.Scope;
				}

				var pageElements = await GetScopedPages();
				if (pageElements is null)
				{
					return;
				}

				// scan on the dialog's own thread with its own OneNote instance so the UI
				// stays responsive and the Cancel button works
				var cancelled = false;
				var scanned = new TaskCompletionSource<bool>();
				var progress = new UI.ProgressDialog(async (dialog, token) =>
				{
					dialog.SetMaximum(Math.Max(pageElements.Count, 1));

					await using var scanner = new OneNote();
					foreach (var pageElement in pageElements)
					{
						if (token.IsCancellationRequested)
						{
							cancelled = true;
							break;
						}

						dialog.SetMessage(pageElement.Attribute("name")?.Value ?? string.Empty);
						dialog.Increment();
						await ScanPage(scanner, pageElement);
					}

					dialog.Close();
				});

				progress.RunModeless((sender, e) => scanned.TrySetResult(true));
				await scanned.Task;

				if (cancelled || progress.DialogResult == DialogResult.Cancel)
				{
					return;
				}

				if (hits.Count == 0)
				{
					ShowInfo(Resx.SummarizeHighlights_None);
					return;
				}

				Page page;
				var isNew = pageId is null;
				if (isNew)
				{
					one.CreatePage(sectionId, out pageId);
					page = await one.GetPage(pageId);
				}
				else
				{
					page = await one.GetPage(pageId);

					// OneNote does not remove omitted content on update so delete the old
					// outlines explicitly, then reload the now-empty page
					var outlines = page.BodyOutlines.ToList();
					logger.WriteLine($"replacing report {pageId}, deleting {outlines.Count} outlines");

					foreach (var outline in outlines)
					{
						var objectId = outline.Attribute("objectID")?.Value;
						if (!string.IsNullOrEmpty(objectId))
						{
							one.DeleteContent(pageId, objectId);
						}
					}

					page = await one.GetPage(pageId);

					// like the Reminder report, also drop any outline content that remains
					var remaining = page.BodyOutlines.ToList();
					logger.WriteLine($"report has {remaining.Count} outlines after delete");
					if (remaining.Count > 0)
					{
						remaining[0].Elements().Remove();
						remaining.Skip(1).Remove();
					}
				}

				var title = string.Format(Resx.SummarizeHighlights_Title, scopeName);
				if (isNew)
				{
					title = await MakeUniqueTitle(sectionId, pageId, title);
				}

				page.Title = title;
				page.SetMeta(MetaNames.HighlightsReport, scope.ToString());

				BuildReport(page);

				await one.Update(page);

				// the progress dialog was closed after scanning so it is safe to navigate
				await one.NavigateTo(pageId, string.Empty);
			}
		}


		/// <summary>
		/// Finds the Page elements within the chosen scope, sets scopeName, and excludes
		/// recycle bins. Returns null if the scope cannot be resolved.
		/// </summary>
		private async Task<List<XElement>> GetScopedPages()
		{
			XElement root;
			Func<XElement, bool> within;

			switch (scope)
			{
				case HighlightScope.Notebooks:
					root = await one.GetNotebooks(OneNote.Scope.Pages);
					within = e => true;
					scopeName = Resx.SummarizeHighlights_AllNotebooks;
					break;

				case HighlightScope.Notebook:
					root = await one.GetNotebook(OneNote.Scope.Pages);
					within = e => true;
					break;

				case HighlightScope.SectionGroup:
					root = await one.GetNotebook(OneNote.Scope.Pages);
					var groupId = one.CurrentSectionGroupId;
					within = e => e.Ancestors().Any(a =>
						a.Name.LocalName == "SectionGroup" && a.Attribute("ID")?.Value == groupId);
					break;

				case HighlightScope.Section:
					root = await one.GetNotebook(OneNote.Scope.Pages);
					var sectionId = one.CurrentSectionId;
					within = e => e.Parent?.Attribute("ID")?.Value == sectionId;
					scopeName = Resx.SummarizeHighlights_ThisSection;
					break;

				default:
					root = await one.GetNotebook(OneNote.Scope.Pages);
					within = e => e.Attribute("ID")?.Value == currentPageId;
					scopeName = Resx.SummarizeHighlights_ThisPage;
					break;
			}

			if (root is null)
			{
				ShowError(Resx.SummarizeHighlights_None);
				return null;
			}

			// remove recycle bin nodes
			root.Descendants()
				.Where(n => n.Name.LocalName == "UnfiledNotes" ||
							n.Attribute("isRecycleBin") != null ||
							n.Attribute("isInRecycleBin") != null)
				.ToList()
				.ForEach(n => n.Remove());

			var ns = root.GetNamespaceOfPrefix(OneNote.Prefix);

			if (scopeName is null)
			{
				if (scope == HighlightScope.Notebook)
				{
					scopeName = root.Attribute("name")?.Value;
				}
				else if (scope == HighlightScope.SectionGroup)
				{
					var id = one.CurrentSectionGroupId;
					scopeName = root.DescendantsAndSelf(ns + "SectionGroup")
						.FirstOrDefault(g => g.Attribute("ID")?.Value == id)?
						.Attribute("name")?.Value;
				}

				scopeName ??= root.Attribute("name")?.Value ?? string.Empty;
			}

			// never include highlight reports, whether the one being replaced or older ones
			return root.Descendants(ns + "Page")
				.Where(within)
				.Where(p => !p.Elements(ns + "Meta").Any(
					m => m.Attribute("name")?.Value == MetaNames.HighlightsReport))
				.ToList();
		}


		private async Task ScanPage(OneNote scanner, XElement pageElement)
		{
			var pageId = pageElement.Attribute("ID").Value;

			var page = await scanner.GetPage(pageId, OneNote.PageDetail.Basic);
			if (page is null || page.GetMetaContent(MetaNames.HighlightsReport) is not null)
			{
				// skip prior reports, including the one we're about to replace
				return;
			}

			var ns = page.Namespace;

			// the page's own background, accounting for automatic and dark Office themes
			var pageColor = page.GetPageColor(out _, out _);

			var pageModified = ParseTime(pageElement.Attribute("lastModifiedTime")?.Value);

			PageHit hit = null;

			foreach (var oe in page.Root.Descendants(ns + "OE"))
			{
				var runs = oe.Elements(ns + "T").ToList();
				if (runs.Count == 0)
				{
					continue;
				}

				var html = string.Concat(runs.Select(r => r.GetCData()?.Value ?? string.Empty));
				var count = HighlightPattern.Matches(html).Cast<Match>()
					.Count(m => IsHighlight(m.Groups[1].Value, pageColor));

				if (count == 0)
				{
					continue;
				}

				hit ??= CreatePageHit(scanner, pageElement, pageId, page.Title);

				var objectId = oe.Attribute("objectID")?.Value;
				var modified = ParseTime(oe.Attribute("lastModifiedTime")?.Value);

				hit.Paragraphs.Add(new ParagraphHit
				{
					Html = html,
					Link = scanner.GetHyperlink(pageId, objectId ?? string.Empty),
					Modified = modified == DateTime.MinValue ? pageModified : modified
				});

				highlightCount += count;
			}

			if (hit is not null)
			{
				hits.Add(hit);
			}
		}


		private static PageHit CreatePageHit(
			OneNote scanner, XElement pageElement, string pageId, string title)
		{
			var section = pageElement.Parent;
			var names = section.AncestorsAndSelf()
				.Where(e => e.Name.LocalName == "Section" || e.Name.LocalName == "SectionGroup")
				.Reverse()
				.Select(e => e.Attribute("name")?.Value)
				.ToList();

			var notebook = section.Ancestors()
				.FirstOrDefault(e => e.Name.LocalName == "Notebook")?
				.Attribute("name")?.Value;

			return new PageHit
			{
				Notebook = notebook,
				SectionId = section.Attribute("ID")?.Value,
				SectionPath = string.Join(" › ", names),
				PageId = pageId,
				Title = string.IsNullOrWhiteSpace(title) ? "(untitled)" : title,
				Link = scanner.GetHyperlink(pageId, string.Empty)
			};
		}


		/// <summary>
		/// Determines whether the CSS background value is a visible highlight, ignoring
		/// colors that have no contrast against the page, such as white on a white page
		/// or black on a black page.
		/// </summary>
		private static bool IsHighlight(string value, Color pageColor)
		{
			value = value.Trim();

			if (value.Length == 0 ||
				value.Equals("automatic", StringComparison.OrdinalIgnoreCase) ||
				value.Equals("transparent", StringComparison.OrdinalIgnoreCase) ||
				value.Equals("none", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			Color color;
			try
			{
				color = ColorHelper.FromHtml(value);
			}
			catch (Exception)
			{
				// can't interpret the color so assume it is visible
				return true;
			}

			if (color.IsEmpty || (!color.IsKnownColor && color.A == 0))
			{
				// unrecognized color name, assume it is visible
				return true;
			}

			return !HasNoContrast(color, pageColor);
		}


		private static bool HasNoContrast(Color color, Color pageColor)
		{
			return color.Matches(pageColor) ||
				Math.Abs(color.GetBrightness() - pageColor.GetBrightness()) < 0.1;
		}


		private static DateTime ParseTime(string value)
		{
			// RoundtripKind preserves the UTC "Z" suffix so ToLocalTime works
			return string.IsNullOrEmpty(value)
				? DateTime.MinValue
				: DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
		}


		private static string FormatTime(DateTime time)
		{
			if (time.Kind == DateTimeKind.Utc)
			{
				time = time.ToLocalTime();
			}

			return time.ToString("g", AddIn.Locale);
		}


		/// <summary>
		/// Appends a " (n)" counter to the title if another page in the section already
		/// has that title.
		/// </summary>
		private async Task<string> MakeUniqueTitle(string sectionId, string newPageId, string title)
		{
			var section = await one.GetSection(sectionId);
			var ns = section.GetNamespaceOfPrefix(OneNote.Prefix);

			// ignore the page we just created
			var names = new HashSet<string>(
				section.Elements(ns + "Page")
					.Where(e => e.Attribute("ID")?.Value != newPageId)
					.Select(e => e.Attribute("name")?.Value ?? string.Empty),
				StringComparer.CurrentCultureIgnoreCase);

			var candidate = title;
			for (var n = 1; names.Contains(candidate); n++)
			{
				candidate = $"{title} ({n})";
			}

			return candidate;
		}


		private async Task<string> FindReport()
		{
			var hierarchy = await one.SearchMeta(one.CurrentSectionId, MetaNames.HighlightsReport);
			if (hierarchy is null)
			{
				logger.WriteLine("SearchMeta returned no hierarchy looking for a prior highlights report");
				return null;
			}

			var hns = hierarchy.GetNamespaceOfPrefix(OneNote.Prefix);

			var pageId = hierarchy.Descendants(hns + "Meta")
				.Where(e => e.Attribute("name").Value == MetaNames.HighlightsReport)
				.Select(e => e.Parent.Attribute("ID").Value)
				.FirstOrDefault();

			if (pageId is null)
			{
				return null;
			}

			// don't navigate to the old report here; the user stays where they started
			// until the new report is generated

			DialogResult answer;
			using (var dialog = new ReportRemindersReuseDialog())
			{
				if (dialog.ShowDialog(owner) == DialogResult.Cancel)
				{
					return string.Empty;
				}

				answer = dialog.Option;
			}

			// OK = overwrite, Yes = create new
			return answer == DialogResult.OK ? pageId : null;
		}


		private void BuildReport(Page page)
		{
			var ns = page.Namespace;
			PageNamespace.Set(ns);

			var h1 = page.GetQuickStyle(Styles.StandardStyles.Heading1).Index;
			var h2 = page.GetQuickStyle(Styles.StandardStyles.Heading2).Index;
			var container = page.EnsureContentContainer();

			var paragraphCount = hits.Sum(h => h.Paragraphs.Count);

			container.Add(
				new Paragraph(string.Format(Resx.SummarizeHighlights_Summary,
					$"<span style='font-weight:bold'>{Encode(scopeName)}</span>",
					highlightCount, paragraphCount, hits.Count)),
				new Paragraph($"<span style='{CiteCss}'>{Encode(Resx.SummarizeHighlights_Explain)}</span>"),
				new Paragraph(string.Empty));

			string notebook = null;
			string sectionId = null;

			foreach (var hit in hits)
			{
				if (hit.Notebook != notebook)
				{
					notebook = hit.Notebook;
					sectionId = null;
					container.Add(
						new Paragraph(Encode(notebook)).SetQuickStyle(h1),
						new Paragraph(string.Empty));
				}

				if (hit.SectionId != sectionId)
				{
					sectionId = hit.SectionId;
					container.Add(
						new Paragraph(Encode(hit.SectionPath)).SetQuickStyle(h2),
						new Paragraph(string.Empty));
				}

				container.Add(new Paragraph(
					$"<span style='font-weight:bold'><a href=\"{hit.Link}\">{Encode(hit.Title)}</a></span>"));

				var bullets = new ContentList(ns);
				foreach (var paragraph in hit.Paragraphs)
				{
					var cite = string.Format(Resx.SummarizeHighlights_Modified, FormatTime(paragraph.Modified));
					bullets.Add(new Bullet(ns,
						$"{paragraph.Html}<br>\n<span style='{CiteCss}'>{Encode(cite)} · " +
						$"<a href=\"{paragraph.Link}\">{Encode(Resx.SummarizeHighlights_GoTo)}</a></span>"));
				}

				container.Add(new Paragraph(bullets), new Paragraph(string.Empty));
			}
		}


		private static string Encode(string text)
		{
			return WebUtility.HtmlEncode(text ?? string.Empty);
		}
	}
}
