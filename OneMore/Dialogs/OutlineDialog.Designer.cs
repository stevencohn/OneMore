namespace River.OneMoreAddIn.Dialogs
{
	partial class OutlineDialog
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
			this.tagLabel = new System.Windows.Forms.Label();
			this.tagButton = new System.Windows.Forms.Button();
			this.indentTagBox = new System.Windows.Forms.CheckBox();
			this.indentBox = new System.Windows.Forms.CheckBox();
			this.removeTagsBox = new System.Windows.Forms.CheckBox();
			this.numberingGroup.SuspendLayout();
			this.indentationsGroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Enabled = false;
			this.okButton.Location = new System.Drawing.Point(262, 432);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.TabIndex = 9;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(368, 432);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.TabIndex = 8;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
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
			this.numberingGroup.Location = new System.Drawing.Point(14, 18);
			this.numberingGroup.Name = "numberingGroup";
			this.numberingGroup.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.numberingGroup.Size = new System.Drawing.Size(454, 225);
			this.numberingGroup.TabIndex = 11;
			this.numberingGroup.TabStop = false;
			this.numberingGroup.Text = "Numbering";
			// 
			// cleanBox
			// 
			this.cleanBox.AutoSize = true;
			this.cleanBox.Location = new System.Drawing.Point(26, 172);
			this.cleanBox.Name = "cleanBox";
			this.cleanBox.Padding = new System.Windows.Forms.Padding(0, 9, 0, 0);
			this.cleanBox.Size = new System.Drawing.Size(290, 33);
			this.cleanBox.TabIndex = 14;
			this.cleanBox.Text = "Remove/cleanup existing numbering";
			this.cleanBox.UseVisualStyleBackColor = true;
			this.cleanBox.CheckedChanged += new System.EventHandler(this.cleanBox_CheckedChanged);
			// 
			// alphaDemoBox
			// 
			this.alphaDemoBox.BackColor = System.Drawing.SystemColors.Window;
			this.alphaDemoBox.Enabled = false;
			this.alphaDemoBox.Location = new System.Drawing.Point(82, 98);
			this.alphaDemoBox.Multiline = true;
			this.alphaDemoBox.Name = "alphaDemoBox";
			this.alphaDemoBox.ReadOnly = true;
			this.alphaDemoBox.Size = new System.Drawing.Size(128, 67);
			this.alphaDemoBox.TabIndex = 12;
			this.alphaDemoBox.TabStop = false;
			this.alphaDemoBox.Text = "1.\r\n   a.\r\n      i.";
			// 
			// alphaRadio
			// 
			this.alphaRadio.AutoSize = true;
			this.alphaRadio.Checked = true;
			this.alphaRadio.Enabled = false;
			this.alphaRadio.Location = new System.Drawing.Point(56, 69);
			this.alphaRadio.Name = "alphaRadio";
			this.alphaRadio.Size = new System.Drawing.Size(136, 24);
			this.alphaRadio.TabIndex = 0;
			this.alphaRadio.TabStop = true;
			this.alphaRadio.Text = "Alpha-numeric";
			this.alphaRadio.UseVisualStyleBackColor = true;
			// 
			// numDemoBox
			// 
			this.numDemoBox.BackColor = System.Drawing.SystemColors.Window;
			this.numDemoBox.Enabled = false;
			this.numDemoBox.Location = new System.Drawing.Point(290, 98);
			this.numDemoBox.Multiline = true;
			this.numDemoBox.Name = "numDemoBox";
			this.numDemoBox.ReadOnly = true;
			this.numDemoBox.Size = new System.Drawing.Size(128, 67);
			this.numDemoBox.TabIndex = 13;
			this.numDemoBox.TabStop = false;
			this.numDemoBox.Text = "1.\r\n   1.1.\r\n      1.1.1.";
			// 
			// numRadio
			// 
			this.numRadio.AutoSize = true;
			this.numRadio.Enabled = false;
			this.numRadio.Location = new System.Drawing.Point(261, 69);
			this.numRadio.Name = "numRadio";
			this.numRadio.Size = new System.Drawing.Size(92, 24);
			this.numRadio.TabIndex = 1;
			this.numRadio.Text = "Numeric";
			this.numRadio.UseVisualStyleBackColor = true;
			// 
			// numberingBox
			// 
			this.numberingBox.AutoSize = true;
			this.numberingBox.Location = new System.Drawing.Point(26, 34);
			this.numberingBox.Name = "numberingBox";
			this.numberingBox.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
			this.numberingBox.Size = new System.Drawing.Size(145, 29);
			this.numberingBox.TabIndex = 10;
			this.numberingBox.Text = "Add Numbering";
			this.numberingBox.UseVisualStyleBackColor = true;
			this.numberingBox.CheckedChanged += new System.EventHandler(this.numberingBox_CheckedChanged);
			// 
			// indentationsGroup
			// 
			this.indentationsGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.indentationsGroup.Controls.Add(this.removeTagsBox);
			this.indentationsGroup.Controls.Add(this.tagLabel);
			this.indentationsGroup.Controls.Add(this.tagButton);
			this.indentationsGroup.Controls.Add(this.indentTagBox);
			this.indentationsGroup.Controls.Add(this.indentBox);
			this.indentationsGroup.Location = new System.Drawing.Point(14, 248);
			this.indentationsGroup.Name = "indentationsGroup";
			this.indentationsGroup.Size = new System.Drawing.Size(454, 160);
			this.indentationsGroup.TabIndex = 12;
			this.indentationsGroup.TabStop = false;
			this.indentationsGroup.Text = "Indentations";
			// 
			// tagLabel
			// 
			this.tagLabel.AutoSize = true;
			this.tagLabel.Location = new System.Drawing.Point(51, 115);
			this.tagLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.tagLabel.Name = "tagLabel";
			this.tagLabel.Size = new System.Drawing.Size(40, 20);
			this.tagLabel.TabIndex = 18;
			this.tagLabel.Text = "Tag:";
			// 
			// tagButton
			// 
			this.tagButton.BackColor = System.Drawing.SystemColors.Window;
			this.tagButton.Enabled = false;
			this.tagButton.Location = new System.Drawing.Point(104, 108);
			this.tagButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tagButton.Name = "tagButton";
			this.tagButton.Size = new System.Drawing.Size(60, 38);
			this.tagButton.TabIndex = 17;
			this.tagButton.Text = "?";
			this.tagButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.tagButton.UseVisualStyleBackColor = false;
			this.tagButton.Click += new System.EventHandler(this.tagButton_Click);
			// 
			// indentTagBox
			// 
			this.indentTagBox.AutoSize = true;
			this.indentTagBox.Enabled = false;
			this.indentTagBox.Location = new System.Drawing.Point(26, 74);
			this.indentTagBox.Name = "indentTagBox";
			this.indentTagBox.Size = new System.Drawing.Size(281, 24);
			this.indentTagBox.TabIndex = 16;
			this.indentTagBox.Text = "Indent only below tagged headings";
			this.indentTagBox.UseVisualStyleBackColor = true;
			this.indentTagBox.CheckedChanged += new System.EventHandler(this.indentTagBox_CheckedChanged);
			// 
			// indentBox
			// 
			this.indentBox.AutoSize = true;
			this.indentBox.Location = new System.Drawing.Point(26, 25);
			this.indentBox.Name = "indentBox";
			this.indentBox.Padding = new System.Windows.Forms.Padding(0, 9, 0, 0);
			this.indentBox.Size = new System.Drawing.Size(245, 33);
			this.indentBox.TabIndex = 15;
			this.indentBox.Text = "Indent content below heading";
			this.indentBox.UseVisualStyleBackColor = true;
			this.indentBox.CheckedChanged += new System.EventHandler(this.indentBox_CheckedChanged);
			// 
			// removeTagsBox
			// 
			this.removeTagsBox.AutoSize = true;
			this.removeTagsBox.Enabled = false;
			this.removeTagsBox.Location = new System.Drawing.Point(190, 114);
			this.removeTagsBox.Name = "removeTagsBox";
			this.removeTagsBox.Size = new System.Drawing.Size(129, 24);
			this.removeTagsBox.TabIndex = 19;
			this.removeTagsBox.Text = "Remove tags";
			this.removeTagsBox.UseVisualStyleBackColor = true;
			// 
			// OutlineDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(482, 483);
			this.Controls.Add(this.indentationsGroup);
			this.Controls.Add(this.numberingGroup);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "OutlineDialog";
			this.Padding = new System.Windows.Forms.Padding(10, 15, 10, 9);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Outline Formatting";
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
		private System.Windows.Forms.CheckBox removeTagsBox;
	}
}