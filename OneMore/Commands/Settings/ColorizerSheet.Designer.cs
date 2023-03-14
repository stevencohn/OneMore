
namespace River.OneMoreAddIn.Settings
{
	partial class ColorizerSheet
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
			this.family2Box = new River.OneMoreAddIn.UI.FontComboBox();
			this.size2Box = new System.Windows.Forms.ComboBox();
			this.font2Label = new System.Windows.Forms.Label();
			this.fixedBox = new System.Windows.Forms.CheckBox();
			this.familyBox = new River.OneMoreAddIn.UI.FontComboBox();
			this.sizeBox = new System.Windows.Forms.ComboBox();
			this.fontLabel = new System.Windows.Forms.Label();
			this.applyBox = new System.Windows.Forms.CheckBox();
			this.layoutPanel.SuspendLayout();
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
			this.introBox.Text = "Customize the behavior of the Colorize command. Secondary font applies when colla" +
    "psing PlantUML";
			// 
			// layoutPanel
			// 
			this.layoutPanel.Controls.Add(this.family2Box);
			this.layoutPanel.Controls.Add(this.size2Box);
			this.layoutPanel.Controls.Add(this.font2Label);
			this.layoutPanel.Controls.Add(this.fixedBox);
			this.layoutPanel.Controls.Add(this.familyBox);
			this.layoutPanel.Controls.Add(this.sizeBox);
			this.layoutPanel.Controls.Add(this.fontLabel);
			this.layoutPanel.Controls.Add(this.applyBox);
			this.layoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.layoutPanel.Location = new System.Drawing.Point(13, 74);
			this.layoutPanel.Margin = new System.Windows.Forms.Padding(0);
			this.layoutPanel.Name = "layoutPanel";
			this.layoutPanel.Size = new System.Drawing.Size(772, 416);
			this.layoutPanel.TabIndex = 4;
			// 
			// family2Box
			// 
			this.family2Box.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.family2Box.DropDownHeight = 400;
			this.family2Box.DropDownWidth = 350;
			this.family2Box.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.family2Box.FormattingEnabled = true;
			this.family2Box.IntegralHeight = false;
			this.family2Box.Location = new System.Drawing.Point(135, 122);
			this.family2Box.Name = "family2Box";
			this.family2Box.Size = new System.Drawing.Size(355, 32);
			this.family2Box.TabIndex = 16;
			// 
			// size2Box
			// 
			this.size2Box.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.size2Box.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.size2Box.FormattingEnabled = true;
			this.size2Box.Items.AddRange(new object[] {
            "8",
            "9",
            "9.5",
            "10",
            "10.5",
            "11",
            "11.5",
            "12",
            "14",
            "16",
            "18",
            "20",
            "22",
            "24",
            "26"});
			this.size2Box.Location = new System.Drawing.Point(496, 121);
			this.size2Box.Name = "size2Box";
			this.size2Box.Size = new System.Drawing.Size(104, 33);
			this.size2Box.TabIndex = 15;
			// 
			// font2Label
			// 
			this.font2Label.AutoSize = true;
			this.font2Label.Location = new System.Drawing.Point(6, 129);
			this.font2Label.Name = "font2Label";
			this.font2Label.Size = new System.Drawing.Size(85, 20);
			this.font2Label.TabIndex = 14;
			this.font2Label.Text = "Secondary";
			// 
			// fixedBox
			// 
			this.fixedBox.AutoSize = true;
			this.fixedBox.Location = new System.Drawing.Point(135, 204);
			this.fixedBox.Name = "fixedBox";
			this.fixedBox.Size = new System.Drawing.Size(226, 24);
			this.fixedBox.TabIndex = 3;
			this.fixedBox.Text = "Show only fixed-width fonts";
			this.fixedBox.UseVisualStyleBackColor = true;
			this.fixedBox.CheckedChanged += new System.EventHandler(this.FilterFontsOnCheckedChanged);
			// 
			// familyBox
			// 
			this.familyBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.familyBox.DropDownHeight = 400;
			this.familyBox.DropDownWidth = 350;
			this.familyBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.familyBox.FormattingEnabled = true;
			this.familyBox.IntegralHeight = false;
			this.familyBox.Location = new System.Drawing.Point(135, 68);
			this.familyBox.Name = "familyBox";
			this.familyBox.Size = new System.Drawing.Size(355, 32);
			this.familyBox.TabIndex = 1;
			// 
			// sizeBox
			// 
			this.sizeBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.sizeBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.sizeBox.FormattingEnabled = true;
			this.sizeBox.Items.AddRange(new object[] {
            "8",
            "9",
            "9.5",
            "10",
            "10.5",
            "11",
            "11.5",
            "12",
            "14",
            "16",
            "18",
            "20",
            "22",
            "24",
            "26"});
			this.sizeBox.Location = new System.Drawing.Point(496, 68);
			this.sizeBox.Name = "sizeBox";
			this.sizeBox.Size = new System.Drawing.Size(104, 33);
			this.sizeBox.TabIndex = 2;
			// 
			// fontLabel
			// 
			this.fontLabel.AutoSize = true;
			this.fontLabel.Location = new System.Drawing.Point(6, 75);
			this.fontLabel.Name = "fontLabel";
			this.fontLabel.Size = new System.Drawing.Size(61, 20);
			this.fontLabel.TabIndex = 13;
			this.fontLabel.Text = "Primary";
			// 
			// applyBox
			// 
			this.applyBox.AutoSize = true;
			this.applyBox.Location = new System.Drawing.Point(3, 6);
			this.applyBox.Name = "applyBox";
			this.applyBox.Size = new System.Drawing.Size(408, 24);
			this.applyBox.TabIndex = 0;
			this.applyBox.Text = "Always apply the following fonts when colorizing code";
			this.applyBox.UseVisualStyleBackColor = true;
			// 
			// ColorizerSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.layoutPanel);
			this.Controls.Add(this.introBox);
			this.Name = "ColorizerSheet";
			this.Padding = new System.Windows.Forms.Padding(13, 8, 15, 10);
			this.Size = new System.Drawing.Size(800, 500);
			this.layoutPanel.ResumeLayout(false);
			this.layoutPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox introBox;
		private System.Windows.Forms.Panel layoutPanel;
		private System.Windows.Forms.CheckBox applyBox;
		private UI.FontComboBox familyBox;
		private System.Windows.Forms.ComboBox sizeBox;
		private System.Windows.Forms.Label fontLabel;
		private System.Windows.Forms.CheckBox fixedBox;
		private System.Windows.Forms.ComboBox size2Box;
		private System.Windows.Forms.Label font2Label;
		private UI.FontComboBox family2Box;
	}
}
