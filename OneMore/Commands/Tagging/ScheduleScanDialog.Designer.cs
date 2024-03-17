namespace River.OneMoreAddIn.Commands
{
	partial class ScheduleScanDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScheduleScanDialog));
			this.topPanel = new System.Windows.Forms.Panel();
			this.warningLabel = new River.OneMoreAddIn.UI.MoreMultilineLabel();
			this.nowRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.laterRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.hintLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.scheduleLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.dateTimePicker = new System.Windows.Forms.DateTimePicker();
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.introBox = new River.OneMoreAddIn.UI.MoreMultilineLabel();
			this.topPanel.SuspendLayout();
			this.buttonPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// topPanel
			// 
			this.topPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.topPanel.Controls.Add(this.warningLabel);
			this.topPanel.Controls.Add(this.nowRadio);
			this.topPanel.Controls.Add(this.laterRadio);
			this.topPanel.Controls.Add(this.hintLabel);
			this.topPanel.Controls.Add(this.scheduleLabel);
			this.topPanel.Controls.Add(this.dateTimePicker);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.topPanel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.topPanel.Location = new System.Drawing.Point(0, 62);
			this.topPanel.Margin = new System.Windows.Forms.Padding(0);
			this.topPanel.Name = "topPanel";
			this.topPanel.Padding = new System.Windows.Forms.Padding(40, 40, 20, 20);
			this.topPanel.Size = new System.Drawing.Size(698, 315);
			this.topPanel.TabIndex = 4;
			// 
			// warningLabel
			// 
			this.warningLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.warningLabel.Location = new System.Drawing.Point(70, 216);
			this.warningLabel.Name = "warningLabel";
			this.warningLabel.Size = new System.Drawing.Size(605, 46);
			this.warningLabel.TabIndex = 6;
			this.warningLabel.Text = "OneNote may appear sluggish while scanning. If OneNote is closed, it cannot be op" +
    "ened until the scan completes.";
			this.warningLabel.ThemedBack = null;
			this.warningLabel.ThemedFore = null;
			// 
			// nowRadio
			// 
			this.nowRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.nowRadio.Location = new System.Drawing.Point(43, 185);
			this.nowRadio.Name = "nowRadio";
			this.nowRadio.Size = new System.Drawing.Size(370, 25);
			this.nowRadio.TabIndex = 5;
			this.nowRadio.Text = "Run the san now";
			this.nowRadio.UseVisualStyleBackColor = true;
			// 
			// laterRadio
			// 
			this.laterRadio.Checked = true;
			this.laterRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.laterRadio.Location = new System.Drawing.Point(43, 43);
			this.laterRadio.Name = "laterRadio";
			this.laterRadio.Size = new System.Drawing.Size(492, 25);
			this.laterRadio.TabIndex = 4;
			this.laterRadio.TabStop = true;
			this.laterRadio.Text = "Schedule the scan at a later time";
			this.laterRadio.UseVisualStyleBackColor = true;
			// 
			// hintLabel
			// 
			this.hintLabel.AutoSize = true;
			this.hintLabel.Location = new System.Drawing.Point(71, 71);
			this.hintLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 12);
			this.hintLabel.Name = "hintLabel";
			this.hintLabel.Size = new System.Drawing.Size(463, 20);
			this.hintLabel.TabIndex = 3;
			this.hintLabel.Text = "It is best to schedule the scan during off-hours, such as midnight\r\n";
			this.hintLabel.ThemedBack = null;
			this.hintLabel.ThemedFore = null;
			// 
			// scheduleLabel
			// 
			this.scheduleLabel.AutoSize = true;
			this.scheduleLabel.Location = new System.Drawing.Point(71, 120);
			this.scheduleLabel.Name = "scheduleLabel";
			this.scheduleLabel.Size = new System.Drawing.Size(76, 20);
			this.scheduleLabel.TabIndex = 2;
			this.scheduleLabel.Text = "Schedule";
			this.scheduleLabel.ThemedBack = null;
			this.scheduleLabel.ThemedFore = null;
			// 
			// dateTimePicker
			// 
			this.dateTimePicker.CustomFormat = "ddd, MMMM d, yyyy h:mm tt";
			this.dateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.dateTimePicker.Location = new System.Drawing.Point(257, 115);
			this.dateTimePicker.Name = "dateTimePicker";
			this.dateTimePicker.Size = new System.Drawing.Size(379, 26);
			this.dateTimePicker.TabIndex = 1;
			// 
			// buttonPanel
			// 
			this.buttonPanel.BackColor = System.Drawing.SystemColors.Control;
			this.buttonPanel.Controls.Add(this.cancelButton);
			this.buttonPanel.Controls.Add(this.okButton);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.buttonPanel.Location = new System.Drawing.Point(0, 377);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Size = new System.Drawing.Size(698, 61);
			this.buttonPanel.TabIndex = 5;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(571, 13);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(115, 36);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(450, 13);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(115, 36);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// introBox
			// 
			this.introBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.introBox.Location = new System.Drawing.Point(0, 0);
			this.introBox.Name = "introBox";
			this.introBox.Padding = new System.Windows.Forms.Padding(30, 12, 20, 12);
			this.introBox.Size = new System.Drawing.Size(698, 62);
			this.introBox.TabIndex = 11;
			this.introBox.Text = "The hashtag catalog has not yet been created. Choose below when OneMore should bu" +
    "ild the catalog.";
			this.introBox.ThemedBack = "Control";
			this.introBox.ThemedFore = "ControlText";
			// 
			// ScheduleScanDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(698, 438);
			this.Controls.Add(this.topPanel);
			this.Controls.Add(this.introBox);
			this.Controls.Add(this.buttonPanel);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ScheduleScanDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Schedule Hashtag Scanning";
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.buttonPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel topPanel;
		private System.Windows.Forms.Panel buttonPanel;
		private River.OneMoreAddIn.UI.MoreButton cancelButton;
		private River.OneMoreAddIn.UI.MoreButton okButton;
		private System.Windows.Forms.DateTimePicker dateTimePicker;
		private River.OneMoreAddIn.UI.MoreLabel scheduleLabel;
		private River.OneMoreAddIn.UI.MoreLabel hintLabel;
		private UI.MoreRadioButton laterRadio;
		private UI.MoreRadioButton nowRadio;
		private UI.MoreMultilineLabel introBox;
		private UI.MoreMultilineLabel warningLabel;
	}
}