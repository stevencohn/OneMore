namespace River.OneMoreAddIn.Commands
{
	partial class HashtaggerDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HashtaggerDialog));
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.mainFlow = new System.Windows.Forms.FlowLayoutPanel();
			this.recentGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.recentFlow = new System.Windows.Forms.FlowLayoutPanel();
			this.commonGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.commonFlow = new System.Windows.Forms.FlowLayoutPanel();
			this.cloudGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.cloudFlow = new System.Windows.Forms.FlowLayoutPanel();
			this.spacerPanel = new River.OneMoreAddIn.UI.MorePanel();
			this.controlPanel = new River.OneMoreAddIn.UI.MorePanel();
			this.clearLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.bankBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.menuButton = new River.OneMoreAddIn.UI.MoreButton();
			this.tagsBox = new River.OneMoreAddIn.UI.MoreMultilineLabel();
			this.tagsLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.showRecentMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.showCommonMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.buttonPanel.SuspendLayout();
			this.mainFlow.SuspendLayout();
			this.recentGroup.SuspendLayout();
			this.commonGroup.SuspendLayout();
			this.cloudGroup.SuspendLayout();
			this.controlPanel.SuspendLayout();
			this.contextMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonPanel
			// 
			this.buttonPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.buttonPanel.Controls.Add(this.okButton);
			this.buttonPanel.Controls.Add(this.cancelButton);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.Location = new System.Drawing.Point(0, 578);
			this.buttonPanel.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Padding = new System.Windows.Forms.Padding(0, 0, 10, 10);
			this.buttonPanel.Size = new System.Drawing.Size(778, 66);
			this.buttonPanel.TabIndex = 4;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Enabled = false;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(559, 17);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(100, 36);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 0;
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
			this.cancelButton.Location = new System.Drawing.Point(665, 17);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(100, 36);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// mainFlow
			// 
			this.mainFlow.AutoScroll = true;
			this.mainFlow.AutoSize = true;
			this.mainFlow.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.mainFlow.BackColor = System.Drawing.SystemColors.Control;
			this.mainFlow.Controls.Add(this.recentGroup);
			this.mainFlow.Controls.Add(this.commonGroup);
			this.mainFlow.Controls.Add(this.cloudGroup);
			this.mainFlow.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainFlow.Location = new System.Drawing.Point(0, 119);
			this.mainFlow.MinimumSize = new System.Drawing.Size(400, 170);
			this.mainFlow.Name = "mainFlow";
			this.mainFlow.Padding = new System.Windows.Forms.Padding(15, 10, 15, 15);
			this.mainFlow.Size = new System.Drawing.Size(778, 444);
			this.mainFlow.TabIndex = 6;
			this.mainFlow.Resize += new System.EventHandler(this.ResizeFlows);
			// 
			// recentGroup
			// 
			this.recentGroup.BackColor = System.Drawing.SystemColors.ControlLight;
			this.recentGroup.BorderThickness = 3;
			this.recentGroup.Controls.Add(this.recentFlow);
			this.mainFlow.SetFlowBreak(this.recentGroup, true);
			this.recentGroup.ForeColor = System.Drawing.SystemColors.ControlText;
			this.recentGroup.Location = new System.Drawing.Point(18, 13);
			this.recentGroup.Margin = new System.Windows.Forms.Padding(3, 3, 3, 15);
			this.recentGroup.MinimumSize = new System.Drawing.Size(400, 80);
			this.recentGroup.Name = "recentGroup";
			this.recentGroup.Padding = new System.Windows.Forms.Padding(5);
			this.recentGroup.ShowOnlyTopEdge = true;
			this.recentGroup.Size = new System.Drawing.Size(550, 80);
			this.recentGroup.TabIndex = 6;
			this.recentGroup.TabStop = false;
			this.recentGroup.Text = "Recently used tags";
			this.recentGroup.ThemedBorder = "HotTrack";
			this.recentGroup.ThemedFore = "HotTrack";
			// 
			// recentFlow
			// 
			this.recentFlow.Dock = System.Windows.Forms.DockStyle.Fill;
			this.recentFlow.Location = new System.Drawing.Point(5, 24);
			this.recentFlow.Margin = new System.Windows.Forms.Padding(5);
			this.recentFlow.Name = "recentFlow";
			this.recentFlow.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
			this.recentFlow.Size = new System.Drawing.Size(540, 51);
			this.recentFlow.TabIndex = 5;
			// 
			// commonGroup
			// 
			this.commonGroup.BackColor = System.Drawing.SystemColors.ControlLight;
			this.commonGroup.BorderThickness = 3;
			this.commonGroup.Controls.Add(this.commonFlow);
			this.mainFlow.SetFlowBreak(this.commonGroup, true);
			this.commonGroup.ForeColor = System.Drawing.SystemColors.ControlText;
			this.commonGroup.Location = new System.Drawing.Point(18, 111);
			this.commonGroup.Margin = new System.Windows.Forms.Padding(3, 3, 3, 15);
			this.commonGroup.MinimumSize = new System.Drawing.Size(400, 80);
			this.commonGroup.Name = "commonGroup";
			this.commonGroup.Padding = new System.Windows.Forms.Padding(5);
			this.commonGroup.ShowOnlyTopEdge = true;
			this.commonGroup.Size = new System.Drawing.Size(550, 80);
			this.commonGroup.TabIndex = 7;
			this.commonGroup.TabStop = false;
			this.commonGroup.Text = "Common words on this page";
			this.commonGroup.ThemedBorder = "HotTrack";
			this.commonGroup.ThemedFore = "HotTrack";
			// 
			// commonFlow
			// 
			this.commonFlow.Dock = System.Windows.Forms.DockStyle.Fill;
			this.commonFlow.Location = new System.Drawing.Point(5, 24);
			this.commonFlow.Margin = new System.Windows.Forms.Padding(5);
			this.commonFlow.Name = "commonFlow";
			this.commonFlow.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
			this.commonFlow.Size = new System.Drawing.Size(540, 51);
			this.commonFlow.TabIndex = 5;
			// 
			// cloudGroup
			// 
			this.cloudGroup.BackColor = System.Drawing.SystemColors.ControlLight;
			this.cloudGroup.BorderThickness = 3;
			this.cloudGroup.Controls.Add(this.cloudFlow);
			this.mainFlow.SetFlowBreak(this.cloudGroup, true);
			this.cloudGroup.ForeColor = System.Drawing.SystemColors.ControlText;
			this.cloudGroup.Location = new System.Drawing.Point(18, 209);
			this.cloudGroup.MinimumSize = new System.Drawing.Size(400, 80);
			this.cloudGroup.Name = "cloudGroup";
			this.cloudGroup.Padding = new System.Windows.Forms.Padding(5);
			this.cloudGroup.ShowOnlyTopEdge = true;
			this.cloudGroup.Size = new System.Drawing.Size(550, 80);
			this.cloudGroup.TabIndex = 8;
			this.cloudGroup.TabStop = false;
			this.cloudGroup.Text = "All Hashtags";
			this.cloudGroup.ThemedBorder = "HotTrack";
			this.cloudGroup.ThemedFore = "HotTrack";
			// 
			// cloudFlow
			// 
			this.cloudFlow.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cloudFlow.Location = new System.Drawing.Point(5, 24);
			this.cloudFlow.Margin = new System.Windows.Forms.Padding(5);
			this.cloudFlow.Name = "cloudFlow";
			this.cloudFlow.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
			this.cloudFlow.Size = new System.Drawing.Size(540, 51);
			this.cloudFlow.TabIndex = 5;
			// 
			// spacerPanel
			// 
			this.spacerPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.spacerPanel.BottomBorderColor = System.Drawing.SystemColors.ActiveBorder;
			this.spacerPanel.BottomBorderSize = 2;
			this.spacerPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.spacerPanel.Location = new System.Drawing.Point(0, 563);
			this.spacerPanel.Name = "spacerPanel";
			this.spacerPanel.Size = new System.Drawing.Size(778, 15);
			this.spacerPanel.TabIndex = 9;
			this.spacerPanel.ThemedBack = null;
			this.spacerPanel.ThemedFore = null;
			this.spacerPanel.TopBorderColor = System.Drawing.SystemColors.Control;
			this.spacerPanel.TopBorderSize = 0;
			// 
			// controlPanel
			// 
			this.controlPanel.BottomBorderColor = System.Drawing.SystemColors.ActiveBorder;
			this.controlPanel.BottomBorderSize = 0;
			this.controlPanel.Controls.Add(this.clearLink);
			this.controlPanel.Controls.Add(this.bankBox);
			this.controlPanel.Controls.Add(this.menuButton);
			this.controlPanel.Controls.Add(this.tagsBox);
			this.controlPanel.Controls.Add(this.tagsLabel);
			this.controlPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.controlPanel.Location = new System.Drawing.Point(0, 0);
			this.controlPanel.Name = "controlPanel";
			this.controlPanel.Padding = new System.Windows.Forms.Padding(15);
			this.controlPanel.Size = new System.Drawing.Size(778, 119);
			this.controlPanel.TabIndex = 10;
			this.controlPanel.ThemedBack = null;
			this.controlPanel.ThemedFore = null;
			this.controlPanel.TopBorderColor = System.Drawing.SystemColors.Control;
			this.controlPanel.TopBorderSize = 0;
			// 
			// clearLink
			// 
			this.clearLink.ActiveLinkColor = System.Drawing.Color.MediumOrchid;
			this.clearLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.clearLink.AutoSize = true;
			this.clearLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.clearLink.HoverColor = System.Drawing.Color.Orchid;
			this.clearLink.LinkColor = System.Drawing.Color.MediumOrchid;
			this.clearLink.Location = new System.Drawing.Point(658, 77);
			this.clearLink.Name = "clearLink";
			this.clearLink.Size = new System.Drawing.Size(46, 20);
			this.clearLink.StrictColors = false;
			this.clearLink.TabIndex = 7;
			this.clearLink.TabStop = true;
			this.clearLink.Text = "Clear";
			this.clearLink.ThemedBack = null;
			this.clearLink.ThemedFore = null;
			this.clearLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.clearLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ClearTags);
			// 
			// bankBox
			// 
			this.bankBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.bankBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.bankBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.bankBox.Location = new System.Drawing.Point(93, 76);
			this.bankBox.Name = "bankBox";
			this.bankBox.Size = new System.Drawing.Size(255, 25);
			this.bankBox.StylizeImage = false;
			this.bankBox.TabIndex = 6;
			this.bankBox.Text = "Add to tag bank at top of page";
			this.bankBox.ThemedBack = null;
			this.bankBox.ThemedFore = null;
			this.bankBox.UseVisualStyleBackColor = false;
			// 
			// menuButton
			// 
			this.menuButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.menuButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.menuButton.FlatAppearance.BorderSize = 0;
			this.menuButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.menuButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.menuButton.ForeColor = System.Drawing.SystemColors.GrayText;
			this.menuButton.ImageOver = null;
			this.menuButton.Location = new System.Drawing.Point(710, 18);
			this.menuButton.Name = "menuButton";
			this.menuButton.ShowBorder = true;
			this.menuButton.Size = new System.Drawing.Size(50, 31);
			this.menuButton.StylizeImage = false;
			this.menuButton.TabIndex = 5;
			this.menuButton.Text = "•••";
			this.menuButton.ThemedBack = null;
			this.menuButton.ThemedFore = null;
			this.menuButton.UseVisualStyleBackColor = true;
			this.menuButton.Click += new System.EventHandler(this.ShowMenu);
			// 
			// tagsBox
			// 
			this.tagsBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tagsBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.tagsBox.Location = new System.Drawing.Point(93, 18);
			this.tagsBox.Name = "tagsBox";
			this.tagsBox.Size = new System.Drawing.Size(611, 52);
			this.tagsBox.TabIndex = 1;
			this.tagsBox.ThemedBack = null;
			this.tagsBox.ThemedFore = null;
			// 
			// tagsLabel
			// 
			this.tagsLabel.AutoSize = true;
			this.tagsLabel.Location = new System.Drawing.Point(14, 18);
			this.tagsLabel.Name = "tagsLabel";
			this.tagsLabel.Size = new System.Drawing.Size(44, 20);
			this.tagsLabel.TabIndex = 0;
			this.tagsLabel.Text = "Tags";
			this.tagsLabel.ThemedBack = null;
			this.tagsLabel.ThemedFore = null;
			// 
			// contextMenu
			// 
			this.contextMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showRecentMenuItem,
            this.showCommonMenuItem});
			this.contextMenu.Name = "contextMenu";
			this.contextMenu.Size = new System.Drawing.Size(261, 68);
			this.contextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.PrepareContextMenu);
			// 
			// showRecentMenuItem
			// 
			this.showRecentMenuItem.Image = global::River.OneMoreAddIn.Properties.Resources.e_CheckMark;
			this.showRecentMenuItem.Name = "showRecentMenuItem";
			this.showRecentMenuItem.Size = new System.Drawing.Size(260, 32);
			this.showRecentMenuItem.Text = "Hide recently used";
			this.showRecentMenuItem.Click += new System.EventHandler(this.ShowHideGroup);
			// 
			// showCommonMenuItem
			// 
			this.showCommonMenuItem.Image = global::River.OneMoreAddIn.Properties.Resources.e_CheckMark;
			this.showCommonMenuItem.Name = "showCommonMenuItem";
			this.showCommonMenuItem.Size = new System.Drawing.Size(260, 32);
			this.showCommonMenuItem.Text = "Hide common words";
			this.showCommonMenuItem.Click += new System.EventHandler(this.ShowHideGroup);
			// 
			// HashtaggerDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(778, 644);
			this.Controls.Add(this.mainFlow);
			this.Controls.Add(this.controlPanel);
			this.Controls.Add(this.spacerPanel);
			this.Controls.Add(this.buttonPanel);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(650, 600);
			this.Name = "HashtaggerDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Hashtags";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SaveSettings);
			this.Load += new System.EventHandler(this.LoadTagsOnLoad);
			this.buttonPanel.ResumeLayout(false);
			this.mainFlow.ResumeLayout(false);
			this.recentGroup.ResumeLayout(false);
			this.commonGroup.ResumeLayout(false);
			this.cloudGroup.ResumeLayout(false);
			this.controlPanel.ResumeLayout(false);
			this.controlPanel.PerformLayout();
			this.contextMenu.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Panel buttonPanel;
		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private System.Windows.Forms.FlowLayoutPanel mainFlow;
		private UI.MoreGroupBox recentGroup;
		private System.Windows.Forms.FlowLayoutPanel recentFlow;
		private UI.MoreGroupBox commonGroup;
		private System.Windows.Forms.FlowLayoutPanel commonFlow;
		private UI.MoreGroupBox cloudGroup;
		private System.Windows.Forms.FlowLayoutPanel cloudFlow;
		private UI.MorePanel spacerPanel;
		private UI.MorePanel controlPanel;
		private UI.MoreLabel tagsLabel;
		private UI.MoreMultilineLabel tagsBox;
		private UI.MoreButton menuButton;
		private System.Windows.Forms.ContextMenuStrip contextMenu;
		private System.Windows.Forms.ToolStripMenuItem showRecentMenuItem;
		private System.Windows.Forms.ToolStripMenuItem showCommonMenuItem;
		private UI.MoreCheckBox bankBox;
		private UI.MoreLinkLabel clearLink;
	}
}