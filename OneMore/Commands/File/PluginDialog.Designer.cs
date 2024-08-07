namespace River.OneMoreAddIn.Commands
{
	partial class PluginDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PluginDialog));
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.runButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cmdLabel = new System.Windows.Forms.Label();
			this.cmdBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.browseButton = new River.OneMoreAddIn.UI.MoreButton();
			this.updateRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.createRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.pageNameBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.childBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.argsBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.argsLabel = new System.Windows.Forms.Label();
			this.browseArgsButton = new River.OneMoreAddIn.UI.MoreButton();
			this.saveButton = new River.OneMoreAddIn.UI.MoreButton();
			this.pluginsLabel = new System.Windows.Forms.Label();
			this.pluginsBox = new System.Windows.Forms.ComboBox();
			this.nameLabel = new System.Windows.Forms.Label();
			this.nameBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.errorBox = new System.Windows.Forms.PictureBox();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.timeoutBox = new System.Windows.Forms.NumericUpDown();
			this.timeoutLabel = new System.Windows.Forms.Label();
			this.targetBox = new System.Windows.Forms.ComboBox();
			this.targetLabel = new System.Windows.Forms.Label();
			this.skipLockRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.failLockRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.pageGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.sectionGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.trialBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.userArgsLabel = new System.Windows.Forms.Label();
			this.userArgsBox = new River.OneMoreAddIn.UI.MoreTextBox();
			((System.ComponentModel.ISupportInitialize)(this.errorBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.timeoutBox)).BeginInit();
			this.pageGroup.SuspendLayout();
			this.sectionGroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(654, 569);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(120, 35);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 12;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.runButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.runButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.runButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.runButton.Enabled = false;
			this.runButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.runButton.ImageOver = null;
			this.runButton.Location = new System.Drawing.Point(528, 569);
			this.runButton.Name = "okButton";
			this.runButton.ShowBorder = true;
			this.runButton.Size = new System.Drawing.Size(120, 35);
			this.runButton.StylizeImage = false;
			this.runButton.TabIndex = 11;
			this.runButton.Text = "Run";
			this.runButton.ThemedBack = null;
			this.runButton.ThemedFore = null;
			this.runButton.UseVisualStyleBackColor = true;
			this.runButton.Click += new System.EventHandler(this.OK);
			// 
			// cmdLabel
			// 
			this.cmdLabel.AutoSize = true;
			this.cmdLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.cmdLabel.Location = new System.Drawing.Point(18, 127);
			this.cmdLabel.Name = "cmdLabel";
			this.cmdLabel.Size = new System.Drawing.Size(82, 20);
			this.cmdLabel.TabIndex = 2;
			this.cmdLabel.Text = "Command";
			// 
			// cmdBox
			// 
			this.cmdBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.cmdBox.Location = new System.Drawing.Point(149, 122);
			this.cmdBox.Name = "cmdBox";
			this.cmdBox.ProcessEnterKey = false;
			this.cmdBox.Size = new System.Drawing.Size(578, 26);
			this.cmdBox.TabIndex = 2;
			this.cmdBox.ThemedBack = null;
			this.cmdBox.ThemedFore = null;
			this.cmdBox.TextChanged += new System.EventHandler(this.ChangeText);
			// 
			// browseButton
			// 
			this.browseButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.browseButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.browseButton.ImageOver = null;
			this.browseButton.Location = new System.Drawing.Point(733, 122);
			this.browseButton.Name = "browseButton";
			this.browseButton.ShowBorder = true;
			this.browseButton.Size = new System.Drawing.Size(36, 31);
			this.browseButton.StylizeImage = false;
			this.browseButton.TabIndex = 3;
			this.browseButton.Text = "...";
			this.browseButton.ThemedBack = null;
			this.browseButton.ThemedFore = null;
			this.browseButton.UseVisualStyleBackColor = true;
			this.browseButton.Click += new System.EventHandler(this.BrowsePath);
			// 
			// updateRadio
			// 
			this.updateRadio.Checked = true;
			this.updateRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.updateRadio.Location = new System.Drawing.Point(6, 25);
			this.updateRadio.Name = "updateRadio";
			this.updateRadio.Size = new System.Drawing.Size(185, 25);
			this.updateRadio.TabIndex = 0;
			this.updateRadio.TabStop = true;
			this.updateRadio.Text = "Update current page";
			this.updateRadio.UseVisualStyleBackColor = true;
			this.updateRadio.CheckedChanged += new System.EventHandler(this.updateRadio_CheckedChanged);
			// 
			// createRadio
			// 
			this.createRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.createRadio.Location = new System.Drawing.Point(6, 55);
			this.createRadio.Name = "createRadio";
			this.createRadio.Size = new System.Drawing.Size(229, 25);
			this.createRadio.TabIndex = 1;
			this.createRadio.Text = "Create a new page named";
			this.createRadio.UseVisualStyleBackColor = true;
			// 
			// pageNameBox
			// 
			this.pageNameBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pageNameBox.Enabled = false;
			this.pageNameBox.Location = new System.Drawing.Point(31, 85);
			this.pageNameBox.Name = "pageNameBox";
			this.pageNameBox.ProcessEnterKey = false;
			this.pageNameBox.Size = new System.Drawing.Size(469, 26);
			this.pageNameBox.TabIndex = 2;
			this.pageNameBox.ThemedBack = null;
			this.pageNameBox.ThemedFore = null;
			this.pageNameBox.TextChanged += new System.EventHandler(this.ChangeText);
			// 
			// childBox
			// 
			this.childBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.childBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.childBox.Enabled = false;
			this.childBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.childBox.Location = new System.Drawing.Point(31, 117);
			this.childBox.Name = "childBox";
			this.childBox.Size = new System.Drawing.Size(206, 25);
			this.childBox.StylizeImage = false;
			this.childBox.TabIndex = 3;
			this.childBox.Text = "as child of current page";
			this.childBox.ThemedBack = null;
			this.childBox.ThemedFore = null;
			this.childBox.UseVisualStyleBackColor = true;
			this.childBox.CheckedChanged += new System.EventHandler(this.ChangeAsChild);
			// 
			// argsBox
			// 
			this.argsBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.argsBox.Location = new System.Drawing.Point(149, 156);
			this.argsBox.Name = "argsBox";
			this.argsBox.ProcessEnterKey = false;
			this.argsBox.Size = new System.Drawing.Size(578, 26);
			this.argsBox.TabIndex = 4;
			this.argsBox.ThemedBack = null;
			this.argsBox.ThemedFore = null;
			this.argsBox.TextChanged += new System.EventHandler(this.ChangeText);
			// 
			// argsLabel
			// 
			this.argsLabel.AutoSize = true;
			this.argsLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.argsLabel.Location = new System.Drawing.Point(18, 162);
			this.argsLabel.Name = "argsLabel";
			this.argsLabel.Size = new System.Drawing.Size(87, 20);
			this.argsLabel.TabIndex = 11;
			this.argsLabel.Text = "Arguments";
			// 
			// browseArgsButton
			// 
			this.browseArgsButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.browseArgsButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.browseArgsButton.ImageOver = null;
			this.browseArgsButton.Location = new System.Drawing.Point(733, 153);
			this.browseArgsButton.Name = "browseArgsButton";
			this.browseArgsButton.ShowBorder = true;
			this.browseArgsButton.Size = new System.Drawing.Size(36, 31);
			this.browseArgsButton.StylizeImage = false;
			this.browseArgsButton.TabIndex = 5;
			this.browseArgsButton.Text = "...";
			this.browseArgsButton.ThemedBack = null;
			this.browseArgsButton.ThemedFore = null;
			this.browseArgsButton.UseVisualStyleBackColor = true;
			this.browseArgsButton.Click += new System.EventHandler(this.BrowsePath);
			// 
			// saveButton
			// 
			this.saveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.saveButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.saveButton.Enabled = false;
			this.saveButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.saveButton.ImageOver = null;
			this.saveButton.Location = new System.Drawing.Point(402, 569);
			this.saveButton.Name = "saveButton";
			this.saveButton.ShowBorder = true;
			this.saveButton.Size = new System.Drawing.Size(120, 35);
			this.saveButton.StylizeImage = false;
			this.saveButton.TabIndex = 10;
			this.saveButton.Text = "Save";
			this.saveButton.ThemedBack = null;
			this.saveButton.ThemedFore = null;
			this.saveButton.UseVisualStyleBackColor = true;
			this.saveButton.Click += new System.EventHandler(this.SavePlugin);
			// 
			// pluginsLabel
			// 
			this.pluginsLabel.AutoSize = true;
			this.pluginsLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.pluginsLabel.Location = new System.Drawing.Point(18, 35);
			this.pluginsLabel.Name = "pluginsLabel";
			this.pluginsLabel.Size = new System.Drawing.Size(60, 20);
			this.pluginsLabel.TabIndex = 15;
			this.pluginsLabel.Text = "Plugins";
			// 
			// pluginsBox
			// 
			this.pluginsBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.pluginsBox.FormattingEnabled = true;
			this.pluginsBox.Location = new System.Drawing.Point(149, 28);
			this.pluginsBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.pluginsBox.Name = "pluginsBox";
			this.pluginsBox.Size = new System.Drawing.Size(578, 28);
			this.pluginsBox.TabIndex = 0;
			this.pluginsBox.SelectedIndexChanged += new System.EventHandler(this.ViewPredefined);
			// 
			// nameLabel
			// 
			this.nameLabel.AutoSize = true;
			this.nameLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.nameLabel.Location = new System.Drawing.Point(18, 83);
			this.nameLabel.Name = "nameLabel";
			this.nameLabel.Size = new System.Drawing.Size(51, 20);
			this.nameLabel.TabIndex = 18;
			this.nameLabel.Text = "Name";
			// 
			// nameBox
			// 
			this.nameBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.nameBox.Location = new System.Drawing.Point(149, 75);
			this.nameBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.nameBox.Name = "nameBox";
			this.nameBox.ProcessEnterKey = false;
			this.nameBox.Size = new System.Drawing.Size(578, 26);
			this.nameBox.TabIndex = 1;
			this.nameBox.ThemedBack = null;
			this.nameBox.ThemedFore = null;
			this.nameBox.TextChanged += new System.EventHandler(this.ChangeText);
			// 
			// errorBox
			// 
			this.errorBox.Image = ((System.Drawing.Image)(resources.GetObject("errorBox.Image")));
			this.errorBox.Location = new System.Drawing.Point(733, 75);
			this.errorBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.errorBox.Name = "errorBox";
			this.errorBox.Size = new System.Drawing.Size(26, 35);
			this.errorBox.TabIndex = 19;
			this.errorBox.TabStop = false;
			this.errorBox.Visible = false;
			// 
			// timeoutBox
			// 
			this.timeoutBox.Location = new System.Drawing.Point(149, 250);
			this.timeoutBox.Maximum = new decimal(new int[] {
            300,
            0,
            0,
            0});
			this.timeoutBox.Name = "timeoutBox";
			this.timeoutBox.Size = new System.Drawing.Size(120, 26);
			this.timeoutBox.TabIndex = 7;
			this.timeoutBox.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
			this.timeoutBox.ValueChanged += new System.EventHandler(this.ChangeTimeout);
			// 
			// timeoutLabel
			// 
			this.timeoutLabel.AutoSize = true;
			this.timeoutLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.timeoutLabel.Location = new System.Drawing.Point(18, 252);
			this.timeoutLabel.Name = "timeoutLabel";
			this.timeoutLabel.Size = new System.Drawing.Size(66, 20);
			this.timeoutLabel.TabIndex = 21;
			this.timeoutLabel.Text = "Timeout";
			// 
			// targetBox
			// 
			this.targetBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.targetBox.FormattingEnabled = true;
			this.targetBox.Items.AddRange(new object[] {
            "Current page",
            "Current section and its pages",
            "Current notebook and its sections",
            "Current notebook and its pages",
            "All notebooks and their pages"});
			this.targetBox.Location = new System.Drawing.Point(149, 293);
			this.targetBox.Name = "targetBox";
			this.targetBox.Size = new System.Drawing.Size(314, 28);
			this.targetBox.TabIndex = 8;
			this.targetBox.SelectedIndexChanged += new System.EventHandler(this.ChangeTarget);
			// 
			// targetLabel
			// 
			this.targetLabel.AutoSize = true;
			this.targetLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.targetLabel.Location = new System.Drawing.Point(18, 296);
			this.targetLabel.Name = "targetLabel";
			this.targetLabel.Size = new System.Drawing.Size(55, 20);
			this.targetLabel.TabIndex = 23;
			this.targetLabel.Text = "Target";
			// 
			// skipLockRadio
			// 
			this.skipLockRadio.Checked = true;
			this.skipLockRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.skipLockRadio.Location = new System.Drawing.Point(9, 28);
			this.skipLockRadio.Name = "skipLockRadio";
			this.skipLockRadio.Size = new System.Drawing.Size(185, 25);
			this.skipLockRadio.TabIndex = 0;
			this.skipLockRadio.TabStop = true;
			this.skipLockRadio.Text = "Skip locked sections";
			this.skipLockRadio.UseVisualStyleBackColor = true;
			// 
			// failLockRadio
			// 
			this.failLockRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.failLockRadio.Location = new System.Drawing.Point(9, 58);
			this.failLockRadio.Name = "failLockRadio";
			this.failLockRadio.Size = new System.Drawing.Size(231, 25);
			this.failLockRadio.TabIndex = 1;
			this.failLockRadio.Text = "Fail if any section is locked";
			this.failLockRadio.UseVisualStyleBackColor = true;
			// 
			// pageGroup
			// 
			this.pageGroup.Controls.Add(this.updateRadio);
			this.pageGroup.Controls.Add(this.createRadio);
			this.pageGroup.Controls.Add(this.pageNameBox);
			this.pageGroup.Controls.Add(this.childBox);
			this.pageGroup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.pageGroup.Location = new System.Drawing.Point(149, 339);
			this.pageGroup.Name = "pageGroup";
			this.pageGroup.Size = new System.Drawing.Size(620, 159);
			this.pageGroup.TabIndex = 26;
			this.pageGroup.TabStop = false;
			this.pageGroup.ThemedBorder = null;
			this.pageGroup.ThemedFore = null;
			// 
			// sectionGroup
			// 
			this.sectionGroup.Controls.Add(this.skipLockRadio);
			this.sectionGroup.Controls.Add(this.failLockRadio);
			this.sectionGroup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.sectionGroup.Location = new System.Drawing.Point(505, 238);
			this.sectionGroup.Name = "sectionGroup";
			this.sectionGroup.Size = new System.Drawing.Size(264, 98);
			this.sectionGroup.TabIndex = 27;
			this.sectionGroup.TabStop = false;
			this.sectionGroup.ThemedBorder = null;
			this.sectionGroup.ThemedFore = null;
			this.sectionGroup.Visible = false;
			// 
			// trialBox
			// 
			this.trialBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.trialBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.trialBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.trialBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.trialBox.Location = new System.Drawing.Point(155, 520);
			this.trialBox.Name = "trialBox";
			this.trialBox.Size = new System.Drawing.Size(427, 25);
			this.trialBox.StylizeImage = false;
			this.trialBox.TabIndex = 9;
			this.trialBox.Text = "Trial run - keep cache file and do not update OneNote";
			this.trialBox.ThemedBack = null;
			this.trialBox.ThemedFore = null;
			this.trialBox.UseVisualStyleBackColor = true;
			// 
			// userArgsLabel
			// 
			this.userArgsLabel.AutoSize = true;
			this.userArgsLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.userArgsLabel.Location = new System.Drawing.Point(18, 208);
			this.userArgsLabel.Name = "userArgsLabel";
			this.userArgsLabel.Size = new System.Drawing.Size(125, 20);
			this.userArgsLabel.TabIndex = 30;
			this.userArgsLabel.Text = "User Arguments";
			// 
			// userArgsBox
			// 
			this.userArgsBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.userArgsBox.Location = new System.Drawing.Point(149, 205);
			this.userArgsBox.Name = "userArgsBox";
			this.userArgsBox.ProcessEnterKey = false;
			this.userArgsBox.Size = new System.Drawing.Size(578, 26);
			this.userArgsBox.TabIndex = 6;
			this.userArgsBox.ThemedBack = null;
			this.userArgsBox.ThemedFore = null;
			this.userArgsBox.TextChanged += new System.EventHandler(this.ChangeText);
			// 
			// PluginDialog
			// 
			this.AcceptButton = this.runButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(792, 625);
			this.Controls.Add(this.userArgsLabel);
			this.Controls.Add(this.userArgsBox);
			this.Controls.Add(this.trialBox);
			this.Controls.Add(this.sectionGroup);
			this.Controls.Add(this.pageGroup);
			this.Controls.Add(this.targetLabel);
			this.Controls.Add(this.targetBox);
			this.Controls.Add(this.timeoutLabel);
			this.Controls.Add(this.timeoutBox);
			this.Controls.Add(this.errorBox);
			this.Controls.Add(this.nameBox);
			this.Controls.Add(this.nameLabel);
			this.Controls.Add(this.pluginsBox);
			this.Controls.Add(this.pluginsLabel);
			this.Controls.Add(this.saveButton);
			this.Controls.Add(this.browseArgsButton);
			this.Controls.Add(this.argsLabel);
			this.Controls.Add(this.argsBox);
			this.Controls.Add(this.browseButton);
			this.Controls.Add(this.cmdBox);
			this.Controls.Add(this.cmdLabel);
			this.Controls.Add(this.runButton);
			this.Controls.Add(this.cancelButton);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "PluginDialog";
			this.Padding = new System.Windows.Forms.Padding(15);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Run Plugin";
			((System.ComponentModel.ISupportInitialize)(this.errorBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.timeoutBox)).EndInit();
			this.pageGroup.ResumeLayout(false);
			this.pageGroup.PerformLayout();
			this.sectionGroup.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreButton cancelButton;
		private UI.MoreButton runButton;
		private System.Windows.Forms.Label cmdLabel;
		private UI.MoreTextBox cmdBox;
		private UI.MoreButton browseButton;
		private UI.MoreRadioButton updateRadio;
		private UI.MoreRadioButton createRadio;
		private UI.MoreTextBox pageNameBox;
		private UI.MoreCheckBox childBox;
		private UI.MoreTextBox argsBox;
		private System.Windows.Forms.Label argsLabel;
		private UI.MoreButton browseArgsButton;
		private UI.MoreButton saveButton;
		private System.Windows.Forms.Label pluginsLabel;
		private System.Windows.Forms.ComboBox pluginsBox;
		private System.Windows.Forms.Label nameLabel;
		private UI.MoreTextBox nameBox;
		private System.Windows.Forms.PictureBox errorBox;
		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.NumericUpDown timeoutBox;
		private System.Windows.Forms.Label timeoutLabel;
		private System.Windows.Forms.ComboBox targetBox;
		private System.Windows.Forms.Label targetLabel;
		private UI.MoreRadioButton skipLockRadio;
		private UI.MoreRadioButton failLockRadio;
		private UI.MoreGroupBox pageGroup;
		private UI.MoreGroupBox sectionGroup;
		private UI.MoreCheckBox trialBox;
		private System.Windows.Forms.Label userArgsLabel;
		private UI.MoreTextBox userArgsBox;
	}
}