
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
			this.splitByGroup = new System.Windows.Forms.GroupBox();
			this.byLinksLabel = new System.Windows.Forms.Label();
			this.byHeading1Label = new System.Windows.Forms.Label();
			this.byLinksBox = new System.Windows.Forms.RadioButton();
			this.byHeading1Box = new System.Windows.Forms.RadioButton();
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.taggedBox = new System.Windows.Forms.CheckBox();
			this.filterGroup = new System.Windows.Forms.GroupBox();
			this.tagLabel = new System.Windows.Forms.Label();
			this.tagButton = new System.Windows.Forms.Button();
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
			this.splitByGroup.Location = new System.Drawing.Point(13, 13);
			this.splitByGroup.Margin = new System.Windows.Forms.Padding(3, 3, 3, 5);
			this.splitByGroup.Name = "splitByGroup";
			this.splitByGroup.Padding = new System.Windows.Forms.Padding(10);
			this.splitByGroup.Size = new System.Drawing.Size(348, 116);
			this.splitByGroup.TabIndex = 0;
			this.splitByGroup.TabStop = false;
			this.splitByGroup.Text = "Split by";
			// 
			// byLinksLabel
			// 
			this.byLinksLabel.AutoSize = true;
			this.byLinksLabel.Location = new System.Drawing.Point(32, 92);
			this.byLinksLabel.Name = "byLinksLabel";
			this.byLinksLabel.Size = new System.Drawing.Size(170, 13);
			this.byLinksLabel.TabIndex = 3;
			this.byLinksLabel.Text = "Move content to each linked page";
			// 
			// byHeading1Label
			// 
			this.byHeading1Label.AutoSize = true;
			this.byHeading1Label.Location = new System.Drawing.Point(32, 46);
			this.byHeading1Label.Margin = new System.Windows.Forms.Padding(3, 0, 3, 10);
			this.byHeading1Label.Name = "byHeading1Label";
			this.byHeading1Label.Size = new System.Drawing.Size(295, 13);
			this.byHeading1Label.TabIndex = 2;
			this.byHeading1Label.Text = "Link headings to new pages and move content to each page";
			// 
			// byLinksBox
			// 
			this.byLinksBox.AutoSize = true;
			this.byLinksBox.Location = new System.Drawing.Point(13, 72);
			this.byLinksBox.Name = "byLinksBox";
			this.byLinksBox.Size = new System.Drawing.Size(101, 17);
			this.byLinksBox.TabIndex = 1;
			this.byLinksBox.Text = "Page (wiki) links";
			this.byLinksBox.UseVisualStyleBackColor = true;
			// 
			// byHeading1Box
			// 
			this.byHeading1Box.AutoSize = true;
			this.byHeading1Box.Checked = true;
			this.byHeading1Box.Location = new System.Drawing.Point(13, 26);
			this.byHeading1Box.Name = "byHeading1Box";
			this.byHeading1Box.Size = new System.Drawing.Size(74, 17);
			this.byHeading1Box.TabIndex = 0;
			this.byHeading1Box.TabStop = true;
			this.byHeading1Box.Text = "Heading 1";
			this.byHeading1Box.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(286, 242);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 23);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(205, 242);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 2;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// taggedBox
			// 
			this.taggedBox.AutoSize = true;
			this.taggedBox.Location = new System.Drawing.Point(13, 26);
			this.taggedBox.Name = "taggedBox";
			this.taggedBox.Size = new System.Drawing.Size(211, 17);
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
			this.filterGroup.Location = new System.Drawing.Point(13, 137);
			this.filterGroup.Name = "filterGroup";
			this.filterGroup.Padding = new System.Windows.Forms.Padding(10);
			this.filterGroup.Size = new System.Drawing.Size(348, 91);
			this.filterGroup.TabIndex = 5;
			this.filterGroup.TabStop = false;
			this.filterGroup.Text = "Filter";
			// 
			// tagLabel
			// 
			this.tagLabel.AutoSize = true;
			this.tagLabel.Location = new System.Drawing.Point(32, 55);
			this.tagLabel.Name = "tagLabel";
			this.tagLabel.Size = new System.Drawing.Size(29, 13);
			this.tagLabel.TabIndex = 20;
			this.tagLabel.Text = "Tag:";
			// 
			// tagButton
			// 
			this.tagButton.BackColor = System.Drawing.SystemColors.Window;
			this.tagButton.Enabled = false;
			this.tagButton.Location = new System.Drawing.Point(67, 49);
			this.tagButton.Name = "tagButton";
			this.tagButton.Size = new System.Drawing.Size(40, 25);
			this.tagButton.TabIndex = 19;
			this.tagButton.Text = "?";
			this.tagButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.tagButton.UseVisualStyleBackColor = false;
			this.tagButton.Click += new System.EventHandler(this.SetTag);
			// 
			// SplitDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(374, 278);
			this.Controls.Add(this.filterGroup);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.splitByGroup);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SplitDialog";
			this.Padding = new System.Windows.Forms.Padding(10);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Split Page";
			this.TopMost = true;
			this.splitByGroup.ResumeLayout(false);
			this.splitByGroup.PerformLayout();
			this.filterGroup.ResumeLayout(false);
			this.filterGroup.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox splitByGroup;
		private System.Windows.Forms.Label byLinksLabel;
		private System.Windows.Forms.Label byHeading1Label;
		private System.Windows.Forms.RadioButton byLinksBox;
		private System.Windows.Forms.RadioButton byHeading1Box;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.CheckBox taggedBox;
		private System.Windows.Forms.GroupBox filterGroup;
		private System.Windows.Forms.Label tagLabel;
		private System.Windows.Forms.Button tagButton;
	}
}