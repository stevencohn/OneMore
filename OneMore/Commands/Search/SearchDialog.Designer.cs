namespace River.OneMoreAddIn.Commands.Search
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SearchDialog));
			this.introLabel = new System.Windows.Forms.Label();
			this.findLabel = new System.Windows.Forms.Label();
			this.findBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.searchButton = new River.OneMoreAddIn.UI.MoreButton();
			this.copyButton = new River.OneMoreAddIn.UI.MoreButton();
			this.moveButton = new River.OneMoreAddIn.UI.MoreButton();
			this.resultTree = new River.OneMoreAddIn.UI.HierarchyView();
			this.scopeBox = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// introLabel
			// 
			this.introLabel.AutoSize = true;
			this.introLabel.Location = new System.Drawing.Point(26, 23);
			this.introLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 15);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(579, 20);
			this.introLabel.TabIndex = 0;
			this.introLabel.Text = "Enter one or more keywords, \"quoted phrases\", and use uppercase AND and OR";
			// 
			// findLabel
			// 
			this.findLabel.AutoSize = true;
			this.findLabel.Location = new System.Drawing.Point(26, 66);
			this.findLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.findLabel.Name = "findLabel";
			this.findLabel.Size = new System.Drawing.Size(44, 20);
			this.findLabel.TabIndex = 1;
			this.findLabel.Text = "Find:";
			// 
			// findBox
			// 
			this.findBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.findBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.findBox.Location = new System.Drawing.Point(81, 63);
			this.findBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.findBox.Name = "findBox";
			this.findBox.Size = new System.Drawing.Size(406, 26);
			this.findBox.TabIndex = 0;
			this.findBox.ThemedBack = null;
			this.findBox.ThemedFore = null;
			this.findBox.TextChanged += new System.EventHandler(this.ChangeQuery);
			this.findBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SearchOnKeydown);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(641, 538);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 15);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(112, 35);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 6;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.Nevermind);
			// 
			// searchButton
			// 
			this.searchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.searchButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.searchButton.Enabled = false;
			this.searchButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.searchButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Search;
			this.searchButton.ImageOver = null;
			this.searchButton.Location = new System.Drawing.Point(495, 62);
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
			// copyButton
			// 
			this.copyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.copyButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.copyButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.copyButton.Enabled = false;
			this.copyButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.copyButton.ImageOver = null;
			this.copyButton.Location = new System.Drawing.Point(401, 538);
			this.copyButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.copyButton.Name = "copyButton";
			this.copyButton.ShowBorder = true;
			this.copyButton.Size = new System.Drawing.Size(112, 35);
			this.copyButton.StylizeImage = false;
			this.copyButton.TabIndex = 4;
			this.copyButton.Text = "Copy";
			this.copyButton.ThemedBack = null;
			this.copyButton.ThemedFore = null;
			this.copyButton.UseVisualStyleBackColor = true;
			this.copyButton.Click += new System.EventHandler(this.CopyPressed);
			// 
			// moveButton
			// 
			this.moveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.moveButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.moveButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.moveButton.Enabled = false;
			this.moveButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.moveButton.ImageOver = null;
			this.moveButton.Location = new System.Drawing.Point(521, 538);
			this.moveButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.moveButton.Name = "moveButton";
			this.moveButton.ShowBorder = true;
			this.moveButton.Size = new System.Drawing.Size(112, 35);
			this.moveButton.StylizeImage = false;
			this.moveButton.TabIndex = 5;
			this.moveButton.Text = "Move";
			this.moveButton.ThemedBack = null;
			this.moveButton.ThemedFore = null;
			this.moveButton.UseVisualStyleBackColor = true;
			this.moveButton.Click += new System.EventHandler(this.MovePressed);
			// 
			// resultTree
			// 
			this.resultTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.resultTree.CheckBoxes = true;
			this.resultTree.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
			this.resultTree.HideSelection = false;
			this.resultTree.HotTracking = true;
			this.resultTree.Location = new System.Drawing.Point(25, 112);
			this.resultTree.Name = "resultTree";
			this.resultTree.ShowLines = false;
			this.resultTree.ShowRootLines = false;
			this.resultTree.Size = new System.Drawing.Size(728, 401);
			this.resultTree.Suspend = false;
			this.resultTree.TabIndex = 3;
			this.resultTree.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.TreeAfterCheck);
			this.resultTree.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.ClickNode);
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
			this.scopeBox.Location = new System.Drawing.Point(570, 65);
			this.scopeBox.Name = "scopeBox";
			this.scopeBox.Size = new System.Drawing.Size(183, 28);
			this.scopeBox.TabIndex = 2;
			// 
			// SearchDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(778, 594);
			this.Controls.Add(this.scopeBox);
			this.Controls.Add(this.resultTree);
			this.Controls.Add(this.copyButton);
			this.Controls.Add(this.moveButton);
			this.Controls.Add(this.searchButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.findBox);
			this.Controls.Add(this.findLabel);
			this.Controls.Add(this.introLabel);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(722, 400);
			this.Name = "SearchDialog";
			this.Padding = new System.Windows.Forms.Padding(22, 23, 22, 15);
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Search and Move or Copy";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label introLabel;
		private System.Windows.Forms.Label findLabel;
		private UI.MoreTextBox findBox;
		private UI.MoreButton cancelButton;
		private UI.MoreButton searchButton;
		private UI.MoreButton copyButton;
		private UI.MoreButton moveButton;
		private River.OneMoreAddIn.UI.HierarchyView resultTree;
		private System.Windows.Forms.ComboBox scopeBox;
	}
}