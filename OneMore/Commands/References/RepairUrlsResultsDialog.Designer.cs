
namespace River.OneMoreAddIn.Commands
{
	partial class RepairUrlsResultsDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RepairUrlsResultsDialog));
			this.listView = new River.OneMoreAddIn.UI.MoreListView();
			this.statusColumn = new System.Windows.Forms.ColumnHeader();
			this.linkColumn = new System.Windows.Forms.ColumnHeader();
			this.pageColumn = new System.Windows.Forms.ColumnHeader();
			this.detailColumn = new System.Windows.Forms.ColumnHeader();
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.copyButton = new River.OneMoreAddIn.UI.MoreButton();
			this.goButton = new River.OneMoreAddIn.UI.MoreButton();
			this.closeButton = new River.OneMoreAddIn.UI.MoreButton();
			this.topPanel = new System.Windows.Forms.Panel();
			this.summaryLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.filterLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.filterBox = new River.OneMoreAddIn.UI.MoreComboBox();
			this.buttonPanel.SuspendLayout();
			this.topPanel.SuspendLayout();
			this.SuspendLayout();
			//
			// listView
			//
			this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.statusColumn,
            this.pageColumn,
            this.linkColumn,
            this.detailColumn});
			this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listView.FullRowSelect = true;
			this.listView.HideSelection = false;
			this.listView.Location = new System.Drawing.Point(0, 60);
			this.listView.MultiSelect = false;
			this.listView.Name = "listView";
			this.listView.Size = new System.Drawing.Size(878, 424);
			this.listView.TabIndex = 0;
			this.listView.UseCompatibleStateImageBehavior = false;
			this.listView.View = System.Windows.Forms.View.Details;
			this.listView.DoubleClick += new System.EventHandler(this.GoToPage);
			//
			// statusColumn
			//
			this.statusColumn.Name = "statusColumn";
			this.statusColumn.Text = "Status";
			//
			// linkColumn
			//
			this.linkColumn.Name = "linkColumn";
			this.linkColumn.Text = "Link";
			//
			// pageColumn
			//
			this.pageColumn.Name = "pageColumn";
			this.pageColumn.Text = "Page";
			//
			// detailColumn
			//
			this.detailColumn.Name = "detailColumn";
			this.detailColumn.Text = "Details";
			//
			// buttonPanel
			//
			this.buttonPanel.BackColor = System.Drawing.SystemColors.ControlLight;
			this.buttonPanel.Controls.Add(this.copyButton);
			this.buttonPanel.Controls.Add(this.goButton);
			this.buttonPanel.Controls.Add(this.closeButton);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.buttonPanel.Location = new System.Drawing.Point(0, 484);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Padding = new System.Windows.Forms.Padding(8);
			this.buttonPanel.Size = new System.Drawing.Size(878, 60);
			this.buttonPanel.TabIndex = 4;
			//
			// copyButton
			//
			this.copyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.copyButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.copyButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.copyButton.ImageOver = null;
			this.copyButton.Location = new System.Drawing.Point(388, 11);
			this.copyButton.Name = "copyButton";
			this.copyButton.ShowBorder = true;
			this.copyButton.Size = new System.Drawing.Size(224, 38);
			this.copyButton.StylizeImage = false;
			this.copyButton.TabIndex = 2;
			this.copyButton.Text = "Copy to new page";
			this.copyButton.ThemedBack = null;
			this.copyButton.ThemedFore = null;
			this.copyButton.UseVisualStyleBackColor = true;
			this.copyButton.Click += new System.EventHandler(this.CopyToPage);
			//
			// goButton
			//
			this.goButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.goButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.goButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.goButton.ImageOver = null;
			this.goButton.Location = new System.Drawing.Point(618, 11);
			this.goButton.Name = "goButton";
			this.goButton.ShowBorder = true;
			this.goButton.Size = new System.Drawing.Size(120, 38);
			this.goButton.StylizeImage = false;
			this.goButton.TabIndex = 0;
			this.goButton.Text = "Go";
			this.goButton.ThemedBack = null;
			this.goButton.ThemedFore = null;
			this.goButton.UseVisualStyleBackColor = true;
			this.goButton.Click += new System.EventHandler(this.GoToPage);
			//
			// closeButton
			//
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.closeButton.ImageOver = null;
			this.closeButton.Location = new System.Drawing.Point(744, 11);
			this.closeButton.Name = "closeButton";
			this.closeButton.ShowBorder = true;
			this.closeButton.Size = new System.Drawing.Size(120, 38);
			this.closeButton.StylizeImage = false;
			this.closeButton.TabIndex = 1;
			this.closeButton.Text = "Close";
			this.closeButton.ThemedBack = null;
			this.closeButton.ThemedFore = null;
			this.closeButton.UseVisualStyleBackColor = true;
			this.closeButton.Click += new System.EventHandler(this.CloseDialog);
			//
			// topPanel
			//
			this.topPanel.BackColor = System.Drawing.SystemColors.ControlLight;
			this.topPanel.Controls.Add(this.filterBox);
			this.topPanel.Controls.Add(this.filterLabel);
			this.topPanel.Controls.Add(this.summaryLabel);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.topPanel.Location = new System.Drawing.Point(0, 0);
			this.topPanel.Name = "topPanel";
			this.topPanel.Padding = new System.Windows.Forms.Padding(15);
			this.topPanel.Size = new System.Drawing.Size(878, 60);
			this.topPanel.TabIndex = 5;
			//
			// summaryLabel
			//
			this.summaryLabel.AutoSize = true;
			this.summaryLabel.Location = new System.Drawing.Point(18, 20);
			this.summaryLabel.Name = "summaryLabel";
			this.summaryLabel.Size = new System.Drawing.Size(100, 20);
			this.summaryLabel.TabIndex = 0;
			this.summaryLabel.Text = "Summary";
			this.summaryLabel.ThemedBack = null;
			this.summaryLabel.ThemedFore = null;
			//
			// filterLabel
			//
			this.filterLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.filterLabel.AutoSize = true;
			this.filterLabel.Location = new System.Drawing.Point(630, 20);
			this.filterLabel.Name = "filterLabel";
			this.filterLabel.Size = new System.Drawing.Size(49, 20);
			this.filterLabel.TabIndex = 1;
			this.filterLabel.Text = "Filter";
			this.filterLabel.ThemedBack = null;
			this.filterLabel.ThemedFore = null;
			//
			// filterBox
			//
			this.filterBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.filterBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.filterBox.FormattingEnabled = true;
			this.filterBox.Location = new System.Drawing.Point(700, 16);
			this.filterBox.Name = "filterBox";
			this.filterBox.Size = new System.Drawing.Size(160, 28);
			this.filterBox.TabIndex = 2;
			this.filterBox.ThemedBack = null;
			this.filterBox.ThemedFore = null;
			this.filterBox.SelectedIndexChanged += new System.EventHandler(this.FilterResults);
			//
			// RepairUrlsResultsDialog
			//
			this.AcceptButton = this.goButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.closeButton;
			this.ClientSize = new System.Drawing.Size(878, 544);
			this.Controls.Add(this.listView);
			this.Controls.Add(this.topPanel);
			this.Controls.Add(this.buttonPanel);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(700, 400);
			this.Name = "RepairUrlsResultsDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Repair URLs Results";
			this.buttonPanel.ResumeLayout(false);
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.ResumeLayout(false);

		}
		#endregion

		private UI.MoreListView listView;
		private System.Windows.Forms.ColumnHeader statusColumn;
		private System.Windows.Forms.ColumnHeader linkColumn;
		private System.Windows.Forms.ColumnHeader pageColumn;
		private System.Windows.Forms.ColumnHeader detailColumn;
		private System.Windows.Forms.Panel buttonPanel;
		private UI.MoreButton copyButton;
		private UI.MoreButton goButton;
		private UI.MoreButton closeButton;
		private System.Windows.Forms.Panel topPanel;
		private UI.MoreLabel summaryLabel;
		private UI.MoreLabel filterLabel;
		private UI.MoreComboBox filterBox;
	}
}
