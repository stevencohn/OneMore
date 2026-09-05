
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
			this.duplicateGroup.SuspendLayout();
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
			// PageSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.duplicateGroup);
			this.Controls.Add(this.introBox);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Name = "PageSheet";
			this.Padding = new System.Windows.Forms.Padding(13, 8, 15, 10);
			this.Size = new System.Drawing.Size(800, 300);
			this.duplicateGroup.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreMultilineLabel introBox;
		private UI.MoreGroupBox duplicateGroup;
		private UI.MoreCheckBox insertNoteBox;
		private UI.MoreCheckBox insertBacklinksBox;
		private UI.MoreCheckBox refreshTocBox;
	}
}
