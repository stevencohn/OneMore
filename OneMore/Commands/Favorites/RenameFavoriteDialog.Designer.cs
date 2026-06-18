namespace River.OneMoreAddIn.Commands.Favorites
{
	partial class RenameFavoriteDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RenameFavoriteDialog));
			this.originalNameLabel = new System.Windows.Forms.Label();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.revertButton = new River.OneMoreAddIn.UI.MoreButton();
			this.nameLabel = new System.Windows.Forms.Label();
			this.nameBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.errorLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.SuspendLayout();
			// 
			// originalNameLabel
			// 
			this.originalNameLabel.AutoSize = true;
			this.originalNameLabel.Location = new System.Drawing.Point(23, 23);
			this.originalNameLabel.Name = "originalNameLabel";
			this.originalNameLabel.Size = new System.Drawing.Size(135, 20);
			this.originalNameLabel.TabIndex = 16;
			this.originalNameLabel.Text = "Original Name: {0}";
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(396, 136);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(116, 38);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 11;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.Accept);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(516, 136);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(116, 38);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 12;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// revertButton
			// 
			this.revertButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.revertButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.revertButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.revertButton.ImageOver = null;
			this.revertButton.Location = new System.Drawing.Point(23, 136);
			this.revertButton.Name = "revertButton";
			this.revertButton.ShowBorder = true;
			this.revertButton.Size = new System.Drawing.Size(116, 38);
			this.revertButton.StylizeImage = false;
			this.revertButton.TabIndex = 17;
			this.revertButton.Text = "Revert";
			this.revertButton.ThemedBack = null;
			this.revertButton.ThemedFore = null;
			this.revertButton.UseVisualStyleBackColor = true;
			this.revertButton.Click += new System.EventHandler(this.Revert);
			// 
			// nameLabel
			// 
			this.nameLabel.AutoSize = true;
			this.nameLabel.Location = new System.Drawing.Point(23, 62);
			this.nameLabel.Name = "nameLabel";
			this.nameLabel.Size = new System.Drawing.Size(51, 20);
			this.nameLabel.TabIndex = 13;
			this.nameLabel.Text = "Name";
			// 
			// nameBox
			// 
			this.nameBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.nameBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.nameBox.Location = new System.Drawing.Point(118, 59);
			this.nameBox.Name = "nameBox";
			this.nameBox.ProcessEnterKey = false;
			this.nameBox.Size = new System.Drawing.Size(514, 26);
			this.nameBox.TabIndex = 14;
			this.nameBox.ThemedBack = null;
			this.nameBox.ThemedFore = null;
			this.nameBox.TextChanged += new System.EventHandler(this.NameBoxTextChanged);
			// 
			// errorLabel
			// 
			this.errorLabel.AutoSize = true;
			this.errorLabel.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.errorLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.errorLabel.Location = new System.Drawing.Point(114, 88);
			this.errorLabel.Name = "errorLabel";
			this.errorLabel.Size = new System.Drawing.Size(282, 20);
			this.errorLabel.TabIndex = 15;
			this.errorLabel.Text = "Name exists. Choose a different name.";
			this.errorLabel.ThemedBack = "ControlLightLight";
			this.errorLabel.ThemedFore = "ErrorText";
			this.errorLabel.Visible = false;
			// 
			// RenameFavoriteDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(650, 186);
			this.Controls.Add(this.errorLabel);
			this.Controls.Add(this.nameBox);
			this.Controls.Add(this.nameLabel);
			this.Controls.Add(this.revertButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.originalNameLabel);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "RenameFavoriteDialog";
			this.Padding = new System.Windows.Forms.Padding(20, 20, 0, 0);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Rename Favorite";
			this.Activated += new System.EventHandler(this.DialogLoad);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label originalNameLabel;
		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private UI.MoreButton revertButton;
		private System.Windows.Forms.Label nameLabel;
		private UI.MoreTextBox nameBox;
		private UI.MoreLabel errorLabel;
	}
}
