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
			this.pathBox = new System.Windows.Forms.TextBox();
			this.browseButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.wordGroup = new System.Windows.Forms.GroupBox();
			this.wordCreateButton = new System.Windows.Forms.RadioButton();
			this.wordAppendButton = new System.Windows.Forms.RadioButton();
			this.powerGroup = new System.Windows.Forms.GroupBox();
			this.powerCreateButton = new System.Windows.Forms.RadioButton();
			this.powerAppendButton = new System.Windows.Forms.RadioButton();
			this.powerSectionButton = new System.Windows.Forms.RadioButton();
			this.introLabel = new System.Windows.Forms.Label();
			this.notInstalledLabel = new System.Windows.Forms.Label();
			this.wordGroup.SuspendLayout();
			this.powerGroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// fileLabel
			// 
			this.fileLabel.AutoSize = true;
			this.fileLabel.Location = new System.Drawing.Point(18, 57);
			this.fileLabel.Name = "fileLabel";
			this.fileLabel.Size = new System.Drawing.Size(38, 20);
			this.fileLabel.TabIndex = 0;
			this.fileLabel.Text = "File:";
			// 
			// pathBox
			// 
			this.pathBox.Location = new System.Drawing.Point(62, 54);
			this.pathBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 15);
			this.pathBox.Name = "pathBox";
			this.pathBox.Size = new System.Drawing.Size(449, 26);
			this.pathBox.TabIndex = 1;
			this.pathBox.TextChanged += new System.EventHandler(this.ChangePath);
			// 
			// browseButton
			// 
			this.browseButton.Image = global::River.OneMoreAddIn.Properties.Resources.Open;
			this.browseButton.Location = new System.Drawing.Point(517, 54);
			this.browseButton.Name = "browseButton";
			this.browseButton.Size = new System.Drawing.Size(44, 26);
			this.browseButton.TabIndex = 4;
			this.browseButton.UseVisualStyleBackColor = true;
			this.browseButton.Click += new System.EventHandler(this.BrowseFile);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(462, 261);
			this.cancelButton.Name = "cancelButton";
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
			this.okButton.Location = new System.Drawing.Point(356, 261);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.TabIndex = 7;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// wordGroup
			// 
			this.wordGroup.Controls.Add(this.wordCreateButton);
			this.wordGroup.Controls.Add(this.wordAppendButton);
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
			this.wordCreateButton.AutoSize = true;
			this.wordCreateButton.Location = new System.Drawing.Point(40, 67);
			this.wordCreateButton.Name = "wordCreateButton";
			this.wordCreateButton.Size = new System.Drawing.Size(155, 24);
			this.wordCreateButton.TabIndex = 10;
			this.wordCreateButton.TabStop = true;
			this.wordCreateButton.Text = "Create new page";
			this.wordCreateButton.UseVisualStyleBackColor = true;
			// 
			// wordAppendButton
			// 
			this.wordAppendButton.AutoSize = true;
			this.wordAppendButton.Checked = true;
			this.wordAppendButton.Location = new System.Drawing.Point(40, 32);
			this.wordAppendButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
			this.wordAppendButton.Name = "wordAppendButton";
			this.wordAppendButton.Size = new System.Drawing.Size(177, 24);
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
			this.powerCreateButton.AutoSize = true;
			this.powerCreateButton.Location = new System.Drawing.Point(40, 67);
			this.powerCreateButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
			this.powerCreateButton.Name = "powerCreateButton";
			this.powerCreateButton.Size = new System.Drawing.Size(155, 24);
			this.powerCreateButton.TabIndex = 1;
			this.powerCreateButton.TabStop = true;
			this.powerCreateButton.Text = "Create new page";
			this.powerCreateButton.UseVisualStyleBackColor = true;
			// 
			// powerAppendButton
			// 
			this.powerAppendButton.AutoSize = true;
			this.powerAppendButton.Checked = true;
			this.powerAppendButton.Location = new System.Drawing.Point(40, 32);
			this.powerAppendButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
			this.powerAppendButton.Name = "powerAppendButton";
			this.powerAppendButton.Size = new System.Drawing.Size(177, 24);
			this.powerAppendButton.TabIndex = 0;
			this.powerAppendButton.TabStop = true;
			this.powerAppendButton.Text = "Append to this page";
			this.powerAppendButton.UseVisualStyleBackColor = true;
			// 
			// powerSectionButton
			// 
			this.powerSectionButton.AutoSize = true;
			this.powerSectionButton.Location = new System.Drawing.Point(40, 101);
			this.powerSectionButton.Name = "powerSectionButton";
			this.powerSectionButton.Size = new System.Drawing.Size(343, 24);
			this.powerSectionButton.TabIndex = 2;
			this.powerSectionButton.TabStop = true;
			this.powerSectionButton.Text = "Create a new section with a page each slide";
			this.powerSectionButton.UseVisualStyleBackColor = true;
			// 
			// introLabel
			// 
			this.introLabel.AutoSize = true;
			this.introLabel.Location = new System.Drawing.Point(18, 15);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(433, 20);
			this.introLabel.TabIndex = 9;
			this.introLabel.Text = "Select a Word, PowerPoint, OneNote, Markdown, or XML file";
			// 
			// notInstalledLabel
			// 
			this.notInstalledLabel.AutoSize = true;
			this.notInstalledLabel.Location = new System.Drawing.Point(27, 113);
			this.notInstalledLabel.Name = "notInstalledLabel";
			this.notInstalledLabel.Size = new System.Drawing.Size(306, 20);
			this.notInstalledLabel.TabIndex = 10;
			this.notInstalledLabel.Text = "The required Office product is not installed";
			this.notInstalledLabel.Visible = false;
			// 
			// ImportDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(579, 317);
			this.Controls.Add(this.notInstalledLabel);
			this.Controls.Add(this.introLabel);
			this.Controls.Add(this.wordGroup);
			this.Controls.Add(this.powerGroup);
			this.Controls.Add(this.browseButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.pathBox);
			this.Controls.Add(this.fileLabel);
			this.Controls.Add(this.cancelButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ImportDialog";
			this.Padding = new System.Windows.Forms.Padding(15);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Import";
			this.wordGroup.ResumeLayout(false);
			this.wordGroup.PerformLayout();
			this.powerGroup.ResumeLayout(false);
			this.powerGroup.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label fileLabel;
		private System.Windows.Forms.TextBox pathBox;
		private System.Windows.Forms.Button browseButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.GroupBox wordGroup;
		private System.Windows.Forms.RadioButton wordCreateButton;
		private System.Windows.Forms.RadioButton wordAppendButton;
		private System.Windows.Forms.GroupBox powerGroup;
		private System.Windows.Forms.RadioButton powerCreateButton;
		private System.Windows.Forms.RadioButton powerAppendButton;
		private System.Windows.Forms.RadioButton powerSectionButton;
		private System.Windows.Forms.Label introLabel;
		private System.Windows.Forms.Label notInstalledLabel;
	}
}