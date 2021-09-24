
namespace River.OneMoreAddIn.Commands
{
	partial class UpdateDialog
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateDialog));
			this.currentLabel = new System.Windows.Forms.Label();
			this.versionLabel = new System.Windows.Forms.Label();
			this.lastUpdatedLabel = new System.Windows.Forms.Label();
			this.releaseNotesLink = new UI.MoreLinkLabel();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.versionBox = new System.Windows.Forms.Label();
			this.lastUpdatedBox = new System.Windows.Forms.Label();
			this.readyPanel = new System.Windows.Forms.Panel();
			this.updatePanel = new System.Windows.Forms.Panel();
			this.upReleaseDateBox = new System.Windows.Forms.Label();
			this.releaseDateLabel = new System.Windows.Forms.Label();
			this.upLastUpdatedBox = new System.Windows.Forms.Label();
			this.upLastUpdatedLabel = new System.Windows.Forms.Label();
			this.upCurrentVersionLabel = new System.Windows.Forms.Label();
			this.upCurrentVersionBox = new System.Windows.Forms.Label();
			this.lineLabel = new System.Windows.Forms.Label();
			this.upDescriptionBox = new System.Windows.Forms.Label();
			this.upVersionLabel = new System.Windows.Forms.Label();
			this.upVersionBox = new System.Windows.Forms.Label();
			this.upDescriptionLabel = new System.Windows.Forms.Label();
			this.upReleaseNotesLink = new UI.MoreLinkLabel();
			this.upIntroLabel = new System.Windows.Forms.Label();
			this.upOKButton = new System.Windows.Forms.Button();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.readyPanel.SuspendLayout();
			this.updatePanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// currentLabel
			// 
			this.currentLabel.AutoSize = true;
			this.currentLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.currentLabel.Location = new System.Drawing.Point(3, 1);
			this.currentLabel.Name = "currentLabel";
			this.currentLabel.Size = new System.Drawing.Size(205, 25);
			this.currentLabel.TabIndex = 0;
			this.currentLabel.Text = "OneMore is up to date";
			// 
			// versionLabel
			// 
			this.versionLabel.AutoSize = true;
			this.versionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.versionLabel.Location = new System.Drawing.Point(31, 46);
			this.versionLabel.Name = "versionLabel";
			this.versionLabel.Size = new System.Drawing.Size(145, 20);
			this.versionLabel.TabIndex = 1;
			this.versionLabel.Text = "Installed version:";
			// 
			// lastUpdatedLabel
			// 
			this.lastUpdatedLabel.AutoSize = true;
			this.lastUpdatedLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lastUpdatedLabel.Location = new System.Drawing.Point(31, 74);
			this.lastUpdatedLabel.Name = "lastUpdatedLabel";
			this.lastUpdatedLabel.Size = new System.Drawing.Size(120, 20);
			this.lastUpdatedLabel.TabIndex = 2;
			this.lastUpdatedLabel.Text = "Last updated:";
			// 
			// releaseNotesLink
			// 
			this.releaseNotesLink.AutoSize = true;
			this.releaseNotesLink.Location = new System.Drawing.Point(31, 104);
			this.releaseNotesLink.Name = "releaseNotesLink";
			this.releaseNotesLink.Size = new System.Drawing.Size(112, 20);
			this.releaseNotesLink.TabIndex = 5;
			this.releaseNotesLink.TabStop = true;
			this.releaseNotesLink.Text = "Release notes";
			this.releaseNotesLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GotoReleaseNotes);
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(480, 142);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(104, 42);
			this.okButton.TabIndex = 6;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(480, 254);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(104, 42);
			this.cancelButton.TabIndex = 7;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// versionBox
			// 
			this.versionBox.AutoSize = true;
			this.versionBox.Location = new System.Drawing.Point(204, 46);
			this.versionBox.Name = "versionBox";
			this.versionBox.Size = new System.Drawing.Size(59, 20);
			this.versionBox.TabIndex = 8;
			this.versionBox.Text = "version";
			// 
			// lastUpdatedBox
			// 
			this.lastUpdatedBox.AutoSize = true;
			this.lastUpdatedBox.Location = new System.Drawing.Point(204, 74);
			this.lastUpdatedBox.Name = "lastUpdatedBox";
			this.lastUpdatedBox.Size = new System.Drawing.Size(41, 20);
			this.lastUpdatedBox.TabIndex = 9;
			this.lastUpdatedBox.Text = "date";
			// 
			// readyPanel
			// 
			this.readyPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.readyPanel.Controls.Add(this.currentLabel);
			this.readyPanel.Controls.Add(this.lastUpdatedBox);
			this.readyPanel.Controls.Add(this.okButton);
			this.readyPanel.Controls.Add(this.versionLabel);
			this.readyPanel.Controls.Add(this.versionBox);
			this.readyPanel.Controls.Add(this.lastUpdatedLabel);
			this.readyPanel.Controls.Add(this.releaseNotesLink);
			this.readyPanel.Location = new System.Drawing.Point(23, 23);
			this.readyPanel.Name = "readyPanel";
			this.readyPanel.Size = new System.Drawing.Size(587, 187);
			this.readyPanel.TabIndex = 10;
			// 
			// updatePanel
			// 
			this.updatePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.updatePanel.Controls.Add(this.upReleaseDateBox);
			this.updatePanel.Controls.Add(this.releaseDateLabel);
			this.updatePanel.Controls.Add(this.upLastUpdatedBox);
			this.updatePanel.Controls.Add(this.upLastUpdatedLabel);
			this.updatePanel.Controls.Add(this.upCurrentVersionLabel);
			this.updatePanel.Controls.Add(this.upCurrentVersionBox);
			this.updatePanel.Controls.Add(this.lineLabel);
			this.updatePanel.Controls.Add(this.upDescriptionBox);
			this.updatePanel.Controls.Add(this.upVersionLabel);
			this.updatePanel.Controls.Add(this.upVersionBox);
			this.updatePanel.Controls.Add(this.upDescriptionLabel);
			this.updatePanel.Controls.Add(this.upReleaseNotesLink);
			this.updatePanel.Controls.Add(this.upIntroLabel);
			this.updatePanel.Controls.Add(this.upOKButton);
			this.updatePanel.Controls.Add(this.cancelButton);
			this.updatePanel.Location = new System.Drawing.Point(23, 216);
			this.updatePanel.Name = "updatePanel";
			this.updatePanel.Size = new System.Drawing.Size(587, 299);
			this.updatePanel.TabIndex = 11;
			// 
			// upReleaseDateBox
			// 
			this.upReleaseDateBox.AutoSize = true;
			this.upReleaseDateBox.Location = new System.Drawing.Point(204, 101);
			this.upReleaseDateBox.Name = "upReleaseDateBox";
			this.upReleaseDateBox.Size = new System.Drawing.Size(41, 20);
			this.upReleaseDateBox.TabIndex = 21;
			this.upReleaseDateBox.Text = "date";
			// 
			// releaseDateLabel
			// 
			this.releaseDateLabel.AutoSize = true;
			this.releaseDateLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.releaseDateLabel.Location = new System.Drawing.Point(31, 101);
			this.releaseDateLabel.Name = "releaseDateLabel";
			this.releaseDateLabel.Size = new System.Drawing.Size(121, 20);
			this.releaseDateLabel.TabIndex = 20;
			this.releaseDateLabel.Text = "Release date:";
			// 
			// upLastUpdatedBox
			// 
			this.upLastUpdatedBox.AutoSize = true;
			this.upLastUpdatedBox.Location = new System.Drawing.Point(204, 215);
			this.upLastUpdatedBox.Name = "upLastUpdatedBox";
			this.upLastUpdatedBox.Size = new System.Drawing.Size(41, 20);
			this.upLastUpdatedBox.TabIndex = 19;
			this.upLastUpdatedBox.Text = "date";
			// 
			// upLastUpdatedLabel
			// 
			this.upLastUpdatedLabel.AutoSize = true;
			this.upLastUpdatedLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.upLastUpdatedLabel.Location = new System.Drawing.Point(31, 215);
			this.upLastUpdatedLabel.Name = "upLastUpdatedLabel";
			this.upLastUpdatedLabel.Size = new System.Drawing.Size(120, 20);
			this.upLastUpdatedLabel.TabIndex = 18;
			this.upLastUpdatedLabel.Text = "Last updated:";
			// 
			// upCurrentVersionLabel
			// 
			this.upCurrentVersionLabel.AutoSize = true;
			this.upCurrentVersionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.upCurrentVersionLabel.Location = new System.Drawing.Point(31, 188);
			this.upCurrentVersionLabel.Name = "upCurrentVersionLabel";
			this.upCurrentVersionLabel.Size = new System.Drawing.Size(136, 20);
			this.upCurrentVersionLabel.TabIndex = 16;
			this.upCurrentVersionLabel.Text = "Current version:";
			// 
			// upCurrentVersionBox
			// 
			this.upCurrentVersionBox.AutoSize = true;
			this.upCurrentVersionBox.Location = new System.Drawing.Point(204, 188);
			this.upCurrentVersionBox.Name = "upCurrentVersionBox";
			this.upCurrentVersionBox.Size = new System.Drawing.Size(59, 20);
			this.upCurrentVersionBox.TabIndex = 17;
			this.upCurrentVersionBox.Text = "version";
			// 
			// lineLabel
			// 
			this.lineLabel.AutoSize = true;
			this.lineLabel.Location = new System.Drawing.Point(31, 152);
			this.lineLabel.Name = "lineLabel";
			this.lineLabel.Size = new System.Drawing.Size(342, 20);
			this.lineLabel.TabIndex = 15;
			this.lineLabel.Text = "_____________________________________";
			// 
			// upDescriptionBox
			// 
			this.upDescriptionBox.AutoSize = true;
			this.upDescriptionBox.Location = new System.Drawing.Point(204, 70);
			this.upDescriptionBox.Name = "upDescriptionBox";
			this.upDescriptionBox.Size = new System.Drawing.Size(86, 20);
			this.upDescriptionBox.TabIndex = 14;
			this.upDescriptionBox.Text = "description";
			// 
			// upVersionLabel
			// 
			this.upVersionLabel.AutoSize = true;
			this.upVersionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.upVersionLabel.Location = new System.Drawing.Point(31, 41);
			this.upVersionLabel.Name = "upVersionLabel";
			this.upVersionLabel.Size = new System.Drawing.Size(135, 20);
			this.upVersionLabel.TabIndex = 10;
			this.upVersionLabel.Text = "Update version:";
			// 
			// upVersionBox
			// 
			this.upVersionBox.AutoSize = true;
			this.upVersionBox.Location = new System.Drawing.Point(204, 41);
			this.upVersionBox.Name = "upVersionBox";
			this.upVersionBox.Size = new System.Drawing.Size(59, 20);
			this.upVersionBox.TabIndex = 13;
			this.upVersionBox.Text = "version";
			// 
			// upDescriptionLabel
			// 
			this.upDescriptionLabel.AutoSize = true;
			this.upDescriptionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.upDescriptionLabel.Location = new System.Drawing.Point(31, 70);
			this.upDescriptionLabel.Name = "upDescriptionLabel";
			this.upDescriptionLabel.Size = new System.Drawing.Size(105, 20);
			this.upDescriptionLabel.TabIndex = 11;
			this.upDescriptionLabel.Text = "Description:";
			// 
			// upReleaseNotesLink
			// 
			this.upReleaseNotesLink.AutoSize = true;
			this.upReleaseNotesLink.Location = new System.Drawing.Point(31, 132);
			this.upReleaseNotesLink.Name = "upReleaseNotesLink";
			this.upReleaseNotesLink.Size = new System.Drawing.Size(112, 20);
			this.upReleaseNotesLink.TabIndex = 12;
			this.upReleaseNotesLink.TabStop = true;
			this.upReleaseNotesLink.Text = "Release notes";
			this.upReleaseNotesLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GotoReleaseNotes);
			// 
			// upIntroLabel
			// 
			this.upIntroLabel.AutoSize = true;
			this.upIntroLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.upIntroLabel.Location = new System.Drawing.Point(3, 1);
			this.upIntroLabel.Name = "upIntroLabel";
			this.upIntroLabel.Size = new System.Drawing.Size(242, 25);
			this.upIntroLabel.TabIndex = 8;
			this.upIntroLabel.Text = "OneMore update available";
			// 
			// upOKButton
			// 
			this.upOKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.upOKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.upOKButton.Location = new System.Drawing.Point(370, 254);
			this.upOKButton.Name = "upOKButton";
			this.upOKButton.Size = new System.Drawing.Size(104, 42);
			this.upOKButton.TabIndex = 7;
			this.upOKButton.Text = "Update";
			this.upOKButton.UseVisualStyleBackColor = true;
			// 
			// timer
			// 
			this.timer.Interval = 1000;
			this.timer.Tick += new System.EventHandler(this.TimerTick);
			// 
			// UpdateDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(633, 543);
			this.Controls.Add(this.updatePanel);
			this.Controls.Add(this.readyPanel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "UpdateDialog";
			this.Padding = new System.Windows.Forms.Padding(20);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Check for OneMore Updates";
			this.readyPanel.ResumeLayout(false);
			this.readyPanel.PerformLayout();
			this.updatePanel.ResumeLayout(false);
			this.updatePanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label currentLabel;
		private System.Windows.Forms.Label versionLabel;
		private System.Windows.Forms.Label lastUpdatedLabel;
		private UI.MoreLinkLabel releaseNotesLink;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Label versionBox;
		private System.Windows.Forms.Label lastUpdatedBox;
		private System.Windows.Forms.Panel readyPanel;
		private System.Windows.Forms.Panel updatePanel;
		private System.Windows.Forms.Button upOKButton;
		private System.Windows.Forms.Label upDescriptionBox;
		private System.Windows.Forms.Label upVersionLabel;
		private System.Windows.Forms.Label upVersionBox;
		private System.Windows.Forms.Label upDescriptionLabel;
		private UI.MoreLinkLabel upReleaseNotesLink;
		private System.Windows.Forms.Label upIntroLabel;
		private System.Windows.Forms.Label lineLabel;
		private System.Windows.Forms.Label upLastUpdatedBox;
		private System.Windows.Forms.Label upLastUpdatedLabel;
		private System.Windows.Forms.Label upCurrentVersionLabel;
		private System.Windows.Forms.Label upCurrentVersionBox;
		private System.Windows.Forms.Label upReleaseDateBox;
		private System.Windows.Forms.Label releaseDateLabel;
		private System.Windows.Forms.Timer timer;
	}
}