namespace River.OneMoreAddIn.Commands
{
	partial class RunPluginDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RunPluginDialog));
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
			this.saveLink = new System.Windows.Forms.LinkLabel();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(427, 168);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
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
			this.okButton.Location = new System.Drawing.Point(343, 168);
			this.okButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
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
			this.pathLabel.Location = new System.Drawing.Point(12, 22);
			this.pathLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.pathLabel.Name = "pathLabel";
			this.pathLabel.Size = new System.Drawing.Size(86, 13);
			this.pathLabel.TabIndex = 2;
			this.pathLabel.Text = "Plugin Command";
			// 
			// cmdBox
			// 
			this.cmdBox.Location = new System.Drawing.Point(107, 20);
			this.cmdBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.cmdBox.Name = "cmdBox";
			this.cmdBox.Size = new System.Drawing.Size(373, 20);
			this.cmdBox.TabIndex = 0;
			this.cmdBox.TextChanged += new System.EventHandler(this.ChangeCommand);
			// 
			// browseButton
			// 
			this.browseButton.Location = new System.Drawing.Point(483, 19);
			this.browseButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
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
			this.updateRadio.Location = new System.Drawing.Point(15, 80);
			this.updateRadio.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
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
			this.createRadio.Location = new System.Drawing.Point(15, 107);
			this.createRadio.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.createRadio.Name = "createRadio";
			this.createRadio.Size = new System.Drawing.Size(150, 17);
			this.createRadio.TabIndex = 4;
			this.createRadio.Text = "Create a new page named";
			this.createRadio.UseVisualStyleBackColor = true;
			// 
			// nameBox
			// 
			this.nameBox.Enabled = false;
			this.nameBox.Location = new System.Drawing.Point(166, 107);
			this.nameBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.nameBox.Name = "nameBox";
			this.nameBox.Size = new System.Drawing.Size(314, 20);
			this.nameBox.TabIndex = 5;
			this.nameBox.TextChanged += new System.EventHandler(this.nameBox_TextChanged);
			// 
			// childBox
			// 
			this.childBox.AutoSize = true;
			this.childBox.Enabled = false;
			this.childBox.Location = new System.Drawing.Point(15, 129);
			this.childBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.childBox.Name = "childBox";
			this.childBox.Size = new System.Drawing.Size(171, 17);
			this.childBox.TabIndex = 6;
			this.childBox.Text = "Create as child of current page";
			this.childBox.UseVisualStyleBackColor = true;
			// 
			// argsBox
			// 
			this.argsBox.Location = new System.Drawing.Point(107, 50);
			this.argsBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.argsBox.Name = "argsBox";
			this.argsBox.Size = new System.Drawing.Size(373, 20);
			this.argsBox.TabIndex = 2;
			// 
			// argLabel
			// 
			this.argLabel.AutoSize = true;
			this.argLabel.Location = new System.Drawing.Point(12, 52);
			this.argLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.argLabel.Name = "argLabel";
			this.argLabel.Size = new System.Drawing.Size(57, 13);
			this.argLabel.TabIndex = 11;
			this.argLabel.Text = "Arguments";
			// 
			// browseArgsButton
			// 
			this.browseArgsButton.Location = new System.Drawing.Point(483, 49);
			this.browseArgsButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.browseArgsButton.Name = "browseArgsButton";
			this.browseArgsButton.Size = new System.Drawing.Size(24, 20);
			this.browseArgsButton.TabIndex = 12;
			this.browseArgsButton.Text = "...";
			this.browseArgsButton.UseVisualStyleBackColor = true;
			this.browseArgsButton.Click += new System.EventHandler(this.BrowseArgumentPath);
			// 
			// saveLink
			// 
			this.saveLink.AutoSize = true;
			this.saveLink.Enabled = false;
			this.saveLink.Location = new System.Drawing.Point(12, 173);
			this.saveLink.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.saveLink.Name = "saveLink";
			this.saveLink.Size = new System.Drawing.Size(63, 13);
			this.saveLink.TabIndex = 13;
			this.saveLink.TabStop = true;
			this.saveLink.Text = "Save plugin";
			this.saveLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.SavePlugin);
			// 
			// RunPluginDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(519, 204);
			this.Controls.Add(this.saveLink);
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
			this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "RunPluginDialog";
			this.Padding = new System.Windows.Forms.Padding(10, 10, 10, 10);
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
		private System.Windows.Forms.LinkLabel saveLink;
	}
}