namespace River.OneMoreAddIn.Commands
{
	partial class InsertCalendarDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InsertCalendarDialog));
			this.yearLabel = new System.Windows.Forms.Label();
			this.yearBox = new System.Windows.Forms.NumericUpDown();
			this.monthLabel = new System.Windows.Forms.Label();
			this.formatLabel = new System.Windows.Forms.Label();
			this.monthBox = new System.Windows.Forms.ComboBox();
			this.smallRadio = new System.Windows.Forms.RadioButton();
			this.largeRadio = new System.Windows.Forms.RadioButton();
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.indentBox = new System.Windows.Forms.CheckBox();
			this.firstLabel = new System.Windows.Forms.Label();
			this.sundayButton = new System.Windows.Forms.RadioButton();
			this.mondayButton = new System.Windows.Forms.RadioButton();
			this.firstPanel = new System.Windows.Forms.Panel();
			this.formatPanel = new System.Windows.Forms.Panel();
			this.clickLabel = new System.Windows.Forms.Label();
			this.shadingBox = new River.OneMoreAddIn.UI.MorePictureBox();
			this.colorLabel = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.yearBox)).BeginInit();
			this.firstPanel.SuspendLayout();
			this.formatPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.shadingBox)).BeginInit();
			this.SuspendLayout();
			// 
			// yearLabel
			// 
			this.yearLabel.AutoSize = true;
			this.yearLabel.Location = new System.Drawing.Point(20, 38);
			this.yearLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.yearLabel.Name = "yearLabel";
			this.yearLabel.Size = new System.Drawing.Size(47, 20);
			this.yearLabel.TabIndex = 0;
			this.yearLabel.Text = "Year:";
			// 
			// yearBox
			// 
			this.yearBox.Location = new System.Drawing.Point(198, 36);
			this.yearBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.yearBox.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.yearBox.Name = "yearBox";
			this.yearBox.Size = new System.Drawing.Size(266, 26);
			this.yearBox.TabIndex = 1;
			// 
			// monthLabel
			// 
			this.monthLabel.AutoSize = true;
			this.monthLabel.Location = new System.Drawing.Point(20, 80);
			this.monthLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.monthLabel.Name = "monthLabel";
			this.monthLabel.Size = new System.Drawing.Size(58, 20);
			this.monthLabel.TabIndex = 2;
			this.monthLabel.Text = "Month:";
			// 
			// formatLabel
			// 
			this.formatLabel.AutoSize = true;
			this.formatLabel.Location = new System.Drawing.Point(4, 63);
			this.formatLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.formatLabel.Name = "formatLabel";
			this.formatLabel.Size = new System.Drawing.Size(64, 20);
			this.formatLabel.TabIndex = 3;
			this.formatLabel.Text = "Format:";
			// 
			// monthBox
			// 
			this.monthBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.monthBox.FormattingEnabled = true;
			this.monthBox.Location = new System.Drawing.Point(198, 77);
			this.monthBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.monthBox.MaxDropDownItems = 12;
			this.monthBox.Name = "monthBox";
			this.monthBox.Size = new System.Drawing.Size(265, 28);
			this.monthBox.TabIndex = 4;
			// 
			// smallRadio
			// 
			this.smallRadio.AutoSize = true;
			this.smallRadio.Location = new System.Drawing.Point(184, 91);
			this.smallRadio.Name = "smallRadio";
			this.smallRadio.Size = new System.Drawing.Size(73, 24);
			this.smallRadio.TabIndex = 5;
			this.smallRadio.Text = "Small";
			this.smallRadio.UseVisualStyleBackColor = true;
			// 
			// largeRadio
			// 
			this.largeRadio.AutoSize = true;
			this.largeRadio.Checked = true;
			this.largeRadio.Location = new System.Drawing.Point(182, 61);
			this.largeRadio.Name = "largeRadio";
			this.largeRadio.Size = new System.Drawing.Size(75, 24);
			this.largeRadio.TabIndex = 6;
			this.largeRadio.TabStop = true;
			this.largeRadio.Text = "Large";
			this.largeRadio.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(376, 388);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(112, 35);
			this.cancelButton.TabIndex = 7;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(254, 388);
			this.okButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(112, 35);
			this.okButton.TabIndex = 8;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// indentBox
			// 
			this.indentBox.AutoSize = true;
			this.indentBox.Location = new System.Drawing.Point(184, 141);
			this.indentBox.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
			this.indentBox.Name = "indentBox";
			this.indentBox.Size = new System.Drawing.Size(146, 24);
			this.indentBox.TabIndex = 9;
			this.indentBox.Text = "Indent calendar";
			this.indentBox.UseVisualStyleBackColor = true;
			// 
			// firstLabel
			// 
			this.firstLabel.AutoSize = true;
			this.firstLabel.Location = new System.Drawing.Point(4, 10);
			this.firstLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.firstLabel.Name = "firstLabel";
			this.firstLabel.Size = new System.Drawing.Size(132, 20);
			this.firstLabel.TabIndex = 10;
			this.firstLabel.Text = "First day of week:";
			// 
			// sundayButton
			// 
			this.sundayButton.AutoSize = true;
			this.sundayButton.Checked = true;
			this.sundayButton.Location = new System.Drawing.Point(182, 8);
			this.sundayButton.Name = "sundayButton";
			this.sundayButton.Size = new System.Drawing.Size(88, 24);
			this.sundayButton.TabIndex = 11;
			this.sundayButton.TabStop = true;
			this.sundayButton.Text = "Sunday";
			this.sundayButton.UseVisualStyleBackColor = true;
			// 
			// mondayButton
			// 
			this.mondayButton.AutoSize = true;
			this.mondayButton.Location = new System.Drawing.Point(182, 38);
			this.mondayButton.Name = "mondayButton";
			this.mondayButton.Size = new System.Drawing.Size(90, 24);
			this.mondayButton.TabIndex = 12;
			this.mondayButton.Text = "Monday";
			this.mondayButton.UseVisualStyleBackColor = true;
			// 
			// firstPanel
			// 
			this.firstPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.firstPanel.Controls.Add(this.firstLabel);
			this.firstPanel.Controls.Add(this.mondayButton);
			this.firstPanel.Controls.Add(this.sundayButton);
			this.firstPanel.Location = new System.Drawing.Point(16, 122);
			this.firstPanel.Name = "firstPanel";
			this.firstPanel.Size = new System.Drawing.Size(474, 66);
			this.firstPanel.TabIndex = 13;
			// 
			// formatPanel
			// 
			this.formatPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.formatPanel.Controls.Add(this.clickLabel);
			this.formatPanel.Controls.Add(this.shadingBox);
			this.formatPanel.Controls.Add(this.colorLabel);
			this.formatPanel.Controls.Add(this.formatLabel);
			this.formatPanel.Controls.Add(this.smallRadio);
			this.formatPanel.Controls.Add(this.indentBox);
			this.formatPanel.Controls.Add(this.largeRadio);
			this.formatPanel.Location = new System.Drawing.Point(16, 194);
			this.formatPanel.Name = "formatPanel";
			this.formatPanel.Size = new System.Drawing.Size(472, 173);
			this.formatPanel.TabIndex = 14;
			// 
			// clickLabel
			// 
			this.clickLabel.AutoSize = true;
			this.clickLabel.Location = new System.Drawing.Point(310, 13);
			this.clickLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.clickLabel.Name = "clickLabel";
			this.clickLabel.Size = new System.Drawing.Size(114, 20);
			this.clickLabel.TabIndex = 12;
			this.clickLabel.Text = "click to change";
			// 
			// shadingBox
			// 
			this.shadingBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(235)))), ((int)(((byte)(246)))));
			this.shadingBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.shadingBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.shadingBox.Location = new System.Drawing.Point(184, 13);
			this.shadingBox.Name = "shadingBox";
			this.shadingBox.Size = new System.Drawing.Size(119, 24);
			this.shadingBox.TabIndex = 11;
			this.shadingBox.TabStop = false;
			this.shadingBox.Click += new System.EventHandler(this.ChangeHeadingColor);
			// 
			// colorLabel
			// 
			this.colorLabel.AutoSize = true;
			this.colorLabel.Location = new System.Drawing.Point(4, 13);
			this.colorLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.colorLabel.Name = "colorLabel";
			this.colorLabel.Size = new System.Drawing.Size(111, 20);
			this.colorLabel.TabIndex = 10;
			this.colorLabel.Text = "Heading color:";
			// 
			// InsertCalendarDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(508, 443);
			this.Controls.Add(this.formatPanel);
			this.Controls.Add(this.firstPanel);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.monthBox);
			this.Controls.Add(this.monthLabel);
			this.Controls.Add(this.yearBox);
			this.Controls.Add(this.yearLabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "InsertCalendarDialog";
			this.Padding = new System.Windows.Forms.Padding(15, 31, 15, 15);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Insert Calendar";
			((System.ComponentModel.ISupportInitialize)(this.yearBox)).EndInit();
			this.firstPanel.ResumeLayout(false);
			this.firstPanel.PerformLayout();
			this.formatPanel.ResumeLayout(false);
			this.formatPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.shadingBox)).EndInit();
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
		private System.Windows.Forms.CheckBox indentBox;
		private System.Windows.Forms.Label firstLabel;
		private System.Windows.Forms.RadioButton sundayButton;
		private System.Windows.Forms.RadioButton mondayButton;
		private System.Windows.Forms.Panel firstPanel;
		private System.Windows.Forms.Panel formatPanel;
		private System.Windows.Forms.Label colorLabel;
		private UI.MorePictureBox shadingBox;
		private System.Windows.Forms.Label clickLabel;
	}
}