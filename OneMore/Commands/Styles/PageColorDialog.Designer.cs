namespace River.OneMoreAddIn.Commands
{
	partial class PageColorDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PageColorDialog));
			this.omBox = new River.OneMoreAddIn.UI.MorePictureBox();
			this.omButton = new System.Windows.Forms.Button();
			this.customButton = new System.Windows.Forms.Button();
			this.customBox = new River.OneMoreAddIn.UI.MorePictureBox();
			this.noLabel = new System.Windows.Forms.Label();
			this.noButton = new System.Windows.Forms.Button();
			this.noPanel = new System.Windows.Forms.Panel();
			this.cancelButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.omBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.customBox)).BeginInit();
			this.noPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// omBox
			// 
			this.omBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.omBox.Location = new System.Drawing.Point(18, 18);
			this.omBox.Name = "omBox";
			this.omBox.Size = new System.Drawing.Size(450, 60);
			this.omBox.TabIndex = 0;
			this.omBox.TabStop = false;
			this.omBox.Click += new System.EventHandler(this.ChooseColor);
			// 
			// omButton
			// 
			this.omButton.Location = new System.Drawing.Point(474, 28);
			this.omButton.Name = "omButton";
			this.omButton.Size = new System.Drawing.Size(202, 40);
			this.omButton.TabIndex = 3;
			this.omButton.Text = "Selected Color";
			this.omButton.UseVisualStyleBackColor = true;
			this.omButton.Click += new System.EventHandler(this.ApplyColor);
			// 
			// customButton
			// 
			this.customButton.Location = new System.Drawing.Point(474, 110);
			this.customButton.Name = "customButton";
			this.customButton.Size = new System.Drawing.Size(202, 40);
			this.customButton.TabIndex = 4;
			this.customButton.Text = "Custom Color";
			this.customButton.UseVisualStyleBackColor = true;
			this.customButton.Click += new System.EventHandler(this.ApplyCustomColor);
			// 
			// customBox
			// 
			this.customBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.customBox.Location = new System.Drawing.Point(18, 100);
			this.customBox.Name = "customBox";
			this.customBox.Size = new System.Drawing.Size(450, 60);
			this.customBox.TabIndex = 5;
			this.customBox.TabStop = false;
			this.customBox.Click += new System.EventHandler(this.ChooseCustomColor);
			// 
			// noLabel
			// 
			this.noLabel.AutoSize = true;
			this.noLabel.Location = new System.Drawing.Point(15, 19);
			this.noLabel.Name = "noLabel";
			this.noLabel.Size = new System.Drawing.Size(179, 20);
			this.noLabel.TabIndex = 6;
			this.noLabel.Text = "or choose no page color";
			// 
			// noButton
			// 
			this.noButton.Location = new System.Drawing.Point(474, 191);
			this.noButton.Name = "noButton";
			this.noButton.Size = new System.Drawing.Size(202, 40);
			this.noButton.TabIndex = 7;
			this.noButton.Text = "No Color";
			this.noButton.UseVisualStyleBackColor = true;
			this.noButton.Click += new System.EventHandler(this.ApplyNoColor);
			// 
			// noPanel
			// 
			this.noPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.noPanel.Controls.Add(this.noLabel);
			this.noPanel.Location = new System.Drawing.Point(18, 182);
			this.noPanel.Name = "noPanel";
			this.noPanel.Size = new System.Drawing.Size(450, 60);
			this.noPanel.TabIndex = 8;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(584, 551);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(130, 40);
			this.cancelButton.TabIndex = 9;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// PageColorDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(727, 604);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.noPanel);
			this.Controls.Add(this.noButton);
			this.Controls.Add(this.customButton);
			this.Controls.Add(this.omButton);
			this.Controls.Add(this.omBox);
			this.Controls.Add(this.customBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "PageColorDialog";
			this.Padding = new System.Windows.Forms.Padding(15, 15, 10, 10);
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Page Color";
			((System.ComponentModel.ISupportInitialize)(this.omBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.customBox)).EndInit();
			this.noPanel.ResumeLayout(false);
			this.noPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private River.OneMoreAddIn.UI.MorePictureBox omBox;
		private System.Windows.Forms.Button omButton;
		private System.Windows.Forms.Button customButton;
		private River.OneMoreAddIn.UI.MorePictureBox customBox;
		private System.Windows.Forms.Label noLabel;
		private System.Windows.Forms.Button noButton;
		private System.Windows.Forms.Panel noPanel;
		private System.Windows.Forms.Button cancelButton;
	}
}