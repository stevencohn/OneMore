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
			this.introBox = new System.Windows.Forms.TextBox();
			this.editRibbonBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.editIconBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.formulaRibbonBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.formulaIconBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.editGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.editPicture = new System.Windows.Forms.PictureBox();
			this.formulaGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.formulaPicture = new System.Windows.Forms.PictureBox();
			this.editGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.editPicture)).BeginInit();
			this.formulaGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.formulaPicture)).BeginInit();
			this.SuspendLayout();
			// 
			// introBox
			// 
			this.introBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.introBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.introBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.introBox.Location = new System.Drawing.Point(10, 9);
			this.introBox.Multiline = true;
			this.introBox.Name = "introBox";
			this.introBox.ReadOnly = true;
			this.introBox.Size = new System.Drawing.Size(780, 66);
			this.introBox.TabIndex = 3;
			this.introBox.Text = "Choose which commands to include in the ribbon bar and whether to display icons o" +
    "r icons and lables. Restart OneNote to see changes.";
			// 
			// editRibbonBox
			// 
			this.editRibbonBox.Location = new System.Drawing.Point(22, 34);
			this.editRibbonBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.editRibbonBox.Name = "editRibbonBox";
			this.editRibbonBox.Size = new System.Drawing.Size(44, 24);
			this.editRibbonBox.TabIndex = 5;
			this.editRibbonBox.UseVisualStyleBackColor = true;
			this.editRibbonBox.CheckedChanged += new System.EventHandler(this.CheckedChanged);
			// 
			// editIconBox
			// 
			this.editIconBox.Enabled = false;
			this.editIconBox.Location = new System.Drawing.Point(344, 34);
			this.editIconBox.Name = "editIconBox";
			this.editIconBox.Size = new System.Drawing.Size(293, 24);
			this.editIconBox.TabIndex = 6;
			this.editIconBox.Text = "Show only icons for edit commands";
			this.editIconBox.UseVisualStyleBackColor = true;
			// 
			// formulaRibbonBox
			// 
			this.formulaRibbonBox.Location = new System.Drawing.Point(22, 34);
			this.formulaRibbonBox.Name = "formulaRibbonBox";
			this.formulaRibbonBox.Size = new System.Drawing.Size(44, 24);
			this.formulaRibbonBox.TabIndex = 7;
			this.formulaRibbonBox.UseVisualStyleBackColor = true;
			this.formulaRibbonBox.CheckedChanged += new System.EventHandler(this.CheckedChanged);
			// 
			// formulaIconBox
			// 
			this.formulaIconBox.Enabled = false;
			this.formulaIconBox.Location = new System.Drawing.Point(344, 34);
			this.formulaIconBox.Name = "formulaIconBox";
			this.formulaIconBox.Size = new System.Drawing.Size(321, 24);
			this.formulaIconBox.TabIndex = 8;
			this.formulaIconBox.Text = "Show only icons for formula commands";
			this.formulaIconBox.UseVisualStyleBackColor = true;
			// 
			// editGroup
			// 
			this.editGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.editGroup.Controls.Add(this.editPicture);
			this.editGroup.Controls.Add(this.editRibbonBox);
			this.editGroup.Controls.Add(this.editIconBox);
			this.editGroup.Location = new System.Drawing.Point(14, 82);
			this.editGroup.Name = "editGroup";
			this.editGroup.Padding = new System.Windows.Forms.Padding(20, 9, 3, 3);
			this.editGroup.Size = new System.Drawing.Size(774, 155);
			this.editGroup.TabIndex = 9;
			this.editGroup.TabStop = false;
			this.editGroup.Text = "Edit Commands";
			// 
			// editPicture
			// 
			this.editPicture.Image = ((System.Drawing.Image)(resources.GetObject("editPicture.Image")));
			this.editPicture.Location = new System.Drawing.Point(62, 34);
			this.editPicture.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.editPicture.Name = "editPicture";
			this.editPicture.Size = new System.Drawing.Size(212, 95);
			this.editPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.editPicture.TabIndex = 11;
			this.editPicture.TabStop = false;
			this.editPicture.Click += new System.EventHandler(this.ClickPicture);
			// 
			// formulaGroup
			// 
			this.formulaGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.formulaGroup.Controls.Add(this.formulaPicture);
			this.formulaGroup.Controls.Add(this.formulaRibbonBox);
			this.formulaGroup.Controls.Add(this.formulaIconBox);
			this.formulaGroup.Location = new System.Drawing.Point(14, 245);
			this.formulaGroup.Name = "formulaGroup";
			this.formulaGroup.Padding = new System.Windows.Forms.Padding(20, 9, 3, 3);
			this.formulaGroup.Size = new System.Drawing.Size(774, 154);
			this.formulaGroup.TabIndex = 10;
			this.formulaGroup.TabStop = false;
			this.formulaGroup.Text = "Formula Commands";
			// 
			// formulaPicture
			// 
			this.formulaPicture.Image = ((System.Drawing.Image)(resources.GetObject("formulaPicture.Image")));
			this.formulaPicture.Location = new System.Drawing.Point(62, 32);
			this.formulaPicture.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.formulaPicture.Name = "formulaPicture";
			this.formulaPicture.Size = new System.Drawing.Size(212, 95);
			this.formulaPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.formulaPicture.TabIndex = 12;
			this.formulaPicture.TabStop = false;
			this.formulaPicture.Click += new System.EventHandler(this.ClickPicture);
			// 
			// RibbonBarSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.formulaGroup);
			this.Controls.Add(this.editGroup);
			this.Controls.Add(this.introBox);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Name = "RibbonBarSheet";
			this.Padding = new System.Windows.Forms.Padding(10, 9, 10, 9);
			this.Size = new System.Drawing.Size(800, 500);
			this.editGroup.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.editPicture)).EndInit();
			this.formulaGroup.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.formulaPicture)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private UI.MoreCheckBox editRibbonBox;
		private UI.MoreCheckBox editIconBox;
		private UI.MoreCheckBox formulaRibbonBox;
		private UI.MoreCheckBox formulaIconBox;
		private UI.MoreGroupBox editGroup;
		private UI.MoreGroupBox formulaGroup;
		private System.Windows.Forms.PictureBox editPicture;
		private System.Windows.Forms.PictureBox formulaPicture;
		private System.Windows.Forms.TextBox introBox;
	}
}
