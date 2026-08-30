namespace River.OneMoreAddIn.Commands.Workspaces
{
	partial class ShowWindowsDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShowWindowsDialog));
			this.listView = new River.OneMoreAddIn.UI.MoreListView();
			this.pageColumn = new System.Windows.Forms.ColumnHeader();
			this.locationColumn = new System.Windows.Forms.ColumnHeader();
			this.zColumn = new System.Windows.Forms.ColumnHeader();
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.goButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.searchPanel = new System.Windows.Forms.Panel();
			this.searchBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.searchLabel = new System.Windows.Forms.Label();
			this.buttonPanel.SuspendLayout();
			this.searchPanel.SuspendLayout();
			this.SuspendLayout();
			//
			// listView
			//
			this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.pageColumn,
            this.locationColumn,
            this.zColumn});
			this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listView.Location = new System.Drawing.Point(0, 74);
			this.listView.MultiSelect = false;
			this.listView.Name = "listView";
			this.listView.Size = new System.Drawing.Size(778, 410);
			this.listView.TabIndex = 1;
			this.listView.DoubleClick += new System.EventHandler(this.ChooseByDoubleClick);
			this.listView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ChooseByKeyboard);
			//
			// pageColumn
			//
			this.pageColumn.Name = "pageColumn";
			this.pageColumn.Text = "Page";
			//
			// locationColumn
			//
			this.locationColumn.Name = "locationColumn";
			this.locationColumn.Text = "Location";
			//
			// zColumn
			//
			this.zColumn.Name = "zColumn";
			this.zColumn.Text = "Z-Order";
			//
			// buttonPanel
			//
			this.buttonPanel.BackColor = System.Drawing.SystemColors.ControlLight;
			this.buttonPanel.Controls.Add(this.goButton);
			this.buttonPanel.Controls.Add(this.cancelButton);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.buttonPanel.Location = new System.Drawing.Point(0, 484);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Padding = new System.Windows.Forms.Padding(8);
			this.buttonPanel.Size = new System.Drawing.Size(778, 60);
			this.buttonPanel.TabIndex = 1;
			//
			// goButton
			//
			this.goButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.goButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.goButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.goButton.ImageOver = null;
			this.goButton.Location = new System.Drawing.Point(518, 11);
			this.goButton.Name = "goButton";
			this.goButton.ShowBorder = true;
			this.goButton.Size = new System.Drawing.Size(120, 38);
			this.goButton.StylizeImage = false;
			this.goButton.TabIndex = 0;
			this.goButton.Text = "Go";
			this.goButton.ThemedBack = null;
			this.goButton.ThemedFore = null;
			this.goButton.UseVisualStyleBackColor = true;
			this.goButton.Click += new System.EventHandler(this.ChooseByClick);
			//
			// cancelButton
			//
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(644, 11);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(120, 38);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			//
			// searchPanel
			//
			this.searchPanel.BackColor = System.Drawing.SystemColors.ControlLight;
			this.searchPanel.Controls.Add(this.searchBox);
			this.searchPanel.Controls.Add(this.searchLabel);
			this.searchPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.searchPanel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.searchPanel.Location = new System.Drawing.Point(0, 0);
			this.searchPanel.Name = "searchPanel";
			this.searchPanel.Padding = new System.Windows.Forms.Padding(15);
			this.searchPanel.Size = new System.Drawing.Size(778, 74);
			this.searchPanel.TabIndex = 0;
			//
			// searchBox
			//
			this.searchBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.searchBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.searchBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.searchBox.Location = new System.Drawing.Point(138, 18);
			this.searchBox.Name = "searchBox";
			this.searchBox.ProcessEnterKey = false;
			this.searchBox.Size = new System.Drawing.Size(625, 28);
			this.searchBox.TabIndex = 0;
			this.searchBox.ThemedBack = null;
			this.searchBox.ThemedFore = null;
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
			// ShowWindowsDialog
			//
			this.AcceptButton = this.goButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(778, 544);
			this.Controls.Add(this.listView);
			this.Controls.Add(this.searchPanel);
			this.Controls.Add(this.buttonPanel);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(600, 400);
			this.Name = "ShowWindowsDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Open Windows";
			this.Activated += new System.EventHandler(this.FocusOnActivated);
			this.buttonPanel.ResumeLayout(false);
			this.searchPanel.ResumeLayout(false);
			this.searchPanel.PerformLayout();
			this.ResumeLayout(false);

		}
		#endregion

		private River.OneMoreAddIn.UI.MoreListView listView;
		private System.Windows.Forms.ColumnHeader pageColumn;
		private System.Windows.Forms.ColumnHeader locationColumn;
		private System.Windows.Forms.ColumnHeader zColumn;
		private System.Windows.Forms.Panel buttonPanel;
		private River.OneMoreAddIn.UI.MoreButton goButton;
		private River.OneMoreAddIn.UI.MoreButton cancelButton;
		private System.Windows.Forms.Panel searchPanel;
		private River.OneMoreAddIn.UI.MoreTextBox searchBox;
		private System.Windows.Forms.Label searchLabel;
	}
}
