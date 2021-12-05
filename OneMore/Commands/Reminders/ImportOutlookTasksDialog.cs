//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Helpers.Office;
	using System.Drawing;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class ImportOutlookTasksDialog : UI.LocalizableForm
	{
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
		}


		public ImportOutlookTasksDialog(OutlookTaskFolders folders)
			: this()
		{
			PopulateTree(folders);

			treeView.ExpandAll();
		}


		private void PopulateTree(OutlookTaskFolders folders, TreeNode parent = null)
		{
			foreach (var folder in folders)
			{
				var node = parent == null
					? treeView.Nodes.Add(folder.EntryID, folder.Name, 0)
					: parent.Nodes.Add(folder.EntryID, folder.Name, 0);

				node.Tag = folder;

				PopulateTree(folder.Folders, node);

				foreach (var task in folder.Tasks)
				{
					var leaf = node.Nodes.Add(task.EntryID, task.Subject, 2);
					leaf.Tag = task;

					if (!string.IsNullOrEmpty(task.OneNoteTaskID))
					{
						leaf.Checked = true;
						leaf.ForeColor = SystemColors.GrayText;
					}
				}
			}
		}


		private void TreeView_BeforeSelect(object sender, TreeViewCancelEventArgs e)
		{
			if (e.Node.Tag is OutlookTask task)
			{
				if (!string.IsNullOrEmpty(task.OneNoteTaskID))
				{
					e.Cancel = true;
				}
			}
		}

		private void TreeView_AfterCollapse(object sender, TreeViewEventArgs e)
		{
			e.Node.ImageIndex = 0;
		}

		private void TreeView_AfterExpand(object sender, TreeViewEventArgs e)
		{
			e.Node.ImageIndex = 1;
		}
	}
}
