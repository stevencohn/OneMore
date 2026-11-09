
namespace River.OneMoreAddIn.Settings
{
	partial class BackupSheet
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
			this.statusLabel = new System.Windows.Forms.Label();
			this.noteLabel = new System.Windows.Forms.Label();
			this.backupNowButton = new River.OneMoreAddIn.UI.MoreButton();
			this.keepBox = new System.Windows.Forms.NumericUpDown();
			this.keepLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.intervalBox = new River.OneMoreAddIn.UI.MoreComboBox();
			this.intervalLabel = new System.Windows.Forms.Label();
			this.folderButton = new River.OneMoreAddIn.UI.MoreButton();
			this.folderBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.customRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.oneNoteLabel = new System.Windows.Forms.Label();
			this.oneNoteRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.enableBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.layoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.keepBox)).BeginInit();
			this.SuspendLayout();
			//
			// introBox
			//
			this.introBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.introBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.introBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.introBox.Location = new System.Drawing.Point(13, 8);
			this.introBox.Name = "introBox";
			this.introBox.Padding = new System.Windows.Forms.Padding(0, 0, 0, 36);
			this.introBox.Size = new System.Drawing.Size(772, 66);
			this.introBox.TabIndex = 0;
			this.introBox.Text = "Periodically back up the OneMore database (OneMore.db), which holds your hashtags, favorites, layouts and search index.";
			this.introBox.ThemedBack = "ControlLightLight";
			this.introBox.ThemedFore = "ControlText";
			//
			// layoutPanel
			//
			this.layoutPanel.Controls.Add(this.noteLabel);
			this.layoutPanel.Controls.Add(this.statusLabel);
			this.layoutPanel.Controls.Add(this.backupNowButton);
			this.layoutPanel.Controls.Add(this.keepBox);
			this.layoutPanel.Controls.Add(this.keepLabel);
			this.layoutPanel.Controls.Add(this.intervalBox);
			this.layoutPanel.Controls.Add(this.intervalLabel);
			this.layoutPanel.Controls.Add(this.folderButton);
			this.layoutPanel.Controls.Add(this.folderBox);
			this.layoutPanel.Controls.Add(this.customRadio);
			this.layoutPanel.Controls.Add(this.oneNoteLabel);
			this.layoutPanel.Controls.Add(this.oneNoteRadio);
			this.layoutPanel.Controls.Add(this.enableBox);
			this.layoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.layoutPanel.Location = new System.Drawing.Point(13, 74);
			this.layoutPanel.Margin = new System.Windows.Forms.Padding(0);
			this.layoutPanel.Name = "layoutPanel";
			this.layoutPanel.Size = new System.Drawing.Size(772, 416);
			this.layoutPanel.TabIndex = 1;
			//
			// statusLabel
			//
			this.statusLabel.AutoSize = true;
			this.statusLabel.Location = new System.Drawing.Point(190, 342);
			this.statusLabel.Name = "statusLabel";
			this.statusLabel.Size = new System.Drawing.Size(0, 20);
			this.statusLabel.TabIndex = 11;
			//
			// noteLabel
			//
			this.noteLabel.AutoSize = true;
			this.noteLabel.Location = new System.Drawing.Point(7, 385);
			this.noteLabel.MaximumSize = new System.Drawing.Size(740, 0);
			this.noteLabel.Name = "noteLabel";
			this.noteLabel.Size = new System.Drawing.Size(0, 20);
			this.noteLabel.TabIndex = 12;
			this.noteLabel.Visible = false;
			//
			// backupNowButton
			//
			this.backupNowButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.backupNowButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.backupNowButton.ImageOver = null;
			this.backupNowButton.Location = new System.Drawing.Point(7, 335);
			this.backupNowButton.Name = "backupNowButton";
			this.backupNowButton.ShowBorder = true;
			this.backupNowButton.Size = new System.Drawing.Size(170, 35);
			this.backupNowButton.StylizeImage = false;
			this.backupNowButton.TabIndex = 10;
			this.backupNowButton.Text = "Back up now";
			this.backupNowButton.ThemedBack = null;
			this.backupNowButton.ThemedFore = null;
			this.backupNowButton.UseVisualStyleBackColor = true;
			this.backupNowButton.Click += new System.EventHandler(this.BackupNow);
			//
			// keepBox
			//
			this.keepBox.Location = new System.Drawing.Point(347, 277);
			this.keepBox.Maximum = new decimal(new int[] {
            40,
            0,
            0,
            0});
			this.keepBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.keepBox.Name = "keepBox";
			this.keepBox.Size = new System.Drawing.Size(120, 26);
			this.keepBox.TabIndex = 9;
			this.keepBox.Value = new decimal(new int[] {
            7,
            0,
            0,
            0});
			//
			// keepLabel
			//
			this.keepLabel.AutoSize = true;
			this.keepLabel.Location = new System.Drawing.Point(7, 279);
			this.keepLabel.Name = "keepLabel";
			this.keepLabel.Size = new System.Drawing.Size(235, 20);
			this.keepLabel.TabIndex = 8;
			this.keepLabel.Text = "Number of backup copies to keep";
			this.keepLabel.Scrubber = this.keepBox;
			//
			// intervalBox
			//
			this.intervalBox.BackColor = System.Drawing.SystemColors.Control;
			this.intervalBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.intervalBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.intervalBox.FormattingEnabled = true;
			this.intervalBox.Location = new System.Drawing.Point(347, 227);
			this.intervalBox.Name = "intervalBox";
			this.intervalBox.Size = new System.Drawing.Size(220, 28);
			this.intervalBox.TabIndex = 7;
			//
			// intervalLabel
			//
			this.intervalLabel.AutoSize = true;
			this.intervalLabel.Location = new System.Drawing.Point(7, 231);
			this.intervalLabel.Name = "intervalLabel";
			this.intervalLabel.Size = new System.Drawing.Size(55, 20);
			this.intervalLabel.TabIndex = 6;
			this.intervalLabel.Text = "Back up interval";
			//
			// folderButton
			//
			this.folderButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.folderButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.folderButton.ImageOver = null;
			this.folderButton.Location = new System.Drawing.Point(717, 168);
			this.folderButton.Name = "folderButton";
			this.folderButton.ShowBorder = true;
			this.folderButton.Size = new System.Drawing.Size(36, 31);
			this.folderButton.StylizeImage = false;
			this.folderButton.TabIndex = 5;
			this.folderButton.Text = "...";
			this.folderButton.ThemedBack = null;
			this.folderButton.ThemedFore = null;
			this.folderButton.UseVisualStyleBackColor = true;
			this.folderButton.Click += new System.EventHandler(this.BrowseFolder);
			//
			// folderBox
			//
			this.folderBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.folderBox.Location = new System.Drawing.Point(37, 170);
			this.folderBox.Name = "folderBox";
			this.folderBox.ProcessEnterKey = false;
			this.folderBox.Size = new System.Drawing.Size(673, 26);
			this.folderBox.TabIndex = 4;
			this.folderBox.ThemedBack = null;
			this.folderBox.ThemedFore = null;
			//
			// customRadio
			//
			this.customRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.customRadio.Location = new System.Drawing.Point(18, 135);
			this.customRadio.Name = "customRadio";
			this.customRadio.Size = new System.Drawing.Size(400, 25);
			this.customRadio.TabIndex = 3;
			this.customRadio.Text = "Use a custom folder";
			this.customRadio.UseVisualStyleBackColor = true;
			this.customRadio.CheckedChanged += new System.EventHandler(this.ChangeState);
			//
			// oneNoteLabel
			//
			this.oneNoteLabel.AutoSize = true;
			this.oneNoteLabel.Location = new System.Drawing.Point(37, 98);
			this.oneNoteLabel.Name = "oneNoteLabel";
			this.oneNoteLabel.Size = new System.Drawing.Size(0, 20);
			this.oneNoteLabel.TabIndex = 2;
			//
			// oneNoteRadio
			//
			this.oneNoteRadio.Checked = true;
			this.oneNoteRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.oneNoteRadio.Location = new System.Drawing.Point(18, 68);
			this.oneNoteRadio.Name = "oneNoteRadio";
			this.oneNoteRadio.Size = new System.Drawing.Size(400, 25);
			this.oneNoteRadio.TabIndex = 1;
			this.oneNoteRadio.TabStop = true;
			this.oneNoteRadio.Text = "Use the OneNote backup folder";
			this.oneNoteRadio.UseVisualStyleBackColor = true;
			this.oneNoteRadio.CheckedChanged += new System.EventHandler(this.ChangeState);
			//
			// enableBox
			//
			this.enableBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.enableBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.enableBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.enableBox.Location = new System.Drawing.Point(7, 10);
			this.enableBox.Name = "enableBox";
			this.enableBox.Size = new System.Drawing.Size(400, 25);
			this.enableBox.StylizeImage = false;
			this.enableBox.TabIndex = 0;
			this.enableBox.Text = "Back up the OneMore database";
			this.enableBox.ThemedBack = null;
			this.enableBox.ThemedFore = null;
			this.enableBox.UseVisualStyleBackColor = false;
			this.enableBox.CheckedChanged += new System.EventHandler(this.ChangeState);
			//
			// BackupSheet
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.layoutPanel);
			this.Controls.Add(this.introBox);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Name = "BackupSheet";
			this.Padding = new System.Windows.Forms.Padding(13, 8, 15, 10);
			this.Size = new System.Drawing.Size(800, 500);
			this.layoutPanel.ResumeLayout(false);
			this.layoutPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.keepBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreMultilineLabel introBox;
		private System.Windows.Forms.Panel layoutPanel;
		private UI.MoreCheckBox enableBox;
		private UI.MoreRadioButton oneNoteRadio;
		private System.Windows.Forms.Label oneNoteLabel;
		private UI.MoreRadioButton customRadio;
		private UI.MoreTextBox folderBox;
		private UI.MoreButton folderButton;
		private System.Windows.Forms.Label intervalLabel;
		private UI.MoreComboBox intervalBox;
		private UI.MoreLabel keepLabel;
		private System.Windows.Forms.NumericUpDown keepBox;
		private UI.MoreButton backupNowButton;
		private System.Windows.Forms.Label statusLabel;
		private System.Windows.Forms.Label noteLabel;
	}
}
