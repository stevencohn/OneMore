using System.Drawing;

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
			this.cancelButton = new System.Windows.Forms.Button();
			this.combo = new System.Windows.Forms.ComboBox();
			this.nameLabel = new System.Windows.Forms.Label();
			this.elementsGroup = new System.Windows.Forms.GroupBox();
			this.elementsBox = new River.OneMoreAddIn.UI.MoreListView();
			this.resetButtonPanel = new System.Windows.Forms.Panel();
			this.resetButton = new System.Windows.Forms.Button();
			this.previewGroup = new System.Windows.Forms.GroupBox();
			this.previewBox = new System.Windows.Forms.PictureBox();
			this.toolstrip = new River.OneMoreAddIn.UI.ScaledToolStrip();
			this.newButton = new System.Windows.Forms.ToolStripButton();
			this.renameButton = new System.Windows.Forms.ToolStripButton();
			this.saveButton = new System.Windows.Forms.ToolStripButton();
			this.toolsep1 = new System.Windows.Forms.ToolStripSeparator();
			this.deleteButton = new System.Windows.Forms.ToolStripButton();
			this.topPanel = new System.Windows.Forms.Panel();
			this.tabs = new System.Windows.Forms.TabControl();
			this.colorsTab = new System.Windows.Forms.TabPage();
			this.previewDockPanel = new System.Windows.Forms.Panel();
			this.fontsTab = new System.Windows.Forms.TabPage();
			this.fontsGroup = new System.Windows.Forms.GroupBox();
			this.applyFontButton = new System.Windows.Forms.Button();
			this.familyBox = new River.OneMoreAddIn.UI.FontComboBox();
			this.sizeBox = new System.Windows.Forms.ComboBox();
			this.fontToolstrip = new River.OneMoreAddIn.UI.ScaledToolStrip();
			this.boldButton = new System.Windows.Forms.ToolStripButton();
			this.italicButton = new System.Windows.Forms.ToolStripButton();
			this.underlineButton = new System.Windows.Forms.ToolStripButton();
			this.colorButton = new System.Windows.Forms.ToolStripSplitButton();
			this.defaultBlackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.fontElementsGroup = new System.Windows.Forms.GroupBox();
			this.defaultFontButton = new System.Windows.Forms.Button();
			this.selectedFontLabel = new System.Windows.Forms.Label();
			this.resetFontButton = new System.Windows.Forms.Button();
			this.colorFontsBox = new River.OneMoreAddIn.UI.MoreListView();
			this.bottomPanel = new System.Windows.Forms.Panel();
			this.button1 = new System.Windows.Forms.Button();
			this.elementsGroup.SuspendLayout();
			this.resetButtonPanel.SuspendLayout();
			this.previewGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.previewBox)).BeginInit();
			this.toolstrip.SuspendLayout();
			this.topPanel.SuspendLayout();
			this.tabs.SuspendLayout();
			this.colorsTab.SuspendLayout();
			this.previewDockPanel.SuspendLayout();
			this.fontsTab.SuspendLayout();
			this.fontsGroup.SuspendLayout();
			this.fontToolstrip.SuspendLayout();
			this.fontElementsGroup.SuspendLayout();
			this.bottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(710, 10);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(110, 34);
			this.cancelButton.TabIndex = 2;
			this.cancelButton.Text = "Close";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// combo
			// 
			this.combo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.combo.FormattingEnabled = true;
			this.combo.Location = new System.Drawing.Point(93, 7);
			this.combo.Name = "combo";
			this.combo.Size = new System.Drawing.Size(406, 28);
			this.combo.TabIndex = 0;
			this.combo.SelectedIndexChanged += new System.EventHandler(this.ChooseTheme);
			// 
			// nameLabel
			// 
			this.nameLabel.AutoSize = true;
			this.nameLabel.Location = new System.Drawing.Point(3, 13);
			this.nameLabel.Name = "nameLabel";
			this.nameLabel.Size = new System.Drawing.Size(51, 20);
			this.nameLabel.TabIndex = 7;
			this.nameLabel.Text = "Name";
			// 
			// elementsGroup
			// 
			this.elementsGroup.Controls.Add(this.elementsBox);
			this.elementsGroup.Controls.Add(this.resetButtonPanel);
			this.elementsGroup.Dock = System.Windows.Forms.DockStyle.Fill;
			this.elementsGroup.Location = new System.Drawing.Point(3, 3);
			this.elementsGroup.Name = "elementsGroup";
			this.elementsGroup.Padding = new System.Windows.Forms.Padding(10, 7, 7, 7);
			this.elementsGroup.Size = new System.Drawing.Size(507, 513);
			this.elementsGroup.TabIndex = 8;
			this.elementsGroup.TabStop = false;
			this.elementsGroup.Text = "Table Elements";
			// 
			// elementsBox
			// 
			this.elementsBox.ControlPadding = 2;
			this.elementsBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.elementsBox.FullRowSelect = true;
			this.elementsBox.HideSelection = false;
			this.elementsBox.HighlightBackground = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(193)))), ((int)(((byte)(255)))));
			this.elementsBox.HighlightForeground = System.Drawing.SystemColors.HighlightText;
			this.elementsBox.Location = new System.Drawing.Point(10, 26);
			this.elementsBox.Name = "elementsBox";
			this.elementsBox.RowHeight = 28;
			this.elementsBox.Size = new System.Drawing.Size(490, 431);
			this.elementsBox.SortedBackground = System.Drawing.SystemColors.Window;
			this.elementsBox.TabIndex = 0;
			this.elementsBox.UseCompatibleStateImageBehavior = false;
			this.elementsBox.View = System.Windows.Forms.View.Details;
			// 
			// resetButtonPanel
			// 
			this.resetButtonPanel.Controls.Add(this.resetButton);
			this.resetButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.resetButtonPanel.Location = new System.Drawing.Point(10, 457);
			this.resetButtonPanel.Name = "resetButtonPanel";
			this.resetButtonPanel.Size = new System.Drawing.Size(490, 49);
			this.resetButtonPanel.TabIndex = 2;
			// 
			// resetButton
			// 
			this.resetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.resetButton.Enabled = false;
			this.resetButton.Location = new System.Drawing.Point(3, 12);
			this.resetButton.Name = "resetButton";
			this.resetButton.Size = new System.Drawing.Size(110, 34);
			this.resetButton.TabIndex = 1;
			this.resetButton.Text = "Reset all";
			this.resetButton.UseVisualStyleBackColor = true;
			this.resetButton.Click += new System.EventHandler(this.ResetTheme);
			// 
			// previewGroup
			// 
			this.previewGroup.Controls.Add(this.previewBox);
			this.previewGroup.Location = new System.Drawing.Point(6, 3);
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
			// toolstrip
			// 
			this.toolstrip.Dock = System.Windows.Forms.DockStyle.None;
			this.toolstrip.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.toolstrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newButton,
            this.renameButton,
            this.saveButton,
            this.toolsep1,
            this.deleteButton});
			this.toolstrip.Location = new System.Drawing.Point(505, 5);
			this.toolstrip.Name = "toolstrip";
			this.toolstrip.Size = new System.Drawing.Size(160, 33);
			this.toolstrip.TabIndex = 10;
			this.toolstrip.Text = "toolstrip";
			// 
			// newButton
			// 
			this.newButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.newButton.Image = global::River.OneMoreAddIn.Properties.Resources.NewStyle;
			this.newButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.newButton.Name = "newButton";
			this.newButton.Size = new System.Drawing.Size(34, 28);
			this.newButton.Text = "New Style";
			this.newButton.Click += new System.EventHandler(this.CreateNewTheme);
			// 
			// renameButton
			// 
			this.renameButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.renameButton.Image = global::River.OneMoreAddIn.Properties.Resources.Rename;
			this.renameButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.renameButton.Name = "renameButton";
			this.renameButton.Size = new System.Drawing.Size(34, 28);
			this.renameButton.Text = "Rename";
			this.renameButton.Click += new System.EventHandler(this.RenameTheme);
			// 
			// saveButton
			// 
			this.saveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.saveButton.Enabled = false;
			this.saveButton.Image = global::River.OneMoreAddIn.Properties.Resources.SaveAs;
			this.saveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = new System.Drawing.Size(34, 28);
			this.saveButton.Text = "Save";
			this.saveButton.Click += new System.EventHandler(this.SaveTheme);
			// 
			// toolsep1
			// 
			this.toolsep1.Name = "toolsep1";
			this.toolsep1.Size = new System.Drawing.Size(6, 33);
			// 
			// deleteButton
			// 
			this.deleteButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.deleteButton.Image = global::River.OneMoreAddIn.Properties.Resources.Delete;
			this.deleteButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.Size = new System.Drawing.Size(34, 28);
			this.deleteButton.Text = "Delete";
			this.deleteButton.Click += new System.EventHandler(this.DeleteTheme);
			// 
			// topPanel
			// 
			this.topPanel.Controls.Add(this.nameLabel);
			this.topPanel.Controls.Add(this.toolstrip);
			this.topPanel.Controls.Add(this.combo);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = new System.Drawing.Point(20, 20);
			this.topPanel.Name = "topPanel";
			this.topPanel.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.topPanel.Size = new System.Drawing.Size(829, 66);
			this.topPanel.TabIndex = 11;
			// 
			// tabs
			// 
			this.tabs.Controls.Add(this.colorsTab);
			this.tabs.Controls.Add(this.fontsTab);
			this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabs.Location = new System.Drawing.Point(20, 86);
			this.tabs.Name = "tabs";
			this.tabs.SelectedIndex = 0;
			this.tabs.Size = new System.Drawing.Size(829, 552);
			this.tabs.TabIndex = 12;
			// 
			// colorsTab
			// 
			this.colorsTab.Controls.Add(this.elementsGroup);
			this.colorsTab.Controls.Add(this.previewDockPanel);
			this.colorsTab.Location = new System.Drawing.Point(4, 29);
			this.colorsTab.Name = "colorsTab";
			this.colorsTab.Padding = new System.Windows.Forms.Padding(3);
			this.colorsTab.Size = new System.Drawing.Size(821, 519);
			this.colorsTab.TabIndex = 0;
			this.colorsTab.Text = "Colors";
			this.colorsTab.UseVisualStyleBackColor = true;
			// 
			// previewDockPanel
			// 
			this.previewDockPanel.Controls.Add(this.previewGroup);
			this.previewDockPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.previewDockPanel.Location = new System.Drawing.Point(510, 3);
			this.previewDockPanel.Name = "previewDockPanel";
			this.previewDockPanel.Size = new System.Drawing.Size(308, 513);
			this.previewDockPanel.TabIndex = 10;
			// 
			// fontsTab
			// 
			this.fontsTab.Controls.Add(this.fontsGroup);
			this.fontsTab.Controls.Add(this.fontElementsGroup);
			this.fontsTab.Location = new System.Drawing.Point(4, 29);
			this.fontsTab.Name = "fontsTab";
			this.fontsTab.Padding = new System.Windows.Forms.Padding(3);
			this.fontsTab.Size = new System.Drawing.Size(821, 519);
			this.fontsTab.TabIndex = 1;
			this.fontsTab.Text = "Fonts";
			this.fontsTab.UseVisualStyleBackColor = true;
			// 
			// fontsGroup
			// 
			this.fontsGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.fontsGroup.Controls.Add(this.applyFontButton);
			this.fontsGroup.Controls.Add(this.familyBox);
			this.fontsGroup.Controls.Add(this.sizeBox);
			this.fontsGroup.Controls.Add(this.fontToolstrip);
			this.fontsGroup.Enabled = false;
			this.fontsGroup.Location = new System.Drawing.Point(6, 268);
			this.fontsGroup.Name = "fontsGroup";
			this.fontsGroup.Padding = new System.Windows.Forms.Padding(10, 3, 10, 3);
			this.fontsGroup.Size = new System.Drawing.Size(640, 115);
			this.fontsGroup.TabIndex = 16;
			this.fontsGroup.TabStop = false;
			this.fontsGroup.Text = "Font";
			// 
			// applyFontButton
			// 
			this.applyFontButton.Location = new System.Drawing.Point(512, 25);
			this.applyFontButton.Name = "applyFontButton";
			this.applyFontButton.Size = new System.Drawing.Size(110, 34);
			this.applyFontButton.TabIndex = 15;
			this.applyFontButton.Text = "Apply";
			this.applyFontButton.UseVisualStyleBackColor = true;
			this.applyFontButton.Click += new System.EventHandler(this.ApplyFont);
			// 
			// familyBox
			// 
			this.familyBox.DropDownHeight = 400;
			this.familyBox.DropDownWidth = 350;
			this.familyBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.familyBox.FormattingEnabled = true;
			this.familyBox.IntegralHeight = false;
			this.familyBox.Location = new System.Drawing.Point(13, 25);
			this.familyBox.Name = "familyBox";
			this.familyBox.Size = new System.Drawing.Size(377, 32);
			this.familyBox.TabIndex = 11;
			this.familyBox.SelectedIndexChanged += new System.EventHandler(this.ChangeFontFont);
			// 
			// sizeBox
			// 
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
			this.sizeBox.Location = new System.Drawing.Point(396, 24);
			this.sizeBox.Name = "sizeBox";
			this.sizeBox.Size = new System.Drawing.Size(110, 33);
			this.sizeBox.TabIndex = 12;
			this.sizeBox.SelectedIndexChanged += new System.EventHandler(this.ChangeFontFont);
			// 
			// fontToolstrip
			// 
			this.fontToolstrip.BackColor = System.Drawing.Color.Transparent;
			this.fontToolstrip.Dock = System.Windows.Forms.DockStyle.None;
			this.fontToolstrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.fontToolstrip.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.fontToolstrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.boldButton,
            this.italicButton,
            this.underlineButton,
            this.colorButton});
			this.fontToolstrip.Location = new System.Drawing.Point(13, 60);
			this.fontToolstrip.Name = "fontToolstrip";
			this.fontToolstrip.Padding = new System.Windows.Forms.Padding(0);
			this.fontToolstrip.Size = new System.Drawing.Size(153, 33);
			this.fontToolstrip.TabIndex = 14;
			// 
			// boldButton
			// 
			this.boldButton.CheckOnClick = true;
			this.boldButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.boldButton.Image = ((System.Drawing.Image)(resources.GetObject("boldButton.Image")));
			this.boldButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.boldButton.Margin = new System.Windows.Forms.Padding(0, 2, 1, 3);
			this.boldButton.Name = "boldButton";
			this.boldButton.Size = new System.Drawing.Size(34, 28);
			this.boldButton.Text = "Bold";
			this.boldButton.CheckStateChanged += new System.EventHandler(this.ChangeFontFont);
			// 
			// italicButton
			// 
			this.italicButton.CheckOnClick = true;
			this.italicButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.italicButton.Image = ((System.Drawing.Image)(resources.GetObject("italicButton.Image")));
			this.italicButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.italicButton.Margin = new System.Windows.Forms.Padding(0, 2, 1, 3);
			this.italicButton.Name = "italicButton";
			this.italicButton.Size = new System.Drawing.Size(34, 28);
			this.italicButton.Text = "Italic";
			this.italicButton.CheckStateChanged += new System.EventHandler(this.ChangeFontFont);
			// 
			// underlineButton
			// 
			this.underlineButton.CheckOnClick = true;
			this.underlineButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.underlineButton.Image = ((System.Drawing.Image)(resources.GetObject("underlineButton.Image")));
			this.underlineButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.underlineButton.Margin = new System.Windows.Forms.Padding(0, 2, 1, 3);
			this.underlineButton.Name = "underlineButton";
			this.underlineButton.Size = new System.Drawing.Size(34, 28);
			this.underlineButton.Text = "Underline";
			this.underlineButton.CheckStateChanged += new System.EventHandler(this.ChangeFontFont);
			// 
			// colorButton
			// 
			this.colorButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.colorButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.defaultBlackToolStripMenuItem});
			this.colorButton.Image = ((System.Drawing.Image)(resources.GetObject("colorButton.Image")));
			this.colorButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.colorButton.Margin = new System.Windows.Forms.Padding(0, 2, 1, 3);
			this.colorButton.Name = "colorButton";
			this.colorButton.Size = new System.Drawing.Size(45, 28);
			this.colorButton.ToolTipText = "Text Color";
			this.colorButton.ButtonClick += new System.EventHandler(this.ChangeFontColor);
			// 
			// defaultBlackToolStripMenuItem
			// 
			this.defaultBlackToolStripMenuItem.Name = "defaultBlackToolStripMenuItem";
			this.defaultBlackToolStripMenuItem.Size = new System.Drawing.Size(226, 34);
			this.defaultBlackToolStripMenuItem.Text = "Default (Black)";
			this.defaultBlackToolStripMenuItem.Click += new System.EventHandler(this.SetFontColorDefault);
			// 
			// fontElementsGroup
			// 
			this.fontElementsGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.fontElementsGroup.Controls.Add(this.defaultFontButton);
			this.fontElementsGroup.Controls.Add(this.selectedFontLabel);
			this.fontElementsGroup.Controls.Add(this.resetFontButton);
			this.fontElementsGroup.Controls.Add(this.colorFontsBox);
			this.fontElementsGroup.Location = new System.Drawing.Point(6, 6);
			this.fontElementsGroup.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.fontElementsGroup.Name = "fontElementsGroup";
			this.fontElementsGroup.Padding = new System.Windows.Forms.Padding(10, 3, 10, 3);
			this.fontElementsGroup.Size = new System.Drawing.Size(718, 249);
			this.fontElementsGroup.TabIndex = 15;
			this.fontElementsGroup.TabStop = false;
			this.fontElementsGroup.Text = "Table Elements";
			// 
			// defaultFontButton
			// 
			this.defaultFontButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.defaultFontButton.Location = new System.Drawing.Point(231, 209);
			this.defaultFontButton.Name = "defaultFontButton";
			this.defaultFontButton.Size = new System.Drawing.Size(110, 34);
			this.defaultFontButton.TabIndex = 3;
			this.defaultFontButton.Text = "Default";
			this.defaultFontButton.UseVisualStyleBackColor = true;
			this.defaultFontButton.Click += new System.EventHandler(this.DefaultSelectedFont);
			// 
			// selectedFontLabel
			// 
			this.selectedFontLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.selectedFontLabel.AutoSize = true;
			this.selectedFontLabel.Location = new System.Drawing.Point(13, 215);
			this.selectedFontLabel.Name = "selectedFontLabel";
			this.selectedFontLabel.Size = new System.Drawing.Size(76, 20);
			this.selectedFontLabel.TabIndex = 2;
			this.selectedFontLabel.Text = "Selected:";
			// 
			// resetFontButton
			// 
			this.resetFontButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.resetFontButton.Location = new System.Drawing.Point(115, 208);
			this.resetFontButton.Name = "resetFontButton";
			this.resetFontButton.Size = new System.Drawing.Size(110, 34);
			this.resetFontButton.TabIndex = 1;
			this.resetFontButton.Text = "Reset";
			this.resetFontButton.UseVisualStyleBackColor = true;
			this.resetFontButton.Click += new System.EventHandler(this.ResetSelectedFont);
			// 
			// colorFontsBox
			// 
			this.colorFontsBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.colorFontsBox.ControlPadding = 2;
			this.colorFontsBox.FullRowSelect = true;
			this.colorFontsBox.HideSelection = false;
			this.colorFontsBox.HighlightBackground = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(193)))), ((int)(((byte)(255)))));
			this.colorFontsBox.HighlightForeground = System.Drawing.SystemColors.HighlightText;
			this.colorFontsBox.Location = new System.Drawing.Point(13, 25);
			this.colorFontsBox.Name = "colorFontsBox";
			this.colorFontsBox.RowHeight = 28;
			this.colorFontsBox.Size = new System.Drawing.Size(692, 177);
			this.colorFontsBox.SortedBackground = System.Drawing.Color.Transparent;
			this.colorFontsBox.TabIndex = 0;
			this.colorFontsBox.UseCompatibleStateImageBehavior = false;
			this.colorFontsBox.View = System.Windows.Forms.View.Details;
			this.colorFontsBox.SelectedIndexChanged += new System.EventHandler(this.ShowFontProperties);
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(this.cancelButton);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = new System.Drawing.Point(20, 638);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Padding = new System.Windows.Forms.Padding(6);
			this.bottomPanel.Size = new System.Drawing.Size(829, 53);
			this.bottomPanel.TabIndex = 13;
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.button1.Enabled = false;
			this.button1.Location = new System.Drawing.Point(13, 209);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(110, 34);
			this.button1.TabIndex = 1;
			this.button1.Text = "Reset";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// EditTableThemesDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(859, 701);
			this.Controls.Add(this.tabs);
			this.Controls.Add(this.topPanel);
			this.Controls.Add(this.bottomPanel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(859, 643);
			this.Name = "EditTableThemesDialog";
			this.Padding = new System.Windows.Forms.Padding(20, 20, 10, 10);
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Custom Table Styles";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConfirmClosing);
			this.elementsGroup.ResumeLayout(false);
			this.resetButtonPanel.ResumeLayout(false);
			this.previewGroup.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.previewBox)).EndInit();
			this.toolstrip.ResumeLayout(false);
			this.toolstrip.PerformLayout();
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.tabs.ResumeLayout(false);
			this.colorsTab.ResumeLayout(false);
			this.previewDockPanel.ResumeLayout(false);
			this.fontsTab.ResumeLayout(false);
			this.fontsGroup.ResumeLayout(false);
			this.fontsGroup.PerformLayout();
			this.fontToolstrip.ResumeLayout(false);
			this.fontToolstrip.PerformLayout();
			this.fontElementsGroup.ResumeLayout(false);
			this.fontElementsGroup.PerformLayout();
			this.bottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}


		#endregion
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.ComboBox combo;
		private System.Windows.Forms.Label nameLabel;
		private System.Windows.Forms.GroupBox elementsGroup;
		private River.OneMoreAddIn.UI.MoreListView elementsBox;
		private System.Windows.Forms.GroupBox previewGroup;
		private River.OneMoreAddIn.UI.ScaledToolStrip toolstrip;
		private System.Windows.Forms.ToolStripButton deleteButton;
		private System.Windows.Forms.Button resetButton;
		private System.Windows.Forms.PictureBox previewBox;
		private System.Windows.Forms.ToolStripButton saveButton;
		private System.Windows.Forms.ToolStripButton newButton;
		private System.Windows.Forms.ToolStripButton renameButton;
		private System.Windows.Forms.ToolStripSeparator toolsep1;
		private System.Windows.Forms.Panel topPanel;
		private System.Windows.Forms.TabControl tabs;
		private System.Windows.Forms.TabPage colorsTab;
		private System.Windows.Forms.TabPage fontsTab;
		private System.Windows.Forms.Panel bottomPanel;
		private UI.FontComboBox familyBox;
		private System.Windows.Forms.ComboBox sizeBox;
		private UI.ScaledToolStrip fontToolstrip;
		private System.Windows.Forms.ToolStripButton boldButton;
		private System.Windows.Forms.ToolStripButton italicButton;
		private System.Windows.Forms.ToolStripButton underlineButton;
		private System.Windows.Forms.ToolStripSplitButton colorButton;
		private System.Windows.Forms.ToolStripMenuItem defaultBlackToolStripMenuItem;
		private System.Windows.Forms.Button resetFontButton;
		private UI.MoreListView colorFontsBox;
		private System.Windows.Forms.GroupBox fontsGroup;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.GroupBox fontElementsGroup;
		private System.Windows.Forms.Button applyFontButton;
		private System.Windows.Forms.Button defaultFontButton;
		private System.Windows.Forms.Label selectedFontLabel;
		private System.Windows.Forms.Panel previewDockPanel;
		private System.Windows.Forms.Panel resetButtonPanel;
	}
}