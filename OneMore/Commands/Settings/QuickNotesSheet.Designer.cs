
namespace River.OneMoreAddIn.Settings
{
	partial class QuickNotesSheet
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
			this.components = new System.ComponentModel.Container();
			this.introBox = new System.Windows.Forms.TextBox();
			this.layoutPanel = new System.Windows.Forms.Panel();
			this.optionsGroup = new System.Windows.Forms.GroupBox();
			this.titleBox = new System.Windows.Forms.CheckBox();
			this.stampBox = new System.Windows.Forms.CheckBox();
			this.sectionGroup = new System.Windows.Forms.GroupBox();
			this.sectionButton = new System.Windows.Forms.RadioButton();
			this.sectionLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.notebookGroup = new System.Windows.Forms.GroupBox();
			this.groupingBox = new System.Windows.Forms.ComboBox();
			this.groupingLabel = new System.Windows.Forms.Label();
			this.notebookButton = new System.Windows.Forms.RadioButton();
			this.notebookLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.tooltip = new System.Windows.Forms.ToolTip(this.components);
			this.layoutPanel.SuspendLayout();
			this.optionsGroup.SuspendLayout();
			this.sectionGroup.SuspendLayout();
			this.notebookGroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// introBox
			// 
			this.introBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.introBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.introBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.introBox.Location = new System.Drawing.Point(13, 8);
			this.introBox.Multiline = true;
			this.introBox.Name = "introBox";
			this.introBox.ReadOnly = true;
			this.introBox.Size = new System.Drawing.Size(772, 60);
			this.introBox.TabIndex = 3;
			this.introBox.Text = "Customize the behavior of the Organize Quick Notes command";
			// 
			// layoutPanel
			// 
			this.layoutPanel.Controls.Add(this.optionsGroup);
			this.layoutPanel.Controls.Add(this.sectionGroup);
			this.layoutPanel.Controls.Add(this.notebookGroup);
			this.layoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.layoutPanel.Location = new System.Drawing.Point(13, 68);
			this.layoutPanel.Margin = new System.Windows.Forms.Padding(0);
			this.layoutPanel.Name = "layoutPanel";
			this.layoutPanel.Size = new System.Drawing.Size(772, 422);
			this.layoutPanel.TabIndex = 4;
			// 
			// optionsGroup
			// 
			this.optionsGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.optionsGroup.Controls.Add(this.titleBox);
			this.optionsGroup.Controls.Add(this.stampBox);
			this.optionsGroup.Location = new System.Drawing.Point(3, 269);
			this.optionsGroup.Name = "optionsGroup";
			this.optionsGroup.Padding = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.optionsGroup.Size = new System.Drawing.Size(766, 103);
			this.optionsGroup.TabIndex = 10;
			this.optionsGroup.TabStop = false;
			this.optionsGroup.Text = "Options";
			// 
			// titleBox
			// 
			this.titleBox.AutoSize = true;
			this.titleBox.Checked = true;
			this.titleBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.titleBox.Location = new System.Drawing.Point(13, 25);
			this.titleBox.Name = "titleBox";
			this.titleBox.Size = new System.Drawing.Size(390, 24);
			this.titleBox.TabIndex = 3;
			this.titleBox.Text = "Set the page title to the first fiew words of the note";
			this.titleBox.UseVisualStyleBackColor = true;
			// 
			// stampBox
			// 
			this.stampBox.AutoSize = true;
			this.stampBox.Location = new System.Drawing.Point(13, 55);
			this.stampBox.Name = "stampBox";
			this.stampBox.Size = new System.Drawing.Size(431, 24);
			this.stampBox.TabIndex = 2;
			this.stampBox.Text = "Prepend the title with the date that the note was created";
			this.stampBox.UseVisualStyleBackColor = true;
			// 
			// sectionGroup
			// 
			this.sectionGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.sectionGroup.Controls.Add(this.sectionButton);
			this.sectionGroup.Controls.Add(this.sectionLink);
			this.sectionGroup.Location = new System.Drawing.Point(3, 160);
			this.sectionGroup.Name = "sectionGroup";
			this.sectionGroup.Padding = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.sectionGroup.Size = new System.Drawing.Size(766, 103);
			this.sectionGroup.TabIndex = 9;
			this.sectionGroup.TabStop = false;
			this.sectionGroup.Text = "Section";
			// 
			// sectionButton
			// 
			this.sectionButton.AutoSize = true;
			this.sectionButton.Location = new System.Drawing.Point(13, 25);
			this.sectionButton.Name = "sectionButton";
			this.sectionButton.Size = new System.Drawing.Size(276, 24);
			this.sectionButton.TabIndex = 1;
			this.sectionButton.TabStop = true;
			this.sectionButton.Text = "Move Quick Notes into this section";
			this.sectionButton.UseVisualStyleBackColor = true;
			this.sectionButton.CheckedChanged += new System.EventHandler(this.ChangeOrgOption);
			// 
			// sectionLink
			// 
			this.sectionLink.AutoSize = true;
			this.sectionLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.sectionLink.HoverColor = System.Drawing.Color.MediumOrchid;
			this.sectionLink.Location = new System.Drawing.Point(37, 52);
			this.sectionLink.Name = "sectionLink";
			this.sectionLink.Size = new System.Drawing.Size(224, 20);
			this.sectionLink.TabIndex = 5;
			this.sectionLink.TabStop = true;
			this.sectionLink.Text = "Click here to select the section";
			this.sectionLink.Click += new System.EventHandler(this.SelectSection);
			// 
			// notebookGroup
			// 
			this.notebookGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.notebookGroup.Controls.Add(this.groupingBox);
			this.notebookGroup.Controls.Add(this.groupingLabel);
			this.notebookGroup.Controls.Add(this.notebookButton);
			this.notebookGroup.Controls.Add(this.notebookLink);
			this.notebookGroup.Location = new System.Drawing.Point(3, 6);
			this.notebookGroup.Name = "notebookGroup";
			this.notebookGroup.Padding = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.notebookGroup.Size = new System.Drawing.Size(766, 148);
			this.notebookGroup.TabIndex = 8;
			this.notebookGroup.TabStop = false;
			this.notebookGroup.Text = "Notebook";
			// 
			// groupingBox
			// 
			this.groupingBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.groupingBox.FormattingEnabled = true;
			this.groupingBox.Items.AddRange(new object[] {
            "Work Week (2023-02-13 W7)",
            "Month (2023-02)",
            "Quarter (2023 Q1)",
            "Year (2023)",
            "#Keyword in first line of note"});
			this.groupingBox.Location = new System.Drawing.Point(277, 90);
			this.groupingBox.Name = "groupingBox";
			this.groupingBox.Size = new System.Drawing.Size(350, 28);
			this.groupingBox.TabIndex = 8;
			// 
			// groupingLabel
			// 
			this.groupingLabel.AutoSize = true;
			this.groupingLabel.Location = new System.Drawing.Point(37, 93);
			this.groupingLabel.Name = "groupingLabel";
			this.groupingLabel.Size = new System.Drawing.Size(167, 20);
			this.groupingLabel.TabIndex = 7;
			this.groupingLabel.Text = "Group into sections by";
			// 
			// notebookButton
			// 
			this.notebookButton.AutoSize = true;
			this.notebookButton.Checked = true;
			this.notebookButton.Location = new System.Drawing.Point(13, 25);
			this.notebookButton.Name = "notebookButton";
			this.notebookButton.Size = new System.Drawing.Size(292, 24);
			this.notebookButton.TabIndex = 0;
			this.notebookButton.TabStop = true;
			this.notebookButton.Text = "Move Quick Notes into this notebook";
			this.notebookButton.UseVisualStyleBackColor = true;
			this.notebookButton.CheckedChanged += new System.EventHandler(this.ChangeOrgOption);
			// 
			// notebookLink
			// 
			this.notebookLink.AutoSize = true;
			this.notebookLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.notebookLink.HoverColor = System.Drawing.Color.MediumOrchid;
			this.notebookLink.Location = new System.Drawing.Point(37, 52);
			this.notebookLink.Name = "notebookLink";
			this.notebookLink.Size = new System.Drawing.Size(240, 20);
			this.notebookLink.TabIndex = 6;
			this.notebookLink.TabStop = true;
			this.notebookLink.Text = "Click here to select the notebook";
			this.notebookLink.Click += new System.EventHandler(this.SelectNotebook);
			// 
			// QuickNotesSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.layoutPanel);
			this.Controls.Add(this.introBox);
			this.Name = "QuickNotesSheet";
			this.Padding = new System.Windows.Forms.Padding(13, 8, 15, 10);
			this.Size = new System.Drawing.Size(800, 500);
			this.layoutPanel.ResumeLayout(false);
			this.optionsGroup.ResumeLayout(false);
			this.optionsGroup.PerformLayout();
			this.sectionGroup.ResumeLayout(false);
			this.sectionGroup.PerformLayout();
			this.notebookGroup.ResumeLayout(false);
			this.notebookGroup.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox introBox;
		private System.Windows.Forms.Panel layoutPanel;
		private System.Windows.Forms.GroupBox notebookGroup;
		private System.Windows.Forms.ComboBox groupingBox;
		private System.Windows.Forms.Label groupingLabel;
		private System.Windows.Forms.RadioButton notebookButton;
		private River.OneMoreAddIn.UI.MoreLinkLabel notebookLink;
		private River.OneMoreAddIn.UI.MoreLinkLabel sectionLink;
		private System.Windows.Forms.CheckBox titleBox;
		private System.Windows.Forms.CheckBox stampBox;
		private System.Windows.Forms.RadioButton sectionButton;
		private System.Windows.Forms.GroupBox sectionGroup;
		private System.Windows.Forms.GroupBox optionsGroup;
		private System.Windows.Forms.ToolTip tooltip;
	}
}
