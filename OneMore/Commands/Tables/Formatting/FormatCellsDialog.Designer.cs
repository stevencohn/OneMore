namespace River.OneMoreAddIn.Commands
{
	partial class FormatCellsDialog
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
			this.scopeGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.selectedRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.columnsRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.categoryLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.categoryBox = new River.OneMoreAddIn.UI.MoreComboBox();
			this.detectedLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.decimalsLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.decimalsBox = new System.Windows.Forms.NumericUpDown();
			this.separatorBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.negativeLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.negativeBox = new River.OneMoreAddIn.UI.MoreComboBox();
			this.symbolLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.symbolBox = new River.OneMoreAddIn.UI.MoreComboBox();
			this.fractionRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.percentRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.patternLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.patternBox = new River.OneMoreAddIn.UI.MoreComboBox();
			this.previewLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.previewList = new River.OneMoreAddIn.UI.MoreListView();
			this.beforeColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.afterColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.scopeGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.decimalsBox)).BeginInit();
			this.SuspendLayout();
			//
			// scopeGroup
			//
			this.scopeGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.scopeGroup.Controls.Add(this.selectedRadio);
			this.scopeGroup.Controls.Add(this.columnsRadio);
			this.scopeGroup.Location = new System.Drawing.Point(30, 20);
			this.scopeGroup.Name = "scopeGroup";
			this.scopeGroup.Size = new System.Drawing.Size(840, 110);
			this.scopeGroup.TabIndex = 0;
			this.scopeGroup.TabStop = false;
			this.scopeGroup.Text = "Apply to";
			//
			// selectedRadio
			//
			this.selectedRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.selectedRadio.Location = new System.Drawing.Point(20, 30);
			this.selectedRadio.Name = "selectedRadio";
			this.selectedRadio.Size = new System.Drawing.Size(800, 30);
			this.selectedRadio.TabIndex = 0;
			this.selectedRadio.TabStop = true;
			this.selectedRadio.Text = "Selected cells";
			this.selectedRadio.UseVisualStyleBackColor = true;
			this.selectedRadio.CheckedChanged += new System.EventHandler(this.ChangeScope);
			//
			// columnsRadio
			//
			this.columnsRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.columnsRadio.Location = new System.Drawing.Point(20, 68);
			this.columnsRadio.Name = "columnsRadio";
			this.columnsRadio.Size = new System.Drawing.Size(800, 30);
			this.columnsRadio.TabIndex = 1;
			this.columnsRadio.TabStop = true;
			this.columnsRadio.Text = "Entire columns";
			this.columnsRadio.UseVisualStyleBackColor = true;
			this.columnsRadio.CheckedChanged += new System.EventHandler(this.ChangeScope);
			//
			// categoryLabel
			//
			this.categoryLabel.AutoSize = true;
			this.categoryLabel.Location = new System.Drawing.Point(30, 148);
			this.categoryLabel.Name = "categoryLabel";
			this.categoryLabel.Size = new System.Drawing.Size(76, 20);
			this.categoryLabel.TabIndex = 1;
			this.categoryLabel.Text = "Category";
			this.categoryLabel.ThemedBack = null;
			this.categoryLabel.ThemedFore = null;
			//
			// categoryBox
			//
			this.categoryBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.categoryBox.FormattingEnabled = true;
			this.categoryBox.Location = new System.Drawing.Point(30, 178);
			this.categoryBox.Name = "categoryBox";
			this.categoryBox.Size = new System.Drawing.Size(240, 28);
			this.categoryBox.TabIndex = 2;
			this.categoryBox.SelectedIndexChanged += new System.EventHandler(this.ChangeCategory);
			//
			// detectedLabel
			//
			this.detectedLabel.AutoSize = true;
			this.detectedLabel.Location = new System.Drawing.Point(30, 214);
			this.detectedLabel.Name = "detectedLabel";
			this.detectedLabel.Size = new System.Drawing.Size(100, 20);
			this.detectedLabel.TabIndex = 18;
			this.detectedLabel.Text = "Detected";
			this.detectedLabel.ThemedBack = null;
			this.detectedLabel.ThemedFore = "GrayText";
			this.detectedLabel.Visible = false;
			//
			// decimalsLabel
			//
			this.decimalsLabel.AutoSize = true;
			this.decimalsLabel.Location = new System.Drawing.Point(300, 148);
			this.decimalsLabel.Name = "decimalsLabel";
			this.decimalsLabel.Size = new System.Drawing.Size(113, 20);
			this.decimalsLabel.TabIndex = 3;
			this.decimalsLabel.Scrubber = this.decimalsBox;
			this.decimalsLabel.Text = "Decimal places";
			this.decimalsLabel.ThemedBack = null;
			this.decimalsLabel.ThemedFore = null;
			//
			// decimalsBox
			//
			this.decimalsBox.Location = new System.Drawing.Point(300, 178);
			this.decimalsBox.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
			this.decimalsBox.Name = "decimalsBox";
			this.decimalsBox.Size = new System.Drawing.Size(90, 27);
			this.decimalsBox.TabIndex = 4;
			this.decimalsBox.Value = new decimal(new int[] { 2, 0, 0, 0 });
			this.decimalsBox.ValueChanged += new System.EventHandler(this.ChangeOption);
			//
			// separatorBox
			//
			this.separatorBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.separatorBox.Location = new System.Drawing.Point(430, 178);
			this.separatorBox.Name = "separatorBox";
			this.separatorBox.Size = new System.Drawing.Size(400, 30);
			this.separatorBox.TabIndex = 5;
			this.separatorBox.Text = "Use 1000 separator";
			this.separatorBox.UseVisualStyleBackColor = true;
			this.separatorBox.CheckedChanged += new System.EventHandler(this.ChangeOption);
			//
			// negativeLabel
			//
			this.negativeLabel.AutoSize = true;
			this.negativeLabel.Location = new System.Drawing.Point(300, 222);
			this.negativeLabel.Name = "negativeLabel";
			this.negativeLabel.Size = new System.Drawing.Size(120, 20);
			this.negativeLabel.TabIndex = 6;
			this.negativeLabel.Text = "Negative numbers";
			this.negativeLabel.ThemedBack = null;
			this.negativeLabel.ThemedFore = null;
			//
			// negativeBox
			//
			this.negativeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.negativeBox.FormattingEnabled = true;
			this.negativeBox.Location = new System.Drawing.Point(300, 252);
			this.negativeBox.Name = "negativeBox";
			this.negativeBox.Size = new System.Drawing.Size(200, 28);
			this.negativeBox.TabIndex = 7;
			this.negativeBox.SelectedIndexChanged += new System.EventHandler(this.ChangeOption);
			//
			// symbolLabel
			//
			this.symbolLabel.AutoSize = true;
			this.symbolLabel.Location = new System.Drawing.Point(540, 222);
			this.symbolLabel.Name = "symbolLabel";
			this.symbolLabel.Size = new System.Drawing.Size(61, 20);
			this.symbolLabel.TabIndex = 8;
			this.symbolLabel.Text = "Symbol";
			this.symbolLabel.ThemedBack = null;
			this.symbolLabel.ThemedFore = null;
			//
			// symbolBox
			//
			this.symbolBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.symbolBox.FormattingEnabled = true;
			this.symbolBox.Location = new System.Drawing.Point(540, 252);
			this.symbolBox.Name = "symbolBox";
			this.symbolBox.Size = new System.Drawing.Size(200, 28);
			this.symbolBox.TabIndex = 9;
			this.symbolBox.SelectedIndexChanged += new System.EventHandler(this.ChangeOption);
			//
			// fractionRadio
			//
			this.fractionRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.fractionRadio.Location = new System.Drawing.Point(300, 218);
			this.fractionRadio.Name = "fractionRadio";
			this.fractionRadio.Size = new System.Drawing.Size(570, 30);
			this.fractionRadio.TabIndex = 10;
			this.fractionRadio.TabStop = true;
			this.fractionRadio.Text = "Value is a fraction (0.25 becomes 25%)";
			this.fractionRadio.UseVisualStyleBackColor = true;
			this.fractionRadio.CheckedChanged += new System.EventHandler(this.ChangeOption);
			//
			// percentRadio
			//
			this.percentRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.percentRadio.Location = new System.Drawing.Point(300, 254);
			this.percentRadio.Name = "percentRadio";
			this.percentRadio.Size = new System.Drawing.Size(570, 30);
			this.percentRadio.TabIndex = 11;
			this.percentRadio.TabStop = true;
			this.percentRadio.Text = "Value is already a percent (25 becomes 25%)";
			this.percentRadio.UseVisualStyleBackColor = true;
			this.percentRadio.CheckedChanged += new System.EventHandler(this.ChangeOption);
			//
			// patternLabel
			//
			this.patternLabel.AutoSize = true;
			this.patternLabel.Location = new System.Drawing.Point(300, 148);
			this.patternLabel.Name = "patternLabel";
			this.patternLabel.Size = new System.Drawing.Size(48, 20);
			this.patternLabel.TabIndex = 12;
			this.patternLabel.Text = "Type";
			this.patternLabel.ThemedBack = null;
			this.patternLabel.ThemedFore = null;
			//
			// patternBox
			//
			this.patternBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.patternBox.FormattingEnabled = true;
			this.patternBox.Location = new System.Drawing.Point(300, 178);
			this.patternBox.Name = "patternBox";
			this.patternBox.Size = new System.Drawing.Size(340, 28);
			this.patternBox.TabIndex = 13;
			this.patternBox.SelectedIndexChanged += new System.EventHandler(this.ChangeOption);
			//
			// previewLabel
			//
			this.previewLabel.AutoSize = true;
			this.previewLabel.Location = new System.Drawing.Point(30, 310);
			this.previewLabel.Name = "previewLabel";
			this.previewLabel.Size = new System.Drawing.Size(66, 20);
			this.previewLabel.TabIndex = 14;
			this.previewLabel.Text = "Preview";
			this.previewLabel.ThemedBack = null;
			this.previewLabel.ThemedFore = null;
			//
			// previewList
			//
			this.previewList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.previewList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.beforeColumn,
            this.afterColumn});
			this.previewList.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.previewList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.previewList.Location = new System.Drawing.Point(30, 340);
			this.previewList.MultiSelect = false;
			this.previewList.Name = "previewList";
			this.previewList.Size = new System.Drawing.Size(840, 200);
			this.previewList.TabIndex = 15;
			this.previewList.View = System.Windows.Forms.View.Details;
			//
			// beforeColumn
			//
			this.beforeColumn.Text = "Before";
			this.beforeColumn.Width = 400;
			//
			// afterColumn
			//
			this.afterColumn.Text = "After";
			this.afterColumn.Width = 400;
			//
			// okButton
			//
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(630, 562);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(116, 38);
			this.okButton.TabIndex = 16;
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
			this.cancelButton.Location = new System.Drawing.Point(754, 562);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(116, 38);
			this.cancelButton.TabIndex = 17;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			//
			// FormatCellsDialog
			//
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(900, 620);
			this.Controls.Add(this.scopeGroup);
			this.Controls.Add(this.categoryLabel);
			this.Controls.Add(this.categoryBox);
			this.Controls.Add(this.detectedLabel);
			this.Controls.Add(this.decimalsLabel);
			this.Controls.Add(this.decimalsBox);
			this.Controls.Add(this.separatorBox);
			this.Controls.Add(this.negativeLabel);
			this.Controls.Add(this.negativeBox);
			this.Controls.Add(this.symbolLabel);
			this.Controls.Add(this.symbolBox);
			this.Controls.Add(this.fractionRadio);
			this.Controls.Add(this.percentRadio);
			this.Controls.Add(this.patternLabel);
			this.Controls.Add(this.patternBox);
			this.Controls.Add(this.previewLabel);
			this.Controls.Add(this.previewList);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "FormatCellsDialog";
			this.Padding = new System.Windows.Forms.Padding(20, 20, 10, 20);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Format Cells";
			this.scopeGroup.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.decimalsBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreGroupBox scopeGroup;
		private UI.MoreRadioButton selectedRadio;
		private UI.MoreRadioButton columnsRadio;
		private UI.MoreLabel categoryLabel;
		private UI.MoreComboBox categoryBox;
		private UI.MoreLabel detectedLabel;
		private UI.MoreLabel decimalsLabel;
		private System.Windows.Forms.NumericUpDown decimalsBox;
		private UI.MoreCheckBox separatorBox;
		private UI.MoreLabel negativeLabel;
		private UI.MoreComboBox negativeBox;
		private UI.MoreLabel symbolLabel;
		private UI.MoreComboBox symbolBox;
		private UI.MoreRadioButton fractionRadio;
		private UI.MoreRadioButton percentRadio;
		private UI.MoreLabel patternLabel;
		private UI.MoreComboBox patternBox;
		private UI.MoreLabel previewLabel;
		private UI.MoreListView previewList;
		private System.Windows.Forms.ColumnHeader beforeColumn;
		private System.Windows.Forms.ColumnHeader afterColumn;
		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
	}
}
