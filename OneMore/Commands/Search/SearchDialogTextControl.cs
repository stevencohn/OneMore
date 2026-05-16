//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

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
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal partial class SearchDialogTextControl : MoreUserControl
	{

		private sealed class SearchHit
		{
			public string PlainText { get; set; }
			public string Hyperlink { get; set; }
		}

		// One entry per visible row in the results ListView.
		// Groups are section headers; hits are navigable matches.
		private sealed class ResultItem
		{
			public bool IsGroup { get; set; }
			public string Text { get; set; }
			public Color SectionColor { get; set; }     // group only: left swatch color
			public Color SectionBackColor { get; set; }  // group only: row background tint
			public string Hyperlink { get; set; }        // hit only
		}


		//private const int CreatedAfter = 1;
		private const int CreatedBefore = 2;
		private const int UpdatedAfter = 3;
		private const int UpdatedBefore = 4;

		// Matches the original HighlightBackground / HighlightForeground from the Designer
		private static readonly Color SelectionBack = Color.FromArgb(215, 193, 255);
		private static readonly Color SelectionFore = SystemColors.HighlightText;

		private readonly ILogger logger;
		private readonly Regex cleaner;
		private CancellationTokenSource source;
		private bool grouping;

		private readonly List<ResultItem> results = new();
		private Font groupFont;
		private Font hitFont;
		private SolidBrush hitBackBrush;       // unselected hit row background
		private SolidBrush selectionBackBrush; // selected hit row background


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
					"cancelButton=word_Cancel"
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
			//
			// NOTE, instead of ignoring escape sequences, use WebUtility.HtmlDecode(input);
			//
			//
			cleaner = new Regex(
				@"(?:<\s*(?:span|a)[^>]*?>)|(?:</(?:span|a)>)|(?:&#\d+;)",
				RegexOptions.Compiled);

			groupFont = new Font("Segoe UI", 8.5f, FontStyle.Bold, GraphicsUnit.Point);
			hitFont = new Font("Segoe UI", 8.5f, FontStyle.Regular, GraphicsUnit.Point);
			selectionBackBrush = new SolidBrush(SelectionBack);

			// Wire virtual-mode events now so they are ready before first show
			resultsView.RetrieveVirtualItem += OnRetrieveVirtualItem;
			resultsView.DrawItem += OnDrawItem;
			resultsView.DrawSubItem += OnDrawSubItem;
			resultsView.DrawColumnHeader += (s, e) => e.DrawDefault = true;
			resultsView.MouseClick += OnResultsMouseClick;

			Disposed += (_, _) =>
			{
				groupFont?.Dispose();
				hitFont?.Dispose();
				hitBackBrush?.Dispose();
				selectionBackBrush?.Dispose();
			};
		}


		public event EventHandler<SearchCloseEventArgs> SearchClosing;


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

			// BackColor comes from the theme; create the brush now that it is known
			resultsView.BackColor = manager.GetColor("ListView");
			hitBackBrush = new SolidBrush(resultsView.BackColor);

			hitColumn.Width = resultsView.Width - SystemInformation.VerticalScrollBarWidth;

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


		private void ResizeResultsView(object sender, EventArgs e)
		{
			if (sender is ListView view)
			{
				hitColumn.Width = view.Width - SystemInformation.VerticalScrollBarWidth;
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
			nextButton.Visible = prevButton.Visible = results.Any(r => !r.IsGroup);
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Results list management

		private void ClearResults()
		{
			results.Clear();
			resultsView.VirtualListSize = 0;
		}


		/// <summary>
		/// Appends a section group header (when groupTitle is non-null) followed by
		/// all hits for that page. Updating VirtualListSize once per page batch keeps
		/// the number of repaints proportional to pages searched, not matches found.
		/// </summary>
		private void AddPageResults(string groupTitle, string sectionColor, IList<SearchHit> hits)
		{
			if (groupTitle is not null)
			{
				var color = ColorHelper.FromHtml(sectionColor);
				var bg = resultsView.BackColor;

				// 15% section color blended with the list background for the row tint
				var tinted = Color.FromArgb(
					(color.R * 15 + bg.R * 85) / 100,
					(color.G * 15 + bg.G * 85) / 100,
					(color.B * 15 + bg.B * 85) / 100);

				results.Add(new ResultItem
				{
					IsGroup = true,
					Text = groupTitle,
					SectionColor = color,
					SectionBackColor = tinted
				});
			}

			foreach (var hit in hits)
			{
				results.Add(new ResultItem
				{
					Text = hit.PlainText,
					Hyperlink = hit.Hyperlink
				});
			}

			resultsView.VirtualListSize = results.Count;
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Virtual ListView rendering

		private void OnRetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
		{
			if (e.ItemIndex >= results.Count) return;
			var result = results[e.ItemIndex];

			// The item text and font are used by the ListView for keyboard search and
			// accessibility; actual painting happens in OnDrawItem / OnDrawSubItem.
			e.Item = new ListViewItem(result.Text)
			{
				Font = result.IsGroup ? groupFont : hitFont
			};
		}


		private void OnDrawItem(object sender, DrawListViewItemEventArgs e)
		{
			if (e.ItemIndex >= results.Count) return;

			var result = results[e.ItemIndex];
			var selected = (e.State & ListViewItemStates.Selected) != 0;

			if (result.IsGroup)
			{
				using var bgBrush = new SolidBrush(result.SectionBackColor);
				e.Graphics.FillRectangle(bgBrush, e.Bounds);
			}
			else if (selected)
			{
				e.Graphics.FillRectangle(selectionBackBrush, e.Bounds);
			}
			else
			{
				e.Graphics.FillRectangle(hitBackBrush, e.Bounds);
			}

			if ((e.State & ListViewItemStates.Focused) != 0)
			{
				e.DrawFocusRectangle();
			}
		}


		private void OnDrawSubItem(object sender, DrawListViewSubItemEventArgs e)
		{
			if (e.ItemIndex >= results.Count) return;

			var result = results[e.ItemIndex];

			// MultiSelect = false so there is at most one selected index
			var selected = resultsView.SelectedIndices.Count > 0
				&& resultsView.SelectedIndices[0] == e.ItemIndex;

			const TextFormatFlags flags =
				TextFormatFlags.VerticalCenter |
				TextFormatFlags.EndEllipsis |
				TextFormatFlags.NoPrefix |
				TextFormatFlags.SingleLine;

			if (result.IsGroup)
			{
				// Colored swatch on the left edge indicating the section's accent color
				const int pad = 4;
				const int swatchW = 8;
				var swatch = new Rectangle(
					e.Bounds.X + pad, e.Bounds.Y + pad,
					swatchW, e.Bounds.Height - pad * 2);
				using var swatchBrush = new SolidBrush(result.SectionColor);
				e.Graphics.FillRectangle(swatchBrush, swatch);

				var textRect = new Rectangle(
					swatch.Right + pad * 2, e.Bounds.Y,
					e.Bounds.Width - swatch.Right - pad * 2, e.Bounds.Height);
				TextRenderer.DrawText(e.Graphics, result.Text, groupFont, textRect, ForeColor, flags);
			}
			else
			{
				var foreColor = selected ? SelectionFore : ForeColor;
				var textRect = new Rectangle(
					e.Bounds.X + 4, e.Bounds.Y, e.Bounds.Width - 4, e.Bounds.Height);
				TextRenderer.DrawText(e.Graphics, result.Text, hitFont, textRect, foreColor, flags);
			}
		}


		private async void OnResultsMouseClick(object sender, MouseEventArgs e)
		{
			var hit = resultsView.HitTest(e.X, e.Y);
			if (hit.Item is null || hit.Item.Index >= results.Count) return;

			var result = results[hit.Item.Index];
			if (result.IsGroup) return;

			await NavigateTo(result);
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
			else if (e.KeyCode == Keys.Enter && e.Modifiers == Keys.None
				&& resultsView.SelectedIndices.Count > 0)
			{
				var index = resultsView.SelectedIndices[0];
				if (index < results.Count && !results[index].IsGroup)
				{
					NavigateTo(results[index]);
				}

				e.Handled = true;
			}
		}


		private void MoveToPreviousSelection(object sender, EventArgs e) => MoveTo(-1);
		private void MoveToNextSelection(object sender, EventArgs e) => MoveTo(1);


		private async void MoveTo(int delta)
		{
			if (results.Count == 0) return;

			// Start from the current selection, or from the appropriate edge when nothing is selected
			var current = resultsView.SelectedIndices.Count > 0
				? resultsView.SelectedIndices[0]
				: (delta > 0 ? -1 : results.Count);

			// Walk in the requested direction, skipping group header rows
			var i = current;
			var found = false;
			while (true)
			{
				i += delta;
				if (i < 0 || i >= results.Count) break;
				if (!results[i].IsGroup) { found = true; break; }
			}

			if (!found) return;

			resultsView.SelectedIndices.Clear();
			resultsView.SelectedIndices.Add(i);
			resultsView.EnsureVisible(i);
			resultsView.Focus();

			await NavigateTo(results[i]);
		}


		private async Task NavigateTo(ResultItem result)
		{
			await using var one = new OneNote();
			await one.NavigateTo(result.Hyperlink);
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

				var hits = await SearchPageBody(one, page, finder);

				// skip adding results if cancelled mid-page
				if (source.IsCancellationRequested) break;

				if (hits.Any())
				{
					AddPageResults(
						grouping ? $"{info.Path}/{pageName}" : null,
						info.Color,
						hits);
				}
			}
		}


		private async Task SearchPage(OneNote one, Page page, Regex finder)
		{
			var hits = await SearchPageBody(one, page, finder);
			if (hits.Any())
			{
				AddPageResults(null, null, hits);
			}
		}


		private async Task<IList<SearchHit>> SearchPageBody(OneNote one, Page page, Regex finder)
		{
			var ns = page.Namespace;

			// Materialize paragraphs on the UI thread before going to the thread pool,
			// so the lazy XLinq query is evaluated while we own the XDocument.
			var paragraphs = page.BodyOutlines
				.Descendants(ns + "OE")
				.Where(e => e.Elements(ns + "T").Any())
				.ToList();

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
				var results = new List<(string objectId, string text)>();
				foreach (var paragraph in paragraphs)
				{
					if (token.IsCancellationRequested) break;

					var text = GetRawText(paragraph, ns);
					if (text.Length > 0 && finder.IsMatch(text))
					{
						results.Add((paragraph.Attribute("objectID").Value, text));
					}
				}
				return results;
			});

			// Back on the UI thread — resolve hyperlinks via COM.
			var hits = new List<SearchHit>(matched.Count);
			foreach (var (objectId, text) in matched)
			{
				hits.Add(new SearchHit
				{
					PlainText = text,
					Hyperlink = one.GetHyperlink(page.PageId, objectId)
				});
			}
			return hits;
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
