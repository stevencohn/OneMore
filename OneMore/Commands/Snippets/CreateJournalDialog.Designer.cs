namespace River.OneMoreAddIn.Commands
{
	partial class CreateJournalDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateJournalDialog));
			this.titleLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.titleBox = new River.OneMoreAddIn.UI.MoreComboBox();
			this.yearLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.yearBox = new River.OneMoreAddIn.UI.MoreNumericUpDown();
			this.monthLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.monthBox = new River.OneMoreAddIn.UI.MoreComboBox();
			this.dayRangeLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.dayRangePanel = new System.Windows.Forms.Panel();
			this.weekdaysWeekendsRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.weekdaysOnlyRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.dateFormatLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.dateFormatBox = new River.OneMoreAddIn.UI.MoreComboBox();
			this.destinationLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.destinationPanel = new System.Windows.Forms.Panel();
			this.newSectionRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.currentSectionRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.sectionNameLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.sectionNameBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.hashtagsLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.hashtagsBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.errorLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.previewLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			((System.ComponentModel.ISupportInitialize)(this.yearBox)).BeginInit();
			this.dayRangePanel.SuspendLayout();
			this.destinationPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// titleLabel
			// 
			this.titleLabel.AutoSize = true;
			this.titleLabel.Location = new System.Drawing.Point(20, 20);
			this.titleLabel.Name = "titleLabel";
			this.titleLabel.Size = new System.Drawing.Size(38, 20);
			this.titleLabel.TabIndex = 0;
			this.titleLabel.Text = "Title";
			this.titleLabel.ThemedBack = null;
			this.titleLabel.ThemedFore = null;
			// 
			// titleBox
			// 
			this.titleBox.FormattingEnabled = true;
			this.titleBox.Location = new System.Drawing.Point(198, 17);
			this.titleBox.MaxLength = 50;
			this.titleBox.Name = "titleBox";
			this.titleBox.Size = new System.Drawing.Size(266, 27);
			this.titleBox.TabIndex = 1;
			this.titleBox.ThemedBack = null;
			this.titleBox.ThemedFore = null;
			this.titleBox.TextChanged += new System.EventHandler(this.ValidateFields);
			// 
			// yearLabel
			// 
			this.yearLabel.AutoSize = true;
			this.yearLabel.Location = new System.Drawing.Point(20, 60);
			this.yearLabel.Name = "yearLabel";
			this.yearLabel.Size = new System.Drawing.Size(43, 20);
			this.yearLabel.TabIndex = 2;
			this.yearLabel.Text = "Year";
			this.yearLabel.ThemedBack = null;
			this.yearLabel.ThemedFore = null;
			// 
			// yearBox
			// 
			this.yearBox.Location = new System.Drawing.Point(198, 57);
			this.yearBox.Maximum = new decimal(new int[] {
            2100,
            0,
            0,
            0});
			this.yearBox.Minimum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
			this.yearBox.Name = "yearBox";
			this.yearBox.Size = new System.Drawing.Size(120, 26);
			this.yearBox.TabIndex = 3;
			this.yearBox.ThemedBack = null;
			this.yearBox.ThemedFore = "ControlText";
			this.yearBox.Value = new decimal(new int[] {
            2000,
            0,
            0,
            0});
			this.yearBox.ValueChanged += new System.EventHandler(this.ValidateFields);
			// 
			// monthLabel
			// 
			this.monthLabel.AutoSize = true;
			this.monthLabel.Location = new System.Drawing.Point(20, 100);
			this.monthLabel.Name = "monthLabel";
			this.monthLabel.Size = new System.Drawing.Size(54, 20);
			this.monthLabel.TabIndex = 4;
			this.monthLabel.Text = "Month";
			this.monthLabel.ThemedBack = null;
			this.monthLabel.ThemedFore = null;
			// 
			// monthBox
			// 
			this.monthBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.monthBox.FormattingEnabled = true;
			this.monthBox.Location = new System.Drawing.Point(198, 97);
			this.monthBox.MaxDropDownItems = 12;
			this.monthBox.Name = "monthBox";
			this.monthBox.Size = new System.Drawing.Size(266, 27);
			this.monthBox.TabIndex = 5;
			this.monthBox.ThemedBack = null;
			this.monthBox.ThemedFore = null;
			this.monthBox.SelectedIndexChanged += new System.EventHandler(this.ValidateFields);
			// 
			// dayRangeLabel
			// 
			this.dayRangeLabel.AutoSize = true;
			this.dayRangeLabel.Location = new System.Drawing.Point(20, 140);
			this.dayRangeLabel.Name = "dayRangeLabel";
			this.dayRangeLabel.Size = new System.Drawing.Size(82, 20);
			this.dayRangeLabel.TabIndex = 6;
			this.dayRangeLabel.Text = "Day range";
			this.dayRangeLabel.ThemedBack = null;
			this.dayRangeLabel.ThemedFore = null;
			// 
			// dayRangePanel
			// 
			this.dayRangePanel.Controls.Add(this.weekdaysWeekendsRadio);
			this.dayRangePanel.Controls.Add(this.weekdaysOnlyRadio);
			this.dayRangePanel.Location = new System.Drawing.Point(198, 137);
			this.dayRangePanel.Name = "dayRangePanel";
			this.dayRangePanel.Size = new System.Drawing.Size(266, 58);
			this.dayRangePanel.TabIndex = 7;
			// 
			// weekdaysWeekendsRadio
			// 
			this.weekdaysWeekendsRadio.Checked = true;
			this.weekdaysWeekendsRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.weekdaysWeekendsRadio.Location = new System.Drawing.Point(3, 30);
			this.weekdaysWeekendsRadio.Name = "weekdaysWeekendsRadio";
			this.weekdaysWeekendsRadio.Size = new System.Drawing.Size(223, 25);
			this.weekdaysWeekendsRadio.TabIndex = 1;
			this.weekdaysWeekendsRadio.TabStop = true;
			this.weekdaysWeekendsRadio.Text = "Weekdays and weekends";
			this.weekdaysWeekendsRadio.UseVisualStyleBackColor = true;
			this.weekdaysWeekendsRadio.CheckedChanged += new System.EventHandler(this.ValidateFields);
			// 
			// weekdaysOnlyRadio
			// 
			this.weekdaysOnlyRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.weekdaysOnlyRadio.Location = new System.Drawing.Point(3, 3);
			this.weekdaysOnlyRadio.Name = "weekdaysOnlyRadio";
			this.weekdaysOnlyRadio.Size = new System.Drawing.Size(160, 25);
			this.weekdaysOnlyRadio.TabIndex = 0;
			this.weekdaysOnlyRadio.Text = "Weekdays only";
			this.weekdaysOnlyRadio.UseVisualStyleBackColor = true;
			this.weekdaysOnlyRadio.CheckedChanged += new System.EventHandler(this.ValidateFields);
			// 
			// dateFormatLabel
			// 
			this.dateFormatLabel.AutoSize = true;
			this.dateFormatLabel.Location = new System.Drawing.Point(20, 207);
			this.dateFormatLabel.Name = "dateFormatLabel";
			this.dateFormatLabel.Size = new System.Drawing.Size(94, 20);
			this.dateFormatLabel.TabIndex = 9;
			this.dateFormatLabel.Text = "Date format";
			this.dateFormatLabel.ThemedBack = null;
			this.dateFormatLabel.ThemedFore = null;
			// 
			// dateFormatBox
			// 
			this.dateFormatBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.dateFormatBox.FormattingEnabled = true;
			this.dateFormatBox.Location = new System.Drawing.Point(198, 204);
			this.dateFormatBox.Name = "dateFormatBox";
			this.dateFormatBox.Size = new System.Drawing.Size(266, 27);
			this.dateFormatBox.TabIndex = 10;
			this.dateFormatBox.ThemedBack = null;
			this.dateFormatBox.ThemedFore = null;
			this.dateFormatBox.SelectedIndexChanged += new System.EventHandler(this.ValidateFields);
			// 
			// destinationLabel
			// 
			this.destinationLabel.AutoSize = true;
			this.destinationLabel.Location = new System.Drawing.Point(20, 245);
			this.destinationLabel.Name = "destinationLabel";
			this.destinationLabel.Size = new System.Drawing.Size(90, 20);
			this.destinationLabel.TabIndex = 11;
			this.destinationLabel.Text = "Destination";
			this.destinationLabel.ThemedBack = null;
			this.destinationLabel.ThemedFore = null;
			// 
			// destinationPanel
			// 
			this.destinationPanel.Controls.Add(this.newSectionRadio);
			this.destinationPanel.Controls.Add(this.currentSectionRadio);
			this.destinationPanel.Location = new System.Drawing.Point(198, 242);
			this.destinationPanel.Name = "destinationPanel";
			this.destinationPanel.Size = new System.Drawing.Size(266, 58);
			this.destinationPanel.TabIndex = 12;
			// 
			// newSectionRadio
			// 
			this.newSectionRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.newSectionRadio.Location = new System.Drawing.Point(3, 30);
			this.newSectionRadio.Name = "newSectionRadio";
			this.newSectionRadio.Size = new System.Drawing.Size(160, 25);
			this.newSectionRadio.TabIndex = 1;
			this.newSectionRadio.Text = "New section";
			this.newSectionRadio.UseVisualStyleBackColor = true;
			this.newSectionRadio.CheckedChanged += new System.EventHandler(this.ChangeDestination);
			// 
			// currentSectionRadio
			// 
			this.currentSectionRadio.Checked = true;
			this.currentSectionRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.currentSectionRadio.Location = new System.Drawing.Point(3, 3);
			this.currentSectionRadio.Name = "currentSectionRadio";
			this.currentSectionRadio.Size = new System.Drawing.Size(160, 25);
			this.currentSectionRadio.TabIndex = 0;
			this.currentSectionRadio.TabStop = true;
			this.currentSectionRadio.Text = "Current section";
			this.currentSectionRadio.UseVisualStyleBackColor = true;
			this.currentSectionRadio.CheckedChanged += new System.EventHandler(this.ChangeDestination);
			// 
			// sectionNameLabel
			// 
			this.sectionNameLabel.AutoSize = true;
			this.sectionNameLabel.Location = new System.Drawing.Point(20, 316);
			this.sectionNameLabel.Name = "sectionNameLabel";
			this.sectionNameLabel.Size = new System.Drawing.Size(139, 20);
			this.sectionNameLabel.TabIndex = 14;
			this.sectionNameLabel.Text = "New section name";
			this.sectionNameLabel.ThemedBack = null;
			this.sectionNameLabel.ThemedFore = null;
			// 
			// sectionNameBox
			// 
			this.sectionNameBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.sectionNameBox.Enabled = false;
			this.sectionNameBox.Location = new System.Drawing.Point(198, 313);
			this.sectionNameBox.MaxLength = 50;
			this.sectionNameBox.Name = "sectionNameBox";
			this.sectionNameBox.ProcessEnterKey = false;
			this.sectionNameBox.Size = new System.Drawing.Size(333, 26);
			this.sectionNameBox.TabIndex = 15;
			this.sectionNameBox.ThemedBack = null;
			this.sectionNameBox.ThemedFore = null;
			this.sectionNameBox.TextChanged += new System.EventHandler(this.EditSectionName);
			// 
			// hashtagsLabel
			// 
			this.hashtagsLabel.AutoSize = true;
			this.hashtagsLabel.Location = new System.Drawing.Point(20, 356);
			this.hashtagsLabel.Name = "hashtagsLabel";
			this.hashtagsLabel.Size = new System.Drawing.Size(78, 20);
			this.hashtagsLabel.TabIndex = 16;
			this.hashtagsLabel.Text = "Hashtags";
			this.hashtagsLabel.ThemedBack = null;
			this.hashtagsLabel.ThemedFore = null;
			// 
			// hashtagsBox
			// 
			this.hashtagsBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.hashtagsBox.Location = new System.Drawing.Point(198, 353);
			this.hashtagsBox.Name = "hashtagsBox";
			this.hashtagsBox.ProcessEnterKey = false;
			this.hashtagsBox.Size = new System.Drawing.Size(333, 26);
			this.hashtagsBox.TabIndex = 17;
			this.hashtagsBox.ThemedBack = null;
			this.hashtagsBox.ThemedFore = null;
			// 
			// errorLabel
			// 
			this.errorLabel.AutoSize = true;
			this.errorLabel.Location = new System.Drawing.Point(20, 391);
			this.errorLabel.MaximumSize = new System.Drawing.Size(444, 0);
			this.errorLabel.Name = "errorLabel";
			this.errorLabel.Size = new System.Drawing.Size(0, 20);
			this.errorLabel.TabIndex = 18;
			this.errorLabel.ThemedBack = null;
			this.errorLabel.ThemedFore = "ErrorText";
			this.errorLabel.Visible = false;
			// 
			// previewLabel
			// 
			this.previewLabel.AutoSize = true;
			this.previewLabel.Location = new System.Drawing.Point(20, 421);
			this.previewLabel.MaximumSize = new System.Drawing.Size(444, 0);
			this.previewLabel.Name = "previewLabel";
			this.previewLabel.Size = new System.Drawing.Size(0, 20);
			this.previewLabel.TabIndex = 19;
			this.previewLabel.ThemedBack = null;
			this.previewLabel.ThemedFore = null;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Enabled = false;
			this.okButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(301, 472);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(112, 35);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 20;
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
			this.cancelButton.Location = new System.Drawing.Point(419, 472);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(112, 35);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 21;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = false;
			// 
			// CreateJournalDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(549, 525);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.previewLabel);
			this.Controls.Add(this.errorLabel);
			this.Controls.Add(this.hashtagsBox);
			this.Controls.Add(this.hashtagsLabel);
			this.Controls.Add(this.sectionNameBox);
			this.Controls.Add(this.sectionNameLabel);
			this.Controls.Add(this.destinationPanel);
			this.Controls.Add(this.destinationLabel);
			this.Controls.Add(this.dateFormatBox);
			this.Controls.Add(this.dateFormatLabel);
			this.Controls.Add(this.dayRangePanel);
			this.Controls.Add(this.dayRangeLabel);
			this.Controls.Add(this.monthBox);
			this.Controls.Add(this.monthLabel);
			this.Controls.Add(this.yearBox);
			this.Controls.Add(this.yearLabel);
			this.Controls.Add(this.titleBox);
			this.Controls.Add(this.titleLabel);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CreateJournalDialog";
			this.Padding = new System.Windows.Forms.Padding(15, 31, 15, 15);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Create Journal";
			((System.ComponentModel.ISupportInitialize)(this.yearBox)).EndInit();
			this.dayRangePanel.ResumeLayout(false);
			this.destinationPanel.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreLabel titleLabel;
		private UI.MoreComboBox titleBox;
		private UI.MoreLabel yearLabel;
		private UI.MoreNumericUpDown yearBox;
		private UI.MoreLabel monthLabel;
		private UI.MoreComboBox monthBox;
		private UI.MoreLabel dayRangeLabel;
		private System.Windows.Forms.Panel dayRangePanel;
		private UI.MoreRadioButton weekdaysOnlyRadio;
		private UI.MoreRadioButton weekdaysWeekendsRadio;
		private UI.MoreLabel dateFormatLabel;
		private UI.MoreComboBox dateFormatBox;
		private UI.MoreLabel destinationLabel;
		private System.Windows.Forms.Panel destinationPanel;
		private UI.MoreRadioButton currentSectionRadio;
		private UI.MoreRadioButton newSectionRadio;
		private UI.MoreLabel sectionNameLabel;
		private UI.MoreTextBox sectionNameBox;
		private UI.MoreLabel hashtagsLabel;
		private UI.MoreTextBox hashtagsBox;
		private UI.MoreLabel errorLabel;
		private UI.MoreLabel previewLabel;
		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
	}
}
