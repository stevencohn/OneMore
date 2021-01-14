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
			this.errorTip = new System.Windows.Forms.ToolTip(this.components);
			((System.ComponentModel.ISupportInitialize)(this.errorBox)).BeginInit();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(427, 227);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(2);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(80, 23);
			this.cancelButton.TabIndex = 12;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Enabled = false;
			this.okButton.Location = new System.Drawing.Point(343, 227);
			this.okButton.Margin = new System.Windows.Forms.Padding(2);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(80, 23);
			this.okButton.TabIndex = 11;
			this.okButton.Text = "Run";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OK);
			// 
			// cmdLabel
			// 
			this.cmdLabel.AutoSize = true;
			this.cmdLabel.Location = new System.Drawing.Point(12, 82);
			this.cmdLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.cmdLabel.Name = "cmdLabel";
			this.cmdLabel.Size = new System.Drawing.Size(54, 13);
			this.cmdLabel.TabIndex = 2;
			this.cmdLabel.Text = "Command";
			// 
			// cmdBox
			// 
			this.cmdBox.Location = new System.Drawing.Point(93, 80);
			this.cmdBox.Margin = new System.Windows.Forms.Padding(2);
			this.cmdBox.Name = "cmdBox";
			this.cmdBox.Size = new System.Drawing.Size(387, 20);
			this.cmdBox.TabIndex = 2;
			this.cmdBox.TextChanged += new System.EventHandler(this.ChangeText);
			// 
			// browseButton
			// 
			this.browseButton.Location = new System.Drawing.Point(483, 79);
			this.browseButton.Margin = new System.Windows.Forms.Padding(2);
			this.browseButton.Name = "browseButton";
			this.browseButton.Size = new System.Drawing.Size(24, 20);
			this.browseButton.TabIndex = 3;
			this.browseButton.Text = "...";
			this.browseButton.UseVisualStyleBackColor = true;
			this.browseButton.Click += new System.EventHandler(this.BrowsePath);
			// 
			// updateRadio
			// 
			this.updateRadio.AutoSize = true;
			this.updateRadio.Checked = true;
			this.updateRadio.Location = new System.Drawing.Point(15, 140);
			this.updateRadio.Margin = new System.Windows.Forms.Padding(2);
			this.updateRadio.Name = "updateRadio";
			this.updateRadio.Size = new System.Drawing.Size(123, 17);
			this.updateRadio.TabIndex = 6;
			this.updateRadio.TabStop = true;
			this.updateRadio.Text = "Update current page";
			this.updateRadio.UseVisualStyleBackColor = true;
			this.updateRadio.CheckedChanged += new System.EventHandler(this.updateRadio_CheckedChanged);
			// 
			// createRadio
			// 
			this.createRadio.AutoSize = true;
			this.createRadio.Location = new System.Drawing.Point(15, 167);
			this.createRadio.Margin = new System.Windows.Forms.Padding(2);
			this.createRadio.Name = "createRadio";
			this.createRadio.Size = new System.Drawing.Size(150, 17);
			this.createRadio.TabIndex = 7;
			this.createRadio.Text = "Create a new page named";
			this.createRadio.UseVisualStyleBackColor = true;
			// 
			// pageNameBox
			// 
			this.pageNameBox.Enabled = false;
			this.pageNameBox.Location = new System.Drawing.Point(166, 166);
			this.pageNameBox.Margin = new System.Windows.Forms.Padding(2);
			this.pageNameBox.Name = "pageNameBox";
			this.pageNameBox.Size = new System.Drawing.Size(314, 20);
			this.pageNameBox.TabIndex = 8;
			this.pageNameBox.TextChanged += new System.EventHandler(this.ChangeText);
			// 
			// childBox
			// 
			this.childBox.AutoSize = true;
			this.childBox.Enabled = false;
			this.childBox.Location = new System.Drawing.Point(166, 188);
			this.childBox.Margin = new System.Windows.Forms.Padding(2);
			this.childBox.Name = "childBox";
			this.childBox.Size = new System.Drawing.Size(137, 17);
			this.childBox.TabIndex = 9;
			this.childBox.Text = "as child of current page";
			this.childBox.UseVisualStyleBackColor = true;
			this.childBox.CheckedChanged += new System.EventHandler(this.ChangeAsChild);
			// 
			// argsBox
			// 
			this.argsBox.Location = new System.Drawing.Point(93, 110);
			this.argsBox.Margin = new System.Windows.Forms.Padding(2);
			this.argsBox.Name = "argsBox";
			this.argsBox.Size = new System.Drawing.Size(387, 20);
			this.argsBox.TabIndex = 4;
			this.argsBox.TextChanged += new System.EventHandler(this.ChangeText);
			// 
			// argsLabel
			// 
			this.argsLabel.AutoSize = true;
			this.argsLabel.Location = new System.Drawing.Point(12, 112);
			this.argsLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.argsLabel.Name = "argsLabel";
			this.argsLabel.Size = new System.Drawing.Size(57, 13);
			this.argsLabel.TabIndex = 11;
			this.argsLabel.Text = "Arguments";
			// 
			// browseArgsButton
			// 
			this.browseArgsButton.Location = new System.Drawing.Point(483, 109);
			this.browseArgsButton.Margin = new System.Windows.Forms.Padding(2);
			this.browseArgsButton.Name = "browseArgsButton";
			this.browseArgsButton.Size = new System.Drawing.Size(24, 20);
			this.browseArgsButton.TabIndex = 5;
			this.browseArgsButton.Text = "...";
			this.browseArgsButton.UseVisualStyleBackColor = true;
			this.browseArgsButton.Click += new System.EventHandler(this.BrowsePath);
			// 
			// saveButton
			// 
			this.saveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.saveButton.Enabled = false;
			this.saveButton.Location = new System.Drawing.Point(259, 227);
			this.saveButton.Margin = new System.Windows.Forms.Padding(2);
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = new System.Drawing.Size(80, 23);
			this.saveButton.TabIndex = 10;
			this.saveButton.Text = "Save";
			this.saveButton.UseVisualStyleBackColor = true;
			this.saveButton.Click += new System.EventHandler(this.SavePlugin);
			// 
			// pluginsLabel
			// 
			this.pluginsLabel.AutoSize = true;
			this.pluginsLabel.Location = new System.Drawing.Point(12, 23);
			this.pluginsLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.pluginsLabel.Name = "pluginsLabel";
			this.pluginsLabel.Size = new System.Drawing.Size(41, 13);
			this.pluginsLabel.TabIndex = 15;
			this.pluginsLabel.Text = "Plugins";
			// 
			// pluginsBox
			// 
			this.pluginsBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.pluginsBox.FormattingEnabled = true;
			this.pluginsBox.Location = new System.Drawing.Point(93, 20);
			this.pluginsBox.Name = "pluginsBox";
			this.pluginsBox.Size = new System.Drawing.Size(387, 21);
			this.pluginsBox.TabIndex = 0;
			this.pluginsBox.SelectedIndexChanged += new System.EventHandler(this.ViewPredefined);
			// 
			// nameLabel
			// 
			this.nameLabel.AutoSize = true;
			this.nameLabel.Location = new System.Drawing.Point(12, 54);
			this.nameLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.nameLabel.Name = "nameLabel";
			this.nameLabel.Size = new System.Drawing.Size(35, 13);
			this.nameLabel.TabIndex = 18;
			this.nameLabel.Text = "Name";
			// 
			// nameBox
			// 
			this.nameBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.nameBox.Location = new System.Drawing.Point(93, 51);
			this.nameBox.Name = "nameBox";
			this.nameBox.Size = new System.Drawing.Size(387, 20);
			this.nameBox.TabIndex = 1;
			this.nameBox.TextChanged += new System.EventHandler(this.ChangeText);
			// 
			// errorBox
			// 
			this.errorBox.Image = ((System.Drawing.Image)(resources.GetObject("errorBox.Image")));
			this.errorBox.Location = new System.Drawing.Point(483, 51);
			this.errorBox.Name = "errorBox";
			this.errorBox.Size = new System.Drawing.Size(17, 23);
			this.errorBox.TabIndex = 19;
			this.errorBox.TabStop = false;
			this.errorTip.SetToolTip(this.errorBox, "Invalid or duplicate name");
			this.errorBox.Visible = false;
			// 
			// PluginDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(519, 263);
			this.Controls.Add(this.errorBox);
			this.Controls.Add(this.nameBox);
			this.Controls.Add(this.nameLabel);
			this.Controls.Add(this.pluginsBox);
			this.Controls.Add(this.pluginsLabel);
			this.Controls.Add(this.saveButton);
			this.Controls.Add(this.browseArgsButton);
			this.Controls.Add(this.argsLabel);
			this.Controls.Add(this.argsBox);
			this.Controls.Add(this.childBox);
			this.Controls.Add(this.pageNameBox);
			this.Controls.Add(this.createRadio);
			this.Controls.Add(this.updateRadio);
			this.Controls.Add(this.browseButton);
			this.Controls.Add(this.cmdBox);
			this.Controls.Add(this.cmdLabel);
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
			((System.ComponentModel.ISupportInitialize)(this.errorBox)).EndInit();
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
		private System.Windows.Forms.ToolTip errorTip;
	}
}