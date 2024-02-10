namespace River.OneMoreAddIn.Commands
{
	partial class PronunciateDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PronunciateDialog));
			this.wordLabel = new System.Windows.Forms.Label();
			this.wordBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.languageLabel = new System.Windows.Forms.Label();
			this.languagesBox = new System.Windows.Forms.ComboBox();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.SuspendLayout();
			// 
			// wordLabel
			// 
			this.wordLabel.AutoSize = true;
			this.wordLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.wordLabel.Location = new System.Drawing.Point(22, 35);
			this.wordLabel.Name = "wordLabel";
			this.wordLabel.Size = new System.Drawing.Size(47, 20);
			this.wordLabel.TabIndex = 0;
			this.wordLabel.Text = "Word";
			// 
			// wordBox
			// 
			this.wordBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.wordBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.wordBox.Location = new System.Drawing.Point(110, 32);
			this.wordBox.Name = "wordBox";
			this.wordBox.Size = new System.Drawing.Size(379, 26);
			this.wordBox.TabIndex = 2;
			this.wordBox.ThemedBack = null;
			this.wordBox.ThemedFore = null;
			// 
			// languageLabel
			// 
			this.languageLabel.AutoSize = true;
			this.languageLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.languageLabel.Location = new System.Drawing.Point(22, 75);
			this.languageLabel.Name = "languageLabel";
			this.languageLabel.Size = new System.Drawing.Size(81, 20);
			this.languageLabel.TabIndex = 2;
			this.languageLabel.Text = "Language";
			// 
			// languagesBox
			// 
			this.languagesBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.languagesBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.languagesBox.FormattingEnabled = true;
			this.languagesBox.Items.AddRange(new object[] {
            "English",
            "Hindi",
            "Spanish",
            "French",
            "Japanese",
            "Russian",
            "German",
            "Italian",
            "Korean",
            "Brazilian Portuguese",
            "Chinese (Simplified)",
            "Arabic",
            "Turkish"});
			this.languagesBox.Location = new System.Drawing.Point(110, 72);
			this.languagesBox.MaxDropDownItems = 13;
			this.languagesBox.Name = "languagesBox";
			this.languagesBox.Size = new System.Drawing.Size(379, 28);
			this.languagesBox.TabIndex = 3;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(389, 141);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.TabIndex = 1;
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
			this.okButton.Location = new System.Drawing.Point(283, 141);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// PronunciateDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(513, 202);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.languagesBox);
			this.Controls.Add(this.languageLabel);
			this.Controls.Add(this.wordBox);
			this.Controls.Add(this.wordLabel);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "PronunciateDialog";
			this.Padding = new System.Windows.Forms.Padding(20, 29, 20, 20);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Phonetics";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label wordLabel;
		private UI.MoreTextBox wordBox;
		private System.Windows.Forms.Label languageLabel;
		private System.Windows.Forms.ComboBox languagesBox;
		private UI.MoreButton cancelButton;
		private UI.MoreButton okButton;
	}
}