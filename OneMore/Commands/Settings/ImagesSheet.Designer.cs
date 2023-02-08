
namespace River.OneMoreAddIn.Settings
{
	partial class ImagesSheet
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
			this.introBox = new System.Windows.Forms.TextBox();
			this.layoutPanel = new System.Windows.Forms.Panel();
			this.plantGroup = new System.Windows.Forms.GroupBox();
			this.plantCollapseBox = new System.Windows.Forms.CheckBox();
			this.plantAfterBox = new System.Windows.Forms.CheckBox();
			this.layoutPanel.SuspendLayout();
			this.plantGroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// introBox
			// 
			this.introBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.introBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.introBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.introBox.Location = new System.Drawing.Point(13, 8);
			this.introBox.Multiline = true;
			this.introBox.Name = "introBox";
			this.introBox.ReadOnly = true;
			this.introBox.Size = new System.Drawing.Size(772, 66);
			this.introBox.TabIndex = 3;
			this.introBox.Text = "Customize the defaults for Image commands";
			// 
			// layoutPanel
			// 
			this.layoutPanel.Controls.Add(this.plantGroup);
			this.layoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.layoutPanel.Location = new System.Drawing.Point(13, 74);
			this.layoutPanel.Margin = new System.Windows.Forms.Padding(0);
			this.layoutPanel.Name = "layoutPanel";
			this.layoutPanel.Size = new System.Drawing.Size(772, 416);
			this.layoutPanel.TabIndex = 4;
			// 
			// plantGroup
			// 
			this.plantGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.plantGroup.Controls.Add(this.plantCollapseBox);
			this.plantGroup.Controls.Add(this.plantAfterBox);
			this.plantGroup.Location = new System.Drawing.Point(3, 6);
			this.plantGroup.Name = "plantGroup";
			this.plantGroup.Size = new System.Drawing.Size(766, 128);
			this.plantGroup.TabIndex = 0;
			this.plantGroup.TabStop = false;
			this.plantGroup.Text = "PlantUML Options";
			// 
			// plantCollapseBox
			// 
			this.plantCollapseBox.AutoSize = true;
			this.plantCollapseBox.Location = new System.Drawing.Point(32, 67);
			this.plantCollapseBox.Name = "plantCollapseBox";
			this.plantCollapseBox.Size = new System.Drawing.Size(200, 24);
			this.plantCollapseBox.TabIndex = 1;
			this.plantCollapseBox.Text = "Collapse PlantUML text";
			this.plantCollapseBox.UseVisualStyleBackColor = true;
			// 
			// plantAfterBox
			// 
			this.plantAfterBox.AutoSize = true;
			this.plantAfterBox.Location = new System.Drawing.Point(32, 37);
			this.plantAfterBox.Name = "plantAfterBox";
			this.plantAfterBox.Size = new System.Drawing.Size(276, 24);
			this.plantAfterBox.TabIndex = 0;
			this.plantAfterBox.Text = "Insert drawing after PlantUML text";
			this.plantAfterBox.UseVisualStyleBackColor = true;
			// 
			// ImagesSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.layoutPanel);
			this.Controls.Add(this.introBox);
			this.Name = "ImagesSheet";
			this.Padding = new System.Windows.Forms.Padding(13, 8, 15, 10);
			this.Size = new System.Drawing.Size(800, 500);
			this.layoutPanel.ResumeLayout(false);
			this.plantGroup.ResumeLayout(false);
			this.plantGroup.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox introBox;
		private System.Windows.Forms.Panel layoutPanel;
		private System.Windows.Forms.GroupBox plantGroup;
		private System.Windows.Forms.CheckBox plantCollapseBox;
		private System.Windows.Forms.CheckBox plantAfterBox;
	}
}
