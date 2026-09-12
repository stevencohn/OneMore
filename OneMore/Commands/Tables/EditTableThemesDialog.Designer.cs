namespace River.OneMoreAddIn.Commands
{
	partial class EditTableThemesDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditTableThemesDialog));
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.saveButton = new River.OneMoreAddIn.UI.MoreButton();
			this.combo = new UI.MoreComboBox();
			this.themeLabel = new UI.MoreLabel();
			this.newButton = new River.OneMoreAddIn.UI.MoreButton();
			this.duplicateButton = new River.OneMoreAddIn.UI.MoreButton();
			this.renameButton = new River.OneMoreAddIn.UI.MoreButton();
			this.deleteButton = new River.OneMoreAddIn.UI.MoreButton();
			this.topPanel = new System.Windows.Forms.Panel();
			this.tabs = new River.OneMoreAddIn.UI.MoreTabControl();
			this.colorsTab = new System.Windows.Forms.TabPage();
			this.colorsListPanel = new System.Windows.Forms.Panel();
			this.previewDockPanel = new System.Windows.Forms.Panel();
			this.previewGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.previewBox = new System.Windows.Forms.PictureBox();
			this.fontsTab = new System.Windows.Forms.TabPage();
			this.fontsListPanel = new System.Windows.Forms.Panel();
			this.fontsGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.familyBox = new River.OneMoreAddIn.UI.FontComboBox();
			this.sizeBox = new UI.MoreComboBox();
			this.fontToolstrip = new River.OneMoreAddIn.UI.MoreToolStrip();
			this.boldButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.italicButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.underlineButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.colorButton = new River.OneMoreAddIn.UI.MoreSplitButton();
			this.defaultBlackToolStripMenuItem = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.sampleBorderPanel = new System.Windows.Forms.Panel();
			this.sampleLabel = new UI.MoreLabel();
			this.resetToDefaultLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.applyFontButton = new River.OneMoreAddIn.UI.MoreButton();
			this.bottomPanel = new System.Windows.Forms.Panel();
			this.resetAllLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.topPanel.SuspendLayout();
			this.tabs.SuspendLayout();
			this.colorsTab.SuspendLayout();
			this.previewDockPanel.SuspendLayout();
			this.previewGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.previewBox)).BeginInit();
			this.fontsTab.SuspendLayout();
			this.fontsGroup.SuspendLayout();
			this.fontToolstrip.SuspendLayout();
			this.sampleBorderPanel.SuspendLayout();
			this.bottomPanel.SuspendLayout();
			this.SuspendLayout();
			//
			// cancelButton
			//
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(710, 10);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(110, 34);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 3;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			//
			// saveButton
			//
			this.saveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.saveButton.Enabled = false;
			this.saveButton.ImageOver = null;
			this.saveButton.Location = new System.Drawing.Point(594, 10);
			this.saveButton.Name = "saveButton";
			this.saveButton.ShowBorder = true;
			this.saveButton.Size = new System.Drawing.Size(110, 34);
			this.saveButton.StylizeImage = false;
			this.saveButton.TabIndex = 2;
			this.saveButton.Text = "Save";
			this.saveButton.UseVisualStyleBackColor = true;
			this.saveButton.Click += new System.EventHandler(this.SaveAll);
			//
			// combo
			//
			this.combo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.combo.FormattingEnabled = true;
			this.combo.Location = new System.Drawing.Point(63, 7);
			this.combo.Name = "combo";
			this.combo.Size = new System.Drawing.Size(346, 28);
			this.combo.TabIndex = 0;
			this.combo.SelectedIndexChanged += new System.EventHandler(this.ChooseTheme);
			//
			// themeLabel
			//
			this.themeLabel.AutoSize = true;
			this.themeLabel.Location = new System.Drawing.Point(3, 13);
			this.themeLabel.Name = "themeLabel";
			this.themeLabel.Size = new System.Drawing.Size(51, 20);
			this.themeLabel.TabIndex = 7;
			this.themeLabel.Text = "Theme";
			//
			// newButton
			//
			this.newButton.Location = new System.Drawing.Point(415, 5);
			this.newButton.Name = "newButton";
			this.newButton.ShowBorder = true;
			this.newButton.Size = new System.Drawing.Size(90, 30);
			this.newButton.StylizeImage = false;
			this.newButton.TabIndex = 1;
			this.newButton.Text = "New";
			this.newButton.UseVisualStyleBackColor = true;
			this.newButton.Click += new System.EventHandler(this.CreateNewTheme);
			//
			// duplicateButton
			//
			this.duplicateButton.Location = new System.Drawing.Point(511, 5);
			this.duplicateButton.Name = "duplicateButton";
			this.duplicateButton.ShowBorder = true;
			this.duplicateButton.Size = new System.Drawing.Size(90, 30);
			this.duplicateButton.StylizeImage = false;
			this.duplicateButton.TabIndex = 2;
			this.duplicateButton.Text = "Duplicate";
			this.duplicateButton.UseVisualStyleBackColor = true;
			this.duplicateButton.Click += new System.EventHandler(this.ShowDuplicateGallery);
			//
			// renameButton
			//
			this.renameButton.Location = new System.Drawing.Point(607, 5);
			this.renameButton.Name = "renameButton";
			this.renameButton.ShowBorder = true;
			this.renameButton.Size = new System.Drawing.Size(90, 30);
			this.renameButton.StylizeImage = false;
			this.renameButton.TabIndex = 3;
			this.renameButton.Text = "Rename";
			this.renameButton.UseVisualStyleBackColor = true;
			this.renameButton.Click += new System.EventHandler(this.RenameTheme);
			//
			// deleteButton
			//
			this.deleteButton.Location = new System.Drawing.Point(703, 5);
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.ShowBorder = true;
			this.deleteButton.Size = new System.Drawing.Size(90, 30);
			this.deleteButton.StylizeImage = false;
			this.deleteButton.TabIndex = 4;
			this.deleteButton.Text = "Delete";
			this.deleteButton.ThemedFore = "ErrorText";
			this.deleteButton.UseVisualStyleBackColor = true;
			this.deleteButton.Click += new System.EventHandler(this.DeleteTheme);
			//
			// topPanel
			//
			this.topPanel.Controls.Add(this.themeLabel);
			this.topPanel.Controls.Add(this.combo);
			this.topPanel.Controls.Add(this.newButton);
			this.topPanel.Controls.Add(this.duplicateButton);
			this.topPanel.Controls.Add(this.renameButton);
			this.topPanel.Controls.Add(this.deleteButton);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = new System.Drawing.Point(20, 20);
			this.topPanel.Name = "topPanel";
			this.topPanel.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.topPanel.Size = new System.Drawing.Size(829, 64);
			this.topPanel.TabIndex = 11;
			//
			// tabs
			//
			this.tabs.Background = "ControlLight";
			this.tabs.Controls.Add(this.colorsTab);
			this.tabs.Controls.Add(this.fontsTab);
			this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabs.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
			this.tabs.InactiveTabBack = "ControlDarkDark";
			this.tabs.InactiveTabFore = "DarkText";
			this.tabs.Location = new System.Drawing.Point(20, 84);
			this.tabs.Margin = new System.Windows.Forms.Padding(0);
			this.tabs.Name = "tabs";
			this.tabs.Padding = new System.Drawing.Point(0, 0);
			this.tabs.SelectedIndex = 0;
			this.tabs.Size = new System.Drawing.Size(829, 554);
			this.tabs.TabIndex = 12;
			//
			// colorsTab
			//
			this.colorsTab.BackColor = System.Drawing.SystemColors.Window;
			this.colorsTab.Controls.Add(this.colorsListPanel);
			this.colorsTab.Controls.Add(this.previewDockPanel);
			this.colorsTab.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.colorsTab.ForeColor = System.Drawing.SystemColors.WindowText;
			this.colorsTab.Location = new System.Drawing.Point(4, 29);
			this.colorsTab.Name = "colorsTab";
			this.colorsTab.Padding = new System.Windows.Forms.Padding(5, 15, 5, 5);
			this.colorsTab.Size = new System.Drawing.Size(821, 541);
			this.colorsTab.TabIndex = 0;
			this.colorsTab.Text = "Colors";
			//
			// colorsListPanel
			//
			this.colorsListPanel.AutoScroll = true;
			this.colorsListPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.colorsListPanel.Location = new System.Drawing.Point(5, 15);
			this.colorsListPanel.Name = "colorsListPanel";
			this.colorsListPanel.Size = new System.Drawing.Size(415, 521);
			this.colorsListPanel.TabIndex = 8;
			//
			// previewDockPanel
			//
			this.previewDockPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.previewDockPanel.Controls.Add(this.previewGroup);
			this.previewDockPanel.Location = new System.Drawing.Point(508, 18);
			this.previewDockPanel.Name = "previewDockPanel";
			this.previewDockPanel.Size = new System.Drawing.Size(308, 518);
			this.previewDockPanel.TabIndex = 10;
			//
			// previewGroup
			//
			this.previewGroup.Controls.Add(this.previewBox);
			this.previewGroup.Location = new System.Drawing.Point(17, 3);
			this.previewGroup.Name = "previewGroup";
			this.previewGroup.Padding = new System.Windows.Forms.Padding(10);
			this.previewGroup.Size = new System.Drawing.Size(217, 199);
			this.previewGroup.TabIndex = 9;
			this.previewGroup.TabStop = false;
			this.previewGroup.Text = "Preview";
			//
			// previewBox
			//
			this.previewBox.BackColor = System.Drawing.SystemColors.Window;
			this.previewBox.Location = new System.Drawing.Point(13, 32);
			this.previewBox.Name = "previewBox";
			this.previewBox.Size = new System.Drawing.Size(190, 125);
			this.previewBox.TabIndex = 0;
			this.previewBox.TabStop = false;
			//
			// fontsTab
			//
			this.fontsTab.BackColor = System.Drawing.SystemColors.Window;
			this.fontsTab.Controls.Add(this.fontsGroup);
			this.fontsTab.Controls.Add(this.fontsListPanel);
			this.fontsTab.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.fontsTab.ForeColor = System.Drawing.SystemColors.WindowText;
			this.fontsTab.Location = new System.Drawing.Point(4, 29);
			this.fontsTab.Name = "fontsTab";
			this.fontsTab.Padding = new System.Windows.Forms.Padding(5, 15, 5, 5);
			this.fontsTab.Size = new System.Drawing.Size(821, 541);
			this.fontsTab.TabIndex = 1;
			this.fontsTab.Text = "Fonts";
			//
			// fontsListPanel
			//
			this.fontsListPanel.AutoScroll = true;
			this.fontsListPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.fontsListPanel.Location = new System.Drawing.Point(5, 15);
			this.fontsListPanel.Name = "fontsListPanel";
			this.fontsListPanel.Size = new System.Drawing.Size(280, 521);
			this.fontsListPanel.TabIndex = 15;
			//
			// fontsGroup
			//
			this.fontsGroup.Controls.Add(this.applyFontButton);
			this.fontsGroup.Controls.Add(this.resetToDefaultLink);
			this.fontsGroup.Controls.Add(this.sampleBorderPanel);
			this.fontsGroup.Controls.Add(this.fontToolstrip);
			this.fontsGroup.Controls.Add(this.sizeBox);
			this.fontsGroup.Controls.Add(this.familyBox);
			this.fontsGroup.Dock = System.Windows.Forms.DockStyle.Fill;
			this.fontsGroup.Location = new System.Drawing.Point(5, 15);
			this.fontsGroup.Name = "fontsGroup";
			this.fontsGroup.Padding = new System.Windows.Forms.Padding(10, 3, 10, 3);
			this.fontsGroup.ShowOnlyTopEdge = true;
			this.fontsGroup.Size = new System.Drawing.Size(531, 521);
			this.fontsGroup.TabIndex = 16;
			this.fontsGroup.TabStop = false;
			this.fontsGroup.Text = "Font";
			//
			// familyBox
			//
			this.familyBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.familyBox.DropDownHeight = 400;
			this.familyBox.DropDownWidth = 350;
			this.familyBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.familyBox.FormattingEnabled = true;
			this.familyBox.IntegralHeight = false;
			this.familyBox.Location = new System.Drawing.Point(13, 30);
			this.familyBox.Name = "familyBox";
			this.familyBox.Size = new System.Drawing.Size(310, 32);
			this.familyBox.TabIndex = 0;
			this.familyBox.SelectedIndexChanged += new System.EventHandler(this.ChangeFontFont);
			//
			// sizeBox
			//
			this.sizeBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.sizeBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.sizeBox.FormattingEnabled = true;
			this.sizeBox.Items.AddRange(new object[] {
			"8",
			"9",
			"9.5",
			"10",
			"10.5",
			"11",
			"11.5",
			"12",
			"14",
			"16",
			"18",
			"20",
			"22",
			"24",
			"26"});
			this.sizeBox.Location = new System.Drawing.Point(333, 30);
			this.sizeBox.Name = "sizeBox";
			this.sizeBox.Size = new System.Drawing.Size(90, 33);
			this.sizeBox.TabIndex = 1;
			this.sizeBox.SelectedIndexChanged += new System.EventHandler(this.ChangeFontFont);
			//
			// fontToolstrip
			//
			this.fontToolstrip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.fontToolstrip.Dock = System.Windows.Forms.DockStyle.None;
			this.fontToolstrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.fontToolstrip.ImageScalingSize = new System.Drawing.Size(18, 18);
			this.fontToolstrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.boldButton,
			this.italicButton,
			this.underlineButton,
			this.colorButton});
			this.fontToolstrip.Location = new System.Drawing.Point(13, 72);
			this.fontToolstrip.Name = "fontToolstrip";
			this.fontToolstrip.Padding = new System.Windows.Forms.Padding(0);
			this.fontToolstrip.Size = new System.Drawing.Size(147, 27);
			this.fontToolstrip.TabIndex = 2;
			//
			// boldButton
			//
			this.boldButton.CheckOnClick = true;
			this.boldButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.boldButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Bold;
			this.boldButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.boldButton.Margin = new System.Windows.Forms.Padding(0, 2, 1, 3);
			this.boldButton.Name = "boldButton";
			this.boldButton.Size = new System.Drawing.Size(34, 22);
			this.boldButton.Text = "Bold";
			this.boldButton.CheckStateChanged += new System.EventHandler(this.ChangeFontFont);
			//
			// italicButton
			//
			this.italicButton.CheckOnClick = true;
			this.italicButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.italicButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Italic;
			this.italicButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.italicButton.Margin = new System.Windows.Forms.Padding(0, 2, 1, 3);
			this.italicButton.Name = "italicButton";
			this.italicButton.Size = new System.Drawing.Size(34, 22);
			this.italicButton.Text = "Italic";
			this.italicButton.CheckStateChanged += new System.EventHandler(this.ChangeFontFont);
			//
			// underlineButton
			//
			this.underlineButton.CheckOnClick = true;
			this.underlineButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.underlineButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Underline;
			this.underlineButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.underlineButton.Margin = new System.Windows.Forms.Padding(0, 2, 1, 3);
			this.underlineButton.Name = "underlineButton";
			this.underlineButton.Size = new System.Drawing.Size(34, 22);
			this.underlineButton.Text = "Underline";
			this.underlineButton.CheckStateChanged += new System.EventHandler(this.ChangeFontFont);
			//
			// colorButton
			//
			this.colorButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.colorButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.defaultBlackToolStripMenuItem});
			this.colorButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_FontColor;
			this.colorButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.colorButton.Margin = new System.Windows.Forms.Padding(0, 2, 1, 3);
			this.colorButton.Name = "colorButton";
			this.colorButton.Size = new System.Drawing.Size(39, 22);
			this.colorButton.ToolTipText = "Text Color";
			this.colorButton.ButtonClick += new System.EventHandler(this.ChangeFontColor);
			//
			// defaultBlackToolStripMenuItem
			//
			this.defaultBlackToolStripMenuItem.Image = null;
			this.defaultBlackToolStripMenuItem.Name = "defaultBlackToolStripMenuItem";
			this.defaultBlackToolStripMenuItem.Size = new System.Drawing.Size(226, 34);
			this.defaultBlackToolStripMenuItem.Text = "Default (Black)";
			this.defaultBlackToolStripMenuItem.Click += new System.EventHandler(this.SetFontColorDefault);
			//
			// sampleBorderPanel
			//
			this.sampleBorderPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.sampleBorderPanel.Controls.Add(this.sampleLabel);
			this.sampleBorderPanel.Location = new System.Drawing.Point(13, 112);
			this.sampleBorderPanel.Name = "sampleBorderPanel";
			this.sampleBorderPanel.Padding = new System.Windows.Forms.Padding(16);
			this.sampleBorderPanel.Size = new System.Drawing.Size(505, 60);
			this.sampleBorderPanel.TabIndex = 3;
			//
			// sampleLabel
			//
			this.sampleLabel.AutoSize = false;
			this.sampleLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sampleLabel.Name = "sampleLabel";
			this.sampleLabel.TabIndex = 0;
			this.sampleLabel.Text = "Table text sample";
			this.sampleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			//
			// resetToDefaultLink
			//
			this.resetToDefaultLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.resetToDefaultLink.AutoSize = true;
			this.resetToDefaultLink.Location = new System.Drawing.Point(13, 475);
			this.resetToDefaultLink.Name = "resetToDefaultLink";
			this.resetToDefaultLink.TabIndex = 4;
			this.resetToDefaultLink.TabStop = true;
			this.resetToDefaultLink.Text = "Reset to default";
			this.resetToDefaultLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ResetFontRoleToDefault);
			//
			// applyFontButton
			//
			this.applyFontButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.applyFontButton.ImageOver = null;
			this.applyFontButton.Location = new System.Drawing.Point(408, 468);
			this.applyFontButton.Name = "applyFontButton";
			this.applyFontButton.ShowBorder = true;
			this.applyFontButton.Size = new System.Drawing.Size(110, 34);
			this.applyFontButton.StylizeImage = false;
			this.applyFontButton.TabIndex = 5;
			this.applyFontButton.Text = "Apply";
			this.applyFontButton.UseVisualStyleBackColor = true;
			this.applyFontButton.Click += new System.EventHandler(this.ApplyFont);
			//
			// bottomPanel
			//
			this.bottomPanel.Controls.Add(this.resetAllLink);
			this.bottomPanel.Controls.Add(this.saveButton);
			this.bottomPanel.Controls.Add(this.cancelButton);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = new System.Drawing.Point(20, 638);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Padding = new System.Windows.Forms.Padding(6);
			this.bottomPanel.Size = new System.Drawing.Size(829, 53);
			this.bottomPanel.TabIndex = 13;
			//
			// resetAllLink
			//
			this.resetAllLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.resetAllLink.AutoSize = true;
			this.resetAllLink.Location = new System.Drawing.Point(9, 18);
			this.resetAllLink.Name = "resetAllLink";
			this.resetAllLink.TabIndex = 0;
			this.resetAllLink.TabStop = true;
			this.resetAllLink.Text = "Reset All";
			this.resetAllLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ResetAllColors);
			//
			// EditTableThemesDialog
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(1000, 80);
			this.Controls.Add(this.tabs);
			this.Controls.Add(this.topPanel);
			this.Controls.Add(this.bottomPanel);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(880, 750);
			this.Name = "EditTableThemesDialog";
			this.Padding = new System.Windows.Forms.Padding(20, 20, 10, 10);
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Custom Table Styles";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConfirmClosing);
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.tabs.ResumeLayout(false);
			this.colorsTab.ResumeLayout(false);
			this.previewDockPanel.ResumeLayout(false);
			this.previewGroup.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.previewBox)).EndInit();
			this.fontsTab.ResumeLayout(false);
			this.fontsGroup.ResumeLayout(false);
			this.fontsGroup.PerformLayout();
			this.fontToolstrip.ResumeLayout(false);
			this.fontToolstrip.PerformLayout();
			this.sampleBorderPanel.ResumeLayout(false);
			this.bottomPanel.ResumeLayout(false);
			this.bottomPanel.PerformLayout();
			this.ResumeLayout(false);

		}


		#endregion

		private UI.MoreButton cancelButton;
		private UI.MoreButton saveButton;
		private UI.MoreComboBox combo;
		private UI.MoreLabel themeLabel;
		private UI.MoreButton newButton;
		private UI.MoreButton duplicateButton;
		private UI.MoreButton renameButton;
		private UI.MoreButton deleteButton;
		private System.Windows.Forms.Panel topPanel;
		private UI.MoreTabControl tabs;
		private System.Windows.Forms.TabPage colorsTab;
		private System.Windows.Forms.Panel colorsListPanel;
		private System.Windows.Forms.Panel previewDockPanel;
		private UI.MoreGroupBox previewGroup;
		private System.Windows.Forms.PictureBox previewBox;
		private System.Windows.Forms.TabPage fontsTab;
		private System.Windows.Forms.Panel fontsListPanel;
		private UI.MoreGroupBox fontsGroup;
		private UI.FontComboBox familyBox;
		private UI.MoreComboBox sizeBox;
		private UI.MoreToolStrip fontToolstrip;
		private UI.MoreMenuItem boldButton;
		private UI.MoreMenuItem italicButton;
		private UI.MoreMenuItem underlineButton;
		private UI.MoreSplitButton colorButton;
		private UI.MoreMenuItem defaultBlackToolStripMenuItem;
		private System.Windows.Forms.Panel sampleBorderPanel;
		private UI.MoreLabel sampleLabel;
		private UI.MoreLinkLabel resetToDefaultLink;
		private UI.MoreButton applyFontButton;
		private System.Windows.Forms.Panel bottomPanel;
		private UI.MoreLinkLabel resetAllLink;
	}
}
