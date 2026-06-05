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


		public SearchDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.word_Search;

				Localize(new string[]
				{
					"introLabel",
					"findLabel=word_Find",
					"matchBox",
					"regBox",
					"includeTocBox",
					"indexButton=word_Index",
					"moveButton=word_Move",
					"copyButton=word_Copy",
					"cancelButton=word_Cancel",
					"selectAllLink",
					"clearAllLink"
				});

				scopeBox.Items.Clear();
				scopeBox.Items.AddRange(Resx.SearchDialogText_scopeOptions.Split('\n'));

				dateSelector.Items.Clear();
				dateSelector.Items.AddRange(Resx.SearchDialog_dateOptions.Split('\n'));
			}

			scopeBox.SelectedIndex = 2;
			dateSelector.SelectedIndex = 0;
			dateTimePicker.MaxDate = DateTime.Today.AddDays(1);
			pageLabel.Text = string.Empty;

			DefaultControl = findBox;
			ElevatedWithOneNote = true;

			resultsView.CardActivated += OnCardActivated;
			resultsView.HitActivated += OnHitActivated;
			resultsView.CheckedChanged += OnCheckedChanged;
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
			moveButton.Visible = copyButton.Visible = indexButton.Visible = false;

			// Build the regex once for the entire search; all scope methods share it
			var finder = new TextMatchBuilder(regBox.Checked, matchBox.Checked)
				.BuildRegex(findBox.Text);

			if (finder is null)
			{
				// invalid regex — ChangedText should have caught this, but guard anyway
				RestoreControls();
				return;
			}

			lastFinder = finder;

			// async void must not let exceptions escape to the SynchronizationContext,
			// which under the dllhost surrogate can take down OneNote
			try
			{
				source = new CancellationTokenSource();

				var options = new SearchOptions
				{
					IncludeToc = includeTocBox.Checked,
					DateFilter = (DateFilterMode)dateSelector.SelectedIndex,
					DateValue = dateTimePicker.Value.Date
				};

				void OnTotal(int count) => SetupProgressBar(count);
				void OnPage(string name) { progressBar.Increment(1); pageLabel.Text = name; }
				void OnFound(string pageId, string groupTitle, string color, IList<SearchHit> hits)
					=> AddPageResults(pageId, groupTitle, color, hits);

				var engine = new SearchEngine(options, source.Token, OnTotal, OnPage, OnFound);

				await using var one = new OneNote();

				if (scopeBox.SelectedIndex == 0)
				{
					await engine.SearchNotebook(one, finder);
				}
				else if (scopeBox.SelectedIndex == 1)
				{
					var section = await one.GetSection();
					await engine.SearchSection(one, section, finder);
				}
				else
				{
					logger.StartClock();
					var page = await one.GetPage(one.CurrentPageId, OneNote.PageDetail.Basic);
					logger.WriteTime("loaded page", keepRunning: true);
					await engine.SearchPage(page, finder);
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
			indexButton.Visible = moveButton.Visible = copyButton.Visible = resultsView.HasHits;
			resultsHeaderPanel.Visible = resultsView.HasHits;

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
			await NavigateTo(e.PageId, string.Empty, e.NewWindow);
		}


		private async void OnHitActivated(object sender, NavigateHitEventArgs e)
		{
			await NavigateTo(e.PageId, e.ObjectId, e.NewWindow);
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
