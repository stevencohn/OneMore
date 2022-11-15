using System.Drawing;

namespace OneMoreCalendar
{
	partial class SettingsForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.optionsLabel = new System.Windows.Forms.Label();
			this.notebooksLabel = new System.Windows.Forms.Label();
			this.themeLabel = new System.Windows.Forms.Label();
			this.settingsPanel = new System.Windows.Forms.Panel();
			this.userModeButton = new OneMoreCalendar.MoreRadioButton();
			this.emptyBox = new OneMoreCalendar.MoreCheckBox();
			this.deletedBox = new OneMoreCalendar.MoreCheckBox();
			this.okButton = new OneMoreCalendar.MoreButton();
			this.cancelButton = new OneMoreCalendar.MoreButton();
			this.notebooksBox = new OneMoreCalendar.MoreCheckedListBox();
			this.createdBox = new OneMoreCalendar.MoreCheckBox();
			this.modifiedBox = new OneMoreCalendar.MoreCheckBox();
			this.darkModeButton = new OneMoreCalendar.MoreRadioButton();
			this.lightModeButton = new OneMoreCalendar.MoreRadioButton();
			this.systemModeButton = new OneMoreCalendar.MoreRadioButton();
			this.logLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.aboutLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.settingsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// optionsLabel
			// 
			this.optionsLabel.AutoSize = true;
			this.optionsLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.optionsLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(115)))), ((int)(((byte)(53)))), ((int)(((byte)(110)))));
			this.optionsLabel.Location = new System.Drawing.Point(23, 15);
			this.optionsLabel.Name = "optionsLabel";
			this.optionsLabel.Size = new System.Drawing.Size(98, 32);
			this.optionsLabel.TabIndex = 0;
			this.optionsLabel.Text = "Options";
			// 
			// notebooksLabel
			// 
			this.notebooksLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.notebooksLabel.AutoSize = true;
			this.notebooksLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.notebooksLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(115)))), ((int)(((byte)(53)))), ((int)(((byte)(110)))));
			this.notebooksLabel.Location = new System.Drawing.Point(23, 354);
			this.notebooksLabel.Name = "notebooksLabel";
			this.notebooksLabel.Size = new System.Drawing.Size(131, 32);
			this.notebooksLabel.TabIndex = 1;
			this.notebooksLabel.Text = "Notebooks";
			// 
			// themeLabel
			// 
			this.themeLabel.AutoSize = true;
			this.themeLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.themeLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(115)))), ((int)(((byte)(53)))), ((int)(((byte)(110)))));
			this.themeLabel.Location = new System.Drawing.Point(22, 190);
			this.themeLabel.Name = "themeLabel";
			this.themeLabel.Size = new System.Drawing.Size(88, 32);
			this.themeLabel.TabIndex = 1;
			this.themeLabel.Text = "Theme";
			// 
			// settingsPanel
			// 
			this.settingsPanel.BackColor = System.Drawing.Color.Transparent;
			this.settingsPanel.Controls.Add(this.userModeButton);
			this.settingsPanel.Controls.Add(this.emptyBox);
			this.settingsPanel.Controls.Add(this.deletedBox);
			this.settingsPanel.Controls.Add(this.okButton);
			this.settingsPanel.Controls.Add(this.cancelButton);
			this.settingsPanel.Controls.Add(this.notebooksBox);
			this.settingsPanel.Controls.Add(this.createdBox);
			this.settingsPanel.Controls.Add(this.modifiedBox);
			this.settingsPanel.Controls.Add(this.notebooksLabel);
			this.settingsPanel.Controls.Add(this.optionsLabel);
			this.settingsPanel.Controls.Add(this.themeLabel);
			this.settingsPanel.Controls.Add(this.darkModeButton);
			this.settingsPanel.Controls.Add(this.lightModeButton);
			this.settingsPanel.Controls.Add(this.systemModeButton);
			this.settingsPanel.Controls.Add(this.logLink);
			this.settingsPanel.Controls.Add(this.aboutLink);
			this.settingsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.settingsPanel.Location = new System.Drawing.Point(4, 4);
			this.settingsPanel.Margin = new System.Windows.Forms.Padding(0);
			this.settingsPanel.Name = "settingsPanel";
			this.settingsPanel.Padding = new System.Windows.Forms.Padding(20, 20, 10, 10);
			this.settingsPanel.Size = new System.Drawing.Size(507, 598);
			this.settingsPanel.TabIndex = 2;
			// 
			// userModeButton
			// 
			this.userModeButton.AutoSize = true;
			this.userModeButton.BackColor = System.Drawing.Color.White;
			this.userModeButton.Location = new System.Drawing.Point(48, 321);
			this.userModeButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 2);
			this.userModeButton.Name = "userModeButton";
			this.userModeButton.Round = true;
			this.userModeButton.Size = new System.Drawing.Size(89, 24);
			this.userModeButton.TabIndex = 15;
			this.userModeButton.Text = "Custom";
			this.userModeButton.UseVisualStyleBackColor = false;
			// 
			// emptyBox
			// 
			this.emptyBox.AutoSize = true;
			this.emptyBox.BackColor = System.Drawing.Color.White;
			this.emptyBox.Location = new System.Drawing.Point(48, 153);
			this.emptyBox.Name = "emptyBox";
			this.emptyBox.Size = new System.Drawing.Size(244, 24);
			this.emptyBox.TabIndex = 11;
			this.emptyBox.Text = "Hide empty days in detail view";
			this.emptyBox.UseVisualStyleBackColor = false;
			// 
			// deletedBox
			// 
			this.deletedBox.AutoSize = true;
			this.deletedBox.BackColor = System.Drawing.Color.White;
			this.deletedBox.Location = new System.Drawing.Point(48, 123);
			this.deletedBox.Name = "deletedBox";
			this.deletedBox.Size = new System.Drawing.Size(201, 24);
			this.deletedBox.TabIndex = 10;
			this.deletedBox.Text = "Included deleted pages";
			this.deletedBox.UseVisualStyleBackColor = false;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.SystemColors.Window;
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(338, 551);
			this.okButton.Name = "okButton";
			this.okButton.PreferredBack = System.Drawing.Color.Empty;
			this.okButton.PreferredFore = System.Drawing.Color.Empty;
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(75, 34);
			this.okButton.TabIndex = 9;
			this.okButton.Text = "Apply";
			this.okButton.UseVisualStyleBackColor = false;
			this.okButton.Click += new System.EventHandler(this.Apply);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.SystemColors.Window;
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(419, 551);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.PreferredBack = System.Drawing.Color.Empty;
			this.cancelButton.PreferredFore = System.Drawing.Color.Empty;
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(75, 34);
			this.cancelButton.TabIndex = 8;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = false;
			this.cancelButton.Click += new System.EventHandler(this.Cancel);
			// 
			// notebooksBox
			// 
			this.notebooksBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.notebooksBox.BackColor = System.Drawing.SystemColors.Window;
			this.notebooksBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.notebooksBox.CheckOnClick = true;
			this.notebooksBox.FormattingEnabled = true;
			this.notebooksBox.Location = new System.Drawing.Point(48, 398);
			this.notebooksBox.Name = "notebooksBox";
			this.notebooksBox.Size = new System.Drawing.Size(446, 138);
			this.notebooksBox.TabIndex = 7;
			this.notebooksBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ValidateCheckedItems);
			// 
			// createdBox
			// 
			this.createdBox.AutoSize = true;
			this.createdBox.BackColor = System.Drawing.Color.White;
			this.createdBox.Location = new System.Drawing.Point(48, 63);
			this.createdBox.Name = "createdBox";
			this.createdBox.Size = new System.Drawing.Size(114, 24);
			this.createdBox.TabIndex = 5;
			this.createdBox.Text = "Created on";
			this.createdBox.UseVisualStyleBackColor = false;
			this.createdBox.CheckedChanged += new System.EventHandler(this.ChangeFilter);
			// 
			// modifiedBox
			// 
			this.modifiedBox.AutoSize = true;
			this.modifiedBox.BackColor = System.Drawing.Color.White;
			this.modifiedBox.Checked = true;
			this.modifiedBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.modifiedBox.Location = new System.Drawing.Point(48, 93);
			this.modifiedBox.Name = "modifiedBox";
			this.modifiedBox.Size = new System.Drawing.Size(152, 24);
			this.modifiedBox.TabIndex = 6;
			this.modifiedBox.Text = "Last modified on";
			this.modifiedBox.UseVisualStyleBackColor = false;
			this.modifiedBox.CheckedChanged += new System.EventHandler(this.ChangeFilter);
			// 
			// darkModeButton
			// 
			this.darkModeButton.AutoSize = true;
			this.darkModeButton.BackColor = System.Drawing.Color.White;
			this.darkModeButton.Location = new System.Drawing.Point(48, 234);
			this.darkModeButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 2);
			this.darkModeButton.Name = "darkModeButton";
			this.darkModeButton.Round = true;
			this.darkModeButton.Size = new System.Drawing.Size(68, 24);
			this.darkModeButton.TabIndex = 12;
			this.darkModeButton.Text = "Dark";
			this.darkModeButton.UseVisualStyleBackColor = false;
			// 
			// lightModeButton
			// 
			this.lightModeButton.AutoSize = true;
			this.lightModeButton.BackColor = System.Drawing.Color.White;
			this.lightModeButton.Location = new System.Drawing.Point(48, 263);
			this.lightModeButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 2);
			this.lightModeButton.Name = "lightModeButton";
			this.lightModeButton.Round = true;
			this.lightModeButton.Size = new System.Drawing.Size(69, 24);
			this.lightModeButton.TabIndex = 13;
			this.lightModeButton.Text = "Light";
			this.lightModeButton.UseVisualStyleBackColor = false;
			// 
			// systemModeButton
			// 
			this.systemModeButton.AutoSize = true;
			this.systemModeButton.BackColor = System.Drawing.Color.White;
			this.systemModeButton.Checked = true;
			this.systemModeButton.Location = new System.Drawing.Point(48, 292);
			this.systemModeButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 2);
			this.systemModeButton.Name = "systemModeButton";
			this.systemModeButton.Round = true;
			this.systemModeButton.Size = new System.Drawing.Size(87, 24);
			this.systemModeButton.TabIndex = 14;
			this.systemModeButton.TabStop = true;
			this.systemModeButton.Text = "System";
			this.systemModeButton.UseVisualStyleBackColor = false;
			// 
			// logLink
			// 
			this.logLink.ActiveLinkColor = System.Drawing.Color.DarkOrchid;
			this.logLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.logLink.AutoSize = true;
			this.logLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.logLink.HoverColor = System.Drawing.Color.MediumOrchid;
			this.logLink.LinkColor = System.Drawing.SystemColors.ControlDark;
			this.logLink.Location = new System.Drawing.Point(24, 558);
			this.logLink.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.logLink.MaximumSize = new System.Drawing.Size(420, 0);
			this.logLink.Name = "logLink";
			this.logLink.Size = new System.Drawing.Size(97, 20);
			this.logLink.TabIndex = 5;
			this.logLink.TabStop = true;
			this.logLink.Text = "Open log file";
			this.logLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OpenLog);
			// 
			// aboutLink
			// 
			this.aboutLink.ActiveLinkColor = System.Drawing.Color.DarkOrchid;
			this.aboutLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.aboutLink.AutoSize = true;
			this.aboutLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.aboutLink.HoverColor = System.Drawing.Color.MediumOrchid;
			this.aboutLink.LinkColor = System.Drawing.SystemColors.ControlDark;
			this.aboutLink.Location = new System.Drawing.Point(144, 558);
			this.aboutLink.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.aboutLink.MaximumSize = new System.Drawing.Size(420, 0);
			this.aboutLink.Name = "aboutLink";
			this.aboutLink.Size = new System.Drawing.Size(52, 20);
			this.aboutLink.TabIndex = 5;
			this.aboutLink.TabStop = true;
			this.aboutLink.Text = "About";
			this.aboutLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ShowAbout);
			// 
			// SettingsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(515, 606);
			this.Controls.Add(this.settingsPanel);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SettingsForm";
			this.Padding = new System.Windows.Forms.Padding(4);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.TopMost = true;
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SettingsForm_KeyDown);
			this.settingsPanel.ResumeLayout(false);
			this.settingsPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label optionsLabel;
		private System.Windows.Forms.Label notebooksLabel;
		private System.Windows.Forms.Label themeLabel;
		private System.Windows.Forms.Panel settingsPanel;
		private MoreCheckBox createdBox;
		private MoreCheckBox modifiedBox;
		private MoreCheckedListBox notebooksBox;
		private MoreButton okButton;
		private MoreButton cancelButton;
		private MoreCheckBox deletedBox;
		private MoreRadioButton lightModeButton;
		private MoreRadioButton darkModeButton;
		private MoreRadioButton systemModeButton;
		private River.OneMoreAddIn.UI.MoreLinkLabel logLink;
		private River.OneMoreAddIn.UI.MoreLinkLabel aboutLink;
		private MoreCheckBox emptyBox;
		private MoreRadioButton userModeButton;
	}
}