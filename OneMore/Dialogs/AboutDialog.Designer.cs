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
			this.okButton.Location = new System.Drawing.Point(471, 238);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(104, 42);
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
			this.logLabel.Location = new System.Drawing.Point(14, 238);
			this.logLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.logLabel.MaximumSize = new System.Drawing.Size(420, 0);
			this.logLabel.Name = "logLabel";
			this.logLabel.Size = new System.Drawing.Size(65, 20);
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
			this.clearLogLabel.Location = new System.Drawing.Point(12, 265);
			this.clearLogLabel.Name = "clearLogLabel";
			this.clearLogLabel.Size = new System.Drawing.Size(122, 20);
			this.clearLogLabel.TabIndex = 6;
			this.clearLogLabel.TabStop = true;
			this.clearLogLabel.Text = "Clear the log file";
			this.clearLogLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ClearLog);
			// 
			// homeLink
			// 
			this.homeLink.AutoSize = true;
			this.homeLink.LinkColor = System.Drawing.SystemColors.Highlight;
			this.homeLink.Location = new System.Drawing.Point(156, 119);
			this.homeLink.Name = "homeLink";
			this.homeLink.Padding = new System.Windows.Forms.Padding(0, 15, 0, 0);
			this.homeLink.Size = new System.Drawing.Size(291, 35);
			this.homeLink.TabIndex = 7;
			this.homeLink.TabStop = true;
			this.homeLink.Text = "https://github.com/stevencohn/OneMore";
			this.homeLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GoHome);
			// 
			// updateLink
			// 
			this.updateLink.AutoSize = true;
			this.updateLink.LinkColor = System.Drawing.SystemColors.Highlight;
			this.updateLink.Location = new System.Drawing.Point(156, 154);
			this.updateLink.Name = "updateLink";
			this.updateLink.Padding = new System.Windows.Forms.Padding(0, 15, 0, 0);
			this.updateLink.Size = new System.Drawing.Size(142, 35);
			this.updateLink.TabIndex = 11;
			this.updateLink.TabStop = true;
			this.updateLink.Text = "Check for Updates";
			this.updateLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.CheckForUpdates);
			// 
			// AboutDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.CancelButton = this.okButton;
			this.ClientSize = new System.Drawing.Size(588, 294);
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
		private System.Windows.Forms.LinkLabel logLabel;
		private System.Windows.Forms.LinkLabel clearLogLabel;
		private System.Windows.Forms.LinkLabel homeLink;
		private System.Windows.Forms.LinkLabel updateLink;
	}
}