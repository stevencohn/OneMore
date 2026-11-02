namespace River.OneMoreAddIn.Commands
{
	partial class CustomSortDialog
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
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.topGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.columnLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.columnBox = new River.OneMoreAddIn.UI.MoreComboBox();
			this.orderLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.orderBox = new River.OneMoreAddIn.UI.MoreComboBox();
			this.typeLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.typeBox = new River.OneMoreAddIn.UI.MoreComboBox();
			this.addButton = new River.OneMoreAddIn.UI.MoreButton();
			this.rulesLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.ruleList = new River.OneMoreAddIn.UI.MoreListView();
			this.numColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.nameColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.orderColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.typeColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.upButton = new River.OneMoreAddIn.UI.MoreButton();
			this.downButton = new River.OneMoreAddIn.UI.MoreButton();
			this.deleteButton = new River.OneMoreAddIn.UI.MoreButton();
			this.noteLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.topGroup.SuspendLayout();
			this.SuspendLayout();
			//
			// topGroup
			//
			this.topGroup.Controls.Add(this.columnLabel);
			this.topGroup.Controls.Add(this.columnBox);
			this.topGroup.Controls.Add(this.orderLabel);
			this.topGroup.Controls.Add(this.orderBox);
			this.topGroup.Controls.Add(this.typeLabel);
			this.topGroup.Controls.Add(this.typeBox);
			this.topGroup.Controls.Add(this.addButton);
			this.topGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.topGroup.Location = new System.Drawing.Point(30, 20);
			this.topGroup.Name = "topGroup";
			this.topGroup.Size = new System.Drawing.Size(840, 110);
			this.topGroup.TabIndex = 0;
			this.topGroup.TabStop = false;
			//
			// columnLabel
			//
			this.columnLabel.AutoSize = true;
			this.columnLabel.Location = new System.Drawing.Point(15, 22);
			this.columnLabel.Name = "columnLabel";
			this.columnLabel.Size = new System.Drawing.Size(58, 20);
			this.columnLabel.TabIndex = 0;
			this.columnLabel.Text = "Column";
			this.columnLabel.ThemedBack = null;
			this.columnLabel.ThemedFore = null;
			//
			// columnBox
			//
			this.columnBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.columnBox.FormattingEnabled = true;
			this.columnBox.Location = new System.Drawing.Point(15, 52);
			this.columnBox.Name = "columnBox";
			this.columnBox.Size = new System.Drawing.Size(260, 28);
			this.columnBox.TabIndex = 1;
			//
			// orderLabel
			//
			this.orderLabel.AutoSize = true;
			this.orderLabel.Location = new System.Drawing.Point(295, 22);
			this.orderLabel.Name = "orderLabel";
			this.orderLabel.Size = new System.Drawing.Size(77, 20);
			this.orderLabel.TabIndex = 2;
			this.orderLabel.Text = "Sort order";
			this.orderLabel.ThemedBack = null;
			this.orderLabel.ThemedFore = null;
			//
			// orderBox
			//
			this.orderBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.orderBox.FormattingEnabled = true;
			this.orderBox.Location = new System.Drawing.Point(295, 52);
			this.orderBox.Name = "orderBox";
			this.orderBox.Size = new System.Drawing.Size(170, 28);
			this.orderBox.TabIndex = 3;
			//
			// typeLabel
			//
			this.typeLabel.AutoSize = true;
			this.typeLabel.Location = new System.Drawing.Point(485, 22);
			this.typeLabel.Name = "typeLabel";
			this.typeLabel.Size = new System.Drawing.Size(80, 20);
			this.typeLabel.TabIndex = 4;
			this.typeLabel.Text = "Data type";
			this.typeLabel.ThemedBack = null;
			this.typeLabel.ThemedFore = null;
			//
			// typeBox
			//
			this.typeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.typeBox.FormattingEnabled = true;
			this.typeBox.Location = new System.Drawing.Point(485, 52);
			this.typeBox.Name = "typeBox";
			this.typeBox.Size = new System.Drawing.Size(190, 28);
			this.typeBox.TabIndex = 5;
			//
			// addButton
			//
			this.addButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.addButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.addButton.ImageOver = null;
			this.addButton.Location = new System.Drawing.Point(695, 47);
			this.addButton.Name = "addButton";
			this.addButton.ShowBorder = true;
			this.addButton.Size = new System.Drawing.Size(130, 38);
			this.addButton.TabIndex = 6;
			this.addButton.Text = "Add rule";
			this.addButton.ThemedBack = null;
			this.addButton.ThemedFore = null;
			this.addButton.UseVisualStyleBackColor = true;
			this.addButton.Click += new System.EventHandler(this.AddRule);
			//
			// rulesLabel
			//
			this.rulesLabel.AutoSize = true;
			this.rulesLabel.Location = new System.Drawing.Point(30, 148);
			this.rulesLabel.Name = "rulesLabel";
			this.rulesLabel.Size = new System.Drawing.Size(190, 20);
			this.rulesLabel.TabIndex = 1;
			this.rulesLabel.Text = "Sort rules (first is primary)";
			this.rulesLabel.ThemedBack = null;
			this.rulesLabel.ThemedFore = null;
			//
			// ruleList
			//
			this.ruleList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ruleList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.numColumn,
            this.nameColumn,
            this.orderColumn,
            this.typeColumn});
			this.ruleList.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ruleList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.ruleList.Location = new System.Drawing.Point(30, 178);
			this.ruleList.MultiSelect = false;
			this.ruleList.Name = "ruleList";
			this.ruleList.Size = new System.Drawing.Size(780, 360);
			this.ruleList.TabIndex = 2;
			this.ruleList.View = System.Windows.Forms.View.Details;
			this.ruleList.SelectedIndexChanged += new System.EventHandler(this.ChangeSelection);
			//
			// numColumn
			//
			this.numColumn.Text = "#";
			this.numColumn.Width = 60;
			//
			// nameColumn
			//
			this.nameColumn.Text = "Column";
			this.nameColumn.Width = 300;
			//
			// orderColumn
			//
			this.orderColumn.Text = "Order";
			this.orderColumn.Width = 180;
			//
			// typeColumn
			//
			this.typeColumn.Text = "Data type";
			this.typeColumn.Width = 200;
			//
			// upButton
			//
			this.upButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.upButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.upButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.upButton.Font = new System.Drawing.Font("Segoe UI Symbol", 10F);
			this.upButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.upButton.ImageOver = null;
			this.upButton.Location = new System.Drawing.Point(822, 178);
			this.upButton.Name = "upButton";
			this.upButton.ShowBorder = true;
			this.upButton.Size = new System.Drawing.Size(48, 40);
			this.upButton.TabIndex = 3;
			this.upButton.Text = "▲";
			this.upButton.ThemedBack = null;
			this.upButton.ThemedFore = null;
			this.upButton.UseVisualStyleBackColor = false;
			this.upButton.Click += new System.EventHandler(this.MoveUp);
			//
			// downButton
			//
			this.downButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.downButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.downButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.downButton.Font = new System.Drawing.Font("Segoe UI Symbol", 10F);
			this.downButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.downButton.ImageOver = null;
			this.downButton.Location = new System.Drawing.Point(822, 226);
			this.downButton.Name = "downButton";
			this.downButton.ShowBorder = true;
			this.downButton.Size = new System.Drawing.Size(48, 40);
			this.downButton.TabIndex = 4;
			this.downButton.Text = "▼";
			this.downButton.ThemedBack = null;
			this.downButton.ThemedFore = null;
			this.downButton.UseVisualStyleBackColor = false;
			this.downButton.Click += new System.EventHandler(this.MoveDown);
			//
			// deleteButton
			//
			this.deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.deleteButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.deleteButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.deleteButton.Font = new System.Drawing.Font("Segoe UI Symbol", 10F);
			this.deleteButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.deleteButton.ImageOver = null;
			this.deleteButton.Location = new System.Drawing.Point(822, 274);
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.ShowBorder = true;
			this.deleteButton.Size = new System.Drawing.Size(48, 40);
			this.deleteButton.TabIndex = 5;
			this.deleteButton.Text = "\U0001F5D1";
			this.deleteButton.ThemedBack = null;
			this.deleteButton.ThemedFore = null;
			this.deleteButton.UseVisualStyleBackColor = false;
			this.deleteButton.Click += new System.EventHandler(this.DeleteRule);
			//
			// noteLabel
			//
			this.noteLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.noteLabel.AutoSize = true;
			this.noteLabel.Location = new System.Drawing.Point(30, 582);
			this.noteLabel.Name = "noteLabel";
			this.noteLabel.Size = new System.Drawing.Size(207, 20);
			this.noteLabel.TabIndex = 6;
			this.noteLabel.Text = "Blank cells always sort last.";
			this.noteLabel.ThemedBack = null;
			this.noteLabel.ThemedFore = "GrayText";
			//
			// okButton
			//
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(630, 572);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(116, 38);
			this.okButton.TabIndex = 7;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			//
			// cancelButton
			//
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(754, 572);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(116, 38);
			this.cancelButton.TabIndex = 8;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			//
			// CustomSortDialog
			//
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(900, 630);
			this.Controls.Add(this.topGroup);
			this.Controls.Add(this.rulesLabel);
			this.Controls.Add(this.ruleList);
			this.Controls.Add(this.upButton);
			this.Controls.Add(this.downButton);
			this.Controls.Add(this.deleteButton);
			this.Controls.Add(this.noteLabel);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "CustomSortDialog";
			this.Padding = new System.Windows.Forms.Padding(20, 20, 10, 20);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Sort Table";
			this.topGroup.ResumeLayout(false);
			this.topGroup.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private UI.MoreGroupBox topGroup;
		private UI.MoreLabel columnLabel;
		private UI.MoreComboBox columnBox;
		private UI.MoreLabel orderLabel;
		private UI.MoreComboBox orderBox;
		private UI.MoreLabel typeLabel;
		private UI.MoreComboBox typeBox;
		private UI.MoreButton addButton;
		private UI.MoreLabel rulesLabel;
		private UI.MoreListView ruleList;
		private System.Windows.Forms.ColumnHeader numColumn;
		private System.Windows.Forms.ColumnHeader nameColumn;
		private System.Windows.Forms.ColumnHeader orderColumn;
		private System.Windows.Forms.ColumnHeader typeColumn;
		private UI.MoreButton upButton;
		private UI.MoreButton downButton;
		private UI.MoreButton deleteButton;
		private UI.MoreLabel noteLabel;
	}
}
