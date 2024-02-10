
namespace River.OneMoreAddIn.Commands
{
	partial class SplitDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplitDialog));
			this.splitByGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.byLinksLabel = new System.Windows.Forms.Label();
			this.byHeading1Label = new System.Windows.Forms.Label();
			this.byLinksBox = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.byHeading1Box = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.taggedBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.filterGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.tagLabel = new System.Windows.Forms.Label();
			this.tagButton = new River.OneMoreAddIn.UI.MoreButton();
			this.splitByGroup.SuspendLayout();
			this.filterGroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitByGroup
			// 
			this.splitByGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.splitByGroup.Controls.Add(this.byLinksLabel);
			this.splitByGroup.Controls.Add(this.byHeading1Label);
			this.splitByGroup.Controls.Add(this.byLinksBox);
			this.splitByGroup.Controls.Add(this.byHeading1Box);
			this.splitByGroup.Location = new System.Drawing.Point(20, 20);
			this.splitByGroup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 8);
			this.splitByGroup.Name = "splitByGroup";
			this.splitByGroup.Padding = new System.Windows.Forms.Padding(15);
			this.splitByGroup.Size = new System.Drawing.Size(522, 178);
			this.splitByGroup.TabIndex = 0;
			this.splitByGroup.TabStop = false;
			this.splitByGroup.Text = "Split by";
			// 
			// byLinksLabel
			// 
			this.byLinksLabel.AutoSize = true;
			this.byLinksLabel.Location = new System.Drawing.Point(48, 142);
			this.byLinksLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.byLinksLabel.Name = "byLinksLabel";
			this.byLinksLabel.Size = new System.Drawing.Size(247, 20);
			this.byLinksLabel.TabIndex = 3;
			this.byLinksLabel.Text = "Move content to each linked page";
			// 
			// byHeading1Label
			// 
			this.byHeading1Label.AutoSize = true;
			this.byHeading1Label.Location = new System.Drawing.Point(48, 71);
			this.byHeading1Label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 15);
			this.byHeading1Label.Name = "byHeading1Label";
			this.byHeading1Label.Size = new System.Drawing.Size(434, 20);
			this.byHeading1Label.TabIndex = 2;
			this.byHeading1Label.Text = "Link headings to new pages and move content to each page";
			// 
			// byLinksBox
			// 
			this.byLinksBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.byLinksBox.Location = new System.Drawing.Point(20, 111);
			this.byLinksBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.byLinksBox.Name = "byLinksBox";
			this.byLinksBox.Size = new System.Drawing.Size(154, 25);
			this.byLinksBox.TabIndex = 1;
			this.byLinksBox.Text = "Page (wiki) links";
			this.byLinksBox.UseVisualStyleBackColor = true;
			// 
			// byHeading1Box
			// 
			this.byHeading1Box.Checked = true;
			this.byHeading1Box.Cursor = System.Windows.Forms.Cursors.Hand;
			this.byHeading1Box.Location = new System.Drawing.Point(20, 40);
			this.byHeading1Box.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.byHeading1Box.Name = "byHeading1Box";
			this.byHeading1Box.Size = new System.Drawing.Size(110, 25);
			this.byHeading1Box.TabIndex = 0;
			this.byHeading1Box.TabStop = true;
			this.byHeading1Box.Text = "Heading 1";
			this.byHeading1Box.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(429, 372);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(112, 35);
			this.cancelButton.TabIndex = 1;
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
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(308, 372);
			this.okButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(112, 35);
			this.okButton.TabIndex = 2;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// taggedBox
			// 
			this.taggedBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.taggedBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.taggedBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.taggedBox.Location = new System.Drawing.Point(20, 40);
			this.taggedBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.taggedBox.Name = "taggedBox";
			this.taggedBox.Size = new System.Drawing.Size(321, 25);
			this.taggedBox.TabIndex = 4;
			this.taggedBox.Text = "Apply only to tagged headings and lnks";
			this.taggedBox.UseVisualStyleBackColor = true;
			this.taggedBox.Click += new System.EventHandler(this.ToggleTagged);
			// 
			// filterGroup
			// 
			this.filterGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.filterGroup.Controls.Add(this.tagLabel);
			this.filterGroup.Controls.Add(this.tagButton);
			this.filterGroup.Controls.Add(this.taggedBox);
			this.filterGroup.Location = new System.Drawing.Point(20, 211);
			this.filterGroup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.filterGroup.Name = "filterGroup";
			this.filterGroup.Padding = new System.Windows.Forms.Padding(15);
			this.filterGroup.Size = new System.Drawing.Size(522, 140);
			this.filterGroup.TabIndex = 5;
			this.filterGroup.TabStop = false;
			this.filterGroup.Text = "Filter";
			// 
			// tagLabel
			// 
			this.tagLabel.AutoSize = true;
			this.tagLabel.Location = new System.Drawing.Point(48, 85);
			this.tagLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.tagLabel.Name = "tagLabel";
			this.tagLabel.Size = new System.Drawing.Size(40, 20);
			this.tagLabel.TabIndex = 20;
			this.tagLabel.Text = "Tag:";
			// 
			// tagButton
			// 
			this.tagButton.BackColor = System.Drawing.SystemColors.Window;
			this.tagButton.Enabled = false;
			this.tagButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.tagButton.ImageOver = null;
			this.tagButton.Location = new System.Drawing.Point(100, 75);
			this.tagButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tagButton.Name = "tagButton";
			this.tagButton.ShowBorder = true;
			this.tagButton.Size = new System.Drawing.Size(60, 38);
			this.tagButton.TabIndex = 19;
			this.tagButton.Text = "?";
			this.tagButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.tagButton.ThemedBack = null;
			this.tagButton.ThemedFore = null;
			this.tagButton.UseVisualStyleBackColor = false;
			this.tagButton.Click += new System.EventHandler(this.SetTag);
			// 
			// SplitDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(561, 428);
			this.Controls.Add(this.filterGroup);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.splitByGroup);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SplitDialog";
			this.Padding = new System.Windows.Forms.Padding(15);
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Split Page";
			this.splitByGroup.ResumeLayout(false);
			this.splitByGroup.PerformLayout();
			this.filterGroup.ResumeLayout(false);
			this.filterGroup.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreGroupBox splitByGroup;
		private System.Windows.Forms.Label byLinksLabel;
		private System.Windows.Forms.Label byHeading1Label;
		private UI.MoreRadioButton byLinksBox;
		private UI.MoreRadioButton byHeading1Box;
		private UI.MoreButton cancelButton;
		private UI.MoreButton okButton;
		private UI.MoreCheckBox taggedBox;
		private UI.MoreGroupBox filterGroup;
		private System.Windows.Forms.Label tagLabel;
		private UI.MoreButton tagButton;
	}
}