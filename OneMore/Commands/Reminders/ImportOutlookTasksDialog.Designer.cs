namespace River.OneMoreAddIn.Commands
{
	partial class ImportOutlookTasksDialog
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportOutlookTasksDialog));
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.introBox = new System.Windows.Forms.TextBox();
			this.warningBox = new System.Windows.Forms.RichTextBox();
			this.resetLabel = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.imageList = new System.Windows.Forms.ImageList(this.components);
			this.tree = new Aga.Controls.Tree.TreeViewAdv();
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.tableButton = new System.Windows.Forms.RadioButton();
			this.listButton = new System.Windows.Forms.RadioButton();
			this.optionsPanel = new System.Windows.Forms.Panel();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.resetInfoLabel = new System.Windows.Forms.Label();
			this.buttonPanel.SuspendLayout();
			this.optionsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(563, 6);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.TabIndex = 9;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(669, 6);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.TabIndex = 10;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// introBox
			// 
			this.introBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.introBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.introBox.Enabled = false;
			this.introBox.Location = new System.Drawing.Point(20, 20);
			this.introBox.Multiline = true;
			this.introBox.Name = "introBox";
			this.introBox.ReadOnly = true;
			this.introBox.Size = new System.Drawing.Size(772, 51);
			this.introBox.TabIndex = 0;
			this.introBox.Text = "Choose an entire folder or individual tasks to import from Outlook into OneNote. " +
    "Disabled items indicate tasks already linked to OneNote";
			// 
			// warningBox
			// 
			this.warningBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.warningBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.warningBox.Enabled = false;
			this.warningBox.Location = new System.Drawing.Point(20, 71);
			this.warningBox.Name = "warningBox";
			this.warningBox.ReadOnly = true;
			this.warningBox.Size = new System.Drawing.Size(772, 66);
			this.warningBox.TabIndex = 0;
			this.warningBox.Text = resources.GetString("warningBox.Text");
			// 
			// resetLabel
			// 
			this.resetLabel.AutoSize = true;
			this.resetLabel.Cursor = System.Windows.Forms.Cursors.Hand;
			this.resetLabel.ForeColor = System.Drawing.SystemColors.GrayText;
			this.resetLabel.LinkColor = System.Drawing.Color.DimGray;
			this.resetLabel.Location = new System.Drawing.Point(30, 15);
			this.resetLabel.Name = "resetLabel";
			this.resetLabel.Size = new System.Drawing.Size(166, 20);
			this.resetLabel.TabIndex = 11;
			this.resetLabel.TabStop = true;
			this.resetLabel.Text = "Reset orphaned tasks";
			this.toolTip.SetToolTip(this.resetLabel, "An orphaned task is one that was imported but its linked paragraph was deleted. T" +
        "his will reset orphaned tasks so they can be imported again.");
			this.resetLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ResetOrphanedTasks);
			// 
			// imageList
			// 
			this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
			this.imageList.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList.Images.SetKeyName(0, "FolderClose.png");
			this.imageList.Images.SetKeyName(1, "FolderOpen.png");
			this.imageList.Images.SetKeyName(2, "Task.png");
			// 
			// tree
			// 
			this.tree.AutoRowHeight = true;
			this.tree.BackColor = System.Drawing.SystemColors.Window;
			this.tree.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.tree.ColumnHeaderHeight = 0;
			this.tree.Cursor = System.Windows.Forms.Cursors.Default;
			this.tree.DefaultToolTipProvider = null;
			this.tree.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tree.DragDropMarkColor = System.Drawing.Color.Black;
			this.tree.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tree.FullRowSelectActiveColor = System.Drawing.Color.Empty;
			this.tree.FullRowSelectInactiveColor = System.Drawing.Color.Empty;
			this.tree.LineColor = System.Drawing.SystemColors.ControlDark;
			this.tree.LoadOnDemand = true;
			this.tree.Location = new System.Drawing.Point(20, 137);
			this.tree.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tree.Model = null;
			this.tree.Name = "tree";
			this.tree.NodeFilter = null;
			this.tree.SelectedNode = null;
			this.tree.SelectionMode = Aga.Controls.Tree.TreeSelectionMode.MultiSameParent;
			this.tree.ShowPerformance = false;
			this.tree.Size = new System.Drawing.Size(772, 315);
			this.tree.TabIndex = 0;
			// 
			// buttonPanel
			// 
			this.buttonPanel.Controls.Add(this.tableButton);
			this.buttonPanel.Controls.Add(this.listButton);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.Location = new System.Drawing.Point(20, 452);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Size = new System.Drawing.Size(772, 72);
			this.buttonPanel.TabIndex = 13;
			// 
			// tableButton
			// 
			this.tableButton.AutoSize = true;
			this.tableButton.Checked = true;
			this.tableButton.Location = new System.Drawing.Point(3, 8);
			this.tableButton.Name = "tableButton";
			this.tableButton.Size = new System.Drawing.Size(217, 24);
			this.tableButton.TabIndex = 1;
			this.tableButton.TabStop = true;
			this.tableButton.Text = "Generate task detail table";
			this.tableButton.UseVisualStyleBackColor = true;
			// 
			// listButton
			// 
			this.listButton.AutoSize = true;
			this.listButton.Location = new System.Drawing.Point(3, 38);
			this.listButton.Name = "listButton";
			this.listButton.Size = new System.Drawing.Size(159, 24);
			this.listButton.TabIndex = 0;
			this.listButton.Text = "Generate task list";
			this.listButton.UseVisualStyleBackColor = true;
			// 
			// optionsPanel
			// 
			this.optionsPanel.Controls.Add(this.resetInfoLabel);
			this.optionsPanel.Controls.Add(this.resetLabel);
			this.optionsPanel.Controls.Add(this.cancelButton);
			this.optionsPanel.Controls.Add(this.okButton);
			this.optionsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.optionsPanel.Location = new System.Drawing.Point(20, 524);
			this.optionsPanel.Name = "optionsPanel";
			this.optionsPanel.Size = new System.Drawing.Size(772, 47);
			this.optionsPanel.TabIndex = 14;
			// 
			// resetInfoLabel
			// 
			this.resetInfoLabel.AutoSize = true;
			this.resetInfoLabel.Location = new System.Drawing.Point(236, 15);
			this.resetInfoLabel.Name = "resetInfoLabel";
			this.resetInfoLabel.Size = new System.Drawing.Size(76, 20);
			this.resetInfoLabel.TabIndex = 12;
			this.resetInfoLabel.Text = "reset-info";
			// 
			// ImportOutlookTasksDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(812, 591);
			this.Controls.Add(this.tree);
			this.Controls.Add(this.warningBox);
			this.Controls.Add(this.introBox);
			this.Controls.Add(this.buttonPanel);
			this.Controls.Add(this.optionsPanel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ImportOutlookTasksDialog";
			this.Padding = new System.Windows.Forms.Padding(20);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Import Tasks from Outlook";
			this.buttonPanel.ResumeLayout(false);
			this.buttonPanel.PerformLayout();
			this.optionsPanel.ResumeLayout(false);
			this.optionsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.TextBox introBox;
		private System.Windows.Forms.RichTextBox warningBox;
		private System.Windows.Forms.Panel optionsPanel;
		private System.Windows.Forms.Panel buttonPanel;
		private System.Windows.Forms.ImageList imageList;
		private Aga.Controls.Tree.TreeViewAdv tree;
		private System.Windows.Forms.RadioButton tableButton;
		private System.Windows.Forms.RadioButton listButton;
		private UI.MoreLinkLabel resetLabel;
		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.Label resetInfoLabel;
	}
}