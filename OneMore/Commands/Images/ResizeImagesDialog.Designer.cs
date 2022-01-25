using System;
using System.IO;

namespace River.OneMoreAddIn.Commands
{
	partial class ResizeImagesDialog
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
			image?.Dispose();

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResizeImagesDialog));
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.pctRadio = new System.Windows.Forms.RadioButton();
			this.absRadio = new System.Windows.Forms.RadioButton();
			this.pctUpDown = new System.Windows.Forms.NumericUpDown();
			this.pctLabel = new System.Windows.Forms.Label();
			this.aspectBox = new System.Windows.Forms.CheckBox();
			this.widthUpDown = new System.Windows.Forms.NumericUpDown();
			this.heightUpDown = new System.Windows.Forms.NumericUpDown();
			this.widthLabel = new System.Windows.Forms.Label();
			this.heightLabel = new System.Windows.Forms.Label();
			this.currentLabel = new System.Windows.Forms.Label();
			this.presetRadio = new System.Windows.Forms.RadioButton();
			this.presetUpDown = new System.Windows.Forms.NumericUpDown();
			this.presetLabel = new System.Windows.Forms.Label();
			this.origLabel = new System.Windows.Forms.Label();
			this.sizeLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.origSizeLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.allLabel = new System.Windows.Forms.Label();
			this.qualBox = new System.Windows.Forms.GroupBox();
			this.preserveBox = new System.Windows.Forms.CheckBox();
			this.qualLabel = new System.Windows.Forms.Label();
			this.qualBar = new System.Windows.Forms.TrackBar();
			this.opacityBox = new System.Windows.Forms.NumericUpDown();
			this.opacityPctLabel = new System.Windows.Forms.Label();
			this.opacityLabel = new System.Windows.Forms.Label();
			this.previewGroup = new System.Windows.Forms.GroupBox();
			this.previewBox = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.pctUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.widthUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.heightUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.presetUpDown)).BeginInit();
			this.qualBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.qualBar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.opacityBox)).BeginInit();
			this.previewGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.previewBox)).BeginInit();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(599, 380);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(2);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(67, 25);
			this.cancelButton.TabIndex = 11;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.Cancel);
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.Location = new System.Drawing.Point(529, 380);
			this.okButton.Margin = new System.Windows.Forms.Padding(2);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(67, 25);
			this.okButton.TabIndex = 10;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OK);
			// 
			// pctRadio
			// 
			this.pctRadio.AutoSize = true;
			this.pctRadio.Checked = true;
			this.pctRadio.Location = new System.Drawing.Point(16, 77);
			this.pctRadio.Margin = new System.Windows.Forms.Padding(2);
			this.pctRadio.Name = "pctRadio";
			this.pctRadio.Size = new System.Drawing.Size(80, 17);
			this.pctRadio.TabIndex = 2;
			this.pctRadio.TabStop = true;
			this.pctRadio.Text = "Percentage";
			this.pctRadio.UseVisualStyleBackColor = true;
			this.pctRadio.Click += new System.EventHandler(this.RadioClick);
			this.pctRadio.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RadioKeyDown);
			// 
			// absRadio
			// 
			this.absRadio.AutoSize = true;
			this.absRadio.Location = new System.Drawing.Point(16, 116);
			this.absRadio.Margin = new System.Windows.Forms.Padding(2);
			this.absRadio.Name = "absRadio";
			this.absRadio.Size = new System.Drawing.Size(66, 17);
			this.absRadio.TabIndex = 4;
			this.absRadio.Text = "Absolute";
			this.absRadio.UseVisualStyleBackColor = true;
			this.absRadio.Click += new System.EventHandler(this.RadioClick);
			this.absRadio.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RadioKeyDown);
			// 
			// pctUpDown
			// 
			this.pctUpDown.Location = new System.Drawing.Point(133, 77);
			this.pctUpDown.Margin = new System.Windows.Forms.Padding(2);
			this.pctUpDown.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
			this.pctUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.pctUpDown.Name = "pctUpDown";
			this.pctUpDown.Size = new System.Drawing.Size(63, 20);
			this.pctUpDown.TabIndex = 3;
			this.pctUpDown.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.pctUpDown.ValueChanged += new System.EventHandler(this.PercentValueChanged);
			// 
			// pctLabel
			// 
			this.pctLabel.AutoSize = true;
			this.pctLabel.Location = new System.Drawing.Point(200, 79);
			this.pctLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.pctLabel.Name = "pctLabel";
			this.pctLabel.Size = new System.Drawing.Size(15, 13);
			this.pctLabel.TabIndex = 5;
			this.pctLabel.Text = "%";
			// 
			// aspectBox
			// 
			this.aspectBox.AutoSize = true;
			this.aspectBox.Checked = true;
			this.aspectBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.aspectBox.Enabled = false;
			this.aspectBox.Location = new System.Drawing.Point(133, 116);
			this.aspectBox.Margin = new System.Windows.Forms.Padding(2);
			this.aspectBox.Name = "aspectBox";
			this.aspectBox.Size = new System.Drawing.Size(124, 17);
			this.aspectBox.TabIndex = 5;
			this.aspectBox.Text = "Maintain aspect ratio";
			this.aspectBox.UseVisualStyleBackColor = true;
			this.aspectBox.CheckedChanged += new System.EventHandler(this.MaintainAspectCheckedChanged);
			// 
			// widthUpDown
			// 
			this.widthUpDown.Enabled = false;
			this.widthUpDown.Location = new System.Drawing.Point(133, 137);
			this.widthUpDown.Margin = new System.Windows.Forms.Padding(2);
			this.widthUpDown.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
			this.widthUpDown.Name = "widthUpDown";
			this.widthUpDown.Size = new System.Drawing.Size(97, 20);
			this.widthUpDown.TabIndex = 6;
			this.widthUpDown.ValueChanged += new System.EventHandler(this.WidthValueChanged);
			// 
			// heightUpDown
			// 
			this.heightUpDown.Enabled = false;
			this.heightUpDown.Location = new System.Drawing.Point(133, 161);
			this.heightUpDown.Margin = new System.Windows.Forms.Padding(2);
			this.heightUpDown.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
			this.heightUpDown.Name = "heightUpDown";
			this.heightUpDown.Size = new System.Drawing.Size(97, 20);
			this.heightUpDown.TabIndex = 7;
			this.heightUpDown.ValueChanged += new System.EventHandler(this.HeightValueChanged);
			// 
			// widthLabel
			// 
			this.widthLabel.AutoSize = true;
			this.widthLabel.Location = new System.Drawing.Point(234, 139);
			this.widthLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.widthLabel.Name = "widthLabel";
			this.widthLabel.Size = new System.Drawing.Size(35, 13);
			this.widthLabel.TabIndex = 9;
			this.widthLabel.Text = "Width";
			// 
			// heightLabel
			// 
			this.heightLabel.AutoSize = true;
			this.heightLabel.Location = new System.Drawing.Point(234, 163);
			this.heightLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.heightLabel.Name = "heightLabel";
			this.heightLabel.Size = new System.Drawing.Size(38, 13);
			this.heightLabel.TabIndex = 10;
			this.heightLabel.Text = "Height";
			// 
			// currentLabel
			// 
			this.currentLabel.AutoSize = true;
			this.currentLabel.Location = new System.Drawing.Point(32, 25);
			this.currentLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.currentLabel.Name = "currentLabel";
			this.currentLabel.Size = new System.Drawing.Size(51, 13);
			this.currentLabel.TabIndex = 11;
			this.currentLabel.Text = "View size";
			// 
			// presetRadio
			// 
			this.presetRadio.AutoSize = true;
			this.presetRadio.Location = new System.Drawing.Point(16, 191);
			this.presetRadio.Margin = new System.Windows.Forms.Padding(2);
			this.presetRadio.Name = "presetRadio";
			this.presetRadio.Size = new System.Drawing.Size(55, 17);
			this.presetRadio.TabIndex = 8;
			this.presetRadio.Text = "Preset";
			this.presetRadio.UseVisualStyleBackColor = true;
			this.presetRadio.Click += new System.EventHandler(this.RadioClick);
			this.presetRadio.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RadioKeyDown);
			// 
			// presetUpDown
			// 
			this.presetUpDown.Enabled = false;
			this.presetUpDown.Location = new System.Drawing.Point(133, 191);
			this.presetUpDown.Margin = new System.Windows.Forms.Padding(2);
			this.presetUpDown.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
			this.presetUpDown.Name = "presetUpDown";
			this.presetUpDown.Size = new System.Drawing.Size(97, 20);
			this.presetUpDown.TabIndex = 9;
			this.presetUpDown.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
			this.presetUpDown.ValueChanged += new System.EventHandler(this.PresetValueChanged);
			// 
			// presetLabel
			// 
			this.presetLabel.AutoSize = true;
			this.presetLabel.Location = new System.Drawing.Point(234, 193);
			this.presetLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.presetLabel.Name = "presetLabel";
			this.presetLabel.Size = new System.Drawing.Size(35, 13);
			this.presetLabel.TabIndex = 15;
			this.presetLabel.Text = "Width";
			// 
			// origLabel
			// 
			this.origLabel.AutoSize = true;
			this.origLabel.Location = new System.Drawing.Point(32, 38);
			this.origLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.origLabel.Name = "origLabel";
			this.origLabel.Size = new System.Drawing.Size(65, 13);
			this.origLabel.TabIndex = 16;
			this.origLabel.Text = "Storage size";
			// 
			// sizeLink
			// 
			this.sizeLink.AutoSize = true;
			this.sizeLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.sizeLink.Location = new System.Drawing.Point(130, 25);
			this.sizeLink.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.sizeLink.Name = "sizeLink";
			this.sizeLink.Size = new System.Drawing.Size(54, 13);
			this.sizeLink.TabIndex = 0;
			this.sizeLink.TabStop = true;
			this.sizeLink.Text = "100 x 100";
			this.sizeLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ResetToCurrentSize);
			// 
			// origSizeLink
			// 
			this.origSizeLink.AutoSize = true;
			this.origSizeLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.origSizeLink.Location = new System.Drawing.Point(130, 38);
			this.origSizeLink.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.origSizeLink.Name = "origSizeLink";
			this.origSizeLink.Size = new System.Drawing.Size(54, 13);
			this.origSizeLink.TabIndex = 1;
			this.origSizeLink.TabStop = true;
			this.origSizeLink.Text = "100 x 100";
			this.origSizeLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ResetToOriginalSize);
			// 
			// allLabel
			// 
			this.allLabel.AutoSize = true;
			this.allLabel.Location = new System.Drawing.Point(196, 25);
			this.allLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.allLabel.Name = "allLabel";
			this.allLabel.Size = new System.Drawing.Size(95, 13);
			this.allLabel.TabIndex = 20;
			this.allLabel.Text = "all images on page";
			this.allLabel.Visible = false;
			// 
			// qualBox
			// 
			this.qualBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.qualBox.Controls.Add(this.preserveBox);
			this.qualBox.Controls.Add(this.qualLabel);
			this.qualBox.Controls.Add(this.qualBar);
			this.qualBox.Location = new System.Drawing.Point(17, 278);
			this.qualBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 5);
			this.qualBox.Name = "qualBox";
			this.qualBox.Padding = new System.Windows.Forms.Padding(2);
			this.qualBox.Size = new System.Drawing.Size(312, 94);
			this.qualBox.TabIndex = 21;
			this.qualBox.TabStop = false;
			this.qualBox.Text = "Storage: 0 bytes";
			// 
			// preserveBox
			// 
			this.preserveBox.AutoSize = true;
			this.preserveBox.Checked = true;
			this.preserveBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.preserveBox.Location = new System.Drawing.Point(18, 16);
			this.preserveBox.Margin = new System.Windows.Forms.Padding(2);
			this.preserveBox.Name = "preserveBox";
			this.preserveBox.Size = new System.Drawing.Size(127, 17);
			this.preserveBox.TabIndex = 0;
			this.preserveBox.Text = "Preserve storage size";
			this.preserveBox.UseVisualStyleBackColor = true;
			this.preserveBox.CheckedChanged += new System.EventHandler(this.EstimateStorage);
			// 
			// qualLabel
			// 
			this.qualLabel.AutoSize = true;
			this.qualLabel.Location = new System.Drawing.Point(15, 71);
			this.qualLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.qualLabel.Name = "qualLabel";
			this.qualLabel.Size = new System.Drawing.Size(66, 13);
			this.qualLabel.TabIndex = 1;
			this.qualLabel.Text = "100% quality";
			// 
			// qualBar
			// 
			this.qualBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.qualBar.AutoSize = false;
			this.qualBar.LargeChange = 10;
			this.qualBar.Location = new System.Drawing.Point(18, 36);
			this.qualBar.Margin = new System.Windows.Forms.Padding(2);
			this.qualBar.Maximum = 100;
			this.qualBar.Minimum = 5;
			this.qualBar.Name = "qualBar";
			this.qualBar.Size = new System.Drawing.Size(266, 33);
			this.qualBar.SmallChange = 5;
			this.qualBar.TabIndex = 1;
			this.qualBar.TickFrequency = 5;
			this.qualBar.Value = 100;
			this.qualBar.Scroll += new System.EventHandler(this.EstimateStorage);
			// 
			// opacityBox
			// 
			this.opacityBox.Location = new System.Drawing.Point(133, 234);
			this.opacityBox.Margin = new System.Windows.Forms.Padding(2);
			this.opacityBox.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
			this.opacityBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.opacityBox.Name = "opacityBox";
			this.opacityBox.Size = new System.Drawing.Size(63, 20);
			this.opacityBox.TabIndex = 22;
			this.opacityBox.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.opacityBox.ValueChanged += new System.EventHandler(this.OpacityValueChanged);
			// 
			// opacityPctLabel
			// 
			this.opacityPctLabel.AutoSize = true;
			this.opacityPctLabel.Location = new System.Drawing.Point(200, 236);
			this.opacityPctLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.opacityPctLabel.Name = "opacityPctLabel";
			this.opacityPctLabel.Size = new System.Drawing.Size(15, 13);
			this.opacityPctLabel.TabIndex = 23;
			this.opacityPctLabel.Text = "%";
			// 
			// opacityLabel
			// 
			this.opacityLabel.AutoSize = true;
			this.opacityLabel.Location = new System.Drawing.Point(32, 236);
			this.opacityLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.opacityLabel.Name = "opacityLabel";
			this.opacityLabel.Size = new System.Drawing.Size(43, 13);
			this.opacityLabel.TabIndex = 24;
			this.opacityLabel.Text = "Opacity";
			// 
			// previewGroup
			// 
			this.previewGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.previewGroup.Controls.Add(this.previewBox);
			this.previewGroup.Location = new System.Drawing.Point(341, 25);
			this.previewGroup.Name = "previewGroup";
			this.previewGroup.Padding = new System.Windows.Forms.Padding(15);
			this.previewGroup.Size = new System.Drawing.Size(325, 347);
			this.previewGroup.TabIndex = 25;
			this.previewGroup.TabStop = false;
			this.previewGroup.Text = "Preview";
			// 
			// previewBox
			// 
			this.previewBox.BackColor = System.Drawing.Color.White;
			this.previewBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.previewBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.previewBox.Location = new System.Drawing.Point(15, 28);
			this.previewBox.Name = "previewBox";
			this.previewBox.Size = new System.Drawing.Size(295, 304);
			this.previewBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.previewBox.TabIndex = 0;
			this.previewBox.TabStop = false;
			// 
			// ResizeImagesDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(684, 417);
			this.Controls.Add(this.previewGroup);
			this.Controls.Add(this.opacityLabel);
			this.Controls.Add(this.opacityPctLabel);
			this.Controls.Add(this.opacityBox);
			this.Controls.Add(this.qualBox);
			this.Controls.Add(this.allLabel);
			this.Controls.Add(this.origSizeLink);
			this.Controls.Add(this.sizeLink);
			this.Controls.Add(this.origLabel);
			this.Controls.Add(this.presetLabel);
			this.Controls.Add(this.presetUpDown);
			this.Controls.Add(this.presetRadio);
			this.Controls.Add(this.currentLabel);
			this.Controls.Add(this.heightLabel);
			this.Controls.Add(this.widthLabel);
			this.Controls.Add(this.heightUpDown);
			this.Controls.Add(this.widthUpDown);
			this.Controls.Add(this.aspectBox);
			this.Controls.Add(this.pctLabel);
			this.Controls.Add(this.pctUpDown);
			this.Controls.Add(this.absRadio);
			this.Controls.Add(this.pctRadio);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(2);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ResizeImagesDialog";
			this.Padding = new System.Windows.Forms.Padding(15, 15, 15, 5);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Resize Image";
			((System.ComponentModel.ISupportInitialize)(this.pctUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.widthUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.heightUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.presetUpDown)).EndInit();
			this.qualBox.ResumeLayout(false);
			this.qualBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.qualBar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.opacityBox)).EndInit();
			this.previewGroup.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.previewBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.RadioButton pctRadio;
		private System.Windows.Forms.RadioButton absRadio;
		private System.Windows.Forms.NumericUpDown pctUpDown;
		private System.Windows.Forms.Label pctLabel;
		private System.Windows.Forms.CheckBox aspectBox;
		private System.Windows.Forms.NumericUpDown widthUpDown;
		private System.Windows.Forms.NumericUpDown heightUpDown;
		private System.Windows.Forms.Label widthLabel;
		private System.Windows.Forms.Label heightLabel;
		private System.Windows.Forms.Label currentLabel;
		private System.Windows.Forms.RadioButton presetRadio;
		private System.Windows.Forms.NumericUpDown presetUpDown;
		private System.Windows.Forms.Label presetLabel;
		private System.Windows.Forms.Label origLabel;
		private UI.MoreLinkLabel sizeLink;
		private UI.MoreLinkLabel origSizeLink;
		private System.Windows.Forms.Label allLabel;
		private System.Windows.Forms.GroupBox qualBox;
		private System.Windows.Forms.Label qualLabel;
		private System.Windows.Forms.TrackBar qualBar;
		private System.Windows.Forms.CheckBox preserveBox;
		private System.Windows.Forms.NumericUpDown opacityBox;
		private System.Windows.Forms.Label opacityPctLabel;
		private System.Windows.Forms.Label opacityLabel;
		private System.Windows.Forms.GroupBox previewGroup;
		private System.Windows.Forms.PictureBox previewBox;
	}
}