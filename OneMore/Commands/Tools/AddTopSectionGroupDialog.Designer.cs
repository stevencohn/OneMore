namespace River.OneMoreAddIn.Commands
{
	partial class AddTopSectionGroupDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddTopSectionGroupDialog));
			this.nameLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.nameBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.errorLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.SuspendLayout();
			//
			// nameLabel
			//
			this.nameLabel.AutoSize = true;
			this.nameLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.nameLabel.Location = new System.Drawing.Point(23, 23);
			this.nameLabel.Name = "nameLabel";
			this.nameLabel.Size = new System.Drawing.Size(51, 20);
			this.nameLabel.TabIndex = 0;
			this.nameLabel.Text = "Name";
			this.nameLabel.ThemedBack = null;
			this.nameLabel.ThemedFore = null;
			//
			// nameBox
			//
			this.nameBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.nameBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.nameBox.Location = new System.Drawing.Point(118, 20);
			this.nameBox.Name = "nameBox";
			this.nameBox.ProcessEnterKey = false;
			this.nameBox.Size = new System.Drawing.Size(514, 26);
			this.nameBox.TabIndex = 1;
			this.nameBox.ThemedBack = null;
			this.nameBox.ThemedFore = null;
			this.nameBox.TextChanged += new System.EventHandler(this.NameBoxTextChanged);
			//
			// errorLabel
			//
			this.errorLabel.AutoSize = true;
			this.errorLabel.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.errorLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.errorLabel.Location = new System.Drawing.Point(114, 49);
			this.errorLabel.Name = "errorLabel";
			this.errorLabel.Size = new System.Drawing.Size(282, 20);
			this.errorLabel.TabIndex = 2;
			this.errorLabel.Text = "Name exists. Choose a different name.";
			this.errorLabel.ThemedBack = "ControlLightLight";
			this.errorLabel.ThemedFore = "ErrorText";
			this.errorLabel.Visible = false;
			//
			// okButton
			//
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(396, 97);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(116, 38);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 3;
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
			this.cancelButton.Location = new System.Drawing.Point(516, 97);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(116, 38);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 4;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			//
			// AddTopSectionGroupDialog
			//
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(650, 155);
			this.Controls.Add(this.errorLabel);
			this.Controls.Add(this.nameBox);
			this.Controls.Add(this.nameLabel);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AddTopSectionGroupDialog";
			this.Padding = new System.Windows.Forms.Padding(20, 20, 0, 0);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Add Top-Level Section Group";
			this.Activated += new System.EventHandler(this.DialogLoad);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreLabel nameLabel;
		private UI.MoreTextBox nameBox;
		private UI.MoreLabel errorLabel;
		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
	}
}
