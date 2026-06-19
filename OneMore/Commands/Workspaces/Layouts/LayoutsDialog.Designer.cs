namespace River.OneMoreAddIn.Commands.Layouts
{
	partial class LayoutsDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LayoutsDialog));
			this.listView = new River.OneMoreAddIn.UI.MoreListView();
			this.nameColumn = new System.Windows.Forms.ColumnHeader();
			this.locationColumn = new System.Windows.Forms.ColumnHeader();
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.goButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.buttonPanel.SuspendLayout();
			this.SuspendLayout();
			//
			// listView
			//
			this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameColumn,
            this.locationColumn});
			this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listView.Location = new System.Drawing.Point(0, 0);
			this.listView.MultiSelect = false;
			this.listView.Name = "listView";
			this.listView.Size = new System.Drawing.Size(778, 484);
			this.listView.TabIndex = 0;
			this.listView.DoubleClick += new System.EventHandler(this.ChooseByDoubleClick);
			this.listView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ChooseByKeyboard);
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
			// LayoutsDialog
			//
			this.AcceptButton = this.goButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(778, 544);
			this.Controls.Add(this.listView);
			this.Controls.Add(this.buttonPanel);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(600, 400);
			this.Name = "LayoutsDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Layouts";
			this.Load += new System.EventHandler(this.BindOnLoad);
			this.buttonPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private River.OneMoreAddIn.UI.MoreListView listView;
		private System.Windows.Forms.ColumnHeader nameColumn;
		private System.Windows.Forms.ColumnHeader locationColumn;
		private System.Windows.Forms.Panel buttonPanel;
		private River.OneMoreAddIn.UI.MoreButton goButton;
		private River.OneMoreAddIn.UI.MoreButton cancelButton;
	}
}
