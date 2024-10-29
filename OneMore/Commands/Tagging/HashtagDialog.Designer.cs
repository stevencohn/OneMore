namespace River.OneMoreAddIn.Commands
{
	partial class HashtagDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HashtagDialog));
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.contextPanel = new System.Windows.Forms.FlowLayoutPanel();
			this.topPanel = new River.OneMoreAddIn.UI.MorePanel();
			this.sensitiveBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.menuButton = new River.OneMoreAddIn.UI.MoreButton();
			this.barLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.checkAllLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.uncheckAllLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.scopeBox = new System.Windows.Forms.ComboBox();
			this.searchButton = new River.OneMoreAddIn.UI.MoreButton();
			this.tagBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.introBox = new River.OneMoreAddIn.UI.MoreMultilineLabel();
			this.controlPanel = new System.Windows.Forms.Panel();
			this.scanLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.lastScanLabel = new System.Windows.Forms.Label();
			this.indexButton = new River.OneMoreAddIn.UI.MoreButton();
			this.copyButton = new River.OneMoreAddIn.UI.MoreButton();
			this.moveButton = new River.OneMoreAddIn.UI.MoreButton();
			this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.scanButton = new System.Windows.Forms.ToolStripMenuItem();
			this.scheduleButton = new System.Windows.Forms.ToolStripMenuItem();
			this.offlineNotebooksButton = new System.Windows.Forms.ToolStripMenuItem();
			this.tooltip = new System.Windows.Forms.ToolTip(this.components);
			this.topPanel.SuspendLayout();
			this.controlPanel.SuspendLayout();
			this.contextMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(864, 13);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(112, 35);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 0;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.DoCancel);
			// 
			// contextPanel
			// 
			this.contextPanel.AutoScroll = true;
			this.contextPanel.BackColor = System.Drawing.SystemColors.AppWorkspace;
			this.contextPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.contextPanel.Location = new System.Drawing.Point(0, 143);
			this.contextPanel.Name = "contextPanel";
			this.contextPanel.Padding = new System.Windows.Forms.Padding(6);
			this.contextPanel.Size = new System.Drawing.Size(988, 641);
			this.contextPanel.TabIndex = 7;
			// 
			// topPanel
			// 
			this.topPanel.BackColor = System.Drawing.SystemColors.Control;
			this.topPanel.BottomBorderColor = System.Drawing.SystemColors.WindowFrame;
			this.topPanel.BottomBorderSize = 1;
			this.topPanel.Controls.Add(this.sensitiveBox);
			this.topPanel.Controls.Add(this.menuButton);
			this.topPanel.Controls.Add(this.barLabel);
			this.topPanel.Controls.Add(this.checkAllLink);
			this.topPanel.Controls.Add(this.uncheckAllLink);
			this.topPanel.Controls.Add(this.scopeBox);
			this.topPanel.Controls.Add(this.searchButton);
			this.topPanel.Controls.Add(this.tagBox);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = new System.Drawing.Point(0, 62);
			this.topPanel.Margin = new System.Windows.Forms.Padding(0);
			this.topPanel.Name = "topPanel";
			this.topPanel.Padding = new System.Windows.Forms.Padding(10);
			this.topPanel.Size = new System.Drawing.Size(988, 81);
			this.topPanel.TabIndex = 8;
			this.topPanel.ThemedBack = null;
			this.topPanel.ThemedFore = null;
			this.topPanel.TopBorderColor = System.Drawing.SystemColors.Control;
			this.topPanel.TopBorderSize = 0;
			// 
			// sensitiveBox
			// 
			this.sensitiveBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.sensitiveBox.Appearance = System.Windows.Forms.Appearance.Button;
			this.sensitiveBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.sensitiveBox.BackgroundImage = global::River.OneMoreAddIn.Properties.Resources.m_NewStyle;
			this.sensitiveBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.sensitiveBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.sensitiveBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.sensitiveBox.Location = new System.Drawing.Point(615, 12);
			this.sensitiveBox.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
			this.sensitiveBox.Name = "sensitiveBox";
			this.sensitiveBox.Padding = new System.Windows.Forms.Padding(2);
			this.sensitiveBox.Size = new System.Drawing.Size(31, 31);
			this.sensitiveBox.StylizeImage = true;
			this.sensitiveBox.TabIndex = 8;
			this.sensitiveBox.ThemedBack = null;
			this.sensitiveBox.ThemedFore = null;
			this.sensitiveBox.UseVisualStyleBackColor = false;
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
			this.menuButton.Location = new System.Drawing.Point(925, 12);
			this.menuButton.Name = "menuButton";
			this.menuButton.ShowBorder = true;
			this.menuButton.Size = new System.Drawing.Size(50, 31);
			this.menuButton.StylizeImage = false;
			this.menuButton.TabIndex = 4;
			this.menuButton.Text = "•••";
			this.menuButton.ThemedBack = null;
			this.menuButton.ThemedFore = null;
			this.menuButton.UseVisualStyleBackColor = true;
			this.menuButton.Click += new System.EventHandler(this.ShowMenu);
			// 
			// barLabel
			// 
			this.barLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.barLabel.AutoSize = true;
			this.barLabel.Location = new System.Drawing.Point(808, 51);
			this.barLabel.Name = "barLabel";
			this.barLabel.Size = new System.Drawing.Size(14, 20);
			this.barLabel.TabIndex = 7;
			this.barLabel.Text = "|";
			this.barLabel.ThemedBack = null;
			this.barLabel.ThemedFore = null;
			// 
			// checkAllLink
			// 
			this.checkAllLink.ActiveLinkColor = System.Drawing.Color.Orchid;
			this.checkAllLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkAllLink.AutoSize = true;
			this.checkAllLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.checkAllLink.HoverColor = System.Drawing.Color.MediumOrchid;
			this.checkAllLink.LinkColor = System.Drawing.Color.MediumOrchid;
			this.checkAllLink.Location = new System.Drawing.Point(729, 51);
			this.checkAllLink.Name = "checkAllLink";
			this.checkAllLink.Size = new System.Drawing.Size(73, 20);
			this.checkAllLink.StrictColors = false;
			this.checkAllLink.TabIndex = 2;
			this.checkAllLink.TabStop = true;
			this.checkAllLink.Text = "Check all";
			this.checkAllLink.ThemedBack = null;
			this.checkAllLink.ThemedFore = null;
			this.checkAllLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.checkAllLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ToggleAllChecks);
			// 
			// uncheckAllLink
			// 
			this.uncheckAllLink.ActiveLinkColor = System.Drawing.Color.Orchid;
			this.uncheckAllLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.uncheckAllLink.AutoSize = true;
			this.uncheckAllLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.uncheckAllLink.HoverColor = System.Drawing.Color.MediumOrchid;
			this.uncheckAllLink.LinkColor = System.Drawing.Color.MediumOrchid;
			this.uncheckAllLink.Location = new System.Drawing.Point(828, 51);
			this.uncheckAllLink.Name = "uncheckAllLink";
			this.uncheckAllLink.Size = new System.Drawing.Size(91, 20);
			this.uncheckAllLink.StrictColors = false;
			this.uncheckAllLink.TabIndex = 3;
			this.uncheckAllLink.TabStop = true;
			this.uncheckAllLink.Text = "Uncheck all";
			this.uncheckAllLink.ThemedBack = null;
			this.uncheckAllLink.ThemedFore = null;
			this.uncheckAllLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.uncheckAllLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ToggleAllChecks);
			// 
			// scopeBox
			// 
			this.scopeBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.scopeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.scopeBox.FormattingEnabled = true;
			this.scopeBox.Items.AddRange(new object[] {
            "All notebooks",
            "This notebook",
            "This section",
            "This page"});
			this.scopeBox.Location = new System.Drawing.Point(655, 12);
			this.scopeBox.Name = "scopeBox";
			this.scopeBox.Size = new System.Drawing.Size(196, 28);
			this.scopeBox.TabIndex = 4;
			this.scopeBox.SelectedIndexChanged += new System.EventHandler(this.DoPopulateTags);
			// 
			// searchButton
			// 
			this.searchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.searchButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.searchButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.searchButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.searchButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Search;
			this.searchButton.ImageOver = null;
			this.searchButton.Location = new System.Drawing.Point(869, 12);
			this.searchButton.Name = "searchButton";
			this.searchButton.ShowBorder = true;
			this.searchButton.Size = new System.Drawing.Size(50, 31);
			this.searchButton.StylizeImage = true;
			this.searchButton.TabIndex = 1;
			this.searchButton.ThemedBack = null;
			this.searchButton.ThemedFore = null;
			this.searchButton.UseVisualStyleBackColor = true;
			this.searchButton.Click += new System.EventHandler(this.DoSearchTags);
			// 
			// tagBox
			// 
			this.tagBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tagBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.tagBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tagBox.Location = new System.Drawing.Point(41, 12);
			this.tagBox.Name = "tagBox";
			this.tagBox.ProcessEnterKey = false;
			this.tagBox.Size = new System.Drawing.Size(565, 28);
			this.tagBox.TabIndex = 0;
			this.tagBox.ThemedBack = null;
			this.tagBox.ThemedFore = null;
			// 
			// introBox
			// 
			this.introBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.introBox.Location = new System.Drawing.Point(0, 0);
			this.introBox.Name = "introBox";
			this.introBox.Padding = new System.Windows.Forms.Padding(12);
			this.introBox.Size = new System.Drawing.Size(988, 62);
			this.introBox.TabIndex = 10;
			this.introBox.Text = "Type any part of one or more hashtags. Wildcards are implied unless a tag is ende" +
    "d with a period. Parenthesis and logical operators are allowed.";
			this.introBox.ThemedBack = "Control";
			this.introBox.ThemedFore = "ControlText";
			// 
			// controlPanel
			// 
			this.controlPanel.BackColor = System.Drawing.SystemColors.Control;
			this.controlPanel.Controls.Add(this.scanLink);
			this.controlPanel.Controls.Add(this.lastScanLabel);
			this.controlPanel.Controls.Add(this.indexButton);
			this.controlPanel.Controls.Add(this.copyButton);
			this.controlPanel.Controls.Add(this.moveButton);
			this.controlPanel.Controls.Add(this.cancelButton);
			this.controlPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.controlPanel.Location = new System.Drawing.Point(0, 784);
			this.controlPanel.Name = "controlPanel";
			this.controlPanel.Size = new System.Drawing.Size(988, 60);
			this.controlPanel.TabIndex = 9;
			// 
			// scanLink
			// 
			this.scanLink.ActiveLinkColor = System.Drawing.Color.MediumOrchid;
			this.scanLink.AutoSize = true;
			this.scanLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.scanLink.HoverColor = System.Drawing.Color.Orchid;
			this.scanLink.LinkColor = System.Drawing.Color.MediumOrchid;
			this.scanLink.Location = new System.Drawing.Point(121, 19);
			this.scanLink.Name = "scanLink";
			this.scanLink.Size = new System.Drawing.Size(336, 20);
			this.scanLink.StrictColors = false;
			this.scanLink.TabIndex = 11;
			this.scanLink.TabStop = true;
			this.scanLink.Text = "New notebooks discovered. Click here to scan.";
			this.scanLink.ThemedBack = null;
			this.scanLink.ThemedFore = null;
			this.scanLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.scanLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ScanDiscovered);
			// 
			// lastScanLabel
			// 
			this.lastScanLabel.AutoSize = true;
			this.lastScanLabel.ForeColor = System.Drawing.SystemColors.GrayText;
			this.lastScanLabel.Location = new System.Drawing.Point(12, 19);
			this.lastScanLabel.Name = "lastScanLabel";
			this.lastScanLabel.Size = new System.Drawing.Size(82, 20);
			this.lastScanLabel.TabIndex = 10;
			this.lastScanLabel.Text = "Last scan:";
			// 
			// indexButton
			// 
			this.indexButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.indexButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.indexButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.indexButton.Enabled = false;
			this.indexButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.indexButton.ImageOver = null;
			this.indexButton.Location = new System.Drawing.Point(487, 12);
			this.indexButton.Margin = new System.Windows.Forms.Padding(4, 5, 10, 9);
			this.indexButton.Name = "indexButton";
			this.indexButton.ShowBorder = true;
			this.indexButton.Size = new System.Drawing.Size(112, 35);
			this.indexButton.StylizeImage = false;
			this.indexButton.TabIndex = 1;
			this.indexButton.Text = "Index";
			this.indexButton.ThemedBack = null;
			this.indexButton.ThemedFore = null;
			this.indexButton.UseVisualStyleBackColor = true;
			this.indexButton.Click += new System.EventHandler(this.DoSomething);
			// 
			// copyButton
			// 
			this.copyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.copyButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.copyButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.copyButton.Enabled = false;
			this.copyButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.copyButton.ImageOver = null;
			this.copyButton.Location = new System.Drawing.Point(613, 12);
			this.copyButton.Margin = new System.Windows.Forms.Padding(4, 5, 10, 9);
			this.copyButton.Name = "copyButton";
			this.copyButton.ShowBorder = true;
			this.copyButton.Size = new System.Drawing.Size(112, 35);
			this.copyButton.StylizeImage = false;
			this.copyButton.TabIndex = 2;
			this.copyButton.Text = "Copy";
			this.copyButton.ThemedBack = null;
			this.copyButton.ThemedFore = null;
			this.copyButton.UseVisualStyleBackColor = true;
			this.copyButton.Click += new System.EventHandler(this.DoSomething);
			// 
			// moveButton
			// 
			this.moveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.moveButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.moveButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.moveButton.Enabled = false;
			this.moveButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.moveButton.ImageOver = null;
			this.moveButton.Location = new System.Drawing.Point(739, 13);
			this.moveButton.Margin = new System.Windows.Forms.Padding(4, 5, 10, 9);
			this.moveButton.Name = "moveButton";
			this.moveButton.ShowBorder = true;
			this.moveButton.Size = new System.Drawing.Size(112, 35);
			this.moveButton.StylizeImage = false;
			this.moveButton.TabIndex = 3;
			this.moveButton.Text = "Move";
			this.moveButton.ThemedBack = null;
			this.moveButton.ThemedFore = null;
			this.moveButton.UseVisualStyleBackColor = true;
			this.moveButton.Click += new System.EventHandler(this.DoSomething);
			// 
			// contextMenu
			// 
			this.contextMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.scanButton,
            this.scheduleButton,
            this.offlineNotebooksButton});
			this.contextMenu.Name = "contextMenu";
			this.contextMenu.Size = new System.Drawing.Size(282, 100);
			this.contextMenu.Opened += new System.EventHandler(this.PrepareContextMenu);
			// 
			// scanButton
			// 
			this.scanButton.Name = "scanButton";
			this.scanButton.Size = new System.Drawing.Size(281, 32);
			this.scanButton.Text = "Scan Now";
			this.scanButton.Click += new System.EventHandler(this.ScanNow);
			// 
			// scheduleButton
			// 
			this.scheduleButton.Name = "scheduleButton";
			this.scheduleButton.Size = new System.Drawing.Size(281, 32);
			this.scheduleButton.Text = "Schedule Scan";
			this.scheduleButton.Click += new System.EventHandler(this.DoScheduleScan);
			// 
			// offlineNotebooksButton
			// 
			this.offlineNotebooksButton.Image = global::River.OneMoreAddIn.Properties.Resources.e_CheckMark;
			this.offlineNotebooksButton.Name = "offlineNotebooksButton";
			this.offlineNotebooksButton.Size = new System.Drawing.Size(281, 32);
			this.offlineNotebooksButton.Text = "Hide Offline Notebooks";
			this.offlineNotebooksButton.Click += new System.EventHandler(this.ToggleOfflineNotebooks);
			// 
			// HashtagDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(988, 844);
			this.Controls.Add(this.contextPanel);
			this.Controls.Add(this.controlPanel);
			this.Controls.Add(this.topPanel);
			this.Controls.Add(this.introBox);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.MinimumSize = new System.Drawing.Size(900, 400);
			this.Name = "HashtagDialog";
			this.Text = "Search Hashtags";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SaveSettings);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoKeyDown);
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.controlPanel.ResumeLayout(false);
			this.controlPanel.PerformLayout();
			this.contextMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreButton cancelButton;
		private System.Windows.Forms.FlowLayoutPanel contextPanel;
		private UI.MorePanel topPanel;
		private System.Windows.Forms.Panel controlPanel;
		private UI.MoreTextBox tagBox;
		private UI.MoreButton searchButton;
		private System.Windows.Forms.ComboBox scopeBox;
		private UI.MoreLinkLabel checkAllLink;
		private UI.MoreLinkLabel uncheckAllLink;
		private UI.MoreButton indexButton;
		private UI.MoreButton copyButton;
		private UI.MoreButton moveButton;
		private System.Windows.Forms.Label lastScanLabel;
		private UI.MoreButton menuButton;
		private System.Windows.Forms.ContextMenuStrip contextMenu;
		private System.Windows.Forms.ToolStripMenuItem scanButton;
		private UI.MoreMultilineLabel introBox;
		private System.Windows.Forms.ToolStripMenuItem scheduleButton;
		private System.Windows.Forms.ToolStripMenuItem offlineNotebooksButton;
		private UI.MoreLabel barLabel;
		private System.Windows.Forms.ToolTip tooltip;
		private UI.MoreCheckBox sensitiveBox;
		private UI.MoreLinkLabel scanLink;
	}
}