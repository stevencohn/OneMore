namespace River.OneMoreAddIn.Commands
{
	partial class SearchDialog
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
			if (disposing)
			{
				source?.Dispose();
				components?.Dispose();
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SearchDialog));
			this.introLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.moveButton = new River.OneMoreAddIn.UI.MoreButton();
			this.copyButton = new River.OneMoreAddIn.UI.MoreButton();
			this.findLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.findBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.searchButton = new River.OneMoreAddIn.UI.MoreButton();
			this.resultsView = new River.OneMoreAddIn.Commands.SearchResultsCardView();
			this.morePanel1 = new River.OneMoreAddIn.UI.MorePanel();
			this.prevButton = new River.OneMoreAddIn.UI.MoreButton();
			this.nextButton = new River.OneMoreAddIn.UI.MoreButton();
			this.pageLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.morePanel2 = new River.OneMoreAddIn.UI.MorePanel();
			this.optionsPanel = new River.OneMoreAddIn.UI.MorePanel();
			this.dateSelector = new River.OneMoreAddIn.UI.MoreComboBox();
			this.dateTimePicker = new System.Windows.Forms.DateTimePicker();
			this.regBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.matchBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.scopeBox = new River.OneMoreAddIn.UI.MoreComboBox();
			this.resultsHeaderPanel = new River.OneMoreAddIn.UI.MorePanel();
			this.selectAllLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.barLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.clearAllLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.includeTocBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.morePanel1.SuspendLayout();
			this.morePanel2.SuspendLayout();
			this.optionsPanel.SuspendLayout();
			this.resultsHeaderPanel.SuspendLayout();
			this.SuspendLayout();
			//
			// introLabel
			//
			this.introLabel.AutoSize = true;
			this.introLabel.Location = new System.Drawing.Point(4, 3);
			this.introLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 15);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(603, 20);
			this.introLabel.TabIndex = 9;
			this.introLabel.Text = "Enter the search criteria, optionally including AND, OR, NOT, quotes and parenthe" +
    "sis\r\n";
			this.introLabel.ThemedBack = null;
			this.introLabel.ThemedFore = null;
			//
			// cancelButton
			//
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(663, 16);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 15);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(112, 35);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 4;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.Nevermind);
			//
			// moveButton
			//
			this.moveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.moveButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.moveButton.Enabled = false;
			this.moveButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.moveButton.ImageOver = null;
			this.moveButton.Location = new System.Drawing.Point(543, 16);
			this.moveButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 15);
			this.moveButton.Name = "moveButton";
			this.moveButton.ShowBorder = true;
			this.moveButton.Size = new System.Drawing.Size(112, 35);
			this.moveButton.StylizeImage = false;
			this.moveButton.TabIndex = 3;
			this.moveButton.Text = "Move";
			this.moveButton.ThemedBack = null;
			this.moveButton.ThemedFore = null;
			this.moveButton.UseVisualStyleBackColor = true;
			this.moveButton.Visible = false;
			this.moveButton.Click += new System.EventHandler(this.MovePressed);
			//
			// copyButton
			//
			this.copyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.copyButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.copyButton.Enabled = false;
			this.copyButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.copyButton.ImageOver = null;
			this.copyButton.Location = new System.Drawing.Point(423, 16);
			this.copyButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 15);
			this.copyButton.Name = "copyButton";
			this.copyButton.ShowBorder = true;
			this.copyButton.Size = new System.Drawing.Size(112, 35);
			this.copyButton.StylizeImage = false;
			this.copyButton.TabIndex = 2;
			this.copyButton.Text = "Copy";
			this.copyButton.ThemedBack = null;
			this.copyButton.ThemedFore = null;
			this.copyButton.UseVisualStyleBackColor = true;
			this.copyButton.Visible = false;
			this.copyButton.Click += new System.EventHandler(this.CopyPressed);
			//
			// findLabel
			//
			this.findLabel.AutoSize = true;
			this.findLabel.Location = new System.Drawing.Point(7, 11);
			this.findLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.findLabel.Name = "findLabel";
			this.findLabel.Size = new System.Drawing.Size(40, 20);
			this.findLabel.TabIndex = 11;
			this.findLabel.Text = "Find";
			this.findLabel.ThemedBack = null;
			this.findLabel.ThemedFore = null;
			//
			// findBox
			//
			this.findBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.findBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.findBox.Location = new System.Drawing.Point(58, 9);
			this.findBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.findBox.Name = "findBox";
			this.findBox.ProcessEnterKey = false;
			this.findBox.Size = new System.Drawing.Size(451, 26);
			this.findBox.TabIndex = 0;
			this.findBox.ThemedBack = null;
			this.findBox.ThemedFore = null;
			this.findBox.TextChanged += new System.EventHandler(this.ChangedText);
			this.findBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SearchOnKeydown);
			//
			// searchButton
			//
			this.searchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.searchButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.searchButton.Enabled = false;
			this.searchButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.searchButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Search;
			this.searchButton.ImageOver = null;
			this.searchButton.Location = new System.Drawing.Point(706, 9);
			this.searchButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 15);
			this.searchButton.Name = "searchButton";
			this.searchButton.ShowBorder = true;
			this.searchButton.Size = new System.Drawing.Size(68, 32);
			this.searchButton.StylizeImage = true;
			this.searchButton.TabIndex = 2;
			this.searchButton.ThemedBack = null;
			this.searchButton.ThemedFore = null;
			this.searchButton.UseVisualStyleBackColor = true;
			this.searchButton.Click += new System.EventHandler(this.Search);
			//
			// resultsView
			//
			this.resultsView.AutoScroll = true;
			this.resultsView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(171)))), ((int)(((byte)(171)))));
			this.resultsView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.resultsView.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.resultsView.Location = new System.Drawing.Point(15, 237);
			this.resultsView.Name = "resultsView";
			this.resultsView.Size = new System.Drawing.Size(782, 323);
			this.resultsView.TabIndex = 0;
			this.resultsView.TabStop = false;
			this.resultsView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.HandleNavKey);
			//
			// morePanel1
			//
			this.morePanel1.BottomBorderColor = System.Drawing.SystemColors.ActiveBorder;
			this.morePanel1.BottomBorderSize = 0;
			this.morePanel1.Controls.Add(this.prevButton);
			this.morePanel1.Controls.Add(this.nextButton);
			this.morePanel1.Controls.Add(this.pageLabel);
			this.morePanel1.Controls.Add(this.progressBar);
			this.morePanel1.Controls.Add(this.copyButton);
			this.morePanel1.Controls.Add(this.moveButton);
			this.morePanel1.Controls.Add(this.cancelButton);
			this.morePanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.morePanel1.Location = new System.Drawing.Point(15, 560);
			this.morePanel1.Name = "morePanel1";
			this.morePanel1.Padding = new System.Windows.Forms.Padding(3);
			this.morePanel1.Size = new System.Drawing.Size(782, 66);
			this.morePanel1.TabIndex = 1;
			this.morePanel1.ThemedBack = null;
			this.morePanel1.ThemedFore = null;
			this.morePanel1.TopBorderColor = System.Drawing.SystemColors.Control;
			this.morePanel1.TopBorderSize = 0;
			//
			// prevButton
			//
			this.prevButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.prevButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.prevButton.Image = global::River.OneMoreAddIn.Properties.Resources.UpArrow;
			this.prevButton.ImageOver = null;
			this.prevButton.Location = new System.Drawing.Point(93, 18);
			this.prevButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 15);
			this.prevButton.Name = "prevButton";
			this.prevButton.ShowBorder = true;
			this.prevButton.Size = new System.Drawing.Size(60, 30);
			this.prevButton.StylizeImage = true;
			this.prevButton.TabIndex = 1;
			this.prevButton.ThemedBack = null;
			this.prevButton.ThemedFore = null;
			this.prevButton.UseVisualStyleBackColor = true;
			this.prevButton.Visible = false;
			this.prevButton.Click += new System.EventHandler(this.MoveToPreviousSelection);
			//
			// nextButton
			//
			this.nextButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.nextButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.nextButton.Image = global::River.OneMoreAddIn.Properties.Resources.DownArrow;
			this.nextButton.ImageOver = null;
			this.nextButton.Location = new System.Drawing.Point(25, 18);
			this.nextButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 15);
			this.nextButton.Name = "nextButton";
			this.nextButton.ShowBorder = true;
			this.nextButton.Size = new System.Drawing.Size(60, 30);
			this.nextButton.StylizeImage = true;
			this.nextButton.TabIndex = 0;
			this.nextButton.ThemedBack = null;
			this.nextButton.ThemedFore = null;
			this.nextButton.UseVisualStyleBackColor = true;
			this.nextButton.Visible = false;
			this.nextButton.Click += new System.EventHandler(this.MoveToNextSelection);
			//
			// pageLabel
			//
			this.pageLabel.AutoSize = true;
			this.pageLabel.Location = new System.Drawing.Point(21, 8);
			this.pageLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 15);
			this.pageLabel.Name = "pageLabel";
			this.pageLabel.Size = new System.Drawing.Size(45, 20);
			this.pageLabel.TabIndex = 10;
			this.pageLabel.Text = "page";
			this.pageLabel.ThemedBack = null;
			this.pageLabel.ThemedFore = null;
			//
			// progressBar
			//
			this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar.Location = new System.Drawing.Point(25, 32);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(615, 16);
			this.progressBar.Step = 1;
			this.progressBar.TabIndex = 4;
			this.progressBar.Visible = false;
			//
			// morePanel2
			//
			this.morePanel2.BottomBorderColor = System.Drawing.SystemColors.ActiveBorder;
			this.morePanel2.BottomBorderSize = 0;
			this.morePanel2.Controls.Add(this.introLabel);
			this.morePanel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.morePanel2.Location = new System.Drawing.Point(15, 15);
			this.morePanel2.Margin = new System.Windows.Forms.Padding(0);
			this.morePanel2.Name = "morePanel2";
			this.morePanel2.Padding = new System.Windows.Forms.Padding(3);
			this.morePanel2.Size = new System.Drawing.Size(782, 46);
			this.morePanel2.TabIndex = 13;
			this.morePanel2.ThemedBack = null;
			this.morePanel2.ThemedFore = null;
			this.morePanel2.TopBorderColor = System.Drawing.SystemColors.Control;
			this.morePanel2.TopBorderSize = 0;
			//
			// optionsPanel
			//
			this.optionsPanel.BottomBorderColor = System.Drawing.SystemColors.ActiveBorder;
			this.optionsPanel.BottomBorderSize = 0;
			this.optionsPanel.Controls.Add(this.includeTocBox);
			this.optionsPanel.Controls.Add(this.dateSelector);
			this.optionsPanel.Controls.Add(this.dateTimePicker);
			this.optionsPanel.Controls.Add(this.regBox);
			this.optionsPanel.Controls.Add(this.matchBox);
			this.optionsPanel.Controls.Add(this.scopeBox);
			this.optionsPanel.Controls.Add(this.findBox);
			this.optionsPanel.Controls.Add(this.findLabel);
			this.optionsPanel.Controls.Add(this.searchButton);
			this.optionsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.optionsPanel.Location = new System.Drawing.Point(15, 61);
			this.optionsPanel.Margin = new System.Windows.Forms.Padding(0);
			this.optionsPanel.Name = "optionsPanel";
			this.optionsPanel.Padding = new System.Windows.Forms.Padding(3);
			this.optionsPanel.Size = new System.Drawing.Size(782, 146);
			this.optionsPanel.TabIndex = 0;
			this.optionsPanel.ThemedBack = null;
			this.optionsPanel.ThemedFore = null;
			this.optionsPanel.TopBorderColor = System.Drawing.SystemColors.Control;
			this.optionsPanel.TopBorderSize = 0;
			//
			// dateSelector
			//
			this.dateSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.dateSelector.Enabled = false;
			this.dateSelector.FormattingEnabled = true;
			this.dateSelector.Items.AddRange(new object[] {
            "Any date",
            "Created after",
            "Created before",
            "Modified after",
            "Modified before"});
			this.dateSelector.Location = new System.Drawing.Point(275, 44);
			this.dateSelector.Name = "dateSelector";
			this.dateSelector.Size = new System.Drawing.Size(234, 27);
			this.dateSelector.TabIndex = 5;
			this.dateSelector.ThemedBack = null;
			this.dateSelector.ThemedFore = null;
			this.dateSelector.SelectedIndexChanged += new System.EventHandler(this.ChangeDateSelector);
			//
			// dateTimePicker
			//
			this.dateTimePicker.Enabled = false;
			this.dateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateTimePicker.Location = new System.Drawing.Point(516, 44);
			this.dateTimePicker.MaxDate = new System.DateTime(3000, 12, 31, 0, 0, 0, 0);
			this.dateTimePicker.MinDate = new System.DateTime(1990, 1, 1, 0, 0, 0, 0);
			this.dateTimePicker.Name = "dateTimePicker";
			this.dateTimePicker.Size = new System.Drawing.Size(183, 26);
			this.dateTimePicker.TabIndex = 6;
			//
			// regBox
			//
			this.regBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.regBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.regBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.regBox.Location = new System.Drawing.Point(58, 72);
			this.regBox.Margin = new System.Windows.Forms.Padding(4, 1, 4, 3);
			this.regBox.Name = "regBox";
			this.regBox.Size = new System.Drawing.Size(213, 25);
			this.regBox.StylizeImage = false;
			this.regBox.TabIndex = 4;
			this.regBox.Text = "Use regular expressions";
			this.regBox.ThemedBack = null;
			this.regBox.ThemedFore = null;
			this.regBox.UseVisualStyleBackColor = true;
			this.regBox.CheckedChanged += new System.EventHandler(this.TogglerRegBox);
			//
			// matchBox
			//
			this.matchBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.matchBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.matchBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.matchBox.Location = new System.Drawing.Point(58, 44);
			this.matchBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 3);
			this.matchBox.Name = "matchBox";
			this.matchBox.Size = new System.Drawing.Size(119, 25);
			this.matchBox.StylizeImage = false;
			this.matchBox.TabIndex = 3;
			this.matchBox.Text = "Match case";
			this.matchBox.ThemedBack = null;
			this.matchBox.ThemedFore = null;
			this.matchBox.UseVisualStyleBackColor = true;
			//
			// scopeBox
			//
			this.scopeBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.scopeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.scopeBox.FormattingEnabled = true;
			this.scopeBox.Items.AddRange(new object[] {
            "In this notebook",
            "In this section",
            "On this page"});
			this.scopeBox.Location = new System.Drawing.Point(516, 9);
			this.scopeBox.Name = "scopeBox";
			this.scopeBox.Size = new System.Drawing.Size(183, 27);
			this.scopeBox.TabIndex = 1;
			this.scopeBox.ThemedBack = null;
			this.scopeBox.ThemedFore = null;
			this.scopeBox.SelectedIndexChanged += new System.EventHandler(this.ChangeScope);
			//
			// resultsHeaderPanel
			//
			this.resultsHeaderPanel.BottomBorderColor = System.Drawing.SystemColors.ActiveBorder;
			this.resultsHeaderPanel.BottomBorderSize = 0;
			this.resultsHeaderPanel.Controls.Add(this.selectAllLink);
			this.resultsHeaderPanel.Controls.Add(this.barLabel);
			this.resultsHeaderPanel.Controls.Add(this.clearAllLink);
			this.resultsHeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.resultsHeaderPanel.Location = new System.Drawing.Point(15, 207);
			this.resultsHeaderPanel.Margin = new System.Windows.Forms.Padding(0);
			this.resultsHeaderPanel.Name = "resultsHeaderPanel";
			this.resultsHeaderPanel.Padding = new System.Windows.Forms.Padding(3);
			this.resultsHeaderPanel.Size = new System.Drawing.Size(782, 30);
			this.resultsHeaderPanel.TabIndex = 14;
			this.resultsHeaderPanel.ThemedBack = null;
			this.resultsHeaderPanel.ThemedFore = null;
			this.resultsHeaderPanel.TopBorderColor = System.Drawing.SystemColors.Control;
			this.resultsHeaderPanel.TopBorderSize = 0;
			this.resultsHeaderPanel.Visible = false;
			//
			// selectAllLink
			//
			this.selectAllLink.ActiveLinkColor = System.Drawing.Color.Orchid;
			this.selectAllLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.selectAllLink.AutoSize = true;
			this.selectAllLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.selectAllLink.HoverColor = System.Drawing.Color.MediumOrchid;
			this.selectAllLink.LinkColor = System.Drawing.Color.MediumOrchid;
			this.selectAllLink.Location = new System.Drawing.Point(567, 7);
			this.selectAllLink.Name = "selectAllLink";
			this.selectAllLink.Selected = false;
			this.selectAllLink.Size = new System.Drawing.Size(73, 20);
			this.selectAllLink.StrictColors = false;
			this.selectAllLink.TabIndex = 0;
			this.selectAllLink.TabStop = true;
			this.selectAllLink.Text = "Select all";
			this.selectAllLink.ThemedBack = null;
			this.selectAllLink.ThemedFore = null;
			this.selectAllLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.selectAllLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.SelectAll);
			//
			// barLabel
			//
			this.barLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.barLabel.AutoSize = true;
			this.barLabel.Location = new System.Drawing.Point(648, 7);
			this.barLabel.Name = "barLabel";
			this.barLabel.Size = new System.Drawing.Size(14, 20);
			this.barLabel.TabIndex = 2;
			this.barLabel.Text = "|";
			this.barLabel.ThemedBack = null;
			this.barLabel.ThemedFore = null;
			//
			// clearAllLink
			//
			this.clearAllLink.ActiveLinkColor = System.Drawing.Color.Orchid;
			this.clearAllLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.clearAllLink.AutoSize = true;
			this.clearAllLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.clearAllLink.HoverColor = System.Drawing.Color.MediumOrchid;
			this.clearAllLink.LinkColor = System.Drawing.Color.MediumOrchid;
			this.clearAllLink.Location = new System.Drawing.Point(662, 7);
			this.clearAllLink.Name = "clearAllLink";
			this.clearAllLink.Selected = false;
			this.clearAllLink.Size = new System.Drawing.Size(113, 20);
			this.clearAllLink.StrictColors = false;
			this.clearAllLink.TabIndex = 1;
			this.clearAllLink.TabStop = true;
			this.clearAllLink.Text = "Clear selection";
			this.clearAllLink.ThemedBack = null;
			this.clearAllLink.ThemedFore = null;
			this.clearAllLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.clearAllLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ClearSelection);
			//
			// includeTocBox
			//
			this.includeTocBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.includeTocBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.includeTocBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.includeTocBox.Location = new System.Drawing.Point(58, 101);
			this.includeTocBox.Margin = new System.Windows.Forms.Padding(4, 1, 4, 3);
			this.includeTocBox.Name = "includeTocBox";
			this.includeTocBox.Size = new System.Drawing.Size(261, 25);
			this.includeTocBox.StylizeImage = false;
			this.includeTocBox.TabIndex = 12;
			this.includeTocBox.Text = "Search text in table of contents";
			this.includeTocBox.ThemedBack = null;
			this.includeTocBox.ThemedFore = null;
			this.includeTocBox.UseVisualStyleBackColor = true;
			//
			// SearchDialog
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.ClientSize = new System.Drawing.Size(1122, 723);
			this.Controls.Add(this.resultsView);
			this.Controls.Add(this.resultsHeaderPanel);
			this.Controls.Add(this.optionsPanel);
			this.Controls.Add(this.morePanel2);
			this.Controls.Add(this.morePanel1);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(720, 391);
			this.Name = "SearchDialog";
			this.Padding = new System.Windows.Forms.Padding(15, 15, 15, 10);
			this.Text = "Search";
			this.morePanel1.ResumeLayout(false);
			this.morePanel1.PerformLayout();
			this.morePanel2.ResumeLayout(false);
			this.morePanel2.PerformLayout();
			this.optionsPanel.ResumeLayout(false);
			this.optionsPanel.PerformLayout();
			this.resultsHeaderPanel.ResumeLayout(false);
			this.resultsHeaderPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreLabel introLabel;
		private UI.MoreButton cancelButton;
		private UI.MoreButton moveButton;
		private UI.MoreButton copyButton;
		private UI.MoreLabel findLabel;
		private UI.MoreTextBox findBox;
		private UI.MoreButton searchButton;
		private SearchResultsCardView resultsView;
		private UI.MorePanel morePanel1;
		private UI.MorePanel morePanel2;
		private UI.MorePanel optionsPanel;
		private UI.MorePanel resultsHeaderPanel;
		private UI.MoreLinkLabel selectAllLink;
		private UI.MoreLabel barLabel;
		private UI.MoreLinkLabel clearAllLink;
		private System.Windows.Forms.ProgressBar progressBar;
		private UI.MoreLabel pageLabel;
		private UI.MoreComboBox scopeBox;
		private UI.MoreCheckBox regBox;
		private UI.MoreCheckBox matchBox;
		private UI.MoreButton prevButton;
		private UI.MoreButton nextButton;
		private UI.MoreComboBox dateSelector;
		private System.Windows.Forms.DateTimePicker dateTimePicker;
		private UI.MoreCheckBox includeTocBox;
	}
}
