namespace River.OneMoreAddIn.Commands
{
	partial class RemoveSpacingDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RemoveSpacingDialog));
			this.beforeBox = new System.Windows.Forms.CheckBox();
			this.afterBox = new System.Windows.Forms.CheckBox();
			this.betweenBox = new System.Windows.Forms.CheckBox();
			this.headingsBox = new System.Windows.Forms.CheckBox();
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// beforeBox
			// 
			this.beforeBox.AutoSize = true;
			this.beforeBox.Checked = true;
			this.beforeBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.beforeBox.Location = new System.Drawing.Point(15, 15);
			this.beforeBox.Name = "beforeBox";
			this.beforeBox.Size = new System.Drawing.Size(195, 17);
			this.beforeBox.TabIndex = 0;
			this.beforeBox.Text = "Remove spacing before paragraphs";
			this.beforeBox.UseVisualStyleBackColor = true;
			// 
			// afterBox
			// 
			this.afterBox.AutoSize = true;
			this.afterBox.Checked = true;
			this.afterBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.afterBox.Location = new System.Drawing.Point(15, 38);
			this.afterBox.Name = "afterBox";
			this.afterBox.Size = new System.Drawing.Size(186, 17);
			this.afterBox.TabIndex = 1;
			this.afterBox.Text = "Remove spacing after paragraphs";
			this.afterBox.UseVisualStyleBackColor = true;
			// 
			// betweenBox
			// 
			this.betweenBox.AutoSize = true;
			this.betweenBox.Checked = true;
			this.betweenBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.betweenBox.Location = new System.Drawing.Point(15, 61);
			this.betweenBox.Name = "betweenBox";
			this.betweenBox.Size = new System.Drawing.Size(174, 17);
			this.betweenBox.TabIndex = 2;
			this.betweenBox.Text = "Remove spacing between lines";
			this.betweenBox.UseVisualStyleBackColor = true;
			// 
			// headingsBox
			// 
			this.headingsBox.AutoSize = true;
			this.headingsBox.Checked = true;
			this.headingsBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.headingsBox.Location = new System.Drawing.Point(15, 101);
			this.headingsBox.Name = "headingsBox";
			this.headingsBox.Size = new System.Drawing.Size(107, 17);
			this.headingsBox.TabIndex = 3;
			this.headingsBox.Text = "Include headings";
			this.headingsBox.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(180, 140);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 23);
			this.cancelButton.TabIndex = 4;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(99, 140);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 5;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// RemoveSpacingDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(270, 178);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.headingsBox);
			this.Controls.Add(this.betweenBox);
			this.Controls.Add(this.afterBox);
			this.Controls.Add(this.beforeBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "RemoveSpacingDialog";
			this.Padding = new System.Windows.Forms.Padding(12);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Remove Spacing";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.CheckBox beforeBox;
		private System.Windows.Forms.CheckBox afterBox;
		private System.Windows.Forms.CheckBox betweenBox;
		private System.Windows.Forms.CheckBox headingsBox;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
	}
}