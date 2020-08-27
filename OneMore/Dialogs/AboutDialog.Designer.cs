namespace River.OneMoreAddIn.Dialogs
{
	partial class AboutDialog
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose (bool disposing)
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
		private void InitializeComponent ()
		{
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.titleLabel = new System.Windows.Forms.Label();
			this.versionLabel = new System.Windows.Forms.Label();
			this.copyLabel = new System.Windows.Forms.Label();
			this.okButton = new System.Windows.Forms.Button();
			this.logLabel = new System.Windows.Forms.LinkLabel();
			this.clearLogLabel = new System.Windows.Forms.LinkLabel();
			this.homeLink = new System.Windows.Forms.LinkLabel();
			this.updateLink = new System.Windows.Forms.LinkLabel();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = global::River.OneMoreAddIn.Properties.Resources.Logo;
			this.pictureBox1.Location = new System.Drawing.Point(8, 8);
			this.pictureBox1.Margin = new System.Windows.Forms.Padding(2);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(83, 81);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// titleLabel
			// 
			this.titleLabel.AutoSize = true;
			this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.titleLabel.Location = new System.Drawing.Point(104, 17);
			this.titleLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.titleLabel.Name = "titleLabel";
			this.titleLabel.Size = new System.Drawing.Size(193, 15);
			this.titleLabel.TabIndex = 1;
			this.titleLabel.Text = "OneMore Add-in for OneNote 2016";
			// 
			// versionLabel
			// 
			this.versionLabel.AutoSize = true;
			this.versionLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.versionLabel.Location = new System.Drawing.Point(104, 32);
			this.versionLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.versionLabel.Name = "versionLabel";
			this.versionLabel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
			this.versionLabel.Size = new System.Drawing.Size(63, 20);
			this.versionLabel.TabIndex = 2;
			this.versionLabel.Text = "Version 1.0";
			// 
			// copyLabel
			// 
			this.copyLabel.AutoSize = true;
			this.copyLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.copyLabel.Location = new System.Drawing.Point(104, 52);
			this.copyLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.copyLabel.Name = "copyLabel";
			this.copyLabel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
			this.copyLabel.Size = new System.Drawing.Size(214, 20);
			this.copyLabel.TabIndex = 3;
			this.copyLabel.Text = "Copyright @ 2016-2020 Steven M Cohn";
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.okButton.Location = new System.Drawing.Point(306, 155);
			this.okButton.Margin = new System.Windows.Forms.Padding(2);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(69, 27);
			this.okButton.TabIndex = 4;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OK);
			// 
			// logLabel
			// 
			this.logLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.logLabel.AutoSize = true;
			this.logLabel.LinkColor = System.Drawing.SystemColors.ControlDark;
			this.logLabel.Location = new System.Drawing.Point(9, 155);
			this.logLabel.MaximumSize = new System.Drawing.Size(280, 0);
			this.logLabel.Name = "logLabel";
			this.logLabel.Size = new System.Drawing.Size(43, 13);
			this.logLabel.TabIndex = 5;
			this.logLabel.TabStop = true;
			this.logLabel.Text = "tempfile";
			this.logLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OpenLog);
			// 
			// clearLogLabel
			// 
			this.clearLogLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.clearLogLabel.AutoSize = true;
			this.clearLogLabel.LinkColor = System.Drawing.SystemColors.ControlDark;
			this.clearLogLabel.Location = new System.Drawing.Point(8, 172);
			this.clearLogLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.clearLogLabel.Name = "clearLogLabel";
			this.clearLogLabel.Size = new System.Drawing.Size(82, 13);
			this.clearLogLabel.TabIndex = 6;
			this.clearLogLabel.TabStop = true;
			this.clearLogLabel.Text = "Clear the log file";
			this.clearLogLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ClearLog);
			// 
			// homeLink
			// 
			this.homeLink.AutoSize = true;
			this.homeLink.LinkColor = System.Drawing.SystemColors.Highlight;
			this.homeLink.Location = new System.Drawing.Point(104, 72);
			this.homeLink.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.homeLink.Name = "homeLink";
			this.homeLink.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
			this.homeLink.Size = new System.Drawing.Size(205, 23);
			this.homeLink.TabIndex = 7;
			this.homeLink.TabStop = true;
			this.homeLink.Text = "https://github.com/stevencohn/OneMore";
			this.homeLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GoHome);
			// 
			// updateLink
			// 
			this.updateLink.AutoSize = true;
			this.updateLink.LinkColor = System.Drawing.SystemColors.Highlight;
			this.updateLink.Location = new System.Drawing.Point(104, 95);
			this.updateLink.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.updateLink.Name = "updateLink";
			this.updateLink.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
			this.updateLink.Size = new System.Drawing.Size(96, 23);
			this.updateLink.TabIndex = 11;
			this.updateLink.TabStop = true;
			this.updateLink.Text = "Check for Updates";
			this.updateLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.CheckForUpdates);
			// 
			// AboutDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.CancelButton = this.okButton;
			this.ClientSize = new System.Drawing.Size(384, 191);
			this.Controls.Add(this.updateLink);
			this.Controls.Add(this.homeLink);
			this.Controls.Add(this.clearLogLabel);
			this.Controls.Add(this.logLabel);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.copyLabel);
			this.Controls.Add(this.versionLabel);
			this.Controls.Add(this.titleLabel);
			this.Controls.Add(this.pictureBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(2);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AboutDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "OneMore Add-in";
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label titleLabel;
		private System.Windows.Forms.Label versionLabel;
		private System.Windows.Forms.Label copyLabel;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.LinkLabel logLabel;
		private System.Windows.Forms.LinkLabel clearLogLabel;
		private System.Windows.Forms.LinkLabel homeLink;
		private System.Windows.Forms.LinkLabel updateLink;
	}
}