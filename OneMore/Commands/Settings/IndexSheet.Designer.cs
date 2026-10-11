
namespace River.OneMoreAddIn.Settings
{
	partial class IndexSheet
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
			this.enabledBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.allBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.selectLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.statusLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.buildLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.buildNote = new River.OneMoreAddIn.UI.MoreMultilineLabel();
			this.deleteLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.layoutPanel.SuspendLayout();
			this.SuspendLayout();
			//
			// introBox
			//
			this.introBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.introBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.introBox.Location = new System.Drawing.Point(13, 8);
			this.introBox.Name = "introBox";
			this.introBox.Padding = new System.Windows.Forms.Padding(0, 0, 0, 36);
			this.introBox.Size = new System.Drawing.Size(772, 96);
			this.introBox.TabIndex = 0;
			this.introBox.Text = "Build an index of the text of your notebooks so that the Search command can find " +
    "text almost instantly. The index is kept on this computer and is brought up to d" +
    "ate in the background. Your pages are never changed.";
			this.introBox.ThemedBack = "ControlLightLight";
			this.introBox.ThemedFore = "ControlText";
			//
			// layoutPanel
			//
			this.layoutPanel.Controls.Add(this.enabledBox);
			this.layoutPanel.Controls.Add(this.allBox);
			this.layoutPanel.Controls.Add(this.selectLink);
			this.layoutPanel.Controls.Add(this.statusLabel);
			this.layoutPanel.Controls.Add(this.buildLink);
			this.layoutPanel.Controls.Add(this.buildNote);
			this.layoutPanel.Controls.Add(this.deleteLink);
			this.layoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.layoutPanel.Location = new System.Drawing.Point(13, 104);
			this.layoutPanel.Margin = new System.Windows.Forms.Padding(0);
			this.layoutPanel.Name = "layoutPanel";
			this.layoutPanel.Size = new System.Drawing.Size(772, 386);
			this.layoutPanel.TabIndex = 1;
			//
			// enabledBox
			//
			this.enabledBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.enabledBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.enabledBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.enabledBox.Location = new System.Drawing.Point(7, 10);
			this.enabledBox.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
			this.enabledBox.Name = "enabledBox";
			this.enabledBox.Size = new System.Drawing.Size(700, 25);
			this.enabledBox.StylizeImage = false;
			this.enabledBox.TabIndex = 0;
			this.enabledBox.Text = "Keep a search index of my notebooks up to date";
			this.enabledBox.ThemedBack = null;
			this.enabledBox.ThemedFore = null;
			this.enabledBox.UseVisualStyleBackColor = true;
			this.enabledBox.CheckedChanged += new System.EventHandler(this.ToggleEnabled);
			//
			// allBox
			//
			this.allBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.allBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.allBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.allBox.Location = new System.Drawing.Point(34, 45);
			this.allBox.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
			this.allBox.Name = "allBox";
			this.allBox.Size = new System.Drawing.Size(673, 25);
			this.allBox.StylizeImage = false;
			this.allBox.TabIndex = 1;
			this.allBox.Text = "Index all notebooks";
			this.allBox.ThemedBack = null;
			this.allBox.ThemedFore = null;
			this.allBox.UseVisualStyleBackColor = true;
			this.allBox.CheckedChanged += new System.EventHandler(this.ToggleEnabled);
			//
			// selectLink
			//
			this.selectLink.Active = false;
			this.selectLink.ActiveLinkColor = System.Drawing.Color.MediumOrchid;
			this.selectLink.AutoSize = true;
			this.selectLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.selectLink.HoverColor = System.Drawing.Color.Orchid;
			this.selectLink.LinkColor = System.Drawing.Color.MediumOrchid;
			this.selectLink.Location = new System.Drawing.Point(61, 80);
			this.selectLink.Name = "selectLink";
			this.selectLink.NavMode = false;
			this.selectLink.Selected = false;
			this.selectLink.Size = new System.Drawing.Size(245, 20);
			this.selectLink.StrictColors = false;
			this.selectLink.TabIndex = 2;
			this.selectLink.TabStop = true;
			this.selectLink.Text = "Select the notebooks to index";
			this.selectLink.ThemedBack = null;
			this.selectLink.ThemedFore = null;
			this.selectLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.selectLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.SelectNotebooks);
			//
			// statusLabel
			//
			this.statusLabel.AutoSize = true;
			this.statusLabel.Location = new System.Drawing.Point(7, 130);
			this.statusLabel.Name = "statusLabel";
			this.statusLabel.Size = new System.Drawing.Size(180, 20);
			this.statusLabel.TabIndex = 3;
			this.statusLabel.Text = "The index is empty";
			//
			// buildLink
			//
			this.buildLink.Active = false;
			this.buildLink.ActiveLinkColor = System.Drawing.Color.MediumOrchid;
			this.buildLink.AutoSize = true;
			this.buildLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.buildLink.HoverColor = System.Drawing.Color.Orchid;
			this.buildLink.LinkColor = System.Drawing.Color.MediumOrchid;
			this.buildLink.Location = new System.Drawing.Point(7, 170);
			this.buildLink.Name = "buildLink";
			this.buildLink.NavMode = false;
			this.buildLink.Selected = false;
			this.buildLink.Size = new System.Drawing.Size(165, 20);
			this.buildLink.StrictColors = false;
			this.buildLink.TabIndex = 4;
			this.buildLink.TabStop = true;
			this.buildLink.Text = "Build the index now";
			this.buildLink.ThemedBack = null;
			this.buildLink.ThemedFore = null;
			this.buildLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.buildLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.BuildNow);
			//
			// buildNote
			//
			this.buildNote.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.buildNote.Location = new System.Drawing.Point(7, 197);
			this.buildNote.Name = "buildNote";
			this.buildNote.Size = new System.Drawing.Size(755, 70);
			this.buildNote.TabIndex = 5;
			this.buildNote.Text = "Reads every page that is not yet indexed. This can take a few minutes for a large" +
    " collection and may slow OneNote while it runs. Otherwise the index is built a l" +
    "ittle at a time in the background.";
			this.buildNote.ThemedBack = null;
			this.buildNote.ThemedFore = null;
			//
			// deleteLink
			//
			this.deleteLink.Active = false;
			this.deleteLink.ActiveLinkColor = System.Drawing.Color.MediumOrchid;
			this.deleteLink.AutoSize = true;
			this.deleteLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.deleteLink.HoverColor = System.Drawing.Color.Orchid;
			this.deleteLink.LinkColor = System.Drawing.Color.MediumOrchid;
			this.deleteLink.Location = new System.Drawing.Point(7, 280);
			this.deleteLink.Name = "deleteLink";
			this.deleteLink.NavMode = false;
			this.deleteLink.Selected = false;
			this.deleteLink.Size = new System.Drawing.Size(130, 20);
			this.deleteLink.StrictColors = false;
			this.deleteLink.TabIndex = 6;
			this.deleteLink.TabStop = true;
			this.deleteLink.Text = "Delete the index";
			this.deleteLink.ThemedBack = null;
			this.deleteLink.ThemedFore = null;
			this.deleteLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.deleteLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.DeleteIndex);
			//
			// IndexSheet
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.layoutPanel);
			this.Controls.Add(this.introBox);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Name = "IndexSheet";
			this.Padding = new System.Windows.Forms.Padding(13, 8, 15, 10);
			this.Size = new System.Drawing.Size(800, 500);
			this.layoutPanel.ResumeLayout(false);
			this.layoutPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreMultilineLabel introBox;
		private System.Windows.Forms.Panel layoutPanel;
		private UI.MoreCheckBox enabledBox;
		private UI.MoreCheckBox allBox;
		private UI.MoreLinkLabel selectLink;
		private UI.MoreLabel statusLabel;
		private UI.MoreLinkLabel buildLink;
		private UI.MoreMultilineLabel buildNote;
		private UI.MoreLinkLabel deleteLink;
	}
}
