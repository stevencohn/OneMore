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
				textSheet?.Dispose();
				actionSheet?.Dispose();
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
			this.textTab = new System.Windows.Forms.TabPage();
			this.textSheet = new River.OneMoreAddIn.Commands.SearchDialogTextControl();
			this.actionTab = new System.Windows.Forms.TabPage();
			this.actionSheet = new River.OneMoreAddIn.Commands.SearchDialogActionControl();
			this.tabControl.SuspendLayout();
			this.textTab.SuspendLayout();
			this.actionTab.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl
			// 
			this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl.Border = "Control";
			this.tabControl.Controls.Add(this.textTab);
			this.tabControl.Controls.Add(this.actionTab);
			this.tabControl.Location = new System.Drawing.Point(10, 9);
			this.tabControl.Margin = new System.Windows.Forms.Padding(0);
			this.tabControl.Name = "tabControl";
			this.tabControl.Padding = new System.Drawing.Point(0, 0);
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(1102, 703);
			this.tabControl.TabIndex = 0;
			this.tabControl.SelectedIndexChanged += new System.EventHandler(this.TabSelected);
			// 
			// textTab
			// 
			this.textTab.Controls.Add(this.textSheet);
			this.textTab.Location = new System.Drawing.Point(4, 29);
			this.textTab.Margin = new System.Windows.Forms.Padding(0);
			this.textTab.Name = "textTab";
			this.textTab.Size = new System.Drawing.Size(1094, 670);
			this.textTab.TabIndex = 1;
			this.textTab.Text = "Search";
			this.textTab.UseVisualStyleBackColor = true;
			// 
			// textSheet
			// 
			this.textSheet.BackColor = System.Drawing.Color.Transparent;
			this.textSheet.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textSheet.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.textSheet.Location = new System.Drawing.Point(0, 0);
			this.textSheet.Margin = new System.Windows.Forms.Padding(0);
			this.textSheet.Name = "textSheet";
			this.textSheet.Padding = new System.Windows.Forms.Padding(15, 15, 15, 9);
			this.textSheet.Size = new System.Drawing.Size(1094, 670);
			this.textSheet.TabIndex = 0;
			this.textSheet.SearchClosing += new System.EventHandler<River.OneMoreAddIn.Commands.SearchCloseEventArgs>(this.ClosingSearch);
			// 
			// actionTab
			// 
			this.actionTab.Controls.Add(this.actionSheet);
			this.actionTab.Location = new System.Drawing.Point(4, 29);
			this.actionTab.Margin = new System.Windows.Forms.Padding(0);
			this.actionTab.Name = "actionTab";
			this.actionTab.Size = new System.Drawing.Size(1094, 670);
			this.actionTab.TabIndex = 0;
			this.actionTab.Text = "Search and Move";
			this.actionTab.UseVisualStyleBackColor = true;
			// 
			// actionSheet
			// 
			this.actionSheet.BackColor = System.Drawing.Color.Transparent;
			this.actionSheet.Dock = System.Windows.Forms.DockStyle.Fill;
			this.actionSheet.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.actionSheet.Location = new System.Drawing.Point(0, 0);
			this.actionSheet.Margin = new System.Windows.Forms.Padding(0);
			this.actionSheet.Name = "actionSheet";
			this.actionSheet.Padding = new System.Windows.Forms.Padding(15, 15, 15, 9);
			this.actionSheet.Size = new System.Drawing.Size(1094, 670);
			this.actionSheet.TabIndex = 0;
			this.actionSheet.SearchClosing += new System.EventHandler<River.OneMoreAddIn.Commands.SearchCloseEventArgs>(this.ClosingSearch);
			// 
			// SearchDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.ClientSize = new System.Drawing.Size(1122, 723);
			this.Controls.Add(this.tabControl);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(720, 391);
			this.Name = "SearchDialog";
			this.Padding = new System.Windows.Forms.Padding(10, 9, 10, 9);
			this.Text = "Search";
			this.tabControl.ResumeLayout(false);
			this.textTab.ResumeLayout(false);
			this.actionTab.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreTabControl tabControl;
		private System.Windows.Forms.TabPage actionTab;
		private System.Windows.Forms.TabPage textTab;
		private SearchDialogActionControl actionSheet;
		private SearchDialogTextControl textSheet;
	}
}