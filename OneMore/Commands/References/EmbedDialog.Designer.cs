namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;

	partial class EmbedDialog
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
			this.sourceLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.sourceNameLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.targetLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.targetNameLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.beginTagLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.beginTagBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.endTagLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.endTagBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.formatLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.formattedRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.plaintextRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.stylePanel = new System.Windows.Forms.Panel();
			this.styleLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.styleBox = new River.OneMoreAddIn.UI.MoreComboBox();
			this.indentCheck = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.noteLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.stylePanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// sourceLabel
			// 
			this.sourceLabel.AutoSize = true;
			this.sourceLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.sourceLabel.Location = new System.Drawing.Point(15, 23);
			this.sourceLabel.Name = "sourceLabel";
			this.sourceLabel.Size = new System.Drawing.Size(60, 20);
			this.sourceLabel.TabIndex = 0;
			this.sourceLabel.Text = "Source";
			this.sourceLabel.ThemedBack = null;
			this.sourceLabel.ThemedFore = null;
			// 
			// sourceNameLabel
			// 
			this.sourceNameLabel.AutoEllipsis = true;
			this.sourceNameLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.sourceNameLabel.Location = new System.Drawing.Point(130, 23);
			this.sourceNameLabel.Name = "sourceNameLabel";
			this.sourceNameLabel.Size = new System.Drawing.Size(512, 20);
			this.sourceNameLabel.TabIndex = 1;
			this.sourceNameLabel.ThemedBack = null;
			this.sourceNameLabel.ThemedFore = null;
			// 
			// targetLabel
			// 
			this.targetLabel.AutoSize = true;
			this.targetLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.targetLabel.Location = new System.Drawing.Point(15, 50);
			this.targetLabel.Name = "targetLabel";
			this.targetLabel.Size = new System.Drawing.Size(55, 20);
			this.targetLabel.TabIndex = 2;
			this.targetLabel.Text = "Target";
			this.targetLabel.ThemedBack = null;
			this.targetLabel.ThemedFore = null;
			// 
			// targetNameLabel
			// 
			this.targetNameLabel.AutoEllipsis = true;
			this.targetNameLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.targetNameLabel.Location = new System.Drawing.Point(130, 50);
			this.targetNameLabel.Name = "targetNameLabel";
			this.targetNameLabel.Size = new System.Drawing.Size(512, 20);
			this.targetNameLabel.TabIndex = 3;
			this.targetNameLabel.ThemedBack = null;
			this.targetNameLabel.ThemedFore = null;
			// 
			// beginTagLabel
			// 
			this.beginTagLabel.AutoSize = true;
			this.beginTagLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.beginTagLabel.Location = new System.Drawing.Point(15, 90);
			this.beginTagLabel.Name = "beginTagLabel";
			this.beginTagLabel.Size = new System.Drawing.Size(77, 20);
			this.beginTagLabel.TabIndex = 4;
			this.beginTagLabel.Text = "Begin tag";
			this.beginTagLabel.ThemedBack = null;
			this.beginTagLabel.ThemedFore = null;
			// 
			// beginTagBox
			// 
			this.beginTagBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.beginTagBox.Location = new System.Drawing.Point(130, 87);
			this.beginTagBox.Name = "beginTagBox";
			this.beginTagBox.ProcessEnterKey = false;
			this.beginTagBox.Size = new System.Drawing.Size(400, 26);
			this.beginTagBox.TabIndex = 5;
			this.beginTagBox.ThemedBack = null;
			this.beginTagBox.ThemedFore = null;
			this.beginTagBox.TextChanged += new System.EventHandler(this.SetNote);
			// 
			// endTagLabel
			// 
			this.endTagLabel.AutoSize = true;
			this.endTagLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.endTagLabel.Location = new System.Drawing.Point(15, 125);
			this.endTagLabel.Name = "endTagLabel";
			this.endTagLabel.Size = new System.Drawing.Size(65, 20);
			this.endTagLabel.TabIndex = 6;
			this.endTagLabel.Text = "End tag";
			this.endTagLabel.ThemedBack = null;
			this.endTagLabel.ThemedFore = null;
			// 
			// endTagBox
			// 
			this.endTagBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.endTagBox.Location = new System.Drawing.Point(130, 122);
			this.endTagBox.Name = "endTagBox";
			this.endTagBox.ProcessEnterKey = false;
			this.endTagBox.Size = new System.Drawing.Size(400, 26);
			this.endTagBox.TabIndex = 7;
			this.endTagBox.ThemedBack = null;
			this.endTagBox.ThemedFore = null;
			this.endTagBox.TextChanged += new System.EventHandler(this.SetNote);
			// 
			// formatLabel
			// 
			this.formatLabel.AutoSize = true;
			this.formatLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.formatLabel.Location = new System.Drawing.Point(15, 256);
			this.formatLabel.Name = "formatLabel";
			this.formatLabel.Size = new System.Drawing.Size(60, 20);
			this.formatLabel.TabIndex = 8;
			this.formatLabel.Text = "Format";
			this.formatLabel.ThemedBack = null;
			this.formatLabel.ThemedFore = null;
			// 
			// formattedRadio
			// 
			this.formattedRadio.Checked = true;
			this.formattedRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.formattedRadio.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.formattedRadio.Location = new System.Drawing.Point(130, 254);
			this.formattedRadio.Name = "formattedRadio";
			this.formattedRadio.Size = new System.Drawing.Size(230, 25);
			this.formattedRadio.TabIndex = 9;
			this.formattedRadio.TabStop = true;
			this.formattedRadio.Text = "Formatted content";
			this.formattedRadio.UseVisualStyleBackColor = true;
			this.formattedRadio.CheckedChanged += new System.EventHandler(this.ToggleStyle);
			// 
			// plaintextRadio
			// 
			this.plaintextRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.plaintextRadio.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.plaintextRadio.Location = new System.Drawing.Point(130, 288);
			this.plaintextRadio.Name = "plaintextRadio";
			this.plaintextRadio.Size = new System.Drawing.Size(150, 25);
			this.plaintextRadio.TabIndex = 10;
			this.plaintextRadio.Text = "Plain text";
			this.plaintextRadio.UseVisualStyleBackColor = true;
			this.plaintextRadio.CheckedChanged += new System.EventHandler(this.ToggleStyle);
			// 
			// stylePanel
			// 
			this.stylePanel.Controls.Add(this.styleLabel);
			this.stylePanel.Controls.Add(this.styleBox);
			this.stylePanel.Location = new System.Drawing.Point(156, 319);
			this.stylePanel.Name = "stylePanel";
			this.stylePanel.Size = new System.Drawing.Size(394, 38);
			this.stylePanel.TabIndex = 11;
			this.stylePanel.Visible = false;
			// 
			// styleLabel
			// 
			this.styleLabel.AutoSize = true;
			this.styleLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.styleLabel.Location = new System.Drawing.Point(3, 11);
			this.styleLabel.Name = "styleLabel";
			this.styleLabel.Size = new System.Drawing.Size(79, 20);
			this.styleLabel.TabIndex = 0;
			this.styleLabel.Text = "Text style:";
			this.styleLabel.ThemedBack = null;
			this.styleLabel.ThemedFore = null;
			// 
			// styleBox
			// 
			this.styleBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.styleBox.FormattingEnabled = true;
			this.styleBox.Items.AddRange(new object[] {
            "Normal",
            "Italic",
            "Gray",
            "Quote",
            "Citation"});
			this.styleBox.Location = new System.Drawing.Point(100, 5);
			this.styleBox.Name = "styleBox";
			this.styleBox.Size = new System.Drawing.Size(200, 27);
			this.styleBox.TabIndex = 1;
			this.styleBox.ThemedBack = null;
			this.styleBox.ThemedFore = null;
			// 
			// indentCheck
			// 
			this.indentCheck.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.indentCheck.Cursor = System.Windows.Forms.Cursors.Hand;
			this.indentCheck.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.indentCheck.Location = new System.Drawing.Point(130, 206);
			this.indentCheck.Name = "indentCheck";
			this.indentCheck.Size = new System.Drawing.Size(200, 25);
			this.indentCheck.StylizeImage = false;
			this.indentCheck.TabIndex = 14;
			this.indentCheck.Text = "Indent content";
			this.indentCheck.ThemedBack = null;
			this.indentCheck.ThemedFore = null;
			this.indentCheck.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(411, 392);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(112, 35);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 15;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(533, 392);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(112, 35);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 16;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// noteLabel
			// 
			this.noteLabel.AutoEllipsis = true;
			this.noteLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.noteLabel.Location = new System.Drawing.Point(133, 162);
			this.noteLabel.Name = "noteLabel";
			this.noteLabel.Size = new System.Drawing.Size(512, 20);
			this.noteLabel.TabIndex = 17;
			this.noteLabel.Text = "Full page";
			this.noteLabel.ThemedBack = null;
			this.noteLabel.ThemedFore = null;
			// 
			// EmbedDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(660, 442);
			this.Controls.Add(this.noteLabel);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.indentCheck);
			this.Controls.Add(this.stylePanel);
			this.Controls.Add(this.plaintextRadio);
			this.Controls.Add(this.formattedRadio);
			this.Controls.Add(this.formatLabel);
			this.Controls.Add(this.endTagBox);
			this.Controls.Add(this.endTagLabel);
			this.Controls.Add(this.beginTagBox);
			this.Controls.Add(this.beginTagLabel);
			this.Controls.Add(this.targetNameLabel);
			this.Controls.Add(this.targetLabel);
			this.Controls.Add(this.sourceNameLabel);
			this.Controls.Add(this.sourceLabel);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "EmbedDialog";
			this.Padding = new System.Windows.Forms.Padding(15);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Embed Options";
			this.stylePanel.ResumeLayout(false);
			this.stylePanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreLabel sourceLabel;
		private UI.MoreLabel sourceNameLabel;
		private UI.MoreLabel targetLabel;
		private UI.MoreLabel targetNameLabel;
		private UI.MoreLabel beginTagLabel;
		private UI.MoreTextBox beginTagBox;
		private UI.MoreLabel endTagLabel;
		private UI.MoreTextBox endTagBox;
		private UI.MoreLabel formatLabel;
		private UI.MoreRadioButton formattedRadio;
		private UI.MoreRadioButton plaintextRadio;
		private System.Windows.Forms.Panel stylePanel;
		private UI.MoreLabel styleLabel;
		private UI.MoreComboBox styleBox;
		private UI.MoreCheckBox indentCheck;
		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private MoreLabel noteLabel;
	}
}
