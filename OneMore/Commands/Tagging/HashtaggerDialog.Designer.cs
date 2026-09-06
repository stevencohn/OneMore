namespace River.OneMoreAddIn.Commands
{
	partial class HashtaggerDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HashtaggerDialog));
			this.commonWordsMenu = new River.OneMoreAddIn.UI.MoreContextMenuStrip();
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.controlPanel = new River.OneMoreAddIn.UI.MorePanel();
			this.commonWordsButton = new River.OneMoreAddIn.UI.MoreButton();
			this.findBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.findLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.bankBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.tagsBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.tagsLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.buttonPanel.SuspendLayout();
			this.controlPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// commonWordsMenu
			// 
			this.commonWordsMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.commonWordsMenu.Name = "commonWordsMenu";
			this.commonWordsMenu.Size = new System.Drawing.Size(61, 4);
			// 
			// buttonPanel
			// 
			this.buttonPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.buttonPanel.Controls.Add(this.okButton);
			this.buttonPanel.Controls.Add(this.cancelButton);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.Location = new System.Drawing.Point(0, 183);
			this.buttonPanel.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Padding = new System.Windows.Forms.Padding(0, 0, 10, 10);
			this.buttonPanel.Size = new System.Drawing.Size(778, 66);
			this.buttonPanel.TabIndex = 4;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Enabled = false;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(559, 17);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(100, 36);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 6;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(665, 17);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(100, 36);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 7;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// controlPanel
			// 
			this.controlPanel.BottomBorderColor = System.Drawing.SystemColors.ActiveBorder;
			this.controlPanel.BottomBorderSize = 0;
			this.controlPanel.Controls.Add(this.commonWordsButton);
			this.controlPanel.Controls.Add(this.findBox);
			this.controlPanel.Controls.Add(this.findLabel);
			this.controlPanel.Controls.Add(this.bankBox);
			this.controlPanel.Controls.Add(this.tagsBox);
			this.controlPanel.Controls.Add(this.tagsLabel);
			this.controlPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.controlPanel.Location = new System.Drawing.Point(0, 0);
			this.controlPanel.Name = "controlPanel";
			this.controlPanel.Padding = new System.Windows.Forms.Padding(15);
			this.controlPanel.Size = new System.Drawing.Size(778, 177);
			this.controlPanel.TabIndex = 10;
			this.controlPanel.ThemedBack = null;
			this.controlPanel.ThemedFore = null;
			this.controlPanel.TopBorderColor = System.Drawing.SystemColors.Control;
			this.controlPanel.TopBorderSize = 0;
			// 
			// commonWordsButton
			// 
			this.commonWordsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.commonWordsButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.commonWordsButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.commonWordsButton.ImageOver = null;
			this.commonWordsButton.Location = new System.Drawing.Point(566, 118);
			this.commonWordsButton.Name = "commonWordsButton";
			this.commonWordsButton.ShowBorder = true;
			this.commonWordsButton.Size = new System.Drawing.Size(197, 29);
			this.commonWordsButton.StylizeImage = false;
			this.commonWordsButton.TabIndex = 5;
			this.commonWordsButton.Text = "Common Words";
			this.commonWordsButton.ThemedBack = null;
			this.commonWordsButton.ThemedFore = null;
			this.commonWordsButton.UseVisualStyleBackColor = true;
			this.commonWordsButton.Click += new System.EventHandler(this.ShowCommonWordsMenu);
			// 
			// findBox
			// 
			this.findBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.findBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.findBox.Location = new System.Drawing.Point(93, 120);
			this.findBox.Name = "findBox";
			this.findBox.ProcessEnterKey = false;
			this.findBox.Size = new System.Drawing.Size(467, 26);
			this.findBox.TabIndex = 4;
			this.findBox.ThemedBack = null;
			this.findBox.ThemedFore = null;
			// 
			// findLabel
			// 
			this.findLabel.AutoSize = true;
			this.findLabel.Location = new System.Drawing.Point(14, 123);
			this.findLabel.Name = "findLabel";
			this.findLabel.Size = new System.Drawing.Size(40, 20);
			this.findLabel.TabIndex = 3;
			this.findLabel.Text = "Find";
			this.findLabel.ThemedBack = null;
			this.findLabel.ThemedFore = null;
			// 
			// bankBox
			// 
			this.bankBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.bankBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.bankBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.bankBox.Location = new System.Drawing.Point(93, 76);
			this.bankBox.Name = "bankBox";
			this.bankBox.Size = new System.Drawing.Size(255, 25);
			this.bankBox.StylizeImage = false;
			this.bankBox.TabIndex = 2;
			this.bankBox.Text = "Add to tag bank at top of page";
			this.bankBox.ThemedBack = null;
			this.bankBox.ThemedFore = null;
			this.bankBox.UseVisualStyleBackColor = false;
			// 
			// tagsBox
			// 
			this.tagsBox.AcceptsReturn = true;
			this.tagsBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tagsBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.tagsBox.Location = new System.Drawing.Point(93, 18);
			this.tagsBox.Multiline = true;
			this.tagsBox.Name = "tagsBox";
			this.tagsBox.ProcessEnterKey = false;
			this.tagsBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.tagsBox.Size = new System.Drawing.Size(667, 52);
			this.tagsBox.TabIndex = 1;
			this.tagsBox.ThemedBack = null;
			this.tagsBox.ThemedFore = null;
			this.tagsBox.TextChanged += new System.EventHandler(this.DoTagsBoxChanged);
			this.tagsBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SuppressTagsBoxEnter);
			// 
			// tagsLabel
			// 
			this.tagsLabel.AutoSize = true;
			this.tagsLabel.Location = new System.Drawing.Point(14, 18);
			this.tagsLabel.Name = "tagsLabel";
			this.tagsLabel.Size = new System.Drawing.Size(44, 20);
			this.tagsLabel.TabIndex = 0;
			this.tagsLabel.Text = "Tags";
			this.tagsLabel.ThemedBack = null;
			this.tagsLabel.ThemedFore = null;
			// 
			// HashtaggerDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(778, 249);
			this.Controls.Add(this.controlPanel);
			this.Controls.Add(this.buttonPanel);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximumSize = new System.Drawing.Size(1200, 305);
			this.MinimumSize = new System.Drawing.Size(650, 305);
			this.Name = "HashtaggerDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Hashtags";
			this.Load += new System.EventHandler(this.LoadTagsOnLoad);
			this.buttonPanel.ResumeLayout(false);
			this.controlPanel.ResumeLayout(false);
			this.controlPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel buttonPanel;
		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private UI.MorePanel controlPanel;
		private UI.MoreLabel tagsLabel;
		private UI.MoreTextBox tagsBox;
		private UI.MoreCheckBox bankBox;
		private UI.MoreLabel findLabel;
		private UI.MoreTextBox findBox;
		private UI.MoreButton commonWordsButton;
		private UI.MoreContextMenuStrip commonWordsMenu;
	}
}
