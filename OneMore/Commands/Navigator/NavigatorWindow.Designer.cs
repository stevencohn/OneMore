namespace River.OneMoreAddIn.Commands
{
	partial class NavigatorWindow
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

				foreach (var item in trash)
				{
					item?.Dispose();
				}

				trash.Clear();
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NavigatorWindow));
			this.controlPanel = new System.Windows.Forms.Panel();
			this.closeButton = new River.OneMoreAddIn.UI.MoreButton();
			this.mainContainer = new System.Windows.Forms.SplitContainer();
			this.pageBox = new River.OneMoreAddIn.UI.MoreFlowLayoutPanel();
			this.pageHeadPanel = new River.OneMoreAddIn.UI.MorePanel();
			this.refreshButton = new River.OneMoreAddIn.UI.MoreButton();
			this.pageHeadLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.subContainer = new System.Windows.Forms.SplitContainer();
			this.pinnedBox = new River.OneMoreAddIn.UI.MoreListView();
			this.pinnedHeadPanel = new River.OneMoreAddIn.UI.MorePanel();
			this.pinnedToolPanel = new System.Windows.Forms.Panel();
			this.copyPinnedButton = new River.OneMoreAddIn.UI.MoreButton();
			this.upButton = new River.OneMoreAddIn.UI.MoreButton();
			this.unpinButton = new River.OneMoreAddIn.UI.MoreButton();
			this.downButton = new River.OneMoreAddIn.UI.MoreButton();
			this.pinnedHeadLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.historyBox = new River.OneMoreAddIn.UI.MoreListView();
			this.historyHeadPanel = new River.OneMoreAddIn.UI.MorePanel();
			this.historyToolPanel = new System.Windows.Forms.Panel();
			this.deleteHistoryButton = new River.OneMoreAddIn.UI.MoreButton();
			this.pinButton = new River.OneMoreAddIn.UI.MoreButton();
			this.copyHistoryButton = new River.OneMoreAddIn.UI.MoreButton();
			this.historyHeadLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.tooltip = new System.Windows.Forms.ToolTip(this.components);
			this.controlPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainContainer)).BeginInit();
			this.mainContainer.Panel1.SuspendLayout();
			this.mainContainer.Panel2.SuspendLayout();
			this.mainContainer.SuspendLayout();
			this.pageHeadPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.subContainer)).BeginInit();
			this.subContainer.Panel1.SuspendLayout();
			this.subContainer.Panel2.SuspendLayout();
			this.subContainer.SuspendLayout();
			this.pinnedHeadPanel.SuspendLayout();
			this.pinnedToolPanel.SuspendLayout();
			this.historyHeadPanel.SuspendLayout();
			this.historyToolPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// controlPanel
			// 
			this.controlPanel.BackColor = System.Drawing.SystemColors.Control;
			this.controlPanel.Controls.Add(this.closeButton);
			this.controlPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.controlPanel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.controlPanel.Location = new System.Drawing.Point(0, 880);
			this.controlPanel.Name = "controlPanel";
			this.controlPanel.Padding = new System.Windows.Forms.Padding(0, 5, 10, 10);
			this.controlPanel.Size = new System.Drawing.Size(528, 64);
			this.controlPanel.TabIndex = 0;
			// 
			// closeButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.closeButton.ImageOver = null;
			this.closeButton.Location = new System.Drawing.Point(407, 13);
			this.closeButton.Name = "closeButton";
			this.closeButton.ShowBorder = true;
			this.closeButton.Size = new System.Drawing.Size(108, 38);
			this.closeButton.StylizeImage = false;
			this.closeButton.TabIndex = 0;
			this.closeButton.Text = "Close";
			this.closeButton.ThemedBack = null;
			this.closeButton.ThemedFore = null;
			this.closeButton.UseVisualStyleBackColor = true;
			this.closeButton.Click += new System.EventHandler(this.CloseOnClick);
			// 
			// mainContainer
			// 
			this.mainContainer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(143)))), ((int)(((byte)(32)))), ((int)(((byte)(199)))));
			this.mainContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainContainer.Location = new System.Drawing.Point(0, 0);
			this.mainContainer.Name = "mainContainer";
			this.mainContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// mainContainer.Panel1
			// 
			this.mainContainer.Panel1.BackColor = System.Drawing.SystemColors.Window;
			this.mainContainer.Panel1.Controls.Add(this.pageBox);
			this.mainContainer.Panel1.Controls.Add(this.pageHeadPanel);
			this.mainContainer.Panel1MinSize = 80;
			// 
			// mainContainer.Panel2
			// 
			this.mainContainer.Panel2.Controls.Add(this.subContainer);
			this.mainContainer.Panel2MinSize = 100;
			this.mainContainer.Size = new System.Drawing.Size(528, 880);
			this.mainContainer.SplitterDistance = 291;
			this.mainContainer.TabIndex = 1;
			// 
			// pageBox
			// 
			this.pageBox.AutoScroll = true;
			this.pageBox.BackColor = System.Drawing.SystemColors.Control;
			this.pageBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pageBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pageBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.pageBox.Location = new System.Drawing.Point(0, 41);
			this.pageBox.Name = "pageBox";
			this.pageBox.Size = new System.Drawing.Size(528, 250);
			this.pageBox.TabIndex = 0;
			this.pageBox.Resize += new System.EventHandler(this.ResizePageBox);
			// 
			// pageHeadPanel
			// 
			this.pageHeadPanel.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.pageHeadPanel.BottomBorderColor = System.Drawing.SystemColors.ActiveBorder;
			this.pageHeadPanel.BottomBorderSize = 0;
			this.pageHeadPanel.Controls.Add(this.refreshButton);
			this.pageHeadPanel.Controls.Add(this.pageHeadLabel);
			this.pageHeadPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.pageHeadPanel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.pageHeadPanel.Location = new System.Drawing.Point(0, 0);
			this.pageHeadPanel.Margin = new System.Windows.Forms.Padding(0);
			this.pageHeadPanel.Name = "pageHeadPanel";
			this.pageHeadPanel.Size = new System.Drawing.Size(528, 41);
			this.pageHeadPanel.TabIndex = 0;
			this.pageHeadPanel.ThemedBack = "ControlDarkDark";
			this.pageHeadPanel.ThemedFore = "DarkText";
			this.pageHeadPanel.TopBorderColor = System.Drawing.SystemColors.Control;
			this.pageHeadPanel.TopBorderSize = 0;
			this.pageHeadPanel.Click += new System.EventHandler(this.PanelFocusOnClick);
			// 
			// refreshButton
			// 
			this.refreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.refreshButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.refreshButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.refreshButton.FlatAppearance.BorderSize = 0;
			this.refreshButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.refreshButton.Font = new System.Drawing.Font("Segoe UI Symbol", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.refreshButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.refreshButton.ImageOver = null;
			this.refreshButton.Location = new System.Drawing.Point(488, 5);
			this.refreshButton.Name = "refreshButton";
			this.refreshButton.ShowBorder = true;
			this.refreshButton.Size = new System.Drawing.Size(32, 32);
			this.refreshButton.StylizeImage = false;
			this.refreshButton.TabIndex = 0;
			this.refreshButton.Text = "↻";
			this.refreshButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.refreshButton.ThemedBack = null;
			this.refreshButton.ThemedFore = null;
			this.tooltip.SetToolTip(this.refreshButton, "Refresh headings from current page (F5)");
			this.refreshButton.UseVisualStyleBackColor = false;
			this.refreshButton.Click += new System.EventHandler(this.RefreshPageHeadings);
			// 
			// pageHeadLabel
			// 
			this.pageHeadLabel.AutoSize = true;
			this.pageHeadLabel.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pageHeadLabel.Location = new System.Drawing.Point(12, 9);
			this.pageHeadLabel.Name = "pageHeadLabel";
			this.pageHeadLabel.Size = new System.Drawing.Size(216, 22);
			this.pageHeadLabel.TabIndex = 0;
			this.pageHeadLabel.Text = "Current page title here";
			this.pageHeadLabel.ThemedBack = null;
			this.pageHeadLabel.ThemedFore = null;
			this.pageHeadLabel.Click += new System.EventHandler(this.PanelFocusOnClick);
			// 
			// subContainer
			// 
			this.subContainer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(143)))), ((int)(((byte)(32)))), ((int)(((byte)(199)))));
			this.subContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.subContainer.Location = new System.Drawing.Point(0, 0);
			this.subContainer.Name = "subContainer";
			this.subContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// subContainer.Panel1
			// 
			this.subContainer.Panel1.BackColor = System.Drawing.SystemColors.Window;
			this.subContainer.Panel1.Controls.Add(this.pinnedBox);
			this.subContainer.Panel1.Controls.Add(this.pinnedHeadPanel);
			this.subContainer.Panel1MinSize = 80;
			// 
			// subContainer.Panel2
			// 
			this.subContainer.Panel2.BackColor = System.Drawing.SystemColors.Window;
			this.subContainer.Panel2.Controls.Add(this.historyBox);
			this.subContainer.Panel2.Controls.Add(this.historyHeadPanel);
			this.subContainer.Panel2MinSize = 80;
			this.subContainer.Size = new System.Drawing.Size(528, 585);
			this.subContainer.SplitterDistance = 253;
			this.subContainer.TabIndex = 0;
			// 
			// pinnedBox
			// 
			this.pinnedBox.AllowItemReorder = false;
			this.pinnedBox.ControlPadding = 2;
			this.pinnedBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pinnedBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pinnedBox.FullRowSelect = true;
			this.pinnedBox.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.pinnedBox.HideSelection = false;
			this.pinnedBox.HighlightBackground = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(213)))), ((int)(((byte)(228)))));
			this.pinnedBox.HighlightForeground = System.Drawing.SystemColors.HighlightText;
			this.pinnedBox.Location = new System.Drawing.Point(0, 41);
			this.pinnedBox.Name = "pinnedBox";
			this.pinnedBox.RowHeight = 29;
			this.pinnedBox.Size = new System.Drawing.Size(528, 212);
			this.pinnedBox.SortedBackground = System.Drawing.SystemColors.Window;
			this.pinnedBox.TabIndex = 0;
			this.pinnedBox.UseCompatibleStateImageBehavior = false;
			this.pinnedBox.View = System.Windows.Forms.View.Details;
			// 
			// pinnedHeadPanel
			// 
			this.pinnedHeadPanel.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.pinnedHeadPanel.BottomBorderColor = System.Drawing.SystemColors.ActiveBorder;
			this.pinnedHeadPanel.BottomBorderSize = 0;
			this.pinnedHeadPanel.Controls.Add(this.pinnedToolPanel);
			this.pinnedHeadPanel.Controls.Add(this.pinnedHeadLabel);
			this.pinnedHeadPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.pinnedHeadPanel.Location = new System.Drawing.Point(0, 0);
			this.pinnedHeadPanel.Margin = new System.Windows.Forms.Padding(0);
			this.pinnedHeadPanel.Name = "pinnedHeadPanel";
			this.pinnedHeadPanel.Padding = new System.Windows.Forms.Padding(0, 2, 5, 2);
			this.pinnedHeadPanel.Size = new System.Drawing.Size(528, 41);
			this.pinnedHeadPanel.TabIndex = 1;
			this.pinnedHeadPanel.ThemedBack = "ControlDarkDark";
			this.pinnedHeadPanel.ThemedFore = "DarkText";
			this.pinnedHeadPanel.TopBorderColor = System.Drawing.SystemColors.Control;
			this.pinnedHeadPanel.TopBorderSize = 0;
			this.pinnedHeadPanel.Click += new System.EventHandler(this.PanelFocusOnClick);
			// 
			// pinnedToolPanel
			// 
			this.pinnedToolPanel.Controls.Add(this.copyPinnedButton);
			this.pinnedToolPanel.Controls.Add(this.upButton);
			this.pinnedToolPanel.Controls.Add(this.unpinButton);
			this.pinnedToolPanel.Controls.Add(this.downButton);
			this.pinnedToolPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.pinnedToolPanel.Location = new System.Drawing.Point(335, 2);
			this.pinnedToolPanel.Name = "pinnedToolPanel";
			this.pinnedToolPanel.Size = new System.Drawing.Size(188, 37);
			this.pinnedToolPanel.TabIndex = 2;
			// 
			// copyPinnedButton
			// 
			this.copyPinnedButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.copyPinnedButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.copyPinnedButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.copyPinnedButton.FlatAppearance.BorderSize = 0;
			this.copyPinnedButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.copyPinnedButton.Font = new System.Drawing.Font("Segoe UI Symbol", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.copyPinnedButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.copyPinnedButton.ImageOver = null;
			this.copyPinnedButton.Location = new System.Drawing.Point(110, 3);
			this.copyPinnedButton.Name = "copyPinnedButton";
			this.copyPinnedButton.ShowBorder = true;
			this.copyPinnedButton.Size = new System.Drawing.Size(32, 32);
			this.copyPinnedButton.StylizeImage = false;
			this.copyPinnedButton.TabIndex = 2;
			this.copyPinnedButton.Text = "🗍";
			this.copyPinnedButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.copyPinnedButton.ThemedBack = null;
			this.copyPinnedButton.ThemedFore = null;
			this.tooltip.SetToolTip(this.copyPinnedButton, "Copy selected items to the clipboard");
			this.copyPinnedButton.UseVisualStyleBackColor = false;
			this.copyPinnedButton.Click += new System.EventHandler(this.CopyLinks);
			// 
			// upButton
			// 
			this.upButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.upButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.upButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.upButton.FlatAppearance.BorderSize = 0;
			this.upButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.upButton.Font = new System.Drawing.Font("Segoe UI Symbol", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.upButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.upButton.ImageOver = null;
			this.upButton.Location = new System.Drawing.Point(26, 3);
			this.upButton.Name = "upButton";
			this.upButton.ShowBorder = true;
			this.upButton.Size = new System.Drawing.Size(32, 32);
			this.upButton.StylizeImage = false;
			this.upButton.TabIndex = 0;
			this.upButton.Text = "▲";
			this.upButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.upButton.ThemedBack = null;
			this.upButton.ThemedFore = null;
			this.tooltip.SetToolTip(this.upButton, "Move selected items up");
			this.upButton.UseVisualStyleBackColor = false;
			this.upButton.Click += new System.EventHandler(this.MoveUpOnClick);
			// 
			// unpinButton
			// 
			this.unpinButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.unpinButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.unpinButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.unpinButton.FlatAppearance.BorderSize = 0;
			this.unpinButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.unpinButton.Font = new System.Drawing.Font("Segoe UI Symbol", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.unpinButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.unpinButton.ImageOver = null;
			this.unpinButton.Location = new System.Drawing.Point(153, 3);
			this.unpinButton.Name = "unpinButton";
			this.unpinButton.ShowBorder = true;
			this.unpinButton.Size = new System.Drawing.Size(32, 32);
			this.unpinButton.StylizeImage = false;
			this.unpinButton.TabIndex = 3;
			this.unpinButton.Text = "🗑";
			this.unpinButton.ThemedBack = null;
			this.unpinButton.ThemedFore = null;
			this.tooltip.SetToolTip(this.unpinButton, "Remove selected items from My Reading List");
			this.unpinButton.UseVisualStyleBackColor = false;
			this.unpinButton.Click += new System.EventHandler(this.UnpinOnClick);
			// 
			// downButton
			// 
			this.downButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.downButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.downButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.downButton.FlatAppearance.BorderSize = 0;
			this.downButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.downButton.Font = new System.Drawing.Font("Segoe UI Symbol", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.downButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.downButton.ImageOver = null;
			this.downButton.Location = new System.Drawing.Point(68, 3);
			this.downButton.Name = "downButton";
			this.downButton.ShowBorder = true;
			this.downButton.Size = new System.Drawing.Size(32, 32);
			this.downButton.StylizeImage = false;
			this.downButton.TabIndex = 1;
			this.downButton.Text = "▼";
			this.downButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.downButton.ThemedBack = null;
			this.downButton.ThemedFore = null;
			this.tooltip.SetToolTip(this.downButton, "Move selected items down");
			this.downButton.UseVisualStyleBackColor = false;
			this.downButton.Click += new System.EventHandler(this.MoveDownOnClick);
			// 
			// pinnedHeadLabel
			// 
			this.pinnedHeadLabel.AutoSize = true;
			this.pinnedHeadLabel.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pinnedHeadLabel.Location = new System.Drawing.Point(12, 12);
			this.pinnedHeadLabel.Name = "pinnedHeadLabel";
			this.pinnedHeadLabel.Size = new System.Drawing.Size(151, 22);
			this.pinnedHeadLabel.TabIndex = 1;
			this.pinnedHeadLabel.Text = "My Reading List";
			this.pinnedHeadLabel.ThemedBack = null;
			this.pinnedHeadLabel.ThemedFore = null;
			this.pinnedHeadLabel.Click += new System.EventHandler(this.PanelFocusOnClick);
			// 
			// historyBox
			// 
			this.historyBox.AllowItemReorder = false;
			this.historyBox.ControlPadding = 2;
			this.historyBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.historyBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.historyBox.FullRowSelect = true;
			this.historyBox.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.historyBox.HideSelection = false;
			this.historyBox.HighlightBackground = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(213)))), ((int)(((byte)(228)))));
			this.historyBox.HighlightForeground = System.Drawing.SystemColors.HighlightText;
			this.historyBox.Location = new System.Drawing.Point(0, 41);
			this.historyBox.Name = "historyBox";
			this.historyBox.RowHeight = 29;
			this.historyBox.Size = new System.Drawing.Size(528, 287);
			this.historyBox.SortedBackground = System.Drawing.SystemColors.Window;
			this.historyBox.TabIndex = 0;
			this.historyBox.UseCompatibleStateImageBehavior = false;
			this.historyBox.View = System.Windows.Forms.View.Details;
			this.historyBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.HistoryKeyDown);
			// 
			// historyHeadPanel
			// 
			this.historyHeadPanel.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.historyHeadPanel.BottomBorderColor = System.Drawing.SystemColors.ActiveBorder;
			this.historyHeadPanel.BottomBorderSize = 0;
			this.historyHeadPanel.Controls.Add(this.historyToolPanel);
			this.historyHeadPanel.Controls.Add(this.historyHeadLabel);
			this.historyHeadPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.historyHeadPanel.Location = new System.Drawing.Point(0, 0);
			this.historyHeadPanel.Margin = new System.Windows.Forms.Padding(0);
			this.historyHeadPanel.Name = "historyHeadPanel";
			this.historyHeadPanel.Padding = new System.Windows.Forms.Padding(5, 2, 5, 2);
			this.historyHeadPanel.Size = new System.Drawing.Size(528, 41);
			this.historyHeadPanel.TabIndex = 1;
			this.historyHeadPanel.ThemedBack = "ControlDarkDark";
			this.historyHeadPanel.ThemedFore = "DarkText";
			this.historyHeadPanel.TopBorderColor = System.Drawing.SystemColors.Control;
			this.historyHeadPanel.TopBorderSize = 0;
			this.historyHeadPanel.Click += new System.EventHandler(this.PanelFocusOnClick);
			// 
			// historyToolPanel
			// 
			this.historyToolPanel.Controls.Add(this.deleteHistoryButton);
			this.historyToolPanel.Controls.Add(this.pinButton);
			this.historyToolPanel.Controls.Add(this.copyHistoryButton);
			this.historyToolPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.historyToolPanel.Location = new System.Drawing.Point(335, 2);
			this.historyToolPanel.Name = "historyToolPanel";
			this.historyToolPanel.Size = new System.Drawing.Size(188, 37);
			this.historyToolPanel.TabIndex = 2;
			// 
			// deleteHistoryButton
			// 
			this.deleteHistoryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.deleteHistoryButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.deleteHistoryButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.deleteHistoryButton.FlatAppearance.BorderSize = 0;
			this.deleteHistoryButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.deleteHistoryButton.Font = new System.Drawing.Font("Segoe UI Symbol", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.deleteHistoryButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.deleteHistoryButton.ImageOver = null;
			this.deleteHistoryButton.Location = new System.Drawing.Point(153, 3);
			this.deleteHistoryButton.Name = "deleteHistoryButton";
			this.deleteHistoryButton.ShowBorder = true;
			this.deleteHistoryButton.Size = new System.Drawing.Size(32, 32);
			this.deleteHistoryButton.StylizeImage = false;
			this.deleteHistoryButton.TabIndex = 4;
			this.deleteHistoryButton.Text = "🗑";
			this.deleteHistoryButton.ThemedBack = null;
			this.deleteHistoryButton.ThemedFore = null;
			this.tooltip.SetToolTip(this.deleteHistoryButton, "Delete selected items from History");
			this.deleteHistoryButton.UseVisualStyleBackColor = false;
			this.deleteHistoryButton.Click += new System.EventHandler(this.DeleteHistoryRecords);
			// 
			// pinButton
			// 
			this.pinButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.pinButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.pinButton.FlatAppearance.BorderSize = 0;
			this.pinButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.pinButton.Font = new System.Drawing.Font("Segoe UI Symbol", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pinButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.pinButton.ImageOver = null;
			this.pinButton.Location = new System.Drawing.Point(68, 3);
			this.pinButton.Name = "pinButton";
			this.pinButton.ShowBorder = true;
			this.pinButton.Size = new System.Drawing.Size(32, 32);
			this.pinButton.StylizeImage = false;
			this.pinButton.TabIndex = 1;
			this.pinButton.Text = "👓";
			this.pinButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.pinButton.ThemedBack = null;
			this.pinButton.ThemedFore = null;
			this.tooltip.SetToolTip(this.pinButton, "Add selected items to My Reading List");
			this.pinButton.UseVisualStyleBackColor = false;
			this.pinButton.Click += new System.EventHandler(this.PinOnClick);
			// 
			// copyHistoryButton
			// 
			this.copyHistoryButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.copyHistoryButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.copyHistoryButton.FlatAppearance.BorderSize = 0;
			this.copyHistoryButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.copyHistoryButton.Font = new System.Drawing.Font("Segoe UI Symbol", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.copyHistoryButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.copyHistoryButton.ImageOver = null;
			this.copyHistoryButton.Location = new System.Drawing.Point(110, 3);
			this.copyHistoryButton.Name = "copyHistoryButton";
			this.copyHistoryButton.ShowBorder = true;
			this.copyHistoryButton.Size = new System.Drawing.Size(32, 32);
			this.copyHistoryButton.StylizeImage = false;
			this.copyHistoryButton.TabIndex = 0;
			this.copyHistoryButton.Text = "🗍";
			this.copyHistoryButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.copyHistoryButton.ThemedBack = null;
			this.copyHistoryButton.ThemedFore = null;
			this.tooltip.SetToolTip(this.copyHistoryButton, "Copy selected items to the clipboard");
			this.copyHistoryButton.UseVisualStyleBackColor = false;
			this.copyHistoryButton.Click += new System.EventHandler(this.CopyLinks);
			// 
			// historyHeadLabel
			// 
			this.historyHeadLabel.AutoSize = true;
			this.historyHeadLabel.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.historyHeadLabel.Location = new System.Drawing.Point(17, 12);
			this.historyHeadLabel.Name = "historyHeadLabel";
			this.historyHeadLabel.Size = new System.Drawing.Size(74, 22);
			this.historyHeadLabel.TabIndex = 1;
			this.historyHeadLabel.Text = "History";
			this.historyHeadLabel.ThemedBack = null;
			this.historyHeadLabel.ThemedFore = null;
			this.historyHeadLabel.Click += new System.EventHandler(this.PanelFocusOnClick);
			// 
			// NavigatorWindow
			// 
			this.AcceptButton = this.closeButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(528, 944);
			this.Controls.Add(this.mainContainer);
			this.Controls.Add(this.controlPanel);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(1000, 1500);
			this.MinimumSize = new System.Drawing.Size(400, 300);
			this.Name = "NavigatorWindow";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "OneMore Navigator";
			this.Activated += new System.EventHandler(this.TopOnShownOnActivate);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SaveOnFormClosing);
			this.Load += new System.EventHandler(this.PositionOnLoad);
			this.Shown += new System.EventHandler(this.TopOnShownOnActivate);
			this.LocationChanged += new System.EventHandler(this.TrackOnLocationChanged);
			this.SizeChanged += new System.EventHandler(this.SetLimitsOnSizeChanged);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoKeyDown);
			this.Layout += new System.Windows.Forms.LayoutEventHandler(this.TrackMinimizedOnLayout);
			this.Move += new System.EventHandler(this.RestrictOnMove);
			this.controlPanel.ResumeLayout(false);
			this.mainContainer.Panel1.ResumeLayout(false);
			this.mainContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainContainer)).EndInit();
			this.mainContainer.ResumeLayout(false);
			this.pageHeadPanel.ResumeLayout(false);
			this.pageHeadPanel.PerformLayout();
			this.subContainer.Panel1.ResumeLayout(false);
			this.subContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.subContainer)).EndInit();
			this.subContainer.ResumeLayout(false);
			this.pinnedHeadPanel.ResumeLayout(false);
			this.pinnedHeadPanel.PerformLayout();
			this.pinnedToolPanel.ResumeLayout(false);
			this.historyHeadPanel.ResumeLayout(false);
			this.historyHeadPanel.PerformLayout();
			this.historyToolPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel controlPanel;
		private UI.MoreButton closeButton;
		private System.Windows.Forms.SplitContainer mainContainer;
		private UI.MorePanel pageHeadPanel;
		private System.Windows.Forms.SplitContainer subContainer;
		private UI.MorePanel pinnedHeadPanel;
		private UI.MorePanel historyHeadPanel;
		private River.OneMoreAddIn.UI.MoreLabel pageHeadLabel;
		private River.OneMoreAddIn.UI.MoreLabel pinnedHeadLabel;
		private River.OneMoreAddIn.UI.MoreLabel historyHeadLabel;
		private River.OneMoreAddIn.UI.MoreListView pinnedBox;
		private River.OneMoreAddIn.UI.MoreListView historyBox;
		private River.OneMoreAddIn.UI.MoreFlowLayoutPanel pageBox;
		private River.OneMoreAddIn.UI.MoreButton pinButton;
		private River.OneMoreAddIn.UI.MoreButton unpinButton;
		private River.OneMoreAddIn.UI.MoreButton refreshButton;
		private System.Windows.Forms.ToolTip tooltip;
		private River.OneMoreAddIn.UI.MoreButton upButton;
		private River.OneMoreAddIn.UI.MoreButton downButton;
		private River.OneMoreAddIn.UI.MoreButton copyPinnedButton;
		private River.OneMoreAddIn.UI.MoreButton copyHistoryButton;
		private System.Windows.Forms.Panel historyToolPanel;
		private System.Windows.Forms.Panel pinnedToolPanel;
		private UI.MoreButton deleteHistoryButton;
	}
}