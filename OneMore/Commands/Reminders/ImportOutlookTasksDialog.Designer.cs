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
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.introBox = new River.OneMoreAddIn.UI.MoreMultilineLabel();
			this.warningBox = new River.OneMoreAddIn.UI.MoreRichLabel();
			this.resetLabel = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.imageList = new System.Windows.Forms.ImageList(this.components);
			this.tree = new Aga.Controls.Tree.TreeViewAdv();
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.tableButton = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.listButton = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.optionsPanel = new System.Windows.Forms.Panel();
			this.resetInfoLabel = new System.Windows.Forms.Label();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.buttonPanel.SuspendLayout();
			this.optionsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(563, 10);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.TabIndex = 1;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(669, 10);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.TabIndex = 2;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// introBox
			// 
			this.introBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.introBox.Location = new System.Drawing.Point(20, 20);
			this.introBox.Name = "introBox";
			this.introBox.Padding = new System.Windows.Forms.Padding(0, 0, 0, 12);
			this.introBox.Size = new System.Drawing.Size(772, 61);
			this.introBox.TabIndex = 0;
			this.introBox.Text = "Choose an entire folder or individual tasks to import from Outlook into OneNote. " +
    "Disabled items indicate tasks already linked to OneNote";
			this.introBox.ThemedBack = "ControlLight";
			this.introBox.ThemedFore = "ControlText";
			// 
			// warningBox
			// 
			this.warningBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.warningBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.warningBox.Location = new System.Drawing.Point(20, 81);
			this.warningBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
			this.warningBox.Name = "warningBox";
			this.warningBox.ReadOnly = true;
			this.warningBox.Size = new System.Drawing.Size(772, 66);
			this.warningBox.TabIndex = 0;
			this.warningBox.TabStop = false;
			this.warningBox.Text = resources.GetString("warningBox.Text");
			// 
			// resetLabel
			// 
			this.resetLabel.ActiveLinkColor = System.Drawing.Color.MediumOrchid;
			this.resetLabel.AutoSize = true;
			this.resetLabel.Cursor = System.Windows.Forms.Cursors.Hand;
			this.resetLabel.ForeColor = System.Drawing.SystemColors.GrayText;
			this.resetLabel.HoverColor = System.Drawing.Color.MediumOrchid;
			this.resetLabel.LinkColor = System.Drawing.Color.DimGray;
			this.resetLabel.Location = new System.Drawing.Point(30, 15);
			this.resetLabel.Name = "resetLabel";
			this.resetLabel.Size = new System.Drawing.Size(166, 20);
			this.resetLabel.StrictColors = false;
			this.resetLabel.TabIndex = 0;
			this.resetLabel.TabStop = true;
			this.resetLabel.Text = "Reset orphaned tasks";
			this.resetLabel.ThemedBack = null;
			this.resetLabel.ThemedFore = null;
			this.toolTip.SetToolTip(this.resetLabel, "An orphaned task is one that was imported but its linked paragraph was deleted. T" +
        "his will reset orphaned tasks so they can be imported again.");
			this.resetLabel.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
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
			this.tree.Location = new System.Drawing.Point(20, 147);
			this.tree.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tree.Model = null;
			this.tree.Name = "tree";
			this.tree.NodeFilter = null;
			this.tree.SelectedNode = null;
			this.tree.SelectionMode = Aga.Controls.Tree.TreeSelectionMode.MultiSameParent;
			this.tree.ShowPerformance = false;
			this.tree.Size = new System.Drawing.Size(772, 307);
			this.tree.TabIndex = 0;
			// 
			// buttonPanel
			// 
			this.buttonPanel.Controls.Add(this.tableButton);
			this.buttonPanel.Controls.Add(this.listButton);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.Location = new System.Drawing.Point(20, 454);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Size = new System.Drawing.Size(772, 78);
			this.buttonPanel.TabIndex = 13;
			// 
			// tableButton
			// 
			this.tableButton.Checked = true;
			this.tableButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.tableButton.Location = new System.Drawing.Point(3, 12);
			this.tableButton.Name = "tableButton";
			this.tableButton.Size = new System.Drawing.Size(223, 25);
			this.tableButton.TabIndex = 0;
			this.tableButton.TabStop = true;
			this.tableButton.Text = "Generate task detail table";
			this.tableButton.UseVisualStyleBackColor = true;
			// 
			// listButton
			// 
			this.listButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.listButton.Location = new System.Drawing.Point(3, 42);
			this.listButton.Name = "listButton";
			this.listButton.Size = new System.Drawing.Size(163, 25);
			this.listButton.TabIndex = 1;
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
			this.optionsPanel.Location = new System.Drawing.Point(20, 532);
			this.optionsPanel.Name = "optionsPanel";
			this.optionsPanel.Size = new System.Drawing.Size(772, 51);
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
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(812, 603);
			this.Controls.Add(this.tree);
			this.Controls.Add(this.warningBox);
			this.Controls.Add(this.introBox);
			this.Controls.Add(this.buttonPanel);
			this.Controls.Add(this.optionsPanel);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ImportOutlookTasksDialog";
			this.Padding = new System.Windows.Forms.Padding(20);
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Import Tasks from Outlook";
			this.buttonPanel.ResumeLayout(false);
			this.optionsPanel.ResumeLayout(false);
			this.optionsPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private UI.MoreMultilineLabel introBox;
		private River.OneMoreAddIn.UI.MoreRichLabel warningBox;
		private System.Windows.Forms.Panel optionsPanel;
		private System.Windows.Forms.Panel buttonPanel;
		private System.Windows.Forms.ImageList imageList;
		private Aga.Controls.Tree.TreeViewAdv tree;
		private UI.MoreRadioButton tableButton;
		private UI.MoreRadioButton listButton;
		private UI.MoreLinkLabel resetLabel;
		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.Label resetInfoLabel;
	}
}