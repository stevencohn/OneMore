namespace River.OneMoreAddIn.Commands
{
	partial class ExportDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportDialog));
			this.folderLabel = new System.Windows.Forms.Label();
			this.pathBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.formatLabel = new System.Windows.Forms.Label();
			this.formatBox = new System.Windows.Forms.ComboBox();
			this.groupBox = new UI.MoreGroupBox();
			this.embeddedBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.underBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.attachmentsBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.browseButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.groupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// folderLabel
			// 
			this.folderLabel.AutoSize = true;
			this.folderLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.folderLabel.Location = new System.Drawing.Point(14, 45);
			this.folderLabel.Name = "folderLabel";
			this.folderLabel.Size = new System.Drawing.Size(58, 20);
			this.folderLabel.TabIndex = 0;
			this.folderLabel.Text = "Folder:";
			// 
			// pathBox
			// 
			this.pathBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pathBox.Location = new System.Drawing.Point(82, 42);
			this.pathBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 9);
			this.pathBox.Name = "pathBox";
			this.pathBox.Size = new System.Drawing.Size(400, 26);
			this.pathBox.TabIndex = 1;
			this.pathBox.TextChanged += new System.EventHandler(this.ChangePath);
			// 
			// formatLabel
			// 
			this.formatLabel.AutoSize = true;
			this.formatLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.formatLabel.Location = new System.Drawing.Point(14, 83);
			this.formatLabel.Name = "formatLabel";
			this.formatLabel.Size = new System.Drawing.Size(64, 20);
			this.formatLabel.TabIndex = 2;
			this.formatLabel.Text = "Format:";
			// 
			// formatBox
			// 
			this.formatBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.formatBox.DropDownWidth = 250;
			this.formatBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.formatBox.FormattingEnabled = true;
			this.formatBox.Items.AddRange(new object[] {
            "HTML File (*.htm)",
            "PDF File (*.pdf)",
            "Word File (*.docx)",
            "XML File (*.xml)",
            "Markdown File (*.md)",
            "OneNote File (*.one)"});
			this.formatBox.Location = new System.Drawing.Point(82, 80);
			this.formatBox.Name = "formatBox";
			this.formatBox.Size = new System.Drawing.Size(250, 28);
			this.formatBox.TabIndex = 3;
			this.formatBox.SelectedIndexChanged += new System.EventHandler(this.ChangeFormat);
			// 
			// groupBox
			// 
			this.groupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox.Controls.Add(this.embeddedBox);
			this.groupBox.Controls.Add(this.underBox);
			this.groupBox.Controls.Add(this.attachmentsBox);
			this.groupBox.Controls.Add(this.browseButton);
			this.groupBox.Controls.Add(this.pathBox);
			this.groupBox.Controls.Add(this.folderLabel);
			this.groupBox.Controls.Add(this.formatBox);
			this.groupBox.Controls.Add(this.formatLabel);
			this.groupBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.groupBox.Location = new System.Drawing.Point(18, 18);
			this.groupBox.Name = "groupBox";
			this.groupBox.Padding = new System.Windows.Forms.Padding(10, 9, 10, 9);
			this.groupBox.Size = new System.Drawing.Size(634, 236);
			this.groupBox.TabIndex = 5;
			this.groupBox.TabStop = false;
			this.groupBox.Text = "Export 2 Pages";
			// 
			// embeddedBox
			// 
			this.embeddedBox.AutoSize = true;
			this.embeddedBox.Enabled = false;
			this.embeddedBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.embeddedBox.Location = new System.Drawing.Point(82, 154);
			this.embeddedBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.embeddedBox.Name = "embeddedBox";
			this.embeddedBox.Size = new System.Drawing.Size(179, 24);
			this.embeddedBox.TabIndex = 7;
			this.embeddedBox.Text = "Embed attachments";
			this.embeddedBox.UseVisualStyleBackColor = true;
			// 
			// underBox
			// 
			this.underBox.AutoSize = true;
			this.underBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.underBox.Location = new System.Drawing.Point(82, 186);
			this.underBox.Name = "underBox";
			this.underBox.Size = new System.Drawing.Size(380, 24);
			this.underBox.TabIndex = 6;
			this.underBox.Text = "Replace spaces in the filename with underscores";
			this.underBox.UseVisualStyleBackColor = true;
			// 
			// attachmentsBox
			// 
			this.attachmentsBox.AutoSize = true;
			this.attachmentsBox.Enabled = false;
			this.attachmentsBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.attachmentsBox.Location = new System.Drawing.Point(82, 120);
			this.attachmentsBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.attachmentsBox.Name = "attachmentsBox";
			this.attachmentsBox.Size = new System.Drawing.Size(180, 24);
			this.attachmentsBox.TabIndex = 5;
			this.attachmentsBox.Text = "Include attachments";
			this.attachmentsBox.UseVisualStyleBackColor = true;
			this.attachmentsBox.CheckedChanged += new System.EventHandler(this.ChangeIncludeAttachments);
			// 
			// browseButton
			// 
			this.browseButton.Image = ((System.Drawing.Image)(resources.GetObject("browseButton.Image")));
			this.browseButton.ImageOver = null;
			this.browseButton.Location = new System.Drawing.Point(489, 40);
			this.browseButton.Name = "browseButton";
			this.browseButton.ShowBorder = false;
			this.browseButton.Size = new System.Drawing.Size(44, 34);
			this.browseButton.TabIndex = 4;
			this.browseButton.UseVisualStyleBackColor = true;
			this.browseButton.Click += new System.EventHandler(this.BrowseFolders);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(554, 270);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.TabIndex = 6;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Enabled = false;
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(447, 270);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.TabIndex = 7;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// ExportDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(670, 327);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.groupBox);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ExportDialog";
			this.Padding = new System.Windows.Forms.Padding(15);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Export Pages";
			this.groupBox.ResumeLayout(false);
			this.groupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label folderLabel;
		private UI.MoreTextBox pathBox;
		private System.Windows.Forms.Label formatLabel;
		private System.Windows.Forms.ComboBox formatBox;
		private UI.MoreGroupBox groupBox;
		private UI.MoreButton browseButton;
		private UI.MoreButton cancelButton;
		private UI.MoreButton okButton;
		private UI.MoreCheckBox attachmentsBox;
		private UI.MoreCheckBox underBox;
		private UI.MoreCheckBox embeddedBox;
	}
}