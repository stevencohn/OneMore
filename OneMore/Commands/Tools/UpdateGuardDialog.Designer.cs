namespace River.OneMoreAddIn.Commands
{
	partial class UpdateGuardDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateGuardDialog));
			this.topPanel = new System.Windows.Forms.Panel();
			this.browseLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.messageBox = new River.OneMoreAddIn.UI.MoreRichLabel();
			this.iconBox = new System.Windows.Forms.PictureBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.topPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.iconBox)).BeginInit();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// topPanel
			// 
			this.topPanel.BackColor = System.Drawing.SystemColors.Info;
			this.topPanel.Controls.Add(this.browseLink);
			this.topPanel.Controls.Add(this.messageBox);
			this.topPanel.Controls.Add(this.iconBox);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.topPanel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.topPanel.Location = new System.Drawing.Point(0, 0);
			this.topPanel.Margin = new System.Windows.Forms.Padding(0);
			this.topPanel.Name = "topPanel";
			this.topPanel.Padding = new System.Windows.Forms.Padding(20);
			this.topPanel.Size = new System.Drawing.Size(800, 184);
			this.topPanel.TabIndex = 4;
			// 
			// browseLink
			// 
			this.browseLink.ActiveLinkColor = System.Drawing.Color.DarkOrchid;
			this.browseLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.browseLink.AutoSize = true;
			this.browseLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.browseLink.HoverColor = System.Drawing.Color.Orchid;
			this.browseLink.LinkColor = System.Drawing.Color.MediumOrchid;
			this.browseLink.Location = new System.Drawing.Point(90, 144);
			this.browseLink.Name = "browseLink";
			this.browseLink.Selected = false;
			this.browseLink.Size = new System.Drawing.Size(297, 20);
			this.browseLink.StrictColors = false;
			this.browseLink.TabIndex = 2;
			this.browseLink.TabStop = true;
			this.browseLink.Text = "Open the download page in your browser";
			this.browseLink.ThemedBack = null;
			this.browseLink.ThemedFore = null;
			this.browseLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.browseLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.BrowseReleases);
			// 
			// messageBox
			// 
			this.messageBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.messageBox.BackColor = System.Drawing.SystemColors.Info;
			this.messageBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.messageBox.Cursor = System.Windows.Forms.Cursors.Default;
			this.messageBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.messageBox.ForeColor = System.Drawing.SystemColors.InfoText;
			this.messageBox.Location = new System.Drawing.Point(94, 23);
			this.messageBox.Name = "messageBox";
			this.messageBox.ReadOnly = true;
			this.messageBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.messageBox.Size = new System.Drawing.Size(661, 118);
			this.messageBox.TabIndex = 1;
			this.messageBox.TabStop = false;
			this.messageBox.Text = "OneNote is running under Click-to-Run virtualization. You cannot update OneMore u" +
    "sing this dialog. Close OneNote, download the OneMore installer and run the inst" +
    "aller manually.";
			// 
			// iconBox
			// 
			this.iconBox.Location = new System.Drawing.Point(23, 23);
			this.iconBox.Name = "iconBox";
			this.iconBox.Size = new System.Drawing.Size(48, 48);
			this.iconBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.iconBox.TabIndex = 0;
			this.iconBox.TabStop = false;
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.SystemColors.Control;
			this.panel1.Controls.Add(this.okButton);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.ForeColor = System.Drawing.SystemColors.ControlText;
			this.panel1.Location = new System.Drawing.Point(0, 184);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(800, 61);
			this.panel1.TabIndex = 5;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(673, 13);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(115, 36);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// UpdateGuardDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 245);
			this.Controls.Add(this.topPanel);
			this.Controls.Add(this.panel1);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "UpdateGuardDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Blocked by Click-to-run";
			this.TopMost = true;
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.iconBox)).EndInit();
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel topPanel;
		private UI.MoreLinkLabel browseLink;
		private UI.MoreRichLabel messageBox;
		private System.Windows.Forms.PictureBox iconBox;
		private System.Windows.Forms.Panel panel1;
		private UI.MoreButton okButton;
	}
}