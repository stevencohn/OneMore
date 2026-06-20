namespace River.OneMoreAddIn.Commands.Layouts
{
	partial class ManageLayoutsControl
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
			this.listView = new River.OneMoreAddIn.UI.MoreListView();
			this.nameColumn = new System.Windows.Forms.ColumnHeader();
			this.locationColumn = new System.Windows.Forms.ColumnHeader();
			this.toolStrip = new River.OneMoreAddIn.UI.MoreToolStrip();
			this.sortButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.upButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.downButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.renameButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.captureLayoutButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.restoreButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.deleteButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.checkButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
			this.importButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.exportButton = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.itemContextMenu = new River.OneMoreAddIn.UI.MoreContextMenuStrip();
			this.renameMenuItem = new River.OneMoreAddIn.UI.MoreMenuItem();
			this.components.Add(this.itemContextMenu);
			this.toolStrip.SuspendLayout();
			this.itemContextMenu.SuspendLayout();
			this.SuspendLayout();
			//
			// listView
			//
			this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameColumn,
            this.locationColumn});
			this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listView.Location = new System.Drawing.Point(0, 29);
			this.listView.MultiSelect = true;
			this.listView.Name = "listView";
			this.listView.Size = new System.Drawing.Size(778, 455);
			this.listView.TabIndex = 0;
			this.listView.SelectedIndexChanged += new System.EventHandler(this.RefreshToolbarState);
			this.listView.ItemMoved += new System.EventHandler<River.OneMoreAddIn.UI.MoreListView.ItemMovedEventArgs>(this.ListViewItemMoved);
			this.listView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ShowContextMenu);
			this.listView.DoubleClick += new System.EventHandler(this.RenameOnDoubleClick);
			//
			// nameColumn
			//
			this.nameColumn.Name = "nameColumn";
			this.nameColumn.Text = "Name";
			//
			// locationColumn
			//
			this.locationColumn.Name = "locationColumn";
			this.locationColumn.Text = "Location";
			//
			// toolStrip
			//
			this.toolStrip.Dock = System.Windows.Forms.DockStyle.Top;
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sortButton,
            this.toolStripSeparator1,
            this.upButton,
            this.downButton,
            this.toolStripSeparator2,
            this.renameButton,
            this.toolStripSeparator5,
            this.captureLayoutButton,
            this.restoreButton,
            this.toolStripSeparator3,
            this.deleteButton,
            this.toolStripSeparator4,
            this.checkButton,
            this.toolStripSeparator6,
            this.importButton,
            this.exportButton});
			this.toolStrip.Location = new System.Drawing.Point(0, 0);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Size = new System.Drawing.Size(778, 29);
			this.toolStrip.TabIndex = 1;
			//
			// sortButton
			//
			this.sortButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.sortButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Sort;
			this.sortButton.Name = "sortButton";
			this.sortButton.Size = new System.Drawing.Size(40, 29);
			this.sortButton.ToolTipText = "Sort current layout";
			this.sortButton.Click += new System.EventHandler(this.SortCurrentGroup);
			//
			// toolStripSeparator1
			//
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 29);
			//
			// upButton
			//
			this.upButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.upButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_MoveUp;
			this.upButton.Name = "upButton";
			this.upButton.Size = new System.Drawing.Size(40, 29);
			this.upButton.ToolTipText = "Move up";
			this.upButton.Click += new System.EventHandler(this.MoveUp);
			//
			// downButton
			//
			this.downButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.downButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_MoveDown;
			this.downButton.Name = "downButton";
			this.downButton.Size = new System.Drawing.Size(40, 29);
			this.downButton.ToolTipText = "Move down";
			this.downButton.Click += new System.EventHandler(this.MoveDown);
			//
			// toolStripSeparator2
			//
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 29);
			//
			// renameButton
			//
			this.renameButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.renameButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Rename;
			this.renameButton.Name = "renameButton";
			this.renameButton.Size = new System.Drawing.Size(40, 29);
			this.renameButton.ToolTipText = "Rename";
			this.renameButton.Click += new System.EventHandler(this.RenameSelected);
			//
			// toolStripSeparator5
			//
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.toolStripSeparator5.Size = new System.Drawing.Size(6, 29);
			//
			// captureLayoutButton
			//
			this.captureLayoutButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.captureLayoutButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Layout;
			this.captureLayoutButton.Name = "captureLayoutButton";
			this.captureLayoutButton.Size = new System.Drawing.Size(40, 29);
			this.captureLayoutButton.ToolTipText = "Capture Layout";
			this.captureLayoutButton.Click += new System.EventHandler(this.CaptureLayout);
			//
			// restoreButton
			//
			this.restoreButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.restoreButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Play;
			this.restoreButton.Name = "restoreButton";
			this.restoreButton.Size = new System.Drawing.Size(40, 29);
			this.restoreButton.ToolTipText = "Restore Layout";
			this.restoreButton.Click += new System.EventHandler(this.RestoreLayout);
			//
			// toolStripSeparator3
			//
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(6, 29);
			//
			// deleteButton
			//
			this.deleteButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.deleteButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Delete;
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.Size = new System.Drawing.Size(40, 29);
			this.deleteButton.ToolTipText = "Delete";
			this.deleteButton.Click += new System.EventHandler(this.DeleteSelected);
			//
			// toolStripSeparator4
			//
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(6, 29);
			//
			// checkButton
			//
			this.checkButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.checkButton.Image = global::River.OneMoreAddIn.Properties.Resources.e_CheckMark;
			this.checkButton.Name = "checkButton";
			this.checkButton.Size = new System.Drawing.Size(40, 29);
			this.checkButton.ToolTipText = "Check layouts";
			this.checkButton.Click += new System.EventHandler(this.CheckLayouts);
			//
			// toolStripSeparator6
			//
			this.toolStripSeparator6.Name = "toolStripSeparator6";
			this.toolStripSeparator6.Size = new System.Drawing.Size(6, 29);
			//
			// importButton
			//
			this.importButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.importButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Import;
			this.importButton.Name = "importButton";
			this.importButton.Size = new System.Drawing.Size(40, 29);
			this.importButton.ToolTipText = "Import layouts";
			this.importButton.Click += new System.EventHandler(this.ImportLayouts);
			//
			// exportButton
			//
			this.exportButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.exportButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Export;
			this.exportButton.Name = "exportButton";
			this.exportButton.Size = new System.Drawing.Size(40, 29);
			this.exportButton.ToolTipText = "Export layouts";
			this.exportButton.Click += new System.EventHandler(this.ExportLayouts);
			//
			// itemContextMenu
			//
			this.itemContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.renameMenuItem});
			this.itemContextMenu.Name = "itemContextMenu";
			this.itemContextMenu.Size = new System.Drawing.Size(180, 30);
			//
			// renameMenuItem
			//
			this.renameMenuItem.Name = "renameMenuItem";
			this.renameMenuItem.Size = new System.Drawing.Size(180, 26);
			this.renameMenuItem.Text = "Rename Window";
			this.renameMenuItem.Click += new System.EventHandler(this.RenameSelected);
			//
			// LayoutsManagerControl
			//
			this.Controls.Add(this.listView);
			this.Controls.Add(this.toolStrip);
			this.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Name = "LayoutsManagerControl";
			this.Size = new System.Drawing.Size(778, 484);
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.itemContextMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private River.OneMoreAddIn.UI.MoreListView listView;
		private System.Windows.Forms.ColumnHeader nameColumn;
		private System.Windows.Forms.ColumnHeader locationColumn;
		private River.OneMoreAddIn.UI.MoreToolStrip toolStrip;
		private River.OneMoreAddIn.UI.MoreMenuItem sortButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private River.OneMoreAddIn.UI.MoreMenuItem upButton;
		private River.OneMoreAddIn.UI.MoreMenuItem downButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private River.OneMoreAddIn.UI.MoreMenuItem renameButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
		private River.OneMoreAddIn.UI.MoreMenuItem captureLayoutButton;
		private River.OneMoreAddIn.UI.MoreMenuItem restoreButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private River.OneMoreAddIn.UI.MoreMenuItem deleteButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private River.OneMoreAddIn.UI.MoreMenuItem checkButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
		private River.OneMoreAddIn.UI.MoreMenuItem importButton;
		private River.OneMoreAddIn.UI.MoreMenuItem exportButton;
		private River.OneMoreAddIn.UI.MoreContextMenuStrip itemContextMenu;
		private River.OneMoreAddIn.UI.MoreMenuItem renameMenuItem;
	}
}
