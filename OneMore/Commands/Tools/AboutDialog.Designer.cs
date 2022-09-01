namespace River.OneMoreAddIn.Commands
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
			this.logLabel = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.clearLogLabel = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.homeLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.updateLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.pleaseLabel = new System.Windows.Forms.Label();
			this.sponsorButton = new River.OneMoreAddIn.UI.MoreButton();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = global::River.OneMoreAddIn.Properties.Resources.Logo;
			this.pictureBox1.Location = new System.Drawing.Point(12, 12);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(124, 125);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// titleLabel
			// 
			this.titleLabel.AutoSize = true;
			this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.titleLabel.Location = new System.Drawing.Point(156, 26);
			this.titleLabel.Name = "titleLabel";
			this.titleLabel.Size = new System.Drawing.Size(312, 25);
			this.titleLabel.TabIndex = 1;
			this.titleLabel.Text = "OneMore Add-in for OneNote 2016";
			// 
			// versionLabel
			// 
			this.versionLabel.AutoSize = true;
			this.versionLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.versionLabel.Location = new System.Drawing.Point(156, 49);
			this.versionLabel.Name = "versionLabel";
			this.versionLabel.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
			this.versionLabel.Size = new System.Drawing.Size(99, 33);
			this.versionLabel.TabIndex = 2;
			this.versionLabel.Text = "Version 1.0";
			// 
			// copyLabel
			// 
			this.copyLabel.AutoSize = true;
			this.copyLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.copyLabel.Location = new System.Drawing.Point(156, 80);
			this.copyLabel.Name = "copyLabel";
			this.copyLabel.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
			this.copyLabel.Size = new System.Drawing.Size(330, 33);
			this.copyLabel.TabIndex = 3;
			this.copyLabel.Text = "Copyright @ 2016-2020 Steven M Cohn";
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.okButton.Location = new System.Drawing.Point(591, 341);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(104, 42);
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OK);
			// 
			// logLabel
			// 
			this.logLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.logLabel.AutoSize = true;
			this.logLabel.Cursor = System.Windows.Forms.Cursors.Hand;
			this.logLabel.LinkColor = System.Drawing.SystemColors.ControlDark;
			this.logLabel.Location = new System.Drawing.Point(14, 341);
			this.logLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.logLabel.MaximumSize = new System.Drawing.Size(420, 0);
			this.logLabel.Name = "logLabel";
			this.logLabel.Size = new System.Drawing.Size(65, 20);
			this.logLabel.TabIndex = 4;
			this.logLabel.TabStop = true;
			this.logLabel.Text = "tempfile";
			this.logLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OpenLog);
			// 
			// clearLogLabel
			// 
			this.clearLogLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.clearLogLabel.AutoSize = true;
			this.clearLogLabel.Cursor = System.Windows.Forms.Cursors.Hand;
			this.clearLogLabel.LinkColor = System.Drawing.SystemColors.ControlDark;
			this.clearLogLabel.Location = new System.Drawing.Point(12, 368);
			this.clearLogLabel.Name = "clearLogLabel";
			this.clearLogLabel.Size = new System.Drawing.Size(122, 20);
			this.clearLogLabel.TabIndex = 5;
			this.clearLogLabel.TabStop = true;
			this.clearLogLabel.Text = "Clear the log file";
			this.clearLogLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ClearLog);
			// 
			// homeLink
			// 
			this.homeLink.AutoSize = true;
			this.homeLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.homeLink.LinkColor = System.Drawing.SystemColors.Highlight;
			this.homeLink.Location = new System.Drawing.Point(156, 119);
			this.homeLink.Name = "homeLink";
			this.homeLink.Padding = new System.Windows.Forms.Padding(0, 15, 0, 0);
			this.homeLink.Size = new System.Drawing.Size(291, 35);
			this.homeLink.TabIndex = 1;
			this.homeLink.TabStop = true;
			this.homeLink.Text = "https://github.com/stevencohn/OneMore";
			this.homeLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GoHome);
			// 
			// updateLink
			// 
			this.updateLink.AutoSize = true;
			this.updateLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.updateLink.LinkColor = System.Drawing.SystemColors.Highlight;
			this.updateLink.Location = new System.Drawing.Point(156, 154);
			this.updateLink.Name = "updateLink";
			this.updateLink.Padding = new System.Windows.Forms.Padding(0, 15, 0, 0);
			this.updateLink.Size = new System.Drawing.Size(142, 35);
			this.updateLink.TabIndex = 2;
			this.updateLink.TabStop = true;
			this.updateLink.Text = "Check for Updates";
			this.updateLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.CheckForUpdates);
			// 
			// pleaseLabel
			// 
			this.pleaseLabel.AutoSize = true;
			this.pleaseLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pleaseLabel.Location = new System.Drawing.Point(156, 215);
			this.pleaseLabel.Name = "pleaseLabel";
			this.pleaseLabel.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
			this.pleaseLabel.Size = new System.Drawing.Size(393, 33);
			this.pleaseLabel.TabIndex = 12;
			this.pleaseLabel.Text = "Please support future development of OneMore";
			// 
			// sponsorButton
			// 
			this.sponsorButton.AutoSize = true;
			this.sponsorButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.sponsorButton.FlatAppearance.BorderSize = 0;
			this.sponsorButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.sponsorButton.Image = global::River.OneMoreAddIn.Properties.Resources.Sponsor;
			this.sponsorButton.Location = new System.Drawing.Point(160, 251);
			this.sponsorButton.Name = "sponsorButton";
			this.sponsorButton.Size = new System.Drawing.Size(149, 53);
			this.sponsorButton.TabIndex = 3;
			this.sponsorButton.Tag = "https://github.com/sponsors/stevencohn";
			this.sponsorButton.UseVisualStyleBackColor = true;
			this.sponsorButton.Click += new System.EventHandler(this.GotoSponsorship);
			this.sponsorButton.MouseEnter += new System.EventHandler(this.EnterSponsor);
			this.sponsorButton.MouseLeave += new System.EventHandler(this.LeaveSponsor);
			// 
			// AboutDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.CancelButton = this.okButton;
			this.ClientSize = new System.Drawing.Size(708, 397);
			this.Controls.Add(this.sponsorButton);
			this.Controls.Add(this.pleaseLabel);
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
		private UI.MoreLinkLabel logLabel;
		private UI.MoreLinkLabel clearLogLabel;
		private UI.MoreLinkLabel homeLink;
		private UI.MoreLinkLabel updateLink;
		private System.Windows.Forms.Label pleaseLabel;
		private UI.MoreButton sponsorButton;
	}
}