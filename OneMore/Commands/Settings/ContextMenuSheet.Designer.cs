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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ContextMenuSheet));
			this.introBox = new UI.MoreMultilineLabel();
			this.contentPanel = new System.Windows.Forms.Panel();
			this.linePanel = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			// 
			// introBox
			// 
			this.introBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.introBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.introBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.introBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.introBox.Location = new System.Drawing.Point(10, 10);
			this.introBox.Name = "introBox";
			this.introBox.ThemedBack = "ControlLightLight";
			this.introBox.ThemedFore = "ControlText";
			this.introBox.Size = new System.Drawing.Size(757, 158);
			this.introBox.TabIndex = 1;
			this.introBox.Text = resources.GetString("introBox.Text");
			// 
			// contentPanel
			// 
			this.contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.contentPanel.Location = new System.Drawing.Point(10, 173);
			this.contentPanel.Name = "contentPanel";
			this.contentPanel.Size = new System.Drawing.Size(757, 398);
			this.contentPanel.TabIndex = 2;
			// 
			// linePanel
			// 
			this.linePanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(115)))), ((int)(((byte)(53)))), ((int)(((byte)(110)))));
			this.linePanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.linePanel.Location = new System.Drawing.Point(10, 168);
			this.linePanel.Name = "linePanel";
			this.linePanel.Size = new System.Drawing.Size(757, 5);
			this.linePanel.TabIndex = 3;
			// 
			// ContextMenuSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.contentPanel);
			this.Controls.Add(this.linePanel);
			this.Controls.Add(this.introBox);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Name = "ContextMenuSheet";
			this.Padding = new System.Windows.Forms.Padding(10);
			this.Size = new System.Drawing.Size(777, 581);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreMultilineLabel introBox;
		private System.Windows.Forms.Panel contentPanel;
		private System.Windows.Forms.Panel linePanel;
	}
}
