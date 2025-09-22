namespace River.OneMoreAddIn.Commands
{
	partial class SearchDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SearchDialog));
			this.tabControl = new River.OneMoreAddIn.UI.MoreTabControl();
			this.searchTab = new System.Windows.Forms.TabPage();
			this.searchDialogTextControl = new River.OneMoreAddIn.Commands.SearchDialogTextControl();
			this.searchAndGoTab = new System.Windows.Forms.TabPage();
			this.searchAndGoControl = new River.OneMoreAddIn.Commands.SearchDialogActionControl();
			this.tabControl.SuspendLayout();
			this.searchTab.SuspendLayout();
			this.searchAndGoTab.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl
			// 
			this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl.Controls.Add(this.searchTab);
			this.tabControl.Controls.Add(this.searchAndGoTab);
			this.tabControl.Location = new System.Drawing.Point(7, 6);
			this.tabControl.Margin = new System.Windows.Forms.Padding(0);
			this.tabControl.Name = "tabControl";
			this.tabControl.Padding = new System.Drawing.Point(0, 0);
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(735, 457);
			this.tabControl.TabIndex = 7;
			// 
			// searchTab
			// 
			this.searchTab.Controls.Add(this.searchDialogTextControl);
			this.searchTab.Location = new System.Drawing.Point(4, 25);
			this.searchTab.Margin = new System.Windows.Forms.Padding(0);
			this.searchTab.Name = "searchTab";
			this.searchTab.Size = new System.Drawing.Size(727, 428);
			this.searchTab.TabIndex = 1;
			this.searchTab.Text = "Search";
			this.searchTab.UseVisualStyleBackColor = true;
			// 
			// searchDialogTextControl
			// 
			this.searchDialogTextControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.searchDialogTextControl.BackColor = System.Drawing.Color.Transparent;
			this.searchDialogTextControl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.searchDialogTextControl.Location = new System.Drawing.Point(0, 0);
			this.searchDialogTextControl.Margin = new System.Windows.Forms.Padding(0);
			this.searchDialogTextControl.Name = "searchDialogTextControl";
			this.searchDialogTextControl.Padding = new System.Windows.Forms.Padding(10, 10, 10, 6);
			this.searchDialogTextControl.Size = new System.Drawing.Size(727, 428);
			this.searchDialogTextControl.TabIndex = 0;
			// 
			// searchAndGoTab
			// 
			this.searchAndGoTab.Controls.Add(this.searchAndGoControl);
			this.searchAndGoTab.Location = new System.Drawing.Point(4, 25);
			this.searchAndGoTab.Margin = new System.Windows.Forms.Padding(2);
			this.searchAndGoTab.Name = "searchAndGoTab";
			this.searchAndGoTab.Padding = new System.Windows.Forms.Padding(2);
			this.searchAndGoTab.Size = new System.Drawing.Size(727, 428);
			this.searchAndGoTab.TabIndex = 0;
			this.searchAndGoTab.Text = "Search and Move";
			this.searchAndGoTab.UseVisualStyleBackColor = true;
			// 
			// searchAndGoControl
			// 
			this.searchAndGoControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.searchAndGoControl.BackColor = System.Drawing.Color.Transparent;
			this.searchAndGoControl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.searchAndGoControl.Location = new System.Drawing.Point(2, 2);
			this.searchAndGoControl.Margin = new System.Windows.Forms.Padding(0);
			this.searchAndGoControl.Name = "searchAndGoControl";
			this.searchAndGoControl.Padding = new System.Windows.Forms.Padding(10, 10, 10, 6);
			this.searchAndGoControl.Size = new System.Drawing.Size(723, 424);
			this.searchAndGoControl.TabIndex = 0;
			// 
			// SearchDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.ClientSize = new System.Drawing.Size(748, 470);
			this.Controls.Add(this.tabControl);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(487, 274);
			this.Name = "SearchDialog";
			this.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Search and Move or Copy";
			this.tabControl.ResumeLayout(false);
			this.searchTab.ResumeLayout(false);
			this.searchAndGoTab.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreTabControl tabControl;
		private System.Windows.Forms.TabPage searchAndGoTab;
		private System.Windows.Forms.TabPage searchTab;
		private SearchDialogActionControl searchAndGoControl;
		private SearchDialogTextControl searchDialogTextControl;
	}
}