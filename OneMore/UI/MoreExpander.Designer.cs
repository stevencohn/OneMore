namespace River.OneMoreAddIn.UI
{
	partial class MoreExpander
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

				image?.Dispose();
				grayed?.Dispose();
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
			this.header = new River.OneMoreAddIn.UI.MorePictureBox();
			((System.ComponentModel.ISupportInitialize)(this.header)).BeginInit();
			this.SuspendLayout();
			// 
			// header
			// 
			this.header.Cursor = System.Windows.Forms.Cursors.Hand;
			this.header.Dock = System.Windows.Forms.DockStyle.Fill;
			this.header.Location = new System.Drawing.Point(0, 0);
			this.header.Name = "header";
			this.header.Size = new System.Drawing.Size(550, 50);
			this.header.TabIndex = 0;
			this.header.TabStop = false;
			this.header.Click += new System.EventHandler(this.Toggle);
			this.header.Paint += new System.Windows.Forms.PaintEventHandler(this.Repaint);
			// 
			// MoreExpander
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.header);
			this.ForeColor = System.Drawing.SystemColors.ActiveCaption;
			this.Name = "MoreExpander";
			this.Size = new System.Drawing.Size(550, 50);
			((System.ComponentModel.ISupportInitialize)(this.header)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private River.OneMoreAddIn.UI.MorePictureBox header;
	}
}
