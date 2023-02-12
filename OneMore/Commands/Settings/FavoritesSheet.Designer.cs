namespace River.OneMoreAddIn.Settings
{
	partial class FavoritesSheet
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FavoritesSheet));
			this.gridView = new System.Windows.Forms.DataGridView();
			this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.locationColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.introPanel = new System.Windows.Forms.Panel();
			this.optionsBox = new System.Windows.Forms.GroupBox();
			this.shortcutsBox = new System.Windows.Forms.CheckBox();
			this.toolStrip = new River.OneMoreAddIn.UI.ScaledToolStrip();
			this.sortButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.upButton = new System.Windows.Forms.ToolStripButton();
			this.downButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.deleteLabel = new System.Windows.Forms.ToolStripLabel();
			this.deleteButton = new System.Windows.Forms.ToolStripButton();
			((System.ComponentModel.ISupportInitialize)(this.gridView)).BeginInit();
			this.introPanel.SuspendLayout();
			this.optionsBox.SuspendLayout();
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
            this.locationColumn});
			this.gridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
			this.gridView.Location = new System.Drawing.Point(10, 132);
			this.gridView.MultiSelect = false;
			this.gridView.Name = "gridView";
			this.gridView.RowHeadersVisible = false;
			this.gridView.RowHeadersWidth = 30;
			this.gridView.RowTemplate.Height = 28;
			this.gridView.ShowEditingIcon = false;
			this.gridView.Size = new System.Drawing.Size(780, 359);
			this.gridView.TabIndex = 2;
			this.gridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.FormatCell);
			// 
			// nameColumn
			// 
			this.nameColumn.HeaderText = "Name";
			this.nameColumn.MinimumWidth = 100;
			this.nameColumn.Name = "nameColumn";
			this.nameColumn.Width = 250;
			// 
			// locationColumn
			// 
			this.locationColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.locationColumn.FillWeight = 1000F;
			this.locationColumn.HeaderText = "Location";
			this.locationColumn.MinimumWidth = 8;
			this.locationColumn.Name = "locationColumn";
			// 
			// introPanel
			// 
			this.introPanel.Controls.Add(this.optionsBox);
			this.introPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.introPanel.Location = new System.Drawing.Point(10, 9);
			this.introPanel.Name = "introPanel";
			this.introPanel.Size = new System.Drawing.Size(780, 90);
			this.introPanel.TabIndex = 3;
			// 
			// optionsBox
			// 
			this.optionsBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.optionsBox.Controls.Add(this.shortcutsBox);
			this.optionsBox.Location = new System.Drawing.Point(3, 3);
			this.optionsBox.Name = "optionsBox";
			this.optionsBox.Size = new System.Drawing.Size(774, 75);
			this.optionsBox.TabIndex = 0;
			this.optionsBox.TabStop = false;
			this.optionsBox.Text = "Options";
			// 
			// shortcutsBox
			// 
			this.shortcutsBox.AutoSize = true;
			this.shortcutsBox.Location = new System.Drawing.Point(23, 32);
			this.shortcutsBox.Margin = new System.Windows.Forms.Padding(20, 10, 3, 3);
			this.shortcutsBox.Name = "shortcutsBox";
			this.shortcutsBox.Size = new System.Drawing.Size(356, 24);
			this.shortcutsBox.TabIndex = 0;
			this.shortcutsBox.Text = "Include reference to keyboard shortcuts page";
			this.shortcutsBox.UseVisualStyleBackColor = true;
			// 
			// toolStrip
			// 
			this.toolStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sortButton,
            this.toolStripSeparator2,
            this.upButton,
            this.downButton,
            this.toolStripSeparator1,
            this.deleteLabel,
            this.deleteButton});
			this.toolStrip.Location = new System.Drawing.Point(10, 99);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
			this.toolStrip.Size = new System.Drawing.Size(780, 33);
			this.toolStrip.TabIndex = 4;
			this.toolStrip.Text = "Tool Strip";
			// 
			// sortButton
			// 
			this.sortButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.sortButton.Image = ((System.Drawing.Image)(resources.GetObject("sortButton.Image")));
			this.sortButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.sortButton.Name = "sortButton";
			this.sortButton.Size = new System.Drawing.Size(34, 28);
			this.sortButton.Text = "toolStripButton1";
			this.sortButton.ToolTipText = "Sort by Name";
			this.sortButton.Click += new System.EventHandler(this.SortItems);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 33);
			// 
			// upButton
			// 
			this.upButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.upButton.Image = global::River.OneMoreAddIn.Properties.Resources.UpArrow;
			this.upButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.upButton.Name = "upButton";
			this.upButton.Size = new System.Drawing.Size(34, 28);
			this.upButton.Text = "Move up";
			this.upButton.Click += new System.EventHandler(this.MoveItemUp);
			// 
			// downButton
			// 
			this.downButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.downButton.Image = global::River.OneMoreAddIn.Properties.Resources.DownArrow;
			this.downButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.downButton.Name = "downButton";
			this.downButton.Size = new System.Drawing.Size(34, 28);
			this.downButton.Text = "Move down";
			this.downButton.Click += new System.EventHandler(this.MoveItemDown);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 33);
			// 
			// deleteLabel
			// 
			this.deleteLabel.Name = "deleteLabel";
			this.deleteLabel.Size = new System.Drawing.Size(66, 28);
			this.deleteLabel.Text = "Delete:";
			// 
			// deleteButton
			// 
			this.deleteButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.deleteButton.Image = global::River.OneMoreAddIn.Properties.Resources.Delete;
			this.deleteButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.Size = new System.Drawing.Size(34, 28);
			this.deleteButton.Text = "Delete";
			this.deleteButton.Click += new System.EventHandler(this.DeleteItem);
			// 
			// FavoritesSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.gridView);
			this.Controls.Add(this.toolStrip);
			this.Controls.Add(this.introPanel);
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MinimumSize = new System.Drawing.Size(750, 400);
			this.Name = "FavoritesSheet";
			this.Padding = new System.Windows.Forms.Padding(10, 9, 10, 9);
			this.Size = new System.Drawing.Size(800, 500);
			this.Load += new System.EventHandler(this.LoadData);
			((System.ComponentModel.ISupportInitialize)(this.gridView)).EndInit();
			this.introPanel.ResumeLayout(false);
			this.optionsBox.ResumeLayout(false);
			this.optionsBox.PerformLayout();
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
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton upButton;
		private System.Windows.Forms.ToolStripButton downButton;
		private System.Windows.Forms.ToolStripLabel deleteLabel;
		private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn locationColumn;
		private System.Windows.Forms.ToolStripButton sortButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.GroupBox optionsBox;
		private System.Windows.Forms.CheckBox shortcutsBox;
	}
}