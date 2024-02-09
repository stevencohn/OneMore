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
			this.smallRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.largeRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.indentBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.firstLabel = new System.Windows.Forms.Label();
			this.sundayButton = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.mondayButton = new River.OneMoreAddIn.UI.MoreRadioButton();
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
			this.yearLabel.Size = new System.Drawing.Size(43, 20);
			this.yearLabel.TabIndex = 0;
			this.yearLabel.Text = "Year";
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
			this.yearBox.TabIndex = 2;
			// 
			// monthLabel
			// 
			this.monthLabel.AutoSize = true;
			this.monthLabel.Location = new System.Drawing.Point(20, 80);
			this.monthLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.monthLabel.Name = "monthLabel";
			this.monthLabel.Size = new System.Drawing.Size(54, 20);
			this.monthLabel.TabIndex = 2;
			this.monthLabel.Text = "Month";
			// 
			// formatLabel
			// 
			this.formatLabel.AutoSize = true;
			this.formatLabel.Location = new System.Drawing.Point(4, 63);
			this.formatLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.formatLabel.Name = "formatLabel";
			this.formatLabel.Size = new System.Drawing.Size(60, 20);
			this.formatLabel.TabIndex = 3;
			this.formatLabel.Text = "Format";
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
			this.smallRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.smallRadio.Location = new System.Drawing.Point(182, 92);
			this.smallRadio.Name = "smallRadio";
			this.smallRadio.Size = new System.Drawing.Size(77, 25);
			this.smallRadio.TabIndex = 1;
			this.smallRadio.Text = "Small";
			this.smallRadio.UseVisualStyleBackColor = true;
			// 
			// largeRadio
			// 
			this.largeRadio.Checked = true;
			this.largeRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.largeRadio.Location = new System.Drawing.Point(182, 58);
			this.largeRadio.Name = "largeRadio";
			this.largeRadio.Size = new System.Drawing.Size(78, 25);
			this.largeRadio.TabIndex = 0;
			this.largeRadio.TabStop = true;
			this.largeRadio.Text = "Large";
			this.largeRadio.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(376, 392);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(112, 35);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = false;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(254, 392);
			this.okButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(112, 35);
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = false;
			// 
			// indentBox
			// 
			this.indentBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.indentBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.indentBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.indentBox.Location = new System.Drawing.Point(182, 136);
			this.indentBox.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
			this.indentBox.Name = "indentBox";
			this.indentBox.Size = new System.Drawing.Size(149, 25);
			this.indentBox.TabIndex = 2;
			this.indentBox.Text = "Indent calendar";
			this.indentBox.UseVisualStyleBackColor = false;
			// 
			// firstLabel
			// 
			this.firstLabel.AutoSize = true;
			this.firstLabel.Location = new System.Drawing.Point(4, 10);
			this.firstLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.firstLabel.Name = "firstLabel";
			this.firstLabel.Size = new System.Drawing.Size(128, 20);
			this.firstLabel.TabIndex = 10;
			this.firstLabel.Text = "First day of week";
			// 
			// sundayButton
			// 
			this.sundayButton.Checked = true;
			this.sundayButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.sundayButton.Location = new System.Drawing.Point(182, 8);
			this.sundayButton.Name = "sundayButton";
			this.sundayButton.Size = new System.Drawing.Size(91, 25);
			this.sundayButton.TabIndex = 0;
			this.sundayButton.TabStop = true;
			this.sundayButton.Text = "Sunday";
			this.sundayButton.UseVisualStyleBackColor = true;
			// 
			// mondayButton
			// 
			this.mondayButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.mondayButton.Location = new System.Drawing.Point(182, 40);
			this.mondayButton.Name = "mondayButton";
			this.mondayButton.Size = new System.Drawing.Size(94, 25);
			this.mondayButton.TabIndex = 1;
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
			this.firstPanel.Size = new System.Drawing.Size(474, 74);
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
			this.formatPanel.Location = new System.Drawing.Point(16, 202);
			this.formatPanel.Name = "formatPanel";
			this.formatPanel.Size = new System.Drawing.Size(472, 173);
			this.formatPanel.TabIndex = 3;
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
			this.shadingBox.BackColor = System.Drawing.Color.AliceBlue;
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
			this.colorLabel.Size = new System.Drawing.Size(107, 20);
			this.colorLabel.TabIndex = 10;
			this.colorLabel.Text = "Heading color";
			// 
			// InsertCalendarDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(508, 447);
			this.Controls.Add(this.formatPanel);
			this.Controls.Add(this.firstPanel);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.monthBox);
			this.Controls.Add(this.monthLabel);
			this.Controls.Add(this.yearBox);
			this.Controls.Add(this.yearLabel);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "InsertCalendarDialog";
			this.Padding = new System.Windows.Forms.Padding(15, 31, 15, 15);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
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
		private UI.MoreRadioButton smallRadio;
		private UI.MoreRadioButton largeRadio;
		private UI.MoreButton cancelButton;
		private UI.MoreButton okButton;
		private UI.MoreCheckBox indentBox;
		private System.Windows.Forms.Label firstLabel;
		private UI.MoreRadioButton sundayButton;
		private UI.MoreRadioButton mondayButton;
		private System.Windows.Forms.Panel firstPanel;
		private System.Windows.Forms.Panel formatPanel;
		private System.Windows.Forms.Label colorLabel;
		private UI.MorePictureBox shadingBox;
		private System.Windows.Forms.Label clickLabel;
	}
}