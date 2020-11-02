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
			this.editGroup.SuspendLayout();
			this.formulaGroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// introBox
			// 
			this.introBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.introBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.introBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.introBox.Location = new System.Drawing.Point(10, 10);
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
			this.editRibbonBox.AutoSize = true;
			this.editRibbonBox.Image = ((System.Drawing.Image)(resources.GetObject("editRibbonBox.Image")));
			this.editRibbonBox.Location = new System.Drawing.Point(23, 32);
			this.editRibbonBox.Name = "editRibbonBox";
			this.editRibbonBox.Size = new System.Drawing.Size(216, 95);
			this.editRibbonBox.TabIndex = 5;
			this.editRibbonBox.UseVisualStyleBackColor = true;
			this.editRibbonBox.CheckedChanged += new System.EventHandler(this.CheckedChanged);
			// 
			// editIconBox
			// 
			this.editIconBox.AutoSize = true;
			this.editIconBox.Enabled = false;
			this.editIconBox.Location = new System.Drawing.Point(343, 67);
			this.editIconBox.Name = "editIconBox";
			this.editIconBox.Size = new System.Drawing.Size(283, 24);
			this.editIconBox.TabIndex = 6;
			this.editIconBox.Text = "Show only icons for edit commands";
			this.editIconBox.UseVisualStyleBackColor = true;
			// 
			// formulaRibbonBox
			// 
			this.formulaRibbonBox.AutoSize = true;
			this.formulaRibbonBox.Image = ((System.Drawing.Image)(resources.GetObject("formulaRibbonBox.Image")));
			this.formulaRibbonBox.Location = new System.Drawing.Point(23, 25);
			this.formulaRibbonBox.Name = "formulaRibbonBox";
			this.formulaRibbonBox.Size = new System.Drawing.Size(216, 95);
			this.formulaRibbonBox.TabIndex = 7;
			this.formulaRibbonBox.UseVisualStyleBackColor = true;
			this.formulaRibbonBox.CheckedChanged += new System.EventHandler(this.CheckedChanged);
			// 
			// formulaIconBox
			// 
			this.formulaIconBox.AutoSize = true;
			this.formulaIconBox.Enabled = false;
			this.formulaIconBox.Location = new System.Drawing.Point(343, 60);
			this.formulaIconBox.Name = "formulaIconBox";
			this.formulaIconBox.Size = new System.Drawing.Size(310, 24);
			this.formulaIconBox.TabIndex = 8;
			this.formulaIconBox.Text = "Show only icons for formula commands";
			this.formulaIconBox.UseVisualStyleBackColor = true;
			// 
			// editGroup
			// 
			this.editGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.editGroup.Controls.Add(this.editRibbonBox);
			this.editGroup.Controls.Add(this.editIconBox);
			this.editGroup.Location = new System.Drawing.Point(13, 82);
			this.editGroup.Name = "editGroup";
			this.editGroup.Padding = new System.Windows.Forms.Padding(20, 10, 3, 3);
			this.editGroup.Size = new System.Drawing.Size(774, 156);
			this.editGroup.TabIndex = 9;
			this.editGroup.TabStop = false;
			this.editGroup.Text = "Edit Commands";
			// 
			// formulaGroup
			// 
			this.formulaGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.formulaGroup.Controls.Add(this.formulaRibbonBox);
			this.formulaGroup.Controls.Add(this.formulaIconBox);
			this.formulaGroup.Location = new System.Drawing.Point(13, 244);
			this.formulaGroup.Name = "formulaGroup";
			this.formulaGroup.Size = new System.Drawing.Size(774, 154);
			this.formulaGroup.TabIndex = 10;
			this.formulaGroup.TabStop = false;
			this.formulaGroup.Text = "Formula Commands";
			// 
			// RibbonBarSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.formulaGroup);
			this.Controls.Add(this.editGroup);
			this.Controls.Add(this.introBox);
			this.Name = "RibbonBarSheet";
			this.Padding = new System.Windows.Forms.Padding(10);
			this.Size = new System.Drawing.Size(800, 500);
			this.editGroup.ResumeLayout(false);
			this.editGroup.PerformLayout();
			this.formulaGroup.ResumeLayout(false);
			this.formulaGroup.PerformLayout();
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
	}
}
