namespace River.OneMoreAddIn.Dialogs
{
	partial class XmlDialog
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose (bool disposing)
		{
			if (disposing)
			{
				if (manager != null)
				{
					manager.Dispose();
				}

				if (components != null)
				{
					components.Dispose();
				}
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XmlDialog));
			this.tabs = new System.Windows.Forms.TabControl();
			this.pageTab = new System.Windows.Forms.TabPage();
			this.pageBox = new System.Windows.Forms.RichTextBox();
			this.hierTab = new System.Windows.Forms.TabPage();
			this.panel2 = new System.Windows.Forms.Panel();
			this.hierBox = new System.Windows.Forms.RichTextBox();
			this.hierButtonsPanel = new System.Windows.Forms.FlowLayoutPanel();
			this.notebooksHierButton = new System.Windows.Forms.RadioButton();
			this.sectionsHierButton = new System.Windows.Forms.RadioButton();
			this.pagesHierButton = new System.Windows.Forms.RadioButton();
			this.currSectionButton = new System.Windows.Forms.RadioButton();
			this.currNotebookButton = new System.Windows.Forms.RadioButton();
			this.cancelButton = new System.Windows.Forms.Button();
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.okButton = new System.Windows.Forms.Button();
			this.pageLink = new System.Windows.Forms.Label();
			this.pagePath = new System.Windows.Forms.Label();
			this.pageName = new System.Windows.Forms.Label();
			this.pageLinkLabel = new System.Windows.Forms.Label();
			this.pagePathLabel = new System.Windows.Forms.Label();
			this.pageNameLabel = new System.Windows.Forms.Label();
			this.pageInfoPanel = new System.Windows.Forms.Panel();
			this.hideLFBox = new System.Windows.Forms.CheckBox();
			this.hideBox = new System.Windows.Forms.CheckBox();
			this.pageInfoBox = new System.Windows.Forms.ListBox();
			this.pageInfoLabel = new System.Windows.Forms.Label();
			this.selectButton = new System.Windows.Forms.Button();
			this.topPanel = new System.Windows.Forms.Panel();
			this.wrapBox = new System.Windows.Forms.CheckBox();
			this.findBox = new System.Windows.Forms.TextBox();
			this.findButton = new System.Windows.Forms.Button();
			this.masterPanel = new System.Windows.Forms.Panel();
			this.tabs.SuspendLayout();
			this.pageTab.SuspendLayout();
			this.hierTab.SuspendLayout();
			this.panel2.SuspendLayout();
			this.hierButtonsPanel.SuspendLayout();
			this.buttonPanel.SuspendLayout();
			this.pageInfoPanel.SuspendLayout();
			this.topPanel.SuspendLayout();
			this.masterPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabs
			// 
			this.tabs.Controls.Add(this.pageTab);
			this.tabs.Controls.Add(this.hierTab);
			this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabs.Location = new System.Drawing.Point(0, 83);
			this.tabs.Margin = new System.Windows.Forms.Padding(1);
			this.tabs.Name = "tabs";
			this.tabs.SelectedIndex = 0;
			this.tabs.Size = new System.Drawing.Size(779, 477);
			this.tabs.TabIndex = 0;
			this.tabs.SelectedIndexChanged += new System.EventHandler(this.ChangeSelectedTab);
			// 
			// pageTab
			// 
			this.pageTab.Controls.Add(this.pageBox);
			this.pageTab.Location = new System.Drawing.Point(4, 22);
			this.pageTab.Margin = new System.Windows.Forms.Padding(1);
			this.pageTab.Name = "pageTab";
			this.pageTab.Padding = new System.Windows.Forms.Padding(1);
			this.pageTab.Size = new System.Drawing.Size(771, 451);
			this.pageTab.TabIndex = 0;
			this.pageTab.Text = "Page";
			this.pageTab.UseVisualStyleBackColor = true;
			// 
			// pageBox
			// 
			this.pageBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pageBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pageBox.Font = new System.Drawing.Font("Lucida Console", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pageBox.Location = new System.Drawing.Point(1, 1);
			this.pageBox.Margin = new System.Windows.Forms.Padding(1);
			this.pageBox.Name = "pageBox";
			this.pageBox.Size = new System.Drawing.Size(769, 449);
			this.pageBox.TabIndex = 7;
			this.pageBox.Text = "";
			this.pageBox.WordWrap = false;
			this.pageBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.XmlBoxKeyUp);
			// 
			// hierTab
			// 
			this.hierTab.Controls.Add(this.panel2);
			this.hierTab.Location = new System.Drawing.Point(4, 22);
			this.hierTab.Margin = new System.Windows.Forms.Padding(1);
			this.hierTab.Name = "hierTab";
			this.hierTab.Padding = new System.Windows.Forms.Padding(1);
			this.hierTab.Size = new System.Drawing.Size(771, 451);
			this.hierTab.TabIndex = 1;
			this.hierTab.Text = "Hierarchy";
			this.hierTab.UseVisualStyleBackColor = true;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.hierBox);
			this.panel2.Controls.Add(this.hierButtonsPanel);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(1, 1);
			this.panel2.Margin = new System.Windows.Forms.Padding(2);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(769, 449);
			this.panel2.TabIndex = 6;
			// 
			// hierBox
			// 
			this.hierBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.hierBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.hierBox.Font = new System.Drawing.Font("Lucida Console", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.hierBox.Location = new System.Drawing.Point(0, 38);
			this.hierBox.Margin = new System.Windows.Forms.Padding(1);
			this.hierBox.Name = "hierBox";
			this.hierBox.Size = new System.Drawing.Size(769, 411);
			this.hierBox.TabIndex = 0;
			this.hierBox.Text = "";
			this.hierBox.WordWrap = false;
			this.hierBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.XmlBoxKeyUp);
			// 
			// hierButtonsPanel
			// 
			this.hierButtonsPanel.BackColor = System.Drawing.SystemColors.Info;
			this.hierButtonsPanel.Controls.Add(this.notebooksHierButton);
			this.hierButtonsPanel.Controls.Add(this.sectionsHierButton);
			this.hierButtonsPanel.Controls.Add(this.pagesHierButton);
			this.hierButtonsPanel.Controls.Add(this.currSectionButton);
			this.hierButtonsPanel.Controls.Add(this.currNotebookButton);
			this.hierButtonsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.hierButtonsPanel.Location = new System.Drawing.Point(0, 0);
			this.hierButtonsPanel.Name = "hierButtonsPanel";
			this.hierButtonsPanel.Padding = new System.Windows.Forms.Padding(5);
			this.hierButtonsPanel.Size = new System.Drawing.Size(769, 38);
			this.hierButtonsPanel.TabIndex = 1;
			// 
			// notebooksHierButton
			// 
			this.notebooksHierButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.notebooksHierButton.AutoSize = true;
			this.notebooksHierButton.Checked = true;
			this.notebooksHierButton.Location = new System.Drawing.Point(7, 7);
			this.notebooksHierButton.Margin = new System.Windows.Forms.Padding(2);
			this.notebooksHierButton.Name = "notebooksHierButton";
			this.notebooksHierButton.Size = new System.Drawing.Size(69, 23);
			this.notebooksHierButton.TabIndex = 2;
			this.notebooksHierButton.TabStop = true;
			this.notebooksHierButton.Text = "Notebooks";
			this.notebooksHierButton.UseVisualStyleBackColor = true;
			this.notebooksHierButton.CheckedChanged += new System.EventHandler(this.ShowNotebooks);
			// 
			// sectionsHierButton
			// 
			this.sectionsHierButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.sectionsHierButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.sectionsHierButton.AutoSize = true;
			this.sectionsHierButton.Location = new System.Drawing.Point(80, 7);
			this.sectionsHierButton.Margin = new System.Windows.Forms.Padding(2);
			this.sectionsHierButton.Name = "sectionsHierButton";
			this.sectionsHierButton.Size = new System.Drawing.Size(58, 23);
			this.sectionsHierButton.TabIndex = 3;
			this.sectionsHierButton.Text = "Sections";
			this.sectionsHierButton.UseVisualStyleBackColor = true;
			this.sectionsHierButton.CheckedChanged += new System.EventHandler(this.ShowSections);
			// 
			// pagesHierButton
			// 
			this.pagesHierButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.pagesHierButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.pagesHierButton.AutoSize = true;
			this.pagesHierButton.Location = new System.Drawing.Point(142, 7);
			this.pagesHierButton.Margin = new System.Windows.Forms.Padding(2);
			this.pagesHierButton.Name = "pagesHierButton";
			this.pagesHierButton.Size = new System.Drawing.Size(47, 23);
			this.pagesHierButton.TabIndex = 4;
			this.pagesHierButton.Text = "Pages";
			this.pagesHierButton.UseVisualStyleBackColor = true;
			this.pagesHierButton.CheckedChanged += new System.EventHandler(this.ShowPages);
			// 
			// currSectionButton
			// 
			this.currSectionButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.currSectionButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.currSectionButton.AutoSize = true;
			this.currSectionButton.Location = new System.Drawing.Point(193, 7);
			this.currSectionButton.Margin = new System.Windows.Forms.Padding(2);
			this.currSectionButton.Name = "currSectionButton";
			this.currSectionButton.Size = new System.Drawing.Size(90, 23);
			this.currSectionButton.TabIndex = 5;
			this.currSectionButton.Text = "Current Section";
			this.currSectionButton.UseVisualStyleBackColor = true;
			this.currSectionButton.CheckedChanged += new System.EventHandler(this.ShowCurrentSection);
			// 
			// currNotebookButton
			// 
			this.currNotebookButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.currNotebookButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.currNotebookButton.AutoSize = true;
			this.currNotebookButton.Location = new System.Drawing.Point(287, 7);
			this.currNotebookButton.Margin = new System.Windows.Forms.Padding(2);
			this.currNotebookButton.Name = "currNotebookButton";
			this.currNotebookButton.Size = new System.Drawing.Size(101, 23);
			this.currNotebookButton.TabIndex = 7;
			this.currNotebookButton.Text = "Current Notebook";
			this.currNotebookButton.UseVisualStyleBackColor = true;
			this.currNotebookButton.CheckedChanged += new System.EventHandler(this.ShowCurrentNotebook);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(685, 12);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(1);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(83, 23);
			this.cancelButton.TabIndex = 9;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.Click += new System.EventHandler(this.Close);
			// 
			// buttonPanel
			// 
			this.buttonPanel.Controls.Add(this.cancelButton);
			this.buttonPanel.Controls.Add(this.okButton);
			this.buttonPanel.Controls.Add(this.pageLink);
			this.buttonPanel.Controls.Add(this.pagePath);
			this.buttonPanel.Controls.Add(this.pageName);
			this.buttonPanel.Controls.Add(this.pageLinkLabel);
			this.buttonPanel.Controls.Add(this.pagePathLabel);
			this.buttonPanel.Controls.Add(this.pageNameLabel);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.Location = new System.Drawing.Point(0, 560);
			this.buttonPanel.Margin = new System.Windows.Forms.Padding(1);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Size = new System.Drawing.Size(779, 44);
			this.buttonPanel.TabIndex = 4;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.Location = new System.Drawing.Point(597, 12);
			this.okButton.Margin = new System.Windows.Forms.Padding(1);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(83, 23);
			this.okButton.TabIndex = 8;
			this.okButton.Text = "Update Page";
			this.okButton.Click += new System.EventHandler(this.Update);
			// 
			// pageLink
			// 
			this.pageLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.pageLink.AutoSize = true;
			this.pageLink.Location = new System.Drawing.Point(43, 31);
			this.pageLink.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.pageLink.Name = "pageLink";
			this.pageLink.Size = new System.Drawing.Size(10, 13);
			this.pageLink.TabIndex = 0;
			this.pageLink.Text = "-";
			// 
			// pagePath
			// 
			this.pagePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.pagePath.AutoSize = true;
			this.pagePath.Location = new System.Drawing.Point(43, 18);
			this.pagePath.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.pagePath.Name = "pagePath";
			this.pagePath.Size = new System.Drawing.Size(10, 13);
			this.pagePath.TabIndex = 0;
			this.pagePath.Text = "-";
			// 
			// pageName
			// 
			this.pageName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.pageName.AutoSize = true;
			this.pageName.Location = new System.Drawing.Point(43, 5);
			this.pageName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.pageName.Name = "pageName";
			this.pageName.Size = new System.Drawing.Size(10, 13);
			this.pageName.TabIndex = 0;
			this.pageName.Text = "-";
			// 
			// pageLinkLabel
			// 
			this.pageLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.pageLinkLabel.AutoSize = true;
			this.pageLinkLabel.Location = new System.Drawing.Point(2, 31);
			this.pageLinkLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.pageLinkLabel.Name = "pageLinkLabel";
			this.pageLinkLabel.Size = new System.Drawing.Size(30, 13);
			this.pageLinkLabel.TabIndex = 0;
			this.pageLinkLabel.Text = "Link:";
			// 
			// pagePathLabel
			// 
			this.pagePathLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.pagePathLabel.AutoSize = true;
			this.pagePathLabel.Location = new System.Drawing.Point(2, 18);
			this.pagePathLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.pagePathLabel.Name = "pagePathLabel";
			this.pagePathLabel.Size = new System.Drawing.Size(32, 13);
			this.pagePathLabel.TabIndex = 0;
			this.pagePathLabel.Text = "Path:";
			// 
			// pageNameLabel
			// 
			this.pageNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.pageNameLabel.AutoSize = true;
			this.pageNameLabel.Location = new System.Drawing.Point(2, 5);
			this.pageNameLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.pageNameLabel.Name = "pageNameLabel";
			this.pageNameLabel.Size = new System.Drawing.Size(38, 13);
			this.pageNameLabel.TabIndex = 0;
			this.pageNameLabel.Text = "Name:";
			// 
			// pageInfoPanel
			// 
			this.pageInfoPanel.Controls.Add(this.hideLFBox);
			this.pageInfoPanel.Controls.Add(this.hideBox);
			this.pageInfoPanel.Controls.Add(this.pageInfoBox);
			this.pageInfoPanel.Controls.Add(this.pageInfoLabel);
			this.pageInfoPanel.Location = new System.Drawing.Point(272, 5);
			this.pageInfoPanel.Margin = new System.Windows.Forms.Padding(2);
			this.pageInfoPanel.Name = "pageInfoPanel";
			this.pageInfoPanel.Size = new System.Drawing.Size(413, 68);
			this.pageInfoPanel.TabIndex = 9;
			// 
			// hideLFBox
			// 
			this.hideLFBox.AutoSize = true;
			this.hideLFBox.Location = new System.Drawing.Point(249, 34);
			this.hideLFBox.Name = "hideLFBox";
			this.hideLFBox.Size = new System.Drawing.Size(143, 17);
			this.hideLFBox.TabIndex = 6;
			this.hideLFBox.Text = "Remove LF from CDATA";
			this.hideLFBox.UseVisualStyleBackColor = true;
			this.hideLFBox.CheckedChanged += new System.EventHandler(this.HideAttributes);
			// 
			// hideBox
			// 
			this.hideBox.AutoSize = true;
			this.hideBox.Checked = true;
			this.hideBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.hideBox.Location = new System.Drawing.Point(249, 11);
			this.hideBox.Name = "hideBox";
			this.hideBox.Size = new System.Drawing.Size(140, 17);
			this.hideBox.TabIndex = 5;
			this.hideBox.Text = "Hide edited-by attributes";
			this.hideBox.UseVisualStyleBackColor = true;
			this.hideBox.CheckedChanged += new System.EventHandler(this.HideAttributes);
			// 
			// pageInfoBox
			// 
			this.pageInfoBox.Location = new System.Drawing.Point(63, 7);
			this.pageInfoBox.Name = "pageInfoBox";
			this.pageInfoBox.Size = new System.Drawing.Size(150, 56);
			this.pageInfoBox.TabIndex = 4;
			this.pageInfoBox.SelectedValueChanged += new System.EventHandler(this.ChangeInfoScope);
			// 
			// pageInfoLabel
			// 
			this.pageInfoLabel.AutoSize = true;
			this.pageInfoLabel.Location = new System.Drawing.Point(5, 12);
			this.pageInfoLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.pageInfoLabel.Name = "pageInfoLabel";
			this.pageInfoLabel.Size = new System.Drawing.Size(53, 13);
			this.pageInfoLabel.TabIndex = 0;
			this.pageInfoLabel.Text = "PageInfo:";
			// 
			// selectButton
			// 
			this.selectButton.Image = ((System.Drawing.Image)(resources.GetObject("selectButton.Image")));
			this.selectButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.selectButton.Location = new System.Drawing.Point(110, 36);
			this.selectButton.Margin = new System.Windows.Forms.Padding(1);
			this.selectButton.Name = "selectButton";
			this.selectButton.Size = new System.Drawing.Size(78, 23);
			this.selectButton.TabIndex = 3;
			this.selectButton.Text = "Select All";
			this.selectButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.selectButton.Click += new System.EventHandler(this.SelectAll);
			// 
			// topPanel
			// 
			this.topPanel.Controls.Add(this.wrapBox);
			this.topPanel.Controls.Add(this.findBox);
			this.topPanel.Controls.Add(this.findButton);
			this.topPanel.Controls.Add(this.selectButton);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = new System.Drawing.Point(0, 0);
			this.topPanel.Margin = new System.Windows.Forms.Padding(1);
			this.topPanel.Name = "topPanel";
			this.topPanel.Padding = new System.Windows.Forms.Padding(5, 5, 0, 0);
			this.topPanel.Size = new System.Drawing.Size(779, 83);
			this.topPanel.TabIndex = 5;
			// 
			// wrapBox
			// 
			this.wrapBox.AutoSize = true;
			this.wrapBox.Location = new System.Drawing.Point(18, 40);
			this.wrapBox.Margin = new System.Windows.Forms.Padding(2);
			this.wrapBox.Name = "wrapBox";
			this.wrapBox.Size = new System.Drawing.Size(72, 17);
			this.wrapBox.TabIndex = 2;
			this.wrapBox.Text = "Wrap text";
			this.wrapBox.UseVisualStyleBackColor = true;
			this.wrapBox.CheckedChanged += new System.EventHandler(this.ChangeWrap);
			// 
			// findBox
			// 
			this.findBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.findBox.Location = new System.Drawing.Point(7, 7);
			this.findBox.Margin = new System.Windows.Forms.Padding(2);
			this.findBox.Name = "findBox";
			this.findBox.Size = new System.Drawing.Size(181, 21);
			this.findBox.TabIndex = 0;
			this.findBox.TextChanged += new System.EventHandler(this.ChangeFindText);
			this.findBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FindBoxKeyUP);
			// 
			// findButton
			// 
			this.findButton.Enabled = false;
			this.findButton.Image = ((System.Drawing.Image)(resources.GetObject("findButton.Image")));
			this.findButton.Location = new System.Drawing.Point(192, 6);
			this.findButton.Margin = new System.Windows.Forms.Padding(2);
			this.findButton.Name = "findButton";
			this.findButton.Size = new System.Drawing.Size(39, 23);
			this.findButton.TabIndex = 1;
			this.findButton.UseVisualStyleBackColor = true;
			this.findButton.Click += new System.EventHandler(this.ClickFind);
			// 
			// masterPanel
			// 
			this.masterPanel.Controls.Add(this.tabs);
			this.masterPanel.Controls.Add(this.topPanel);
			this.masterPanel.Controls.Add(this.buttonPanel);
			this.masterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.masterPanel.Location = new System.Drawing.Point(5, 5);
			this.masterPanel.Margin = new System.Windows.Forms.Padding(1);
			this.masterPanel.Name = "masterPanel";
			this.masterPanel.Size = new System.Drawing.Size(779, 604);
			this.masterPanel.TabIndex = 6;
			// 
			// XmlDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(789, 614);
			this.Controls.Add(this.pageInfoPanel);
			this.Controls.Add(this.masterPanel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(1);
			this.MinimumSize = new System.Drawing.Size(801, 327);
			this.Name = "XmlDialog";
			this.Padding = new System.Windows.Forms.Padding(5);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "OneMore XML";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.tabs.ResumeLayout(false);
			this.pageTab.ResumeLayout(false);
			this.hierTab.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.hierButtonsPanel.ResumeLayout(false);
			this.hierButtonsPanel.PerformLayout();
			this.buttonPanel.ResumeLayout(false);
			this.buttonPanel.PerformLayout();
			this.pageInfoPanel.ResumeLayout(false);
			this.pageInfoPanel.PerformLayout();
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.masterPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl tabs;
		private System.Windows.Forms.TabPage pageTab;
		private System.Windows.Forms.TabPage hierTab;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Panel buttonPanel;
		private System.Windows.Forms.RichTextBox pageBox;
		private System.Windows.Forms.RichTextBox hierBox;
		private System.Windows.Forms.Panel topPanel;
		private System.Windows.Forms.Panel masterPanel;
		private System.Windows.Forms.TextBox findBox;
		private System.Windows.Forms.Button findButton;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.RadioButton pagesHierButton;
		private System.Windows.Forms.RadioButton notebooksHierButton;
		private System.Windows.Forms.RadioButton sectionsHierButton;
		private System.Windows.Forms.RadioButton currSectionButton;
		private System.Windows.Forms.RadioButton currNotebookButton;
		private System.Windows.Forms.Button selectButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Label pageInfoLabel;
		private System.Windows.Forms.Panel pageInfoPanel;
		private System.Windows.Forms.CheckBox wrapBox;
		private System.Windows.Forms.CheckBox hideBox;
		private System.Windows.Forms.CheckBox hideLFBox;
		private System.Windows.Forms.Label pageNameLabel;
		private System.Windows.Forms.Label pageLink;
		private System.Windows.Forms.Label pagePath;
		private System.Windows.Forms.Label pageName;
		private System.Windows.Forms.Label pageLinkLabel;
		private System.Windows.Forms.Label pagePathLabel;
		private System.Windows.Forms.ListBox pageInfoBox;
		private System.Windows.Forms.FlowLayoutPanel hierButtonsPanel;
	}
}