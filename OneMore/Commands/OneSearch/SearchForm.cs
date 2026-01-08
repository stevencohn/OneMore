//************************************************************************************************
// OneSearch UI
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.OneSearch
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.IO;
	using System.Windows.Forms;


	internal sealed class OneSearchForm : Form
	{
		private readonly OneSearchService service;
		private readonly SearchEngine searchEngine;
		private OneSearchSettings settings;
		private const string ResultsLabel = "\u7ed3\u679c";

		private TextBox queryBox;
		private ComboBox scopeBox;
		private CheckBox regexBox;
		private CheckBox caseBox;
		private TextBox cacheBox;
		private ListView resultsView;
		private Label statusLabel;


		public OneSearchForm(OneSearchService service)
		{
			this.service = service;
			searchEngine = new SearchEngine();
			settings = service.LoadSettings();

			InitializeComponent();
			ApplySettings();
		}


		private void InitializeComponent()
		{
			Text = "OneSearch";
			Size = new Size(860, 560);
			MinimumSize = new Size(700, 420);

			var layout = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 1,
				RowCount = 4
			};
			layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
			Controls.Add(layout);

			var searchRow = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill,
				AutoSize = true,
				FlowDirection = FlowDirection.LeftToRight,
				WrapContents = false,
				Padding = new Padding(8, 8, 8, 4)
			};
			searchRow.Controls.Add(new Label
			{
				Text = "\u641c\u7d22",
				AutoSize = true,
				TextAlign = ContentAlignment.MiddleLeft,
				Margin = new Padding(0, 6, 8, 0)
			});

			queryBox = new TextBox
			{
				Width = 420
			};
			queryBox.KeyDown += (sender, args) =>
			{
				if (args.KeyCode == Keys.Enter)
				{
					args.SuppressKeyPress = true;
					RunSearch();
				}
			};
			searchRow.Controls.Add(queryBox);

			var searchButton = new Button
			{
				Text = "\u641c\u7d22",
				AutoSize = true,
				Margin = new Padding(8, 0, 0, 0)
			};
			searchButton.Click += (sender, args) => RunSearch();
			searchRow.Controls.Add(searchButton);

			layout.Controls.Add(searchRow, 0, 0);

			var optionsRow = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill,
				AutoSize = true,
				FlowDirection = FlowDirection.LeftToRight,
				WrapContents = false,
				Padding = new Padding(8, 4, 8, 4)
			};
			optionsRow.Controls.Add(new Label
			{
				Text = "\u8303\u56f4",
				AutoSize = true,
				TextAlign = ContentAlignment.MiddleLeft,
				Margin = new Padding(0, 6, 8, 0)
			});

			scopeBox = new ComboBox
			{
				DropDownStyle = ComboBoxStyle.DropDownList,
				Width = 160
			};
			scopeBox.Items.AddRange(new object[]
			{
				new ScopeItem(SearchScope.CurrentPage, "\u5f53\u524d\u9875\u9762"),
				new ScopeItem(SearchScope.CurrentSection, "\u5f53\u524d\u5206\u533a"),
				new ScopeItem(SearchScope.CurrentNotebook, "\u5f53\u524d\u7b14\u8bb0\u672c"),
				new ScopeItem(SearchScope.AllNotebooks, "\u5168\u90e8\u7b14\u8bb0\u672c")
			});
			optionsRow.Controls.Add(scopeBox);

			regexBox = new CheckBox
			{
				Text = "\u6b63\u5219",
				AutoSize = true,
				Margin = new Padding(12, 4, 0, 0)
			};
			optionsRow.Controls.Add(regexBox);

			caseBox = new CheckBox
			{
				Text = "\u533a\u5206\u5927\u5c0f\u5199",
				AutoSize = true,
				Margin = new Padding(8, 4, 0, 0)
			};
			optionsRow.Controls.Add(caseBox);

			var syncButton = new Button
			{
				Text = "\u540c\u6b65",
				AutoSize = true,
				Margin = new Padding(12, 0, 0, 0)
			};
			syncButton.Click += (sender, args) => RunSync();
			optionsRow.Controls.Add(syncButton);

			var clearButton = new Button
			{
				Text = "\u6e05\u9664\u7f13\u5b58",
				AutoSize = true,
				Margin = new Padding(8, 0, 0, 0)
			};
			clearButton.Click += (sender, args) => RunClearCache();
			optionsRow.Controls.Add(clearButton);

			layout.Controls.Add(optionsRow, 0, 1);

			var cacheRow = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill,
				AutoSize = true,
				FlowDirection = FlowDirection.LeftToRight,
				WrapContents = false,
				Padding = new Padding(8, 4, 8, 4)
			};
			cacheRow.Controls.Add(new Label
			{
				Text = "\u7f13\u5b58\u76ee\u5f55",
				AutoSize = true,
				TextAlign = ContentAlignment.MiddleLeft,
				Margin = new Padding(0, 6, 8, 0)
			});

			cacheBox = new TextBox
			{
				Width = 420
			};
			cacheRow.Controls.Add(cacheBox);

			var browseButton = new Button
			{
				Text = "\u6d4f\u89c8",
				AutoSize = true,
				Margin = new Padding(8, 0, 0, 0)
			};
			browseButton.Click += (sender, args) => BrowseCacheFolder();
			cacheRow.Controls.Add(browseButton);

			layout.Controls.Add(cacheRow, 0, 2);

			var resultsPanel = new Panel
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(8, 4, 8, 8)
			};

			resultsView = new ListView
			{
				Dock = DockStyle.Fill,
				View = View.Details,
				FullRowSelect = true,
				MultiSelect = false
			};
			resultsView.Columns.Add("\u6807\u9898", 200);
			resultsView.Columns.Add("\u5206\u533a", 160);
			resultsView.Columns.Add("\u7b14\u8bb0\u672c", 160);
			resultsView.Columns.Add("\u6587\u672c\u7248\u6bb5", 300);
			resultsView.DoubleClick += (sender, args) => NavigateToSelected();
			resultsPanel.Controls.Add(resultsView);

			statusLabel = new Label
			{
				Dock = DockStyle.Bottom,
				Height = 20,
				TextAlign = ContentAlignment.MiddleLeft
			};
			resultsPanel.Controls.Add(statusLabel);

			layout.Controls.Add(resultsPanel, 0, 3);
		}


		private void ApplySettings()
		{
			queryBox.Text = string.Empty;
			cacheBox.Text = settings.CacheRoot ?? string.Empty;
			regexBox.Checked = settings.UseRegex;
			caseBox.Checked = settings.CaseSensitive;
			SelectScope(settings.DefaultScope);
		}


		private void SelectScope(SearchScope scope)
		{
			foreach (var item in scopeBox.Items)
			{
				if (item is ScopeItem scopeItem && scopeItem.Scope == scope)
				{
					scopeBox.SelectedItem = item;
					return;
				}
			}

			if (scopeBox.Items.Count > 0)
			{
				scopeBox.SelectedIndex = 0;
			}
		}


		private SearchScope GetSelectedScope()
		{
			if (scopeBox.SelectedItem is ScopeItem scopeItem)
			{
				return scopeItem.Scope;
			}

			return SearchScope.AllNotebooks;
		}


		private void RunSearch()
		{
			SaveSettings();

			var context = service.GetContext();
			var query = new SearchQuery
			{
				Text = queryBox.Text,
				UseRegex = regexBox.Checked,
				CaseSensitive = caseBox.Checked,
				Scope = GetSelectedScope(),
				CacheRoot = cacheBox.Text,
				CurrentPageId = context.CurrentPageId,
				CurrentSectionId = context.CurrentSectionId,
				CurrentNotebookId = context.CurrentNotebookId
			};

			if (string.IsNullOrWhiteSpace(query.CacheRoot) || !Directory.Exists(query.CacheRoot))
			{
				MessageBox.Show("\u7f13\u5b58\u76ee\u5f55\u4e0d\u5b58\u5728\uff0c\u8bf7\u5148\u540c\u6b65");
				return;
			}

			if (query.Scope == SearchScope.CurrentPage && string.IsNullOrEmpty(query.CurrentPageId))
			{
				MessageBox.Show("\u65e0\u6cd5\u83b7\u53d6\u5f53\u524d\u9875\u9762");
				return;
			}

			if (query.Scope == SearchScope.CurrentSection && string.IsNullOrEmpty(query.CurrentSectionId))
			{
				MessageBox.Show("\u65e0\u6cd5\u83b7\u53d6\u5f53\u524d\u5206\u533a");
				return;
			}

			if (query.Scope == SearchScope.CurrentNotebook && string.IsNullOrEmpty(query.CurrentNotebookId))
			{
				MessageBox.Show("\u65e0\u6cd5\u83b7\u53d6\u5f53\u524d\u7b14\u8bb0\u672c");
				return;
			}

			List<SearchResult> results;
			try
			{
				results = searchEngine.Search(query);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				return;
			}

			PopulateResults(results);
		}


		private void PopulateResults(List<SearchResult> results)
		{
			resultsView.BeginUpdate();
			resultsView.Items.Clear();

			foreach (var result in results)
			{
				var item = new ListViewItem(result.PageTitle ?? string.Empty);
				item.SubItems.Add(result.SectionName ?? string.Empty);
				item.SubItems.Add(result.NotebookName ?? string.Empty);
				item.SubItems.Add(result.Snippet ?? string.Empty);
				item.Tag = result;
				resultsView.Items.Add(item);
			}

			resultsView.EndUpdate();
			statusLabel.Text = $"{ResultsLabel}: {results.Count}";
		}


		private void NavigateToSelected()
		{
			if (resultsView.SelectedItems.Count == 0)
			{
				return;
			}

			if (!(resultsView.SelectedItems[0].Tag is SearchResult result))
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(result.PageId))
			{
				MessageBox.Show("\u65e0\u6cd5\u8df3\u8f6c");
				return;
			}

			try
			{
				service.NavigateTo(result.PageId);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}


		private void RunSync()
		{
			SaveSettings();

			try
			{
				if (string.IsNullOrWhiteSpace(settings.CacheRoot))
				{
					MessageBox.Show("\u8bf7\u8bbe\u7f6e\u7f13\u5b58\u76ee\u5f55");
					return;
				}

				Directory.CreateDirectory(settings.CacheRoot);

				StaTask.Run(() =>
				{
					using var dialog = new OneSearchSyncDialog(service, settings)
					{
						StartPosition = FormStartPosition.CenterScreen
					};
					dialog.ShowDialog();
					return 0;
				}).GetAwaiter().GetResult();

				settings = service.LoadSettings();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}


		private void RunClearCache()
		{
			SaveSettings();

			var confirm = MessageBox.Show("\u786e\u5b9a\u6e05\u9664\u5168\u90e8\u7f13\u5b58\u5417?", "\u6e05\u9664\u7f13\u5b58", MessageBoxButtons.YesNo);
			if (confirm != DialogResult.Yes)
			{
				return;
			}

			try
			{
				service.ClearCache(settings.CacheRoot);
				resultsView.Items.Clear();
				statusLabel.Text = $"{ResultsLabel}: 0";
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}


		private void BrowseCacheFolder()
		{
			using (var dialog = new FolderBrowserDialog())
			{
				dialog.SelectedPath = cacheBox.Text;
				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					cacheBox.Text = dialog.SelectedPath;
					SaveSettings();
				}
			}
		}


		private void SaveSettings()
		{
			settings.CacheRoot = cacheBox.Text;
			settings.UseRegex = regexBox.Checked;
			settings.CaseSensitive = caseBox.Checked;
			settings.DefaultScope = GetSelectedScope();
			service.SaveSettings(settings);
		}


		private sealed class ScopeItem
		{
			public ScopeItem(SearchScope scope, string label)
			{
				Scope = scope;
				Label = label;
			}

			public SearchScope Scope { get; }
			public string Label { get; }

			public override string ToString() => Label;
		}
	}
}
