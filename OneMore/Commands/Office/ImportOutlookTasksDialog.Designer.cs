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
			this.resetLabel = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.linksPanel = new System.Windows.Forms.Panel();
			this.selectAllLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.sep1 = new River.OneMoreAddIn.UI.MoreLabel();
			this.selectNoneLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.taskPanel = new System.Windows.Forms.Panel();
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.tableButton = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.listButton = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.optionsPanel = new System.Windows.Forms.Panel();
			this.resetInfoLabel = new System.Windows.Forms.Label();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.linksPanel.SuspendLayout();
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
			this.introBox.Text = "Choose the tasks to import from Outlook into OneNote. " +
    "Disabled items indicate tasks already linked to OneNote";
			this.introBox.ThemedBack = "ControlLight";
			this.introBox.ThemedFore = "ControlText";
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
			// linksPanel
			//
			this.linksPanel.Controls.Add(this.selectNoneLink);
			this.linksPanel.Controls.Add(this.sep1);
			this.linksPanel.Controls.Add(this.selectAllLink);
			this.linksPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.linksPanel.Location = new System.Drawing.Point(20, 81);
			this.linksPanel.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
			this.linksPanel.Name = "linksPanel";
			this.linksPanel.Size = new System.Drawing.Size(772, 36);
			this.linksPanel.TabIndex = 0;
			//
			// selectAllLink
			//
			this.selectAllLink.Active = false;
			this.selectAllLink.ActiveLinkColor = System.Drawing.Color.DarkOrchid;
			this.selectAllLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.selectAllLink.AutoSize = true;
			this.selectAllLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.selectAllLink.HoverColor = System.Drawing.Color.MediumOrchid;
			this.selectAllLink.LinkColor = System.Drawing.SystemColors.ControlDark;
			this.selectAllLink.Location = new System.Drawing.Point(561, 8);
			this.selectAllLink.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.selectAllLink.MaximumSize = new System.Drawing.Size(420, 0);
			this.selectAllLink.Name = "selectAllLink";
			this.selectAllLink.NavMode = false;
			this.selectAllLink.Selected = false;
			this.selectAllLink.Size = new System.Drawing.Size(75, 20);
			this.selectAllLink.StrictColors = false;
			this.selectAllLink.TabIndex = 1;
			this.selectAllLink.TabStop = true;
			this.selectAllLink.Text = "Select All";
			this.selectAllLink.ThemedBack = null;
			this.selectAllLink.ThemedFore = null;
			this.selectAllLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.selectAllLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.SelectAllTasks);
			//
			// sep1
			//
			this.sep1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.sep1.AutoSize = true;
			this.sep1.Location = new System.Drawing.Point(643, 8);
			this.sep1.Name = "sep1";
			this.sep1.Size = new System.Drawing.Size(14, 20);
			this.sep1.TabIndex = 2;
			this.sep1.Text = "|";
			this.sep1.ThemedBack = null;
			this.sep1.ThemedFore = null;
			//
			// selectNoneLink
			//
			this.selectNoneLink.Active = false;
			this.selectNoneLink.ActiveLinkColor = System.Drawing.Color.DarkOrchid;
			this.selectNoneLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.selectNoneLink.AutoSize = true;
			this.selectNoneLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.selectNoneLink.HoverColor = System.Drawing.Color.MediumOrchid;
			this.selectNoneLink.LinkColor = System.Drawing.SystemColors.ControlDark;
			this.selectNoneLink.Location = new System.Drawing.Point(664, 8);
			this.selectNoneLink.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.selectNoneLink.MaximumSize = new System.Drawing.Size(420, 0);
			this.selectNoneLink.Name = "selectNoneLink";
			this.selectNoneLink.NavMode = false;
			this.selectNoneLink.Selected = false;
			this.selectNoneLink.Size = new System.Drawing.Size(96, 20);
			this.selectNoneLink.StrictColors = false;
			this.selectNoneLink.TabIndex = 3;
			this.selectNoneLink.TabStop = true;
			this.selectNoneLink.Text = "Select None";
			this.selectNoneLink.ThemedBack = null;
			this.selectNoneLink.ThemedFore = null;
			this.selectNoneLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.selectNoneLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.SelectNoneTasks);
			//
			// taskPanel
			//
			this.taskPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.taskPanel.Location = new System.Drawing.Point(20, 129);
			this.taskPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.taskPanel.Name = "taskPanel";
			this.taskPanel.Size = new System.Drawing.Size(772, 325);
			this.taskPanel.TabIndex = 0;
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
			this.Controls.Add(this.taskPanel);
			this.Controls.Add(this.linksPanel);
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
			this.linksPanel.ResumeLayout(false);
			this.linksPanel.PerformLayout();
			this.buttonPanel.ResumeLayout(false);
			this.optionsPanel.ResumeLayout(false);
			this.optionsPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private UI.MoreMultilineLabel introBox;
		private System.Windows.Forms.Panel optionsPanel;
		private System.Windows.Forms.Panel buttonPanel;
		private System.Windows.Forms.Panel linksPanel;
		private UI.MoreLinkLabel selectAllLink;
		private UI.MoreLabel sep1;
		private UI.MoreLinkLabel selectNoneLink;
		private System.Windows.Forms.Panel taskPanel;
		private UI.MoreRadioButton tableButton;
		private UI.MoreRadioButton listButton;
		private UI.MoreLinkLabel resetLabel;
		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.Label resetInfoLabel;
	}
}