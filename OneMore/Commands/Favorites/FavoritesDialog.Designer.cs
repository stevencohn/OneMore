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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FavoritesDialog));
			this.gridView = new System.Windows.Forms.DataGridView();
			this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.locationColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.goButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.searchPanel = new System.Windows.Forms.Panel();
			this.searchBox = new System.Windows.Forms.TextBox();
			this.searchLabel = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.gridView)).BeginInit();
			this.buttonPanel.SuspendLayout();
			this.searchPanel.SuspendLayout();
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
			this.gridView.Size = new System.Drawing.Size(778, 425);
			this.gridView.TabIndex = 0;
			this.gridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.ValidateOnCellFormatting);
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
			this.buttonPanel.Location = new System.Drawing.Point(0, 499);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Size = new System.Drawing.Size(778, 45);
			this.buttonPanel.TabIndex = 4;
			// 
			// goButton
			// 
			this.goButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.goButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.goButton.Location = new System.Drawing.Point(526, 3);
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
			this.cancelButton.Location = new System.Drawing.Point(652, 3);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(120, 38);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// searchPanel
			// 
			this.searchPanel.Controls.Add(this.searchBox);
			this.searchPanel.Controls.Add(this.searchLabel);
			this.searchPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.searchPanel.Location = new System.Drawing.Point(0, 0);
			this.searchPanel.Name = "searchPanel";
			this.searchPanel.Padding = new System.Windows.Forms.Padding(15);
			this.searchPanel.Size = new System.Drawing.Size(778, 74);
			this.searchPanel.TabIndex = 5;
			// 
			// searchBox
			// 
			this.searchBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.searchBox.Location = new System.Drawing.Point(138, 18);
			this.searchBox.Name = "searchBox";
			this.searchBox.Size = new System.Drawing.Size(385, 28);
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
	}
}