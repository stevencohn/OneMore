﻿namespace River.OneMoreAddIn.Commands
{
	partial class ImportOutlookTasksDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportOutlookTasksDialog));
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.introBox = new System.Windows.Forms.TextBox();
			this.imageList = new System.Windows.Forms.ImageList(this.components);
			this.tree = new Aga.Controls.Tree.TreeViewAdv();
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.buttonPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(563, 6);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.TabIndex = 9;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(669, 6);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.TabIndex = 10;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// introBox
			// 
			this.introBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.introBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.introBox.Enabled = false;
			this.introBox.Location = new System.Drawing.Point(20, 20);
			this.introBox.Multiline = true;
			this.introBox.Name = "introBox";
			this.introBox.ReadOnly = true;
			this.introBox.Size = new System.Drawing.Size(772, 64);
			this.introBox.TabIndex = 0;
			this.introBox.Text = "Choose an entire folder or individual tasks to import from Outlook into OneNote. " +
    "Disabled items indicate tasks already linked to OneNote";
			// 
			// imageList
			// 
			this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
			this.imageList.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList.Images.SetKeyName(0, "FolderClose.png");
			this.imageList.Images.SetKeyName(1, "FolderOpen.png");
			this.imageList.Images.SetKeyName(2, "Task.png");
			// 
			// tree
			// 
			this.tree.AutoRowHeight = true;
			this.tree.BackColor = System.Drawing.SystemColors.Window;
			this.tree.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.tree.ColumnHeaderHeight = 0;
			this.tree.Cursor = System.Windows.Forms.Cursors.Default;
			this.tree.DefaultToolTipProvider = null;
			this.tree.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tree.DragDropMarkColor = System.Drawing.Color.Black;
			this.tree.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tree.FullRowSelectActiveColor = System.Drawing.Color.Empty;
			this.tree.FullRowSelectInactiveColor = System.Drawing.Color.Empty;
			this.tree.LineColor = System.Drawing.SystemColors.ControlDark;
			this.tree.LoadOnDemand = true;
			this.tree.Location = new System.Drawing.Point(20, 84);
			this.tree.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tree.Model = null;
			this.tree.Name = "tree";
			this.tree.NodeFilter = null;
			this.tree.SelectedNode = null;
			this.tree.SelectionMode = Aga.Controls.Tree.TreeSelectionMode.MultiSameParent;
			this.tree.ShowPerformance = false;
			this.tree.Size = new System.Drawing.Size(772, 440);
			this.tree.TabIndex = 0;
			// 
			// buttonPanel
			// 
			this.buttonPanel.Controls.Add(this.cancelButton);
			this.buttonPanel.Controls.Add(this.okButton);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.Location = new System.Drawing.Point(20, 524);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Size = new System.Drawing.Size(772, 47);
			this.buttonPanel.TabIndex = 13;
			// 
			// ImportOutlookTasksDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(812, 591);
			this.Controls.Add(this.tree);
			this.Controls.Add(this.introBox);
			this.Controls.Add(this.buttonPanel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ImportOutlookTasksDialog";
			this.Padding = new System.Windows.Forms.Padding(20);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Import Tasks from Outlook";
			this.buttonPanel.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.TextBox introBox;
		private System.Windows.Forms.Panel buttonPanel;
		private System.Windows.Forms.ImageList imageList;
		private Aga.Controls.Tree.TreeViewAdv tree;
	}
}