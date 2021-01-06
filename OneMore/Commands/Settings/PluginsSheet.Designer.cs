namespace River.OneMoreAddIn.Settings
{
	partial class PluginsSheet
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PluginsSheet));
			this.gridView = new System.Windows.Forms.DataGridView();
			this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.cmdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.introPanel = new System.Windows.Forms.Panel();
			this.introLabel = new System.Windows.Forms.Label();
			this.toolStrip = new River.OneMoreAddIn.UI.ScaledToolStrip();
			this.deleteLabel = new System.Windows.Forms.ToolStripLabel();
			this.deleteButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
			this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
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
            this.nameColumn,
            this.cmdColumn});
			this.gridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
			this.gridView.Location = new System.Drawing.Point(7, 73);
			this.gridView.Margin = new System.Windows.Forms.Padding(2);
			this.gridView.MultiSelect = false;
			this.gridView.Name = "gridView";
			this.gridView.RowHeadersVisible = false;
			this.gridView.RowHeadersWidth = 30;
			this.gridView.RowTemplate.Height = 28;
			this.gridView.ShowEditingIcon = false;
			this.gridView.Size = new System.Drawing.Size(519, 246);
			this.gridView.TabIndex = 2;
			// 
			// nameColumn
			// 
			this.nameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.nameColumn.DataPropertyName = "Name";
			this.nameColumn.HeaderText = "Name";
			this.nameColumn.MinimumWidth = 100;
			this.nameColumn.Name = "nameColumn";
			// 
			// cmdColumn
			// 
			this.cmdColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.cmdColumn.DataPropertyName = "FullCommand";
			this.cmdColumn.HeaderText = "Command";
			this.cmdColumn.Name = "cmdColumn";
			// 
			// introPanel
			// 
			this.introPanel.Controls.Add(this.introLabel);
			this.introPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.introPanel.Location = new System.Drawing.Point(7, 6);
			this.introPanel.Margin = new System.Windows.Forms.Padding(2);
			this.introPanel.Name = "introPanel";
			this.introPanel.Size = new System.Drawing.Size(519, 36);
			this.introPanel.TabIndex = 3;
			// 
			// introLabel
			// 
			this.introLabel.AutoSize = true;
			this.introLabel.Location = new System.Drawing.Point(2, 3);
			this.introLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(141, 13);
			this.introLabel.TabIndex = 0;
			this.introLabel.Text = "Manage my custom snippets";
			// 
			// toolStrip
			// 
			this.toolStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.toolStripButton2,
            this.toolStripSeparator1,
            this.deleteLabel,
            this.deleteButton});
			this.toolStrip.Location = new System.Drawing.Point(7, 42);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
			this.toolStrip.Size = new System.Drawing.Size(519, 31);
			this.toolStrip.Stretch = true;
			this.toolStrip.TabIndex = 4;
			this.toolStrip.Text = "Tool Strip";
			// 
			// deleteLabel
			// 
			this.deleteLabel.Name = "deleteLabel";
			this.deleteLabel.Size = new System.Drawing.Size(43, 28);
			this.deleteLabel.Text = "Delete:";
			// 
			// deleteButton
			// 
			this.deleteButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.deleteButton.Image = global::River.OneMoreAddIn.Properties.Resources.Delete;
			this.deleteButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.Size = new System.Drawing.Size(28, 28);
			this.deleteButton.Text = "Delete";
			this.deleteButton.Click += new System.EventHandler(this.DeleteItem);
			// 
			// toolStripButton1
			// 
			this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
			this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton1.Name = "toolStripButton1";
			this.toolStripButton1.Size = new System.Drawing.Size(28, 28);
			this.toolStripButton1.Text = "editButton";
			this.toolStripButton1.Click += new System.EventHandler(this.EditSelection);
			// 
			// toolStripButton2
			// 
			this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton2.Image")));
			this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton2.Name = "toolStripButton2";
			this.toolStripButton2.Size = new System.Drawing.Size(28, 28);
			this.toolStripButton2.Text = "renameButton";
			this.toolStripButton2.Click += new System.EventHandler(this.RenameSelection);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 31);
			// 
			// PluginsSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.gridView);
			this.Controls.Add(this.toolStrip);
			this.Controls.Add(this.introPanel);
			this.MinimumSize = new System.Drawing.Size(500, 260);
			this.Name = "PluginsSheet";
			this.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.Size = new System.Drawing.Size(533, 325);
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
		private System.Windows.Forms.ToolStripLabel deleteLabel;
		private System.Windows.Forms.Label introLabel;
		private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn cmdColumn;
		private System.Windows.Forms.ToolStripButton toolStripButton1;
		private System.Windows.Forms.ToolStripButton toolStripButton2;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
	}
}