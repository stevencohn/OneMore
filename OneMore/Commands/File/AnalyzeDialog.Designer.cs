
namespace River.OneMoreAddIn.Commands
{
	partial class AnalyzeDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AnalyzeDialog));
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.notebookBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.sectionBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.sectionDetailBox = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.allDetailsBox = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.noDetailsBox = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.thumbnailLabel = new System.Windows.Forms.Label();
			this.sizeBox = new System.Windows.Forms.ComboBox();
			this.warningLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(270, 307);
			this.okButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(112, 35);
			this.okButton.TabIndex = 0;
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
			this.cancelButton.Location = new System.Drawing.Point(390, 307);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(112, 35);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// notebookBox
			// 
			this.notebookBox.Checked = true;
			this.notebookBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.notebookBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.notebookBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.notebookBox.Location = new System.Drawing.Point(33, 39);
			this.notebookBox.Name = "notebookBox";
			this.notebookBox.Size = new System.Drawing.Size(290, 25);
			this.notebookBox.TabIndex = 2;
			this.notebookBox.Text = "Include notebook backup summary";
			this.notebookBox.UseVisualStyleBackColor = true;
			this.notebookBox.CheckedChanged += new System.EventHandler(this.Validate);
			// 
			// sectionBox
			// 
			this.sectionBox.Checked = true;
			this.sectionBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.sectionBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.sectionBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.sectionBox.Location = new System.Drawing.Point(33, 69);
			this.sectionBox.Name = "sectionBox";
			this.sectionBox.Size = new System.Drawing.Size(217, 25);
			this.sectionBox.TabIndex = 3;
			this.sectionBox.Text = "Include section summary";
			this.sectionBox.UseVisualStyleBackColor = true;
			this.sectionBox.CheckedChanged += new System.EventHandler(this.Validate);
			// 
			// sectionDetailBox
			// 
			this.sectionDetailBox.Checked = true;
			this.sectionDetailBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.sectionDetailBox.Location = new System.Drawing.Point(33, 123);
			this.sectionDetailBox.Name = "sectionDetailBox";
			this.sectionDetailBox.Size = new System.Drawing.Size(318, 25);
			this.sectionDetailBox.TabIndex = 4;
			this.sectionDetailBox.TabStop = true;
			this.sectionDetailBox.Text = "Include page details for current section";
			this.sectionDetailBox.UseVisualStyleBackColor = true;
			this.sectionDetailBox.CheckedChanged += new System.EventHandler(this.Validate);
			// 
			// allDetailsBox
			// 
			this.allDetailsBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.allDetailsBox.Location = new System.Drawing.Point(33, 153);
			this.allDetailsBox.Name = "allDetailsBox";
			this.allDetailsBox.Size = new System.Drawing.Size(411, 25);
			this.allDetailsBox.TabIndex = 5;
			this.allDetailsBox.Text = "Include page details for all sections in this notebook";
			this.allDetailsBox.UseVisualStyleBackColor = true;
			this.allDetailsBox.CheckedChanged += new System.EventHandler(this.Validate);
			// 
			// noDetailsBox
			// 
			this.noDetailsBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.noDetailsBox.Location = new System.Drawing.Point(33, 183);
			this.noDetailsBox.Name = "noDetailsBox";
			this.noDetailsBox.Size = new System.Drawing.Size(150, 25);
			this.noDetailsBox.TabIndex = 6;
			this.noDetailsBox.Text = "No page details";
			this.noDetailsBox.UseVisualStyleBackColor = true;
			this.noDetailsBox.CheckedChanged += new System.EventHandler(this.Validate);
			// 
			// thumbnailLabel
			// 
			this.thumbnailLabel.AutoSize = true;
			this.thumbnailLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.thumbnailLabel.Location = new System.Drawing.Point(56, 228);
			this.thumbnailLabel.Name = "thumbnailLabel";
			this.thumbnailLabel.Size = new System.Drawing.Size(94, 20);
			this.thumbnailLabel.TabIndex = 12;
			this.thumbnailLabel.Text = "Thumbnails:";
			// 
			// sizeBox
			// 
			this.sizeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.sizeBox.FormattingEnabled = true;
			this.sizeBox.Items.AddRange(new object[] {
            "20 x 20",
            "40 x 40",
            "80 x 80"});
			this.sizeBox.Location = new System.Drawing.Point(156, 225);
			this.sizeBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
			this.sizeBox.Name = "sizeBox";
			this.sizeBox.Size = new System.Drawing.Size(185, 28);
			this.sizeBox.TabIndex = 7;
			this.sizeBox.SelectedIndexChanged += new System.EventHandler(this.Validate);
			// 
			// warningLabel
			// 
			this.warningLabel.AutoSize = true;
			this.warningLabel.ForeColor = System.Drawing.Color.Maroon;
			this.warningLabel.Location = new System.Drawing.Point(152, 259);
			this.warningLabel.Name = "warningLabel";
			this.warningLabel.Size = new System.Drawing.Size(186, 20);
			this.warningLabel.TabIndex = 14;
			this.warningLabel.Text = "Report may take minutes";
			this.warningLabel.ThemedBack = null;
			this.warningLabel.ThemedFore = "ErrorText";
			this.warningLabel.Visible = false;
			// 
			// AnalyzeDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(521, 362);
			this.Controls.Add(this.warningLabel);
			this.Controls.Add(this.sizeBox);
			this.Controls.Add(this.thumbnailLabel);
			this.Controls.Add(this.noDetailsBox);
			this.Controls.Add(this.allDetailsBox);
			this.Controls.Add(this.sectionDetailBox);
			this.Controls.Add(this.sectionBox);
			this.Controls.Add(this.notebookBox);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AnalyzeDialog";
			this.Padding = new System.Windows.Forms.Padding(15);
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Storage Analysis Report Options";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private UI.MoreCheckBox notebookBox;
		private UI.MoreCheckBox sectionBox;
		private UI.MoreRadioButton sectionDetailBox;
		private UI.MoreRadioButton allDetailsBox;
		private UI.MoreRadioButton noDetailsBox;
		private System.Windows.Forms.Label thumbnailLabel;
		private System.Windows.Forms.ComboBox sizeBox;
		private UI.MoreLabel warningLabel;
	}
}