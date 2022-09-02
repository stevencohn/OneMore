namespace OneMoreCalendar
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutDialog));
			this.titleLabel = new System.Windows.Forms.Label();
			this.versionLabel = new System.Windows.Forms.Label();
			this.copyLabel = new System.Windows.Forms.Label();
			this.okButton = new OneMoreCalendar.MoreButton();
			this.pleaseLabel = new System.Windows.Forms.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.sponsorButton = new River.OneMoreAddIn.UI.MoreButton();
			this.homeLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// titleLabel
			// 
			this.titleLabel.AutoSize = true;
			this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.titleLabel.Location = new System.Drawing.Point(156, 26);
			this.titleLabel.Name = "titleLabel";
			this.titleLabel.Size = new System.Drawing.Size(329, 25);
			this.titleLabel.TabIndex = 1;
			this.titleLabel.Text = "OneMore Calendar for OneNote 2016";
			// 
			// versionLabel
			// 
			this.versionLabel.AutoSize = true;
			this.versionLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.versionLabel.Location = new System.Drawing.Point(156, 49);
			this.versionLabel.Name = "versionLabel";
			this.versionLabel.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
			this.versionLabel.Size = new System.Drawing.Size(95, 33);
			this.versionLabel.TabIndex = 2;
			this.versionLabel.Text = "Version {0}";
			// 
			// copyLabel
			// 
			this.copyLabel.AutoSize = true;
			this.copyLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.copyLabel.Location = new System.Drawing.Point(156, 80);
			this.copyLabel.Name = "copyLabel";
			this.copyLabel.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
			this.copyLabel.Size = new System.Drawing.Size(310, 33);
			this.copyLabel.TabIndex = 3;
			this.copyLabel.Text = "Copyright @ 2021-{0} Steven M Cohn";
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.okButton.Location = new System.Drawing.Point(612, 352);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(91, 38);
			this.okButton.TabIndex = 4;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OK);
			// 
			// pleaseLabel
			// 
			this.pleaseLabel.AutoSize = true;
			this.pleaseLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pleaseLabel.Location = new System.Drawing.Point(156, 178);
			this.pleaseLabel.Name = "pleaseLabel";
			this.pleaseLabel.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
			this.pleaseLabel.Size = new System.Drawing.Size(393, 33);
			this.pleaseLabel.TabIndex = 14;
			this.pleaseLabel.Text = "Please support future development of OneMore";
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(12, 12);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(124, 125);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// sponsorButton
			// 
			this.sponsorButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.sponsorButton.FlatAppearance.BorderSize = 0;
			this.sponsorButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.sponsorButton.Image = ((System.Drawing.Image)(resources.GetObject("sponsorButton.Image")));
			this.sponsorButton.Location = new System.Drawing.Point(160, 214);
			this.sponsorButton.Name = "sponsorButton";
			this.sponsorButton.Size = new System.Drawing.Size(149, 53);
			this.sponsorButton.TabIndex = 13;
			this.sponsorButton.Tag = "https://github.com/sponsors/stevencohn";
			this.sponsorButton.Text = " ";
			this.sponsorButton.UseVisualStyleBackColor = true;
			this.sponsorButton.Click += new System.EventHandler(this.GotoSponsorship);
			this.sponsorButton.MouseEnter += new System.EventHandler(this.EnterSponsor);
			this.sponsorButton.MouseLeave += new System.EventHandler(this.LeaveSponsor);
			// 
			// homeLink
			// 
			this.homeLink.ActiveLinkColor = System.Drawing.Color.DarkOrchid;
			this.homeLink.AutoSize = true;
			this.homeLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.homeLink.HoverColor = System.Drawing.Color.MediumOrchid;
			this.homeLink.LinkColor = System.Drawing.Color.DarkGray;
			this.homeLink.Location = new System.Drawing.Point(156, 119);
			this.homeLink.Name = "homeLink";
			this.homeLink.Padding = new System.Windows.Forms.Padding(0, 15, 0, 0);
			this.homeLink.Size = new System.Drawing.Size(291, 35);
			this.homeLink.TabIndex = 7;
			this.homeLink.TabStop = true;
			this.homeLink.Text = "https://github.com/stevencohn/OneMore";
			this.homeLink.VisitedLinkColor = System.Drawing.Color.DarkGray;
			this.homeLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GoHome);
			// 
			// AboutDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.CancelButton = this.okButton;
			this.ClientSize = new System.Drawing.Size(716, 404);
			this.Controls.Add(this.sponsorButton);
			this.Controls.Add(this.pleaseLabel);
			this.Controls.Add(this.homeLink);
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
			this.Text = "OneMore Calendar";
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label titleLabel;
		private System.Windows.Forms.Label versionLabel;
		private System.Windows.Forms.Label copyLabel;
		private MoreButton okButton;
		private River.OneMoreAddIn.UI.MoreLinkLabel homeLink;
		private River.OneMoreAddIn.UI.MoreButton sponsorButton;
		private System.Windows.Forms.Label pleaseLabel;
	}
}