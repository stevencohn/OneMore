
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
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.controlPanel = new System.Windows.Forms.Panel();
			this.tagLabel = new System.Windows.Forms.Label();
			this.introLabel = new System.Windows.Forms.Label();
			this.addButton = new System.Windows.Forms.Button();
			this.tagBox = new River.OneMoreAddIn.UI.FormTextBox();
			this.splitter = new System.Windows.Forms.SplitContainer();
			this.tagsFlow = new System.Windows.Forms.FlowLayoutPanel();
			this.clearPanel = new System.Windows.Forms.Panel();
			this.clearLabel = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.suggestionPanel = new System.Windows.Forms.Panel();
			this.suggestionsFlow = new System.Windows.Forms.FlowLayoutPanel();
			this.recentGroup = new System.Windows.Forms.GroupBox();
			this.recentFlow = new System.Windows.Forms.FlowLayoutPanel();
			this.commonGroup = new System.Windows.Forms.GroupBox();
			this.commonFlow = new System.Windows.Forms.FlowLayoutPanel();
			this.suggestionsLabel = new System.Windows.Forms.Label();
			this.buttonPanel.SuspendLayout();
			this.controlPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitter)).BeginInit();
			this.splitter.Panel1.SuspendLayout();
			this.splitter.Panel2.SuspendLayout();
			this.splitter.SuspendLayout();
			this.clearPanel.SuspendLayout();
			this.suggestionPanel.SuspendLayout();
			this.suggestionsFlow.SuspendLayout();
			this.recentGroup.SuspendLayout();
			this.commonGroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonPanel
			// 
			this.buttonPanel.Controls.Add(this.okButton);
			this.buttonPanel.Controls.Add(this.cancelButton);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.Location = new System.Drawing.Point(0, 638);
			this.buttonPanel.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Padding = new System.Windows.Forms.Padding(0, 0, 10, 10);
			this.buttonPanel.Size = new System.Drawing.Size(941, 54);
			this.buttonPanel.TabIndex = 3;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(722, 11);
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
			this.cancelButton.Location = new System.Drawing.Point(828, 11);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(100, 30);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// controlPanel
			// 
			this.controlPanel.Controls.Add(this.tagLabel);
			this.controlPanel.Controls.Add(this.introLabel);
			this.controlPanel.Controls.Add(this.addButton);
			this.controlPanel.Controls.Add(this.tagBox);
			this.controlPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.controlPanel.Location = new System.Drawing.Point(0, 0);
			this.controlPanel.Name = "controlPanel";
			this.controlPanel.Size = new System.Drawing.Size(941, 106);
			this.controlPanel.TabIndex = 9;
			// 
			// tagLabel
			// 
			this.tagLabel.AutoSize = true;
			this.tagLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tagLabel.ForeColor = System.Drawing.SystemColors.GrayText;
			this.tagLabel.Location = new System.Drawing.Point(9, 3);
			this.tagLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 4);
			this.tagLabel.Name = "tagLabel";
			this.tagLabel.Size = new System.Drawing.Size(107, 22);
			this.tagLabel.TabIndex = 0;
			this.tagLabel.Text = "Page Tags";
			// 
			// introLabel
			// 
			this.introLabel.AutoSize = true;
			this.introLabel.Location = new System.Drawing.Point(9, 29);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(206, 20);
			this.introLabel.TabIndex = 1;
			this.introLabel.Text = "Separate tags with commas";
			// 
			// addButton
			// 
			this.addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.addButton.Location = new System.Drawing.Point(828, 50);
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
			this.tagBox.BackColor = System.Drawing.SystemColors.Info;
			this.tagBox.Location = new System.Drawing.Point(18, 52);
			this.tagBox.Name = "tagBox";
			this.tagBox.Size = new System.Drawing.Size(804, 26);
			this.tagBox.TabIndex = 0;
			// 
			// splitter
			// 
			this.splitter.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
			this.splitter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitter.Location = new System.Drawing.Point(0, 106);
			this.splitter.Margin = new System.Windows.Forms.Padding(3, 4, 3, 3);
			this.splitter.Name = "splitter";
			this.splitter.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitter.Panel1
			// 
			this.splitter.Panel1.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.splitter.Panel1.Controls.Add(this.tagsFlow);
			this.splitter.Panel1.Controls.Add(this.clearPanel);
			this.splitter.Panel1.Padding = new System.Windows.Forms.Padding(15, 0, 15, 0);
			this.splitter.Panel1MinSize = 100;
			// 
			// splitter.Panel2
			// 
			this.splitter.Panel2.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.splitter.Panel2.Controls.Add(this.suggestionPanel);
			this.splitter.Panel2.Margin = new System.Windows.Forms.Padding(0, 0, 15, 0);
			this.splitter.Panel2MinSize = 300;
			this.splitter.Size = new System.Drawing.Size(941, 532);
			this.splitter.SplitterDistance = 180;
			this.splitter.SplitterWidth = 8;
			this.splitter.TabIndex = 8;
			// 
			// tagsFlow
			// 
			this.tagsFlow.AutoScroll = true;
			this.tagsFlow.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.tagsFlow.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tagsFlow.Location = new System.Drawing.Point(15, 0);
			this.tagsFlow.MinimumSize = new System.Drawing.Size(0, 100);
			this.tagsFlow.Name = "tagsFlow";
			this.tagsFlow.Size = new System.Drawing.Size(911, 145);
			this.tagsFlow.TabIndex = 4;
			// 
			// clearPanel
			// 
			this.clearPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.clearPanel.Controls.Add(this.clearLabel);
			this.clearPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.clearPanel.Location = new System.Drawing.Point(15, 145);
			this.clearPanel.Name = "clearPanel";
			this.clearPanel.Size = new System.Drawing.Size(911, 35);
			this.clearPanel.TabIndex = 6;
			// 
			// clearLabel
			// 
			this.clearLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.clearLabel.AutoSize = true;
			this.clearLabel.Cursor = System.Windows.Forms.Cursors.Hand;
			this.clearLabel.Enabled = false;
			this.clearLabel.LinkColor = System.Drawing.Color.DodgerBlue;
			this.clearLabel.Location = new System.Drawing.Point(822, 7);
			this.clearLabel.Margin = new System.Windows.Forms.Padding(3, 8, 8, 8);
			this.clearLabel.Name = "clearLabel";
			this.clearLabel.Size = new System.Drawing.Size(87, 20);
			this.clearLabel.TabIndex = 5;
			this.clearLabel.TabStop = true;
			this.clearLabel.Text = "Remove all";
			this.clearLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.RemoveAllTags);
			// 
			// suggestionPanel
			// 
			this.suggestionPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.suggestionPanel.Controls.Add(this.suggestionsFlow);
			this.suggestionPanel.Controls.Add(this.suggestionsLabel);
			this.suggestionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.suggestionPanel.Location = new System.Drawing.Point(0, 0);
			this.suggestionPanel.MinimumSize = new System.Drawing.Size(0, 200);
			this.suggestionPanel.Name = "suggestionPanel";
			this.suggestionPanel.Padding = new System.Windows.Forms.Padding(10);
			this.suggestionPanel.Size = new System.Drawing.Size(941, 344);
			this.suggestionPanel.TabIndex = 2;
			// 
			// suggestionsFlow
			// 
			this.suggestionsFlow.AutoScroll = true;
			this.suggestionsFlow.Controls.Add(this.recentGroup);
			this.suggestionsFlow.Controls.Add(this.commonGroup);
			this.suggestionsFlow.Dock = System.Windows.Forms.DockStyle.Fill;
			this.suggestionsFlow.Location = new System.Drawing.Point(10, 38);
			this.suggestionsFlow.MinimumSize = new System.Drawing.Size(400, 170);
			this.suggestionsFlow.Name = "suggestionsFlow";
			this.suggestionsFlow.Size = new System.Drawing.Size(921, 296);
			this.suggestionsFlow.TabIndex = 5;
			this.suggestionsFlow.Resize += new System.EventHandler(this.SuggestionsResize);
			// 
			// recentGroup
			// 
			this.recentGroup.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.recentGroup.Controls.Add(this.recentFlow);
			this.suggestionsFlow.SetFlowBreak(this.recentGroup, true);
			this.recentGroup.Location = new System.Drawing.Point(3, 3);
			this.recentGroup.MinimumSize = new System.Drawing.Size(400, 80);
			this.recentGroup.Name = "recentGroup";
			this.recentGroup.Padding = new System.Windows.Forms.Padding(5);
			this.recentGroup.Size = new System.Drawing.Size(400, 80);
			this.recentGroup.TabIndex = 6;
			this.recentGroup.TabStop = false;
			this.recentGroup.Text = "Recently used";
			// 
			// recentFlow
			// 
			this.recentFlow.Dock = System.Windows.Forms.DockStyle.Fill;
			this.recentFlow.Location = new System.Drawing.Point(5, 24);
			this.recentFlow.Margin = new System.Windows.Forms.Padding(5);
			this.recentFlow.Name = "recentFlow";
			this.recentFlow.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
			this.recentFlow.Size = new System.Drawing.Size(390, 51);
			this.recentFlow.TabIndex = 5;
			// 
			// commonGroup
			// 
			this.commonGroup.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.commonGroup.Controls.Add(this.commonFlow);
			this.suggestionsFlow.SetFlowBreak(this.commonGroup, true);
			this.commonGroup.Location = new System.Drawing.Point(3, 89);
			this.commonGroup.MinimumSize = new System.Drawing.Size(400, 80);
			this.commonGroup.Name = "commonGroup";
			this.commonGroup.Padding = new System.Windows.Forms.Padding(5);
			this.commonGroup.Size = new System.Drawing.Size(400, 80);
			this.commonGroup.TabIndex = 7;
			this.commonGroup.TabStop = false;
			this.commonGroup.Text = "Common words on this page";
			// 
			// commonFlow
			// 
			this.commonFlow.Dock = System.Windows.Forms.DockStyle.Fill;
			this.commonFlow.Location = new System.Drawing.Point(5, 24);
			this.commonFlow.Margin = new System.Windows.Forms.Padding(5);
			this.commonFlow.Name = "commonFlow";
			this.commonFlow.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
			this.commonFlow.Size = new System.Drawing.Size(390, 51);
			this.commonFlow.TabIndex = 5;
			// 
			// suggestionsLabel
			// 
			this.suggestionsLabel.AutoSize = true;
			this.suggestionsLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.suggestionsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.suggestionsLabel.ForeColor = System.Drawing.SystemColors.GrayText;
			this.suggestionsLabel.Location = new System.Drawing.Point(10, 10);
			this.suggestionsLabel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 4);
			this.suggestionsLabel.Name = "suggestionsLabel";
			this.suggestionsLabel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 6);
			this.suggestionsLabel.Size = new System.Drawing.Size(120, 28);
			this.suggestionsLabel.TabIndex = 1;
			this.suggestionsLabel.Text = "Suggestions";
			// 
			// TaggingDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(941, 692);
			this.Controls.Add(this.splitter);
			this.Controls.Add(this.controlPanel);
			this.Controls.Add(this.buttonPanel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(700, 700);
			this.Name = "TaggingDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Page Tags";
			this.Load += new System.EventHandler(this.DialogLoad);
			this.buttonPanel.ResumeLayout(false);
			this.controlPanel.ResumeLayout(false);
			this.controlPanel.PerformLayout();
			this.splitter.Panel1.ResumeLayout(false);
			this.splitter.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitter)).EndInit();
			this.splitter.ResumeLayout(false);
			this.clearPanel.ResumeLayout(false);
			this.clearPanel.PerformLayout();
			this.suggestionPanel.ResumeLayout(false);
			this.suggestionPanel.PerformLayout();
			this.suggestionsFlow.ResumeLayout(false);
			this.recentGroup.ResumeLayout(false);
			this.commonGroup.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.Panel buttonPanel;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Panel controlPanel;
		private System.Windows.Forms.Label tagLabel;
		private System.Windows.Forms.Label introLabel;
		private System.Windows.Forms.Button addButton;
		private UI.FormTextBox tagBox;
		private System.Windows.Forms.SplitContainer splitter;
		private System.Windows.Forms.FlowLayoutPanel tagsFlow;
		private System.Windows.Forms.Panel clearPanel;
		private UI.MoreLinkLabel clearLabel;
		private System.Windows.Forms.Panel suggestionPanel;
		private System.Windows.Forms.FlowLayoutPanel suggestionsFlow;
		private System.Windows.Forms.GroupBox recentGroup;
		private System.Windows.Forms.FlowLayoutPanel recentFlow;
		private System.Windows.Forms.GroupBox commonGroup;
		private System.Windows.Forms.FlowLayoutPanel commonFlow;
		private System.Windows.Forms.Label suggestionsLabel;
	}
}