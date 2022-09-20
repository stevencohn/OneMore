namespace River.OneMoreAddIn.Settings
{
	partial class SnippetsSheet
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
			this.gridView = new System.Windows.Forms.DataGridView();
			this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.introPanel = new System.Windows.Forms.Panel();
			this.introLabel = new System.Windows.Forms.Label();
			this.toolStrip = new River.OneMoreAddIn.UI.ScaledToolStrip();
			this.renameButton = new System.Windows.Forms.ToolStripButton();
			this.deleteButton = new System.Windows.Forms.ToolStripButton();
			((System.ComponentModel.ISupportInitialize)(this.gridView)).BeginInit();
			this.introPanel.SuspendLayout();
			this.toolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// gridView
			// 
			this.gridView.AllowUserToAddRows = false;
			this.gridView.AllowUserToResizeRows = false;
			this.gridView.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
			this.gridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.gridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.nameColumn});
			this.gridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
			this.gridView.Location = new System.Drawing.Point(10, 98);
			this.gridView.MultiSelect = false;
			this.gridView.Name = "gridView";
			this.gridView.RowHeadersVisible = false;
			this.gridView.RowHeadersWidth = 30;
			this.gridView.RowTemplate.Height = 28;
			this.gridView.ShowEditingIcon = false;
			this.gridView.Size = new System.Drawing.Size(780, 393);
			this.gridView.TabIndex = 2;
			// 
			// nameColumn
			// 
			this.nameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.nameColumn.HeaderText = "Name";
			this.nameColumn.MinimumWidth = 100;
			this.nameColumn.Name = "nameColumn";
			// 
			// introPanel
			// 
			this.introPanel.Controls.Add(this.introLabel);
			this.introPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.introPanel.Location = new System.Drawing.Point(10, 9);
			this.introPanel.Name = "introPanel";
			this.introPanel.Size = new System.Drawing.Size(780, 55);
			this.introPanel.TabIndex = 3;
			// 
			// introLabel
			// 
			this.introLabel.AutoSize = true;
			this.introLabel.Location = new System.Drawing.Point(3, 0);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(211, 20);
			this.introLabel.TabIndex = 0;
			this.introLabel.Text = "Manage my custom snippets";
			// 
			// toolStrip
			// 
			this.toolStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.renameButton,
            this.deleteButton});
			this.toolStrip.Location = new System.Drawing.Point(10, 64);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
			this.toolStrip.Size = new System.Drawing.Size(780, 34);
			this.toolStrip.Stretch = true;
			this.toolStrip.TabIndex = 4;
			this.toolStrip.Text = "Tool Strip";
			// 
			// renameButton
			// 
			this.renameButton.Image = global::River.OneMoreAddIn.Properties.Resources.Rename;
			this.renameButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.renameButton.Name = "renameButton";
			this.renameButton.Size = new System.Drawing.Size(103, 29);
			this.renameButton.Text = "Rename";
			this.renameButton.Click += new System.EventHandler(this.RenameItem);
			// 
			// deleteButton
			// 
			this.deleteButton.Image = global::River.OneMoreAddIn.Properties.Resources.Delete;
			this.deleteButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.Size = new System.Drawing.Size(90, 29);
			this.deleteButton.Text = "Delete";
			this.deleteButton.Click += new System.EventHandler(this.DeleteItem);
			// 
			// SnippetsSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.gridView);
			this.Controls.Add(this.toolStrip);
			this.Controls.Add(this.introPanel);
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MinimumSize = new System.Drawing.Size(750, 400);
			this.Name = "SnippetsSheet";
			this.Padding = new System.Windows.Forms.Padding(10, 9, 10, 9);
			this.Size = new System.Drawing.Size(800, 500);
			((System.ComponentModel.ISupportInitialize)(this.gridView)).EndInit();
			this.introPanel.ResumeLayout(false);
			this.introPanel.PerformLayout();
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.DataGridView gridView;
		private System.Windows.Forms.Panel introPanel;
		private UI.ScaledToolStrip toolStrip;
		private System.Windows.Forms.ToolStripButton deleteButton;
		private System.Windows.Forms.Label introLabel;
		private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
		private System.Windows.Forms.ToolStripButton renameButton;
	}
}