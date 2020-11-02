namespace River.OneMoreAddIn.Settings
{
	partial class HighlightsSheet
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HighlightsSheet));
			this.introBox = new System.Windows.Forms.TextBox();
			this.normalRadio = new System.Windows.Forms.RadioButton();
			this.fadedRadio = new System.Windows.Forms.RadioButton();
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
			this.introBox.TabIndex = 2;
			this.introBox.Text = "The highlighter command will cycle through the colors of the chosen theme. Colors" +
    " shown here are for light background pages. Dark background pages use automatic " +
    "highlight colors";
			// 
			// normalRadio
			// 
			this.normalRadio.AutoSize = true;
			this.normalRadio.Checked = true;
			this.normalRadio.Image = ((System.Drawing.Image)(resources.GetObject("normalRadio.Image")));
			this.normalRadio.Location = new System.Drawing.Point(13, 82);
			this.normalRadio.Name = "normalRadio";
			this.normalRadio.Size = new System.Drawing.Size(242, 41);
			this.normalRadio.TabIndex = 3;
			this.normalRadio.TabStop = true;
			this.normalRadio.UseVisualStyleBackColor = true;
			// 
			// fadedRadio
			// 
			this.fadedRadio.AutoSize = true;
			this.fadedRadio.Image = ((System.Drawing.Image)(resources.GetObject("fadedRadio.Image")));
			this.fadedRadio.Location = new System.Drawing.Point(13, 142);
			this.fadedRadio.Name = "fadedRadio";
			this.fadedRadio.Size = new System.Drawing.Size(242, 41);
			this.fadedRadio.TabIndex = 4;
			this.fadedRadio.UseVisualStyleBackColor = true;
			// 
			// HighlightsSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.fadedRadio);
			this.Controls.Add(this.normalRadio);
			this.Controls.Add(this.introBox);
			this.Name = "HighlightsSheet";
			this.Padding = new System.Windows.Forms.Padding(10);
			this.Size = new System.Drawing.Size(800, 500);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox introBox;
		private System.Windows.Forms.RadioButton normalRadio;
		private System.Windows.Forms.RadioButton fadedRadio;
	}
}
