﻿
namespace River.OneMoreAddIn.Commands
{
	partial class ReminderDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReminderDialog));
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.startDateBox = new System.Windows.Forms.DateTimePicker();
			this.dueDateBox = new System.Windows.Forms.DateTimePicker();
			this.startDateLabel = new System.Windows.Forms.Label();
			this.dueDateLabel = new System.Windows.Forms.Label();
			this.subjectLabel = new System.Windows.Forms.Label();
			this.subjectBox = new System.Windows.Forms.TextBox();
			this.statusLabel = new System.Windows.Forms.Label();
			this.statusBox = new System.Windows.Forms.ComboBox();
			this.priorityBox = new System.Windows.Forms.ComboBox();
			this.priorityLabel = new System.Windows.Forms.Label();
			this.percentLabel = new System.Windows.Forms.Label();
			this.percentBox = new System.Windows.Forms.NumericUpDown();
			this.startedBox = new System.Windows.Forms.TextBox();
			this.startedLabel = new System.Windows.Forms.Label();
			this.completedLabel = new System.Windows.Forms.Label();
			this.completedBox = new System.Windows.Forms.TextBox();
			this.tagButton = new System.Windows.Forms.Button();
			this.tagLabel = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.percentBox)).BeginInit();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Enabled = false;
			this.okButton.Location = new System.Drawing.Point(624, 401);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.TabIndex = 9;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OK);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(730, 401);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.TabIndex = 8;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// startDateBox
			// 
			this.startDateBox.CustomFormat = "dddd, MMMM d, yyyy h:mm tt";
			this.startDateBox.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.startDateBox.Location = new System.Drawing.Point(145, 120);
			this.startDateBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.startDateBox.Name = "startDateBox";
			this.startDateBox.Size = new System.Drawing.Size(434, 26);
			this.startDateBox.TabIndex = 10;
			// 
			// dueDateBox
			// 
			this.dueDateBox.CustomFormat = "dddd, MMMM d, yyyy h:mm tt";
			this.dueDateBox.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.dueDateBox.Location = new System.Drawing.Point(145, 198);
			this.dueDateBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.dueDateBox.Name = "dueDateBox";
			this.dueDateBox.Size = new System.Drawing.Size(434, 26);
			this.dueDateBox.TabIndex = 11;
			// 
			// startDateLabel
			// 
			this.startDateLabel.AutoSize = true;
			this.startDateLabel.Location = new System.Drawing.Point(23, 125);
			this.startDateLabel.Name = "startDateLabel";
			this.startDateLabel.Size = new System.Drawing.Size(80, 20);
			this.startDateLabel.TabIndex = 12;
			this.startDateLabel.Text = "Start date";
			// 
			// dueDateLabel
			// 
			this.dueDateLabel.AutoSize = true;
			this.dueDateLabel.Location = new System.Drawing.Point(23, 203);
			this.dueDateLabel.Name = "dueDateLabel";
			this.dueDateLabel.Size = new System.Drawing.Size(75, 20);
			this.dueDateLabel.TabIndex = 13;
			this.dueDateLabel.Text = "Due date";
			// 
			// subjectLabel
			// 
			this.subjectLabel.AutoSize = true;
			this.subjectLabel.Location = new System.Drawing.Point(23, 84);
			this.subjectLabel.Name = "subjectLabel";
			this.subjectLabel.Size = new System.Drawing.Size(63, 20);
			this.subjectLabel.TabIndex = 14;
			this.subjectLabel.Text = "Subject";
			// 
			// subjectBox
			// 
			this.subjectBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.subjectBox.Location = new System.Drawing.Point(145, 81);
			this.subjectBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.subjectBox.MaxLength = 200;
			this.subjectBox.Name = "subjectBox";
			this.subjectBox.Size = new System.Drawing.Size(685, 26);
			this.subjectBox.TabIndex = 15;
			this.subjectBox.TextChanged += new System.EventHandler(this.ChangeSubject);
			// 
			// statusLabel
			// 
			this.statusLabel.AutoSize = true;
			this.statusLabel.Location = new System.Drawing.Point(23, 279);
			this.statusLabel.Name = "statusLabel";
			this.statusLabel.Size = new System.Drawing.Size(56, 20);
			this.statusLabel.TabIndex = 16;
			this.statusLabel.Text = "Status";
			// 
			// statusBox
			// 
			this.statusBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.statusBox.FormattingEnabled = true;
			this.statusBox.Items.AddRange(new object[] {
            "Not started",
            "In progress",
            "Completed",
            "Waiting on someone else",
            "Deferred"});
			this.statusBox.Location = new System.Drawing.Point(145, 276);
			this.statusBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.statusBox.Name = "statusBox";
			this.statusBox.Size = new System.Drawing.Size(308, 28);
			this.statusBox.TabIndex = 17;
			this.statusBox.SelectedIndexChanged += new System.EventHandler(this.ChangeStatus);
			// 
			// priorityBox
			// 
			this.priorityBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.priorityBox.FormattingEnabled = true;
			this.priorityBox.Items.AddRange(new object[] {
            "Low",
            "Medium",
            "High"});
			this.priorityBox.Location = new System.Drawing.Point(145, 317);
			this.priorityBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.priorityBox.Name = "priorityBox";
			this.priorityBox.Size = new System.Drawing.Size(242, 28);
			this.priorityBox.TabIndex = 18;
			// 
			// priorityLabel
			// 
			this.priorityLabel.AutoSize = true;
			this.priorityLabel.Location = new System.Drawing.Point(23, 320);
			this.priorityLabel.Name = "priorityLabel";
			this.priorityLabel.Size = new System.Drawing.Size(56, 20);
			this.priorityLabel.TabIndex = 19;
			this.priorityLabel.Text = "Priority";
			// 
			// percentLabel
			// 
			this.percentLabel.AutoSize = true;
			this.percentLabel.Location = new System.Drawing.Point(23, 360);
			this.percentLabel.Name = "percentLabel";
			this.percentLabel.Size = new System.Drawing.Size(95, 20);
			this.percentLabel.TabIndex = 20;
			this.percentLabel.Text = "% Complete";
			// 
			// percentBox
			// 
			this.percentBox.Increment = new decimal(new int[] {
            25,
            0,
            0,
            0});
			this.percentBox.Location = new System.Drawing.Point(145, 358);
			this.percentBox.Name = "percentBox";
			this.percentBox.Size = new System.Drawing.Size(92, 26);
			this.percentBox.TabIndex = 21;
			this.percentBox.ValueChanged += new System.EventHandler(this.ChangePercent);
			// 
			// startedBox
			// 
			this.startedBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.startedBox.Location = new System.Drawing.Point(145, 159);
			this.startedBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.startedBox.MaxLength = 200;
			this.startedBox.Name = "startedBox";
			this.startedBox.ReadOnly = true;
			this.startedBox.Size = new System.Drawing.Size(434, 19);
			this.startedBox.TabIndex = 22;
			// 
			// startedLabel
			// 
			this.startedLabel.AutoSize = true;
			this.startedLabel.Location = new System.Drawing.Point(23, 162);
			this.startedLabel.Name = "startedLabel";
			this.startedLabel.Size = new System.Drawing.Size(62, 20);
			this.startedLabel.TabIndex = 23;
			this.startedLabel.Text = "Started";
			// 
			// completedLabel
			// 
			this.completedLabel.AutoSize = true;
			this.completedLabel.Location = new System.Drawing.Point(23, 240);
			this.completedLabel.Name = "completedLabel";
			this.completedLabel.Size = new System.Drawing.Size(86, 20);
			this.completedLabel.TabIndex = 25;
			this.completedLabel.Text = "Completed";
			// 
			// completedBox
			// 
			this.completedBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.completedBox.Location = new System.Drawing.Point(145, 237);
			this.completedBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.completedBox.MaxLength = 200;
			this.completedBox.Name = "completedBox";
			this.completedBox.ReadOnly = true;
			this.completedBox.Size = new System.Drawing.Size(434, 19);
			this.completedBox.TabIndex = 24;
			// 
			// tagButton
			// 
			this.tagButton.BackColor = System.Drawing.SystemColors.Window;
			this.tagButton.Location = new System.Drawing.Point(145, 30);
			this.tagButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 10);
			this.tagButton.Name = "tagButton";
			this.tagButton.Size = new System.Drawing.Size(60, 38);
			this.tagButton.TabIndex = 26;
			this.tagButton.Text = "?";
			this.tagButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.tagButton.UseVisualStyleBackColor = false;
			this.tagButton.Click += new System.EventHandler(this.SelectTag);
			// 
			// tagLabel
			// 
			this.tagLabel.AutoSize = true;
			this.tagLabel.Location = new System.Drawing.Point(23, 39);
			this.tagLabel.Name = "tagLabel";
			this.tagLabel.Size = new System.Drawing.Size(36, 20);
			this.tagLabel.TabIndex = 27;
			this.tagLabel.Text = "Tag";
			// 
			// ReminderDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(853, 462);
			this.Controls.Add(this.tagLabel);
			this.Controls.Add(this.tagButton);
			this.Controls.Add(this.completedLabel);
			this.Controls.Add(this.completedBox);
			this.Controls.Add(this.startedLabel);
			this.Controls.Add(this.startedBox);
			this.Controls.Add(this.percentBox);
			this.Controls.Add(this.percentLabel);
			this.Controls.Add(this.priorityLabel);
			this.Controls.Add(this.priorityBox);
			this.Controls.Add(this.statusBox);
			this.Controls.Add(this.statusLabel);
			this.Controls.Add(this.subjectBox);
			this.Controls.Add(this.subjectLabel);
			this.Controls.Add(this.dueDateLabel);
			this.Controls.Add(this.startDateLabel);
			this.Controls.Add(this.dueDateBox);
			this.Controls.Add(this.startDateBox);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ReminderDialog";
			this.Padding = new System.Windows.Forms.Padding(20, 25, 20, 20);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Reminder";
			((System.ComponentModel.ISupportInitialize)(this.percentBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.DateTimePicker startDateBox;
		private System.Windows.Forms.DateTimePicker dueDateBox;
		private System.Windows.Forms.Label startDateLabel;
		private System.Windows.Forms.Label dueDateLabel;
		private System.Windows.Forms.Label subjectLabel;
		private System.Windows.Forms.TextBox subjectBox;
		private System.Windows.Forms.Label statusLabel;
		private System.Windows.Forms.ComboBox statusBox;
		private System.Windows.Forms.ComboBox priorityBox;
		private System.Windows.Forms.Label priorityLabel;
		private System.Windows.Forms.Label percentLabel;
		private System.Windows.Forms.NumericUpDown percentBox;
		private System.Windows.Forms.TextBox startedBox;
		private System.Windows.Forms.Label startedLabel;
		private System.Windows.Forms.Label completedLabel;
		private System.Windows.Forms.TextBox completedBox;
		private System.Windows.Forms.Button tagButton;
		private System.Windows.Forms.Label tagLabel;
	}
}