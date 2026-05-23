//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S125 // ignore commented out code

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Globalization;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal partial class SearchDialogTextControl : MoreUserControl
	{

		private sealed class SearchHit
		{
			public string PlainText { get; set; }
			public string ObjectId { get; set; }
		}


		//private const int CreatedAfter = 1;
		private const int CreatedBefore = 2;
		private const int UpdatedAfter = 3;
		private const int UpdatedBefore = 4;

		private readonly ILogger logger;
		private readonly Regex cleaner;
		private CancellationTokenSource source;
		private bool grouping;


		public SearchDialogTextControl()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introLabel",
					"findLabel=word_Find",
					"matchBox",
					"regBox",
					"moveButton=word_Move",
					"copyButton=word_Copy",
					"cancelButton=word_Cancel",
					"selectAllLink",
					"clearAllLink"
				});

				scopeBox.Items.Clear();
				scopeBox.Items.AddRange(Resx.SearchDialogText_scopeOptions.Split('\n'));

				dateSelector.Items.Clear();
				dateSelector.Items.AddRange(Resx.SearchDialogTextControl_dateOptions.Split('\n'));
			}

			scopeBox.SelectedIndex = 2;
			dateSelector.SelectedIndex = 0;
			dateTimePicker.MaxDate = DateTime.Today.AddDays(1);
			pageLabel.Text = string.Empty;

			logger = Logger.Current;

			// pattern to remove SPAN|A elements and &#nn; escaped characters
			//
			// NOTE, instead of ignoring escape sequences, use WebUtility.HtmlDecode(input);
			//
			cleaner = new Regex(
				@"(?:<\s*(?:span|a)[^>]*?>)|(?:</(?:span|a)>)|(?:&#\d+;)",
				RegexOptions.Compiled);

			resultsView.CardActivated += OnCardActivated;
			resultsView.HitActivated += OnHitActivated;
			resultsView.CheckedChanged += OnCheckedChanged;
		}


		public event EventHandler<SearchCloseEventArgs> SearchClosing;


		public bool CopySelections { get; private set; }


		public List<string> SelectedPages { get; private set; }


		private void OnCheckedChanged(object sender, EventArgs e)
		{
			var hasChecked = resultsView.CheckedCount > 0;
			moveButton.Enabled = hasChecked;
			copyButton.Enabled = hasChecked;
		}


		private void MovePressed(object sender, EventArgs e)
		{
			CopySelections = false;
			SelectedPages = resultsView.GetCheckedPageIds().ToList();
			SearchClosing?.Invoke(this, new(DialogResult.OK));
		}


		private void CopyPressed(object sender, EventArgs e)
		{
			CopySelections = true;
			SelectedPages = resultsView.GetCheckedPageIds().ToList();
			SearchClosing?.Invoke(this, new(DialogResult.OK));
		}


		private void SelectAll(object sender, LinkLabelLinkClickedEventArgs e)
		{
			resultsView.CheckAll();
		}


		private void ClearSelection(object sender, LinkLabelLinkClickedEventArgs e)
		{
			resultsView.ClearChecked();
		}


		private void Nevermind(object sender, EventArgs e)
		{
			// snapshot the token source — Search() nulls the field after completion,
			// so a race here would otherwise NRE
			var cts = source;
			if (cts is not null)
			{
				logger.WriteLine("cancelling search");
				try { cts.Cancel(); }
				catch (ObjectDisposedException) { /* search already completed */ }
				return;
			}

			logger.WriteLine("closing search");
			SearchClosing?.Invoke(this, new(DialogResult.Cancel));
		}


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			findBox.Focus();
		}


		private void ChangedText(object sender, EventArgs e)
		{
			var text = findBox.Text.Trim();

			if (text.Length == 0)
			{
				searchButton.Enabled = false;
				searchButton.NotifyDefault(false);
				return;
			}

			if (regBox.Checked)
			{
				try
				{
					// validate regular expression
					_ = new Regex(text);
				}
				catch
				{
					// swallow bad regex
					searchButton.Enabled = false;
					searchButton.NotifyDefault(false);
					return;
				}

			}

			searchButton.Enabled = true;
			searchButton.NotifyDefault(true);
		}


		private void SearchOnKeydown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter &&
				findBox.Text.Trim().Length > 0)
			{
				Search(sender, e);
				e.Handled = true;
			}
		}


		private void ChangeDateSelector(object sender, EventArgs e)
		{
			dateTimePicker.Enabled = dateSelector.SelectedIndex > 0;
		}


		private void ChangeScope(object sender, EventArgs e)
		{
			// not Page scope
			dateSelector.Enabled = scopeBox.SelectedIndex < scopeBox.Items.Count - 1;
			dateTimePicker.Enabled = dateSelector.Enabled && dateSelector.SelectedIndex > 0;
		}


		private void TogglerRegBox(object sender, EventArgs e)
		{
			searchButton.Enabled = !regBox.Checked;
			ChangedText(sender, e);
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private async void Search(object sender, EventArgs e)
		{
			if (!searchButton.Enabled)
			{
				// presume Regex is chosen but the input text is an invalid regular expression
				return;
			}

			if (InvokeRequired)
			{
				BeginInvoke(new Action<object, EventArgs>(Search), sender, e);
				return;
			}

			// set controls and prepare for search...

			findBox.Enabled = false;
			searchButton.Enabled = false;

			ClearResults();
			nextButton.Visible = prevButton.Visible = false;
			moveButton.Visible = copyButton.Visible = false;

			// Build the regex once for the entire search; all scope methods share it
			var finder = new TextMatchBuilder(regBox.Checked, matchBox.Checked)
				.BuildRegex(findBox.Text);

			if (finder is null)
			{
				// invalid regex — ChangedText should have caught this, but guard anyway
				RestoreControls();
				return;
			}

			// async void must not let exceptions escape to the SynchronizationContext,
			// which under the dllhost surrogate can take down OneNote
			try
			{
				await using var one = new OneNote();

				// search by scope...

				if (scopeBox.SelectedIndex == 0)
				{
					await SearchNotebook(one, finder);
				}
				else if (scopeBox.SelectedIndex == 1)
				{
					grouping = true;
					var section = await one.GetSection();
					var ns = one.GetNamespace(section);

					if (dateSelector.SelectedIndex > 0)
					{
						FilterPagesByDate(section, ns);
					}

					SetupProgressBar(section.Descendants(ns + "Page").Count());
					await SearchSection(one, section, finder);
				}
				else
				{
					logger.StartClock();
					grouping = false;

					var page = await one.GetPage(one.CurrentPageId, OneNote.PageDetail.Basic);
					logger.WriteTime("loaded page", keepRunning: true);

					await SearchPage(one, page, finder);
					logger.WriteTime("search complete");
				}
			}
			catch (OperationCanceledException)
			{
				logger.WriteLine("search cancelled");
			}
			catch (Exception exc)
			{
				logger.WriteLine("error during search", exc);
			}
			finally
			{
				// restore controls and dispose the cancellation source even on failure
				source?.Dispose();
				source = null;

				RestoreControls();
			}
		}


		private void RestoreControls()
		{
			pageLabel.Text = string.Empty;
			progressBar.Visible = false;
			findBox.Enabled = true;
			searchButton.Enabled = true;
			findBox.Focus();
			searchButton.NotifyDefault(true);

			// only show nav buttons if there is at least one navigable hit
			nextButton.Visible = prevButton.Visible = resultsView.HasHits;
			moveButton.Visible = copyButton.Visible = resultsView.HasHits;
			resultsHeaderPanel.Visible = resultsView.HasHits;
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Results list management

		private void ClearResults()
		{
			resultsView.Clear();
			resultsHeaderPanel.Visible = false;
			moveButton.Enabled = false;
			copyButton.Enabled = false;
		}


		/// <summary>
		/// Appends a card for the page (when pageId/groupTitle are non-null) with all its hits.
		/// Appending one card per page keeps the number of repaints proportional to pages
		/// searched, not individual matches. SearchResultsCardView.AppendCard primes the
		/// paint queue via Update() using the same trick as the previous virtual ListView.
		/// </summary>
		private void AddPageResults(string pageId, string groupTitle, string sectionColor, IList<SearchHit> hits)
		{
			var swatch = groupTitle != null ? ColorHelper.FromHtml(sectionColor) : Color.Empty;

			var card = new CardModel
			{
				Title = groupTitle,
				PageId = pageId,
				SectionColor = swatch
			};

			foreach (var hit in hits)
			{
				card.Hits.Add(new CardHit
				{
					PlainText = hit.PlainText,
					PageId = pageId,
					ObjectId = hit.ObjectId
				});
			}

			resultsView.AppendCard(card);
		}


		private async void OnCardActivated(object sender, NavigateCardEventArgs e)
		{
			await NavigateTo(e.PageId, string.Empty);
		}


		private async void OnHitActivated(object sender, NavigateHitEventArgs e)
		{
			await NavigateTo(e.PageId, e.ObjectId);
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Keyboard navigation

		private void HandleNavKey(object sender, KeyEventArgs e)
		{
			if ((e.KeyCode == Keys.N || e.KeyCode == Keys.Down) && e.Modifiers == Keys.None)
			{
				MoveTo(1);
				e.Handled = true;
			}
			else if ((e.KeyCode == Keys.P || e.KeyCode == Keys.Up) && e.Modifiers == Keys.None)
			{
				MoveTo(-1);
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Enter && e.Modifiers == Keys.None)
			{
				var target = resultsView.GetSelectedTarget();
				if (target.HasValue)
				{
					_ = NavigateTo(target.Value.pageId, target.Value.objectId);
				}
				e.Handled = true;
			}
		}


		private void MoveToPreviousSelection(object sender, EventArgs e) => MoveTo(-1);
		private void MoveToNextSelection(object sender, EventArgs e) => MoveTo(1);


		private async void MoveTo(int delta)
		{
			resultsView.MoveSelection(delta);
			resultsView.Focus();

			var target = resultsView.GetSelectedTarget();
			if (target.HasValue)
			{
				await NavigateTo(target.Value.pageId, target.Value.objectId);
			}
		}


		private async Task NavigateTo(string pageId, string objectId = "")
		{
			await using var one = new OneNote();
			await one.NavigateTo(pageId, objectId);
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Search pipeline

		private void FilterPagesByDate(XElement parent, XNamespace ns)
		{
			var field = "dateTime";
			var after = true;

			switch (dateSelector.SelectedIndex)
			{
				case CreatedBefore:
					after = false;
					break;

				case UpdatedAfter:
					field = "lastModifiedTime";
					break;

				case UpdatedBefore:
					field = "lastModifiedTime";
					after = false;
					break;
			}

			var date = dateTimePicker.Value.Date;

			var pages = parent.Descendants(ns + "Page").ToList();
			foreach (var page in pages)
			{
				var value = DateTime.Parse(
					page.Attribute(field).Value, CultureInfo.InvariantCulture).ToLocalTime().Date;

				// note: these comparisons are inverted because we Remove() non-matches
				if ((after && value < date) || (!after && value > date))
				{
					page.Remove();
				}
			}
		}


		private async Task SearchNotebook(OneNote one, Regex finder)
		{
			grouping = true;

			var notebook = await one.GetNotebook(OneNote.Scope.Pages);
			var ns = one.GetNamespace(notebook);

			if (dateSelector.SelectedIndex > 0)
			{
				FilterPagesByDate(notebook, ns);
			}

			SetupProgressBar(notebook.Descendants(ns + "Page").Count());
			await TraverseSections(notebook, string.Empty);

			// decending recursive section traversal
			async Task TraverseSections(XElement parent, string path)
			{
				var sections = parent.Elements(ns + "Section").Where(e =>
					e.Attribute("isRecycleBin") is null &&
					e.Attribute("isInRecycleBin") is null);

				foreach (var section in sections)
				{
					if (source.IsCancellationRequested) break;
					await Task.Yield();
					await SearchSection(one, section, finder);
				}

				var sectionGroups = parent.Elements(ns + "SectionGroup")
					.Where(e => e.Attribute("isRecycleBin") is null);

				foreach (var group in sectionGroups)
				{
					if (source.IsCancellationRequested) break;
					await Task.Yield();

					var groupName = group.Attribute("name").Value;
					path = path.Length == 0 ? groupName : $"{path}/{groupName}";

					await TraverseSections(group, path);
				}
			}
		}


		private void SetupProgressBar(int count)
		{
			progressBar.Visible = true;
			progressBar.Maximum = count;
			progressBar.Value = 0;
			source = new CancellationTokenSource();
		}


		private async Task SearchSection(OneNote one, XElement section, Regex finder)
		{
			var ns = one.GetNamespace(section);
			var sectionId = section.Attribute("ID").Value;
			var info = await one.GetSectionInfo(sectionId);

			var pageIds = section.Elements(ns + "Page")
				.Select(e => e.Attribute("ID").Value)
				.ToList();

			foreach (var pageId in pageIds)
			{
				if (source.IsCancellationRequested) break;
				await Task.Yield();

				progressBar.Increment(1);

				var page = await one.GetPage(pageId, OneNote.PageDetail.Basic);
				var pageName = page.Root.Attribute("name").Value;

				if (grouping)
				{
					pageLabel.Text = pageName;
				}

				var hits = await SearchPageBody(page, finder);

				// skip adding results if cancelled mid-page
				if (source.IsCancellationRequested) break;

				if (hits.Any())
				{
					AddPageResults(
						pageId,
						grouping ? $"{info.Path}/{pageName}" : null,
						info.Color,
						hits);
				}
			}
		}


		private async Task SearchPage(OneNote one, Page page, Regex finder)
		{
			var hits = await SearchPageBody(page, finder);
			if (hits.Any())
			{
				AddPageResults(page.PageId, null, null, hits);
			}
		}


		private async Task<IList<SearchHit>> SearchPageBody(Page page, Regex finder)
		{
			var ns = page.Namespace;

			// Materialize paragraphs on the UI thread before going to the thread pool,
			// so the lazy XLinq query is evaluated while we own the XDocument.
			var paragraphs = page.BodyOutlines
				.Descendants(ns + "OE")
				.ToList();

			if (!includeTocBox.Checked)
			{
				paragraphs = paragraphs.Where(e => e.Elements(ns + "T").Any()
					&& !e.AncestorsAndSelf(ns + "OE")
						.Any(a => a.Elements(ns + "Meta")
							.Any(m => m.Attribute("name")?.Value == "omToc")))
				.ToList();
			}

			if (paragraphs.Count == 0)
			{
				return Array.Empty<SearchHit>();
			}

			var token = source?.Token ?? CancellationToken.None;

			// Text extraction and regex matching on a background thread — pure XLinq, no COM.
			// IsCancellationRequested is checked without throwing to avoid propagating
			// OperationCanceledException through the async void Search() call chain.
			var matched = await Task.Run(() =>
			{
				var hits = new List<(string objectId, string text)>();
				foreach (var paragraph in paragraphs)
				{
					if (token.IsCancellationRequested) break;

					var text = GetRawText(paragraph, ns);
					if (text.Length > 0 && finder.IsMatch(text))
					{
						hits.Add((paragraph.Attribute("objectID").Value, text));
					}
				}
				return hits;
			});

			// Hyperlinks are resolved lazily at navigation time (one.NavigateTo(pageId, objectId))
			// so no COM round-trip is needed here per hit.
			return matched.Select(m => new SearchHit
			{
				PlainText = HttpUtility.HtmlDecode(m.text),
				ObjectId  = m.objectId
			}).ToList();
		}


		private string GetRawText(XElement paragraph, XNamespace ns)
		{
			var text = string.Empty;
			paragraph.Elements(ns + "T").ForEach(e =>
			{
				// custom cleaner regex adds filter for "&#nnn;" escapes, instead of TextValue
				//
				//
				// NOTE, instead of ignoring escape sequences, use WebUtility.HtmlDecode(input);
				//
				//
				var line = cleaner.Replace(e.Value, string.Empty).Trim();
				if (line.Length > 0)
				{
					text = $"{text}{line} ";
				}
			});

			return text.Trim();
		}
	}
}
