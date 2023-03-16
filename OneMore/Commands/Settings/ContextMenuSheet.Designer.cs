namespace River.OneMoreAddIn.Settings
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
			this.introBox = new System.Windows.Forms.TextBox();
			this.contentPanel = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			// 
			// introBox
			// 
			this.introBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.introBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.introBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.introBox.Location = new System.Drawing.Point(10, 10);
			this.introBox.Multiline = true;
			this.introBox.Name = "introBox";
			this.introBox.ReadOnly = true;
			this.introBox.Size = new System.Drawing.Size(600, 66);
			this.introBox.TabIndex = 1;
			this.introBox.Text = "Choose commands to display in the Page context menu. Menu items include all comma" +
    "nds in that menu. Restart OneNote to see changes.\r\n";
			// 
			// contentPanel
			// 
			this.contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.contentPanel.Location = new System.Drawing.Point(10, 76);
			this.contentPanel.Name = "contentPanel";
			this.contentPanel.Size = new System.Drawing.Size(600, 384);
			this.contentPanel.TabIndex = 2;
			// 
			// ContextMenuSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.contentPanel);
			this.Controls.Add(this.introBox);
			this.Name = "ContextMenuSheet";
			this.Padding = new System.Windows.Forms.Padding(10);
			this.Size = new System.Drawing.Size(620, 470);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox introBox;
		private System.Windows.Forms.Panel contentPanel;
	}
}
