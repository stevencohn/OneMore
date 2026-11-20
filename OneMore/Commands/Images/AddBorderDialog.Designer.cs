namespace River.OneMoreAddIn.Commands
{
	partial class AddBorderDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddBorderDialog));
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.colorLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.colorBox = new River.OneMoreAddIn.UI.MorePictureBox();
			this.widthLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.widthBox = new River.OneMoreAddIn.UI.MoreNumericUpDown();
			this.shadowBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.colorBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.widthBox)).BeginInit();
			this.SuspendLayout();
			//
			// okButton
			//
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(158, 178);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.TabIndex = 4;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OK);
			//
			// cancelButton
			//
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(264, 178);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.TabIndex = 5;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			//
			// colorLabel
			//
			this.colorLabel.AutoSize = true;
			this.colorLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.colorLabel.Location = new System.Drawing.Point(24, 28);
			this.colorLabel.Name = "colorLabel";
			this.colorLabel.Size = new System.Drawing.Size(55, 20);
			this.colorLabel.TabIndex = 0;
			this.colorLabel.Text = "Color";
			//
			// colorBox
			//
			this.colorBox.BackColor = System.Drawing.Color.Black;
			this.colorBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.colorBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.colorBox.Location = new System.Drawing.Point(178, 24);
			this.colorBox.Name = "colorBox";
			this.colorBox.Size = new System.Drawing.Size(120, 28);
			this.colorBox.TabIndex = 1;
			this.colorBox.TabStop = false;
			this.colorBox.Click += new System.EventHandler(this.ChooseColor);
			//
			// widthLabel
			//
			this.widthLabel.AutoSize = true;
			this.widthLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.widthLabel.Location = new System.Drawing.Point(24, 72);
			this.widthLabel.Name = "widthLabel";
			this.widthLabel.Scrubber = this.widthBox;
			this.widthLabel.Size = new System.Drawing.Size(52, 20);
			this.widthLabel.TabIndex = 6;
			this.widthLabel.Text = "Width";
			//
			// widthBox
			//
			this.widthBox.BackColor = System.Drawing.SystemColors.Window;
			this.widthBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.widthBox.Location = new System.Drawing.Point(178, 69);
			this.widthBox.DecimalPlaces = 2;
			this.widthBox.Increment = new decimal(new int[] {
            25,
            0,
            0,
            131072});
			this.widthBox.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
			this.widthBox.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
			this.widthBox.Name = "widthBox";
			this.widthBox.Size = new System.Drawing.Size(120, 26);
			this.widthBox.TabIndex = 2;
			this.widthBox.ThemedBack = null;
			this.widthBox.ThemedFore = null;
			this.widthBox.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
			//
			// shadowBox
			//
			this.shadowBox.AutoSize = true;
			this.shadowBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.shadowBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.shadowBox.Location = new System.Drawing.Point(28, 116);
			this.shadowBox.Name = "shadowBox";
			this.shadowBox.Size = new System.Drawing.Size(150, 24);
			this.shadowBox.TabIndex = 3;
			this.shadowBox.Text = "Add drop shadow";
			this.shadowBox.UseVisualStyleBackColor = true;
			//
			// AddBorderDialog
			//
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(388, 238);
			this.Controls.Add(this.shadowBox);
			this.Controls.Add(this.widthBox);
			this.Controls.Add(this.widthLabel);
			this.Controls.Add(this.colorBox);
			this.Controls.Add(this.colorLabel);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.ForeColor = System.Drawing.Color.Black;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AddBorderDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Add Border";
			((System.ComponentModel.ISupportInitialize)(this.colorBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.widthBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private UI.MoreLabel colorLabel;
		private UI.MorePictureBox colorBox;
		private UI.MoreLabel widthLabel;
		private UI.MoreNumericUpDown widthBox;
		private UI.MoreCheckBox shadowBox;
	}
}
