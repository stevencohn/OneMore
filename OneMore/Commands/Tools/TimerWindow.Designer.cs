
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
			this.toolstrip = new River.OneMoreAddIn.UI.MoreToolStrip();
			this.copyButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.spacer1 = new System.Windows.Forms.ToolStripLabel();
			this.resetButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.spacer2 = new System.Windows.Forms.ToolStripLabel();
			this.closeButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.toolstrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// timeLabel
			// 
			this.timeLabel.BackColor = System.Drawing.Color.Transparent;
			this.timeLabel.Font = new System.Drawing.Font("Stencil", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.timeLabel.Location = new System.Drawing.Point(38, 14);
			this.timeLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.timeLabel.Name = "timeLabel";
			this.timeLabel.Size = new System.Drawing.Size(141, 43);
			this.timeLabel.TabIndex = 0;
			this.timeLabel.Text = "00:00:00";
			this.timeLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.StartWindowDrag);
			// 
			// toolstrip
			// 
			this.toolstrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.toolstrip.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.toolstrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolstrip.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.toolstrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyButton,
            this.spacer1,
            this.resetButton,
            this.spacer2,
            this.closeButton});
			this.toolstrip.Location = new System.Drawing.Point(0, 59);
			this.toolstrip.Name = "toolstrip";
			this.toolstrip.Padding = new System.Windows.Forms.Padding(38, 0, 0, 0);
			this.toolstrip.Size = new System.Drawing.Size(210, 38);
			this.toolstrip.TabIndex = 1;
			this.toolstrip.Text = "Close Timer";
			this.toolstrip.MouseDown += new System.Windows.Forms.MouseEventHandler(this.StartWindowDrag);
			// 
			// copyButton
			// 
			this.copyButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.copyButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Copy;
			this.copyButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.copyButton.Name = "copyButton";
			this.copyButton.Size = new System.Drawing.Size(40, 38);
			this.copyButton.Text = "Copy Current Time";
			this.copyButton.Click += new System.EventHandler(this.CopyTime);
			// 
			// spacer1
			// 
			this.spacer1.Name = "spacer1";
			this.spacer1.Size = new System.Drawing.Size(17, 33);
			this.spacer1.Text = " ";
			// 
			// resetButton
			// 
			this.resetButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.resetButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Restart;
			this.resetButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.resetButton.Name = "resetButton";
			this.resetButton.Size = new System.Drawing.Size(40, 38);
			this.resetButton.Text = "Restart Timer";
			this.resetButton.Click += new System.EventHandler(this.RestartTimer);
			// 
			// spacer2
			// 
			this.spacer2.Name = "spacer2";
			this.spacer2.Size = new System.Drawing.Size(17, 25);
			this.spacer2.Text = " ";
			// 
			// closeButton
			// 
			this.closeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.closeButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Stop;
			this.closeButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(40, 38);
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
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Thistle;
			this.ClientSize = new System.Drawing.Size(210, 97);
			this.Controls.Add(this.toolstrip);
			this.Controls.Add(this.timeLabel);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "TimerWindow";
			this.Opacity = 0.85D;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "OneNote Timer";
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.StartWindowDrag);
			this.Move += new System.EventHandler(this.MoveWindow);
			this.toolstrip.ResumeLayout(false);
			this.toolstrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label timeLabel;
		private UI.MoreToolStrip toolstrip;
		private UI.MoreMenuItem copyButton;
		private UI.MoreMenuItem resetButton;
		private UI.MoreMenuItem closeButton;
		private System.Windows.Forms.Timer timer;
		private System.Windows.Forms.ToolStripLabel spacer1;
		private System.Windows.Forms.ToolStripLabel spacer2;
	}
}