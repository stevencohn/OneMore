﻿namespace River.OneMoreAddIn.Settings
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
			System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("General Options");
			System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Colorizer");
			System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Command Aliases");
			System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Context Menu");
			System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Favorites");
			System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("File Import");
			System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("Hashtags");
			System.Windows.Forms.TreeNode treeNode8 = new System.Windows.Forms.TreeNode("Highlighter Themes");
			System.Windows.Forms.TreeNode treeNode9 = new System.Windows.Forms.TreeNode("Horizontal Lines");
			System.Windows.Forms.TreeNode treeNode10 = new System.Windows.Forms.TreeNode("Images");
			System.Windows.Forms.TreeNode treeNode11 = new System.Windows.Forms.TreeNode("Keyboard Shortcuts");
			System.Windows.Forms.TreeNode treeNode12 = new System.Windows.Forms.TreeNode("Navigator");
			System.Windows.Forms.TreeNode treeNode13 = new System.Windows.Forms.TreeNode("Plugins");
			System.Windows.Forms.TreeNode treeNode14 = new System.Windows.Forms.TreeNode("Quick Notes");
			System.Windows.Forms.TreeNode treeNode15 = new System.Windows.Forms.TreeNode("Ribbon Bar");
			System.Windows.Forms.TreeNode treeNode16 = new System.Windows.Forms.TreeNode("Search Engines");
			System.Windows.Forms.TreeNode treeNode17 = new System.Windows.Forms.TreeNode("Snippets");
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
			this.buttonPanel.Location = new System.Drawing.Point(15, 684);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Size = new System.Drawing.Size(1126, 45);
			this.buttonPanel.TabIndex = 0;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(874, 3);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(120, 38);
			this.okButton.TabIndex = 1;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OK);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(1000, 3);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(120, 38);
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
			this.editorPanel.Location = new System.Drawing.Point(333, 18);
			this.editorPanel.Name = "editorPanel";
			this.editorPanel.Padding = new System.Windows.Forms.Padding(2);
			this.editorPanel.Size = new System.Drawing.Size(806, 661);
			this.editorPanel.TabIndex = 1;
			// 
			// contentPanel
			// 
			this.contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.contentPanel.Location = new System.Drawing.Point(2, 34);
			this.contentPanel.MinimumSize = new System.Drawing.Size(800, 500);
			this.contentPanel.Name = "contentPanel";
			this.contentPanel.Padding = new System.Windows.Forms.Padding(15);
			this.contentPanel.Size = new System.Drawing.Size(800, 623);
			this.contentPanel.TabIndex = 1;
			// 
			// headerLabel
			// 
			this.headerLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.headerLabel.Font = new System.Drawing.Font("Tahoma", 9F);
			this.headerLabel.ForeColor = System.Drawing.Color.White;
			this.headerLabel.Location = new System.Drawing.Point(2, 2);
			this.headerLabel.Name = "headerLabel";
			this.headerLabel.Size = new System.Drawing.Size(800, 32);
			this.headerLabel.TabIndex = 0;
			this.headerLabel.Text = "Header";
			this.headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// navTree
			// 
			this.navTree.Dock = System.Windows.Forms.DockStyle.Left;
			this.navTree.FullRowSelect = true;
			this.navTree.HideSelection = false;
			this.navTree.Location = new System.Drawing.Point(15, 15);
			this.navTree.Margin = new System.Windows.Forms.Padding(3, 3, 15, 3);
			this.navTree.Name = "navTree";
			treeNode1.Name = "generalNode";
			treeNode1.Text = "General Options";
			treeNode2.Name = "colorizerNode";
			treeNode2.Text = "Colorizer";
			treeNode3.Name = "aliasNode";
			treeNode3.Text = "Command Aliases";
			treeNode4.Name = "contextNode";
			treeNode4.Text = "Context Menu";
			treeNode5.Name = "favoritesNode";
			treeNode5.Text = "Favorites";
			treeNode6.Name = "fileImportNode";
			treeNode6.Text = "File Import";
			treeNode7.Name = "hashtagsNode";
			treeNode7.Text = "Hashtags";
			treeNode8.Name = "highlightNode";
			treeNode8.Text = "Highlighter Themes";
			treeNode9.Name = "linesNode";
			treeNode9.Text = "Horizontal Lines";
			treeNode10.Name = "imagesNode";
			treeNode10.Text = "Images";
			treeNode11.Name = "keyboardNode";
			treeNode11.Text = "Keyboard Shortcuts";
			treeNode12.Name = "navigatorNode";
			treeNode12.Text = "Navigator";
			treeNode13.Name = "pluginsNode";
			treeNode13.Text = "Plugins";
			treeNode14.Name = "quickNotesNode";
			treeNode14.Text = "Quick Notes";
			treeNode15.Name = "ribbonNode";
			treeNode15.Text = "Ribbon Bar";
			treeNode16.Name = "searchNode";
			treeNode16.Text = "Search Engines";
			treeNode17.Name = "snippetsNode";
			treeNode17.Text = "Snippets";
			this.navTree.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode3,
            treeNode4,
            treeNode5,
            treeNode6,
            treeNode7,
            treeNode8,
            treeNode9,
            treeNode10,
            treeNode11,
            treeNode12,
            treeNode13,
            treeNode14,
            treeNode15,
            treeNode16,
            treeNode17});
			this.navTree.Size = new System.Drawing.Size(300, 669);
			this.navTree.TabIndex = 2;
			this.navTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.Navigate);
			// 
			// SettingsDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(1156, 744);
			this.Controls.Add(this.editorPanel);
			this.Controls.Add(this.navTree);
			this.Controls.Add(this.buttonPanel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SettingsDialog";
			this.Padding = new System.Windows.Forms.Padding(15);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "OneMore Settings";
			this.Load += new System.EventHandler(this.InitializeLoad);
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