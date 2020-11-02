namespace River.OneMoreAddIn.Settings
{
	partial class RibbonBarSheet
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
			this.introBox.Size = new System.Drawing.Size(780, 66);
			this.introBox.TabIndex = 3;
			this.introBox.Text = "The highlighter command will cycle through the colors of the chosen theme. Colors" +
    " shown here are for light background pages. Dark background pages use automatic " +
    "highlight colors";
			// 
			// RibbonBarSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.introBox);
			this.Name = "RibbonBarSheet";
			this.Padding = new System.Windows.Forms.Padding(10);
			this.Size = new System.Drawing.Size(800, 500);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox introBox;
	}
}
