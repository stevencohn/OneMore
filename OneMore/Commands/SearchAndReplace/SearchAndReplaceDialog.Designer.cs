namespace River.OneMoreAddIn.Commands
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
			this.whatLabel = new System.Windows.Forms.Label();
			this.withLabel = new System.Windows.Forms.Label();
			this.matchBox = new System.Windows.Forms.CheckBox();
			this.regBox = new System.Windows.Forms.CheckBox();
			this.whatBox = new System.Windows.Forms.ComboBox();
			this.withBox = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(386, 192);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.TabIndex = 5;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Enabled = false;
			this.okButton.Location = new System.Drawing.Point(279, 192);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.TabIndex = 4;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// whatLabel
			// 
			this.whatLabel.AutoSize = true;
			this.whatLabel.Location = new System.Drawing.Point(16, 31);
			this.whatLabel.Name = "whatLabel";
			this.whatLabel.Size = new System.Drawing.Size(82, 20);
			this.whatLabel.TabIndex = 5;
			this.whatLabel.Text = "Find what:";
			// 
			// withLabel
			// 
			this.withLabel.AutoSize = true;
			this.withLabel.Location = new System.Drawing.Point(16, 70);
			this.withLabel.Name = "withLabel";
			this.withLabel.Size = new System.Drawing.Size(104, 20);
			this.withLabel.TabIndex = 6;
			this.withLabel.Text = "Replace with:";
			// 
			// matchBox
			// 
			this.matchBox.AutoSize = true;
			this.matchBox.Location = new System.Drawing.Point(154, 108);
			this.matchBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.matchBox.Name = "matchBox";
			this.matchBox.Size = new System.Drawing.Size(117, 24);
			this.matchBox.TabIndex = 2;
			this.matchBox.Text = "Match case";
			this.matchBox.UseVisualStyleBackColor = true;
			// 
			// regBox
			// 
			this.regBox.AutoSize = true;
			this.regBox.Location = new System.Drawing.Point(154, 142);
			this.regBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.regBox.Name = "regBox";
			this.regBox.Size = new System.Drawing.Size(205, 24);
			this.regBox.TabIndex = 3;
			this.regBox.Text = "Use regular expressions";
			this.regBox.UseVisualStyleBackColor = true;
			// 
			// whatBox
			// 
			this.whatBox.FormattingEnabled = true;
			this.whatBox.Location = new System.Drawing.Point(154, 28);
			this.whatBox.Name = "whatBox";
			this.whatBox.Size = new System.Drawing.Size(330, 28);
			this.whatBox.TabIndex = 7;
			this.whatBox.SelectedIndexChanged += new System.EventHandler(this.SelectedWhat);
			this.whatBox.TextChanged += new System.EventHandler(this.WTextChanged);
			// 
			// withBox
			// 
			this.withBox.FormattingEnabled = true;
			this.withBox.Location = new System.Drawing.Point(154, 67);
			this.withBox.Name = "withBox";
			this.withBox.Size = new System.Drawing.Size(330, 28);
			this.withBox.TabIndex = 8;
			// 
			// SearchAndReplaceDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(502, 248);
			this.Controls.Add(this.withBox);
			this.Controls.Add(this.whatBox);
			this.Controls.Add(this.regBox);
			this.Controls.Add(this.matchBox);
			this.Controls.Add(this.withLabel);
			this.Controls.Add(this.whatLabel);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SearchAndReplaceDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Search and Replace";
			this.Activated += new System.EventHandler(this.SearchAndReplaceDialog_Shown);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Label whatLabel;
		private System.Windows.Forms.Label withLabel;
		private System.Windows.Forms.CheckBox matchBox;
		private System.Windows.Forms.CheckBox regBox;
		private System.Windows.Forms.ComboBox whatBox;
		private System.Windows.Forms.ComboBox withBox;
	}
}