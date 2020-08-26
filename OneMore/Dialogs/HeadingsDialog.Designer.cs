namespace River.OneMoreAddIn.Dialogs
{
	partial class HeadingsDialog
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
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.numberingGroup = new System.Windows.Forms.GroupBox();
			this.cleanBox = new System.Windows.Forms.CheckBox();
			this.alphaDemoBox = new System.Windows.Forms.TextBox();
			this.alphaRadio = new System.Windows.Forms.RadioButton();
			this.numDemoBox = new System.Windows.Forms.TextBox();
			this.numRadio = new System.Windows.Forms.RadioButton();
			this.numberingBox = new System.Windows.Forms.CheckBox();
			this.indentationsGroup = new System.Windows.Forms.GroupBox();
			this.indentTagBox = new System.Windows.Forms.CheckBox();
			this.indentBox = new System.Windows.Forms.CheckBox();
			this.tagButton = new System.Windows.Forms.Button();
			this.tagLabel = new System.Windows.Forms.Label();
			this.numberingGroup.SuspendLayout();
			this.indentationsGroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(175, 281);
			this.okButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(67, 25);
			this.okButton.TabIndex = 9;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(245, 281);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(67, 25);
			this.cancelButton.TabIndex = 8;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// numberingGroup
			// 
			this.numberingGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.numberingGroup.Controls.Add(this.cleanBox);
			this.numberingGroup.Controls.Add(this.alphaDemoBox);
			this.numberingGroup.Controls.Add(this.alphaRadio);
			this.numberingGroup.Controls.Add(this.numDemoBox);
			this.numberingGroup.Controls.Add(this.numRadio);
			this.numberingGroup.Controls.Add(this.numberingBox);
			this.numberingGroup.Location = new System.Drawing.Point(9, 12);
			this.numberingGroup.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.numberingGroup.Name = "numberingGroup";
			this.numberingGroup.Size = new System.Drawing.Size(303, 146);
			this.numberingGroup.TabIndex = 11;
			this.numberingGroup.TabStop = false;
			this.numberingGroup.Text = "Numbering";
			// 
			// cleanBox
			// 
			this.cleanBox.AutoSize = true;
			this.cleanBox.Location = new System.Drawing.Point(17, 112);
			this.cleanBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.cleanBox.Name = "cleanBox";
			this.cleanBox.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
			this.cleanBox.Size = new System.Drawing.Size(199, 23);
			this.cleanBox.TabIndex = 14;
			this.cleanBox.Text = "Remove/cleanup existing numbering";
			this.cleanBox.UseVisualStyleBackColor = true;
			// 
			// alphaDemoBox
			// 
			this.alphaDemoBox.BackColor = System.Drawing.SystemColors.Window;
			this.alphaDemoBox.Enabled = false;
			this.alphaDemoBox.Location = new System.Drawing.Point(55, 64);
			this.alphaDemoBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.alphaDemoBox.Multiline = true;
			this.alphaDemoBox.Name = "alphaDemoBox";
			this.alphaDemoBox.ReadOnly = true;
			this.alphaDemoBox.Size = new System.Drawing.Size(87, 45);
			this.alphaDemoBox.TabIndex = 12;
			this.alphaDemoBox.TabStop = false;
			this.alphaDemoBox.Text = "1.\r\n   a.\r\n      i.";
			// 
			// alphaRadio
			// 
			this.alphaRadio.AutoSize = true;
			this.alphaRadio.Enabled = false;
			this.alphaRadio.Location = new System.Drawing.Point(37, 45);
			this.alphaRadio.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.alphaRadio.Name = "alphaRadio";
			this.alphaRadio.Size = new System.Drawing.Size(92, 17);
			this.alphaRadio.TabIndex = 0;
			this.alphaRadio.TabStop = true;
			this.alphaRadio.Text = "Alpha-numeric";
			this.alphaRadio.UseVisualStyleBackColor = true;
			// 
			// numDemoBox
			// 
			this.numDemoBox.BackColor = System.Drawing.SystemColors.Window;
			this.numDemoBox.Enabled = false;
			this.numDemoBox.Location = new System.Drawing.Point(193, 64);
			this.numDemoBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.numDemoBox.Multiline = true;
			this.numDemoBox.Name = "numDemoBox";
			this.numDemoBox.ReadOnly = true;
			this.numDemoBox.Size = new System.Drawing.Size(87, 45);
			this.numDemoBox.TabIndex = 13;
			this.numDemoBox.TabStop = false;
			this.numDemoBox.Text = "1.\r\n   1.1.\r\n      1.1.1.";
			// 
			// numRadio
			// 
			this.numRadio.AutoSize = true;
			this.numRadio.Enabled = false;
			this.numRadio.Location = new System.Drawing.Point(174, 45);
			this.numRadio.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.numRadio.Name = "numRadio";
			this.numRadio.Size = new System.Drawing.Size(64, 17);
			this.numRadio.TabIndex = 1;
			this.numRadio.TabStop = true;
			this.numRadio.Text = "Numeric";
			this.numRadio.UseVisualStyleBackColor = true;
			// 
			// numberingBox
			// 
			this.numberingBox.AutoSize = true;
			this.numberingBox.Location = new System.Drawing.Point(17, 22);
			this.numberingBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.numberingBox.Name = "numberingBox";
			this.numberingBox.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.numberingBox.Size = new System.Drawing.Size(99, 20);
			this.numberingBox.TabIndex = 10;
			this.numberingBox.Text = "Add Numbering";
			this.numberingBox.UseVisualStyleBackColor = true;
			this.numberingBox.CheckedChanged += new System.EventHandler(this.numberedBox_CheckedChanged);
			// 
			// indentationsGroup
			// 
			this.indentationsGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.indentationsGroup.Controls.Add(this.tagLabel);
			this.indentationsGroup.Controls.Add(this.tagButton);
			this.indentationsGroup.Controls.Add(this.indentTagBox);
			this.indentationsGroup.Controls.Add(this.indentBox);
			this.indentationsGroup.Location = new System.Drawing.Point(9, 161);
			this.indentationsGroup.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.indentationsGroup.Name = "indentationsGroup";
			this.indentationsGroup.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.indentationsGroup.Size = new System.Drawing.Size(303, 104);
			this.indentationsGroup.TabIndex = 12;
			this.indentationsGroup.TabStop = false;
			this.indentationsGroup.Text = "Indentations";
			// 
			// indentTagBox
			// 
			this.indentTagBox.AutoSize = true;
			this.indentTagBox.Enabled = false;
			this.indentTagBox.Location = new System.Drawing.Point(17, 48);
			this.indentTagBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.indentTagBox.Name = "indentTagBox";
			this.indentTagBox.Size = new System.Drawing.Size(160, 17);
			this.indentTagBox.TabIndex = 16;
			this.indentTagBox.Text = "Indent only tagged headings";
			this.indentTagBox.UseVisualStyleBackColor = true;
			// 
			// indentBox
			// 
			this.indentBox.AutoSize = true;
			this.indentBox.Location = new System.Drawing.Point(17, 16);
			this.indentBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.indentBox.Name = "indentBox";
			this.indentBox.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
			this.indentBox.Size = new System.Drawing.Size(167, 23);
			this.indentBox.TabIndex = 15;
			this.indentBox.Text = "Indent content below heading";
			this.indentBox.UseVisualStyleBackColor = true;
			// 
			// tagButton
			// 
			this.tagButton.Enabled = false;
			this.tagButton.Location = new System.Drawing.Point(69, 70);
			this.tagButton.Name = "tagButton";
			this.tagButton.Size = new System.Drawing.Size(40, 23);
			this.tagButton.TabIndex = 17;
			this.tagButton.Text = "?";
			this.tagButton.UseVisualStyleBackColor = true;
			// 
			// tagLabel
			// 
			this.tagLabel.AutoSize = true;
			this.tagLabel.Location = new System.Drawing.Point(34, 75);
			this.tagLabel.Name = "tagLabel";
			this.tagLabel.Size = new System.Drawing.Size(29, 13);
			this.tagLabel.TabIndex = 18;
			this.tagLabel.Text = "Tag:";
			// 
			// HeadingsDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(321, 314);
			this.Controls.Add(this.indentationsGroup);
			this.Controls.Add(this.numberingGroup);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "HeadingsDialog";
			this.Padding = new System.Windows.Forms.Padding(7, 10, 7, 6);
			this.ShowInTaskbar = false;
			this.Text = "Headings";
			this.numberingGroup.ResumeLayout(false);
			this.numberingGroup.PerformLayout();
			this.indentationsGroup.ResumeLayout(false);
			this.indentationsGroup.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.GroupBox numberingGroup;
		private System.Windows.Forms.TextBox numDemoBox;
		private System.Windows.Forms.CheckBox numberingBox;
		private System.Windows.Forms.TextBox alphaDemoBox;
		private System.Windows.Forms.RadioButton numRadio;
		private System.Windows.Forms.RadioButton alphaRadio;
		private System.Windows.Forms.CheckBox cleanBox;
		private System.Windows.Forms.GroupBox indentationsGroup;
		private System.Windows.Forms.CheckBox indentTagBox;
		private System.Windows.Forms.CheckBox indentBox;
		private System.Windows.Forms.Label tagLabel;
		private System.Windows.Forms.Button tagButton;
	}
}