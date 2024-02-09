namespace River.OneMoreAddIn.Commands
{
	partial class InsertCellsDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InsertCellsDialog));
			this.shiftDownRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.shiftRightRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.numLabel = new System.Windows.Forms.Label();
			this.numCellsBox = new System.Windows.Forms.NumericUpDown();
			((System.ComponentModel.ISupportInitialize)(this.numCellsBox)).BeginInit();
			this.SuspendLayout();
			// 
			// shiftDownRadio
			// 
			this.shiftDownRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.shiftDownRadio.Location = new System.Drawing.Point(28, 28);
			this.shiftDownRadio.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
			this.shiftDownRadio.Name = "shiftDownRadio";
			this.shiftDownRadio.Size = new System.Drawing.Size(150, 25);
			this.shiftDownRadio.TabIndex = 0;
			this.shiftDownRadio.TabStop = true;
			this.shiftDownRadio.Text = "Shift cells down";
			this.shiftDownRadio.UseVisualStyleBackColor = true;
			// 
			// shiftRightRadio
			// 
			this.shiftRightRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.shiftRightRadio.Location = new System.Drawing.Point(28, 63);
			this.shiftRightRadio.Margin = new System.Windows.Forms.Padding(3, 3, 3, 15);
			this.shiftRightRadio.Name = "shiftRightRadio";
			this.shiftRightRadio.Size = new System.Drawing.Size(143, 25);
			this.shiftRightRadio.TabIndex = 1;
			this.shiftRightRadio.TabStop = true;
			this.shiftRightRadio.Text = "Shift cells right";
			this.shiftRightRadio.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(296, 167);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(110, 34);
			this.cancelButton.TabIndex = 2;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(170, 167);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(110, 34);
			this.okButton.TabIndex = 3;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// numLabel
			// 
			this.numLabel.AutoSize = true;
			this.numLabel.Location = new System.Drawing.Point(28, 105);
			this.numLabel.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.numLabel.Name = "numLabel";
			this.numLabel.Size = new System.Drawing.Size(183, 20);
			this.numLabel.TabIndex = 4;
			this.numLabel.Text = "Number of cells to insert:";
			// 
			// numCellsBox
			// 
			this.numCellsBox.Location = new System.Drawing.Point(217, 103);
			this.numCellsBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numCellsBox.Name = "numCellsBox";
			this.numCellsBox.Size = new System.Drawing.Size(103, 26);
			this.numCellsBox.TabIndex = 5;
			this.numCellsBox.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// InsertCellsDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(424, 219);
			this.Controls.Add(this.numCellsBox);
			this.Controls.Add(this.numLabel);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.shiftRightRadio);
			this.Controls.Add(this.shiftDownRadio);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "InsertCellsDialog";
			this.Padding = new System.Windows.Forms.Padding(25, 25, 15, 15);
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Insert Cells";
			((System.ComponentModel.ISupportInitialize)(this.numCellsBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreRadioButton shiftDownRadio;
		private UI.MoreRadioButton shiftRightRadio;
		private UI.MoreButton cancelButton;
		private UI.MoreButton okButton;
		private System.Windows.Forms.Label numLabel;
		private System.Windows.Forms.NumericUpDown numCellsBox;
	}
}