namespace River.OneMoreAddIn
{
	partial class FlatToggleButton
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose (bool disposing)
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
		private void InitializeComponent ()
		{
			this.pictureBox = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// pictureBox
			// 
			this.pictureBox.BackColor = System.Drawing.Color.Transparent;
			this.pictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pictureBox.Location = new System.Drawing.Point(4, 4);
			this.pictureBox.Name = "pictureBox";
			this.pictureBox.Size = new System.Drawing.Size(42, 22);
			this.pictureBox.TabIndex = 0;
			this.pictureBox.TabStop = false;
			this.pictureBox.Click += new System.EventHandler(this.ClickPictureBox);
			this.pictureBox.MouseEnter += new System.EventHandler(this.MakeItHot);
			this.pictureBox.MouseLeave += new System.EventHandler(this.MakeItCool);
			// 
			// FlatToggleButton
			// 
			this.Controls.Add(this.pictureBox);
			this.Name = "FlatToggleButton";
			this.Padding = new System.Windows.Forms.Padding(4);
			this.Size = new System.Drawing.Size(50, 30);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.FlatToggleButton_Paint);
			this.MouseEnter += new System.EventHandler(this.MakeItHot);
			this.MouseLeave += new System.EventHandler(this.MakeItCool);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PictureBox pictureBox;
	}
}
