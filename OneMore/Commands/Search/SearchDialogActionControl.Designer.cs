namespace River.OneMoreAddIn.Commands
{
	partial class SearchDialogActionControl
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
			this.scopeBox = new System.Windows.Forms.ComboBox();
			this.resultTree = new River.OneMoreAddIn.UI.HierarchyView();
			this.copyButton = new River.OneMoreAddIn.UI.MoreButton();
			this.moveButton = new River.OneMoreAddIn.UI.MoreButton();
			this.searchButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.findBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.findLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.introLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.morePanel1 = new River.OneMoreAddIn.UI.MorePanel();
			this.morePanel2 = new River.OneMoreAddIn.UI.MorePanel();
			this.morePanel3 = new River.OneMoreAddIn.UI.MorePanel();
			this.morePanel1.SuspendLayout();
			this.morePanel2.SuspendLayout();
			this.morePanel3.SuspendLayout();
			this.SuspendLayout();
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
			this.scopeBox.Location = new System.Drawing.Point(590, 9);
			this.scopeBox.Name = "scopeBox";
			this.scopeBox.Size = new System.Drawing.Size(183, 28);
			this.scopeBox.TabIndex = 2;
			// 
			// resultTree
			// 
			this.resultTree.CheckBoxes = true;
			this.resultTree.Dock = System.Windows.Forms.DockStyle.Fill;
			this.resultTree.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
			this.resultTree.HideSelection = false;
			this.resultTree.HotTracking = true;
			this.resultTree.Location = new System.Drawing.Point(15, 125);
			this.resultTree.Name = "resultTree";
			this.resultTree.ShowLines = false;
			this.resultTree.ShowRootLines = false;
			this.resultTree.Size = new System.Drawing.Size(782, 435);
			this.resultTree.Suspend = false;
			this.resultTree.TabIndex = 12;
			this.resultTree.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.TreeAfterCheck);
			this.resultTree.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.ClickNode);
			// 
			// copyButton
			// 
			this.copyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.copyButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.copyButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.copyButton.Enabled = false;
			this.copyButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.copyButton.ImageOver = null;
			this.copyButton.Location = new System.Drawing.Point(423, 13);
			this.copyButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.copyButton.Name = "copyButton";
			this.copyButton.ShowBorder = true;
			this.copyButton.Size = new System.Drawing.Size(112, 35);
			this.copyButton.StylizeImage = false;
			this.copyButton.TabIndex = 0;
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
			this.moveButton.Location = new System.Drawing.Point(543, 13);
			this.moveButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.moveButton.Name = "moveButton";
			this.moveButton.ShowBorder = true;
			this.moveButton.Size = new System.Drawing.Size(112, 35);
			this.moveButton.StylizeImage = false;
			this.moveButton.TabIndex = 1;
			this.moveButton.Text = "Move";
			this.moveButton.ThemedBack = null;
			this.moveButton.ThemedFore = null;
			this.moveButton.UseVisualStyleBackColor = true;
			this.moveButton.Click += new System.EventHandler(this.MovePressed);
			// 
			// searchButton
			// 
			this.searchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.searchButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.searchButton.Enabled = false;
			this.searchButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.searchButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Search;
			this.searchButton.ImageOver = null;
			this.searchButton.Location = new System.Drawing.Point(515, 5);
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
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(663, 13);
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
			// findBox
			// 
			this.findBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.findBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.findBox.Location = new System.Drawing.Point(59, 8);
			this.findBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.findBox.Name = "findBox";
			this.findBox.ProcessEnterKey = false;
			this.findBox.Size = new System.Drawing.Size(453, 26);
			this.findBox.TabIndex = 0;
			this.findBox.ThemedBack = null;
			this.findBox.ThemedFore = null;
			this.findBox.TextChanged += new System.EventHandler(this.ChangeQuery);
			this.findBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SearchOnKeydown);
			// 
			// findLabel
			// 
			this.findLabel.AutoSize = true;
			this.findLabel.Location = new System.Drawing.Point(7, 11);
			this.findLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.findLabel.Name = "findLabel";
			this.findLabel.Size = new System.Drawing.Size(44, 20);
			this.findLabel.TabIndex = 10;
			this.findLabel.Text = "Find:";
			this.findLabel.ThemedBack = null;
			this.findLabel.ThemedFore = null;
			// 
			// introLabel
			// 
			this.introLabel.AutoSize = true;
			this.introLabel.Location = new System.Drawing.Point(4, 3);
			this.introLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 15);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(579, 20);
			this.introLabel.TabIndex = 8;
			this.introLabel.Text = "Enter one or more keywords, \"quoted phrases\", and use uppercase AND and OR";
			this.introLabel.ThemedBack = null;
			this.introLabel.ThemedFore = null;
			// 
			// morePanel1
			// 
			this.morePanel1.BottomBorderColor = System.Drawing.SystemColors.ActiveBorder;
			this.morePanel1.BottomBorderSize = 0;
			this.morePanel1.Controls.Add(this.cancelButton);
			this.morePanel1.Controls.Add(this.moveButton);
			this.morePanel1.Controls.Add(this.copyButton);
			this.morePanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.morePanel1.Location = new System.Drawing.Point(15, 560);
			this.morePanel1.Margin = new System.Windows.Forms.Padding(0);
			this.morePanel1.Name = "morePanel1";
			this.morePanel1.Padding = new System.Windows.Forms.Padding(3);
			this.morePanel1.Size = new System.Drawing.Size(782, 66);
			this.morePanel1.TabIndex = 1;
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
			this.morePanel2.TabIndex = 14;
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
			this.morePanel3.Controls.Add(this.findLabel);
			this.morePanel3.Controls.Add(this.searchButton);
			this.morePanel3.Controls.Add(this.scopeBox);
			this.morePanel3.Dock = System.Windows.Forms.DockStyle.Top;
			this.morePanel3.Location = new System.Drawing.Point(15, 61);
			this.morePanel3.Margin = new System.Windows.Forms.Padding(0);
			this.morePanel3.Name = "morePanel3";
			this.morePanel3.Padding = new System.Windows.Forms.Padding(3);
			this.morePanel3.Size = new System.Drawing.Size(782, 64);
			this.morePanel3.TabIndex = 0;
			this.morePanel3.ThemedBack = null;
			this.morePanel3.ThemedFore = null;
			this.morePanel3.TopBorderColor = System.Drawing.SystemColors.Control;
			this.morePanel3.TopBorderSize = 0;
			// 
			// SearchDialogActionControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.resultTree);
			this.Controls.Add(this.morePanel3);
			this.Controls.Add(this.morePanel2);
			this.Controls.Add(this.morePanel1);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "SearchDialogActionControl";
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

		private System.Windows.Forms.ComboBox scopeBox;
		private UI.HierarchyView resultTree;
		private UI.MoreButton copyButton;
		private UI.MoreButton moveButton;
		private UI.MoreButton searchButton;
		private UI.MoreButton cancelButton;
		private UI.MoreTextBox findBox;
		private UI.MoreLabel findLabel;
		private UI.MoreLabel introLabel;
		private UI.MorePanel morePanel1;
		private UI.MorePanel morePanel2;
		private UI.MorePanel morePanel3;
	}
}
