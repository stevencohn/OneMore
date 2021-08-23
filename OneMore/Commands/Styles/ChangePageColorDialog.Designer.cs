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
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(382, 206);
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
			this.okButton.Location = new System.Drawing.Point(276, 206);
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
			this.introLabel.Size = new System.Drawing.Size(469, 63);
			this.introLabel.TabIndex = 6;
			this.introLabel.Text = "After choosing a page color, load one of the predefined styles or customize your " +
    "own styles so all content has enough contrast to be visible.\r\n\r\n";
			// 
			// colorsBox
			// 
			this.colorsBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
			this.colorsBox.DropDownHeight = 495;
			this.colorsBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.colorsBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.colorsBox.FormattingEnabled = true;
			this.colorsBox.IntegralHeight = false;
			this.colorsBox.Location = new System.Drawing.Point(17, 97);
			this.colorsBox.Name = "colorsBox";
			this.colorsBox.Size = new System.Drawing.Size(359, 36);
			this.colorsBox.TabIndex = 7;
			// 
			// customLink
			// 
			this.customLink.AutoSize = true;
			this.customLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.customLink.Location = new System.Drawing.Point(13, 136);
			this.customLink.Name = "customLink";
			this.customLink.Size = new System.Drawing.Size(171, 20);
			this.customLink.TabIndex = 8;
			this.customLink.TabStop = true;
			this.customLink.Text = "Choose a custom color";
			this.customLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ChooseCustomColor);
			// 
			// ChangePageColorDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(495, 257);
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
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Label introLabel;
		private UI.ColorsComboBox colorsBox;
		private UI.MoreLinkLabel customLink;
	}
}