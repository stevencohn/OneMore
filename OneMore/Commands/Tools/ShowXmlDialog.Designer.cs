namespace River.OneMoreAddIn.Commands
{
	partial class ShowXmlDialog
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose (bool disposing)
		{
			if (disposing)
			{
				if (one != null)
				{
					one.Dispose();
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShowXmlDialog));
			this.tabs = new System.Windows.Forms.TabControl();
			this.pageTab = new System.Windows.Forms.TabPage();
			this.pageBox = new System.Windows.Forms.RichTextBox();
			this.hierTab = new System.Windows.Forms.TabPage();
			this.panel2 = new System.Windows.Forms.Panel();
			this.hierBox = new System.Windows.Forms.RichTextBox();
			this.hierButtonsPanel = new System.Windows.Forms.FlowLayoutPanel();
			this.notebooksButton = new System.Windows.Forms.RadioButton();
			this.notebookButton = new System.Windows.Forms.RadioButton();
			this.sectionButton = new System.Windows.Forms.RadioButton();
			this.currSectionButton = new System.Windows.Forms.RadioButton();
			this.manualLabel = new System.Windows.Forms.Label();
			this.manualIdBox = new System.Windows.Forms.TextBox();
			this.manualFxBox = new System.Windows.Forms.ComboBox();
			this.manulRunButton = new System.Windows.Forms.Button();
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
			this.tabs.Location = new System.Drawing.Point(0, 128);
			this.tabs.Margin = new System.Windows.Forms.Padding(2);
			this.tabs.Name = "tabs";
			this.tabs.SelectedIndex = 0;
			this.tabs.Size = new System.Drawing.Size(1462, 832);
			this.tabs.TabIndex = 0;
			this.tabs.SelectedIndexChanged += new System.EventHandler(this.ChangeSelectedTab);
			// 
			// pageTab
			// 
			this.pageTab.Controls.Add(this.pageBox);
			this.pageTab.Location = new System.Drawing.Point(4, 29);
			this.pageTab.Margin = new System.Windows.Forms.Padding(2);
			this.pageTab.Name = "pageTab";
			this.pageTab.Padding = new System.Windows.Forms.Padding(2);
			this.pageTab.Size = new System.Drawing.Size(1454, 799);
			this.pageTab.TabIndex = 0;
			this.pageTab.Text = "Page";
			this.pageTab.UseVisualStyleBackColor = true;
			// 
			// pageBox
			// 
			this.pageBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pageBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pageBox.Font = new System.Drawing.Font("Lucida Console", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pageBox.Location = new System.Drawing.Point(2, 2);
			this.pageBox.Margin = new System.Windows.Forms.Padding(2);
			this.pageBox.Name = "pageBox";
			this.pageBox.Size = new System.Drawing.Size(1450, 795);
			this.pageBox.TabIndex = 7;
			this.pageBox.Text = "";
			this.pageBox.WordWrap = false;
			this.pageBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.XmlBoxKeyUp);
			// 
			// hierTab
			// 
			this.hierTab.Controls.Add(this.panel2);
			this.hierTab.Location = new System.Drawing.Point(4, 29);
			this.hierTab.Margin = new System.Windows.Forms.Padding(2);
			this.hierTab.Name = "hierTab";
			this.hierTab.Padding = new System.Windows.Forms.Padding(2);
			this.hierTab.Size = new System.Drawing.Size(1454, 799);
			this.hierTab.TabIndex = 1;
			this.hierTab.Text = "Hierarchy";
			this.hierTab.UseVisualStyleBackColor = true;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.hierBox);
			this.panel2.Controls.Add(this.hierButtonsPanel);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(2, 2);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(1450, 795);
			this.panel2.TabIndex = 6;
			// 
			// hierBox
			// 
			this.hierBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.hierBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.hierBox.Font = new System.Drawing.Font("Lucida Console", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.hierBox.Location = new System.Drawing.Point(0, 58);
			this.hierBox.Margin = new System.Windows.Forms.Padding(2);
			this.hierBox.Name = "hierBox";
			this.hierBox.Size = new System.Drawing.Size(1450, 737);
			this.hierBox.TabIndex = 0;
			this.hierBox.Text = "";
			this.hierBox.WordWrap = false;
			this.hierBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.XmlBoxKeyUp);
			// 
			// hierButtonsPanel
			// 
			this.hierButtonsPanel.BackColor = System.Drawing.SystemColors.Info;
			this.hierButtonsPanel.Controls.Add(this.notebooksButton);
			this.hierButtonsPanel.Controls.Add(this.notebookButton);
			this.hierButtonsPanel.Controls.Add(this.sectionButton);
			this.hierButtonsPanel.Controls.Add(this.currSectionButton);
			this.hierButtonsPanel.Controls.Add(this.manualLabel);
			this.hierButtonsPanel.Controls.Add(this.manualIdBox);
			this.hierButtonsPanel.Controls.Add(this.manualFxBox);
			this.hierButtonsPanel.Controls.Add(this.manulRunButton);
			this.hierButtonsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.hierButtonsPanel.Location = new System.Drawing.Point(0, 0);
			this.hierButtonsPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.hierButtonsPanel.Name = "hierButtonsPanel";
			this.hierButtonsPanel.Padding = new System.Windows.Forms.Padding(8);
			this.hierButtonsPanel.Size = new System.Drawing.Size(1450, 58);
			this.hierButtonsPanel.TabIndex = 1;
			// 
			// notebooksButton
			// 
			this.notebooksButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.notebooksButton.AutoSize = true;
			this.notebooksButton.Checked = true;
			this.notebooksButton.Location = new System.Drawing.Point(11, 11);
			this.notebooksButton.Name = "notebooksButton";
			this.notebooksButton.Size = new System.Drawing.Size(96, 30);
			this.notebooksButton.TabIndex = 2;
			this.notebooksButton.TabStop = true;
			this.notebooksButton.Text = "Notebooks";
			this.notebooksButton.UseVisualStyleBackColor = true;
			this.notebooksButton.CheckedChanged += new System.EventHandler(this.GetNotebooks);
			// 
			// notebookButton
			// 
			this.notebookButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.notebookButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.notebookButton.AutoSize = true;
			this.notebookButton.Location = new System.Drawing.Point(113, 11);
			this.notebookButton.Name = "notebookButton";
			this.notebookButton.Size = new System.Drawing.Size(159, 30);
			this.notebookButton.TabIndex = 3;
			this.notebookButton.Text = "Notebook+Sections";
			this.notebookButton.UseVisualStyleBackColor = true;
			this.notebookButton.CheckedChanged += new System.EventHandler(this.GetNotebook);
			// 
			// sectionButton
			// 
			this.sectionButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.sectionButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.sectionButton.AutoSize = true;
			this.sectionButton.Location = new System.Drawing.Point(278, 11);
			this.sectionButton.Name = "sectionButton";
			this.sectionButton.Size = new System.Drawing.Size(142, 30);
			this.sectionButton.TabIndex = 4;
			this.sectionButton.Text = "Notebook+Pages";
			this.sectionButton.UseVisualStyleBackColor = true;
			this.sectionButton.CheckedChanged += new System.EventHandler(this.GetSection);
			// 
			// currSectionButton
			// 
			this.currSectionButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.currSectionButton.AutoSize = true;
			this.currSectionButton.Location = new System.Drawing.Point(426, 11);
			this.currSectionButton.Name = "currSectionButton";
			this.currSectionButton.Size = new System.Drawing.Size(127, 30);
			this.currSectionButton.TabIndex = 5;
			this.currSectionButton.Text = "Section+Pages";
			this.currSectionButton.UseVisualStyleBackColor = true;
			this.currSectionButton.CheckedChanged += new System.EventHandler(this.ShowCurrentSection);
			// 
			// manualLabel
			// 
			this.manualLabel.AutoSize = true;
			this.manualLabel.Location = new System.Drawing.Point(656, 16);
			this.manualLabel.Margin = new System.Windows.Forms.Padding(100, 8, 3, 0);
			this.manualLabel.Name = "manualLabel";
			this.manualLabel.Size = new System.Drawing.Size(132, 20);
			this.manualLabel.TabIndex = 6;
			this.manualLabel.Text = "Manual ObjectID:";
			// 
			// manualIdBox
			// 
			this.manualIdBox.Location = new System.Drawing.Point(794, 14);
			this.manualIdBox.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.manualIdBox.Name = "manualIdBox";
			this.manualIdBox.Size = new System.Drawing.Size(300, 26);
			this.manualIdBox.TabIndex = 7;
			this.manualIdBox.TextChanged += new System.EventHandler(this.CheckManualInput);
			// 
			// manualFxBox
			// 
			this.manualFxBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.manualFxBox.FormattingEnabled = true;
			this.manualFxBox.Items.AddRange(new object[] {
            "GetNotebook",
            "GetSection",
            "GetPage"});
			this.manualFxBox.Location = new System.Drawing.Point(1100, 13);
			this.manualFxBox.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
			this.manualFxBox.Name = "manualFxBox";
			this.manualFxBox.Size = new System.Drawing.Size(150, 28);
			this.manualFxBox.TabIndex = 8;
			this.manualFxBox.SelectedIndexChanged += new System.EventHandler(this.CheckManualInput);
			// 
			// manulRunButton
			// 
			this.manulRunButton.Enabled = false;
			this.manulRunButton.Image = ((System.Drawing.Image)(resources.GetObject("manulRunButton.Image")));
			this.manulRunButton.Location = new System.Drawing.Point(1256, 11);
			this.manulRunButton.Name = "manulRunButton";
			this.manulRunButton.Size = new System.Drawing.Size(47, 32);
			this.manulRunButton.TabIndex = 9;
			this.manulRunButton.UseVisualStyleBackColor = true;
			this.manulRunButton.Click += new System.EventHandler(this.RunManual);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(1322, 18);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(2);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(124, 35);
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
			this.buttonPanel.Location = new System.Drawing.Point(0, 960);
			this.buttonPanel.Margin = new System.Windows.Forms.Padding(2);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Size = new System.Drawing.Size(1462, 68);
			this.buttonPanel.TabIndex = 4;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.Enabled = false;
			this.okButton.Location = new System.Drawing.Point(1190, 18);
			this.okButton.Margin = new System.Windows.Forms.Padding(2);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(124, 35);
			this.okButton.TabIndex = 8;
			this.okButton.Text = "Update Page";
			this.okButton.Click += new System.EventHandler(this.Update);
			// 
			// pageLink
			// 
			this.pageLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.pageLink.AutoSize = true;
			this.pageLink.Location = new System.Drawing.Point(64, 48);
			this.pageLink.Name = "pageLink";
			this.pageLink.Size = new System.Drawing.Size(14, 20);
			this.pageLink.TabIndex = 0;
			this.pageLink.Text = "-";
			// 
			// pagePath
			// 
			this.pagePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.pagePath.AutoSize = true;
			this.pagePath.Location = new System.Drawing.Point(64, 28);
			this.pagePath.Name = "pagePath";
			this.pagePath.Size = new System.Drawing.Size(14, 20);
			this.pagePath.TabIndex = 0;
			this.pagePath.Text = "-";
			// 
			// pageName
			// 
			this.pageName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.pageName.AutoSize = true;
			this.pageName.Location = new System.Drawing.Point(64, 8);
			this.pageName.Name = "pageName";
			this.pageName.Size = new System.Drawing.Size(14, 20);
			this.pageName.TabIndex = 0;
			this.pageName.Text = "-";
			// 
			// pageLinkLabel
			// 
			this.pageLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.pageLinkLabel.AutoSize = true;
			this.pageLinkLabel.Location = new System.Drawing.Point(3, 48);
			this.pageLinkLabel.Name = "pageLinkLabel";
			this.pageLinkLabel.Size = new System.Drawing.Size(42, 20);
			this.pageLinkLabel.TabIndex = 0;
			this.pageLinkLabel.Text = "Link:";
			// 
			// pagePathLabel
			// 
			this.pagePathLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.pagePathLabel.AutoSize = true;
			this.pagePathLabel.Location = new System.Drawing.Point(3, 28);
			this.pagePathLabel.Name = "pagePathLabel";
			this.pagePathLabel.Size = new System.Drawing.Size(46, 20);
			this.pagePathLabel.TabIndex = 0;
			this.pagePathLabel.Text = "Path:";
			// 
			// pageNameLabel
			// 
			this.pageNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.pageNameLabel.AutoSize = true;
			this.pageNameLabel.Location = new System.Drawing.Point(3, 8);
			this.pageNameLabel.Name = "pageNameLabel";
			this.pageNameLabel.Size = new System.Drawing.Size(55, 20);
			this.pageNameLabel.TabIndex = 0;
			this.pageNameLabel.Text = "Name:";
			// 
			// pageInfoPanel
			// 
			this.pageInfoPanel.Controls.Add(this.hideLFBox);
			this.pageInfoPanel.Controls.Add(this.hideBox);
			this.pageInfoPanel.Controls.Add(this.pageInfoBox);
			this.pageInfoPanel.Controls.Add(this.pageInfoLabel);
			this.pageInfoPanel.Location = new System.Drawing.Point(408, 8);
			this.pageInfoPanel.Name = "pageInfoPanel";
			this.pageInfoPanel.Size = new System.Drawing.Size(802, 123);
			this.pageInfoPanel.TabIndex = 9;
			// 
			// hideLFBox
			// 
			this.hideLFBox.AutoSize = true;
			this.hideLFBox.Checked = true;
			this.hideLFBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.hideLFBox.Location = new System.Drawing.Point(367, 50);
			this.hideLFBox.Name = "hideLFBox";
			this.hideLFBox.Size = new System.Drawing.Size(211, 24);
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
			this.hideBox.Location = new System.Drawing.Point(367, 20);
			this.hideBox.Name = "hideBox";
			this.hideBox.Size = new System.Drawing.Size(370, 24);
			this.hideBox.TabIndex = 5;
			this.hideBox.Text = "Hide edited-by attributes (uncheck to edit page)";
			this.hideBox.UseVisualStyleBackColor = true;
			this.hideBox.CheckedChanged += new System.EventHandler(this.HideAttributes);
			// 
			// pageInfoBox
			// 
			this.pageInfoBox.ItemHeight = 20;
			this.pageInfoBox.Location = new System.Drawing.Point(94, 11);
			this.pageInfoBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.pageInfoBox.Name = "pageInfoBox";
			this.pageInfoBox.Size = new System.Drawing.Size(223, 104);
			this.pageInfoBox.TabIndex = 4;
			this.pageInfoBox.SelectedValueChanged += new System.EventHandler(this.ChangeInfoScope);
			// 
			// pageInfoLabel
			// 
			this.pageInfoLabel.AutoSize = true;
			this.pageInfoLabel.Location = new System.Drawing.Point(8, 18);
			this.pageInfoLabel.Name = "pageInfoLabel";
			this.pageInfoLabel.Size = new System.Drawing.Size(78, 20);
			this.pageInfoLabel.TabIndex = 0;
			this.pageInfoLabel.Text = "PageInfo:";
			// 
			// selectButton
			// 
			this.selectButton.Image = ((System.Drawing.Image)(resources.GetObject("selectButton.Image")));
			this.selectButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.selectButton.Location = new System.Drawing.Point(165, 55);
			this.selectButton.Margin = new System.Windows.Forms.Padding(2);
			this.selectButton.Name = "selectButton";
			this.selectButton.Size = new System.Drawing.Size(117, 35);
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
			this.topPanel.Margin = new System.Windows.Forms.Padding(2);
			this.topPanel.Name = "topPanel";
			this.topPanel.Padding = new System.Windows.Forms.Padding(8, 8, 0, 0);
			this.topPanel.Size = new System.Drawing.Size(1462, 128);
			this.topPanel.TabIndex = 5;
			// 
			// wrapBox
			// 
			this.wrapBox.AutoSize = true;
			this.wrapBox.Location = new System.Drawing.Point(27, 62);
			this.wrapBox.Name = "wrapBox";
			this.wrapBox.Size = new System.Drawing.Size(103, 24);
			this.wrapBox.TabIndex = 2;
			this.wrapBox.Text = "Wrap text";
			this.wrapBox.UseVisualStyleBackColor = true;
			this.wrapBox.CheckedChanged += new System.EventHandler(this.ChangeWrap);
			// 
			// findBox
			// 
			this.findBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.findBox.Location = new System.Drawing.Point(10, 11);
			this.findBox.Name = "findBox";
			this.findBox.Size = new System.Drawing.Size(270, 28);
			this.findBox.TabIndex = 0;
			this.findBox.TextChanged += new System.EventHandler(this.ChangeFindText);
			this.findBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FindBoxKeyUP);
			// 
			// findButton
			// 
			this.findButton.Enabled = false;
			this.findButton.Image = ((System.Drawing.Image)(resources.GetObject("findButton.Image")));
			this.findButton.Location = new System.Drawing.Point(288, 9);
			this.findButton.Name = "findButton";
			this.findButton.Size = new System.Drawing.Size(58, 35);
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
			this.masterPanel.Location = new System.Drawing.Point(8, 8);
			this.masterPanel.Margin = new System.Windows.Forms.Padding(2);
			this.masterPanel.Name = "masterPanel";
			this.masterPanel.Size = new System.Drawing.Size(1462, 1028);
			this.masterPanel.TabIndex = 6;
			// 
			// ShowXmlDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(1478, 1044);
			this.Controls.Add(this.pageInfoPanel);
			this.Controls.Add(this.masterPanel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(2);
			this.MinimumSize = new System.Drawing.Size(1500, 500);
			this.Name = "ShowXmlDialog";
			this.Padding = new System.Windows.Forms.Padding(8);
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
		private System.Windows.Forms.RadioButton sectionButton;
		private System.Windows.Forms.RadioButton notebooksButton;
		private System.Windows.Forms.RadioButton notebookButton;
		private System.Windows.Forms.RadioButton currSectionButton;
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
		private System.Windows.Forms.Label manualLabel;
		private System.Windows.Forms.TextBox manualIdBox;
		private System.Windows.Forms.ComboBox manualFxBox;
		private System.Windows.Forms.Button manulRunButton;
	}
}