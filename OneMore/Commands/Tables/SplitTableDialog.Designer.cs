
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
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.copyHeaderBox = new System.Windows.Forms.CheckBox();
			this.fixedColsBox = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(186, 152);
			this.okButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(112, 35);
			this.okButton.TabIndex = 4;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(308, 152);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(112, 35);
			this.cancelButton.TabIndex = 3;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// copyHeaderBox
			// 
			this.copyHeaderBox.AutoSize = true;
			this.copyHeaderBox.Location = new System.Drawing.Point(20, 37);
			this.copyHeaderBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.copyHeaderBox.Name = "copyHeaderBox";
			this.copyHeaderBox.Size = new System.Drawing.Size(224, 24);
			this.copyHeaderBox.TabIndex = 5;
			this.copyHeaderBox.Text = "Duplicate table header row";
			this.copyHeaderBox.UseVisualStyleBackColor = true;
			// 
			// fixedColsBox
			// 
			this.fixedColsBox.AutoSize = true;
			this.fixedColsBox.Location = new System.Drawing.Point(20, 72);
			this.fixedColsBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.fixedColsBox.Name = "fixedColsBox";
			this.fixedColsBox.Size = new System.Drawing.Size(242, 24);
			this.fixedColsBox.TabIndex = 6;
			this.fixedColsBox.Text = "Set all columns as fixed-width";
			this.fixedColsBox.UseVisualStyleBackColor = true;
			// 
			// SplitTableDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(440, 208);
			this.Controls.Add(this.fixedColsBox);
			this.Controls.Add(this.copyHeaderBox);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SplitTableDialog";
			this.Padding = new System.Windows.Forms.Padding(15, 15, 15, 15);
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Split Table Options";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.CheckBox copyHeaderBox;
		private System.Windows.Forms.CheckBox fixedColsBox;
	}
}