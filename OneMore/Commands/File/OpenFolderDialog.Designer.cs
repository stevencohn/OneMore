namespace River.OneMoreAddIn.Commands
{
	partial class OpenFolderDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OpenFolderDialog));
			this.folderLabel = new System.Windows.Forms.Label();
			this.pathBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.browseButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.editBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.SuspendLayout();
			// 
			// folderLabel
			// 
			this.folderLabel.AutoSize = true;
			this.folderLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.folderLabel.Location = new System.Drawing.Point(14, 45);
			this.folderLabel.Name = "folderLabel";
			this.folderLabel.Size = new System.Drawing.Size(54, 20);
			this.folderLabel.TabIndex = 0;
			this.folderLabel.Text = "Folder";
			// 
			// pathBox
			// 
			this.pathBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pathBox.Location = new System.Drawing.Point(105, 42);
			this.pathBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 9);
			this.pathBox.Name = "pathBox";
			this.pathBox.ProcessEnterKey = false;
			this.pathBox.Size = new System.Drawing.Size(495, 26);
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
			this.browseButton.Location = new System.Drawing.Point(606, 42);
			this.browseButton.Name = "browseButton";
			this.browseButton.ShowBorder = false;
			this.browseButton.Size = new System.Drawing.Size(44, 34);
			this.browseButton.StylizeImage = true;
			this.browseButton.TabIndex = 4;
			this.browseButton.ThemedBack = null;
			this.browseButton.ThemedFore = null;
			this.browseButton.UseVisualStyleBackColor = true;
			this.browseButton.Click += new System.EventHandler(this.BrowseFolders);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(575, 160);
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
			this.okButton.Location = new System.Drawing.Point(469, 160);
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
			// editBox
			// 
			this.editBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.editBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.editBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.editBox.Location = new System.Drawing.Point(105, 92);
			this.editBox.Name = "editBox";
			this.editBox.Size = new System.Drawing.Size(522, 25);
			this.editBox.StylizeImage = false;
			this.editBox.TabIndex = 8;
			this.editBox.Text = "Remove timestamps from section names, e.g. (On 4-12-2025)";
			this.editBox.ThemedBack = null;
			this.editBox.ThemedFore = null;
			this.editBox.UseVisualStyleBackColor = false;
			// 
			// OpenFolderDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(693, 217);
			this.Controls.Add(this.editBox);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.browseButton);
			this.Controls.Add(this.pathBox);
			this.Controls.Add(this.folderLabel);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "OpenFolderDialog";
			this.Padding = new System.Windows.Forms.Padding(15);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Open Folder as Notebook";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label folderLabel;
		private UI.MoreTextBox pathBox;
		private UI.MoreButton browseButton;
		private UI.MoreButton cancelButton;
		private UI.MoreButton okButton;
		private UI.MoreCheckBox editBox;
	}
}