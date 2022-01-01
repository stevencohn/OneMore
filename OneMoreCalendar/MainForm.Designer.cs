namespace OneMoreCalendar
{
	partial class MainForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.topPanel = new System.Windows.Forms.Panel();
			this.monthButton = new System.Windows.Forms.RadioButton();
			this.dayButton = new System.Windows.Forms.RadioButton();
			this.settingsButton = new System.Windows.Forms.CheckBox();
			this.dateLabel = new System.Windows.Forms.Label();
			this.contentPanel = new System.Windows.Forms.Panel();
			this.statusStrip = new System.Windows.Forms.StatusStrip();
			this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.nextButton = new OneMoreCalendar.NavigationButton();
			this.prevButton = new OneMoreCalendar.NavigationButton();
			this.topPanel.SuspendLayout();
			this.statusStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// topPanel
			// 
			this.topPanel.Controls.Add(this.monthButton);
			this.topPanel.Controls.Add(this.dayButton);
			this.topPanel.Controls.Add(this.settingsButton);
			this.topPanel.Controls.Add(this.nextButton);
			this.topPanel.Controls.Add(this.prevButton);
			this.topPanel.Controls.Add(this.dateLabel);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = new System.Drawing.Point(0, 0);
			this.topPanel.Name = "topPanel";
			this.topPanel.Size = new System.Drawing.Size(978, 95);
			this.topPanel.TabIndex = 0;
			// 
			// monthButton
			// 
			this.monthButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.monthButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.monthButton.Checked = true;
			this.monthButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(115)))), ((int)(((byte)(53)))), ((int)(((byte)(110)))));
			this.monthButton.FlatAppearance.BorderSize = 0;
			this.monthButton.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(218)))), ((int)(((byte)(238)))));
			this.monthButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(218)))), ((int)(((byte)(238)))));
			this.monthButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(237)))), ((int)(((byte)(247)))));
			this.monthButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.monthButton.Image = ((System.Drawing.Image)(resources.GetObject("monthButton.Image")));
			this.monthButton.Location = new System.Drawing.Point(720, 12);
			this.monthButton.Name = "monthButton";
			this.monthButton.Size = new System.Drawing.Size(64, 64);
			this.monthButton.TabIndex = 6;
			this.monthButton.TabStop = true;
			this.monthButton.UseVisualStyleBackColor = true;
			this.monthButton.CheckedChanged += new System.EventHandler(this.ChangeView);
			// 
			// dayButton
			// 
			this.dayButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.dayButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.dayButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(115)))), ((int)(((byte)(53)))), ((int)(((byte)(110)))));
			this.dayButton.FlatAppearance.BorderSize = 0;
			this.dayButton.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(218)))), ((int)(((byte)(238)))));
			this.dayButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(218)))), ((int)(((byte)(238)))));
			this.dayButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(237)))), ((int)(((byte)(247)))));
			this.dayButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.dayButton.Image = ((System.Drawing.Image)(resources.GetObject("dayButton.Image")));
			this.dayButton.Location = new System.Drawing.Point(790, 12);
			this.dayButton.Name = "dayButton";
			this.dayButton.Size = new System.Drawing.Size(64, 64);
			this.dayButton.TabIndex = 5;
			this.dayButton.UseVisualStyleBackColor = true;
			this.dayButton.CheckedChanged += new System.EventHandler(this.ChangeView);
			// 
			// settingsButton
			// 
			this.settingsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.settingsButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.settingsButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(115)))), ((int)(((byte)(53)))), ((int)(((byte)(110)))));
			this.settingsButton.FlatAppearance.BorderSize = 0;
			this.settingsButton.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(218)))), ((int)(((byte)(238)))));
			this.settingsButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(218)))), ((int)(((byte)(238)))));
			this.settingsButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(237)))), ((int)(((byte)(247)))));
			this.settingsButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.settingsButton.Image = ((System.Drawing.Image)(resources.GetObject("settingsButton.Image")));
			this.settingsButton.Location = new System.Drawing.Point(902, 12);
			this.settingsButton.Name = "settingsButton";
			this.settingsButton.Size = new System.Drawing.Size(64, 64);
			this.settingsButton.TabIndex = 4;
			this.settingsButton.TabStop = false;
			this.settingsButton.UseVisualStyleBackColor = true;
			this.settingsButton.Click += new System.EventHandler(this.ShowSettings);
			// 
			// dateLabel
			// 
			this.dateLabel.AutoSize = true;
			this.dateLabel.Font = new System.Drawing.Font("Segoe UI Light", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(115)))), ((int)(((byte)(53)))), ((int)(((byte)(110)))));
			this.dateLabel.Location = new System.Drawing.Point(81, 17);
			this.dateLabel.Name = "dateLabel";
			this.dateLabel.Size = new System.Drawing.Size(284, 54);
			this.dateLabel.TabIndex = 0;
			this.dateLabel.Text = "December 2021";
			// 
			// contentPanel
			// 
			this.contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.contentPanel.Location = new System.Drawing.Point(0, 95);
			this.contentPanel.Margin = new System.Windows.Forms.Padding(0);
			this.contentPanel.Name = "contentPanel";
			this.contentPanel.Size = new System.Drawing.Size(978, 527);
			this.contentPanel.TabIndex = 2;
			// 
			// statusStrip
			// 
			this.statusStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
			this.statusStrip.Location = new System.Drawing.Point(0, 622);
			this.statusStrip.Name = "statusStrip";
			this.statusStrip.Size = new System.Drawing.Size(978, 22);
			this.statusStrip.TabIndex = 0;
			this.statusStrip.Text = "statusStrip1";
			// 
			// statusLabel
			// 
			this.statusLabel.Margin = new System.Windows.Forms.Padding(9, 4, 0, 3);
			this.statusLabel.Name = "statusLabel";
			this.statusLabel.Size = new System.Drawing.Size(0, 15);
			// 
			// nextButton
			// 
			this.nextButton.Direction = System.Windows.Forms.ArrowDirection.Right;
			this.nextButton.Enabled = false;
			this.nextButton.FlatAppearance.BorderSize = 0;
			this.nextButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.nextButton.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.nextButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(115)))), ((int)(((byte)(53)))), ((int)(((byte)(110)))));
			this.nextButton.Location = new System.Drawing.Point(44, 17);
			this.nextButton.Name = "nextButton";
			this.nextButton.Size = new System.Drawing.Size(31, 54);
			this.nextButton.TabIndex = 2;
			this.nextButton.Text = ">";
			this.nextButton.UseVisualStyleBackColor = true;
			this.nextButton.Click += new System.EventHandler(this.GotoNext);
			// 
			// prevButton
			// 
			this.prevButton.Direction = System.Windows.Forms.ArrowDirection.Left;
			this.prevButton.FlatAppearance.BorderSize = 0;
			this.prevButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.prevButton.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.prevButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(115)))), ((int)(((byte)(53)))), ((int)(((byte)(110)))));
			this.prevButton.Location = new System.Drawing.Point(12, 17);
			this.prevButton.Name = "prevButton";
			this.prevButton.Size = new System.Drawing.Size(31, 54);
			this.prevButton.TabIndex = 1;
			this.prevButton.Text = "<";
			this.prevButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.prevButton.UseVisualStyleBackColor = true;
			this.prevButton.Click += new System.EventHandler(this.GotoPrevious);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(978, 644);
			this.Controls.Add(this.contentPanel);
			this.Controls.Add(this.statusStrip);
			this.Controls.Add(this.topPanel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.MinimumSize = new System.Drawing.Size(700, 500);
			this.Name = "MainForm";
			this.Text = "OneMore Calendar";
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.statusStrip.ResumeLayout(false);
			this.statusStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Panel topPanel;
		private System.Windows.Forms.Panel contentPanel;
		private System.Windows.Forms.Label dateLabel;
		private NavigationButton nextButton;
		private NavigationButton prevButton;
		private System.Windows.Forms.CheckBox settingsButton;
		private System.Windows.Forms.RadioButton monthButton;
		private System.Windows.Forms.RadioButton dayButton;
		private System.Windows.Forms.StatusStrip statusStrip;
		private System.Windows.Forms.ToolStripStatusLabel statusLabel;
	}
}

