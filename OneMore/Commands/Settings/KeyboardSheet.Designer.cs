﻿namespace River.OneMoreAddIn.Settings
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
			this.gridView = new KeyboardGridView();
			this.cmdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.keyColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.introPanel = new System.Windows.Forms.Panel();
			this.introLabel = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.gridView)).BeginInit();
			this.introPanel.SuspendLayout();
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
			this.gridView.Location = new System.Drawing.Point(10, 64);
			this.gridView.MultiSelect = false;
			this.gridView.Name = "gridView";
			this.gridView.RowHeadersVisible = false;
			this.gridView.RowHeadersWidth = 30;
			this.gridView.RowTemplate.Height = 28;
			this.gridView.ShowEditingIcon = false;
			this.gridView.Size = new System.Drawing.Size(780, 427);
			this.gridView.TabIndex = 2;
			this.gridView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.gridView_KeyDown);
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
			// KeyboardSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.gridView);
			this.Controls.Add(this.introPanel);
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MinimumSize = new System.Drawing.Size(750, 400);
			this.Name = "KeyboardSheet";
			this.Padding = new System.Windows.Forms.Padding(10, 9, 10, 9);
			this.Size = new System.Drawing.Size(800, 500);
			((System.ComponentModel.ISupportInitialize)(this.gridView)).EndInit();
			this.introPanel.ResumeLayout(false);
			this.introPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion
		private KeyboardGridView gridView;
		private System.Windows.Forms.Panel introPanel;
		private System.Windows.Forms.Label introLabel;
		private System.Windows.Forms.DataGridViewTextBoxColumn cmdColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn keyColumn;
	}
}