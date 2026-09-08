namespace River.OneMoreAddIn.Commands
{
	partial class ConvertOutlineDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConvertOutlineDialog));
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.depthLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.depthBox = new River.OneMoreAddIn.UI.MoreNumericUpDown();
			this.themeLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.themeBox = new River.OneMoreAddIn.UI.MoreComboBox();
			((System.ComponentModel.ISupportInitialize)(this.depthBox)).BeginInit();
			this.SuspendLayout();
			//
			// okButton
			//
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(269, 152);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(120, 38);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 2;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			//
			// cancelButton
			//
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(397, 152);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(120, 38);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 3;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			//
			// depthLabel
			//
			this.depthLabel.AutoSize = true;
			this.depthLabel.Location = new System.Drawing.Point(23, 33);
			this.depthLabel.Name = "depthLabel";
			this.depthLabel.Size = new System.Drawing.Size(120, 20);
			this.depthLabel.TabIndex = 0;
			this.depthLabel.Text = "Heading depth";
			this.depthLabel.ThemedBack = null;
			this.depthLabel.ThemedFore = null;
			//
			// depthBox
			//
			this.depthBox.Location = new System.Drawing.Point(253, 28);
			this.depthBox.Maximum = new decimal(new int[] {
            6,
            0,
            0,
            0});
			this.depthBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.depthBox.Name = "depthBox";
			this.depthBox.Size = new System.Drawing.Size(96, 26);
			this.depthBox.TabIndex = 0;
			this.depthBox.ThemedBack = null;
			this.depthBox.ThemedFore = "ControlText";
			this.depthBox.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			//
			// themeLabel
			//
			this.themeLabel.AutoSize = true;
			this.themeLabel.Location = new System.Drawing.Point(23, 75);
			this.themeLabel.Name = "themeLabel";
			this.themeLabel.Size = new System.Drawing.Size(120, 20);
			this.themeLabel.TabIndex = 1;
			this.themeLabel.Text = "Heading theme";
			this.themeLabel.ThemedBack = null;
			this.themeLabel.ThemedFore = null;
			//
			// themeBox
			//
			this.themeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.themeBox.FormattingEnabled = true;
			this.themeBox.Location = new System.Drawing.Point(253, 70);
			this.themeBox.Name = "themeBox";
			this.themeBox.Size = new System.Drawing.Size(264, 27);
			this.themeBox.TabIndex = 1;
			this.themeBox.ThemedBack = null;
			this.themeBox.ThemedFore = null;
			//
			// ConvertOutlineDialog
			//
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(541, 210);
			this.Controls.Add(this.themeLabel);
			this.Controls.Add(this.themeBox);
			this.Controls.Add(this.depthLabel);
			this.Controls.Add(this.depthBox);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ConvertOutlineDialog";
			this.Padding = new System.Windows.Forms.Padding(20, 25, 0, 0);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Convert Outline";
			((System.ComponentModel.ISupportInitialize)(this.depthBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private UI.MoreLabel depthLabel;
		private UI.MoreNumericUpDown depthBox;
		private UI.MoreLabel themeLabel;
		private UI.MoreComboBox themeBox;
	}
}
