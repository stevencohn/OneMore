namespace River.OneMoreAddIn
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
			this.colorStrip = new System.Windows.Forms.ToolStrip();
			this.backColorButton = new System.Windows.Forms.ToolStripSplitButton();
			this.transparentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.colorButton = new System.Windows.Forms.ToolStripSplitButton();
			this.defaultBlackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.spaceBeforeSpinner = new System.Windows.Forms.NumericUpDown();
			this.spaceAfterSpinner = new System.Windows.Forms.NumericUpDown();
			this.previewBox = new System.Windows.Forms.PictureBox();
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.underlineButton = new River.OneMoreAddIn.FlatToggleButton();
			this.italicButton = new River.OneMoreAddIn.FlatToggleButton();
			this.boldButton = new River.OneMoreAddIn.FlatToggleButton();
			this.familyBox = new River.OneMoreAddIn.FontComboBox();
			this.namesBox = new System.Windows.Forms.ComboBox();
			this.headingBox = new System.Windows.Forms.CheckBox();
			this.mainTools = new System.Windows.Forms.ToolStrip();
			this.reorderButton = new System.Windows.Forms.ToolStripButton();
			this.deleteButton = new System.Windows.Forms.ToolStripButton();
			this.panel1 = new System.Windows.Forms.Panel();
			this.colorStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.spaceBeforeSpinner)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.spaceAfterSpinner)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.previewBox)).BeginInit();
			this.mainTools.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// sizeBox
			// 
			this.sizeBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.sizeBox.FormattingEnabled = true;
			this.sizeBox.Items.AddRange(new object[] {
            "8",
            "9",
            "10",
            "11",
            "12",
            "14",
            "16",
            "18",
            "20",
            "22",
            "24",
            "26"});
			this.sizeBox.Location = new System.Drawing.Point(381, 78);
			this.sizeBox.Name = "sizeBox";
			this.sizeBox.Size = new System.Drawing.Size(80, 33);
			this.sizeBox.TabIndex = 1;
			this.sizeBox.SelectedIndexChanged += new System.EventHandler(this.UpdateFont);
			// 
			// beforeLabel
			// 
			this.beforeLabel.AutoSize = true;
			this.beforeLabel.Location = new System.Drawing.Point(16, 195);
			this.beforeLabel.Name = "beforeLabel";
			this.beforeLabel.Size = new System.Drawing.Size(107, 20);
			this.beforeLabel.TabIndex = 6;
			this.beforeLabel.Text = "Space Before";
			// 
			// afterLabel
			// 
			this.afterLabel.AutoSize = true;
			this.afterLabel.Location = new System.Drawing.Point(29, 232);
			this.afterLabel.Name = "afterLabel";
			this.afterLabel.Size = new System.Drawing.Size(94, 20);
			this.afterLabel.TabIndex = 7;
			this.afterLabel.Text = "Space After";
			// 
			// nameLabel
			// 
			this.nameLabel.AutoSize = true;
			this.nameLabel.Location = new System.Drawing.Point(33, 30);
			this.nameLabel.Name = "nameLabel";
			this.nameLabel.Size = new System.Drawing.Size(90, 20);
			this.nameLabel.TabIndex = 8;
			this.nameLabel.Text = "Style Name";
			// 
			// nameBox
			// 
			this.nameBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.nameBox.Location = new System.Drawing.Point(129, 23);
			this.nameBox.Name = "nameBox";
			this.nameBox.Size = new System.Drawing.Size(332, 31);
			this.nameBox.TabIndex = 9;
			this.nameBox.TextChanged += new System.EventHandler(this.nameBox_TextChanged);
			// 
			// fontLabel
			// 
			this.fontLabel.AutoSize = true;
			this.fontLabel.Location = new System.Drawing.Point(81, 85);
			this.fontLabel.Name = "fontLabel";
			this.fontLabel.Size = new System.Drawing.Size(42, 20);
			this.fontLabel.TabIndex = 10;
			this.fontLabel.Text = "Font";
			// 
			// colorStrip
			// 
			this.colorStrip.AutoSize = false;
			this.colorStrip.BackColor = System.Drawing.Color.Transparent;
			this.colorStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.colorStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.colorStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.colorStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.backColorButton,
            this.colorButton});
			this.colorStrip.Location = new System.Drawing.Point(294, 120);
			this.colorStrip.Name = "colorStrip";
			this.colorStrip.Padding = new System.Windows.Forms.Padding(0);
			this.colorStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.colorStrip.Size = new System.Drawing.Size(167, 50);
			this.colorStrip.TabIndex = 15;
			// 
			// backColorButton
			// 
			this.backColorButton.AutoSize = false;
			this.backColorButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.backColorButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.transparentToolStripMenuItem});
			this.backColorButton.Image = global::River.OneMoreAddIn.Properties.Resources.TextHighlightColorPicker;
			this.backColorButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.backColorButton.Name = "backColorButton";
			this.backColorButton.Size = new System.Drawing.Size(50, 34);
			this.backColorButton.ToolTipText = "Highlight Color";
			this.backColorButton.ButtonClick += new System.EventHandler(this.backColorButton_ButtonClick);
			// 
			// transparentToolStripMenuItem
			// 
			this.transparentToolStripMenuItem.Name = "transparentToolStripMenuItem";
			this.transparentToolStripMenuItem.Size = new System.Drawing.Size(187, 30);
			this.transparentToolStripMenuItem.Text = "Transparent";
			this.transparentToolStripMenuItem.Click += new System.EventHandler(this.transparentToolStripMenuItem_Click);
			// 
			// colorButton
			// 
			this.colorButton.AutoSize = false;
			this.colorButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.colorButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.defaultBlackToolStripMenuItem});
			this.colorButton.Image = global::River.OneMoreAddIn.Properties.Resources.FontColor;
			this.colorButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.colorButton.Margin = new System.Windows.Forms.Padding(4, 1, 0, 2);
			this.colorButton.Name = "colorButton";
			this.colorButton.Size = new System.Drawing.Size(50, 34);
			this.colorButton.ToolTipText = "Text Color";
			this.colorButton.ButtonClick += new System.EventHandler(this.colorButton_Click);
			// 
			// defaultBlackToolStripMenuItem
			// 
			this.defaultBlackToolStripMenuItem.Name = "defaultBlackToolStripMenuItem";
			this.defaultBlackToolStripMenuItem.Size = new System.Drawing.Size(208, 30);
			this.defaultBlackToolStripMenuItem.Text = "Default (Black)";
			this.defaultBlackToolStripMenuItem.Click += new System.EventHandler(this.defaultBlackToolStripMenuItem_Click);
			// 
			// spaceBeforeSpinner
			// 
			this.spaceBeforeSpinner.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.spaceBeforeSpinner.Location = new System.Drawing.Point(129, 189);
			this.spaceBeforeSpinner.Name = "spaceBeforeSpinner";
			this.spaceBeforeSpinner.Size = new System.Drawing.Size(90, 31);
			this.spaceBeforeSpinner.TabIndex = 16;
			this.spaceBeforeSpinner.ValueChanged += new System.EventHandler(this.spaceBeforeSpinner_ValueChanged);
			// 
			// spaceAfterSpinner
			// 
			this.spaceAfterSpinner.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.spaceAfterSpinner.Location = new System.Drawing.Point(129, 226);
			this.spaceAfterSpinner.Name = "spaceAfterSpinner";
			this.spaceAfterSpinner.Size = new System.Drawing.Size(90, 31);
			this.spaceAfterSpinner.TabIndex = 17;
			this.spaceAfterSpinner.ValueChanged += new System.EventHandler(this.spaceAfterSpinner_ValueChanged);
			// 
			// previewBox
			// 
			this.previewBox.BackColor = System.Drawing.Color.White;
			this.previewBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.previewBox.Location = new System.Drawing.Point(259, 189);
			this.previewBox.Name = "previewBox";
			this.previewBox.Size = new System.Drawing.Size(202, 107);
			this.previewBox.TabIndex = 18;
			this.previewBox.TabStop = false;
			this.previewBox.Paint += new System.Windows.Forms.PaintEventHandler(this.previewBox_Paint);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(347, 326);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(115, 38);
			this.cancelButton.TabIndex = 19;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(226, 326);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(115, 38);
			this.okButton.TabIndex = 20;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// underlineButton
			// 
			this.underlineButton.BackColor = System.Drawing.SystemColors.Control;
			this.underlineButton.Checked = false;
			this.underlineButton.Image = global::River.OneMoreAddIn.Properties.Resources.Underline;
			this.underlineButton.Location = new System.Drawing.Point(241, 132);
			this.underlineButton.Name = "underlineButton";
			this.underlineButton.Padding = new System.Windows.Forms.Padding(4);
			this.underlineButton.Size = new System.Drawing.Size(50, 34);
			this.underlineButton.TabIndex = 14;
			this.underlineButton.CheckedChanged += new System.EventHandler(this.UpdateFont);
			// 
			// italicButton
			// 
			this.italicButton.BackColor = System.Drawing.SystemColors.Control;
			this.italicButton.Checked = false;
			this.italicButton.Image = global::River.OneMoreAddIn.Properties.Resources.Italic;
			this.italicButton.Location = new System.Drawing.Point(185, 132);
			this.italicButton.Name = "italicButton";
			this.italicButton.Padding = new System.Windows.Forms.Padding(4);
			this.italicButton.Size = new System.Drawing.Size(50, 34);
			this.italicButton.TabIndex = 13;
			this.italicButton.CheckedChanged += new System.EventHandler(this.UpdateFont);
			// 
			// boldButton
			// 
			this.boldButton.BackColor = System.Drawing.SystemColors.Control;
			this.boldButton.Checked = false;
			this.boldButton.Image = global::River.OneMoreAddIn.Properties.Resources.Bold;
			this.boldButton.Location = new System.Drawing.Point(129, 132);
			this.boldButton.Name = "boldButton";
			this.boldButton.Padding = new System.Windows.Forms.Padding(4);
			this.boldButton.Size = new System.Drawing.Size(50, 34);
			this.boldButton.TabIndex = 12;
			this.boldButton.CheckedChanged += new System.EventHandler(this.UpdateFont);
			// 
			// familyBox
			// 
			this.familyBox.DropDownHeight = 400;
			this.familyBox.DropDownWidth = 350;
			this.familyBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.familyBox.FormattingEnabled = true;
			this.familyBox.IntegralHeight = false;
			this.familyBox.Items.AddRange(new object[] {
            "Agency FB",
            "Algerian",
            "Antitled OT Medium",
            "AntitledBook",
            "AntitledRegular",
            "Arial",
            "Arial Black",
            "Arial Narrow",
            "Arial Rounded MT Bold",
            "Bahnschrift",
            "Bahnschrift Light",
            "Bahnschrift SemiBold",
            "Bahnschrift SemiLight",
            "Baskerville Old Face",
            "Bauhaus 93",
            "Bell MT",
            "Berlin Sans FB",
            "Berlin Sans FB Demi",
            "Bernard MT Condensed",
            "Blackadder ITC",
            "Bodoni MT",
            "Bodoni MT Black",
            "Bodoni MT Condensed",
            "Bodoni MT Poster Compressed",
            "Book Antiqua",
            "Bookman Old Style",
            "Bookshelf Symbol 7",
            "Bradley Hand ITC",
            "Britannic Bold",
            "Broadway",
            "Brush Script MT",
            "Calibri",
            "Calibri Light",
            "Californian FB",
            "Calisto MT",
            "Cambria",
            "Cambria Math",
            "Candara",
            "Castellar",
            "Centaur",
            "Century",
            "Century Gothic",
            "Century Schoolbook",
            "Chiller",
            "ChollaSans",
            "ChollaSans-Thin",
            "ChollaSans Thin",
            "ChollaSansBold",
            "ChollaUnicase",
            "Colonna MT",
            "Comic Sans MS",
            "Consolas",
            "Constantia",
            "Cooper Black",
            "Copperplate Gothic Bold",
            "Copperplate Gothic Light",
            "Corbel",
            "Courier New",
            "Curlz MT",
            "DRAFTSMAN",
            "Ebrima",
            "Edwardian Script ITC",
            "Elephant",
            "Engravers MT",
            "Eras Bold ITC",
            "Eras Demi ITC",
            "Eras Light ITC",
            "Eras Medium ITC",
            "Felix Titling",
            "Footlight MT Light",
            "Forte",
            "Franklin Gothic Book",
            "Franklin Gothic Demi",
            "Franklin Gothic Demi Cond",
            "Franklin Gothic Heavy",
            "Franklin Gothic Medium",
            "Franklin Gothic Medium Cond",
            "Freestyle Script",
            "French Script MT",
            "Futura Lt BT",
            "Gabriola",
            "Gadugi",
            "Garamond",
            "Georgia",
            "Gigi",
            "Gill Sans MT",
            "Gill Sans MT Condensed",
            "Gill Sans MT Ext Condensed Bold",
            "Gill Sans Ultra Bold",
            "Gill Sans Ultra Bold Condensed",
            "Gloucester MT Extra Condensed",
            "Goudy Old Style",
            "Goudy Stout",
            "Haettenschweiler",
            "Harlow Solid Italic",
            "Harrington",
            "HelvLight",
            "High Tower Text",
            "HoloLens MDL2 Assets",
            "Impact",
            "Imprint MT Shadow",
            "Informal Roman",
            "Javanese Text",
            "Jokerman",
            "Juice ITC",
            "Kristen ITC",
            "Kunstler Script",
            "Lato",
            "Leelawadee UI",
            "Leelawadee UI Semilight",
            "Lucida Bright",
            "Lucida Calligraphy",
            "Lucida Console",
            "Lucida Fax",
            "Lucida Handwriting",
            "Lucida Sans",
            "Lucida Sans Typewriter",
            "Lucida Sans Unicode",
            "Magneto",
            "Maiandra GD",
            "Malgun Gothic",
            "Malgun Gothic Semilight",
            "Marlett",
            "Matura MT Script Capitals",
            "Microsoft Himalaya",
            "Microsoft JhengHei",
            "Microsoft JhengHei Light",
            "Microsoft JhengHei UI",
            "Microsoft JhengHei UI Light",
            "Microsoft New Tai Lue",
            "Microsoft PhagsPa",
            "Microsoft Sans Serif",
            "Microsoft Tai Le",
            "Microsoft YaHei",
            "Microsoft YaHei Light",
            "Microsoft YaHei UI",
            "Microsoft YaHei UI Light",
            "Microsoft Yi Baiti",
            "MingLiU-ExtB",
            "MingLiU_HKSCS-ExtB",
            "Mistral",
            "Modern No. 20",
            "Mongolian Baiti",
            "Monotype Corsiva",
            "MS Gothic",
            "MS Outlook",
            "MS PGothic",
            "MS Reference Sans Serif",
            "MS Reference Specialty",
            "MS UI Gothic",
            "MT Extra",
            "MV Boli",
            "Myanmar Text",
            "Niagara Engraved",
            "Niagara Solid",
            "Nirmala UI",
            "Nirmala UI Semilight",
            "NSimSun",
            "OCR-A II",
            "OCR A Extended",
            "OCR B MT",
            "Old English Text MT",
            "Onyx",
            "Open Sans",
            "Open Sans Extrabold",
            "Open Sans Light",
            "Open Sans Semibold",
            "Palace Script MT",
            "Palatino Linotype",
            "Papyrus",
            "Parchment",
            "Perpetua",
            "Perpetua Titling MT",
            "Playbill",
            "PMingLiU-ExtB",
            "Poor Richard",
            "Pristina",
            "QuickType II",
            "QuickType II Condensed",
            "QuickType II Mono",
            "QuickType II Pi",
            "Rage Italic",
            "Ravie",
            "Rockwell",
            "Rockwell Condensed",
            "Rockwell Extra Bold",
            "Script MT Bold",
            "Segoe MDL2 Assets",
            "Segoe Print",
            "Segoe Script",
            "Segoe UI",
            "Segoe UI Black",
            "Segoe UI Emoji",
            "Segoe UI Historic",
            "Segoe UI Light",
            "Segoe UI Semibold",
            "Segoe UI Semilight",
            "Segoe UI Symbol",
            "Showcard Gothic",
            "SimSun",
            "SimSun-ExtB",
            "Sitka Banner",
            "Sitka Display",
            "Sitka Heading",
            "Sitka Small",
            "Sitka Subheading",
            "Sitka Text",
            "SMC Handwriting",
            "Snap ITC",
            "Stencil",
            "Sylfaen",
            "Symbol",
            "Tahoma",
            "TeamViewer11",
            "Tempus Sans ITC",
            "Times New Roman",
            "Trajan Pro",
            "Trebuchet MS",
            "Tw Cen MT",
            "Tw Cen MT Condensed",
            "Tw Cen MT Condensed Extra Bold",
            "Verdana",
            "Viner Hand ITC",
            "Vivaldi",
            "Vladimir Script",
            "Webdings",
            "Wide Latin",
            "Wingdings",
            "Wingdings 2",
            "Wingdings 3",
            "Yu Gothic",
            "Yu Gothic Light",
            "Yu Gothic Medium",
            "Yu Gothic UI",
            "Yu Gothic UI Light",
            "Yu Gothic UI Semibold",
            "Yu Gothic UI Semilight",
            "ZWAdobeF"});
			this.familyBox.Location = new System.Drawing.Point(129, 78);
			this.familyBox.Name = "familyBox";
			this.familyBox.Size = new System.Drawing.Size(246, 32);
			this.familyBox.TabIndex = 0;
			this.familyBox.SelectedIndexChanged += new System.EventHandler(this.UpdateFont);
			// 
			// namesBox
			// 
			this.namesBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.namesBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.namesBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.namesBox.FormattingEnabled = true;
			this.namesBox.Location = new System.Drawing.Point(33, 331);
			this.namesBox.Name = "namesBox";
			this.namesBox.Size = new System.Drawing.Size(121, 33);
			this.namesBox.TabIndex = 21;
			this.namesBox.Visible = false;
			this.namesBox.SelectedIndexChanged += new System.EventHandler(this.namesBox_SelectedIndexChanged);
			// 
			// headingBox
			// 
			this.headingBox.AutoSize = true;
			this.headingBox.Location = new System.Drawing.Point(129, 272);
			this.headingBox.Name = "headingBox";
			this.headingBox.Size = new System.Drawing.Size(95, 24);
			this.headingBox.TabIndex = 23;
			this.headingBox.Text = "Heading";
			this.headingBox.UseVisualStyleBackColor = true;
			this.headingBox.CheckedChanged += new System.EventHandler(this.headingBox_CheckedChanged);
			// 
			// mainTools
			// 
			this.mainTools.AutoSize = false;
			this.mainTools.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.mainTools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.reorderButton,
            this.deleteButton});
			this.mainTools.Location = new System.Drawing.Point(4, 4);
			this.mainTools.Name = "mainTools";
			this.mainTools.Size = new System.Drawing.Size(505, 38);
			this.mainTools.TabIndex = 24;
			// 
			// reorderButton
			// 
			this.reorderButton.Image = global::River.OneMoreAddIn.Properties.Resources.Reorder;
			this.reorderButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.reorderButton.Name = "reorderButton";
			this.reorderButton.Size = new System.Drawing.Size(102, 35);
			this.reorderButton.Text = "Reorder";
			// 
			// deleteButton
			// 
			this.deleteButton.Image = global::River.OneMoreAddIn.Properties.Resources.Delete;
			this.deleteButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.deleteButton.Margin = new System.Windows.Forms.Padding(5, 1, 0, 2);
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.Size = new System.Drawing.Size(90, 35);
			this.deleteButton.Text = "Delete";
			this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.nameLabel);
			this.panel1.Controls.Add(this.familyBox);
			this.panel1.Controls.Add(this.okButton);
			this.panel1.Controls.Add(this.headingBox);
			this.panel1.Controls.Add(this.cancelButton);
			this.panel1.Controls.Add(this.sizeBox);
			this.panel1.Controls.Add(this.beforeLabel);
			this.panel1.Controls.Add(this.namesBox);
			this.panel1.Controls.Add(this.afterLabel);
			this.panel1.Controls.Add(this.nameBox);
			this.panel1.Controls.Add(this.fontLabel);
			this.panel1.Controls.Add(this.previewBox);
			this.panel1.Controls.Add(this.boldButton);
			this.panel1.Controls.Add(this.spaceAfterSpinner);
			this.panel1.Controls.Add(this.italicButton);
			this.panel1.Controls.Add(this.spaceBeforeSpinner);
			this.panel1.Controls.Add(this.underlineButton);
			this.panel1.Controls.Add(this.colorStrip);
			this.panel1.Location = new System.Drawing.Point(7, 45);
			this.panel1.Name = "panel1";
			this.panel1.Padding = new System.Windows.Forms.Padding(30, 30, 30, 10);
			this.panel1.Size = new System.Drawing.Size(495, 377);
			this.panel1.TabIndex = 25;
			// 
			// StyleDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(509, 430);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.mainTools);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "StyleDialog";
			this.Padding = new System.Windows.Forms.Padding(4, 4, 0, 0);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Custom Styles";
			this.Shown += new System.EventHandler(this.StyleDialog_Shown);
			this.colorStrip.ResumeLayout(false);
			this.colorStrip.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.spaceBeforeSpinner)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.spaceAfterSpinner)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.previewBox)).EndInit();
			this.mainTools.ResumeLayout(false);
			this.mainTools.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private FontComboBox familyBox;
		private System.Windows.Forms.ComboBox sizeBox;
		private System.Windows.Forms.Label beforeLabel;
		private System.Windows.Forms.Label afterLabel;
		private System.Windows.Forms.Label nameLabel;
		private System.Windows.Forms.TextBox nameBox;
		private System.Windows.Forms.Label fontLabel;
		private FlatToggleButton boldButton;
		private FlatToggleButton italicButton;
		private FlatToggleButton underlineButton;
		private System.Windows.Forms.ToolStrip colorStrip;
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
		private System.Windows.Forms.CheckBox headingBox;
		private System.Windows.Forms.ToolStrip mainTools;
		private System.Windows.Forms.ToolStripButton reorderButton;
		private System.Windows.Forms.ToolStripButton deleteButton;
		private System.Windows.Forms.Panel panel1;
	}
}