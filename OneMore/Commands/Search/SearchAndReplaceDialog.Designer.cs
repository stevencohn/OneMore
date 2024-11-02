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
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.whatLabel = new System.Windows.Forms.Label();
			this.withLabel = new System.Windows.Forms.Label();
			this.matchBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.regBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.whatBox = new System.Windows.Forms.ComboBox();
			this.withBox = new System.Windows.Forms.ComboBox();
			this.whatStatusLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.withStatusLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.rawBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.scopeBox = new System.Windows.Forms.ComboBox();
			this.scopeLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(513, 286);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 5;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Enabled = false;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(406, 286);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 4;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
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
			this.matchBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.matchBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.matchBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.matchBox.Location = new System.Drawing.Point(160, 79);
			this.matchBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 3);
			this.matchBox.Name = "matchBox";
			this.matchBox.Size = new System.Drawing.Size(119, 25);
			this.matchBox.StylizeImage = false;
			this.matchBox.TabIndex = 1;
			this.matchBox.Text = "Match case";
			this.matchBox.ThemedBack = null;
			this.matchBox.ThemedFore = null;
			this.matchBox.UseVisualStyleBackColor = true;
			// 
			// regBox
			// 
			this.regBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.regBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.regBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.regBox.Location = new System.Drawing.Point(160, 107);
			this.regBox.Margin = new System.Windows.Forms.Padding(4, 1, 4, 3);
			this.regBox.Name = "regBox";
			this.regBox.Size = new System.Drawing.Size(213, 25);
			this.regBox.StylizeImage = false;
			this.regBox.TabIndex = 2;
			this.regBox.Text = "Use regular expressions";
			this.regBox.ThemedBack = null;
			this.regBox.ThemedFore = null;
			this.regBox.UseVisualStyleBackColor = true;
			this.regBox.CheckedChanged += new System.EventHandler(this.ToggleRegex);
			// 
			// whatBox
			// 
			this.whatBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.whatBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.whatBox.FormattingEnabled = true;
			this.whatBox.Location = new System.Drawing.Point(154, 21);
			this.whatBox.Name = "whatBox";
			this.whatBox.Size = new System.Drawing.Size(455, 30);
			this.whatBox.TabIndex = 0;
			this.whatBox.SelectedIndexChanged += new System.EventHandler(this.SelectedWhat);
			this.whatBox.TextChanged += new System.EventHandler(this.CheckPattern);
			// 
			// withBox
			// 
			this.withBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.withBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.withBox.FormattingEnabled = true;
			this.withBox.Location = new System.Drawing.Point(154, 157);
			this.withBox.Name = "withBox";
			this.withBox.Size = new System.Drawing.Size(455, 30);
			this.withBox.TabIndex = 3;
			this.withBox.TextChanged += new System.EventHandler(this.CheckXmlFormat);
			// 
			// whatStatusLabel
			// 
			this.whatStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.whatStatusLabel.ForeColor = System.Drawing.Color.Brown;
			this.whatStatusLabel.Location = new System.Drawing.Point(156, 54);
			this.whatStatusLabel.Name = "whatStatusLabel";
			this.whatStatusLabel.Size = new System.Drawing.Size(453, 20);
			this.whatStatusLabel.TabIndex = 9;
			this.whatStatusLabel.Text = "Invalid regular expression";
			this.whatStatusLabel.ThemedBack = null;
			this.whatStatusLabel.ThemedFore = "ErrorText";
			// 
			// withStatusLabel
			// 
			this.withStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.withStatusLabel.ForeColor = System.Drawing.SystemColors.MenuHighlight;
			this.withStatusLabel.Location = new System.Drawing.Point(156, 190);
			this.withStatusLabel.Name = "withStatusLabel";
			this.withStatusLabel.Size = new System.Drawing.Size(453, 20);
			this.withStatusLabel.TabIndex = 10;
			this.withStatusLabel.Text = "Valid substitution parameters: $1";
			this.withStatusLabel.ThemedBack = null;
			this.withStatusLabel.ThemedFore = "HintText";
			// 
			// rawBox
			// 
			this.rawBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.rawBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.rawBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.rawBox.Location = new System.Drawing.Point(154, 281);
			this.rawBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 3);
			this.rawBox.Name = "rawBox";
			this.rawBox.Size = new System.Drawing.Size(199, 25);
			this.rawBox.StylizeImage = false;
			this.rawBox.TabIndex = 11;
			this.rawBox.Text = "Replace with raw XML";
			this.rawBox.ThemedBack = null;
			this.rawBox.ThemedFore = null;
			this.rawBox.UseVisualStyleBackColor = true;
			this.rawBox.CheckedChanged += new System.EventHandler(this.ToggleRawXml);
			// 
			// scopeBox
			// 
			this.scopeBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.scopeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.scopeBox.FormattingEnabled = true;
			this.scopeBox.Items.AddRange(new object[] {
            "This page",
            "This section",
            "This notebook",
            "All notebooks"});
			this.scopeBox.Location = new System.Drawing.Point(154, 227);
			this.scopeBox.Name = "scopeBox";
			this.scopeBox.Size = new System.Drawing.Size(220, 28);
			this.scopeBox.TabIndex = 12;
			this.scopeBox.SelectedIndexChanged += new System.EventHandler(this.ChangeScope);
			// 
			// scopeLabel
			// 
			this.scopeLabel.AutoSize = true;
			this.scopeLabel.Location = new System.Drawing.Point(16, 230);
			this.scopeLabel.Name = "scopeLabel";
			this.scopeLabel.Size = new System.Drawing.Size(55, 20);
			this.scopeLabel.TabIndex = 13;
			this.scopeLabel.Text = "Scope";
			// 
			// SearchAndReplaceDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(629, 342);
			this.Controls.Add(this.scopeLabel);
			this.Controls.Add(this.scopeBox);
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
			this.ForeColor = System.Drawing.SystemColors.ControlText;
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

		private UI.MoreButton cancelButton;
		private UI.MoreButton okButton;
		private System.Windows.Forms.Label whatLabel;
		private System.Windows.Forms.Label withLabel;
		private UI.MoreCheckBox matchBox;
		private UI.MoreCheckBox regBox;
		private System.Windows.Forms.ComboBox whatBox;
		private System.Windows.Forms.ComboBox withBox;
		private UI.MoreLabel whatStatusLabel;
		private UI.MoreLabel withStatusLabel;
		private UI.MoreCheckBox rawBox;
		private System.Windows.Forms.ComboBox scopeBox;
		private System.Windows.Forms.Label scopeLabel;
	}
}