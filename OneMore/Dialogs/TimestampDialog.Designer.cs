namespace River.OneMoreAddIn.Dialogs
{
	partial class TimestampDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TimestampDialog));
			this.hideRadio = new System.Windows.Forms.RadioButton();
			this.showRadio = new System.Windows.Forms.RadioButton();
			this.toggleGroup = new System.Windows.Forms.GroupBox();
			this.scopeGroup = new System.Windows.Forms.GroupBox();
			this.sectionRadio = new System.Windows.Forms.RadioButton();
			this.pageRadio = new System.Windows.Forms.RadioButton();
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.toggleGroup.SuspendLayout();
			this.scopeGroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// hideRadio
			// 
			this.hideRadio.AutoSize = true;
			this.hideRadio.Checked = true;
			this.hideRadio.Location = new System.Drawing.Point(18, 37);
			this.hideRadio.Name = "hideRadio";
			this.hideRadio.Size = new System.Drawing.Size(168, 24);
			this.hideRadio.TabIndex = 0;
			this.hideRadio.TabStop = true;
			this.hideRadio.Text = "Hide date and time";
			this.hideRadio.UseVisualStyleBackColor = true;
			// 
			// showRadio
			// 
			this.showRadio.AutoSize = true;
			this.showRadio.Location = new System.Drawing.Point(18, 67);
			this.showRadio.Name = "showRadio";
			this.showRadio.Size = new System.Drawing.Size(175, 24);
			this.showRadio.TabIndex = 1;
			this.showRadio.Text = "Show date and time";
			this.showRadio.UseVisualStyleBackColor = true;
			// 
			// toggleGroup
			// 
			this.toggleGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.toggleGroup.Controls.Add(this.hideRadio);
			this.toggleGroup.Controls.Add(this.showRadio);
			this.toggleGroup.Location = new System.Drawing.Point(18, 18);
			this.toggleGroup.Name = "toggleGroup";
			this.toggleGroup.Padding = new System.Windows.Forms.Padding(15);
			this.toggleGroup.Size = new System.Drawing.Size(424, 119);
			this.toggleGroup.TabIndex = 2;
			this.toggleGroup.TabStop = false;
			this.toggleGroup.Text = "Toggle";
			// 
			// scopeGroup
			// 
			this.scopeGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.scopeGroup.Controls.Add(this.sectionRadio);
			this.scopeGroup.Controls.Add(this.pageRadio);
			this.scopeGroup.Location = new System.Drawing.Point(18, 159);
			this.scopeGroup.Name = "scopeGroup";
			this.scopeGroup.Padding = new System.Windows.Forms.Padding(15);
			this.scopeGroup.Size = new System.Drawing.Size(424, 124);
			this.scopeGroup.TabIndex = 3;
			this.scopeGroup.TabStop = false;
			this.scopeGroup.Text = "Scope";
			// 
			// sectionRadio
			// 
			this.sectionRadio.AutoSize = true;
			this.sectionRadio.Location = new System.Drawing.Point(18, 67);
			this.sectionRadio.Name = "sectionRadio";
			this.sectionRadio.Size = new System.Drawing.Size(199, 24);
			this.sectionRadio.TabIndex = 1;
			this.sectionRadio.Text = "All pages in this section";
			this.sectionRadio.UseVisualStyleBackColor = true;
			// 
			// pageRadio
			// 
			this.pageRadio.AutoSize = true;
			this.pageRadio.Checked = true;
			this.pageRadio.Location = new System.Drawing.Point(18, 37);
			this.pageRadio.Name = "pageRadio";
			this.pageRadio.Size = new System.Drawing.Size(103, 24);
			this.pageRadio.TabIndex = 0;
			this.pageRadio.TabStop = true;
			this.pageRadio.Text = "This page";
			this.pageRadio.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(342, 305);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.TabIndex = 4;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(236, 305);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.TabIndex = 5;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// TimestampDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(460, 361);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.scopeGroup);
			this.Controls.Add(this.toggleGroup);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "TimestampDialog";
			this.Padding = new System.Windows.Forms.Padding(15);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Show/Hide Date and Time";
			this.toggleGroup.ResumeLayout(false);
			this.toggleGroup.PerformLayout();
			this.scopeGroup.ResumeLayout(false);
			this.scopeGroup.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RadioButton hideRadio;
		private System.Windows.Forms.RadioButton showRadio;
		private System.Windows.Forms.GroupBox toggleGroup;
		private System.Windows.Forms.GroupBox scopeGroup;
		private System.Windows.Forms.RadioButton sectionRadio;
		private System.Windows.Forms.RadioButton pageRadio;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
	}
}