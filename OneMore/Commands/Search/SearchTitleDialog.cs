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

			resultsView.CardActivated += OnCardActivated;
			resultsView.CheckedChanged += OnCheckedChanged;

			debounceTimer = new Timer { Interval = DebounceMilliseconds };
			debounceTimer.Tick += DebounceTick;
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
			searchButton.Enabled = text.Trim().Length > 0;

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
				RunSearch();
				e.Handled = true;
			}
		}


		private void Search(object sender, EventArgs e)
		{
			debounceTimer.Stop();
			RunSearch();
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
		private async void RunSearch()
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
					await DoSearchAsync();
				}
				while (searchPending);
			}
			finally
			{
				searching = false;
			}
		}


		private async Task DoSearchAsync()
		{
			var query = TitleQueryParser.Parse(findBox.Text);

			ClearResults();

			if (string.IsNullOrEmpty(query.TitleText) && query.Hashtags.Count == 0)
			{
				return;
			}

			if (query.Hashtags.Count > 0 && !HashtagProvider.CatalogExists())
			{
				ShowMessageCard(Resx.SearchTitleDialog_noHashtagCatalog);
				return;
			}

			Regex finder = null;
			if (!string.IsNullOrEmpty(query.TitleText))
			{
				finder = new TextMatchBuilder(false, false).BuildRegex(query.TitleText);
			}

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
				if (query.Hashtags.Count > 0)
				{
					hashtagPageIds = ResolveHashtagPageIds(query.Hashtags, notebooks);
					if (hashtagPageIds.Count == 0)
					{
						RestoreControls();
						return;
					}
				}

				if (notebooks.Count > 1)
				{
					foreach (var nb in notebooks
						.OrderBy(n => n.Name, StringComparer.CurrentCultureIgnoreCase))
					{
						var matches = SearchTitleEngine.SearchNotebook(
							nb.Tree, nb.Name, finder, hashtagPageIds);

						if (matches.Count == 0)
						{
							continue;
						}

						SearchTitleEngine.Sort(matches, query.SortByModified);

						resultsView.AppendCard(new CardModel { Title = nb.Name, IsHeader = true });
						foreach (var match in matches)
						{
							resultsView.AppendCard(ToCard(match));
						}
					}
				}
				else
				{
					var nb = notebooks[0];
					var matches = SearchTitleEngine.SearchNotebook(
						nb.Tree, nb.Name, finder, hashtagPageIds);

					SearchTitleEngine.Sort(matches, query.SortByModified);

					foreach (var match in matches)
					{
						resultsView.AppendCard(ToCard(match));
					}
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
		/// Resolves the notebook(s) targeted by an "nb:\" filter (or the current notebook when
		/// none was specified) to their cached Scope.Pages hierarchy trees, fetching and caching
		/// any notebook not already in the cache. Subsequent searches against an already-cached
		/// notebook require no COM calls at all.
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
				targets = notebookList.Where(n => n.Id == currentNotebookId);
			}
			else if (notebookFilter == "*")
			{
				targets = notebookList;
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


		/// <summary>
		/// Looks up the set of page IDs carrying every one of the given hashtags (implicit AND,
		/// same as the hashtag catalog's own query semantics), restricted to the given notebooks.
		/// </summary>
		private static HashSet<string> ResolveHashtagPageIds(
			List<string> hashtags, List<(string Id, XElement Tree, string Name)> notebooks)
		{
			var pageIds = new HashSet<string>();
			var hashtagQuery = string.Join(" ", hashtags);

			using var provider = new HashtagProvider();

			if (notebooks.Count == 1)
			{
				var tags = provider.SearchTags(
					hashtagQuery, caseSensitive: false, allTags: false,
					parsed: out _, notebookID: notebooks[0].Id);

				foreach (var tag in tags)
				{
					pageIds.Add(tag.PageID);
				}
			}
			else
			{
				var ids = new HashSet<string>(notebooks.Select(n => n.Id));
				var tags = provider.SearchTags(
					hashtagQuery, caseSensitive: false, allTags: false, parsed: out _);

				foreach (var tag in tags)
				{
					if (ids.Contains(tag.NotebookID))
					{
						pageIds.Add(tag.PageID);
					}
				}
			}

			return pageIds;
		}


		private static CardModel ToCard(TitleSearchResult match) => new()
		{
			Title = match.Path,
			PageId = match.PageId,
			SectionColor = string.IsNullOrEmpty(match.Color)
				? Color.Empty
				: ColorHelper.FromHtml(match.Color)
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

			resultsView.Clear();
			resultsHeaderPanel.Visible = false;
			indexButton.Visible = false;
			indexButton.Enabled = false;
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
			await NavigateTo(e.PageId, e.NewWindow);
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
