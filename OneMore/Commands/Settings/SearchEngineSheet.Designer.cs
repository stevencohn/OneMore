namespace River.OneMoreAddIn.Settings
{
	partial class SearchEngineSheet
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SearchEngineSheet));
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			this.gridView = new UI.MoreDataGridView();
			this.iconColumn = new System.Windows.Forms.DataGridViewImageColumn();
			this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.urlColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.introBox = new River.OneMoreAddIn.UI.MoreMultilineLabel();
			this.toolStrip = new River.OneMoreAddIn.UI.MoreToolStrip();
			this.upButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.downButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.refreshButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.deleteButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.gridView)).BeginInit();
			this.toolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// gridView
			// 
			this.gridView.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
			this.gridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.gridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.iconColumn,
            this.nameColumn,
            this.urlColumn});
			this.gridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridView.Location = new System.Drawing.Point(10, 128);
			this.gridView.MultiSelect = false;
			this.gridView.Name = "gridView";
			this.gridView.RowHeadersVisible = false;
			this.gridView.RowHeadersWidth = 30;
			this.gridView.RowTemplate.Height = 28;
			this.gridView.Size = new System.Drawing.Size(780, 363);
			this.gridView.TabIndex = 2;
			this.gridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridView_CellEndEdit);
			this.gridView.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.gridView_DataBindingComplete);
			// 
			// iconColumn
			// 
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle1.ForeColor = System.Drawing.Color.Black;
			dataGridViewCellStyle1.NullValue = ((object)(resources.GetObject("dataGridViewCellStyle1.NullValue")));
			this.iconColumn.DefaultCellStyle = dataGridViewCellStyle1;
			this.iconColumn.HeaderText = "Icon";
			this.iconColumn.Image = global::River.OneMoreAddIn.Properties.Resources.m_NewStyle;
			this.iconColumn.MinimumWidth = 36;
			this.iconColumn.Name = "iconColumn";
			this.iconColumn.ReadOnly = true;
			this.iconColumn.Width = 36;
			// 
			// nameColumn
			// 
			dataGridViewCellStyle2.ForeColor = System.Drawing.Color.Black;
			this.nameColumn.DefaultCellStyle = dataGridViewCellStyle2;
			this.nameColumn.HeaderText = "Name";
			this.nameColumn.MinimumWidth = 100;
			this.nameColumn.Name = "nameColumn";
			this.nameColumn.Width = 150;
			// 
			// urlColumn
			// 
			this.urlColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			dataGridViewCellStyle3.ForeColor = System.Drawing.Color.Black;
			this.urlColumn.DefaultCellStyle = dataGridViewCellStyle3;
			this.urlColumn.FillWeight = 1000F;
			this.urlColumn.HeaderText = "URL Pattern";
			this.urlColumn.MinimumWidth = 8;
			this.urlColumn.Name = "urlColumn";
			// 
			// introBox
			// 
			this.introBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.introBox.Location = new System.Drawing.Point(10, 9);
			this.introBox.Name = "introBox";
			this.introBox.Padding = new System.Windows.Forms.Padding(0, 0, 0, 36);
			this.introBox.Size = new System.Drawing.Size(780, 81);
			this.introBox.TabIndex = 0;
			this.introBox.Text = "URL Patterns must contain a string replacement token, such as &q={0}\r\nSearch engi" +
    "nes will appear in the page context menu, right-click.";
			this.introBox.ThemedBack = "ControlLightLight";
			this.introBox.ThemedFore = null;
			// 
			// toolStrip
			// 
			this.toolStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.upButton,
            this.downButton,
            this.refreshButton,
            this.toolStripSeparator1,
            this.deleteButton});
			this.toolStrip.Location = new System.Drawing.Point(10, 90);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
			this.toolStrip.Size = new System.Drawing.Size(780, 38);
			this.toolStrip.TabIndex = 4;
			this.toolStrip.Text = "Tool Strip";
			// 
			// upButton
			// 
			this.upButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.upButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_MoveUp;
			this.upButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.upButton.Name = "upButton";
			this.upButton.Size = new System.Drawing.Size(40, 38);
			this.upButton.Text = "Move up";
			this.upButton.Click += new System.EventHandler(this.upButton_Click);
			// 
			// downButton
			// 
			this.downButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.downButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_MoveDown;
			this.downButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.downButton.Name = "downButton";
			this.downButton.Size = new System.Drawing.Size(40, 38);
			this.downButton.Text = "Move down";
			this.downButton.Click += new System.EventHandler(this.downButton_Click);
			// 
			// refreshButton
			// 
			this.refreshButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.refreshButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Refresh;
			this.refreshButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.refreshButton.Name = "refreshButton";
			this.refreshButton.Size = new System.Drawing.Size(40, 38);
			this.refreshButton.Text = "Refresh";
			this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 38);
			// 
			// deleteButton
			// 
			this.deleteButton.ForeColor = System.Drawing.Color.Black;
			this.deleteButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Delete;
			this.deleteButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.Size = new System.Drawing.Size(102, 38);
			this.deleteButton.Text = "Delete";
			this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
			// 
			// SearchEngineSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.gridView);
			this.Controls.Add(this.toolStrip);
			this.Controls.Add(this.introBox);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MinimumSize = new System.Drawing.Size(750, 400);
			this.Name = "SearchEngineSheet";
			this.Padding = new System.Windows.Forms.Padding(10, 9, 10, 9);
			this.Size = new System.Drawing.Size(800, 500);
			((System.ComponentModel.ISupportInitialize)(this.gridView)).EndInit();
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreDataGridView gridView;
		private UI.MoreMultilineLabel introBox;
		private UI.MoreToolStrip toolStrip;
		private UI.MoreMenuItem deleteButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private UI.MoreMenuItem upButton;
		private UI.MoreMenuItem downButton;
		private UI.MoreMenuItem refreshButton;
		private System.Windows.Forms.DataGridViewImageColumn iconColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn urlColumn;
	}
}