
namespace River.OneMoreAddIn.Settings
{
	partial class GeneralSheet
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
			this.sequentialBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.themeBox = new System.Windows.Forms.ComboBox();
			this.themeLabel = new System.Windows.Forms.Label();
			this.advancedGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.experimentalBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.verboseBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.langBox = new System.Windows.Forms.ComboBox();
			this.langLabel = new System.Windows.Forms.Label();
			this.checkUpdatesBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.layoutPanel.SuspendLayout();
			this.advancedGroup.SuspendLayout();
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
			this.introBox.Text = "Customize the overall behavior of OneMore";
			this.introBox.ThemedBack = "ControlLightLight";
			this.introBox.ThemedFore = "ControlText";
			// 
			// layoutPanel
			// 
			this.layoutPanel.Controls.Add(this.sequentialBox);
			this.layoutPanel.Controls.Add(this.themeBox);
			this.layoutPanel.Controls.Add(this.themeLabel);
			this.layoutPanel.Controls.Add(this.advancedGroup);
			this.layoutPanel.Controls.Add(this.langBox);
			this.layoutPanel.Controls.Add(this.langLabel);
			this.layoutPanel.Controls.Add(this.checkUpdatesBox);
			this.layoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.layoutPanel.Location = new System.Drawing.Point(13, 74);
			this.layoutPanel.Margin = new System.Windows.Forms.Padding(0);
			this.layoutPanel.Name = "layoutPanel";
			this.layoutPanel.Size = new System.Drawing.Size(772, 416);
			this.layoutPanel.TabIndex = 4;
			// 
			// sequentialBox
			// 
			this.sequentialBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.sequentialBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.sequentialBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.sequentialBox.Location = new System.Drawing.Point(25, 159);
			this.sequentialBox.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
			this.sequentialBox.Name = "sequentialBox";
			this.sequentialBox.Size = new System.Drawing.Size(461, 25);
			this.sequentialBox.StylizeImage = false;
			this.sequentialBox.TabIndex = 7;
			this.sequentialBox.Text = "Allow nonsequential name matching in Command Palettes";
			this.sequentialBox.ThemedBack = null;
			this.sequentialBox.ThemedFore = null;
			this.sequentialBox.UseVisualStyleBackColor = true;
			// 
			// themeBox
			// 
			this.themeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.themeBox.FormattingEnabled = true;
			this.themeBox.Items.AddRange(new object[] {
            "System",
            "Light",
            "Dark"});
			this.themeBox.Location = new System.Drawing.Point(186, 20);
			this.themeBox.Name = "themeBox";
			this.themeBox.Size = new System.Drawing.Size(300, 28);
			this.themeBox.TabIndex = 6;
			// 
			// themeLabel
			// 
			this.themeLabel.AutoSize = true;
			this.themeLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.themeLabel.Location = new System.Drawing.Point(21, 23);
			this.themeLabel.Name = "themeLabel";
			this.themeLabel.Size = new System.Drawing.Size(58, 20);
			this.themeLabel.TabIndex = 5;
			this.themeLabel.Text = "Theme";
			// 
			// advancedGroup
			// 
			this.advancedGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.advancedGroup.BorderThickness = 3;
			this.advancedGroup.Controls.Add(this.experimentalBox);
			this.advancedGroup.Controls.Add(this.verboseBox);
			this.advancedGroup.ForeColor = System.Drawing.SystemColors.ControlText;
			this.advancedGroup.Location = new System.Drawing.Point(7, 289);
			this.advancedGroup.Name = "advancedGroup";
			this.advancedGroup.Padding = new System.Windows.Forms.Padding(15, 10, 10, 10);
			this.advancedGroup.ShowOnlyTopEdge = true;
			this.advancedGroup.Size = new System.Drawing.Size(762, 124);
			this.advancedGroup.TabIndex = 4;
			this.advancedGroup.TabStop = false;
			this.advancedGroup.Text = "Advanced Options";
			this.advancedGroup.ThemedBorder = null;
			this.advancedGroup.ThemedFore = null;
			// 
			// experimentalBox
			// 
			this.experimentalBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.experimentalBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.experimentalBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.experimentalBox.Location = new System.Drawing.Point(18, 62);
			this.experimentalBox.Name = "experimentalBox";
			this.experimentalBox.Size = new System.Drawing.Size(250, 25);
			this.experimentalBox.StylizeImage = false;
			this.experimentalBox.TabIndex = 1;
			this.experimentalBox.Text = "Enable experimental features";
			this.experimentalBox.ThemedBack = null;
			this.experimentalBox.ThemedFore = null;
			this.experimentalBox.UseVisualStyleBackColor = true;
			// 
			// verboseBox
			// 
			this.verboseBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.verboseBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.verboseBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.verboseBox.Location = new System.Drawing.Point(18, 32);
			this.verboseBox.Name = "verboseBox";
			this.verboseBox.Size = new System.Drawing.Size(208, 25);
			this.verboseBox.StylizeImage = false;
			this.verboseBox.TabIndex = 0;
			this.verboseBox.Text = "Enable verbose logging";
			this.verboseBox.ThemedBack = null;
			this.verboseBox.ThemedFore = null;
			this.verboseBox.UseVisualStyleBackColor = true;
			// 
			// langBox
			// 
			this.langBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.langBox.FormattingEnabled = true;
			this.langBox.Location = new System.Drawing.Point(186, 83);
			this.langBox.Name = "langBox";
			this.langBox.Size = new System.Drawing.Size(300, 28);
			this.langBox.TabIndex = 3;
			// 
			// langLabel
			// 
			this.langLabel.AutoSize = true;
			this.langLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.langLabel.Location = new System.Drawing.Point(21, 86);
			this.langLabel.Name = "langLabel";
			this.langLabel.Size = new System.Drawing.Size(81, 20);
			this.langLabel.TabIndex = 2;
			this.langLabel.Text = "Language";
			// 
			// checkUpdatesBox
			// 
			this.checkUpdatesBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.checkUpdatesBox.Checked = true;
			this.checkUpdatesBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkUpdatesBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.checkUpdatesBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.checkUpdatesBox.Location = new System.Drawing.Point(25, 194);
			this.checkUpdatesBox.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
			this.checkUpdatesBox.Name = "checkUpdatesBox";
			this.checkUpdatesBox.Size = new System.Drawing.Size(456, 25);
			this.checkUpdatesBox.StylizeImage = false;
			this.checkUpdatesBox.TabIndex = 1;
			this.checkUpdatesBox.Text = "Check for new versions of OneMore when OneNote starts";
			this.checkUpdatesBox.ThemedBack = null;
			this.checkUpdatesBox.ThemedFore = null;
			this.checkUpdatesBox.UseVisualStyleBackColor = true;
			// 
			// GeneralSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.layoutPanel);
			this.Controls.Add(this.introBox);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.Name = "GeneralSheet";
			this.Padding = new System.Windows.Forms.Padding(13, 8, 15, 10);
			this.Size = new System.Drawing.Size(800, 500);
			this.layoutPanel.ResumeLayout(false);
			this.layoutPanel.PerformLayout();
			this.advancedGroup.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreMultilineLabel introBox;
		private System.Windows.Forms.Panel layoutPanel;
		private UI.MoreCheckBox checkUpdatesBox;
		private System.Windows.Forms.ComboBox langBox;
		private System.Windows.Forms.Label langLabel;
		private UI.MoreGroupBox advancedGroup;
		private UI.MoreCheckBox verboseBox;
		private UI.MoreCheckBox experimentalBox;
		private System.Windows.Forms.ComboBox themeBox;
		private System.Windows.Forms.Label themeLabel;
		private UI.MoreCheckBox sequentialBox;
	}
}
