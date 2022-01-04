namespace OneMoreCalendar
{
	partial class YearsForm
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
			this.listView = new System.Windows.Forms.ListView();
			this.SuspendLayout();
			// 
			// listView
			// 
			this.listView.Activation = System.Windows.Forms.ItemActivation.OneClick;
			this.listView.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listView.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.listView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.listView.HideSelection = false;
			this.listView.HotTracking = true;
			this.listView.HoverSelection = true;
			this.listView.Location = new System.Drawing.Point(4, 4);
			this.listView.Margin = new System.Windows.Forms.Padding(0);
			this.listView.MultiSelect = false;
			this.listView.Name = "listView";
			this.listView.Size = new System.Drawing.Size(510, 442);
			this.listView.TabIndex = 0;
			this.listView.UseCompatibleStateImageBehavior = false;
			this.listView.View = System.Windows.Forms.View.List;
			// 
			// YearsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(518, 450);
			this.Controls.Add(this.listView);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "YearsForm";
			this.Padding = new System.Windows.Forms.Padding(4);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.TopMost = true;
			this.Deactivate += new System.EventHandler(this.YearsForm_Leave);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.YearsForm_KeyDown);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListView listView;
	}
}