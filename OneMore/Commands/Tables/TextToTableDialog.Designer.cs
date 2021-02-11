namespace River.OneMoreAddIn.Commands
{
	partial class TextToTableDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextToTableDialog));
			this.group1 = new System.Windows.Forms.GroupBox();
			this.otherBox = new System.Windows.Forms.TextBox();
			this.otherRadio = new System.Windows.Forms.RadioButton();
			this.commasRadio = new System.Windows.Forms.RadioButton();
			this.tabsRadio = new System.Windows.Forms.RadioButton();
			this.paragraphsRadio = new System.Windows.Forms.RadioButton();
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.group2 = new System.Windows.Forms.GroupBox();
			this.rowsBox = new System.Windows.Forms.NumericUpDown();
			this.columnsBox = new System.Windows.Forms.NumericUpDown();
			this.rowsLabel = new System.Windows.Forms.Label();
			this.columnsLabel = new System.Windows.Forms.Label();
			this.headerBox = new System.Windows.Forms.CheckBox();
			this.unquoteBox = new System.Windows.Forms.CheckBox();
			this.group1.SuspendLayout();
			this.group2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.rowsBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.columnsBox)).BeginInit();
			this.SuspendLayout();
			// 
			// group1
			// 
			this.group1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.group1.Controls.Add(this.otherBox);
			this.group1.Controls.Add(this.otherRadio);
			this.group1.Controls.Add(this.commasRadio);
			this.group1.Controls.Add(this.tabsRadio);
			this.group1.Controls.Add(this.paragraphsRadio);
			this.group1.Location = new System.Drawing.Point(18, 18);
			this.group1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.group1.Name = "group1";
			this.group1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.group1.Size = new System.Drawing.Size(464, 121);
			this.group1.TabIndex = 0;
			this.group1.TabStop = false;
			this.group1.Text = "Separate text at";
			// 
			// otherBox
			// 
			this.otherBox.Enabled = false;
			this.otherBox.Location = new System.Drawing.Point(303, 63);
			this.otherBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.otherBox.MaxLength = 10;
			this.otherBox.Name = "otherBox";
			this.otherBox.Size = new System.Drawing.Size(139, 26);
			this.otherBox.TabIndex = 4;
			this.otherBox.Text = "~";
			this.otherBox.TextChanged += new System.EventHandler(this.ChangeCustomDelimeter);
			// 
			// otherRadio
			// 
			this.otherRadio.AutoSize = true;
			this.otherRadio.Location = new System.Drawing.Point(218, 65);
			this.otherRadio.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.otherRadio.Name = "otherRadio";
			this.otherRadio.Size = new System.Drawing.Size(74, 24);
			this.otherRadio.TabIndex = 3;
			this.otherRadio.Text = "Other";
			this.otherRadio.UseVisualStyleBackColor = true;
			this.otherRadio.CheckedChanged += new System.EventHandler(this.ChangeDelimetedBy);
			// 
			// commasRadio
			// 
			this.commasRadio.AutoSize = true;
			this.commasRadio.Checked = true;
			this.commasRadio.Location = new System.Drawing.Point(218, 29);
			this.commasRadio.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.commasRadio.Name = "commasRadio";
			this.commasRadio.Size = new System.Drawing.Size(97, 24);
			this.commasRadio.TabIndex = 2;
			this.commasRadio.TabStop = true;
			this.commasRadio.Text = "Commas";
			this.commasRadio.UseVisualStyleBackColor = true;
			this.commasRadio.CheckedChanged += new System.EventHandler(this.ChangeDelimetedBy);
			// 
			// tabsRadio
			// 
			this.tabsRadio.AutoSize = true;
			this.tabsRadio.Location = new System.Drawing.Point(24, 65);
			this.tabsRadio.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tabsRadio.Name = "tabsRadio";
			this.tabsRadio.Size = new System.Drawing.Size(69, 24);
			this.tabsRadio.TabIndex = 1;
			this.tabsRadio.Text = "Tabs";
			this.tabsRadio.UseVisualStyleBackColor = true;
			this.tabsRadio.CheckedChanged += new System.EventHandler(this.ChangeDelimetedBy);
			// 
			// paragraphsRadio
			// 
			this.paragraphsRadio.AutoSize = true;
			this.paragraphsRadio.Location = new System.Drawing.Point(24, 29);
			this.paragraphsRadio.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.paragraphsRadio.Name = "paragraphsRadio";
			this.paragraphsRadio.Size = new System.Drawing.Size(116, 24);
			this.paragraphsRadio.TabIndex = 0;
			this.paragraphsRadio.Text = "Paragraphs";
			this.paragraphsRadio.UseVisualStyleBackColor = true;
			this.paragraphsRadio.CheckedChanged += new System.EventHandler(this.ChangeDelimetedBy);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(369, 359);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(112, 35);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(248, 359);
			this.okButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(112, 35);
			this.okButton.TabIndex = 2;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// group2
			// 
			this.group2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.group2.Controls.Add(this.rowsBox);
			this.group2.Controls.Add(this.columnsBox);
			this.group2.Controls.Add(this.rowsLabel);
			this.group2.Controls.Add(this.columnsLabel);
			this.group2.Location = new System.Drawing.Point(18, 149);
			this.group2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.group2.Name = "group2";
			this.group2.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.group2.Size = new System.Drawing.Size(464, 129);
			this.group2.TabIndex = 3;
			this.group2.TabStop = false;
			this.group2.Text = "Table size";
			// 
			// rowsBox
			// 
			this.rowsBox.Location = new System.Drawing.Point(180, 69);
			this.rowsBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.rowsBox.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.rowsBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.rowsBox.Name = "rowsBox";
			this.rowsBox.Size = new System.Drawing.Size(114, 26);
			this.rowsBox.TabIndex = 3;
			this.rowsBox.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// columnsBox
			// 
			this.columnsBox.Location = new System.Drawing.Point(180, 29);
			this.columnsBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.columnsBox.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.columnsBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.columnsBox.Name = "columnsBox";
			this.columnsBox.Size = new System.Drawing.Size(114, 26);
			this.columnsBox.TabIndex = 2;
			this.columnsBox.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// rowsLabel
			// 
			this.rowsLabel.AutoSize = true;
			this.rowsLabel.Location = new System.Drawing.Point(20, 72);
			this.rowsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.rowsLabel.Name = "rowsLabel";
			this.rowsLabel.Size = new System.Drawing.Size(124, 20);
			this.rowsLabel.TabIndex = 1;
			this.rowsLabel.Text = "Number of rows:";
			// 
			// columnsLabel
			// 
			this.columnsLabel.AutoSize = true;
			this.columnsLabel.Location = new System.Drawing.Point(20, 32);
			this.columnsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.columnsLabel.Name = "columnsLabel";
			this.columnsLabel.Size = new System.Drawing.Size(150, 20);
			this.columnsLabel.TabIndex = 0;
			this.columnsLabel.Text = "Number of columns:";
			// 
			// headerBox
			// 
			this.headerBox.AutoSize = true;
			this.headerBox.Location = new System.Drawing.Point(18, 286);
			this.headerBox.Name = "headerBox";
			this.headerBox.Size = new System.Drawing.Size(177, 24);
			this.headerBox.TabIndex = 5;
			this.headerBox.Text = "First row is a header";
			this.headerBox.UseVisualStyleBackColor = true;
			// 
			// unquoteBox
			// 
			this.unquoteBox.AutoSize = true;
			this.unquoteBox.Location = new System.Drawing.Point(18, 316);
			this.unquoteBox.Name = "unquoteBox";
			this.unquoteBox.Size = new System.Drawing.Size(303, 24);
			this.unquoteBox.TabIndex = 6;
			this.unquoteBox.Text = "Remove quotes from complete strings";
			this.unquoteBox.UseVisualStyleBackColor = true;
			// 
			// TextToTableDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(500, 413);
			this.Controls.Add(this.unquoteBox);
			this.Controls.Add(this.headerBox);
			this.Controls.Add(this.group2);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.group1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "TextToTableDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Text to Table";
			this.group1.ResumeLayout(false);
			this.group1.PerformLayout();
			this.group2.ResumeLayout(false);
			this.group2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.rowsBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.columnsBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.GroupBox group1;
		private System.Windows.Forms.TextBox otherBox;
		private System.Windows.Forms.RadioButton otherRadio;
		private System.Windows.Forms.RadioButton commasRadio;
		private System.Windows.Forms.RadioButton tabsRadio;
		private System.Windows.Forms.RadioButton paragraphsRadio;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.GroupBox group2;
		private System.Windows.Forms.NumericUpDown rowsBox;
		private System.Windows.Forms.NumericUpDown columnsBox;
		private System.Windows.Forms.Label rowsLabel;
		private System.Windows.Forms.Label columnsLabel;
		private System.Windows.Forms.CheckBox headerBox;
		private System.Windows.Forms.CheckBox unquoteBox;
	}
}