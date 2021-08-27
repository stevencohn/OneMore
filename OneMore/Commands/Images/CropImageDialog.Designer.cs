namespace River.OneMoreAddIn.Commands
{
	partial class CropImageDialog
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
				selectionRegion?.Dispose();
				selectionPath?.Dispose();
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CropImageDialog));
			this.pictureBox = new System.Windows.Forms.PictureBox();
			this.marchingTimer = new System.Windows.Forms.Timer(this.components);
			this.picturePanel = new System.Windows.Forms.Panel();
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.selectButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.cropButton = new System.Windows.Forms.Button();
			this.statusStrip = new System.Windows.Forms.StatusStrip();
			this.sizeStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
			this.picturePanel.SuspendLayout();
			this.buttonPanel.SuspendLayout();
			this.statusStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// pictureBox
			// 
			this.pictureBox.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.pictureBox.Cursor = System.Windows.Forms.Cursors.Cross;
			this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pictureBox.Location = new System.Drawing.Point(0, 0);
			this.pictureBox.Name = "pictureBox";
			this.pictureBox.Size = new System.Drawing.Size(978, 651);
			this.pictureBox.TabIndex = 1;
			this.pictureBox.TabStop = false;
			this.pictureBox.SizeChanged += new System.EventHandler(this.pictureBox_SizeChanged);
			this.pictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.Picture_Paint);
			this.pictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Picture_MouseDown);
			this.pictureBox.MouseHover += new System.EventHandler(this.Picture_Hover);
			this.pictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Picture_MouseMove);
			this.pictureBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Picture_MouseUp);
			// 
			// marchingTimer
			// 
			this.marchingTimer.Interval = 50;
			this.marchingTimer.Tick += new System.EventHandler(this.MarchingTimer_Tick);
			// 
			// picturePanel
			// 
			this.picturePanel.AutoScrollMinSize = new System.Drawing.Size(300, 300);
			this.picturePanel.AutoSize = true;
			this.picturePanel.Controls.Add(this.pictureBox);
			this.picturePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.picturePanel.Location = new System.Drawing.Point(0, 58);
			this.picturePanel.Name = "picturePanel";
			this.picturePanel.Size = new System.Drawing.Size(978, 651);
			this.picturePanel.TabIndex = 5;
			// 
			// buttonPanel
			// 
			this.buttonPanel.Controls.Add(this.selectButton);
			this.buttonPanel.Controls.Add(this.cancelButton);
			this.buttonPanel.Controls.Add(this.cropButton);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.buttonPanel.Location = new System.Drawing.Point(0, 0);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Padding = new System.Windows.Forms.Padding(6);
			this.buttonPanel.Size = new System.Drawing.Size(978, 58);
			this.buttonPanel.TabIndex = 6;
			// 
			// selectButton
			// 
			this.selectButton.Location = new System.Drawing.Point(12, 9);
			this.selectButton.Name = "selectButton";
			this.selectButton.Size = new System.Drawing.Size(120, 40);
			this.selectButton.TabIndex = 7;
			this.selectButton.Text = "Select All";
			this.selectButton.UseVisualStyleBackColor = true;
			this.selectButton.Click += new System.EventHandler(this.SelectButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(849, 9);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(120, 40);
			this.cancelButton.TabIndex = 6;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// cropButton
			// 
			this.cropButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cropButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cropButton.Enabled = false;
			this.cropButton.Location = new System.Drawing.Point(723, 9);
			this.cropButton.Name = "cropButton";
			this.cropButton.Size = new System.Drawing.Size(120, 40);
			this.cropButton.TabIndex = 5;
			this.cropButton.Text = "Crop";
			this.cropButton.UseVisualStyleBackColor = true;
			this.cropButton.Click += new System.EventHandler(this.CropButton_Click);
			// 
			// statusStrip
			// 
			this.statusStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sizeStatusLabel,
            this.statusLabel});
			this.statusStrip.Location = new System.Drawing.Point(0, 709);
			this.statusStrip.Name = "statusStrip";
			this.statusStrip.Padding = new System.Windows.Forms.Padding(2, 0, 14, 0);
			this.statusStrip.Size = new System.Drawing.Size(978, 36);
			this.statusStrip.TabIndex = 7;
			// 
			// sizeStatusLabel
			// 
			this.sizeStatusLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
			this.sizeStatusLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.sizeStatusLabel.Name = "sizeStatusLabel";
			this.sizeStatusLabel.Size = new System.Drawing.Size(181, 29);
			this.sizeStatusLabel.Text = "Image size: 100x100.";
			// 
			// statusLabel
			// 
			this.statusLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.statusLabel.Name = "statusLabel";
			this.statusLabel.Size = new System.Drawing.Size(543, 29);
			this.statusLabel.Text = "Selection top left: {x}, {y}. Bounding rectangle size: {width} x {height}.";
			this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// CropImageDialog
			// 
			this.AcceptButton = this.cropButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(978, 745);
			this.Controls.Add(this.picturePanel);
			this.Controls.Add(this.statusStrip);
			this.Controls.Add(this.buttonPanel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(739, 585);
			this.Name = "CropImageDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Crop Image";
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
			this.picturePanel.ResumeLayout(false);
			this.buttonPanel.ResumeLayout(false);
			this.statusStrip.ResumeLayout(false);
			this.statusStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox pictureBox;
		private System.Windows.Forms.Timer marchingTimer;
		private System.Windows.Forms.Panel picturePanel;
		private System.Windows.Forms.Panel buttonPanel;
		private System.Windows.Forms.Button cropButton;
		private System.Windows.Forms.StatusStrip statusStrip;
		private System.Windows.Forms.ToolStripStatusLabel statusLabel;
		private System.Windows.Forms.ToolStripStatusLabel sizeStatusLabel;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button selectButton;
	}
}

