//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions

namespace River.OneMoreAddIn.Commands
{
	using Aga.Controls.Tree;
	using Aga.Controls.Tree.NodeControls;
	using River.OneMoreAddIn.Helpers.Office;
	using River.OneMoreAddIn.UI;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Threading;
	using System.Windows.Forms;
	using static River.OneMoreAddIn.OneNote;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class ImportOutlookTasksDialog : UI.LocalizableForm
	{
		private TreeModel model;
		private OneNote one;
		private Outlook outlook;


		#region class TaskNodeIcon
		private sealed class TreeNodeIcon : NodeIcon
		{
			private readonly Image task;
			private readonly Image opened;
			private readonly Image closed;

			public TreeNodeIcon()
			{
				task = Resx.Task;
				opened = Resx.FolderOpen;
				closed = Resx.FolderClose;
			}

			protected override Image GetIcon(TreeNodeAdv node)
			{
				if (node.Tag is Node subnode && subnode.Tag is OutlookTask)
					return task;
				else if (node.CanExpand && node.IsExpanded)
					return opened;
				else
					return closed;
			}
		}
		#endregion class TaskNodeIcon


		public ImportOutlookTasksDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.ImportOutlookTasksDialog_Text;

				Localize(new string[]
				{
					"introBox",
					"warningBox",
					"listButton",
					"tableButton",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}
			else
			{
				// customization only for English
				warningBox.Clear();
				warningBox.AppendText("Note that OneNote does not bind completely to task that " +
					"are not in the Outlook Tasks folder. Tasks from sub-folders are shown ");
				warningBox.AppendFormattedText("in red", Color.Firebrick);
				warningBox.AppendFormattedText(" to indicate that their status flags will not update " +
					"automatically after importing.", SystemColors.GrayText);
			}

			// prepare TreeViewAdv...

			var nodeCheckBox = new NodeCheckBox
			{
				DataPropertyName = "CheckState",
				EditEnabled = true,
				LeftMargin = 0
			};
			nodeCheckBox.CheckStateChanged += CheckStateChanged;
			tree.NodeControls.Add(nodeCheckBox);

			tree.NodeControls.Add(new TreeNodeIcon
			{
				LeftMargin = 1,
				ScaleMode = ImageScaleMode.Clip
			});

			var treeNodeTextBox = new NodeTextBox
			{
				DataPropertyName = "Text",
				LeftMargin = 3
			};
			treeNodeTextBox.DrawText += (sender, args) =>
			{
				if (args.Node.Level > 2)
					args.TextColor = Color.Firebrick;
			};
			tree.NodeControls.Add(treeNodeTextBox);

			model = new TreeModel();
			tree.Model = model;
		}


		public ImportOutlookTasksDialog(OutlookTaskFolders folders)
			: this()
		{
			PopulateTree(folders);
			tree.ExpandAll();
		}


		private void PopulateTree(OutlookTaskFolders folders, Node parent = null)
		{
			foreach (var folder in folders)
			{
				var node = new Node(folder.Name)
				{
					Tag = folder
				};

				PopulateTree(folder.Folders, node);

				foreach (var task in folder.Tasks)
				{
					var leaf = new Node(task.Subject);
					leaf.Tag = task;

					if (!string.IsNullOrEmpty(task.OneNoteTaskID))
					{
						leaf.IsChecked = true;
						leaf.IsEnabled = false;
					}

					node.Nodes.Add(leaf);
				}

				if (parent == null)
				{
					model.Nodes.Add(node);
				}
				else
				{
					parent.Nodes.Add(node);
				}
			}
		}


		public IEnumerable<OutlookTask> SelectedTasks =>
			GetSelectedTask(model.Root, new List<OutlookTask>());


		public bool ShowDetailedTable => tableButton.Checked;


		private void CheckStateChanged(object sender, TreePathEventArgs e)
		{
			if (tree.NodeControls.FirstOrDefault(c => c is NodeCheckBox) is NodeCheckBox box)
			{
				box.CheckStateChanged -= CheckStateChanged;

				var node = model.FindNode(e.Path);
				if (node != null)
				{
					if (node.Tag is OutlookTask task)
					{
						CheckFolderState(node.Parent);
					}
					else if (node.Tag is OutlookTaskFolder folder)
					{
						if (node.CheckState != CheckState.Indeterminate)
						{
							node.Nodes
								.Where(n => n.Tag is OutlookTask t && string.IsNullOrEmpty(t.OneNoteTaskID))
								.ForEach(n => n.CheckState = node.CheckState);
						}
					}
				}

				box.CheckStateChanged += CheckStateChanged;
			}
		}


		private void CheckFolderState(Node node)
		{
			var userChecked = 0;
			var userUnchecked = 0;
			foreach (var child in node.Nodes.Where(n => 
				n.Tag is OutlookTask task && string.IsNullOrEmpty(task.OneNoteTaskID)))
			{
				if (child.IsChecked)
				{
					userChecked++;
				}
				else
				{
					userUnchecked++;
				}
			}

			if (userChecked > 0 && userUnchecked == 0)
			{
				node.IsChecked = true;
			}
			else if (userChecked == 0 && userUnchecked > 0)
			{
				node.IsChecked = false;
			}
			else
			{
				node.CheckState = CheckState.Indeterminate;
			}
		}


		private IEnumerable<OutlookTask> GetSelectedTask(Node node, List<OutlookTask> tasks)
		{
			var t = node.Nodes.Where(n => n.Tag is OutlookTask task && 
				string.IsNullOrEmpty(task.OneNoteTaskID) && n.IsChecked)
				.Select(n => n.Tag as OutlookTask);

			if (t.Any())
			{
				tasks.AddRange(t);
			}

			foreach (var child in node.Nodes.Where(n => n.Tag is OutlookTaskFolder))
			{
				GetSelectedTask(child, tasks);
			}

			return tasks;
		}


		private async void ResetOrphanedTasks(object sender, LinkLabelLinkClickedEventArgs e)
		{
			using (one = new OneNote())
			{
				var source = new CancellationTokenSource();
				var map = await one.BuildHyperlinkMap(OneNote.Scope.Sections, source.Token);
				var count = 0;

				try
				{
					outlook = new Outlook();
					count = ResetOrphanedTasks(map, model.Root, 0);
				}
				finally
				{
					outlook.Dispose();
				}

				if (count == 0)
				{
					UIHelper.ShowMessage(Resx.ImportOutlookTasksDialog_noorphans);
				}
				else
				{
					UIHelper.ShowMessage(
						string.Format(Resx.ImportOutlookTasksDialog_reset, count));
				}
			}
		}


		private int ResetOrphanedTasks(Dictionary<string, HyperlinkInfo> map, Node node, int count)
		{
			var taskNodes = node.Nodes
				.Where(n => n.Tag is OutlookTask task && !string.IsNullOrEmpty(task.OneNoteURL));

			if (taskNodes.Any())
			{
				foreach (var taskNode in taskNodes)
				{
					var task = taskNode.Tag as OutlookTask;

					var key = one.GetHyperKey(task.OneNoteURL);
					if (map.ContainsKey(key))
					{
						var page = one.GetPage(map[key].PageID, PageDetail.Basic);
						if (page == null)
						{
							ResetTask(taskNode, task);
							count++;
						}
						else
						{
							if (!page.Root.Descendants(page.Namespace + "OutlookTask")
								.Any(e => e.Attribute("guidTask").Value == task.OneNoteTaskID))
							{
								ResetTask(taskNode, task);
								count++;
							}
						}
					}
				}
			}

			foreach (var child in node.Nodes.Where(n => n.Tag is OutlookTaskFolder))
			{
				ResetOrphanedTasks(map, child, count);
			}

			return count;
		}


		private void ResetTask(Node node, OutlookTask task)
		{
			task.OneNoteTaskID = null;
			task.OneNoteURL = null;
			task.OneNotePageID = null;
			task.OneNoteObjectID = null;

			outlook.SaveTask(task);

			node.IsEnabled = true;
			node.IsChecked = false;
		}
	}
}
