
namespace River.OneMoreAddIn.Settings
{
	partial class PageSheet
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

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.introBox = new River.OneMoreAddIn.UI.MoreMultilineLabel();
			this.duplicateGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.insertNoteBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.insertBacklinksBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.refreshTocBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.tocGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.tocWarnBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.tocSizeLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.tocSizeBox = new River.OneMoreAddIn.UI.MoreNumericUpDown();
			this.duplicateGroup.SuspendLayout();
			this.tocGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.tocSizeBox)).BeginInit();
			this.SuspendLayout();
			// 
			// introBox
			// 
			this.introBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.introBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.introBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.introBox.Location = new System.Drawing.Point(13, 8);
			this.introBox.Name = "introBox";
			this.introBox.Padding = new System.Windows.Forms.Padding(0, 0, 0, 36);
			this.introBox.Size = new System.Drawing.Size(772, 66);
			this.introBox.TabIndex = 0;
			this.introBox.Text = "Customize options for Page commands";
			this.introBox.ThemedBack = "ControlLightLight";
			this.introBox.ThemedFore = "ControlText";
			// 
			// duplicateGroup
			// 
			this.duplicateGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.duplicateGroup.BorderThickness = 3;
			this.duplicateGroup.Controls.Add(this.insertNoteBox);
			this.duplicateGroup.Controls.Add(this.insertBacklinksBox);
			this.duplicateGroup.Controls.Add(this.refreshTocBox);
			this.duplicateGroup.Location = new System.Drawing.Point(13, 84);
			this.duplicateGroup.Name = "duplicateGroup";
			this.duplicateGroup.Padding = new System.Windows.Forms.Padding(15, 3, 3, 3);
			this.duplicateGroup.ShowOnlyTopEdge = true;
			this.duplicateGroup.Size = new System.Drawing.Size(772, 150);
			this.duplicateGroup.TabIndex = 1;
			this.duplicateGroup.TabStop = false;
			this.duplicateGroup.Text = "Duplicate Page";
			this.duplicateGroup.ThemedBorder = null;
			this.duplicateGroup.ThemedFore = null;
			// 
			// insertNoteBox
			// 
			this.insertNoteBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.insertNoteBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.insertNoteBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.insertNoteBox.Location = new System.Drawing.Point(18, 39);
			this.insertNoteBox.Name = "insertNoteBox";
			this.insertNoteBox.Size = new System.Drawing.Size(540, 25);
			this.insertNoteBox.StylizeImage = false;
			this.insertNoteBox.TabIndex = 0;
			this.insertNoteBox.Text = "Insert duplication note";
			this.insertNoteBox.ThemedBack = null;
			this.insertNoteBox.ThemedFore = null;
			this.insertNoteBox.UseVisualStyleBackColor = true;
			// 
			// insertBacklinksBox
			// 
			this.insertBacklinksBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.insertBacklinksBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.insertBacklinksBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.insertBacklinksBox.Location = new System.Drawing.Point(18, 70);
			this.insertBacklinksBox.Name = "insertBacklinksBox";
			this.insertBacklinksBox.Size = new System.Drawing.Size(540, 25);
			this.insertBacklinksBox.StylizeImage = false;
			this.insertBacklinksBox.TabIndex = 1;
			this.insertBacklinksBox.Text = "Insert heading back-links";
			this.insertBacklinksBox.ThemedBack = null;
			this.insertBacklinksBox.ThemedFore = null;
			this.insertBacklinksBox.UseVisualStyleBackColor = true;
			// 
			// refreshTocBox
			// 
			this.refreshTocBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.refreshTocBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.refreshTocBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.refreshTocBox.Location = new System.Drawing.Point(18, 101);
			this.refreshTocBox.Name = "refreshTocBox";
			this.refreshTocBox.Size = new System.Drawing.Size(540, 25);
			this.refreshTocBox.StylizeImage = false;
			this.refreshTocBox.TabIndex = 2;
			this.refreshTocBox.Text = "Refresh Table of Contents";
			this.refreshTocBox.ThemedBack = null;
			this.refreshTocBox.ThemedFore = null;
			this.refreshTocBox.UseVisualStyleBackColor = true;
			//
			// tocGroup
			//
			this.tocGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tocGroup.BorderThickness = 3;
			this.tocGroup.Controls.Add(this.tocWarnBox);
			this.tocGroup.Controls.Add(this.tocSizeLabel);
			this.tocGroup.Controls.Add(this.tocSizeBox);
			this.tocGroup.Location = new System.Drawing.Point(13, 259);
			this.tocGroup.Name = "tocGroup";
			this.tocGroup.Padding = new System.Windows.Forms.Padding(15, 3, 3, 3);
			this.tocGroup.ShowOnlyTopEdge = true;
			this.tocGroup.Size = new System.Drawing.Size(772, 120);
			this.tocGroup.TabIndex = 2;
			this.tocGroup.TabStop = false;
			this.tocGroup.Text = "Table of Contents";
			this.tocGroup.ThemedBorder = null;
			this.tocGroup.ThemedFore = null;
			//
			// tocWarnBox
			//
			this.tocWarnBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.tocWarnBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.tocWarnBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.tocWarnBox.Location = new System.Drawing.Point(18, 39);
			this.tocWarnBox.Name = "tocWarnBox";
			this.tocWarnBox.Size = new System.Drawing.Size(720, 25);
			this.tocWarnBox.StylizeImage = false;
			this.tocWarnBox.TabIndex = 0;
			this.tocWarnBox.Text = "On Refresh, show warning when table of contents is up to date";
			this.tocWarnBox.ThemedBack = null;
			this.tocWarnBox.ThemedFore = null;
			this.tocWarnBox.UseVisualStyleBackColor = true;
			//
			// tocSizeLabel
			//
			this.tocSizeLabel.AutoSize = true;
			this.tocSizeLabel.Location = new System.Drawing.Point(40, 77);
			this.tocSizeLabel.Name = "tocSizeLabel";
			this.tocSizeLabel.Size = new System.Drawing.Size(180, 20);
			this.tocSizeLabel.TabIndex = 1;
			this.tocSizeLabel.Text = "Minimum page size (KB):";
			this.tocSizeLabel.Scrubber = this.tocSizeBox;
			this.tocSizeLabel.ThemedBack = null;
			this.tocSizeLabel.ThemedFore = null;
			//
			// tocSizeBox
			//
			this.tocSizeBox.Location = new System.Drawing.Point(230, 74);
			this.tocSizeBox.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
			this.tocSizeBox.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.tocSizeBox.Name = "tocSizeBox";
			this.tocSizeBox.Size = new System.Drawing.Size(100, 26);
			this.tocSizeBox.TabIndex = 2;
			this.tocSizeBox.Value = new decimal(new int[] {
            250,
            0,
            0,
            0});
			//
			// PageSheet
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.tocGroup);
			this.Controls.Add(this.duplicateGroup);
			this.Controls.Add(this.introBox);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Name = "PageSheet";
			this.Padding = new System.Windows.Forms.Padding(13, 8, 15, 10);
			this.Size = new System.Drawing.Size(800, 400);
			this.duplicateGroup.ResumeLayout(false);
			this.tocGroup.ResumeLayout(false);
			this.tocGroup.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.tocSizeBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreMultilineLabel introBox;
		private UI.MoreGroupBox duplicateGroup;
		private UI.MoreCheckBox insertNoteBox;
		private UI.MoreCheckBox insertBacklinksBox;
		private UI.MoreCheckBox refreshTocBox;
		private UI.MoreGroupBox tocGroup;
		private UI.MoreCheckBox tocWarnBox;
		private UI.MoreLabel tocSizeLabel;
		private UI.MoreNumericUpDown tocSizeBox;
	}
}
