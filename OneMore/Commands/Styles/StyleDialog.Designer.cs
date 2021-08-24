namespace River.OneMoreAddIn.Commands
{
	partial class StyleDialog
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose (bool disposing)
		{
			if (disposing && (components != null))
			{
				selection?.Dispose();
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent ()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StyleDialog));
			this.sizeBox = new System.Windows.Forms.ComboBox();
			this.beforeLabel = new System.Windows.Forms.Label();
			this.afterLabel = new System.Windows.Forms.Label();
			this.nameLabel = new System.Windows.Forms.Label();
			this.nameBox = new System.Windows.Forms.TextBox();
			this.fontLabel = new System.Windows.Forms.Label();
			this.spaceBeforeSpinner = new System.Windows.Forms.NumericUpDown();
			this.spaceAfterSpinner = new System.Windows.Forms.NumericUpDown();
			this.previewBox = new System.Windows.Forms.PictureBox();
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.namesBox = new System.Windows.Forms.ComboBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.spacingLabel = new System.Windows.Forms.Label();
			this.spacingSpinner = new System.Windows.Forms.NumericUpDown();
			this.styleTypeLabel = new System.Windows.Forms.Label();
			this.styleTypeBox = new System.Windows.Forms.ComboBox();
			this.applyColorsBox = new System.Windows.Forms.CheckBox();
			this.familyBox = new River.OneMoreAddIn.UI.FontComboBox();
			this.toolStrip = new River.OneMoreAddIn.UI.ScaledToolStrip();
			this.boldButton = new System.Windows.Forms.ToolStripButton();
			this.italicButton = new System.Windows.Forms.ToolStripButton();
			this.underlineButton = new System.Windows.Forms.ToolStripButton();
			this.strikeButton = new System.Windows.Forms.ToolStripButton();
			this.superButton = new System.Windows.Forms.ToolStripButton();
			this.subButton = new System.Windows.Forms.ToolStripButton();
			this.backColorButton = new System.Windows.Forms.ToolStripSplitButton();
			this.transparentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.colorButton = new System.Windows.Forms.ToolStripSplitButton();
			this.defaultBlackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mainTools = new River.OneMoreAddIn.UI.ScaledToolStrip();
			this.FileMenu = new System.Windows.Forms.ToolStripDropDownButton();
			this.loadButton = new System.Windows.Forms.ToolStripMenuItem();
			this.saveButton = new System.Windows.Forms.ToolStripMenuItem();
			this.newStyleButton = new System.Windows.Forms.ToolStripButton();
			this.reorderButton = new System.Windows.Forms.ToolStripButton();
			this.deleteButton = new System.Windows.Forms.ToolStripButton();
			((System.ComponentModel.ISupportInitialize)(this.spaceBeforeSpinner)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.spaceAfterSpinner)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.previewBox)).BeginInit();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.spacingSpinner)).BeginInit();
			this.toolStrip.SuspendLayout();
			this.mainTools.SuspendLayout();
			this.SuspendLayout();
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
			this.sizeBox.Location = new System.Drawing.Point(483, 105);
			this.sizeBox.Name = "sizeBox";
			this.sizeBox.Size = new System.Drawing.Size(104, 33);
			this.sizeBox.TabIndex = 4;
			this.sizeBox.SelectedIndexChanged += new System.EventHandler(this.ChangeFontSize);
			// 
			// beforeLabel
			// 
			this.beforeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.beforeLabel.AutoSize = true;
			this.beforeLabel.Location = new System.Drawing.Point(18, 228);
			this.beforeLabel.Name = "beforeLabel";
			this.beforeLabel.Size = new System.Drawing.Size(111, 20);
			this.beforeLabel.TabIndex = 6;
			this.beforeLabel.Text = "Space Before:";
			// 
			// afterLabel
			// 
			this.afterLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.afterLabel.AutoSize = true;
			this.afterLabel.Location = new System.Drawing.Point(18, 265);
			this.afterLabel.Name = "afterLabel";
			this.afterLabel.Size = new System.Drawing.Size(98, 20);
			this.afterLabel.TabIndex = 7;
			this.afterLabel.Text = "Space After:";
			// 
			// nameLabel
			// 
			this.nameLabel.AutoSize = true;
			this.nameLabel.Location = new System.Drawing.Point(18, 29);
			this.nameLabel.Name = "nameLabel";
			this.nameLabel.Size = new System.Drawing.Size(94, 20);
			this.nameLabel.TabIndex = 8;
			this.nameLabel.Text = "Style Name:";
			// 
			// nameBox
			// 
			this.nameBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.nameBox.Location = new System.Drawing.Point(146, 23);
			this.nameBox.Name = "nameBox";
			this.nameBox.Size = new System.Drawing.Size(444, 31);
			this.nameBox.TabIndex = 1;
			this.nameBox.TextChanged += new System.EventHandler(this.ChangeStyleName);
			// 
			// fontLabel
			// 
			this.fontLabel.AutoSize = true;
			this.fontLabel.Location = new System.Drawing.Point(18, 118);
			this.fontLabel.Name = "fontLabel";
			this.fontLabel.Size = new System.Drawing.Size(46, 20);
			this.fontLabel.TabIndex = 10;
			this.fontLabel.Text = "Font:";
			// 
			// spaceBeforeSpinner
			// 
			this.spaceBeforeSpinner.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.spaceBeforeSpinner.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.spaceBeforeSpinner.Location = new System.Drawing.Point(146, 222);
			this.spaceBeforeSpinner.Name = "spaceBeforeSpinner";
			this.spaceBeforeSpinner.Size = new System.Drawing.Size(108, 31);
			this.spaceBeforeSpinner.TabIndex = 7;
			this.spaceBeforeSpinner.ValueChanged += new System.EventHandler(this.ChangeSpaceBefore);
			// 
			// spaceAfterSpinner
			// 
			this.spaceAfterSpinner.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.spaceAfterSpinner.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.spaceAfterSpinner.Location = new System.Drawing.Point(146, 258);
			this.spaceAfterSpinner.Name = "spaceAfterSpinner";
			this.spaceAfterSpinner.Size = new System.Drawing.Size(108, 31);
			this.spaceAfterSpinner.TabIndex = 8;
			this.spaceAfterSpinner.ValueChanged += new System.EventHandler(this.ChangeSpaceAfter);
			// 
			// previewBox
			// 
			this.previewBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.previewBox.BackColor = System.Drawing.Color.White;
			this.previewBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.previewBox.Location = new System.Drawing.Point(278, 222);
			this.previewBox.Name = "previewBox";
			this.previewBox.Size = new System.Drawing.Size(310, 107);
			this.previewBox.TabIndex = 18;
			this.previewBox.TabStop = false;
			this.previewBox.Paint += new System.Windows.Forms.PaintEventHandler(this.RepaintSample);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(472, 368);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(116, 38);
			this.cancelButton.TabIndex = 10;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(314, 368);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(154, 38);
			this.okButton.TabIndex = 9;
			this.okButton.Text = "OK (Save)";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// namesBox
			// 
			this.namesBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.namesBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.namesBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.namesBox.FormattingEnabled = true;
			this.namesBox.Location = new System.Drawing.Point(18, 372);
			this.namesBox.Name = "namesBox";
			this.namesBox.Size = new System.Drawing.Size(121, 33);
			this.namesBox.TabIndex = 21;
			this.namesBox.Visible = false;
			this.namesBox.SelectedIndexChanged += new System.EventHandler(this.ChangeStyleListSelection);
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.Controls.Add(this.spacingLabel);
			this.panel1.Controls.Add(this.spacingSpinner);
			this.panel1.Controls.Add(this.styleTypeLabel);
			this.panel1.Controls.Add(this.styleTypeBox);
			this.panel1.Controls.Add(this.applyColorsBox);
			this.panel1.Controls.Add(this.nameLabel);
			this.panel1.Controls.Add(this.familyBox);
			this.panel1.Controls.Add(this.toolStrip);
			this.panel1.Controls.Add(this.okButton);
			this.panel1.Controls.Add(this.cancelButton);
			this.panel1.Controls.Add(this.sizeBox);
			this.panel1.Controls.Add(this.beforeLabel);
			this.panel1.Controls.Add(this.namesBox);
			this.panel1.Controls.Add(this.afterLabel);
			this.panel1.Controls.Add(this.nameBox);
			this.panel1.Controls.Add(this.fontLabel);
			this.panel1.Controls.Add(this.previewBox);
			this.panel1.Controls.Add(this.spaceAfterSpinner);
			this.panel1.Controls.Add(this.spaceBeforeSpinner);
			this.panel1.Location = new System.Drawing.Point(8, 40);
			this.panel1.Name = "panel1";
			this.panel1.Padding = new System.Windows.Forms.Padding(15, 20, 15, 9);
			this.panel1.Size = new System.Drawing.Size(606, 418);
			this.panel1.TabIndex = 25;
			// 
			// spacingLabel
			// 
			this.spacingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.spacingLabel.AutoSize = true;
			this.spacingLabel.Location = new System.Drawing.Point(18, 301);
			this.spacingLabel.Name = "spacingLabel";
			this.spacingLabel.Size = new System.Drawing.Size(71, 20);
			this.spacingLabel.TabIndex = 29;
			this.spacingLabel.Text = "Spacing:";
			// 
			// spacingSpinner
			// 
			this.spacingSpinner.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.spacingSpinner.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.spacingSpinner.Location = new System.Drawing.Point(146, 295);
			this.spacingSpinner.Name = "spacingSpinner";
			this.spacingSpinner.Size = new System.Drawing.Size(108, 31);
			this.spacingSpinner.TabIndex = 28;
			this.spacingSpinner.ValueChanged += new System.EventHandler(this.ChangeSpacing);
			// 
			// styleTypeLabel
			// 
			this.styleTypeLabel.AutoSize = true;
			this.styleTypeLabel.Location = new System.Drawing.Point(18, 68);
			this.styleTypeLabel.Name = "styleTypeLabel";
			this.styleTypeLabel.Size = new System.Drawing.Size(86, 20);
			this.styleTypeLabel.TabIndex = 27;
			this.styleTypeLabel.Text = "Style Type:";
			// 
			// styleTypeBox
			// 
			this.styleTypeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.styleTypeBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.styleTypeBox.FormattingEnabled = true;
			this.styleTypeBox.Items.AddRange(new object[] {
            "Character - words in paragraph",
            "Paragraph - entire paragraph",
            "Heading - include in TOC"});
			this.styleTypeBox.Location = new System.Drawing.Point(146, 62);
			this.styleTypeBox.Name = "styleTypeBox";
			this.styleTypeBox.Size = new System.Drawing.Size(444, 33);
			this.styleTypeBox.TabIndex = 2;
			this.styleTypeBox.SelectedIndexChanged += new System.EventHandler(this.ChangeStyleType);
			// 
			// applyColorsBox
			// 
			this.applyColorsBox.AutoSize = true;
			this.applyColorsBox.Checked = true;
			this.applyColorsBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.applyColorsBox.Location = new System.Drawing.Point(468, 158);
			this.applyColorsBox.Name = "applyColorsBox";
			this.applyColorsBox.Size = new System.Drawing.Size(120, 24);
			this.applyColorsBox.TabIndex = 6;
			this.applyColorsBox.Text = "Apply colors";
			this.applyColorsBox.UseVisualStyleBackColor = true;
			this.applyColorsBox.CheckedChanged += new System.EventHandler(this.ChangeApplyColorsOption);
			// 
			// familyBox
			// 
			this.familyBox.DropDownHeight = 400;
			this.familyBox.DropDownWidth = 350;
			this.familyBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.familyBox.FormattingEnabled = true;
			this.familyBox.IntegralHeight = false;
			this.familyBox.Location = new System.Drawing.Point(146, 105);
			this.familyBox.Name = "familyBox";
			this.familyBox.Size = new System.Drawing.Size(324, 32);
			this.familyBox.TabIndex = 3;
			this.familyBox.SelectedIndexChanged += new System.EventHandler(this.ChangeFontFamily);
			// 
			// toolStrip
			// 
			this.toolStrip.BackColor = System.Drawing.Color.Transparent;
			this.toolStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.boldButton,
            this.italicButton,
            this.underlineButton,
            this.strikeButton,
            this.superButton,
            this.subButton,
            this.backColorButton,
            this.colorButton});
			this.toolStrip.Location = new System.Drawing.Point(146, 149);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Padding = new System.Windows.Forms.Padding(0);
			this.toolStrip.Size = new System.Drawing.Size(304, 33);
			this.toolStrip.TabIndex = 5;
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
			this.boldButton.CheckStateChanged += new System.EventHandler(this.ChangeFontStyle);
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
			this.italicButton.CheckStateChanged += new System.EventHandler(this.ChangeFontStyle);
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
			this.underlineButton.CheckStateChanged += new System.EventHandler(this.ChangeFontStyle);
			// 
			// strikeButton
			// 
			this.strikeButton.CheckOnClick = true;
			this.strikeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.strikeButton.Image = ((System.Drawing.Image)(resources.GetObject("strikeButton.Image")));
			this.strikeButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.strikeButton.Margin = new System.Windows.Forms.Padding(0, 2, 1, 3);
			this.strikeButton.Name = "strikeButton";
			this.strikeButton.Size = new System.Drawing.Size(34, 28);
			this.strikeButton.Text = "Strikethrough";
			this.strikeButton.CheckStateChanged += new System.EventHandler(this.ChangeFontStyle);
			// 
			// superButton
			// 
			this.superButton.CheckOnClick = true;
			this.superButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.superButton.Image = ((System.Drawing.Image)(resources.GetObject("superButton.Image")));
			this.superButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.superButton.Margin = new System.Windows.Forms.Padding(0, 2, 1, 3);
			this.superButton.Name = "superButton";
			this.superButton.Size = new System.Drawing.Size(34, 28);
			this.superButton.Text = "Superscript";
			this.superButton.CheckStateChanged += new System.EventHandler(this.ChangeFontStyle);
			this.superButton.Click += new System.EventHandler(this.ToggleSuperSub);
			// 
			// subButton
			// 
			this.subButton.CheckOnClick = true;
			this.subButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.subButton.Image = ((System.Drawing.Image)(resources.GetObject("subButton.Image")));
			this.subButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.subButton.Margin = new System.Windows.Forms.Padding(0, 2, 1, 3);
			this.subButton.Name = "subButton";
			this.subButton.Size = new System.Drawing.Size(34, 28);
			this.subButton.Text = "Subscript";
			this.subButton.CheckStateChanged += new System.EventHandler(this.ChangeFontStyle);
			this.subButton.Click += new System.EventHandler(this.ToggleSuperSub);
			// 
			// backColorButton
			// 
			this.backColorButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.backColorButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.transparentToolStripMenuItem});
			this.backColorButton.Image = ((System.Drawing.Image)(resources.GetObject("backColorButton.Image")));
			this.backColorButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.backColorButton.Margin = new System.Windows.Forms.Padding(0, 2, 1, 3);
			this.backColorButton.Name = "backColorButton";
			this.backColorButton.Size = new System.Drawing.Size(45, 28);
			this.backColorButton.ToolTipText = "Highlight Color";
			this.backColorButton.ButtonClick += new System.EventHandler(this.ChangeHighlightColor);
			// 
			// transparentToolStripMenuItem
			// 
			this.transparentToolStripMenuItem.Name = "transparentToolStripMenuItem";
			this.transparentToolStripMenuItem.Size = new System.Drawing.Size(205, 34);
			this.transparentToolStripMenuItem.Text = "Transparent";
			this.transparentToolStripMenuItem.Click += new System.EventHandler(this.ChangeHighlightToDefault);
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
			this.colorButton.ButtonClick += new System.EventHandler(this.ChangeColor);
			// 
			// defaultBlackToolStripMenuItem
			// 
			this.defaultBlackToolStripMenuItem.Name = "defaultBlackToolStripMenuItem";
			this.defaultBlackToolStripMenuItem.Size = new System.Drawing.Size(226, 34);
			this.defaultBlackToolStripMenuItem.Text = "Default (Black)";
			this.defaultBlackToolStripMenuItem.Click += new System.EventHandler(this.ChangeColorToDefault);
			// 
			// mainTools
			// 
			this.mainTools.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.mainTools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileMenu,
            this.newStyleButton,
            this.reorderButton,
            this.deleteButton});
			this.mainTools.Location = new System.Drawing.Point(4, 5);
			this.mainTools.Name = "mainTools";
			this.mainTools.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
			this.mainTools.Size = new System.Drawing.Size(617, 34);
			this.mainTools.TabIndex = 0;
			// 
			// FileMenu
			// 
			this.FileMenu.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.FileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadButton,
            this.saveButton});
			this.FileMenu.Image = ((System.Drawing.Image)(resources.GetObject("FileMenu.Image")));
			this.FileMenu.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.FileMenu.Name = "FileMenu";
			this.FileMenu.Size = new System.Drawing.Size(56, 29);
			this.FileMenu.Text = "File";
			// 
			// loadButton
			// 
			this.loadButton.Image = global::River.OneMoreAddIn.Properties.Resources.Open;
			this.loadButton.Name = "loadButton";
			this.loadButton.Size = new System.Drawing.Size(185, 34);
			this.loadButton.Text = "Open...";
			this.loadButton.Click += new System.EventHandler(this.LoadTheme);
			// 
			// saveButton
			// 
			this.saveButton.Image = global::River.OneMoreAddIn.Properties.Resources.SaveAs;
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = new System.Drawing.Size(185, 34);
			this.saveButton.Text = "Save as...";
			this.saveButton.Click += new System.EventHandler(this.SaveTheme);
			// 
			// newStyleButton
			// 
			this.newStyleButton.Image = global::River.OneMoreAddIn.Properties.Resources.NewStyle;
			this.newStyleButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.newStyleButton.Name = "newStyleButton";
			this.newStyleButton.Size = new System.Drawing.Size(117, 29);
			this.newStyleButton.Text = "New Style";
			this.newStyleButton.Click += new System.EventHandler(this.AddStyle);
			// 
			// reorderButton
			// 
			this.reorderButton.Image = ((System.Drawing.Image)(resources.GetObject("reorderButton.Image")));
			this.reorderButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.reorderButton.Name = "reorderButton";
			this.reorderButton.Size = new System.Drawing.Size(102, 29);
			this.reorderButton.Text = "Reorder";
			this.reorderButton.Click += new System.EventHandler(this.ReorderStyles);
			// 
			// deleteButton
			// 
			this.deleteButton.Image = ((System.Drawing.Image)(resources.GetObject("deleteButton.Image")));
			this.deleteButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.deleteButton.Margin = new System.Windows.Forms.Padding(5, 1, 0, 2);
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.Size = new System.Drawing.Size(90, 31);
			this.deleteButton.Text = "Delete";
			this.deleteButton.Click += new System.EventHandler(this.DeleteStyle);
			// 
			// StyleDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(621, 466);
			this.Controls.Add(this.mainTools);
			this.Controls.Add(this.panel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "StyleDialog";
			this.Padding = new System.Windows.Forms.Padding(4, 5, 0, 5);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Custom Styles";
			((System.ComponentModel.ISupportInitialize)(this.spaceBeforeSpinner)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.spaceAfterSpinner)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.previewBox)).EndInit();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.spacingSpinner)).EndInit();
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.mainTools.ResumeLayout(false);
			this.mainTools.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.FontComboBox familyBox;
		private System.Windows.Forms.ComboBox sizeBox;
		private System.Windows.Forms.Label beforeLabel;
		private System.Windows.Forms.Label afterLabel;
		private System.Windows.Forms.Label nameLabel;
		private System.Windows.Forms.TextBox nameBox;
		private System.Windows.Forms.Label fontLabel;
		private UI.ScaledToolStrip toolStrip;
		private System.Windows.Forms.ToolStripSplitButton colorButton;
		private System.Windows.Forms.NumericUpDown spaceBeforeSpinner;
		private System.Windows.Forms.NumericUpDown spaceAfterSpinner;
		private System.Windows.Forms.ToolStripSplitButton backColorButton;
		private System.Windows.Forms.PictureBox previewBox;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.ComboBox namesBox;
		private System.Windows.Forms.ToolStripMenuItem transparentToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem defaultBlackToolStripMenuItem;
		private UI.ScaledToolStrip mainTools;
		private System.Windows.Forms.ToolStripButton reorderButton;
		private System.Windows.Forms.ToolStripButton deleteButton;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.ToolStripButton boldButton;
		private System.Windows.Forms.ToolStripButton italicButton;
		private System.Windows.Forms.ToolStripButton underlineButton;
		private System.Windows.Forms.CheckBox applyColorsBox;
		private System.Windows.Forms.Label styleTypeLabel;
		private System.Windows.Forms.ComboBox styleTypeBox;
		private System.Windows.Forms.ToolStripButton strikeButton;
		private System.Windows.Forms.ToolStripButton superButton;
		private System.Windows.Forms.ToolStripButton subButton;
		private System.Windows.Forms.ToolStripDropDownButton FileMenu;
		private System.Windows.Forms.ToolStripMenuItem loadButton;
		private System.Windows.Forms.ToolStripMenuItem saveButton;
		private System.Windows.Forms.ToolStripButton newStyleButton;
		private System.Windows.Forms.Label spacingLabel;
		private System.Windows.Forms.NumericUpDown spacingSpinner;
	}
}