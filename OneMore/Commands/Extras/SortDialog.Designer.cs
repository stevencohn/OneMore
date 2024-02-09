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
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.nameButton = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.createdButton = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.scopeBox = new System.Windows.Forms.ComboBox();
			this.scopeLabel = new System.Windows.Forms.Label();
			this.modifiedButton = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.sortPanel = new System.Windows.Forms.Panel();
			this.sortLabel = new System.Windows.Forms.Label();
			this.directionLabel = new System.Windows.Forms.Label();
			this.directionPanel = new System.Windows.Forms.Panel();
			this.desButton = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.ascButton = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.pinNotesBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.sortPanel.SuspendLayout();
			this.directionPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(376, 402);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(120, 38);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.Cancel);
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(245, 402);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(120, 38);
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OK);
			// 
			// nameButton
			// 
			this.nameButton.Checked = true;
			this.nameButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.nameButton.Location = new System.Drawing.Point(3, 3);
			this.nameButton.Name = "nameButton";
			this.nameButton.Size = new System.Drawing.Size(79, 25);
			this.nameButton.TabIndex = 0;
			this.nameButton.TabStop = true;
			this.nameButton.Text = "Name";
			this.nameButton.UseVisualStyleBackColor = true;
			// 
			// createdButton
			// 
			this.createdButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.createdButton.Location = new System.Drawing.Point(3, 32);
			this.createdButton.Name = "createdButton";
			this.createdButton.Size = new System.Drawing.Size(133, 25);
			this.createdButton.TabIndex = 1;
			this.createdButton.Text = "Date Created";
			this.createdButton.UseVisualStyleBackColor = true;
			// 
			// scopeBox
			// 
			this.scopeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.scopeBox.FormattingEnabled = true;
			this.scopeBox.Items.AddRange(new object[] {
            "Children of Current Page",
            "Pages in this section",
            "Sections in this notebook",
            "Sections in this group",
            "Notebooks"});
			this.scopeBox.Location = new System.Drawing.Point(138, 40);
			this.scopeBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 20);
			this.scopeBox.Name = "scopeBox";
			this.scopeBox.Size = new System.Drawing.Size(300, 28);
			this.scopeBox.TabIndex = 2;
			this.scopeBox.SelectedIndexChanged += new System.EventHandler(this.ChangeSelection);
			// 
			// scopeLabel
			// 
			this.scopeLabel.AutoSize = true;
			this.scopeLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.scopeLabel.Location = new System.Drawing.Point(18, 43);
			this.scopeLabel.Name = "scopeLabel";
			this.scopeLabel.Size = new System.Drawing.Size(59, 20);
			this.scopeLabel.TabIndex = 5;
			this.scopeLabel.Text = "Scope:";
			// 
			// modifiedButton
			// 
			this.modifiedButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.modifiedButton.Location = new System.Drawing.Point(3, 63);
			this.modifiedButton.Name = "modifiedButton";
			this.modifiedButton.Size = new System.Drawing.Size(137, 25);
			this.modifiedButton.TabIndex = 2;
			this.modifiedButton.Text = "Date Modified";
			this.modifiedButton.UseVisualStyleBackColor = true;
			// 
			// sortPanel
			// 
			this.sortPanel.Controls.Add(this.nameButton);
			this.sortPanel.Controls.Add(this.modifiedButton);
			this.sortPanel.Controls.Add(this.createdButton);
			this.sortPanel.Location = new System.Drawing.Point(138, 91);
			this.sortPanel.Margin = new System.Windows.Forms.Padding(3, 3, 3, 20);
			this.sortPanel.Name = "sortPanel";
			this.sortPanel.Size = new System.Drawing.Size(353, 98);
			this.sortPanel.TabIndex = 7;
			// 
			// sortLabel
			// 
			this.sortLabel.AutoSize = true;
			this.sortLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.sortLabel.Location = new System.Drawing.Point(18, 96);
			this.sortLabel.Name = "sortLabel";
			this.sortLabel.Size = new System.Drawing.Size(63, 20);
			this.sortLabel.TabIndex = 8;
			this.sortLabel.Text = "Sort by:";
			// 
			// directionLabel
			// 
			this.directionLabel.AutoSize = true;
			this.directionLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.directionLabel.Location = new System.Drawing.Point(22, 212);
			this.directionLabel.Name = "directionLabel";
			this.directionLabel.Size = new System.Drawing.Size(76, 20);
			this.directionLabel.TabIndex = 9;
			this.directionLabel.Text = "Direction:";
			// 
			// directionPanel
			// 
			this.directionPanel.Controls.Add(this.desButton);
			this.directionPanel.Controls.Add(this.ascButton);
			this.directionPanel.Location = new System.Drawing.Point(138, 212);
			this.directionPanel.Margin = new System.Windows.Forms.Padding(3, 3, 3, 20);
			this.directionPanel.Name = "directionPanel";
			this.directionPanel.Size = new System.Drawing.Size(353, 72);
			this.directionPanel.TabIndex = 10;
			// 
			// desButton
			// 
			this.desButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.desButton.Location = new System.Drawing.Point(3, 32);
			this.desButton.Name = "desButton";
			this.desButton.Size = new System.Drawing.Size(123, 25);
			this.desButton.TabIndex = 1;
			this.desButton.Text = "Descending";
			this.desButton.UseVisualStyleBackColor = true;
			// 
			// ascButton
			// 
			this.ascButton.Checked = true;
			this.ascButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.ascButton.Location = new System.Drawing.Point(3, 3);
			this.ascButton.Name = "ascButton";
			this.ascButton.Size = new System.Drawing.Size(112, 25);
			this.ascButton.TabIndex = 0;
			this.ascButton.TabStop = true;
			this.ascButton.Text = "Ascending";
			this.ascButton.UseVisualStyleBackColor = true;
			// 
			// pinNotesBox
			// 
			this.pinNotesBox.Checked = true;
			this.pinNotesBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.pinNotesBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pinNotesBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.pinNotesBox.Location = new System.Drawing.Point(138, 307);
			this.pinNotesBox.Name = "pinNotesBox";
			this.pinNotesBox.Size = new System.Drawing.Size(198, 47);
			this.pinNotesBox.TabIndex = 3;
			this.pinNotesBox.Text = "Pin Notes to top and\r\nQuick Notes to bottom";
			this.pinNotesBox.UseVisualStyleBackColor = true;
			// 
			// SortDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(508, 452);
			this.Controls.Add(this.pinNotesBox);
			this.Controls.Add(this.directionPanel);
			this.Controls.Add(this.directionLabel);
			this.Controls.Add(this.sortLabel);
			this.Controls.Add(this.sortPanel);
			this.Controls.Add(this.scopeLabel);
			this.Controls.Add(this.scopeBox);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SortDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Sort";
			this.sortPanel.ResumeLayout(false);
			this.directionPanel.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreButton cancelButton;
		private UI.MoreButton okButton;
		private UI.MoreRadioButton nameButton;
		private UI.MoreRadioButton createdButton;
		private System.Windows.Forms.ComboBox scopeBox;
		private System.Windows.Forms.Label scopeLabel;
		private UI.MoreRadioButton modifiedButton;
		private System.Windows.Forms.Panel sortPanel;
		private System.Windows.Forms.Label sortLabel;
		private System.Windows.Forms.Label directionLabel;
		private System.Windows.Forms.Panel directionPanel;
		private UI.MoreRadioButton desButton;
		private UI.MoreRadioButton ascButton;
		private UI.MoreCheckBox pinNotesBox;
	}
}