
namespace River.OneMoreAddIn.Settings
{
	partial class NavigatorSheet
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
			this.components = new System.ComponentModel.Container();
			this.introBox = new River.OneMoreAddIn.UI.MoreMultilineLabel();
			this.layoutPanel = new System.Windows.Forms.Panel();
			this.hidePinnedBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.quickBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.advancedGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.disabledBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.secLabel = new System.Windows.Forms.Label();
			this.intervalBox = new System.Windows.Forms.NumericUpDown();
			this.intervalLabel = new System.Windows.Forms.Label();
			this.depthBox = new System.Windows.Forms.NumericUpDown();
			this.depthLabel = new System.Windows.Forms.Label();
			this.corrallBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.tooltip = new System.Windows.Forms.ToolTip(this.components);
			this.layoutPanel.SuspendLayout();
			this.advancedGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.intervalBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.depthBox)).BeginInit();
			this.SuspendLayout();
			// 
			// introBox
			// 
			this.introBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.introBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.introBox.Location = new System.Drawing.Point(13, 8);
			this.introBox.Name = "introBox";
			this.introBox.Padding = new System.Windows.Forms.Padding(0, 0, 0, 36);
			this.introBox.Size = new System.Drawing.Size(772, 66);
			this.introBox.TabIndex = 3;
			this.introBox.Text = "Customize advanced options for the Navigator Service";
			this.introBox.ThemedBack = "ControlLightLight";
			this.introBox.ThemedFore = "ControlText";
			// 
			// layoutPanel
			// 
			this.layoutPanel.Controls.Add(this.hidePinnedBox);
			this.layoutPanel.Controls.Add(this.quickBox);
			this.layoutPanel.Controls.Add(this.advancedGroup);
			this.layoutPanel.Controls.Add(this.secLabel);
			this.layoutPanel.Controls.Add(this.intervalBox);
			this.layoutPanel.Controls.Add(this.intervalLabel);
			this.layoutPanel.Controls.Add(this.depthBox);
			this.layoutPanel.Controls.Add(this.depthLabel);
			this.layoutPanel.Controls.Add(this.corrallBox);
			this.layoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.layoutPanel.Location = new System.Drawing.Point(13, 74);
			this.layoutPanel.Margin = new System.Windows.Forms.Padding(0);
			this.layoutPanel.Name = "layoutPanel";
			this.layoutPanel.Size = new System.Drawing.Size(772, 416);
			this.layoutPanel.TabIndex = 4;
			// 
			// hidePinnedBox
			// 
			this.hidePinnedBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.hidePinnedBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.hidePinnedBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.hidePinnedBox.Location = new System.Drawing.Point(7, 114);
			this.hidePinnedBox.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
			this.hidePinnedBox.Name = "hidePinnedBox";
			this.hidePinnedBox.Size = new System.Drawing.Size(264, 25);
			this.hidePinnedBox.StylizeImage = false;
			this.hidePinnedBox.TabIndex = 7;
			this.hidePinnedBox.Text = "Hide the My Reading List panel";
			this.hidePinnedBox.ThemedBack = null;
			this.hidePinnedBox.ThemedFore = null;
			this.hidePinnedBox.UseVisualStyleBackColor = true;
			// 
			// quickBox
			// 
			this.quickBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.quickBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.quickBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.quickBox.Location = new System.Drawing.Point(7, 144);
			this.quickBox.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
			this.quickBox.Name = "quickBox";
			this.quickBox.Size = new System.Drawing.Size(171, 25);
			this.quickBox.StylizeImage = false;
			this.quickBox.TabIndex = 2;
			this.quickBox.Text = "Track Quick Notes";
			this.quickBox.ThemedBack = null;
			this.quickBox.ThemedFore = null;
			this.tooltip.SetToolTip(this.quickBox, "Enabled only when multiple screens are available");
			this.quickBox.UseVisualStyleBackColor = true;
			// 
			// advancedGroup
			// 
			this.advancedGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.advancedGroup.BorderThickness = 3;
			this.advancedGroup.Controls.Add(this.disabledBox);
			this.advancedGroup.Location = new System.Drawing.Point(10, 274);
			this.advancedGroup.Name = "advancedGroup";
			this.advancedGroup.Padding = new System.Windows.Forms.Padding(15, 3, 3, 3);
			this.advancedGroup.ShowOnlyTopEdge = true;
			this.advancedGroup.Size = new System.Drawing.Size(759, 139);
			this.advancedGroup.TabIndex = 6;
			this.advancedGroup.TabStop = false;
			this.advancedGroup.Text = "Advanced Options";
			this.advancedGroup.ThemedBorder = null;
			this.advancedGroup.ThemedFore = null;
			// 
			// disabledBox
			// 
			this.disabledBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.disabledBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.disabledBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.disabledBox.Location = new System.Drawing.Point(18, 26);
			this.disabledBox.Name = "disabledBox";
			this.disabledBox.Size = new System.Drawing.Size(631, 25);
			this.disabledBox.StylizeImage = false;
			this.disabledBox.TabIndex = 0;
			this.disabledBox.Text = "Disable the navigation service. This will render the Navigator window inoperaable" +
    ".";
			this.disabledBox.ThemedBack = null;
			this.disabledBox.ThemedFore = null;
			this.disabledBox.UseVisualStyleBackColor = true;
			// 
			// secLabel
			// 
			this.secLabel.AutoSize = true;
			this.secLabel.Location = new System.Drawing.Point(523, 62);
			this.secLabel.Name = "secLabel";
			this.secLabel.Size = new System.Drawing.Size(72, 20);
			this.secLabel.TabIndex = 5;
			this.secLabel.Text = "Seconds";
			// 
			// intervalBox
			// 
			this.intervalBox.DecimalPlaces = 2;
			this.intervalBox.Increment = new decimal(new int[] {
            25,
            0,
            0,
            131072});
			this.intervalBox.Location = new System.Drawing.Point(397, 60);
			this.intervalBox.Maximum = new decimal(new int[] {
            600,
            0,
            0,
            65536});
			this.intervalBox.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            65536});
			this.intervalBox.Name = "intervalBox";
			this.intervalBox.Size = new System.Drawing.Size(120, 26);
			this.intervalBox.TabIndex = 1;
			this.intervalBox.Value = new decimal(new int[] {
            20,
            0,
            0,
            65536});
			// 
			// intervalLabel
			// 
			this.intervalLabel.AutoSize = true;
			this.intervalLabel.Location = new System.Drawing.Point(3, 62);
			this.intervalLabel.Name = "intervalLabel";
			this.intervalLabel.Size = new System.Drawing.Size(274, 20);
			this.intervalLabel.TabIndex = 3;
			this.intervalLabel.Text = "Min read time before adding to history";
			// 
			// depthBox
			// 
			this.depthBox.Location = new System.Drawing.Point(397, 8);
			this.depthBox.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
			this.depthBox.Name = "depthBox";
			this.depthBox.Size = new System.Drawing.Size(120, 26);
			this.depthBox.TabIndex = 0;
			this.depthBox.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
			// 
			// depthLabel
			// 
			this.depthLabel.AutoSize = true;
			this.depthLabel.Location = new System.Drawing.Point(3, 10);
			this.depthLabel.Name = "depthLabel";
			this.depthLabel.Size = new System.Drawing.Size(203, 20);
			this.depthLabel.TabIndex = 1;
			this.depthLabel.Text = "Max length of stored history";
			// 
			// corrallBox
			// 
			this.corrallBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.corrallBox.Checked = true;
			this.corrallBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.corrallBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.corrallBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.corrallBox.Location = new System.Drawing.Point(7, 174);
			this.corrallBox.Margin = new System.Windows.Forms.Padding(0);
			this.corrallBox.Name = "corrallBox";
			this.corrallBox.Size = new System.Drawing.Size(400, 25);
			this.corrallBox.StylizeImage = false;
			this.corrallBox.TabIndex = 3;
			this.corrallBox.Text = "Restrict the Navigator window to the active screen";
			this.corrallBox.ThemedBack = null;
			this.corrallBox.ThemedFore = null;
			this.tooltip.SetToolTip(this.corrallBox, "Enabled only when multiple screens are available");
			this.corrallBox.UseVisualStyleBackColor = true;
			// 
			// NavigatorSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.layoutPanel);
			this.Controls.Add(this.introBox);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Name = "NavigatorSheet";
			this.Padding = new System.Windows.Forms.Padding(13, 8, 15, 10);
			this.Size = new System.Drawing.Size(800, 500);
			this.layoutPanel.ResumeLayout(false);
			this.layoutPanel.PerformLayout();
			this.advancedGroup.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.intervalBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.depthBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreMultilineLabel introBox;
		private System.Windows.Forms.Panel layoutPanel;
		private UI.MoreCheckBox corrallBox;
		private System.Windows.Forms.NumericUpDown depthBox;
		private System.Windows.Forms.Label depthLabel;
		private System.Windows.Forms.NumericUpDown intervalBox;
		private System.Windows.Forms.Label intervalLabel;
		private System.Windows.Forms.ToolTip tooltip;
		private System.Windows.Forms.Label secLabel;
		private UI.MoreGroupBox advancedGroup;
		private UI.MoreCheckBox disabledBox;
		private UI.MoreCheckBox quickBox;
		private UI.MoreCheckBox hidePinnedBox;
	}
}
