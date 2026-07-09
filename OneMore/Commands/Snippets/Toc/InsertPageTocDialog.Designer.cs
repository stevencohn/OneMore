namespace River.OneMoreAddIn.Commands
{
	partial class InsertPageTocDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InsertPageTocDialog));
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.topBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.rightAlignBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.secondaryBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.todoLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.todoBox = new River.OneMoreAddIn.UI.MoreComboBox();
			this.locationLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.locationBox = new River.OneMoreAddIn.UI.MoreComboBox();
			this.styleLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.styleBox = new River.OneMoreAddIn.UI.MoreComboBox();
			this.levelsLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.levelsBox = new River.OneMoreAddIn.UI.MoreNumericUpDown();
			((System.ComponentModel.ISupportInitialize)(this.levelsBox)).BeginInit();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(269, 322);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(120, 38);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 11;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.CollectParametersOnOK);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(397, 322);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(120, 38);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 12;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// topBox
			// 
			this.topBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.topBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.topBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.topBox.Location = new System.Drawing.Point(23, 29);
			this.topBox.Name = "topBox";
			this.topBox.Size = new System.Drawing.Size(378, 25);
			this.topBox.StylizeImage = false;
			this.topBox.TabIndex = 0;
			this.topBox.Text = "Add link to each heading to jump to top of page";
			this.topBox.ThemedBack = null;
			this.topBox.ThemedFore = null;
			this.topBox.UseVisualStyleBackColor = true;
			this.topBox.CheckedChanged += new System.EventHandler(this.ToggleTopBox);
			// 
			// rightAlignBox
			// 
			this.rightAlignBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.rightAlignBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.rightAlignBox.Enabled = false;
			this.rightAlignBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.rightAlignBox.Location = new System.Drawing.Point(49, 59);
			this.rightAlignBox.Name = "rightAlignBox";
			this.rightAlignBox.Size = new System.Drawing.Size(230, 25);
			this.rightAlignBox.StylizeImage = false;
			this.rightAlignBox.TabIndex = 1;
			this.rightAlignBox.Text = "Right-align top of page link";
			this.rightAlignBox.ThemedBack = null;
			this.rightAlignBox.ThemedFore = null;
			this.rightAlignBox.UseVisualStyleBackColor = true;
			// 
			// secondaryBox
			// 
			this.secondaryBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.secondaryBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.secondaryBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.secondaryBox.Location = new System.Drawing.Point(23, 97);
			this.secondaryBox.Name = "secondaryBox";
			this.secondaryBox.Size = new System.Drawing.Size(311, 25);
			this.secondaryBox.StylizeImage = false;
			this.secondaryBox.TabIndex = 2;
			this.secondaryBox.Text = "Include headings from tables and lists";
			this.secondaryBox.ThemedBack = null;
			this.secondaryBox.ThemedFore = null;
			this.secondaryBox.UseVisualStyleBackColor = true;
			// 
			// todoLabel
			// 
			this.todoLabel.AutoSize = true;
			this.todoLabel.Location = new System.Drawing.Point(23, 145);
			this.todoLabel.Name = "todoLabel";
			this.todoLabel.Size = new System.Drawing.Size(153, 20);
			this.todoLabel.TabIndex = 3;
			this.todoLabel.Text = "Mark Todo headings";
			this.todoLabel.ThemedBack = null;
			this.todoLabel.ThemedFore = null;
			// 
			// todoBox
			// 
			this.todoBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.todoBox.FormattingEnabled = true;
			this.todoBox.ItemHeight = 24;
			this.todoBox.Location = new System.Drawing.Point(253, 142);
			this.todoBox.Name = "todoBox";
			this.todoBox.Size = new System.Drawing.Size(251, 30);
			this.todoBox.TabIndex = 4;
			this.todoBox.ThemedBack = null;
			this.todoBox.ThemedFore = null;
			// 
			// locationLabel
			// 
			this.locationLabel.AutoSize = true;
			this.locationLabel.Location = new System.Drawing.Point(23, 188);
			this.locationLabel.Name = "locationLabel";
			this.locationLabel.Size = new System.Drawing.Size(115, 20);
			this.locationLabel.TabIndex = 5;
			this.locationLabel.Text = "Insert Location";
			this.locationLabel.ThemedBack = null;
			this.locationLabel.ThemedFore = null;
			// 
			// locationBox
			// 
			this.locationBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.locationBox.FormattingEnabled = true;
			this.locationBox.Items.AddRange(new object[] {
            "At top of page",
            "At top of page, overlayed",
            "At current cursor"});
			this.locationBox.Location = new System.Drawing.Point(253, 182);
			this.locationBox.Name = "locationBox";
			this.locationBox.Size = new System.Drawing.Size(251, 27);
			this.locationBox.TabIndex = 6;
			this.locationBox.ThemedBack = null;
			this.locationBox.ThemedFore = null;
			// 
			// styleLabel
			// 
			this.styleLabel.AutoSize = true;
			this.styleLabel.Location = new System.Drawing.Point(23, 223);
			this.styleLabel.Name = "styleLabel";
			this.styleLabel.Size = new System.Drawing.Size(77, 20);
			this.styleLabel.TabIndex = 7;
			this.styleLabel.Text = "Title Style";
			this.styleLabel.ThemedBack = null;
			this.styleLabel.ThemedFore = null;
			// 
			// styleBox
			// 
			this.styleBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.styleBox.FormattingEnabled = true;
			this.styleBox.Items.AddRange(new object[] {
            "Standard Page Title",
            "Standard Heading 1",
            "Standard Heading 2",
            "Standard Heading 3",
            "Custom Page Title",
            "Custom Heading 1",
            "Custom Heading 2",
            "Custom Heading 3"});
			this.styleBox.Location = new System.Drawing.Point(253, 220);
			this.styleBox.Name = "styleBox";
			this.styleBox.Size = new System.Drawing.Size(251, 27);
			this.styleBox.TabIndex = 8;
			this.styleBox.ThemedBack = null;
			this.styleBox.ThemedFore = null;
			// 
			// levelsLabel
			// 
			this.levelsLabel.AutoSize = true;
			this.levelsLabel.Location = new System.Drawing.Point(23, 263);
			this.levelsLabel.Name = "levelsLabel";
			this.levelsLabel.Size = new System.Drawing.Size(98, 20);
			this.levelsLabel.TabIndex = 9;
			this.levelsLabel.Text = "Show Levels";
			this.levelsLabel.ThemedBack = null;
			this.levelsLabel.ThemedFore = null;
			// 
			// levelsBox
			// 
			this.levelsBox.Location = new System.Drawing.Point(253, 258);
			this.levelsBox.Maximum = new decimal(new int[] {
            6,
            0,
            0,
            0});
			this.levelsBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.levelsBox.Name = "levelsBox";
			this.levelsBox.Size = new System.Drawing.Size(96, 26);
			this.levelsBox.TabIndex = 10;
			this.levelsBox.ThemedBack = null;
			this.levelsBox.ThemedFore = "ControlText";
			this.levelsBox.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// InsertPageTocDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(529, 380);
			this.Controls.Add(this.levelsLabel);
			this.Controls.Add(this.levelsBox);
			this.Controls.Add(this.styleLabel);
			this.Controls.Add(this.styleBox);
			this.Controls.Add(this.locationLabel);
			this.Controls.Add(this.locationBox);
			this.Controls.Add(this.todoLabel);
			this.Controls.Add(this.todoBox);
			this.Controls.Add(this.secondaryBox);
			this.Controls.Add(this.rightAlignBox);
			this.Controls.Add(this.topBox);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "InsertPageTocDialog";
			this.Padding = new System.Windows.Forms.Padding(20, 25, 0, 0);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Insert Table of Contents";
			((System.ComponentModel.ISupportInitialize)(this.levelsBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private UI.MoreCheckBox topBox;
		private UI.MoreCheckBox rightAlignBox;
		private UI.MoreCheckBox secondaryBox;
		private UI.MoreLabel todoLabel;
		private UI.MoreComboBox todoBox;
		private UI.MoreLabel locationLabel;
		private UI.MoreComboBox locationBox;
		private UI.MoreLabel styleLabel;
		private UI.MoreComboBox styleBox;
		private UI.MoreLabel levelsLabel;
		private UI.MoreNumericUpDown levelsBox;
	}
}
