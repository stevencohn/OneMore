namespace River.OneMoreAddIn.Helpers.Settings
{
	partial class ContextMenuSheet
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ContextMenuSheet));
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.commandView = new System.Windows.Forms.ListView();
			this.iconColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.descriptionColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.imageList = new System.Windows.Forms.ImageList(this.components);
			this.SuspendLayout();
			// 
			// textBox1
			// 
			this.textBox1.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.textBox1.Dock = System.Windows.Forms.DockStyle.Top;
			this.textBox1.Location = new System.Drawing.Point(0, 0);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ReadOnly = true;
			this.textBox1.Size = new System.Drawing.Size(620, 54);
			this.textBox1.TabIndex = 1;
			this.textBox1.Text = "Choose commands to display in the Page context menu.\r\nMenus include all commands " +
    "in that menu.\r\n\r\n";
			// 
			// commandView
			// 
			this.commandView.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.commandView.CheckBoxes = true;
			this.commandView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.iconColumn,
            this.descriptionColumn});
			this.commandView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.commandView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.commandView.HideSelection = false;
			this.commandView.Location = new System.Drawing.Point(0, 54);
			this.commandView.MultiSelect = false;
			this.commandView.Name = "commandView";
			this.commandView.Size = new System.Drawing.Size(620, 416);
			this.commandView.SmallImageList = this.imageList;
			this.commandView.TabIndex = 2;
			this.commandView.UseCompatibleStateImageBehavior = false;
			this.commandView.View = System.Windows.Forms.View.Details;
			// 
			// imageList
			// 
			this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
			this.imageList.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList.Images.SetKeyName(0, "AddFootnote.png");
			this.imageList.Images.SetKeyName(1, "Snippets.png");
			this.imageList.Images.SetKeyName(2, "Edit.png");
			this.imageList.Images.SetKeyName(3, "NoSpellCheck.png");
			this.imageList.Images.SetKeyName(4, "PasteRtf.png");
			this.imageList.Images.SetKeyName(5, "Uppercase.png");
			this.imageList.Images.SetKeyName(6, "Lowercase.png");
			this.imageList.Images.SetKeyName(7, "IncreaseSize.png");
			this.imageList.Images.SetKeyName(8, "DecreaseSize.png");
			this.imageList.Images.SetKeyName(9, "Clean.png");
			this.imageList.Images.SetKeyName(10, "RemoveAuthor.png");
			this.imageList.Images.SetKeyName(11, "RemoveCitations.png");
			this.imageList.Images.SetKeyName(12, "RemoveDttm.png");
			this.imageList.Images.SetKeyName(13, "RemoveEmpty.png");
			this.imageList.Images.SetKeyName(14, "RemoveSpacing.png");
			this.imageList.Images.SetKeyName(15, "RemoveWhitespace.png");
			// 
			// ContextMenuSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.commandView);
			this.Controls.Add(this.textBox1);
			this.Name = "ContextMenuSheet";
			this.Size = new System.Drawing.Size(620, 470);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.ListView commandView;
		private System.Windows.Forms.ColumnHeader iconColumn;
		private System.Windows.Forms.ColumnHeader descriptionColumn;
		private System.Windows.Forms.ImageList imageList;
	}
}
