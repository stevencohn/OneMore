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
			this.introLabel = new River.OneMoreAddIn.UI.MoreTextBox();
			this.menuButton = new River.OneMoreAddIn.UI.MoreButton();
			this.barLabel = new System.Windows.Forms.Label();
			this.checkAllLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.uncheckAllLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.scopeBox = new System.Windows.Forms.ComboBox();
			this.searchButton = new River.OneMoreAddIn.UI.MoreButton();
			this.tagBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.lastScanLabel = new System.Windows.Forms.Label();
			this.indexButton = new River.OneMoreAddIn.UI.MoreButton();
			this.copyButton = new River.OneMoreAddIn.UI.MoreButton();
			this.moveButton = new River.OneMoreAddIn.UI.MoreButton();
			this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.scanButton = new System.Windows.Forms.ToolStripMenuItem();
			this.topPanel.SuspendLayout();
			this.panel1.SuspendLayout();
			this.contextMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(864, 13);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.PreferredBack = System.Drawing.Color.Empty;
			this.cancelButton.PreferredFore = System.Drawing.Color.Empty;
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(112, 35);
			this.cancelButton.TabIndex = 6;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.DoCancel);
			// 
			// contextPanel
			// 
			this.contextPanel.AutoScroll = true;
			this.contextPanel.BackColor = System.Drawing.SystemColors.AppWorkspace;
			this.contextPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.contextPanel.Location = new System.Drawing.Point(0, 144);
			this.contextPanel.Name = "contextPanel";
			this.contextPanel.Padding = new System.Windows.Forms.Padding(6);
			this.contextPanel.Size = new System.Drawing.Size(988, 640);
			this.contextPanel.TabIndex = 7;
			// 
			// topPanel
			// 
			this.topPanel.BackColor = System.Drawing.SystemColors.Window;
			this.topPanel.BottomBorderColor = System.Drawing.SystemColors.WindowFrame;
			this.topPanel.BottomBorderSize = 1;
			this.topPanel.Controls.Add(this.introLabel);
			this.topPanel.Controls.Add(this.menuButton);
			this.topPanel.Controls.Add(this.barLabel);
			this.topPanel.Controls.Add(this.checkAllLink);
			this.topPanel.Controls.Add(this.uncheckAllLink);
			this.topPanel.Controls.Add(this.scopeBox);
			this.topPanel.Controls.Add(this.searchButton);
			this.topPanel.Controls.Add(this.tagBox);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = new System.Drawing.Point(0, 0);
			this.topPanel.Margin = new System.Windows.Forms.Padding(0);
			this.topPanel.Name = "topPanel";
			this.topPanel.Padding = new System.Windows.Forms.Padding(10);
			this.topPanel.Size = new System.Drawing.Size(988, 144);
			this.topPanel.TabIndex = 8;
			this.topPanel.TopBorderColor = System.Drawing.SystemColors.Control;
			this.topPanel.TopBorderSize = 0;
			// 
			// introLabel
			// 
			this.introLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.introLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.introLabel.Location = new System.Drawing.Point(41, 13);
			this.introLabel.Multiline = true;
			this.introLabel.Name = "introLabel";
			this.introLabel.PreferredBack = System.Drawing.Color.Empty;
			this.introLabel.PreferredFore = System.Drawing.Color.Empty;
			this.introLabel.Size = new System.Drawing.Size(864, 48);
			this.introLabel.TabIndex = 10;
			this.introLabel.Text = "Type any part of one or more hashtags. Wildcards are implied unless a tag is ende" +
    "d with a period. Parenthesis and logical operators are allowed.";
			// 
			// menuButton
			// 
			this.menuButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.menuButton.FlatAppearance.BorderSize = 0;
			this.menuButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.menuButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.menuButton.ForeColor = System.Drawing.SystemColors.GrayText;
			this.menuButton.ImageOver = null;
			this.menuButton.Location = new System.Drawing.Point(925, 68);
			this.menuButton.Name = "menuButton";
			this.menuButton.PreferredBack = System.Drawing.Color.Empty;
			this.menuButton.PreferredFore = System.Drawing.Color.Empty;
			this.menuButton.ShowBorder = true;
			this.menuButton.Size = new System.Drawing.Size(50, 38);
			this.menuButton.TabIndex = 8;
			this.menuButton.Text = "•••";
			this.menuButton.UseVisualStyleBackColor = true;
			this.menuButton.Click += new System.EventHandler(this.ShowMenu);
			// 
			// barLabel
			// 
			this.barLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.barLabel.AutoSize = true;
			this.barLabel.Location = new System.Drawing.Point(808, 114);
			this.barLabel.Name = "barLabel";
			this.barLabel.Size = new System.Drawing.Size(14, 20);
			this.barLabel.TabIndex = 7;
			this.barLabel.Text = "|";
			// 
			// checkAllLink
			// 
			this.checkAllLink.ActiveLinkColor = System.Drawing.Color.Orchid;
			this.checkAllLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.checkAllLink.AutoSize = true;
			this.checkAllLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.checkAllLink.HoverColor = System.Drawing.Color.MediumOrchid;
			this.checkAllLink.LinkColor = System.Drawing.Color.MediumOrchid;
			this.checkAllLink.Location = new System.Drawing.Point(729, 114);
			this.checkAllLink.Name = "checkAllLink";
			this.checkAllLink.Size = new System.Drawing.Size(73, 20);
			this.checkAllLink.TabIndex = 6;
			this.checkAllLink.TabStop = true;
			this.checkAllLink.Text = "Check all";
			this.checkAllLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.checkAllLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ToggleAllChecks);
			// 
			// uncheckAllLink
			// 
			this.uncheckAllLink.ActiveLinkColor = System.Drawing.Color.Orchid;
			this.uncheckAllLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.uncheckAllLink.AutoSize = true;
			this.uncheckAllLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.uncheckAllLink.HoverColor = System.Drawing.Color.MediumOrchid;
			this.uncheckAllLink.LinkColor = System.Drawing.Color.MediumOrchid;
			this.uncheckAllLink.Location = new System.Drawing.Point(828, 114);
			this.uncheckAllLink.Name = "uncheckAllLink";
			this.uncheckAllLink.Size = new System.Drawing.Size(91, 20);
			this.uncheckAllLink.TabIndex = 5;
			this.uncheckAllLink.TabStop = true;
			this.uncheckAllLink.Text = "Uncheck all";
			this.uncheckAllLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.uncheckAllLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ToggleAllChecks);
			// 
			// scopeBox
			// 
			this.scopeBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.scopeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.scopeBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.scopeBox.FormattingEnabled = true;
			this.scopeBox.Items.AddRange(new object[] {
            "All",
            "This notebook",
            "This section"});
			this.scopeBox.Location = new System.Drawing.Point(655, 75);
			this.scopeBox.Name = "scopeBox";
			this.scopeBox.Size = new System.Drawing.Size(196, 28);
			this.scopeBox.TabIndex = 4;
			this.scopeBox.SelectedIndexChanged += new System.EventHandler(this.PopulateTags);
			// 
			// searchButton
			// 
			this.searchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.searchButton.Image = ((System.Drawing.Image)(resources.GetObject("searchButton.Image")));
			this.searchButton.ImageOver = null;
			this.searchButton.Location = new System.Drawing.Point(859, 72);
			this.searchButton.Name = "searchButton";
			this.searchButton.PreferredBack = System.Drawing.Color.Empty;
			this.searchButton.PreferredFore = System.Drawing.Color.Empty;
			this.searchButton.ShowBorder = false;
			this.searchButton.Size = new System.Drawing.Size(60, 32);
			this.searchButton.TabIndex = 2;
			this.searchButton.UseVisualStyleBackColor = true;
			this.searchButton.Click += new System.EventHandler(this.SearchTags);
			// 
			// tagBox
			// 
			this.tagBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tagBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.tagBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tagBox.Location = new System.Drawing.Point(41, 75);
			this.tagBox.Name = "tagBox";
			this.tagBox.PreferredBack = System.Drawing.Color.Empty;
			this.tagBox.PreferredFore = System.Drawing.Color.Empty;
			this.tagBox.Size = new System.Drawing.Size(608, 28);
			this.tagBox.TabIndex = 0;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.lastScanLabel);
			this.panel1.Controls.Add(this.indexButton);
			this.panel1.Controls.Add(this.copyButton);
			this.panel1.Controls.Add(this.moveButton);
			this.panel1.Controls.Add(this.cancelButton);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 784);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(988, 60);
			this.panel1.TabIndex = 9;
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
			this.indexButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.indexButton.Enabled = false;
			this.indexButton.ImageOver = null;
			this.indexButton.Location = new System.Drawing.Point(487, 12);
			this.indexButton.Margin = new System.Windows.Forms.Padding(4, 5, 10, 9);
			this.indexButton.Name = "indexButton";
			this.indexButton.PreferredBack = System.Drawing.Color.Empty;
			this.indexButton.PreferredFore = System.Drawing.Color.Empty;
			this.indexButton.ShowBorder = true;
			this.indexButton.Size = new System.Drawing.Size(112, 35);
			this.indexButton.TabIndex = 7;
			this.indexButton.Text = "Index";
			this.indexButton.UseVisualStyleBackColor = true;
			this.indexButton.Click += new System.EventHandler(this.DoSomething);
			// 
			// copyButton
			// 
			this.copyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.copyButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.copyButton.Enabled = false;
			this.copyButton.ImageOver = null;
			this.copyButton.Location = new System.Drawing.Point(613, 12);
			this.copyButton.Margin = new System.Windows.Forms.Padding(4, 5, 10, 9);
			this.copyButton.Name = "copyButton";
			this.copyButton.PreferredBack = System.Drawing.Color.Empty;
			this.copyButton.PreferredFore = System.Drawing.Color.Empty;
			this.copyButton.ShowBorder = true;
			this.copyButton.Size = new System.Drawing.Size(112, 35);
			this.copyButton.TabIndex = 8;
			this.copyButton.Text = "Copy";
			this.copyButton.UseVisualStyleBackColor = true;
			this.copyButton.Click += new System.EventHandler(this.DoSomething);
			// 
			// moveButton
			// 
			this.moveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.moveButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.moveButton.Enabled = false;
			this.moveButton.ImageOver = null;
			this.moveButton.Location = new System.Drawing.Point(739, 13);
			this.moveButton.Margin = new System.Windows.Forms.Padding(4, 5, 10, 9);
			this.moveButton.Name = "moveButton";
			this.moveButton.PreferredBack = System.Drawing.Color.Empty;
			this.moveButton.PreferredFore = System.Drawing.Color.Empty;
			this.moveButton.ShowBorder = true;
			this.moveButton.Size = new System.Drawing.Size(112, 35);
			this.moveButton.TabIndex = 9;
			this.moveButton.Text = "Move";
			this.moveButton.UseVisualStyleBackColor = true;
			this.moveButton.Click += new System.EventHandler(this.DoSomething);
			// 
			// contextMenu
			// 
			this.contextMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.scanButton});
			this.contextMenu.Name = "contextMenu";
			this.contextMenu.Size = new System.Drawing.Size(164, 36);
			// 
			// scanButton
			// 
			this.scanButton.Name = "scanButton";
			this.scanButton.Size = new System.Drawing.Size(163, 32);
			this.scanButton.Text = "Scan Now";
			this.scanButton.Click += new System.EventHandler(this.ScanNow);
			// 
			// HashtagDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(988, 844);
			this.Controls.Add(this.contextPanel);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.topPanel);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(900, 400);
			this.Name = "HashtagDialog";
			this.Text = "Find Hashtags";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoKeyDown);
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.contextMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreButton cancelButton;
		private System.Windows.Forms.FlowLayoutPanel contextPanel;
		private UI.MorePanel topPanel;
		private System.Windows.Forms.Panel panel1;
		private UI.MoreTextBox tagBox;
		private UI.MoreButton searchButton;
		private System.Windows.Forms.ComboBox scopeBox;
		private System.Windows.Forms.Label barLabel;
		private UI.MoreLinkLabel checkAllLink;
		private UI.MoreLinkLabel uncheckAllLink;
		private UI.MoreButton indexButton;
		private UI.MoreButton copyButton;
		private UI.MoreButton moveButton;
		private System.Windows.Forms.Label lastScanLabel;
		private UI.MoreButton menuButton;
		private System.Windows.Forms.ContextMenuStrip contextMenu;
		private System.Windows.Forms.ToolStripMenuItem scanButton;
		private UI.MoreTextBox introLabel;
	}
}