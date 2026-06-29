
namespace River.OneMoreAddIn.Settings
{
	partial class NotebooksDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NotebooksDialog));
			this.introPanel = new System.Windows.Forms.Panel();
			this.infoLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.notebooksPanel = new River.OneMoreAddIn.UI.MorePanel();
			this.booksPanel = new River.OneMoreAddIn.UI.MorePanel();
			this.selectAllLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.sep1 = new River.OneMoreAddIn.UI.MoreLabel();
			this.selectNoneLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.introPanel.SuspendLayout();
			this.notebooksPanel.SuspendLayout();
			this.buttonPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// introPanel
			// 
			this.introPanel.BackColor = System.Drawing.SystemColors.Control;
			this.introPanel.Controls.Add(this.infoLabel);
			this.introPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.introPanel.Location = new System.Drawing.Point(0, 0);
			this.introPanel.Name = "introPanel";
			this.introPanel.Padding = new System.Windows.Forms.Padding(30, 16, 20, 12);
			this.introPanel.Size = new System.Drawing.Size(600, 90);
			this.introPanel.TabIndex = 0;
			// 
			// infoLabel
			// 
			this.infoLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.infoLabel.Location = new System.Drawing.Point(30, 16);
			this.infoLabel.Name = "infoLabel";
			this.infoLabel.Size = new System.Drawing.Size(550, 62);
			this.infoLabel.TabIndex = 0;
			this.infoLabel.Text = "Select notebooks to include or exclude from hashtag scanning. ✔ identifies notebo" +
    "oks that have already been scanned.";
			this.infoLabel.ThemedBack = null;
			this.infoLabel.ThemedFore = null;
			// 
			// notebooksPanel
			// 
			this.notebooksPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.notebooksPanel.BottomBorderColor = System.Drawing.Color.Transparent;
			this.notebooksPanel.BottomBorderSize = 0;
			this.notebooksPanel.Controls.Add(this.booksPanel);
			this.notebooksPanel.Controls.Add(this.selectAllLink);
			this.notebooksPanel.Controls.Add(this.sep1);
			this.notebooksPanel.Controls.Add(this.selectNoneLink);
			this.notebooksPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.notebooksPanel.Location = new System.Drawing.Point(0, 90);
			this.notebooksPanel.Name = "notebooksPanel";
			this.notebooksPanel.Padding = new System.Windows.Forms.Padding(0, 0, 20, 20);
			this.notebooksPanel.Size = new System.Drawing.Size(600, 484);
			this.notebooksPanel.TabIndex = 1;
			this.notebooksPanel.ThemedBack = "ControlLightLight";
			this.notebooksPanel.ThemedFore = null;
			this.notebooksPanel.TopBorderColor = System.Drawing.SystemColors.Control;
			this.notebooksPanel.TopBorderSize = 0;
			// 
			// booksPanel
			// 
			this.booksPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.booksPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.booksPanel.BottomBorderColor = System.Drawing.Color.Transparent;
			this.booksPanel.BottomBorderSize = 0;
			this.booksPanel.Location = new System.Drawing.Point(31, 47);
			this.booksPanel.Name = "booksPanel";
			this.booksPanel.Size = new System.Drawing.Size(549, 417);
			this.booksPanel.TabIndex = 4;
			this.booksPanel.ThemedBack = null;
			this.booksPanel.ThemedFore = null;
			this.booksPanel.TopBorderColor = System.Drawing.SystemColors.Control;
			this.booksPanel.TopBorderSize = 0;
			// 
			// selectAllLink
			// 
			this.selectAllLink.Active = false;
			this.selectAllLink.ActiveLinkColor = System.Drawing.Color.DarkOrchid;
			this.selectAllLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.selectAllLink.AutoSize = true;
			this.selectAllLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.selectAllLink.HoverColor = System.Drawing.Color.MediumOrchid;
			this.selectAllLink.LinkColor = System.Drawing.SystemColors.ControlDark;
			this.selectAllLink.Location = new System.Drawing.Point(381, 24);
			this.selectAllLink.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.selectAllLink.MaximumSize = new System.Drawing.Size(420, 0);
			this.selectAllLink.Name = "selectAllLink";
			this.selectAllLink.NavMode = false;
			this.selectAllLink.Selected = false;
			this.selectAllLink.Size = new System.Drawing.Size(75, 20);
			this.selectAllLink.StrictColors = false;
			this.selectAllLink.TabIndex = 1;
			this.selectAllLink.TabStop = true;
			this.selectAllLink.Text = "Select All";
			this.selectAllLink.ThemedBack = null;
			this.selectAllLink.ThemedFore = null;
			this.selectAllLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.selectAllLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.SelectAllNotebooks);
			// 
			// sep1
			// 
			this.sep1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.sep1.AutoSize = true;
			this.sep1.Location = new System.Drawing.Point(463, 24);
			this.sep1.Name = "sep1";
			this.sep1.Size = new System.Drawing.Size(14, 20);
			this.sep1.TabIndex = 2;
			this.sep1.Text = "|";
			this.sep1.ThemedBack = null;
			this.sep1.ThemedFore = null;
			// 
			// selectNoneLink
			// 
			this.selectNoneLink.Active = false;
			this.selectNoneLink.ActiveLinkColor = System.Drawing.Color.DarkOrchid;
			this.selectNoneLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.selectNoneLink.AutoSize = true;
			this.selectNoneLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.selectNoneLink.HoverColor = System.Drawing.Color.MediumOrchid;
			this.selectNoneLink.LinkColor = System.Drawing.SystemColors.ControlDark;
			this.selectNoneLink.Location = new System.Drawing.Point(484, 24);
			this.selectNoneLink.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.selectNoneLink.MaximumSize = new System.Drawing.Size(420, 0);
			this.selectNoneLink.Name = "selectNoneLink";
			this.selectNoneLink.NavMode = false;
			this.selectNoneLink.Selected = false;
			this.selectNoneLink.Size = new System.Drawing.Size(96, 20);
			this.selectNoneLink.StrictColors = false;
			this.selectNoneLink.TabIndex = 3;
			this.selectNoneLink.TabStop = true;
			this.selectNoneLink.Text = "Select None";
			this.selectNoneLink.ThemedBack = null;
			this.selectNoneLink.ThemedFore = null;
			this.selectNoneLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.selectNoneLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.SelectNoneNotebooks);
			// 
			// buttonPanel
			// 
			this.buttonPanel.BackColor = System.Drawing.SystemColors.Control;
			this.buttonPanel.Controls.Add(this.cancelButton);
			this.buttonPanel.Controls.Add(this.okButton);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.buttonPanel.Location = new System.Drawing.Point(0, 574);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Size = new System.Drawing.Size(600, 61);
			this.buttonPanel.TabIndex = 2;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(473, 13);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(115, 36);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(352, 13);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(115, 36);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.SaveInclusion);
			// 
			// NotebooksDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(600, 635);
			this.Controls.Add(this.notebooksPanel);
			this.Controls.Add(this.introPanel);
			this.Controls.Add(this.buttonPanel);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(620, 400);
			this.Name = "NotebooksDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Notebook Selection";
			this.TopMost = true;
			this.introPanel.ResumeLayout(false);
			this.notebooksPanel.ResumeLayout(false);
			this.notebooksPanel.PerformLayout();
			this.buttonPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel introPanel;
		private River.OneMoreAddIn.UI.MoreLabel infoLabel;
		private River.OneMoreAddIn.UI.MorePanel notebooksPanel;
		private River.OneMoreAddIn.UI.MoreLinkLabel selectAllLink;
		private River.OneMoreAddIn.UI.MoreLabel sep1;
		private River.OneMoreAddIn.UI.MoreLinkLabel selectNoneLink;
		private River.OneMoreAddIn.UI.MorePanel booksPanel;
		private System.Windows.Forms.Panel buttonPanel;
		private River.OneMoreAddIn.UI.MoreButton cancelButton;
		private River.OneMoreAddIn.UI.MoreButton okButton;
	}
}
