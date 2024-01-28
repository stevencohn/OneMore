﻿namespace River.OneMoreAddIn.Commands
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
			this.beforeBox = new UI.MoreCheckBox();
			this.afterBox = new UI.MoreCheckBox();
			this.betweenBox = new UI.MoreCheckBox();
			this.headingsBox = new UI.MoreCheckBox();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.SuspendLayout();
			// 
			// beforeBox
			// 
			this.beforeBox.AutoSize = true;
			this.beforeBox.Checked = true;
			this.beforeBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.beforeBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.beforeBox.Location = new System.Drawing.Point(22, 23);
			this.beforeBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.beforeBox.Name = "beforeBox";
			this.beforeBox.Size = new System.Drawing.Size(288, 24);
			this.beforeBox.TabIndex = 1;
			this.beforeBox.Text = "Remove spacing before paragraphs";
			this.beforeBox.UseVisualStyleBackColor = true;
			// 
			// afterBox
			// 
			this.afterBox.AutoSize = true;
			this.afterBox.Checked = true;
			this.afterBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.afterBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.afterBox.Location = new System.Drawing.Point(22, 58);
			this.afterBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.afterBox.Name = "afterBox";
			this.afterBox.Size = new System.Drawing.Size(275, 24);
			this.afterBox.TabIndex = 2;
			this.afterBox.Text = "Remove spacing after paragraphs";
			this.afterBox.UseVisualStyleBackColor = true;
			// 
			// betweenBox
			// 
			this.betweenBox.AutoSize = true;
			this.betweenBox.Checked = true;
			this.betweenBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.betweenBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.betweenBox.Location = new System.Drawing.Point(22, 94);
			this.betweenBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.betweenBox.Name = "betweenBox";
			this.betweenBox.Size = new System.Drawing.Size(254, 24);
			this.betweenBox.TabIndex = 3;
			this.betweenBox.Text = "Remove spacing between lines";
			this.betweenBox.UseVisualStyleBackColor = true;
			// 
			// headingsBox
			// 
			this.headingsBox.AutoSize = true;
			this.headingsBox.Checked = true;
			this.headingsBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.headingsBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.headingsBox.Location = new System.Drawing.Point(22, 155);
			this.headingsBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.headingsBox.Name = "headingsBox";
			this.headingsBox.Size = new System.Drawing.Size(156, 24);
			this.headingsBox.TabIndex = 4;
			this.headingsBox.Text = "Include headings";
			this.headingsBox.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(325, 230);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.PreferredBack = System.Drawing.Color.Empty;
			this.cancelButton.PreferredFore = System.Drawing.Color.Empty;
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(112, 35);
			this.cancelButton.TabIndex = 5;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(205, 230);
			this.okButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.okButton.Name = "okButton";
			this.okButton.PreferredBack = System.Drawing.Color.Empty;
			this.okButton.PreferredFore = System.Drawing.Color.Empty;
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(112, 35);
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// RemoveSpacingDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(451, 280);
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
			this.Padding = new System.Windows.Forms.Padding(18, 18, 10, 10);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Remove Spacing";
			this.ResumeLayout(false);
			this.PerformLayout();

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