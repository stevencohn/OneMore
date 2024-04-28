namespace River.OneMoreAddIn.Commands
{
	partial class ReportRemindersDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReportRemindersDialog));
			this.introLabel = new System.Windows.Forms.Label();
			this.notebookRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.sectionRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.notebooksRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.showCompletedBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.SuspendLayout();
			// 
			// introLabel
			// 
			this.introLabel.AutoSize = true;
			this.introLabel.Location = new System.Drawing.Point(18, 20);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(236, 20);
			this.introLabel.TabIndex = 12;
			this.introLabel.Text = "Generate report for reminders in";
			// 
			// notebookRadio
			// 
			this.notebookRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.notebookRadio.Location = new System.Drawing.Point(22, 97);
			this.notebookRadio.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.notebookRadio.Name = "notebookRadio";
			this.notebookRadio.Size = new System.Drawing.Size(291, 25);
			this.notebookRadio.TabIndex = 1;
			this.notebookRadio.Text = "All sections in the current notebook";
			this.notebookRadio.UseVisualStyleBackColor = true;
			// 
			// sectionRadio
			// 
			this.sectionRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.sectionRadio.Location = new System.Drawing.Point(22, 134);
			this.sectionRadio.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.sectionRadio.Name = "sectionRadio";
			this.sectionRadio.Size = new System.Drawing.Size(176, 25);
			this.sectionRadio.TabIndex = 2;
			this.sectionRadio.Text = "The current section";
			this.sectionRadio.UseVisualStyleBackColor = true;
			// 
			// notebooksRadio
			// 
			this.notebooksRadio.Checked = true;
			this.notebooksRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.notebooksRadio.Location = new System.Drawing.Point(22, 60);
			this.notebooksRadio.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.notebooksRadio.Name = "notebooksRadio";
			this.notebooksRadio.Size = new System.Drawing.Size(134, 25);
			this.notebooksRadio.TabIndex = 0;
			this.notebooksRadio.TabStop = true;
			this.notebooksRadio.Text = "All notebooks";
			this.notebooksRadio.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(420, 264);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(120, 38);
			this.cancelButton.StylizeImage = false;
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
			this.okButton.Location = new System.Drawing.Point(294, 264);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(120, 38);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// showCompletedBox
			// 
			this.showCompletedBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.showCompletedBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.showCompletedBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.showCompletedBox.Location = new System.Drawing.Point(22, 194);
			this.showCompletedBox.Name = "showCompletedBox";
			this.showCompletedBox.Size = new System.Drawing.Size(247, 25);
			this.showCompletedBox.StylizeImage = false;
			this.showCompletedBox.TabIndex = 13;
			this.showCompletedBox.Text = "Include completed reminders";
			this.showCompletedBox.ThemedBack = null;
			this.showCompletedBox.ThemedFore = null;
			this.showCompletedBox.UseVisualStyleBackColor = false;
			// 
			// ReportRemindersDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(553, 315);
			this.Controls.Add(this.showCompletedBox);
			this.Controls.Add(this.notebookRadio);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.sectionRadio);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.notebooksRadio);
			this.Controls.Add(this.introLabel);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "ReportRemindersDialog";
			this.Padding = new System.Windows.Forms.Padding(15, 20, 10, 10);
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Report Scope";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label introLabel;
		private UI.MoreRadioButton notebookRadio;
		private UI.MoreRadioButton sectionRadio;
		private UI.MoreRadioButton notebooksRadio;
		private UI.MoreButton cancelButton;
		private UI.MoreButton okButton;
		private UI.MoreCheckBox showCompletedBox;
	}
}