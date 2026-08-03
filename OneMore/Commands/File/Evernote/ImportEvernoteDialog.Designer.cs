namespace River.OneMoreAddIn.Commands
{
	partial class ImportEvernoteDialog
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
			this.introLabel = new System.Windows.Forms.Label();
			this.fileLabel = new System.Windows.Forms.Label();
			this.pathBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.browseButton = new River.OneMoreAddIn.UI.MoreButton();
			this.folderButton = new River.OneMoreAddIn.UI.MoreButton();
			this.errorLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.includeSubfoldersCheckBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.abortCheckBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.SuspendLayout();
			// 
			// introLabel
			// 
			this.introLabel.AutoSize = true;
			this.introLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.introLabel.Location = new System.Drawing.Point(14, 14);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(334, 20);
			this.introLabel.TabIndex = 0;
			this.introLabel.Text = "Select one or more Evernote (.enex) export files, or a folder, to import";
			// 
			// fileLabel
			// 
			this.fileLabel.AutoSize = true;
			this.fileLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.fileLabel.Location = new System.Drawing.Point(14, 55);
			this.fileLabel.Name = "fileLabel";
			this.fileLabel.Size = new System.Drawing.Size(34, 20);
			this.fileLabel.TabIndex = 1;
			this.fileLabel.Text = "File";
			// 
			// pathBox
			// 
			this.pathBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pathBox.Location = new System.Drawing.Point(105, 52);
			this.pathBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 9);
			this.pathBox.Name = "pathBox";
			this.pathBox.ProcessEnterKey = false;
			this.pathBox.Size = new System.Drawing.Size(495, 26);
			this.pathBox.TabIndex = 2;
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
			this.browseButton.Location = new System.Drawing.Point(606, 52);
			this.browseButton.Name = "browseButton";
			this.browseButton.ShowBorder = false;
			this.browseButton.Size = new System.Drawing.Size(44, 34);
			this.browseButton.StylizeImage = true;
			this.browseButton.TabIndex = 3;
			this.browseButton.ThemedBack = null;
			this.browseButton.ThemedFore = null;
			this.browseButton.UseVisualStyleBackColor = true;
			this.browseButton.Click += new System.EventHandler(this.BrowseFile);
			//
			// folderButton
			//
			this.folderButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.folderButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.folderButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_FolderClose;
			this.folderButton.ImageOver = null;
			this.folderButton.Location = new System.Drawing.Point(656, 52);
			this.folderButton.Name = "folderButton";
			this.folderButton.ShowBorder = false;
			this.folderButton.Size = new System.Drawing.Size(44, 34);
			this.folderButton.StylizeImage = true;
			this.folderButton.TabIndex = 4;
			this.folderButton.ThemedBack = null;
			this.folderButton.ThemedFore = null;
			this.folderButton.UseVisualStyleBackColor = true;
			this.folderButton.Click += new System.EventHandler(this.BrowseFolder);
			//
			// errorLabel
			//
			this.errorLabel.AutoSize = true;
			this.errorLabel.ForeColor = System.Drawing.Color.Maroon;
			this.errorLabel.Location = new System.Drawing.Point(105, 82);
			this.errorLabel.Name = "errorLabel";
			this.errorLabel.Size = new System.Drawing.Size(114, 20);
			this.errorLabel.TabIndex = 5;
			this.errorLabel.Text = "Path not found";
			this.errorLabel.ThemedBack = null;
			this.errorLabel.ThemedFore = "ErrorText";
			this.errorLabel.Visible = false;
			//
			// includeSubfoldersCheckBox
			//
			this.includeSubfoldersCheckBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.includeSubfoldersCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.includeSubfoldersCheckBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.includeSubfoldersCheckBox.Location = new System.Drawing.Point(105, 112);
			this.includeSubfoldersCheckBox.Name = "includeSubfoldersCheckBox";
			this.includeSubfoldersCheckBox.Size = new System.Drawing.Size(583, 25);
			this.includeSubfoldersCheckBox.StylizeImage = false;
			this.includeSubfoldersCheckBox.TabIndex = 6;
			this.includeSubfoldersCheckBox.Text = "Include subfolders when a folder is selected";
			this.includeSubfoldersCheckBox.ThemedBack = null;
			this.includeSubfoldersCheckBox.ThemedFore = null;
			this.includeSubfoldersCheckBox.UseVisualStyleBackColor = false;
			//
			// abortCheckBox
			//
			this.abortCheckBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.abortCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.abortCheckBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.abortCheckBox.Location = new System.Drawing.Point(105, 142);
			this.abortCheckBox.Name = "abortCheckBox";
			this.abortCheckBox.Size = new System.Drawing.Size(583, 25);
			this.abortCheckBox.StylizeImage = false;
			this.abortCheckBox.TabIndex = 7;
			this.abortCheckBox.Text = "Skip the whole note instead of inserting a placeholder for encrypted content";
			this.abortCheckBox.ThemedBack = null;
			this.abortCheckBox.ThemedFore = null;
			this.abortCheckBox.UseVisualStyleBackColor = false;
			//
			// cancelButton
			//
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(685, 210);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 9;
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
			this.okButton.Location = new System.Drawing.Point(579, 210);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 8;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// ImportEvernoteDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(803, 268);
			this.Controls.Add(this.abortCheckBox);
			this.Controls.Add(this.includeSubfoldersCheckBox);
			this.Controls.Add(this.errorLabel);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.folderButton);
			this.Controls.Add(this.browseButton);
			this.Controls.Add(this.pathBox);
			this.Controls.Add(this.fileLabel);
			this.Controls.Add(this.introLabel);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ImportEvernoteDialog";
			this.Padding = new System.Windows.Forms.Padding(15);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Import Evernote";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label introLabel;
		private System.Windows.Forms.Label fileLabel;
		private UI.MoreTextBox pathBox;
		private UI.MoreButton browseButton;
		private UI.MoreButton folderButton;
		private UI.MoreLabel errorLabel;
		private UI.MoreCheckBox includeSubfoldersCheckBox;
		private UI.MoreCheckBox abortCheckBox;
		private UI.MoreButton cancelButton;
		private UI.MoreButton okButton;
	}
}
