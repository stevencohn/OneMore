
namespace River.OneMoreAddIn.Settings
{
	partial class MarkdownSheet
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
			this.layoutPanel = new System.Windows.Forms.Panel();
			this.gfmLineBreaksBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.singleSpacingBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.blankBeforeHeadingsBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.layoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// introBox
			// 
			this.introBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.introBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.introBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.introBox.Location = new System.Drawing.Point(13, 8);
			this.introBox.Name = "introBox";
			this.introBox.Padding = new System.Windows.Forms.Padding(0, 0, 0, 36);
			this.introBox.Size = new System.Drawing.Size(772, 66);
			this.introBox.TabIndex = 3;
			this.introBox.Text = "Customize how Convert Markdown interprets line breaks and paragraph spacing";
			this.introBox.ThemedBack = "ControlLightLight";
			this.introBox.ThemedFore = "ControlText";
			// 
			// layoutPanel
			// 
			this.layoutPanel.Controls.Add(this.gfmLineBreaksBox);
			this.layoutPanel.Controls.Add(this.singleSpacingBox);
			this.layoutPanel.Controls.Add(this.blankBeforeHeadingsBox);
			this.layoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.layoutPanel.Location = new System.Drawing.Point(13, 74);
			this.layoutPanel.Margin = new System.Windows.Forms.Padding(0);
			this.layoutPanel.Name = "layoutPanel";
			this.layoutPanel.Size = new System.Drawing.Size(772, 416);
			this.layoutPanel.TabIndex = 4;
			// 
			// gfmLineBreaksBox
			// 
			this.gfmLineBreaksBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.gfmLineBreaksBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.gfmLineBreaksBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.gfmLineBreaksBox.Location = new System.Drawing.Point(18, 20);
			this.gfmLineBreaksBox.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
			this.gfmLineBreaksBox.Name = "gfmLineBreaksBox";
			this.gfmLineBreaksBox.Size = new System.Drawing.Size(700, 25);
			this.gfmLineBreaksBox.StylizeImage = false;
			this.gfmLineBreaksBox.TabIndex = 0;
			this.gfmLineBreaksBox.Text = "Treat single line breaks as GitHub-Flavored Markdown line breaks";
			this.gfmLineBreaksBox.ThemedBack = null;
			this.gfmLineBreaksBox.ThemedFore = null;
			this.gfmLineBreaksBox.UseVisualStyleBackColor = true;
			// 
			// singleSpacingBox
			// 
			this.singleSpacingBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.singleSpacingBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.singleSpacingBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.singleSpacingBox.Location = new System.Drawing.Point(18, 76);
			this.singleSpacingBox.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
			this.singleSpacingBox.Name = "singleSpacingBox";
			this.singleSpacingBox.Size = new System.Drawing.Size(700, 25);
			this.singleSpacingBox.StylizeImage = false;
			this.singleSpacingBox.TabIndex = 1;
			this.singleSpacingBox.Text = "Use single paragraph spacing instead of adding extra space after each paragraph";
			this.singleSpacingBox.ThemedBack = null;
			this.singleSpacingBox.ThemedFore = null;
			this.singleSpacingBox.UseVisualStyleBackColor = true;
			// 
			// blankBeforeHeadingsBox
			// 
			this.blankBeforeHeadingsBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.blankBeforeHeadingsBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.blankBeforeHeadingsBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.blankBeforeHeadingsBox.Location = new System.Drawing.Point(18, 111);
			this.blankBeforeHeadingsBox.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
			this.blankBeforeHeadingsBox.Name = "blankBeforeHeadingsBox";
			this.blankBeforeHeadingsBox.Size = new System.Drawing.Size(700, 25);
			this.blankBeforeHeadingsBox.StylizeImage = false;
			this.blankBeforeHeadingsBox.TabIndex = 2;
			this.blankBeforeHeadingsBox.Text = "Insert a blank line before headings";
			this.blankBeforeHeadingsBox.ThemedBack = null;
			this.blankBeforeHeadingsBox.ThemedFore = null;
			this.blankBeforeHeadingsBox.UseVisualStyleBackColor = true;
			// 
			// MarkdownSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.layoutPanel);
			this.Controls.Add(this.introBox);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Name = "MarkdownSheet";
			this.Padding = new System.Windows.Forms.Padding(13, 8, 15, 10);
			this.Size = new System.Drawing.Size(800, 500);
			this.layoutPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreMultilineLabel introBox;
		private System.Windows.Forms.Panel layoutPanel;
		private UI.MoreCheckBox gfmLineBreaksBox;
		private UI.MoreCheckBox singleSpacingBox;
		private UI.MoreCheckBox blankBeforeHeadingsBox;
	}
}
