﻿//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
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
			public string PageID { get; set; }
			public string ObjectID { get; set; }
		}

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
					"textLabel=word_Text",
					"matchBox",
					"regBox",
					"cancelButton=word_Cancel"
				});

				scopeBox.Items.Clear();
				scopeBox.Items.AddRange(Resx.SearchDialogText_scopeOptions.Split('\n'));
			}

			scopeBox.SelectedIndex = 2;
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
			logger.WriteLine("cancel");

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


		private void ChangeSelection(object sender, EventArgs e)
		{
			if (resultsView.SelectedItems.Count > 0)
			{
				nextButton.Enabled = prevButton.Enabled = true;
			}
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


		private void TogglerRegBox(object sender, EventArgs e)
		{
			searchButton.Enabled = !regBox.Checked;
			ChangedText(sender, e);
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private async void Search(object sender, EventArgs e)
		{
			findBox.Enabled = false;
			searchButton.Enabled = false;
			resultsView.SuspendLayout();
			if (resultsView.Items.Count > 0)
			{
				ClearResults();

				nextButton.Enabled = prevButton.Enabled = false;
			}

			await using var one = new OneNote();

			if (scopeBox.SelectedIndex == 0)
			{
				await SearchNotebook(one);
			}
			else if (scopeBox.SelectedIndex == 1)
			{
				grouping = true;
				var section = await one.GetSection();
				var ns = one.GetNamespace(section);
				SetupProgressBar(section.Descendants(ns + "Page").Count());
				await SearchSection(one, section);
			}
			else
			{
				var page = await one.GetPage(one.CurrentPageId, OneNote.PageDetail.Basic);
				await SearchPage(one, page);
			}

			source?.Dispose();
			source = null;

			pageLabel.Text = string.Empty;
			progressBar.Visible = false;
			resultsView.ResumeLayout(true);
			findBox.Enabled = true;
			searchButton.Enabled = true;

			findBox.Focus();
			searchButton.NotifyDefault(true);
		}


		private void ClearResults()
		{
			grouping = true;

			foreach (MoreHostedListViewItem item in resultsView.Items)
			{
				if (item.Control is MoreLinkLabel label)
				{
					label.LinkClicked -= NavigateToHit;
				}

				item.Control?.Dispose();
			}

			resultsView.Items.Clear();
		}


		private async Task SearchNotebook(OneNote one)
		{
			var notebook = await one.GetNotebook(OneNote.Scope.Pages);
			var ns = one.GetNamespace(notebook);

			SetupProgressBar(notebook.Descendants(ns + "Page").Count());
			await TraverseSections(notebook, string.Empty);

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
							PageID = page.PageId,
							ObjectID = paragraphID,
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
				var line = e.TextValue(true).Trim();
				if (line.Length > 0)
				{
					text = $"{text}{line} ";
				}
			});

			return cleaner.Replace(text.Trim(), string.Empty);
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
			var index = resultsView.SelectedIndices[0];
			if ((delta < 0 && index > 0) ||
				(delta > 0 && index < resultsView.Items.Count - 1))
			{
				var item = resultsView.Items[index] as MoreHostedListViewItem;
				item.Selected = false;
				var label = item.Control as MoreLinkLabel;
				label.Selected = false;

				item = resultsView.Items[index + delta] as MoreHostedListViewItem;
				item.Selected = true;
				item.EnsureVisible();

				label = item.Control as MoreLinkLabel;
				label.Selected = true;
				NavigateToHit(label, new LinkLabelLinkClickedEventArgs(label.Links[0]));
			}

			resultsView.Focus();
		}


		private void HandleNavKey(object sender, KeyEventArgs e)
		{
			if (resultsView.SelectedItems.Count > 0)
			{
				if (e.KeyCode == Keys.N && e.Modifiers == Keys.None)
				{
					MoveTo(1);
					e.Handled = true;
				}
				else if (e.KeyCode == Keys.P && e.Modifiers == Keys.None)
				{
					MoveTo(-1);
					e.Handled = true;
				}
			}
		}
	}
}
