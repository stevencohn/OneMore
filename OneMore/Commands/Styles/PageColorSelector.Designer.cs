namespace River.OneMoreAddIn.Commands
{
	partial class PageColorSelector
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

				borderPen.Dispose();
				activePen.Dispose();
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
			this.paletteBox = new System.Windows.Forms.PictureBox();
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.autoButton = new UI.MoreCheckBox();
			this.tooltip = new System.Windows.Forms.ToolTip(this.components);
			this.borderPanel = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.paletteBox)).BeginInit();
			this.buttonPanel.SuspendLayout();
			this.borderPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// paletteBox
			// 
			this.paletteBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.paletteBox.Location = new System.Drawing.Point(0, 0);
			this.paletteBox.Name = "paletteBox";
			this.paletteBox.Size = new System.Drawing.Size(261, 385);
			this.paletteBox.TabIndex = 2;
			this.paletteBox.TabStop = false;
			this.paletteBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HitTest);
			this.paletteBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ChooseColor);
			// 
			// buttonPanel
			// 
			this.buttonPanel.Controls.Add(this.autoButton);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.Location = new System.Drawing.Point(0, 385);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Size = new System.Drawing.Size(261, 50);
			this.buttonPanel.TabIndex = 3;
			// 
			// autoButton
			// 
			this.autoButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.autoButton.Location = new System.Drawing.Point(19, 13);
			this.autoButton.Name = "autoButton";
			this.autoButton.Size = new System.Drawing.Size(220, 24);
			this.autoButton.TabIndex = 0;
			this.autoButton.Text = "No color";
			this.autoButton.UseVisualStyleBackColor = true;
			this.autoButton.Click += new System.EventHandler(this.ChooseNoColor);
			this.autoButton.KeyUp += new System.Windows.Forms.KeyEventHandler(this.CheckEscape);
			// 
			// tooltip
			// 
			this.tooltip.AutoPopDelay = 2000;
			this.tooltip.InitialDelay = 500;
			this.tooltip.ReshowDelay = 100;
			// 
			// borderPanel
			// 
			this.borderPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.borderPanel.Controls.Add(this.paletteBox);
			this.borderPanel.Controls.Add(this.buttonPanel);
			this.borderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.borderPanel.Location = new System.Drawing.Point(0, 0);
			this.borderPanel.Name = "borderPanel";
			this.borderPanel.Size = new System.Drawing.Size(263, 437);
			this.borderPanel.TabIndex = 4;
			// 
			// PageColorSelector
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(263, 437);
			this.Controls.Add(this.borderPanel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "PageColorSelector";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.CheckEscape);
			((System.ComponentModel.ISupportInitialize)(this.paletteBox)).EndInit();
			this.buttonPanel.ResumeLayout(false);
			this.borderPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PictureBox paletteBox;
		private System.Windows.Forms.Panel buttonPanel;
		private UI.MoreCheckBox autoButton;
		private System.Windows.Forms.ToolTip tooltip;
		private System.Windows.Forms.Panel borderPanel;
	}
}