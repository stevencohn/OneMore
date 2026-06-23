namespace River.OneMoreAddIn.Commands
{
	partial class EmojiDialog
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EmojiDialog));
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.tabs = new River.OneMoreAddIn.UI.MoreTabControl();
			this.listTab = new System.Windows.Forms.TabPage();
			this.emojiBox = new River.OneMoreAddIn.UI.MoreListView();
			this.nameColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.gridTab = new System.Windows.Forms.TabPage();
			this.gridBox = new River.OneMoreAddIn.UI.MoreIconListView();
			this.searchPanel = new System.Windows.Forms.Panel();
			this.searchLabel = new System.Windows.Forms.Label();
			this.searchBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.categoryPanel = new River.OneMoreAddIn.UI.MorePanel();
			this.generalLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.smileysLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.peopleLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.animalsLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.foodLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.travelLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.activitiesLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.objectsLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.symbolsLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.flagsLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.introLabel = new System.Windows.Forms.Label();
			this.tabs.SuspendLayout();
			this.listTab.SuspendLayout();
			this.gridTab.SuspendLayout();
			this.searchPanel.SuspendLayout();
			this.categoryPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(347, 702);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(99, 38);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 2;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.Cancel);
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(242, 702);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(99, 38);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 1;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OK);
			// 
			// tabs
			// 
			this.tabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabs.Controls.Add(this.listTab);
			this.tabs.Controls.Add(this.gridTab);
			this.tabs.ItemSize = new System.Drawing.Size(0, 32);
			this.tabs.Location = new System.Drawing.Point(12, 62);
			this.tabs.Margin = new System.Windows.Forms.Padding(0);
			this.tabs.Name = "tabs";
			this.tabs.Padding = new System.Drawing.Point(0, 0);
			this.tabs.SelectedIndex = 0;
			this.tabs.Size = new System.Drawing.Size(434, 628);
			this.tabs.TabIndex = 0;
			this.tabs.SelectedIndexChanged += new System.EventHandler(this.DoTabSelected);
			// 
			// listTab
			// 
			this.listTab.Controls.Add(this.emojiBox);
			this.listTab.Location = new System.Drawing.Point(4, 36);
			this.listTab.Name = "listTab";
			this.listTab.Size = new System.Drawing.Size(426, 588);
			this.listTab.TabIndex = 0;
			this.listTab.Text = "Quick Pick";
			// 
			// emojiBox
			// 
			this.emojiBox.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameColumn});
			this.emojiBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.emojiBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.emojiBox.FullRowSelect = true;
			this.emojiBox.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.emojiBox.HideSelection = false;
			this.emojiBox.Location = new System.Drawing.Point(0, 0);
			this.emojiBox.Name = "emojiBox";
			this.emojiBox.OwnerDraw = true;
			this.emojiBox.Size = new System.Drawing.Size(426, 588);
			this.emojiBox.TabIndex = 0;
			this.emojiBox.UseCompatibleStateImageBehavior = false;
			this.emojiBox.View = System.Windows.Forms.View.Details;
			this.emojiBox.SelectedIndexChanged += new System.EventHandler(this.DoListSelectionChanged);
			this.emojiBox.DoubleClick += new System.EventHandler(this.DoubleClickItem);
			// 
			// nameColumn
			// 
			this.nameColumn.Name = "nameColumn";
			// 
			// gridTab
			// 
			this.gridTab.Controls.Add(this.gridBox);
			this.gridTab.Controls.Add(this.searchPanel);
			this.gridTab.Controls.Add(this.categoryPanel);
			this.gridTab.Location = new System.Drawing.Point(4, 36);
			this.gridTab.Name = "gridTab";
			this.gridTab.Size = new System.Drawing.Size(426, 588);
			this.gridTab.TabIndex = 1;
			this.gridTab.Text = "Palette";
			// 
			// gridBox
			// 
			this.gridBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridBox.HideSelection = false;
			this.gridBox.Location = new System.Drawing.Point(200, 56);
			this.gridBox.Name = "gridBox";
			this.gridBox.OwnerDraw = true;
			this.gridBox.Size = new System.Drawing.Size(226, 532);
			this.gridBox.TabIndex = 1;
			this.gridBox.UseCompatibleStateImageBehavior = false;
			this.gridBox.SelectedIndexChanged += new System.EventHandler(this.DoGridSelectionChanged);
			this.gridBox.DoubleClick += new System.EventHandler(this.DoubleClickItem);
			// 
			// searchPanel
			// 
			this.searchPanel.Controls.Add(this.searchLabel);
			this.searchPanel.Controls.Add(this.searchBox);
			this.searchPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.searchPanel.Location = new System.Drawing.Point(200, 0);
			this.searchPanel.Name = "searchPanel";
			this.searchPanel.Size = new System.Drawing.Size(226, 56);
			this.searchPanel.TabIndex = 2;
			// 
			// searchLabel
			// 
			this.searchLabel.AutoSize = true;
			this.searchLabel.Location = new System.Drawing.Point(6, 17);
			this.searchLabel.Name = "searchLabel";
			this.searchLabel.Size = new System.Drawing.Size(60, 20);
			this.searchLabel.TabIndex = 0;
			this.searchLabel.Text = "Search";
			// 
			// searchBox
			// 
			this.searchBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.searchBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.searchBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.searchBox.Location = new System.Drawing.Point(98, 13);
			this.searchBox.Name = "searchBox";
			this.searchBox.ProcessEnterKey = false;
			this.searchBox.Size = new System.Drawing.Size(98, 28);
			this.searchBox.TabIndex = 1;
			this.searchBox.ThemedBack = null;
			this.searchBox.ThemedFore = null;
			this.searchBox.TextChanged += new System.EventHandler(this.FilterGridOnSearchChanged);
			// 
			// categoryPanel
			// 
			this.categoryPanel.BottomBorderColor = System.Drawing.SystemColors.ActiveBorder;
			this.categoryPanel.BottomBorderSize = 0;
			this.categoryPanel.Controls.Add(this.generalLink);
			this.categoryPanel.Controls.Add(this.smileysLink);
			this.categoryPanel.Controls.Add(this.peopleLink);
			this.categoryPanel.Controls.Add(this.animalsLink);
			this.categoryPanel.Controls.Add(this.foodLink);
			this.categoryPanel.Controls.Add(this.travelLink);
			this.categoryPanel.Controls.Add(this.activitiesLink);
			this.categoryPanel.Controls.Add(this.objectsLink);
			this.categoryPanel.Controls.Add(this.symbolsLink);
			this.categoryPanel.Controls.Add(this.flagsLink);
			this.categoryPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.categoryPanel.Location = new System.Drawing.Point(0, 0);
			this.categoryPanel.Name = "categoryPanel";
			this.categoryPanel.Size = new System.Drawing.Size(200, 588);
			this.categoryPanel.TabIndex = 0;
			this.categoryPanel.ThemedBack = null;
			this.categoryPanel.ThemedFore = null;
			this.categoryPanel.TopBorderColor = System.Drawing.SystemColors.Control;
			this.categoryPanel.TopBorderSize = 0;
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
			this.generalLink.Size = new System.Drawing.Size(184, 20);
			this.generalLink.StrictColors = false;
			this.generalLink.TabIndex = 0;
			this.generalLink.TabStop = true;
			this.generalLink.Tag = "General";
			this.generalLink.Text = "General";
			this.generalLink.ThemedBack = null;
			this.generalLink.ThemedFore = null;
			this.generalLink.Click += new System.EventHandler(this.DoCategoryLinkClicked);
			// 
			// smileysLink
			// 
			this.smileysLink.Active = false;
			this.smileysLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.smileysLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.smileysLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.smileysLink.HoverColor = System.Drawing.Color.Orchid;
			this.smileysLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.smileysLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.smileysLink.Location = new System.Drawing.Point(8, 40);
			this.smileysLink.Name = "smileysLink";
			this.smileysLink.NavMode = true;
			this.smileysLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.smileysLink.Selected = false;
			this.smileysLink.Size = new System.Drawing.Size(184, 20);
			this.smileysLink.StrictColors = false;
			this.smileysLink.TabIndex = 1;
			this.smileysLink.TabStop = true;
			this.smileysLink.Tag = "Smileys & Emotion";
			this.smileysLink.Text = "Smileys && Emotion";
			this.smileysLink.ThemedBack = null;
			this.smileysLink.ThemedFore = null;
			this.smileysLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.smileysLink.Click += new System.EventHandler(this.DoCategoryLinkClicked);
			// 
			// peopleLink
			// 
			this.peopleLink.Active = false;
			this.peopleLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.peopleLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.peopleLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.peopleLink.HoverColor = System.Drawing.Color.Orchid;
			this.peopleLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.peopleLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.peopleLink.Location = new System.Drawing.Point(8, 70);
			this.peopleLink.Name = "peopleLink";
			this.peopleLink.NavMode = true;
			this.peopleLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.peopleLink.Selected = false;
			this.peopleLink.Size = new System.Drawing.Size(184, 20);
			this.peopleLink.StrictColors = false;
			this.peopleLink.TabIndex = 2;
			this.peopleLink.TabStop = true;
			this.peopleLink.Tag = "People & Body";
			this.peopleLink.Text = "People && Body";
			this.peopleLink.ThemedBack = null;
			this.peopleLink.ThemedFore = null;
			this.peopleLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.peopleLink.Click += new System.EventHandler(this.DoCategoryLinkClicked);
			// 
			// animalsLink
			// 
			this.animalsLink.Active = false;
			this.animalsLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.animalsLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.animalsLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.animalsLink.HoverColor = System.Drawing.Color.Orchid;
			this.animalsLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.animalsLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.animalsLink.Location = new System.Drawing.Point(8, 100);
			this.animalsLink.Name = "animalsLink";
			this.animalsLink.NavMode = true;
			this.animalsLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.animalsLink.Selected = false;
			this.animalsLink.Size = new System.Drawing.Size(184, 20);
			this.animalsLink.StrictColors = false;
			this.animalsLink.TabIndex = 3;
			this.animalsLink.TabStop = true;
			this.animalsLink.Tag = "Animals & Nature";
			this.animalsLink.Text = "Animals && Nature";
			this.animalsLink.ThemedBack = null;
			this.animalsLink.ThemedFore = null;
			this.animalsLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.animalsLink.Click += new System.EventHandler(this.DoCategoryLinkClicked);
			// 
			// foodLink
			// 
			this.foodLink.Active = false;
			this.foodLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.foodLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.foodLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.foodLink.HoverColor = System.Drawing.Color.Orchid;
			this.foodLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.foodLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.foodLink.Location = new System.Drawing.Point(8, 130);
			this.foodLink.Name = "foodLink";
			this.foodLink.NavMode = true;
			this.foodLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.foodLink.Selected = false;
			this.foodLink.Size = new System.Drawing.Size(184, 20);
			this.foodLink.StrictColors = false;
			this.foodLink.TabIndex = 4;
			this.foodLink.TabStop = true;
			this.foodLink.Tag = "Food & Drink";
			this.foodLink.Text = "Food && Drink";
			this.foodLink.ThemedBack = null;
			this.foodLink.ThemedFore = null;
			this.foodLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.foodLink.Click += new System.EventHandler(this.DoCategoryLinkClicked);
			// 
			// travelLink
			// 
			this.travelLink.Active = false;
			this.travelLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.travelLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.travelLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.travelLink.HoverColor = System.Drawing.Color.Orchid;
			this.travelLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.travelLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.travelLink.Location = new System.Drawing.Point(8, 160);
			this.travelLink.Name = "travelLink";
			this.travelLink.NavMode = true;
			this.travelLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.travelLink.Selected = false;
			this.travelLink.Size = new System.Drawing.Size(184, 20);
			this.travelLink.StrictColors = false;
			this.travelLink.TabIndex = 5;
			this.travelLink.TabStop = true;
			this.travelLink.Tag = "Travel & Places";
			this.travelLink.Text = "Travel && Places";
			this.travelLink.ThemedBack = null;
			this.travelLink.ThemedFore = null;
			this.travelLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.travelLink.Click += new System.EventHandler(this.DoCategoryLinkClicked);
			// 
			// activitiesLink
			// 
			this.activitiesLink.Active = false;
			this.activitiesLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.activitiesLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.activitiesLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.activitiesLink.HoverColor = System.Drawing.Color.Orchid;
			this.activitiesLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.activitiesLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.activitiesLink.Location = new System.Drawing.Point(8, 190);
			this.activitiesLink.Name = "activitiesLink";
			this.activitiesLink.NavMode = true;
			this.activitiesLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.activitiesLink.Selected = false;
			this.activitiesLink.Size = new System.Drawing.Size(184, 20);
			this.activitiesLink.StrictColors = false;
			this.activitiesLink.TabIndex = 6;
			this.activitiesLink.TabStop = true;
			this.activitiesLink.Tag = "Activities";
			this.activitiesLink.Text = "Activities";
			this.activitiesLink.ThemedBack = null;
			this.activitiesLink.ThemedFore = null;
			this.activitiesLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.activitiesLink.Click += new System.EventHandler(this.DoCategoryLinkClicked);
			// 
			// objectsLink
			// 
			this.objectsLink.Active = false;
			this.objectsLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.objectsLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.objectsLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.objectsLink.HoverColor = System.Drawing.Color.Orchid;
			this.objectsLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.objectsLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.objectsLink.Location = new System.Drawing.Point(8, 220);
			this.objectsLink.Name = "objectsLink";
			this.objectsLink.NavMode = true;
			this.objectsLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.objectsLink.Selected = false;
			this.objectsLink.Size = new System.Drawing.Size(184, 20);
			this.objectsLink.StrictColors = false;
			this.objectsLink.TabIndex = 7;
			this.objectsLink.TabStop = true;
			this.objectsLink.Tag = "Objects";
			this.objectsLink.Text = "Objects";
			this.objectsLink.ThemedBack = null;
			this.objectsLink.ThemedFore = null;
			this.objectsLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.objectsLink.Click += new System.EventHandler(this.DoCategoryLinkClicked);
			// 
			// symbolsLink
			// 
			this.symbolsLink.Active = false;
			this.symbolsLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.symbolsLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.symbolsLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.symbolsLink.HoverColor = System.Drawing.Color.Orchid;
			this.symbolsLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.symbolsLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.symbolsLink.Location = new System.Drawing.Point(8, 250);
			this.symbolsLink.Name = "symbolsLink";
			this.symbolsLink.NavMode = true;
			this.symbolsLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.symbolsLink.Selected = false;
			this.symbolsLink.Size = new System.Drawing.Size(184, 20);
			this.symbolsLink.StrictColors = false;
			this.symbolsLink.TabIndex = 8;
			this.symbolsLink.TabStop = true;
			this.symbolsLink.Tag = "Symbols";
			this.symbolsLink.Text = "Symbols";
			this.symbolsLink.ThemedBack = null;
			this.symbolsLink.ThemedFore = null;
			this.symbolsLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.symbolsLink.Click += new System.EventHandler(this.DoCategoryLinkClicked);
			// 
			// flagsLink
			// 
			this.flagsLink.Active = false;
			this.flagsLink.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.flagsLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.flagsLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.flagsLink.HoverColor = System.Drawing.Color.Orchid;
			this.flagsLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.flagsLink.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.flagsLink.Location = new System.Drawing.Point(8, 280);
			this.flagsLink.Name = "flagsLink";
			this.flagsLink.NavMode = true;
			this.flagsLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.flagsLink.Selected = false;
			this.flagsLink.Size = new System.Drawing.Size(184, 20);
			this.flagsLink.StrictColors = false;
			this.flagsLink.TabIndex = 9;
			this.flagsLink.TabStop = true;
			this.flagsLink.Tag = "Flags";
			this.flagsLink.Text = "Flags";
			this.flagsLink.ThemedBack = null;
			this.flagsLink.ThemedFore = null;
			this.flagsLink.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.flagsLink.Click += new System.EventHandler(this.DoCategoryLinkClicked);
			// 
			// introLabel
			// 
			this.introLabel.AutoSize = true;
			this.introLabel.Location = new System.Drawing.Point(14, 26);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(253, 20);
			this.introLabel.TabIndex = 4;
			this.introLabel.Text = "Select one or more emojis to insert";
			// 
			// EmojiDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(458, 752);
			this.Controls.Add(this.introLabel);
			this.Controls.Add(this.tabs);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(400, 600);
			this.Name = "EmojiDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Insert Emojis";
			this.tabs.ResumeLayout(false);
			this.listTab.ResumeLayout(false);
			this.gridTab.ResumeLayout(false);
			this.searchPanel.ResumeLayout(false);
			this.searchPanel.PerformLayout();
			this.categoryPanel.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreButton cancelButton;
		private UI.MoreButton okButton;
		private UI.MoreTabControl tabs;
		private System.Windows.Forms.TabPage listTab;
		private UI.MoreListView emojiBox;
		private System.Windows.Forms.ColumnHeader nameColumn;
		private System.Windows.Forms.TabPage gridTab;
		private System.Windows.Forms.Panel searchPanel;
		private System.Windows.Forms.Label searchLabel;
		private UI.MoreTextBox searchBox;
		private UI.MoreIconListView gridBox;
		private UI.MorePanel categoryPanel;
		private UI.MoreLinkLabel generalLink;
		private UI.MoreLinkLabel smileysLink;
		private UI.MoreLinkLabel peopleLink;
		private UI.MoreLinkLabel animalsLink;
		private UI.MoreLinkLabel foodLink;
		private UI.MoreLinkLabel travelLink;
		private UI.MoreLinkLabel activitiesLink;
		private UI.MoreLinkLabel objectsLink;
		private UI.MoreLinkLabel symbolsLink;
		private UI.MoreLinkLabel flagsLink;
		private System.Windows.Forms.Label introLabel;
	}
}
