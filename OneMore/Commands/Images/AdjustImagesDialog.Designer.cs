using System;
using System.IO;

namespace River.OneMoreAddIn.Commands
{
	partial class AdjustImagesDialog
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
			previewBox.Image = null;
			previewBox?.Dispose();

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdjustImagesDialog));
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.pctRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.absRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.percentBox = new River.OneMoreAddIn.UI.MoreNumericUpDown();
			this.pctLabel = new System.Windows.Forms.Label();
			this.widthBox = new River.OneMoreAddIn.UI.MoreNumericUpDown();
			this.heightBox = new River.OneMoreAddIn.UI.MoreNumericUpDown();
			this.widthLabel = new System.Windows.Forms.Label();
			this.heightLabel = new System.Windows.Forms.Label();
			this.viewSizeLabel = new System.Windows.Forms.Label();
			this.presetRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.presetBox = new River.OneMoreAddIn.UI.MoreNumericUpDown();
			this.presetLabel = new System.Windows.Forms.Label();
			this.imageSizeLabel = new System.Windows.Forms.Label();
			this.viewSizeLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.imageSizeLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.allLabel = new System.Windows.Forms.Label();
			this.preserveBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.qualityLabel = new System.Windows.Forms.Label();
			this.qualBar = new System.Windows.Forms.TrackBar();
			this.opacityBox = new River.OneMoreAddIn.UI.MoreNumericUpDown();
			this.opacityLabel = new System.Windows.Forms.Label();
			this.previewGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.previewBox = new System.Windows.Forms.PictureBox();
			this.brightnessLabel = new System.Windows.Forms.Label();
			this.brightnessBox = new River.OneMoreAddIn.UI.MoreNumericUpDown();
			this.contrastLabel = new System.Windows.Forms.Label();
			this.contrastBox = new River.OneMoreAddIn.UI.MoreNumericUpDown();
			this.brightnessBar = new System.Windows.Forms.TrackBar();
			this.contrastBar = new System.Windows.Forms.TrackBar();
			this.opacityBar = new System.Windows.Forms.TrackBar();
			this.storageLabel = new System.Windows.Forms.Label();
			this.storedSizeLabel = new System.Windows.Forms.Label();
			this.qualBox = new River.OneMoreAddIn.UI.MoreNumericUpDown();
			this.styleBox = new System.Windows.Forms.ComboBox();
			this.styleLabel = new System.Windows.Forms.Label();
			this.lockButton = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.saturationBar = new System.Windows.Forms.TrackBar();
			this.saturationLabel = new System.Windows.Forms.Label();
			this.saturationBox = new River.OneMoreAddIn.UI.MoreNumericUpDown();
			this.limitsBox = new System.Windows.Forms.ComboBox();
			this.repositionBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.resetLinkLabel = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.autoSizeRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.percentBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.widthBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.heightBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.presetBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.qualBar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.opacityBox)).BeginInit();
			this.previewGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.previewBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.brightnessBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.contrastBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.brightnessBar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.contrastBar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.opacityBar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.qualBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.saturationBar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.saturationBox)).BeginInit();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(996, 671);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 24;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.Cancel);
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(890, 671);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 23;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OK);
			// 
			// pctRadio
			// 
			this.pctRadio.Checked = true;
			this.pctRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pctRadio.Location = new System.Drawing.Point(39, 118);
			this.pctRadio.Name = "pctRadio";
			this.pctRadio.Size = new System.Drawing.Size(120, 25);
			this.pctRadio.TabIndex = 1;
			this.pctRadio.TabStop = true;
			this.pctRadio.Text = "Percentage";
			this.pctRadio.UseVisualStyleBackColor = true;
			this.pctRadio.Click += new System.EventHandler(this.RadioClick);
			this.pctRadio.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RadioKeyDown);
			// 
			// absRadio
			// 
			this.absRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.absRadio.Location = new System.Drawing.Point(39, 158);
			this.absRadio.Name = "absRadio";
			this.absRadio.Size = new System.Drawing.Size(100, 25);
			this.absRadio.TabIndex = 3;
			this.absRadio.Text = "Absolute";
			this.absRadio.UseVisualStyleBackColor = true;
			this.absRadio.Click += new System.EventHandler(this.RadioClick);
			this.absRadio.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RadioKeyDown);
			// 
			// percentBox
			// 
			this.percentBox.BackColor = System.Drawing.SystemColors.Window;
			this.percentBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.percentBox.Location = new System.Drawing.Point(188, 118);
			this.percentBox.Maximum = new decimal(new int[] {
            6000,
            0,
            0,
            0});
			this.percentBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.percentBox.Name = "percentBox";
			this.percentBox.Size = new System.Drawing.Size(94, 26);
			this.percentBox.TabIndex = 2;
			this.percentBox.ThemedBack = null;
			this.percentBox.ThemedFore = null;
			this.percentBox.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.percentBox.ValueChanged += new System.EventHandler(this.PercentValueChanged);
			// 
			// pctLabel
			// 
			this.pctLabel.AutoSize = true;
			this.pctLabel.Location = new System.Drawing.Point(288, 122);
			this.pctLabel.Name = "pctLabel";
			this.pctLabel.Size = new System.Drawing.Size(23, 20);
			this.pctLabel.TabIndex = 5;
			this.pctLabel.Text = "%";
			// 
			// widthBox
			// 
			this.widthBox.BackColor = System.Drawing.SystemColors.Window;
			this.widthBox.Enabled = false;
			this.widthBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.widthBox.Location = new System.Drawing.Point(188, 158);
			this.widthBox.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
			this.widthBox.Name = "widthBox";
			this.widthBox.Size = new System.Drawing.Size(123, 26);
			this.widthBox.TabIndex = 4;
			this.widthBox.ThemedBack = null;
			this.widthBox.ThemedFore = null;
			this.widthBox.ValueChanged += new System.EventHandler(this.WidthValueChanged);
			// 
			// heightBox
			// 
			this.heightBox.BackColor = System.Drawing.SystemColors.Window;
			this.heightBox.Enabled = false;
			this.heightBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.heightBox.Location = new System.Drawing.Point(188, 189);
			this.heightBox.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
			this.heightBox.Name = "heightBox";
			this.heightBox.Size = new System.Drawing.Size(123, 26);
			this.heightBox.TabIndex = 5;
			this.heightBox.ThemedBack = null;
			this.heightBox.ThemedFore = null;
			this.heightBox.ValueChanged += new System.EventHandler(this.HeightValueChanged);
			// 
			// widthLabel
			// 
			this.widthLabel.AutoSize = true;
			this.widthLabel.Location = new System.Drawing.Point(317, 160);
			this.widthLabel.Name = "widthLabel";
			this.widthLabel.Size = new System.Drawing.Size(50, 20);
			this.widthLabel.TabIndex = 9;
			this.widthLabel.Text = "Width";
			// 
			// heightLabel
			// 
			this.heightLabel.AutoSize = true;
			this.heightLabel.Location = new System.Drawing.Point(317, 191);
			this.heightLabel.Name = "heightLabel";
			this.heightLabel.Size = new System.Drawing.Size(56, 20);
			this.heightLabel.TabIndex = 10;
			this.heightLabel.Text = "Height";
			// 
			// viewSizeLabel
			// 
			this.viewSizeLabel.AutoSize = true;
			this.viewSizeLabel.Location = new System.Drawing.Point(37, 17);
			this.viewSizeLabel.Name = "viewSizeLabel";
			this.viewSizeLabel.Size = new System.Drawing.Size(75, 20);
			this.viewSizeLabel.TabIndex = 11;
			this.viewSizeLabel.Text = "View size";
			// 
			// presetRadio
			// 
			this.presetRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.presetRadio.Location = new System.Drawing.Point(39, 230);
			this.presetRadio.Name = "presetRadio";
			this.presetRadio.Size = new System.Drawing.Size(83, 25);
			this.presetRadio.TabIndex = 7;
			this.presetRadio.Text = "Preset";
			this.presetRadio.UseVisualStyleBackColor = true;
			this.presetRadio.Click += new System.EventHandler(this.RadioClick);
			this.presetRadio.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RadioKeyDown);
			// 
			// presetBox
			// 
			this.presetBox.BackColor = System.Drawing.SystemColors.Window;
			this.presetBox.Enabled = false;
			this.presetBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.presetBox.Location = new System.Drawing.Point(188, 232);
			this.presetBox.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
			this.presetBox.Name = "presetBox";
			this.presetBox.Size = new System.Drawing.Size(123, 26);
			this.presetBox.TabIndex = 8;
			this.presetBox.ThemedBack = null;
			this.presetBox.ThemedFore = null;
			this.presetBox.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
			this.presetBox.ValueChanged += new System.EventHandler(this.PresetValueChanged);
			// 
			// presetLabel
			// 
			this.presetLabel.AutoSize = true;
			this.presetLabel.Location = new System.Drawing.Point(317, 234);
			this.presetLabel.Name = "presetLabel";
			this.presetLabel.Size = new System.Drawing.Size(50, 20);
			this.presetLabel.TabIndex = 15;
			this.presetLabel.Text = "Width";
			// 
			// imageSizeLabel
			// 
			this.imageSizeLabel.AutoSize = true;
			this.imageSizeLabel.Location = new System.Drawing.Point(37, 38);
			this.imageSizeLabel.Name = "imageSizeLabel";
			this.imageSizeLabel.Size = new System.Drawing.Size(86, 20);
			this.imageSizeLabel.TabIndex = 16;
			this.imageSizeLabel.Text = "Image size";
			// 
			// viewSizeLink
			// 
			this.viewSizeLink.ActiveLinkColor = System.Drawing.Color.MediumOrchid;
			this.viewSizeLink.AutoSize = true;
			this.viewSizeLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.viewSizeLink.HoverColor = System.Drawing.Color.Orchid;
			this.viewSizeLink.LinkColor = System.Drawing.Color.MediumOrchid;
			this.viewSizeLink.Location = new System.Drawing.Point(183, 17);
			this.viewSizeLink.Name = "viewSizeLink";
			this.viewSizeLink.Size = new System.Drawing.Size(78, 20);
			this.viewSizeLink.StrictColors = false;
			this.viewSizeLink.TabIndex = 21;
			this.viewSizeLink.TabStop = true;
			this.viewSizeLink.Text = "100 x 100";
			this.viewSizeLink.ThemedBack = null;
			this.viewSizeLink.ThemedFore = null;
			this.viewSizeLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.viewSizeLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ViewSizeClicked);
			// 
			// imageSizeLink
			// 
			this.imageSizeLink.ActiveLinkColor = System.Drawing.Color.MediumOrchid;
			this.imageSizeLink.AutoSize = true;
			this.imageSizeLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.imageSizeLink.HoverColor = System.Drawing.Color.Orchid;
			this.imageSizeLink.LinkColor = System.Drawing.Color.MediumOrchid;
			this.imageSizeLink.Location = new System.Drawing.Point(183, 38);
			this.imageSizeLink.Name = "imageSizeLink";
			this.imageSizeLink.Size = new System.Drawing.Size(78, 20);
			this.imageSizeLink.StrictColors = false;
			this.imageSizeLink.TabIndex = 22;
			this.imageSizeLink.TabStop = true;
			this.imageSizeLink.Text = "100 x 100";
			this.imageSizeLink.ThemedBack = null;
			this.imageSizeLink.ThemedFore = null;
			this.imageSizeLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.imageSizeLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OriginalSizeClicked);
			// 
			// allLabel
			// 
			this.allLabel.AutoSize = true;
			this.allLabel.Location = new System.Drawing.Point(288, 17);
			this.allLabel.Name = "allLabel";
			this.allLabel.Size = new System.Drawing.Size(170, 20);
			this.allLabel.TabIndex = 20;
			this.allLabel.Text = "all images on this page";
			this.allLabel.Visible = false;
			// 
			// preserveBox
			// 
			this.preserveBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.preserveBox.Checked = true;
			this.preserveBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.preserveBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.preserveBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.preserveBox.Location = new System.Drawing.Point(39, 563);
			this.preserveBox.Name = "preserveBox";
			this.preserveBox.Size = new System.Drawing.Size(194, 25);
			this.preserveBox.StylizeImage = false;
			this.preserveBox.TabIndex = 20;
			this.preserveBox.Text = "Preserve storage size";
			this.preserveBox.ThemedBack = null;
			this.preserveBox.ThemedFore = null;
			this.preserveBox.UseVisualStyleBackColor = true;
			this.preserveBox.CheckedChanged += new System.EventHandler(this.EstimateStorage);
			// 
			// qualityLabel
			// 
			this.qualityLabel.AutoSize = true;
			this.qualityLabel.Location = new System.Drawing.Point(36, 516);
			this.qualityLabel.Name = "qualityLabel";
			this.qualityLabel.Size = new System.Drawing.Size(57, 20);
			this.qualityLabel.TabIndex = 1;
			this.qualityLabel.Text = "Quality";
			this.qualityLabel.DoubleClick += new System.EventHandler(this.ResetDoubleClick);
			// 
			// qualBar
			// 
			this.qualBar.AutoSize = false;
			this.qualBar.LargeChange = 10;
			this.qualBar.Location = new System.Drawing.Point(292, 516);
			this.qualBar.Maximum = 100;
			this.qualBar.Name = "qualBar";
			this.qualBar.Size = new System.Drawing.Size(190, 25);
			this.qualBar.SmallChange = 5;
			this.qualBar.TabIndex = 19;
			this.qualBar.TickFrequency = 5;
			this.qualBar.TickStyle = System.Windows.Forms.TickStyle.None;
			this.qualBar.Value = 100;
			this.qualBar.Scroll += new System.EventHandler(this.EstimateStorage);
			// 
			// opacityBox
			// 
			this.opacityBox.BackColor = System.Drawing.SystemColors.Window;
			this.opacityBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.opacityBox.Location = new System.Drawing.Point(187, 323);
			this.opacityBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.opacityBox.Name = "opacityBox";
			this.opacityBox.Size = new System.Drawing.Size(94, 26);
			this.opacityBox.TabIndex = 9;
			this.opacityBox.ThemedBack = null;
			this.opacityBox.ThemedFore = null;
			this.opacityBox.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.opacityBox.ValueChanged += new System.EventHandler(this.SlideValueChanged);
			// 
			// opacityLabel
			// 
			this.opacityLabel.AutoSize = true;
			this.opacityLabel.Location = new System.Drawing.Point(35, 326);
			this.opacityLabel.Name = "opacityLabel";
			this.opacityLabel.Size = new System.Drawing.Size(62, 20);
			this.opacityLabel.TabIndex = 24;
			this.opacityLabel.Text = "Opacity";
			this.opacityLabel.DoubleClick += new System.EventHandler(this.ResetDoubleClick);
			// 
			// previewGroup
			// 
			this.previewGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.previewGroup.Controls.Add(this.previewBox);
			this.previewGroup.Location = new System.Drawing.Point(512, 38);
			this.previewGroup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.previewGroup.Name = "previewGroup";
			this.previewGroup.Padding = new System.Windows.Forms.Padding(22, 23, 22, 23);
			this.previewGroup.Size = new System.Drawing.Size(584, 609);
			this.previewGroup.TabIndex = 25;
			this.previewGroup.TabStop = false;
			this.previewGroup.Text = "Preview";
			// 
			// previewBox
			// 
			this.previewBox.BackColor = System.Drawing.Color.White;
			this.previewBox.BackgroundImage = global::River.OneMoreAddIn.Properties.Resources.Grid40;
			this.previewBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.previewBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.previewBox.Location = new System.Drawing.Point(22, 42);
			this.previewBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.previewBox.Name = "previewBox";
			this.previewBox.Size = new System.Drawing.Size(540, 544);
			this.previewBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.previewBox.TabIndex = 0;
			this.previewBox.TabStop = false;
			this.previewBox.SizeChanged += new System.EventHandler(this.DrawOnSizeChanged);
			// 
			// brightnessLabel
			// 
			this.brightnessLabel.AutoSize = true;
			this.brightnessLabel.Location = new System.Drawing.Point(35, 358);
			this.brightnessLabel.Name = "brightnessLabel";
			this.brightnessLabel.Size = new System.Drawing.Size(85, 20);
			this.brightnessLabel.TabIndex = 28;
			this.brightnessLabel.Text = "Brightness";
			this.brightnessLabel.DoubleClick += new System.EventHandler(this.ResetDoubleClick);
			// 
			// brightnessBox
			// 
			this.brightnessBox.BackColor = System.Drawing.SystemColors.Window;
			this.brightnessBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.brightnessBox.Location = new System.Drawing.Point(187, 355);
			this.brightnessBox.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
			this.brightnessBox.Name = "brightnessBox";
			this.brightnessBox.Size = new System.Drawing.Size(94, 26);
			this.brightnessBox.TabIndex = 11;
			this.brightnessBox.ThemedBack = null;
			this.brightnessBox.ThemedFore = null;
			this.brightnessBox.ValueChanged += new System.EventHandler(this.SlideValueChanged);
			// 
			// contrastLabel
			// 
			this.contrastLabel.AutoSize = true;
			this.contrastLabel.Location = new System.Drawing.Point(35, 390);
			this.contrastLabel.Name = "contrastLabel";
			this.contrastLabel.Size = new System.Drawing.Size(70, 20);
			this.contrastLabel.TabIndex = 31;
			this.contrastLabel.Text = "Contrast";
			this.contrastLabel.DoubleClick += new System.EventHandler(this.ResetDoubleClick);
			// 
			// contrastBox
			// 
			this.contrastBox.BackColor = System.Drawing.SystemColors.Window;
			this.contrastBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.contrastBox.Location = new System.Drawing.Point(187, 387);
			this.contrastBox.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
			this.contrastBox.Name = "contrastBox";
			this.contrastBox.Size = new System.Drawing.Size(94, 26);
			this.contrastBox.TabIndex = 13;
			this.contrastBox.ThemedBack = null;
			this.contrastBox.ThemedFore = null;
			this.contrastBox.ValueChanged += new System.EventHandler(this.SlideValueChanged);
			// 
			// brightnessBar
			// 
			this.brightnessBar.AutoSize = false;
			this.brightnessBar.LargeChange = 10;
			this.brightnessBar.Location = new System.Drawing.Point(292, 358);
			this.brightnessBar.Maximum = 100;
			this.brightnessBar.Minimum = -100;
			this.brightnessBar.Name = "brightnessBar";
			this.brightnessBar.Size = new System.Drawing.Size(190, 25);
			this.brightnessBar.TabIndex = 12;
			this.brightnessBar.TickFrequency = 5;
			this.brightnessBar.TickStyle = System.Windows.Forms.TickStyle.None;
			this.brightnessBar.Scroll += new System.EventHandler(this.SlideValueChanged);
			// 
			// contrastBar
			// 
			this.contrastBar.AutoSize = false;
			this.contrastBar.LargeChange = 10;
			this.contrastBar.Location = new System.Drawing.Point(292, 389);
			this.contrastBar.Maximum = 100;
			this.contrastBar.Minimum = -100;
			this.contrastBar.Name = "contrastBar";
			this.contrastBar.Size = new System.Drawing.Size(190, 25);
			this.contrastBar.TabIndex = 14;
			this.contrastBar.TickFrequency = 5;
			this.contrastBar.TickStyle = System.Windows.Forms.TickStyle.None;
			this.contrastBar.Scroll += new System.EventHandler(this.SlideValueChanged);
			// 
			// opacityBar
			// 
			this.opacityBar.AutoSize = false;
			this.opacityBar.LargeChange = 10;
			this.opacityBar.Location = new System.Drawing.Point(292, 327);
			this.opacityBar.Maximum = 100;
			this.opacityBar.Minimum = 1;
			this.opacityBar.Name = "opacityBar";
			this.opacityBar.Size = new System.Drawing.Size(190, 25);
			this.opacityBar.TabIndex = 10;
			this.opacityBar.TickFrequency = 5;
			this.opacityBar.TickStyle = System.Windows.Forms.TickStyle.None;
			this.opacityBar.Value = 100;
			this.opacityBar.ValueChanged += new System.EventHandler(this.SlideValueChanged);
			// 
			// storageLabel
			// 
			this.storageLabel.AutoSize = true;
			this.storageLabel.Location = new System.Drawing.Point(37, 60);
			this.storageLabel.Name = "storageLabel";
			this.storageLabel.Size = new System.Drawing.Size(66, 20);
			this.storageLabel.TabIndex = 37;
			this.storageLabel.Text = "Storage";
			// 
			// storedSizeLabel
			// 
			this.storedSizeLabel.AutoSize = true;
			this.storedSizeLabel.Location = new System.Drawing.Point(184, 60);
			this.storedSizeLabel.Name = "storedSizeLabel";
			this.storedSizeLabel.Size = new System.Drawing.Size(60, 20);
			this.storedSizeLabel.TabIndex = 38;
			this.storedSizeLabel.Text = "0 bytes";
			// 
			// qualBox
			// 
			this.qualBox.BackColor = System.Drawing.SystemColors.Window;
			this.qualBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.qualBox.Location = new System.Drawing.Point(187, 514);
			this.qualBox.Name = "qualBox";
			this.qualBox.Size = new System.Drawing.Size(94, 26);
			this.qualBox.TabIndex = 18;
			this.qualBox.ThemedBack = null;
			this.qualBox.ThemedFore = null;
			this.qualBox.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.qualBox.ValueChanged += new System.EventHandler(this.EstimateStorage);
			// 
			// styleBox
			// 
			this.styleBox.BackColor = System.Drawing.SystemColors.Window;
			this.styleBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.styleBox.FormattingEnabled = true;
			this.styleBox.Items.AddRange(new object[] {
            "Original",
            "Gray scale",
            "Sepia",
            "Polaroid",
            "Invert"});
			this.styleBox.Location = new System.Drawing.Point(187, 463);
			this.styleBox.Name = "styleBox";
			this.styleBox.Size = new System.Drawing.Size(295, 28);
			this.styleBox.TabIndex = 17;
			this.styleBox.SelectedIndexChanged += new System.EventHandler(this.StylizeSelectedIndexChanged);
			// 
			// styleLabel
			// 
			this.styleLabel.AutoSize = true;
			this.styleLabel.Location = new System.Drawing.Point(37, 466);
			this.styleLabel.Name = "styleLabel";
			this.styleLabel.Size = new System.Drawing.Size(55, 20);
			this.styleLabel.TabIndex = 43;
			this.styleLabel.Text = "Stylize";
			// 
			// lockButton
			// 
			this.lockButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.lockButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.lockButton.Checked = true;
			this.lockButton.CheckState = System.Windows.Forms.CheckState.Checked;
			this.lockButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lockButton.Enabled = false;
			this.lockButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.lockButton.Image = global::River.OneMoreAddIn.Properties.Resources.Locked;
			this.lockButton.Location = new System.Drawing.Point(403, 167);
			this.lockButton.Name = "lockButton";
			this.lockButton.Size = new System.Drawing.Size(33, 33);
			this.lockButton.StylizeImage = false;
			this.lockButton.TabIndex = 6;
			this.lockButton.ThemedBack = null;
			this.lockButton.ThemedFore = null;
			this.lockButton.UseVisualStyleBackColor = true;
			this.lockButton.CheckedChanged += new System.EventHandler(this.LockAspectCheckedChanged);
			// 
			// saturationBar
			// 
			this.saturationBar.AutoSize = false;
			this.saturationBar.LargeChange = 10;
			this.saturationBar.Location = new System.Drawing.Point(292, 421);
			this.saturationBar.Maximum = 100;
			this.saturationBar.Minimum = -100;
			this.saturationBar.Name = "saturationBar";
			this.saturationBar.Size = new System.Drawing.Size(190, 25);
			this.saturationBar.TabIndex = 16;
			this.saturationBar.TickFrequency = 5;
			this.saturationBar.TickStyle = System.Windows.Forms.TickStyle.None;
			this.saturationBar.Scroll += new System.EventHandler(this.SlideValueChanged);
			// 
			// saturationLabel
			// 
			this.saturationLabel.AutoSize = true;
			this.saturationLabel.Location = new System.Drawing.Point(35, 422);
			this.saturationLabel.Name = "saturationLabel";
			this.saturationLabel.Size = new System.Drawing.Size(83, 20);
			this.saturationLabel.TabIndex = 47;
			this.saturationLabel.Text = "Saturation";
			this.saturationLabel.DoubleClick += new System.EventHandler(this.ResetDoubleClick);
			// 
			// saturationBox
			// 
			this.saturationBox.BackColor = System.Drawing.SystemColors.Window;
			this.saturationBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.saturationBox.Location = new System.Drawing.Point(187, 419);
			this.saturationBox.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
			this.saturationBox.Name = "saturationBox";
			this.saturationBox.Size = new System.Drawing.Size(94, 26);
			this.saturationBox.TabIndex = 15;
			this.saturationBox.ThemedBack = null;
			this.saturationBox.ThemedFore = null;
			this.saturationBox.ValueChanged += new System.EventHandler(this.SlideValueChanged);
			// 
			// limitsBox
			// 
			this.limitsBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.limitsBox.FormattingEnabled = true;
			this.limitsBox.Items.AddRange(new object[] {
            "Resize all images",
            "Do not shrink larger images",
            "Do no enlarge smaller images"});
			this.limitsBox.Location = new System.Drawing.Point(25, 671);
			this.limitsBox.Name = "limitsBox";
			this.limitsBox.Size = new System.Drawing.Size(295, 28);
			this.limitsBox.TabIndex = 0;
			this.limitsBox.Visible = false;
			// 
			// repositionBox
			// 
			this.repositionBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.repositionBox.Checked = true;
			this.repositionBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.repositionBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.repositionBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.repositionBox.Location = new System.Drawing.Point(39, 594);
			this.repositionBox.Name = "repositionBox";
			this.repositionBox.Size = new System.Drawing.Size(262, 25);
			this.repositionBox.StylizeImage = false;
			this.repositionBox.TabIndex = 48;
			this.repositionBox.Text = "Reposition background images";
			this.repositionBox.ThemedBack = null;
			this.repositionBox.ThemedFore = null;
			this.repositionBox.UseVisualStyleBackColor = true;
			this.repositionBox.Visible = false;
			// 
			// resetLinkLabel
			// 
			this.resetLinkLabel.ActiveLinkColor = System.Drawing.Color.MediumOrchid;
			this.resetLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.resetLinkLabel.AutoSize = true;
			this.resetLinkLabel.Cursor = System.Windows.Forms.Cursors.Hand;
			this.resetLinkLabel.HoverColor = System.Drawing.Color.Orchid;
			this.resetLinkLabel.LinkColor = System.Drawing.Color.MediumOrchid;
			this.resetLinkLabel.Location = new System.Drawing.Point(642, 680);
			this.resetLinkLabel.Name = "resetLinkLabel";
			this.resetLinkLabel.Size = new System.Drawing.Size(154, 20);
			this.resetLinkLabel.StrictColors = false;
			this.resetLinkLabel.TabIndex = 49;
			this.resetLinkLabel.TabStop = true;
			this.resetLinkLabel.Text = "Reset default values";
			this.resetLinkLabel.ThemedBack = null;
			this.resetLinkLabel.ThemedFore = null;
			this.resetLinkLabel.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.resetLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ResetDefaultValues);
			// 
			// autoSizeRadio
			// 
			this.autoSizeRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.autoSizeRadio.Location = new System.Drawing.Point(39, 270);
			this.autoSizeRadio.Name = "autoSizeRadio";
			this.autoSizeRadio.Size = new System.Drawing.Size(104, 25);
			this.autoSizeRadio.TabIndex = 50;
			this.autoSizeRadio.Text = "Auto-size";
			this.autoSizeRadio.UseVisualStyleBackColor = true;
			this.autoSizeRadio.Click += new System.EventHandler(this.RadioClick);
			this.autoSizeRadio.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RadioKeyDown);
			// 
			// AdjustImagesDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(1122, 728);
			this.Controls.Add(this.autoSizeRadio);
			this.Controls.Add(this.resetLinkLabel);
			this.Controls.Add(this.repositionBox);
			this.Controls.Add(this.limitsBox);
			this.Controls.Add(this.saturationBar);
			this.Controls.Add(this.saturationLabel);
			this.Controls.Add(this.saturationBox);
			this.Controls.Add(this.lockButton);
			this.Controls.Add(this.styleLabel);
			this.Controls.Add(this.styleBox);
			this.Controls.Add(this.qualBox);
			this.Controls.Add(this.qualBar);
			this.Controls.Add(this.qualityLabel);
			this.Controls.Add(this.preserveBox);
			this.Controls.Add(this.storedSizeLabel);
			this.Controls.Add(this.storageLabel);
			this.Controls.Add(this.opacityBar);
			this.Controls.Add(this.contrastBar);
			this.Controls.Add(this.brightnessBar);
			this.Controls.Add(this.contrastLabel);
			this.Controls.Add(this.contrastBox);
			this.Controls.Add(this.brightnessLabel);
			this.Controls.Add(this.brightnessBox);
			this.Controls.Add(this.previewGroup);
			this.Controls.Add(this.opacityLabel);
			this.Controls.Add(this.opacityBox);
			this.Controls.Add(this.allLabel);
			this.Controls.Add(this.imageSizeLink);
			this.Controls.Add(this.viewSizeLink);
			this.Controls.Add(this.imageSizeLabel);
			this.Controls.Add(this.presetLabel);
			this.Controls.Add(this.presetBox);
			this.Controls.Add(this.presetRadio);
			this.Controls.Add(this.viewSizeLabel);
			this.Controls.Add(this.heightLabel);
			this.Controls.Add(this.widthLabel);
			this.Controls.Add(this.heightBox);
			this.Controls.Add(this.widthBox);
			this.Controls.Add(this.pctLabel);
			this.Controls.Add(this.percentBox);
			this.Controls.Add(this.absRadio);
			this.Controls.Add(this.pctRadio);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimizeBox = false;
			this.Name = "AdjustImagesDialog";
			this.Padding = new System.Windows.Forms.Padding(22, 23, 22, 8);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Resize and Adjust Image";
			((System.ComponentModel.ISupportInitialize)(this.percentBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.widthBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.heightBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.presetBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.qualBar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.opacityBox)).EndInit();
			this.previewGroup.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.previewBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.brightnessBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.contrastBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.brightnessBar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.contrastBar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.opacityBar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.qualBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.saturationBar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.saturationBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreButton cancelButton;
		private UI.MoreButton okButton;
		private UI.MoreRadioButton pctRadio;
		private UI.MoreRadioButton absRadio;
		private UI.MoreNumericUpDown percentBox;
		private System.Windows.Forms.Label pctLabel;
		private UI.MoreNumericUpDown widthBox;
		private UI.MoreNumericUpDown heightBox;
		private System.Windows.Forms.Label widthLabel;
		private System.Windows.Forms.Label heightLabel;
		private System.Windows.Forms.Label viewSizeLabel;
		private UI.MoreRadioButton presetRadio;
		private UI.MoreNumericUpDown presetBox;
		private System.Windows.Forms.Label presetLabel;
		private System.Windows.Forms.Label imageSizeLabel;
		private UI.MoreLinkLabel viewSizeLink;
		private UI.MoreLinkLabel imageSizeLink;
		private System.Windows.Forms.Label allLabel;
		private System.Windows.Forms.Label qualityLabel;
		private System.Windows.Forms.TrackBar qualBar;
		private UI.MoreCheckBox preserveBox;
		private UI.MoreNumericUpDown opacityBox;
		private System.Windows.Forms.Label opacityLabel;
		private UI.MoreGroupBox previewGroup;
		private System.Windows.Forms.PictureBox previewBox;
		private System.Windows.Forms.Label brightnessLabel;
		private UI.MoreNumericUpDown brightnessBox;
		private System.Windows.Forms.Label contrastLabel;
		private UI.MoreNumericUpDown contrastBox;
		private System.Windows.Forms.TrackBar brightnessBar;
		private System.Windows.Forms.TrackBar contrastBar;
		private System.Windows.Forms.TrackBar opacityBar;
		private System.Windows.Forms.Label storageLabel;
		private System.Windows.Forms.Label storedSizeLabel;
		private UI.MoreNumericUpDown qualBox;
		private System.Windows.Forms.ComboBox styleBox;
		private System.Windows.Forms.Label styleLabel;
		private UI.MoreCheckBox lockButton;
		private System.Windows.Forms.TrackBar saturationBar;
		private System.Windows.Forms.Label saturationLabel;
		private UI.MoreNumericUpDown saturationBox;
		private System.Windows.Forms.ComboBox limitsBox;
		private UI.MoreCheckBox repositionBox;
		private UI.MoreLinkLabel resetLinkLabel;
		private UI.MoreRadioButton autoSizeRadio;
	}
}