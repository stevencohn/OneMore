//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.UI;
	using Resx = Properties.Resources;


	internal partial class SearchTitleDialog : MoreForm
	{
		private const int DebounceMilliseconds = 300;

		private readonly Timer debounceTimer;
		private readonly Dictionary<string, XElement> notebookCache = new();
		private List<(string Id, string Name)> notebookList = new();
		private string currentNotebookId;
		private SearchErrorControl errorControl;
		private bool searching;
		private bool searchPending;
		private XElement queries;
		private string lastSearchedText;
		private bool lastSearchRemembered;

		// Search Titles multi-level results: the full unfiltered set built by the last search
		// (dividers + hit cards, in final display order), re-rendered locally into resultsView
		// whenever the type filter bar's selection changes, with no re-search involved.
		private readonly List<CardModel> allCards = new();
		private bool lastSearchScoped;


		public SearchTitleDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.SearchTitleDialog_Text;

				Localize(new string[]
				{
					"introLabel",
					"findLabel=word_Find",
					"indexButton=word_Index",
					"cancelButton=word_Cancel",
					"selectAllLink",
					"clearAllLink"
				});
			}

			DefaultControl = findBox;
			ElevatedWithOneNote = true;
			RememberSize = true;
			StartPosition = FormStartPosition.Manual;

			resultsView.CardActivated += OnCardActivated;
			resultsView.CheckedChanged += OnCheckedChanged;
			resultsView.KeyDown += HandleNavKey;
			resultsView.Enter += ResultsViewEntered;
			typeFilterBar.FilterChanged += (s, e) => RenderFiltered();

			debounceTimer = new Timer { Interval = DebounceMilliseconds };
			debounceTimer.Tick += DebounceTick;

			LoadSettings();
		}


		private void LoadSettings()
		{
			var provider = new SettingsProvider();
			var settings = provider.GetCollection("SearchTitle");

			queries = settings.Get<XElement>("queries");
			if (queries != null)
			{
				foreach (var query in queries.Elements())
				{
					findBox.Items.Add(query.Value);
				}
			}
		}


		private void SaveQuery(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return;
			}

			queries ??= new XElement("queries");

			queries.Elements().FirstOrDefault(e => e.Value == text)?.Remove();

			while (queries.Elements().Count() >= 8)
			{
				queries.Elements().Last().Remove();
			}

			queries.AddFirst(new XElement("query", text));

			var provider = new SettingsProvider();
			var settings = provider.GetCollection("SearchTitle");
			settings.Add("queries", queries);
			provider.SetCollection(settings);
			provider.Save();

			findBox.Items.Clear();
			foreach (var query in queries.Elements())
			{
				findBox.Items.Add(query.Value);
			}
		}


		/// <summary>
		/// Remembers the current search term as a successful MRU entry, but only once per
		/// search cycle and only while findBox still shows the text that produced the results
		/// currently on screen.
		/// </summary>
		private void RememberIfSuccessful()
		{
			if (lastSearchRemembered ||
				string.IsNullOrEmpty(lastSearchedText) ||
				findBox.Text.Trim() != lastSearchedText ||
				!resultsView.HasCards)
			{
				return;
			}

			SaveQuery(lastSearchedText);
			lastSearchRemembered = true;
		}


		private void ResultsViewEntered(object sender, EventArgs e)
		{
			RememberIfSuccessful();
		}


		public string Query { get; private set; }

		public IEnumerable<CardModel> SelectedCards { get; private set; }


		protected override void OnShown(EventArgs e)
		{
			// base method must be called to complete the ElevatedWithOneNote procedure
			base.OnShown(e);
			findBox.Focus();
		}


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			findBox.Focus();
		}


		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Escape)
			{
				Nevermind(this, EventArgs.Empty);
				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Input handling / type-ahead

		private void ChangedText(object sender, EventArgs e)
		{
			var text = findBox.Text;

			debounceTimer.Stop();

			if (TitleQueryParser.CountSignificantChars(text) > 2)
			{
				debounceTimer.Start();
			}
		}


		private void SearchOnKeydown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter && findBox.Text.Trim().Length > 0)
			{
				debounceTimer.Stop();
				RunSearch(remember: true);
				e.Handled = true;
			}
		}


		private void Search(object sender, EventArgs e)
		{
			debounceTimer.Stop();
			RunSearch(remember: true);
		}


		private void DebounceTick(object sender, EventArgs e)
		{
			debounceTimer.Stop();
			RunSearch();
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Search orchestration

		/// <summary>
		/// Entry point for both manual (button/Enter) and type-ahead searches. Coalesces
		/// overlapping triggers so a burst of keystrokes while a search is already running
		/// results in exactly one more run afterward rather than interleaved/dropped updates.
		/// </summary>
		private async void RunSearch(bool remember = false)
		{
			if (searching)
			{
				searchPending = true;
				return;
			}

			searching = true;
			try
			{
				do
				{
					searchPending = false;
					await DoSearchAsync(remember);
					remember = false;
				}
				while (searchPending);
			}
			finally
			{
				searching = false;
			}
		}


		private async Task DoSearchAsync(bool remember)
		{
			var rawText = findBox.Text.Trim();
			lastSearchedText = rawText;
			lastSearchRemembered = false;

			var query = TitleQueryParser.Parse(findBox.Text);

			ClearResults();

			if (string.IsNullOrEmpty(query.TitleText) &&
				query.Hashtags.Count == 0 && query.ExcludeHashtags.Count == 0)
			{
				return;
			}

			if ((query.Hashtags.Count > 0 || query.ExcludeHashtags.Count > 0) &&
				!HashtagProvider.CatalogExists())
			{
				ShowMessageCard(Resx.SearchTitleDialog_noHashtagCatalog);
				return;
			}

			Regex finder = null;
			if (!string.IsNullOrEmpty(query.TitleText))
			{
				finder = new TextMatchBuilder(false, false).BuildRegex(query.TitleText);
			}

			// A search scoped to one specific notebook (by name, or "\\" for the current
			// notebook) excludes that notebook itself from matching - it's the search's scope,
			// not a candidate hit - so its name isn't tested and its filter chip is hidden.
			var scoped = !string.IsNullOrEmpty(query.NotebookFilter) && query.NotebookFilter != "*";
			lastSearchScoped = scoped;

			var hashtagSuffix = query.Hashtags.Count > 0 ? string.Join(" ", query.Hashtags) : null;

			try
			{
				await using var one = new OneNote();

				var notebooks = await ResolveNotebooksAsync(one, query.NotebookFilter);
				if (notebooks.Count == 0)
				{
					RestoreControls();
					return;
				}

				ISet<string> hashtagPageIds = null;
				ISet<string> excludedHashtagPageIds = null;
				if (query.Hashtags.Count > 0 || query.ExcludeHashtags.Count > 0)
				{
					var notebookIds = notebooks.Select(n => n.Id).ToList();
					(hashtagPageIds, excludedHashtagPageIds) = SearchTitleEngine.ResolveHashtagFilters(
						query.Hashtags, query.ExcludeHashtags, notebookIds);

					if (hashtagPageIds != null && hashtagPageIds.Count == 0)
					{
						RestoreControls();
						return;
					}
				}

				if (notebooks.Count > 1)
				{
					if (query.SortByModified)
					{
						var all = new List<TitleSearchResult>();
						foreach (var nb in notebooks)
						{
							all.AddRange(SearchTitleEngine.SearchNotebook(
								nb.Tree, nb.Name, finder, hashtagPageIds, excludedHashtagPageIds,
								matchAllLevels: true, matchNotebookName: !scoped));
						}

						SearchTitleEngine.Sort(all, sortByModified: true);

						foreach (var match in all)
						{
							allCards.Add(ToCard(match, hashtagSuffix));
						}
					}
					else
					{
						foreach (var nb in notebooks
							.OrderBy(n => n.Name, StringComparer.CurrentCultureIgnoreCase))
						{
							var matches = SearchTitleEngine.SearchNotebook(
								nb.Tree, nb.Name, finder, hashtagPageIds, excludedHashtagPageIds,
								matchAllLevels: true, matchNotebookName: !scoped);

							if (matches.Count == 0)
							{
								continue;
							}

							SearchTitleEngine.SortHierarchical(matches);

							allCards.Add(new CardModel { Title = nb.Name, IsHeader = true, IsPlainText = true });
							foreach (var match in matches)
							{
								allCards.Add(ToCard(match, hashtagSuffix));
							}
						}
					}
				}
				else
				{
					var nb = notebooks[0];
					var matches = SearchTitleEngine.SearchNotebook(
						nb.Tree, nb.Name, finder, hashtagPageIds, excludedHashtagPageIds,
						matchAllLevels: true, matchNotebookName: !scoped);

					if (query.SortByModified)
					{
						SearchTitleEngine.Sort(matches, sortByModified: true);
					}
					else
					{
						SearchTitleEngine.SortHierarchical(matches);
					}

					foreach (var match in matches)
					{
						allCards.Add(ToCard(match, hashtagSuffix));
					}
				}

				PopulateFilterBar();
				RenderFiltered();

				if (remember)
				{
					RememberIfSuccessful();
				}

				RestoreControls();
			}
			catch (Exception exc)
			{
				logger.WriteLine("error searching page titles", exc);
				RestoreControls();
			}
		}


		/// <summary>
		/// Resolves the notebook(s) targeted by a "\" filter (or all notebooks when none was
		/// specified; "\\" restricts to just the current notebook) to their cached Scope.Pages
		/// hierarchy trees, fetching and caching any notebook not already in the cache.
		/// Subsequent searches against an already-cached notebook require no COM calls at all.
		/// </summary>
		private async Task<List<(string Id, XElement Tree, string Name)>> ResolveNotebooksAsync(
			OneNote one, string notebookFilter)
		{
			currentNotebookId ??= one.CurrentNotebookId;

			if (notebookList.Count == 0)
			{
				var notebooksEl = await one.GetNotebooks(OneNote.Scope.Notebooks);
				if (notebooksEl != null)
				{
					var ns = one.GetNamespace(notebooksEl);
					notebookList = notebooksEl.Elements(ns + "Notebook")
						.Where(e => e.Attribute("isRecycleBin") is null)
						.Select(e => (
							Id: e.Attribute("ID")?.Value,
							Name: e.Attribute("name")?.Value ?? string.Empty))
						.Where(n => n.Id != null)
						.ToList();
				}
			}

			IEnumerable<(string Id, string Name)> targets;

			if (string.IsNullOrEmpty(notebookFilter))
			{
				targets = notebookList;
			}
			else if (notebookFilter == "*")
			{
				targets = notebookList;
			}
			else if (notebookFilter == "\\")
			{
				targets = notebookList.Where(n => n.Id == currentNotebookId);
			}
			else
			{
				targets = notebookList.Where(n =>
					n.Name.IndexOf(notebookFilter, StringComparison.OrdinalIgnoreCase) >= 0);
			}

			var result = new List<(string Id, XElement Tree, string Name)>();
			foreach (var (id, name) in targets)
			{
				if (!notebookCache.TryGetValue(id, out var tree))
				{
					tree = await one.GetNotebook(id, OneNote.Scope.Pages);
					if (tree != null)
					{
						notebookCache[id] = tree;
					}
				}

				if (tree != null)
				{
					result.Add((id, tree, name));
				}
			}

			return result;
		}


		private static CardModel ToCard(TitleSearchResult match, string hashtagSuffix) => new()
		{
			Title = match.Path,
			PageId = match.PageId,
			SectionColor = string.IsNullOrEmpty(match.Color)
				? Color.Empty
				: ColorHelper.FromHtml(match.Color),
			Modified = match.Modified,
			Level = match.Level,
			HashtagSuffix = match.Level == TitleHitLevel.Page ? hashtagSuffix : null
		};


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Results list management

		private void ClearResults()
		{
			if (errorControl != null)
			{
				Controls.Remove(errorControl);
				errorControl.Dispose();
				errorControl = null;
			}

			allCards.Clear();
			typeFilterBar.Clear();
			resultsView.Clear();
			resultsHeaderPanel.Visible = false;
			filterPanel.Visible = false;
			indexButton.Visible = false;
			indexButton.Enabled = false;
		}


		/// <summary>
		/// Refreshes the type filter chip counts from the current allCards set and resets the
		/// selection to "All". Called once per actual search - never on a filter click, or the
		/// chip the user just selected would immediately be reset back to "All".
		/// </summary>
		private void PopulateFilterBar()
		{
			var counts = new Dictionary<TitleHitLevel, int>();
			var total = 0;

			foreach (var card in allCards)
			{
				if (card.IsHeader || !card.Level.HasValue)
				{
					continue;
				}

				counts.TryGetValue(card.Level.Value, out var n);
				counts[card.Level.Value] = n + 1;
				total++;
			}

			typeFilterBar.SetCounts(total, counts, showNotebookChip: !lastSearchScoped);
			filterPanel.Visible = allCards.Count > 0;
		}


		/// <summary>
		/// Re-renders resultsView from allCards under the type filter bar's current selection.
		/// Purely local - no re-search - so it's cheap to call on every filter chip click.
		/// A grouping divider is kept only when at least one row from its notebook (its own
		/// hit, or one of its children) is still visible under the current filter; the same
		/// CardModel instances are reused (never rebuilt) so IsChecked survives filter switches.
		/// </summary>
		private void RenderFiltered()
		{
			resultsView.Clear();

			var selected = typeFilterBar.SelectedLevel;
			CardModel pendingHeader = null;
			var buffer = new List<CardModel>();

			void FlushGroup()
			{
				if (buffer.Count > 0)
				{
					if (pendingHeader != null)
					{
						resultsView.AppendCard(pendingHeader);
					}

					foreach (var card in buffer)
					{
						resultsView.AppendCard(card);
					}
				}

				buffer.Clear();
			}

			foreach (var card in allCards)
			{
				if (card.IsHeader)
				{
					FlushGroup();
					pendingHeader = card;
					continue;
				}

				if (selected == null || card.Level == selected)
				{
					buffer.Add(card);
				}
			}

			FlushGroup();
		}


		private void RestoreControls()
		{
			resultsHeaderPanel.Visible = resultsView.HasCards;

			if (!resultsView.HasCards)
			{
				ShowMessageCard(Resx.SearchTitleDialog_noResults);
			}
		}


		private void ShowMessageCard(string message)
		{
			errorControl = new SearchErrorControl(message, null)
			{
				Width = resultsView.Width - 20,
				Location = new Point(resultsView.Left + 10, resultsView.Top + 10),
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			};

			Controls.Add(errorControl);
			errorControl.BringToFront();
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Selection and navigation

		private void SelectAll(object sender, LinkLabelLinkClickedEventArgs e)
		{
			resultsView.CheckAll();
		}


		private void ClearSelection(object sender, LinkLabelLinkClickedEventArgs e)
		{
			resultsView.ClearChecked();
		}


		private void OnCheckedChanged(object sender, EventArgs e)
		{
			var hasChecked = resultsView.CheckedCount > 0;
			indexButton.Visible = hasChecked;
			indexButton.Enabled = hasChecked;
		}


		private async void OnCardActivated(object sender, NavigateCardEventArgs e)
		{
			RememberIfSuccessful();
			await NavigateTo(e.PageId, e.NewWindow);
		}


		private async void HandleNavKey(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Down && e.Modifiers == Keys.None)
			{
				resultsView.MoveSelection(1);
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Up && e.Modifiers == Keys.None)
			{
				resultsView.MoveSelection(-1);
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.PageDown && e.Modifiers == Keys.None)
			{
				resultsView.MoveSelectionPage(1);
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.PageUp && e.Modifiers == Keys.None)
			{
				resultsView.MoveSelectionPage(-1);
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Enter && e.Modifiers == Keys.None)
			{
				var target = resultsView.GetSelectedTarget();
				if (target.HasValue)
				{
					await NavigateTo(target.Value.pageId);
				}
				e.Handled = true;
			}
		}


		private static async Task NavigateTo(string pageId, bool newWindow = false)
		{
			await using var one = new OneNote();
			if (newWindow)
			{
				var uri = one.GetHyperlink(pageId, string.Empty);
				if (uri != null)
				{
					await one.NavigateTo(uri, newWindow: true);
				}
			}
			else
			{
				await one.NavigateTo(pageId, string.Empty);
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Index / Cancel

		private void IndexPressed(object sender, EventArgs e)
		{
			Query = findBox.Text;
			SelectedCards = resultsView.GetCheckedCards();
			DialogResult = DialogResult.OK;
			Close();
		}


		private void Nevermind(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
