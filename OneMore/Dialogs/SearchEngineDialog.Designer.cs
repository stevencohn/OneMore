namespace River.OneMoreAddIn.Dialogs
{
	partial class SearchEngineDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SearchEngineDialog));
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.gridView = new System.Windows.Forms.DataGridView();
			this.iconColumn = new System.Windows.Forms.DataGridViewImageColumn();
			this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.urlColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.introPanel = new System.Windows.Forms.Panel();
			this.introLabel = new System.Windows.Forms.Label();
			this.toolStrip = new River.OneMoreAddIn.ScaledToolStrip();
			this.upButton = new System.Windows.Forms.ToolStripButton();
			this.downButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.deleteLabel = new System.Windows.Forms.ToolStripLabel();
			this.deleteButton = new System.Windows.Forms.ToolStripButton();
			this.buttonPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridView)).BeginInit();
			this.introPanel.SuspendLayout();
			this.toolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonPanel
			// 
			this.buttonPanel.Controls.Add(this.okButton);
			this.buttonPanel.Controls.Add(this.cancelButton);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.Location = new System.Drawing.Point(10, 391);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
			this.buttonPanel.Size = new System.Drawing.Size(808, 53);
			this.buttonPanel.TabIndex = 0;
			// 
			// okButton
			// 
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(531, 6);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(120, 38);
			this.okButton.TabIndex = 1;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(657, 6);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(120, 38);
			this.cancelButton.TabIndex = 0;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// gridView
			// 
			this.gridView.BackgroundColor = System.Drawing.SystemColors.Control;
			this.gridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.gridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.iconColumn,
            this.nameColumn,
            this.urlColumn});
			this.gridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridView.Location = new System.Drawing.Point(10, 111);
			this.gridView.MultiSelect = false;
			this.gridView.Name = "gridView";
			this.gridView.RowHeadersWidth = 62;
			this.gridView.RowTemplate.Height = 28;
			this.gridView.Size = new System.Drawing.Size(808, 280);
			this.gridView.TabIndex = 2;
			this.gridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridView_CellEndEdit);
			this.gridView.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.gridView_DataBindingComplete);
			// 
			// iconColumn
			// 
			this.iconColumn.HeaderText = "Icon";
			this.iconColumn.Image = global::River.OneMoreAddIn.Properties.Resources.NewStyle;
			this.iconColumn.MinimumWidth = 46;
			this.iconColumn.Name = "iconColumn";
			this.iconColumn.ReadOnly = true;
			this.iconColumn.Width = 46;
			// 
			// nameColumn
			// 
			this.nameColumn.HeaderText = "Name";
			this.nameColumn.MinimumWidth = 100;
			this.nameColumn.Name = "nameColumn";
			this.nameColumn.Width = 200;
			// 
			// urlColumn
			// 
			this.urlColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.urlColumn.FillWeight = 1000F;
			this.urlColumn.HeaderText = "URL Pattern";
			this.urlColumn.MinimumWidth = 8;
			this.urlColumn.Name = "urlColumn";
			// 
			// introPanel
			// 
			this.introPanel.Controls.Add(this.introLabel);
			this.introPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.introPanel.Location = new System.Drawing.Point(10, 55);
			this.introPanel.Name = "introPanel";
			this.introPanel.Size = new System.Drawing.Size(808, 56);
			this.introPanel.TabIndex = 3;
			// 
			// introLabel
			// 
			this.introLabel.AutoSize = true;
			this.introLabel.Location = new System.Drawing.Point(12, 17);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(497, 20);
			this.introLabel.TabIndex = 0;
			this.introLabel.Text = "URL Patterns must contain a string replacement token, such as &q={0}";
			// 
			// toolStrip
			// 
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.upButton,
            this.downButton,
            this.toolStripSeparator1,
            this.deleteLabel,
            this.deleteButton});
			this.toolStrip.Location = new System.Drawing.Point(10, 10);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Size = new System.Drawing.Size(808, 45);
			this.toolStrip.TabIndex = 4;
			this.toolStrip.Text = "Tool Strip";
			// 
			// upButton
			// 
			this.upButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.upButton.Image = global::River.OneMoreAddIn.Properties.Resources.UpArrow;
			this.upButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.upButton.Name = "upButton";
			this.upButton.Size = new System.Drawing.Size(40, 40);
			this.upButton.Text = "Move up";
			this.upButton.Click += new System.EventHandler(this.upButton_Click);
			// 
			// downButton
			// 
			this.downButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.downButton.Image = global::River.OneMoreAddIn.Properties.Resources.DownArrow;
			this.downButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.downButton.Name = "downButton";
			this.downButton.Size = new System.Drawing.Size(40, 40);
			this.downButton.Text = "Move down";
			this.downButton.Click += new System.EventHandler(this.downButton_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 45);
			// 
			// deleteLabel
			// 
			this.deleteLabel.Name = "deleteLabel";
			this.deleteLabel.Size = new System.Drawing.Size(66, 40);
			this.deleteLabel.Text = "Delete:";
			// 
			// deleteButton
			// 
			this.deleteButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.deleteButton.Image = global::River.OneMoreAddIn.Properties.Resources.Delete;
			this.deleteButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.Size = new System.Drawing.Size(40, 40);
			this.deleteButton.Text = "Delete";
			this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
			// 
			// SearchEngineDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(828, 444);
			this.Controls.Add(this.gridView);
			this.Controls.Add(this.introPanel);
			this.Controls.Add(this.toolStrip);
			this.Controls.Add(this.buttonPanel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(800, 400);
			this.Name = "SearchEngineDialog";
			this.Padding = new System.Windows.Forms.Padding(10, 10, 10, 0);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Search Engines";
			this.buttonPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.gridView)).EndInit();
			this.introPanel.ResumeLayout(false);
			this.introPanel.PerformLayout();
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Panel buttonPanel;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.DataGridView gridView;
		private System.Windows.Forms.Panel introPanel;
		private System.Windows.Forms.Label introLabel;
		private ScaledToolStrip toolStrip;
		private System.Windows.Forms.ToolStripButton deleteButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton upButton;
		private System.Windows.Forms.ToolStripButton downButton;
		private System.Windows.Forms.DataGridViewImageColumn iconColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn urlColumn;
		private System.Windows.Forms.ToolStripLabel deleteLabel;
	}
}