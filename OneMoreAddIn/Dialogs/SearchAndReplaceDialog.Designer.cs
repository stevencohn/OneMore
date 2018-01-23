namespace River.OneMoreAddIn
{
	partial class SearchAndReplaceDialog
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose (bool disposing)
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
		private void InitializeComponent ()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SearchAndReplaceDialog));
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.whatBox = new System.Windows.Forms.TextBox();
			this.withBox = new System.Windows.Forms.TextBox();
			this.whatLabel = new System.Windows.Forms.Label();
			this.withLabel = new System.Windows.Forms.Label();
			this.matchBox = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(243, 97);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(2);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(67, 25);
			this.cancelButton.TabIndex = 4;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Enabled = false;
			this.okButton.Location = new System.Drawing.Point(172, 97);
			this.okButton.Margin = new System.Windows.Forms.Padding(2);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(67, 25);
			this.okButton.TabIndex = 3;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// whatBox
			// 
			this.whatBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.whatBox.Location = new System.Drawing.Point(89, 15);
			this.whatBox.Margin = new System.Windows.Forms.Padding(2);
			this.whatBox.Name = "whatBox";
			this.whatBox.Size = new System.Drawing.Size(221, 21);
			this.whatBox.TabIndex = 0;
			this.whatBox.TextChanged += new System.EventHandler(this.WTextChanged);
			// 
			// withBox
			// 
			this.withBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.withBox.Location = new System.Drawing.Point(89, 44);
			this.withBox.Margin = new System.Windows.Forms.Padding(2);
			this.withBox.Name = "withBox";
			this.withBox.Size = new System.Drawing.Size(221, 21);
			this.withBox.TabIndex = 1;
			this.withBox.TextChanged += new System.EventHandler(this.WTextChanged);
			// 
			// whatLabel
			// 
			this.whatLabel.AutoSize = true;
			this.whatLabel.Location = new System.Drawing.Point(11, 20);
			this.whatLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.whatLabel.Name = "whatLabel";
			this.whatLabel.Size = new System.Drawing.Size(56, 13);
			this.whatLabel.TabIndex = 5;
			this.whatLabel.Text = "Find what:";
			// 
			// withLabel
			// 
			this.withLabel.AutoSize = true;
			this.withLabel.Location = new System.Drawing.Point(11, 49);
			this.withLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.withLabel.Name = "withLabel";
			this.withLabel.Size = new System.Drawing.Size(72, 13);
			this.withLabel.TabIndex = 6;
			this.withLabel.Text = "Replace with:";
			// 
			// matchBox
			// 
			this.matchBox.AutoSize = true;
			this.matchBox.Location = new System.Drawing.Point(89, 70);
			this.matchBox.Name = "matchBox";
			this.matchBox.Size = new System.Drawing.Size(82, 17);
			this.matchBox.TabIndex = 2;
			this.matchBox.Text = "Match case";
			this.matchBox.UseVisualStyleBackColor = true;
			// 
			// SearchAndReplaceDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(321, 133);
			this.Controls.Add(this.matchBox);
			this.Controls.Add(this.withLabel);
			this.Controls.Add(this.whatLabel);
			this.Controls.Add(this.withBox);
			this.Controls.Add(this.whatBox);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(2);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SearchAndReplaceDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Search and Replace";
			this.Shown += new System.EventHandler(this.SearchAndReplaceDialog_Shown);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.TextBox whatBox;
		private System.Windows.Forms.TextBox withBox;
		private System.Windows.Forms.Label whatLabel;
		private System.Windows.Forms.Label withLabel;
		private System.Windows.Forms.CheckBox matchBox;
	}
}