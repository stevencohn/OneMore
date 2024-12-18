//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Globalization;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class HashtagDialog : MoreForm
	{
		private const string SettingsKey = "Hashtags";

		private const string T0 = "0001-01-01T00:00:00.0000Z";

		private readonly string moreID;
		private readonly MoreAutoCompleteList palette;
		private readonly bool experimental;
		private readonly List<HashtagContext> selections;


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
				Text = Resx.HashtagDialog_Title;

				Localize(new string[]
				{
					"introLabel",
					"scopeBox",
					"checkAllLink",
					"uncheckAllLink",
					"scanButton",
					"scanLink",
					"scheduleButton",
					"indexButton=word_Index",
					"moveButton=word_Move",
					"copyButton=word_Copy",
					"cancelButton=word_Cancel"
				});

			}

			DefaultControl = tagBox;

			selections = new List<HashtagContext>();

			palette = new MoreAutoCompleteList
			{
				FreeText = true,
				RecentKicker = Resx.HashtagDialog_recentTags,
				OtherKicker = Resx.HashtagDialog_allTags,
				WordChars = new[] { '#' }
			};

			palette.SetAutoCompleteList(tagBox);
			scopeBox.SelectedIndex = 0;

			searchButton.NotifyDefault(true);

			var setprovider = new SettingsProvider();
			var general = setprovider.GetCollection(nameof(GeneralSheet));

			palette.NonsequentialMatching = general.Get<bool>("nonseqMatching");
			experimental = general.Get<bool>("experimental");

			scanLink.Left = lastScanLabel.Left;
			scanLink.Visible = false;

			ShowScanTimes();
			CheckForNewNotebooks();

			ShowOfflineNotebooks = setprovider
				.GetCollection(SettingsKey)
				.Get("showOffline", true);

			tooltip.SetToolTip(sensitiveBox, Resx.HashtagDialog_sensitiveTip);
		}


		public HashtagDialog(string moreID)
			: this()
		{
			this.moreID = moreID;

			if (string.IsNullOrEmpty(moreID))
			{
				// if not moreID then page has not been scanned or does not have tags
				scopeBox.Items.RemoveAt(3);
			}
		}


		public Commands Command { get; private set; }


		public string Query { get; private set; }


		public IEnumerable<HashtagContext> SelectedPages => selections;


		public bool ShowOfflineNotebooks { get; private set; }


		private void CheckForNewNotebooks()
		{
			Task.Run(async () =>
			{
				await using var one = new OneNote();
				var books = await one.GetNotebooks();
				var ns = books.GetNamespaceOfPrefix(OneNote.Prefix);

				var bookIDs = books.Elements(ns + "Notebook")
					.Where(e => e.Attribute("isRecycleBin") is null)
					.Select(e => e.Attribute("ID").Value);

				var provider = new HashtagProvider();
				var known = provider.ReadKnownNotebooks();

				if (bookIDs.Any(e => !known.Contains(e)))
				{
					lastScanLabel.Visible = false;
					scanLink.Visible = true;
				}
			});
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

			lastScanLabel.Text = string.Format(
				Resx.HashtagDialog_lastScanLabel, lastScan, nextScan);
		}


		private void DoPopulateTags(object sender, EventArgs e)
		{
			Task.Run(async () => { await PopulateTags(sender, e); });
		}


		private async Task PopulateTags(object sender, EventArgs e)
		{
			await using var one = new OneNote();
			var provider = new HashtagProvider();

			var names = scopeBox.SelectedIndex switch
			{
				1 => provider.ReadTagNames(notebookID: one.CurrentNotebookId),
				2 => provider.ReadTagNames(sectionID: one.CurrentSectionId),
				3 => provider.ReadTagNames(moreID: moreID),
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


		private async void DoKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape && !palette.IsPopupVisible)
			{
				e.Handled = true;
				Close();
			}
			else if (e.KeyCode == Keys.Enter)
			{
				e.Handled = true;
				await SearchTags(sender, e);
			}
		}


		private async void DoSearchTags(object sender, EventArgs e)
		{
			await SearchTags(sender, e);
			//Task.Run(async () => { await SearchTags(sender, e); });
		}


		private async Task SearchTags(object sender, EventArgs e)
		{
			if (palette.IsPopupVisible)
			{
				palette.HidePopup(sender, e);
			}

			var where = tagBox.Text.Trim();
			if (where.IsNullOrEmpty())
			{
				return;
			}

			Query = where;

			await using var one = new OneNote();

			var loadedBookIDs = (await one.GetNotebooks()).Elements()
				.Select(e => e.Attribute("ID").Value).ToList();

			var provider = new HashtagProvider();
			string parsed;
			var cs = sensitiveBox.Checked;

			var tags = scopeBox.SelectedIndex switch
			{
				1 => provider.SearchTags(where, cs, out parsed, notebookID: one.CurrentNotebookId),
				2 => provider.SearchTags(where, cs, out parsed, sectionID: one.CurrentSectionId),
				3 => provider.SearchTags(where, cs, out parsed, moreID: moreID),
				_ => provider.SearchTags(where, cs, out parsed)
			};

			if (!ShowOfflineNotebooks)
			{
				// must be ToList?!
				var loaded = tags.Where(t => loadedBookIDs.Contains(t.NotebookID)).ToList();
				tags.Clear();
				tags.AddRange(loaded);
			}

			logger.Verbose($"found {tags.Count} tags using [{parsed}]");

			var width = contextPanel.ClientSize.Width -
				(contextPanel.Padding.Left + contextPanel.Padding.Right) * 2 -
				// make room for scrollbar, expecting it to be drawn
				SystemInformation.VerticalScrollBarWidth;

			if (tags.Any())
			{
				var items = CollateTags(tags, loadedBookIDs);
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
				// resize incase scrollbar isn't needed, use the space
				OnSizeChanged(e);
			}
			else
			{
				var control = new HashtagErrorControl(
					Resx.HashtagDialog_noResults, experimental ? parsed : null)
				{
					Width = width
				};

				contextPanel.SuspendLayout();
				contextPanel.Controls.Clear();
				contextPanel.Controls.Add(control);
				contextPanel.ResumeLayout();
				// resize incase scrollbar isn't needed, use the space
				OnSizeChanged(e);
			}
		}


		private void DoScheduleScan(object sender, EventArgs e)
		{
			var scheduler = new HashtagScheduler();

			using var dialog =
				scheduler.State == ScanningState.None ||
				scheduler.State == ScanningState.Ready
					? new ScheduleScanDialog(true)
					: new ScheduleScanDialog(true, scheduler.StartTime);

			if (scheduler.State != ScanningState.None &&
				scheduler.State != ScanningState.Ready)
			{
				dialog.SetIntroText(string.Format(
					Resx.HashtagSheet_prescheduled,
					scheduler.StartTime.ToString("ddd, MMMM d, yyyy h:mm tt"))
					);
			}
			else
			{
				dialog.SetIntroText(Resx.HashtagSheet_scanNotebooks);
			}

			dialog.SetPreferredIDs(scheduler.Notebooks);

			var result = dialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				scheduler.Notebooks = dialog.GetSelectedNotebooks();
				scheduler.StartTime = dialog.StartTime;
				scheduler.State = ScanningState.PendingScan;

				Task.Run(async () => { await scheduler.Activate(); });
			}
		}


		private void ScanDiscovered(object sender, LinkLabelLinkClickedEventArgs e)
		{
			DoScheduleScan(sender, e);
		}


		private void Control_Checked(object sender, EventArgs e)
		{
			var control = sender as HashtagContextControl;
			var context = (HashtagContext)control.Tag;
			var enabled = control.IsChecked;

			if (!enabled)
			{
				var index = selections.FindIndex(s => s.PageID == context.PageID);
				if (index >= 0)
				{
					selections.RemoveAt(index);
				}

				for (int i = 0; i < contextPanel.Controls.Count; i++)
				{
					if (contextPanel.Controls[i] is HashtagContextControl item && item.IsChecked)
					{
						enabled = true;
						break;
					}
				}
			}
			else if (!selections.Any(s => s.PageID == context.PageID))
			{
				selections.Add(context);
			}

			indexButton.Enabled = moveButton.Enabled = copyButton.Enabled = enabled;
		}


		private HashtagContexts CollateTags(Hashtags tags, IEnumerable<string> loadedNotebookIDs)
		{
			// transform Hashtags collection to HashtagContexts collection...

			var items = new HashtagContexts();

			// tags should be sorted by p.path, p.name so collate based on that assumption
			HashtagContext context = null;
			foreach (var tag in tags)
			{
				if (context == null || context.MoreID != tag.MoreID)
				{
					context = new HashtagContext(tag)
					{
						Available = loadedNotebookIDs.Contains(tag.NotebookID)
					};

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
			// update label BEFORE manually scanning to reflect best quess when service will run
			ShowScanTimes();

			using var scanner = new HashtagScanner();
			await scanner.Scan();
			scanner.Report();

			await PopulateTags(sender, e);
		}


		private void PrepareContextMenu(object sender, System.EventArgs e)
		{
			if (ShowOfflineNotebooks)
			{
				offlineNotebooksButton.Image = Resx.e_CheckMark;
				offlineNotebooksButton.Text = Resx.HashtagDialog_showOfflineMenuItem;
			}
			else
			{
				offlineNotebooksButton.Image = null;
				offlineNotebooksButton.Text = Resx.HashtagDialog_hideOfflineMenuItem;
			}
		}


		private void ToggleOfflineNotebooks(object sender, EventArgs e)
		{
			ShowOfflineNotebooks = !ShowOfflineNotebooks;
		}


		private void DoCancel(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}


		private void SaveSettings(object sender, FormClosingEventArgs e)
		{
			var provider = new SettingsProvider();
			var settings = provider.GetCollection(SettingsKey);
			settings.Add("showOffline", ShowOfflineNotebooks);

			if (settings.IsModified)
			{
				provider.SetCollection(settings);
				provider.Save();
			}
		}
	}
}
