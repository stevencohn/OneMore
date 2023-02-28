
namespace River.OneMoreAddIn.Settings
{
	partial class NavigatorSheet
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
			this.intervalBox = new System.Windows.Forms.NumericUpDown();
			this.intervalLabel = new System.Windows.Forms.Label();
			this.depthBox = new System.Windows.Forms.NumericUpDown();
			this.depthLabel = new System.Windows.Forms.Label();
			this.corrallBox = new System.Windows.Forms.CheckBox();
			this.layoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.intervalBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.depthBox)).BeginInit();
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
			this.introBox.Text = "Customize advanced options for the Navigator Service";
			// 
			// layoutPanel
			// 
			this.layoutPanel.Controls.Add(this.intervalBox);
			this.layoutPanel.Controls.Add(this.intervalLabel);
			this.layoutPanel.Controls.Add(this.depthBox);
			this.layoutPanel.Controls.Add(this.depthLabel);
			this.layoutPanel.Controls.Add(this.corrallBox);
			this.layoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.layoutPanel.Location = new System.Drawing.Point(13, 74);
			this.layoutPanel.Margin = new System.Windows.Forms.Padding(0);
			this.layoutPanel.Name = "layoutPanel";
			this.layoutPanel.Size = new System.Drawing.Size(772, 416);
			this.layoutPanel.TabIndex = 4;
			// 
			// intervalBox
			// 
			this.intervalBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.intervalBox.DecimalPlaces = 2;
			this.intervalBox.Increment = new decimal(new int[] {
            25,
            0,
            0,
            131072});
			this.intervalBox.Location = new System.Drawing.Point(606, 60);
			this.intervalBox.Maximum = new decimal(new int[] {
            600,
            0,
            0,
            65536});
			this.intervalBox.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            65536});
			this.intervalBox.Name = "intervalBox";
			this.intervalBox.Size = new System.Drawing.Size(120, 26);
			this.intervalBox.TabIndex = 4;
			this.intervalBox.Value = new decimal(new int[] {
            125,
            0,
            0,
            131072});
			// 
			// intervalLabel
			// 
			this.intervalLabel.AutoSize = true;
			this.intervalLabel.Location = new System.Drawing.Point(3, 60);
			this.intervalLabel.Name = "intervalLabel";
			this.intervalLabel.Size = new System.Drawing.Size(183, 20);
			this.intervalLabel.TabIndex = 3;
			this.intervalLabel.Text = "Polling interval (seconds)";
			// 
			// depthBox
			// 
			this.depthBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.depthBox.Location = new System.Drawing.Point(606, 8);
			this.depthBox.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
			this.depthBox.Name = "depthBox";
			this.depthBox.Size = new System.Drawing.Size(120, 26);
			this.depthBox.TabIndex = 2;
			this.depthBox.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
			// 
			// depthLabel
			// 
			this.depthLabel.AutoSize = true;
			this.depthLabel.Location = new System.Drawing.Point(3, 8);
			this.depthLabel.Name = "depthLabel";
			this.depthLabel.Size = new System.Drawing.Size(359, 20);
			this.depthLabel.TabIndex = 1;
			this.depthLabel.Text = "Max number of pages to save in navigation history";
			// 
			// corrallBox
			// 
			this.corrallBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.corrallBox.Checked = true;
			this.corrallBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.corrallBox.Location = new System.Drawing.Point(7, 103);
			this.corrallBox.Margin = new System.Windows.Forms.Padding(0);
			this.corrallBox.Name = "corrallBox";
			this.corrallBox.Size = new System.Drawing.Size(765, 43);
			this.corrallBox.TabIndex = 0;
			this.corrallBox.Text = "Restrict the Navigator window to the active screen";
			this.corrallBox.UseVisualStyleBackColor = true;
			// 
			// NavigatorSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.layoutPanel);
			this.Controls.Add(this.introBox);
			this.Name = "NavigatorSheet";
			this.Padding = new System.Windows.Forms.Padding(13, 8, 15, 10);
			this.Size = new System.Drawing.Size(800, 500);
			this.layoutPanel.ResumeLayout(false);
			this.layoutPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.intervalBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.depthBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox introBox;
		private System.Windows.Forms.Panel layoutPanel;
		private System.Windows.Forms.CheckBox corrallBox;
		private System.Windows.Forms.NumericUpDown depthBox;
		private System.Windows.Forms.Label depthLabel;
		private System.Windows.Forms.NumericUpDown intervalBox;
		private System.Windows.Forms.Label intervalLabel;
	}
}
