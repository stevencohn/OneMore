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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StyleDialog));
			this.sizeBox = new System.Windows.Forms.ComboBox();
			this.beforeLabel = new System.Windows.Forms.Label();
			this.afterLabel = new System.Windows.Forms.Label();
			this.nameLabel = new System.Windows.Forms.Label();
			this.nameBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.fontLabel = new System.Windows.Forms.Label();
			this.spaceBeforeSpinner = new System.Windows.Forms.NumericUpDown();
			this.spaceAfterSpinner = new System.Windows.Forms.NumericUpDown();
			this.previewBox = new System.Windows.Forms.PictureBox();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.namesBox = new System.Windows.Forms.ComboBox();
			this.bodyPanel = new System.Windows.Forms.Panel();
			this.styleTools = new River.OneMoreAddIn.UI.MoreToolStrip();
			this.renameButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.deleteButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.fontTools = new River.OneMoreAddIn.UI.MoreToolStrip();
			this.boldButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.italicButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.underlineButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.strikeButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.superButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.subButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.backColorButton = new River.OneMoreAddIn.UI.MoreSplitButton();
			this.transparentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.colorButton = new River.OneMoreAddIn.UI.MoreSplitButton();
			this.defaultBlackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ignoredBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.optionsGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.statusLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.pageColorLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.darkBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.pageColorBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.spacingLabel = new System.Windows.Forms.Label();
			this.spacingSpinner = new System.Windows.Forms.NumericUpDown();
			this.styleTypeLabel = new System.Windows.Forms.Label();
			this.styleTypeBox = new System.Windows.Forms.ComboBox();
			this.applyColorsBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.familyBox = new River.OneMoreAddIn.UI.FontComboBox();
			this.mainTools = new River.OneMoreAddIn.UI.MoreMenuStrip();
			this.loadButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.saveButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.mainSep = new System.Windows.Forms.ToolStripSeparator();
			this.newStyleButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.reorderButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.resetButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.tooltip = new System.Windows.Forms.ToolTip(this.components);
			((System.ComponentModel.ISupportInitialize)(this.spaceBeforeSpinner)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.spaceAfterSpinner)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.previewBox)).BeginInit();
			this.bodyPanel.SuspendLayout();
			this.styleTools.SuspendLayout();
			this.fontTools.SuspendLayout();
			this.optionsGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.spacingSpinner)).BeginInit();
			this.mainTools.SuspendLayout();
			this.SuspendLayout();
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
			this.sizeBox.Location = new System.Drawing.Point(539, 123);
			this.sizeBox.Name = "sizeBox";
			this.sizeBox.Size = new System.Drawing.Size(104, 33);
			this.sizeBox.TabIndex = 3;
			this.sizeBox.SelectedIndexChanged += new System.EventHandler(this.ChangeFontSize);
			this.sizeBox.Enter += new System.EventHandler(this.SetActiveFocus);
			// 
			// beforeLabel
			// 
			this.beforeLabel.AutoSize = true;
			this.beforeLabel.Location = new System.Drawing.Point(18, 289);
			this.beforeLabel.Name = "beforeLabel";
			this.beforeLabel.Size = new System.Drawing.Size(111, 20);
			this.beforeLabel.TabIndex = 6;
			this.beforeLabel.Text = "Space Before:";
			this.tooltip.SetToolTip(this.beforeLabel, "Spacing before a paragraph");
			// 
			// afterLabel
			// 
			this.afterLabel.AutoSize = true;
			this.afterLabel.Location = new System.Drawing.Point(18, 326);
			this.afterLabel.Name = "afterLabel";
			this.afterLabel.Size = new System.Drawing.Size(98, 20);
			this.afterLabel.TabIndex = 7;
			this.afterLabel.Text = "Space After:";
			this.tooltip.SetToolTip(this.afterLabel, "Spacing after a paragraph");
			// 
			// nameLabel
			// 
			this.nameLabel.AutoSize = true;
			this.nameLabel.Location = new System.Drawing.Point(18, 38);
			this.nameLabel.Name = "nameLabel";
			this.nameLabel.Size = new System.Drawing.Size(94, 20);
			this.nameLabel.TabIndex = 8;
			this.nameLabel.Text = "Style Name:";
			this.tooltip.SetToolTip(this.nameLabel, "Must be unique");
			// 
			// nameBox
			// 
			this.nameBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.nameBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.nameBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.nameBox.Location = new System.Drawing.Point(169, 31);
			this.nameBox.Name = "nameBox";
			this.nameBox.ProcessEnterKey = false;
			this.nameBox.Size = new System.Drawing.Size(356, 31);
			this.nameBox.TabIndex = 0;
			this.nameBox.ThemedBack = null;
			this.nameBox.ThemedFore = null;
			this.nameBox.TextChanged += new System.EventHandler(this.ChangeStyleName);
			this.nameBox.Enter += new System.EventHandler(this.SetActiveFocus);
			// 
			// fontLabel
			// 
			this.fontLabel.AutoSize = true;
			this.fontLabel.Location = new System.Drawing.Point(18, 130);
			this.fontLabel.Name = "fontLabel";
			this.fontLabel.Size = new System.Drawing.Size(46, 20);
			this.fontLabel.TabIndex = 10;
			this.fontLabel.Text = "Font:";
			// 
			// spaceBeforeSpinner
			// 
			this.spaceBeforeSpinner.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.spaceBeforeSpinner.Location = new System.Drawing.Point(169, 283);
			this.spaceBeforeSpinner.Name = "spaceBeforeSpinner";
			this.spaceBeforeSpinner.Size = new System.Drawing.Size(108, 31);
			this.spaceBeforeSpinner.TabIndex = 7;
			this.spaceBeforeSpinner.ValueChanged += new System.EventHandler(this.ChangeSpaceBefore);
			this.spaceBeforeSpinner.Enter += new System.EventHandler(this.SetActiveFocus);
			// 
			// spaceAfterSpinner
			// 
			this.spaceAfterSpinner.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.spaceAfterSpinner.Location = new System.Drawing.Point(169, 319);
			this.spaceAfterSpinner.Name = "spaceAfterSpinner";
			this.spaceAfterSpinner.Size = new System.Drawing.Size(108, 31);
			this.spaceAfterSpinner.TabIndex = 8;
			this.spaceAfterSpinner.ValueChanged += new System.EventHandler(this.ChangeSpaceAfter);
			this.spaceAfterSpinner.Enter += new System.EventHandler(this.SetActiveFocus);
			// 
			// previewBox
			// 
			this.previewBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.previewBox.BackColor = System.Drawing.Color.White;
			this.previewBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.previewBox.Location = new System.Drawing.Point(296, 283);
			this.previewBox.Name = "previewBox";
			this.previewBox.Size = new System.Drawing.Size(347, 107);
			this.previewBox.TabIndex = 18;
			this.previewBox.TabStop = false;
			this.previewBox.Paint += new System.Windows.Forms.PaintEventHandler(this.RepaintSample);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(527, 585);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(116, 38);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 12;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(405, 585);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(116, 38);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 11;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.SaveStyle);
			// 
			// namesBox
			// 
			this.namesBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.namesBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.namesBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.namesBox.FormattingEnabled = true;
			this.namesBox.Location = new System.Drawing.Point(18, 589);
			this.namesBox.Name = "namesBox";
			this.namesBox.Size = new System.Drawing.Size(121, 33);
			this.namesBox.TabIndex = 10;
			this.namesBox.Visible = false;
			this.namesBox.SelectedIndexChanged += new System.EventHandler(this.ChangeStyleListSelection);
			// 
			// bodyPanel
			// 
			this.bodyPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.bodyPanel.BackColor = System.Drawing.SystemColors.Control;
			this.bodyPanel.Controls.Add(this.styleTools);
			this.bodyPanel.Controls.Add(this.fontTools);
			this.bodyPanel.Controls.Add(this.ignoredBox);
			this.bodyPanel.Controls.Add(this.optionsGroup);
			this.bodyPanel.Controls.Add(this.spacingLabel);
			this.bodyPanel.Controls.Add(this.spacingSpinner);
			this.bodyPanel.Controls.Add(this.styleTypeLabel);
			this.bodyPanel.Controls.Add(this.styleTypeBox);
			this.bodyPanel.Controls.Add(this.applyColorsBox);
			this.bodyPanel.Controls.Add(this.nameLabel);
			this.bodyPanel.Controls.Add(this.familyBox);
			this.bodyPanel.Controls.Add(this.okButton);
			this.bodyPanel.Controls.Add(this.cancelButton);
			this.bodyPanel.Controls.Add(this.sizeBox);
			this.bodyPanel.Controls.Add(this.beforeLabel);
			this.bodyPanel.Controls.Add(this.namesBox);
			this.bodyPanel.Controls.Add(this.afterLabel);
			this.bodyPanel.Controls.Add(this.nameBox);
			this.bodyPanel.Controls.Add(this.fontLabel);
			this.bodyPanel.Controls.Add(this.previewBox);
			this.bodyPanel.Controls.Add(this.spaceAfterSpinner);
			this.bodyPanel.Controls.Add(this.spaceBeforeSpinner);
			this.bodyPanel.Location = new System.Drawing.Point(8, 35);
			this.bodyPanel.Name = "bodyPanel";
			this.bodyPanel.Padding = new System.Windows.Forms.Padding(15, 20, 15, 9);
			this.bodyPanel.Size = new System.Drawing.Size(661, 635);
			this.bodyPanel.TabIndex = 25;
			// 
			// styleTools
			// 
			this.styleTools.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.styleTools.AutoSize = false;
			this.styleTools.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.styleTools.Dock = System.Windows.Forms.DockStyle.None;
			this.styleTools.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.styleTools.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.styleTools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.renameButton,
            this.deleteButton});
			this.styleTools.Location = new System.Drawing.Point(541, 29);
			this.styleTools.Name = "styleTools";
			this.styleTools.Size = new System.Drawing.Size(89, 31);
			this.styleTools.TabIndex = 31;
			this.styleTools.Text = "moreToolStrip1";
			// 
			// renameButton
			// 
			this.renameButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.renameButton.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.renameButton.ForeColor = System.Drawing.Color.Black;
			this.renameButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Rename;
			this.renameButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.renameButton.Name = "renameButton";
			this.renameButton.Size = new System.Drawing.Size(40, 31);
			this.renameButton.Text = "Rename";
			this.renameButton.ToolTipText = "Rename this style";
			this.renameButton.Click += new System.EventHandler(this.RenameStyle);
			// 
			// deleteButton
			// 
			this.deleteButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.deleteButton.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.deleteButton.ForeColor = System.Drawing.Color.Black;
			this.deleteButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Delete;
			this.deleteButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.deleteButton.Margin = new System.Windows.Forms.Padding(5, 1, 0, 2);
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.Size = new System.Drawing.Size(40, 28);
			this.deleteButton.Text = "Delete";
			this.deleteButton.ToolTipText = "Delete this style";
			this.deleteButton.Click += new System.EventHandler(this.DeleteStyle);
			// 
			// fontTools
			// 
			this.fontTools.AutoSize = false;
			this.fontTools.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.fontTools.Dock = System.Windows.Forms.DockStyle.None;
			this.fontTools.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
			this.fontTools.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.fontTools.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.fontTools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.boldButton,
            this.italicButton,
            this.underlineButton,
            this.strikeButton,
            this.superButton,
            this.subButton,
            this.backColorButton,
            this.colorButton});
			this.fontTools.Location = new System.Drawing.Point(169, 168);
			this.fontTools.Name = "fontTools";
			this.fontTools.Padding = new System.Windows.Forms.Padding(0);
			this.fontTools.Size = new System.Drawing.Size(350, 31);
			this.fontTools.Stretch = true;
			this.fontTools.TabIndex = 4;
			// 
			// boldButton
			// 
			this.boldButton.CheckOnClick = true;
			this.boldButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.boldButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Bold;
			this.boldButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.boldButton.Margin = new System.Windows.Forms.Padding(0, 2, 1, 3);
			this.boldButton.Name = "boldButton";
			this.boldButton.Size = new System.Drawing.Size(40, 26);
			this.boldButton.Text = "Bold";
			this.boldButton.CheckStateChanged += new System.EventHandler(this.ChangeFontStyle);
			// 
			// italicButton
			// 
			this.italicButton.CheckOnClick = true;
			this.italicButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.italicButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Italic;
			this.italicButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.italicButton.Margin = new System.Windows.Forms.Padding(0, 2, 1, 3);
			this.italicButton.Name = "italicButton";
			this.italicButton.Size = new System.Drawing.Size(40, 26);
			this.italicButton.Text = "Italic";
			this.italicButton.CheckStateChanged += new System.EventHandler(this.ChangeFontStyle);
			// 
			// underlineButton
			// 
			this.underlineButton.CheckOnClick = true;
			this.underlineButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.underlineButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Underline;
			this.underlineButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.underlineButton.Margin = new System.Windows.Forms.Padding(0, 2, 1, 3);
			this.underlineButton.Name = "underlineButton";
			this.underlineButton.Size = new System.Drawing.Size(40, 26);
			this.underlineButton.Text = "Underline";
			this.underlineButton.CheckStateChanged += new System.EventHandler(this.ChangeFontStyle);
			// 
			// strikeButton
			// 
			this.strikeButton.CheckOnClick = true;
			this.strikeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.strikeButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Strikethrough;
			this.strikeButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.strikeButton.Margin = new System.Windows.Forms.Padding(0, 2, 1, 3);
			this.strikeButton.Name = "strikeButton";
			this.strikeButton.Size = new System.Drawing.Size(40, 26);
			this.strikeButton.Text = "Strikethrough";
			this.strikeButton.CheckStateChanged += new System.EventHandler(this.ChangeFontStyle);
			// 
			// superButton
			// 
			this.superButton.CheckOnClick = true;
			this.superButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.superButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Superscript;
			this.superButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.superButton.Margin = new System.Windows.Forms.Padding(0, 2, 1, 3);
			this.superButton.Name = "superButton";
			this.superButton.Size = new System.Drawing.Size(40, 26);
			this.superButton.Text = "Superscript";
			this.superButton.CheckStateChanged += new System.EventHandler(this.ChangeFontStyle);
			this.superButton.Click += new System.EventHandler(this.ToggleSuperSub);
			// 
			// subButton
			// 
			this.subButton.CheckOnClick = true;
			this.subButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.subButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Subscript;
			this.subButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.subButton.Margin = new System.Windows.Forms.Padding(0, 2, 1, 3);
			this.subButton.Name = "subButton";
			this.subButton.Size = new System.Drawing.Size(40, 26);
			this.subButton.Text = "Subscript";
			this.subButton.CheckStateChanged += new System.EventHandler(this.ChangeFontStyle);
			this.subButton.Click += new System.EventHandler(this.ToggleSuperSub);
			// 
			// backColorButton
			// 
			this.backColorButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.backColorButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.transparentToolStripMenuItem});
			this.backColorButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Highlighter;
			this.backColorButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.backColorButton.Margin = new System.Windows.Forms.Padding(0, 2, 1, 3);
			this.backColorButton.Name = "backColorButton";
			this.backColorButton.Size = new System.Drawing.Size(45, 26);
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
			this.colorButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_FontColor;
			this.colorButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.colorButton.Margin = new System.Windows.Forms.Padding(0, 2, 1, 3);
			this.colorButton.Name = "colorButton";
			this.colorButton.Size = new System.Drawing.Size(45, 26);
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
			// ignoredBox
			// 
			this.ignoredBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.ignoredBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.ignoredBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.ignoredBox.Location = new System.Drawing.Point(169, 244);
			this.ignoredBox.Name = "ignoredBox";
			this.ignoredBox.Size = new System.Drawing.Size(176, 25);
			this.ignoredBox.StylizeImage = false;
			this.ignoredBox.TabIndex = 6;
			this.ignoredBox.Text = "Disable spell check";
			this.ignoredBox.ThemedBack = null;
			this.ignoredBox.ThemedFore = null;
			this.tooltip.SetToolTip(this.ignoredBox, "Disable spell check for selected text");
			this.ignoredBox.UseVisualStyleBackColor = true;
			this.ignoredBox.CheckedChanged += new System.EventHandler(this.ChangeIgnored);
			// 
			// optionsGroup
			// 
			this.optionsGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.optionsGroup.Controls.Add(this.statusLabel);
			this.optionsGroup.Controls.Add(this.pageColorLink);
			this.optionsGroup.Controls.Add(this.darkBox);
			this.optionsGroup.Controls.Add(this.pageColorBox);
			this.optionsGroup.Location = new System.Drawing.Point(22, 420);
			this.optionsGroup.Name = "optionsGroup";
			this.optionsGroup.ShowOnlyTopEdge = true;
			this.optionsGroup.Size = new System.Drawing.Size(620, 150);
			this.optionsGroup.TabIndex = 30;
			this.optionsGroup.TabStop = false;
			this.optionsGroup.Text = "Options";
			this.optionsGroup.ThemedBorder = null;
			this.optionsGroup.ThemedFore = null;
			// 
			// statusLabel
			// 
			this.statusLabel.AutoSize = true;
			this.statusLabel.ForeColor = System.Drawing.Color.Maroon;
			this.statusLabel.Location = new System.Drawing.Point(46, 117);
			this.statusLabel.Name = "statusLabel";
			this.statusLabel.Size = new System.Drawing.Size(31, 20);
			this.statusLabel.TabIndex = 3;
			this.statusLabel.Text = "OK";
			this.statusLabel.ThemedBack = null;
			this.statusLabel.ThemedFore = "ErrorText";
			// 
			// pageColorLink
			// 
			this.pageColorLink.ActiveLinkColor = System.Drawing.Color.MediumOrchid;
			this.pageColorLink.AutoSize = true;
			this.pageColorLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pageColorLink.HoverColor = System.Drawing.Color.Orchid;
			this.pageColorLink.LinkColor = System.Drawing.Color.MediumOrchid;
			this.pageColorLink.Location = new System.Drawing.Point(46, 90);
			this.pageColorLink.Name = "pageColorLink";
			this.pageColorLink.Size = new System.Drawing.Size(247, 20);
			this.pageColorLink.StrictColors = false;
			this.pageColorLink.TabIndex = 2;
			this.pageColorLink.TabStop = true;
			this.pageColorLink.Text = "Click here to select the page color";
			this.pageColorLink.ThemedBack = null;
			this.pageColorLink.ThemedFore = null;
			this.pageColorLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.pageColorLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.SelectPageColor);
			// 
			// darkBox
			// 
			this.darkBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.darkBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.darkBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.darkBox.Location = new System.Drawing.Point(22, 33);
			this.darkBox.Name = "darkBox";
			this.darkBox.Size = new System.Drawing.Size(350, 25);
			this.darkBox.StylizeImage = false;
			this.darkBox.TabIndex = 0;
			this.darkBox.Text = "Intended for pages with dark bakckgrounds";
			this.darkBox.ThemedBack = null;
			this.darkBox.ThemedFore = null;
			this.tooltip.SetToolTip(this.darkBox, "Used in Page Color dialog to warn about non-readable text");
			this.darkBox.UseVisualStyleBackColor = true;
			// 
			// pageColorBox
			// 
			this.pageColorBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.pageColorBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pageColorBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.pageColorBox.Location = new System.Drawing.Point(22, 63);
			this.pageColorBox.Name = "pageColorBox";
			this.pageColorBox.Size = new System.Drawing.Size(403, 25);
			this.pageColorBox.StylizeImage = false;
			this.pageColorBox.TabIndex = 1;
			this.pageColorBox.Text = "Change the page color when applying these styles";
			this.pageColorBox.ThemedBack = null;
			this.pageColorBox.ThemedFore = null;
			this.pageColorBox.UseVisualStyleBackColor = true;
			this.pageColorBox.CheckedChanged += new System.EventHandler(this.ChangePageColorOption);
			// 
			// spacingLabel
			// 
			this.spacingLabel.AutoSize = true;
			this.spacingLabel.Location = new System.Drawing.Point(18, 362);
			this.spacingLabel.Name = "spacingLabel";
			this.spacingLabel.Size = new System.Drawing.Size(71, 20);
			this.spacingLabel.TabIndex = 29;
			this.spacingLabel.Text = "Spacing:";
			this.tooltip.SetToolTip(this.spacingLabel, "Spacing between lines in a paragraph");
			// 
			// spacingSpinner
			// 
			this.spacingSpinner.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.spacingSpinner.Location = new System.Drawing.Point(169, 356);
			this.spacingSpinner.Name = "spacingSpinner";
			this.spacingSpinner.Size = new System.Drawing.Size(108, 31);
			this.spacingSpinner.TabIndex = 9;
			this.spacingSpinner.ValueChanged += new System.EventHandler(this.ChangeSpacing);
			this.spacingSpinner.Enter += new System.EventHandler(this.SetActiveFocus);
			// 
			// styleTypeLabel
			// 
			this.styleTypeLabel.AutoSize = true;
			this.styleTypeLabel.Location = new System.Drawing.Point(18, 80);
			this.styleTypeLabel.Name = "styleTypeLabel";
			this.styleTypeLabel.Size = new System.Drawing.Size(86, 20);
			this.styleTypeLabel.TabIndex = 27;
			this.styleTypeLabel.Text = "Style Type:";
			this.tooltip.SetToolTip(this.styleTypeLabel, "Determines the scope of text affected by this style");
			// 
			// styleTypeBox
			// 
			this.styleTypeBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.styleTypeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.styleTypeBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.styleTypeBox.FormattingEnabled = true;
			this.styleTypeBox.Items.AddRange(new object[] {
            "Character - words in paragraph",
            "Paragraph - entire paragraph",
            "Heading - include in TOC"});
			this.styleTypeBox.Location = new System.Drawing.Point(169, 77);
			this.styleTypeBox.Name = "styleTypeBox";
			this.styleTypeBox.Size = new System.Drawing.Size(473, 33);
			this.styleTypeBox.TabIndex = 1;
			this.styleTypeBox.SelectedIndexChanged += new System.EventHandler(this.ChangeStyleType);
			// 
			// applyColorsBox
			// 
			this.applyColorsBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.applyColorsBox.Checked = true;
			this.applyColorsBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.applyColorsBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.applyColorsBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.applyColorsBox.Location = new System.Drawing.Point(169, 213);
			this.applyColorsBox.Name = "applyColorsBox";
			this.applyColorsBox.Size = new System.Drawing.Size(125, 25);
			this.applyColorsBox.StylizeImage = false;
			this.applyColorsBox.TabIndex = 5;
			this.applyColorsBox.Text = "Apply colors";
			this.applyColorsBox.ThemedBack = null;
			this.applyColorsBox.ThemedFore = null;
			this.applyColorsBox.UseVisualStyleBackColor = true;
			this.applyColorsBox.CheckedChanged += new System.EventHandler(this.ChangeApplyColorsOption);
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
			this.familyBox.Location = new System.Drawing.Point(169, 123);
			this.familyBox.Name = "familyBox";
			this.familyBox.Size = new System.Drawing.Size(356, 32);
			this.familyBox.TabIndex = 2;
			this.familyBox.SelectedIndexChanged += new System.EventHandler(this.ChangeFontFamily);
			this.familyBox.Enter += new System.EventHandler(this.SetActiveFocus);
			// 
			// mainTools
			// 
			this.mainTools.AutoSize = false;
			this.mainTools.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.mainTools.GripMargin = new System.Windows.Forms.Padding(3);
			this.mainTools.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.mainTools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadButton,
            this.saveButton,
            this.mainSep,
            this.newStyleButton,
            this.reorderButton,
            this.toolStripSeparator1,
            this.resetButton});
			this.mainTools.Location = new System.Drawing.Point(4, 0);
			this.mainTools.Name = "mainTools";
			this.mainTools.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
			this.mainTools.Size = new System.Drawing.Size(672, 32);
			this.mainTools.TabIndex = 0;
			// 
			// loadButton
			// 
			this.loadButton.ForeColor = System.Drawing.Color.Black;
			this.loadButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_FileOpen;
			this.loadButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.loadButton.Name = "loadButton";
			this.loadButton.Size = new System.Drawing.Size(96, 32);
			this.loadButton.Text = "Open";
			this.loadButton.Click += new System.EventHandler(this.LoadTheme);
			// 
			// saveButton
			// 
			this.saveButton.ForeColor = System.Drawing.Color.Black;
			this.saveButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_SaveAs;
			this.saveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = new System.Drawing.Size(111, 32);
			this.saveButton.Text = "Save as";
			this.saveButton.Click += new System.EventHandler(this.SaveTheme);
			// 
			// mainSep
			// 
			this.mainSep.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
			this.mainSep.Name = "mainSep";
			this.mainSep.Size = new System.Drawing.Size(6, 32);
			// 
			// newStyleButton
			// 
			this.newStyleButton.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.newStyleButton.ForeColor = System.Drawing.Color.Black;
			this.newStyleButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_NewStyle;
			this.newStyleButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.newStyleButton.Name = "newStyleButton";
			this.newStyleButton.Size = new System.Drawing.Size(87, 32);
			this.newStyleButton.Text = "New";
			this.newStyleButton.Click += new System.EventHandler(this.AddStyle);
			// 
			// reorderButton
			// 
			this.reorderButton.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.reorderButton.ForeColor = System.Drawing.Color.Black;
			this.reorderButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Reorder;
			this.reorderButton.Name = "reorderButton";
			this.reorderButton.Size = new System.Drawing.Size(114, 32);
			this.reorderButton.Text = "Reorder";
			this.reorderButton.Click += new System.EventHandler(this.ReorderStyles);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 32);
			// 
			// resetButton
			// 
			this.resetButton.Enabled = false;
			this.resetButton.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.resetButton.ForeColor = System.Drawing.Color.Black;
			this.resetButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Reset;
			this.resetButton.Name = "resetButton";
			this.resetButton.Size = new System.Drawing.Size(94, 32);
			this.resetButton.Text = "Reset";
			this.resetButton.Click += new System.EventHandler(this.ResetTheme);
			// 
			// StyleDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(676, 678);
			this.Controls.Add(this.mainTools);
			this.Controls.Add(this.bodyPanel);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "StyleDialog";
			this.Padding = new System.Windows.Forms.Padding(4, 0, 0, 5);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Custom Styles";
			this.TopMost = true;
			((System.ComponentModel.ISupportInitialize)(this.spaceBeforeSpinner)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.spaceAfterSpinner)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.previewBox)).EndInit();
			this.bodyPanel.ResumeLayout(false);
			this.bodyPanel.PerformLayout();
			this.styleTools.ResumeLayout(false);
			this.styleTools.PerformLayout();
			this.fontTools.ResumeLayout(false);
			this.fontTools.PerformLayout();
			this.optionsGroup.ResumeLayout(false);
			this.optionsGroup.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.spacingSpinner)).EndInit();
			this.mainTools.ResumeLayout(false);
			this.mainTools.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private UI.FontComboBox familyBox;
		private System.Windows.Forms.ComboBox sizeBox;
		private System.Windows.Forms.Label beforeLabel;
		private System.Windows.Forms.Label afterLabel;
		private System.Windows.Forms.Label nameLabel;
		private UI.MoreTextBox nameBox;
		private System.Windows.Forms.Label fontLabel;
		private UI.MoreToolStrip fontTools;
		private UI.MoreSplitButton colorButton;
		private System.Windows.Forms.NumericUpDown spaceBeforeSpinner;
		private System.Windows.Forms.NumericUpDown spaceAfterSpinner;
		private UI.MoreSplitButton backColorButton;
		private System.Windows.Forms.PictureBox previewBox;
		private UI.MoreButton cancelButton;
		private UI.MoreButton okButton;
		private System.Windows.Forms.ComboBox namesBox;
		private System.Windows.Forms.ToolStripMenuItem transparentToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem defaultBlackToolStripMenuItem;
		private UI.MoreMenuStrip mainTools;
		private UI.MoreMenuItem reorderButton;
		private UI.MoreMenuItem deleteButton;
		private System.Windows.Forms.Panel bodyPanel;
		private UI.MoreCheckBox applyColorsBox;
		private System.Windows.Forms.Label styleTypeLabel;
		private System.Windows.Forms.ComboBox styleTypeBox;
		private UI.MoreMenuItem loadButton;
		private UI.MoreMenuItem saveButton;
		private UI.MoreMenuItem newStyleButton;
		private System.Windows.Forms.Label spacingLabel;
		private System.Windows.Forms.NumericUpDown spacingSpinner;
		private UI.MoreMenuItem renameButton;
		private UI.MoreGroupBox optionsGroup;
		private UI.MoreCheckBox pageColorBox;
		private UI.MoreCheckBox darkBox;
		private UI.MoreLinkLabel pageColorLink;
		private System.Windows.Forms.ToolTip tooltip;
		private UI.MoreLabel statusLabel;
		private UI.MoreCheckBox ignoredBox;
		private System.Windows.Forms.ToolStripSeparator mainSep;
		private UI.MoreMenuItem boldButton;
		private UI.MoreMenuItem italicButton;
		private UI.MoreMenuItem underlineButton;
		private UI.MoreMenuItem strikeButton;
		private UI.MoreMenuItem superButton;
		private UI.MoreMenuItem subButton;
		private UI.MoreToolStrip styleTools;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private UI.MoreMenuItem resetButton;
	}
}