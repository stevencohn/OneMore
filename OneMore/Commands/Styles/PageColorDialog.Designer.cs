﻿namespace River.OneMoreAddIn.Commands
{
	partial class PageColorDialog
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PageColorDialog));
			this.omBox = new River.OneMoreAddIn.UI.MorePictureBox();
			this.customBox = new River.OneMoreAddIn.UI.MorePictureBox();
			this.noLabel = new System.Windows.Forms.Label();
			this.noPanel = new System.Windows.Forms.Panel();
			this.cancelButton = new System.Windows.Forms.Button();
			this.expander = new River.OneMoreAddIn.UI.MoreExpander();
			this.optionsPanel = new System.Windows.Forms.Panel();
			this.loadThemeLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.currentThemeLabel = new System.Windows.Forms.Label();
			this.applyThemeBox = new System.Windows.Forms.CheckBox();
			this.okButton = new System.Windows.Forms.Button();
			this.omButton = new System.Windows.Forms.RadioButton();
			this.customButton = new System.Windows.Forms.RadioButton();
			this.noButton = new System.Windows.Forms.RadioButton();
			this.statusLabel = new System.Windows.Forms.Label();
			this.tooltip = new System.Windows.Forms.ToolTip(this.components);
			this.scopeBox = new System.Windows.Forms.ComboBox();
			this.detailPanel = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.omBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.customBox)).BeginInit();
			this.noPanel.SuspendLayout();
			this.optionsPanel.SuspendLayout();
			this.detailPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// omBox
			// 
			this.omBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.omBox.Location = new System.Drawing.Point(67, 21);
			this.omBox.Name = "omBox";
			this.omBox.Size = new System.Drawing.Size(550, 60);
			this.omBox.TabIndex = 0;
			this.omBox.TabStop = false;
			this.omBox.Click += new System.EventHandler(this.ChooseColor);
			// 
			// customBox
			// 
			this.customBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.customBox.Location = new System.Drawing.Point(67, 103);
			this.customBox.Name = "customBox";
			this.customBox.Size = new System.Drawing.Size(550, 60);
			this.customBox.TabIndex = 5;
			this.customBox.TabStop = false;
			this.customBox.Click += new System.EventHandler(this.ChooseCustomColor);
			// 
			// noLabel
			// 
			this.noLabel.AutoSize = true;
			this.noLabel.Location = new System.Drawing.Point(15, 19);
			this.noLabel.Name = "noLabel";
			this.noLabel.Size = new System.Drawing.Size(179, 20);
			this.noLabel.TabIndex = 6;
			this.noLabel.Text = "or choose no page color";
			this.noLabel.Click += new System.EventHandler(this.ChooseNoColor);
			// 
			// noPanel
			// 
			this.noPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.noPanel.Controls.Add(this.noLabel);
			this.noPanel.Location = new System.Drawing.Point(67, 185);
			this.noPanel.Name = "noPanel";
			this.noPanel.Size = new System.Drawing.Size(550, 60);
			this.noPanel.TabIndex = 8;
			this.noPanel.Click += new System.EventHandler(this.ChooseNoColor);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(497, 545);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(130, 40);
			this.cancelButton.TabIndex = 9;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// expander
			// 
			this.expander.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.expander.ContentControl = this.optionsPanel;
			this.expander.Cursor = System.Windows.Forms.Cursors.Hand;
			this.expander.Expanded = false;
			this.expander.ForeColor = System.Drawing.SystemColors.ActiveCaption;
			this.expander.Location = new System.Drawing.Point(23, 65);
			this.expander.MaxContentHeight = 130;
			this.expander.Name = "expander";
			this.expander.Size = new System.Drawing.Size(521, 44);
			this.expander.TabIndex = 10;
			this.expander.Title = "Options";
			// 
			// optionsPanel
			// 
			this.optionsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.optionsPanel.Controls.Add(this.loadThemeLink);
			this.optionsPanel.Controls.Add(this.currentThemeLabel);
			this.optionsPanel.Controls.Add(this.applyThemeBox);
			this.optionsPanel.Location = new System.Drawing.Point(44, 412);
			this.optionsPanel.Name = "optionsPanel";
			this.optionsPanel.Padding = new System.Windows.Forms.Padding(4);
			this.optionsPanel.Size = new System.Drawing.Size(583, 122);
			this.optionsPanel.TabIndex = 11;
			// 
			// loadThemeLink
			// 
			this.loadThemeLink.AutoSize = true;
			this.loadThemeLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.loadThemeLink.HoverColor = System.Drawing.Color.MediumOrchid;
			this.loadThemeLink.Location = new System.Drawing.Point(50, 37);
			this.loadThemeLink.Name = "loadThemeLink";
			this.loadThemeLink.Size = new System.Drawing.Size(152, 20);
			this.loadThemeLink.TabIndex = 2;
			this.loadThemeLink.TabStop = true;
			this.loadThemeLink.Text = "Load different styles";
			this.loadThemeLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LoadTheme);
			// 
			// currentThemeLabel
			// 
			this.currentThemeLabel.AutoSize = true;
			this.currentThemeLabel.Location = new System.Drawing.Point(50, 4);
			this.currentThemeLabel.Name = "currentThemeLabel";
			this.currentThemeLabel.Size = new System.Drawing.Size(213, 20);
			this.currentThemeLabel.TabIndex = 1;
			this.currentThemeLabel.Text = "The current style theme is {0}";
			// 
			// applyThemeBox
			// 
			this.applyThemeBox.AutoSize = true;
			this.applyThemeBox.Location = new System.Drawing.Point(51, 82);
			this.applyThemeBox.Name = "applyThemeBox";
			this.applyThemeBox.Size = new System.Drawing.Size(205, 24);
			this.applyThemeBox.TabIndex = 0;
			this.applyThemeBox.Text = "Apply styles to this page";
			this.applyThemeBox.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(361, 545);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(130, 40);
			this.okButton.TabIndex = 12;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.Apply);
			// 
			// omButton
			// 
			this.omButton.AutoSize = true;
			this.omButton.Location = new System.Drawing.Point(33, 39);
			this.omButton.Name = "omButton";
			this.omButton.Size = new System.Drawing.Size(21, 20);
			this.omButton.TabIndex = 13;
			this.omButton.TabStop = true;
			this.omButton.UseVisualStyleBackColor = true;
			this.omButton.CheckedChanged += new System.EventHandler(this.ChangeColorOption);
			// 
			// customButton
			// 
			this.customButton.AutoSize = true;
			this.customButton.Location = new System.Drawing.Point(33, 117);
			this.customButton.Name = "customButton";
			this.customButton.Size = new System.Drawing.Size(21, 20);
			this.customButton.TabIndex = 14;
			this.customButton.TabStop = true;
			this.customButton.UseVisualStyleBackColor = true;
			// 
			// noButton
			// 
			this.noButton.AutoSize = true;
			this.noButton.Location = new System.Drawing.Point(33, 202);
			this.noButton.Name = "noButton";
			this.noButton.Size = new System.Drawing.Size(21, 20);
			this.noButton.TabIndex = 15;
			this.noButton.TabStop = true;
			this.noButton.UseVisualStyleBackColor = true;
			// 
			// statusLabel
			// 
			this.statusLabel.AutoSize = true;
			this.statusLabel.ForeColor = System.Drawing.Color.Brown;
			this.statusLabel.Location = new System.Drawing.Point(110, 262);
			this.statusLabel.Name = "statusLabel";
			this.statusLabel.Size = new System.Drawing.Size(31, 20);
			this.statusLabel.TabIndex = 16;
			this.statusLabel.Text = "OK";
			// 
			// scopeBox
			// 
			this.scopeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.scopeBox.FormattingEnabled = true;
			this.scopeBox.Items.AddRange(new object[] {
            "Apply to this page",
            "Apply to pages in this section",
            "Apply to pages in this notebook"});
			this.scopeBox.Location = new System.Drawing.Point(23, 3);
			this.scopeBox.Name = "scopeBox";
			this.scopeBox.Size = new System.Drawing.Size(352, 28);
			this.scopeBox.TabIndex = 17;
			// 
			// detailPanel
			// 
			this.detailPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.detailPanel.Controls.Add(this.scopeBox);
			this.detailPanel.Controls.Add(this.expander);
			this.detailPanel.Location = new System.Drawing.Point(44, 294);
			this.detailPanel.Name = "detailPanel";
			this.detailPanel.Size = new System.Drawing.Size(583, 112);
			this.detailPanel.TabIndex = 18;
			// 
			// PageColorDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(640, 598);
			this.Controls.Add(this.detailPanel);
			this.Controls.Add(this.statusLabel);
			this.Controls.Add(this.optionsPanel);
			this.Controls.Add(this.noButton);
			this.Controls.Add(this.customButton);
			this.Controls.Add(this.omButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.noPanel);
			this.Controls.Add(this.omBox);
			this.Controls.Add(this.customBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "PageColorDialog";
			this.Padding = new System.Windows.Forms.Padding(15, 15, 10, 10);
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Page Color";
			((System.ComponentModel.ISupportInitialize)(this.omBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.customBox)).EndInit();
			this.noPanel.ResumeLayout(false);
			this.noPanel.PerformLayout();
			this.optionsPanel.ResumeLayout(false);
			this.optionsPanel.PerformLayout();
			this.detailPanel.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private River.OneMoreAddIn.UI.MorePictureBox omBox;
		private River.OneMoreAddIn.UI.MorePictureBox customBox;
		private System.Windows.Forms.Label noLabel;
		private System.Windows.Forms.Panel noPanel;
		private System.Windows.Forms.Button cancelButton;
		private UI.MoreExpander expander;
		private System.Windows.Forms.Panel optionsPanel;
		private UI.MoreLinkLabel loadThemeLink;
		private System.Windows.Forms.Label currentThemeLabel;
		private System.Windows.Forms.CheckBox applyThemeBox;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.RadioButton omButton;
		private System.Windows.Forms.RadioButton customButton;
		private System.Windows.Forms.RadioButton noButton;
		private System.Windows.Forms.Label statusLabel;
		private System.Windows.Forms.ToolTip tooltip;
		private System.Windows.Forms.ComboBox scopeBox;
		private System.Windows.Forms.Panel detailPanel;
	}
}