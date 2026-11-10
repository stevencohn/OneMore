namespace River.OneMoreAddIn.Commands
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormulaDialog));
			this.targetLabel = new River.OneMoreAddIn.Commands.Tables.Formulas.PillLabel();
			this.formulaLabel = new UI.MoreLabel();
			this.formulaBox = new River.OneMoreAddIn.Commands.Tables.Formulas.FormulaEditor();
			this.resultPanel = new System.Windows.Forms.Panel();
			this.resultValue = new UI.MoreLabel();
			this.resultCaption = new UI.MoreLabel();
			this.cellsLabel = new UI.MoreLabel();
			this.cellsBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.formatLabel = new UI.MoreLabel();
			this.formatBox = new UI.MoreComboBox();
			this.decLabel = new UI.MoreLabel();
			this.decimalBox = new System.Windows.Forms.NumericUpDown();
			this.formatHint = new UI.MoreLabel();
			this.tagBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.tagHint = new UI.MoreLabel();
			this.searchLabel = new UI.MoreLabel();
			this.searchBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.tabs = new River.OneMoreAddIn.UI.MoreTabControl();
			this.insertButton = new River.OneMoreAddIn.UI.MoreButton();
			this.countifPanel = new River.OneMoreAddIn.Commands.Tables.Formulas.CountifBuilderPanel();
			this.hintLabel = new UI.MoreLabel();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.tooltip = new System.Windows.Forms.ToolTip(this.components);
			this.resultPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.decimalBox)).BeginInit();
			this.SuspendLayout();
			//
			// targetLabel
			//
			this.targetLabel.AutoSize = true;
			this.targetLabel.Location = new System.Drawing.Point(20, 20);
			this.targetLabel.Name = "targetLabel";
			this.targetLabel.Size = new System.Drawing.Size(120, 20);
			this.targetLabel.TabIndex = 0;
			this.targetLabel.Text = "Target: A1 (1 cell)";
			//
			// formulaLabel
			//
			this.formulaLabel.AutoSize = true;
			this.formulaLabel.Location = new System.Drawing.Point(20, 68);
			this.formulaLabel.Name = "formulaLabel";
			this.formulaLabel.Size = new System.Drawing.Size(67, 20);
			this.formulaLabel.TabIndex = 1;
			this.formulaLabel.Text = "Formula";
			//
			// formulaBox
			//
			this.formulaBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.formulaBox.Location = new System.Drawing.Point(24, 100);
			this.formulaBox.Multiline = false;
			this.formulaBox.Name = "formulaBox";
			this.formulaBox.Size = new System.Drawing.Size(579, 30);
			this.formulaBox.TabIndex = 0;
			this.formulaBox.Text = "";
			this.formulaBox.TextChanged += new System.EventHandler(this.ChangedFormula);
			//
			// resultPanel
			//
			this.resultPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.resultPanel.Controls.Add(this.resultValue);
			this.resultPanel.Controls.Add(this.resultCaption);
			this.resultPanel.Location = new System.Drawing.Point(20, 152);
			this.resultPanel.Name = "resultPanel";
			this.resultPanel.Size = new System.Drawing.Size(589, 92);
			this.resultPanel.TabIndex = 2;
			//
			// resultCaption
			//
			this.resultCaption.AutoSize = true;
			this.resultCaption.Location = new System.Drawing.Point(14, 10);
			this.resultCaption.Name = "resultCaption";
			this.resultCaption.Size = new System.Drawing.Size(100, 20);
			this.resultCaption.TabIndex = 0;
			this.resultCaption.Text = "Result in A1";
			//
			// resultValue
			//
			this.resultValue.AutoSize = false;
			this.resultValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold);
			this.resultValue.Location = new System.Drawing.Point(14, 36);
			this.resultValue.Name = "resultValue";
			this.resultValue.Size = new System.Drawing.Size(561, 40);
			this.resultValue.TabIndex = 1;
			this.resultValue.Text = "";
			//
			// cellsLabel
			//
			this.cellsLabel.AutoSize = true;
			this.cellsLabel.Location = new System.Drawing.Point(20, 276);
			this.cellsLabel.Name = "cellsLabel";
			this.cellsLabel.Size = new System.Drawing.Size(180, 20);
			this.cellsLabel.TabIndex = 3;
			this.cellsLabel.Text = "Formulas that will be added";
			this.cellsLabel.Visible = false;
			//
			// cellsBox
			//
			this.cellsBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.cellsBox.Font = new System.Drawing.Font("Consolas", 10F);
			this.cellsBox.Location = new System.Drawing.Point(20, 302);
			this.cellsBox.Multiline = true;
			this.cellsBox.Name = "cellsBox";
			this.cellsBox.ReadOnly = true;
			this.cellsBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.cellsBox.Size = new System.Drawing.Size(589, 130);
			this.cellsBox.TabIndex = 4;
			this.cellsBox.TabStop = false;
			this.cellsBox.Visible = false;
			//
			// formatLabel
			//
			this.formatLabel.AutoSize = true;
			this.formatLabel.Location = new System.Drawing.Point(20, 450);
			this.formatLabel.Name = "formatLabel";
			this.formatLabel.Size = new System.Drawing.Size(60, 20);
			this.formatLabel.TabIndex = 5;
			this.formatLabel.Text = "Format";
			//
			// formatBox
			//
			this.formatBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.formatBox.FormattingEnabled = true;
			this.formatBox.Items.AddRange(new object[] {
            "Number",
            "Currency",
            "Percentage",
            "Time"});
			this.formatBox.Location = new System.Drawing.Point(20, 478);
			this.formatBox.Name = "formatBox";
			this.formatBox.Size = new System.Drawing.Size(330, 28);
			this.formatBox.TabIndex = 1;
			//
			// decLabel
			//
			this.decLabel.AutoSize = true;
			this.decLabel.Location = new System.Drawing.Point(379, 450);
			this.decLabel.Name = "decLabel";
			this.decLabel.Size = new System.Drawing.Size(116, 20);
			this.decLabel.TabIndex = 6;
			this.decLabel.Text = "Decimal places";
			this.decLabel.Scrubber = this.decimalBox;
			//
			// decimalBox
			//
			this.decimalBox.Location = new System.Drawing.Point(379, 478);
			this.decimalBox.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
			this.decimalBox.Name = "decimalBox";
			this.decimalBox.Size = new System.Drawing.Size(130, 26);
			this.decimalBox.TabIndex = 2;
			//
			// formatHint
			//
			this.formatHint.AutoSize = true;
			this.formatHint.Location = new System.Drawing.Point(20, 514);
			this.formatHint.Name = "formatHint";
			this.formatHint.Size = new System.Drawing.Size(0, 20);
			this.formatHint.TabIndex = 7;
			//
			// tagBox
			//
			this.tagBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.tagBox.Location = new System.Drawing.Point(20, 550);
			this.tagBox.Name = "tagBox";
			this.tagBox.Size = new System.Drawing.Size(300, 25);
			this.tagBox.StylizeImage = false;
			this.tagBox.TabIndex = 3;
			this.tagBox.Text = "Tag formula cells";
			this.tagBox.ThemedBack = null;
			this.tagBox.ThemedFore = null;
			this.tagBox.UseVisualStyleBackColor = true;
			//
			// tagHint
			//
			this.tagHint.AutoSize = false;
			this.tagHint.Location = new System.Drawing.Point(42, 578);
			this.tagHint.Name = "tagHint";
			this.tagHint.Size = new System.Drawing.Size(567, 44);
			this.tagHint.TabIndex = 8;
			//
			// searchLabel
			//
			this.searchLabel.AutoSize = true;
			this.searchLabel.Location = new System.Drawing.Point(639, 16);
			this.searchLabel.Name = "searchLabel";
			this.searchLabel.Size = new System.Drawing.Size(60, 20);
			this.searchLabel.TabIndex = 9;
			this.searchLabel.Text = "Search";
			//
			// searchBox
			//
			this.searchBox.Location = new System.Drawing.Point(639, 42);
			this.searchBox.Name = "searchBox";
			this.searchBox.Size = new System.Drawing.Size(421, 26);
			this.searchBox.TabIndex = 4;
			this.searchBox.TextChanged += new System.EventHandler(this.ChangedSearch);
			//
			// tabs
			//
			this.tabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabs.Location = new System.Drawing.Point(639, 80);
			this.tabs.Name = "tabs";
			this.tabs.SelectedIndex = 0;
			this.tabs.Size = new System.Drawing.Size(421, 586);
			this.tabs.TabIndex = 5;
			this.tabs.SelectedIndexChanged += new System.EventHandler(this.ChangedTab);
			//
			// insertButton
			//
			this.insertButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.insertButton.ImageOver = null;
			this.insertButton.Location = new System.Drawing.Point(880, 684);
			this.insertButton.Name = "insertButton";
			this.insertButton.ShowBorder = true;
			this.insertButton.Size = new System.Drawing.Size(180, 36);
			this.insertButton.StylizeImage = false;
			this.insertButton.TabIndex = 6;
			this.insertButton.Text = "Insert at cursor";
			this.insertButton.ThemedBack = null;
			this.insertButton.ThemedFore = null;
			this.insertButton.UseVisualStyleBackColor = true;
			this.insertButton.Click += new System.EventHandler(this.InsertSelected);
			//
			// countifPanel
			//
			this.countifPanel.Location = new System.Drawing.Point(639, 80);
			this.countifPanel.Name = "countifPanel";
			this.countifPanel.Size = new System.Drawing.Size(421, 470);
			this.countifPanel.TabIndex = 12;
			this.countifPanel.Visible = false;
			//
			// hintLabel
			//
			this.hintLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.hintLabel.AutoSize = true;
			this.hintLabel.Location = new System.Drawing.Point(20, 748);
			this.hintLabel.Name = "hintLabel";
			this.hintLabel.Size = new System.Drawing.Size(180, 20);
			this.hintLabel.TabIndex = 13;
			this.hintLabel.Text = "Syntax checked as you type";
			//
			// cancelButton
			//
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(960, 740);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 8;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			//
			// okButton
			//
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Enabled = false;
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(770, 740);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(180, 38);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 7;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			//
			// FormulaDialog
			//
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(1100, 798);
			this.Controls.Add(this.countifPanel);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.hintLabel);
			this.Controls.Add(this.insertButton);
			this.Controls.Add(this.tabs);
			this.Controls.Add(this.searchBox);
			this.Controls.Add(this.searchLabel);
			this.Controls.Add(this.tagHint);
			this.Controls.Add(this.tagBox);
			this.Controls.Add(this.formatHint);
			this.Controls.Add(this.decimalBox);
			this.Controls.Add(this.decLabel);
			this.Controls.Add(this.formatBox);
			this.Controls.Add(this.formatLabel);
			this.Controls.Add(this.cellsBox);
			this.Controls.Add(this.cellsLabel);
			this.Controls.Add(this.resultPanel);
			this.Controls.Add(this.formulaBox);
			this.Controls.Add(this.formulaLabel);
			this.Controls.Add(this.targetLabel);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormulaDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Formula";
			this.resultPanel.ResumeLayout(false);
			this.resultPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.decimalBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Tables.Formulas.PillLabel targetLabel;
		private UI.MoreLabel formulaLabel;
		private Tables.Formulas.FormulaEditor formulaBox;
		private System.Windows.Forms.Panel resultPanel;
		private UI.MoreLabel resultCaption;
		private UI.MoreLabel resultValue;
		private UI.MoreLabel cellsLabel;
		private UI.MoreTextBox cellsBox;
		private UI.MoreLabel formatLabel;
		private UI.MoreComboBox formatBox;
		private UI.MoreLabel decLabel;
		private System.Windows.Forms.NumericUpDown decimalBox;
		private UI.MoreLabel formatHint;
		private UI.MoreCheckBox tagBox;
		private UI.MoreLabel tagHint;
		private UI.MoreLabel searchLabel;
		private UI.MoreTextBox searchBox;
		private UI.MoreTabControl tabs;
		private UI.MoreButton insertButton;
		private Tables.Formulas.CountifBuilderPanel countifPanel;
		private UI.MoreLabel hintLabel;
		private UI.MoreButton cancelButton;
		private UI.MoreButton okButton;
		private System.Windows.Forms.ToolTip tooltip;
	}
}
