
namespace River.OneMoreAddIn.Commands
{
	partial class SplitTableDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplitTableDialog));
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.copyHeaderBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.fixedColsBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(186, 152);
			this.okButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(112, 35);
			this.okButton.TabIndex = 4;
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
			this.cancelButton.Location = new System.Drawing.Point(308, 152);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(112, 35);
			this.cancelButton.TabIndex = 3;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// copyHeaderBox
			// 
			this.copyHeaderBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.copyHeaderBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.copyHeaderBox.Location = new System.Drawing.Point(27, 37);
			this.copyHeaderBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.copyHeaderBox.Name = "copyHeaderBox";
			this.copyHeaderBox.Size = new System.Drawing.Size(231, 25);
			this.copyHeaderBox.TabIndex = 5;
			this.copyHeaderBox.Text = "Duplicate table header row";
			this.copyHeaderBox.UseVisualStyleBackColor = true;
			// 
			// fixedColsBox
			// 
			this.fixedColsBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.fixedColsBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.fixedColsBox.Location = new System.Drawing.Point(27, 72);
			this.fixedColsBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.fixedColsBox.Name = "fixedColsBox";
			this.fixedColsBox.Size = new System.Drawing.Size(252, 25);
			this.fixedColsBox.TabIndex = 6;
			this.fixedColsBox.Text = "Set all columns as fixed-width";
			this.fixedColsBox.UseVisualStyleBackColor = true;
			// 
			// SplitTableDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(440, 208);
			this.Controls.Add(this.fixedColsBox);
			this.Controls.Add(this.copyHeaderBox);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SplitTableDialog";
			this.Padding = new System.Windows.Forms.Padding(15);
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Split Table Options";
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private UI.MoreCheckBox copyHeaderBox;
		private UI.MoreCheckBox fixedColsBox;
	}
}