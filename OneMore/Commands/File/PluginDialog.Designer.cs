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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PluginDialog));
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.pathLabel = new System.Windows.Forms.Label();
			this.cmdBox = new System.Windows.Forms.TextBox();
			this.browseButton = new System.Windows.Forms.Button();
			this.updateRadio = new System.Windows.Forms.RadioButton();
			this.createRadio = new System.Windows.Forms.RadioButton();
			this.nameBox = new System.Windows.Forms.TextBox();
			this.childBox = new System.Windows.Forms.CheckBox();
			this.argsBox = new System.Windows.Forms.TextBox();
			this.argLabel = new System.Windows.Forms.Label();
			this.browseArgsButton = new System.Windows.Forms.Button();
			this.saveButton = new System.Windows.Forms.Button();
			this.pluginLabel = new System.Windows.Forms.Label();
			this.predefinedBox = new System.Windows.Forms.ComboBox();
			this.pluginNameLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(427, 198);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(2);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(80, 23);
			this.cancelButton.TabIndex = 8;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Enabled = false;
			this.okButton.Location = new System.Drawing.Point(343, 198);
			this.okButton.Margin = new System.Windows.Forms.Padding(2);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(80, 23);
			this.okButton.TabIndex = 7;
			this.okButton.Text = "Run";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// pathLabel
			// 
			this.pathLabel.AutoSize = true;
			this.pathLabel.Location = new System.Drawing.Point(12, 52);
			this.pathLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.pathLabel.Name = "pathLabel";
			this.pathLabel.Size = new System.Drawing.Size(54, 13);
			this.pathLabel.TabIndex = 2;
			this.pathLabel.Text = "Command";
			// 
			// cmdBox
			// 
			this.cmdBox.Location = new System.Drawing.Point(93, 50);
			this.cmdBox.Margin = new System.Windows.Forms.Padding(2);
			this.cmdBox.Name = "cmdBox";
			this.cmdBox.Size = new System.Drawing.Size(387, 20);
			this.cmdBox.TabIndex = 0;
			this.cmdBox.TextChanged += new System.EventHandler(this.ChangeCommand);
			// 
			// browseButton
			// 
			this.browseButton.Location = new System.Drawing.Point(483, 49);
			this.browseButton.Margin = new System.Windows.Forms.Padding(2);
			this.browseButton.Name = "browseButton";
			this.browseButton.Size = new System.Drawing.Size(24, 20);
			this.browseButton.TabIndex = 1;
			this.browseButton.Text = "...";
			this.browseButton.UseVisualStyleBackColor = true;
			this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
			// 
			// updateRadio
			// 
			this.updateRadio.AutoSize = true;
			this.updateRadio.Checked = true;
			this.updateRadio.Location = new System.Drawing.Point(15, 110);
			this.updateRadio.Margin = new System.Windows.Forms.Padding(2);
			this.updateRadio.Name = "updateRadio";
			this.updateRadio.Size = new System.Drawing.Size(123, 17);
			this.updateRadio.TabIndex = 3;
			this.updateRadio.TabStop = true;
			this.updateRadio.Text = "Update current page";
			this.updateRadio.UseVisualStyleBackColor = true;
			this.updateRadio.CheckedChanged += new System.EventHandler(this.updateRadio_CheckedChanged);
			// 
			// createRadio
			// 
			this.createRadio.AutoSize = true;
			this.createRadio.Location = new System.Drawing.Point(15, 137);
			this.createRadio.Margin = new System.Windows.Forms.Padding(2);
			this.createRadio.Name = "createRadio";
			this.createRadio.Size = new System.Drawing.Size(150, 17);
			this.createRadio.TabIndex = 4;
			this.createRadio.Text = "Create a new page named";
			this.createRadio.UseVisualStyleBackColor = true;
			// 
			// nameBox
			// 
			this.nameBox.Enabled = false;
			this.nameBox.Location = new System.Drawing.Point(166, 136);
			this.nameBox.Margin = new System.Windows.Forms.Padding(2);
			this.nameBox.Name = "nameBox";
			this.nameBox.Size = new System.Drawing.Size(314, 20);
			this.nameBox.TabIndex = 5;
			this.nameBox.TextChanged += new System.EventHandler(this.nameBox_TextChanged);
			// 
			// childBox
			// 
			this.childBox.AutoSize = true;
			this.childBox.Enabled = false;
			this.childBox.Location = new System.Drawing.Point(166, 158);
			this.childBox.Margin = new System.Windows.Forms.Padding(2);
			this.childBox.Name = "childBox";
			this.childBox.Size = new System.Drawing.Size(137, 17);
			this.childBox.TabIndex = 6;
			this.childBox.Text = "as child of current page";
			this.childBox.UseVisualStyleBackColor = true;
			// 
			// argsBox
			// 
			this.argsBox.Location = new System.Drawing.Point(93, 80);
			this.argsBox.Margin = new System.Windows.Forms.Padding(2);
			this.argsBox.Name = "argsBox";
			this.argsBox.Size = new System.Drawing.Size(387, 20);
			this.argsBox.TabIndex = 2;
			// 
			// argLabel
			// 
			this.argLabel.AutoSize = true;
			this.argLabel.Location = new System.Drawing.Point(12, 82);
			this.argLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.argLabel.Name = "argLabel";
			this.argLabel.Size = new System.Drawing.Size(57, 13);
			this.argLabel.TabIndex = 11;
			this.argLabel.Text = "Arguments";
			// 
			// browseArgsButton
			// 
			this.browseArgsButton.Location = new System.Drawing.Point(483, 79);
			this.browseArgsButton.Margin = new System.Windows.Forms.Padding(2);
			this.browseArgsButton.Name = "browseArgsButton";
			this.browseArgsButton.Size = new System.Drawing.Size(24, 20);
			this.browseArgsButton.TabIndex = 12;
			this.browseArgsButton.Text = "...";
			this.browseArgsButton.UseVisualStyleBackColor = true;
			this.browseArgsButton.Click += new System.EventHandler(this.BrowseArgumentPath);
			// 
			// saveButton
			// 
			this.saveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.saveButton.Enabled = false;
			this.saveButton.Location = new System.Drawing.Point(259, 198);
			this.saveButton.Margin = new System.Windows.Forms.Padding(2);
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = new System.Drawing.Size(80, 23);
			this.saveButton.TabIndex = 14;
			this.saveButton.Text = "Save";
			this.saveButton.UseVisualStyleBackColor = true;
			this.saveButton.Click += new System.EventHandler(this.SavePlugin);
			// 
			// pluginLabel
			// 
			this.pluginLabel.AutoSize = true;
			this.pluginLabel.Location = new System.Drawing.Point(12, 23);
			this.pluginLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.pluginLabel.Name = "pluginLabel";
			this.pluginLabel.Size = new System.Drawing.Size(36, 13);
			this.pluginLabel.TabIndex = 15;
			this.pluginLabel.Text = "Plugin";
			// 
			// predefinedBox
			// 
			this.predefinedBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.predefinedBox.FormattingEnabled = true;
			this.predefinedBox.Location = new System.Drawing.Point(93, 20);
			this.predefinedBox.Name = "predefinedBox";
			this.predefinedBox.Size = new System.Drawing.Size(387, 21);
			this.predefinedBox.TabIndex = 16;
			this.predefinedBox.SelectedIndexChanged += new System.EventHandler(this.ViewPredefined);
			// 
			// pluginNameLabel
			// 
			this.pluginNameLabel.AutoSize = true;
			this.pluginNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pluginNameLabel.Location = new System.Drawing.Point(12, 208);
			this.pluginNameLabel.Name = "pluginNameLabel";
			this.pluginNameLabel.Size = new System.Drawing.Size(45, 13);
			this.pluginNameLabel.TabIndex = 17;
			this.pluginNameLabel.Text = "-name-";
			this.pluginNameLabel.Visible = false;
			// 
			// PluginDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(519, 234);
			this.Controls.Add(this.pluginNameLabel);
			this.Controls.Add(this.predefinedBox);
			this.Controls.Add(this.pluginLabel);
			this.Controls.Add(this.saveButton);
			this.Controls.Add(this.browseArgsButton);
			this.Controls.Add(this.argLabel);
			this.Controls.Add(this.argsBox);
			this.Controls.Add(this.childBox);
			this.Controls.Add(this.nameBox);
			this.Controls.Add(this.createRadio);
			this.Controls.Add(this.updateRadio);
			this.Controls.Add(this.browseButton);
			this.Controls.Add(this.cmdBox);
			this.Controls.Add(this.pathLabel);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(2);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "PluginDialog";
			this.Padding = new System.Windows.Forms.Padding(10);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Run Plugin";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Label pathLabel;
		private System.Windows.Forms.TextBox cmdBox;
		private System.Windows.Forms.Button browseButton;
		private System.Windows.Forms.RadioButton updateRadio;
		private System.Windows.Forms.RadioButton createRadio;
		private System.Windows.Forms.TextBox nameBox;
		private System.Windows.Forms.CheckBox childBox;
		private System.Windows.Forms.TextBox argsBox;
		private System.Windows.Forms.Label argLabel;
		private System.Windows.Forms.Button browseArgsButton;
		private System.Windows.Forms.Button saveButton;
		private System.Windows.Forms.Label pluginLabel;
		private System.Windows.Forms.ComboBox predefinedBox;
		private System.Windows.Forms.Label pluginNameLabel;
	}
}