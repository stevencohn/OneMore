
namespace River.OneMoreAddIn.Settings
{
	partial class HashtagSheet
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
			this.styleBox = new System.Windows.Forms.ComboBox();
			this.styleLabel = new System.Windows.Forms.Label();
			this.advancedGroup = new System.Windows.Forms.GroupBox();
			this.rebuildBox = new System.Windows.Forms.CheckBox();
			this.disabledBox = new System.Windows.Forms.CheckBox();
			this.minLabel = new System.Windows.Forms.Label();
			this.intervalBox = new System.Windows.Forms.NumericUpDown();
			this.intervalLabel = new System.Windows.Forms.Label();
			this.filterBox = new System.Windows.Forms.CheckBox();
			this.layoutPanel.SuspendLayout();
			this.advancedGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.intervalBox)).BeginInit();
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
			this.introBox.Text = "Customize advanced options for the Hashtag Scanner Service";
			// 
			// layoutPanel
			// 
			this.layoutPanel.Controls.Add(this.filterBox);
			this.layoutPanel.Controls.Add(this.styleBox);
			this.layoutPanel.Controls.Add(this.styleLabel);
			this.layoutPanel.Controls.Add(this.advancedGroup);
			this.layoutPanel.Controls.Add(this.minLabel);
			this.layoutPanel.Controls.Add(this.intervalBox);
			this.layoutPanel.Controls.Add(this.intervalLabel);
			this.layoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.layoutPanel.Location = new System.Drawing.Point(13, 74);
			this.layoutPanel.Margin = new System.Windows.Forms.Padding(0);
			this.layoutPanel.Name = "layoutPanel";
			this.layoutPanel.Size = new System.Drawing.Size(772, 516);
			this.layoutPanel.TabIndex = 4;
			// 
			// styleBox
			// 
			this.styleBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.styleBox.FormattingEnabled = true;
			this.styleBox.Items.AddRange(new object[] {
            "None",
            "Red Foreground",
            "Yellow Background"});
			this.styleBox.Location = new System.Drawing.Point(304, 70);
			this.styleBox.Name = "styleBox";
			this.styleBox.Size = new System.Drawing.Size(280, 28);
			this.styleBox.TabIndex = 8;
			// 
			// styleLabel
			// 
			this.styleLabel.AutoSize = true;
			this.styleLabel.Location = new System.Drawing.Point(7, 73);
			this.styleLabel.Name = "styleLabel";
			this.styleLabel.Size = new System.Drawing.Size(140, 20);
			this.styleLabel.TabIndex = 7;
			this.styleLabel.Text = "Apply custom style";
			// 
			// advancedGroup
			// 
			this.advancedGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.advancedGroup.Controls.Add(this.rebuildBox);
			this.advancedGroup.Controls.Add(this.disabledBox);
			this.advancedGroup.Location = new System.Drawing.Point(10, 352);
			this.advancedGroup.Name = "advancedGroup";
			this.advancedGroup.Padding = new System.Windows.Forms.Padding(15, 3, 3, 3);
			this.advancedGroup.Size = new System.Drawing.Size(759, 126);
			this.advancedGroup.TabIndex = 6;
			this.advancedGroup.TabStop = false;
			this.advancedGroup.Text = "Advanced Options";
			// 
			// rebuildBox
			// 
			this.rebuildBox.AutoSize = true;
			this.rebuildBox.Location = new System.Drawing.Point(18, 41);
			this.rebuildBox.Name = "rebuildBox";
			this.rebuildBox.Size = new System.Drawing.Size(339, 24);
			this.rebuildBox.TabIndex = 10;
			this.rebuildBox.Text = "Rebuild the hashtag database upon restart";
			this.rebuildBox.UseVisualStyleBackColor = true;
			this.rebuildBox.CheckedChanged += new System.EventHandler(this.ConfirmRebuild);
			// 
			// disabledBox
			// 
			this.disabledBox.AutoSize = true;
			this.disabledBox.Location = new System.Drawing.Point(18, 71);
			this.disabledBox.Name = "disabledBox";
			this.disabledBox.Size = new System.Drawing.Size(517, 24);
			this.disabledBox.TabIndex = 0;
			this.disabledBox.Text = "Disable the hashtag service. This will also disable hashtag searching.";
			this.disabledBox.UseVisualStyleBackColor = true;
			// 
			// minLabel
			// 
			this.minLabel.AutoSize = true;
			this.minLabel.Location = new System.Drawing.Point(430, 8);
			this.minLabel.Name = "minLabel";
			this.minLabel.Size = new System.Drawing.Size(65, 20);
			this.minLabel.TabIndex = 5;
			this.minLabel.Text = "Minutes";
			// 
			// intervalBox
			// 
			this.intervalBox.Location = new System.Drawing.Point(304, 6);
			this.intervalBox.Maximum = new decimal(new int[] {
            600,
            0,
            0,
            65536});
			this.intervalBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.intervalBox.Name = "intervalBox";
			this.intervalBox.Size = new System.Drawing.Size(120, 26);
			this.intervalBox.TabIndex = 4;
			this.intervalBox.Value = new decimal(new int[] {
            20,
            0,
            0,
            65536});
			// 
			// intervalLabel
			// 
			this.intervalLabel.AutoSize = true;
			this.intervalLabel.Location = new System.Drawing.Point(7, 8);
			this.intervalLabel.Name = "intervalLabel";
			this.intervalLabel.Size = new System.Drawing.Size(180, 20);
			this.intervalLabel.TabIndex = 3;
			this.intervalLabel.Text = "Scan for hashtags every";
			// 
			// filterBox
			// 
			this.filterBox.AutoSize = true;
			this.filterBox.Location = new System.Drawing.Point(11, 134);
			this.filterBox.Name = "filterBox";
			this.filterBox.Size = new System.Drawing.Size(405, 24);
			this.filterBox.TabIndex = 11;
			this.filterBox.Text = "Exclude HTML Hex colors and C# and C++ directives";
			this.filterBox.UseVisualStyleBackColor = true;
			// 
			// HashtagSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.layoutPanel);
			this.Controls.Add(this.introBox);
			this.Name = "HashtagSheet";
			this.Padding = new System.Windows.Forms.Padding(13, 8, 15, 10);
			this.Size = new System.Drawing.Size(800, 600);
			this.layoutPanel.ResumeLayout(false);
			this.layoutPanel.PerformLayout();
			this.advancedGroup.ResumeLayout(false);
			this.advancedGroup.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.intervalBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox introBox;
		private System.Windows.Forms.Panel layoutPanel;
		private System.Windows.Forms.NumericUpDown intervalBox;
		private System.Windows.Forms.Label intervalLabel;
		private System.Windows.Forms.Label minLabel;
		private System.Windows.Forms.GroupBox advancedGroup;
		private System.Windows.Forms.CheckBox disabledBox;
		private System.Windows.Forms.CheckBox rebuildBox;
		private System.Windows.Forms.ComboBox styleBox;
		private System.Windows.Forms.Label styleLabel;
		private System.Windows.Forms.CheckBox filterBox;
	}
}
