//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.TextIndex;
	using River.OneMoreAddIn.UI;
	using Resx = Properties.Resources;


	internal partial class SearchDialog : MoreForm
	{
		public enum Commands
		{
			Index,
			Copy,
			Move
		}


		private CancellationTokenSource source;
		private SearchErrorControl errorControl;
		private Regex lastFinder;

		// pages visited by navigating to hits, traversed by the back/forward buttons
		private readonly List<(string pageId, string objectId)> history = new();
		private int historyIndex = -1;
		private string homePageId;

		// scopeBox indices; "In all notebooks" is always first (index 0). "In this section
		// group" is omitted from the combo entirely when the current section has no enclosing
		// section group, and "In this page group" is omitted when the current page has no
		// subpages and is not a subpage; omitting an item shifts the indices of the items
		// after it down by one
		private const int allScopeIndex = 0;
		private int introChrome;
		private int notebookScopeIndex;
		private int sectionGroupScopeIndex;
		private int sectionScopeIndex;
		private int pageGroupScopeIndex;
		private int pageScopeIndex;


		public SearchDialog(bool hasPageGroup)
		{
			InitializeComponent();

			// the space the intro panel needs around the label, so it can grow as the label wraps
			introChrome = morePanel2.Height - introLabel.Height;
			morePanel2.Resize += (s, e) => FitIntroLabel();

			if (NeedsLocalizing())
			{
				Text = Resx.word_Search;

				Localize(new string[]
				{
					"introLabel",
					"findLabel=word_Find",
					"indexButton=word_Index",
					"moveButton=word_Move",
					"copyButton=word_Copy",
					"cancelButton=word_Cancel",
					"selectAllLink",
					"clearAllLink"
				});

				scopeBox.Items.Clear();
				scopeBox.Items.AddRange(
					Resx.SearchDialogText_scopeOptions.Split('\n').Select(s => s.Trim()).ToArray());

				dateSelector.Items.Clear();
				dateSelector.Items.AddRange(Resx.SearchDialog_dateOptions.Split(
						new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
						.Select(s => s.Trim()).ToArray());
			}

			// the options and navigation buttons are glyph-only; their names are the tooltips and accessible names
			// and are not Localize()d because that would replace the glyphs with text
			foreach (var (box, name) in new (Control, string)[]
			{
				(matchBox, Resx.SearchDialog_matchBox_Text),
				(regBox, Resx.SearchDialog_regBox_Text),
				(includeTocBox, Resx.SearchDialog_includeTocBox_Text),
				(ocrBox, Resx.SearchDialog_ocrBox_Text),
				(emojiBox, Resx.SearchDialog_emojiBox_Text),
				(sortBox, Resx.SearchDialog_sortBox_Text),
				(nextButton, Resx.SearchDialog_nextButton_Text),
				(prevButton, Resx.SearchDialog_prevButton_Text),
				(backButton, Resx.SearchDialog_backButton_Text),
				(forwardButton, Resx.SearchDialog_forwardButton_Text),
				(homeButton, Resx.SearchDialog_homeButton_Text)
			})
			{
				tooltip.SetToolTip(box, name);
				box.AccessibleName = name;
			}

			FitIntroLabel();

			tooltip.SetToolTip(matchBox, $"{Resx.SearchDialog_matchBox_Text} (Ctrl+S)");
			tooltip.SetToolTip(regBox, $"{Resx.SearchDialog_regBox_Text} (Ctrl+R)");
			tooltip.SetToolTip(includeTocBox, $"{Resx.SearchDialog_includeTocBox_Text} (Ctrl+T)");
			tooltip.SetToolTip(ocrBox, $"{Resx.SearchDialog_ocrBox_Text} (Ctrl+I)");
			tooltip.SetToolTip(emojiBox, $"{Resx.SearchDialog_emojiBox_Text} (Ctrl+E)");
			tooltip.SetToolTip(sortBox, $"{Resx.SearchDialog_sortBox_Text} (Ctrl+M)");

			bool hasSectionGroup;
			using (var one = new OneNote())
			{
				hasSectionGroup = !string.IsNullOrEmpty(one.CurrentSectionGroupId);
				homePageId = one.CurrentPageId;
			}

			// remove the higher index first so the lower index remains valid
			if (!hasPageGroup)
			{
				scopeBox.Items.RemoveAt(3);
			}

			if (!hasSectionGroup)
			{
				scopeBox.Items.RemoveAt(1);
			}

			// "All notebooks" is added apart from the localized list so that a translation that
			// has only the original five lines still works; it is the first, widest scope
			scopeBox.Items.Insert(0, Resx.SearchDialog_scopeAll);

			notebookScopeIndex = 1;
			sectionGroupScopeIndex = hasSectionGroup ? 2 : -1;
			sectionScopeIndex = hasSectionGroup ? 3 : 2;
			pageGroupScopeIndex = hasPageGroup ? sectionScopeIndex + 1 : -1;
			pageScopeIndex = scopeBox.Items.Count - 1;

			// with full text indexing the wider scope is cheap, so default to this notebooks
			scopeBox.SelectedIndex = IndexConsumer.IsEnabled() ? notebookScopeIndex : pageScopeIndex;
			LoadState();

			// after toggling any option, return focus to the search phrase box
			matchBox.CheckedChanged += FocusFindBox;
			regBox.CheckedChanged += FocusFindBox;
			includeTocBox.CheckedChanged += FocusFindBox;
			ocrBox.CheckedChanged += FocusFindBox;
			emojiBox.CheckedChanged += FocusFindBox;
			sortBox.CheckedChanged += FocusFindBox;

			dateSelector.SelectedIndex = 0;
			dateTimePicker.MaxDate = DateTime.Today.AddDays(1);
			pageLabel.Text = string.Empty;

			DefaultControl = findBox;
			ElevatedWithOneNote = true;

			resultsView.CardActivated += OnCardActivated;
			resultsView.HitActivated += OnHitActivated;
			resultsView.CheckedChanged += OnCheckedChanged;
		}


		private const string StateName = "SearchDialog";


		private void FocusFindBox(object sender, EventArgs e)
		{
			findBox.Focus();
		}


		private void LoadState()
		{
			var settings = new SettingsProvider().GetCollection(StateName);

			matchBox.Checked = settings.Get("matchCase", false);
			regBox.Checked = settings.Get("regex", false);
			includeTocBox.Checked = settings.Get("toc", false);
			ocrBox.Checked = settings.Get("ocr", false);

			var scope = settings.Get("scope", string.Empty);
			var index = scope switch
			{
				"all" => allScopeIndex,
				"notebook" => notebookScopeIndex,
				"sectionGroup" => sectionGroupScopeIndex,
				"section" => sectionScopeIndex,
				"pageGroup" => pageGroupScopeIndex,
				"page" => pageScopeIndex,
				_ => -1
			};

			if (index >= 0 && index < scopeBox.Items.Count)
			{
				scopeBox.SelectedIndex = index;
			}
		}


		private void SaveState()
		{
			var index = scopeBox.SelectedIndex;
			string scope = null;
			if (index == allScopeIndex) { scope = "all"; }
			else if (index == notebookScopeIndex) { scope = "notebook"; }
			else if (index == sectionGroupScopeIndex) { scope = "sectionGroup"; }
			else if (index == sectionScopeIndex) { scope = "section"; }
			else if (index == pageGroupScopeIndex) { scope = "pageGroup"; }
			else if (index == pageScopeIndex) { scope = "page"; }

			var provider = new SettingsProvider();
			var settings = provider.GetCollection(StateName);

			settings.Add("matchCase", matchBox.Checked);
			settings.Add("regex", regBox.Checked);
			settings.Add("toc", includeTocBox.Checked);
			settings.Add("ocr", ocrBox.Checked);
			if (scope is not null)
			{
				settings.Add("scope", scope);
			}

			provider.SetCollection(settings);
			provider.Save();
		}


		public Commands Command { get; private set; }

		public string Query { get; private set; }

		public IEnumerable<CardModel> SelectedCards { get; private set; }

		public List<string> SelectedPages { get; private set; }


		protected override void OnShown(EventArgs e)
		{
			// base method must be called to complete the EvelatedWithOneNote procedure
			base.OnShown(e);

			findBox.Focus();
		}


		private void OnCheckedChanged(object sender, EventArgs e)
		{
			var hasChecked = resultsView.CheckedCount > 0;
			indexButton.Enabled = hasChecked;
			moveButton.Enabled = hasChecked;
			copyButton.Enabled = hasChecked;
		}


		private void IndexPressed(object sender, EventArgs e)
		{
			Command = Commands.Index;
			Query = findBox.Text;
			SelectedCards = resultsView.GetCheckedCards();
			DialogResult = DialogResult.OK;
			Close();
		}


		private void MovePressed(object sender, EventArgs e)
		{
			Command = Commands.Move;
			SelectedPages = resultsView.GetCheckedPageIds().ToList();
			DialogResult = DialogResult.OK;
			Close();
		}


		private void CopyPressed(object sender, EventArgs e)
		{
			Command = Commands.Copy;
			SelectedPages = resultsView.GetCheckedPageIds().ToList();
			DialogResult = DialogResult.OK;
			Close();
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
			DialogResult = DialogResult.Cancel;
			Close();
		}


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			// The date controls are anchored to the right edge, so narrowing the dialog slides
			// them left over the toggle buttons. Keep the dialog wide enough that they never touch.
			var gap = sortBox.Margin.Right + dateSelector.Margin.Left;
			var panelWidth = sortBox.Right + gap + (optionsPanel.ClientSize.Width - dateSelector.Left);
			var minimumWidth = panelWidth + Width - optionsPanel.ClientSize.Width;
			if (minimumWidth > MinimumSize.Width)
			{
				MinimumSize = new Size(minimumWidth, MinimumSize.Height);
			}

			if (Width < MinimumSize.Width)
			{
				Width = MinimumSize.Width;
			}

			findBox.Focus();
		}


		/// <summary>
		/// Extracts a "\\" (current notebook) or "\name" (named notebook) notation from the
		/// find text; not applied in regex mode where a backslash is meaningful.
		/// </summary>
		private string GetNotebookNotation(out string text)
		{
			text = findBox.Text;
			if (regBox.Checked)
			{
				return null;
			}

			var filter = TitleQueryParser.ExtractNotebook(text, out var remaining);
			if (filter is not null)
			{
				text = remaining.Trim();
			}

			return filter;
		}


		private void ChangedText(object sender, EventArgs e)
		{
			GetNotebookNotation(out var text);
			text = text.Trim();

			if (text.Length == 0)
			{
				searchButton.Enabled = false;
				searchButton.NotifyDefault(false);
				return;
			}

			if (emojiBox.Checked)
			{
				// only emojis are searched; other text is ignored
				var hasEmoji = TextMatchBuilder.BuildEmojiRegex(text) is not null;
				searchButton.Enabled = hasEmoji;
				searchButton.NotifyDefault(hasEmoji);
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


		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Escape)
			{
				Nevermind(this, EventArgs.Empty);
				return true;
			}

			CheckBox box = null;
			switch (keyData)
			{
				case Keys.Control | Keys.S: box = matchBox; break;
				case Keys.Control | Keys.R: box = regBox; break;
				case Keys.Control | Keys.T: box = includeTocBox; break;
				case Keys.Control | Keys.I: box = ocrBox; break;
				case Keys.Control | Keys.E: box = emojiBox; break;
				case Keys.Control | Keys.M: box = sortBox; break;
			}

			if (box is not null && box.Enabled && box.Visible)
			{
				box.Checked = !box.Checked;
				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}


		private void SearchOnKeydown(object sender, KeyEventArgs e)
		{
			GetNotebookNotation(out var keyText);
			if (e.KeyCode == Keys.Enter &&
				keyText.Trim().Length > 0)
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


		private void ToggleSortBox(object sender, EventArgs e)
		{
			resultsView.SortByModified(sortBox.Checked);
		}


		private void ToggleEmojiBox(object sender, EventArgs e)
		{
			// emoji mode is a live scan of emojis only, so the text options do not apply
			var enabled = !emojiBox.Checked;
			matchBox.Enabled = enabled;
			regBox.Enabled = enabled;
			includeTocBox.Enabled = enabled;
			ocrBox.Enabled = enabled;

			introLabel.Text = enabled
				? Resx.SearchDialog_introLabel_Text
				: Resx.SearchDialog_introEmoji_Text;

			FitIntroLabel();
			ChangedText(sender, e);
		}


		// wraps the intro label to the width of its panel and grows the panel to fit
		private void FitIntroLabel()
		{
			var width = morePanel2.ClientSize.Width
				- morePanel2.Padding.Right - introLabel.Left - introLabel.Margin.Right;

			if (width <= 0)
			{
				return;
			}

			introLabel.MaximumSize = new Size(width, 0);

			var height = introLabel.GetPreferredSize(new Size(width, 0)).Height + introChrome;
			if (morePanel2.Height != height)
			{
				morePanel2.Height = height;

				// the panels below shift down or up; repaint them and their toggle buttons
				// so no stale pixels are left where they used to be
				morePanel2.Invalidate(true);
				optionsPanel.Invalidate(true);
			}
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
			sortBox.Enabled = false;

			ClearResults();
			nextButton.Visible = prevButton.Visible = false;
			backButton.Visible = forwardButton.Visible = homeButton.Visible = false;
			ResetHistory();
			moveButton.Visible = copyButton.Visible = indexButton.Visible = false;

			// Build the regex once for the entire search; all scope methods share it
			var notebookFilter = GetNotebookNotation(out var queryText);
			var emojis = emojiBox.Checked;
			var builder = new TextMatchBuilder(regBox.Checked, matchBox.Checked);
			var finder = emojis
				? TextMatchBuilder.BuildEmojiRegex(queryText)
				: builder.BuildRegex(queryText);

			if (finder is null)
			{
				// invalid regex — ChangedText should have caught this, but guard anyway
				RestoreControls();
				return;
			}

			// the options are now known to be valid and used for a search
			SaveState();

			lastFinder = finder;

			string summary = null;

			// async void must not let exceptions escape to the SynchronizationContext,
			// which under the dllhost surrogate can take down OneNote
			try
			{
				source = new CancellationTokenSource();

				var options = new SearchOptions
				{
					IncludeToc = includeTocBox.Checked && !emojis,
					IncludeOcr = ocrBox.Checked && !emojis,
					Emojis = emojis,
					DateFilter = (DateFilterMode)dateSelector.SelectedIndex,
					DateValue = dateTimePicker.Value.Date
				};

				void OnTotal(int count) => SetupProgressBar(count);
				void OnPage(string name) { progressBar.Increment(1); pageLabel.Text = name; }
				var returned = 0;
				void OnFound(string pageId, string groupTitle, string color,
					DateTime modified, IList<SearchHit> hits)
				{
					returned++;
					AddPageResults(pageId, groupTitle, color, modified, hits);
				}

				var engine = new SearchEngine(options, source.Token, OnTotal, OnPage, OnFound);

				// The text index answers for the pages it has current; the rest are read as before.
				// It does not hold the paragraphs of a generated table of contents, and a single
				// page is always read.
				// Emoji searches never use the index; every page is read and scanned.
				if (!emojis && !options.IncludeToc &&
					(notebookFilter is not null || scopeBox.SelectedIndex != pageScopeIndex))
				{
					pageLabel.Text = Resx.SearchDialog_indexChecking;
					engine.Indexed = await IndexedHits.Load(
							builder, queryText, finder, source.Token, options.IncludeOcr);
				}

				await using var one = new OneNote();
				engine.LivePageID = one.CurrentPageId;

				if (notebookFilter is not null)
				{
					// "\\" is the current notebook, "\*" or a bare "\" is all, else by name
					if (notebookFilter == "\\")
					{
						await engine.SearchNotebook(one, finder);
					}
					else if (notebookFilter.Length == 0 || notebookFilter == "*")
					{
						await engine.SearchAllNotebooks(one, finder);
					}
					else
					{
						await engine.SearchNotebooks(one, notebookFilter, finder);
					}
				}
				else if (scopeBox.SelectedIndex == allScopeIndex)
				{
					await engine.SearchAllNotebooks(one, finder);
				}
				else if (scopeBox.SelectedIndex == notebookScopeIndex)
				{
					await engine.SearchNotebook(one, finder);
				}
				else if (scopeBox.SelectedIndex == sectionGroupScopeIndex)
				{
					// GetSection just calls GetHierarchy(id, hsPages), which is node-kind
					// agnostic, so it also works for a section group ID (see
					// CompareCommand.FetchSubtree for the same precedent)
					var group = await one.GetSection(one.CurrentSectionGroupId);
					if (group is not null)
					{
						await engine.SearchNotebook(one, group, finder);
					}
				}
				else if (scopeBox.SelectedIndex == sectionScopeIndex)
				{
					var section = await one.GetSection();
					await engine.SearchSection(one, section, finder);
				}
				else if (scopeBox.SelectedIndex == pageGroupScopeIndex)
				{
					var section = await one.GetSection();
					if (section is not null)
					{
						await engine.SearchPageGroup(one, section, one.CurrentPageId, finder);
					}
				}
				else
				{
					logger.StartClock();
					var page = await one.GetPage(one.CurrentPageId, OneNote.PageDetail.Basic);
					logger.WriteTime("loaded page", keepRunning: true);
					await engine.SearchPage(page, finder);
					logger.WriteTime("search complete");
				}

				if (engine.Indexed is not null)
				{
					summary = string.Format(
						Resx.SearchDialog_indexSummary,
						engine.IndexedPageCount, engine.ReadPageCount, returned);

					logger.WriteLine(summary);
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

				RestoreControls(summary);
			}
		}


		private void RestoreControls(string summary = null)
		{
			pageLabel.Text = summary ?? string.Empty;
			progressBar.Visible = false;
			findBox.Enabled = true;
			searchButton.Enabled = true;
			sortBox.Enabled = true;

			if (sortBox.Checked)
			{
				resultsView.SortByModified(true);
			}

			findBox.Focus();
			searchButton.NotifyDefault(true);

			// only show nav buttons if there is at least one navigable hit
			nextButton.Visible = prevButton.Visible = resultsView.HasHits;
			backButton.Visible = forwardButton.Visible = homeButton.Visible = resultsView.HasHits;
			UpdateHistoryButtons();
			indexButton.Visible = moveButton.Visible = copyButton.Visible = resultsView.HasHits;

			// a single page has no multi-page results to select, so the line stays collapsed
			resultsHeaderPanel.Visible = resultsView.HasHits && scopeBox.SelectedIndex != pageScopeIndex;

			if (!resultsView.HasHits && lastFinder != null)
			{
				ShowNoResultsCard();
			}
		}


		private void ShowNoResultsCard()
		{
			errorControl = new SearchErrorControl("No results found", lastFinder.ToString())
			{
				Width = resultsView.Width - 20,
				Location = new Point(resultsView.Left + 10, resultsView.Top + 10),
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
			};

			Controls.Add(errorControl);
			errorControl.BringToFront();
		}


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
			indexButton.Enabled = false;
			moveButton.Enabled = false;
			copyButton.Enabled = false;
		}


		/// <summary>
		/// Appends a card for the page (when pageId/groupTitle are non-null) with all its hits.
		/// Appending one card per page keeps the number of repaints proportional to pages
		/// searched, not individual matches. SearchResultsCardView.AppendCard primes the
		/// paint queue via Update() using the same trick as the previous virtual ListView.
		/// </summary>
		private void AddPageResults(
			string pageId, string groupTitle, string sectionColor,
			DateTime modified, IList<SearchHit> hits)
		{
			var swatch = groupTitle != null ? ColorHelper.FromHtml(sectionColor) : Color.Empty;

			var card = new CardModel
			{
				Title = groupTitle,
				PageId = pageId,
				SectionColor = swatch,
				Modified = modified
			};

			foreach (var hit in hits)
			{
				card.Hits.Add(new CardHit
				{
					PlainText = hit.IsImageText
						? FormatImageHit(hit.PlainText)
						: hit.PlainText,
					PageId = pageId,
					ObjectId = hit.ObjectId
				});
			}

			resultsView.AppendCard(card);
		}


		/// <summary>
		/// Text recognized in an image can be a whole screenshot of text, so show a short excerpt
		/// around the first match, marked as coming from an image.
		/// </summary>
		private string FormatImageHit(string text)
		{
			const int Radius = 80;

			var start = 0;
			var length = text.Length;
			var match = lastFinder?.Match(text);

			if (text.Length > Radius * 2)
			{
				var at = match is not null && match.Success ? match.Index : 0;
				start = Math.Max(0, at - Radius);
				length = Math.Min(text.Length - start, Radius * 2 + (match?.Length ?? 0));
			}

			var excerpt = text.Substring(start, length);
			if (start > 0)
			{
				excerpt = "…" + excerpt;
			}

			if (start + length < text.Length)
			{
				excerpt += "…";
			}

			return string.Format(Resx.SearchDialog_imageHit, excerpt);
		}


		private async void OnCardActivated(object sender, NavigateCardEventArgs e)
		{
			await NavigateToHit(e.PageId, string.Empty, e.NewWindow);
		}


		private async void OnHitActivated(object sender, NavigateHitEventArgs e)
		{
			await NavigateToHit(e.PageId, e.ObjectId, e.NewWindow);
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
					_ = NavigateToHit(target.Value.pageId, target.Value.objectId);
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
				await NavigateToHit(target.Value.pageId, target.Value.objectId);
			}
		}


		// navigates to a hit and records it in the history; replaying the history or going
		// home must call NavigateTo directly so as not to alter the history
		private async Task NavigateToHit(string pageId, string objectId = "", bool newWindow = false)
		{
			if (!newWindow &&
				(historyIndex < 0 ||
				history[historyIndex].pageId != pageId ||
				history[historyIndex].objectId != objectId))
			{
				// discard any forward entries, like a web browser
				if (historyIndex < history.Count - 1)
				{
					history.RemoveRange(historyIndex + 1, history.Count - historyIndex - 1);
				}

				history.Add((pageId, objectId));
				historyIndex = history.Count - 1;
				UpdateHistoryButtons();
			}

			await NavigateTo(pageId, objectId, newWindow);
		}


		// slot 0 is always the page the user was on when the dialog opened
		private void ResetHistory()
		{
			history.Clear();
			historyIndex = -1;

			if (!string.IsNullOrEmpty(homePageId))
			{
				history.Add((homePageId, string.Empty));
				historyIndex = 0;
			}
		}


		private async Task GoToHistory(int index)
		{
			historyIndex = index;
			UpdateHistoryButtons();

			var (pageId, objectId) = history[historyIndex];
			resultsView.SelectTarget(pageId, objectId);
			await NavigateTo(pageId, objectId);
		}


		private async void GoBack(object sender, EventArgs e)
		{
			if (historyIndex > 0)
			{
				await GoToHistory(historyIndex - 1);
			}
		}


		private async void GoForward(object sender, EventArgs e)
		{
			if (historyIndex < history.Count - 1)
			{
				await GoToHistory(historyIndex + 1);
			}
		}


		private async void GoHome(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(homePageId) && history.Count > 0)
			{
				await GoToHistory(0);
			}
		}


		private void UpdateHistoryButtons()
		{
			backButton.Enabled = historyIndex > 0;
			forwardButton.Enabled = historyIndex < history.Count - 1;
			homeButton.Enabled = !string.IsNullOrEmpty(homePageId) && historyIndex != 0;
		}


		private static async Task NavigateTo(string pageId, string objectId = "", bool newWindow = false)
		{
			await using var one = new OneNote();
			if (newWindow)
			{
				var uri = one.GetHyperlink(pageId, objectId);
				if (uri != null)
				{
					await one.NavigateTo(uri, newWindow: true);
				}
			}
			else
			{
				await one.NavigateTo(pageId, objectId);
			}
		}


		private void SetupProgressBar(int count)
		{
			progressBar.Visible = true;
			progressBar.Maximum = count;
			progressBar.Value = 0;
		}
	}
}
