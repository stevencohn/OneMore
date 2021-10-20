
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
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.notebookBox = new System.Windows.Forms.CheckBox();
			this.sectionBox = new System.Windows.Forms.CheckBox();
			this.sectionDetailBox = new System.Windows.Forms.RadioButton();
			this.allDetailsBox = new System.Windows.Forms.RadioButton();
			this.noDetailsBox = new System.Windows.Forms.RadioButton();
			this.thumbnailLabel = new System.Windows.Forms.Label();
			this.sizeBox = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(270, 295);
			this.okButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(112, 35);
			this.okButton.TabIndex = 6;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(390, 295);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(112, 35);
			this.cancelButton.TabIndex = 5;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// notebookBox
			// 
			this.notebookBox.AutoSize = true;
			this.notebookBox.Checked = true;
			this.notebookBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.notebookBox.Location = new System.Drawing.Point(33, 39);
			this.notebookBox.Name = "notebookBox";
			this.notebookBox.Size = new System.Drawing.Size(282, 24);
			this.notebookBox.TabIndex = 7;
			this.notebookBox.Text = "Include notebook backup summary";
			this.notebookBox.UseVisualStyleBackColor = true;
			this.notebookBox.CheckedChanged += new System.EventHandler(this.Validate);
			// 
			// sectionBox
			// 
			this.sectionBox.AutoSize = true;
			this.sectionBox.Checked = true;
			this.sectionBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.sectionBox.Location = new System.Drawing.Point(33, 69);
			this.sectionBox.Name = "sectionBox";
			this.sectionBox.Size = new System.Drawing.Size(210, 24);
			this.sectionBox.TabIndex = 8;
			this.sectionBox.Text = "Include section summary";
			this.sectionBox.UseVisualStyleBackColor = true;
			this.sectionBox.CheckedChanged += new System.EventHandler(this.Validate);
			// 
			// sectionDetailBox
			// 
			this.sectionDetailBox.AutoSize = true;
			this.sectionDetailBox.Checked = true;
			this.sectionDetailBox.Location = new System.Drawing.Point(33, 123);
			this.sectionDetailBox.Name = "sectionDetailBox";
			this.sectionDetailBox.Size = new System.Drawing.Size(308, 24);
			this.sectionDetailBox.TabIndex = 9;
			this.sectionDetailBox.TabStop = true;
			this.sectionDetailBox.Text = "Include page details for current section";
			this.sectionDetailBox.UseVisualStyleBackColor = true;
			this.sectionDetailBox.CheckedChanged += new System.EventHandler(this.Validate);
			// 
			// allDetailsBox
			// 
			this.allDetailsBox.AutoSize = true;
			this.allDetailsBox.Location = new System.Drawing.Point(33, 153);
			this.allDetailsBox.Name = "allDetailsBox";
			this.allDetailsBox.Size = new System.Drawing.Size(397, 24);
			this.allDetailsBox.TabIndex = 10;
			this.allDetailsBox.Text = "Include page details for all sections in this notebook";
			this.allDetailsBox.UseVisualStyleBackColor = true;
			this.allDetailsBox.CheckedChanged += new System.EventHandler(this.Validate);
			// 
			// noDetailsBox
			// 
			this.noDetailsBox.AutoSize = true;
			this.noDetailsBox.Location = new System.Drawing.Point(33, 183);
			this.noDetailsBox.Name = "noDetailsBox";
			this.noDetailsBox.Size = new System.Drawing.Size(144, 24);
			this.noDetailsBox.TabIndex = 11;
			this.noDetailsBox.Text = "No page details";
			this.noDetailsBox.UseVisualStyleBackColor = true;
			this.noDetailsBox.CheckedChanged += new System.EventHandler(this.Validate);
			// 
			// thumbnailLabel
			// 
			this.thumbnailLabel.AutoSize = true;
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
			this.sizeBox.Name = "sizeBox";
			this.sizeBox.Size = new System.Drawing.Size(185, 28);
			this.sizeBox.TabIndex = 13;
			// 
			// AnalyzeDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(521, 350);
			this.Controls.Add(this.sizeBox);
			this.Controls.Add(this.thumbnailLabel);
			this.Controls.Add(this.noDetailsBox);
			this.Controls.Add(this.allDetailsBox);
			this.Controls.Add(this.sectionDetailBox);
			this.Controls.Add(this.sectionBox);
			this.Controls.Add(this.notebookBox);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AnalyzeDialog";
			this.Padding = new System.Windows.Forms.Padding(15);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Storage Analysis Report Options";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.CheckBox notebookBox;
		private System.Windows.Forms.CheckBox sectionBox;
		private System.Windows.Forms.RadioButton sectionDetailBox;
		private System.Windows.Forms.RadioButton allDetailsBox;
		private System.Windows.Forms.RadioButton noDetailsBox;
		private System.Windows.Forms.Label thumbnailLabel;
		private System.Windows.Forms.ComboBox sizeBox;
	}
}