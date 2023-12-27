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
			this.whatStatusLabel = new System.Windows.Forms.Label();
			this.withStatusLabel = new System.Windows.Forms.Label();
			this.rawBox = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(458, 264);
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
			this.okButton.Location = new System.Drawing.Point(351, 264);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.TabIndex = 4;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// whatLabel
			// 
			this.whatLabel.AutoSize = true;
			this.whatLabel.Location = new System.Drawing.Point(16, 26);
			this.whatLabel.Name = "whatLabel";
			this.whatLabel.Size = new System.Drawing.Size(78, 20);
			this.whatLabel.TabIndex = 5;
			this.whatLabel.Text = "Find what";
			// 
			// withLabel
			// 
			this.withLabel.AutoSize = true;
			this.withLabel.Location = new System.Drawing.Point(16, 162);
			this.withLabel.Name = "withLabel";
			this.withLabel.Size = new System.Drawing.Size(100, 20);
			this.withLabel.TabIndex = 6;
			this.withLabel.Text = "Replace with";
			// 
			// matchBox
			// 
			this.matchBox.AutoSize = true;
			this.matchBox.Location = new System.Drawing.Point(160, 79);
			this.matchBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 3);
			this.matchBox.Name = "matchBox";
			this.matchBox.Size = new System.Drawing.Size(117, 24);
			this.matchBox.TabIndex = 1;
			this.matchBox.Text = "Match case";
			this.matchBox.UseVisualStyleBackColor = true;
			// 
			// regBox
			// 
			this.regBox.AutoSize = true;
			this.regBox.Location = new System.Drawing.Point(160, 107);
			this.regBox.Margin = new System.Windows.Forms.Padding(4, 1, 4, 3);
			this.regBox.Name = "regBox";
			this.regBox.Size = new System.Drawing.Size(205, 24);
			this.regBox.TabIndex = 2;
			this.regBox.Text = "Use regular expressions";
			this.regBox.UseVisualStyleBackColor = true;
			this.regBox.CheckedChanged += new System.EventHandler(this.ToggleRegex);
			// 
			// whatBox
			// 
			this.whatBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.whatBox.FormattingEnabled = true;
			this.whatBox.Location = new System.Drawing.Point(154, 21);
			this.whatBox.Name = "whatBox";
			this.whatBox.Size = new System.Drawing.Size(400, 30);
			this.whatBox.TabIndex = 0;
			this.whatBox.SelectedIndexChanged += new System.EventHandler(this.SelectedWhat);
			this.whatBox.TextChanged += new System.EventHandler(this.CheckPattern);
			// 
			// withBox
			// 
			this.withBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.withBox.FormattingEnabled = true;
			this.withBox.Location = new System.Drawing.Point(154, 157);
			this.withBox.Name = "withBox";
			this.withBox.Size = new System.Drawing.Size(400, 30);
			this.withBox.TabIndex = 3;
			this.withBox.TextChanged += new System.EventHandler(this.CheckXmlFormat);
			// 
			// whatStatusLabel
			// 
			this.whatStatusLabel.AutoSize = true;
			this.whatStatusLabel.ForeColor = System.Drawing.Color.Maroon;
			this.whatStatusLabel.Location = new System.Drawing.Point(156, 54);
			this.whatStatusLabel.Name = "whatStatusLabel";
			this.whatStatusLabel.Size = new System.Drawing.Size(187, 20);
			this.whatStatusLabel.TabIndex = 9;
			this.whatStatusLabel.Text = "Invalid regular expression";
			// 
			// withStatusLabel
			// 
			this.withStatusLabel.AutoSize = true;
			this.withStatusLabel.ForeColor = System.Drawing.SystemColors.MenuHighlight;
			this.withStatusLabel.Location = new System.Drawing.Point(156, 190);
			this.withStatusLabel.Name = "withStatusLabel";
			this.withStatusLabel.Size = new System.Drawing.Size(241, 20);
			this.withStatusLabel.TabIndex = 10;
			this.withStatusLabel.Text = "Valid substitution parameters: $1";
			// 
			// rawBox
			// 
			this.rawBox.AutoSize = true;
			this.rawBox.Location = new System.Drawing.Point(160, 215);
			this.rawBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 3);
			this.rawBox.Name = "rawBox";
			this.rawBox.Size = new System.Drawing.Size(192, 24);
			this.rawBox.TabIndex = 11;
			this.rawBox.Text = "Replace with raw XML";
			this.rawBox.UseVisualStyleBackColor = true;
			this.rawBox.CheckedChanged += new System.EventHandler(this.ToggleRawXml);
			// 
			// SearchAndReplaceDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(574, 320);
			this.Controls.Add(this.rawBox);
			this.Controls.Add(this.withStatusLabel);
			this.Controls.Add(this.whatStatusLabel);
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
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Search and Replace";
			this.Activated += new System.EventHandler(this.FocusOnWhat);
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
		private System.Windows.Forms.Label whatStatusLabel;
		private System.Windows.Forms.Label withStatusLabel;
		private System.Windows.Forms.CheckBox rawBox;
	}
}