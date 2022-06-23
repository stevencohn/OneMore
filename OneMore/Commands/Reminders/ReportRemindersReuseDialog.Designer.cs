namespace River.OneMoreAddIn.Commands
{
	partial class ReportRemindersReuseDialog
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
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.oldRadio = new System.Windows.Forms.RadioButton();
			this.newRadio = new System.Windows.Forms.RadioButton();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(184, 111);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(120, 38);
			this.okButton.TabIndex = 11;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(310, 111);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(120, 38);
			this.cancelButton.TabIndex = 12;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// oldRadio
			// 
			this.oldRadio.AutoSize = true;
			this.oldRadio.Checked = true;
			this.oldRadio.Location = new System.Drawing.Point(23, 29);
			this.oldRadio.Name = "oldRadio";
			this.oldRadio.Size = new System.Drawing.Size(219, 24);
			this.oldRadio.TabIndex = 13;
			this.oldRadio.TabStop = true;
			this.oldRadio.Text = "Update this existing report";
			this.oldRadio.UseVisualStyleBackColor = true;
			// 
			// newRadio
			// 
			this.newRadio.AutoSize = true;
			this.newRadio.Location = new System.Drawing.Point(23, 63);
			this.newRadio.Name = "newRadio";
			this.newRadio.Size = new System.Drawing.Size(214, 24);
			this.newRadio.TabIndex = 14;
			this.newRadio.Text = "Create a new report page";
			this.newRadio.UseVisualStyleBackColor = true;
			// 
			// ReportRemindersReuseDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(443, 162);
			this.Controls.Add(this.newRadio);
			this.Controls.Add(this.oldRadio);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Name = "ReportRemindersReuseDialog";
			this.Padding = new System.Windows.Forms.Padding(20, 20, 10, 10);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Old or New";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.RadioButton oldRadio;
		private System.Windows.Forms.RadioButton newRadio;
	}
}