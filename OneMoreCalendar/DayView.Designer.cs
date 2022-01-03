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
			this.listbox = new System.Windows.Forms.ListBox();
			this.SuspendLayout();
			// 
			// listbox
			// 
			this.listbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listbox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listbox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
			this.listbox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.listbox.FormattingEnabled = true;
			this.listbox.Location = new System.Drawing.Point(0, 0);
			this.listbox.Margin = new System.Windows.Forms.Padding(0);
			this.listbox.Name = "listbox";
			this.listbox.SelectionMode = System.Windows.Forms.SelectionMode.None;
			this.listbox.Size = new System.Drawing.Size(743, 409);
			this.listbox.TabIndex = 0;
			this.listbox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.DrawDay);
			this.listbox.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.MeasureDay);
			this.listbox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ScrollDays);
			this.listbox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HoverHighlight);
			// 
			// DayView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.listbox);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "DayView";
			this.Size = new System.Drawing.Size(743, 409);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListBox listbox;
	}
}
