
namespace River.OneMoreAddIn.Commands
{
	partial class TaggingDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TaggingDialog));
			this.entryPanel = new System.Windows.Forms.Panel();
			this.tagsFlow = new System.Windows.Forms.FlowLayoutPanel();
			this.addButton = new System.Windows.Forms.Button();
			this.tagBox = new River.OneMoreAddIn.Commands.TaggingDialog.TagsTextBox();
			this.introLabel = new System.Windows.Forms.Label();
			this.tagLabel = new System.Windows.Forms.Label();
			this.splitter = new System.Windows.Forms.Splitter();
			this.suggestionPanel = new System.Windows.Forms.Panel();
			this.suggestionSplits = new System.Windows.Forms.SplitContainer();
			this.recentGroup = new System.Windows.Forms.GroupBox();
			this.recentFlow = new System.Windows.Forms.FlowLayoutPanel();
			this.wordsGroup = new System.Windows.Forms.GroupBox();
			this.wordsFlow = new System.Windows.Forms.FlowLayoutPanel();
			this.suggestionsLabel = new System.Windows.Forms.Label();
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.entryPanel.SuspendLayout();
			this.suggestionPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.suggestionSplits)).BeginInit();
			this.suggestionSplits.Panel1.SuspendLayout();
			this.suggestionSplits.Panel2.SuspendLayout();
			this.suggestionSplits.SuspendLayout();
			this.recentGroup.SuspendLayout();
			this.wordsGroup.SuspendLayout();
			this.buttonPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// entryPanel
			// 
			this.entryPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.entryPanel.Controls.Add(this.tagsFlow);
			this.entryPanel.Controls.Add(this.addButton);
			this.entryPanel.Controls.Add(this.tagBox);
			this.entryPanel.Controls.Add(this.introLabel);
			this.entryPanel.Controls.Add(this.tagLabel);
			this.entryPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.entryPanel.Location = new System.Drawing.Point(0, 0);
			this.entryPanel.Name = "entryPanel";
			this.entryPanel.Padding = new System.Windows.Forms.Padding(10);
			this.entryPanel.Size = new System.Drawing.Size(778, 207);
			this.entryPanel.TabIndex = 0;
			// 
			// tagsFlow
			// 
			this.tagsFlow.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tagsFlow.AutoScroll = true;
			this.tagsFlow.Location = new System.Drawing.Point(13, 96);
			this.tagsFlow.Name = "tagsFlow";
			this.tagsFlow.Size = new System.Drawing.Size(752, 98);
			this.tagsFlow.TabIndex = 4;
			// 
			// addButton
			// 
			this.addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.addButton.Location = new System.Drawing.Point(665, 64);
			this.addButton.Name = "addButton";
			this.addButton.Size = new System.Drawing.Size(100, 30);
			this.addButton.TabIndex = 1;
			this.addButton.Text = "Add";
			this.addButton.UseVisualStyleBackColor = true;
			this.addButton.Click += new System.EventHandler(this.AcceptInput);
			// 
			// tagBox
			// 
			this.tagBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tagBox.Location = new System.Drawing.Point(13, 64);
			this.tagBox.Name = "tagBox";
			this.tagBox.Size = new System.Drawing.Size(646, 26);
			this.tagBox.TabIndex = 0;
			// 
			// introLabel
			// 
			this.introLabel.AutoSize = true;
			this.introLabel.Location = new System.Drawing.Point(13, 41);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(206, 20);
			this.introLabel.TabIndex = 1;
			this.introLabel.Text = "Separate tags with commas";
			// 
			// tagLabel
			// 
			this.tagLabel.AutoSize = true;
			this.tagLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tagLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
			this.tagLabel.Location = new System.Drawing.Point(13, 19);
			this.tagLabel.Name = "tagLabel";
			this.tagLabel.Size = new System.Drawing.Size(94, 22);
			this.tagLabel.TabIndex = 0;
			this.tagLabel.Text = "Hashtags";
			// 
			// splitter
			// 
			this.splitter.BackColor = System.Drawing.SystemColors.ActiveCaption;
			this.splitter.Dock = System.Windows.Forms.DockStyle.Top;
			this.splitter.Location = new System.Drawing.Point(0, 207);
			this.splitter.Name = "splitter";
			this.splitter.Size = new System.Drawing.Size(778, 6);
			this.splitter.TabIndex = 1;
			this.splitter.TabStop = false;
			// 
			// suggestionPanel
			// 
			this.suggestionPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.suggestionPanel.Controls.Add(this.suggestionSplits);
			this.suggestionPanel.Controls.Add(this.suggestionsLabel);
			this.suggestionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.suggestionPanel.Location = new System.Drawing.Point(0, 213);
			this.suggestionPanel.Name = "suggestionPanel";
			this.suggestionPanel.Padding = new System.Windows.Forms.Padding(10);
			this.suggestionPanel.Size = new System.Drawing.Size(778, 396);
			this.suggestionPanel.TabIndex = 2;
			// 
			// suggestionSplits
			// 
			this.suggestionSplits.Dock = System.Windows.Forms.DockStyle.Fill;
			this.suggestionSplits.IsSplitterFixed = true;
			this.suggestionSplits.Location = new System.Drawing.Point(10, 32);
			this.suggestionSplits.Name = "suggestionSplits";
			this.suggestionSplits.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// suggestionSplits.Panel1
			// 
			this.suggestionSplits.Panel1.Controls.Add(this.recentGroup);
			// 
			// suggestionSplits.Panel2
			// 
			this.suggestionSplits.Panel2.Controls.Add(this.wordsGroup);
			this.suggestionSplits.Size = new System.Drawing.Size(758, 354);
			this.suggestionSplits.SplitterDistance = 178;
			this.suggestionSplits.SplitterWidth = 1;
			this.suggestionSplits.TabIndex = 8;
			// 
			// recentGroup
			// 
			this.recentGroup.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.recentGroup.Controls.Add(this.recentFlow);
			this.recentGroup.Dock = System.Windows.Forms.DockStyle.Fill;
			this.recentGroup.Location = new System.Drawing.Point(0, 0);
			this.recentGroup.Name = "recentGroup";
			this.recentGroup.Size = new System.Drawing.Size(758, 178);
			this.recentGroup.TabIndex = 6;
			this.recentGroup.TabStop = false;
			this.recentGroup.Text = "Recently used";
			// 
			// recentFlow
			// 
			this.recentFlow.AutoScroll = true;
			this.recentFlow.Dock = System.Windows.Forms.DockStyle.Fill;
			this.recentFlow.Location = new System.Drawing.Point(3, 22);
			this.recentFlow.Name = "recentFlow";
			this.recentFlow.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
			this.recentFlow.Size = new System.Drawing.Size(752, 153);
			this.recentFlow.TabIndex = 5;
			// 
			// wordsGroup
			// 
			this.wordsGroup.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.wordsGroup.Controls.Add(this.wordsFlow);
			this.wordsGroup.Dock = System.Windows.Forms.DockStyle.Fill;
			this.wordsGroup.Location = new System.Drawing.Point(0, 0);
			this.wordsGroup.Name = "wordsGroup";
			this.wordsGroup.Size = new System.Drawing.Size(758, 175);
			this.wordsGroup.TabIndex = 7;
			this.wordsGroup.TabStop = false;
			this.wordsGroup.Text = "Common words on this page";
			// 
			// wordsFlow
			// 
			this.wordsFlow.AutoScroll = true;
			this.wordsFlow.Dock = System.Windows.Forms.DockStyle.Fill;
			this.wordsFlow.Location = new System.Drawing.Point(3, 22);
			this.wordsFlow.Name = "wordsFlow";
			this.wordsFlow.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
			this.wordsFlow.Size = new System.Drawing.Size(752, 150);
			this.wordsFlow.TabIndex = 5;
			// 
			// suggestionsLabel
			// 
			this.suggestionsLabel.AutoSize = true;
			this.suggestionsLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.suggestionsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.suggestionsLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
			this.suggestionsLabel.Location = new System.Drawing.Point(10, 10);
			this.suggestionsLabel.Name = "suggestionsLabel";
			this.suggestionsLabel.Size = new System.Drawing.Size(120, 22);
			this.suggestionsLabel.TabIndex = 1;
			this.suggestionsLabel.Text = "Suggestions";
			// 
			// buttonPanel
			// 
			this.buttonPanel.Controls.Add(this.okButton);
			this.buttonPanel.Controls.Add(this.cancelButton);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.Location = new System.Drawing.Point(0, 609);
			this.buttonPanel.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Padding = new System.Windows.Forms.Padding(0, 0, 10, 10);
			this.buttonPanel.Size = new System.Drawing.Size(778, 54);
			this.buttonPanel.TabIndex = 3;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(559, 11);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(100, 30);
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(665, 11);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(100, 30);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// TaggingDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(778, 663);
			this.Controls.Add(this.suggestionPanel);
			this.Controls.Add(this.buttonPanel);
			this.Controls.Add(this.splitter);
			this.Controls.Add(this.entryPanel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(700, 400);
			this.Name = "TaggingDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Page Hashtags";
			this.entryPanel.ResumeLayout(false);
			this.entryPanel.PerformLayout();
			this.suggestionPanel.ResumeLayout(false);
			this.suggestionPanel.PerformLayout();
			this.suggestionSplits.Panel1.ResumeLayout(false);
			this.suggestionSplits.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.suggestionSplits)).EndInit();
			this.suggestionSplits.ResumeLayout(false);
			this.recentGroup.ResumeLayout(false);
			this.wordsGroup.ResumeLayout(false);
			this.buttonPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel entryPanel;
		private System.Windows.Forms.FlowLayoutPanel tagsFlow;
		private System.Windows.Forms.Button addButton;
		private TagsTextBox tagBox;
		private System.Windows.Forms.Label introLabel;
		private System.Windows.Forms.Label tagLabel;
		private System.Windows.Forms.Splitter splitter;
		private System.Windows.Forms.Panel suggestionPanel;
		private System.Windows.Forms.Panel buttonPanel;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.GroupBox recentGroup;
		private System.Windows.Forms.GroupBox wordsGroup;
		private System.Windows.Forms.FlowLayoutPanel wordsFlow;
		private System.Windows.Forms.FlowLayoutPanel recentFlow;
		private System.Windows.Forms.Label suggestionsLabel;
		private System.Windows.Forms.SplitContainer suggestionSplits;
	}
}