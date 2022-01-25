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
			if (!string.IsNullOrEmpty(temp) && File.Exists(temp))
			{
				try
				{
					File.Delete(temp);
				}
				catch (Exception exc)
				{
					logger.WriteLine($"error deleting temp file {temp}", exc);
				}
			}

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
			((System.ComponentModel.ISupportInitialize)(this.pctUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.widthUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.heightUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.presetUpDown)).BeginInit();
			this.qualBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.qualBar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.opacityBox)).BeginInit();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(377, 588);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.TabIndex = 11;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.Cancel);
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.Location = new System.Drawing.Point(271, 588);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.TabIndex = 10;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OK);
			// 
			// pctRadio
			// 
			this.pctRadio.AutoSize = true;
			this.pctRadio.Checked = true;
			this.pctRadio.Location = new System.Drawing.Point(17, 113);
			this.pctRadio.Name = "pctRadio";
			this.pctRadio.Size = new System.Drawing.Size(116, 24);
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
			this.absRadio.Location = new System.Drawing.Point(17, 173);
			this.absRadio.Name = "absRadio";
			this.absRadio.Size = new System.Drawing.Size(97, 24);
			this.absRadio.TabIndex = 4;
			this.absRadio.Text = "Absolute";
			this.absRadio.UseVisualStyleBackColor = true;
			this.absRadio.Click += new System.EventHandler(this.RadioClick);
			this.absRadio.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RadioKeyDown);
			// 
			// pctUpDown
			// 
			this.pctUpDown.Location = new System.Drawing.Point(192, 113);
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
			this.pctUpDown.Size = new System.Drawing.Size(95, 26);
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
			this.pctLabel.Location = new System.Drawing.Point(293, 115);
			this.pctLabel.Name = "pctLabel";
			this.pctLabel.Size = new System.Drawing.Size(23, 20);
			this.pctLabel.TabIndex = 5;
			this.pctLabel.Text = "%";
			// 
			// aspectBox
			// 
			this.aspectBox.AutoSize = true;
			this.aspectBox.Checked = true;
			this.aspectBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.aspectBox.Enabled = false;
			this.aspectBox.Location = new System.Drawing.Point(192, 174);
			this.aspectBox.Name = "aspectBox";
			this.aspectBox.Size = new System.Drawing.Size(182, 24);
			this.aspectBox.TabIndex = 5;
			this.aspectBox.Text = "Maintain aspect ratio";
			this.aspectBox.UseVisualStyleBackColor = true;
			this.aspectBox.CheckedChanged += new System.EventHandler(this.MaintainAspectCheckedChanged);
			// 
			// widthUpDown
			// 
			this.widthUpDown.Enabled = false;
			this.widthUpDown.Location = new System.Drawing.Point(192, 206);
			this.widthUpDown.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
			this.widthUpDown.Name = "widthUpDown";
			this.widthUpDown.Size = new System.Drawing.Size(145, 26);
			this.widthUpDown.TabIndex = 6;
			this.widthUpDown.ValueChanged += new System.EventHandler(this.WidthValueChanged);
			// 
			// heightUpDown
			// 
			this.heightUpDown.Enabled = false;
			this.heightUpDown.Location = new System.Drawing.Point(192, 238);
			this.heightUpDown.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
			this.heightUpDown.Name = "heightUpDown";
			this.heightUpDown.Size = new System.Drawing.Size(145, 26);
			this.heightUpDown.TabIndex = 7;
			this.heightUpDown.ValueChanged += new System.EventHandler(this.HeightValueChanged);
			// 
			// widthLabel
			// 
			this.widthLabel.AutoSize = true;
			this.widthLabel.Location = new System.Drawing.Point(343, 208);
			this.widthLabel.Name = "widthLabel";
			this.widthLabel.Size = new System.Drawing.Size(50, 20);
			this.widthLabel.TabIndex = 9;
			this.widthLabel.Text = "Width";
			// 
			// heightLabel
			// 
			this.heightLabel.AutoSize = true;
			this.heightLabel.Location = new System.Drawing.Point(343, 240);
			this.heightLabel.Name = "heightLabel";
			this.heightLabel.Size = new System.Drawing.Size(56, 20);
			this.heightLabel.TabIndex = 10;
			this.heightLabel.Text = "Height";
			// 
			// currentLabel
			// 
			this.currentLabel.AutoSize = true;
			this.currentLabel.Location = new System.Drawing.Point(40, 33);
			this.currentLabel.Name = "currentLabel";
			this.currentLabel.Size = new System.Drawing.Size(75, 20);
			this.currentLabel.TabIndex = 11;
			this.currentLabel.Text = "View size";
			// 
			// presetRadio
			// 
			this.presetRadio.AutoSize = true;
			this.presetRadio.Location = new System.Drawing.Point(17, 301);
			this.presetRadio.Name = "presetRadio";
			this.presetRadio.Size = new System.Drawing.Size(80, 24);
			this.presetRadio.TabIndex = 8;
			this.presetRadio.Text = "Preset";
			this.presetRadio.UseVisualStyleBackColor = true;
			this.presetRadio.Click += new System.EventHandler(this.RadioClick);
			this.presetRadio.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RadioKeyDown);
			// 
			// presetUpDown
			// 
			this.presetUpDown.Enabled = false;
			this.presetUpDown.Location = new System.Drawing.Point(192, 301);
			this.presetUpDown.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
			this.presetUpDown.Name = "presetUpDown";
			this.presetUpDown.Size = new System.Drawing.Size(145, 26);
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
			this.presetLabel.Location = new System.Drawing.Point(343, 303);
			this.presetLabel.Name = "presetLabel";
			this.presetLabel.Size = new System.Drawing.Size(50, 20);
			this.presetLabel.TabIndex = 15;
			this.presetLabel.Text = "Width";
			// 
			// origLabel
			// 
			this.origLabel.AutoSize = true;
			this.origLabel.Location = new System.Drawing.Point(40, 53);
			this.origLabel.Name = "origLabel";
			this.origLabel.Size = new System.Drawing.Size(98, 20);
			this.origLabel.TabIndex = 16;
			this.origLabel.Text = "Storage size";
			// 
			// sizeLink
			// 
			this.sizeLink.AutoSize = true;
			this.sizeLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.sizeLink.Location = new System.Drawing.Point(188, 33);
			this.sizeLink.Name = "sizeLink";
			this.sizeLink.Size = new System.Drawing.Size(78, 20);
			this.sizeLink.TabIndex = 0;
			this.sizeLink.TabStop = true;
			this.sizeLink.Text = "100 x 100";
			this.sizeLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ResetToCurrentSize);
			// 
			// origSizeLink
			// 
			this.origSizeLink.AutoSize = true;
			this.origSizeLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.origSizeLink.Location = new System.Drawing.Point(188, 53);
			this.origSizeLink.Name = "origSizeLink";
			this.origSizeLink.Size = new System.Drawing.Size(78, 20);
			this.origSizeLink.TabIndex = 1;
			this.origSizeLink.TabStop = true;
			this.origSizeLink.Text = "100 x 100";
			this.origSizeLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ResetToOriginalSize);
			// 
			// allLabel
			// 
			this.allLabel.AutoSize = true;
			this.allLabel.Location = new System.Drawing.Point(286, 33);
			this.allLabel.Name = "allLabel";
			this.allLabel.Size = new System.Drawing.Size(141, 20);
			this.allLabel.TabIndex = 20;
			this.allLabel.Text = "all images on page";
			this.allLabel.Visible = false;
			// 
			// qualBox
			// 
			this.qualBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.qualBox.Controls.Add(this.preserveBox);
			this.qualBox.Controls.Add(this.qualLabel);
			this.qualBox.Controls.Add(this.qualBar);
			this.qualBox.Location = new System.Drawing.Point(17, 432);
			this.qualBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
			this.qualBox.Name = "qualBox";
			this.qualBox.Size = new System.Drawing.Size(460, 145);
			this.qualBox.TabIndex = 21;
			this.qualBox.TabStop = false;
			this.qualBox.Text = "Storage: 0 bytes";
			// 
			// preserveBox
			// 
			this.preserveBox.AutoSize = true;
			this.preserveBox.Checked = true;
			this.preserveBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.preserveBox.Location = new System.Drawing.Point(27, 25);
			this.preserveBox.Name = "preserveBox";
			this.preserveBox.Size = new System.Drawing.Size(187, 24);
			this.preserveBox.TabIndex = 0;
			this.preserveBox.Text = "Preserve storage size";
			this.preserveBox.UseVisualStyleBackColor = true;
			this.preserveBox.CheckedChanged += new System.EventHandler(this.EstimateStorage);
			// 
			// qualLabel
			// 
			this.qualLabel.AutoSize = true;
			this.qualLabel.Location = new System.Drawing.Point(23, 109);
			this.qualLabel.Name = "qualLabel";
			this.qualLabel.Size = new System.Drawing.Size(99, 20);
			this.qualLabel.TabIndex = 1;
			this.qualLabel.Text = "100% quality";
			// 
			// qualBar
			// 
			this.qualBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.qualBar.AutoSize = false;
			this.qualBar.LargeChange = 10;
			this.qualBar.Location = new System.Drawing.Point(27, 55);
			this.qualBar.Maximum = 100;
			this.qualBar.Minimum = 5;
			this.qualBar.Name = "qualBar";
			this.qualBar.Size = new System.Drawing.Size(392, 51);
			this.qualBar.SmallChange = 5;
			this.qualBar.TabIndex = 1;
			this.qualBar.TickFrequency = 5;
			this.qualBar.Value = 100;
			this.qualBar.Scroll += new System.EventHandler(this.EstimateStorage);
			// 
			// opacityBox
			// 
			this.opacityBox.Location = new System.Drawing.Point(192, 368);
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
			this.opacityBox.Size = new System.Drawing.Size(95, 26);
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
			this.opacityPctLabel.Location = new System.Drawing.Point(293, 370);
			this.opacityPctLabel.Name = "opacityPctLabel";
			this.opacityPctLabel.Size = new System.Drawing.Size(23, 20);
			this.opacityPctLabel.TabIndex = 23;
			this.opacityPctLabel.Text = "%";
			// 
			// opacityLabel
			// 
			this.opacityLabel.AutoSize = true;
			this.opacityLabel.Location = new System.Drawing.Point(40, 370);
			this.opacityLabel.Name = "opacityLabel";
			this.opacityLabel.Size = new System.Drawing.Size(62, 20);
			this.opacityLabel.TabIndex = 24;
			this.opacityLabel.Text = "Opacity";
			// 
			// ResizeImagesDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(494, 643);
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
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ResizeImagesDialog";
			this.Padding = new System.Windows.Forms.Padding(10, 5, 5, 5);
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
	}
}