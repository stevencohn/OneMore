
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
			this.introBox = new System.Windows.Forms.TextBox();
			this.layoutPanel = new System.Windows.Forms.Panel();
			this.advancedGroup = new System.Windows.Forms.GroupBox();
			this.verboseBox = new System.Windows.Forms.CheckBox();
			this.langBox = new System.Windows.Forms.ComboBox();
			this.langLabel = new System.Windows.Forms.Label();
			this.checkUpdatesBox = new System.Windows.Forms.CheckBox();
			this.experimentalBox = new System.Windows.Forms.CheckBox();
			this.layoutPanel.SuspendLayout();
			this.advancedGroup.SuspendLayout();
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
			this.introBox.Text = "Customize the overall behavior of OneMore";
			// 
			// layoutPanel
			// 
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
			// advancedGroup
			// 
			this.advancedGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.advancedGroup.Controls.Add(this.experimentalBox);
			this.advancedGroup.Controls.Add(this.verboseBox);
			this.advancedGroup.Location = new System.Drawing.Point(7, 274);
			this.advancedGroup.Name = "advancedGroup";
			this.advancedGroup.Padding = new System.Windows.Forms.Padding(15, 10, 10, 10);
			this.advancedGroup.Size = new System.Drawing.Size(762, 139);
			this.advancedGroup.TabIndex = 4;
			this.advancedGroup.TabStop = false;
			this.advancedGroup.Text = "Advanced Options";
			// 
			// verboseBox
			// 
			this.verboseBox.AutoSize = true;
			this.verboseBox.Location = new System.Drawing.Point(18, 32);
			this.verboseBox.Name = "verboseBox";
			this.verboseBox.Size = new System.Drawing.Size(200, 24);
			this.verboseBox.TabIndex = 0;
			this.verboseBox.Text = "Enable verbose logging";
			this.verboseBox.UseVisualStyleBackColor = true;
			// 
			// langBox
			// 
			this.langBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.langBox.FormattingEnabled = true;
			this.langBox.Location = new System.Drawing.Point(25, 101);
			this.langBox.Name = "langBox";
			this.langBox.Size = new System.Drawing.Size(345, 28);
			this.langBox.TabIndex = 3;
			// 
			// langLabel
			// 
			this.langLabel.AutoSize = true;
			this.langLabel.Location = new System.Drawing.Point(21, 78);
			this.langLabel.Name = "langLabel";
			this.langLabel.Size = new System.Drawing.Size(297, 20);
			this.langLabel.TabIndex = 2;
			this.langLabel.Text = "Display language (must restart OneNote)";
			// 
			// checkUpdatesBox
			// 
			this.checkUpdatesBox.Checked = true;
			this.checkUpdatesBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkUpdatesBox.Location = new System.Drawing.Point(25, 0);
			this.checkUpdatesBox.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
			this.checkUpdatesBox.Name = "checkUpdatesBox";
			this.checkUpdatesBox.Size = new System.Drawing.Size(744, 43);
			this.checkUpdatesBox.TabIndex = 1;
			this.checkUpdatesBox.Text = "Check for new versions of OneMore when OneNote starts";
			this.checkUpdatesBox.UseVisualStyleBackColor = true;
			// 
			// experimentalBox
			// 
			this.experimentalBox.AutoSize = true;
			this.experimentalBox.Location = new System.Drawing.Point(18, 62);
			this.experimentalBox.Name = "experimentalBox";
			this.experimentalBox.Size = new System.Drawing.Size(242, 24);
			this.experimentalBox.TabIndex = 1;
			this.experimentalBox.Text = "Enable experimental features";
			this.experimentalBox.UseVisualStyleBackColor = true;
			// 
			// GeneralSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.layoutPanel);
			this.Controls.Add(this.introBox);
			this.Name = "GeneralSheet";
			this.Padding = new System.Windows.Forms.Padding(13, 8, 15, 10);
			this.Size = new System.Drawing.Size(800, 500);
			this.layoutPanel.ResumeLayout(false);
			this.layoutPanel.PerformLayout();
			this.advancedGroup.ResumeLayout(false);
			this.advancedGroup.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox introBox;
		private System.Windows.Forms.Panel layoutPanel;
		private System.Windows.Forms.CheckBox checkUpdatesBox;
		private System.Windows.Forms.ComboBox langBox;
		private System.Windows.Forms.Label langLabel;
		private System.Windows.Forms.GroupBox advancedGroup;
		private System.Windows.Forms.CheckBox verboseBox;
		private System.Windows.Forms.CheckBox experimentalBox;
	}
}
