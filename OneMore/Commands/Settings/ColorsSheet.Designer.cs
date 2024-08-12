
namespace River.OneMoreAddIn.Settings
{
	partial class ColorsSheet
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
			this.introBox = new River.OneMoreAddIn.UI.MoreMultilineLabel();
			this.layoutPanel = new System.Windows.Forms.Panel();
			this.strikeGroupBox = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.strikeBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.strikeColorLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.strikeClickLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.strikeColorBox = new River.OneMoreAddIn.UI.MorePictureBox();
			this.linesGroupBox = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.lineColorLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.lineClickLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.lineColorBox = new River.OneMoreAddIn.UI.MorePictureBox();
			this.lengthBox = new System.Windows.Forms.NumericUpDown();
			this.lengthLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.layoutPanel.SuspendLayout();
			this.strikeGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.strikeColorBox)).BeginInit();
			this.linesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.lineColorBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.lengthBox)).BeginInit();
			this.SuspendLayout();
			// 
			// introBox
			// 
			this.introBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.introBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.introBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.introBox.Location = new System.Drawing.Point(13, 8);
			this.introBox.Name = "introBox";
			this.introBox.Padding = new System.Windows.Forms.Padding(0, 0, 0, 36);
			this.introBox.Size = new System.Drawing.Size(772, 66);
			this.introBox.TabIndex = 3;
			this.introBox.Text = "Customize the default style for horizontal line snippets and task marked by the S" +
    "trike-Through Completed To Do Tags command";
			this.introBox.ThemedBack = "ControlLightLight";
			this.introBox.ThemedFore = "ControlText";
			// 
			// layoutPanel
			// 
			this.layoutPanel.Controls.Add(this.strikeGroupBox);
			this.layoutPanel.Controls.Add(this.linesGroupBox);
			this.layoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.layoutPanel.Location = new System.Drawing.Point(13, 74);
			this.layoutPanel.Margin = new System.Windows.Forms.Padding(0);
			this.layoutPanel.Name = "layoutPanel";
			this.layoutPanel.Padding = new System.Windows.Forms.Padding(5, 15, 5, 5);
			this.layoutPanel.Size = new System.Drawing.Size(772, 416);
			this.layoutPanel.TabIndex = 4;
			// 
			// strikeGroupBox
			// 
			this.strikeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.strikeGroupBox.BorderThickness = 3;
			this.strikeGroupBox.Controls.Add(this.strikeBox);
			this.strikeGroupBox.Controls.Add(this.strikeColorLabel);
			this.strikeGroupBox.Controls.Add(this.strikeClickLabel);
			this.strikeGroupBox.Controls.Add(this.strikeColorBox);
			this.strikeGroupBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.strikeGroupBox.Location = new System.Drawing.Point(8, 182);
			this.strikeGroupBox.Name = "strikeGroupBox";
			this.strikeGroupBox.Padding = new System.Windows.Forms.Padding(20, 10, 10, 10);
			this.strikeGroupBox.ShowOnlyTopEdge = true;
			this.strikeGroupBox.Size = new System.Drawing.Size(756, 161);
			this.strikeGroupBox.TabIndex = 6;
			this.strikeGroupBox.TabStop = false;
			this.strikeGroupBox.Text = "Strikethrough To Do Tags";
			this.strikeGroupBox.ThemedBorder = null;
			this.strikeGroupBox.ThemedFore = null;
			// 
			// strikeBox
			// 
			this.strikeBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.strikeBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.strikeBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.strikeBox.Location = new System.Drawing.Point(27, 55);
			this.strikeBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 15);
			this.strikeBox.Name = "strikeBox";
			this.strikeBox.Size = new System.Drawing.Size(626, 25);
			this.strikeBox.StylizeImage = false;
			this.strikeBox.TabIndex = 5;
			this.strikeBox.Text = "Apply this color when using the Strike-Through Completed To Do Tags command";
			this.strikeBox.ThemedBack = null;
			this.strikeBox.ThemedFore = null;
			this.strikeBox.UseVisualStyleBackColor = false;
			this.strikeBox.CheckedChanged += new System.EventHandler(this.ApplyOnCheckedChanged);
			// 
			// strikeColorLabel
			// 
			this.strikeColorLabel.AutoSize = true;
			this.strikeColorLabel.Location = new System.Drawing.Point(23, 98);
			this.strikeColorLabel.Name = "strikeColorLabel";
			this.strikeColorLabel.Size = new System.Drawing.Size(46, 20);
			this.strikeColorLabel.TabIndex = 0;
			this.strikeColorLabel.Text = "Color";
			this.strikeColorLabel.ThemedBack = null;
			this.strikeColorLabel.ThemedFore = null;
			// 
			// strikeClickLabel
			// 
			this.strikeClickLabel.AutoSize = true;
			this.strikeClickLabel.Location = new System.Drawing.Point(312, 98);
			this.strikeClickLabel.Name = "strikeClickLabel";
			this.strikeClickLabel.Size = new System.Drawing.Size(124, 20);
			this.strikeClickLabel.TabIndex = 4;
			this.strikeClickLabel.Text = "(click to change)";
			this.strikeClickLabel.ThemedBack = null;
			this.strikeClickLabel.ThemedFore = "GrayText";
			// 
			// strikeColorBox
			// 
			this.strikeColorBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.strikeColorBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.strikeColorBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.strikeColorBox.Enabled = false;
			this.strikeColorBox.Location = new System.Drawing.Point(169, 98);
			this.strikeColorBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 15);
			this.strikeColorBox.Name = "strikeColorBox";
			this.strikeColorBox.Size = new System.Drawing.Size(132, 20);
			this.strikeColorBox.TabIndex = 1;
			this.strikeColorBox.TabStop = false;
			this.strikeColorBox.Click += new System.EventHandler(this.ChangeLineColor);
			// 
			// linesGroupBox
			// 
			this.linesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.linesGroupBox.BorderThickness = 3;
			this.linesGroupBox.Controls.Add(this.lineColorLabel);
			this.linesGroupBox.Controls.Add(this.lineClickLabel);
			this.linesGroupBox.Controls.Add(this.lineColorBox);
			this.linesGroupBox.Controls.Add(this.lengthBox);
			this.linesGroupBox.Controls.Add(this.lengthLabel);
			this.linesGroupBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.linesGroupBox.Location = new System.Drawing.Point(8, 18);
			this.linesGroupBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 25);
			this.linesGroupBox.Name = "linesGroupBox";
			this.linesGroupBox.Padding = new System.Windows.Forms.Padding(20, 10, 10, 15);
			this.linesGroupBox.ShowOnlyTopEdge = true;
			this.linesGroupBox.Size = new System.Drawing.Size(756, 136);
			this.linesGroupBox.TabIndex = 5;
			this.linesGroupBox.TabStop = false;
			this.linesGroupBox.Text = "Horizontal Lines";
			this.linesGroupBox.ThemedBorder = null;
			this.linesGroupBox.ThemedFore = null;
			// 
			// lineColorLabel
			// 
			this.lineColorLabel.AutoSize = true;
			this.lineColorLabel.Location = new System.Drawing.Point(23, 56);
			this.lineColorLabel.Name = "lineColorLabel";
			this.lineColorLabel.Size = new System.Drawing.Size(46, 20);
			this.lineColorLabel.TabIndex = 0;
			this.lineColorLabel.Text = "Color";
			this.lineColorLabel.ThemedBack = null;
			this.lineColorLabel.ThemedFore = null;
			// 
			// lineClickLabel
			// 
			this.lineClickLabel.AutoSize = true;
			this.lineClickLabel.Location = new System.Drawing.Point(312, 56);
			this.lineClickLabel.Name = "lineClickLabel";
			this.lineClickLabel.Size = new System.Drawing.Size(124, 20);
			this.lineClickLabel.TabIndex = 4;
			this.lineClickLabel.Text = "(click to change)";
			this.lineClickLabel.ThemedBack = null;
			this.lineClickLabel.ThemedFore = "ControlText";
			// 
			// lineColorBox
			// 
			this.lineColorBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.lineColorBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lineColorBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.lineColorBox.Location = new System.Drawing.Point(169, 56);
			this.lineColorBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 15);
			this.lineColorBox.Name = "lineColorBox";
			this.lineColorBox.Size = new System.Drawing.Size(132, 20);
			this.lineColorBox.TabIndex = 1;
			this.lineColorBox.TabStop = false;
			this.lineColorBox.Click += new System.EventHandler(this.ChangeLineColor);
			// 
			// lengthBox
			// 
			this.lengthBox.Location = new System.Drawing.Point(169, 94);
			this.lengthBox.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.lengthBox.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.lengthBox.Name = "lengthBox";
			this.lengthBox.Size = new System.Drawing.Size(132, 26);
			this.lengthBox.TabIndex = 3;
			this.lengthBox.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
			// 
			// lengthLabel
			// 
			this.lengthLabel.AutoSize = true;
			this.lengthLabel.Location = new System.Drawing.Point(23, 96);
			this.lengthLabel.Name = "lengthLabel";
			this.lengthLabel.Size = new System.Drawing.Size(63, 20);
			this.lengthLabel.TabIndex = 2;
			this.lengthLabel.Text = "Length:";
			this.lengthLabel.ThemedBack = null;
			this.lengthLabel.ThemedFore = null;
			// 
			// ColorsSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.layoutPanel);
			this.Controls.Add(this.introBox);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Name = "ColorsSheet";
			this.Padding = new System.Windows.Forms.Padding(13, 8, 15, 10);
			this.Size = new System.Drawing.Size(800, 500);
			this.layoutPanel.ResumeLayout(false);
			this.strikeGroupBox.ResumeLayout(false);
			this.strikeGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.strikeColorBox)).EndInit();
			this.linesGroupBox.ResumeLayout(false);
			this.linesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.lineColorBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.lengthBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private River.OneMoreAddIn.UI.MoreMultilineLabel introBox;
		private System.Windows.Forms.Panel layoutPanel;
		private River.OneMoreAddIn.UI.MoreLabel lineClickLabel;
		private System.Windows.Forms.NumericUpDown lengthBox;
		private River.OneMoreAddIn.UI.MoreLabel lengthLabel;
		private River.OneMoreAddIn.UI.MorePictureBox lineColorBox;
		private River.OneMoreAddIn.UI.MoreLabel lineColorLabel;
		private River.OneMoreAddIn.UI.MoreGroupBox strikeGroupBox;
		private River.OneMoreAddIn.UI.MoreLabel strikeColorLabel;
		private River.OneMoreAddIn.UI.MoreLabel strikeClickLabel;
		private River.OneMoreAddIn.UI.MorePictureBox strikeColorBox;
		private River.OneMoreAddIn.UI.MoreGroupBox linesGroupBox;
		private River.OneMoreAddIn.UI.MoreCheckBox strikeBox;
	}
}
