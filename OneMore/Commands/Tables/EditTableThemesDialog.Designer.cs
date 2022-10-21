namespace River.OneMoreAddIn.Commands
{
	partial class EditTableThemesDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditTableThemesDialog));
			this.cancelButton = new System.Windows.Forms.Button();
			this.combo = new System.Windows.Forms.ComboBox();
			this.nameLabel = new System.Windows.Forms.Label();
			this.elementsGroup = new System.Windows.Forms.GroupBox();
			this.resetButton = new System.Windows.Forms.Button();
			this.elementsBox = new River.OneMoreAddIn.UI.MoreListView();
			this.previewGroup = new System.Windows.Forms.GroupBox();
			this.previewBox = new System.Windows.Forms.PictureBox();
			this.toolstrip = new System.Windows.Forms.ToolStrip();
			this.newButton = new System.Windows.Forms.ToolStripButton();
			this.saveButton = new System.Windows.Forms.ToolStripButton();
			this.renameButton = new System.Windows.Forms.ToolStripButton();
			this.deleteButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.elementsGroup.SuspendLayout();
			this.previewGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.previewBox)).BeginInit();
			this.toolstrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(641, 534);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(110, 34);
			this.cancelButton.TabIndex = 2;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// combo
			// 
			this.combo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.combo.FormattingEnabled = true;
			this.combo.Location = new System.Drawing.Point(113, 23);
			this.combo.Name = "combo";
			this.combo.Size = new System.Drawing.Size(406, 28);
			this.combo.TabIndex = 0;
			this.combo.SelectedIndexChanged += new System.EventHandler(this.ChooseTheme);
			// 
			// nameLabel
			// 
			this.nameLabel.AutoSize = true;
			this.nameLabel.Location = new System.Drawing.Point(23, 26);
			this.nameLabel.Name = "nameLabel";
			this.nameLabel.Size = new System.Drawing.Size(51, 20);
			this.nameLabel.TabIndex = 7;
			this.nameLabel.Text = "Name";
			// 
			// elementsGroup
			// 
			this.elementsGroup.Controls.Add(this.resetButton);
			this.elementsGroup.Controls.Add(this.elementsBox);
			this.elementsGroup.Location = new System.Drawing.Point(27, 73);
			this.elementsGroup.Name = "elementsGroup";
			this.elementsGroup.Padding = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.elementsGroup.Size = new System.Drawing.Size(492, 441);
			this.elementsGroup.TabIndex = 8;
			this.elementsGroup.TabStop = false;
			this.elementsGroup.Text = "Table Elements";
			// 
			// resetButton
			// 
			this.resetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.resetButton.Enabled = false;
			this.resetButton.Location = new System.Drawing.Point(13, 401);
			this.resetButton.Name = "resetButton";
			this.resetButton.Size = new System.Drawing.Size(110, 34);
			this.resetButton.TabIndex = 1;
			this.resetButton.Text = "Reset";
			this.resetButton.UseVisualStyleBackColor = true;
			this.resetButton.Click += new System.EventHandler(this.ResetTheme);
			// 
			// elementsBox
			// 
			this.elementsBox.ControlPadding = 2;
			this.elementsBox.FullRowSelect = true;
			this.elementsBox.HideSelection = false;
			this.elementsBox.HighlightBackground = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(193)))), ((int)(((byte)(255)))));
			this.elementsBox.Location = new System.Drawing.Point(13, 25);
			this.elementsBox.Name = "elementsBox";
			this.elementsBox.OwnerDraw = true;
			this.elementsBox.Size = new System.Drawing.Size(473, 370);
			this.elementsBox.TabIndex = 0;
			this.elementsBox.UseCompatibleStateImageBehavior = false;
			this.elementsBox.View = System.Windows.Forms.View.Details;
			// 
			// previewGroup
			// 
			this.previewGroup.Controls.Add(this.previewBox);
			this.previewGroup.Location = new System.Drawing.Point(525, 73);
			this.previewGroup.Name = "previewGroup";
			this.previewGroup.Padding = new System.Windows.Forms.Padding(10);
			this.previewGroup.Size = new System.Drawing.Size(217, 199);
			this.previewGroup.TabIndex = 9;
			this.previewGroup.TabStop = false;
			this.previewGroup.Text = "Preview";
			// 
			// previewBox
			// 
			this.previewBox.BackColor = System.Drawing.SystemColors.Window;
			this.previewBox.Location = new System.Drawing.Point(13, 32);
			this.previewBox.Name = "previewBox";
			this.previewBox.Size = new System.Drawing.Size(190, 125);
			this.previewBox.TabIndex = 0;
			this.previewBox.TabStop = false;
			// 
			// toolstrip
			// 
			this.toolstrip.Dock = System.Windows.Forms.DockStyle.None;
			this.toolstrip.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.toolstrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newButton,
            this.renameButton,
            this.saveButton,
            this.toolStripSeparator1,
            this.deleteButton});
			this.toolstrip.Location = new System.Drawing.Point(525, 18);
			this.toolstrip.Name = "toolstrip";
			this.toolstrip.Size = new System.Drawing.Size(206, 33);
			this.toolstrip.TabIndex = 10;
			this.toolstrip.Text = "toolStrip1";
			// 
			// newButton
			// 
			this.newButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.newButton.Image = global::River.OneMoreAddIn.Properties.Resources.NewStyle;
			this.newButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.newButton.Name = "newButton";
			this.newButton.Size = new System.Drawing.Size(34, 28);
			this.newButton.Text = "New Style";
			this.newButton.Click += new System.EventHandler(this.CreateNewTheme);
			// 
			// saveButton
			// 
			this.saveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.saveButton.Enabled = false;
			this.saveButton.Image = global::River.OneMoreAddIn.Properties.Resources.SaveAs;
			this.saveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = new System.Drawing.Size(34, 28);
			this.saveButton.Text = "Save";
			this.saveButton.Click += new System.EventHandler(this.SaveTheme);
			// 
			// renameButton
			// 
			this.renameButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.renameButton.Image = global::River.OneMoreAddIn.Properties.Resources.Rename;
			this.renameButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.renameButton.Name = "renameButton";
			this.renameButton.Size = new System.Drawing.Size(34, 28);
			this.renameButton.Text = "Rename";
			this.renameButton.Click += new System.EventHandler(this.RenameTheme);
			// 
			// deleteButton
			// 
			this.deleteButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.deleteButton.Image = global::River.OneMoreAddIn.Properties.Resources.Delete;
			this.deleteButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.Size = new System.Drawing.Size(34, 28);
			this.deleteButton.Text = "Delete";
			this.deleteButton.Click += new System.EventHandler(this.DeleteTheme);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 33);
			// 
			// EditTableThemesDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(763, 580);
			this.Controls.Add(this.toolstrip);
			this.Controls.Add(this.previewGroup);
			this.Controls.Add(this.elementsGroup);
			this.Controls.Add(this.nameLabel);
			this.Controls.Add(this.combo);
			this.Controls.Add(this.cancelButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "EditTableThemesDialog";
			this.Padding = new System.Windows.Forms.Padding(20, 20, 0, 0);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Custom Table Styles";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConfirmClosing);
			this.elementsGroup.ResumeLayout(false);
			this.previewGroup.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.previewBox)).EndInit();
			this.toolstrip.ResumeLayout(false);
			this.toolstrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.ComboBox combo;
		private System.Windows.Forms.Label nameLabel;
		private System.Windows.Forms.GroupBox elementsGroup;
		private River.OneMoreAddIn.UI.MoreListView elementsBox;
		private System.Windows.Forms.GroupBox previewGroup;
		private System.Windows.Forms.ToolStrip toolstrip;
		private System.Windows.Forms.ToolStripButton deleteButton;
		private System.Windows.Forms.Button resetButton;
		private System.Windows.Forms.PictureBox previewBox;
		private System.Windows.Forms.ToolStripButton saveButton;
		private System.Windows.Forms.ToolStripButton newButton;
		private System.Windows.Forms.ToolStripButton renameButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
	}
}