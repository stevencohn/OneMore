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
			this.tabs = new River.OneMoreAddIn.UI.MoreTabControl();
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
			this.hideEditedByBox2 = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.pidBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.multilineBox2 = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.fnLabel = new System.Windows.Forms.Label();
			this.queryButton = new River.OneMoreAddIn.UI.MoreButton();
			this.functionBox = new System.Windows.Forms.ComboBox();
			this.objectIdBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.manualLabel = new System.Windows.Forms.Label();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.editByBox2 = new System.Windows.Forms.Panel();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.pageLink = new System.Windows.Forms.Label();
			this.pagePath = new System.Windows.Forms.Label();
			this.pageName = new System.Windows.Forms.Label();
			this.pageLinkLabel = new System.Windows.Forms.Label();
			this.pagePathLabel = new System.Windows.Forms.Label();
			this.pageNameLabel = new System.Windows.Forms.Label();
			this.pageOptionsPanel = new System.Windows.Forms.Panel();
			this.editModeBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.saveWindowBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.multilineBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.scopeBox = new System.Windows.Forms.ListBox();
			this.hideEditedByBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.linefeedBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.pageInfoLabel = new System.Windows.Forms.Label();
			this.selectButton = new River.OneMoreAddIn.UI.MoreButton();
			this.topPanel = new System.Windows.Forms.Panel();
			this.wrapBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.findBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.findButton = new River.OneMoreAddIn.UI.MoreButton();
			this.masterPanel = new System.Windows.Forms.Panel();
			this.tooltip = new System.Windows.Forms.ToolTip(this.components);
			this.tabs.SuspendLayout();
			this.pageTab.SuspendLayout();
			this.sectionTab.SuspendLayout();
			this.notebooksTab.SuspendLayout();
			this.nbSectionsTab.SuspendLayout();
			this.nbPagesTab.SuspendLayout();
			this.manualTab.SuspendLayout();
			this.manualPanel.SuspendLayout();
			this.editByBox2.SuspendLayout();
			this.pageOptionsPanel.SuspendLayout();
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
			this.tabs.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tabs.ImageList = this.tabIcons;
			this.tabs.InactiveTabBack = "ControlDarkDark";
			this.tabs.InactiveTabFore = "DarkText";
			this.tabs.Location = new System.Drawing.Point(0, 152);
			this.tabs.Margin = new System.Windows.Forms.Padding(1);
			this.tabs.Name = "tabs";
			this.tabs.Padding = new System.Drawing.Point(0, 0);
			this.tabs.SelectedIndex = 0;
			this.tabs.Size = new System.Drawing.Size(2440, 708);
			this.tabs.TabIndex = 0;
			this.tabs.SelectedIndexChanged += new System.EventHandler(this.RefreshHierarchy);
			this.tabs.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.TabGuard);
			this.tabs.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FocusFind);
			// 
			// pageTab
			// 
			this.pageTab.BackColor = System.Drawing.Color.Transparent;
			this.pageTab.Controls.Add(this.pageBox);
			this.pageTab.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pageTab.ImageIndex = 0;
			this.pageTab.Location = new System.Drawing.Point(4, 29);
			this.pageTab.Margin = new System.Windows.Forms.Padding(2);
			this.pageTab.Name = "pageTab";
			this.pageTab.Padding = new System.Windows.Forms.Padding(2, 2, 8, 2);
			this.pageTab.Size = new System.Drawing.Size(2432, 675);
			this.pageTab.TabIndex = 0;
			this.pageTab.Text = "Page";
			// 
			// pageBox
			// 
			this.pageBox.AcceptsTab = true;
			this.pageBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.pageBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pageBox.Font = new System.Drawing.Font("Lucida Console", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pageBox.Location = new System.Drawing.Point(2, 2);
			this.pageBox.Margin = new System.Windows.Forms.Padding(2);
			this.pageBox.Name = "pageBox";
			this.pageBox.ReadOnly = true;
			this.pageBox.Size = new System.Drawing.Size(2422, 671);
			this.pageBox.TabIndex = 7;
			this.pageBox.Text = "";
			this.pageBox.WordWrap = false;
			this.pageBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.XmlBoxKeyHandlerOnKeyDown);
			this.pageBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.XmlBoxKeyHandlerOnKeyUp);
			// 
			// sectionTab
			// 
			this.sectionTab.Controls.Add(this.sectionBox);
			this.sectionTab.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
			this.sectionTab.ImageIndex = 1;
			this.sectionTab.Location = new System.Drawing.Point(4, 29);
			this.sectionTab.Margin = new System.Windows.Forms.Padding(2);
			this.sectionTab.Name = "sectionTab";
			this.sectionTab.Padding = new System.Windows.Forms.Padding(2, 2, 8, 2);
			this.sectionTab.Size = new System.Drawing.Size(2432, 675);
			this.sectionTab.TabIndex = 2;
			this.sectionTab.Text = "Section";
			this.sectionTab.UseVisualStyleBackColor = true;
			// 
			// sectionBox
			// 
			this.sectionBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.sectionBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sectionBox.Font = new System.Drawing.Font("Lucida Console", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.sectionBox.Location = new System.Drawing.Point(2, 2);
			this.sectionBox.Margin = new System.Windows.Forms.Padding(2);
			this.sectionBox.Name = "sectionBox";
			this.sectionBox.ReadOnly = true;
			this.sectionBox.Size = new System.Drawing.Size(2422, 671);
			this.sectionBox.TabIndex = 1;
			this.sectionBox.Text = "";
			this.sectionBox.WordWrap = false;
			// 
			// notebooksTab
			// 
			this.notebooksTab.Controls.Add(this.notebookBox);
			this.notebooksTab.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
			this.notebooksTab.ImageIndex = 2;
			this.notebooksTab.Location = new System.Drawing.Point(4, 29);
			this.notebooksTab.Margin = new System.Windows.Forms.Padding(2);
			this.notebooksTab.Name = "notebooksTab";
			this.notebooksTab.Padding = new System.Windows.Forms.Padding(2, 2, 8, 2);
			this.notebooksTab.Size = new System.Drawing.Size(2432, 675);
			this.notebooksTab.TabIndex = 3;
			this.notebooksTab.Text = "Notebooks";
			this.notebooksTab.UseVisualStyleBackColor = true;
			// 
			// notebookBox
			// 
			this.notebookBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.notebookBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.notebookBox.Font = new System.Drawing.Font("Lucida Console", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.notebookBox.Location = new System.Drawing.Point(2, 2);
			this.notebookBox.Margin = new System.Windows.Forms.Padding(2);
			this.notebookBox.Name = "notebookBox";
			this.notebookBox.ReadOnly = true;
			this.notebookBox.Size = new System.Drawing.Size(2422, 671);
			this.notebookBox.TabIndex = 1;
			this.notebookBox.Text = "";
			this.notebookBox.WordWrap = false;
			// 
			// nbSectionsTab
			// 
			this.nbSectionsTab.Controls.Add(this.nbSectionBox);
			this.nbSectionsTab.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
			this.nbSectionsTab.ImageIndex = 3;
			this.nbSectionsTab.Location = new System.Drawing.Point(4, 29);
			this.nbSectionsTab.Margin = new System.Windows.Forms.Padding(2);
			this.nbSectionsTab.Name = "nbSectionsTab";
			this.nbSectionsTab.Padding = new System.Windows.Forms.Padding(2, 2, 8, 2);
			this.nbSectionsTab.Size = new System.Drawing.Size(2432, 675);
			this.nbSectionsTab.TabIndex = 4;
			this.nbSectionsTab.Text = "Notebook with Sections";
			this.nbSectionsTab.UseVisualStyleBackColor = true;
			// 
			// nbSectionBox
			// 
			this.nbSectionBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.nbSectionBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.nbSectionBox.Font = new System.Drawing.Font("Lucida Console", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.nbSectionBox.Location = new System.Drawing.Point(2, 2);
			this.nbSectionBox.Margin = new System.Windows.Forms.Padding(2);
			this.nbSectionBox.Name = "nbSectionBox";
			this.nbSectionBox.ReadOnly = true;
			this.nbSectionBox.Size = new System.Drawing.Size(2422, 671);
			this.nbSectionBox.TabIndex = 1;
			this.nbSectionBox.Text = "";
			this.nbSectionBox.WordWrap = false;
			// 
			// nbPagesTab
			// 
			this.nbPagesTab.Controls.Add(this.nbPagesBox);
			this.nbPagesTab.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
			this.nbPagesTab.ImageIndex = 4;
			this.nbPagesTab.Location = new System.Drawing.Point(4, 29);
			this.nbPagesTab.Margin = new System.Windows.Forms.Padding(2);
			this.nbPagesTab.Name = "nbPagesTab";
			this.nbPagesTab.Padding = new System.Windows.Forms.Padding(2, 2, 8, 2);
			this.nbPagesTab.Size = new System.Drawing.Size(2432, 675);
			this.nbPagesTab.TabIndex = 5;
			this.nbPagesTab.Text = "Notebook with Pages";
			this.nbPagesTab.UseVisualStyleBackColor = true;
			// 
			// nbPagesBox
			// 
			this.nbPagesBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.nbPagesBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.nbPagesBox.Font = new System.Drawing.Font("Lucida Console", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.nbPagesBox.Location = new System.Drawing.Point(2, 2);
			this.nbPagesBox.Margin = new System.Windows.Forms.Padding(2);
			this.nbPagesBox.Name = "nbPagesBox";
			this.nbPagesBox.ReadOnly = true;
			this.nbPagesBox.Size = new System.Drawing.Size(2422, 671);
			this.nbPagesBox.TabIndex = 1;
			this.nbPagesBox.Text = "";
			this.nbPagesBox.WordWrap = false;
			// 
			// manualTab
			// 
			this.manualTab.Controls.Add(this.manualBox);
			this.manualTab.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
			this.manualTab.ImageIndex = 5;
			this.manualTab.Location = new System.Drawing.Point(4, 29);
			this.manualTab.Margin = new System.Windows.Forms.Padding(2);
			this.manualTab.Name = "manualTab";
			this.manualTab.Padding = new System.Windows.Forms.Padding(2);
			this.manualTab.Size = new System.Drawing.Size(2432, 675);
			this.manualTab.TabIndex = 1;
			this.manualTab.Text = "Manual lookup";
			this.manualTab.UseVisualStyleBackColor = true;
			// 
			// manualBox
			// 
			this.manualBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.manualBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.manualBox.Font = new System.Drawing.Font("Lucida Console", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.manualBox.Location = new System.Drawing.Point(2, 2);
			this.manualBox.Margin = new System.Windows.Forms.Padding(2);
			this.manualBox.Name = "manualBox";
			this.manualBox.ReadOnly = true;
			this.manualBox.Size = new System.Drawing.Size(2428, 671);
			this.manualBox.TabIndex = 0;
			this.manualBox.Text = "";
			this.manualBox.WordWrap = false;
			this.manualBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.XmlBoxKeyHandlerOnKeyUp);
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
			this.manualPanel.Controls.Add(this.hideEditedByBox2);
			this.manualPanel.Controls.Add(this.pidBox);
			this.manualPanel.Controls.Add(this.multilineBox2);
			this.manualPanel.Controls.Add(this.fnLabel);
			this.manualPanel.Controls.Add(this.queryButton);
			this.manualPanel.Controls.Add(this.functionBox);
			this.manualPanel.Controls.Add(this.objectIdBox);
			this.manualPanel.Controls.Add(this.manualLabel);
			this.manualPanel.Location = new System.Drawing.Point(1397, 4);
			this.manualPanel.Name = "manualPanel";
			this.manualPanel.Size = new System.Drawing.Size(977, 116);
			this.manualPanel.TabIndex = 2;
			this.manualPanel.Visible = false;
			// 
			// hideEditedByBox2
			// 
			this.hideEditedByBox2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.hideEditedByBox2.Checked = true;
			this.hideEditedByBox2.CheckState = System.Windows.Forms.CheckState.Checked;
			this.hideEditedByBox2.Cursor = System.Windows.Forms.Cursors.Hand;
			this.hideEditedByBox2.ForeColor = System.Drawing.SystemColors.ControlText;
			this.hideEditedByBox2.Location = new System.Drawing.Point(3, 18);
			this.hideEditedByBox2.Name = "hideEditedByBox2";
			this.hideEditedByBox2.Size = new System.Drawing.Size(214, 25);
			this.hideEditedByBox2.StylizeImage = false;
			this.hideEditedByBox2.TabIndex = 10;
			this.hideEditedByBox2.Text = "Hide edited-by attributes";
			this.hideEditedByBox2.ThemedBack = null;
			this.hideEditedByBox2.ThemedFore = null;
			this.tooltip.SetToolTip(this.hideEditedByBox2, "Enable to hide ID and timestamp attributes");
			this.hideEditedByBox2.UseVisualStyleBackColor = true;
			this.hideEditedByBox2.Click += new System.EventHandler(this.RefreshOnClick);
			// 
			// pidBox
			// 
			this.pidBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.pidBox.Checked = true;
			this.pidBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.pidBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pidBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.pidBox.Location = new System.Drawing.Point(3, 78);
			this.pidBox.Name = "pidBox";
			this.pidBox.Size = new System.Drawing.Size(101, 25);
			this.pidBox.StylizeImage = false;
			this.pidBox.TabIndex = 10;
			this.pidBox.Text = "Hide PID";
			this.pidBox.ThemedBack = null;
			this.pidBox.ThemedFore = null;
			this.tooltip.SetToolTip(this.pidBox, "Enable to hide personal identifying information");
			this.pidBox.UseVisualStyleBackColor = true;
			this.pidBox.CheckedChanged += new System.EventHandler(this.TogglePid);
			// 
			// multilineBox2
			// 
			this.multilineBox2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.multilineBox2.Cursor = System.Windows.Forms.Cursors.Hand;
			this.multilineBox2.ForeColor = System.Drawing.SystemColors.ControlText;
			this.multilineBox2.Location = new System.Drawing.Point(3, 48);
			this.multilineBox2.Name = "multilineBox2";
			this.multilineBox2.Size = new System.Drawing.Size(201, 25);
			this.multilineBox2.StylizeImage = false;
			this.multilineBox2.TabIndex = 8;
			this.multilineBox2.Text = "Attributes on new lines";
			this.multilineBox2.ThemedBack = null;
			this.multilineBox2.ThemedFore = null;
			this.tooltip.SetToolTip(this.multilineBox2, "Enable to show each attribute on its own line");
			this.multilineBox2.UseVisualStyleBackColor = true;
			this.multilineBox2.Click += new System.EventHandler(this.RefreshOnClick);
			// 
			// fnLabel
			// 
			this.fnLabel.AutoSize = true;
			this.fnLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.fnLabel.Location = new System.Drawing.Point(284, 56);
			this.fnLabel.Margin = new System.Windows.Forms.Padding(100, 8, 3, 0);
			this.fnLabel.Name = "fnLabel";
			this.fnLabel.Size = new System.Drawing.Size(71, 20);
			this.fnLabel.TabIndex = 10;
			this.fnLabel.Text = "Function";
			// 
			// queryButton
			// 
			this.queryButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.queryButton.Enabled = false;
			this.queryButton.Font = new System.Drawing.Font("Segoe UI Symbol", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.queryButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.queryButton.ImageOver = null;
			this.queryButton.Location = new System.Drawing.Point(623, 50);
			this.queryButton.Name = "queryButton";
			this.queryButton.ShowBorder = true;
			this.queryButton.Size = new System.Drawing.Size(47, 32);
			this.queryButton.StylizeImage = false;
			this.queryButton.TabIndex = 9;
			this.queryButton.Text = "▶";
			this.queryButton.ThemedBack = null;
			this.queryButton.ThemedFore = null;
			this.queryButton.UseVisualStyleBackColor = false;
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
            "GetPage",
            "SearchMeta"});
			this.functionBox.Location = new System.Drawing.Point(416, 52);
			this.functionBox.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
			this.functionBox.Name = "functionBox";
			this.functionBox.Size = new System.Drawing.Size(201, 30);
			this.functionBox.TabIndex = 8;
			this.functionBox.SelectedIndexChanged += new System.EventHandler(this.ManualInputChanged);
			// 
			// objectIdBox
			// 
			this.objectIdBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.objectIdBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.objectIdBox.Location = new System.Drawing.Point(416, 15);
			this.objectIdBox.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.objectIdBox.Name = "objectIdBox";
			this.objectIdBox.ProcessEnterKey = false;
			this.objectIdBox.Size = new System.Drawing.Size(509, 29);
			this.objectIdBox.TabIndex = 7;
			this.objectIdBox.ThemedBack = null;
			this.objectIdBox.ThemedFore = null;
			this.objectIdBox.TextChanged += new System.EventHandler(this.ManualInputChanged);
			// 
			// manualLabel
			// 
			this.manualLabel.AutoSize = true;
			this.manualLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.manualLabel.Location = new System.Drawing.Point(284, 17);
			this.manualLabel.Margin = new System.Windows.Forms.Padding(100, 8, 3, 0);
			this.manualLabel.Name = "manualLabel";
			this.manualLabel.Size = new System.Drawing.Size(129, 20);
			this.manualLabel.TabIndex = 6;
			this.manualLabel.Text = "Lookup ObjectID";
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(2301, 23);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(2);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(124, 35);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 9;
			this.cancelButton.Text = "Close";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = false;
			this.cancelButton.Click += new System.EventHandler(this.Close);
			// 
			// editByBox2
			// 
			this.editByBox2.BackColor = System.Drawing.SystemColors.Control;
			this.editByBox2.Controls.Add(this.cancelButton);
			this.editByBox2.Controls.Add(this.okButton);
			this.editByBox2.Controls.Add(this.pageLink);
			this.editByBox2.Controls.Add(this.pagePath);
			this.editByBox2.Controls.Add(this.pageName);
			this.editByBox2.Controls.Add(this.pageLinkLabel);
			this.editByBox2.Controls.Add(this.pagePathLabel);
			this.editByBox2.Controls.Add(this.pageNameLabel);
			this.editByBox2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.editByBox2.Location = new System.Drawing.Point(0, 860);
			this.editByBox2.Margin = new System.Windows.Forms.Padding(2);
			this.editByBox2.Name = "editByBox2";
			this.editByBox2.Size = new System.Drawing.Size(2440, 80);
			this.editByBox2.TabIndex = 4;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.okButton.Enabled = false;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(2169, 23);
			this.okButton.Margin = new System.Windows.Forms.Padding(2);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(124, 35);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 8;
			this.okButton.Text = "Update Page";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = false;
			this.okButton.Click += new System.EventHandler(this.UpdatePage);
			// 
			// pageLink
			// 
			this.pageLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.pageLink.AutoSize = true;
			this.pageLink.BackColor = System.Drawing.SystemColors.Control;
			this.pageLink.ForeColor = System.Drawing.SystemColors.ControlText;
			this.pageLink.Location = new System.Drawing.Point(64, 60);
			this.pageLink.Name = "pageLink";
			this.pageLink.Size = new System.Drawing.Size(14, 20);
			this.pageLink.TabIndex = 0;
			this.pageLink.Text = "-";
			// 
			// pagePath
			// 
			this.pagePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.pagePath.AutoSize = true;
			this.pagePath.BackColor = System.Drawing.SystemColors.Control;
			this.pagePath.ForeColor = System.Drawing.SystemColors.ControlText;
			this.pagePath.Location = new System.Drawing.Point(64, 40);
			this.pagePath.Name = "pagePath";
			this.pagePath.Size = new System.Drawing.Size(14, 20);
			this.pagePath.TabIndex = 0;
			this.pagePath.Text = "-";
			// 
			// pageName
			// 
			this.pageName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.pageName.AutoSize = true;
			this.pageName.BackColor = System.Drawing.SystemColors.Control;
			this.pageName.ForeColor = System.Drawing.SystemColors.ControlText;
			this.pageName.Location = new System.Drawing.Point(64, 20);
			this.pageName.Name = "pageName";
			this.pageName.Size = new System.Drawing.Size(14, 20);
			this.pageName.TabIndex = 0;
			this.pageName.Text = "-";
			// 
			// pageLinkLabel
			// 
			this.pageLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.pageLinkLabel.AutoSize = true;
			this.pageLinkLabel.BackColor = System.Drawing.SystemColors.Control;
			this.pageLinkLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.pageLinkLabel.Location = new System.Drawing.Point(3, 60);
			this.pageLinkLabel.Name = "pageLinkLabel";
			this.pageLinkLabel.Size = new System.Drawing.Size(42, 20);
			this.pageLinkLabel.TabIndex = 0;
			this.pageLinkLabel.Text = "Link:";
			// 
			// pagePathLabel
			// 
			this.pagePathLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.pagePathLabel.AutoSize = true;
			this.pagePathLabel.BackColor = System.Drawing.SystemColors.Control;
			this.pagePathLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.pagePathLabel.Location = new System.Drawing.Point(3, 40);
			this.pagePathLabel.Name = "pagePathLabel";
			this.pagePathLabel.Size = new System.Drawing.Size(46, 20);
			this.pagePathLabel.TabIndex = 0;
			this.pagePathLabel.Text = "Path:";
			// 
			// pageNameLabel
			// 
			this.pageNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.pageNameLabel.AutoSize = true;
			this.pageNameLabel.BackColor = System.Drawing.SystemColors.Control;
			this.pageNameLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.pageNameLabel.Location = new System.Drawing.Point(3, 20);
			this.pageNameLabel.Name = "pageNameLabel";
			this.pageNameLabel.Size = new System.Drawing.Size(55, 20);
			this.pageNameLabel.TabIndex = 0;
			this.pageNameLabel.Text = "Name:";
			// 
			// pageOptionsPanel
			// 
			this.pageOptionsPanel.Controls.Add(this.editModeBox);
			this.pageOptionsPanel.Controls.Add(this.saveWindowBox);
			this.pageOptionsPanel.Controls.Add(this.multilineBox);
			this.pageOptionsPanel.Controls.Add(this.scopeBox);
			this.pageOptionsPanel.Controls.Add(this.hideEditedByBox);
			this.pageOptionsPanel.Controls.Add(this.linefeedBox);
			this.pageOptionsPanel.Controls.Add(this.pageInfoLabel);
			this.pageOptionsPanel.Location = new System.Drawing.Point(422, 4);
			this.pageOptionsPanel.Name = "pageOptionsPanel";
			this.pageOptionsPanel.Size = new System.Drawing.Size(969, 117);
			this.pageOptionsPanel.TabIndex = 9;
			// 
			// editModeBox
			// 
			this.editModeBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.editModeBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.editModeBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.editModeBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.editModeBox.Location = new System.Drawing.Point(330, 18);
			this.editModeBox.Name = "editModeBox";
			this.editModeBox.Size = new System.Drawing.Size(163, 25);
			this.editModeBox.StylizeImage = false;
			this.editModeBox.TabIndex = 9;
			this.editModeBox.Text = "Enable edit mode";
			this.editModeBox.ThemedBack = null;
			this.editModeBox.ThemedFore = null;
			this.tooltip.SetToolTip(this.editModeBox, "Enable to edit this page");
			this.editModeBox.UseVisualStyleBackColor = true;
			this.editModeBox.CheckedChanged += new System.EventHandler(this.ToggleEditMode);
			this.editModeBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FocusBox);
			// 
			// saveWindowBox
			// 
			this.saveWindowBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.saveWindowBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.saveWindowBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.saveWindowBox.Location = new System.Drawing.Point(330, 48);
			this.saveWindowBox.Name = "saveWindowBox";
			this.saveWindowBox.Size = new System.Drawing.Size(194, 25);
			this.saveWindowBox.StylizeImage = false;
			this.saveWindowBox.TabIndex = 7;
			this.saveWindowBox.Text = "Save window location";
			this.saveWindowBox.ThemedBack = null;
			this.saveWindowBox.ThemedFore = null;
			this.tooltip.SetToolTip(this.saveWindowBox, "Enable to save window position and size on close");
			this.saveWindowBox.UseVisualStyleBackColor = true;
			this.saveWindowBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FocusFind);
			// 
			// multilineBox
			// 
			this.multilineBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.multilineBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.multilineBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.multilineBox.Location = new System.Drawing.Point(3, 48);
			this.multilineBox.Name = "multilineBox";
			this.multilineBox.Size = new System.Drawing.Size(201, 25);
			this.multilineBox.StylizeImage = false;
			this.multilineBox.TabIndex = 8;
			this.multilineBox.Text = "Attributes on new lines";
			this.multilineBox.ThemedBack = null;
			this.multilineBox.ThemedFore = null;
			this.tooltip.SetToolTip(this.multilineBox, "Enable to show each attribute on its own line");
			this.multilineBox.UseVisualStyleBackColor = true;
			this.multilineBox.Click += new System.EventHandler(this.RefreshOnClick);
			this.multilineBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FocusFind);
			// 
			// scopeBox
			// 
			this.scopeBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.scopeBox.ItemHeight = 20;
			this.scopeBox.Location = new System.Drawing.Point(708, 10);
			this.scopeBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.scopeBox.Name = "scopeBox";
			this.scopeBox.Size = new System.Drawing.Size(223, 102);
			this.scopeBox.TabIndex = 4;
			this.tooltip.SetToolTip(this.scopeBox, "Select the level of detail to display");
			this.scopeBox.SelectedValueChanged += new System.EventHandler(this.RefreshPage);
			this.scopeBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FocusFind);
			// 
			// hideEditedByBox
			// 
			this.hideEditedByBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.hideEditedByBox.Checked = true;
			this.hideEditedByBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.hideEditedByBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.hideEditedByBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.hideEditedByBox.Location = new System.Drawing.Point(3, 18);
			this.hideEditedByBox.Name = "hideEditedByBox";
			this.hideEditedByBox.Size = new System.Drawing.Size(214, 25);
			this.hideEditedByBox.StylizeImage = false;
			this.hideEditedByBox.TabIndex = 5;
			this.hideEditedByBox.Text = "Hide edited-by attributes";
			this.hideEditedByBox.ThemedBack = null;
			this.hideEditedByBox.ThemedFore = null;
			this.tooltip.SetToolTip(this.hideEditedByBox, "Enable to hide ID and timestamp attributes");
			this.hideEditedByBox.UseVisualStyleBackColor = true;
			this.hideEditedByBox.Click += new System.EventHandler(this.RefreshOnClick);
			this.hideEditedByBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FocusFind);
			// 
			// linefeedBox
			// 
			this.linefeedBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.linefeedBox.Checked = true;
			this.linefeedBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.linefeedBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.linefeedBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.linefeedBox.Location = new System.Drawing.Point(3, 78);
			this.linefeedBox.Name = "linefeedBox";
			this.linefeedBox.Size = new System.Drawing.Size(218, 25);
			this.linefeedBox.StylizeImage = false;
			this.linefeedBox.TabIndex = 6;
			this.linefeedBox.Text = "Remove LF from CDATA";
			this.linefeedBox.ThemedBack = null;
			this.linefeedBox.ThemedFore = null;
			this.tooltip.SetToolTip(this.linefeedBox, "Enable to remove LF/CR from CDATA <spans>");
			this.linefeedBox.UseVisualStyleBackColor = true;
			this.linefeedBox.Click += new System.EventHandler(this.RefreshPage);
			this.linefeedBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FocusFind);
			// 
			// pageInfoLabel
			// 
			this.pageInfoLabel.AutoSize = true;
			this.pageInfoLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.pageInfoLabel.Location = new System.Drawing.Point(623, 12);
			this.pageInfoLabel.Name = "pageInfoLabel";
			this.pageInfoLabel.Size = new System.Drawing.Size(78, 20);
			this.pageInfoLabel.TabIndex = 0;
			this.pageInfoLabel.Text = "PageInfo:";
			// 
			// selectButton
			// 
			this.selectButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.selectButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.selectButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_SelectAll;
			this.selectButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.selectButton.ImageOver = null;
			this.selectButton.Location = new System.Drawing.Point(10, 53);
			this.selectButton.Margin = new System.Windows.Forms.Padding(2);
			this.selectButton.Name = "selectButton";
			this.selectButton.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
			this.selectButton.ShowBorder = true;
			this.selectButton.Size = new System.Drawing.Size(140, 35);
			this.selectButton.StylizeImage = true;
			this.selectButton.TabIndex = 3;
			this.selectButton.Text = "Select All";
			this.selectButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.selectButton.ThemedBack = null;
			this.selectButton.ThemedFore = null;
			this.selectButton.UseVisualStyleBackColor = false;
			this.selectButton.Click += new System.EventHandler(this.SelectAll);
			// 
			// topPanel
			// 
			this.topPanel.BackColor = System.Drawing.SystemColors.Control;
			this.topPanel.Controls.Add(this.manualPanel);
			this.topPanel.Controls.Add(this.pageOptionsPanel);
			this.topPanel.Controls.Add(this.wrapBox);
			this.topPanel.Controls.Add(this.findBox);
			this.topPanel.Controls.Add(this.findButton);
			this.topPanel.Controls.Add(this.selectButton);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = new System.Drawing.Point(0, 0);
			this.topPanel.Margin = new System.Windows.Forms.Padding(2);
			this.topPanel.Name = "topPanel";
			this.topPanel.Padding = new System.Windows.Forms.Padding(8, 1, 0, 0);
			this.topPanel.Size = new System.Drawing.Size(2440, 152);
			this.topPanel.TabIndex = 5;
			// 
			// wrapBox
			// 
			this.wrapBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.wrapBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.wrapBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.wrapBox.Location = new System.Drawing.Point(177, 59);
			this.wrapBox.Name = "wrapBox";
			this.wrapBox.Size = new System.Drawing.Size(106, 25);
			this.wrapBox.StylizeImage = false;
			this.wrapBox.TabIndex = 2;
			this.wrapBox.Text = "Wrap text";
			this.wrapBox.ThemedBack = null;
			this.wrapBox.ThemedFore = null;
			this.wrapBox.UseVisualStyleBackColor = true;
			this.wrapBox.CheckedChanged += new System.EventHandler(this.ToggleWrap);
			// 
			// findBox
			// 
			this.findBox.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.findBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.findBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.findBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.findBox.Location = new System.Drawing.Point(10, 11);
			this.findBox.Name = "findBox";
			this.findBox.ProcessEnterKey = false;
			this.findBox.Size = new System.Drawing.Size(270, 28);
			this.findBox.TabIndex = 0;
			this.findBox.ThemedBack = null;
			this.findBox.ThemedFore = null;
			this.findBox.TextChanged += new System.EventHandler(this.FindOptionsOnTextChanged);
			this.findBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FindOnKeyUp);
			// 
			// findButton
			// 
			this.findButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.findButton.Enabled = false;
			this.findButton.Font = new System.Drawing.Font("Segoe UI Symbol", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.findButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.findButton.ImageOver = null;
			this.findButton.Location = new System.Drawing.Point(286, 8);
			this.findButton.Name = "findButton";
			this.findButton.ShowBorder = true;
			this.findButton.Size = new System.Drawing.Size(66, 35);
			this.findButton.StylizeImage = false;
			this.findButton.TabIndex = 1;
			this.findButton.Text = "🔎";
			this.findButton.ThemedBack = null;
			this.findButton.ThemedFore = null;
			this.findButton.UseVisualStyleBackColor = true;
			this.findButton.Click += new System.EventHandler(this.FindOnClick);
			// 
			// masterPanel
			// 
			this.masterPanel.Controls.Add(this.tabs);
			this.masterPanel.Controls.Add(this.topPanel);
			this.masterPanel.Controls.Add(this.editByBox2);
			this.masterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.masterPanel.Location = new System.Drawing.Point(8, 8);
			this.masterPanel.Margin = new System.Windows.Forms.Padding(2);
			this.masterPanel.Name = "masterPanel";
			this.masterPanel.Size = new System.Drawing.Size(2440, 940);
			this.masterPanel.TabIndex = 6;
			// 
			// ShowXmlDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(2456, 956);
			this.Controls.Add(this.masterPanel);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(2);
			this.MinimumSize = new System.Drawing.Size(800, 500);
			this.Name = "ShowXmlDialog";
			this.Padding = new System.Windows.Forms.Padding(8);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
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
			this.editByBox2.ResumeLayout(false);
			this.editByBox2.PerformLayout();
			this.pageOptionsPanel.ResumeLayout(false);
			this.pageOptionsPanel.PerformLayout();
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.masterPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreTabControl tabs;
		private System.Windows.Forms.TabPage pageTab;
		private System.Windows.Forms.TabPage manualTab;
		private UI.MoreButton cancelButton;
		private System.Windows.Forms.Panel editByBox2;
		private System.Windows.Forms.RichTextBox pageBox;
		private System.Windows.Forms.RichTextBox manualBox;
		private System.Windows.Forms.Panel topPanel;
		private System.Windows.Forms.Panel masterPanel;
		private UI.MoreButton findButton;
		private UI.MoreButton selectButton;
		private UI.MoreButton okButton;
		private System.Windows.Forms.Label pageInfoLabel;
		private System.Windows.Forms.Panel pageOptionsPanel;
		private UI.MoreCheckBox wrapBox;
		private UI.MoreCheckBox hideEditedByBox;
		private UI.MoreCheckBox linefeedBox;
		private System.Windows.Forms.Label pageNameLabel;
		private System.Windows.Forms.Label pageLink;
		private System.Windows.Forms.Label pagePath;
		private System.Windows.Forms.Label pageName;
		private System.Windows.Forms.Label pageLinkLabel;
		private System.Windows.Forms.Label pagePathLabel;
		private System.Windows.Forms.ListBox scopeBox;
		private System.Windows.Forms.Label manualLabel;
		private System.Windows.Forms.ComboBox functionBox;
		private UI.MoreButton queryButton;
		private System.Windows.Forms.TabPage sectionTab;
		private System.Windows.Forms.TabPage notebooksTab;
		private System.Windows.Forms.TabPage nbSectionsTab;
		private System.Windows.Forms.TabPage nbPagesTab;
		private System.Windows.Forms.RichTextBox sectionBox;
		private System.Windows.Forms.RichTextBox notebookBox;
		private System.Windows.Forms.RichTextBox nbSectionBox;
		private System.Windows.Forms.RichTextBox nbPagesBox;
		private System.Windows.Forms.Panel manualPanel;
		private UI.MoreCheckBox pidBox;
		private System.Windows.Forms.ImageList tabIcons;
		private UI.MoreCheckBox saveWindowBox;
		private UI.MoreCheckBox multilineBox;
		private UI.MoreCheckBox editModeBox;
		private System.Windows.Forms.ToolTip tooltip;
		private UI.MoreCheckBox multilineBox2;
		private System.Windows.Forms.Label fnLabel;
		private UI.MoreCheckBox hideEditedByBox2;
		private UI.MoreTextBox findBox;
		private UI.MoreTextBox objectIdBox;
	}
}