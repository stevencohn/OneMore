namespace River.OneMoreAddIn.Commands
{
	partial class SearchAndReplaceSessionDialog
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SearchAndReplaceSessionDialog));
			this.statusLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.replaceButton = new River.OneMoreAddIn.UI.MoreButton();
			this.skipButton = new River.OneMoreAddIn.UI.MoreButton();
			this.replaceAllButton = new River.OneMoreAddIn.UI.MoreButton();
			this.closeButton = new River.OneMoreAddIn.UI.MoreButton();
			this.SuspendLayout();
			// 
			// statusLabel
			// 
			this.statusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.statusLabel.Location = new System.Drawing.Point(28, 33);
			this.statusLabel.Name = "statusLabel";
			this.statusLabel.Size = new System.Drawing.Size(580, 22);
			this.statusLabel.TabIndex = 0;
			this.statusLabel.Text = "Match 1 of 1 — «...»";
			this.statusLabel.ThemedBack = null;
			this.statusLabel.ThemedFore = null;
			// 
			// replaceButton
			// 
			this.replaceButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.replaceButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.replaceButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.replaceButton.ImageOver = null;
			this.replaceButton.Location = new System.Drawing.Point(18, 85);
			this.replaceButton.Name = "replaceButton";
			this.replaceButton.ShowBorder = true;
			this.replaceButton.Size = new System.Drawing.Size(100, 38);
			this.replaceButton.StylizeImage = false;
			this.replaceButton.TabIndex = 1;
			this.replaceButton.Text = "Replace";
			this.replaceButton.ThemedBack = null;
			this.replaceButton.ThemedFore = null;
			this.replaceButton.UseVisualStyleBackColor = true;
			this.replaceButton.Click += new System.EventHandler(this.ReplaceOne);
			// 
			// skipButton
			// 
			this.skipButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.skipButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.skipButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.skipButton.ImageOver = null;
			this.skipButton.Location = new System.Drawing.Point(124, 85);
			this.skipButton.Name = "skipButton";
			this.skipButton.ShowBorder = true;
			this.skipButton.Size = new System.Drawing.Size(100, 38);
			this.skipButton.StylizeImage = false;
			this.skipButton.TabIndex = 2;
			this.skipButton.Text = "Skip";
			this.skipButton.ThemedBack = null;
			this.skipButton.ThemedFore = null;
			this.skipButton.UseVisualStyleBackColor = true;
			this.skipButton.Click += new System.EventHandler(this.SkipOne);
			// 
			// replaceAllButton
			// 
			this.replaceAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.replaceAllButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.replaceAllButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.replaceAllButton.ImageOver = null;
			this.replaceAllButton.Location = new System.Drawing.Point(230, 85);
			this.replaceAllButton.Name = "replaceAllButton";
			this.replaceAllButton.ShowBorder = true;
			this.replaceAllButton.Size = new System.Drawing.Size(100, 38);
			this.replaceAllButton.StylizeImage = false;
			this.replaceAllButton.TabIndex = 3;
			this.replaceAllButton.Text = "Replace All";
			this.replaceAllButton.ThemedBack = null;
			this.replaceAllButton.ThemedFore = null;
			this.replaceAllButton.UseVisualStyleBackColor = true;
			this.replaceAllButton.Click += new System.EventHandler(this.ReplaceAll);
			// 
			// closeButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.closeButton.ImageOver = null;
			this.closeButton.Location = new System.Drawing.Point(508, 85);
			this.closeButton.Name = "closeButton";
			this.closeButton.ShowBorder = true;
			this.closeButton.Size = new System.Drawing.Size(100, 38);
			this.closeButton.StylizeImage = false;
			this.closeButton.TabIndex = 4;
			this.closeButton.Text = "Close";
			this.closeButton.ThemedBack = null;
			this.closeButton.ThemedFore = null;
			this.closeButton.UseVisualStyleBackColor = true;
			this.closeButton.Click += new System.EventHandler(this.CloseSession);
			// 
			// SearchAndReplaceSessionDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.closeButton;
			this.ClientSize = new System.Drawing.Size(626, 141);
			this.Controls.Add(this.statusLabel);
			this.Controls.Add(this.replaceButton);
			this.Controls.Add(this.skipButton);
			this.Controls.Add(this.replaceAllButton);
			this.Controls.Add(this.closeButton);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SearchAndReplaceSessionDialog";
			this.Padding = new System.Windows.Forms.Padding(15);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Search and Replace";
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreLabel statusLabel;
		private UI.MoreButton replaceButton;
		private UI.MoreButton skipButton;
		private UI.MoreButton replaceAllButton;
		private UI.MoreButton closeButton;
	}
}
