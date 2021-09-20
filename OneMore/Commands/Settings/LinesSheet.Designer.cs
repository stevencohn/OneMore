
namespace River.OneMoreAddIn.Settings
{
	partial class LinesSheet
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
			this.introBox = new System.Windows.Forms.TextBox();
			this.layoutPanel = new System.Windows.Forms.Panel();
			this.clickLabel = new System.Windows.Forms.Label();
			this.lengthBox = new System.Windows.Forms.NumericUpDown();
			this.lengthLabel = new System.Windows.Forms.Label();
			this.colorBox = new River.OneMoreAddIn.UI.MorePictureBox();
			this.colorLabel = new System.Windows.Forms.Label();
			this.layoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.lengthBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.colorBox)).BeginInit();
			this.SuspendLayout();
			// 
			// introBox
			// 
			this.introBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.introBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.introBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.introBox.Location = new System.Drawing.Point(13, 8);
			this.introBox.Multiline = true;
			this.introBox.Name = "introBox";
			this.introBox.ReadOnly = true;
			this.introBox.Size = new System.Drawing.Size(772, 66);
			this.introBox.TabIndex = 3;
			this.introBox.Text = "Customize the default style for horizontal line snippets";
			// 
			// layoutPanel
			// 
			this.layoutPanel.Controls.Add(this.clickLabel);
			this.layoutPanel.Controls.Add(this.lengthBox);
			this.layoutPanel.Controls.Add(this.lengthLabel);
			this.layoutPanel.Controls.Add(this.colorBox);
			this.layoutPanel.Controls.Add(this.colorLabel);
			this.layoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.layoutPanel.Location = new System.Drawing.Point(13, 74);
			this.layoutPanel.Margin = new System.Windows.Forms.Padding(0);
			this.layoutPanel.Name = "layoutPanel";
			this.layoutPanel.Size = new System.Drawing.Size(772, 416);
			this.layoutPanel.TabIndex = 4;
			// 
			// clickLabel
			// 
			this.clickLabel.AutoSize = true;
			this.clickLabel.Location = new System.Drawing.Point(231, 26);
			this.clickLabel.Name = "clickLabel";
			this.clickLabel.Size = new System.Drawing.Size(124, 20);
			this.clickLabel.TabIndex = 4;
			this.clickLabel.Text = "(click to change)";
			// 
			// lengthBox
			// 
			this.lengthBox.Location = new System.Drawing.Point(93, 75);
			this.lengthBox.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.lengthBox.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.lengthBox.Name = "lengthBox";
			this.lengthBox.Size = new System.Drawing.Size(132, 26);
			this.lengthBox.TabIndex = 3;
			this.lengthBox.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
			// 
			// lengthLabel
			// 
			this.lengthLabel.AutoSize = true;
			this.lengthLabel.Location = new System.Drawing.Point(3, 77);
			this.lengthLabel.Name = "lengthLabel";
			this.lengthLabel.Size = new System.Drawing.Size(63, 20);
			this.lengthLabel.TabIndex = 2;
			this.lengthLabel.Text = "Length:";
			// 
			// colorBox
			// 
			this.colorBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.colorBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.colorBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.colorBox.Location = new System.Drawing.Point(93, 26);
			this.colorBox.Name = "colorBox";
			this.colorBox.Size = new System.Drawing.Size(132, 20);
			this.colorBox.TabIndex = 1;
			this.colorBox.TabStop = false;
			this.colorBox.Click += new System.EventHandler(this.ChangeLineColor);
			// 
			// colorLabel
			// 
			this.colorLabel.AutoSize = true;
			this.colorLabel.Location = new System.Drawing.Point(3, 26);
			this.colorLabel.Name = "colorLabel";
			this.colorLabel.Size = new System.Drawing.Size(81, 20);
			this.colorLabel.TabIndex = 0;
			this.colorLabel.Text = "Line color:";
			// 
			// LinesSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.layoutPanel);
			this.Controls.Add(this.introBox);
			this.Name = "LinesSheet";
			this.Padding = new System.Windows.Forms.Padding(13, 8, 15, 10);
			this.Size = new System.Drawing.Size(800, 500);
			this.layoutPanel.ResumeLayout(false);
			this.layoutPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.lengthBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.colorBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox introBox;
		private System.Windows.Forms.Panel layoutPanel;
		private System.Windows.Forms.Label clickLabel;
		private System.Windows.Forms.NumericUpDown lengthBox;
		private System.Windows.Forms.Label lengthLabel;
		private River.OneMoreAddIn.UI.MorePictureBox colorBox;
		private System.Windows.Forms.Label colorLabel;
	}
}
