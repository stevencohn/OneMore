//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Helpers.Office;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using static River.OneMoreAddIn.OneNote;
	using Resx = Properties.Resources;


	internal partial class ImportOutlookTasksDialog : UI.MoreForm
	{
		private UI.MoreCheckList taskList;
		private OneNote one;
		private Outlook outlook;


		public ImportOutlookTasksDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.ImportOutlookTasksDialog_Text;

				Localize(new string[]
				{
					"introBox",
					"selectAllLink=phrase_SelectAll",
					"selectNoneLink=phrase_SelectNone",
					"listButton",
					"tableButton",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			resetInfoLabel.Visible = false;
			resetInfoLabel.Left = resetLabel.Left;

			taskList = new UI.MoreCheckList
			{
				Dock = DockStyle.Fill,
				HeaderStyle = ColumnHeaderStyle.None,
				MultiSelect = false,
				SelectedBackColorKey = "LinkHighlight",
				SelectedForeColorKey = "ControlText",
				CanToggleItem = CanToggleTask,
				GetCellStyle = GetTaskCellStyle
			};

			taskList.Columns.Add(string.Empty);
			taskList.SetColumnProportions(1f);
			taskList.CheckChanged += (s, e) => UpdateOkButtonState();
			taskPanel.Controls.Add(taskList);

			okButton.Enabled = false;
		}


		public ImportOutlookTasksDialog(OutlookTaskFolders folders)
			: this()
		{
			PopulateList(folders);
		}


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			AlignLinks();
		}


		private void AlignLinks()
		{
			var noneSize = TextRenderer.MeasureText(selectNoneLink.Text, selectNoneLink.Font);
			selectNoneLink.Left = linksPanel.Width - noneSize.Width;

			var sepSize = TextRenderer.MeasureText(sep1.Text, sep1.Font);
			sep1.Left = selectNoneLink.Left - selectNoneLink.Margin.Left - sepSize.Width - sep1.Margin.Right;

			var allSize = TextRenderer.MeasureText(selectAllLink.Text, selectAllLink.Font);
			selectAllLink.Left = sep1.Left - sep1.Margin.Left - allSize.Width - selectAllLink.Margin.Right;
		}


		private static bool CanToggleTask(ListViewItem item) =>
			item.Tag is OutlookTask task && string.IsNullOrEmpty(task.OneNoteTaskID);


		private static UI.MoreListView.CellStyle GetTaskCellStyle(ListViewItem item, int column) =>
			item.Tag is OutlookTask task && !string.IsNullOrEmpty(task.OneNoteTaskID)
				? new UI.MoreListView.CellStyle(0, false, "GrayText")
				: UI.MoreListView.CellStyle.Default;


		private void PopulateList(OutlookTaskFolders folders)
		{
			var root = folders.FirstOrDefault();
			if (root == null)
			{
				return;
			}

			taskList.BeginUpdate();

			foreach (var task in root.Tasks)
			{
				taskList.Items.Add(new ListViewItem(task.Subject)
				{
					Tag = task,
					Checked = !string.IsNullOrEmpty(task.OneNoteTaskID)
				});
			}

			taskList.EndUpdate();

			UpdateOkButtonState();
		}


		private void UpdateOkButtonState()
		{
			okButton.Enabled = taskList.Items.Cast<ListViewItem>()
				.Any(i => i.Checked && i.Tag is OutlookTask task && string.IsNullOrEmpty(task.OneNoteTaskID));
		}


		public IEnumerable<OutlookTask> SelectedTasks =>
			taskList.Items.Cast<ListViewItem>()
				.Where(i => i.Checked && i.Tag is OutlookTask task && string.IsNullOrEmpty(task.OneNoteTaskID))
				.Select(i => (OutlookTask)i.Tag)
				.ToList();


		public bool ShowDetailedTable => tableButton.Checked;


		private void SelectAllTasks(object sender, LinkLabelLinkClickedEventArgs e)
		{
			foreach (ListViewItem item in taskList.Items)
			{
				if (item.Tag is OutlookTask task && string.IsNullOrEmpty(task.OneNoteTaskID))
				{
					item.Checked = true;
				}
			}

			taskList.Invalidate();
			UpdateOkButtonState();
		}


		private void SelectNoneTasks(object sender, LinkLabelLinkClickedEventArgs e)
		{
			foreach (ListViewItem item in taskList.Items)
			{
				if (item.Tag is OutlookTask task && string.IsNullOrEmpty(task.OneNoteTaskID))
				{
					item.Checked = false;
				}
			}

			taskList.Invalidate();
			UpdateOkButtonState();
		}


		private async void ResetOrphanedTasks(object sender, LinkLabelLinkClickedEventArgs e)
		{
			await using (one = new OneNote())
			{
				using var source = new CancellationTokenSource();

				var map = await new HyperlinkProvider(one)
					.BuildHyperlinkMap(Scope.Sections, source.Token);

				var count = 0;
				using (outlook = new Outlook())
				{
					count = await ResetOrphanedTasks(map);
				}

				if (count > 0)
				{
					UpdateOkButtonState();
				}

				resetLabel.Visible = false;
				resetInfoLabel.Visible = true;

				resetInfoLabel.Text = count == 0
					? Resx.ImportOutlookTasksDialog_noorphans
					: string.Format(Resx.ImportOutlookTasksDialog_reset, count);
			}
		}


		private async Task<int> ResetOrphanedTasks(Dictionary<string, HyperlinkInfo> map)
		{
			var count = 0;

			foreach (var item in taskList.Items.Cast<ListViewItem>())
			{
				if (item.Tag is not OutlookTask task || string.IsNullOrEmpty(task.OneNoteURL))
				{
					continue;
				}

				var key = HyperlinkProvider.GetHyperKey(task.OneNoteURL, out _);
				if (!map.ContainsKey(key))
				{
					continue;
				}

				var page = await one.GetPage(map[key].PageID, PageDetail.Basic);
				if (page == null)
				{
					ResetTask(item, task);
					count++;
				}
				else if (!page.Root.Descendants(page.Namespace + "OutlookTask")
					.Any(x => x.Attribute("guidTask").Value == task.OneNoteTaskID))
				{
					ResetTask(item, task);
					count++;
				}
			}

			return count;
		}


		private void ResetTask(ListViewItem item, OutlookTask task)
		{
			task.OneNoteTaskID = null;
			task.OneNoteURL = null;
			task.OneNotePageID = null;
			task.OneNoteObjectID = null;

			outlook.SaveTask(task);

			item.Checked = true;
			taskList.Invalidate(item.Bounds);
		}
	}
}
