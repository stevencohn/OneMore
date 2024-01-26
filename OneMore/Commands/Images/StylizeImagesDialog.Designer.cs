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
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.foreImagesLabel = new System.Windows.Forms.Label();
			this.backImagesLabel = new System.Windows.Forms.Label();
			this.styleLabel = new System.Windows.Forms.Label();
			this.styleBox = new System.Windows.Forms.ComboBox();
			this.foreBox = new System.Windows.Forms.CheckBox();
			this.backBox = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.Enabled = false;
			this.okButton.Location = new System.Drawing.Point(261, 270);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.TabIndex = 25;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OK);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(367, 270);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.TabIndex = 26;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// foreImagesLabel
			// 
			this.foreImagesLabel.AutoSize = true;
			this.foreImagesLabel.Location = new System.Drawing.Point(69, 67);
			this.foreImagesLabel.Name = "foreImagesLabel";
			this.foreImagesLabel.Size = new System.Drawing.Size(249, 20);
			this.foreImagesLabel.TabIndex = 27;
			this.foreImagesLabel.Text = "Foreground images: 0, selected: 0";
			// 
			// backImagesLabel
			// 
			this.backImagesLabel.AutoSize = true;
			this.backImagesLabel.Location = new System.Drawing.Point(69, 137);
			this.backImagesLabel.Name = "backImagesLabel";
			this.backImagesLabel.Size = new System.Drawing.Size(248, 20);
			this.backImagesLabel.TabIndex = 28;
			this.backImagesLabel.Text = "Background images: 0, selected 0";
			// 
			// styleLabel
			// 
			this.styleLabel.AutoSize = true;
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
			this.styleBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.styleBox.FormattingEnabled = true;
			this.styleBox.Items.AddRange(new object[] {
            "Gray scale",
            "Sepia",
            "Polaroid",
            "Invert"});
			this.styleBox.Location = new System.Drawing.Point(147, 194);
			this.styleBox.Name = "styleBox";
			this.styleBox.Size = new System.Drawing.Size(295, 28);
			this.styleBox.TabIndex = 44;
			// 
			// foreBox
			// 
			this.foreBox.AutoSize = true;
			this.foreBox.Location = new System.Drawing.Point(44, 40);
			this.foreBox.Name = "foreBox";
			this.foreBox.Size = new System.Drawing.Size(229, 24);
			this.foreBox.TabIndex = 46;
			this.foreBox.Text = "Apply to foreground images";
			this.foreBox.UseVisualStyleBackColor = true;
			this.foreBox.CheckedChanged += new System.EventHandler(this.ChangeSelections);
			// 
			// backBox
			// 
			this.backBox.AutoSize = true;
			this.backBox.Location = new System.Drawing.Point(44, 110);
			this.backBox.Name = "backBox";
			this.backBox.Size = new System.Drawing.Size(235, 24);
			this.backBox.TabIndex = 47;
			this.backBox.Text = "Apply to background images";
			this.backBox.UseVisualStyleBackColor = true;
			this.backBox.CheckedChanged += new System.EventHandler(this.ChangeSelections);
			// 
			// StylizeImagesDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(479, 320);
			this.Controls.Add(this.backBox);
			this.Controls.Add(this.foreBox);
			this.Controls.Add(this.styleLabel);
			this.Controls.Add(this.styleBox);
			this.Controls.Add(this.backImagesLabel);
			this.Controls.Add(this.foreImagesLabel);
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

		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Label foreImagesLabel;
		private System.Windows.Forms.Label backImagesLabel;
		private System.Windows.Forms.Label styleLabel;
		private System.Windows.Forms.ComboBox styleBox;
		private System.Windows.Forms.CheckBox foreBox;
		private System.Windows.Forms.CheckBox backBox;
	}
}