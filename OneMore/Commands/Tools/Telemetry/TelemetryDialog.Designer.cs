namespace River.OneMoreAddIn.Commands
{
	partial class TelemetryDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TelemetryDialog));
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.titleLabel = new System.Windows.Forms.Label();
			this.subtitleLabel = new River.OneMoreAddIn.UI.MoreTextBox();
			this.readLabel = new System.Windows.Forms.Label();
			this.yesButton = new River.OneMoreAddIn.UI.MoreButton();
			this.designLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.topPanel = new System.Windows.Forms.Panel();
			this.whyBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.noButton = new River.OneMoreAddIn.UI.MoreButton();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.topPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
			this.pictureBox1.Image = global::River.OneMoreAddIn.Properties.Resources.TLogo;
			this.pictureBox1.Location = new System.Drawing.Point(690, 0);
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
			this.titleLabel.Size = new System.Drawing.Size(327, 38);
			this.titleLabel.TabIndex = 1;
			this.titleLabel.Text = "Help Improve OneMore";
			// 
			// subtitleLabel
			// 
			this.subtitleLabel.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.subtitleLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.subtitleLabel.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.subtitleLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.subtitleLabel.Location = new System.Drawing.Point(50, 61);
			this.subtitleLabel.Name = "subtitleLabel";
			this.subtitleLabel.ProcessEnterKey = false;
			this.subtitleLabel.ReadOnly = true;
			this.subtitleLabel.Size = new System.Drawing.Size(534, 30);
			this.subtitleLabel.TabIndex = 2;
			this.subtitleLabel.Text = "Enable Anonymous Telemetry";
			this.subtitleLabel.ThemedBack = "ControlLightLight";
			this.subtitleLabel.ThemedFore = null;
			// 
			// readLabel
			// 
			this.readLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.readLabel.AutoSize = true;
			this.readLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.readLabel.Location = new System.Drawing.Point(46, 127);
			this.readLabel.Margin = new System.Windows.Forms.Padding(3, 20, 3, 0);
			this.readLabel.Name = "readLabel";
			this.readLabel.Size = new System.Drawing.Size(240, 20);
			this.readLabel.TabIndex = 3;
			this.readLabel.Text = "Read about the telemetry design";
			// 
			// yesButton
			// 
			this.yesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.yesButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.yesButton.DialogResult = System.Windows.Forms.DialogResult.Yes;
			this.yesButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.yesButton.ImageOver = null;
			this.yesButton.Location = new System.Drawing.Point(442, 518);
			this.yesButton.Name = "yesButton";
			this.yesButton.ShowBorder = true;
			this.yesButton.Size = new System.Drawing.Size(511, 42);
			this.yesButton.StylizeImage = false;
			this.yesButton.TabIndex = 0;
			this.yesButton.Text = "Yes, I\'ll help! Enable telemetry";
			this.yesButton.ThemedBack = null;
			this.yesButton.ThemedFore = null;
			this.yesButton.UseVisualStyleBackColor = false;
			this.yesButton.Click += new System.EventHandler(this.EnableTelemetry);
			// 
			// designLink
			// 
			this.designLink.ActiveLinkColor = System.Drawing.Color.DarkOrchid;
			this.designLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.designLink.AutoSize = true;
			this.designLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.designLink.HoverColor = System.Drawing.Color.Orchid;
			this.designLink.LinkColor = System.Drawing.Color.MediumOrchid;
			this.designLink.Location = new System.Drawing.Point(46, 155);
			this.designLink.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
			this.designLink.Name = "designLink";
			this.designLink.Selected = false;
			this.designLink.Size = new System.Drawing.Size(193, 20);
			this.designLink.StrictColors = false;
			this.designLink.TabIndex = 3;
			this.designLink.TabStop = true;
			this.designLink.Text = "https://onemoreaddin.com";
			this.designLink.ThemedBack = null;
			this.designLink.ThemedFore = null;
			this.designLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.designLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GotoDesign);
			// 
			// topPanel
			// 
			this.topPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.topPanel.Controls.Add(this.titleLabel);
			this.topPanel.Controls.Add(this.subtitleLabel);
			this.topPanel.Controls.Add(this.pictureBox1);
			this.topPanel.Controls.Add(this.readLabel);
			this.topPanel.Controls.Add(this.designLink);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = new System.Drawing.Point(0, 0);
			this.topPanel.Name = "topPanel";
			this.topPanel.Padding = new System.Windows.Forms.Padding(40, 20, 0, 0);
			this.topPanel.Size = new System.Drawing.Size(990, 207);
			this.topPanel.TabIndex = 14;
			// 
			// whyBox
			// 
			this.whyBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.whyBox.BackColor = System.Drawing.SystemColors.Control;
			this.whyBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.whyBox.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.whyBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.whyBox.Location = new System.Drawing.Point(50, 229);
			this.whyBox.Multiline = true;
			this.whyBox.Name = "whyBox";
			this.whyBox.ProcessEnterKey = false;
			this.whyBox.ReadOnly = true;
			this.whyBox.Size = new System.Drawing.Size(903, 269);
			this.whyBox.TabIndex = 3;
			this.whyBox.Text = resources.GetString("whyBox.Text");
			this.whyBox.ThemedBack = "Control";
			this.whyBox.ThemedFore = null;
			// 
			// noButton
			// 
			this.noButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.noButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.noButton.DialogResult = System.Windows.Forms.DialogResult.No;
			this.noButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.noButton.ImageOver = null;
			this.noButton.Location = new System.Drawing.Point(442, 572);
			this.noButton.Name = "noButton";
			this.noButton.ShowBorder = true;
			this.noButton.Size = new System.Drawing.Size(511, 42);
			this.noButton.StylizeImage = false;
			this.noButton.TabIndex = 15;
			this.noButton.Text = "No, do not enable telemetry";
			this.noButton.ThemedBack = null;
			this.noButton.ThemedFore = null;
			this.noButton.UseVisualStyleBackColor = false;
			this.noButton.Click += new System.EventHandler(this.DisableTelemetry);
			// 
			// TelemetryDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(990, 649);
			this.Controls.Add(this.noButton);
			this.Controls.Add(this.whyBox);
			this.Controls.Add(this.topPanel);
			this.Controls.Add(this.yesButton);
			this.ForeColor = System.Drawing.Color.Black;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "TelemetryDialog";
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
		private UI.MoreTextBox subtitleLabel;
		private System.Windows.Forms.Label readLabel;
		private UI.MoreButton yesButton;
		private UI.MoreLinkLabel designLink;
		private System.Windows.Forms.Panel topPanel;
		private UI.MoreTextBox whyBox;
		private UI.MoreButton noButton;
	}
}