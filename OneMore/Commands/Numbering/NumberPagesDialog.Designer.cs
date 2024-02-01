namespace River.OneMoreAddIn.Commands
{
	partial class NumberPagesDialog
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
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.numberingGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.introLabel = new System.Windows.Forms.Label();
			this.cleanBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.alphaDemoBox = new River.OneMoreAddIn.UI.MoreLabel();
			this.alphaRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.numDemoBox = new River.OneMoreAddIn.UI.MoreLabel();
			this.numRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.numberingGroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(264, 269);
			this.okButton.Name = "okButton";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(370, 269);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// numberingGroup
			// 
			this.numberingGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.numberingGroup.Controls.Add(this.introLabel);
			this.numberingGroup.Controls.Add(this.cleanBox);
			this.numberingGroup.Controls.Add(this.alphaDemoBox);
			this.numberingGroup.Controls.Add(this.alphaRadio);
			this.numberingGroup.Controls.Add(this.numDemoBox);
			this.numberingGroup.Controls.Add(this.numRadio);
			this.numberingGroup.Location = new System.Drawing.Point(16, 17);
			this.numberingGroup.Name = "numberingGroup";
			this.numberingGroup.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.numberingGroup.Size = new System.Drawing.Size(454, 225);
			this.numberingGroup.TabIndex = 12;
			this.numberingGroup.TabStop = false;
			this.numberingGroup.Text = "Numbering";
			// 
			// introLabel
			// 
			this.introLabel.AutoSize = true;
			this.introLabel.Location = new System.Drawing.Point(21, 38);
			this.introLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(206, 20);
			this.introLabel.TabIndex = 0;
			this.introLabel.Text = "Choose the numbering style";
			// 
			// cleanBox
			// 
			this.cleanBox.Location = new System.Drawing.Point(25, 180);
			this.cleanBox.Name = "cleanBox";
			this.cleanBox.Padding = new System.Windows.Forms.Padding(0, 9, 0, 0);
			this.cleanBox.Size = new System.Drawing.Size(301, 24);
			this.cleanBox.TabIndex = 3;
			this.cleanBox.Text = "Remove/cleanup existing numbering";
			this.cleanBox.UseVisualStyleBackColor = true;
			// 
			// alphaDemoBox
			// 
			this.alphaDemoBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.alphaDemoBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.alphaDemoBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.alphaDemoBox.Location = new System.Drawing.Point(82, 98);
			this.alphaDemoBox.Name = "alphaDemoBox";
			this.alphaDemoBox.ThemedBack = "ControlLightLight";
			this.alphaDemoBox.ThemedFore = "ControlText";
			this.alphaDemoBox.Size = new System.Drawing.Size(128, 67);
			this.alphaDemoBox.TabIndex = 15;
			this.alphaDemoBox.Text = "1.\r\n   a.\r\n      i.";
			// 
			// alphaRadio
			// 
			this.alphaRadio.Checked = true;
			this.alphaRadio.Location = new System.Drawing.Point(56, 69);
			this.alphaRadio.Name = "alphaRadio";
			this.alphaRadio.Size = new System.Drawing.Size(141, 24);
			this.alphaRadio.TabIndex = 1;
			this.alphaRadio.TabStop = true;
			this.alphaRadio.Text = "Alpha-numeric";
			this.alphaRadio.UseVisualStyleBackColor = true;
			// 
			// numDemoBox
			// 
			this.numDemoBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.numDemoBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numDemoBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.numDemoBox.Location = new System.Drawing.Point(290, 98);
			this.numDemoBox.Name = "numDemoBox";
			this.numDemoBox.ThemedBack = "ControlLightLight";
			this.numDemoBox.ThemedFore = "ControlText";
			this.numDemoBox.Size = new System.Drawing.Size(128, 67);
			this.numDemoBox.TabIndex = 16;
			this.numDemoBox.Text = "1.\r\n   1.1.\r\n      1.1.1.";
			// 
			// numRadio
			// 
			this.numRadio.Location = new System.Drawing.Point(261, 69);
			this.numRadio.Name = "numRadio";
			this.numRadio.Size = new System.Drawing.Size(96, 24);
			this.numRadio.TabIndex = 2;
			this.numRadio.Text = "Numeric";
			this.numRadio.UseVisualStyleBackColor = true;
			// 
			// NumberPagesDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(488, 325);
			this.Controls.Add(this.numberingGroup);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "NumberPagesDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Page Numbering";
			this.numberingGroup.ResumeLayout(false);
			this.numberingGroup.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private UI.MoreGroupBox numberingGroup;
		private System.Windows.Forms.Label introLabel;
		private UI.MoreCheckBox cleanBox;
		private UI.MoreLabel alphaDemoBox;
		private UI.MoreRadioButton alphaRadio;
		private UI.MoreLabel numDemoBox;
		private UI.MoreRadioButton numRadio;
	}
}