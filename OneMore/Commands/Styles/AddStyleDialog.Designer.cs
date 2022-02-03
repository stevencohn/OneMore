namespace River.OneMoreAddIn.Commands
{
	partial class AddStyleDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddStyleDialog));
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.nameLabel = new System.Windows.Forms.Label();
			this.nameBox = new System.Windows.Forms.TextBox();
			this.errorLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Enabled = false;
			this.okButton.Location = new System.Drawing.Point(344, 100);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(116, 38);
			this.okButton.TabIndex = 11;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(464, 100);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(116, 38);
			this.cancelButton.TabIndex = 12;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// nameLabel
			// 
			this.nameLabel.AutoSize = true;
			this.nameLabel.Location = new System.Drawing.Point(23, 26);
			this.nameLabel.Name = "nameLabel";
			this.nameLabel.Size = new System.Drawing.Size(51, 20);
			this.nameLabel.TabIndex = 13;
			this.nameLabel.Text = "Name";
			// 
			// nameBox
			// 
			this.nameBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.nameBox.Location = new System.Drawing.Point(118, 23);
			this.nameBox.Name = "nameBox";
			this.nameBox.Size = new System.Drawing.Size(462, 26);
			this.nameBox.TabIndex = 14;
			this.nameBox.TextChanged += new System.EventHandler(this.NameBoxTextChanged);
			// 
			// errorLabel
			// 
			this.errorLabel.AutoSize = true;
			this.errorLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.errorLabel.Location = new System.Drawing.Point(114, 52);
			this.errorLabel.Name = "errorLabel";
			this.errorLabel.Size = new System.Drawing.Size(282, 20);
			this.errorLabel.TabIndex = 15;
			this.errorLabel.Text = "Name exists. Choose a different name.";
			this.errorLabel.Visible = false;
			// 
			// AddStyleDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(592, 150);
			this.Controls.Add(this.errorLabel);
			this.Controls.Add(this.nameBox);
			this.Controls.Add(this.nameLabel);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AddStyleDialog";
			this.Padding = new System.Windows.Forms.Padding(20, 20, 0, 0);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "New Style";
			this.Activated += new System.EventHandler(this.DialogLoad);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Label nameLabel;
		private System.Windows.Forms.TextBox nameBox;
		private System.Windows.Forms.Label errorLabel;
	}
}