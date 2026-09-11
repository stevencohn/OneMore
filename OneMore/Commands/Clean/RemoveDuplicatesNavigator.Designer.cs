namespace River.OneMoreAddIn.Commands
{
	partial class RemoveDuplicatesNavigator
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RemoveDuplicatesNavigator));
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.resultsPanel = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			//
			// cancelButton
			//
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(966, 544);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.TabIndex = 0;
			this.cancelButton.Text = "Close";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.CloseDialog);
			//
			// resultsPanel
			//
			this.resultsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.resultsPanel.AutoScroll = true;
			this.resultsPanel.Location = new System.Drawing.Point(12, 12);
			this.resultsPanel.Name = "resultsPanel";
			this.resultsPanel.Size = new System.Drawing.Size(1054, 526);
			this.resultsPanel.TabIndex = 1;
			//
			// RemoveDuplicatesNavigator
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(1078, 594);
			this.Controls.Add(this.resultsPanel);
			this.Controls.Add(this.cancelButton);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(700, 400);
			this.Name = "RemoveDuplicatesNavigator";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Remove Duplicate Pages";
			this.ResumeLayout(false);

		}

		#endregion
		private UI.MoreButton cancelButton;
		private System.Windows.Forms.Panel resultsPanel;
	}
}
