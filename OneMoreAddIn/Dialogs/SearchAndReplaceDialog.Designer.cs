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
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(364, 115);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.TabIndex = 0;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Enabled = false;
			this.okButton.Location = new System.Drawing.Point(258, 115);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.TabIndex = 1;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// whatBox
			// 
			this.whatBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.whatBox.Location = new System.Drawing.Point(133, 23);
			this.whatBox.Name = "whatBox";
			this.whatBox.Size = new System.Drawing.Size(330, 28);
			this.whatBox.TabIndex = 2;
			this.whatBox.TextChanged += new System.EventHandler(this.WTextChanged);
			// 
			// withBox
			// 
			this.withBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.withBox.Location = new System.Drawing.Point(133, 67);
			this.withBox.Name = "withBox";
			this.withBox.Size = new System.Drawing.Size(330, 28);
			this.withBox.TabIndex = 3;
			this.withBox.TextChanged += new System.EventHandler(this.WTextChanged);
			// 
			// whatLabel
			// 
			this.whatLabel.AutoSize = true;
			this.whatLabel.Location = new System.Drawing.Point(23, 28);
			this.whatLabel.Name = "whatLabel";
			this.whatLabel.Size = new System.Drawing.Size(82, 20);
			this.whatLabel.TabIndex = 4;
			this.whatLabel.Text = "Find what:";
			// 
			// withLabel
			// 
			this.withLabel.AutoSize = true;
			this.withLabel.Location = new System.Drawing.Point(23, 72);
			this.withLabel.Name = "withLabel";
			this.withLabel.Size = new System.Drawing.Size(104, 20);
			this.withLabel.TabIndex = 5;
			this.withLabel.Text = "Replace with:";
			// 
			// SearchAndReplaceDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(482, 171);
			this.Controls.Add(this.withLabel);
			this.Controls.Add(this.whatLabel);
			this.Controls.Add(this.withBox);
			this.Controls.Add(this.whatBox);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SearchAndReplaceDialog";
			this.Padding = new System.Windows.Forms.Padding(20, 20, 15, 15);
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
	}
}