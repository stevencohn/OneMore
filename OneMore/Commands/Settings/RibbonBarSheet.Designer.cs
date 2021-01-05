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
			this.editRibbonBox = new System.Windows.Forms.CheckBox();
			this.editIconBox = new System.Windows.Forms.CheckBox();
			this.formulaRibbonBox = new System.Windows.Forms.CheckBox();
			this.formulaIconBox = new System.Windows.Forms.CheckBox();
			this.editGroup = new System.Windows.Forms.GroupBox();
			this.formulaGroup = new System.Windows.Forms.GroupBox();
			this.editPicture = new System.Windows.Forms.PictureBox();
			this.formulaPicture = new System.Windows.Forms.PictureBox();
			this.editGroup.SuspendLayout();
			this.formulaGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.editPicture)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.formulaPicture)).BeginInit();
			this.SuspendLayout();
			// 
			// introBox
			// 
			this.introBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.introBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.introBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.introBox.Location = new System.Drawing.Point(7, 6);
			this.introBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.introBox.Multiline = true;
			this.introBox.Name = "introBox";
			this.introBox.ReadOnly = true;
			this.introBox.Size = new System.Drawing.Size(519, 43);
			this.introBox.TabIndex = 3;
			this.introBox.Text = "Choose which commands to include in the ribbon bar and whether to display icons o" +
    "r icons and lables. Restart OneNote to see changes.";
			// 
			// editRibbonBox
			// 
			this.editRibbonBox.Location = new System.Drawing.Point(15, 21);
			this.editRibbonBox.Name = "editRibbonBox";
			this.editRibbonBox.Size = new System.Drawing.Size(20, 25);
			this.editRibbonBox.TabIndex = 5;
			this.editRibbonBox.UseVisualStyleBackColor = true;
			this.editRibbonBox.CheckedChanged += new System.EventHandler(this.CheckedChanged);
			// 
			// editIconBox
			// 
			this.editIconBox.AutoSize = true;
			this.editIconBox.Enabled = false;
			this.editIconBox.Location = new System.Drawing.Point(229, 25);
			this.editIconBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.editIconBox.Name = "editIconBox";
			this.editIconBox.Size = new System.Drawing.Size(192, 17);
			this.editIconBox.TabIndex = 6;
			this.editIconBox.Text = "Show only icons for edit commands";
			this.editIconBox.UseVisualStyleBackColor = true;
			// 
			// formulaRibbonBox
			// 
			this.formulaRibbonBox.Location = new System.Drawing.Point(15, 21);
			this.formulaRibbonBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.formulaRibbonBox.Name = "formulaRibbonBox";
			this.formulaRibbonBox.Size = new System.Drawing.Size(20, 25);
			this.formulaRibbonBox.TabIndex = 7;
			this.formulaRibbonBox.UseVisualStyleBackColor = true;
			this.formulaRibbonBox.CheckedChanged += new System.EventHandler(this.CheckedChanged);
			// 
			// formulaIconBox
			// 
			this.formulaIconBox.AutoSize = true;
			this.formulaIconBox.Enabled = false;
			this.formulaIconBox.Location = new System.Drawing.Point(229, 29);
			this.formulaIconBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.formulaIconBox.Name = "formulaIconBox";
			this.formulaIconBox.Size = new System.Drawing.Size(209, 17);
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
			this.editGroup.Location = new System.Drawing.Point(9, 53);
			this.editGroup.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.editGroup.Name = "editGroup";
			this.editGroup.Padding = new System.Windows.Forms.Padding(13, 6, 2, 2);
			this.editGroup.Size = new System.Drawing.Size(516, 101);
			this.editGroup.TabIndex = 9;
			this.editGroup.TabStop = false;
			this.editGroup.Text = "Edit Commands";
			// 
			// formulaGroup
			// 
			this.formulaGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.formulaGroup.Controls.Add(this.formulaPicture);
			this.formulaGroup.Controls.Add(this.formulaRibbonBox);
			this.formulaGroup.Controls.Add(this.formulaIconBox);
			this.formulaGroup.Location = new System.Drawing.Point(9, 159);
			this.formulaGroup.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.formulaGroup.Name = "formulaGroup";
			this.formulaGroup.Padding = new System.Windows.Forms.Padding(13, 6, 2, 2);
			this.formulaGroup.Size = new System.Drawing.Size(516, 100);
			this.formulaGroup.TabIndex = 10;
			this.formulaGroup.TabStop = false;
			this.formulaGroup.Text = "Formula Commands";
			// 
			// editPicture
			// 
			this.editPicture.Image = ((System.Drawing.Image)(resources.GetObject("editPicture.Image")));
			this.editPicture.Location = new System.Drawing.Point(41, 22);
			this.editPicture.Name = "editPicture";
			this.editPicture.Size = new System.Drawing.Size(141, 62);
			this.editPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.editPicture.TabIndex = 11;
			this.editPicture.TabStop = false;
			this.editPicture.Click += new System.EventHandler(this.ClickPicture);
			// 
			// formulaPicture
			// 
			this.formulaPicture.Image = ((System.Drawing.Image)(resources.GetObject("formulaPicture.Image")));
			this.formulaPicture.Location = new System.Drawing.Point(40, 22);
			this.formulaPicture.Name = "formulaPicture";
			this.formulaPicture.Size = new System.Drawing.Size(141, 62);
			this.formulaPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.formulaPicture.TabIndex = 12;
			this.formulaPicture.TabStop = false;
			this.formulaPicture.Click += new System.EventHandler(this.ClickPicture);
			// 
			// RibbonBarSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.formulaGroup);
			this.Controls.Add(this.editGroup);
			this.Controls.Add(this.introBox);
			this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.Name = "RibbonBarSheet";
			this.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.Size = new System.Drawing.Size(533, 325);
			this.editGroup.ResumeLayout(false);
			this.editGroup.PerformLayout();
			this.formulaGroup.ResumeLayout(false);
			this.formulaGroup.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.editPicture)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.formulaPicture)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox introBox;
		private System.Windows.Forms.CheckBox editRibbonBox;
		private System.Windows.Forms.CheckBox editIconBox;
		private System.Windows.Forms.CheckBox formulaRibbonBox;
		private System.Windows.Forms.CheckBox formulaIconBox;
		private System.Windows.Forms.GroupBox editGroup;
		private System.Windows.Forms.GroupBox formulaGroup;
		private System.Windows.Forms.PictureBox editPicture;
		private System.Windows.Forms.PictureBox formulaPicture;
	}
}
