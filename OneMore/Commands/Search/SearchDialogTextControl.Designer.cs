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
			if (disposing && (components != null))
			{
				components.Dispose();
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
			this.textLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.findBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.searchButton = new River.OneMoreAddIn.UI.MoreButton();
			this.resultsView = new River.OneMoreAddIn.UI.MoreListView();
			this.hitColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.morePanel1 = new River.OneMoreAddIn.UI.MorePanel();
			this.morePanel2 = new River.OneMoreAddIn.UI.MorePanel();
			this.morePanel3 = new River.OneMoreAddIn.UI.MorePanel();
			this.morePanel1.SuspendLayout();
			this.morePanel2.SuspendLayout();
			this.morePanel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// introLabel
			// 
			this.introLabel.AutoSize = true;
			this.introLabel.Location = new System.Drawing.Point(7, 3);
			this.introLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 15);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(399, 20);
			this.introLabel.TabIndex = 9;
			this.introLabel.Text = "Enter the search criteria to find text on the current page";
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
			this.cancelButton.Location = new System.Drawing.Point(666, 16);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 15);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(112, 35);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 3;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.Nevermind);
			// 
			// textLabel
			// 
			this.textLabel.AutoSize = true;
			this.textLabel.Location = new System.Drawing.Point(7, 10);
			this.textLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.textLabel.Name = "textLabel";
			this.textLabel.Size = new System.Drawing.Size(43, 20);
			this.textLabel.TabIndex = 11;
			this.textLabel.Text = "Text:";
			this.textLabel.ThemedBack = null;
			this.textLabel.ThemedFore = null;
			// 
			// findBox
			// 
			this.findBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.findBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.findBox.Location = new System.Drawing.Point(58, 8);
			this.findBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.findBox.Name = "findBox";
			this.findBox.ProcessEnterKey = false;
			this.findBox.Size = new System.Drawing.Size(555, 26);
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
			this.searchButton.Location = new System.Drawing.Point(621, 4);
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
			this.resultsView.Location = new System.Drawing.Point(15, 125);
			this.resultsView.Name = "resultsView";
			this.resultsView.RowHeight = 24;
			this.resultsView.Size = new System.Drawing.Size(782, 435);
			this.resultsView.SortedBackground = System.Drawing.SystemColors.Window;
			this.resultsView.TabIndex = 2;
			this.resultsView.UseCompatibleStateImageBehavior = false;
			this.resultsView.View = System.Windows.Forms.View.Details;
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
			this.morePanel1.Controls.Add(this.cancelButton);
			this.morePanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.morePanel1.Location = new System.Drawing.Point(15, 560);
			this.morePanel1.Name = "morePanel1";
			this.morePanel1.Size = new System.Drawing.Size(782, 66);
			this.morePanel1.TabIndex = 12;
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
			this.morePanel2.Size = new System.Drawing.Size(782, 46);
			this.morePanel2.TabIndex = 13;
			this.morePanel2.ThemedBack = null;
			this.morePanel2.ThemedFore = null;
			this.morePanel2.TopBorderColor = System.Drawing.SystemColors.Control;
			this.morePanel2.TopBorderSize = 0;
			// 
			// morePanel3
			// 
			this.morePanel3.BottomBorderColor = System.Drawing.SystemColors.ActiveBorder;
			this.morePanel3.BottomBorderSize = 0;
			this.morePanel3.Controls.Add(this.findBox);
			this.morePanel3.Controls.Add(this.textLabel);
			this.morePanel3.Controls.Add(this.searchButton);
			this.morePanel3.Dock = System.Windows.Forms.DockStyle.Top;
			this.morePanel3.Location = new System.Drawing.Point(15, 61);
			this.morePanel3.Margin = new System.Windows.Forms.Padding(0);
			this.morePanel3.Name = "morePanel3";
			this.morePanel3.Padding = new System.Windows.Forms.Padding(3);
			this.morePanel3.Size = new System.Drawing.Size(782, 64);
			this.morePanel3.TabIndex = 14;
			this.morePanel3.ThemedBack = null;
			this.morePanel3.ThemedFore = null;
			this.morePanel3.TopBorderColor = System.Drawing.SystemColors.Control;
			this.morePanel3.TopBorderSize = 0;
			// 
			// SearchDialogTextControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.resultsView);
			this.Controls.Add(this.morePanel3);
			this.Controls.Add(this.morePanel2);
			this.Controls.Add(this.morePanel1);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "SearchDialogTextControl";
			this.Padding = new System.Windows.Forms.Padding(15, 15, 15, 10);
			this.Size = new System.Drawing.Size(812, 636);
			this.morePanel1.ResumeLayout(false);
			this.morePanel2.ResumeLayout(false);
			this.morePanel2.PerformLayout();
			this.morePanel3.ResumeLayout(false);
			this.morePanel3.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreLabel introLabel;
		private UI.MoreButton cancelButton;
		private UI.MoreLabel textLabel;
		private UI.MoreTextBox findBox;
		private UI.MoreButton searchButton;
		private UI.MoreListView resultsView;
		private System.Windows.Forms.ColumnHeader hitColumn;
		private UI.MorePanel morePanel1;
		private UI.MorePanel morePanel2;
		private UI.MorePanel morePanel3;
	}
}
