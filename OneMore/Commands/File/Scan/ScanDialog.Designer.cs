namespace River.OneMoreAddIn.Commands
{
	partial class ScanDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScanDialog));
			this.morePanel1 = new River.OneMoreAddIn.UI.MorePanel();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.scannerLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.scannerBox = new System.Windows.Forms.ComboBox();
			this.contentPanel = new River.OneMoreAddIn.UI.MorePanel();
			this.contrastBox = new River.OneMoreAddIn.UI.MoreNumericUpDown();
			this.brightnessBox = new River.OneMoreAddIn.UI.MoreNumericUpDown();
			this.contrastSlider = new System.Windows.Forms.TrackBar();
			this.brightnessSlider = new System.Windows.Forms.TrackBar();
			this.constrastLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.brightnessLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.resolutionLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.resolutionBox = new System.Windows.Forms.ComboBox();
			this.colorLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.colorBox = new System.Windows.Forms.ComboBox();
			this.sizeBox = new System.Windows.Forms.ComboBox();
			this.sourceBox = new System.Windows.Forms.ComboBox();
			this.profileBox = new System.Windows.Forms.ComboBox();
			this.sizeLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.sourceLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.profileLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.modelLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.morePanel1.SuspendLayout();
			this.contentPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.contrastBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.brightnessBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.contrastSlider)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.brightnessSlider)).BeginInit();
			this.SuspendLayout();
			// 
			// morePanel1
			// 
			this.morePanel1.BottomBorderColor = System.Drawing.SystemColors.ActiveBorder;
			this.morePanel1.BottomBorderSize = 0;
			this.morePanel1.Controls.Add(this.progressBar);
			this.morePanel1.Controls.Add(this.okButton);
			this.morePanel1.Controls.Add(this.cancelButton);
			this.morePanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.morePanel1.Location = new System.Drawing.Point(0, 488);
			this.morePanel1.Name = "morePanel1";
			this.morePanel1.Size = new System.Drawing.Size(607, 66);
			this.morePanel1.TabIndex = 1;
			this.morePanel1.ThemedBack = null;
			this.morePanel1.ThemedFore = null;
			this.morePanel1.TopBorderColor = System.Drawing.SystemColors.Control;
			this.morePanel1.TopBorderSize = 0;
			// 
			// progressBar
			// 
			this.progressBar.Location = new System.Drawing.Point(27, 26);
			this.progressBar.Maximum = 60;
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(421, 16);
			this.progressBar.Step = 1;
			this.progressBar.TabIndex = 4;
			this.progressBar.Visible = false;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.Enabled = false;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(362, 17);
			this.okButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(112, 35);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.BeginScanning);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(482, 17);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(112, 35);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.Cancel);
			// 
			// scannerLabel
			// 
			this.scannerLabel.AutoSize = true;
			this.scannerLabel.Location = new System.Drawing.Point(23, 50);
			this.scannerLabel.Name = "scannerLabel";
			this.scannerLabel.Size = new System.Drawing.Size(69, 20);
			this.scannerLabel.TabIndex = 3;
			this.scannerLabel.Text = "Scanner";
			this.scannerLabel.ThemedBack = null;
			this.scannerLabel.ThemedFore = null;
			// 
			// scannerBox
			// 
			this.scannerBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.scannerBox.FormattingEnabled = true;
			this.scannerBox.Items.AddRange(new object[] {
            "In this notebook",
            "In this section",
            "On this page"});
			this.scannerBox.Location = new System.Drawing.Point(167, 47);
			this.scannerBox.Name = "scannerBox";
			this.scannerBox.Size = new System.Drawing.Size(400, 28);
			this.scannerBox.TabIndex = 0;
			this.scannerBox.SelectedIndexChanged += new System.EventHandler(this.ChangeScanner);
			// 
			// contentPanel
			// 
			this.contentPanel.BottomBorderColor = System.Drawing.SystemColors.ActiveBorder;
			this.contentPanel.BottomBorderSize = 0;
			this.contentPanel.Controls.Add(this.contrastBox);
			this.contentPanel.Controls.Add(this.brightnessBox);
			this.contentPanel.Controls.Add(this.contrastSlider);
			this.contentPanel.Controls.Add(this.brightnessSlider);
			this.contentPanel.Controls.Add(this.constrastLabel);
			this.contentPanel.Controls.Add(this.brightnessLabel);
			this.contentPanel.Controls.Add(this.resolutionLabel);
			this.contentPanel.Controls.Add(this.resolutionBox);
			this.contentPanel.Controls.Add(this.colorLabel);
			this.contentPanel.Controls.Add(this.colorBox);
			this.contentPanel.Controls.Add(this.sizeBox);
			this.contentPanel.Controls.Add(this.sourceBox);
			this.contentPanel.Controls.Add(this.profileBox);
			this.contentPanel.Controls.Add(this.sizeLabel);
			this.contentPanel.Controls.Add(this.sourceLabel);
			this.contentPanel.Controls.Add(this.profileLabel);
			this.contentPanel.Controls.Add(this.modelLabel);
			this.contentPanel.Controls.Add(this.scannerBox);
			this.contentPanel.Controls.Add(this.scannerLabel);
			this.contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.contentPanel.Location = new System.Drawing.Point(0, 0);
			this.contentPanel.Name = "contentPanel";
			this.contentPanel.Padding = new System.Windows.Forms.Padding(20);
			this.contentPanel.Size = new System.Drawing.Size(607, 488);
			this.contentPanel.TabIndex = 0;
			this.contentPanel.ThemedBack = null;
			this.contentPanel.ThemedFore = null;
			this.contentPanel.TopBorderColor = System.Drawing.SystemColors.Control;
			this.contentPanel.TopBorderSize = 0;
			// 
			// contrastBox
			// 
			this.contrastBox.BackColor = System.Drawing.SystemColors.Window;
			this.contrastBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.contrastBox.Location = new System.Drawing.Point(495, 388);
			this.contrastBox.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
			this.contrastBox.Name = "contrastBox";
			this.contrastBox.Size = new System.Drawing.Size(72, 26);
			this.contrastBox.TabIndex = 20;
			this.contrastBox.ThemedBack = null;
			this.contrastBox.ThemedFore = null;
			// 
			// brightnessBox
			// 
			this.brightnessBox.BackColor = System.Drawing.SystemColors.Window;
			this.brightnessBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.brightnessBox.Location = new System.Drawing.Point(495, 352);
			this.brightnessBox.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
			this.brightnessBox.Name = "brightnessBox";
			this.brightnessBox.Size = new System.Drawing.Size(72, 26);
			this.brightnessBox.TabIndex = 19;
			this.brightnessBox.ThemedBack = null;
			this.brightnessBox.ThemedFore = null;
			this.brightnessBox.ValueChanged += new System.EventHandler(this.ChangedUpDown);
			// 
			// contrastSlider
			// 
			this.contrastSlider.AutoSize = false;
			this.contrastSlider.Enabled = false;
			this.contrastSlider.Location = new System.Drawing.Point(245, 389);
			this.contrastSlider.Maximum = 100;
			this.contrastSlider.Minimum = -100;
			this.contrastSlider.Name = "contrastSlider";
			this.contrastSlider.Size = new System.Drawing.Size(244, 25);
			this.contrastSlider.TabIndex = 8;
			this.contrastSlider.TickStyle = System.Windows.Forms.TickStyle.None;
			this.contrastSlider.ValueChanged += new System.EventHandler(this.ChangedSlider);
			// 
			// brightnessSlider
			// 
			this.brightnessSlider.AutoSize = false;
			this.brightnessSlider.Enabled = false;
			this.brightnessSlider.Location = new System.Drawing.Point(245, 353);
			this.brightnessSlider.Maximum = 100;
			this.brightnessSlider.Minimum = -100;
			this.brightnessSlider.Name = "brightnessSlider";
			this.brightnessSlider.Size = new System.Drawing.Size(244, 25);
			this.brightnessSlider.TabIndex = 6;
			this.brightnessSlider.TickStyle = System.Windows.Forms.TickStyle.None;
			this.brightnessSlider.ValueChanged += new System.EventHandler(this.ChangedSlider);
			// 
			// constrastLabel
			// 
			this.constrastLabel.AutoSize = true;
			this.constrastLabel.Location = new System.Drawing.Point(23, 395);
			this.constrastLabel.Name = "constrastLabel";
			this.constrastLabel.Size = new System.Drawing.Size(70, 20);
			this.constrastLabel.TabIndex = 18;
			this.constrastLabel.Text = "Contrast";
			this.constrastLabel.ThemedBack = null;
			this.constrastLabel.ThemedFore = null;
			// 
			// brightnessLabel
			// 
			this.brightnessLabel.AutoSize = true;
			this.brightnessLabel.Location = new System.Drawing.Point(23, 355);
			this.brightnessLabel.Name = "brightnessLabel";
			this.brightnessLabel.Size = new System.Drawing.Size(85, 20);
			this.brightnessLabel.TabIndex = 17;
			this.brightnessLabel.Text = "Brightness";
			this.brightnessLabel.ThemedBack = null;
			this.brightnessLabel.ThemedFore = null;
			// 
			// resolutionLabel
			// 
			this.resolutionLabel.AutoSize = true;
			this.resolutionLabel.Location = new System.Drawing.Point(23, 302);
			this.resolutionLabel.Name = "resolutionLabel";
			this.resolutionLabel.Size = new System.Drawing.Size(85, 20);
			this.resolutionLabel.TabIndex = 16;
			this.resolutionLabel.Text = "Resolution";
			this.resolutionLabel.ThemedBack = null;
			this.resolutionLabel.ThemedFore = null;
			// 
			// resolutionBox
			// 
			this.resolutionBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.resolutionBox.Enabled = false;
			this.resolutionBox.FormattingEnabled = true;
			this.resolutionBox.Location = new System.Drawing.Point(245, 299);
			this.resolutionBox.Name = "resolutionBox";
			this.resolutionBox.Size = new System.Drawing.Size(322, 28);
			this.resolutionBox.TabIndex = 5;
			// 
			// colorLabel
			// 
			this.colorLabel.AutoSize = true;
			this.colorLabel.Location = new System.Drawing.Point(23, 268);
			this.colorLabel.Name = "colorLabel";
			this.colorLabel.Size = new System.Drawing.Size(96, 20);
			this.colorLabel.TabIndex = 14;
			this.colorLabel.Text = "Color format";
			this.colorLabel.ThemedBack = null;
			this.colorLabel.ThemedFore = null;
			// 
			// colorBox
			// 
			this.colorBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.colorBox.Enabled = false;
			this.colorBox.FormattingEnabled = true;
			this.colorBox.Items.AddRange(new object[] {
            "Color",
            "Grayscale",
            "Black and White"});
			this.colorBox.Location = new System.Drawing.Point(245, 265);
			this.colorBox.Name = "colorBox";
			this.colorBox.Size = new System.Drawing.Size(322, 28);
			this.colorBox.TabIndex = 4;
			// 
			// sizeBox
			// 
			this.sizeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.sizeBox.DropDownWidth = 500;
			this.sizeBox.Enabled = false;
			this.sizeBox.FormattingEnabled = true;
			this.sizeBox.Location = new System.Drawing.Point(245, 205);
			this.sizeBox.Name = "sizeBox";
			this.sizeBox.Size = new System.Drawing.Size(322, 28);
			this.sizeBox.TabIndex = 3;
			// 
			// sourceBox
			// 
			this.sourceBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.sourceBox.Enabled = false;
			this.sourceBox.FormattingEnabled = true;
			this.sourceBox.Items.AddRange(new object[] {
            "Flatbed",
            "Feeder"});
			this.sourceBox.Location = new System.Drawing.Point(245, 170);
			this.sourceBox.Name = "sourceBox";
			this.sourceBox.Size = new System.Drawing.Size(322, 28);
			this.sourceBox.TabIndex = 2;
			// 
			// profileBox
			// 
			this.profileBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.profileBox.Enabled = false;
			this.profileBox.FormattingEnabled = true;
			this.profileBox.Location = new System.Drawing.Point(245, 136);
			this.profileBox.Name = "profileBox";
			this.profileBox.Size = new System.Drawing.Size(322, 28);
			this.profileBox.TabIndex = 1;
			// 
			// sizeLabel
			// 
			this.sizeLabel.AutoSize = true;
			this.sizeLabel.Location = new System.Drawing.Point(23, 208);
			this.sizeLabel.Name = "sizeLabel";
			this.sizeLabel.Size = new System.Drawing.Size(40, 20);
			this.sizeLabel.TabIndex = 9;
			this.sizeLabel.Text = "Size";
			this.sizeLabel.ThemedBack = null;
			this.sizeLabel.ThemedFore = null;
			// 
			// sourceLabel
			// 
			this.sourceLabel.AutoSize = true;
			this.sourceLabel.Location = new System.Drawing.Point(23, 174);
			this.sourceLabel.Name = "sourceLabel";
			this.sourceLabel.Size = new System.Drawing.Size(60, 20);
			this.sourceLabel.TabIndex = 8;
			this.sourceLabel.Text = "Source";
			this.sourceLabel.ThemedBack = null;
			this.sourceLabel.ThemedFore = null;
			// 
			// profileLabel
			// 
			this.profileLabel.AutoSize = true;
			this.profileLabel.Location = new System.Drawing.Point(23, 144);
			this.profileLabel.Name = "profileLabel";
			this.profileLabel.Size = new System.Drawing.Size(53, 20);
			this.profileLabel.TabIndex = 7;
			this.profileLabel.Text = "Profile";
			this.profileLabel.ThemedBack = null;
			this.profileLabel.ThemedFore = null;
			// 
			// modelLabel
			// 
			this.modelLabel.AutoSize = true;
			this.modelLabel.Location = new System.Drawing.Point(165, 82);
			this.modelLabel.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
			this.modelLabel.Name = "modelLabel";
			this.modelLabel.Size = new System.Drawing.Size(14, 20);
			this.modelLabel.TabIndex = 6;
			this.modelLabel.Text = "-";
			this.modelLabel.ThemedBack = null;
			this.modelLabel.ThemedFore = null;
			// 
			// ScanDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(607, 554);
			this.Controls.Add(this.contentPanel);
			this.Controls.Add(this.morePanel1);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ScanDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Scan";
			this.morePanel1.ResumeLayout(false);
			this.contentPanel.ResumeLayout(false);
			this.contentPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.contrastBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.brightnessBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.contrastSlider)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.brightnessSlider)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MorePanel morePanel1;
		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private UI.MoreLabel scannerLabel;
		private System.Windows.Forms.ComboBox scannerBox;
		private UI.MorePanel contentPanel;
		private System.Windows.Forms.ComboBox sourceBox;
		private System.Windows.Forms.ComboBox profileBox;
		private UI.MoreLabel sizeLabel;
		private UI.MoreLabel sourceLabel;
		private UI.MoreLabel profileLabel;
		private UI.MoreLabel modelLabel;
		private UI.MoreLabel resolutionLabel;
		private System.Windows.Forms.ComboBox resolutionBox;
		private UI.MoreLabel colorLabel;
		private System.Windows.Forms.ComboBox colorBox;
		private System.Windows.Forms.ComboBox sizeBox;
		private System.Windows.Forms.TrackBar brightnessSlider;
		private UI.MoreLabel constrastLabel;
		private UI.MoreLabel brightnessLabel;
		private System.Windows.Forms.TrackBar contrastSlider;
		private System.Windows.Forms.ProgressBar progressBar;
		private UI.MoreNumericUpDown contrastBox;
		private UI.MoreNumericUpDown brightnessBox;
	}
}