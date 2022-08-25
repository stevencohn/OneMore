namespace River.OneMoreAddIn.Commands
{
	partial class CrawlWebPageDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CrawlWebPageDialog));
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.unselectLabel = new System.Windows.Forms.LinkLabel();
			this.selectLabel = new System.Windows.Forms.LinkLabel();
			this.introBox = new System.Windows.Forms.TextBox();
			this.gridView = new River.OneMoreAddIn.Settings.KeyboardGridView();
			this.selectedColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.addressColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.textColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.orderColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.useTextBox = new System.Windows.Forms.CheckBox();
			this.buttonPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridView)).BeginInit();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Enabled = false;
			this.okButton.Location = new System.Drawing.Point(944, 49);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.TabIndex = 11;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(1050, 49);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.TabIndex = 10;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// buttonPanel
			// 
			this.buttonPanel.Controls.Add(this.useTextBox);
			this.buttonPanel.Controls.Add(this.unselectLabel);
			this.buttonPanel.Controls.Add(this.selectLabel);
			this.buttonPanel.Controls.Add(this.cancelButton);
			this.buttonPanel.Controls.Add(this.okButton);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.Location = new System.Drawing.Point(20, 439);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Size = new System.Drawing.Size(1153, 90);
			this.buttonPanel.TabIndex = 13;
			// 
			// unselectLabel
			// 
			this.unselectLabel.AutoSize = true;
			this.unselectLabel.Location = new System.Drawing.Point(32, 55);
			this.unselectLabel.Name = "unselectLabel";
			this.unselectLabel.Size = new System.Drawing.Size(94, 20);
			this.unselectLabel.TabIndex = 13;
			this.unselectLabel.TabStop = true;
			this.unselectLabel.Text = "Select none";
			this.unselectLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.SelectNoneItems);
			// 
			// selectLabel
			// 
			this.selectLabel.AutoSize = true;
			this.selectLabel.Location = new System.Drawing.Point(32, 20);
			this.selectLabel.Name = "selectLabel";
			this.selectLabel.Size = new System.Drawing.Size(73, 20);
			this.selectLabel.TabIndex = 12;
			this.selectLabel.TabStop = true;
			this.selectLabel.Text = "Select all";
			this.selectLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.SelectAllItems);
			// 
			// introBox
			// 
			this.introBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.introBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.introBox.Location = new System.Drawing.Point(20, 20);
			this.introBox.Multiline = true;
			this.introBox.Name = "introBox";
			this.introBox.ReadOnly = true;
			this.introBox.Size = new System.Drawing.Size(1153, 62);
			this.introBox.TabIndex = 15;
			this.introBox.Text = "Select links to import as sub-pages. These will be relinked to from this page.";
			// 
			// gridView
			// 
			this.gridView.AllowUserToAddRows = false;
			this.gridView.AllowUserToDeleteRows = false;
			this.gridView.AllowUserToResizeRows = false;
			this.gridView.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
			this.gridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
			this.gridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
			this.gridView.ColumnHeadersHeight = 34;
			this.gridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.selectedColumn,
            this.addressColumn,
            this.textColumn,
            this.orderColumn});
			this.gridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnF2;
			this.gridView.Location = new System.Drawing.Point(20, 82);
			this.gridView.MultiSelect = false;
			this.gridView.Name = "gridView";
			this.gridView.RowHeadersVisible = false;
			this.gridView.RowHeadersWidth = 30;
			this.gridView.RowTemplate.Height = 28;
			this.gridView.ShowEditingIcon = false;
			this.gridView.Size = new System.Drawing.Size(1153, 357);
			this.gridView.TabIndex = 16;
			this.gridView.CurrentCellDirtyStateChanged += new System.EventHandler(this.DirtyStateChanged);
			this.gridView.Resize += new System.EventHandler(this.GridResize);
			// 
			// selectedColumn
			// 
			this.selectedColumn.DataPropertyName = "Selected";
			this.selectedColumn.FalseValue = "false";
			this.selectedColumn.HeaderText = "";
			this.selectedColumn.MinimumWidth = 50;
			this.selectedColumn.Name = "selectedColumn";
			this.selectedColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.selectedColumn.TrueValue = "true";
			this.selectedColumn.Width = 50;
			// 
			// addressColumn
			// 
			this.addressColumn.DataPropertyName = "Address";
			this.addressColumn.FillWeight = 200F;
			this.addressColumn.HeaderText = "Address";
			this.addressColumn.MinimumWidth = 200;
			this.addressColumn.Name = "addressColumn";
			this.addressColumn.ReadOnly = true;
			this.addressColumn.Width = 706;
			// 
			// textColumn
			// 
			this.textColumn.DataPropertyName = "Text";
			this.textColumn.FillWeight = 200F;
			this.textColumn.HeaderText = "Text";
			this.textColumn.MinimumWidth = 300;
			this.textColumn.Name = "textColumn";
			this.textColumn.ReadOnly = true;
			this.textColumn.Width = 300;
			// 
			// orderColumn
			// 
			this.orderColumn.DataPropertyName = "Order";
			this.orderColumn.HeaderText = "Order";
			this.orderColumn.MinimumWidth = 20;
			this.orderColumn.Name = "orderColumn";
			this.orderColumn.ReadOnly = true;
			this.orderColumn.Width = 90;
			// 
			// useTextBox
			// 
			this.useTextBox.AutoSize = true;
			this.useTextBox.Checked = true;
			this.useTextBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.useTextBox.Location = new System.Drawing.Point(944, 16);
			this.useTextBox.Name = "useTextBox";
			this.useTextBox.Size = new System.Drawing.Size(139, 24);
			this.useTextBox.TabIndex = 14;
			this.useTextBox.Text = "Use Text Titles";
			this.useTextBox.UseVisualStyleBackColor = true;
			// 
			// CrawlWebPageDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(1188, 544);
			this.Controls.Add(this.gridView);
			this.Controls.Add(this.introBox);
			this.Controls.Add(this.buttonPanel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(800, 400);
			this.Name = "CrawlWebPageDialog";
			this.Padding = new System.Windows.Forms.Padding(20, 20, 15, 15);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Import Web Sub-pages";
			this.buttonPanel.ResumeLayout(false);
			this.buttonPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridView)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Panel buttonPanel;
		private System.Windows.Forms.TextBox introBox;
		private Settings.KeyboardGridView gridView;
		private System.Windows.Forms.DataGridViewCheckBoxColumn selectedColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn addressColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn textColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn orderColumn;
		private System.Windows.Forms.LinkLabel unselectLabel;
		private System.Windows.Forms.LinkLabel selectLabel;
		private System.Windows.Forms.CheckBox useTextBox;
	}
}