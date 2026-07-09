namespace River.OneMoreAddIn.Commands
{
	partial class InsertSectionTocDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InsertSectionTocDialog));
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.previewBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.timeBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.headingsBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.levelsLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.levelsBox = new River.OneMoreAddIn.UI.MoreNumericUpDown();
			((System.ComponentModel.ISupportInitialize)(this.levelsBox)).BeginInit();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(200, 245);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(120, 38);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.CollectParametersOnOK);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(328, 245);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(120, 38);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// previewBox
			// 
			this.previewBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.previewBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.previewBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.previewBox.Location = new System.Drawing.Point(23, 20);
			this.previewBox.Name = "previewBox";
			this.previewBox.Size = new System.Drawing.Size(280, 25);
			this.previewBox.StylizeImage = false;
			this.previewBox.TabIndex = 2;
			this.previewBox.Text = "Include text preview of each page";
			this.previewBox.ThemedBack = null;
			this.previewBox.ThemedFore = null;
			this.previewBox.UseVisualStyleBackColor = true;
			// 
			// timeBox
			// 
			this.timeBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.timeBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.timeBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.timeBox.Location = new System.Drawing.Point(23, 54);
			this.timeBox.Name = "timeBox";
			this.timeBox.Size = new System.Drawing.Size(233, 25);
			this.timeBox.StylizeImage = false;
			this.timeBox.TabIndex = 3;
			this.timeBox.Text = "Update page date and time";
			this.timeBox.ThemedBack = null;
			this.timeBox.ThemedFore = null;
			this.timeBox.UseVisualStyleBackColor = true;
			// 
			// headingsBox
			// 
			this.headingsBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.headingsBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.headingsBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.headingsBox.Location = new System.Drawing.Point(23, 107);
			this.headingsBox.Name = "headingsBox";
			this.headingsBox.Size = new System.Drawing.Size(594, 38);
			this.headingsBox.StylizeImage = false;
			this.headingsBox.TabIndex = 4;
			this.headingsBox.Text = "Include headings on each page";
			this.headingsBox.ThemedBack = null;
			this.headingsBox.ThemedFore = null;
			this.headingsBox.UseVisualStyleBackColor = true;
			this.headingsBox.CheckedChanged += new System.EventHandler(this.ToggleHeadings);
			// 
			// levelsLabel
			// 
			this.levelsLabel.AutoSize = true;
			this.levelsLabel.Location = new System.Drawing.Point(23, 156);
			this.levelsLabel.Name = "levelsLabel";
			this.levelsLabel.Size = new System.Drawing.Size(98, 20);
			this.levelsLabel.TabIndex = 11;
			this.levelsLabel.Text = "Show Levels";
			this.levelsLabel.ThemedBack = null;
			this.levelsLabel.ThemedFore = null;
			// 
			// levelsBox
			// 
			this.levelsBox.Enabled = false;
			this.levelsBox.Location = new System.Drawing.Point(179, 154);
			this.levelsBox.Maximum = new decimal(new int[] {
            6,
            0,
            0,
            0});
			this.levelsBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.levelsBox.Name = "levelsBox";
			this.levelsBox.Size = new System.Drawing.Size(96, 26);
			this.levelsBox.TabIndex = 5;
			this.levelsBox.ThemedBack = null;
			this.levelsBox.ThemedFore = "ControlText";
			this.levelsBox.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// InsertSectionTocDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(460, 310);
			this.Controls.Add(this.levelsLabel);
			this.Controls.Add(this.levelsBox);
			this.Controls.Add(this.headingsBox);
			this.Controls.Add(this.timeBox);
			this.Controls.Add(this.previewBox);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "InsertSectionTocDialog";
			this.Padding = new System.Windows.Forms.Padding(20, 25, 0, 0);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Tables of Pages in Section";
			((System.ComponentModel.ISupportInitialize)(this.levelsBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private UI.MoreCheckBox previewBox;
		private UI.MoreCheckBox timeBox;
		private UI.MoreCheckBox headingsBox;
		private UI.MoreLabel levelsLabel;
		private UI.MoreNumericUpDown levelsBox;
	}
}
