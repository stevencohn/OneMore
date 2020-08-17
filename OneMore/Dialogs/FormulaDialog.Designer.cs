namespace River.OneMoreAddIn
{
	partial class FormulaDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormulaDialog));
			this.rangeLabel = new System.Windows.Forms.Label();
			this.formatLabel = new System.Windows.Forms.Label();
			this.formatBox = new System.Windows.Forms.ComboBox();
			this.functionLabel = new System.Windows.Forms.Label();
			this.functionBox = new System.Windows.Forms.ListBox();
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.colButton = new System.Windows.Forms.RadioButton();
			this.rowButton = new System.Windows.Forms.RadioButton();
			this.SuspendLayout();
			// 
			// rangeLabel
			// 
			this.rangeLabel.AutoSize = true;
			this.rangeLabel.Location = new System.Drawing.Point(22, 24);
			this.rangeLabel.Name = "rangeLabel";
			this.rangeLabel.Size = new System.Drawing.Size(61, 20);
			this.rangeLabel.TabIndex = 0;
			this.rangeLabel.Text = "Range:";
			// 
			// formatLabel
			// 
			this.formatLabel.AutoSize = true;
			this.formatLabel.Location = new System.Drawing.Point(22, 98);
			this.formatLabel.Name = "formatLabel";
			this.formatLabel.Size = new System.Drawing.Size(64, 20);
			this.formatLabel.TabIndex = 2;
			this.formatLabel.Text = "Format:";
			// 
			// formatBox
			// 
			this.formatBox.FormattingEnabled = true;
			this.formatBox.Items.AddRange(new object[] {
            "Number",
            "Currency",
            "Percentage"});
			this.formatBox.Location = new System.Drawing.Point(103, 95);
			this.formatBox.Name = "formatBox";
			this.formatBox.Size = new System.Drawing.Size(261, 28);
			this.formatBox.TabIndex = 3;
			// 
			// functionLabel
			// 
			this.functionLabel.AutoSize = true;
			this.functionLabel.Location = new System.Drawing.Point(22, 142);
			this.functionLabel.Name = "functionLabel";
			this.functionLabel.Size = new System.Drawing.Size(75, 20);
			this.functionLabel.TabIndex = 4;
			this.functionLabel.Text = "Function:";
			// 
			// functionBox
			// 
			this.functionBox.FormattingEnabled = true;
			this.functionBox.ItemHeight = 20;
			this.functionBox.Items.AddRange(new object[] {
            "Sum",
            "Average",
            "Min",
            "Max",
            "Range",
            "Median",
            "Mode",
            "Variance",
            "Standard Deviation"});
			this.functionBox.Location = new System.Drawing.Point(103, 142);
			this.functionBox.Name = "functionBox";
			this.functionBox.Size = new System.Drawing.Size(261, 184);
			this.functionBox.TabIndex = 5;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(269, 358);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.TabIndex = 6;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.Cancel);
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(163, 358);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.TabIndex = 7;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OK);
			// 
			// colButton
			// 
			this.colButton.AutoSize = true;
			this.colButton.Checked = true;
			this.colButton.Location = new System.Drawing.Point(103, 22);
			this.colButton.Name = "colButton";
			this.colButton.Size = new System.Drawing.Size(170, 24);
			this.colButton.TabIndex = 8;
			this.colButton.TabStop = true;
			this.colButton.Text = "Column cells above";
			this.colButton.UseVisualStyleBackColor = true;
			// 
			// rowButton
			// 
			this.rowButton.AutoSize = true;
			this.rowButton.Location = new System.Drawing.Point(103, 52);
			this.rowButton.Name = "rowButton";
			this.rowButton.Size = new System.Drawing.Size(172, 24);
			this.rowButton.TabIndex = 9;
			this.rowButton.Text = "Row cells to the left";
			this.rowButton.UseVisualStyleBackColor = true;
			// 
			// FormulaDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(382, 409);
			this.Controls.Add(this.rowButton);
			this.Controls.Add(this.colButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.functionBox);
			this.Controls.Add(this.functionLabel);
			this.Controls.Add(this.formatBox);
			this.Controls.Add(this.formatLabel);
			this.Controls.Add(this.rangeLabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormulaDialog";
			this.Padding = new System.Windows.Forms.Padding(10, 15, 10, 10);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Formula";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label rangeLabel;
		private System.Windows.Forms.Label formatLabel;
		private System.Windows.Forms.ComboBox formatBox;
		private System.Windows.Forms.Label functionLabel;
		private System.Windows.Forms.ListBox functionBox;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.RadioButton colButton;
		private System.Windows.Forms.RadioButton rowButton;
	}
}