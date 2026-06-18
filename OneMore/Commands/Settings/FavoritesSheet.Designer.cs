namespace River.OneMoreAddIn.Settings
{
	partial class FavoritesSheet
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
			this.optionsBox = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.dropLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.dropLabel = new River.OneMoreAddIn.UI.MoreMultilineLabel();
			this.shortcutsBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.layoutPanel = new System.Windows.Forms.Panel();
			this.introBox = new River.OneMoreAddIn.UI.MoreMultilineLabel();
			this.optionsBox.SuspendLayout();
			this.layoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// optionsBox
			// 
			this.optionsBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.optionsBox.Controls.Add(this.dropLink);
			this.optionsBox.Controls.Add(this.dropLabel);
			this.optionsBox.Location = new System.Drawing.Point(3, 237);
			this.optionsBox.Name = "optionsBox";
			this.optionsBox.ShowOnlyTopEdge = true;
			this.optionsBox.Size = new System.Drawing.Size(768, 164);
			this.optionsBox.TabIndex = 0;
			this.optionsBox.TabStop = false;
			this.optionsBox.Text = "Advanced Options";
			this.optionsBox.ThemedBorder = null;
			this.optionsBox.ThemedFore = null;
			// 
			// dropLink
			// 
			this.dropLink.ActiveLinkColor = System.Drawing.Color.MediumOrchid;
			this.dropLink.AutoSize = true;
			this.dropLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.dropLink.HoverColor = System.Drawing.Color.Orchid;
			this.dropLink.LinkColor = System.Drawing.Color.MediumOrchid;
			this.dropLink.Location = new System.Drawing.Point(7, 38);
			this.dropLink.Name = "dropLink";
			this.dropLink.Selected = false;
			this.dropLink.Size = new System.Drawing.Size(334, 20);
			this.dropLink.StrictColors = false;
			this.dropLink.TabIndex = 2;
			this.dropLink.TabStop = true;
			this.dropLink.Text = "Drop the Favorites schema from the database";
			this.dropLink.ThemedBack = null;
			this.dropLink.ThemedFore = null;
			this.dropLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.dropLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.DropSchema);
			// 
			// dropLabel
			// 
			this.dropLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dropLabel.Location = new System.Drawing.Point(6, 64);
			this.dropLabel.Name = "dropLabel";
			this.dropLabel.Size = new System.Drawing.Size(756, 100);
			this.dropLabel.TabIndex = 3;
			this.dropLabel.Text = "Delete all Favorites data from your local OneMore.db database. Use this only if t" +
    "o erase the entire Favorites collection and reimport an old Favorites.xml file." +
    "";
			this.dropLabel.ThemedBack = null;
			this.dropLabel.ThemedFore = null;
			// 
			// shortcutsBox
			// 
			this.shortcutsBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.shortcutsBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.shortcutsBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.shortcutsBox.Location = new System.Drawing.Point(14, 23);
			this.shortcutsBox.Margin = new System.Windows.Forms.Padding(20, 10, 3, 3);
			this.shortcutsBox.Name = "shortcutsBox";
			this.shortcutsBox.Size = new System.Drawing.Size(365, 25);
			this.shortcutsBox.StylizeImage = false;
			this.shortcutsBox.TabIndex = 0;
			this.shortcutsBox.Text = "Include reference to keyboard shortcuts page";
			this.shortcutsBox.ThemedBack = null;
			this.shortcutsBox.ThemedFore = null;
			this.shortcutsBox.UseVisualStyleBackColor = true;
			// 
			// layoutPanel
			// 
			this.layoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.layoutPanel.Controls.Add(this.shortcutsBox);
			this.layoutPanel.Controls.Add(this.optionsBox);
			this.layoutPanel.Location = new System.Drawing.Point(13, 84);
			this.layoutPanel.Name = "layoutPanel";
			this.layoutPanel.Size = new System.Drawing.Size(774, 404);
			this.layoutPanel.TabIndex = 4;
			// 
			// introBox
			// 
			this.introBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.introBox.Location = new System.Drawing.Point(13, 12);
			this.introBox.Name = "introBox";
			this.introBox.Size = new System.Drawing.Size(774, 66);
			this.introBox.TabIndex = 5;
			this.introBox.Text = "Custom options for Favorites";
			this.introBox.ThemedBack = null;
			this.introBox.ThemedFore = null;
			// 
			// FavoritesSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.introBox);
			this.Controls.Add(this.layoutPanel);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MinimumSize = new System.Drawing.Size(750, 400);
			this.Name = "FavoritesSheet";
			this.Padding = new System.Windows.Forms.Padding(10, 9, 10, 9);
			this.Size = new System.Drawing.Size(800, 500);
			this.optionsBox.ResumeLayout(false);
			this.optionsBox.PerformLayout();
			this.layoutPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion
		private UI.MoreGroupBox optionsBox;
		private UI.MoreCheckBox shortcutsBox;
		private System.Windows.Forms.Panel layoutPanel;
		private UI.MoreMultilineLabel introBox;
		private UI.MoreMultilineLabel dropLabel;
		private UI.MoreLinkLabel dropLink;
	}
}