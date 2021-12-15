﻿//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Aga.Controls.Tree;
	using Aga.Controls.Tree.NodeControls;
	using River.OneMoreAddIn.Helpers.Office;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class ImportOutlookTasksDialog : UI.LocalizableForm
	{
		private readonly TreeModel model;


		#region class TaskNodeIcon
		private sealed class TaskNodeIcon : NodeIcon
		{
			private readonly Image task;
			private readonly Image opened;
			private readonly Image closed;

			public TaskNodeIcon()
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
				//Text = Resx.RemindDialog_Text;

				Localize(new string[]
				{
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
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

			tree.NodeControls.Add(new TaskNodeIcon
			{
				LeftMargin = 1,
				ScaleMode = ImageScaleMode.Clip
			});

			tree.NodeControls.Add(new NodeTextBox
			{
				DataPropertyName = "Text",
				LeftMargin = 3
			});

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
				var node = new Node(folder.Name);
				node.Tag = folder;

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


		private void CheckStateChanged(object sender, TreePathEventArgs e)
		{
			var nodeCheckBox = tree.NodeControls.FirstOrDefault(c => c is NodeCheckBox) as NodeCheckBox;
			if (nodeCheckBox != null)
			{
				nodeCheckBox.CheckStateChanged -= CheckStateChanged;

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

				nodeCheckBox.CheckStateChanged += CheckStateChanged;
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
	}
}
