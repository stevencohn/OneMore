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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SearchDialog));
			this.introLabel = new System.Windows.Forms.Label();
			this.findLabel = new System.Windows.Forms.Label();
			this.findBox = new System.Windows.Forms.TextBox();
			this.cancelButton = new System.Windows.Forms.Button();
			this.searchButton = new System.Windows.Forms.Button();
			this.allButton = new System.Windows.Forms.RadioButton();
			this.notebookButton = new System.Windows.Forms.RadioButton();
			this.sectionButton = new System.Windows.Forms.RadioButton();
			this.copyButton = new System.Windows.Forms.Button();
			this.moveButton = new System.Windows.Forms.Button();
			this.resultTree = new System.Windows.Forms.TreeView();
			this.treeImages = new System.Windows.Forms.ImageList(this.components);
			this.SuspendLayout();
			// 
			// introLabel
			// 
			this.introLabel.AutoSize = true;
			this.introLabel.Location = new System.Drawing.Point(27, 23);
			this.introLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 15);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(579, 20);
			this.introLabel.TabIndex = 0;
			this.introLabel.Text = "Enter one or more keywords, \"quoted phrases\", and use uppercase AND and OR";
			// 
			// findLabel
			// 
			this.findLabel.AutoSize = true;
			this.findLabel.Location = new System.Drawing.Point(27, 68);
			this.findLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.findLabel.Name = "findLabel";
			this.findLabel.Size = new System.Drawing.Size(44, 20);
			this.findLabel.TabIndex = 1;
			this.findLabel.Text = "Find:";
			// 
			// findBox
			// 
			this.findBox.Location = new System.Drawing.Point(81, 63);
			this.findBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.findBox.Name = "findBox";
			this.findBox.Size = new System.Drawing.Size(594, 26);
			this.findBox.TabIndex = 2;
			this.findBox.TextChanged += new System.EventHandler(this.ChangeQuery);
			this.findBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SearchOnKeydown);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(564, 188);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 15);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(112, 35);
			this.cancelButton.TabIndex = 3;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// searchButton
			// 
			this.searchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.searchButton.Enabled = false;
			this.searchButton.Location = new System.Drawing.Point(442, 188);
			this.searchButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 15);
			this.searchButton.Name = "searchButton";
			this.searchButton.Size = new System.Drawing.Size(112, 35);
			this.searchButton.TabIndex = 4;
			this.searchButton.Text = "Search";
			this.searchButton.UseVisualStyleBackColor = true;
			this.searchButton.Click += new System.EventHandler(this.Search);
			// 
			// allButton
			// 
			this.allButton.AutoSize = true;
			this.allButton.Checked = true;
			this.allButton.Location = new System.Drawing.Point(81, 103);
			this.allButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 0);
			this.allButton.Name = "allButton";
			this.allButton.Size = new System.Drawing.Size(146, 24);
			this.allButton.TabIndex = 5;
			this.allButton.TabStop = true;
			this.allButton.Text = "In all notebooks";
			this.allButton.UseVisualStyleBackColor = true;
			// 
			// notebookButton
			// 
			this.notebookButton.AutoSize = true;
			this.notebookButton.Location = new System.Drawing.Point(81, 134);
			this.notebookButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 0);
			this.notebookButton.Name = "notebookButton";
			this.notebookButton.Size = new System.Drawing.Size(148, 24);
			this.notebookButton.TabIndex = 6;
			this.notebookButton.Text = "In this notebook";
			this.notebookButton.UseVisualStyleBackColor = true;
			// 
			// sectionButton
			// 
			this.sectionButton.AutoSize = true;
			this.sectionButton.Location = new System.Drawing.Point(81, 165);
			this.sectionButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 0);
			this.sectionButton.Name = "sectionButton";
			this.sectionButton.Size = new System.Drawing.Size(132, 24);
			this.sectionButton.TabIndex = 7;
			this.sectionButton.Text = "In this section";
			this.sectionButton.UseVisualStyleBackColor = true;
			// 
			// copyButton
			// 
			this.copyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.copyButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.copyButton.Enabled = false;
			this.copyButton.Location = new System.Drawing.Point(442, 689);
			this.copyButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.copyButton.Name = "copyButton";
			this.copyButton.Size = new System.Drawing.Size(112, 35);
			this.copyButton.TabIndex = 10;
			this.copyButton.Text = "Copy";
			this.copyButton.UseVisualStyleBackColor = true;
			this.copyButton.Click += new System.EventHandler(this.CopyPressed);
			// 
			// moveButton
			// 
			this.moveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.moveButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.moveButton.Enabled = false;
			this.moveButton.Location = new System.Drawing.Point(564, 689);
			this.moveButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.moveButton.Name = "moveButton";
			this.moveButton.Size = new System.Drawing.Size(112, 35);
			this.moveButton.TabIndex = 9;
			this.moveButton.Text = "Move";
			this.moveButton.UseVisualStyleBackColor = true;
			this.moveButton.Click += new System.EventHandler(this.MovePressed);
			// 
			// resultTree
			// 
			this.resultTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.resultTree.CheckBoxes = true;
			this.resultTree.HideSelection = false;
			this.resultTree.ImageIndex = 0;
			this.resultTree.ImageList = this.treeImages;
			this.resultTree.Location = new System.Drawing.Point(31, 241);
			this.resultTree.Name = "resultTree";
			this.resultTree.SelectedImageIndex = 0;
			this.resultTree.ShowLines = false;
			this.resultTree.ShowRootLines = false;
			this.resultTree.Size = new System.Drawing.Size(645, 423);
			this.resultTree.TabIndex = 11;
			// 
			// treeImages
			// 
			this.treeImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("treeImages.ImageStream")));
			this.treeImages.TransparentColor = System.Drawing.Color.Transparent;
			this.treeImages.Images.SetKeyName(0, "NotebookMask.png");
			this.treeImages.Images.SetKeyName(1, "SectionGroup.png");
			this.treeImages.Images.SetKeyName(2, "SectionMask.png");
			// 
			// SearchDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(704, 744);
			this.Controls.Add(this.resultTree);
			this.Controls.Add(this.copyButton);
			this.Controls.Add(this.moveButton);
			this.Controls.Add(this.sectionButton);
			this.Controls.Add(this.notebookButton);
			this.Controls.Add(this.allButton);
			this.Controls.Add(this.searchButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.findBox);
			this.Controls.Add(this.findLabel);
			this.Controls.Add(this.introLabel);
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(726, 300);
			this.Name = "SearchDialog";
			this.Padding = new System.Windows.Forms.Padding(22, 23, 22, 15);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Search and Move or Copy";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label introLabel;
		private System.Windows.Forms.Label findLabel;
		private System.Windows.Forms.TextBox findBox;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button searchButton;
		private System.Windows.Forms.RadioButton allButton;
		private System.Windows.Forms.RadioButton notebookButton;
		private System.Windows.Forms.RadioButton sectionButton;
		private System.Windows.Forms.Button copyButton;
		private System.Windows.Forms.Button moveButton;
		private System.Windows.Forms.TreeView resultTree;
		private System.Windows.Forms.ImageList treeImages;
	}
}