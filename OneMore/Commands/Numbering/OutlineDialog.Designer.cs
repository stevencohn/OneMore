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
			this.numberingGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
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
			this.numberingGroup.SuspendLayout();
			this.indentationsGroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Enabled = false;
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(262, 432);
			this.okButton.Name = "okButton";
			this.okButton.PreferredBack = null;
			this.okButton.PreferredFore = null;
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(368, 432);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.PreferredBack = null;
			this.cancelButton.PreferredFore = null;
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// numberingGroup
			// 
			this.numberingGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.numberingGroup.Controls.Add(this.cleanBox);
			this.numberingGroup.Controls.Add(this.alphaDemoBox);
			this.numberingGroup.Controls.Add(this.alphaRadio);
			this.numberingGroup.Controls.Add(this.numDemoBox);
			this.numberingGroup.Controls.Add(this.numRadio);
			this.numberingGroup.Controls.Add(this.numberingBox);
			this.numberingGroup.Location = new System.Drawing.Point(14, 18);
			this.numberingGroup.Name = "numberingGroup";
			this.numberingGroup.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.numberingGroup.Size = new System.Drawing.Size(454, 225);
			this.numberingGroup.TabIndex = 13;
			this.numberingGroup.TabStop = false;
			this.numberingGroup.Text = "Numbering";
			// 
			// cleanBox
			// 
			this.cleanBox.Location = new System.Drawing.Point(26, 178);
			this.cleanBox.Name = "cleanBox";
			this.cleanBox.Padding = new System.Windows.Forms.Padding(0, 9, 0, 0);
			this.cleanBox.Size = new System.Drawing.Size(301, 24);
			this.cleanBox.TabIndex = 3;
			this.cleanBox.Text = "Remove/cleanup existing numbering";
			this.cleanBox.UseVisualStyleBackColor = true;
			this.cleanBox.CheckedChanged += new System.EventHandler(this.cleanBox_CheckedChanged);
			// 
			// alphaDemoBox
			// 
			this.alphaDemoBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.alphaDemoBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.alphaDemoBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.alphaDemoBox.Location = new System.Drawing.Point(82, 98);
			this.alphaDemoBox.Name = "alphaDemoBox";
			this.alphaDemoBox.PreferredBack = "ControlLightLight";
			this.alphaDemoBox.PreferredFore = "ControlText";
			this.alphaDemoBox.Size = new System.Drawing.Size(128, 67);
			this.alphaDemoBox.TabIndex = 15;
			this.alphaDemoBox.Text = "1.\r\n   a.\r\n      i.";
			// 
			// alphaRadio
			// 
			this.alphaRadio.Checked = true;
			this.alphaRadio.Enabled = false;
			this.alphaRadio.Location = new System.Drawing.Point(56, 69);
			this.alphaRadio.Name = "alphaRadio";
			this.alphaRadio.Size = new System.Drawing.Size(141, 24);
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
			this.numDemoBox.Location = new System.Drawing.Point(290, 98);
			this.numDemoBox.Name = "numDemoBox";
			this.numDemoBox.PreferredBack = "ControlLightLight";
			this.numDemoBox.PreferredFore = "ControlText";
			this.numDemoBox.Size = new System.Drawing.Size(128, 67);
			this.numDemoBox.TabIndex = 13;
			this.numDemoBox.Text = "1.\r\n   1.1.\r\n      1.1.1.";
			// 
			// numRadio
			// 
			this.numRadio.Enabled = false;
			this.numRadio.Location = new System.Drawing.Point(261, 69);
			this.numRadio.Name = "numRadio";
			this.numRadio.Size = new System.Drawing.Size(96, 24);
			this.numRadio.TabIndex = 2;
			this.numRadio.Text = "Numeric";
			this.numRadio.UseVisualStyleBackColor = true;
			// 
			// numberingBox
			// 
			this.numberingBox.Location = new System.Drawing.Point(26, 34);
			this.numberingBox.Name = "numberingBox";
			this.numberingBox.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
			this.numberingBox.Size = new System.Drawing.Size(149, 24);
			this.numberingBox.TabIndex = 0;
			this.numberingBox.Text = "Add Numbering";
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
			this.indentationsGroup.Location = new System.Drawing.Point(14, 248);
			this.indentationsGroup.Name = "indentationsGroup";
			this.indentationsGroup.Size = new System.Drawing.Size(454, 160);
			this.indentationsGroup.TabIndex = 12;
			this.indentationsGroup.TabStop = false;
			this.indentationsGroup.Text = "Indentations";
			// 
			// removeTagsBox
			// 
			this.removeTagsBox.Enabled = false;
			this.removeTagsBox.Location = new System.Drawing.Point(190, 114);
			this.removeTagsBox.Name = "removeTagsBox";
			this.removeTagsBox.Size = new System.Drawing.Size(131, 24);
			this.removeTagsBox.TabIndex = 3;
			this.removeTagsBox.Text = "Remove tags";
			this.removeTagsBox.UseVisualStyleBackColor = true;
			// 
			// tagLabel
			// 
			this.tagLabel.AutoSize = true;
			this.tagLabel.Location = new System.Drawing.Point(51, 115);
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
			this.tagButton.ImageOver = null;
			this.tagButton.Location = new System.Drawing.Point(104, 108);
			this.tagButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tagButton.Name = "tagButton";
			this.tagButton.PreferredBack = null;
			this.tagButton.PreferredFore = null;
			this.tagButton.ShowBorder = true;
			this.tagButton.Size = new System.Drawing.Size(60, 38);
			this.tagButton.TabIndex = 2;
			this.tagButton.Text = "?";
			this.tagButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.tagButton.UseVisualStyleBackColor = false;
			this.tagButton.Click += new System.EventHandler(this.tagButton_Click);
			// 
			// indentTagBox
			// 
			this.indentTagBox.Location = new System.Drawing.Point(26, 74);
			this.indentTagBox.Name = "indentTagBox";
			this.indentTagBox.Size = new System.Drawing.Size(377, 24);
			this.indentTagBox.TabIndex = 1;
			this.indentTagBox.Text = "Indent only below tagged headings/paragraphs";
			this.tooltip.SetToolTip(this.indentTagBox, "Indents content below only tagged headings or non-heading paragraphs");
			this.indentTagBox.UseVisualStyleBackColor = true;
			this.indentTagBox.CheckedChanged += new System.EventHandler(this.indentTagBox_CheckedChanged);
			// 
			// indentBox
			// 
			this.indentBox.Location = new System.Drawing.Point(26, 33);
			this.indentBox.Name = "indentBox";
			this.indentBox.Padding = new System.Windows.Forms.Padding(0, 9, 0, 0);
			this.indentBox.Size = new System.Drawing.Size(258, 24);
			this.indentBox.TabIndex = 0;
			this.indentBox.Text = "Indent content below headings";
			this.indentBox.UseVisualStyleBackColor = true;
			this.indentBox.CheckedChanged += new System.EventHandler(this.indentBox_CheckedChanged);
			// 
			// OutlineDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(482, 483);
			this.Controls.Add(this.indentationsGroup);
			this.Controls.Add(this.numberingGroup);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "OutlineDialog";
			this.Padding = new System.Windows.Forms.Padding(10, 15, 10, 9);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Outline Formatting";
			this.numberingGroup.ResumeLayout(false);
			this.indentationsGroup.ResumeLayout(false);
			this.indentationsGroup.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
        private UI.MoreGroupBox numberingGroup;
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