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
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.cmdLabel = new System.Windows.Forms.Label();
			this.cmdBox = new System.Windows.Forms.TextBox();
			this.browseButton = new System.Windows.Forms.Button();
			this.updateRadio = new System.Windows.Forms.RadioButton();
			this.createRadio = new System.Windows.Forms.RadioButton();
			this.pageNameBox = new System.Windows.Forms.TextBox();
			this.childBox = new System.Windows.Forms.CheckBox();
			this.argsBox = new System.Windows.Forms.TextBox();
			this.argsLabel = new System.Windows.Forms.Label();
			this.browseArgsButton = new System.Windows.Forms.Button();
			this.saveButton = new System.Windows.Forms.Button();
			this.pluginsLabel = new System.Windows.Forms.Label();
			this.pluginsBox = new System.Windows.Forms.ComboBox();
			this.nameLabel = new System.Windows.Forms.Label();
			this.nameBox = new System.Windows.Forms.TextBox();
			this.errorBox = new System.Windows.Forms.PictureBox();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.timeoutBox = new System.Windows.Forms.NumericUpDown();
			this.timeoutLabel = new System.Windows.Forms.Label();
			this.targetBox = new System.Windows.Forms.ComboBox();
			this.targetLabel = new System.Windows.Forms.Label();
			this.skipLockRadio = new System.Windows.Forms.RadioButton();
			this.failLockRadio = new System.Windows.Forms.RadioButton();
			this.pageGroup = new System.Windows.Forms.GroupBox();
			this.sectionGroup = new System.Windows.Forms.GroupBox();
			((System.ComponentModel.ISupportInitialize)(this.errorBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.timeoutBox)).BeginInit();
			this.pageGroup.SuspendLayout();
			this.sectionGroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(640, 473);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(120, 35);
			this.cancelButton.TabIndex = 12;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Enabled = false;
			this.okButton.Location = new System.Drawing.Point(514, 473);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(120, 35);
			this.okButton.TabIndex = 11;
			this.okButton.Text = "Run";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OK);
			// 
			// cmdLabel
			// 
			this.cmdLabel.AutoSize = true;
			this.cmdLabel.Location = new System.Drawing.Point(18, 126);
			this.cmdLabel.Name = "cmdLabel";
			this.cmdLabel.Size = new System.Drawing.Size(82, 20);
			this.cmdLabel.TabIndex = 2;
			this.cmdLabel.Text = "Command";
			// 
			// cmdBox
			// 
			this.cmdBox.Location = new System.Drawing.Point(140, 123);
			this.cmdBox.Name = "cmdBox";
			this.cmdBox.Size = new System.Drawing.Size(578, 26);
			this.cmdBox.TabIndex = 2;
			this.cmdBox.TextChanged += new System.EventHandler(this.ChangeText);
			// 
			// browseButton
			// 
			this.browseButton.Location = new System.Drawing.Point(724, 122);
			this.browseButton.Name = "browseButton";
			this.browseButton.Size = new System.Drawing.Size(36, 31);
			this.browseButton.TabIndex = 3;
			this.browseButton.Text = "...";
			this.browseButton.UseVisualStyleBackColor = true;
			this.browseButton.Click += new System.EventHandler(this.BrowsePath);
			// 
			// updateRadio
			// 
			this.updateRadio.AutoSize = true;
			this.updateRadio.Checked = true;
			this.updateRadio.Location = new System.Drawing.Point(6, 25);
			this.updateRadio.Name = "updateRadio";
			this.updateRadio.Size = new System.Drawing.Size(181, 24);
			this.updateRadio.TabIndex = 6;
			this.updateRadio.TabStop = true;
			this.updateRadio.Text = "Update current page";
			this.updateRadio.UseVisualStyleBackColor = true;
			this.updateRadio.CheckedChanged += new System.EventHandler(this.updateRadio_CheckedChanged);
			// 
			// createRadio
			// 
			this.createRadio.AutoSize = true;
			this.createRadio.Location = new System.Drawing.Point(6, 55);
			this.createRadio.Name = "createRadio";
			this.createRadio.Size = new System.Drawing.Size(221, 24);
			this.createRadio.TabIndex = 7;
			this.createRadio.Text = "Create a new page named";
			this.createRadio.UseVisualStyleBackColor = true;
			// 
			// pageNameBox
			// 
			this.pageNameBox.Enabled = false;
			this.pageNameBox.Location = new System.Drawing.Point(31, 85);
			this.pageNameBox.Name = "pageNameBox";
			this.pageNameBox.Size = new System.Drawing.Size(469, 26);
			this.pageNameBox.TabIndex = 8;
			this.pageNameBox.TextChanged += new System.EventHandler(this.ChangeText);
			// 
			// childBox
			// 
			this.childBox.AutoSize = true;
			this.childBox.Enabled = false;
			this.childBox.Location = new System.Drawing.Point(31, 117);
			this.childBox.Name = "childBox";
			this.childBox.Size = new System.Drawing.Size(200, 24);
			this.childBox.TabIndex = 9;
			this.childBox.Text = "as child of current page";
			this.childBox.UseVisualStyleBackColor = true;
			this.childBox.CheckedChanged += new System.EventHandler(this.ChangeAsChild);
			// 
			// argsBox
			// 
			this.argsBox.Location = new System.Drawing.Point(140, 169);
			this.argsBox.Name = "argsBox";
			this.argsBox.Size = new System.Drawing.Size(578, 26);
			this.argsBox.TabIndex = 4;
			this.argsBox.TextChanged += new System.EventHandler(this.ChangeText);
			// 
			// argsLabel
			// 
			this.argsLabel.AutoSize = true;
			this.argsLabel.Location = new System.Drawing.Point(18, 172);
			this.argsLabel.Name = "argsLabel";
			this.argsLabel.Size = new System.Drawing.Size(87, 20);
			this.argsLabel.TabIndex = 11;
			this.argsLabel.Text = "Arguments";
			// 
			// browseArgsButton
			// 
			this.browseArgsButton.Location = new System.Drawing.Point(724, 168);
			this.browseArgsButton.Name = "browseArgsButton";
			this.browseArgsButton.Size = new System.Drawing.Size(36, 31);
			this.browseArgsButton.TabIndex = 5;
			this.browseArgsButton.Text = "...";
			this.browseArgsButton.UseVisualStyleBackColor = true;
			this.browseArgsButton.Click += new System.EventHandler(this.BrowsePath);
			// 
			// saveButton
			// 
			this.saveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.saveButton.Enabled = false;
			this.saveButton.Location = new System.Drawing.Point(388, 473);
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = new System.Drawing.Size(120, 35);
			this.saveButton.TabIndex = 10;
			this.saveButton.Text = "Save";
			this.saveButton.UseVisualStyleBackColor = true;
			this.saveButton.Click += new System.EventHandler(this.SavePlugin);
			// 
			// pluginsLabel
			// 
			this.pluginsLabel.AutoSize = true;
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
			this.pluginsBox.Location = new System.Drawing.Point(140, 31);
			this.pluginsBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.pluginsBox.Name = "pluginsBox";
			this.pluginsBox.Size = new System.Drawing.Size(578, 28);
			this.pluginsBox.TabIndex = 0;
			this.pluginsBox.SelectedIndexChanged += new System.EventHandler(this.ViewPredefined);
			// 
			// nameLabel
			// 
			this.nameLabel.AutoSize = true;
			this.nameLabel.Location = new System.Drawing.Point(18, 83);
			this.nameLabel.Name = "nameLabel";
			this.nameLabel.Size = new System.Drawing.Size(51, 20);
			this.nameLabel.TabIndex = 18;
			this.nameLabel.Text = "Name";
			// 
			// nameBox
			// 
			this.nameBox.Location = new System.Drawing.Point(140, 78);
			this.nameBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.nameBox.Name = "nameBox";
			this.nameBox.Size = new System.Drawing.Size(578, 26);
			this.nameBox.TabIndex = 1;
			this.nameBox.TextChanged += new System.EventHandler(this.ChangeText);
			// 
			// errorBox
			// 
			this.errorBox.Image = ((System.Drawing.Image)(resources.GetObject("errorBox.Image")));
			this.errorBox.Location = new System.Drawing.Point(724, 78);
			this.errorBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.errorBox.Name = "errorBox";
			this.errorBox.Size = new System.Drawing.Size(26, 35);
			this.errorBox.TabIndex = 19;
			this.errorBox.TabStop = false;
			this.errorBox.Visible = false;
			// 
			// timeoutBox
			// 
			this.timeoutBox.Location = new System.Drawing.Point(140, 213);
			this.timeoutBox.Maximum = new decimal(new int[] {
            300,
            0,
            0,
            0});
			this.timeoutBox.Name = "timeoutBox";
			this.timeoutBox.Size = new System.Drawing.Size(120, 26);
			this.timeoutBox.TabIndex = 20;
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
			this.timeoutLabel.Location = new System.Drawing.Point(18, 215);
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
            "Current notebook"});
			this.targetBox.Location = new System.Drawing.Point(140, 256);
			this.targetBox.Name = "targetBox";
			this.targetBox.Size = new System.Drawing.Size(314, 28);
			this.targetBox.TabIndex = 22;
			this.targetBox.SelectedIndexChanged += new System.EventHandler(this.ChangeTarget);
			// 
			// targetLabel
			// 
			this.targetLabel.AutoSize = true;
			this.targetLabel.Location = new System.Drawing.Point(18, 259);
			this.targetLabel.Name = "targetLabel";
			this.targetLabel.Size = new System.Drawing.Size(55, 20);
			this.targetLabel.TabIndex = 23;
			this.targetLabel.Text = "Target";
			// 
			// skipLockRadio
			// 
			this.skipLockRadio.AutoSize = true;
			this.skipLockRadio.Checked = true;
			this.skipLockRadio.Location = new System.Drawing.Point(9, 28);
			this.skipLockRadio.Name = "skipLockRadio";
			this.skipLockRadio.Size = new System.Drawing.Size(178, 24);
			this.skipLockRadio.TabIndex = 24;
			this.skipLockRadio.TabStop = true;
			this.skipLockRadio.Text = "Skip locked sections";
			this.skipLockRadio.UseVisualStyleBackColor = true;
			// 
			// failLockButton
			// 
			this.failLockRadio.AutoSize = true;
			this.failLockRadio.Location = new System.Drawing.Point(9, 58);
			this.failLockRadio.Name = "failLockButton";
			this.failLockRadio.Size = new System.Drawing.Size(220, 24);
			this.failLockRadio.TabIndex = 25;
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
			this.pageGroup.Location = new System.Drawing.Point(140, 290);
			this.pageGroup.Name = "pageGroup";
			this.pageGroup.Size = new System.Drawing.Size(620, 159);
			this.pageGroup.TabIndex = 26;
			this.pageGroup.TabStop = false;
			// 
			// sectionGroup
			// 
			this.sectionGroup.Controls.Add(this.skipLockRadio);
			this.sectionGroup.Controls.Add(this.failLockRadio);
			this.sectionGroup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.sectionGroup.Location = new System.Drawing.Point(496, 201);
			this.sectionGroup.Name = "sectionGroup";
			this.sectionGroup.Size = new System.Drawing.Size(264, 98);
			this.sectionGroup.TabIndex = 27;
			this.sectionGroup.TabStop = false;
			this.sectionGroup.Visible = false;
			// 
			// PluginDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(778, 529);
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
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "PluginDialog";
			this.Padding = new System.Windows.Forms.Padding(15);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Run Plugin";
			((System.ComponentModel.ISupportInitialize)(this.errorBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.timeoutBox)).EndInit();
			this.pageGroup.ResumeLayout(false);
			this.pageGroup.PerformLayout();
			this.sectionGroup.ResumeLayout(false);
			this.sectionGroup.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Label cmdLabel;
		private System.Windows.Forms.TextBox cmdBox;
		private System.Windows.Forms.Button browseButton;
		private System.Windows.Forms.RadioButton updateRadio;
		private System.Windows.Forms.RadioButton createRadio;
		private System.Windows.Forms.TextBox pageNameBox;
		private System.Windows.Forms.CheckBox childBox;
		private System.Windows.Forms.TextBox argsBox;
		private System.Windows.Forms.Label argsLabel;
		private System.Windows.Forms.Button browseArgsButton;
		private System.Windows.Forms.Button saveButton;
		private System.Windows.Forms.Label pluginsLabel;
		private System.Windows.Forms.ComboBox pluginsBox;
		private System.Windows.Forms.Label nameLabel;
		private System.Windows.Forms.TextBox nameBox;
		private System.Windows.Forms.PictureBox errorBox;
		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.NumericUpDown timeoutBox;
		private System.Windows.Forms.Label timeoutLabel;
		private System.Windows.Forms.ComboBox targetBox;
		private System.Windows.Forms.Label targetLabel;
		private System.Windows.Forms.RadioButton skipLockRadio;
		private System.Windows.Forms.RadioButton failLockRadio;
		private System.Windows.Forms.GroupBox pageGroup;
		private System.Windows.Forms.GroupBox sectionGroup;
	}
}