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
			this.textSheet = new River.OneMoreAddIn.Commands.SearchDialogTextControl();
			this.SuspendLayout();
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
			this.textSheet.Size = new System.Drawing.Size(1122, 723);
			this.textSheet.TabIndex = 0;
			this.textSheet.SearchClosing += new System.EventHandler<River.OneMoreAddIn.Commands.SearchCloseEventArgs>(this.ClosingSearch);
			//
			// SearchDialog
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.ClientSize = new System.Drawing.Size(1122, 723);
			this.Controls.Add(this.textSheet);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(720, 391);
			this.Name = "SearchDialog";
			this.Padding = new System.Windows.Forms.Padding(0);
			this.Text = "Search";
			this.ResumeLayout(false);

		}

		#endregion

		private SearchDialogTextControl textSheet;
	}
}