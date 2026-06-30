namespace River.OneMoreAddIn.Settings
{
	partial class SettingsDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsDialog));
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.editorPanel = new System.Windows.Forms.Panel();
			this.contentPanel = new System.Windows.Forms.Panel();
			this.headerLabel = new River.OneMoreAddIn.Settings.FadingLabel();
			this.navPanel = new System.Windows.Forms.Panel();
			this.generalLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.colorizerLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.colorsLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.aliasLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.contextLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.favoritesLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.fileImportLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.hashtagsLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.highlightLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.imagesLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.keyboardLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.markdownLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.navigatorLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.pluginsLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.quickNotesLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.ribbonLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.searchLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.snippetsLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.tableThemesLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.variablesLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.buttonPanel.SuspendLayout();
			this.editorPanel.SuspendLayout();
			this.navPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonPanel
			// 
			this.buttonPanel.Controls.Add(this.okButton);
			this.buttonPanel.Controls.Add(this.cancelButton);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.Location = new System.Drawing.Point(15, 672);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
			this.buttonPanel.Size = new System.Drawing.Size(1126, 57);
			this.buttonPanel.TabIndex = 0;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(874, 13);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(120, 38);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 1;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OK);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(1000, 13);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(120, 38);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 0;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// editorPanel
			// 
			this.editorPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.editorPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.editorPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.editorPanel.Controls.Add(this.contentPanel);
			this.editorPanel.Controls.Add(this.headerLabel);
			this.editorPanel.Location = new System.Drawing.Point(313, 15);
			this.editorPanel.Name = "editorPanel";
			this.editorPanel.Padding = new System.Windows.Forms.Padding(2);
			this.editorPanel.Size = new System.Drawing.Size(826, 657);
			this.editorPanel.TabIndex = 1;
			// 
			// contentPanel
			// 
			this.contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.contentPanel.Location = new System.Drawing.Point(2, 34);
			this.contentPanel.MinimumSize = new System.Drawing.Size(800, 500);
			this.contentPanel.Name = "contentPanel";
			this.contentPanel.Padding = new System.Windows.Forms.Padding(15);
			this.contentPanel.Size = new System.Drawing.Size(820, 619);
			this.contentPanel.TabIndex = 1;
			// 
			// headerLabel
			// 
			this.headerLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.headerLabel.Font = new System.Drawing.Font("Tahoma", 9F);
			this.headerLabel.ForeColor = System.Drawing.Color.White;
			this.headerLabel.Location = new System.Drawing.Point(2, 2);
			this.headerLabel.Name = "headerLabel";
			this.headerLabel.Size = new System.Drawing.Size(820, 32);
			this.headerLabel.TabIndex = 0;
			this.headerLabel.Text = "Header";
			this.headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// navPanel
			// 
			this.navPanel.AutoScroll = true;
			this.navPanel.BackColor = System.Drawing.SystemColors.Window;
			this.navPanel.Controls.Add(this.generalLink);
			this.navPanel.Controls.Add(this.colorizerLink);
			this.navPanel.Controls.Add(this.colorsLink);
			this.navPanel.Controls.Add(this.aliasLink);
			this.navPanel.Controls.Add(this.contextLink);
			this.navPanel.Controls.Add(this.favoritesLink);
			this.navPanel.Controls.Add(this.fileImportLink);
			this.navPanel.Controls.Add(this.hashtagsLink);
			this.navPanel.Controls.Add(this.highlightLink);
			this.navPanel.Controls.Add(this.imagesLink);
			this.navPanel.Controls.Add(this.keyboardLink);
			this.navPanel.Controls.Add(this.markdownLink);
			this.navPanel.Controls.Add(this.navigatorLink);
			this.navPanel.Controls.Add(this.pluginsLink);
			this.navPanel.Controls.Add(this.quickNotesLink);
			this.navPanel.Controls.Add(this.ribbonLink);
			this.navPanel.Controls.Add(this.searchLink);
			this.navPanel.Controls.Add(this.snippetsLink);
			this.navPanel.Controls.Add(this.tableThemesLink);
			this.navPanel.Controls.Add(this.variablesLink);
			this.navPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.navPanel.Location = new System.Drawing.Point(15, 15);
			this.navPanel.Margin = new System.Windows.Forms.Padding(3, 3, 15, 3);
			this.navPanel.Name = "navPanel";
			this.navPanel.Size = new System.Drawing.Size(280, 657);
			this.navPanel.TabIndex = 2;
			// 
			// generalLink
			// 
			this.generalLink.Active = true;
			this.generalLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.generalLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.generalLink.HoverColor = System.Drawing.Color.Orchid;
			this.generalLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.generalLink.Location = new System.Drawing.Point(8, 10);
			this.generalLink.Name = "generalLink";
			this.generalLink.NavMode = true;
			this.generalLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.generalLink.Selected = false;
			this.generalLink.Size = new System.Drawing.Size(256, 20);
			this.generalLink.StrictColors = false;
			this.generalLink.TabIndex = 0;
			this.generalLink.TabStop = true;
			this.generalLink.Tag = 0;
			this.generalLink.Text = "General Options";
			this.generalLink.ThemedBack = null;
			this.generalLink.ThemedFore = null;
			this.generalLink.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoLinkKeyDown);
			this.generalLink.Click += new System.EventHandler(this.DoLinkClicked);
			// 
			// colorizerLink
			// 
			this.colorizerLink.Active = false;
			this.colorizerLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.colorizerLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.colorizerLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.colorizerLink.HoverColor = System.Drawing.Color.Orchid;
			this.colorizerLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.colorizerLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.colorizerLink.Location = new System.Drawing.Point(8, 40);
			this.colorizerLink.Name = "colorizerLink";
			this.colorizerLink.NavMode = true;
			this.colorizerLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.colorizerLink.Selected = false;
			this.colorizerLink.Size = new System.Drawing.Size(256, 20);
			this.colorizerLink.StrictColors = false;
			this.colorizerLink.TabIndex = 1;
			this.colorizerLink.TabStop = true;
			this.colorizerLink.Tag = 1;
			this.colorizerLink.Text = "Colorizer";
			this.colorizerLink.ThemedBack = null;
			this.colorizerLink.ThemedFore = null;
			this.colorizerLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.colorizerLink.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoLinkKeyDown);
			this.colorizerLink.Click += new System.EventHandler(this.DoLinkClicked);
			// 
			// colorsLink
			// 
			this.colorsLink.Active = false;
			this.colorsLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.colorsLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.colorsLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.colorsLink.HoverColor = System.Drawing.Color.Orchid;
			this.colorsLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.colorsLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.colorsLink.Location = new System.Drawing.Point(8, 70);
			this.colorsLink.Name = "colorsLink";
			this.colorsLink.NavMode = true;
			this.colorsLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.colorsLink.Selected = false;
			this.colorsLink.Size = new System.Drawing.Size(256, 20);
			this.colorsLink.StrictColors = false;
			this.colorsLink.TabIndex = 2;
			this.colorsLink.TabStop = true;
			this.colorsLink.Tag = 2;
			this.colorsLink.Text = "Colors";
			this.colorsLink.ThemedBack = null;
			this.colorsLink.ThemedFore = null;
			this.colorsLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.colorsLink.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoLinkKeyDown);
			this.colorsLink.Click += new System.EventHandler(this.DoLinkClicked);
			// 
			// aliasLink
			// 
			this.aliasLink.Active = false;
			this.aliasLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.aliasLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.aliasLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.aliasLink.HoverColor = System.Drawing.Color.Orchid;
			this.aliasLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.aliasLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.aliasLink.Location = new System.Drawing.Point(8, 100);
			this.aliasLink.Name = "aliasLink";
			this.aliasLink.NavMode = true;
			this.aliasLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.aliasLink.Selected = false;
			this.aliasLink.Size = new System.Drawing.Size(256, 20);
			this.aliasLink.StrictColors = false;
			this.aliasLink.TabIndex = 3;
			this.aliasLink.TabStop = true;
			this.aliasLink.Tag = 3;
			this.aliasLink.Text = "Command Aliases";
			this.aliasLink.ThemedBack = null;
			this.aliasLink.ThemedFore = null;
			this.aliasLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.aliasLink.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoLinkKeyDown);
			this.aliasLink.Click += new System.EventHandler(this.DoLinkClicked);
			// 
			// contextLink
			// 
			this.contextLink.Active = false;
			this.contextLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.contextLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.contextLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.contextLink.HoverColor = System.Drawing.Color.Orchid;
			this.contextLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.contextLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.contextLink.Location = new System.Drawing.Point(8, 130);
			this.contextLink.Name = "contextLink";
			this.contextLink.NavMode = true;
			this.contextLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.contextLink.Selected = false;
			this.contextLink.Size = new System.Drawing.Size(256, 20);
			this.contextLink.StrictColors = false;
			this.contextLink.TabIndex = 4;
			this.contextLink.TabStop = true;
			this.contextLink.Tag = 4;
			this.contextLink.Text = "Context Menu";
			this.contextLink.ThemedBack = null;
			this.contextLink.ThemedFore = null;
			this.contextLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.contextLink.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoLinkKeyDown);
			this.contextLink.Click += new System.EventHandler(this.DoLinkClicked);
			// 
			// favoritesLink
			// 
			this.favoritesLink.Active = false;
			this.favoritesLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.favoritesLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.favoritesLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.favoritesLink.HoverColor = System.Drawing.Color.Orchid;
			this.favoritesLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.favoritesLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.favoritesLink.Location = new System.Drawing.Point(8, 160);
			this.favoritesLink.Name = "favoritesLink";
			this.favoritesLink.NavMode = true;
			this.favoritesLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.favoritesLink.Selected = false;
			this.favoritesLink.Size = new System.Drawing.Size(256, 20);
			this.favoritesLink.StrictColors = false;
			this.favoritesLink.TabIndex = 5;
			this.favoritesLink.TabStop = true;
			this.favoritesLink.Tag = 5;
			this.favoritesLink.Text = "Favorites";
			this.favoritesLink.ThemedBack = null;
			this.favoritesLink.ThemedFore = null;
			this.favoritesLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.favoritesLink.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoLinkKeyDown);
			this.favoritesLink.Click += new System.EventHandler(this.DoLinkClicked);
			// 
			// fileImportLink
			// 
			this.fileImportLink.Active = false;
			this.fileImportLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.fileImportLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.fileImportLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.fileImportLink.HoverColor = System.Drawing.Color.Orchid;
			this.fileImportLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.fileImportLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.fileImportLink.Location = new System.Drawing.Point(8, 190);
			this.fileImportLink.Name = "fileImportLink";
			this.fileImportLink.NavMode = true;
			this.fileImportLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.fileImportLink.Selected = false;
			this.fileImportLink.Size = new System.Drawing.Size(256, 20);
			this.fileImportLink.StrictColors = false;
			this.fileImportLink.TabIndex = 6;
			this.fileImportLink.TabStop = true;
			this.fileImportLink.Tag = 6;
			this.fileImportLink.Text = "File Import";
			this.fileImportLink.ThemedBack = null;
			this.fileImportLink.ThemedFore = null;
			this.fileImportLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.fileImportLink.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoLinkKeyDown);
			this.fileImportLink.Click += new System.EventHandler(this.DoLinkClicked);
			// 
			// hashtagsLink
			// 
			this.hashtagsLink.Active = false;
			this.hashtagsLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.hashtagsLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.hashtagsLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.hashtagsLink.HoverColor = System.Drawing.Color.Orchid;
			this.hashtagsLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.hashtagsLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.hashtagsLink.Location = new System.Drawing.Point(8, 220);
			this.hashtagsLink.Name = "hashtagsLink";
			this.hashtagsLink.NavMode = true;
			this.hashtagsLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.hashtagsLink.Selected = false;
			this.hashtagsLink.Size = new System.Drawing.Size(256, 20);
			this.hashtagsLink.StrictColors = false;
			this.hashtagsLink.TabIndex = 7;
			this.hashtagsLink.TabStop = true;
			this.hashtagsLink.Tag = 7;
			this.hashtagsLink.Text = "Hashtags";
			this.hashtagsLink.ThemedBack = null;
			this.hashtagsLink.ThemedFore = null;
			this.hashtagsLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.hashtagsLink.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoLinkKeyDown);
			this.hashtagsLink.Click += new System.EventHandler(this.DoLinkClicked);
			// 
			// highlightLink
			// 
			this.highlightLink.Active = false;
			this.highlightLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.highlightLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.highlightLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.highlightLink.HoverColor = System.Drawing.Color.Orchid;
			this.highlightLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.highlightLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.highlightLink.Location = new System.Drawing.Point(8, 250);
			this.highlightLink.Name = "highlightLink";
			this.highlightLink.NavMode = true;
			this.highlightLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.highlightLink.Selected = false;
			this.highlightLink.Size = new System.Drawing.Size(256, 20);
			this.highlightLink.StrictColors = false;
			this.highlightLink.TabIndex = 8;
			this.highlightLink.TabStop = true;
			this.highlightLink.Tag = 8;
			this.highlightLink.Text = "Highlighter Themes";
			this.highlightLink.ThemedBack = null;
			this.highlightLink.ThemedFore = null;
			this.highlightLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.highlightLink.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoLinkKeyDown);
			this.highlightLink.Click += new System.EventHandler(this.DoLinkClicked);
			// 
			// imagesLink
			// 
			this.imagesLink.Active = false;
			this.imagesLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.imagesLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.imagesLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.imagesLink.HoverColor = System.Drawing.Color.Orchid;
			this.imagesLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.imagesLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.imagesLink.Location = new System.Drawing.Point(8, 280);
			this.imagesLink.Name = "imagesLink";
			this.imagesLink.NavMode = true;
			this.imagesLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.imagesLink.Selected = false;
			this.imagesLink.Size = new System.Drawing.Size(256, 20);
			this.imagesLink.StrictColors = false;
			this.imagesLink.TabIndex = 9;
			this.imagesLink.TabStop = true;
			this.imagesLink.Tag = 9;
			this.imagesLink.Text = "Images";
			this.imagesLink.ThemedBack = null;
			this.imagesLink.ThemedFore = null;
			this.imagesLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.imagesLink.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoLinkKeyDown);
			this.imagesLink.Click += new System.EventHandler(this.DoLinkClicked);
			// 
			// keyboardLink
			// 
			this.keyboardLink.Active = false;
			this.keyboardLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.keyboardLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.keyboardLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.keyboardLink.HoverColor = System.Drawing.Color.Orchid;
			this.keyboardLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.keyboardLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.keyboardLink.Location = new System.Drawing.Point(8, 310);
			this.keyboardLink.Name = "keyboardLink";
			this.keyboardLink.NavMode = true;
			this.keyboardLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.keyboardLink.Selected = false;
			this.keyboardLink.Size = new System.Drawing.Size(256, 20);
			this.keyboardLink.StrictColors = false;
			this.keyboardLink.TabIndex = 10;
			this.keyboardLink.TabStop = true;
			this.keyboardLink.Tag = 10;
			this.keyboardLink.Text = "Keyboard Shortcuts";
			this.keyboardLink.ThemedBack = null;
			this.keyboardLink.ThemedFore = null;
			this.keyboardLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.keyboardLink.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoLinkKeyDown);
			this.keyboardLink.Click += new System.EventHandler(this.DoLinkClicked);
			//
			// markdownLink
			//
			this.markdownLink.Active = false;
			this.markdownLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.markdownLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.markdownLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.markdownLink.HoverColor = System.Drawing.Color.Orchid;
			this.markdownLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.markdownLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.markdownLink.Location = new System.Drawing.Point(8, 340);
			this.markdownLink.Name = "markdownLink";
			this.markdownLink.NavMode = true;
			this.markdownLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.markdownLink.Selected = false;
			this.markdownLink.Size = new System.Drawing.Size(256, 20);
			this.markdownLink.StrictColors = false;
			this.markdownLink.TabIndex = 11;
			this.markdownLink.TabStop = true;
			this.markdownLink.Tag = 11;
			this.markdownLink.Text = "Markdown";
			this.markdownLink.ThemedBack = null;
			this.markdownLink.ThemedFore = null;
			this.markdownLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.markdownLink.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoLinkKeyDown);
			this.markdownLink.Click += new System.EventHandler(this.DoLinkClicked);
			//
			// navigatorLink
			//
			this.navigatorLink.Active = false;
			this.navigatorLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.navigatorLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.navigatorLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.navigatorLink.HoverColor = System.Drawing.Color.Orchid;
			this.navigatorLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.navigatorLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.navigatorLink.Location = new System.Drawing.Point(8, 340);
			this.navigatorLink.Name = "navigatorLink";
			this.navigatorLink.NavMode = true;
			this.navigatorLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.navigatorLink.Selected = false;
			this.navigatorLink.Size = new System.Drawing.Size(256, 20);
			this.navigatorLink.StrictColors = false;
			this.navigatorLink.TabIndex = 12;
			this.navigatorLink.TabStop = true;
			this.navigatorLink.Tag = 12;
			this.navigatorLink.Text = "Navigator";
			this.navigatorLink.ThemedBack = null;
			this.navigatorLink.ThemedFore = null;
			this.navigatorLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.navigatorLink.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoLinkKeyDown);
			this.navigatorLink.Click += new System.EventHandler(this.DoLinkClicked);
			// 
			// pluginsLink
			// 
			this.pluginsLink.Active = false;
			this.pluginsLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.pluginsLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pluginsLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pluginsLink.HoverColor = System.Drawing.Color.Orchid;
			this.pluginsLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.pluginsLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.pluginsLink.Location = new System.Drawing.Point(8, 370);
			this.pluginsLink.Name = "pluginsLink";
			this.pluginsLink.NavMode = true;
			this.pluginsLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.pluginsLink.Selected = false;
			this.pluginsLink.Size = new System.Drawing.Size(256, 20);
			this.pluginsLink.StrictColors = false;
			this.pluginsLink.TabIndex = 13;
			this.pluginsLink.TabStop = true;
			this.pluginsLink.Tag = 13;
			this.pluginsLink.Text = "Plugins";
			this.pluginsLink.ThemedBack = null;
			this.pluginsLink.ThemedFore = null;
			this.pluginsLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.pluginsLink.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoLinkKeyDown);
			this.pluginsLink.Click += new System.EventHandler(this.DoLinkClicked);
			// 
			// quickNotesLink
			// 
			this.quickNotesLink.Active = false;
			this.quickNotesLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.quickNotesLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.quickNotesLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.quickNotesLink.HoverColor = System.Drawing.Color.Orchid;
			this.quickNotesLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.quickNotesLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.quickNotesLink.Location = new System.Drawing.Point(8, 400);
			this.quickNotesLink.Name = "quickNotesLink";
			this.quickNotesLink.NavMode = true;
			this.quickNotesLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.quickNotesLink.Selected = false;
			this.quickNotesLink.Size = new System.Drawing.Size(256, 20);
			this.quickNotesLink.StrictColors = false;
			this.quickNotesLink.TabIndex = 14;
			this.quickNotesLink.TabStop = true;
			this.quickNotesLink.Tag = 14;
			this.quickNotesLink.Text = "Quick Notes";
			this.quickNotesLink.ThemedBack = null;
			this.quickNotesLink.ThemedFore = null;
			this.quickNotesLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.quickNotesLink.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoLinkKeyDown);
			this.quickNotesLink.Click += new System.EventHandler(this.DoLinkClicked);
			// 
			// ribbonLink
			// 
			this.ribbonLink.Active = false;
			this.ribbonLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.ribbonLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ribbonLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.ribbonLink.HoverColor = System.Drawing.Color.Orchid;
			this.ribbonLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.ribbonLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.ribbonLink.Location = new System.Drawing.Point(8, 430);
			this.ribbonLink.Name = "ribbonLink";
			this.ribbonLink.NavMode = true;
			this.ribbonLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.ribbonLink.Selected = false;
			this.ribbonLink.Size = new System.Drawing.Size(256, 20);
			this.ribbonLink.StrictColors = false;
			this.ribbonLink.TabIndex = 15;
			this.ribbonLink.TabStop = true;
			this.ribbonLink.Tag = 15;
			this.ribbonLink.Text = "Ribbon Bar";
			this.ribbonLink.ThemedBack = null;
			this.ribbonLink.ThemedFore = null;
			this.ribbonLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.ribbonLink.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoLinkKeyDown);
			this.ribbonLink.Click += new System.EventHandler(this.DoLinkClicked);
			// 
			// searchLink
			// 
			this.searchLink.Active = false;
			this.searchLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.searchLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.searchLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.searchLink.HoverColor = System.Drawing.Color.Orchid;
			this.searchLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.searchLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.searchLink.Location = new System.Drawing.Point(8, 460);
			this.searchLink.Name = "searchLink";
			this.searchLink.NavMode = true;
			this.searchLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.searchLink.Selected = false;
			this.searchLink.Size = new System.Drawing.Size(256, 20);
			this.searchLink.StrictColors = false;
			this.searchLink.TabIndex = 16;
			this.searchLink.TabStop = true;
			this.searchLink.Tag = 16;
			this.searchLink.Text = "Search Engines";
			this.searchLink.ThemedBack = null;
			this.searchLink.ThemedFore = null;
			this.searchLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.searchLink.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoLinkKeyDown);
			this.searchLink.Click += new System.EventHandler(this.DoLinkClicked);
			// 
			// snippetsLink
			// 
			this.snippetsLink.Active = false;
			this.snippetsLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.snippetsLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.snippetsLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.snippetsLink.HoverColor = System.Drawing.Color.Orchid;
			this.snippetsLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.snippetsLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.snippetsLink.Location = new System.Drawing.Point(8, 490);
			this.snippetsLink.Name = "snippetsLink";
			this.snippetsLink.NavMode = true;
			this.snippetsLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.snippetsLink.Selected = false;
			this.snippetsLink.Size = new System.Drawing.Size(256, 20);
			this.snippetsLink.StrictColors = false;
			this.snippetsLink.TabIndex = 17;
			this.snippetsLink.TabStop = true;
			this.snippetsLink.Tag = 17;
			this.snippetsLink.Text = "Snippets";
			this.snippetsLink.ThemedBack = null;
			this.snippetsLink.ThemedFore = null;
			this.snippetsLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.snippetsLink.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoLinkKeyDown);
			this.snippetsLink.Click += new System.EventHandler(this.DoLinkClicked);
			// 
			// tableThemesLink
			// 
			this.tableThemesLink.Active = false;
			this.tableThemesLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.tableThemesLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableThemesLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.tableThemesLink.HoverColor = System.Drawing.Color.Orchid;
			this.tableThemesLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.tableThemesLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.tableThemesLink.Location = new System.Drawing.Point(8, 520);
			this.tableThemesLink.Name = "tableThemesLink";
			this.tableThemesLink.NavMode = true;
			this.tableThemesLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.tableThemesLink.Selected = false;
			this.tableThemesLink.Size = new System.Drawing.Size(256, 20);
			this.tableThemesLink.StrictColors = false;
			this.tableThemesLink.TabIndex = 18;
			this.tableThemesLink.TabStop = true;
			this.tableThemesLink.Tag = 18;
			this.tableThemesLink.Text = "Table Themes";
			this.tableThemesLink.ThemedBack = null;
			this.tableThemesLink.ThemedFore = null;
			this.tableThemesLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.tableThemesLink.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoLinkKeyDown);
			this.tableThemesLink.Click += new System.EventHandler(this.DoLinkClicked);
			// 
			// variablesLink
			// 
			this.variablesLink.Active = false;
			this.variablesLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.variablesLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.variablesLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.variablesLink.HoverColor = System.Drawing.Color.Orchid;
			this.variablesLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.variablesLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.variablesLink.Location = new System.Drawing.Point(8, 550);
			this.variablesLink.Name = "variablesLink";
			this.variablesLink.NavMode = true;
			this.variablesLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.variablesLink.Selected = false;
			this.variablesLink.Size = new System.Drawing.Size(256, 20);
			this.variablesLink.StrictColors = false;
			this.variablesLink.TabIndex = 19;
			this.variablesLink.TabStop = true;
			this.variablesLink.Tag = 19;
			this.variablesLink.Text = "Variables";
			this.variablesLink.ThemedBack = null;
			this.variablesLink.ThemedFore = null;
			this.variablesLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.variablesLink.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoLinkKeyDown);
			this.variablesLink.Click += new System.EventHandler(this.DoLinkClicked);
			// 
			// SettingsDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(1156, 744);
			this.Controls.Add(this.editorPanel);
			this.Controls.Add(this.navPanel);
			this.Controls.Add(this.buttonPanel);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SettingsDialog";
			this.Padding = new System.Windows.Forms.Padding(15);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "OneMore Settings";
			this.Load += new System.EventHandler(this.InitializeLoad);
			this.buttonPanel.ResumeLayout(false);
			this.editorPanel.ResumeLayout(false);
			this.navPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel buttonPanel;
		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private System.Windows.Forms.Panel editorPanel;
		private System.Windows.Forms.Panel contentPanel;
		private FadingLabel headerLabel;
		private System.Windows.Forms.Panel navPanel;
		private UI.MoreLinkLabel generalLink;
		private UI.MoreLinkLabel colorizerLink;
		private UI.MoreLinkLabel colorsLink;
		private UI.MoreLinkLabel aliasLink;
		private UI.MoreLinkLabel contextLink;
		private UI.MoreLinkLabel favoritesLink;
		private UI.MoreLinkLabel fileImportLink;
		private UI.MoreLinkLabel hashtagsLink;
		private UI.MoreLinkLabel highlightLink;
		private UI.MoreLinkLabel imagesLink;
		private UI.MoreLinkLabel keyboardLink;
		private UI.MoreLinkLabel markdownLink;
		private UI.MoreLinkLabel navigatorLink;
		private UI.MoreLinkLabel pluginsLink;
		private UI.MoreLinkLabel quickNotesLink;
		private UI.MoreLinkLabel ribbonLink;
		private UI.MoreLinkLabel searchLink;
		private UI.MoreLinkLabel snippetsLink;
		private UI.MoreLinkLabel tableThemesLink;
		private UI.MoreLinkLabel variablesLink;
	}
}