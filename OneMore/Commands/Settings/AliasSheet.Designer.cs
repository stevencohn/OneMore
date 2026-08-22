namespace River.OneMoreAddIn.Settings
{
	partial class AliasSheet
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
			this.gridView = new UI.MoreDataGridView();
			this.cmdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.aliasColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.introBox = new River.OneMoreAddIn.UI.MoreMultilineLabel();
			this.filterPanel = new River.OneMoreAddIn.UI.MorePanel();
			this.filterLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.filterBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.filterClearButton = new River.OneMoreAddIn.UI.MoreButton();
			((System.ComponentModel.ISupportInitialize)(this.gridView)).BeginInit();
			this.filterPanel.SuspendLayout();
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
            this.aliasColumn});
			this.gridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridView.Location = new System.Drawing.Point(10, 104);
			this.gridView.MultiSelect = false;
			this.gridView.Name = "gridView";
			this.gridView.RowHeadersVisible = false;
			this.gridView.RowHeadersWidth = 30;
			this.gridView.RowTemplate.Height = 28;
			this.gridView.ShowEditingIcon = false;
			this.gridView.Size = new System.Drawing.Size(780, 387);
			this.gridView.TabIndex = 2;
			this.gridView.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.ValidateAlias);
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
			// aliasColumn
			// 
			this.aliasColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.aliasColumn.FillWeight = 200F;
			this.aliasColumn.HeaderText = "Alias";
			this.aliasColumn.MinimumWidth = 300;
			this.aliasColumn.Name = "aliasColumn";
			this.aliasColumn.Width = 300;
			// 
			// introBox
			// 
			this.introBox.AutoSize = true;
			this.introBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.introBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.introBox.Location = new System.Drawing.Point(10, 9);
			this.introBox.Name = "introBox";
			this.introBox.Padding = new System.Windows.Forms.Padding(0, 0, 0, 36);
			this.introBox.ThemedBack = "ControlLightLight";
			this.introBox.ThemedFore = null;
			this.introBox.Size = new System.Drawing.Size(780, 56);
			this.introBox.TabIndex = 0;
			this.introBox.Text = "Define command aliases for use in the Command Palette";
			//
			// filterPanel
			//
			this.filterPanel.BottomBorderSize = 0;
			this.filterPanel.Controls.Add(this.filterClearButton);
			this.filterPanel.Controls.Add(this.filterBox);
			this.filterPanel.Controls.Add(this.filterLabel);
			this.filterPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.filterPanel.Location = new System.Drawing.Point(10, 65);
			this.filterPanel.Name = "filterPanel";
			this.filterPanel.Size = new System.Drawing.Size(780, 39);
			this.filterPanel.TabIndex = 1;
			this.filterPanel.ThemedBack = "ControlLightLight";
			this.filterPanel.TopBorderSize = 0;
			//
			// filterLabel
			//
			this.filterLabel.AutoSize = true;
			this.filterLabel.Location = new System.Drawing.Point(0, 9);
			this.filterLabel.Name = "filterLabel";
			this.filterLabel.Size = new System.Drawing.Size(48, 20);
			this.filterLabel.TabIndex = 0;
			this.filterLabel.Text = "Filter";
			//
			// filterBox
			//
			this.filterBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.filterBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.filterBox.Location = new System.Drawing.Point(60, 4);
			this.filterBox.Name = "filterBox";
			this.filterBox.ProcessEnterKey = true;
			this.filterBox.Size = new System.Drawing.Size(680, 26);
			this.filterBox.TabIndex = 1;
			this.filterBox.ThemedBack = null;
			this.filterBox.ThemedFore = null;
			this.filterBox.TextChanged += new System.EventHandler(this.FilterCommands);
			//
			// filterClearButton
			//
			this.filterClearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.filterClearButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.filterClearButton.ImageOver = null;
			this.filterClearButton.Location = new System.Drawing.Point(750, 2);
			this.filterClearButton.Name = "filterClearButton";
			this.filterClearButton.ShowBorder = false;
			this.filterClearButton.Size = new System.Drawing.Size(30, 30);
			this.filterClearButton.StylizeImage = false;
			this.filterClearButton.TabIndex = 2;
			this.filterClearButton.Text = "✕";
			this.filterClearButton.ThemedBack = null;
			this.filterClearButton.ThemedFore = null;
			this.filterClearButton.UseVisualStyleBackColor = true;
			this.filterClearButton.Click += new System.EventHandler(this.ClearFilter);
			//
			// AliasSheet
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.gridView);
			this.Controls.Add(this.filterPanel);
			this.Controls.Add(this.introBox);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MinimumSize = new System.Drawing.Size(750, 400);
			this.Name = "AliasSheet";
			this.Padding = new System.Windows.Forms.Padding(10, 9, 10, 9);
			this.Size = new System.Drawing.Size(800, 500);
			((System.ComponentModel.ISupportInitialize)(this.gridView)).EndInit();
			this.filterPanel.ResumeLayout(false);
			this.filterPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private UI.MoreDataGridView gridView;
		private UI.MoreMultilineLabel introBox;
		private UI.MorePanel filterPanel;
		private UI.MoreLabel filterLabel;
		private UI.MoreTextBox filterBox;
		private UI.MoreButton filterClearButton;
		private System.Windows.Forms.DataGridViewTextBoxColumn cmdColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn aliasColumn;
	}
}