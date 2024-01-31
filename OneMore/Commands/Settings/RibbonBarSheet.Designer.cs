namespace River.OneMoreAddIn.Settings
{
	partial class RibbonBarSheet
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RibbonBarSheet));
			this.introBox = new River.OneMoreAddIn.UI.MoreMultilineLabel();
			this.editRibbonBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.editIconBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.formulaRibbonBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.formulaIconBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.quickGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.formulaPicture = new River.OneMoreAddIn.UI.MorePictureBox();
			this.editPicture = new River.OneMoreAddIn.UI.MorePictureBox();
			this.positionIntroLabel = new River.OneMoreAddIn.UI.MoreMultilineLabel();
			this.positionGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.positionBox = new System.Windows.Forms.ComboBox();
			this.positionLabel = new System.Windows.Forms.Label();
			this.quickGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.formulaPicture)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.editPicture)).BeginInit();
			this.positionGroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// introBox
			// 
			this.introBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.introBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.introBox.Location = new System.Drawing.Point(13, 182);
			this.introBox.Name = "introBox";
			this.introBox.PreferredBack = "ControlLightLight";
			this.introBox.PreferredFore = null;
			this.introBox.Size = new System.Drawing.Size(774, 66);
			this.introBox.TabIndex = 3;
			this.introBox.Text = "Choose which commands to include in the ribbon bar and whether to display icons o" +
    "r icons and lables. Restart OneNote to see changes.";
			// 
			// editRibbonBox
			// 
			this.editRibbonBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.editRibbonBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.editRibbonBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.editRibbonBox.Location = new System.Drawing.Point(254, 38);
			this.editRibbonBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.editRibbonBox.Name = "editRibbonBox";
			this.editRibbonBox.Size = new System.Drawing.Size(153, 25);
			this.editRibbonBox.TabIndex = 5;
			this.editRibbonBox.Text = "Edit Commands";
			this.editRibbonBox.UseVisualStyleBackColor = true;
			this.editRibbonBox.CheckedChanged += new System.EventHandler(this.CheckedChanged);
			// 
			// editIconBox
			// 
			this.editIconBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.editIconBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.editIconBox.Enabled = false;
			this.editIconBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.editIconBox.Location = new System.Drawing.Point(254, 71);
			this.editIconBox.Name = "editIconBox";
			this.editIconBox.Size = new System.Drawing.Size(294, 25);
			this.editIconBox.TabIndex = 6;
			this.editIconBox.Text = "Show only icons for edit commands";
			this.editIconBox.UseVisualStyleBackColor = true;
			// 
			// formulaRibbonBox
			// 
			this.formulaRibbonBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.formulaRibbonBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.formulaRibbonBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.formulaRibbonBox.Location = new System.Drawing.Point(254, 149);
			this.formulaRibbonBox.Name = "formulaRibbonBox";
			this.formulaRibbonBox.Size = new System.Drawing.Size(185, 25);
			this.formulaRibbonBox.TabIndex = 7;
			this.formulaRibbonBox.Text = "Formula Commands";
			this.formulaRibbonBox.UseVisualStyleBackColor = true;
			this.formulaRibbonBox.CheckedChanged += new System.EventHandler(this.CheckedChanged);
			// 
			// formulaIconBox
			// 
			this.formulaIconBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.formulaIconBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.formulaIconBox.Enabled = false;
			this.formulaIconBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.formulaIconBox.Location = new System.Drawing.Point(254, 180);
			this.formulaIconBox.Name = "formulaIconBox";
			this.formulaIconBox.Size = new System.Drawing.Size(322, 25);
			this.formulaIconBox.TabIndex = 8;
			this.formulaIconBox.Text = "Show only icons for formula commands";
			this.formulaIconBox.UseVisualStyleBackColor = true;
			// 
			// quickGroup
			// 
			this.quickGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.quickGroup.Controls.Add(this.formulaIconBox);
			this.quickGroup.Controls.Add(this.formulaRibbonBox);
			this.quickGroup.Controls.Add(this.formulaPicture);
			this.quickGroup.Controls.Add(this.editPicture);
			this.quickGroup.Controls.Add(this.editRibbonBox);
			this.quickGroup.Controls.Add(this.editIconBox);
			this.quickGroup.Location = new System.Drawing.Point(13, 254);
			this.quickGroup.Name = "quickGroup";
			this.quickGroup.Padding = new System.Windows.Forms.Padding(20, 15, 3, 3);
			this.quickGroup.Size = new System.Drawing.Size(774, 265);
			this.quickGroup.TabIndex = 9;
			this.quickGroup.TabStop = false;
			this.quickGroup.Text = "Quick Commands";
			// 
			// formulaPicture
			// 
			this.formulaPicture.BackColor = System.Drawing.Color.White;
			this.formulaPicture.Cursor = System.Windows.Forms.Cursors.Hand;
			this.formulaPicture.Image = ((System.Drawing.Image)(resources.GetObject("formulaPicture.Image")));
			this.formulaPicture.Location = new System.Drawing.Point(24, 149);
			this.formulaPicture.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.formulaPicture.Name = "formulaPicture";
			this.formulaPicture.Size = new System.Drawing.Size(212, 95);
			this.formulaPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.formulaPicture.TabIndex = 12;
			this.formulaPicture.TabStop = false;
			this.formulaPicture.Click += new System.EventHandler(this.ClickPicture);
			// 
			// editPicture
			// 
			this.editPicture.BackColor = System.Drawing.Color.White;
			this.editPicture.Cursor = System.Windows.Forms.Cursors.Hand;
			this.editPicture.Image = ((System.Drawing.Image)(resources.GetObject("editPicture.Image")));
			this.editPicture.Location = new System.Drawing.Point(24, 39);
			this.editPicture.Margin = new System.Windows.Forms.Padding(4, 5, 4, 10);
			this.editPicture.Name = "editPicture";
			this.editPicture.Size = new System.Drawing.Size(212, 95);
			this.editPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.editPicture.TabIndex = 11;
			this.editPicture.TabStop = false;
			this.editPicture.Click += new System.EventHandler(this.ClickPicture);
			// 
			// positionIntroLabel
			// 
			this.positionIntroLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.positionIntroLabel.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.positionIntroLabel.Location = new System.Drawing.Point(13, 12);
			this.positionIntroLabel.Name = "positionIntroLabel";
			this.positionIntroLabel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 36);
			this.positionIntroLabel.PreferredBack = "ControlLightLight";
			this.positionIntroLabel.PreferredFore = null;
			this.positionIntroLabel.Size = new System.Drawing.Size(774, 53);
			this.positionIntroLabel.TabIndex = 4;
			this.positionIntroLabel.Text = "Select the location of the OneMore ribbon group.";
			// 
			// positionGroup
			// 
			this.positionGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.positionGroup.Controls.Add(this.positionBox);
			this.positionGroup.Controls.Add(this.positionLabel);
			this.positionGroup.Location = new System.Drawing.Point(13, 71);
			this.positionGroup.Margin = new System.Windows.Forms.Padding(3, 3, 3, 15);
			this.positionGroup.Name = "positionGroup";
			this.positionGroup.Padding = new System.Windows.Forms.Padding(20, 20, 3, 3);
			this.positionGroup.Size = new System.Drawing.Size(774, 93);
			this.positionGroup.TabIndex = 0;
			this.positionGroup.TabStop = false;
			this.positionGroup.Text = "Position";
			// 
			// positionBox
			// 
			this.positionBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.positionBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.positionBox.FormattingEnabled = true;
			this.positionBox.Items.AddRange(new object[] {
            "Clipboard Group",
            "Basic Text Group",
            "Styles Group",
            "Tags Group",
            "Email Group",
            "Meetings Group",
            "After last group"});
			this.positionBox.Location = new System.Drawing.Point(254, 36);
			this.positionBox.Name = "positionBox";
			this.positionBox.Size = new System.Drawing.Size(378, 28);
			this.positionBox.TabIndex = 1;
			// 
			// positionLabel
			// 
			this.positionLabel.AutoSize = true;
			this.positionLabel.Location = new System.Drawing.Point(23, 39);
			this.positionLabel.Name = "positionLabel";
			this.positionLabel.Size = new System.Drawing.Size(163, 20);
			this.positionLabel.TabIndex = 0;
			this.positionLabel.Text = "After OneMore Group";
			// 
			// RibbonBarSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.positionIntroLabel);
			this.Controls.Add(this.positionGroup);
			this.Controls.Add(this.quickGroup);
			this.Controls.Add(this.introBox);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Name = "RibbonBarSheet";
			this.Padding = new System.Windows.Forms.Padding(10, 9, 10, 9);
			this.Size = new System.Drawing.Size(800, 537);
			this.quickGroup.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.formulaPicture)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.editPicture)).EndInit();
			this.positionGroup.ResumeLayout(false);
			this.positionGroup.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreCheckBox editRibbonBox;
		private UI.MoreCheckBox editIconBox;
		private UI.MoreCheckBox formulaRibbonBox;
		private UI.MoreCheckBox formulaIconBox;
		private UI.MoreGroupBox quickGroup;
		private UI.MorePictureBox editPicture;
		private UI.MorePictureBox formulaPicture;
		private UI.MoreMultilineLabel introBox;
		private UI.MoreGroupBox positionGroup;
		private UI.MoreMultilineLabel positionIntroLabel;
		private System.Windows.Forms.Label positionLabel;
		private System.Windows.Forms.ComboBox positionBox;
	}
}
