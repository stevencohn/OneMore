
namespace River.OneMoreAddIn.Commands
{
	partial class TimerWindow
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TimerWindow));
			this.timeLabel = new System.Windows.Forms.Label();
			this.toolstrip = new River.OneMoreAddIn.UI.ScaledToolStrip();
			this.copyButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.resetButton = new System.Windows.Forms.ToolStripButton();
			this.closeButton = new System.Windows.Forms.ToolStripButton();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.toolstrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// timeLabel
			// 
			this.timeLabel.BackColor = System.Drawing.Color.Transparent;
			this.timeLabel.Font = new System.Drawing.Font("Stencil", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.timeLabel.Location = new System.Drawing.Point(25, 9);
			this.timeLabel.Name = "timeLabel";
			this.timeLabel.Size = new System.Drawing.Size(94, 28);
			this.timeLabel.TabIndex = 0;
			this.timeLabel.Text = "00:00:00";
			this.timeLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.StartWindowDrag);
			// 
			// toolstrip
			// 
			this.toolstrip.BackColor = System.Drawing.Color.Transparent;
			this.toolstrip.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.toolstrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyButton,
            this.toolStripSeparator1,
            this.resetButton,
            this.closeButton});
			this.toolstrip.Location = new System.Drawing.Point(0, 38);
			this.toolstrip.Name = "toolstrip";
			this.toolstrip.Padding = new System.Windows.Forms.Padding(20, 0, 1, 0);
			this.toolstrip.Size = new System.Drawing.Size(140, 25);
			this.toolstrip.TabIndex = 1;
			this.toolstrip.Text = "Close Timer";
			this.toolstrip.MouseDown += new System.Windows.Forms.MouseEventHandler(this.StartWindowDrag);
			// 
			// copyButton
			// 
			this.copyButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.copyButton.Image = ((System.Drawing.Image)(resources.GetObject("copyButton.Image")));
			this.copyButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.copyButton.Name = "copyButton";
			this.copyButton.Size = new System.Drawing.Size(23, 22);
			this.copyButton.Text = "Copy Current Time";
			this.copyButton.Click += new System.EventHandler(this.CopyTime);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// resetButton
			// 
			this.resetButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.resetButton.Image = global::River.OneMoreAddIn.Properties.Resources.Refresh;
			this.resetButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.resetButton.Name = "resetButton";
			this.resetButton.Size = new System.Drawing.Size(23, 22);
			this.resetButton.Text = "Restart Timer";
			this.resetButton.Click += new System.EventHandler(this.RestartTimer);
			// 
			// closeButton
			// 
			this.closeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.closeButton.Image = global::River.OneMoreAddIn.Properties.Resources.Delete;
			this.closeButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(23, 22);
			this.closeButton.Text = "Close Timer";
			this.closeButton.Click += new System.EventHandler(this.CloseWindow);
			// 
			// timer
			// 
			this.timer.Interval = 1000;
			this.timer.Tick += new System.EventHandler(this.Tick);
			// 
			// TimerWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Thistle;
			this.ClientSize = new System.Drawing.Size(140, 63);
			this.Controls.Add(this.toolstrip);
			this.Controls.Add(this.timeLabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "TimerWindow";
			this.Opacity = 0.8D;
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "CountdownWindow";
			this.Load += new System.EventHandler(this.TimerWindow_Load);
			this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.EscapeWindow);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.StartWindowDrag);
			this.toolstrip.ResumeLayout(false);
			this.toolstrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label timeLabel;
		private UI.ScaledToolStrip toolstrip;
		private System.Windows.Forms.ToolStripButton copyButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton resetButton;
		private System.Windows.Forms.ToolStripButton closeButton;
		private System.Windows.Forms.Timer timer;
	}
}