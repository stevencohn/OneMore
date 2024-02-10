namespace River.OneMoreAddIn.Commands
{
	partial class StylizeImagesDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StylizeImagesDialog));
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.styleLabel = new System.Windows.Forms.Label();
			this.styleBox = new System.Windows.Forms.ComboBox();
			this.foreBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.backBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.Enabled = false;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(261, 270);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OK);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(367, 270);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// styleLabel
			// 
			this.styleLabel.AutoSize = true;
			this.styleLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.styleLabel.Location = new System.Drawing.Point(40, 197);
			this.styleLabel.Name = "styleLabel";
			this.styleLabel.Size = new System.Drawing.Size(44, 20);
			this.styleLabel.TabIndex = 45;
			this.styleLabel.Text = "Style";
			// 
			// styleBox
			// 
			this.styleBox.BackColor = System.Drawing.SystemColors.Window;
			this.styleBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.styleBox.FormattingEnabled = true;
			this.styleBox.Items.AddRange(new object[] {
            "Gray scale",
            "Sepia",
            "Polaroid",
            "Invert"});
			this.styleBox.Location = new System.Drawing.Point(147, 194);
			this.styleBox.Name = "styleBox";
			this.styleBox.Size = new System.Drawing.Size(295, 28);
			this.styleBox.TabIndex = 6;
			// 
			// foreBox
			// 
			this.foreBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.foreBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.foreBox.Location = new System.Drawing.Point(44, 24);
			this.foreBox.MinimumSize = new System.Drawing.Size(285, 64);
			this.foreBox.Name = "foreBox";
			this.foreBox.Size = new System.Drawing.Size(285, 64);
			this.foreBox.TabIndex = 2;
			this.foreBox.Text = "Apply to foreground images\r\nForeground images: 0, selected: 0";
			this.foreBox.UseVisualStyleBackColor = true;
			this.foreBox.CheckedChanged += new System.EventHandler(this.ChangeSelections);
			// 
			// backBox
			// 
			this.backBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.backBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.backBox.Location = new System.Drawing.Point(44, 103);
			this.backBox.MinimumSize = new System.Drawing.Size(283, 66);
			this.backBox.Name = "backBox";
			this.backBox.Size = new System.Drawing.Size(283, 66);
			this.backBox.TabIndex = 4;
			this.backBox.Text = "Apply to background images\r\nBackground images: 0, selected 0";
			this.backBox.UseVisualStyleBackColor = true;
			this.backBox.CheckedChanged += new System.EventHandler(this.ChangeSelections);
			// 
			// StylizeImagesDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(479, 320);
			this.Controls.Add(this.backBox);
			this.Controls.Add(this.foreBox);
			this.Controls.Add(this.styleLabel);
			this.Controls.Add(this.styleBox);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.ForeColor = System.Drawing.Color.Black;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "StylizeImagesDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Stylize Images";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private System.Windows.Forms.Label styleLabel;
		private System.Windows.Forms.ComboBox styleBox;
		private UI.MoreCheckBox foreBox;
		private UI.MoreCheckBox backBox;
	}
}