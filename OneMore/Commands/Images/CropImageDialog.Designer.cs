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
			this.introPanel = new System.Windows.Forms.Panel();
			this.introText = new System.Windows.Forms.Label();
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.rotationBox = new System.Windows.Forms.NumericUpDown();
			this.rotationBar = new System.Windows.Forms.TrackBar();
			this.selectButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cropButton = new River.OneMoreAddIn.UI.MoreButton();
			this.statusStrip = new System.Windows.Forms.StatusStrip();
			this.sizeStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
			this.picturePanel.SuspendLayout();
			this.introPanel.SuspendLayout();
			this.buttonPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.rotationBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.rotationBar)).BeginInit();
			this.statusStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// pictureBox
			// 
			this.pictureBox.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.pictureBox.Cursor = System.Windows.Forms.Cursors.Cross;
			this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pictureBox.Location = new System.Drawing.Point(0, 66);
			this.pictureBox.Name = "pictureBox";
			this.pictureBox.Size = new System.Drawing.Size(1489, 704);
			this.pictureBox.TabIndex = 1;
			this.pictureBox.TabStop = false;
			this.pictureBox.SizeChanged += new System.EventHandler(this.PictureBox_SizeChanged);
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
			this.picturePanel.AutoScroll = true;
			this.picturePanel.AutoScrollMinSize = new System.Drawing.Size(300, 300);
			this.picturePanel.AutoSize = true;
			this.picturePanel.Controls.Add(this.pictureBox);
			this.picturePanel.Controls.Add(this.introPanel);
			this.picturePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.picturePanel.Location = new System.Drawing.Point(0, 0);
			this.picturePanel.Name = "picturePanel";
			this.picturePanel.Size = new System.Drawing.Size(1489, 770);
			this.picturePanel.TabIndex = 5;
			// 
			// introPanel
			// 
			this.introPanel.BackColor = System.Drawing.SystemColors.ControlLight;
			this.introPanel.Controls.Add(this.introText);
			this.introPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.introPanel.Location = new System.Drawing.Point(0, 0);
			this.introPanel.Name = "introPanel";
			this.introPanel.Padding = new System.Windows.Forms.Padding(10);
			this.introPanel.Size = new System.Drawing.Size(1489, 66);
			this.introPanel.TabIndex = 2;
			// 
			// introText
			// 
			this.introText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.introText.ForeColor = System.Drawing.SystemColors.ControlText;
			this.introText.Location = new System.Drawing.Point(13, 12);
			this.introText.Name = "introText";
			this.introText.Size = new System.Drawing.Size(1463, 26);
			this.introText.TabIndex = 0;
			this.introText.Text = "Use the mouse to select an area to crop. Ctrl+D to deselect. Use the slider to ro" +
    "tate the image. Click OK to finish.";
			// 
			// buttonPanel
			// 
			this.buttonPanel.BackColor = System.Drawing.SystemColors.ControlLight;
			this.buttonPanel.Controls.Add(this.rotationBox);
			this.buttonPanel.Controls.Add(this.rotationBar);
			this.buttonPanel.Controls.Add(this.selectButton);
			this.buttonPanel.Controls.Add(this.cancelButton);
			this.buttonPanel.Controls.Add(this.cropButton);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.Location = new System.Drawing.Point(0, 770);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Padding = new System.Windows.Forms.Padding(6);
			this.buttonPanel.Size = new System.Drawing.Size(1489, 65);
			this.buttonPanel.TabIndex = 6;
			// 
			// rotationBox
			// 
			this.rotationBox.Location = new System.Drawing.Point(549, 17);
			this.rotationBox.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
			this.rotationBox.Name = "rotationBox";
			this.rotationBox.Size = new System.Drawing.Size(78, 26);
			this.rotationBox.TabIndex = 3;
			this.rotationBox.ValueChanged += new System.EventHandler(this.ChangeRotation);
			// 
			// rotationBar
			// 
			this.rotationBar.Location = new System.Drawing.Point(183, 9);
			this.rotationBar.Maximum = 360;
			this.rotationBar.MaximumSize = new System.Drawing.Size(360, 69);
			this.rotationBar.MinimumSize = new System.Drawing.Size(360, 69);
			this.rotationBar.Name = "rotationBar";
			this.rotationBar.Size = new System.Drawing.Size(360, 69);
			this.rotationBar.TabIndex = 2;
			this.rotationBar.TickFrequency = 10;
			this.rotationBar.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
			this.rotationBar.Scroll += new System.EventHandler(this.ChangeRotation);
			// 
			// selectButton
			// 
			this.selectButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.selectButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.selectButton.ImageOver = null;
			this.selectButton.Location = new System.Drawing.Point(12, 9);
			this.selectButton.Name = "selectButton";
			this.selectButton.ShowBorder = true;
			this.selectButton.Size = new System.Drawing.Size(120, 40);
			this.selectButton.TabIndex = 4;
			this.selectButton.Text = "Select All";
			this.selectButton.ThemedBack = null;
			this.selectButton.ThemedFore = null;
			this.selectButton.UseVisualStyleBackColor = true;
			this.selectButton.Click += new System.EventHandler(this.SelectButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(1373, 9);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(103, 40);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// cropButton
			// 
			this.cropButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cropButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cropButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cropButton.Enabled = false;
			this.cropButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cropButton.ImageOver = null;
			this.cropButton.Location = new System.Drawing.Point(1265, 9);
			this.cropButton.Name = "cropButton";
			this.cropButton.ShowBorder = true;
			this.cropButton.Size = new System.Drawing.Size(102, 40);
			this.cropButton.TabIndex = 0;
			this.cropButton.Text = "OK";
			this.cropButton.ThemedBack = null;
			this.cropButton.ThemedFore = null;
			this.cropButton.UseVisualStyleBackColor = true;
			this.cropButton.Click += new System.EventHandler(this.CropButton_Click);
			// 
			// statusStrip
			// 
			this.statusStrip.BackColor = System.Drawing.SystemColors.Control;
			this.statusStrip.ForeColor = System.Drawing.SystemColors.ControlText;
			this.statusStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sizeStatusLabel,
            this.statusLabel});
			this.statusStrip.Location = new System.Drawing.Point(0, 835);
			this.statusStrip.Name = "statusStrip";
			this.statusStrip.Padding = new System.Windows.Forms.Padding(2, 0, 14, 0);
			this.statusStrip.Size = new System.Drawing.Size(1489, 36);
			this.statusStrip.TabIndex = 7;
			// 
			// sizeStatusLabel
			// 
			this.sizeStatusLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
			this.sizeStatusLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.sizeStatusLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.sizeStatusLabel.Name = "sizeStatusLabel";
			this.sizeStatusLabel.Size = new System.Drawing.Size(181, 29);
			this.sizeStatusLabel.Text = "Image size: 100x100.";
			// 
			// statusLabel
			// 
			this.statusLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.statusLabel.ForeColor = System.Drawing.SystemColors.ControlText;
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
			this.ClientSize = new System.Drawing.Size(1489, 871);
			this.Controls.Add(this.picturePanel);
			this.Controls.Add(this.buttonPanel);
			this.Controls.Add(this.statusStrip);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(900, 600);
			this.Name = "CropImageDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Crop and Rotate";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CropImageDialog_FormClosed);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CropImageDialog_KeyDown);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
			this.picturePanel.ResumeLayout(false);
			this.introPanel.ResumeLayout(false);
			this.buttonPanel.ResumeLayout(false);
			this.buttonPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.rotationBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.rotationBar)).EndInit();
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
		private UI.MoreButton cropButton;
		private System.Windows.Forms.StatusStrip statusStrip;
		private System.Windows.Forms.ToolStripStatusLabel statusLabel;
		private System.Windows.Forms.ToolStripStatusLabel sizeStatusLabel;
		private UI.MoreButton cancelButton;
		private UI.MoreButton selectButton;
		private System.Windows.Forms.Panel introPanel;
		private System.Windows.Forms.Label introText;
		private System.Windows.Forms.TrackBar rotationBar;
		private System.Windows.Forms.NumericUpDown rotationBox;
	}
}

