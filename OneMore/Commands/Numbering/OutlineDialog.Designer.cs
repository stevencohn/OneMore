namespace River.OneMoreAddIn.Commands
{
	partial class OutlineDialog
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
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OutlineDialog));
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cleanBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.alphaDemoBox = new River.OneMoreAddIn.UI.MoreLabel();
			this.alphaRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.numDemoBox = new River.OneMoreAddIn.UI.MoreLabel();
			this.numRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.numberingBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.indentationsGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.removeTagsBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.tagLabel = new System.Windows.Forms.Label();
			this.tagButton = new River.OneMoreAddIn.UI.MoreButton();
			this.indentTagBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.indentBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.tooltip = new System.Windows.Forms.ToolTip(this.components);
			this.indentationsGroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Enabled = false;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(257, 429);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(363, 429);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// cleanBox
			// 
			this.cleanBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cleanBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cleanBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cleanBox.Location = new System.Drawing.Point(24, 173);
			this.cleanBox.Name = "cleanBox";
			this.cleanBox.Padding = new System.Windows.Forms.Padding(0, 9, 0, 0);
			this.cleanBox.Size = new System.Drawing.Size(302, 25);
			this.cleanBox.StylizeImage = false;
			this.cleanBox.TabIndex = 3;
			this.cleanBox.Text = "Remove/cleanup existing numbering";
			this.cleanBox.ThemedBack = null;
			this.cleanBox.ThemedFore = null;
			this.cleanBox.UseVisualStyleBackColor = true;
			this.cleanBox.CheckedChanged += new System.EventHandler(this.cleanBox_CheckedChanged);
			// 
			// alphaDemoBox
			// 
			this.alphaDemoBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.alphaDemoBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.alphaDemoBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.alphaDemoBox.Location = new System.Drawing.Point(80, 93);
			this.alphaDemoBox.Name = "alphaDemoBox";
			this.alphaDemoBox.Size = new System.Drawing.Size(128, 67);
			this.alphaDemoBox.TabIndex = 15;
			this.alphaDemoBox.Text = "1.\r\n   a.\r\n      i.";
			this.alphaDemoBox.ThemedBack = "ControlLightLight";
			this.alphaDemoBox.ThemedFore = "ControlText";
			// 
			// alphaRadio
			// 
			this.alphaRadio.Checked = true;
			this.alphaRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.alphaRadio.Enabled = false;
			this.alphaRadio.Location = new System.Drawing.Point(54, 64);
			this.alphaRadio.Name = "alphaRadio";
			this.alphaRadio.Size = new System.Drawing.Size(142, 25);
			this.alphaRadio.TabIndex = 1;
			this.alphaRadio.TabStop = true;
			this.alphaRadio.Text = "Alpha-numeric";
			this.alphaRadio.UseVisualStyleBackColor = true;
			// 
			// numDemoBox
			// 
			this.numDemoBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.numDemoBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numDemoBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.numDemoBox.Location = new System.Drawing.Point(288, 93);
			this.numDemoBox.Name = "numDemoBox";
			this.numDemoBox.Size = new System.Drawing.Size(128, 67);
			this.numDemoBox.TabIndex = 13;
			this.numDemoBox.Text = "1.\r\n   1.1.\r\n      1.1.1.";
			this.numDemoBox.ThemedBack = "ControlLightLight";
			this.numDemoBox.ThemedFore = "ControlText";
			// 
			// numRadio
			// 
			this.numRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.numRadio.Enabled = false;
			this.numRadio.Location = new System.Drawing.Point(259, 64);
			this.numRadio.Name = "numRadio";
			this.numRadio.Size = new System.Drawing.Size(97, 25);
			this.numRadio.TabIndex = 2;
			this.numRadio.Text = "Numeric";
			this.numRadio.UseVisualStyleBackColor = true;
			// 
			// numberingBox
			// 
			this.numberingBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.numberingBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.numberingBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.numberingBox.Location = new System.Drawing.Point(24, 29);
			this.numberingBox.Name = "numberingBox";
			this.numberingBox.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
			this.numberingBox.Size = new System.Drawing.Size(150, 25);
			this.numberingBox.StylizeImage = false;
			this.numberingBox.TabIndex = 0;
			this.numberingBox.Text = "Add Numbering";
			this.numberingBox.ThemedBack = null;
			this.numberingBox.ThemedFore = null;
			this.tooltip.SetToolTip(this.numberingBox, "Applies to both standard and custom Headings");
			this.numberingBox.UseVisualStyleBackColor = true;
			this.numberingBox.CheckedChanged += new System.EventHandler(this.numberingBox_CheckedChanged);
			// 
			// indentationsGroup
			// 
			this.indentationsGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.indentationsGroup.Controls.Add(this.removeTagsBox);
			this.indentationsGroup.Controls.Add(this.tagLabel);
			this.indentationsGroup.Controls.Add(this.tagButton);
			this.indentationsGroup.Controls.Add(this.indentTagBox);
			this.indentationsGroup.Controls.Add(this.indentBox);
			this.indentationsGroup.Location = new System.Drawing.Point(18, 234);
			this.indentationsGroup.Name = "indentationsGroup";
			this.indentationsGroup.ShowOnlyTopEdge = true;
			this.indentationsGroup.Size = new System.Drawing.Size(446, 160);
			this.indentationsGroup.TabIndex = 12;
			this.indentationsGroup.TabStop = false;
			this.indentationsGroup.Text = "Indentations";
			this.indentationsGroup.ThemedBorder = null;
			this.indentationsGroup.ThemedFore = null;
			// 
			// removeTagsBox
			// 
			this.removeTagsBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.removeTagsBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.removeTagsBox.Enabled = false;
			this.removeTagsBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.removeTagsBox.Location = new System.Drawing.Point(167, 114);
			this.removeTagsBox.Name = "removeTagsBox";
			this.removeTagsBox.Size = new System.Drawing.Size(132, 25);
			this.removeTagsBox.StylizeImage = false;
			this.removeTagsBox.TabIndex = 3;
			this.removeTagsBox.Text = "Remove tags";
			this.removeTagsBox.ThemedBack = null;
			this.removeTagsBox.ThemedFore = null;
			this.removeTagsBox.UseVisualStyleBackColor = true;
			// 
			// tagLabel
			// 
			this.tagLabel.AutoSize = true;
			this.tagLabel.Location = new System.Drawing.Point(35, 115);
			this.tagLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.tagLabel.Name = "tagLabel";
			this.tagLabel.Size = new System.Drawing.Size(40, 20);
			this.tagLabel.TabIndex = 18;
			this.tagLabel.Text = "Tag:";
			// 
			// tagButton
			// 
			this.tagButton.BackColor = System.Drawing.SystemColors.Window;
			this.tagButton.Enabled = false;
			this.tagButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.tagButton.ImageOver = null;
			this.tagButton.Location = new System.Drawing.Point(83, 108);
			this.tagButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tagButton.Name = "tagButton";
			this.tagButton.ShowBorder = true;
			this.tagButton.Size = new System.Drawing.Size(60, 38);
			this.tagButton.StylizeImage = false;
			this.tagButton.TabIndex = 2;
			this.tagButton.Text = "?";
			this.tagButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.tagButton.ThemedBack = null;
			this.tagButton.ThemedFore = null;
			this.tagButton.UseVisualStyleBackColor = false;
			this.tagButton.Click += new System.EventHandler(this.tagButton_Click);
			// 
			// indentTagBox
			// 
			this.indentTagBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.indentTagBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.indentTagBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.indentTagBox.Location = new System.Drawing.Point(6, 75);
			this.indentTagBox.Name = "indentTagBox";
			this.indentTagBox.Size = new System.Drawing.Size(378, 25);
			this.indentTagBox.StylizeImage = false;
			this.indentTagBox.TabIndex = 1;
			this.indentTagBox.Text = "Indent only below tagged headings/paragraphs";
			this.indentTagBox.ThemedBack = null;
			this.indentTagBox.ThemedFore = null;
			this.tooltip.SetToolTip(this.indentTagBox, "Indents content below only tagged headings or non-heading paragraphs");
			this.indentTagBox.UseVisualStyleBackColor = true;
			this.indentTagBox.CheckedChanged += new System.EventHandler(this.indentTagBox_CheckedChanged);
			// 
			// indentBox
			// 
			this.indentBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.indentBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.indentBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.indentBox.Location = new System.Drawing.Point(6, 34);
			this.indentBox.Name = "indentBox";
			this.indentBox.Padding = new System.Windows.Forms.Padding(0, 9, 0, 0);
			this.indentBox.Size = new System.Drawing.Size(259, 25);
			this.indentBox.StylizeImage = false;
			this.indentBox.TabIndex = 0;
			this.indentBox.Text = "Indent content below headings";
			this.indentBox.ThemedBack = null;
			this.indentBox.ThemedFore = null;
			this.indentBox.UseVisualStyleBackColor = true;
			this.indentBox.CheckedChanged += new System.EventHandler(this.indentBox_CheckedChanged);
			// 
			// OutlineDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(482, 480);
			this.Controls.Add(this.cleanBox);
			this.Controls.Add(this.indentationsGroup);
			this.Controls.Add(this.alphaDemoBox);
			this.Controls.Add(this.alphaRadio);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.numDemoBox);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.numRadio);
			this.Controls.Add(this.numberingBox);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "OutlineDialog";
			this.Padding = new System.Windows.Forms.Padding(15, 15, 15, 9);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Outline Formatting";
			this.indentationsGroup.ResumeLayout(false);
			this.indentationsGroup.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private UI.MoreLabel numDemoBox;
		private UI.MoreCheckBox numberingBox;
		private UI.MoreLabel alphaDemoBox;
		private UI.MoreRadioButton numRadio;
		private UI.MoreRadioButton alphaRadio;
		private UI.MoreCheckBox cleanBox;
		private UI.MoreGroupBox indentationsGroup;
		private UI.MoreCheckBox indentTagBox;
		private UI.MoreCheckBox indentBox;
		private System.Windows.Forms.Label tagLabel;
		private UI.MoreButton tagButton;
		private UI.MoreCheckBox removeTagsBox;
		private System.Windows.Forms.ToolTip tooltip;
	}
}