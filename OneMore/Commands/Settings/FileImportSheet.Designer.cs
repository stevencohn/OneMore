
namespace River.OneMoreAddIn.Settings
{
	partial class FileImportSheet
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

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.introBox = new River.OneMoreAddIn.UI.MoreMultilineLabel();
			this.layoutPanel = new System.Windows.Forms.Panel();
			this.quickGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.sectionLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.sectionLabel = new System.Windows.Forms.Label();
			this.folderButton = new River.OneMoreAddIn.UI.MoreButton();
			this.folderBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.folderLabel = new System.Windows.Forms.Label();
			this.quickIntroLabel = new River.OneMoreAddIn.UI.MoreMultilineLabel();
			this.ppGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.widthBox = new System.Windows.Forms.NumericUpDown();
			this.widthLabel = new System.Windows.Forms.Label();
			this.layoutPanel.SuspendLayout();
			this.quickGroup.SuspendLayout();
			this.ppGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.widthBox)).BeginInit();
			this.SuspendLayout();
			// 
			// introBox
			// 
			this.introBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.introBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.introBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.introBox.Location = new System.Drawing.Point(13, 8);
			this.introBox.Name = "introBox";
			this.introBox.Padding = new System.Windows.Forms.Padding(0, 0, 0, 36);
			this.introBox.Size = new System.Drawing.Size(772, 66);
			this.introBox.TabIndex = 3;
			this.introBox.Text = "File Import Options";
			this.introBox.ThemedBack = "ControlLightLight";
			this.introBox.ThemedFore = "ControlText";
			// 
			// layoutPanel
			// 
			this.layoutPanel.Controls.Add(this.quickGroup);
			this.layoutPanel.Controls.Add(this.ppGroup);
			this.layoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.layoutPanel.Location = new System.Drawing.Point(13, 74);
			this.layoutPanel.Margin = new System.Windows.Forms.Padding(0);
			this.layoutPanel.Name = "layoutPanel";
			this.layoutPanel.Size = new System.Drawing.Size(772, 416);
			this.layoutPanel.TabIndex = 4;
			// 
			// quickGroup
			// 
			this.quickGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.quickGroup.Controls.Add(this.sectionLink);
			this.quickGroup.Controls.Add(this.sectionLabel);
			this.quickGroup.Controls.Add(this.folderButton);
			this.quickGroup.Controls.Add(this.folderBox);
			this.quickGroup.Controls.Add(this.folderLabel);
			this.quickGroup.Controls.Add(this.quickIntroLabel);
			this.quickGroup.Location = new System.Drawing.Point(3, 110);
			this.quickGroup.Name = "quickGroup";
			this.quickGroup.Padding = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.quickGroup.ShowOnlyTopEdge = true;
			this.quickGroup.Size = new System.Drawing.Size(766, 230);
			this.quickGroup.TabIndex = 6;
			this.quickGroup.TabStop = false;
			this.quickGroup.Text = "Quick Import";
			this.quickGroup.ThemedBorder = null;
			this.quickGroup.ThemedFore = null;
			// 
			// sectionLink
			// 
			this.sectionLink.Active = false;
			this.sectionLink.ActiveLinkColor = System.Drawing.Color.MediumOrchid;
			this.sectionLink.AutoSize = true;
			this.sectionLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.sectionLink.HoverColor = System.Drawing.Color.MediumOrchid;
			this.sectionLink.LinkColor = System.Drawing.Color.MediumOrchid;
			this.sectionLink.Location = new System.Drawing.Point(124, 152);
			this.sectionLink.Name = "sectionLink";
			this.sectionLink.NavMode = false;
			this.sectionLink.Selected = false;
			this.sectionLink.Size = new System.Drawing.Size(121, 20);
			this.sectionLink.StrictColors = false;
			this.sectionLink.TabIndex = 5;
			this.sectionLink.TabStop = true;
			this.sectionLink.Text = "Select section...";
			this.sectionLink.ThemedBack = null;
			this.sectionLink.ThemedFore = null;
			this.sectionLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.sectionLink.Click += new System.EventHandler(this.SelectSection);
			// 
			// sectionLabel
			// 
			this.sectionLabel.AutoSize = true;
			this.sectionLabel.Location = new System.Drawing.Point(13, 152);
			this.sectionLabel.Name = "sectionLabel";
			this.sectionLabel.Size = new System.Drawing.Size(63, 20);
			this.sectionLabel.TabIndex = 4;
			this.sectionLabel.Text = "Section";
			// 
			// folderButton
			// 
			this.folderButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.folderButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.folderButton.ImageOver = null;
			this.folderButton.Location = new System.Drawing.Point(717, 100);
			this.folderButton.Name = "folderButton";
			this.folderButton.ShowBorder = true;
			this.folderButton.Size = new System.Drawing.Size(36, 31);
			this.folderButton.StylizeImage = false;
			this.folderButton.TabIndex = 3;
			this.folderButton.Text = "...";
			this.folderButton.ThemedBack = null;
			this.folderButton.ThemedFore = null;
			this.folderButton.UseVisualStyleBackColor = true;
			this.folderButton.Click += new System.EventHandler(this.BrowseFolder);
			// 
			// folderBox
			// 
			this.folderBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.folderBox.Location = new System.Drawing.Point(128, 101);
			this.folderBox.Name = "folderBox";
			this.folderBox.ProcessEnterKey = false;
			this.folderBox.Size = new System.Drawing.Size(582, 26);
			this.folderBox.TabIndex = 2;
			this.folderBox.ThemedBack = null;
			this.folderBox.ThemedFore = null;
			// 
			// folderLabel
			// 
			this.folderLabel.AutoSize = true;
			this.folderLabel.Location = new System.Drawing.Point(13, 105);
			this.folderLabel.Name = "folderLabel";
			this.folderLabel.Size = new System.Drawing.Size(54, 20);
			this.folderLabel.TabIndex = 1;
			this.folderLabel.Text = "Folder";
			// 
			// quickIntroLabel
			// 
			this.quickIntroLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.quickIntroLabel.Location = new System.Drawing.Point(13, 44);
			this.quickIntroLabel.Name = "quickIntroLabel";
			this.quickIntroLabel.Size = new System.Drawing.Size(740, 45);
			this.quickIntroLabel.TabIndex = 0;
			this.quickIntroLabel.Text = "These options enable the quick-import feature";
			this.quickIntroLabel.ThemedBack = null;
			this.quickIntroLabel.ThemedFore = null;
			// 
			// ppGroup
			// 
			this.ppGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ppGroup.Controls.Add(this.widthBox);
			this.ppGroup.Controls.Add(this.widthLabel);
			this.ppGroup.Location = new System.Drawing.Point(3, 6);
			this.ppGroup.Name = "ppGroup";
			this.ppGroup.Padding = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.ppGroup.ShowOnlyTopEdge = true;
			this.ppGroup.Size = new System.Drawing.Size(766, 98);
			this.ppGroup.TabIndex = 5;
			this.ppGroup.TabStop = false;
			this.ppGroup.Text = "PowerPoint and PDF";
			this.ppGroup.ThemedBorder = null;
			this.ppGroup.ThemedFore = null;
			// 
			// widthBox
			// 
			this.widthBox.Location = new System.Drawing.Point(347, 41);
			this.widthBox.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
			this.widthBox.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.widthBox.Name = "widthBox";
			this.widthBox.Size = new System.Drawing.Size(120, 26);
			this.widthBox.TabIndex = 1;
			this.widthBox.Value = new decimal(new int[] {
            600,
            0,
            0,
            0});
			// 
			// widthLabel
			// 
			this.widthLabel.AutoSize = true;
			this.widthLabel.Location = new System.Drawing.Point(13, 43);
			this.widthLabel.Name = "widthLabel";
			this.widthLabel.Size = new System.Drawing.Size(211, 20);
			this.widthLabel.TabIndex = 0;
			this.widthLabel.Text = "Preferred import image width";
			// 
			// FileImportSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.layoutPanel);
			this.Controls.Add(this.introBox);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Name = "FileImportSheet";
			this.Padding = new System.Windows.Forms.Padding(13, 8, 15, 10);
			this.Size = new System.Drawing.Size(800, 500);
			this.layoutPanel.ResumeLayout(false);
			this.quickGroup.ResumeLayout(false);
			this.quickGroup.PerformLayout();
			this.ppGroup.ResumeLayout(false);
			this.ppGroup.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.widthBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreMultilineLabel introBox;
		private System.Windows.Forms.Panel layoutPanel;
		private UI.MoreGroupBox ppGroup;
		private System.Windows.Forms.NumericUpDown widthBox;
		private System.Windows.Forms.Label widthLabel;
		private UI.MoreGroupBox quickGroup;
		private UI.MoreMultilineLabel quickIntroLabel;
		private System.Windows.Forms.Label folderLabel;
		private UI.MoreTextBox folderBox;
		private UI.MoreButton folderButton;
		private System.Windows.Forms.Label sectionLabel;
		private UI.MoreLinkLabel sectionLink;
	}
}
