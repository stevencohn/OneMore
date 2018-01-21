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
			this.tabs.Location = new System.Drawing.Point(0, 111);
			this.tabs.Margin = new System.Windows.Forms.Padding(2);
			this.tabs.Name = "tabs";
			this.tabs.SelectedIndex = 0;
			this.tabs.Size = new System.Drawing.Size(1162, 754);
			this.tabs.TabIndex = 1;
			this.tabs.SelectedIndexChanged += new System.EventHandler(this.tabs_SelectedIndexChanged);
			// 
			// pageTab
			// 
			this.pageTab.Controls.Add(this.pageBox);
			this.pageTab.Location = new System.Drawing.Point(4, 29);
			this.pageTab.Margin = new System.Windows.Forms.Padding(2);
			this.pageTab.Name = "pageTab";
			this.pageTab.Padding = new System.Windows.Forms.Padding(2);
			this.pageTab.Size = new System.Drawing.Size(1154, 721);
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
			this.pageBox.Size = new System.Drawing.Size(1150, 717);
			this.pageBox.TabIndex = 0;
			this.pageBox.Text = "";
			this.pageBox.WordWrap = false;
			this.pageBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.pageBox_KeyUp);
			// 
			// hierTab
			// 
			this.hierTab.Controls.Add(this.panel2);
			this.hierTab.Controls.Add(this.panel1);
			this.hierTab.Location = new System.Drawing.Point(4, 29);
			this.hierTab.Margin = new System.Windows.Forms.Padding(2);
			this.hierTab.Name = "hierTab";
			this.hierTab.Padding = new System.Windows.Forms.Padding(2);
			this.hierTab.Size = new System.Drawing.Size(1154, 721);
			this.hierTab.TabIndex = 1;
			this.hierTab.Text = "Hierarchy";
			this.hierTab.UseVisualStyleBackColor = true;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.hierBox);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(2, 56);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(1150, 663);
			this.panel2.TabIndex = 6;
			// 
			// hierBox
			// 
			this.hierBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.hierBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.hierBox.Font = new System.Drawing.Font("Lucida Console", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.hierBox.Location = new System.Drawing.Point(0, 0);
			this.hierBox.Margin = new System.Windows.Forms.Padding(2);
			this.hierBox.Name = "hierBox";
			this.hierBox.Size = new System.Drawing.Size(1150, 663);
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
			this.panel1.Location = new System.Drawing.Point(2, 2);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(1150, 54);
			this.panel1.TabIndex = 5;
			// 
			// currNotebookButton
			// 
			this.currNotebookButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.currNotebookButton.AutoSize = true;
			this.currNotebookButton.Location = new System.Drawing.Point(269, 12);
			this.currNotebookButton.Name = "currNotebookButton";
			this.currNotebookButton.Size = new System.Drawing.Size(145, 30);
			this.currNotebookButton.TabIndex = 7;
			this.currNotebookButton.Text = "Current Notebook";
			this.currNotebookButton.UseVisualStyleBackColor = true;
			this.currNotebookButton.CheckedChanged += new System.EventHandler(this.currNotebookButton_CheckedChanged);
			// 
			// currSectionButton
			// 
			this.currSectionButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.currSectionButton.AutoSize = true;
			this.currSectionButton.Location = new System.Drawing.Point(420, 12);
			this.currSectionButton.Name = "currSectionButton";
			this.currSectionButton.Size = new System.Drawing.Size(130, 30);
			this.currSectionButton.TabIndex = 5;
			this.currSectionButton.Text = "Current Section";
			this.currSectionButton.UseVisualStyleBackColor = true;
			this.currSectionButton.CheckedChanged += new System.EventHandler(this.currSectionButton_CheckedChanged);
			// 
			// pagesHierButton
			// 
			this.pagesHierButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.pagesHierButton.AutoSize = true;
			this.pagesHierButton.Location = new System.Drawing.Point(199, 10);
			this.pagesHierButton.Name = "pagesHierButton";
			this.pagesHierButton.Size = new System.Drawing.Size(64, 30);
			this.pagesHierButton.TabIndex = 4;
			this.pagesHierButton.Text = "Pages";
			this.pagesHierButton.UseVisualStyleBackColor = true;
			this.pagesHierButton.CheckedChanged += new System.EventHandler(this.pagesHierButton_CheckedChanged);
			// 
			// notebooksHierButton
			// 
			this.notebooksHierButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.notebooksHierButton.AutoSize = true;
			this.notebooksHierButton.Checked = true;
			this.notebooksHierButton.Location = new System.Drawing.Point(10, 10);
			this.notebooksHierButton.Name = "notebooksHierButton";
			this.notebooksHierButton.Size = new System.Drawing.Size(96, 30);
			this.notebooksHierButton.TabIndex = 2;
			this.notebooksHierButton.TabStop = true;
			this.notebooksHierButton.Text = "Notebooks";
			this.notebooksHierButton.UseVisualStyleBackColor = true;
			this.notebooksHierButton.CheckedChanged += new System.EventHandler(this.notebooksHierButton_CheckedChanged);
			// 
			// sectionsHierButton
			// 
			this.sectionsHierButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.sectionsHierButton.AutoSize = true;
			this.sectionsHierButton.Location = new System.Drawing.Point(112, 10);
			this.sectionsHierButton.Name = "sectionsHierButton";
			this.sectionsHierButton.Size = new System.Drawing.Size(81, 30);
			this.sectionsHierButton.TabIndex = 3;
			this.sectionsHierButton.Text = "Sections";
			this.sectionsHierButton.UseVisualStyleBackColor = true;
			this.sectionsHierButton.CheckedChanged += new System.EventHandler(this.sectionsHierButton_CheckedChanged);
			// 
			// closeButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = new System.Drawing.Point(1021, 14);
			this.closeButton.Margin = new System.Windows.Forms.Padding(2);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(125, 36);
			this.closeButton.TabIndex = 3;
			this.closeButton.Text = "Cancel";
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// buttonPanel
			// 
			this.buttonPanel.Controls.Add(this.updateButton);
			this.buttonPanel.Controls.Add(this.closeButton);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.Location = new System.Drawing.Point(0, 865);
			this.buttonPanel.Margin = new System.Windows.Forms.Padding(2);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Size = new System.Drawing.Size(1162, 63);
			this.buttonPanel.TabIndex = 4;
			// 
			// updateButton
			// 
			this.updateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.updateButton.Location = new System.Drawing.Point(890, 14);
			this.updateButton.Margin = new System.Windows.Forms.Padding(2);
			this.updateButton.Name = "updateButton";
			this.updateButton.Size = new System.Drawing.Size(125, 36);
			this.updateButton.TabIndex = 5;
			this.updateButton.Text = "Update Page";
			this.updateButton.Click += new System.EventHandler(this.updateButton_Click);
			// 
			// pageInfoPanel
			// 
			this.pageInfoPanel.Controls.Add(this.pageInfoLabel);
			this.pageInfoPanel.Controls.Add(this.pageInfoBox);
			this.pageInfoPanel.Location = new System.Drawing.Point(267, 45);
			this.pageInfoPanel.Name = "pageInfoPanel";
			this.pageInfoPanel.Size = new System.Drawing.Size(316, 57);
			this.pageInfoPanel.TabIndex = 9;
			// 
			// pageInfoLabel
			// 
			this.pageInfoLabel.AutoSize = true;
			this.pageInfoLabel.Location = new System.Drawing.Point(9, 19);
			this.pageInfoLabel.Name = "pageInfoLabel";
			this.pageInfoLabel.Size = new System.Drawing.Size(78, 20);
			this.pageInfoLabel.TabIndex = 8;
			this.pageInfoLabel.Text = "PageInfo:";
			// 
			// pageInfoBox
			// 
			this.pageInfoBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.pageInfoBox.DropDownWidth = 260;
			this.pageInfoBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pageInfoBox.FormattingEnabled = true;
			this.pageInfoBox.Location = new System.Drawing.Point(93, 16);
			this.pageInfoBox.Name = "pageInfoBox";
			this.pageInfoBox.Size = new System.Drawing.Size(180, 30);
			this.pageInfoBox.TabIndex = 7;
			this.pageInfoBox.SelectedIndexChanged += new System.EventHandler(this.pageInfoBox_SelectedIndexChanged);
			// 
			// selectButton
			// 
			this.selectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.selectButton.Location = new System.Drawing.Point(6, 56);
			this.selectButton.Margin = new System.Windows.Forms.Padding(2);
			this.selectButton.Name = "selectButton";
			this.selectButton.Size = new System.Drawing.Size(125, 36);
			this.selectButton.TabIndex = 4;
			this.selectButton.Text = "Select All";
			this.selectButton.Click += new System.EventHandler(this.selectButton_Click);
			// 
			// topPanel
			// 
			this.topPanel.Controls.Add(this.wrapBox);
			this.topPanel.Controls.Add(this.pageInfoPanel);
			this.topPanel.Controls.Add(this.findBox);
			this.topPanel.Controls.Add(this.findButton);
			this.topPanel.Controls.Add(this.selectButton);
			this.topPanel.Controls.Add(this.introLabel);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = new System.Drawing.Point(0, 0);
			this.topPanel.Margin = new System.Windows.Forms.Padding(2);
			this.topPanel.Name = "topPanel";
			this.topPanel.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
			this.topPanel.Size = new System.Drawing.Size(1162, 111);
			this.topPanel.TabIndex = 5;
			// 
			// wrapBox
			// 
			this.wrapBox.AutoSize = true;
			this.wrapBox.Location = new System.Drawing.Point(166, 63);
			this.wrapBox.Name = "wrapBox";
			this.wrapBox.Size = new System.Drawing.Size(73, 24);
			this.wrapBox.TabIndex = 10;
			this.wrapBox.Text = "Wrap";
			this.wrapBox.UseVisualStyleBackColor = true;
			this.wrapBox.CheckedChanged += new System.EventHandler(this.wrapBox_CheckedChanged);
			// 
			// findBox
			// 
			this.findBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.findBox.Location = new System.Drawing.Point(744, 14);
			this.findBox.MinimumSize = new System.Drawing.Size(270, 36);
			this.findBox.Name = "findBox";
			this.findBox.Size = new System.Drawing.Size(271, 28);
			this.findBox.TabIndex = 5;
			this.findBox.TextChanged += new System.EventHandler(this.findBox_TextChanged);
			this.findBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.findBox_KeyUp);
			// 
			// findButton
			// 
			this.findButton.Enabled = false;
			this.findButton.Location = new System.Drawing.Point(1021, 11);
			this.findButton.MinimumSize = new System.Drawing.Size(125, 36);
			this.findButton.Name = "findButton";
			this.findButton.Size = new System.Drawing.Size(125, 36);
			this.findButton.TabIndex = 4;
			this.findButton.Text = "Search";
			this.findButton.UseVisualStyleBackColor = true;
			this.findButton.Click += new System.EventHandler(this.findButton_Click);
			// 
			// introLabel
			// 
			this.introLabel.AutoSize = true;
			this.introLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.introLabel.Location = new System.Drawing.Point(0, 8);
			this.introLabel.Name = "introLabel";
			this.introLabel.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
			this.introLabel.Size = new System.Drawing.Size(611, 28);
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
			this.masterPanel.Location = new System.Drawing.Point(8, 8);
			this.masterPanel.Margin = new System.Windows.Forms.Padding(2);
			this.masterPanel.Name = "masterPanel";
			this.masterPanel.Size = new System.Drawing.Size(1162, 928);
			this.masterPanel.TabIndex = 6;
			// 
			// XmlDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.closeButton;
			this.ClientSize = new System.Drawing.Size(1178, 944);
			this.Controls.Add(this.masterPanel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(2);
			this.MinimumSize = new System.Drawing.Size(1200, 500);
			this.Name = "XmlDialog";
			this.Padding = new System.Windows.Forms.Padding(8);
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
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
	}
}