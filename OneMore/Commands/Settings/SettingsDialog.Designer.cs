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
			System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Context Menu");
			System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Favorites");
			System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Highlighter Themes");
			System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Plugins");
			System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Ribbon Bar");
			System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Search Engines");
			System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("Snippets");
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsDialog));
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.editorPanel = new System.Windows.Forms.Panel();
			this.contentPanel = new System.Windows.Forms.Panel();
			this.headerLabel = new River.OneMoreAddIn.Settings.FadingLabel();
			this.navTree = new System.Windows.Forms.TreeView();
			this.buttonPanel.SuspendLayout();
			this.editorPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonPanel
			// 
			this.buttonPanel.Controls.Add(this.okButton);
			this.buttonPanel.Controls.Add(this.cancelButton);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.Location = new System.Drawing.Point(10, 374);
			this.buttonPanel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Size = new System.Drawing.Size(751, 30);
			this.buttonPanel.TabIndex = 0;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(583, 2);
			this.okButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(80, 25);
			this.okButton.TabIndex = 1;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OK);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(667, 2);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(80, 25);
			this.cancelButton.TabIndex = 0;
			this.cancelButton.Text = "Cancel";
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
			this.editorPanel.Location = new System.Drawing.Point(222, 12);
			this.editorPanel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.editorPanel.Name = "editorPanel";
			this.editorPanel.Padding = new System.Windows.Forms.Padding(1, 1, 1, 1);
			this.editorPanel.Size = new System.Drawing.Size(538, 359);
			this.editorPanel.TabIndex = 1;
			// 
			// contentPanel
			// 
			this.contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.contentPanel.Location = new System.Drawing.Point(1, 22);
			this.contentPanel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.contentPanel.MinimumSize = new System.Drawing.Size(533, 333);
			this.contentPanel.Name = "contentPanel";
			this.contentPanel.Padding = new System.Windows.Forms.Padding(10, 10, 10, 10);
			this.contentPanel.Size = new System.Drawing.Size(534, 334);
			this.contentPanel.TabIndex = 1;
			// 
			// headerLabel
			// 
			this.headerLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.headerLabel.Font = new System.Drawing.Font("Tahoma", 9F);
			this.headerLabel.ForeColor = System.Drawing.Color.White;
			this.headerLabel.Location = new System.Drawing.Point(1, 1);
			this.headerLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.headerLabel.Name = "headerLabel";
			this.headerLabel.Size = new System.Drawing.Size(534, 21);
			this.headerLabel.TabIndex = 0;
			this.headerLabel.Text = "Header";
			this.headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// navTree
			// 
			this.navTree.Dock = System.Windows.Forms.DockStyle.Left;
			this.navTree.FullRowSelect = true;
			this.navTree.HideSelection = false;
			this.navTree.Location = new System.Drawing.Point(10, 10);
			this.navTree.Margin = new System.Windows.Forms.Padding(2, 2, 10, 2);
			this.navTree.Name = "navTree";
			treeNode1.Name = "contextNode";
			treeNode1.Text = "Context Menu";
			treeNode2.Name = "favoritesNode";
			treeNode2.Text = "Favorites";
			treeNode3.Name = "highlightNode";
			treeNode3.Text = "Highlighter Themes";
			treeNode4.Name = "pluginsNode";
			treeNode4.Text = "Plugins";
			treeNode5.Name = "ribbonNode";
			treeNode5.Text = "Ribbon Bar";
			treeNode6.Name = "searchNode";
			treeNode6.Text = "Search Engines";
			treeNode7.Name = "snippetsNode";
			treeNode7.Text = "Snippets";
			this.navTree.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode3,
            treeNode4,
            treeNode5,
            treeNode6,
            treeNode7});
			this.navTree.Size = new System.Drawing.Size(201, 364);
			this.navTree.TabIndex = 2;
			this.navTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.Navigate);
			// 
			// SettingsDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(771, 414);
			this.Controls.Add(this.editorPanel);
			this.Controls.Add(this.navTree);
			this.Controls.Add(this.buttonPanel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SettingsDialog";
			this.Padding = new System.Windows.Forms.Padding(10, 10, 10, 10);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "OneMore Settings";
			this.buttonPanel.ResumeLayout(false);
			this.editorPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel buttonPanel;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Panel editorPanel;
		private System.Windows.Forms.Panel contentPanel;
		private FadingLabel headerLabel;
		private System.Windows.Forms.TreeView navTree;
	}
}