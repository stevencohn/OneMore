namespace River.OneMoreAddIn.Commands
{
	partial class InsertTocDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InsertTocDialog));
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.pageRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.sectionRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.topBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.notebookRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.pagesBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.rightAlignBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.previewBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.preview2Box = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.locationBox = new System.Windows.Forms.ComboBox();
			this.locationLabel = new System.Windows.Forms.Label();
			this.styleLabel = new System.Windows.Forms.Label();
			this.styleBox = new System.Windows.Forms.ComboBox();
			this.tabs = new River.OneMoreAddIn.UI.MoreTabControl();
			this.insertTab = new System.Windows.Forms.TabPage();
			this.exportTab = new System.Windows.Forms.TabPage();
			this.browseButton = new River.OneMoreAddIn.UI.MoreButton();
			this.pathBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.pathLabel = new System.Windows.Forms.Label();
			this.exportLabel = new River.OneMoreAddIn.UI.MoreMultilineLabel();
			this.scopeSelector = new River.OneMoreAddIn.UI.ScopeSelector();
			this.tabs.SuspendLayout();
			this.insertTab.SuspendLayout();
			this.exportTab.SuspendLayout();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(286, 473);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(120, 38);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(412, 473);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(120, 38);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// pageRadio
			// 
			this.pageRadio.Checked = true;
			this.pageRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pageRadio.Location = new System.Drawing.Point(13, 13);
			this.pageRadio.Name = "pageRadio";
			this.pageRadio.Size = new System.Drawing.Size(300, 25);
			this.pageRadio.TabIndex = 2;
			this.pageRadio.TabStop = true;
			this.pageRadio.Text = "Insert table of headings on this page";
			this.pageRadio.UseVisualStyleBackColor = true;
			this.pageRadio.CheckedChanged += new System.EventHandler(this.ChangedRadio);
			// 
			// sectionRadio
			// 
			this.sectionRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.sectionRadio.Location = new System.Drawing.Point(13, 206);
			this.sectionRadio.Margin = new System.Windows.Forms.Padding(3, 25, 3, 3);
			this.sectionRadio.Name = "sectionRadio";
			this.sectionRadio.Size = new System.Drawing.Size(359, 25);
			this.sectionRadio.TabIndex = 7;
			this.sectionRadio.Text = "New page with index of pages in this section";
			this.sectionRadio.UseVisualStyleBackColor = true;
			this.sectionRadio.CheckedChanged += new System.EventHandler(this.ChangedRadio);
			// 
			// topBox
			// 
			this.topBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.topBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.topBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.topBox.Location = new System.Drawing.Point(41, 46);
			this.topBox.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.topBox.Name = "topBox";
			this.topBox.Size = new System.Drawing.Size(378, 25);
			this.topBox.StylizeImage = false;
			this.topBox.TabIndex = 3;
			this.topBox.Text = "Add link to each heading to jump to top of page";
			this.topBox.ThemedBack = null;
			this.topBox.ThemedFore = null;
			this.topBox.UseVisualStyleBackColor = true;
			this.topBox.CheckedChanged += new System.EventHandler(this.ToggleRightAlignOption);
			// 
			// notebookRadio
			// 
			this.notebookRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.notebookRadio.Location = new System.Drawing.Point(13, 298);
			this.notebookRadio.Margin = new System.Windows.Forms.Padding(3, 25, 3, 3);
			this.notebookRadio.Name = "notebookRadio";
			this.notebookRadio.Size = new System.Drawing.Size(390, 25);
			this.notebookRadio.TabIndex = 9;
			this.notebookRadio.Text = "New page with index of sections in this notebook";
			this.notebookRadio.UseVisualStyleBackColor = true;
			this.notebookRadio.CheckedChanged += new System.EventHandler(this.ChangedRadio);
			// 
			// pagesBox
			// 
			this.pagesBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.pagesBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pagesBox.Enabled = false;
			this.pagesBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.pagesBox.Location = new System.Drawing.Point(41, 331);
			this.pagesBox.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.pagesBox.Name = "pagesBox";
			this.pagesBox.Size = new System.Drawing.Size(252, 25);
			this.pagesBox.StylizeImage = false;
			this.pagesBox.TabIndex = 10;
			this.pagesBox.Text = "Include pages in each section";
			this.pagesBox.ThemedBack = null;
			this.pagesBox.ThemedFore = null;
			this.pagesBox.UseVisualStyleBackColor = true;
			this.pagesBox.CheckedChanged += new System.EventHandler(this.PagesBoxCheckedChanged);
			// 
			// rightAlignBox
			// 
			this.rightAlignBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.rightAlignBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.rightAlignBox.Enabled = false;
			this.rightAlignBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.rightAlignBox.Location = new System.Drawing.Point(41, 76);
			this.rightAlignBox.Name = "rightAlignBox";
			this.rightAlignBox.Size = new System.Drawing.Size(230, 25);
			this.rightAlignBox.StylizeImage = false;
			this.rightAlignBox.TabIndex = 4;
			this.rightAlignBox.Text = "Right-align top of page link";
			this.rightAlignBox.ThemedBack = null;
			this.rightAlignBox.ThemedFore = null;
			this.rightAlignBox.UseVisualStyleBackColor = true;
			// 
			// previewBox
			// 
			this.previewBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.previewBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.previewBox.Enabled = false;
			this.previewBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.previewBox.Location = new System.Drawing.Point(41, 239);
			this.previewBox.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.previewBox.Name = "previewBox";
			this.previewBox.Size = new System.Drawing.Size(280, 25);
			this.previewBox.StylizeImage = false;
			this.previewBox.TabIndex = 8;
			this.previewBox.Text = "Include text preview of each page";
			this.previewBox.ThemedBack = null;
			this.previewBox.ThemedFore = null;
			this.previewBox.UseVisualStyleBackColor = true;
			// 
			// preview2Box
			// 
			this.preview2Box.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.preview2Box.Cursor = System.Windows.Forms.Cursors.Hand;
			this.preview2Box.Enabled = false;
			this.preview2Box.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.preview2Box.Location = new System.Drawing.Point(41, 363);
			this.preview2Box.Name = "preview2Box";
			this.preview2Box.Size = new System.Drawing.Size(280, 25);
			this.preview2Box.StylizeImage = false;
			this.preview2Box.TabIndex = 11;
			this.preview2Box.Text = "Include text preview of each page";
			this.preview2Box.ThemedBack = null;
			this.preview2Box.ThemedFore = null;
			this.preview2Box.UseVisualStyleBackColor = true;
			// 
			// locationBox
			// 
			this.locationBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.locationBox.FormattingEnabled = true;
			this.locationBox.Items.AddRange(new object[] {
            "At top of page",
            "At current cursor"});
			this.locationBox.Location = new System.Drawing.Point(212, 109);
			this.locationBox.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.locationBox.Name = "locationBox";
			this.locationBox.Size = new System.Drawing.Size(217, 28);
			this.locationBox.TabIndex = 5;
			// 
			// locationLabel
			// 
			this.locationLabel.AutoSize = true;
			this.locationLabel.Location = new System.Drawing.Point(37, 112);
			this.locationLabel.Name = "locationLabel";
			this.locationLabel.Size = new System.Drawing.Size(115, 20);
			this.locationLabel.TabIndex = 11;
			this.locationLabel.Text = "Insert Location";
			// 
			// styleLabel
			// 
			this.styleLabel.AutoSize = true;
			this.styleLabel.Location = new System.Drawing.Point(37, 149);
			this.styleLabel.Name = "styleLabel";
			this.styleLabel.Size = new System.Drawing.Size(77, 20);
			this.styleLabel.TabIndex = 12;
			this.styleLabel.Text = "Title Style";
			// 
			// styleBox
			// 
			this.styleBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.styleBox.FormattingEnabled = true;
			this.styleBox.Items.AddRange(new object[] {
            "Standard Page Title",
            "Standard Heading 1",
            "Standard Heading 2",
            "Standard Heading 3",
            "Custom Page Title",
            "Custom Heading 1",
            "Custom Heading 2",
            "Custom Heading 3"});
			this.styleBox.Location = new System.Drawing.Point(212, 146);
			this.styleBox.Name = "styleBox";
			this.styleBox.Size = new System.Drawing.Size(217, 28);
			this.styleBox.TabIndex = 6;
			// 
			// tabs
			// 
			this.tabs.Controls.Add(this.insertTab);
			this.tabs.Controls.Add(this.exportTab);
			this.tabs.Location = new System.Drawing.Point(15, 15);
			this.tabs.Margin = new System.Windows.Forms.Padding(0);
			this.tabs.Multiline = true;
			this.tabs.Name = "tabs";
			this.tabs.Padding = new System.Drawing.Point(0, 0);
			this.tabs.SelectedIndex = 0;
			this.tabs.Size = new System.Drawing.Size(521, 444);
			this.tabs.TabIndex = 13;
			// 
			// insertTab
			// 
			this.insertTab.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.insertTab.Controls.Add(this.pageRadio);
			this.insertTab.Controls.Add(this.styleBox);
			this.insertTab.Controls.Add(this.sectionRadio);
			this.insertTab.Controls.Add(this.styleLabel);
			this.insertTab.Controls.Add(this.topBox);
			this.insertTab.Controls.Add(this.locationLabel);
			this.insertTab.Controls.Add(this.notebookRadio);
			this.insertTab.Controls.Add(this.locationBox);
			this.insertTab.Controls.Add(this.pagesBox);
			this.insertTab.Controls.Add(this.preview2Box);
			this.insertTab.Controls.Add(this.rightAlignBox);
			this.insertTab.Controls.Add(this.previewBox);
			this.insertTab.Location = new System.Drawing.Point(4, 29);
			this.insertTab.Name = "insertTab";
			this.insertTab.Padding = new System.Windows.Forms.Padding(10);
			this.insertTab.Size = new System.Drawing.Size(513, 411);
			this.insertTab.TabIndex = 0;
			this.insertTab.Text = "Insert Table";
			// 
			// exportTab
			// 
			this.exportTab.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.exportTab.Controls.Add(this.browseButton);
			this.exportTab.Controls.Add(this.pathBox);
			this.exportTab.Controls.Add(this.pathLabel);
			this.exportTab.Controls.Add(this.exportLabel);
			this.exportTab.Controls.Add(this.scopeSelector);
			this.exportTab.Location = new System.Drawing.Point(4, 29);
			this.exportTab.Name = "exportTab";
			this.exportTab.Padding = new System.Windows.Forms.Padding(10);
			this.exportTab.Size = new System.Drawing.Size(513, 411);
			this.exportTab.TabIndex = 1;
			this.exportTab.Text = "Export Data";
			// 
			// browseButton
			// 
			this.browseButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.browseButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.browseButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_FileOpen;
			this.browseButton.ImageOver = null;
			this.browseButton.Location = new System.Drawing.Point(456, 240);
			this.browseButton.Name = "browseButton";
			this.browseButton.ShowBorder = false;
			this.browseButton.Size = new System.Drawing.Size(44, 34);
			this.browseButton.StylizeImage = true;
			this.browseButton.TabIndex = 7;
			this.browseButton.ThemedBack = null;
			this.browseButton.ThemedFore = null;
			this.browseButton.UseVisualStyleBackColor = true;
			this.browseButton.Click += new System.EventHandler(this.BrowseExportFile);
			// 
			// pathBox
			// 
			this.pathBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pathBox.Location = new System.Drawing.Point(17, 245);
			this.pathBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 9);
			this.pathBox.Name = "pathBox";
			this.pathBox.ProcessEnterKey = false;
			this.pathBox.Size = new System.Drawing.Size(433, 26);
			this.pathBox.TabIndex = 6;
			this.pathBox.ThemedBack = null;
			this.pathBox.ThemedFore = null;
			// 
			// pathLabel
			// 
			this.pathLabel.AutoSize = true;
			this.pathLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.pathLabel.Location = new System.Drawing.Point(13, 222);
			this.pathLabel.Name = "pathLabel";
			this.pathLabel.Size = new System.Drawing.Size(66, 20);
			this.pathLabel.TabIndex = 5;
			this.pathLabel.Text = "Save as";
			// 
			// exportLabel
			// 
			this.exportLabel.Location = new System.Drawing.Point(15, 13);
			this.exportLabel.Name = "exportLabel";
			this.exportLabel.Size = new System.Drawing.Size(485, 76);
			this.exportLabel.TabIndex = 1;
			this.exportLabel.Text = "Export table of contents as an Excel CSV including time stamps and other details";
			this.exportLabel.ThemedBack = null;
			this.exportLabel.ThemedFore = null;
			// 
			// scopeSelector
			// 
			this.scopeSelector.Location = new System.Drawing.Point(15, 95);
			this.scopeSelector.Name = "scopeSelector";
			this.scopeSelector.Scopes = ((River.OneMoreAddIn.UI.SelectorScope)(((River.OneMoreAddIn.UI.SelectorScope.Section | River.OneMoreAddIn.UI.SelectorScope.Notebook) 
            | River.OneMoreAddIn.UI.SelectorScope.Notebooks)));
			this.scopeSelector.Size = new System.Drawing.Size(485, 96);
			this.scopeSelector.TabIndex = 0;
			// 
			// InsertTocDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(554, 523);
			this.Controls.Add(this.tabs);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "InsertTocDialog";
			this.Padding = new System.Windows.Forms.Padding(15, 15, 15, 0);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Table of Contents";
			this.tabs.ResumeLayout(false);
			this.insertTab.ResumeLayout(false);
			this.insertTab.PerformLayout();
			this.exportTab.ResumeLayout(false);
			this.exportTab.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private UI.MoreRadioButton pageRadio;
		private UI.MoreRadioButton sectionRadio;
		private UI.MoreCheckBox topBox;
		private UI.MoreRadioButton notebookRadio;
		private UI.MoreCheckBox pagesBox;
		private UI.MoreCheckBox rightAlignBox;
		private UI.MoreCheckBox previewBox;
		private UI.MoreCheckBox preview2Box;
		private System.Windows.Forms.ComboBox locationBox;
		private System.Windows.Forms.Label locationLabel;
		private System.Windows.Forms.Label styleLabel;
		private System.Windows.Forms.ComboBox styleBox;
		private UI.MoreTabControl tabs;
		private System.Windows.Forms.TabPage insertTab;
		private System.Windows.Forms.TabPage exportTab;
		private UI.MoreMultilineLabel exportLabel;
		private UI.ScopeSelector scopeSelector;
		private UI.MoreButton browseButton;
		private UI.MoreTextBox pathBox;
		private System.Windows.Forms.Label pathLabel;
	}
}