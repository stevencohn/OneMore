namespace River.OneMoreAddIn.Commands
{
	partial class ImportDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportDialog));
			this.fileLabel = new System.Windows.Forms.Label();
			this.pathBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.browseButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.wordGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.wordCreateButton = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.wordAppendButton = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.powerGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.powerCreateButton = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.powerAppendButton = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.powerSectionButton = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.pdfGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.pdfCreateButton = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.pdfAppendButton = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.introLabel = new System.Windows.Forms.Label();
			this.notInstalledLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.errorLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.wordGroup.SuspendLayout();
			this.powerGroup.SuspendLayout();
			this.pdfGroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// fileLabel
			// 
			this.fileLabel.AutoSize = true;
			this.fileLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.fileLabel.Location = new System.Drawing.Point(18, 57);
			this.fileLabel.Name = "fileLabel";
			this.fileLabel.Size = new System.Drawing.Size(38, 20);
			this.fileLabel.TabIndex = 0;
			this.fileLabel.Text = "File:";
			// 
			// pathBox
			// 
			this.pathBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pathBox.Location = new System.Drawing.Point(62, 54);
			this.pathBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 15);
			this.pathBox.Name = "pathBox";
			this.pathBox.Size = new System.Drawing.Size(449, 26);
			this.pathBox.TabIndex = 1;
			this.pathBox.ThemedBack = null;
			this.pathBox.ThemedFore = null;
			this.pathBox.TextChanged += new System.EventHandler(this.ChangePath);
			// 
			// browseButton
			// 
			this.browseButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.browseButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.browseButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_FileOpen;
			this.browseButton.ImageOver = null;
			this.browseButton.Location = new System.Drawing.Point(517, 54);
			this.browseButton.Name = "browseButton";
			this.browseButton.ShowBorder = false;
			this.browseButton.Size = new System.Drawing.Size(44, 26);
			this.browseButton.StylizeImage = true;
			this.browseButton.TabIndex = 4;
			this.browseButton.ThemedBack = null;
			this.browseButton.ThemedFore = null;
			this.browseButton.UseVisualStyleBackColor = true;
			this.browseButton.Click += new System.EventHandler(this.BrowseFile);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(462, 261);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 6;
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
			this.okButton.Location = new System.Drawing.Point(356, 261);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 7;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// wordGroup
			// 
			this.wordGroup.Controls.Add(this.wordCreateButton);
			this.wordGroup.Controls.Add(this.wordAppendButton);
			this.wordGroup.ForeColor = System.Drawing.SystemColors.ControlText;
			this.wordGroup.Location = new System.Drawing.Point(22, 98);
			this.wordGroup.Name = "wordGroup";
			this.wordGroup.Padding = new System.Windows.Forms.Padding(3, 10, 3, 3);
			this.wordGroup.Size = new System.Drawing.Size(539, 144);
			this.wordGroup.TabIndex = 8;
			this.wordGroup.TabStop = false;
			this.wordGroup.Text = "Word Options";
			// 
			// wordCreateButton
			// 
			this.wordCreateButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.wordCreateButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.wordCreateButton.Location = new System.Drawing.Point(40, 67);
			this.wordCreateButton.Name = "wordCreateButton";
			this.wordCreateButton.Size = new System.Drawing.Size(160, 25);
			this.wordCreateButton.TabIndex = 10;
			this.wordCreateButton.TabStop = true;
			this.wordCreateButton.Text = "Create new page";
			this.wordCreateButton.UseVisualStyleBackColor = true;
			// 
			// wordAppendButton
			// 
			this.wordAppendButton.Checked = true;
			this.wordAppendButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.wordAppendButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.wordAppendButton.Location = new System.Drawing.Point(40, 32);
			this.wordAppendButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
			this.wordAppendButton.Name = "wordAppendButton";
			this.wordAppendButton.Size = new System.Drawing.Size(181, 25);
			this.wordAppendButton.TabIndex = 9;
			this.wordAppendButton.TabStop = true;
			this.wordAppendButton.Text = "Append to this page";
			this.wordAppendButton.UseVisualStyleBackColor = true;
			// 
			// powerGroup
			// 
			this.powerGroup.Controls.Add(this.powerCreateButton);
			this.powerGroup.Controls.Add(this.powerAppendButton);
			this.powerGroup.Controls.Add(this.powerSectionButton);
			this.powerGroup.ForeColor = System.Drawing.SystemColors.ControlText;
			this.powerGroup.Location = new System.Drawing.Point(22, 98);
			this.powerGroup.Name = "powerGroup";
			this.powerGroup.Padding = new System.Windows.Forms.Padding(3, 10, 3, 3);
			this.powerGroup.Size = new System.Drawing.Size(539, 144);
			this.powerGroup.TabIndex = 9;
			this.powerGroup.TabStop = false;
			this.powerGroup.Text = "PowerPoint Options";
			// 
			// powerCreateButton
			// 
			this.powerCreateButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.powerCreateButton.Location = new System.Drawing.Point(40, 67);
			this.powerCreateButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
			this.powerCreateButton.Name = "powerCreateButton";
			this.powerCreateButton.Size = new System.Drawing.Size(160, 25);
			this.powerCreateButton.TabIndex = 1;
			this.powerCreateButton.TabStop = true;
			this.powerCreateButton.Text = "Create new page";
			this.powerCreateButton.UseVisualStyleBackColor = true;
			// 
			// powerAppendButton
			// 
			this.powerAppendButton.Checked = true;
			this.powerAppendButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.powerAppendButton.Location = new System.Drawing.Point(40, 32);
			this.powerAppendButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
			this.powerAppendButton.Name = "powerAppendButton";
			this.powerAppendButton.Size = new System.Drawing.Size(181, 25);
			this.powerAppendButton.TabIndex = 0;
			this.powerAppendButton.TabStop = true;
			this.powerAppendButton.Text = "Append to this page";
			this.powerAppendButton.UseVisualStyleBackColor = true;
			// 
			// powerSectionButton
			// 
			this.powerSectionButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.powerSectionButton.Location = new System.Drawing.Point(40, 101);
			this.powerSectionButton.Name = "powerSectionButton";
			this.powerSectionButton.Size = new System.Drawing.Size(357, 25);
			this.powerSectionButton.TabIndex = 2;
			this.powerSectionButton.TabStop = true;
			this.powerSectionButton.Text = "Create a new section with a page each slide";
			this.powerSectionButton.UseVisualStyleBackColor = true;
			// 
			// pdfGroup
			// 
			this.pdfGroup.Controls.Add(this.pdfCreateButton);
			this.pdfGroup.Controls.Add(this.pdfAppendButton);
			this.pdfGroup.Location = new System.Drawing.Point(22, 98);
			this.pdfGroup.Name = "pdfGroup";
			this.pdfGroup.Padding = new System.Windows.Forms.Padding(3, 10, 3, 3);
			this.pdfGroup.Size = new System.Drawing.Size(539, 144);
			this.pdfGroup.TabIndex = 8;
			this.pdfGroup.TabStop = false;
			this.pdfGroup.Text = "PDF Options";
			// 
			// pdfCreateButton
			// 
			this.pdfCreateButton.Checked = true;
			this.pdfCreateButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pdfCreateButton.Location = new System.Drawing.Point(40, 67);
			this.pdfCreateButton.Name = "pdfCreateButton";
			this.pdfCreateButton.Size = new System.Drawing.Size(160, 25);
			this.pdfCreateButton.TabIndex = 10;
			this.pdfCreateButton.TabStop = true;
			this.pdfCreateButton.Text = "Create new page";
			this.pdfCreateButton.UseVisualStyleBackColor = true;
			// 
			// pdfAppendButton
			// 
			this.pdfAppendButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pdfAppendButton.Location = new System.Drawing.Point(40, 32);
			this.pdfAppendButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
			this.pdfAppendButton.Name = "pdfAppendButton";
			this.pdfAppendButton.Size = new System.Drawing.Size(181, 25);
			this.pdfAppendButton.TabIndex = 9;
			this.pdfAppendButton.TabStop = true;
			this.pdfAppendButton.Text = "Append to this page";
			this.pdfAppendButton.UseVisualStyleBackColor = true;
			// 
			// introLabel
			// 
			this.introLabel.AutoSize = true;
			this.introLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.introLabel.Location = new System.Drawing.Point(18, 15);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(433, 20);
			this.introLabel.TabIndex = 9;
			this.introLabel.Text = "Select a Word, PowerPoint, OneNote, Markdown, or XML file";
			// 
			// notInstalledLabel
			// 
			this.notInstalledLabel.AutoSize = true;
			this.notInstalledLabel.ForeColor = System.Drawing.Color.Maroon;
			this.notInstalledLabel.Location = new System.Drawing.Point(27, 113);
			this.notInstalledLabel.Name = "notInstalledLabel";
			this.notInstalledLabel.Size = new System.Drawing.Size(306, 20);
			this.notInstalledLabel.TabIndex = 10;
			this.notInstalledLabel.Text = "The required Office product is not installed";
			this.notInstalledLabel.ThemedBack = null;
			this.notInstalledLabel.ThemedFore = "ErrorText";
			this.notInstalledLabel.Visible = false;
			// 
			// errorLabel
			// 
			this.errorLabel.AutoSize = true;
			this.errorLabel.ForeColor = System.Drawing.Color.Maroon;
			this.errorLabel.Location = new System.Drawing.Point(27, 270);
			this.errorLabel.Name = "errorLabel";
			this.errorLabel.Size = new System.Drawing.Size(114, 20);
			this.errorLabel.TabIndex = 11;
			this.errorLabel.Text = "Path not found";
			this.errorLabel.ThemedBack = null;
			this.errorLabel.ThemedFore = "ErrorText";
			this.errorLabel.Visible = false;
			// 
			// ImportDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(579, 317);
			this.Controls.Add(this.errorLabel);
			this.Controls.Add(this.notInstalledLabel);
			this.Controls.Add(this.introLabel);
			this.Controls.Add(this.wordGroup);
			this.Controls.Add(this.powerGroup);
			this.Controls.Add(this.pdfGroup);
			this.Controls.Add(this.browseButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.pathBox);
			this.Controls.Add(this.fileLabel);
			this.Controls.Add(this.cancelButton);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ImportDialog";
			this.Padding = new System.Windows.Forms.Padding(15);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Import";
			this.wordGroup.ResumeLayout(false);
			this.powerGroup.ResumeLayout(false);
			this.pdfGroup.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label fileLabel;
		private UI.MoreTextBox pathBox;
		private UI.MoreButton browseButton;
		private UI.MoreButton cancelButton;
		private UI.MoreButton okButton;
		private UI.MoreGroupBox wordGroup;
		private UI.MoreRadioButton wordCreateButton;
		private UI.MoreRadioButton wordAppendButton;
		private UI.MoreGroupBox powerGroup;
		private UI.MoreRadioButton powerCreateButton;
		private UI.MoreRadioButton powerAppendButton;
		private UI.MoreRadioButton powerSectionButton;
		private UI.MoreGroupBox pdfGroup;
		private UI.MoreRadioButton pdfCreateButton;
		private UI.MoreRadioButton pdfAppendButton;
		private System.Windows.Forms.Label introLabel;
		private UI.MoreLabel notInstalledLabel;
		private UI.MoreLabel errorLabel;
	}
}