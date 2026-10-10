
namespace River.OneMoreAddIn.Commands
{
	partial class RemoveEmptyDialog
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
			this.removeRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.keepRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.exactRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.headingBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.noteLabel = new System.Windows.Forms.Label();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.SuspendLayout();
			// 
			// removeRadio
			// 
			this.removeRadio.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.removeRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.removeRadio.Location = new System.Drawing.Point(28, 28);
			this.removeRadio.Name = "removeRadio";
			this.removeRadio.Size = new System.Drawing.Size(601, 25);
			this.removeRadio.TabIndex = 0;
			this.removeRadio.Text = "Remove all blank lines between paragraphs";
			this.removeRadio.UseVisualStyleBackColor = true;
			// 
			// keepRadio
			// 
			this.keepRadio.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.keepRadio.Checked = true;
			this.keepRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.keepRadio.Location = new System.Drawing.Point(28, 64);
			this.keepRadio.Name = "keepRadio";
			this.keepRadio.Size = new System.Drawing.Size(601, 25);
			this.keepRadio.TabIndex = 1;
			this.keepRadio.TabStop = true;
			this.keepRadio.Text = "Keep at most one blank line between paragraphs";
			this.keepRadio.UseVisualStyleBackColor = true;
			// 
			// exactRadio
			// 
			this.exactRadio.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.exactRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.exactRadio.Location = new System.Drawing.Point(28, 100);
			this.exactRadio.Name = "exactRadio";
			this.exactRadio.Size = new System.Drawing.Size(601, 25);
			this.exactRadio.TabIndex = 2;
			this.exactRadio.Text = "Ensure exactly one blank line between paragraphs";
			this.exactRadio.UseVisualStyleBackColor = true;
			// 
			// headingBox
			// 
			this.headingBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.headingBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.headingBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.headingBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.headingBox.Location = new System.Drawing.Point(28, 152);
			this.headingBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.headingBox.Name = "headingBox";
			this.headingBox.Size = new System.Drawing.Size(601, 25);
			this.headingBox.StylizeImage = false;
			this.headingBox.TabIndex = 3;
			this.headingBox.Text = "Put one blank line after each heading";
			this.headingBox.ThemedBack = null;
			this.headingBox.ThemedFore = null;
			this.headingBox.UseVisualStyleBackColor = true;
			// 
			// noteLabel
			// 
			this.noteLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.noteLabel.BackColor = System.Drawing.Color.Transparent;
			this.noteLabel.Location = new System.Drawing.Point(52, 182);
			this.noteLabel.Name = "noteLabel";
			this.noteLabel.Size = new System.Drawing.Size(577, 60);
			this.noteLabel.TabIndex = 4;
			this.noteLabel.Text = "Unchecked removes blank lines after headings. Empty headings are always removed o" +
    "r converted to normal blank lines.";
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(423, 262);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 5;
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
			this.cancelButton.Location = new System.Drawing.Point(529, 262);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 6;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = false;
			// 
			// RemoveEmptyDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(657, 328);
			this.Controls.Add(this.removeRadio);
			this.Controls.Add(this.keepRadio);
			this.Controls.Add(this.exactRadio);
			this.Controls.Add(this.headingBox);
			this.Controls.Add(this.noteLabel);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "RemoveEmptyDialog";
			this.Padding = new System.Windows.Forms.Padding(28);
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Adjust Blank Lines";
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreRadioButton removeRadio;
		private UI.MoreRadioButton keepRadio;
		private UI.MoreRadioButton exactRadio;
		private UI.MoreCheckBox headingBox;
		private System.Windows.Forms.Label noteLabel;
		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
	}
}
