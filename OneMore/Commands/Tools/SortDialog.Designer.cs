namespace River.OneMoreAddIn.Commands
{
	partial class SortDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SortDialog));
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.nameButton = new System.Windows.Forms.RadioButton();
			this.createdButton = new System.Windows.Forms.RadioButton();
			this.scopeBox = new System.Windows.Forms.ComboBox();
			this.scopeLabel = new System.Windows.Forms.Label();
			this.modifiedButton = new System.Windows.Forms.RadioButton();
			this.sortPanel = new System.Windows.Forms.Panel();
			this.sortLabel = new System.Windows.Forms.Label();
			this.directionLabel = new System.Windows.Forms.Label();
			this.directionPanel = new System.Windows.Forms.Panel();
			this.desButton = new System.Windows.Forms.RadioButton();
			this.ascButton = new System.Windows.Forms.RadioButton();
			this.pinNotesBox = new System.Windows.Forms.CheckBox();
			this.sortPanel.SuspendLayout();
			this.directionPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(202, 239);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(67, 25);
			this.cancelButton.TabIndex = 0;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.Cancel);
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(131, 240);
			this.okButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(67, 25);
			this.okButton.TabIndex = 1;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OK);
			// 
			// nameButton
			// 
			this.nameButton.AutoSize = true;
			this.nameButton.Checked = true;
			this.nameButton.Location = new System.Drawing.Point(2, 2);
			this.nameButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.nameButton.Name = "nameButton";
			this.nameButton.Size = new System.Drawing.Size(53, 17);
			this.nameButton.TabIndex = 2;
			this.nameButton.TabStop = true;
			this.nameButton.Text = "Name";
			this.nameButton.UseVisualStyleBackColor = true;
			// 
			// createdButton
			// 
			this.createdButton.AutoSize = true;
			this.createdButton.Location = new System.Drawing.Point(2, 21);
			this.createdButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.createdButton.Name = "createdButton";
			this.createdButton.Size = new System.Drawing.Size(88, 17);
			this.createdButton.TabIndex = 3;
			this.createdButton.Text = "Date Created";
			this.createdButton.UseVisualStyleBackColor = true;
			// 
			// scopeBox
			// 
			this.scopeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.scopeBox.FormattingEnabled = true;
			this.scopeBox.Items.AddRange(new object[] {
            "Pages in this section",
            "Sections in this notebook",
            "Notebooks"});
			this.scopeBox.Location = new System.Drawing.Point(69, 26);
			this.scopeBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.scopeBox.Name = "scopeBox";
			this.scopeBox.Size = new System.Drawing.Size(201, 21);
			this.scopeBox.TabIndex = 4;
			this.scopeBox.SelectedIndexChanged += new System.EventHandler(this.ChangeSelection);
			// 
			// scopeLabel
			// 
			this.scopeLabel.AutoSize = true;
			this.scopeLabel.Location = new System.Drawing.Point(12, 28);
			this.scopeLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.scopeLabel.Name = "scopeLabel";
			this.scopeLabel.Size = new System.Drawing.Size(41, 13);
			this.scopeLabel.TabIndex = 5;
			this.scopeLabel.Text = "Scope:";
			// 
			// modifiedButton
			// 
			this.modifiedButton.AutoSize = true;
			this.modifiedButton.Location = new System.Drawing.Point(2, 41);
			this.modifiedButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.modifiedButton.Name = "modifiedButton";
			this.modifiedButton.Size = new System.Drawing.Size(91, 17);
			this.modifiedButton.TabIndex = 6;
			this.modifiedButton.Text = "Date Modified";
			this.modifiedButton.UseVisualStyleBackColor = true;
			// 
			// sortPanel
			// 
			this.sortPanel.Controls.Add(this.nameButton);
			this.sortPanel.Controls.Add(this.modifiedButton);
			this.sortPanel.Controls.Add(this.createdButton);
			this.sortPanel.Location = new System.Drawing.Point(69, 59);
			this.sortPanel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.sortPanel.Name = "sortPanel";
			this.sortPanel.Size = new System.Drawing.Size(200, 64);
			this.sortPanel.TabIndex = 7;
			// 
			// sortLabel
			// 
			this.sortLabel.AutoSize = true;
			this.sortLabel.Location = new System.Drawing.Point(12, 59);
			this.sortLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.sortLabel.Name = "sortLabel";
			this.sortLabel.Size = new System.Drawing.Size(43, 13);
			this.sortLabel.TabIndex = 8;
			this.sortLabel.Text = "Sort by:";
			// 
			// directionLabel
			// 
			this.directionLabel.AutoSize = true;
			this.directionLabel.Location = new System.Drawing.Point(15, 138);
			this.directionLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.directionLabel.Name = "directionLabel";
			this.directionLabel.Size = new System.Drawing.Size(52, 13);
			this.directionLabel.TabIndex = 9;
			this.directionLabel.Text = "Direction:";
			// 
			// directionPanel
			// 
			this.directionPanel.Controls.Add(this.desButton);
			this.directionPanel.Controls.Add(this.ascButton);
			this.directionPanel.Location = new System.Drawing.Point(69, 138);
			this.directionPanel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.directionPanel.Name = "directionPanel";
			this.directionPanel.Size = new System.Drawing.Size(200, 47);
			this.directionPanel.TabIndex = 10;
			// 
			// desButton
			// 
			this.desButton.AutoSize = true;
			this.desButton.Location = new System.Drawing.Point(2, 21);
			this.desButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.desButton.Name = "desButton";
			this.desButton.Size = new System.Drawing.Size(82, 17);
			this.desButton.TabIndex = 1;
			this.desButton.Text = "Descending";
			this.desButton.UseVisualStyleBackColor = true;
			// 
			// ascButton
			// 
			this.ascButton.AutoSize = true;
			this.ascButton.Checked = true;
			this.ascButton.Location = new System.Drawing.Point(2, 2);
			this.ascButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.ascButton.Name = "ascButton";
			this.ascButton.Size = new System.Drawing.Size(75, 17);
			this.ascButton.TabIndex = 0;
			this.ascButton.TabStop = true;
			this.ascButton.Text = "Ascending";
			this.ascButton.UseVisualStyleBackColor = true;
			// 
			// pinNotesBox
			// 
			this.pinNotesBox.AutoSize = true;
			this.pinNotesBox.Checked = true;
			this.pinNotesBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.pinNotesBox.Location = new System.Drawing.Point(69, 194);
			this.pinNotesBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.pinNotesBox.Name = "pinNotesBox";
			this.pinNotesBox.Size = new System.Drawing.Size(132, 30);
			this.pinNotesBox.TabIndex = 11;
			this.pinNotesBox.Text = "Pin Notes to top and\r\nQuick Notes to bottom";
			this.pinNotesBox.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.pinNotesBox.UseVisualStyleBackColor = true;
			// 
			// SortDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(284, 272);
			this.Controls.Add(this.pinNotesBox);
			this.Controls.Add(this.directionPanel);
			this.Controls.Add(this.directionLabel);
			this.Controls.Add(this.sortLabel);
			this.Controls.Add(this.sortPanel);
			this.Controls.Add(this.scopeLabel);
			this.Controls.Add(this.scopeBox);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SortDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Sort";
			this.sortPanel.ResumeLayout(false);
			this.sortPanel.PerformLayout();
			this.directionPanel.ResumeLayout(false);
			this.directionPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.RadioButton nameButton;
		private System.Windows.Forms.RadioButton createdButton;
		private System.Windows.Forms.ComboBox scopeBox;
		private System.Windows.Forms.Label scopeLabel;
		private System.Windows.Forms.RadioButton modifiedButton;
		private System.Windows.Forms.Panel sortPanel;
		private System.Windows.Forms.Label sortLabel;
		private System.Windows.Forms.Label directionLabel;
		private System.Windows.Forms.Panel directionPanel;
		private System.Windows.Forms.RadioButton desButton;
		private System.Windows.Forms.RadioButton ascButton;
		private System.Windows.Forms.CheckBox pinNotesBox;
	}
}