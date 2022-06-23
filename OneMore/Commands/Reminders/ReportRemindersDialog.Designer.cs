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
			this.introLabel = new System.Windows.Forms.Label();
			this.groupBox = new System.Windows.Forms.GroupBox();
			this.notebookRadio = new System.Windows.Forms.RadioButton();
			this.sectionRadio = new System.Windows.Forms.RadioButton();
			this.notebooksRadio = new System.Windows.Forms.RadioButton();
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.groupBox.SuspendLayout();
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
			// groupBox
			// 
			this.groupBox.Controls.Add(this.notebookRadio);
			this.groupBox.Controls.Add(this.sectionRadio);
			this.groupBox.Controls.Add(this.notebooksRadio);
			this.groupBox.Location = new System.Drawing.Point(22, 59);
			this.groupBox.Name = "groupBox";
			this.groupBox.Padding = new System.Windows.Forms.Padding(20);
			this.groupBox.Size = new System.Drawing.Size(536, 171);
			this.groupBox.TabIndex = 11;
			this.groupBox.TabStop = false;
			this.groupBox.Text = "Scope";
			// 
			// notebookRadio
			// 
			this.notebookRadio.AutoSize = true;
			this.notebookRadio.Location = new System.Drawing.Point(23, 79);
			this.notebookRadio.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.notebookRadio.Name = "notebookRadio";
			this.notebookRadio.Size = new System.Drawing.Size(282, 24);
			this.notebookRadio.TabIndex = 3;
			this.notebookRadio.Text = "All sections in the current notebook";
			this.notebookRadio.UseVisualStyleBackColor = true;
			// 
			// sectionRadio
			// 
			this.sectionRadio.AutoSize = true;
			this.sectionRadio.Location = new System.Drawing.Point(23, 116);
			this.sectionRadio.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.sectionRadio.Name = "sectionRadio";
			this.sectionRadio.Size = new System.Drawing.Size(170, 24);
			this.sectionRadio.TabIndex = 4;
			this.sectionRadio.Text = "The current section";
			this.sectionRadio.UseVisualStyleBackColor = true;
			// 
			// notebooksRadio
			// 
			this.notebooksRadio.AutoSize = true;
			this.notebooksRadio.Checked = true;
			this.notebooksRadio.Location = new System.Drawing.Point(23, 42);
			this.notebooksRadio.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.notebooksRadio.Name = "notebooksRadio";
			this.notebooksRadio.Size = new System.Drawing.Size(130, 24);
			this.notebooksRadio.TabIndex = 2;
			this.notebooksRadio.TabStop = true;
			this.notebooksRadio.Text = "All notebooks";
			this.notebooksRadio.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(441, 241);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(120, 38);
			this.cancelButton.TabIndex = 10;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(315, 241);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(120, 38);
			this.okButton.TabIndex = 9;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// ReportRemindersDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(574, 292);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.introLabel);
			this.Controls.Add(this.groupBox);
			this.Name = "ReportRemindersDialog";
			this.Padding = new System.Windows.Forms.Padding(15, 20, 10, 10);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Report Scope";
			this.groupBox.ResumeLayout(false);
			this.groupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label introLabel;
		private System.Windows.Forms.GroupBox groupBox;
		private System.Windows.Forms.RadioButton notebookRadio;
		private System.Windows.Forms.RadioButton sectionRadio;
		private System.Windows.Forms.RadioButton notebooksRadio;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
	}
}