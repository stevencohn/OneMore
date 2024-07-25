﻿namespace River.OneMoreAddIn.Commands
{
	partial class InsertTocDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InsertTocDialog));
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.pageRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.sectionRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.topBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.notebookRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.pagesBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.rightAlignBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.sectionPagePreviewBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.notebookPagePreviewBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.locationBox = new System.Windows.Forms.ComboBox();
			this.locationLabel = new System.Windows.Forms.Label();
			this.styleLabel = new System.Windows.Forms.Label();
			this.styleBox = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(241, 435);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(120, 38);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.CollectParametersOnOK);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(367, 435);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(120, 38);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// pageRadio
			// 
			this.pageRadio.Checked = true;
			this.pageRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pageRadio.Location = new System.Drawing.Point(28, 28);
			this.pageRadio.Name = "pageRadio";
			this.pageRadio.Size = new System.Drawing.Size(300, 25);
			this.pageRadio.TabIndex = 2;
			this.pageRadio.TabStop = true;
			this.pageRadio.Text = "Insert table of headings on this page";
			this.pageRadio.UseVisualStyleBackColor = true;
			this.pageRadio.CheckedChanged += new System.EventHandler(this.ChangeScopeRadioSelection);
			// 
			// sectionRadio
			// 
			this.sectionRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.sectionRadio.Location = new System.Drawing.Point(28, 221);
			this.sectionRadio.Margin = new System.Windows.Forms.Padding(3, 25, 3, 3);
			this.sectionRadio.Name = "sectionRadio";
			this.sectionRadio.Size = new System.Drawing.Size(359, 25);
			this.sectionRadio.TabIndex = 7;
			this.sectionRadio.Text = "New page with index of pages in this section";
			this.sectionRadio.UseVisualStyleBackColor = true;
			this.sectionRadio.CheckedChanged += new System.EventHandler(this.ChangeScopeRadioSelection);
			// 
			// topBox
			// 
			this.topBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.topBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.topBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.topBox.Location = new System.Drawing.Point(56, 61);
			this.topBox.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.topBox.Name = "topBox";
			this.topBox.Size = new System.Drawing.Size(378, 25);
			this.topBox.StylizeImage = false;
			this.topBox.TabIndex = 3;
			this.topBox.Text = "Add link to each heading to jump to top of page";
			this.topBox.ThemedBack = null;
			this.topBox.ThemedFore = null;
			this.topBox.UseVisualStyleBackColor = true;
			this.topBox.CheckedChanged += new System.EventHandler(this.ToggleTopBox);
			// 
			// notebookRadio
			// 
			this.notebookRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.notebookRadio.Location = new System.Drawing.Point(28, 313);
			this.notebookRadio.Margin = new System.Windows.Forms.Padding(3, 25, 3, 3);
			this.notebookRadio.Name = "notebookRadio";
			this.notebookRadio.Size = new System.Drawing.Size(390, 25);
			this.notebookRadio.TabIndex = 9;
			this.notebookRadio.Text = "New page with index of sections in this notebook";
			this.notebookRadio.UseVisualStyleBackColor = true;
			this.notebookRadio.CheckedChanged += new System.EventHandler(this.ChangeScopeRadioSelection);
			// 
			// pagesBox
			// 
			this.pagesBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.pagesBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pagesBox.Enabled = false;
			this.pagesBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.pagesBox.Location = new System.Drawing.Point(56, 346);
			this.pagesBox.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.pagesBox.Name = "pagesBox";
			this.pagesBox.Size = new System.Drawing.Size(252, 25);
			this.pagesBox.StylizeImage = false;
			this.pagesBox.TabIndex = 10;
			this.pagesBox.Text = "Include pages in each section";
			this.pagesBox.ThemedBack = null;
			this.pagesBox.ThemedFore = null;
			this.pagesBox.UseVisualStyleBackColor = true;
			this.pagesBox.CheckedChanged += new System.EventHandler(this.TogglePagesBox);
			// 
			// rightAlignBox
			// 
			this.rightAlignBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.rightAlignBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.rightAlignBox.Enabled = false;
			this.rightAlignBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.rightAlignBox.Location = new System.Drawing.Point(56, 91);
			this.rightAlignBox.Name = "rightAlignBox";
			this.rightAlignBox.Size = new System.Drawing.Size(230, 25);
			this.rightAlignBox.StylizeImage = false;
			this.rightAlignBox.TabIndex = 4;
			this.rightAlignBox.Text = "Right-align top of page link";
			this.rightAlignBox.ThemedBack = null;
			this.rightAlignBox.ThemedFore = null;
			this.rightAlignBox.UseVisualStyleBackColor = true;
			// 
			// sectionPagePreviewBox
			// 
			this.sectionPagePreviewBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.sectionPagePreviewBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.sectionPagePreviewBox.Enabled = false;
			this.sectionPagePreviewBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.sectionPagePreviewBox.Location = new System.Drawing.Point(56, 254);
			this.sectionPagePreviewBox.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.sectionPagePreviewBox.Name = "sectionPagePreviewBox";
			this.sectionPagePreviewBox.Size = new System.Drawing.Size(280, 25);
			this.sectionPagePreviewBox.StylizeImage = false;
			this.sectionPagePreviewBox.TabIndex = 8;
			this.sectionPagePreviewBox.Text = "Include text preview of each page";
			this.sectionPagePreviewBox.ThemedBack = null;
			this.sectionPagePreviewBox.ThemedFore = null;
			this.sectionPagePreviewBox.UseVisualStyleBackColor = true;
			// 
			// notebookPagePreviewBox
			// 
			this.notebookPagePreviewBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.notebookPagePreviewBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.notebookPagePreviewBox.Enabled = false;
			this.notebookPagePreviewBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.notebookPagePreviewBox.Location = new System.Drawing.Point(56, 378);
			this.notebookPagePreviewBox.Name = "notebookPagePreviewBox";
			this.notebookPagePreviewBox.Size = new System.Drawing.Size(280, 25);
			this.notebookPagePreviewBox.StylizeImage = false;
			this.notebookPagePreviewBox.TabIndex = 11;
			this.notebookPagePreviewBox.Text = "Include text preview of each page";
			this.notebookPagePreviewBox.ThemedBack = null;
			this.notebookPagePreviewBox.ThemedFore = null;
			this.notebookPagePreviewBox.UseVisualStyleBackColor = true;
			// 
			// locationBox
			// 
			this.locationBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.locationBox.FormattingEnabled = true;
			this.locationBox.Items.AddRange(new object[] {
            "At top of page",
            "At current cursor"});
			this.locationBox.Location = new System.Drawing.Point(227, 124);
			this.locationBox.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.locationBox.Name = "locationBox";
			this.locationBox.Size = new System.Drawing.Size(217, 28);
			this.locationBox.TabIndex = 5;
			// 
			// locationLabel
			// 
			this.locationLabel.AutoSize = true;
			this.locationLabel.Location = new System.Drawing.Point(52, 127);
			this.locationLabel.Name = "locationLabel";
			this.locationLabel.Size = new System.Drawing.Size(115, 20);
			this.locationLabel.TabIndex = 11;
			this.locationLabel.Text = "Insert Location";
			// 
			// styleLabel
			// 
			this.styleLabel.AutoSize = true;
			this.styleLabel.Location = new System.Drawing.Point(52, 164);
			this.styleLabel.Name = "styleLabel";
			this.styleLabel.Size = new System.Drawing.Size(77, 20);
			this.styleLabel.TabIndex = 12;
			this.styleLabel.Text = "Title Style";
			// 
			// styleBox
			// 
			this.styleBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.styleBox.FormattingEnabled = true;
			this.styleBox.Items.AddRange(new object[] {
            "Standard Page Title",
            "Standard Heading 1",
            "Standard Heading 2",
            "Standard Heading 3",
            "Custom Page Title",
            "Custom Heading 1",
            "Custom Heading 2",
            "Custom Heading 3"});
			this.styleBox.Location = new System.Drawing.Point(227, 161);
			this.styleBox.Name = "styleBox";
			this.styleBox.Size = new System.Drawing.Size(217, 28);
			this.styleBox.TabIndex = 6;
			// 
			// InsertTocDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(499, 485);
			this.Controls.Add(this.styleBox);
			this.Controls.Add(this.styleLabel);
			this.Controls.Add(this.locationLabel);
			this.Controls.Add(this.locationBox);
			this.Controls.Add(this.notebookPagePreviewBox);
			this.Controls.Add(this.sectionPagePreviewBox);
			this.Controls.Add(this.rightAlignBox);
			this.Controls.Add(this.pagesBox);
			this.Controls.Add(this.notebookRadio);
			this.Controls.Add(this.topBox);
			this.Controls.Add(this.sectionRadio);
			this.Controls.Add(this.pageRadio);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "InsertTocDialog";
			this.Padding = new System.Windows.Forms.Padding(25, 25, 0, 0);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Table of Contents";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private UI.MoreRadioButton pageRadio;
		private UI.MoreRadioButton sectionRadio;
		private UI.MoreCheckBox topBox;
		private UI.MoreRadioButton notebookRadio;
		private UI.MoreCheckBox pagesBox;
		private UI.MoreCheckBox rightAlignBox;
		private UI.MoreCheckBox sectionPagePreviewBox;
		private UI.MoreCheckBox notebookPagePreviewBox;
		private System.Windows.Forms.ComboBox locationBox;
		private System.Windows.Forms.Label locationLabel;
		private System.Windows.Forms.Label styleLabel;
		private System.Windows.Forms.ComboBox styleBox;
	}
}