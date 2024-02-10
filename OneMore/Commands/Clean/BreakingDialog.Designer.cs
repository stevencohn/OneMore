
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
			this.oneButton = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.twoButton = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.SuspendLayout();
			// 
			// oneButton
			// 
			this.oneButton.Checked = true;
			this.oneButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.oneButton.Location = new System.Drawing.Point(28, 28);
			this.oneButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 15);
			this.oneButton.Name = "oneButton";
			this.oneButton.Size = new System.Drawing.Size(261, 25);
			this.oneButton.TabIndex = 0;
			this.oneButton.TabStop = true;
			this.oneButton.Text = "One space between sentences";
			this.oneButton.UseVisualStyleBackColor = true;
			// 
			// twoButton
			// 
			this.twoButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.twoButton.Location = new System.Drawing.Point(28, 71);
			this.twoButton.Name = "twoButton";
			this.twoButton.Size = new System.Drawing.Size(269, 25);
			this.twoButton.TabIndex = 1;
			this.twoButton.Text = "Two spaces between sentences";
			this.twoButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(246, 142);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = false;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(352, 142);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = false;
			// 
			// BreakingDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(465, 193);
			this.Controls.Add(this.oneButton);
			this.Controls.Add(this.twoButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "BreakingDialog";
			this.Padding = new System.Windows.Forms.Padding(25, 25, 10, 10);
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Change Sentence Spacing";
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreRadioButton oneButton;
		private UI.MoreRadioButton twoButton;
		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
	}
}