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
			this.versionLabel = new River.OneMoreAddIn.UI.MoreTextBox();
			this.copyLabel = new System.Windows.Forms.Label();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.logLabel = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.clearLogLabel = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.homeLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.updateLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.pleaseLabel = new System.Windows.Forms.Label();
			this.sponsorButton = new River.OneMoreAddIn.UI.MoreButton();
			this.githubLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.topPanel = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.topPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
			this.pictureBox1.Image = global::River.OneMoreAddIn.Properties.Resources.Logo;
			this.pictureBox1.Location = new System.Drawing.Point(590, 0);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(307, 292);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// titleLabel
			// 
			this.titleLabel.AutoSize = true;
			this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.titleLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.titleLabel.Location = new System.Drawing.Point(43, 20);
			this.titleLabel.Name = "titleLabel";
			this.titleLabel.Size = new System.Drawing.Size(411, 38);
			this.titleLabel.TabIndex = 1;
			this.titleLabel.Text = "OneMore Add-in for OneNote";
			// 
			// versionLabel
			// 
			this.versionLabel.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.versionLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.versionLabel.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.versionLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.versionLabel.Location = new System.Drawing.Point(50, 61);
			this.versionLabel.Name = "versionLabel";
			this.versionLabel.ReadOnly = true;
			this.versionLabel.Size = new System.Drawing.Size(534, 30);
			this.versionLabel.TabIndex = 2;
			this.versionLabel.Text = "Version 1.0";
			this.versionLabel.ThemedBack = "ControlLightLight";
			this.versionLabel.ThemedFore = null;
			// 
			// copyLabel
			// 
			this.copyLabel.AutoSize = true;
			this.copyLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.copyLabel.Location = new System.Drawing.Point(46, 230);
			this.copyLabel.Margin = new System.Windows.Forms.Padding(3, 20, 3, 0);
			this.copyLabel.Name = "copyLabel";
			this.copyLabel.Size = new System.Drawing.Size(427, 20);
			this.copyLabel.TabIndex = 3;
			this.copyLabel.Text = "Copyright @ 2016-2020 Steven M Cohn. All rights reserved.";
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.okButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(748, 502);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(130, 42);
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = false;
			this.okButton.Click += new System.EventHandler(this.OK);
			// 
			// logLabel
			// 
			this.logLabel.ActiveLinkColor = System.Drawing.Color.DarkOrchid;
			this.logLabel.AutoSize = true;
			this.logLabel.Cursor = System.Windows.Forms.Cursors.Hand;
			this.logLabel.ForeColor = System.Drawing.SystemColors.GrayText;
			this.logLabel.HoverColor = System.Drawing.Color.MediumOrchid;
			this.logLabel.LinkColor = System.Drawing.SystemColors.GrayText;
			this.logLabel.Location = new System.Drawing.Point(46, 442);
			this.logLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.logLabel.MaximumSize = new System.Drawing.Size(420, 0);
			this.logLabel.Name = "logLabel";
			this.logLabel.Size = new System.Drawing.Size(65, 20);
			this.logLabel.StrictColors = false;
			this.logLabel.TabIndex = 5;
			this.logLabel.TabStop = true;
			this.logLabel.Text = "tempfile";
			this.logLabel.ThemedBack = null;
			this.logLabel.ThemedFore = "GrayText";
			this.logLabel.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.logLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OpenLog);
			// 
			// clearLogLabel
			// 
			this.clearLogLabel.ActiveLinkColor = System.Drawing.Color.DarkOrchid;
			this.clearLogLabel.AutoSize = true;
			this.clearLogLabel.Cursor = System.Windows.Forms.Cursors.Hand;
			this.clearLogLabel.ForeColor = System.Drawing.SystemColors.GrayText;
			this.clearLogLabel.HoverColor = System.Drawing.Color.MediumOrchid;
			this.clearLogLabel.LinkColor = System.Drawing.SystemColors.GrayText;
			this.clearLogLabel.Location = new System.Drawing.Point(46, 477);
			this.clearLogLabel.Margin = new System.Windows.Forms.Padding(3, 15, 3, 0);
			this.clearLogLabel.Name = "clearLogLabel";
			this.clearLogLabel.Size = new System.Drawing.Size(122, 20);
			this.clearLogLabel.StrictColors = false;
			this.clearLogLabel.TabIndex = 6;
			this.clearLogLabel.TabStop = true;
			this.clearLogLabel.Text = "Clear the log file";
			this.clearLogLabel.ThemedBack = null;
			this.clearLogLabel.ThemedFore = "GrayText";
			this.clearLogLabel.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.clearLogLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ClearLog);
			// 
			// homeLink
			// 
			this.homeLink.ActiveLinkColor = System.Drawing.Color.DarkOrchid;
			this.homeLink.AutoSize = true;
			this.homeLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.homeLink.HoverColor = System.Drawing.Color.Orchid;
			this.homeLink.LinkColor = System.Drawing.Color.MediumOrchid;
			this.homeLink.Location = new System.Drawing.Point(46, 300);
			this.homeLink.Margin = new System.Windows.Forms.Padding(3, 15, 3, 0);
			this.homeLink.Name = "homeLink";
			this.homeLink.Size = new System.Drawing.Size(193, 20);
			this.homeLink.StrictColors = false;
			this.homeLink.TabIndex = 3;
			this.homeLink.TabStop = true;
			this.homeLink.Text = "https://onemoreaddin.com";
			this.homeLink.ThemedBack = null;
			this.homeLink.ThemedFore = null;
			this.homeLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.homeLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GoHome);
			// 
			// updateLink
			// 
			this.updateLink.ActiveLinkColor = System.Drawing.Color.DarkOrchid;
			this.updateLink.AutoSize = true;
			this.updateLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.updateLink.HoverColor = System.Drawing.Color.Orchid;
			this.updateLink.LinkColor = System.Drawing.Color.MediumOrchid;
			this.updateLink.Location = new System.Drawing.Point(46, 265);
			this.updateLink.Margin = new System.Windows.Forms.Padding(3, 15, 3, 0);
			this.updateLink.Name = "updateLink";
			this.updateLink.Size = new System.Drawing.Size(142, 20);
			this.updateLink.StrictColors = false;
			this.updateLink.TabIndex = 2;
			this.updateLink.TabStop = true;
			this.updateLink.Text = "Check for Updates";
			this.updateLink.ThemedBack = null;
			this.updateLink.ThemedFore = null;
			this.updateLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.updateLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.CheckForUpdates);
			// 
			// pleaseLabel
			// 
			this.pleaseLabel.AutoSize = true;
			this.pleaseLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pleaseLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.pleaseLabel.Location = new System.Drawing.Point(45, 370);
			this.pleaseLabel.Margin = new System.Windows.Forms.Padding(3, 15, 3, 0);
			this.pleaseLabel.Name = "pleaseLabel";
			this.pleaseLabel.Size = new System.Drawing.Size(393, 25);
			this.pleaseLabel.TabIndex = 12;
			this.pleaseLabel.Text = "Please support future development of OneMore";
			// 
			// sponsorButton
			// 
			this.sponsorButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.sponsorButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.sponsorButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.sponsorButton.FlatAppearance.BorderSize = 0;
			this.sponsorButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.sponsorButton.ImageOver = null;
			this.sponsorButton.Location = new System.Drawing.Point(444, 362);
			this.sponsorButton.Name = "sponsorButton";
			this.sponsorButton.ShowBorder = true;
			this.sponsorButton.Size = new System.Drawing.Size(141, 45);
			this.sponsorButton.TabIndex = 1;
			this.sponsorButton.ThemedBack = null;
			this.sponsorButton.ThemedFore = null;
			this.sponsorButton.UseVisualStyleBackColor = true;
			this.sponsorButton.Click += new System.EventHandler(this.GotoSponsorship);
			// 
			// githubLink
			// 
			this.githubLink.ActiveLinkColor = System.Drawing.Color.DarkOrchid;
			this.githubLink.AutoSize = true;
			this.githubLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.githubLink.HoverColor = System.Drawing.Color.Orchid;
			this.githubLink.LinkColor = System.Drawing.Color.MediumOrchid;
			this.githubLink.Location = new System.Drawing.Point(46, 335);
			this.githubLink.Margin = new System.Windows.Forms.Padding(3, 15, 3, 0);
			this.githubLink.Name = "githubLink";
			this.githubLink.Size = new System.Drawing.Size(194, 20);
			this.githubLink.StrictColors = false;
			this.githubLink.TabIndex = 4;
			this.githubLink.TabStop = true;
			this.githubLink.Text = "See the project on GitHub";
			this.githubLink.ThemedBack = null;
			this.githubLink.ThemedFore = null;
			this.githubLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.githubLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GoGitHub);
			// 
			// topPanel
			// 
			this.topPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.topPanel.Controls.Add(this.titleLabel);
			this.topPanel.Controls.Add(this.versionLabel);
			this.topPanel.Controls.Add(this.pictureBox1);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = new System.Drawing.Point(0, 0);
			this.topPanel.Name = "topPanel";
			this.topPanel.Padding = new System.Windows.Forms.Padding(40, 20, 0, 0);
			this.topPanel.Size = new System.Drawing.Size(890, 207);
			this.topPanel.TabIndex = 14;
			// 
			// AboutDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CancelButton = this.okButton;
			this.ClientSize = new System.Drawing.Size(890, 556);
			this.Controls.Add(this.topPanel);
			this.Controls.Add(this.githubLink);
			this.Controls.Add(this.sponsorButton);
			this.Controls.Add(this.pleaseLabel);
			this.Controls.Add(this.updateLink);
			this.Controls.Add(this.homeLink);
			this.Controls.Add(this.clearLogLabel);
			this.Controls.Add(this.logLabel);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.copyLabel);
			this.ForeColor = System.Drawing.Color.Black;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AboutDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "OneMore Add-in";
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label titleLabel;
		private UI.MoreTextBox versionLabel;
		private System.Windows.Forms.Label copyLabel;
		private UI.MoreButton okButton;
		private UI.MoreLinkLabel logLabel;
		private UI.MoreLinkLabel clearLogLabel;
		private UI.MoreLinkLabel homeLink;
		private UI.MoreLinkLabel updateLink;
		private System.Windows.Forms.Label pleaseLabel;
		private UI.MoreButton sponsorButton;
		private UI.MoreLinkLabel githubLink;
		private System.Windows.Forms.Panel topPanel;
	}
}