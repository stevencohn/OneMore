namespace River.OneMoreAddIn.Commands
{
	partial class ChangePageColorDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChangePageColorDialog));
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.introLabel = new System.Windows.Forms.Label();
			this.colorsBox = new River.OneMoreAddIn.UI.ColorsComboBox();
			this.customLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.themeIntroLabel = new System.Windows.Forms.Label();
			this.themeGroup = new System.Windows.Forms.GroupBox();
			this.applyBox = new System.Windows.Forms.CheckBox();
			this.loadLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.themeLabel = new System.Windows.Forms.Label();
			this.currentLabel = new System.Windows.Forms.Label();
			this.statusLabel = new System.Windows.Forms.Label();
			this.themeGroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(413, 412);
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
			this.okButton.Location = new System.Drawing.Point(307, 412);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.TabIndex = 5;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// introLabel
			// 
			this.introLabel.Location = new System.Drawing.Point(13, 10);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(500, 53);
			this.introLabel.TabIndex = 6;
			this.introLabel.Text = "Select on of the predefined background colors or choose a custom color.";
			// 
			// colorsBox
			// 
			this.colorsBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
			this.colorsBox.DropDownHeight = 495;
			this.colorsBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.colorsBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.colorsBox.FormattingEnabled = true;
			this.colorsBox.IntegralHeight = false;
			this.colorsBox.Location = new System.Drawing.Point(39, 66);
			this.colorsBox.Name = "colorsBox";
			this.colorsBox.Size = new System.Drawing.Size(337, 36);
			this.colorsBox.TabIndex = 7;
			this.colorsBox.ColorChanged += new System.EventHandler(this.AnalyzeColorSelection);
			// 
			// customLink
			// 
			this.customLink.AutoSize = true;
			this.customLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.customLink.Location = new System.Drawing.Point(35, 109);
			this.customLink.Name = "customLink";
			this.customLink.Size = new System.Drawing.Size(171, 20);
			this.customLink.TabIndex = 8;
			this.customLink.TabStop = true;
			this.customLink.Text = "Choose a custom color";
			this.customLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ChooseCustomColor);
			// 
			// themeIntroLabel
			// 
			this.themeIntroLabel.Location = new System.Drawing.Point(11, 27);
			this.themeIntroLabel.Name = "themeIntroLabel";
			this.themeIntroLabel.Size = new System.Drawing.Size(468, 53);
			this.themeIntroLabel.TabIndex = 9;
			this.themeIntroLabel.Text = "Optionally, load one of the predefined style themes or customize your own theme s" +
    "o all content has enough contrast to be visible.\r\n\r\n";
			// 
			// themeGroup
			// 
			this.themeGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.themeGroup.Controls.Add(this.applyBox);
			this.themeGroup.Controls.Add(this.loadLink);
			this.themeGroup.Controls.Add(this.themeLabel);
			this.themeGroup.Controls.Add(this.currentLabel);
			this.themeGroup.Controls.Add(this.themeIntroLabel);
			this.themeGroup.Location = new System.Drawing.Point(17, 150);
			this.themeGroup.Name = "themeGroup";
			this.themeGroup.Padding = new System.Windows.Forms.Padding(8);
			this.themeGroup.Size = new System.Drawing.Size(496, 212);
			this.themeGroup.TabIndex = 10;
			this.themeGroup.TabStop = false;
			this.themeGroup.Text = "Style Theme";
			// 
			// applyBox
			// 
			this.applyBox.AutoSize = true;
			this.applyBox.Checked = true;
			this.applyBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.applyBox.Location = new System.Drawing.Point(22, 171);
			this.applyBox.Name = "applyBox";
			this.applyBox.Size = new System.Drawing.Size(246, 24);
			this.applyBox.TabIndex = 13;
			this.applyBox.Text = "Apply style theme to this page";
			this.applyBox.UseVisualStyleBackColor = true;
			// 
			// loadLink
			// 
			this.loadLink.AutoSize = true;
			this.loadLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.loadLink.Location = new System.Drawing.Point(18, 124);
			this.loadLink.Name = "loadLink";
			this.loadLink.Size = new System.Drawing.Size(143, 20);
			this.loadLink.TabIndex = 12;
			this.loadLink.TabStop = true;
			this.loadLink.Text = "Load a style theme";
			this.loadLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LoadStyleTheme);
			// 
			// themeLabel
			// 
			this.themeLabel.AutoSize = true;
			this.themeLabel.Location = new System.Drawing.Point(147, 90);
			this.themeLabel.Name = "themeLabel";
			this.themeLabel.Size = new System.Drawing.Size(14, 20);
			this.themeLabel.TabIndex = 11;
			this.themeLabel.Text = "-";
			// 
			// currentLabel
			// 
			this.currentLabel.AutoSize = true;
			this.currentLabel.Location = new System.Drawing.Point(18, 90);
			this.currentLabel.Name = "currentLabel";
			this.currentLabel.Size = new System.Drawing.Size(123, 20);
			this.currentLabel.TabIndex = 10;
			this.currentLabel.Text = "Current Theme: ";
			// 
			// statusLabel
			// 
			this.statusLabel.AutoSize = true;
			this.statusLabel.ForeColor = System.Drawing.Color.Brown;
			this.statusLabel.Location = new System.Drawing.Point(28, 365);
			this.statusLabel.Name = "statusLabel";
			this.statusLabel.Size = new System.Drawing.Size(31, 20);
			this.statusLabel.TabIndex = 11;
			this.statusLabel.Text = "OK";
			// 
			// ChangePageColorDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(526, 463);
			this.Controls.Add(this.statusLabel);
			this.Controls.Add(this.themeGroup);
			this.Controls.Add(this.customLink);
			this.Controls.Add(this.colorsBox);
			this.Controls.Add(this.introLabel);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ChangePageColorDialog";
			this.Padding = new System.Windows.Forms.Padding(10);
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Change Page Theme";
			this.themeGroup.ResumeLayout(false);
			this.themeGroup.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Label introLabel;
		private UI.ColorsComboBox colorsBox;
		private UI.MoreLinkLabel customLink;
		private System.Windows.Forms.Label themeIntroLabel;
		private System.Windows.Forms.GroupBox themeGroup;
		private System.Windows.Forms.Label currentLabel;
		private System.Windows.Forms.Label themeLabel;
		private UI.MoreLinkLabel loadLink;
		private System.Windows.Forms.CheckBox applyBox;
		private System.Windows.Forms.Label statusLabel;
	}
}