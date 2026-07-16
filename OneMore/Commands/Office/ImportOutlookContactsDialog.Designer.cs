namespace River.OneMoreAddIn.Commands
{
	partial class ImportOutlookContactsDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportOutlookContactsDialog));
			this.stepIndicator = new River.OneMoreAddIn.UI.MoreStepIndicator();
			this.folderPanel = new System.Windows.Forms.Panel();
			this.categoryPanel = new System.Windows.Forms.Panel();
			this.contactPanel = new System.Windows.Forms.Panel();
			this.optionsPanel = new System.Windows.Forms.Panel();
			this.templateGroupBox = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.bothRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.businessRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.personalRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.sortGroupBox = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.companyRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.firstNameRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.lastNameRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.footerPanel = new System.Windows.Forms.Panel();
			this.selectionCountLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.nextButton = new River.OneMoreAddIn.UI.MoreButton();
			this.backButton = new River.OneMoreAddIn.UI.MoreButton();
			this.optionsPanel.SuspendLayout();
			this.templateGroupBox.SuspendLayout();
			this.sortGroupBox.SuspendLayout();
			this.footerPanel.SuspendLayout();
			this.SuspendLayout();
			//
			// stepIndicator
			//
			this.stepIndicator.CurrentStep = 1;
			this.stepIndicator.Dock = System.Windows.Forms.DockStyle.Top;
			this.stepIndicator.Labels = new string[] {
        River.OneMoreAddIn.Properties.Resources.ImportOutlookContactsDialog_stepFolders,
        River.OneMoreAddIn.Properties.Resources.ImportOutlookContactsDialog_stepCategories,
        River.OneMoreAddIn.Properties.Resources.ImportOutlookContactsDialog_stepContacts,
        River.OneMoreAddIn.Properties.Resources.ImportOutlookContactsDialog_stepOptions};
			this.stepIndicator.Location = new System.Drawing.Point(16, 16);
			this.stepIndicator.Name = "stepIndicator";
			this.stepIndicator.Size = new System.Drawing.Size(746, 72);
			this.stepIndicator.TabIndex = 0;
			// 
			// folderPanel
			// 
			this.folderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.folderPanel.Location = new System.Drawing.Point(16, 88);
			this.folderPanel.Name = "folderPanel";
			this.folderPanel.Size = new System.Drawing.Size(746, 385);
			this.folderPanel.TabIndex = 1;
			// 
			// categoryPanel
			// 
			this.categoryPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.categoryPanel.Location = new System.Drawing.Point(16, 88);
			this.categoryPanel.Name = "categoryPanel";
			this.categoryPanel.Size = new System.Drawing.Size(746, 385);
			this.categoryPanel.TabIndex = 2;
			this.categoryPanel.Visible = false;
			// 
			// contactPanel
			// 
			this.contactPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.contactPanel.Location = new System.Drawing.Point(16, 88);
			this.contactPanel.Name = "contactPanel";
			this.contactPanel.Size = new System.Drawing.Size(746, 385);
			this.contactPanel.TabIndex = 3;
			this.contactPanel.Visible = false;
			//
			// optionsPanel
			//
			this.optionsPanel.Controls.Add(this.sortGroupBox);
			this.optionsPanel.Controls.Add(this.templateGroupBox);
			this.optionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.optionsPanel.Location = new System.Drawing.Point(16, 88);
			this.optionsPanel.Name = "optionsPanel";
			this.optionsPanel.Size = new System.Drawing.Size(746, 385);
			this.optionsPanel.TabIndex = 4;
			this.optionsPanel.Visible = false;
			//
			// templateGroupBox
			//
			this.templateGroupBox.Controls.Add(this.bothRadio);
			this.templateGroupBox.Controls.Add(this.businessRadio);
			this.templateGroupBox.Controls.Add(this.personalRadio);
			this.templateGroupBox.Location = new System.Drawing.Point(0, 0);
			this.templateGroupBox.Name = "templateGroupBox";
			this.templateGroupBox.Padding = new System.Windows.Forms.Padding(3, 15, 3, 3);
			this.templateGroupBox.ShowOnlyTopEdge = true;
			this.templateGroupBox.Size = new System.Drawing.Size(560, 160);
			this.templateGroupBox.TabIndex = 0;
			this.templateGroupBox.TabStop = false;
			this.templateGroupBox.Text = "Template";
			this.templateGroupBox.ThemedBorder = null;
			this.templateGroupBox.ThemedFore = null;
			//
			// bothRadio
			//
			this.bothRadio.Checked = true;
			this.bothRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.bothRadio.Location = new System.Drawing.Point(18, 96);
			this.bothRadio.Name = "bothRadio";
			this.bothRadio.Size = new System.Drawing.Size(520, 25);
			this.bothRadio.TabIndex = 2;
			this.bothRadio.TabStop = true;
			this.bothRadio.Text = "Both - show both personal and business details";
			this.bothRadio.UseVisualStyleBackColor = true;
			//
			// businessRadio
			//
			this.businessRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.businessRadio.Location = new System.Drawing.Point(18, 65);
			this.businessRadio.Name = "businessRadio";
			this.businessRadio.Size = new System.Drawing.Size(520, 25);
			this.businessRadio.TabIndex = 1;
			this.businessRadio.Text = "Business - show only Business details";
			this.businessRadio.UseVisualStyleBackColor = true;
			//
			// personalRadio
			//
			this.personalRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.personalRadio.Location = new System.Drawing.Point(18, 34);
			this.personalRadio.Name = "personalRadio";
			this.personalRadio.Size = new System.Drawing.Size(520, 25);
			this.personalRadio.TabIndex = 0;
			this.personalRadio.Text = "Personal - show only Personal/Home details";
			this.personalRadio.UseVisualStyleBackColor = true;
			//
			// sortGroupBox
			//
			this.sortGroupBox.Controls.Add(this.companyRadio);
			this.sortGroupBox.Controls.Add(this.firstNameRadio);
			this.sortGroupBox.Controls.Add(this.lastNameRadio);
			this.sortGroupBox.Location = new System.Drawing.Point(0, 180);
			this.sortGroupBox.Name = "sortGroupBox";
			this.sortGroupBox.Padding = new System.Windows.Forms.Padding(3, 15, 3, 3);
			this.sortGroupBox.ShowOnlyTopEdge = true;
			this.sortGroupBox.Size = new System.Drawing.Size(560, 160);
			this.sortGroupBox.TabIndex = 1;
			this.sortGroupBox.TabStop = false;
			this.sortGroupBox.Text = "Sort by";
			this.sortGroupBox.ThemedBorder = null;
			this.sortGroupBox.ThemedFore = null;
			//
			// companyRadio
			//
			this.companyRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.companyRadio.Location = new System.Drawing.Point(18, 96);
			this.companyRadio.Name = "companyRadio";
			this.companyRadio.Size = new System.Drawing.Size(520, 25);
			this.companyRadio.TabIndex = 2;
			this.companyRadio.Text = "Company name";
			this.companyRadio.UseVisualStyleBackColor = true;
			//
			// firstNameRadio
			//
			this.firstNameRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.firstNameRadio.Location = new System.Drawing.Point(18, 65);
			this.firstNameRadio.Name = "firstNameRadio";
			this.firstNameRadio.Size = new System.Drawing.Size(520, 25);
			this.firstNameRadio.TabIndex = 1;
			this.firstNameRadio.Text = "First name";
			this.firstNameRadio.UseVisualStyleBackColor = true;
			//
			// lastNameRadio
			//
			this.lastNameRadio.Checked = true;
			this.lastNameRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lastNameRadio.Location = new System.Drawing.Point(18, 34);
			this.lastNameRadio.Name = "lastNameRadio";
			this.lastNameRadio.Size = new System.Drawing.Size(520, 25);
			this.lastNameRadio.TabIndex = 0;
			this.lastNameRadio.TabStop = true;
			this.lastNameRadio.Text = "Last name";
			this.lastNameRadio.UseVisualStyleBackColor = true;
			//
			// footerPanel
			// 
			this.footerPanel.Controls.Add(this.selectionCountLabel);
			this.footerPanel.Controls.Add(this.cancelButton);
			this.footerPanel.Controls.Add(this.nextButton);
			this.footerPanel.Controls.Add(this.backButton);
			this.footerPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.footerPanel.Location = new System.Drawing.Point(16, 473);
			this.footerPanel.Name = "footerPanel";
			this.footerPanel.Size = new System.Drawing.Size(746, 55);
			this.footerPanel.TabIndex = 4;
			// 
			// selectionCountLabel
			// 
			this.selectionCountLabel.AutoSize = true;
			this.selectionCountLabel.Location = new System.Drawing.Point(0, 15);
			this.selectionCountLabel.Name = "selectionCountLabel";
			this.selectionCountLabel.Size = new System.Drawing.Size(0, 20);
			this.selectionCountLabel.TabIndex = 0;
			this.selectionCountLabel.ThemedBack = null;
			this.selectionCountLabel.ThemedFore = null;
			this.selectionCountLabel.Visible = false;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(643, 11);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 3;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = false;
			// 
			// nextButton
			// 
			this.nextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.nextButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.nextButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.nextButton.ImageOver = null;
			this.nextButton.Location = new System.Drawing.Point(537, 11);
			this.nextButton.Name = "nextButton";
			this.nextButton.ShowBorder = true;
			this.nextButton.Size = new System.Drawing.Size(100, 38);
			this.nextButton.StylizeImage = false;
			this.nextButton.TabIndex = 2;
			this.nextButton.Text = "Next >";
			this.nextButton.ThemedBack = null;
			this.nextButton.ThemedFore = null;
			this.nextButton.UseVisualStyleBackColor = false;
			this.nextButton.Click += new System.EventHandler(this.NextButton_Click);
			// 
			// backButton
			// 
			this.backButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.backButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.backButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.backButton.ImageOver = null;
			this.backButton.Location = new System.Drawing.Point(431, 11);
			this.backButton.Name = "backButton";
			this.backButton.ShowBorder = true;
			this.backButton.Size = new System.Drawing.Size(100, 38);
			this.backButton.StylizeImage = false;
			this.backButton.TabIndex = 1;
			this.backButton.Text = "< Back";
			this.backButton.ThemedBack = null;
			this.backButton.ThemedFore = null;
			this.backButton.UseVisualStyleBackColor = false;
			this.backButton.Click += new System.EventHandler(this.BackButton_Click);
			// 
			// ImportOutlookContactsDialog
			// 
			this.AcceptButton = this.nextButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(778, 544);
			this.Controls.Add(this.optionsPanel);
			this.Controls.Add(this.contactPanel);
			this.Controls.Add(this.categoryPanel);
			this.Controls.Add(this.folderPanel);
			this.Controls.Add(this.footerPanel);
			this.Controls.Add(this.stepIndicator);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(800, 600);
			this.Name = "ImportOutlookContactsDialog";
			this.Padding = new System.Windows.Forms.Padding(16);
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Import Outlook Contacts";
			this.optionsPanel.ResumeLayout(false);
			this.templateGroupBox.ResumeLayout(false);
			this.sortGroupBox.ResumeLayout(false);
			this.footerPanel.ResumeLayout(false);
			this.footerPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreStepIndicator stepIndicator;
		private System.Windows.Forms.Panel folderPanel;
		private System.Windows.Forms.Panel categoryPanel;
		private System.Windows.Forms.Panel contactPanel;
		private System.Windows.Forms.Panel optionsPanel;
		private UI.MoreGroupBox templateGroupBox;
		private UI.MoreRadioButton personalRadio;
		private UI.MoreRadioButton businessRadio;
		private UI.MoreRadioButton bothRadio;
		private UI.MoreGroupBox sortGroupBox;
		private UI.MoreRadioButton lastNameRadio;
		private UI.MoreRadioButton firstNameRadio;
		private UI.MoreRadioButton companyRadio;
		private System.Windows.Forms.Panel footerPanel;
		private UI.MoreLabel selectionCountLabel;
		private UI.MoreButton backButton;
		private UI.MoreButton nextButton;
		private UI.MoreButton cancelButton;
	}
}
