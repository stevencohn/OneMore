namespace River.OneMoreAddIn.Commands
{
	partial class SearchTitleDialog
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
				debounceTimer?.Dispose();
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SearchTitleDialog));
			this.introLabel = new River.OneMoreAddIn.UI.MoreMultilineLabel();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.indexButton = new River.OneMoreAddIn.UI.MoreButton();
			this.findLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.findBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.searchButton = new River.OneMoreAddIn.UI.MoreButton();
			this.resultsView = new River.OneMoreAddIn.Commands.SearchResultsCardView();
			this.morePanel1 = new River.OneMoreAddIn.UI.MorePanel();
			this.morePanel2 = new River.OneMoreAddIn.UI.MorePanel();
			this.queryPanel = new River.OneMoreAddIn.UI.MorePanel();
			this.resultsHeaderPanel = new River.OneMoreAddIn.UI.MorePanel();
			this.selectAllLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.barLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.clearAllLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.morePanel1.SuspendLayout();
			this.morePanel2.SuspendLayout();
			this.queryPanel.SuspendLayout();
			this.resultsHeaderPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// introLabel
			// 
			this.introLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.introLabel.AutoSize = true;
			this.introLabel.Location = new System.Drawing.Point(4, 3);
			this.introLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 15);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(779, 71);
			this.introLabel.TabIndex = 0;
			this.introLabel.Text = "Enter a page title query. Prefix with \">\" to sort by most recently modified; use " +
    "\"nb:\\name\" to scope to a notebook (\"nb:\\*\" for all), and \"#tag\" to filter by has" +
    "htag.";
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
			// indexButton
			// 
			this.indexButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.indexButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.indexButton.Enabled = false;
			this.indexButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.indexButton.ImageOver = null;
			this.indexButton.Location = new System.Drawing.Point(547, 16);
			this.indexButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 15);
			this.indexButton.Name = "indexButton";
			this.indexButton.ShowBorder = true;
			this.indexButton.Size = new System.Drawing.Size(112, 35);
			this.indexButton.StylizeImage = false;
			this.indexButton.TabIndex = 3;
			this.indexButton.Text = "Index";
			this.indexButton.ThemedBack = null;
			this.indexButton.ThemedFore = null;
			this.indexButton.UseVisualStyleBackColor = true;
			this.indexButton.Visible = false;
			this.indexButton.Click += new System.EventHandler(this.IndexPressed);
			// 
			// findLabel
			// 
			this.findLabel.AutoSize = true;
			this.findLabel.Location = new System.Drawing.Point(7, 11);
			this.findLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.findLabel.Name = "findLabel";
			this.findLabel.Size = new System.Drawing.Size(40, 20);
			this.findLabel.TabIndex = 0;
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
			this.findBox.Size = new System.Drawing.Size(658, 26);
			this.findBox.TabIndex = 1;
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
			this.searchButton.Location = new System.Drawing.Point(724, 9);
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
			this.resultsView.Location = new System.Drawing.Point(15, 169);
			this.resultsView.Name = "resultsView";
			this.resultsView.Size = new System.Drawing.Size(790, 321);
			this.resultsView.TabIndex = 3;
			this.resultsView.TabStop = false;
			// 
			// morePanel1
			// 
			this.morePanel1.BottomBorderColor = System.Drawing.SystemColors.ActiveBorder;
			this.morePanel1.BottomBorderSize = 0;
			this.morePanel1.Controls.Add(this.indexButton);
			this.morePanel1.Controls.Add(this.cancelButton);
			this.morePanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.morePanel1.Location = new System.Drawing.Point(15, 490);
			this.morePanel1.Name = "morePanel1";
			this.morePanel1.Padding = new System.Windows.Forms.Padding(3);
			this.morePanel1.Size = new System.Drawing.Size(790, 66);
			this.morePanel1.TabIndex = 4;
			this.morePanel1.ThemedBack = null;
			this.morePanel1.ThemedFore = null;
			this.morePanel1.TopBorderColor = System.Drawing.SystemColors.Control;
			this.morePanel1.TopBorderSize = 0;
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
			this.morePanel2.Size = new System.Drawing.Size(790, 78);
			this.morePanel2.TabIndex = 0;
			this.morePanel2.ThemedBack = null;
			this.morePanel2.ThemedFore = null;
			this.morePanel2.TopBorderColor = System.Drawing.SystemColors.Control;
			this.morePanel2.TopBorderSize = 0;
			// 
			// queryPanel
			// 
			this.queryPanel.BottomBorderColor = System.Drawing.SystemColors.ActiveBorder;
			this.queryPanel.BottomBorderSize = 0;
			this.queryPanel.Controls.Add(this.findBox);
			this.queryPanel.Controls.Add(this.findLabel);
			this.queryPanel.Controls.Add(this.searchButton);
			this.queryPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.queryPanel.Location = new System.Drawing.Point(15, 93);
			this.queryPanel.Margin = new System.Windows.Forms.Padding(0);
			this.queryPanel.Name = "queryPanel";
			this.queryPanel.Padding = new System.Windows.Forms.Padding(3);
			this.queryPanel.Size = new System.Drawing.Size(790, 46);
			this.queryPanel.TabIndex = 1;
			this.queryPanel.ThemedBack = null;
			this.queryPanel.ThemedFore = null;
			this.queryPanel.TopBorderColor = System.Drawing.SystemColors.Control;
			this.queryPanel.TopBorderSize = 0;
			// 
			// resultsHeaderPanel
			// 
			this.resultsHeaderPanel.BottomBorderColor = System.Drawing.SystemColors.ActiveBorder;
			this.resultsHeaderPanel.BottomBorderSize = 0;
			this.resultsHeaderPanel.Controls.Add(this.selectAllLink);
			this.resultsHeaderPanel.Controls.Add(this.barLabel);
			this.resultsHeaderPanel.Controls.Add(this.clearAllLink);
			this.resultsHeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.resultsHeaderPanel.Location = new System.Drawing.Point(15, 139);
			this.resultsHeaderPanel.Margin = new System.Windows.Forms.Padding(0);
			this.resultsHeaderPanel.Name = "resultsHeaderPanel";
			this.resultsHeaderPanel.Padding = new System.Windows.Forms.Padding(3);
			this.resultsHeaderPanel.Size = new System.Drawing.Size(790, 30);
			this.resultsHeaderPanel.TabIndex = 2;
			this.resultsHeaderPanel.ThemedBack = null;
			this.resultsHeaderPanel.ThemedFore = null;
			this.resultsHeaderPanel.TopBorderColor = System.Drawing.SystemColors.Control;
			this.resultsHeaderPanel.TopBorderSize = 0;
			this.resultsHeaderPanel.Visible = false;
			// 
			// selectAllLink
			// 
			this.selectAllLink.Active = false;
			this.selectAllLink.ActiveLinkColor = System.Drawing.Color.Orchid;
			this.selectAllLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.selectAllLink.AutoSize = true;
			this.selectAllLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.selectAllLink.HoverColor = System.Drawing.Color.MediumOrchid;
			this.selectAllLink.LinkColor = System.Drawing.Color.MediumOrchid;
			this.selectAllLink.Location = new System.Drawing.Point(567, 7);
			this.selectAllLink.Name = "selectAllLink";
			this.selectAllLink.NavMode = false;
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
			this.barLabel.Location = new System.Drawing.Point(644, 7);
			this.barLabel.Name = "barLabel";
			this.barLabel.Size = new System.Drawing.Size(14, 20);
			this.barLabel.TabIndex = 2;
			this.barLabel.Text = "|";
			this.barLabel.ThemedBack = null;
			this.barLabel.ThemedFore = null;
			// 
			// clearAllLink
			// 
			this.clearAllLink.Active = false;
			this.clearAllLink.ActiveLinkColor = System.Drawing.Color.Orchid;
			this.clearAllLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.clearAllLink.AutoSize = true;
			this.clearAllLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.clearAllLink.HoverColor = System.Drawing.Color.MediumOrchid;
			this.clearAllLink.LinkColor = System.Drawing.Color.MediumOrchid;
			this.clearAllLink.Location = new System.Drawing.Point(662, 7);
			this.clearAllLink.Name = "clearAllLink";
			this.clearAllLink.NavMode = false;
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
			// SearchTitleDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.ClientSize = new System.Drawing.Size(820, 566);
			this.Controls.Add(this.resultsView);
			this.Controls.Add(this.resultsHeaderPanel);
			this.Controls.Add(this.queryPanel);
			this.Controls.Add(this.morePanel2);
			this.Controls.Add(this.morePanel1);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(620, 380);
			this.Name = "SearchTitleDialog";
			this.Padding = new System.Windows.Forms.Padding(15, 15, 15, 10);
			this.Text = "Search Page Titles";
			this.morePanel1.ResumeLayout(false);
			this.morePanel2.ResumeLayout(false);
			this.morePanel2.PerformLayout();
			this.queryPanel.ResumeLayout(false);
			this.queryPanel.PerformLayout();
			this.resultsHeaderPanel.ResumeLayout(false);
			this.resultsHeaderPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreMultilineLabel introLabel;
		private UI.MoreButton cancelButton;
		private UI.MoreButton indexButton;
		private UI.MoreLabel findLabel;
		private UI.MoreTextBox findBox;
		private UI.MoreButton searchButton;
		private SearchResultsCardView resultsView;
		private UI.MorePanel morePanel1;
		private UI.MorePanel morePanel2;
		private UI.MorePanel queryPanel;
		private UI.MorePanel resultsHeaderPanel;
		private UI.MoreLinkLabel selectAllLink;
		private UI.MoreLabel barLabel;
		private UI.MoreLinkLabel clearAllLink;
	}
}
