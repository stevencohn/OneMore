namespace River.OneMoreAddIn.Settings
{
	partial class KeyboardSheet
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
			this.gridView = new River.OneMoreAddIn.Settings.KeyboardGridView();
			this.cmdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.keyColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.introPanel = new System.Windows.Forms.Panel();
			this.introLabel = new System.Windows.Forms.Label();
			this.toolstrip = new System.Windows.Forms.ToolStrip();
			this.clearButton = new System.Windows.Forms.ToolStripButton();
			this.resetButton = new System.Windows.Forms.ToolStripButton();
			this.resetAllButton = new System.Windows.Forms.ToolStripButton();
			((System.ComponentModel.ISupportInitialize)(this.gridView)).BeginInit();
			this.introPanel.SuspendLayout();
			this.toolstrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// gridView
			// 
			this.gridView.AllowUserToAddRows = false;
			this.gridView.AllowUserToDeleteRows = false;
			this.gridView.AllowUserToResizeRows = false;
			this.gridView.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
			this.gridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.gridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.cmdColumn,
            this.keyColumn});
			this.gridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
			this.gridView.Location = new System.Drawing.Point(10, 98);
			this.gridView.MultiSelect = false;
			this.gridView.Name = "gridView";
			this.gridView.RowHeadersVisible = false;
			this.gridView.RowHeadersWidth = 30;
			this.gridView.RowTemplate.Height = 28;
			this.gridView.ShowEditingIcon = false;
			this.gridView.Size = new System.Drawing.Size(780, 393);
			this.gridView.TabIndex = 2;
			this.gridView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AssignOnKeyDown);
			// 
			// cmdColumn
			// 
			this.cmdColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.cmdColumn.HeaderText = "Command";
			this.cmdColumn.MinimumWidth = 200;
			this.cmdColumn.Name = "cmdColumn";
			this.cmdColumn.ReadOnly = true;
			this.cmdColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// keyColumn
			// 
			this.keyColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.keyColumn.FillWeight = 200F;
			this.keyColumn.HeaderText = "Key Sequence";
			this.keyColumn.MinimumWidth = 300;
			this.keyColumn.Name = "keyColumn";
			this.keyColumn.Width = 300;
			// 
			// introPanel
			// 
			this.introPanel.Controls.Add(this.introLabel);
			this.introPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.introPanel.Location = new System.Drawing.Point(10, 9);
			this.introPanel.Name = "introPanel";
			this.introPanel.Size = new System.Drawing.Size(780, 55);
			this.introPanel.TabIndex = 3;
			// 
			// introLabel
			// 
			this.introLabel.AutoSize = true;
			this.introLabel.Location = new System.Drawing.Point(3, 0);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(619, 20);
			this.introLabel.TabIndex = 0;
			this.introLabel.Text = "Manage my custom keyboard shortcuts. Select a command and press a key sequence.";
			// 
			// toolstrip
			// 
			this.toolstrip.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.toolstrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearButton,
            this.resetButton,
            this.resetAllButton});
			this.toolstrip.Location = new System.Drawing.Point(10, 64);
			this.toolstrip.Name = "toolstrip";
			this.toolstrip.Size = new System.Drawing.Size(780, 34);
			this.toolstrip.TabIndex = 4;
			this.toolstrip.Text = "toolStrip1";
			// 
			// clearButton
			// 
			this.clearButton.Image = global::River.OneMoreAddIn.Properties.Resources.Delete;
			this.clearButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.clearButton.Name = "clearButton";
			this.clearButton.Size = new System.Drawing.Size(79, 29);
			this.clearButton.Text = "Clear";
			this.clearButton.Click += new System.EventHandler(this.ClearCommand);
			// 
			// resetButton
			// 
			this.resetButton.Image = global::River.OneMoreAddIn.Properties.Resources.Bullet;
			this.resetButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.resetButton.Name = "resetButton";
			this.resetButton.Size = new System.Drawing.Size(82, 29);
			this.resetButton.Text = "Reset";
			this.resetButton.Click += new System.EventHandler(this.ResetCommand);
			// 
			// resetAllButton
			// 
			this.resetAllButton.Image = global::River.OneMoreAddIn.Properties.Resources.Refresh;
			this.resetAllButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.resetAllButton.Name = "resetAllButton";
			this.resetAllButton.Size = new System.Drawing.Size(107, 29);
			this.resetAllButton.Text = "Reset All";
			this.resetAllButton.Click += new System.EventHandler(this.ResetAllDefaults);
			// 
			// KeyboardSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.gridView);
			this.Controls.Add(this.toolstrip);
			this.Controls.Add(this.introPanel);
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MinimumSize = new System.Drawing.Size(750, 400);
			this.Name = "KeyboardSheet";
			this.Padding = new System.Windows.Forms.Padding(10, 9, 10, 9);
			this.Size = new System.Drawing.Size(800, 500);
			((System.ComponentModel.ISupportInitialize)(this.gridView)).EndInit();
			this.introPanel.ResumeLayout(false);
			this.introPanel.PerformLayout();
			this.toolstrip.ResumeLayout(false);
			this.toolstrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private KeyboardGridView gridView;
		private System.Windows.Forms.Panel introPanel;
		private System.Windows.Forms.Label introLabel;
		private System.Windows.Forms.DataGridViewTextBoxColumn cmdColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn keyColumn;
		private System.Windows.Forms.ToolStrip toolstrip;
		private System.Windows.Forms.ToolStripButton clearButton;
		private System.Windows.Forms.ToolStripButton resetButton;
		private System.Windows.Forms.ToolStripButton resetAllButton;
	}
}