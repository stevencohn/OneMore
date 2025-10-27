namespace River.OneMoreAddIn.Commands
{
	partial class SearchDialogTextControl
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
				resultsView?.Dispose();
				source?.Dispose();
				components?.Dispose();
			}

			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.introLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.findLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.findBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.searchButton = new River.OneMoreAddIn.UI.MoreButton();
			this.resultsView = new River.OneMoreAddIn.UI.MoreListView();
			this.hitColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.morePanel1 = new River.OneMoreAddIn.UI.MorePanel();
			this.prevButton = new River.OneMoreAddIn.UI.MoreButton();
			this.nextButton = new River.OneMoreAddIn.UI.MoreButton();
			this.pageLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.morePanel2 = new River.OneMoreAddIn.UI.MorePanel();
			this.optionsPanel = new River.OneMoreAddIn.UI.MorePanel();
			this.dateSelector = new System.Windows.Forms.ComboBox();
			this.dateTimePicker = new System.Windows.Forms.DateTimePicker();
			this.regBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.matchBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.scopeBox = new System.Windows.Forms.ComboBox();
			this.morePanel1.SuspendLayout();
			this.morePanel2.SuspendLayout();
			this.optionsPanel.SuspendLayout();
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
			this.cancelButton.TabIndex = 2;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.Nevermind);
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
			this.searchButton.Location = new System.Drawing.Point(515, 8);
			this.searchButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 15);
			this.searchButton.Name = "searchButton";
			this.searchButton.ShowBorder = true;
			this.searchButton.Size = new System.Drawing.Size(68, 32);
			this.searchButton.StylizeImage = true;
			this.searchButton.TabIndex = 1;
			this.searchButton.ThemedBack = null;
			this.searchButton.ThemedFore = null;
			this.searchButton.UseVisualStyleBackColor = true;
			this.searchButton.Click += new System.EventHandler(this.Search);
			// 
			// resultsView
			// 
			this.resultsView.AllowItemReorder = false;
			this.resultsView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.hitColumn});
			this.resultsView.ControlPadding = 2;
			this.resultsView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.resultsView.FullRowSelect = true;
			this.resultsView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.resultsView.HideSelection = false;
			this.resultsView.HighlightBackground = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(193)))), ((int)(((byte)(255)))));
			this.resultsView.HighlightForeground = System.Drawing.SystemColors.HighlightText;
			this.resultsView.Location = new System.Drawing.Point(15, 207);
			this.resultsView.MultiSelect = false;
			this.resultsView.Name = "resultsView";
			this.resultsView.RowHeight = 24;
			this.resultsView.ShowGroups = false;
			this.resultsView.Size = new System.Drawing.Size(782, 353);
			this.resultsView.SortedBackground = System.Drawing.SystemColors.Window;
			this.resultsView.TabIndex = 0;
			this.resultsView.TabStop = false;
			this.resultsView.UseCompatibleStateImageBehavior = false;
			this.resultsView.View = System.Windows.Forms.View.Details;
			this.resultsView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.HandleNavKey);
			this.resultsView.Resize += new System.EventHandler(this.ResizeResultsView);
			// 
			// hitColumn
			// 
			this.hitColumn.Text = "Hit";
			// 
			// morePanel1
			// 
			this.morePanel1.BottomBorderColor = System.Drawing.SystemColors.ActiveBorder;
			this.morePanel1.BottomBorderSize = 0;
			this.morePanel1.Controls.Add(this.prevButton);
			this.morePanel1.Controls.Add(this.nextButton);
			this.morePanel1.Controls.Add(this.pageLabel);
			this.morePanel1.Controls.Add(this.progressBar);
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
			this.progressBar.Size = new System.Drawing.Size(549, 16);
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
			this.dateSelector.Location = new System.Drawing.Point(58, 103);
			this.dateSelector.Name = "dateSelector";
			this.dateSelector.Size = new System.Drawing.Size(234, 28);
			this.dateSelector.TabIndex = 5;
			this.dateSelector.SelectedIndexChanged += new System.EventHandler(this.ChangeDateSelector);
			// 
			// dateTimePicker
			// 
			this.dateTimePicker.Enabled = false;
			this.dateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateTimePicker.Location = new System.Drawing.Point(298, 103);
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
			this.scopeBox.Location = new System.Drawing.Point(592, 9);
			this.scopeBox.Name = "scopeBox";
			this.scopeBox.Size = new System.Drawing.Size(183, 28);
			this.scopeBox.TabIndex = 2;
			this.scopeBox.SelectedIndexChanged += new System.EventHandler(this.ChangeScope);
			// 
			// SearchDialogTextControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.resultsView);
			this.Controls.Add(this.optionsPanel);
			this.Controls.Add(this.morePanel2);
			this.Controls.Add(this.morePanel1);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "SearchDialogTextControl";
			this.Padding = new System.Windows.Forms.Padding(15, 15, 15, 10);
			this.Size = new System.Drawing.Size(812, 636);
			this.morePanel1.ResumeLayout(false);
			this.morePanel1.PerformLayout();
			this.morePanel2.ResumeLayout(false);
			this.morePanel2.PerformLayout();
			this.optionsPanel.ResumeLayout(false);
			this.optionsPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreLabel introLabel;
		private UI.MoreButton cancelButton;
		private UI.MoreLabel findLabel;
		private UI.MoreTextBox findBox;
		private UI.MoreButton searchButton;
		private UI.MoreListView resultsView;
		private System.Windows.Forms.ColumnHeader hitColumn;
		private UI.MorePanel morePanel1;
		private UI.MorePanel morePanel2;
		private UI.MorePanel optionsPanel;
		private System.Windows.Forms.ProgressBar progressBar;
		private UI.MoreLabel pageLabel;
		private System.Windows.Forms.ComboBox scopeBox;
		private UI.MoreCheckBox regBox;
		private UI.MoreCheckBox matchBox;
		private UI.MoreButton prevButton;
		private UI.MoreButton nextButton;
		private System.Windows.Forms.ComboBox dateSelector;
		private System.Windows.Forms.DateTimePicker dateTimePicker;
	}
}
