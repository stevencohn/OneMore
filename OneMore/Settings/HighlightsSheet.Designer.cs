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
			this.themesGroup = new System.Windows.Forms.GroupBox();
			this.deepPicture = new System.Windows.Forms.PictureBox();
			this.fadedPicture = new System.Windows.Forms.PictureBox();
			this.normalPicture = new System.Windows.Forms.PictureBox();
			this.deepRadio = new System.Windows.Forms.RadioButton();
			this.themesGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.deepPicture)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.fadedPicture)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.normalPicture)).BeginInit();
			this.SuspendLayout();
			// 
			// introBox
			// 
			this.introBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.introBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.introBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.introBox.Location = new System.Drawing.Point(7, 6);
			this.introBox.Margin = new System.Windows.Forms.Padding(2);
			this.introBox.Multiline = true;
			this.introBox.Name = "introBox";
			this.introBox.ReadOnly = true;
			this.introBox.Size = new System.Drawing.Size(519, 43);
			this.introBox.TabIndex = 2;
			this.introBox.Text = "The highlighter command will cycle through the colors of the chosen theme. Colors" +
    " shown here are for light background pages. Dark background pages use automatic " +
    "highlight colors";
			// 
			// normalRadio
			// 
			this.normalRadio.Checked = true;
			this.normalRadio.Location = new System.Drawing.Point(12, 24);
			this.normalRadio.Margin = new System.Windows.Forms.Padding(2, 2, 2, 8);
			this.normalRadio.Name = "normalRadio";
			this.normalRadio.Size = new System.Drawing.Size(65, 27);
			this.normalRadio.TabIndex = 3;
			this.normalRadio.TabStop = true;
			this.normalRadio.Text = "Bright";
			this.normalRadio.UseVisualStyleBackColor = true;
			// 
			// fadedRadio
			// 
			this.fadedRadio.Location = new System.Drawing.Point(12, 61);
			this.fadedRadio.Margin = new System.Windows.Forms.Padding(2, 2, 2, 8);
			this.fadedRadio.Name = "fadedRadio";
			this.fadedRadio.Size = new System.Drawing.Size(65, 27);
			this.fadedRadio.TabIndex = 4;
			this.fadedRadio.Text = "Faded";
			this.fadedRadio.UseVisualStyleBackColor = true;
			// 
			// themesGroup
			// 
			this.themesGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.themesGroup.Controls.Add(this.deepPicture);
			this.themesGroup.Controls.Add(this.fadedPicture);
			this.themesGroup.Controls.Add(this.normalPicture);
			this.themesGroup.Controls.Add(this.deepRadio);
			this.themesGroup.Controls.Add(this.normalRadio);
			this.themesGroup.Controls.Add(this.fadedRadio);
			this.themesGroup.Location = new System.Drawing.Point(9, 53);
			this.themesGroup.Margin = new System.Windows.Forms.Padding(2);
			this.themesGroup.Name = "themesGroup";
			this.themesGroup.Padding = new System.Windows.Forms.Padding(10, 10, 2, 2);
			this.themesGroup.Size = new System.Drawing.Size(516, 183);
			this.themesGroup.TabIndex = 5;
			this.themesGroup.TabStop = false;
			this.themesGroup.Text = "Themes";
			// 
			// deepPicture
			// 
			this.deepPicture.Image = ((System.Drawing.Image)(resources.GetObject("deepPicture.Image")));
			this.deepPicture.Location = new System.Drawing.Point(82, 98);
			this.deepPicture.Name = "deepPicture";
			this.deepPicture.Size = new System.Drawing.Size(160, 27);
			this.deepPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.deepPicture.TabIndex = 8;
			this.deepPicture.TabStop = false;
			this.deepPicture.Click += new System.EventHandler(this.ClickPicture);
			// 
			// fadedPicture
			// 
			this.fadedPicture.Image = ((System.Drawing.Image)(resources.GetObject("fadedPicture.Image")));
			this.fadedPicture.Location = new System.Drawing.Point(82, 61);
			this.fadedPicture.Name = "fadedPicture";
			this.fadedPicture.Size = new System.Drawing.Size(160, 27);
			this.fadedPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.fadedPicture.TabIndex = 7;
			this.fadedPicture.TabStop = false;
			this.fadedPicture.Click += new System.EventHandler(this.ClickPicture);
			// 
			// normalPicture
			// 
			this.normalPicture.Image = ((System.Drawing.Image)(resources.GetObject("normalPicture.Image")));
			this.normalPicture.Location = new System.Drawing.Point(82, 24);
			this.normalPicture.Name = "normalPicture";
			this.normalPicture.Size = new System.Drawing.Size(160, 27);
			this.normalPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.normalPicture.TabIndex = 6;
			this.normalPicture.TabStop = false;
			this.normalPicture.Click += new System.EventHandler(this.ClickPicture);
			// 
			// deepRadio
			// 
			this.deepRadio.Location = new System.Drawing.Point(12, 98);
			this.deepRadio.Margin = new System.Windows.Forms.Padding(2, 2, 2, 8);
			this.deepRadio.Name = "deepRadio";
			this.deepRadio.Size = new System.Drawing.Size(65, 27);
			this.deepRadio.TabIndex = 5;
			this.deepRadio.Text = "Deep";
			this.deepRadio.UseVisualStyleBackColor = true;
			// 
			// HighlightsSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.themesGroup);
			this.Controls.Add(this.introBox);
			this.Margin = new System.Windows.Forms.Padding(2);
			this.Name = "HighlightsSheet";
			this.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.Size = new System.Drawing.Size(533, 325);
			this.themesGroup.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.deepPicture)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.fadedPicture)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.normalPicture)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox introBox;
		private System.Windows.Forms.RadioButton normalRadio;
		private System.Windows.Forms.RadioButton fadedRadio;
		private System.Windows.Forms.GroupBox themesGroup;
		private System.Windows.Forms.RadioButton deepRadio;
		private System.Windows.Forms.PictureBox deepPicture;
		private System.Windows.Forms.PictureBox fadedPicture;
		private System.Windows.Forms.PictureBox normalPicture;
	}
}
