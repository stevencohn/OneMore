
namespace River.OneMoreAddIn.Commands
{
	partial class BreakingDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BreakingDialog));
			this.oneButton = new System.Windows.Forms.RadioButton();
			this.twoButton = new System.Windows.Forms.RadioButton();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.groupBox = new System.Windows.Forms.GroupBox();
			this.groupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// oneButton
			// 
			this.oneButton.AutoSize = true;
			this.oneButton.Checked = true;
			this.oneButton.Location = new System.Drawing.Point(13, 32);
			this.oneButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 15);
			this.oneButton.Name = "oneButton";
			this.oneButton.Size = new System.Drawing.Size(254, 24);
			this.oneButton.TabIndex = 0;
			this.oneButton.TabStop = true;
			this.oneButton.Text = "One space between sentences";
			this.oneButton.UseVisualStyleBackColor = true;
			// 
			// twoButton
			// 
			this.twoButton.AutoSize = true;
			this.twoButton.Location = new System.Drawing.Point(13, 74);
			this.twoButton.Name = "twoButton";
			this.twoButton.Size = new System.Drawing.Size(261, 24);
			this.twoButton.TabIndex = 1;
			this.twoButton.Text = "Two spaces between sentences";
			this.twoButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(246, 176);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.TabIndex = 7;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(352, 176);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.TabIndex = 6;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// groupBox
			// 
			this.groupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox.Controls.Add(this.oneButton);
			this.groupBox.Controls.Add(this.twoButton);
			this.groupBox.Location = new System.Drawing.Point(23, 23);
			this.groupBox.Name = "groupBox";
			this.groupBox.Padding = new System.Windows.Forms.Padding(10, 10, 3, 3);
			this.groupBox.Size = new System.Drawing.Size(429, 134);
			this.groupBox.TabIndex = 8;
			this.groupBox.TabStop = false;
			this.groupBox.Text = "Spacing";
			// 
			// BreakingDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(465, 227);
			this.Controls.Add(this.groupBox);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "BreakingDialog";
			this.Padding = new System.Windows.Forms.Padding(20, 20, 10, 10);
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Change Sentence Spacing";
			this.groupBox.ResumeLayout(false);
			this.groupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RadioButton oneButton;
		private System.Windows.Forms.RadioButton twoButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.GroupBox groupBox;
	}
}