namespace River.OneMoreAddIn.Commands.Compare
{
	partial class CompareDialog
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
		/// Required method for Designer support - the bulk of this dialog's layout (the
		/// header/legend, action toolbars, and their buttons) is built programmatically in
		/// CompareDialog.cs instead of here, since it depends on runtime data (the diff tree,
		/// node names) that the designer can't represent.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CompareDialog));
			this.diffView = new River.OneMoreAddIn.Commands.Compare.HierarchyDiffView();
			this.SuspendLayout();
			//
			// diffView
			//
			this.diffView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.diffView.Name = "diffView";
			this.diffView.TabIndex = 0;
			//
			// CompareDialog
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1800, 1200);
			this.Controls.Add(this.diffView);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.MinimumSize = new System.Drawing.Size(700, 480);
			this.Name = "CompareDialog";
			this.ShowInTaskbar = true;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.ResumeLayout(false);

		}

		#endregion

		private HierarchyDiffView diffView;
	}
}
