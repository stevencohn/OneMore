//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Aga.Controls.Tree;
	using Aga.Controls.Tree.NodeControls;
	using River.OneMoreAddIn.Helpers.Office;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class ImportOutlookTasksDialog : UI.LocalizableForm
	{
		private readonly TreeModel model;


		private sealed class TaskNodeIcon : NodeIcon
		{
			private readonly Image task;
			private readonly Image opened;
			private readonly Image closed;

			public TaskNodeIcon()
			{
				task = MakeTransparent(Resx.Task);
				opened = MakeTransparent(Resx.FolderOpen);
				closed = MakeTransparent(Resx.FolderClose);
			}

			private static Image MakeTransparent(Bitmap bitmap)
			{
				bitmap.MakeTransparent(bitmap.GetPixel(0, 0));
				return bitmap;
			}

			protected override Image GetIcon(TreeNodeAdv node)
			{
				Image icon = base.GetIcon(node);
				if (icon != null)
					return icon;
				else if (node.IsLeaf)
					return task;
				else if (node.CanExpand && node.IsExpanded)
					return opened;
				else
					return closed;
			}
		}


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

			var nodeCheckBox = new NodeCheckBox
			{
				DataPropertyName = "CheckState",
				EditEnabled = true,
				LeftMargin = 0,
				ThreeState = true
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

				var locked = 0;
				foreach (var task in folder.Tasks)
				{
					var leaf = new Node(task.Subject);
					leaf.Tag = task;

					if (!string.IsNullOrEmpty(task.OneNoteTaskID))
					{
						leaf.IsChecked = true;
						leaf.IsEnabled = false;
						locked++;
					}

					node.Nodes.Add(leaf);
				}

				if (locked == folder.Tasks.Count)
					node.CheckState = CheckState.Checked;
				else if (locked > 0)
					node.CheckState = CheckState.Indeterminate;

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


		private void CheckStateChanged(object sender, TreePathEventArgs e)
		{
			var nodeCheckBox = tree.NodeControls.FirstOrDefault(c => c is NodeCheckBox) as NodeCheckBox;
			if (nodeCheckBox != null)
			{
				nodeCheckBox.CheckStateChanged -= CheckStateChanged;

				var node = model.FindNode(e.Path);
				if (node != null)
				{
					CheckStateChanged(node, node.CheckState);
				}

				nodeCheckBox.CheckStateChanged += CheckStateChanged;
			}
		}


		private void CheckStateChanged(Node node, CheckState state)
		{
			if (node.Tag is OutlookTaskFolder folder)
			{
				var n = model.FindNode(e.Path);
				var checkbox = n.CheckState
			}
		}
	}
}
