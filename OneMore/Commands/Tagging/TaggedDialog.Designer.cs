
namespace River.OneMoreAddIn.Commands
{
	partial class TaggedDialog
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

				one.Dispose();
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TaggedDialog));
			this.searchButton = new System.Windows.Forms.Button();
			this.filterBox = new River.OneMoreAddIn.Dialogs.FormTextBox();
			this.tagsLabel = new System.Windows.Forms.Label();
			this.scopeBox = new System.Windows.Forms.ComboBox();
			this.introLabel = new System.Windows.Forms.Label();
			this.cancelButton = new System.Windows.Forms.Button();
			this.searchPanel = new System.Windows.Forms.Panel();
			this.clearLabel = new System.Windows.Forms.LinkLabel();
			this.tagsFlow = new System.Windows.Forms.FlowLayoutPanel();
			this.resultTree = new River.OneMoreAddIn.HierarchyView();
			this.resultPanel = new System.Windows.Forms.Panel();
			this.toolStrip = new River.OneMoreAddIn.ScaledToolStrip();
			this.toolSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.checkNoneButton = new System.Windows.Forms.ToolStripButton();
			this.checkAllButton = new System.Windows.Forms.ToolStripButton();
			this.toolSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.indexButton = new System.Windows.Forms.ToolStripButton();
			this.toolSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.copyToButton = new System.Windows.Forms.ToolStripButton();
			this.moveToButton = new System.Windows.Forms.ToolStripButton();
			this.splitContainer = new System.Windows.Forms.SplitContainer();
			this.searchPanel.SuspendLayout();
			this.resultPanel.SuspendLayout();
			this.toolStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// searchButton
			// 
			this.searchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.searchButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.searchButton.Enabled = false;
			this.searchButton.Image = ((System.Drawing.Image)(resources.GetObject("searchButton.Image")));
			this.searchButton.Location = new System.Drawing.Point(508, 52);
			this.searchButton.Name = "searchButton";
			this.searchButton.Size = new System.Drawing.Size(60, 32);
			this.searchButton.TabIndex = 0;
			this.searchButton.UseVisualStyleBackColor = true;
			this.searchButton.Click += new System.EventHandler(this.Search);
			// 
			// filterBox
			// 
			this.filterBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.filterBox.Location = new System.Drawing.Point(80, 52);
			this.filterBox.Name = "filterBox";
			this.filterBox.Size = new System.Drawing.Size(424, 26);
			this.filterBox.TabIndex = 1;
			this.filterBox.TextChanged += new System.EventHandler(this.ChangedFilter);
			// 
			// tagsLabel
			// 
			this.tagsLabel.AutoSize = true;
			this.tagsLabel.Location = new System.Drawing.Point(24, 55);
			this.tagsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.tagsLabel.Name = "tagsLabel";
			this.tagsLabel.Size = new System.Drawing.Size(48, 20);
			this.tagsLabel.TabIndex = 9;
			this.tagsLabel.Text = "Tags:";
			// 
			// scopeBox
			// 
			this.scopeBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.scopeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.scopeBox.FormattingEnabled = true;
			this.scopeBox.Items.AddRange(new object[] {
            "In all notebooks",
            "In this notebook",
            "In this section"});
			this.scopeBox.Location = new System.Drawing.Point(574, 52);
			this.scopeBox.Name = "scopeBox";
			this.scopeBox.Size = new System.Drawing.Size(180, 28);
			this.scopeBox.TabIndex = 10;
			this.scopeBox.SelectedIndexChanged += new System.EventHandler(this.ChangeScope);
			// 
			// introLabel
			// 
			this.introLabel.AutoSize = true;
			this.introLabel.Location = new System.Drawing.Point(22, 20);
			this.introLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 9);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(577, 20);
			this.introLabel.TabIndex = 11;
			this.introLabel.Text = "Enter one or more tags separated by commas. Use -tag to exclude indvidual tags";
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(647, 280);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 5, 10, 9);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(112, 35);
			this.cancelButton.TabIndex = 12;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.Cancel);
			// 
			// searchPanel
			// 
			this.searchPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.searchPanel.Controls.Add(this.clearLabel);
			this.searchPanel.Controls.Add(this.tagsFlow);
			this.searchPanel.Controls.Add(this.introLabel);
			this.searchPanel.Controls.Add(this.searchButton);
			this.searchPanel.Controls.Add(this.filterBox);
			this.searchPanel.Controls.Add(this.scopeBox);
			this.searchPanel.Controls.Add(this.tagsLabel);
			this.searchPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.searchPanel.Location = new System.Drawing.Point(0, 0);
			this.searchPanel.MinimumSize = new System.Drawing.Size(700, 200);
			this.searchPanel.Name = "searchPanel";
			this.searchPanel.Padding = new System.Windows.Forms.Padding(20, 20, 20, 9);
			this.searchPanel.Size = new System.Drawing.Size(778, 307);
			this.searchPanel.TabIndex = 13;
			// 
			// clearLabel
			// 
			this.clearLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.clearLabel.AutoSize = true;
			this.clearLabel.Enabled = false;
			this.clearLabel.LinkColor = System.Drawing.Color.DodgerBlue;
			this.clearLabel.Location = new System.Drawing.Point(690, 276);
			this.clearLabel.Name = "clearLabel";
			this.clearLabel.Size = new System.Drawing.Size(65, 20);
			this.clearLabel.TabIndex = 13;
			this.clearLabel.TabStop = true;
			this.clearLabel.Text = "Clear all";
			this.clearLabel.Click += new System.EventHandler(this.ClearFilters);
			// 
			// tagsFlow
			// 
			this.tagsFlow.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tagsFlow.AutoScroll = true;
			this.tagsFlow.Location = new System.Drawing.Point(22, 94);
			this.tagsFlow.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
			this.tagsFlow.Name = "tagsFlow";
			this.tagsFlow.Size = new System.Drawing.Size(732, 179);
			this.tagsFlow.TabIndex = 12;
			// 
			// resultTree
			// 
			this.resultTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.resultTree.CheckBoxes = true;
			this.resultTree.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
			this.resultTree.HotTracking = true;
			this.resultTree.Location = new System.Drawing.Point(18, 45);
			this.resultTree.Margin = new System.Windows.Forms.Padding(10);
			this.resultTree.Name = "resultTree";
			this.resultTree.ShowLines = false;
			this.resultTree.ShowRootLines = false;
			this.resultTree.Size = new System.Drawing.Size(741, 220);
			this.resultTree.Suspend = false;
			this.resultTree.TabIndex = 15;
			this.resultTree.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.ClickNode);
			// 
			// resultPanel
			// 
			this.resultPanel.Controls.Add(this.toolStrip);
			this.resultPanel.Controls.Add(this.resultTree);
			this.resultPanel.Controls.Add(this.cancelButton);
			this.resultPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.resultPanel.Location = new System.Drawing.Point(0, 0);
			this.resultPanel.Name = "resultPanel";
			this.resultPanel.Padding = new System.Windows.Forms.Padding(10);
			this.resultPanel.Size = new System.Drawing.Size(778, 333);
			this.resultPanel.TabIndex = 16;
			// 
			// toolStrip
			// 
			this.toolStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStrip.Enabled = false;
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolSeparator1,
            this.checkNoneButton,
            this.checkAllButton,
            this.toolSeparator2,
            this.indexButton,
            this.toolSeparator3,
            this.copyToButton,
            this.moveToButton});
			this.toolStrip.Location = new System.Drawing.Point(24, 10);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
			this.toolStrip.Size = new System.Drawing.Size(289, 45);
			this.toolStrip.TabIndex = 16;
			this.toolStrip.Text = "Tool Strip";
			// 
			// toolSeparator1
			// 
			this.toolSeparator1.Name = "toolSeparator1";
			this.toolSeparator1.Size = new System.Drawing.Size(6, 45);
			// 
			// checkNoneButton
			// 
			this.checkNoneButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.checkNoneButton.Image = ((System.Drawing.Image)(resources.GetObject("checkNoneButton.Image")));
			this.checkNoneButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.checkNoneButton.Margin = new System.Windows.Forms.Padding(0, 2, 2, 3);
			this.checkNoneButton.Name = "checkNoneButton";
			this.checkNoneButton.Size = new System.Drawing.Size(34, 40);
			this.checkNoneButton.Text = "Unselect all pages";
			this.checkNoneButton.Click += new System.EventHandler(this.ToggleChecks);
			// 
			// checkAllButton
			// 
			this.checkAllButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.checkAllButton.Image = ((System.Drawing.Image)(resources.GetObject("checkAllButton.Image")));
			this.checkAllButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.checkAllButton.Margin = new System.Windows.Forms.Padding(0, 2, 2, 3);
			this.checkAllButton.Name = "checkAllButton";
			this.checkAllButton.Size = new System.Drawing.Size(34, 40);
			this.checkAllButton.Text = "Select all pages";
			this.checkAllButton.Click += new System.EventHandler(this.ToggleChecks);
			// 
			// toolSeparator2
			// 
			this.toolSeparator2.Name = "toolSeparator2";
			this.toolSeparator2.Size = new System.Drawing.Size(6, 45);
			// 
			// indexButton
			// 
			this.indexButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.indexButton.Image = ((System.Drawing.Image)(resources.GetObject("indexButton.Image")));
			this.indexButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.indexButton.Margin = new System.Windows.Forms.Padding(0, 2, 2, 3);
			this.indexButton.Name = "indexButton";
			this.indexButton.Size = new System.Drawing.Size(34, 40);
			this.indexButton.Text = "Create index page";
			// 
			// toolSeparator3
			// 
			this.toolSeparator3.Name = "toolSeparator3";
			this.toolSeparator3.Size = new System.Drawing.Size(6, 45);
			// 
			// copyToButton
			// 
			this.copyToButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.copyToButton.Image = ((System.Drawing.Image)(resources.GetObject("copyToButton.Image")));
			this.copyToButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.copyToButton.Margin = new System.Windows.Forms.Padding(0, 2, 2, 3);
			this.copyToButton.Name = "copyToButton";
			this.copyToButton.Size = new System.Drawing.Size(34, 40);
			this.copyToButton.Text = "Copy pages to...";
			// 
			// moveToButton
			// 
			this.moveToButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.moveToButton.Image = ((System.Drawing.Image)(resources.GetObject("moveToButton.Image")));
			this.moveToButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.moveToButton.Margin = new System.Windows.Forms.Padding(0, 2, 2, 3);
			this.moveToButton.Name = "moveToButton";
			this.moveToButton.Size = new System.Drawing.Size(34, 40);
			this.moveToButton.Text = "Move pages to...";
			// 
			// splitContainer
			// 
			this.splitContainer.BackColor = System.Drawing.SystemColors.ActiveCaption;
			this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer.Location = new System.Drawing.Point(0, 0);
			this.splitContainer.Name = "splitContainer";
			this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.searchPanel);
			this.splitContainer.Panel1MinSize = 200;
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.BackColor = System.Drawing.SystemColors.Control;
			this.splitContainer.Panel2.Controls.Add(this.resultPanel);
			this.splitContainer.Panel2MinSize = 200;
			this.splitContainer.Size = new System.Drawing.Size(778, 645);
			this.splitContainer.SplitterDistance = 307;
			this.splitContainer.SplitterWidth = 5;
			this.splitContainer.TabIndex = 17;
			// 
			// TaggedDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(778, 645);
			this.Controls.Add(this.splitContainer);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(727, 491);
			this.Name = "TaggedDialog";
			this.Text = "Find Tagged Pages";
			this.TopMost = true;
			this.searchPanel.ResumeLayout(false);
			this.searchPanel.PerformLayout();
			this.resultPanel.ResumeLayout(false);
			this.resultPanel.PerformLayout();
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button searchButton;
		private River.OneMoreAddIn.Dialogs.FormTextBox filterBox;
		private System.Windows.Forms.Label tagsLabel;
		private System.Windows.Forms.ComboBox scopeBox;
		private System.Windows.Forms.Label introLabel;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Panel searchPanel;
		private System.Windows.Forms.FlowLayoutPanel tagsFlow;
		private System.Windows.Forms.LinkLabel clearLabel;
		private HierarchyView resultTree;
		private System.Windows.Forms.Panel resultPanel;
		private System.Windows.Forms.SplitContainer splitContainer;
		private ScaledToolStrip toolStrip;
		private System.Windows.Forms.ToolStripButton checkNoneButton;
		private System.Windows.Forms.ToolStripButton checkAllButton;
		private System.Windows.Forms.ToolStripButton indexButton;
		private System.Windows.Forms.ToolStripButton copyToButton;
		private System.Windows.Forms.ToolStripButton moveToButton;
		private System.Windows.Forms.ToolStripSeparator toolSeparator1;
		private System.Windows.Forms.ToolStripSeparator toolSeparator2;
		private System.Windows.Forms.ToolStripSeparator toolSeparator3;
	}
}