namespace OneMoreCalendar
{
	partial class DetailPagePanel
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
			this.layout = new System.Windows.Forms.FlowLayoutPanel();
			this.SuspendLayout();
			// 
			// layout
			// 
			this.layout.AutoSize = true;
			this.layout.BackColor = System.Drawing.Color.White;
			this.layout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.layout.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.layout.Location = new System.Drawing.Point(0, 0);
			this.layout.Margin = new System.Windows.Forms.Padding(0);
			this.layout.Name = "layout";
			this.layout.Size = new System.Drawing.Size(418, 209);
			this.layout.TabIndex = 0;
			this.layout.WrapContents = false;
			// 
			// DetailPagePanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.layout);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "DetailPagePanel";
			this.Size = new System.Drawing.Size(418, 209);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.FlowLayoutPanel layout;
	}
}
