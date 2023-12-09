namespace River.OneMoreAddIn.Commands
{
	partial class HashtagDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HashtagDialog));
			this.cancelButton = new System.Windows.Forms.Button();
			this.contextPanel = new System.Windows.Forms.FlowLayoutPanel();
			this.topPanel = new River.OneMoreAddIn.UI.MorePanel();
			this.barLabel = new System.Windows.Forms.Label();
			this.checkAllLink = new System.Windows.Forms.LinkLabel();
			this.uncheckAllLink = new System.Windows.Forms.LinkLabel();
			this.scopeBox = new System.Windows.Forms.ComboBox();
			this.introLabel = new System.Windows.Forms.Label();
			this.searchButton = new System.Windows.Forms.Button();
			this.tagBox = new System.Windows.Forms.TextBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.indexButton = new System.Windows.Forms.Button();
			this.copyButton = new System.Windows.Forms.Button();
			this.moveButton = new System.Windows.Forms.Button();
			this.topPanel.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(864, 13);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(112, 35);
			this.cancelButton.TabIndex = 6;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.DoCancel);
			this.cancelButton.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.DoPreviewKeyDown);
			// 
			// contextPanel
			// 
			this.contextPanel.AutoScroll = true;
			this.contextPanel.BackColor = System.Drawing.SystemColors.AppWorkspace;
			this.contextPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.contextPanel.Location = new System.Drawing.Point(0, 114);
			this.contextPanel.Name = "contextPanel";
			this.contextPanel.Padding = new System.Windows.Forms.Padding(6);
			this.contextPanel.Size = new System.Drawing.Size(988, 396);
			this.contextPanel.TabIndex = 7;
			// 
			// topPanel
			// 
			this.topPanel.BackColor = System.Drawing.SystemColors.Window;
			this.topPanel.BottomBorderColor = System.Drawing.SystemColors.WindowFrame;
			this.topPanel.BottomBorderSize = 1;
			this.topPanel.Controls.Add(this.barLabel);
			this.topPanel.Controls.Add(this.checkAllLink);
			this.topPanel.Controls.Add(this.uncheckAllLink);
			this.topPanel.Controls.Add(this.scopeBox);
			this.topPanel.Controls.Add(this.introLabel);
			this.topPanel.Controls.Add(this.searchButton);
			this.topPanel.Controls.Add(this.tagBox);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = new System.Drawing.Point(0, 0);
			this.topPanel.Margin = new System.Windows.Forms.Padding(0);
			this.topPanel.Name = "topPanel";
			this.topPanel.Padding = new System.Windows.Forms.Padding(10);
			this.topPanel.Size = new System.Drawing.Size(988, 114);
			this.topPanel.TabIndex = 8;
			// 
			// barLabel
			// 
			this.barLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.barLabel.AutoSize = true;
			this.barLabel.Location = new System.Drawing.Point(864, 84);
			this.barLabel.Name = "barLabel";
			this.barLabel.Size = new System.Drawing.Size(14, 20);
			this.barLabel.TabIndex = 7;
			this.barLabel.Text = "|";
			// 
			// checkAllLink
			// 
			this.checkAllLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkAllLink.AutoSize = true;
			this.checkAllLink.Location = new System.Drawing.Point(785, 84);
			this.checkAllLink.Name = "checkAllLink";
			this.checkAllLink.Size = new System.Drawing.Size(73, 20);
			this.checkAllLink.TabIndex = 6;
			this.checkAllLink.TabStop = true;
			this.checkAllLink.Text = "Check all";
			this.checkAllLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ToggleAllChecks);
			// 
			// uncheckAllLink
			// 
			this.uncheckAllLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.uncheckAllLink.AutoSize = true;
			this.uncheckAllLink.Location = new System.Drawing.Point(884, 84);
			this.uncheckAllLink.Name = "uncheckAllLink";
			this.uncheckAllLink.Size = new System.Drawing.Size(91, 20);
			this.uncheckAllLink.TabIndex = 5;
			this.uncheckAllLink.TabStop = true;
			this.uncheckAllLink.Text = "Uncheck all";
			this.uncheckAllLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ToggleAllChecks);
			// 
			// scopeBox
			// 
			this.scopeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.scopeBox.FormattingEnabled = true;
			this.scopeBox.Items.AddRange(new object[] {
            "All",
            "This notebook",
            "This section"});
			this.scopeBox.Location = new System.Drawing.Point(387, 38);
			this.scopeBox.Name = "scopeBox";
			this.scopeBox.Size = new System.Drawing.Size(164, 28);
			this.scopeBox.TabIndex = 4;
			this.scopeBox.SelectedIndexChanged += new System.EventHandler(this.PopulateTags);
			// 
			// introLabel
			// 
			this.introLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.introLabel.AutoSize = true;
			this.introLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.introLabel.ForeColor = System.Drawing.SystemColors.Highlight;
			this.introLabel.Location = new System.Drawing.Point(831, 41);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(144, 25);
			this.introLabel.TabIndex = 3;
			this.introLabel.Text = "PROTOTYPE";
			// 
			// searchButton
			// 
			this.searchButton.Image = ((System.Drawing.Image)(resources.GetObject("searchButton.Image")));
			this.searchButton.Location = new System.Drawing.Point(557, 37);
			this.searchButton.Name = "searchButton";
			this.searchButton.Size = new System.Drawing.Size(60, 32);
			this.searchButton.TabIndex = 2;
			this.searchButton.UseVisualStyleBackColor = true;
			this.searchButton.Click += new System.EventHandler(this.SearchTags);
			// 
			// tagBox
			// 
			this.tagBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tagBox.Location = new System.Drawing.Point(41, 38);
			this.tagBox.Name = "tagBox";
			this.tagBox.Size = new System.Drawing.Size(340, 28);
			this.tagBox.TabIndex = 0;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.indexButton);
			this.panel1.Controls.Add(this.copyButton);
			this.panel1.Controls.Add(this.moveButton);
			this.panel1.Controls.Add(this.cancelButton);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 510);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(988, 60);
			this.panel1.TabIndex = 9;
			// 
			// indexButton
			// 
			this.indexButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.indexButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.indexButton.Enabled = false;
			this.indexButton.Location = new System.Drawing.Point(487, 12);
			this.indexButton.Margin = new System.Windows.Forms.Padding(4, 5, 10, 9);
			this.indexButton.Name = "indexButton";
			this.indexButton.Size = new System.Drawing.Size(112, 35);
			this.indexButton.TabIndex = 7;
			this.indexButton.Text = "Index";
			this.indexButton.UseVisualStyleBackColor = true;
			this.indexButton.Click += new System.EventHandler(this.DoSomething);
			// 
			// copyButton
			// 
			this.copyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.copyButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.copyButton.Enabled = false;
			this.copyButton.Location = new System.Drawing.Point(613, 12);
			this.copyButton.Margin = new System.Windows.Forms.Padding(4, 5, 10, 9);
			this.copyButton.Name = "copyButton";
			this.copyButton.Size = new System.Drawing.Size(112, 35);
			this.copyButton.TabIndex = 8;
			this.copyButton.Text = "Copy";
			this.copyButton.UseVisualStyleBackColor = true;
			this.copyButton.Click += new System.EventHandler(this.DoSomething);
			// 
			// moveButton
			// 
			this.moveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.moveButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.moveButton.Enabled = false;
			this.moveButton.Location = new System.Drawing.Point(739, 13);
			this.moveButton.Margin = new System.Windows.Forms.Padding(4, 5, 10, 9);
			this.moveButton.Name = "moveButton";
			this.moveButton.Size = new System.Drawing.Size(112, 35);
			this.moveButton.TabIndex = 9;
			this.moveButton.Text = "Move";
			this.moveButton.UseVisualStyleBackColor = true;
			this.moveButton.Click += new System.EventHandler(this.DoSomething);
			// 
			// HashtagDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(988, 570);
			this.Controls.Add(this.contextPanel);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.topPanel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(900, 400);
			this.Name = "HashtagDialog";
			this.Text = "Find Hashtags";
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.FlowLayoutPanel contextPanel;
		private UI.MorePanel topPanel;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.TextBox tagBox;
		private System.Windows.Forms.Button searchButton;
		private System.Windows.Forms.Label introLabel;
		private System.Windows.Forms.ComboBox scopeBox;
		private System.Windows.Forms.Label barLabel;
		private System.Windows.Forms.LinkLabel checkAllLink;
		private System.Windows.Forms.LinkLabel uncheckAllLink;
		private System.Windows.Forms.Button indexButton;
		private System.Windows.Forms.Button copyButton;
		private System.Windows.Forms.Button moveButton;
	}
}