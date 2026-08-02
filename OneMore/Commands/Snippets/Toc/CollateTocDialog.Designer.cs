namespace River.OneMoreAddIn.Commands
{
	partial class CollateTocDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CollateTocDialog));
			this.introPanel = new System.Windows.Forms.Panel();
			this.introLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.notebooksHostPanel = new River.OneMoreAddIn.UI.MorePanel();
			this.hashtagPanel = new System.Windows.Forms.Panel();
			this.tagBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.hashtagLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.introPanel.SuspendLayout();
			this.hashtagPanel.SuspendLayout();
			this.buttonPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// introPanel
			// 
			this.introPanel.BackColor = System.Drawing.SystemColors.Control;
			this.introPanel.Controls.Add(this.introLabel);
			this.introPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.introPanel.Location = new System.Drawing.Point(0, 0);
			this.introPanel.Name = "introPanel";
			this.introPanel.Padding = new System.Windows.Forms.Padding(30, 16, 20, 12);
			this.introPanel.Size = new System.Drawing.Size(684, 117);
			this.introPanel.TabIndex = 0;
			// 
			// introLabel
			// 
			this.introLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.introLabel.Location = new System.Drawing.Point(30, 16);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(634, 89);
			this.introLabel.TabIndex = 0;
			this.introLabel.Text = "Select the notebooks to search and enter one or more hashtags. Pages tagged with " +
    "at least one of the specified hashtags will have their tables of content collate" +
    "d into a single index page.";
			this.introLabel.ThemedBack = null;
			this.introLabel.ThemedFore = null;
			// 
			// notebooksHostPanel
			// 
			this.notebooksHostPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.notebooksHostPanel.BottomBorderColor = System.Drawing.Color.Transparent;
			this.notebooksHostPanel.BottomBorderSize = 0;
			this.notebooksHostPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.notebooksHostPanel.Location = new System.Drawing.Point(0, 117);
			this.notebooksHostPanel.Name = "notebooksHostPanel";
			this.notebooksHostPanel.Padding = new System.Windows.Forms.Padding(30, 0, 20, 20);
			this.notebooksHostPanel.Size = new System.Drawing.Size(684, 357);
			this.notebooksHostPanel.TabIndex = 1;
			this.notebooksHostPanel.ThemedBack = "ControlLightLight";
			this.notebooksHostPanel.ThemedFore = null;
			this.notebooksHostPanel.TopBorderColor = System.Drawing.SystemColors.Control;
			this.notebooksHostPanel.TopBorderSize = 0;
			// 
			// hashtagPanel
			// 
			this.hashtagPanel.BackColor = System.Drawing.SystemColors.Control;
			this.hashtagPanel.Controls.Add(this.tagBox);
			this.hashtagPanel.Controls.Add(this.hashtagLabel);
			this.hashtagPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.hashtagPanel.Location = new System.Drawing.Point(0, 474);
			this.hashtagPanel.Name = "hashtagPanel";
			this.hashtagPanel.Padding = new System.Windows.Forms.Padding(30, 10, 20, 10);
			this.hashtagPanel.Size = new System.Drawing.Size(684, 60);
			this.hashtagPanel.TabIndex = 2;
			// 
			// tagBox
			// 
			this.tagBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tagBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.tagBox.Location = new System.Drawing.Point(140, 15);
			this.tagBox.Name = "tagBox";
			this.tagBox.ProcessEnterKey = false;
			this.tagBox.Size = new System.Drawing.Size(524, 26);
			this.tagBox.TabIndex = 1;
			this.tagBox.ThemedBack = null;
			this.tagBox.ThemedFore = null;
			// 
			// hashtagLabel
			// 
			this.hashtagLabel.AutoSize = true;
			this.hashtagLabel.Location = new System.Drawing.Point(30, 18);
			this.hashtagLabel.Name = "hashtagLabel";
			this.hashtagLabel.Size = new System.Drawing.Size(78, 20);
			this.hashtagLabel.TabIndex = 0;
			this.hashtagLabel.Text = "Hashtags";
			this.hashtagLabel.ThemedBack = null;
			this.hashtagLabel.ThemedFore = null;
			// 
			// buttonPanel
			// 
			this.buttonPanel.BackColor = System.Drawing.SystemColors.Control;
			this.buttonPanel.Controls.Add(this.cancelButton);
			this.buttonPanel.Controls.Add(this.okButton);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.buttonPanel.Location = new System.Drawing.Point(0, 534);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Size = new System.Drawing.Size(684, 61);
			this.buttonPanel.TabIndex = 3;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(557, 13);
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
			this.okButton.Location = new System.Drawing.Point(436, 13);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(115, 36);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.Accept);
			// 
			// CollateTocDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(684, 595);
			this.Controls.Add(this.notebooksHostPanel);
			this.Controls.Add(this.hashtagPanel);
			this.Controls.Add(this.buttonPanel);
			this.Controls.Add(this.introPanel);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(620, 400);
			this.Name = "CollateTocDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Collate Tables of Content";
			this.TopMost = true;
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoKeyDown);
			this.introPanel.ResumeLayout(false);
			this.hashtagPanel.ResumeLayout(false);
			this.hashtagPanel.PerformLayout();
			this.buttonPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel introPanel;
		private River.OneMoreAddIn.UI.MoreLabel introLabel;
		private River.OneMoreAddIn.UI.MorePanel notebooksHostPanel;
		private System.Windows.Forms.Panel hashtagPanel;
		private River.OneMoreAddIn.UI.MoreTextBox tagBox;
		private River.OneMoreAddIn.UI.MoreLabel hashtagLabel;
		private System.Windows.Forms.Panel buttonPanel;
		private River.OneMoreAddIn.UI.MoreButton cancelButton;
		private River.OneMoreAddIn.UI.MoreButton okButton;
	}
}
