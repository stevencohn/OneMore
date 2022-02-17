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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShowXmlDialog));
			this.tabs = new System.Windows.Forms.TabControl();
			this.pageTab = new System.Windows.Forms.TabPage();
			this.pageBox = new System.Windows.Forms.RichTextBox();
			this.sectionTab = new System.Windows.Forms.TabPage();
			this.sectionBox = new System.Windows.Forms.RichTextBox();
			this.notebooksTab = new System.Windows.Forms.TabPage();
			this.notebookBox = new System.Windows.Forms.RichTextBox();
			this.nbSectionsTab = new System.Windows.Forms.TabPage();
			this.nbSectionBox = new System.Windows.Forms.RichTextBox();
			this.nbPagesTab = new System.Windows.Forms.TabPage();
			this.nbPagesBox = new System.Windows.Forms.RichTextBox();
			this.manualTab = new System.Windows.Forms.TabPage();
			this.manualBox = new System.Windows.Forms.RichTextBox();
			this.tabIcons = new System.Windows.Forms.ImageList(this.components);
			this.manualPanel = new System.Windows.Forms.Panel();
			this.hidePidBox = new System.Windows.Forms.CheckBox();
			this.queryButton = new System.Windows.Forms.Button();
			this.functionBox = new System.Windows.Forms.ComboBox();
			this.objectIdBox = new System.Windows.Forms.TextBox();
			this.manualLabel = new System.Windows.Forms.Label();
			this.cancelButton = new System.Windows.Forms.Button();
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.okButton = new System.Windows.Forms.Button();
			this.pageLink = new System.Windows.Forms.Label();
			this.pagePath = new System.Windows.Forms.Label();
			this.pageName = new System.Windows.Forms.Label();
			this.pageLinkLabel = new System.Windows.Forms.Label();
			this.pagePathLabel = new System.Windows.Forms.Label();
			this.pageNameLabel = new System.Windows.Forms.Label();
			this.pagePanel = new System.Windows.Forms.Panel();
			this.hideLFBox = new System.Windows.Forms.CheckBox();
			this.hideBox = new System.Windows.Forms.CheckBox();
			this.scopeBox = new System.Windows.Forms.ListBox();
			this.pageInfoLabel = new System.Windows.Forms.Label();
			this.selectButton = new System.Windows.Forms.Button();
			this.topPanel = new System.Windows.Forms.Panel();
			this.wrapBox = new System.Windows.Forms.CheckBox();
			this.findBox = new System.Windows.Forms.TextBox();
			this.findButton = new System.Windows.Forms.Button();
			this.masterPanel = new System.Windows.Forms.Panel();
			this.tabs.SuspendLayout();
			this.pageTab.SuspendLayout();
			this.sectionTab.SuspendLayout();
			this.notebooksTab.SuspendLayout();
			this.nbSectionsTab.SuspendLayout();
			this.nbPagesTab.SuspendLayout();
			this.manualTab.SuspendLayout();
			this.manualPanel.SuspendLayout();
			this.buttonPanel.SuspendLayout();
			this.pagePanel.SuspendLayout();
			this.topPanel.SuspendLayout();
			this.masterPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabs
			// 
			this.tabs.Controls.Add(this.pageTab);
			this.tabs.Controls.Add(this.sectionTab);
			this.tabs.Controls.Add(this.notebooksTab);
			this.tabs.Controls.Add(this.nbSectionsTab);
			this.tabs.Controls.Add(this.nbPagesTab);
			this.tabs.Controls.Add(this.manualTab);
			this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabs.ImageList = this.tabIcons;
			this.tabs.Location = new System.Drawing.Point(0, 107);
			this.tabs.Margin = new System.Windows.Forms.Padding(2);
			this.tabs.Name = "tabs";
			this.tabs.SelectedIndex = 0;
			this.tabs.Size = new System.Drawing.Size(1462, 853);
			this.tabs.TabIndex = 0;
			this.tabs.SelectedIndexChanged += new System.EventHandler(this.TabsSelectedIndexChanged);
			this.tabs.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.TabsSelecting);
			// 
			// pageTab
			// 
			this.pageTab.BackColor = System.Drawing.Color.Transparent;
			this.pageTab.Controls.Add(this.pageBox);
			this.pageTab.ImageIndex = 0;
			this.pageTab.Location = new System.Drawing.Point(4, 29);
			this.pageTab.Margin = new System.Windows.Forms.Padding(2);
			this.pageTab.Name = "pageTab";
			this.pageTab.Padding = new System.Windows.Forms.Padding(2);
			this.pageTab.Size = new System.Drawing.Size(1454, 820);
			this.pageTab.TabIndex = 0;
			this.pageTab.Text = "Page";
			// 
			// pageBox
			// 
			this.pageBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pageBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pageBox.Font = new System.Drawing.Font("Lucida Console", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pageBox.Location = new System.Drawing.Point(2, 2);
			this.pageBox.Margin = new System.Windows.Forms.Padding(2);
			this.pageBox.Name = "pageBox";
			this.pageBox.Size = new System.Drawing.Size(1450, 816);
			this.pageBox.TabIndex = 7;
			this.pageBox.Text = "";
			this.pageBox.WordWrap = false;
			this.pageBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.XmlBoxKeyUp);
			// 
			// sectionTab
			// 
			this.sectionTab.Controls.Add(this.sectionBox);
			this.sectionTab.ImageIndex = 1;
			this.sectionTab.Location = new System.Drawing.Point(4, 29);
			this.sectionTab.Margin = new System.Windows.Forms.Padding(2);
			this.sectionTab.Name = "sectionTab";
			this.sectionTab.Padding = new System.Windows.Forms.Padding(2);
			this.sectionTab.Size = new System.Drawing.Size(1454, 820);
			this.sectionTab.TabIndex = 2;
			this.sectionTab.Text = "Section";
			this.sectionTab.UseVisualStyleBackColor = true;
			// 
			// sectionBox
			// 
			this.sectionBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.sectionBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sectionBox.Font = new System.Drawing.Font("Lucida Console", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.sectionBox.Location = new System.Drawing.Point(2, 2);
			this.sectionBox.Margin = new System.Windows.Forms.Padding(2);
			this.sectionBox.Name = "sectionBox";
			this.sectionBox.Size = new System.Drawing.Size(1450, 816);
			this.sectionBox.TabIndex = 1;
			this.sectionBox.Text = "";
			this.sectionBox.WordWrap = false;
			// 
			// notebooksTab
			// 
			this.notebooksTab.Controls.Add(this.notebookBox);
			this.notebooksTab.ImageIndex = 2;
			this.notebooksTab.Location = new System.Drawing.Point(4, 29);
			this.notebooksTab.Margin = new System.Windows.Forms.Padding(2);
			this.notebooksTab.Name = "notebooksTab";
			this.notebooksTab.Padding = new System.Windows.Forms.Padding(2);
			this.notebooksTab.Size = new System.Drawing.Size(1454, 820);
			this.notebooksTab.TabIndex = 3;
			this.notebooksTab.Text = "Notebooks";
			this.notebooksTab.UseVisualStyleBackColor = true;
			// 
			// notebookBox
			// 
			this.notebookBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.notebookBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.notebookBox.Font = new System.Drawing.Font("Lucida Console", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.notebookBox.Location = new System.Drawing.Point(2, 2);
			this.notebookBox.Margin = new System.Windows.Forms.Padding(2);
			this.notebookBox.Name = "notebookBox";
			this.notebookBox.Size = new System.Drawing.Size(1450, 816);
			this.notebookBox.TabIndex = 1;
			this.notebookBox.Text = "";
			this.notebookBox.WordWrap = false;
			// 
			// nbSectionsTab
			// 
			this.nbSectionsTab.Controls.Add(this.nbSectionBox);
			this.nbSectionsTab.ImageIndex = 3;
			this.nbSectionsTab.Location = new System.Drawing.Point(4, 29);
			this.nbSectionsTab.Margin = new System.Windows.Forms.Padding(2);
			this.nbSectionsTab.Name = "nbSectionsTab";
			this.nbSectionsTab.Padding = new System.Windows.Forms.Padding(2);
			this.nbSectionsTab.Size = new System.Drawing.Size(1454, 820);
			this.nbSectionsTab.TabIndex = 4;
			this.nbSectionsTab.Text = "Notebook with Sections";
			this.nbSectionsTab.UseVisualStyleBackColor = true;
			// 
			// nbSectionBox
			// 
			this.nbSectionBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.nbSectionBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.nbSectionBox.Font = new System.Drawing.Font("Lucida Console", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.nbSectionBox.Location = new System.Drawing.Point(2, 2);
			this.nbSectionBox.Margin = new System.Windows.Forms.Padding(2);
			this.nbSectionBox.Name = "nbSectionBox";
			this.nbSectionBox.Size = new System.Drawing.Size(1450, 816);
			this.nbSectionBox.TabIndex = 1;
			this.nbSectionBox.Text = "";
			this.nbSectionBox.WordWrap = false;
			// 
			// nbPagesTab
			// 
			this.nbPagesTab.Controls.Add(this.nbPagesBox);
			this.nbPagesTab.ImageIndex = 4;
			this.nbPagesTab.Location = new System.Drawing.Point(4, 29);
			this.nbPagesTab.Margin = new System.Windows.Forms.Padding(2);
			this.nbPagesTab.Name = "nbPagesTab";
			this.nbPagesTab.Padding = new System.Windows.Forms.Padding(2);
			this.nbPagesTab.Size = new System.Drawing.Size(1454, 820);
			this.nbPagesTab.TabIndex = 5;
			this.nbPagesTab.Text = "Notebook with Pages";
			this.nbPagesTab.UseVisualStyleBackColor = true;
			// 
			// nbPagesBox
			// 
			this.nbPagesBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.nbPagesBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.nbPagesBox.Font = new System.Drawing.Font("Lucida Console", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.nbPagesBox.Location = new System.Drawing.Point(2, 2);
			this.nbPagesBox.Margin = new System.Windows.Forms.Padding(2);
			this.nbPagesBox.Name = "nbPagesBox";
			this.nbPagesBox.Size = new System.Drawing.Size(1450, 816);
			this.nbPagesBox.TabIndex = 1;
			this.nbPagesBox.Text = "";
			this.nbPagesBox.WordWrap = false;
			// 
			// manualTab
			// 
			this.manualTab.Controls.Add(this.manualBox);
			this.manualTab.ImageIndex = 5;
			this.manualTab.Location = new System.Drawing.Point(4, 29);
			this.manualTab.Margin = new System.Windows.Forms.Padding(2);
			this.manualTab.Name = "manualTab";
			this.manualTab.Padding = new System.Windows.Forms.Padding(2);
			this.manualTab.Size = new System.Drawing.Size(1454, 820);
			this.manualTab.TabIndex = 1;
			this.manualTab.Text = "Manual lookup";
			this.manualTab.UseVisualStyleBackColor = true;
			// 
			// manualBox
			// 
			this.manualBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.manualBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.manualBox.Font = new System.Drawing.Font("Lucida Console", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.manualBox.Location = new System.Drawing.Point(2, 2);
			this.manualBox.Margin = new System.Windows.Forms.Padding(2);
			this.manualBox.Name = "manualBox";
			this.manualBox.Size = new System.Drawing.Size(1450, 816);
			this.manualBox.TabIndex = 0;
			this.manualBox.Text = "";
			this.manualBox.WordWrap = false;
			this.manualBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.XmlBoxKeyUp);
			// 
			// tabIcons
			// 
			this.tabIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("tabIcons.ImageStream")));
			this.tabIcons.TransparentColor = System.Drawing.Color.Transparent;
			this.tabIcons.Images.SetKeyName(0, "Page.png");
			this.tabIcons.Images.SetKeyName(1, "Section.png");
			this.tabIcons.Images.SetKeyName(2, "Notebook.png");
			this.tabIcons.Images.SetKeyName(3, "SectionGroup.png");
			this.tabIcons.Images.SetKeyName(4, "Pages.png");
			this.tabIcons.Images.SetKeyName(5, "Search.png");
			// 
			// manualPanel
			// 
			this.manualPanel.Controls.Add(this.hidePidBox);
			this.manualPanel.Controls.Add(this.queryButton);
			this.manualPanel.Controls.Add(this.functionBox);
			this.manualPanel.Controls.Add(this.objectIdBox);
			this.manualPanel.Controls.Add(this.manualLabel);
			this.manualPanel.Location = new System.Drawing.Point(1208, 0);
			this.manualPanel.Name = "manualPanel";
			this.manualPanel.Size = new System.Drawing.Size(928, 90);
			this.manualPanel.TabIndex = 2;
			this.manualPanel.Visible = false;
			// 
			// hidePidBox
			// 
			this.hidePidBox.AutoSize = true;
			this.hidePidBox.Checked = true;
			this.hidePidBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.hidePidBox.Location = new System.Drawing.Point(13, 61);
			this.hidePidBox.Name = "hidePidBox";
			this.hidePidBox.Size = new System.Drawing.Size(99, 24);
			this.hidePidBox.TabIndex = 10;
			this.hidePidBox.Text = "Hide PID";
			this.hidePidBox.UseVisualStyleBackColor = true;
			this.hidePidBox.CheckedChanged += new System.EventHandler(this.HidePidCheckedChanged);
			// 
			// queryButton
			// 
			this.queryButton.Enabled = false;
			this.queryButton.Image = ((System.Drawing.Image)(resources.GetObject("queryButton.Image")));
			this.queryButton.Location = new System.Drawing.Point(862, 11);
			this.queryButton.Name = "queryButton";
			this.queryButton.Size = new System.Drawing.Size(47, 32);
			this.queryButton.TabIndex = 9;
			this.queryButton.UseVisualStyleBackColor = true;
			this.queryButton.Click += new System.EventHandler(this.RunManual);
			// 
			// functionBox
			// 
			this.functionBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.functionBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.functionBox.FormattingEnabled = true;
			this.functionBox.Items.AddRange(new object[] {
            "GetNotebook",
            "GetSection",
            "GetPage"});
			this.functionBox.Location = new System.Drawing.Point(655, 13);
			this.functionBox.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
			this.functionBox.Name = "functionBox";
			this.functionBox.Size = new System.Drawing.Size(201, 30);
			this.functionBox.TabIndex = 8;
			this.functionBox.SelectedIndexChanged += new System.EventHandler(this.ManualInputChanged);
			// 
			// objectIdBox
			// 
			this.objectIdBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.objectIdBox.Location = new System.Drawing.Point(140, 13);
			this.objectIdBox.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.objectIdBox.Name = "objectIdBox";
			this.objectIdBox.Size = new System.Drawing.Size(509, 29);
			this.objectIdBox.TabIndex = 7;
			this.objectIdBox.TextChanged += new System.EventHandler(this.ManualInputChanged);
			// 
			// manualLabel
			// 
			this.manualLabel.AutoSize = true;
			this.manualLabel.Location = new System.Drawing.Point(8, 15);
			this.manualLabel.Margin = new System.Windows.Forms.Padding(100, 8, 3, 0);
			this.manualLabel.Name = "manualLabel";
			this.manualLabel.Size = new System.Drawing.Size(128, 20);
			this.manualLabel.TabIndex = 6;
			this.manualLabel.Text = "Manual ObjectID";
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
			// pagePanel
			// 
			this.pagePanel.Controls.Add(this.hideLFBox);
			this.pagePanel.Controls.Add(this.hideBox);
			this.pagePanel.Controls.Add(this.scopeBox);
			this.pagePanel.Controls.Add(this.pageInfoLabel);
			this.pagePanel.Location = new System.Drawing.Point(408, 8);
			this.pagePanel.Name = "pagePanel";
			this.pagePanel.Size = new System.Drawing.Size(802, 90);
			this.pagePanel.TabIndex = 9;
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
			this.hideLFBox.CheckedChanged += new System.EventHandler(this.HideCheckedChanged);
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
			this.hideBox.CheckedChanged += new System.EventHandler(this.HideCheckedChanged);
			// 
			// scopeBox
			// 
			this.scopeBox.ItemHeight = 20;
			this.scopeBox.Location = new System.Drawing.Point(94, 11);
			this.scopeBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.scopeBox.Name = "scopeBox";
			this.scopeBox.Size = new System.Drawing.Size(223, 64);
			this.scopeBox.TabIndex = 4;
			this.scopeBox.SelectedValueChanged += new System.EventHandler(this.ScopeSelectedValueChanged);
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
			this.selectButton.Location = new System.Drawing.Point(163, 50);
			this.selectButton.Margin = new System.Windows.Forms.Padding(2);
			this.selectButton.Name = "selectButton";
			this.selectButton.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
			this.selectButton.Size = new System.Drawing.Size(117, 35);
			this.selectButton.TabIndex = 3;
			this.selectButton.Text = "Select All";
			this.selectButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.selectButton.Click += new System.EventHandler(this.SelectAllClicked);
			// 
			// topPanel
			// 
			this.topPanel.Controls.Add(this.manualPanel);
			this.topPanel.Controls.Add(this.wrapBox);
			this.topPanel.Controls.Add(this.findBox);
			this.topPanel.Controls.Add(this.findButton);
			this.topPanel.Controls.Add(this.selectButton);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = new System.Drawing.Point(0, 0);
			this.topPanel.Margin = new System.Windows.Forms.Padding(2);
			this.topPanel.Name = "topPanel";
			this.topPanel.Padding = new System.Windows.Forms.Padding(8, 8, 0, 0);
			this.topPanel.Size = new System.Drawing.Size(1462, 107);
			this.topPanel.TabIndex = 5;
			// 
			// wrapBox
			// 
			this.wrapBox.AutoSize = true;
			this.wrapBox.Location = new System.Drawing.Point(27, 56);
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
			this.findBox.TextChanged += new System.EventHandler(this.FindTextChanged);
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
			this.findButton.Click += new System.EventHandler(this.FindClicked);
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
			this.Controls.Add(this.pagePanel);
			this.Controls.Add(this.masterPanel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(2);
			this.MinimumSize = new System.Drawing.Size(1500, 500);
			this.Name = "ShowXmlDialog";
			this.Padding = new System.Windows.Forms.Padding(8);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "OneMore XML";
			this.tabs.ResumeLayout(false);
			this.pageTab.ResumeLayout(false);
			this.sectionTab.ResumeLayout(false);
			this.notebooksTab.ResumeLayout(false);
			this.nbSectionsTab.ResumeLayout(false);
			this.nbPagesTab.ResumeLayout(false);
			this.manualTab.ResumeLayout(false);
			this.manualPanel.ResumeLayout(false);
			this.manualPanel.PerformLayout();
			this.buttonPanel.ResumeLayout(false);
			this.buttonPanel.PerformLayout();
			this.pagePanel.ResumeLayout(false);
			this.pagePanel.PerformLayout();
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.masterPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl tabs;
		private System.Windows.Forms.TabPage pageTab;
		private System.Windows.Forms.TabPage manualTab;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Panel buttonPanel;
		private System.Windows.Forms.RichTextBox pageBox;
		private System.Windows.Forms.RichTextBox manualBox;
		private System.Windows.Forms.Panel topPanel;
		private System.Windows.Forms.Panel masterPanel;
		private System.Windows.Forms.TextBox findBox;
		private System.Windows.Forms.Button findButton;
		private System.Windows.Forms.Button selectButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Label pageInfoLabel;
		private System.Windows.Forms.Panel pagePanel;
		private System.Windows.Forms.CheckBox wrapBox;
		private System.Windows.Forms.CheckBox hideBox;
		private System.Windows.Forms.CheckBox hideLFBox;
		private System.Windows.Forms.Label pageNameLabel;
		private System.Windows.Forms.Label pageLink;
		private System.Windows.Forms.Label pagePath;
		private System.Windows.Forms.Label pageName;
		private System.Windows.Forms.Label pageLinkLabel;
		private System.Windows.Forms.Label pagePathLabel;
		private System.Windows.Forms.ListBox scopeBox;
		private System.Windows.Forms.Label manualLabel;
		private System.Windows.Forms.TextBox objectIdBox;
		private System.Windows.Forms.ComboBox functionBox;
		private System.Windows.Forms.Button queryButton;
		private System.Windows.Forms.TabPage sectionTab;
		private System.Windows.Forms.TabPage notebooksTab;
		private System.Windows.Forms.TabPage nbSectionsTab;
		private System.Windows.Forms.TabPage nbPagesTab;
		private System.Windows.Forms.RichTextBox sectionBox;
		private System.Windows.Forms.RichTextBox notebookBox;
		private System.Windows.Forms.RichTextBox nbSectionBox;
		private System.Windows.Forms.RichTextBox nbPagesBox;
		private System.Windows.Forms.Panel manualPanel;
		private System.Windows.Forms.CheckBox hidePidBox;
		private System.Windows.Forms.ImageList tabIcons;
	}
}