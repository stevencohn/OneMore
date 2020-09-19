namespace River.OneMoreAddIn.Dialogs
{
	partial class RunPluginDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RunPluginDialog));
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.pathLabel = new System.Windows.Forms.Label();
			this.pathBox = new System.Windows.Forms.TextBox();
			this.browseButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(583, 91);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(120, 36);
			this.cancelButton.TabIndex = 0;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Enabled = false;
			this.okButton.Location = new System.Drawing.Point(457, 91);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(120, 36);
			this.okButton.TabIndex = 1;
			this.okButton.Text = "Run";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// pathLabel
			// 
			this.pathLabel.AutoSize = true;
			this.pathLabel.Location = new System.Drawing.Point(18, 34);
			this.pathLabel.Name = "pathLabel";
			this.pathLabel.Size = new System.Drawing.Size(117, 20);
			this.pathLabel.TabIndex = 2;
			this.pathLabel.Text = "Plugin Location";
			// 
			// pathBox
			// 
			this.pathBox.Location = new System.Drawing.Point(141, 31);
			this.pathBox.Name = "pathBox";
			this.pathBox.Size = new System.Drawing.Size(521, 26);
			this.pathBox.TabIndex = 3;
			this.pathBox.TextChanged += new System.EventHandler(this.pathBox_TextChanged);
			// 
			// browseButton
			// 
			this.browseButton.Location = new System.Drawing.Point(668, 31);
			this.browseButton.Name = "browseButton";
			this.browseButton.Size = new System.Drawing.Size(36, 26);
			this.browseButton.TabIndex = 4;
			this.browseButton.Text = "...";
			this.browseButton.UseVisualStyleBackColor = true;
			this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
			// 
			// RunPluginDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(721, 145);
			this.Controls.Add(this.browseButton);
			this.Controls.Add(this.pathBox);
			this.Controls.Add(this.pathLabel);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "RunPluginDialog";
			this.Padding = new System.Windows.Forms.Padding(15);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Run Plugin";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Label pathLabel;
		private System.Windows.Forms.TextBox pathBox;
		private System.Windows.Forms.Button browseButton;
	}
}