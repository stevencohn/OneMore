
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
			this.filterBox = new River.OneMoreAddIn.UI.FormTextBox();
			this.tagsLabel = new System.Windows.Forms.Label();
			this.scopeBox = new System.Windows.Forms.ComboBox();
			this.introLabel = new System.Windows.Forms.Label();
			this.cancelButton = new System.Windows.Forms.Button();
			this.searchPanel = new System.Windows.Forms.Panel();
			this.opBox = new System.Windows.Forms.ComboBox();
			this.clearLabel = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.tagsFlow = new System.Windows.Forms.FlowLayoutPanel();
			this.resultTree = new River.OneMoreAddIn.UI.HierarchyView();
			this.resultPanel = new System.Windows.Forms.Panel();
			this.clearAllLabel = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.checkAllLabel = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.indexButton = new System.Windows.Forms.Button();
			this.copyButton = new System.Windows.Forms.Button();
			this.moveButton = new System.Windows.Forms.Button();
			this.splitContainer = new System.Windows.Forms.SplitContainer();
			this.searchPanel.SuspendLayout();
			this.resultPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// searchButton
			// 
			this.searchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.searchButton.Enabled = false;
			this.searchButton.Image = ((System.Drawing.Image)(resources.GetObject("searchButton.Image")));
			this.searchButton.Location = new System.Drawing.Point(795, 49);
			this.searchButton.Name = "searchButton";
			this.searchButton.Size = new System.Drawing.Size(60, 32);
			this.searchButton.TabIndex = 1;
			this.searchButton.UseVisualStyleBackColor = true;
			this.searchButton.Click += new System.EventHandler(this.Search);
			// 
			// filterBox
			// 
			this.filterBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.filterBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.filterBox.Location = new System.Drawing.Point(80, 52);
			this.filterBox.Name = "filterBox";
			this.filterBox.Size = new System.Drawing.Size(422, 28);
			this.filterBox.TabIndex = 0;
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
			this.scopeBox.Location = new System.Drawing.Point(608, 52);
			this.scopeBox.Name = "scopeBox";
			this.scopeBox.Size = new System.Drawing.Size(180, 28);
			this.scopeBox.TabIndex = 2;
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
			this.cancelButton.Location = new System.Drawing.Point(747, 327);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 5, 10, 9);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(112, 35);
			this.cancelButton.TabIndex = 5;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.Cancel);
			// 
			// searchPanel
			// 
			this.searchPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.searchPanel.Controls.Add(this.opBox);
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
			this.searchPanel.Size = new System.Drawing.Size(878, 259);
			this.searchPanel.TabIndex = 13;
			// 
			// opBox
			// 
			this.opBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.opBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.opBox.FormattingEnabled = true;
			this.opBox.Items.AddRange(new object[] {
            "All",
            "Any"});
			this.opBox.Location = new System.Drawing.Point(508, 54);
			this.opBox.Name = "opBox";
			this.opBox.Size = new System.Drawing.Size(94, 28);
			this.opBox.TabIndex = 13;
			// 
			// clearLabel
			// 
			this.clearLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.clearLabel.AutoSize = true;
			this.clearLabel.Cursor = System.Windows.Forms.Cursors.Hand;
			this.clearLabel.Enabled = false;
			this.clearLabel.LinkColor = System.Drawing.Color.DodgerBlue;
			this.clearLabel.Location = new System.Drawing.Point(790, 228);
			this.clearLabel.Name = "clearLabel";
			this.clearLabel.Size = new System.Drawing.Size(65, 20);
			this.clearLabel.TabIndex = 3;
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
			this.tagsFlow.Size = new System.Drawing.Size(832, 131);
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
			this.resultTree.Location = new System.Drawing.Point(18, 40);
			this.resultTree.Margin = new System.Windows.Forms.Padding(10);
			this.resultTree.Name = "resultTree";
			this.resultTree.ShowLines = false;
			this.resultTree.ShowRootLines = false;
			this.resultTree.Size = new System.Drawing.Size(841, 272);
			this.resultTree.Suspend = false;
			this.resultTree.TabIndex = 15;
			this.resultTree.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.TreeAfterCheck);
			this.resultTree.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.ClickNode);
			// 
			// resultPanel
			// 
			this.resultPanel.Controls.Add(this.clearAllLabel);
			this.resultPanel.Controls.Add(this.checkAllLabel);
			this.resultPanel.Controls.Add(this.indexButton);
			this.resultPanel.Controls.Add(this.copyButton);
			this.resultPanel.Controls.Add(this.moveButton);
			this.resultPanel.Controls.Add(this.resultTree);
			this.resultPanel.Controls.Add(this.cancelButton);
			this.resultPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.resultPanel.Location = new System.Drawing.Point(0, 0);
			this.resultPanel.Name = "resultPanel";
			this.resultPanel.Padding = new System.Windows.Forms.Padding(10);
			this.resultPanel.Size = new System.Drawing.Size(878, 380);
			this.resultPanel.TabIndex = 16;
			// 
			// clearAllLabel
			// 
			this.clearAllLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.clearAllLabel.AutoSize = true;
			this.clearAllLabel.Cursor = System.Windows.Forms.Cursors.Hand;
			this.clearAllLabel.Enabled = false;
			this.clearAllLabel.LinkColor = System.Drawing.Color.DodgerBlue;
			this.clearAllLabel.Location = new System.Drawing.Point(715, 10);
			this.clearAllLabel.Name = "clearAllLabel";
			this.clearAllLabel.Size = new System.Drawing.Size(139, 20);
			this.clearAllLabel.TabIndex = 1;
			this.clearAllLabel.TabStop = true;
			this.clearAllLabel.Text = "Uncheck all pages";
			this.clearAllLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ToggleChecks);
			// 
			// checkAllLabel
			// 
			this.checkAllLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkAllLabel.AutoSize = true;
			this.checkAllLabel.Cursor = System.Windows.Forms.Cursors.Hand;
			this.checkAllLabel.Enabled = false;
			this.checkAllLabel.LinkColor = System.Drawing.Color.DodgerBlue;
			this.checkAllLabel.Location = new System.Drawing.Point(578, 10);
			this.checkAllLabel.Margin = new System.Windows.Forms.Padding(3, 0, 10, 0);
			this.checkAllLabel.Name = "checkAllLabel";
			this.checkAllLabel.Size = new System.Drawing.Size(121, 20);
			this.checkAllLabel.TabIndex = 0;
			this.checkAllLabel.TabStop = true;
			this.checkAllLabel.Text = "Check all pages";
			this.checkAllLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ToggleChecks);
			// 
			// indexButton
			// 
			this.indexButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.indexButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.indexButton.Enabled = false;
			this.indexButton.Location = new System.Drawing.Point(369, 326);
			this.indexButton.Margin = new System.Windows.Forms.Padding(4, 5, 10, 9);
			this.indexButton.Name = "indexButton";
			this.indexButton.Size = new System.Drawing.Size(112, 35);
			this.indexButton.TabIndex = 2;
			this.indexButton.Text = "Index";
			this.indexButton.UseVisualStyleBackColor = true;
			this.indexButton.Click += new System.EventHandler(this.IndexPressed);
			// 
			// copyButton
			// 
			this.copyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.copyButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.copyButton.Enabled = false;
			this.copyButton.Location = new System.Drawing.Point(495, 326);
			this.copyButton.Margin = new System.Windows.Forms.Padding(4, 5, 10, 9);
			this.copyButton.Name = "copyButton";
			this.copyButton.Size = new System.Drawing.Size(112, 35);
			this.copyButton.TabIndex = 3;
			this.copyButton.Text = "Copy";
			this.copyButton.UseVisualStyleBackColor = true;
			this.copyButton.Click += new System.EventHandler(this.CopyPressed);
			// 
			// moveButton
			// 
			this.moveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.moveButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.moveButton.Enabled = false;
			this.moveButton.Location = new System.Drawing.Point(621, 327);
			this.moveButton.Margin = new System.Windows.Forms.Padding(4, 5, 10, 9);
			this.moveButton.Name = "moveButton";
			this.moveButton.Size = new System.Drawing.Size(112, 35);
			this.moveButton.TabIndex = 4;
			this.moveButton.Text = "Move";
			this.moveButton.UseVisualStyleBackColor = true;
			this.moveButton.Click += new System.EventHandler(this.MovePressed);
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
			this.splitContainer.Size = new System.Drawing.Size(878, 644);
			this.splitContainer.SplitterDistance = 259;
			this.splitContainer.SplitterWidth = 5;
			this.splitContainer.TabIndex = 17;
			// 
			// TaggedDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(878, 644);
			this.Controls.Add(this.splitContainer);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(727, 491);
			this.Name = "TaggedDialog";
			this.Text = "Find Tagged Pages";
			this.searchPanel.ResumeLayout(false);
			this.searchPanel.PerformLayout();
			this.resultPanel.ResumeLayout(false);
			this.resultPanel.PerformLayout();
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button searchButton;
		private River.OneMoreAddIn.UI.FormTextBox filterBox;
		private System.Windows.Forms.Label tagsLabel;
		private System.Windows.Forms.ComboBox scopeBox;
		private System.Windows.Forms.Label introLabel;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Panel searchPanel;
		private System.Windows.Forms.FlowLayoutPanel tagsFlow;
		private UI.MoreLinkLabel clearLabel;
		private UI.HierarchyView resultTree;
		private System.Windows.Forms.Panel resultPanel;
		private System.Windows.Forms.SplitContainer splitContainer;
		private UI.MoreLinkLabel clearAllLabel;
		private UI.MoreLinkLabel checkAllLabel;
		private System.Windows.Forms.Button indexButton;
		private System.Windows.Forms.Button copyButton;
		private System.Windows.Forms.Button moveButton;
		private System.Windows.Forms.ComboBox opBox;
	}
}