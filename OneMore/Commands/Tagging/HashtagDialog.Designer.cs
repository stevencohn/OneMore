namespace River.OneMoreAddIn.Commands
{
	partial class HashtagDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HashtagDialog));
			this.cancelButton = new System.Windows.Forms.Button();
			this.contextPanel = new System.Windows.Forms.FlowLayoutPanel();
			this.topPanel = new System.Windows.Forms.Panel();
			this.searchButton = new System.Windows.Forms.Button();
			this.tagBox = new System.Windows.Forms.TextBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.topPanel.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(864, 13);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(112, 35);
			this.cancelButton.TabIndex = 6;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			this.cancelButton.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.DoPreviewKeyDown);
			// 
			// contextPanel
			// 
			this.contextPanel.AutoScroll = true;
			this.contextPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.contextPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.contextPanel.Location = new System.Drawing.Point(0, 114);
			this.contextPanel.Name = "contextPanel";
			this.contextPanel.Padding = new System.Windows.Forms.Padding(3);
			this.contextPanel.Size = new System.Drawing.Size(988, 396);
			this.contextPanel.TabIndex = 7;
			this.contextPanel.WrapContents = false;
			// 
			// topPanel
			// 
			this.topPanel.BackColor = System.Drawing.SystemColors.Window;
			this.topPanel.Controls.Add(this.searchButton);
			this.topPanel.Controls.Add(this.tagBox);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = new System.Drawing.Point(0, 0);
			this.topPanel.Margin = new System.Windows.Forms.Padding(0);
			this.topPanel.Name = "topPanel";
			this.topPanel.Padding = new System.Windows.Forms.Padding(10);
			this.topPanel.Size = new System.Drawing.Size(988, 114);
			this.topPanel.TabIndex = 8;
			// 
			// searchButton
			// 
			this.searchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.searchButton.Image = ((System.Drawing.Image)(resources.GetObject("searchButton.Image")));
			this.searchButton.Location = new System.Drawing.Point(557, 39);
			this.searchButton.Name = "searchButton";
			this.searchButton.Size = new System.Drawing.Size(60, 32);
			this.searchButton.TabIndex = 2;
			this.searchButton.UseVisualStyleBackColor = true;
			this.searchButton.Click += new System.EventHandler(this.SearchTags);
			// 
			// tagBox
			// 
			this.tagBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tagBox.Location = new System.Drawing.Point(41, 40);
			this.tagBox.Name = "tagBox";
			this.tagBox.Size = new System.Drawing.Size(510, 28);
			this.tagBox.TabIndex = 0;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.cancelButton);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 510);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(988, 60);
			this.panel1.TabIndex = 9;
			// 
			// HashtagDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(988, 570);
			this.Controls.Add(this.contextPanel);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.topPanel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "HashtagDialog";
			this.Text = "Find Hashtags";
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.FlowLayoutPanel contextPanel;
		private System.Windows.Forms.Panel topPanel;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.TextBox tagBox;
		private System.Windows.Forms.Button searchButton;
	}
}