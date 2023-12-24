namespace River.OneMoreAddIn.Commands.Favorites
{
	partial class FavoritesDialog
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FavoritesDialog));
			this.gridView = new System.Windows.Forms.DataGridView();
			this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.locationColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.goButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.searchPanel = new System.Windows.Forms.Panel();
			this.menuButton = new System.Windows.Forms.Button();
			this.searchBox = new System.Windows.Forms.TextBox();
			this.searchLabel = new System.Windows.Forms.Label();
			this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.addButton = new System.Windows.Forms.ToolStripMenuItem();
			this.checkButton = new System.Windows.Forms.ToolStripMenuItem();
			this.sortByNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.manageButton = new System.Windows.Forms.ToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.gridView)).BeginInit();
			this.buttonPanel.SuspendLayout();
			this.searchPanel.SuspendLayout();
			this.contextMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// gridView
			// 
			this.gridView.AllowUserToAddRows = false;
			this.gridView.AllowUserToDeleteRows = false;
			this.gridView.AllowUserToResizeRows = false;
			this.gridView.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
			this.gridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.gridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.nameColumn,
            this.locationColumn});
			this.gridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
			this.gridView.Location = new System.Drawing.Point(0, 74);
			this.gridView.MultiSelect = false;
			this.gridView.Name = "gridView";
			this.gridView.RowHeadersVisible = false;
			this.gridView.RowHeadersWidth = 30;
			this.gridView.RowTemplate.Height = 28;
			this.gridView.ShowEditingIcon = false;
			this.gridView.Size = new System.Drawing.Size(778, 410);
			this.gridView.TabIndex = 0;
			this.gridView.CellContentDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.ChooseByDoubleClick);
			this.gridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.ValidateOnCellFormatting);
			this.gridView.GotFocus += new System.EventHandler(this.RefocusOnGotFocus);
			this.gridView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ChooseByKeyboard);
			// 
			// nameColumn
			// 
			this.nameColumn.HeaderText = "Name";
			this.nameColumn.MinimumWidth = 100;
			this.nameColumn.Name = "nameColumn";
			this.nameColumn.Width = 250;
			// 
			// locationColumn
			// 
			this.locationColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.locationColumn.FillWeight = 1000F;
			this.locationColumn.HeaderText = "Location";
			this.locationColumn.MinimumWidth = 8;
			this.locationColumn.Name = "locationColumn";
			// 
			// buttonPanel
			// 
			this.buttonPanel.Controls.Add(this.goButton);
			this.buttonPanel.Controls.Add(this.cancelButton);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.Location = new System.Drawing.Point(0, 484);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Padding = new System.Windows.Forms.Padding(8);
			this.buttonPanel.Size = new System.Drawing.Size(778, 60);
			this.buttonPanel.TabIndex = 4;
			// 
			// goButton
			// 
			this.goButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.goButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.goButton.Location = new System.Drawing.Point(518, 11);
			this.goButton.Name = "goButton";
			this.goButton.Size = new System.Drawing.Size(120, 38);
			this.goButton.TabIndex = 0;
			this.goButton.Text = "Go";
			this.goButton.UseVisualStyleBackColor = true;
			this.goButton.Click += new System.EventHandler(this.ChooseByClick);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(644, 11);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(120, 38);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// searchPanel
			// 
			this.searchPanel.Controls.Add(this.menuButton);
			this.searchPanel.Controls.Add(this.searchBox);
			this.searchPanel.Controls.Add(this.searchLabel);
			this.searchPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.searchPanel.Location = new System.Drawing.Point(0, 0);
			this.searchPanel.Name = "searchPanel";
			this.searchPanel.Padding = new System.Windows.Forms.Padding(15);
			this.searchPanel.Size = new System.Drawing.Size(778, 74);
			this.searchPanel.TabIndex = 5;
			// 
			// menuButton
			// 
			this.menuButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.menuButton.FlatAppearance.BorderSize = 0;
			this.menuButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.menuButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.menuButton.ForeColor = System.Drawing.SystemColors.GrayText;
			this.menuButton.Location = new System.Drawing.Point(710, 18);
			this.menuButton.Name = "menuButton";
			this.menuButton.Size = new System.Drawing.Size(50, 38);
			this.menuButton.TabIndex = 1;
			this.menuButton.Text = "•••";
			this.menuButton.UseVisualStyleBackColor = true;
			this.menuButton.Click += new System.EventHandler(this.ShowMenu);
			// 
			// searchBox
			// 
			this.searchBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.searchBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.searchBox.Location = new System.Drawing.Point(138, 18);
			this.searchBox.Name = "searchBox";
			this.searchBox.Size = new System.Drawing.Size(566, 28);
			this.searchBox.TabIndex = 0;
			this.searchBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FilterRowOnKeyUp);
			// 
			// searchLabel
			// 
			this.searchLabel.AutoSize = true;
			this.searchLabel.Location = new System.Drawing.Point(30, 23);
			this.searchLabel.Name = "searchLabel";
			this.searchLabel.Size = new System.Drawing.Size(60, 20);
			this.searchLabel.TabIndex = 0;
			this.searchLabel.Text = "Search";
			// 
			// contextMenu
			// 
			this.contextMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addButton,
            this.checkButton,
            this.sortByNameToolStripMenuItem,
            this.manageButton});
			this.contextMenu.Name = "contextMenu";
			this.contextMenu.Size = new System.Drawing.Size(233, 132);
			// 
			// addButton
			// 
			this.addButton.Image = global::River.OneMoreAddIn.Properties.Resources.Journal;
			this.addButton.Name = "addButton";
			this.addButton.Size = new System.Drawing.Size(232, 32);
			this.addButton.Text = "Add Current Page";
			this.addButton.Click += new System.EventHandler(this.AddCurrentPage);
			// 
			// checkButton
			// 
			this.checkButton.Image = global::River.OneMoreAddIn.Properties.Resources.CheckMark;
			this.checkButton.Name = "checkButton";
			this.checkButton.Size = new System.Drawing.Size(232, 32);
			this.checkButton.Text = "Check Favorites";
			this.checkButton.Click += new System.EventHandler(this.CheckFavorites);
			// 
			// sortByNameToolStripMenuItem
			// 
			this.sortByNameToolStripMenuItem.Image = global::River.OneMoreAddIn.Properties.Resources.DownArrow;
			this.sortByNameToolStripMenuItem.Name = "sortByNameToolStripMenuItem";
			this.sortByNameToolStripMenuItem.Size = new System.Drawing.Size(232, 32);
			this.sortByNameToolStripMenuItem.Text = "Sort By Name";
			this.sortByNameToolStripMenuItem.Click += new System.EventHandler(this.SortFavorites);
			// 
			// manageButton
			// 
			this.manageButton.Image = global::River.OneMoreAddIn.Properties.Resources.Hammer;
			this.manageButton.Name = "manageButton";
			this.manageButton.Size = new System.Drawing.Size(232, 32);
			this.manageButton.Text = "Manage Favorites";
			this.manageButton.Click += new System.EventHandler(this.ManageFavorites);
			// 
			// FavoritesDialog
			// 
			this.AcceptButton = this.goButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(778, 544);
			this.Controls.Add(this.gridView);
			this.Controls.Add(this.searchPanel);
			this.Controls.Add(this.buttonPanel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(600, 400);
			this.Name = "FavoritesDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Favorites";
			this.Activated += new System.EventHandler(this.FocusOnActivated);
			this.Load += new System.EventHandler(this.BindOnLoad);
			((System.ComponentModel.ISupportInitialize)(this.gridView)).EndInit();
			this.buttonPanel.ResumeLayout(false);
			this.searchPanel.ResumeLayout(false);
			this.searchPanel.PerformLayout();
			this.contextMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private System.Windows.Forms.DataGridView gridView;
		private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn locationColumn;
		private System.Windows.Forms.Panel buttonPanel;
		private System.Windows.Forms.Button goButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Panel searchPanel;
		private System.Windows.Forms.TextBox searchBox;
		private System.Windows.Forms.Label searchLabel;
		private System.Windows.Forms.Button menuButton;
		private System.Windows.Forms.ContextMenuStrip contextMenu;
		private System.Windows.Forms.ToolStripMenuItem checkButton;
		private System.Windows.Forms.ToolStripMenuItem manageButton;
		private System.Windows.Forms.ToolStripMenuItem addButton;
		private System.Windows.Forms.ToolStripMenuItem sortByNameToolStripMenuItem;
	}
}