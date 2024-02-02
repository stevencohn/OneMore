
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
			this.introBox = new River.OneMoreAddIn.UI.MoreMultilineLabel();
			this.layoutPanel = new System.Windows.Forms.Panel();
			this.barLabel = new System.Windows.Forms.Label();
			this.allLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.noneLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.langView = new System.Windows.Forms.ListView();
			this.enabledLabel = new System.Windows.Forms.Label();
			this.family2Box = new River.OneMoreAddIn.UI.FontComboBox();
			this.size2Box = new System.Windows.Forms.ComboBox();
			this.font2Label = new System.Windows.Forms.Label();
			this.fixedBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.familyBox = new River.OneMoreAddIn.UI.FontComboBox();
			this.sizeBox = new System.Windows.Forms.ComboBox();
			this.fontLabel = new System.Windows.Forms.Label();
			this.applyBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.layoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// introBox
			// 
			this.introBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.introBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.introBox.Location = new System.Drawing.Point(13, 8);
			this.introBox.Name = "introBox";
			this.introBox.ThemedBack = "ControlLightLight";
			this.introBox.ThemedFore = null;
			this.introBox.Size = new System.Drawing.Size(772, 66);
			this.introBox.TabIndex = 3;
			this.introBox.Text = "Customize the behavior of the Colorize command. Secondary font applies when colla" +
    "psing PlantUML";
			// 
			// layoutPanel
			// 
			this.layoutPanel.Controls.Add(this.barLabel);
			this.layoutPanel.Controls.Add(this.allLink);
			this.layoutPanel.Controls.Add(this.noneLink);
			this.layoutPanel.Controls.Add(this.langView);
			this.layoutPanel.Controls.Add(this.enabledLabel);
			this.layoutPanel.Controls.Add(this.family2Box);
			this.layoutPanel.Controls.Add(this.size2Box);
			this.layoutPanel.Controls.Add(this.font2Label);
			this.layoutPanel.Controls.Add(this.fixedBox);
			this.layoutPanel.Controls.Add(this.familyBox);
			this.layoutPanel.Controls.Add(this.sizeBox);
			this.layoutPanel.Controls.Add(this.fontLabel);
			this.layoutPanel.Controls.Add(this.applyBox);
			this.layoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.layoutPanel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.layoutPanel.Location = new System.Drawing.Point(13, 74);
			this.layoutPanel.Margin = new System.Windows.Forms.Padding(0);
			this.layoutPanel.Name = "layoutPanel";
			this.layoutPanel.Size = new System.Drawing.Size(772, 416);
			this.layoutPanel.TabIndex = 4;
			// 
			// barLabel
			// 
			this.barLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.barLabel.AutoSize = true;
			this.barLabel.Location = new System.Drawing.Point(533, 217);
			this.barLabel.Name = "barLabel";
			this.barLabel.Size = new System.Drawing.Size(14, 20);
			this.barLabel.TabIndex = 21;
			this.barLabel.Text = "|";
			// 
			// allLink
			// 
			this.allLink.ActiveLinkColor = System.Drawing.Color.MediumOrchid;
			this.allLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.allLink.AutoSize = true;
			this.allLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.allLink.HoverColor = System.Drawing.Color.Orchid;
			this.allLink.LinkColor = System.Drawing.Color.MediumOrchid;
			this.allLink.Location = new System.Drawing.Point(501, 217);
			this.allLink.Name = "allLink";
			this.allLink.Size = new System.Drawing.Size(26, 20);
			this.allLink.TabIndex = 20;
			this.allLink.TabStop = true;
			this.allLink.Text = "All";
			this.allLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.allLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ToggleLanguages);
			// 
			// noneLink
			// 
			this.noneLink.ActiveLinkColor = System.Drawing.Color.MediumOrchid;
			this.noneLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.noneLink.AutoSize = true;
			this.noneLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.noneLink.HoverColor = System.Drawing.Color.Orchid;
			this.noneLink.LinkColor = System.Drawing.Color.MediumOrchid;
			this.noneLink.Location = new System.Drawing.Point(553, 217);
			this.noneLink.Name = "noneLink";
			this.noneLink.Size = new System.Drawing.Size(47, 20);
			this.noneLink.TabIndex = 19;
			this.noneLink.TabStop = true;
			this.noneLink.Text = "None";
			this.noneLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.noneLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ToggleLanguages);
			// 
			// langView
			// 
			this.langView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.langView.BackColor = System.Drawing.SystemColors.Window;
			this.langView.CheckBoxes = true;
			this.langView.HideSelection = false;
			this.langView.Location = new System.Drawing.Point(10, 240);
			this.langView.Name = "langView";
			this.langView.Size = new System.Drawing.Size(590, 173);
			this.langView.TabIndex = 18;
			this.langView.UseCompatibleStateImageBehavior = false;
			this.langView.View = System.Windows.Forms.View.SmallIcon;
			// 
			// enabledLabel
			// 
			this.enabledLabel.AutoSize = true;
			this.enabledLabel.Location = new System.Drawing.Point(6, 217);
			this.enabledLabel.Name = "enabledLabel";
			this.enabledLabel.Size = new System.Drawing.Size(146, 20);
			this.enabledLabel.TabIndex = 17;
			this.enabledLabel.Text = "Enabled languages";
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
			this.family2Box.Location = new System.Drawing.Point(135, 106);
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
			this.size2Box.Location = new System.Drawing.Point(496, 105);
			this.size2Box.Name = "size2Box";
			this.size2Box.Size = new System.Drawing.Size(104, 33);
			this.size2Box.TabIndex = 15;
			// 
			// font2Label
			// 
			this.font2Label.AutoSize = true;
			this.font2Label.Location = new System.Drawing.Point(6, 113);
			this.font2Label.Name = "font2Label";
			this.font2Label.Size = new System.Drawing.Size(85, 20);
			this.font2Label.TabIndex = 14;
			this.font2Label.Text = "Secondary";
			// 
			// fixedBox
			// 
			this.fixedBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.fixedBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.fixedBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.fixedBox.Location = new System.Drawing.Point(135, 167);
			this.fixedBox.Name = "fixedBox";
			this.fixedBox.Size = new System.Drawing.Size(235, 25);
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
			this.familyBox.Location = new System.Drawing.Point(135, 57);
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
			this.sizeBox.Location = new System.Drawing.Point(496, 57);
			this.sizeBox.Name = "sizeBox";
			this.sizeBox.Size = new System.Drawing.Size(104, 33);
			this.sizeBox.TabIndex = 2;
			// 
			// fontLabel
			// 
			this.fontLabel.AutoSize = true;
			this.fontLabel.Location = new System.Drawing.Point(6, 64);
			this.fontLabel.Name = "fontLabel";
			this.fontLabel.Size = new System.Drawing.Size(61, 20);
			this.fontLabel.TabIndex = 13;
			this.fontLabel.Text = "Primary";
			// 
			// applyBox
			// 
			this.applyBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.applyBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.applyBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.applyBox.Location = new System.Drawing.Point(3, 6);
			this.applyBox.Name = "applyBox";
			this.applyBox.Size = new System.Drawing.Size(426, 25);
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
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.Name = "ColorizerSheet";
			this.Padding = new System.Windows.Forms.Padding(13, 8, 15, 10);
			this.Size = new System.Drawing.Size(800, 500);
			this.layoutPanel.ResumeLayout(false);
			this.layoutPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreMultilineLabel introBox;
		private System.Windows.Forms.Panel layoutPanel;
		private UI.MoreCheckBox applyBox;
		private UI.FontComboBox familyBox;
		private System.Windows.Forms.ComboBox sizeBox;
		private System.Windows.Forms.Label fontLabel;
		private UI.MoreCheckBox fixedBox;
		private System.Windows.Forms.ComboBox size2Box;
		private System.Windows.Forms.Label font2Label;
		private UI.FontComboBox family2Box;
		private System.Windows.Forms.ListView langView;
		private System.Windows.Forms.Label enabledLabel;
		private System.Windows.Forms.Label barLabel;
		private UI.MoreLinkLabel allLink;
		private UI.MoreLinkLabel noneLink;
	}
}
