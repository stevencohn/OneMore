namespace OneMoreCalendar
{
	partial class DayView
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
			this.listView = new System.Windows.Forms.ListView();
			this.titleColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.createdColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.modifiedColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.SuspendLayout();
			// 
			// listView
			// 
			this.listView.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.titleColumn,
            this.createdColumn,
            this.modifiedColumn});
			this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listView.GridLines = true;
			this.listView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listView.Location = new System.Drawing.Point(0, 0);
			this.listView.Margin = new System.Windows.Forms.Padding(0);
			this.listView.MultiSelect = false;
			this.listView.Name = "listView";
			this.listView.Size = new System.Drawing.Size(743, 409);
			this.listView.TabIndex = 0;
			this.listView.UseCompatibleStateImageBehavior = false;
			this.listView.View = System.Windows.Forms.View.Details;
			this.listView.VirtualListSize = 31;
			// 
			// titleColumn
			// 
			this.titleColumn.Text = "Page";
			this.titleColumn.Width = 300;
			// 
			// createdColumn
			// 
			this.createdColumn.Text = "Created";
			this.createdColumn.Width = 200;
			// 
			// modifiedColumn
			// 
			this.modifiedColumn.Text = "Modified";
			this.modifiedColumn.Width = 200;
			// 
			// DayView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.listView);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "DayView";
			this.Size = new System.Drawing.Size(743, 409);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListView listView;
		private System.Windows.Forms.ColumnHeader titleColumn;
		private System.Windows.Forms.ColumnHeader createdColumn;
		private System.Windows.Forms.ColumnHeader modifiedColumn;
	}
}
