namespace River.OneMoreAddIn.Settings
{
	partial class SnippetsSheet
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			this.gridView = new River.OneMoreAddIn.UI.MoreDataGridView();
			this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.introBox = new River.OneMoreAddIn.UI.MoreMultilineLabel();
			this.toolStrip = new River.OneMoreAddIn.UI.MoreToolStrip();
			this.renameButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.deleteButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.optionsPanel = new River.OneMoreAddIn.UI.MorePanel();
			this.codeStyleBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.tabs = new River.OneMoreAddIn.UI.MoreTabControl();
			this.boxTypesTab = new System.Windows.Forms.TabPage();
			this.boxWidthLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.boxWidthBox = new River.OneMoreAddIn.UI.MoreNumericUpDown();
			this.mySnippetsTab = new System.Windows.Forms.TabPage();
			((System.ComponentModel.ISupportInitialize)(this.gridView)).BeginInit();
			this.toolStrip.SuspendLayout();
			this.optionsPanel.SuspendLayout();
			this.tabs.SuspendLayout();
			this.boxTypesTab.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.boxWidthBox)).BeginInit();
			this.mySnippetsTab.SuspendLayout();
			this.SuspendLayout();
			//
			// gridView
			//
			this.gridView.AllowUserToAddRows = false;
			this.gridView.AllowUserToResizeRows = false;
			this.gridView.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
			this.gridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.gridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.nameColumn});
			this.gridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
			this.gridView.Location = new System.Drawing.Point(0, 125);
			this.gridView.MultiSelect = false;
			this.gridView.Name = "gridView";
			this.gridView.RowHeadersVisible = false;
			this.gridView.RowHeadersWidth = 30;
			this.gridView.RowTemplate.Height = 28;
			this.gridView.ShowEditingIcon = false;
			this.gridView.Size = new System.Drawing.Size(772, 265);
			this.gridView.TabIndex = 2;
			this.gridView.ThemedBack = null;
			this.gridView.ThemedFore = null;
			//
			// nameColumn
			//
			this.nameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			dataGridViewCellStyle1.ForeColor = System.Drawing.Color.Black;
			this.nameColumn.DefaultCellStyle = dataGridViewCellStyle1;
			this.nameColumn.HeaderText = "Name";
			this.nameColumn.MinimumWidth = 100;
			this.nameColumn.Name = "nameColumn";
			//
			// introBox
			//
			this.introBox.AutoSize = true;
			this.introBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.introBox.Location = new System.Drawing.Point(10, 9);
			this.introBox.Name = "introBox";
			this.introBox.Padding = new System.Windows.Forms.Padding(0, 0, 0, 20);
			this.introBox.Size = new System.Drawing.Size(780, 40);
			this.introBox.TabIndex = 0;
			this.introBox.Text = "Manage box options and My Snippets";
			this.introBox.ThemedBack = "ControlLightLight";
			this.introBox.ThemedFore = null;
			//
			// toolStrip
			//
			this.toolStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip.ImageScalingSize = new System.Drawing.Size(18, 18);
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.renameButton,
            this.deleteButton});
			this.toolStrip.Location = new System.Drawing.Point(0, 96);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
			this.toolStrip.Size = new System.Drawing.Size(772, 29);
			this.toolStrip.Stretch = true;
			this.toolStrip.TabIndex = 1;
			this.toolStrip.Text = "Tool Strip";
			//
			// renameButton
			//
			this.renameButton.ForeColor = System.Drawing.Color.Black;
			this.renameButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Rename;
			this.renameButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.renameButton.Name = "renameButton";
			this.renameButton.Size = new System.Drawing.Size(109, 29);
			this.renameButton.Text = "Rename";
			this.renameButton.Click += new System.EventHandler(this.RenameItem);
			//
			// deleteButton
			//
			this.deleteButton.ForeColor = System.Drawing.Color.Black;
			this.deleteButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Delete;
			this.deleteButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.Size = new System.Drawing.Size(96, 29);
			this.deleteButton.Text = "Delete";
			this.deleteButton.Click += new System.EventHandler(this.DeleteItem);
			//
			// optionsPanel
			//
			this.optionsPanel.BottomBorderColor = System.Drawing.SystemColors.ActiveBorder;
			this.optionsPanel.BottomBorderSize = 2;
			this.optionsPanel.Controls.Add(this.codeStyleBox);
			this.optionsPanel.Controls.Add(this.boxWidthLabel);
			this.optionsPanel.Controls.Add(this.boxWidthBox);
			this.optionsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.optionsPanel.Location = new System.Drawing.Point(0, 0);
			this.optionsPanel.Name = "optionsPanel";
			this.optionsPanel.Size = new System.Drawing.Size(772, 122);
			this.optionsPanel.TabIndex = 0;
			this.optionsPanel.ThemedBack = null;
			this.optionsPanel.ThemedFore = null;
			this.optionsPanel.TopBorderColor = System.Drawing.SystemColors.Control;
			this.optionsPanel.TopBorderSize = 0;
			//
			// codeStyleBox
			//
			this.codeStyleBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.codeStyleBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.codeStyleBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.codeStyleBox.Location = new System.Drawing.Point(18, 19);
			this.codeStyleBox.Name = "codeStyleBox";
			this.codeStyleBox.Size = new System.Drawing.Size(375, 25);
			this.codeStyleBox.StylizeImage = false;
			this.codeStyleBox.TabIndex = 0;
			this.codeStyleBox.Text = "Always apply \"code\" style to Code Box content";
			this.codeStyleBox.ThemedBack = null;
			this.codeStyleBox.ThemedFore = null;
			this.codeStyleBox.UseVisualStyleBackColor = false;
			//
			// tabs
			//
			this.tabs.Controls.Add(this.boxTypesTab);
			this.tabs.Controls.Add(this.mySnippetsTab);
			this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabs.Location = new System.Drawing.Point(10, 49);
			this.tabs.Name = "tabs";
			this.tabs.SelectedIndex = 0;
			this.tabs.Size = new System.Drawing.Size(780, 432);
			this.tabs.TabIndex = 1;
			//
			// boxTypesTab
			//
			this.boxTypesTab.Controls.Add(this.optionsPanel);
			this.boxTypesTab.Location = new System.Drawing.Point(4, 36);
			this.boxTypesTab.Name = "boxTypesTab";
			this.boxTypesTab.Padding = new System.Windows.Forms.Padding(8);
			this.boxTypesTab.Size = new System.Drawing.Size(772, 392);
			this.boxTypesTab.TabIndex = 0;
			this.boxTypesTab.Text = "Box Types";
			//
			// boxWidthLabel
			//
			this.boxWidthLabel.AutoSize = true;
			this.boxWidthLabel.Location = new System.Drawing.Point(18, 60);
			this.boxWidthLabel.Name = "boxWidthLabel";
			this.boxWidthLabel.Size = new System.Drawing.Size(70, 20);
			this.boxWidthLabel.TabIndex = 1;
			this.boxWidthLabel.Text = "Box width:";
			this.boxWidthLabel.ThemedBack = null;
			this.boxWidthLabel.ThemedFore = null;
			//
			// boxWidthBox
			//
			this.boxWidthBox.Location = new System.Drawing.Point(106, 57);
			this.boxWidthBox.Maximum = new decimal(new int[] {
            1200,
            0,
            0,
            0});
			this.boxWidthBox.Minimum = new decimal(new int[] {
            200,
            0,
            0,
            0});
			this.boxWidthBox.Name = "boxWidthBox";
			this.boxWidthBox.Size = new System.Drawing.Size(80, 26);
			this.boxWidthBox.TabIndex = 2;
			this.boxWidthBox.Value = new decimal(new int[] {
            600,
            0,
            0,
            0});
			//
			// mySnippetsTab
			//
			this.mySnippetsTab.Controls.Add(this.gridView);
			this.mySnippetsTab.Controls.Add(this.toolStrip);
			this.mySnippetsTab.Location = new System.Drawing.Point(4, 36);
			this.mySnippetsTab.Name = "mySnippetsTab";
			this.mySnippetsTab.Size = new System.Drawing.Size(772, 392);
			this.mySnippetsTab.TabIndex = 1;
			this.mySnippetsTab.Text = "My Snippets";
			//
			// SnippetsSheet
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.tabs);
			this.Controls.Add(this.introBox);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MinimumSize = new System.Drawing.Size(750, 400);
			this.Name = "SnippetsSheet";
			this.Padding = new System.Windows.Forms.Padding(10, 9, 10, 9);
			this.Size = new System.Drawing.Size(800, 500);
			((System.ComponentModel.ISupportInitialize)(this.gridView)).EndInit();
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.boxWidthBox)).EndInit();
			this.optionsPanel.ResumeLayout(false);
			this.optionsPanel.PerformLayout();
			this.tabs.ResumeLayout(false);
			this.boxTypesTab.ResumeLayout(false);
			this.boxTypesTab.PerformLayout();
			this.mySnippetsTab.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreDataGridView gridView;
		private UI.MoreToolStrip toolStrip;
		private UI.MoreMenuItem deleteButton;
		private UI.MoreMultilineLabel introBox;
		private UI.MoreMenuItem renameButton;
		private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
		private UI.MorePanel optionsPanel;
		private UI.MoreCheckBox codeStyleBox;
		private UI.MoreTabControl tabs;
		private System.Windows.Forms.TabPage boxTypesTab;
		private UI.MoreLabel boxWidthLabel;
		private UI.MoreNumericUpDown boxWidthBox;
		private System.Windows.Forms.TabPage mySnippetsTab;
	}
}
