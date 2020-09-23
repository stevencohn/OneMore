namespace River.OneMoreAddIn
{
	partial class XmlDialog
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose (bool disposing)
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
		private void InitializeComponent ()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XmlDialog));
			this.tabs = new System.Windows.Forms.TabControl();
			this.pageTab = new System.Windows.Forms.TabPage();
			this.pageBox = new System.Windows.Forms.RichTextBox();
			this.hierTab = new System.Windows.Forms.TabPage();
			this.panel2 = new System.Windows.Forms.Panel();
			this.hierBox = new System.Windows.Forms.RichTextBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.currNotebookButton = new System.Windows.Forms.RadioButton();
			this.currSectionButton = new System.Windows.Forms.RadioButton();
			this.pagesHierButton = new System.Windows.Forms.RadioButton();
			this.notebooksHierButton = new System.Windows.Forms.RadioButton();
			this.sectionsHierButton = new System.Windows.Forms.RadioButton();
			this.closeButton = new System.Windows.Forms.Button();
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.updateButton = new System.Windows.Forms.Button();
			this.pageInfoPanel = new System.Windows.Forms.Panel();
			this.pageInfoLabel = new System.Windows.Forms.Label();
			this.pageInfoBox = new System.Windows.Forms.ComboBox();
			this.selectButton = new System.Windows.Forms.Button();
			this.topPanel = new System.Windows.Forms.Panel();
			this.hideBox = new System.Windows.Forms.CheckBox();
			this.wrapBox = new System.Windows.Forms.CheckBox();
			this.findBox = new System.Windows.Forms.TextBox();
			this.findButton = new System.Windows.Forms.Button();
			this.introLabel = new System.Windows.Forms.Label();
			this.masterPanel = new System.Windows.Forms.Panel();
			this.tabs.SuspendLayout();
			this.pageTab.SuspendLayout();
			this.hierTab.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panel1.SuspendLayout();
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
			this.tabs.Location = new System.Drawing.Point(0, 72);
			this.tabs.Margin = new System.Windows.Forms.Padding(1);
			this.tabs.Name = "tabs";
			this.tabs.SelectedIndex = 0;
			this.tabs.Size = new System.Drawing.Size(779, 491);
			this.tabs.TabIndex = 1;
			this.tabs.SelectedIndexChanged += new System.EventHandler(this.ChangeSelectedTab);
			// 
			// pageTab
			// 
			this.pageTab.Controls.Add(this.pageBox);
			this.pageTab.Location = new System.Drawing.Point(4, 22);
			this.pageTab.Margin = new System.Windows.Forms.Padding(1);
			this.pageTab.Name = "pageTab";
			this.pageTab.Padding = new System.Windows.Forms.Padding(1);
			this.pageTab.Size = new System.Drawing.Size(771, 465);
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
			this.pageBox.Size = new System.Drawing.Size(769, 463);
			this.pageBox.TabIndex = 0;
			this.pageBox.Text = "";
			this.pageBox.WordWrap = false;
			this.pageBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.PageBoxKeyUp);
			// 
			// hierTab
			// 
			this.hierTab.Controls.Add(this.panel2);
			this.hierTab.Controls.Add(this.panel1);
			this.hierTab.Location = new System.Drawing.Point(4, 22);
			this.hierTab.Margin = new System.Windows.Forms.Padding(1);
			this.hierTab.Name = "hierTab";
			this.hierTab.Padding = new System.Windows.Forms.Padding(1);
			this.hierTab.Size = new System.Drawing.Size(771, 465);
			this.hierTab.TabIndex = 1;
			this.hierTab.Text = "Hierarchy";
			this.hierTab.UseVisualStyleBackColor = true;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.hierBox);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(1, 36);
			this.panel2.Margin = new System.Windows.Forms.Padding(2);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(769, 428);
			this.panel2.TabIndex = 6;
			// 
			// hierBox
			// 
			this.hierBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.hierBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.hierBox.Font = new System.Drawing.Font("Lucida Console", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.hierBox.Location = new System.Drawing.Point(0, 0);
			this.hierBox.Margin = new System.Windows.Forms.Padding(1);
			this.hierBox.Name = "hierBox";
			this.hierBox.Size = new System.Drawing.Size(769, 428);
			this.hierBox.TabIndex = 0;
			this.hierBox.Text = "";
			this.hierBox.WordWrap = false;
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.SystemColors.Info;
			this.panel1.Controls.Add(this.currNotebookButton);
			this.panel1.Controls.Add(this.currSectionButton);
			this.panel1.Controls.Add(this.pagesHierButton);
			this.panel1.Controls.Add(this.notebooksHierButton);
			this.panel1.Controls.Add(this.sectionsHierButton);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(1, 1);
			this.panel1.Margin = new System.Windows.Forms.Padding(2);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(769, 35);
			this.panel1.TabIndex = 5;
			// 
			// currNotebookButton
			// 
			this.currNotebookButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.currNotebookButton.AutoSize = true;
			this.currNotebookButton.Location = new System.Drawing.Point(187, 6);
			this.currNotebookButton.Margin = new System.Windows.Forms.Padding(2);
			this.currNotebookButton.Name = "currNotebookButton";
			this.currNotebookButton.Size = new System.Drawing.Size(101, 23);
			this.currNotebookButton.TabIndex = 7;
			this.currNotebookButton.Text = "Current Notebook";
			this.currNotebookButton.UseVisualStyleBackColor = true;
			this.currNotebookButton.CheckedChanged += new System.EventHandler(this.ShowCurrentNotebook);
			// 
			// currSectionButton
			// 
			this.currSectionButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.currSectionButton.AutoSize = true;
			this.currSectionButton.Location = new System.Drawing.Point(290, 6);
			this.currSectionButton.Margin = new System.Windows.Forms.Padding(2);
			this.currSectionButton.Name = "currSectionButton";
			this.currSectionButton.Size = new System.Drawing.Size(90, 23);
			this.currSectionButton.TabIndex = 5;
			this.currSectionButton.Text = "Current Section";
			this.currSectionButton.UseVisualStyleBackColor = true;
			this.currSectionButton.CheckedChanged += new System.EventHandler(this.ShowCurrentSection);
			// 
			// pagesHierButton
			// 
			this.pagesHierButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.pagesHierButton.AutoSize = true;
			this.pagesHierButton.Location = new System.Drawing.Point(138, 6);
			this.pagesHierButton.Margin = new System.Windows.Forms.Padding(2);
			this.pagesHierButton.Name = "pagesHierButton";
			this.pagesHierButton.Size = new System.Drawing.Size(47, 23);
			this.pagesHierButton.TabIndex = 4;
			this.pagesHierButton.Text = "Pages";
			this.pagesHierButton.UseVisualStyleBackColor = true;
			this.pagesHierButton.CheckedChanged += new System.EventHandler(this.ShowPages);
			// 
			// notebooksHierButton
			// 
			this.notebooksHierButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.notebooksHierButton.AutoSize = true;
			this.notebooksHierButton.Checked = true;
			this.notebooksHierButton.Location = new System.Drawing.Point(7, 6);
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
			this.sectionsHierButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.sectionsHierButton.AutoSize = true;
			this.sectionsHierButton.Location = new System.Drawing.Point(78, 6);
			this.sectionsHierButton.Margin = new System.Windows.Forms.Padding(2);
			this.sectionsHierButton.Name = "sectionsHierButton";
			this.sectionsHierButton.Size = new System.Drawing.Size(58, 23);
			this.sectionsHierButton.TabIndex = 3;
			this.sectionsHierButton.Text = "Sections";
			this.sectionsHierButton.UseVisualStyleBackColor = true;
			this.sectionsHierButton.CheckedChanged += new System.EventHandler(this.ShowSections);
			// 
			// closeButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = new System.Drawing.Point(685, 9);
			this.closeButton.Margin = new System.Windows.Forms.Padding(1);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(83, 23);
			this.closeButton.TabIndex = 3;
			this.closeButton.Text = "Cancel";
			this.closeButton.Click += new System.EventHandler(this.Close);
			// 
			// buttonPanel
			// 
			this.buttonPanel.Controls.Add(this.updateButton);
			this.buttonPanel.Controls.Add(this.closeButton);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.Location = new System.Drawing.Point(0, 563);
			this.buttonPanel.Margin = new System.Windows.Forms.Padding(1);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Size = new System.Drawing.Size(779, 41);
			this.buttonPanel.TabIndex = 4;
			// 
			// updateButton
			// 
			this.updateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.updateButton.Location = new System.Drawing.Point(597, 9);
			this.updateButton.Margin = new System.Windows.Forms.Padding(1);
			this.updateButton.Name = "updateButton";
			this.updateButton.Size = new System.Drawing.Size(83, 23);
			this.updateButton.TabIndex = 5;
			this.updateButton.Text = "Update Page";
			this.updateButton.Click += new System.EventHandler(this.Update);
			// 
			// pageInfoPanel
			// 
			this.pageInfoPanel.Controls.Add(this.pageInfoLabel);
			this.pageInfoPanel.Controls.Add(this.pageInfoBox);
			this.pageInfoPanel.Location = new System.Drawing.Point(178, 29);
			this.pageInfoPanel.Margin = new System.Windows.Forms.Padding(2);
			this.pageInfoPanel.Name = "pageInfoPanel";
			this.pageInfoPanel.Size = new System.Drawing.Size(211, 37);
			this.pageInfoPanel.TabIndex = 9;
			// 
			// pageInfoLabel
			// 
			this.pageInfoLabel.AutoSize = true;
			this.pageInfoLabel.Location = new System.Drawing.Point(6, 12);
			this.pageInfoLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.pageInfoLabel.Name = "pageInfoLabel";
			this.pageInfoLabel.Size = new System.Drawing.Size(53, 13);
			this.pageInfoLabel.TabIndex = 8;
			this.pageInfoLabel.Text = "PageInfo:";
			// 
			// pageInfoBox
			// 
			this.pageInfoBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.pageInfoBox.DropDownWidth = 260;
			this.pageInfoBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pageInfoBox.FormattingEnabled = true;
			this.pageInfoBox.Location = new System.Drawing.Point(62, 10);
			this.pageInfoBox.Margin = new System.Windows.Forms.Padding(2);
			this.pageInfoBox.Name = "pageInfoBox";
			this.pageInfoBox.Size = new System.Drawing.Size(121, 23);
			this.pageInfoBox.TabIndex = 7;
			this.pageInfoBox.SelectedIndexChanged += new System.EventHandler(this.ChangeInfoScope);
			// 
			// selectButton
			// 
			this.selectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.selectButton.Location = new System.Drawing.Point(4, 36);
			this.selectButton.Margin = new System.Windows.Forms.Padding(1);
			this.selectButton.Name = "selectButton";
			this.selectButton.Size = new System.Drawing.Size(83, 23);
			this.selectButton.TabIndex = 4;
			this.selectButton.Text = "Select All";
			this.selectButton.Click += new System.EventHandler(this.SelectAll);
			// 
			// topPanel
			// 
			this.topPanel.Controls.Add(this.hideBox);
			this.topPanel.Controls.Add(this.wrapBox);
			this.topPanel.Controls.Add(this.pageInfoPanel);
			this.topPanel.Controls.Add(this.findBox);
			this.topPanel.Controls.Add(this.findButton);
			this.topPanel.Controls.Add(this.selectButton);
			this.topPanel.Controls.Add(this.introLabel);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = new System.Drawing.Point(0, 0);
			this.topPanel.Margin = new System.Windows.Forms.Padding(1);
			this.topPanel.Name = "topPanel";
			this.topPanel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
			this.topPanel.Size = new System.Drawing.Size(779, 72);
			this.topPanel.TabIndex = 5;
			// 
			// hideBox
			// 
			this.hideBox.AutoSize = true;
			this.hideBox.Location = new System.Drawing.Point(496, 43);
			this.hideBox.Name = "hideBox";
			this.hideBox.Size = new System.Drawing.Size(140, 17);
			this.hideBox.TabIndex = 11;
			this.hideBox.Text = "Hide edited-by attributes";
			this.hideBox.UseVisualStyleBackColor = true;
			this.hideBox.CheckedChanged += new System.EventHandler(this.HideAttributes);
			// 
			// wrapBox
			// 
			this.wrapBox.AutoSize = true;
			this.wrapBox.Location = new System.Drawing.Point(111, 41);
			this.wrapBox.Margin = new System.Windows.Forms.Padding(2);
			this.wrapBox.Name = "wrapBox";
			this.wrapBox.Size = new System.Drawing.Size(52, 17);
			this.wrapBox.TabIndex = 10;
			this.wrapBox.Text = "Wrap";
			this.wrapBox.UseVisualStyleBackColor = true;
			this.wrapBox.CheckedChanged += new System.EventHandler(this.ChangeWrap);
			// 
			// findBox
			// 
			this.findBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.findBox.Location = new System.Drawing.Point(496, 9);
			this.findBox.Margin = new System.Windows.Forms.Padding(2);
			this.findBox.MinimumSize = new System.Drawing.Size(181, 36);
			this.findBox.Name = "findBox";
			this.findBox.Size = new System.Drawing.Size(182, 21);
			this.findBox.TabIndex = 5;
			this.findBox.TextChanged += new System.EventHandler(this.ChangeFindText);
			this.findBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FindBoxKeyUP);
			// 
			// findButton
			// 
			this.findButton.Enabled = false;
			this.findButton.Location = new System.Drawing.Point(681, 7);
			this.findButton.Margin = new System.Windows.Forms.Padding(2);
			this.findButton.MinimumSize = new System.Drawing.Size(83, 23);
			this.findButton.Name = "findButton";
			this.findButton.Size = new System.Drawing.Size(83, 23);
			this.findButton.TabIndex = 4;
			this.findButton.Text = "Search";
			this.findButton.UseVisualStyleBackColor = true;
			this.findButton.Click += new System.EventHandler(this.ClickFind);
			// 
			// introLabel
			// 
			this.introLabel.AutoSize = true;
			this.introLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.introLabel.Location = new System.Drawing.Point(0, 5);
			this.introLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.introLabel.Name = "introLabel";
			this.introLabel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
			this.introLabel.Size = new System.Drawing.Size(410, 18);
			this.introLabel.TabIndex = 3;
			this.introLabel.Text = "Shows the XML representation of the currently selected page and notebook hierarch" +
    "y";
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
			this.CancelButton = this.closeButton;
			this.ClientSize = new System.Drawing.Size(789, 614);
			this.Controls.Add(this.masterPanel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(1);
			this.MinimumSize = new System.Drawing.Size(803, 333);
			this.Name = "XmlDialog";
			this.Padding = new System.Windows.Forms.Padding(5);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "OneMore XML";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.XmlDialog_FormClosing);
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.tabs.ResumeLayout(false);
			this.pageTab.ResumeLayout(false);
			this.hierTab.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.buttonPanel.ResumeLayout(false);
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
		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.Panel buttonPanel;
		private System.Windows.Forms.RichTextBox pageBox;
		private System.Windows.Forms.RichTextBox hierBox;
		private System.Windows.Forms.Panel topPanel;
		private System.Windows.Forms.Panel masterPanel;
		private System.Windows.Forms.Label introLabel;
		private System.Windows.Forms.TextBox findBox;
		private System.Windows.Forms.Button findButton;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.RadioButton pagesHierButton;
		private System.Windows.Forms.RadioButton notebooksHierButton;
		private System.Windows.Forms.RadioButton sectionsHierButton;
		private System.Windows.Forms.RadioButton currSectionButton;
		private System.Windows.Forms.RadioButton currNotebookButton;
		private System.Windows.Forms.Button selectButton;
		private System.Windows.Forms.Button updateButton;
		private System.Windows.Forms.ComboBox pageInfoBox;
		private System.Windows.Forms.Label pageInfoLabel;
		private System.Windows.Forms.Panel pageInfoPanel;
		private System.Windows.Forms.CheckBox wrapBox;
		private System.Windows.Forms.CheckBox hideBox;
	}
}