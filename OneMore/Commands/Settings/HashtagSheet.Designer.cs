
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
			this.components = new System.ComponentModel.Container();
			this.introBox = new River.OneMoreAddIn.UI.MoreMultilineLabel();
			this.intervalLabel = new System.Windows.Forms.Label();
			this.intervalBox = new System.Windows.Forms.NumericUpDown();
			this.minLabel = new System.Windows.Forms.Label();
			this.advancedGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.warningLabel = new River.OneMoreAddIn.UI.MoreMultilineLabel();
			this.scheduleLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.disabledBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.styleLabel = new System.Windows.Forms.Label();
			this.styleBox = new System.Windows.Forms.ComboBox();
			this.filterBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.delayLabel = new System.Windows.Forms.Label();
			this.delayBox = new System.Windows.Forms.NumericUpDown();
			this.msLabel = new System.Windows.Forms.Label();
			this.layoutPanel = new System.Windows.Forms.Panel();
			this.tooltip = new System.Windows.Forms.ToolTip(this.components);
			((System.ComponentModel.ISupportInitialize)(this.intervalBox)).BeginInit();
			this.advancedGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.delayBox)).BeginInit();
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
			this.introBox.Text = "Customize advanced options for the Hashtag Scanner Service";
			this.introBox.ThemedBack = "ControlLightLight";
			this.introBox.ThemedFore = "ControlText";
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
			// minLabel
			// 
			this.minLabel.AutoSize = true;
			this.minLabel.Location = new System.Drawing.Point(430, 8);
			this.minLabel.Name = "minLabel";
			this.minLabel.Size = new System.Drawing.Size(65, 20);
			this.minLabel.TabIndex = 5;
			this.minLabel.Text = "Minutes";
			// 
			// advancedGroup
			// 
			this.advancedGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.advancedGroup.BorderThickness = 3;
			this.advancedGroup.Controls.Add(this.warningLabel);
			this.advancedGroup.Controls.Add(this.scheduleLink);
			this.advancedGroup.Controls.Add(this.disabledBox);
			this.advancedGroup.Location = new System.Drawing.Point(10, 212);
			this.advancedGroup.Name = "advancedGroup";
			this.advancedGroup.Padding = new System.Windows.Forms.Padding(15, 3, 3, 3);
			this.advancedGroup.ShowOnlyTopEdge = true;
			this.advancedGroup.Size = new System.Drawing.Size(759, 253);
			this.advancedGroup.TabIndex = 6;
			this.advancedGroup.TabStop = false;
			this.advancedGroup.Text = "Advanced Options";
			this.advancedGroup.ThemedBorder = null;
			this.advancedGroup.ThemedFore = null;
			// 
			// warningLabel
			// 
			this.warningLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.warningLabel.Location = new System.Drawing.Point(18, 58);
			this.warningLabel.Name = "warningLabel";
			this.warningLabel.Size = new System.Drawing.Size(735, 49);
			this.warningLabel.TabIndex = 2;
			this.warningLabel.Text = "This should be used after adding or removing one or more notebooks. It is recomme" +
    "nded to schedule rebuilds after-hours, such as midnight.";
			this.warningLabel.ThemedBack = null;
			this.warningLabel.ThemedFore = null;
			// 
			// scheduleLink
			// 
			this.scheduleLink.ActiveLinkColor = System.Drawing.Color.MediumOrchid;
			this.scheduleLink.AutoSize = true;
			this.scheduleLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.scheduleLink.HoverColor = System.Drawing.Color.Orchid;
			this.scheduleLink.LinkColor = System.Drawing.Color.MediumOrchid;
			this.scheduleLink.Location = new System.Drawing.Point(18, 35);
			this.scheduleLink.Name = "scheduleLink";
			this.scheduleLink.Size = new System.Drawing.Size(310, 20);
			this.scheduleLink.StrictColors = false;
			this.scheduleLink.TabIndex = 1;
			this.scheduleLink.TabStop = true;
			this.scheduleLink.Text = "Schedule a rebuild of your hashtag catalog";
			this.scheduleLink.ThemedBack = null;
			this.scheduleLink.ThemedFore = null;
			this.scheduleLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.scheduleLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ScheduleRebuild);
			// 
			// disabledBox
			// 
			this.disabledBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.disabledBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.disabledBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.disabledBox.Location = new System.Drawing.Point(18, 203);
			this.disabledBox.Name = "disabledBox";
			this.disabledBox.Size = new System.Drawing.Size(540, 25);
			this.disabledBox.StylizeImage = false;
			this.disabledBox.TabIndex = 0;
			this.disabledBox.Text = "Disable the hashtag service. This will also disable hashtag searching.";
			this.disabledBox.ThemedBack = null;
			this.disabledBox.ThemedFore = null;
			this.disabledBox.UseVisualStyleBackColor = true;
			// 
			// styleLabel
			// 
			this.styleLabel.AutoSize = true;
			this.styleLabel.Location = new System.Drawing.Point(7, 144);
			this.styleLabel.Name = "styleLabel";
			this.styleLabel.Size = new System.Drawing.Size(140, 20);
			this.styleLabel.TabIndex = 7;
			this.styleLabel.Text = "Apply custom style";
			// 
			// styleBox
			// 
			this.styleBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.styleBox.FormattingEnabled = true;
			this.styleBox.Items.AddRange(new object[] {
            "None",
            "Red Foreground",
            "Yellow Background"});
			this.styleBox.Location = new System.Drawing.Point(304, 141);
			this.styleBox.Name = "styleBox";
			this.styleBox.Size = new System.Drawing.Size(280, 28);
			this.styleBox.TabIndex = 8;
			// 
			// filterBox
			// 
			this.filterBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.filterBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.filterBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.filterBox.Location = new System.Drawing.Point(11, 87);
			this.filterBox.Name = "filterBox";
			this.filterBox.Size = new System.Drawing.Size(497, 25);
			this.filterBox.StylizeImage = false;
			this.filterBox.TabIndex = 11;
			this.filterBox.Text = "Treat HTML Hex colors and C# and C++ directives as hashtags";
			this.filterBox.ThemedBack = null;
			this.filterBox.ThemedFore = null;
			this.filterBox.UseVisualStyleBackColor = true;
			// 
			// delayLabel
			// 
			this.delayLabel.AutoSize = true;
			this.delayLabel.Location = new System.Drawing.Point(7, 40);
			this.delayLabel.Name = "delayLabel";
			this.delayLabel.Size = new System.Drawing.Size(162, 20);
			this.delayLabel.TabIndex = 12;
			this.delayLabel.Text = "Delay between pages";
			// 
			// delayBox
			// 
			this.delayBox.Location = new System.Drawing.Point(304, 38);
			this.delayBox.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
			this.delayBox.Name = "delayBox";
			this.delayBox.Size = new System.Drawing.Size(120, 26);
			this.delayBox.TabIndex = 13;
			// 
			// msLabel
			// 
			this.msLabel.AutoSize = true;
			this.msLabel.Location = new System.Drawing.Point(430, 40);
			this.msLabel.Name = "msLabel";
			this.msLabel.Size = new System.Drawing.Size(30, 20);
			this.msLabel.TabIndex = 14;
			this.msLabel.Text = "ms";
			// 
			// layoutPanel
			// 
			this.layoutPanel.Controls.Add(this.msLabel);
			this.layoutPanel.Controls.Add(this.delayBox);
			this.layoutPanel.Controls.Add(this.delayLabel);
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
			this.layoutPanel.Size = new System.Drawing.Size(772, 476);
			this.layoutPanel.TabIndex = 4;
			// 
			// HashtagSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.layoutPanel);
			this.Controls.Add(this.introBox);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Name = "HashtagSheet";
			this.Padding = new System.Windows.Forms.Padding(13, 8, 15, 10);
			this.Size = new System.Drawing.Size(800, 560);
			((System.ComponentModel.ISupportInitialize)(this.intervalBox)).EndInit();
			this.advancedGroup.ResumeLayout(false);
			this.advancedGroup.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.delayBox)).EndInit();
			this.layoutPanel.ResumeLayout(false);
			this.layoutPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreMultilineLabel introBox;
		private System.Windows.Forms.Label intervalLabel;
		private System.Windows.Forms.NumericUpDown intervalBox;
		private System.Windows.Forms.Label minLabel;
		private UI.MoreGroupBox advancedGroup;
		private UI.MoreCheckBox disabledBox;
		private System.Windows.Forms.Label styleLabel;
		private System.Windows.Forms.ComboBox styleBox;
		private UI.MoreCheckBox filterBox;
		private System.Windows.Forms.Label delayLabel;
		private System.Windows.Forms.NumericUpDown delayBox;
		private System.Windows.Forms.Label msLabel;
		private System.Windows.Forms.Panel layoutPanel;
		private UI.MoreMultilineLabel warningLabel;
		private UI.MoreLinkLabel scheduleLink;
		private System.Windows.Forms.ToolTip tooltip;
	}
}
