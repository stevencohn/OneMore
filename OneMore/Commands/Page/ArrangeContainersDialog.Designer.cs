
namespace River.OneMoreAddIn.Commands
{
	partial class ArrangeContainersDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ArrangeContainersDialog));
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.verticalButton = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.flowButton = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.columnsLabel = new System.Windows.Forms.Label();
			this.columnsBox = new System.Windows.Forms.NumericUpDown();
			this.widthBox = new System.Windows.Forms.NumericUpDown();
			this.widthLabel = new System.Windows.Forms.Label();
			this.setWidthCheckBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.setWidthBox = new System.Windows.Forms.NumericUpDown();
			this.indentBox = new System.Windows.Forms.NumericUpDown();
			this.indentLabel = new System.Windows.Forms.Label();
			this.optionsPanel = new River.OneMoreAddIn.UI.MorePanel();
			((System.ComponentModel.ISupportInitialize)(this.columnsBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.widthBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.setWidthBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.indentBox)).BeginInit();
			this.optionsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(285, 317);
			this.okButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(112, 35);
			this.okButton.TabIndex = 6;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.SaveSettingsOnClick);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(406, 317);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(112, 35);
			this.cancelButton.TabIndex = 7;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// verticalButton
			// 
			this.verticalButton.Checked = true;
			this.verticalButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.verticalButton.Location = new System.Drawing.Point(33, 23);
			this.verticalButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.verticalButton.Name = "verticalButton";
			this.verticalButton.Size = new System.Drawing.Size(369, 25);
			this.verticalButton.TabIndex = 0;
			this.verticalButton.TabStop = true;
			this.verticalButton.Text = "Arrange all containers vertically, top to bottom";
			this.verticalButton.UseVisualStyleBackColor = true;
			this.verticalButton.CheckedChanged += new System.EventHandler(this.ChangeSelection);
			// 
			// flowButton
			// 
			this.flowButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.flowButton.Location = new System.Drawing.Point(33, 115);
			this.flowButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
			this.flowButton.Name = "flowButton";
			this.flowButton.Size = new System.Drawing.Size(409, 25);
			this.flowButton.TabIndex = 3;
			this.flowButton.Text = "Arrange in columns and then rows, fit to page width";
			this.flowButton.UseVisualStyleBackColor = true;
			this.flowButton.CheckedChanged += new System.EventHandler(this.ChangeSelection);
			// 
			// columnsLabel
			// 
			this.columnsLabel.AutoSize = true;
			this.columnsLabel.Location = new System.Drawing.Point(68, 152);
			this.columnsLabel.Name = "columnsLabel";
			this.columnsLabel.Size = new System.Drawing.Size(71, 20);
			this.columnsLabel.TabIndex = 7;
			this.columnsLabel.Text = "Columns";
			// 
			// columnsBox
			// 
			this.columnsBox.Enabled = false;
			this.columnsBox.Location = new System.Drawing.Point(208, 150);
			this.columnsBox.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.columnsBox.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
			this.columnsBox.Name = "columnsBox";
			this.columnsBox.Size = new System.Drawing.Size(131, 26);
			this.columnsBox.TabIndex = 4;
			this.columnsBox.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
			// 
			// widthBox
			// 
			this.widthBox.Enabled = false;
			this.widthBox.Location = new System.Drawing.Point(208, 182);
			this.widthBox.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
			this.widthBox.Minimum = new decimal(new int[] {
            200,
            0,
            0,
            0});
			this.widthBox.Name = "widthBox";
			this.widthBox.Size = new System.Drawing.Size(131, 26);
			this.widthBox.TabIndex = 5;
			this.widthBox.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
			// 
			// widthLabel
			// 
			this.widthLabel.AutoSize = true;
			this.widthLabel.Location = new System.Drawing.Point(68, 184);
			this.widthLabel.Name = "widthLabel";
			this.widthLabel.Size = new System.Drawing.Size(87, 20);
			this.widthLabel.TabIndex = 10;
			this.widthLabel.Text = "Page width";
			// 
			// setWidthCheckBox
			// 
			this.setWidthCheckBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.setWidthCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.setWidthCheckBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.setWidthCheckBox.Location = new System.Drawing.Point(72, 60);
			this.setWidthCheckBox.Name = "setWidthCheckBox";
			this.setWidthCheckBox.Size = new System.Drawing.Size(78, 25);
			this.setWidthCheckBox.TabIndex = 1;
			this.setWidthCheckBox.Text = "Width";
			this.setWidthCheckBox.UseVisualStyleBackColor = true;
			// 
			// setWidthBox
			// 
			this.setWidthBox.Location = new System.Drawing.Point(208, 59);
			this.setWidthBox.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
			this.setWidthBox.Minimum = new decimal(new int[] {
            200,
            0,
            0,
            0});
			this.setWidthBox.Name = "setWidthBox";
			this.setWidthBox.Size = new System.Drawing.Size(131, 26);
			this.setWidthBox.TabIndex = 2;
			this.setWidthBox.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
			// 
			// indentBox
			// 
			this.indentBox.Location = new System.Drawing.Point(185, 20);
			this.indentBox.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
			this.indentBox.Name = "indentBox";
			this.indentBox.Size = new System.Drawing.Size(131, 26);
			this.indentBox.TabIndex = 11;
			// 
			// indentLabel
			// 
			this.indentLabel.AutoSize = true;
			this.indentLabel.Location = new System.Drawing.Point(6, 22);
			this.indentLabel.Name = "indentLabel";
			this.indentLabel.Size = new System.Drawing.Size(55, 20);
			this.indentLabel.TabIndex = 12;
			this.indentLabel.Text = "Indent";
			// 
			// optionsPanel
			// 
			this.optionsPanel.BottomBorderColor = System.Drawing.SystemColors.Control;
			this.optionsPanel.BottomBorderSize = 0;
			this.optionsPanel.Controls.Add(this.indentBox);
			this.optionsPanel.Controls.Add(this.indentLabel);
			this.optionsPanel.Location = new System.Drawing.Point(23, 229);
			this.optionsPanel.Name = "optionsPanel";
			this.optionsPanel.Size = new System.Drawing.Size(485, 66);
			this.optionsPanel.TabIndex = 13;
			this.optionsPanel.ThemedBack = "ControlLight";
			this.optionsPanel.ThemedFore = null;
			this.optionsPanel.TopBorderColor = System.Drawing.SystemColors.ActiveBorder;
			this.optionsPanel.TopBorderSize = 1;
			// 
			// ArrangeContainersDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(537, 372);
			this.Controls.Add(this.optionsPanel);
			this.Controls.Add(this.setWidthBox);
			this.Controls.Add(this.setWidthCheckBox);
			this.Controls.Add(this.widthLabel);
			this.Controls.Add(this.widthBox);
			this.Controls.Add(this.columnsBox);
			this.Controls.Add(this.columnsLabel);
			this.Controls.Add(this.flowButton);
			this.Controls.Add(this.verticalButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ArrangeContainersDialog";
			this.Padding = new System.Windows.Forms.Padding(20, 20, 15, 15);
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Arrange Containers";
			((System.ComponentModel.ISupportInitialize)(this.columnsBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.widthBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.setWidthBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.indentBox)).EndInit();
			this.optionsPanel.ResumeLayout(false);
			this.optionsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private UI.MoreRadioButton verticalButton;
		private UI.MoreRadioButton flowButton;
		private System.Windows.Forms.Label columnsLabel;
		private System.Windows.Forms.NumericUpDown columnsBox;
		private System.Windows.Forms.NumericUpDown widthBox;
		private System.Windows.Forms.Label widthLabel;
		private UI.MoreCheckBox setWidthCheckBox;
		private System.Windows.Forms.NumericUpDown setWidthBox;
		private System.Windows.Forms.NumericUpDown indentBox;
		private System.Windows.Forms.Label indentLabel;
		private UI.MorePanel optionsPanel;
	}
}