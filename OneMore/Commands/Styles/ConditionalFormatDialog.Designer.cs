namespace River.OneMoreAddIn.Commands
{
	partial class ConditionalFormatDialog
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
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.patternLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.patternBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.patternStatusLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.builtinRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.customRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.styleLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.styleBox = new River.OneMoreAddIn.UI.MoreComboBox();
			this.hintLabel = new River.OneMoreAddIn.UI.MoreMultilineLabel();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(465, 304);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(90, 36);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 8;
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
			this.okButton.Enabled = false;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(369, 304);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(90, 36);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 7;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// patternLabel
			// 
			this.patternLabel.AutoSize = true;
			this.patternLabel.Location = new System.Drawing.Point(16, 22);
			this.patternLabel.Name = "patternLabel";
			this.patternLabel.Size = new System.Drawing.Size(145, 20);
			this.patternLabel.TabIndex = 0;
			this.patternLabel.Text = "Regular expression";
			this.patternLabel.ThemedBack = null;
			this.patternLabel.ThemedFore = null;
			// 
			// patternBox
			// 
			this.patternBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.patternBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.patternBox.Location = new System.Drawing.Point(186, 18);
			this.patternBox.Name = "patternBox";
			this.patternBox.ProcessEnterKey = false;
			this.patternBox.Size = new System.Drawing.Size(361, 26);
			this.patternBox.TabIndex = 1;
			this.patternBox.ThemedBack = null;
			this.patternBox.ThemedFore = null;
			this.patternBox.TextChanged += new System.EventHandler(this.CheckPattern);
			// 
			// patternStatusLabel
			// 
			this.patternStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.patternStatusLabel.ForeColor = System.Drawing.Color.Brown;
			this.patternStatusLabel.Location = new System.Drawing.Point(186, 51);
			this.patternStatusLabel.Name = "patternStatusLabel";
			this.patternStatusLabel.Size = new System.Drawing.Size(361, 20);
			this.patternStatusLabel.TabIndex = 2;
			this.patternStatusLabel.Text = "Invalid regular expression";
			this.patternStatusLabel.ThemedBack = null;
			this.patternStatusLabel.ThemedFore = "ErrorText";
			// 
			// builtinRadio
			// 
			this.builtinRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.builtinRadio.Location = new System.Drawing.Point(20, 130);
			this.builtinRadio.Name = "builtinRadio";
			this.builtinRadio.Size = new System.Drawing.Size(220, 25);
			this.builtinRadio.TabIndex = 3;
			this.builtinRadio.Text = "Built-in OneNote style";
			this.builtinRadio.UseVisualStyleBackColor = true;
			this.builtinRadio.CheckedChanged += new System.EventHandler(this.ToggleStyleSource);
			// 
			// customRadio
			// 
			this.customRadio.Checked = true;
			this.customRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.customRadio.Location = new System.Drawing.Point(20, 99);
			this.customRadio.Name = "customRadio";
			this.customRadio.Size = new System.Drawing.Size(220, 25);
			this.customRadio.TabIndex = 4;
			this.customRadio.TabStop = true;
			this.customRadio.Text = "Custom OneMore style";
			this.customRadio.UseVisualStyleBackColor = true;
			this.customRadio.CheckedChanged += new System.EventHandler(this.ToggleStyleSource);
			// 
			// styleLabel
			// 
			this.styleLabel.AutoSize = true;
			this.styleLabel.Location = new System.Drawing.Point(16, 176);
			this.styleLabel.Name = "styleLabel";
			this.styleLabel.Size = new System.Drawing.Size(44, 20);
			this.styleLabel.TabIndex = 5;
			this.styleLabel.Text = "Style";
			this.styleLabel.ThemedBack = null;
			this.styleLabel.ThemedFore = null;
			// 
			// styleBox
			// 
			this.styleBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.styleBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.styleBox.FormattingEnabled = true;
			this.styleBox.Location = new System.Drawing.Point(186, 173);
			this.styleBox.Name = "styleBox";
			this.styleBox.Size = new System.Drawing.Size(361, 27);
			this.styleBox.TabIndex = 6;
			this.styleBox.ThemedBack = null;
			this.styleBox.ThemedFore = null;
			this.styleBox.SelectedIndexChanged += new System.EventHandler(this.ChangeStyleSelection);
			// 
			// hintLabel
			// 
			this.hintLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.hintLabel.Location = new System.Drawing.Point(20, 225);
			this.hintLabel.Name = "hintLabel";
			this.hintLabel.Size = new System.Drawing.Size(531, 73);
			this.hintLabel.TabIndex = 9;
			this.hintLabel.Text = "Paragraph styles apply only their font and color formatting to matched text; the " +
    "containing paragraph is not converted to that style.";
			this.hintLabel.ThemedBack = null;
			this.hintLabel.ThemedFore = "HintText";
			// 
			// ConditionalFormatDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(563, 352);
			this.Controls.Add(this.hintLabel);
			this.Controls.Add(this.styleBox);
			this.Controls.Add(this.styleLabel);
			this.Controls.Add(this.customRadio);
			this.Controls.Add(this.builtinRadio);
			this.Controls.Add(this.patternStatusLabel);
			this.Controls.Add(this.patternBox);
			this.Controls.Add(this.patternLabel);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ConditionalFormatDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Apply Conditional Formatting";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreButton cancelButton;
		private UI.MoreButton okButton;
		private UI.MoreLabel patternLabel;
		private UI.MoreTextBox patternBox;
		private UI.MoreLabel patternStatusLabel;
		private UI.MoreRadioButton builtinRadio;
		private UI.MoreRadioButton customRadio;
		private UI.MoreLabel styleLabel;
		private UI.MoreComboBox styleBox;
		private UI.MoreMultilineLabel hintLabel;
	}
}
