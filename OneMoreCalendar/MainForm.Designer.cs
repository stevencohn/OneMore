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
			this.nextButton = new OneMoreCalendar.NavigationButton();
			this.prevButton = new OneMoreCalendar.NavigationButton();
			this.dateLabel = new System.Windows.Forms.Label();
			this.bottomPanel = new System.Windows.Forms.Panel();
			this.contentPanel = new System.Windows.Forms.Panel();
			this.topPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// topPanel
			// 
			this.topPanel.Controls.Add(this.nextButton);
			this.topPanel.Controls.Add(this.prevButton);
			this.topPanel.Controls.Add(this.dateLabel);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = new System.Drawing.Point(0, 0);
			this.topPanel.Name = "topPanel";
			this.topPanel.Size = new System.Drawing.Size(978, 95);
			this.topPanel.TabIndex = 0;
			// 
			// nextButton
			// 
			this.nextButton.FlatAppearance.BorderSize = 0;
			this.nextButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.nextButton.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.nextButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.nextButton.Forward = true;
			this.nextButton.Location = new System.Drawing.Point(44, 17);
			this.nextButton.Name = "nextButton";
			this.nextButton.Size = new System.Drawing.Size(31, 54);
			this.nextButton.TabIndex = 2;
			this.nextButton.Text = ">";
			this.nextButton.UseVisualStyleBackColor = true;
			// 
			// prevButton
			// 
			this.prevButton.FlatAppearance.BorderSize = 0;
			this.prevButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.prevButton.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.prevButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.prevButton.Forward = false;
			this.prevButton.Location = new System.Drawing.Point(12, 17);
			this.prevButton.Name = "prevButton";
			this.prevButton.Size = new System.Drawing.Size(31, 54);
			this.prevButton.TabIndex = 1;
			this.prevButton.Text = "<";
			this.prevButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.prevButton.UseVisualStyleBackColor = true;
			// 
			// dateLabel
			// 
			this.dateLabel.AutoSize = true;
			this.dateLabel.Font = new System.Drawing.Font("Segoe UI Semilight", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateLabel.Location = new System.Drawing.Point(81, 17);
			this.dateLabel.Name = "dateLabel";
			this.dateLabel.Size = new System.Drawing.Size(292, 54);
			this.dateLabel.TabIndex = 0;
			this.dateLabel.Text = "December 2021";
			// 
			// bottomPanel
			// 
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = new System.Drawing.Point(0, 601);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = new System.Drawing.Size(978, 43);
			this.bottomPanel.TabIndex = 1;
			// 
			// contentPanel
			// 
			this.contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.contentPanel.Location = new System.Drawing.Point(0, 95);
			this.contentPanel.Margin = new System.Windows.Forms.Padding(0);
			this.contentPanel.Name = "contentPanel";
			this.contentPanel.Size = new System.Drawing.Size(978, 506);
			this.contentPanel.TabIndex = 2;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(978, 644);
			this.Controls.Add(this.contentPanel);
			this.Controls.Add(this.bottomPanel);
			this.Controls.Add(this.topPanel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(700, 500);
			this.Name = "MainForm";
			this.Text = "OneMore Calendar";
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel topPanel;
		private System.Windows.Forms.Panel bottomPanel;
		private System.Windows.Forms.Panel contentPanel;
		private System.Windows.Forms.Label dateLabel;
		private NavigationButton nextButton;
		private NavigationButton prevButton;
	}
}

