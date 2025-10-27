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

		private sealed class SearchResultBuffer
		{
			private readonly List<SearchHit> hits = new();
			private readonly object gate = new();

			public int Count => hits.Count;

			public SearchHit Get(int index)
			{
				lock (gate)
				{
					return index < hits.Count ? hits[index] : null;
				}
			}

			public void AddRange(IEnumerable<SearchHit> hit)
			{
				lock (gate)
				{
					hits.AddRange(hit);
				}
			}
		}

		private sealed class SearchEngine
		{
			public event Action<List<SearchHit>> PageReady;

			public void StartSearch(string query)
			{
				Task.Run(() =>
				{
					for (int page = 0; page < 100; page++)
					{
						Thread.Sleep(300); // Simulate search delay
						var results = Enumerable.Range(page * 50, 50)
							.Select(i => new SearchHit { PlainText = $"Result {i}", Hyperlink = $"https://example.com/{i}" })
							.ToList();

						PageReady?.Invoke(results);
					}
				});
			}
		}

		private sealed class SearchCoordinator
		{
			private readonly SearchResultBuffer buffer;
			private readonly ListView listview;

			public SearchCoordinator(SearchResultBuffer buffer, ListView listView)
			{
				this.buffer = buffer;
				this.listview = listView;
			}

			public void Attach(SearchEngine engine)
			{
				engine.PageReady += results =>
				{
					buffer.AddRange(results);
					listview.Invoke(new Action(() =>
					{
						listview.VirtualListSize = buffer.Count;
						listview.Invalidate();
					}));
				};
			}
		}


		//private const int CreatedAfter = 1;
		private const int CreatedBefore = 2;
		private const int UpdatedAfter = 3;
		private const int UpdatedBefore = 4;

		private readonly ILogger logger;
		private readonly Regex cleaner;
		private CancellationTokenSource source;
		private bool grouping;

		private SearchResultBuffer buffer;


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
			cleaner = new Regex(
				@"(?:<\s*(?:span|a)[^>]*?>)|(?:</(?:span|a)>)|(?:&#\d+;)",
				RegexOptions.Compiled);
		}


		public event EventHandler<SearchCloseEventArgs> SearchClosing;


		private void Nevermind(object sender, EventArgs e)
		{
			if (source is not null)
			{
				logger.WriteLine("cancelling search");
				source.Cancel();
				return;
			}

			logger.WriteLine("closing search");
			SearchClosing?.Invoke(this, new(DialogResult.Cancel));
		}


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			resultsView.BackColor = manager.GetColor("ListView");

			var rowWidth = Width - SystemInformation.VerticalScrollBarWidth * 2;
			resultsView.Columns[0].Width = rowWidth;

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
			if (sender is MoreListView view)
			{
				var w = (int)(view.Width - SystemInformation.VerticalScrollBarWidth * 1.5);
				foreach (ColumnHeader column in view.Columns)
				{
					column.Width = w;
				}

				try
				{
					foreach (MoreHostedListViewItem hosted in view.Items)
					{
						if (hosted is not null)
						{
							hosted.Control.Width = w;
						}
					}
				}
				catch (Exception)
				{
					// swallow null reference after CancellationRequested
				}
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
			resultsView.SuspendLayout();

			ClearResults();
			nextButton.Visible = prevButton.Visible = false;

			await using var one = new OneNote();

			// search by scope...

			if (scopeBox.SelectedIndex == 0)
			{
				await SearchNotebook(one);
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
				await SearchSection(one, section);
			}
			else
			{
				logger.StartClock();
				grouping = false;

				var page = await one.GetPage(one.CurrentPageId, OneNote.PageDetail.Basic);
				logger.WriteTime("loaded page", keepRunning: true);

				await SearchPage(one, page);
				logger.WriteTime("search complete");
			}

			// restore controls...

			source?.Dispose();
			source = null;

			pageLabel.Text = string.Empty;
			progressBar.Visible = false;
			findBox.Enabled = true;
			searchButton.Enabled = true;

			resultsView.ResumeLayout();

			findBox.Focus();
			searchButton.NotifyDefault(true);

			nextButton.Visible = prevButton.Visible = resultsView.Items.Count > 0;
		}


		private void RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
		{
			var item = buffer.Get(e.ItemIndex);
			e.Item = new ListViewItem(item?.PlainText ?? "Loading...");
		}


		private void ClearResults()
		{
			foreach (MoreHostedListViewItem item in resultsView.Items)
			{
				if (item.Control is MoreLinkLabel label)
				{
					// detaches event handler to avoid memory leak
					label.Dispose();
				}
				else if (item.Control is SearchGroupControl group)
				{
					group.Dispose();
				}
			}

			resultsView.Items.Clear();
		}


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


		private async Task SearchNotebook(OneNote one)
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
					try
					{
						if (source?.IsCancellationRequested == true) { break; }
						await Task.Delay(10, source.Token);
					}
					catch (TaskCanceledException)
					{
						break;
					}

					await SearchSection(one, section);
				}

				var sectionGroups = parent.Elements(ns + "SectionGroup")
					.Where(e => e.Attribute("isRecycleBin") is null);

				foreach (var group in sectionGroups)
				{
					try
					{
						if (source?.IsCancellationRequested == true) { break; }
						await Task.Delay(10, source.Token);
					}
					catch (TaskCanceledException)
					{
						break;
					}

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


		private async Task SearchSection(OneNote one, XElement section)
		{
			var ns = one.GetNamespace(section);
			var sectionId = section.Attribute("ID").Value;
			var info = await one.GetSectionInfo(sectionId);

			var pageIds = section.Elements(ns + "Page")
				.Select(e => e.Attribute("ID").Value)
				.ToList();

			foreach (var pageId in pageIds)
			{
				try
				{
					if (source?.IsCancellationRequested == true) { break; }
					await Task.Delay(10, source.Token);
				}
				catch (TaskCanceledException)
				{
					break;
				}

				progressBar.Increment(1);

				var page = await one.GetPage(pageId, OneNote.PageDetail.Basic);
				var pageName = page.Root.Attribute("name").Value;

				if (grouping)
				{
					pageLabel.Text = pageName;
				}

				var hits = await SearchPageBody(one, page);
				if (hits.Any())
				{
					if (grouping)
					{
						var group = new SearchGroupControl(info.Color, $"{info.Path}/{pageName}");
						group.ApplyTheme(manager);

						resultsView.AddHostedItem(group);
					}

					foreach (var hit in hits)
					{
						AddResult(hit);
					}
				}
			}
		}


		private async Task SearchPage(OneNote one, Page page)
		{
			var hits = await SearchPageBody(one, page);
			if (hits.Any())
			{
				foreach (var hit in hits)
				{
					AddResult(hit);
				}
			}
		}


		private async Task<IList<SearchHit>> SearchPageBody(OneNote one, Page page)
		{
			var hits = new List<SearchHit>();

			var ns = page.Namespace;
			var paragraphs = page.BodyOutlines
				.Descendants(ns + "OE")
				.Where(e => e.Elements(ns + "T").Any());

			if (!paragraphs.Any())
			{
				return hits;
			}

			var builder = new TextMatchBuilder(regBox.Checked, matchBox.Checked);

			var finder = builder.BuildRegex(findBox.Text);
			//logger.WriteLine(finder.ToString());

			foreach (var paragraph in paragraphs)
			{
				try
				{
					if (source?.IsCancellationRequested == true) { break; }
					await Task.Yield();
				}
				catch (TaskCanceledException)
				{
					break;
				}

				var text = GetRawText(paragraph, ns);
				if (text.Length > 0)
				{
					//logger.WriteLine($"testing [{text}]");
					if (finder.IsMatch(text))
					{
						//logger.WriteLine("match");
						var paragraphID = paragraph.Attribute("objectID").Value;

						hits.Add(new SearchHit
						{
							PlainText = text,
							Hyperlink = one.GetHyperlink(page.PageId, paragraphID)
						});
					}
				}
			}

			return hits;
		}


		private string GetRawText(XElement paragraph, XNamespace ns)
		{
			var text = string.Empty;
			paragraph.Elements(ns + "T").ForEach(e =>
			{
				// custom cleaner regex adds filter for "&#nnn;" escapes, instead of TextValue
				//var line = e.TextValue(true).Trim();
				var line = cleaner.Replace(e.Value, string.Empty).Trim();
				if (line.Length > 0)
				{
					text = $"{text}{line} ";
				}
			});

			return text.Trim();
		}


		private void AddResult(SearchHit hit)
		{
			var link = new MoreLinkLabel
			{
				Text = hit.PlainText,
				Font = new("Segoe UI", 8.5f, FontStyle.Regular, GraphicsUnit.Point),
				Padding = new(0),
				Margin = new(0, 0, 0, 0),
				Width = resultsView.Width
			};

			link.Links.Add(new(0, 0, hit));
			link.LinkClicked += NavigateToHit;

			resultsView.AddHostedItem(link);
		}


		private async void NavigateToHit(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (e.Link.LinkData is SearchHit hit)
			{
				if (resultsView.SelectedItems.Count > 0)
				{
					var item = resultsView.SelectedItems[0] as MoreHostedListViewItem;
					item.Selected = false;

					var link = item.Control as MoreLinkLabel;
					link.Selected = false;
				}

				var label = sender as MoreLinkLabel;

				// Convert LinkLabel location to ListView client coordinates
				var relativePoint = resultsView.PointToClient(label.PointToScreen(Point.Empty));
				var info = resultsView.HitTest(relativePoint);
				if (info.Item is not null)
				{
					var item = info.Item;
					item.Selected = true;
					label.Selected = true;
				}

				await using var one = new OneNote();
				await one.NavigateTo(hit.Hyperlink);

				resultsView.Focus();
			}
		}


		private void MoveToPreviousSelection(object sender, EventArgs e)
		{
			MoveTo(-1);
		}

		private void MoveToNextSelection(object sender, EventArgs e)
		{
			MoveTo(1);
		}

		private void MoveTo(int delta)
		{
			MoreHostedListViewItem item = null;
			MoreLinkLabel label = null;

			if (resultsView.SelectedItems.Count == 0)
			{
				item = resultsView.Items[0] as MoreHostedListViewItem;
				item.Selected = true;
				label = item.Control as MoreLinkLabel;
				label.Selected = true;
				NavigateToHit(label, new LinkLabelLinkClickedEventArgs(label.Links[0]));
				return;
			}

			var index = resultsView.SelectedIndices[0];
			var found = false;
			var i = index;

			// find next or previous MoreLinkLabel item, skipping group headers...

			while (!found && (
				(delta < 0 && i > 0) ||
				(delta > 0 && i < resultsView.Items.Count - 1)))
			{
				i += delta;
				if (resultsView.Items[i] is MoreHostedListViewItem icast &&
					icast.Control is MoreLinkLabel lcast)
				{
					item = icast;
					label = lcast;
					found = true;
				}
			}

			if (found)
			{
				// found next or previous available item

				// deselect current
				var curItem = resultsView.Items[index] as MoreHostedListViewItem;
				curItem.Selected = false;
				var curLabel = curItem.Control as MoreLinkLabel;
				curLabel.Selected = false;

				// select new
				item.Selected = true;
				item.EnsureVisible();

				label.Selected = true;
				NavigateToHit(label, new LinkLabelLinkClickedEventArgs(label.Links[0]));
			}

			resultsView.Focus();
		}


		private void HandleNavKey(object sender, KeyEventArgs e)
		{
			if (resultsView.SelectedItems.Count > 0)
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
			}
		}
	}
}
