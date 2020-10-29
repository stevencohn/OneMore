namespace River.OneMoreAddIn.Commands.Search
{
	partial class SearchDialog
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
			this.introLabel = new System.Windows.Forms.Label();
			this.findLabel = new System.Windows.Forms.Label();
			this.findBox = new System.Windows.Forms.TextBox();
			this.cancelButton = new System.Windows.Forms.Button();
			this.searchButton = new System.Windows.Forms.Button();
			this.allButton = new System.Windows.Forms.RadioButton();
			this.notebookButton = new System.Windows.Forms.RadioButton();
			this.sectionButton = new System.Windows.Forms.RadioButton();
			this.resultBox = new System.Windows.Forms.TextBox();
			this.copyButton = new System.Windows.Forms.Button();
			this.moveButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// introLabel
			// 
			this.introLabel.AutoSize = true;
			this.introLabel.Location = new System.Drawing.Point(18, 15);
			this.introLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 10);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(391, 13);
			this.introLabel.TabIndex = 0;
			this.introLabel.Text = "Enter one or more keywords, \"quoted phrases\", and use uppercase AND and OR";
			// 
			// findLabel
			// 
			this.findLabel.AutoSize = true;
			this.findLabel.Location = new System.Drawing.Point(18, 44);
			this.findLabel.Name = "findLabel";
			this.findLabel.Size = new System.Drawing.Size(30, 13);
			this.findLabel.TabIndex = 1;
			this.findLabel.Text = "Find:";
			// 
			// findBox
			// 
			this.findBox.Location = new System.Drawing.Point(54, 41);
			this.findBox.Name = "findBox";
			this.findBox.Size = new System.Drawing.Size(397, 20);
			this.findBox.TabIndex = 2;
			this.findBox.TextChanged += new System.EventHandler(this.ChangeQuery);
			this.findBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SearchOnKeydown);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(376, 122);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 23);
			this.cancelButton.TabIndex = 3;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// searchButton
			// 
			this.searchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.searchButton.Enabled = false;
			this.searchButton.Location = new System.Drawing.Point(295, 122);
			this.searchButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.searchButton.Name = "searchButton";
			this.searchButton.Size = new System.Drawing.Size(75, 23);
			this.searchButton.TabIndex = 4;
			this.searchButton.Text = "Search";
			this.searchButton.UseVisualStyleBackColor = true;
			this.searchButton.Click += new System.EventHandler(this.Search);
			// 
			// allButton
			// 
			this.allButton.AutoSize = true;
			this.allButton.Checked = true;
			this.allButton.Location = new System.Drawing.Point(54, 67);
			this.allButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.allButton.Name = "allButton";
			this.allButton.Size = new System.Drawing.Size(100, 17);
			this.allButton.TabIndex = 5;
			this.allButton.TabStop = true;
			this.allButton.Text = "In all notebooks";
			this.allButton.UseVisualStyleBackColor = true;
			// 
			// notebookButton
			// 
			this.notebookButton.AutoSize = true;
			this.notebookButton.Location = new System.Drawing.Point(54, 87);
			this.notebookButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.notebookButton.Name = "notebookButton";
			this.notebookButton.Size = new System.Drawing.Size(101, 17);
			this.notebookButton.TabIndex = 6;
			this.notebookButton.Text = "In this notebook";
			this.notebookButton.UseVisualStyleBackColor = true;
			// 
			// sectionButton
			// 
			this.sectionButton.AutoSize = true;
			this.sectionButton.Location = new System.Drawing.Point(54, 107);
			this.sectionButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.sectionButton.Name = "sectionButton";
			this.sectionButton.Size = new System.Drawing.Size(90, 17);
			this.sectionButton.TabIndex = 7;
			this.sectionButton.Text = "In this section";
			this.sectionButton.UseVisualStyleBackColor = true;
			// 
			// resultBox
			// 
			this.resultBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.resultBox.Location = new System.Drawing.Point(21, 158);
			this.resultBox.Multiline = true;
			this.resultBox.Name = "resultBox";
			this.resultBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.resultBox.Size = new System.Drawing.Size(430, 268);
			this.resultBox.TabIndex = 8;
			// 
			// copyButton
			// 
			this.copyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.copyButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.copyButton.Enabled = false;
			this.copyButton.Location = new System.Drawing.Point(295, 432);
			this.copyButton.Name = "copyButton";
			this.copyButton.Size = new System.Drawing.Size(75, 23);
			this.copyButton.TabIndex = 10;
			this.copyButton.Text = "Copy";
			this.copyButton.UseVisualStyleBackColor = true;
			this.copyButton.Click += new System.EventHandler(this.CopyPressed);
			// 
			// moveButton
			// 
			this.moveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.moveButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.moveButton.Enabled = false;
			this.moveButton.Location = new System.Drawing.Point(376, 432);
			this.moveButton.Name = "moveButton";
			this.moveButton.Size = new System.Drawing.Size(75, 23);
			this.moveButton.TabIndex = 9;
			this.moveButton.Text = "Move";
			this.moveButton.UseVisualStyleBackColor = true;
			this.moveButton.Click += new System.EventHandler(this.MovePressed);
			// 
			// SearchDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(469, 468);
			this.Controls.Add(this.copyButton);
			this.Controls.Add(this.moveButton);
			this.Controls.Add(this.resultBox);
			this.Controls.Add(this.sectionButton);
			this.Controls.Add(this.notebookButton);
			this.Controls.Add(this.allButton);
			this.Controls.Add(this.searchButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.findBox);
			this.Controls.Add(this.findLabel);
			this.Controls.Add(this.introLabel);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(485, 200);
			this.Name = "SearchDialog";
			this.Padding = new System.Windows.Forms.Padding(15, 15, 15, 10);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Search";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label introLabel;
		private System.Windows.Forms.Label findLabel;
		private System.Windows.Forms.TextBox findBox;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button searchButton;
		private System.Windows.Forms.RadioButton allButton;
		private System.Windows.Forms.RadioButton notebookButton;
		private System.Windows.Forms.RadioButton sectionButton;
		private System.Windows.Forms.TextBox resultBox;
		private System.Windows.Forms.Button copyButton;
		private System.Windows.Forms.Button moveButton;
	}
}