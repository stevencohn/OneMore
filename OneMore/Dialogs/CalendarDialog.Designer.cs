namespace River.OneMoreAddIn.Dialogs
{
	partial class CalendarDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CalendarDialog));
			this.yearLabel = new System.Windows.Forms.Label();
			this.yearBox = new System.Windows.Forms.NumericUpDown();
			this.monthLabel = new System.Windows.Forms.Label();
			this.formatLabel = new System.Windows.Forms.Label();
			this.monthBox = new System.Windows.Forms.ComboBox();
			this.smallRadio = new System.Windows.Forms.RadioButton();
			this.largeRadio = new System.Windows.Forms.RadioButton();
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.yearBox)).BeginInit();
			this.SuspendLayout();
			// 
			// yearLabel
			// 
			this.yearLabel.AutoSize = true;
			this.yearLabel.Location = new System.Drawing.Point(13, 25);
			this.yearLabel.Name = "yearLabel";
			this.yearLabel.Size = new System.Drawing.Size(32, 13);
			this.yearLabel.TabIndex = 0;
			this.yearLabel.Text = "Year:";
			// 
			// yearBox
			// 
			this.yearBox.Location = new System.Drawing.Point(91, 23);
			this.yearBox.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.yearBox.Name = "yearBox";
			this.yearBox.Size = new System.Drawing.Size(177, 20);
			this.yearBox.TabIndex = 1;
			// 
			// monthLabel
			// 
			this.monthLabel.AutoSize = true;
			this.monthLabel.Location = new System.Drawing.Point(13, 52);
			this.monthLabel.Name = "monthLabel";
			this.monthLabel.Size = new System.Drawing.Size(40, 13);
			this.monthLabel.TabIndex = 2;
			this.monthLabel.Text = "Month:";
			// 
			// formatLabel
			// 
			this.formatLabel.AutoSize = true;
			this.formatLabel.Location = new System.Drawing.Point(13, 78);
			this.formatLabel.Name = "formatLabel";
			this.formatLabel.Size = new System.Drawing.Size(42, 13);
			this.formatLabel.TabIndex = 3;
			this.formatLabel.Text = "Format:";
			// 
			// monthBox
			// 
			this.monthBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.monthBox.FormattingEnabled = true;
			this.monthBox.Location = new System.Drawing.Point(91, 49);
			this.monthBox.MaxDropDownItems = 12;
			this.monthBox.Name = "monthBox";
			this.monthBox.Size = new System.Drawing.Size(178, 21);
			this.monthBox.TabIndex = 4;
			// 
			// smallRadio
			// 
			this.smallRadio.AutoSize = true;
			this.smallRadio.Location = new System.Drawing.Point(91, 99);
			this.smallRadio.Name = "smallRadio";
			this.smallRadio.Size = new System.Drawing.Size(50, 17);
			this.smallRadio.TabIndex = 5;
			this.smallRadio.Text = "Small";
			this.smallRadio.UseVisualStyleBackColor = true;
			// 
			// largeRadio
			// 
			this.largeRadio.AutoSize = true;
			this.largeRadio.Checked = true;
			this.largeRadio.Location = new System.Drawing.Point(91, 76);
			this.largeRadio.Name = "largeRadio";
			this.largeRadio.Size = new System.Drawing.Size(52, 17);
			this.largeRadio.TabIndex = 6;
			this.largeRadio.TabStop = true;
			this.largeRadio.Text = "Large";
			this.largeRadio.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(204, 139);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 23);
			this.cancelButton.TabIndex = 7;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(123, 139);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 8;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// CalendarDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(292, 175);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.largeRadio);
			this.Controls.Add(this.smallRadio);
			this.Controls.Add(this.monthBox);
			this.Controls.Add(this.formatLabel);
			this.Controls.Add(this.monthLabel);
			this.Controls.Add(this.yearBox);
			this.Controls.Add(this.yearLabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.ShowInTaskbar = false;
			this.Name = "CalendarDialog";
			this.Padding = new System.Windows.Forms.Padding(10, 20, 10, 10);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Insert Calendar";
			((System.ComponentModel.ISupportInitialize)(this.yearBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label yearLabel;
		private System.Windows.Forms.NumericUpDown yearBox;
		private System.Windows.Forms.Label monthLabel;
		private System.Windows.Forms.Label formatLabel;
		private System.Windows.Forms.ComboBox monthBox;
		private System.Windows.Forms.RadioButton smallRadio;
		private System.Windows.Forms.RadioButton largeRadio;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
	}
}