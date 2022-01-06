namespace OneMoreCalendar
{
	partial class DetailView
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
			this.grid = new System.Windows.Forms.DataGridView();
			this.dateColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.sectionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.pageColumn = new System.Windows.Forms.DataGridViewLinkColumn();
			this.createdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.modifiedColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.grid.AllowUserToAddRows = false;
			this.grid.AllowUserToDeleteRows = false;
			this.grid.AllowUserToResizeRows = false;
			dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(250)))), ((int)(((byte)(254)))));
			this.grid.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
			this.grid.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
			this.grid.BackgroundColor = System.Drawing.Color.White;
			this.grid.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.grid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
			this.grid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.grid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
			this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dateColumn,
            this.sectionColumn,
            this.pageColumn,
            this.createdColumn,
            this.modifiedColumn});
			dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle8.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.grid.DefaultCellStyle = dataGridViewCellStyle8;
			this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grid.GridColor = System.Drawing.Color.LightGray;
			this.grid.Location = new System.Drawing.Point(0, 0);
			this.grid.Margin = new System.Windows.Forms.Padding(0);
			this.grid.MultiSelect = false;
			this.grid.Name = "grid";
			this.grid.ReadOnly = true;
			this.grid.RowHeadersVisible = false;
			this.grid.RowHeadersWidth = 62;
			this.grid.RowTemplate.Height = 28;
			this.grid.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.grid.Size = new System.Drawing.Size(763, 523);
			this.grid.TabIndex = 0;
			// 
			// dateColumn
			// 
			this.dateColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
			dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle3.Padding = new System.Windows.Forms.Padding(20, 0, 20, 0);
			this.dateColumn.DefaultCellStyle = dataGridViewCellStyle3;
			this.dateColumn.HeaderText = "Date";
			this.dateColumn.MinimumWidth = 120;
			this.dateColumn.Name = "dateColumn";
			this.dateColumn.ReadOnly = true;
			this.dateColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.dateColumn.Width = 120;
			// 
			// sectionColumn
			// 
			this.sectionColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
			dataGridViewCellStyle4.Padding = new System.Windows.Forms.Padding(0, 0, 20, 0);
			dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.sectionColumn.DefaultCellStyle = dataGridViewCellStyle4;
			this.sectionColumn.HeaderText = "Section";
			this.sectionColumn.MinimumWidth = 150;
			this.sectionColumn.Name = "sectionColumn";
			this.sectionColumn.ReadOnly = true;
			this.sectionColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.sectionColumn.Width = 150;
			// 
			// pageColumn
			// 
			this.pageColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			dataGridViewCellStyle5.Padding = new System.Windows.Forms.Padding(0, 0, 20, 0);
			dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.pageColumn.DefaultCellStyle = dataGridViewCellStyle5;
			this.pageColumn.HeaderText = "Page";
			this.pageColumn.MinimumWidth = 8;
			this.pageColumn.Name = "pageColumn";
			this.pageColumn.ReadOnly = true;
			this.pageColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.pageColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			// 
			// createdColumn
			// 
			this.createdColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
			dataGridViewCellStyle6.Padding = new System.Windows.Forms.Padding(0, 0, 20, 0);
			dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.createdColumn.DefaultCellStyle = dataGridViewCellStyle6;
			this.createdColumn.HeaderText = "Created";
			this.createdColumn.MinimumWidth = 120;
			this.createdColumn.Name = "createdColumn";
			this.createdColumn.ReadOnly = true;
			this.createdColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.createdColumn.Width = 120;
			// 
			// modifiedColumn
			// 
			this.modifiedColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
			dataGridViewCellStyle7.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
			dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.modifiedColumn.DefaultCellStyle = dataGridViewCellStyle7;
			this.modifiedColumn.HeaderText = "Modified";
			this.modifiedColumn.MinimumWidth = 120;
			this.modifiedColumn.Name = "modifiedColumn";
			this.modifiedColumn.ReadOnly = true;
			this.modifiedColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
			this.modifiedColumn.Width = 120;
			// 
			// DetailView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.grid);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "DetailView";
			this.Size = new System.Drawing.Size(763, 523);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataGridView grid;
		private System.Windows.Forms.DataGridViewTextBoxColumn dateColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn sectionColumn;
		private System.Windows.Forms.DataGridViewLinkColumn pageColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn createdColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn modifiedColumn;
	}
}
