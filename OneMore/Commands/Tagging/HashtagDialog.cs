//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Drawing;
	using System.Globalization;
	using System.Linq;
	using System.Windows.Forms;


	internal partial class HashtagDialog : LocalizableForm
	{
		private const string T0 = "0001-01-01T00:00:00.0000Z";

		private readonly MoreAutoCompleteList palette;
		private readonly bool experimental;


		public enum Commands
		{
			Index,
			Copy,
			Move
		}


		public HashtagDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				// ...
			}

			palette = new MoreAutoCompleteList
			{
				FreeText = true,
				RecentKicker = "recent tags",
				OtherKicker = "all tags",
				WordChars = new[] { '#' }
			};

			palette.SetAutoCompleteList(tagBox);
			scopeBox.SelectedIndex = 0;

			experimental = new SettingsProvider()
				.GetCollection("GeneralSheet").Get<bool>("experimental");

			ShowScanTimes();
		}


		public Commands Command { get; private set; }


		public IEnumerable<string> SelectedPages
		{
			get
			{
				for (var i = 0; i < contextPanel.Controls.Count; i++)
				{
					if (contextPanel.Controls[i] is HashtagContextControl item && item.IsChecked)
					{
						yield return item.PageID;
					}
				}
			}
		}


		private void ShowScanTimes()
		{
			var scan = new HashtagProvider().ReadScanTime();
			var lastScanTime = DateTime.Parse(scan, CultureInfo.InvariantCulture);
			var lastScan = lastScanTime.ToShortTimeString();

			var settings = new SettingsProvider();
			var collection = settings.GetCollection("HashtagSheet");
			var interval = collection.Get("interval", 2);
			var nextScan = lastScanTime.AddMinutes(interval).ToShortTimeString();

			lastScanLabel.Text = $"Last scan: {lastScan}, Next scan: {nextScan}";
		}


		private void PopulateTags(object sender, EventArgs e)
		{
			using var one = new OneNote();
			var provider = new HashtagProvider();

			var names = scopeBox.SelectedIndex switch
			{
				1 => provider.ReadTagNames(notebookID: one.CurrentNotebookId),
				2 => provider.ReadTagNames(sectionID: one.CurrentSectionId),
				_ => provider.ReadTagNames(),
			};

			var recent = scopeBox.SelectedIndex switch
			{
				1 => provider.ReadLatestTagNames(notebookID: one.CurrentNotebookId),
				2 => provider.ReadLatestTagNames(sectionID: one.CurrentSectionId),
				_ => provider.ReadLatestTagNames(),
			};

			logger.Verbose($"discovered {names.Count()} tags, {recent.Count()} mru");

			palette.LoadCommands(names.ToArray(), recent.ToArray());
		}


		private void DoKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape && !palette.IsPopupVisible)
			{
				e.Handled = true;
				Close();
			}
			else if (e.KeyCode == Keys.Enter)
			{
				e.Handled = true;
				SearchTags(sender, e);
			}
		}


		private void SearchTags(object sender, EventArgs e)
		{
			var where = tagBox.Text.Trim();
			if (where.IsNullOrEmpty())
			{
				return;
			}

			using var one = new OneNote();
			var provider = new HashtagProvider();
			string parsed;

			var tags = scopeBox.SelectedIndex switch
			{
				1 => provider.SearchTags(where, out parsed, notebookID: one.CurrentNotebookId),
				2 => provider.SearchTags(where, out parsed, sectionID: one.CurrentSectionId),
				_ => provider.SearchTags(where, out parsed)
			};

			logger.Verbose($"found {tags.Count} tags using [{parsed}]");

			var width = contextPanel.ClientSize.Width -
				(contextPanel.Padding.Left + contextPanel.Padding.Right) * 2 - 20;

			if (tags.Any())
			{
				var items = CollateTags(tags);
				tags.Clear();

				var controls = new HashtagContextControl[items.Count];

				for (var i = 0; i < items.Count; i++)
				{
					var control = new HashtagContextControl(items[i])
					{
						Width = width
					};

					control.Checked += Control_Checked;
					controls[i] = control;
				}

				contextPanel.SuspendLayout();
				contextPanel.Controls.Clear();
				contextPanel.Controls.AddRange(controls);
				contextPanel.ResumeLayout();
			}
			else
			{
				var control = new HashtagErrorControl(
					"No results found", experimental ? parsed : null)
				{
					Width = width
				};

				contextPanel.SuspendLayout();
				contextPanel.Controls.Clear();
				contextPanel.Controls.Add(control);
				contextPanel.ResumeLayout();
			}
		}


		private void Control_Checked(object sender, EventArgs e)
		{
			var control = sender as HashtagContextControl;
			var enabled = control.IsChecked;

			if (!enabled)
			{
				for (int i = 0; i < contextPanel.Controls.Count; i++)
				{
					if (contextPanel.Controls[i] is HashtagContextControl item && item.IsChecked)
					{
						enabled = true;
						break;
					}
				}
			}

			indexButton.Enabled = moveButton.Enabled = copyButton.Enabled = enabled;
		}


		private HashtagContexts CollateTags(Hashtags tags)
		{
			// transform Hashtags collection to HashtagContexts collection...

			var items = new HashtagContexts();

			// tags should be sorted by p.path, p.name so collate based on that assumption
			HashtagContext context = null;
			foreach (var tag in tags)
			{
				if (context == null || context.MoreID != tag.MoreID)
				{
					context = new HashtagContext(tag);
					items.Add(context);
				}
				else
				{
					// de-dupe the paragraphs; if there are multiple tags in one paragraph
					if (!context.HasSnippet(tag.ObjectID))
					{
						context.AddSnippet(tag);
					}
				}
			}

			return items;
		}


		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);

			var width = contextPanel.ClientSize.Width -
				(contextPanel.Padding.Left + contextPanel.Padding.Right) * 2;

			for (var i = 0; i < contextPanel.Controls.Count; i++)
			{
				contextPanel.Controls[i].Width = width;
			}
		}


		private void ToggleAllChecks(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var ticked = sender == checkAllLink;
			for (var i = 0; i < contextPanel.Controls.Count; i++)
			{
				if (contextPanel.Controls[i] is HashtagContextControl item)
				{
					item.IsChecked = ticked;
				}
			}
		}


		private void DoSomething(object sender, EventArgs e)
		{
			if (sender == indexButton)
			{
				Command = Commands.Index;
			}
			else if (sender == copyButton)
			{
				Command = Commands.Copy;
			}
			else
			{
				Command = Commands.Move;
			}

			DialogResult = DialogResult.OK;
			Close();
		}


		private void ShowMenu(object sender, EventArgs e)
		{
			var scanTime = new HashtagProvider().ReadScanTime();

			if (scanTime.CompareTo(T0) > 0)
			{
				contextMenu.Show(menuButton, new Point(
					-(contextMenu.Width - menuButton.Width),
					menuButton.Height));
			}
		}


		private async void ScanNow(object sender, EventArgs e)
		{
			// update label BEFORE manually scan to reflect best quess when service will run
			ShowScanTimes();

			using var scanner = new HashtagScanner();

			var clock = new Stopwatch();
			clock.Start();

			var (dirtyPages, totalPages) = await scanner.Scan();

			clock.Stop();
			var time = clock.ElapsedMilliseconds;
			logger.WriteLine($"scanned {totalPages} pages, updating {dirtyPages}, in {time}ms");

			PopulateTags(sender, e);
		}


		private void DoCancel(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
