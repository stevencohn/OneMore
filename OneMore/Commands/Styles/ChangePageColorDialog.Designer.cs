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
			this.label1 = new System.Windows.Forms.Label();
			this.themeGroup = new System.Windows.Forms.GroupBox();
			this.themeGroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(413, 348);
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
			this.okButton.Location = new System.Drawing.Point(307, 348);
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
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(11, 27);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(468, 53);
			this.label1.TabIndex = 9;
			this.label1.Text = "Optionally, load one of the predefined style themes or customize your own theme s" +
    "o all content has enough contrast to be visible.\r\n\r\n";
			// 
			// themeGroup
			// 
			this.themeGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.themeGroup.Controls.Add(this.label1);
			this.themeGroup.Location = new System.Drawing.Point(17, 150);
			this.themeGroup.Name = "themeGroup";
			this.themeGroup.Padding = new System.Windows.Forms.Padding(8);
			this.themeGroup.Size = new System.Drawing.Size(496, 182);
			this.themeGroup.TabIndex = 10;
			this.themeGroup.TabStop = false;
			this.themeGroup.Text = "Style Theme";
			// 
			// ChangePageColorDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(526, 399);
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
			this.Text = "Change Page Color";
			this.themeGroup.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Label introLabel;
		private UI.ColorsComboBox colorsBox;
		private UI.MoreLinkLabel customLink;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox themeGroup;
	}
}