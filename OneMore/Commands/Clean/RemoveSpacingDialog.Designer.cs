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
			this.beforeBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.afterBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.betweenBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.headingsBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.SuspendLayout();
			// 
			// beforeBox
			// 
			this.beforeBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.beforeBox.Checked = true;
			this.beforeBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.beforeBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.beforeBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.beforeBox.Location = new System.Drawing.Point(29, 30);
			this.beforeBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.beforeBox.Name = "beforeBox";
			this.beforeBox.Size = new System.Drawing.Size(297, 25);
			this.beforeBox.TabIndex = 1;
			this.beforeBox.Text = "Remove spacing before paragraphs";
			this.beforeBox.UseVisualStyleBackColor = true;
			// 
			// afterBox
			// 
			this.afterBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.afterBox.Checked = true;
			this.afterBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.afterBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.afterBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.afterBox.Location = new System.Drawing.Point(29, 65);
			this.afterBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.afterBox.Name = "afterBox";
			this.afterBox.Size = new System.Drawing.Size(283, 25);
			this.afterBox.TabIndex = 2;
			this.afterBox.Text = "Remove spacing after paragraphs";
			this.afterBox.UseVisualStyleBackColor = true;
			// 
			// betweenBox
			// 
			this.betweenBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.betweenBox.Checked = true;
			this.betweenBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.betweenBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.betweenBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.betweenBox.Location = new System.Drawing.Point(29, 101);
			this.betweenBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.betweenBox.Name = "betweenBox";
			this.betweenBox.Size = new System.Drawing.Size(263, 25);
			this.betweenBox.TabIndex = 3;
			this.betweenBox.Text = "Remove spacing between lines";
			this.betweenBox.UseVisualStyleBackColor = true;
			// 
			// headingsBox
			// 
			this.headingsBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.headingsBox.Checked = true;
			this.headingsBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.headingsBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.headingsBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.headingsBox.Location = new System.Drawing.Point(29, 162);
			this.headingsBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.headingsBox.Name = "headingsBox";
			this.headingsBox.Size = new System.Drawing.Size(160, 25);
			this.headingsBox.TabIndex = 4;
			this.headingsBox.Text = "Include headings";
			this.headingsBox.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(342, 230);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(112, 35);
			this.cancelButton.TabIndex = 5;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(222, 230);
			this.okButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(112, 35);
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// RemoveSpacingDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(468, 280);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.headingsBox);
			this.Controls.Add(this.betweenBox);
			this.Controls.Add(this.afterBox);
			this.Controls.Add(this.beforeBox);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "RemoveSpacingDialog";
			this.Padding = new System.Windows.Forms.Padding(25, 25, 10, 10);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Remove Spacing";
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreCheckBox beforeBox;
		private UI.MoreCheckBox afterBox;
		private UI.MoreCheckBox betweenBox;
		private UI.MoreCheckBox headingsBox;
		private UI.MoreButton cancelButton;
		private UI.MoreButton okButton;
	}
}