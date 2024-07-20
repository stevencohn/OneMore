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
			this.introBox = new River.OneMoreAddIn.UI.MoreMultilineLabel();
			this.normalRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.fadedRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.themesGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.deepPicture = new System.Windows.Forms.PictureBox();
			this.fadedPicture = new System.Windows.Forms.PictureBox();
			this.normalPicture = new System.Windows.Forms.PictureBox();
			this.deepRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.themesGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.deepPicture)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.fadedPicture)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.normalPicture)).BeginInit();
			this.SuspendLayout();
			// 
			// introBox
			// 
			this.introBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.introBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.introBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.introBox.Location = new System.Drawing.Point(10, 9);
			this.introBox.Name = "introBox";
			this.introBox.Padding = new System.Windows.Forms.Padding(0, 0, 0, 36);
			this.introBox.Size = new System.Drawing.Size(780, 66);
			this.introBox.TabIndex = 2;
			this.introBox.Text = "The highlighter command will cycle through the colors of the chosen theme. Colors" +
    " shown here are for light background pages. Dark background pages use automatic " +
    "highlight colors";
			this.introBox.ThemedBack = "ControlLightLight";
			this.introBox.ThemedFore = "ControlText";
			// 
			// normalRadio
			// 
			this.normalRadio.Checked = true;
			this.normalRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.normalRadio.Location = new System.Drawing.Point(18, 46);
			this.normalRadio.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
			this.normalRadio.Name = "normalRadio";
			this.normalRadio.Size = new System.Drawing.Size(79, 25);
			this.normalRadio.TabIndex = 3;
			this.normalRadio.TabStop = true;
			this.normalRadio.Text = "Bright";
			this.normalRadio.UseVisualStyleBackColor = true;
			// 
			// fadedRadio
			// 
			this.fadedRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.fadedRadio.Location = new System.Drawing.Point(18, 104);
			this.fadedRadio.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
			this.fadedRadio.Name = "fadedRadio";
			this.fadedRadio.Size = new System.Drawing.Size(82, 25);
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
			this.themesGroup.Location = new System.Drawing.Point(14, 82);
			this.themesGroup.Name = "themesGroup";
			this.themesGroup.Padding = new System.Windows.Forms.Padding(15, 15, 3, 3);
			this.themesGroup.ShowOnlyTopEdge = true;
			this.themesGroup.Size = new System.Drawing.Size(774, 282);
			this.themesGroup.TabIndex = 5;
			this.themesGroup.TabStop = false;
			this.themesGroup.Text = "Themes";
			this.themesGroup.ThemedBorder = null;
			this.themesGroup.ThemedFore = null;
			// 
			// deepPicture
			// 
			this.deepPicture.Image = ((System.Drawing.Image)(resources.GetObject("deepPicture.Image")));
			this.deepPicture.Location = new System.Drawing.Point(148, 151);
			this.deepPicture.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.deepPicture.Name = "deepPicture";
			this.deepPicture.Size = new System.Drawing.Size(240, 42);
			this.deepPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.deepPicture.TabIndex = 8;
			this.deepPicture.TabStop = false;
			this.deepPicture.Click += new System.EventHandler(this.ClickPicture);
			// 
			// fadedPicture
			// 
			this.fadedPicture.Image = ((System.Drawing.Image)(resources.GetObject("fadedPicture.Image")));
			this.fadedPicture.Location = new System.Drawing.Point(148, 94);
			this.fadedPicture.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.fadedPicture.Name = "fadedPicture";
			this.fadedPicture.Size = new System.Drawing.Size(240, 42);
			this.fadedPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.fadedPicture.TabIndex = 7;
			this.fadedPicture.TabStop = false;
			this.fadedPicture.Click += new System.EventHandler(this.ClickPicture);
			// 
			// normalPicture
			// 
			this.normalPicture.Image = ((System.Drawing.Image)(resources.GetObject("normalPicture.Image")));
			this.normalPicture.Location = new System.Drawing.Point(148, 37);
			this.normalPicture.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.normalPicture.Name = "normalPicture";
			this.normalPicture.Size = new System.Drawing.Size(240, 42);
			this.normalPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.normalPicture.TabIndex = 6;
			this.normalPicture.TabStop = false;
			this.normalPicture.Click += new System.EventHandler(this.ClickPicture);
			// 
			// deepRadio
			// 
			this.deepRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.deepRadio.Location = new System.Drawing.Point(18, 161);
			this.deepRadio.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
			this.deepRadio.Name = "deepRadio";
			this.deepRadio.Size = new System.Drawing.Size(75, 25);
			this.deepRadio.TabIndex = 5;
			this.deepRadio.Text = "Deep";
			this.deepRadio.UseVisualStyleBackColor = true;
			// 
			// HighlightsSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.themesGroup);
			this.Controls.Add(this.introBox);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Name = "HighlightsSheet";
			this.Padding = new System.Windows.Forms.Padding(10, 9, 10, 9);
			this.Size = new System.Drawing.Size(800, 500);
			this.themesGroup.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.deepPicture)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.fadedPicture)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.normalPicture)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreMultilineLabel introBox;
		private UI.MoreRadioButton normalRadio;
		private UI.MoreRadioButton fadedRadio;
		private UI.MoreGroupBox themesGroup;
		private UI.MoreRadioButton deepRadio;
		private System.Windows.Forms.PictureBox deepPicture;
		private System.Windows.Forms.PictureBox fadedPicture;
		private System.Windows.Forms.PictureBox normalPicture;
	}
}
