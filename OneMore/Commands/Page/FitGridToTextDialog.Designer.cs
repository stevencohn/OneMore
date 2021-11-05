namespace River.OneMoreAddIn.Commands
{
	partial class FitGridToTextDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FitGridToTextDialog));
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.autoButton = new System.Windows.Forms.RadioButton();
			this.customButton = new System.Windows.Forms.RadioButton();
			this.sizeLabel = new System.Windows.Forms.Label();
			this.sizeBox = new System.Windows.Forms.NumericUpDown();
			this.recommendBox = new System.Windows.Forms.TextBox();
			((System.ComponentModel.ISupportInitialize)(this.sizeBox)).BeginInit();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(264, 204);
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
			this.cancelButton.Location = new System.Drawing.Point(385, 204);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(112, 35);
			this.cancelButton.TabIndex = 3;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// autoButton
			// 
			this.autoButton.AutoSize = true;
			this.autoButton.Checked = true;
			this.autoButton.Location = new System.Drawing.Point(23, 23);
			this.autoButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 15);
			this.autoButton.Name = "autoButton";
			this.autoButton.Size = new System.Drawing.Size(391, 24);
			this.autoButton.TabIndex = 5;
			this.autoButton.TabStop = true;
			this.autoButton.Text = "Automatically adjust grid to most common font size";
			this.autoButton.UseVisualStyleBackColor = true;
			this.autoButton.CheckedChanged += new System.EventHandler(this.ChangeSelection);
			// 
			// customButton
			// 
			this.customButton.AutoSize = true;
			this.customButton.Location = new System.Drawing.Point(23, 65);
			this.customButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
			this.customButton.Name = "customButton";
			this.customButton.Size = new System.Drawing.Size(171, 24);
			this.customButton.TabIndex = 6;
			this.customButton.Text = "Customize grid size";
			this.customButton.UseVisualStyleBackColor = true;
			this.customButton.CheckedChanged += new System.EventHandler(this.ChangeSelection);
			// 
			// sizeLabel
			// 
			this.sizeLabel.AutoSize = true;
			this.sizeLabel.Location = new System.Drawing.Point(58, 102);
			this.sizeLabel.Name = "sizeLabel";
			this.sizeLabel.Size = new System.Drawing.Size(40, 20);
			this.sizeLabel.TabIndex = 7;
			this.sizeLabel.Text = "Size";
			// 
			// sizeBox
			// 
			this.sizeBox.DecimalPlaces = 4;
			this.sizeBox.Enabled = false;
			this.sizeBox.Location = new System.Drawing.Point(156, 100);
			this.sizeBox.Minimum = new decimal(new int[] {
            6,
            0,
            0,
            0});
			this.sizeBox.Name = "sizeBox";
			this.sizeBox.Size = new System.Drawing.Size(89, 26);
			this.sizeBox.TabIndex = 8;
			this.sizeBox.Value = new decimal(new int[] {
            6,
            0,
            0,
            0});
			// 
			// recommendBox
			// 
			this.recommendBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.recommendBox.Location = new System.Drawing.Point(62, 138);
			this.recommendBox.Multiline = true;
			this.recommendBox.Name = "recommendBox";
			this.recommendBox.ReadOnly = true;
			this.recommendBox.Size = new System.Drawing.Size(432, 52);
			this.recommendBox.TabIndex = 10;
			this.recommendBox.Text = "Recommended size for 11pt font is 13.12345";
			// 
			// FitGridToTextDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(509, 252);
			this.Controls.Add(this.recommendBox);
			this.Controls.Add(this.sizeBox);
			this.Controls.Add(this.sizeLabel);
			this.Controls.Add(this.customButton);
			this.Controls.Add(this.autoButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FitGridToTextDialog";
			this.Padding = new System.Windows.Forms.Padding(20, 20, 8, 8);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Fit Grid to Text";
			((System.ComponentModel.ISupportInitialize)(this.sizeBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.RadioButton autoButton;
		private System.Windows.Forms.RadioButton customButton;
		private System.Windows.Forms.Label sizeLabel;
		private System.Windows.Forms.NumericUpDown sizeBox;
		private System.Windows.Forms.TextBox recommendBox;
	}
}