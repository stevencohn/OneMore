namespace River.OneMoreAddIn.Commands
{
	partial class EmojiDialog
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose (bool disposing)
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
		private void InitializeComponent ()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EmojiDialog));
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.tabs = new River.OneMoreAddIn.UI.MoreTabControl();
			this.listTab = new System.Windows.Forms.TabPage();
			this.emojiBox = new River.OneMoreAddIn.UI.MoreListView();
			this.nameColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.gridTab = new System.Windows.Forms.TabPage();
			this.categoryPanel = new River.OneMoreAddIn.UI.MorePanel();
			this.generalBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.smileysBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.peopleBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.animalsBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.foodBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.travelBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.activitiesBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.objectsBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.symbolsBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.flagsBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.gridBox = new River.OneMoreAddIn.UI.MoreIconListView();
			this.introLabel = new System.Windows.Forms.Label();
			this.tabs.SuspendLayout();
			this.listTab.SuspendLayout();
			this.gridTab.SuspendLayout();
			this.categoryPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(285, 702);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(99, 38);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 2;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.Cancel);
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(180, 702);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(99, 38);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 1;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OK);
			// 
			// tabs
			// 
			this.tabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabs.Controls.Add(this.listTab);
			this.tabs.Controls.Add(this.gridTab);
			this.tabs.Location = new System.Drawing.Point(12, 62);
			this.tabs.Margin = new System.Windows.Forms.Padding(0);
			this.tabs.Name = "tabs";
			this.tabs.Padding = new System.Drawing.Point(0, 0);
			this.tabs.SelectedIndex = 0;
			this.tabs.Size = new System.Drawing.Size(372, 628);
			this.tabs.TabIndex = 0;
			this.tabs.SelectedIndexChanged += new System.EventHandler(this.DoTabSelected);
			// 
			// listTab
			// 
			this.listTab.Controls.Add(this.emojiBox);
			this.listTab.Location = new System.Drawing.Point(4, 29);
			this.listTab.Name = "listTab";
			this.listTab.Size = new System.Drawing.Size(364, 595);
			this.listTab.TabIndex = 0;
			this.listTab.Text = "List";
			// 
			// emojiBox
			// 
			this.emojiBox.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameColumn});
			this.emojiBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.emojiBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.emojiBox.FullRowSelect = true;
			this.emojiBox.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.emojiBox.HideSelection = false;
			this.emojiBox.Location = new System.Drawing.Point(0, 0);
			this.emojiBox.Name = "emojiBox";
			this.emojiBox.OwnerDraw = true;
			this.emojiBox.Size = new System.Drawing.Size(364, 595);
			this.emojiBox.TabIndex = 0;
			this.emojiBox.UseCompatibleStateImageBehavior = false;
			this.emojiBox.View = System.Windows.Forms.View.Details;
			this.emojiBox.SelectedIndexChanged += new System.EventHandler(this.DoListSelectionChanged);
			this.emojiBox.DoubleClick += new System.EventHandler(this.DoubleClickItem);
			// 
			// nameColumn
			// 
			this.nameColumn.Name = "nameColumn";
			// 
			// gridTab
			// 
			this.gridTab.Controls.Add(this.gridBox);
			this.gridTab.Controls.Add(this.categoryPanel);
			this.gridTab.Location = new System.Drawing.Point(4, 29);
			this.gridTab.Name = "gridTab";
			this.gridTab.Size = new System.Drawing.Size(364, 595);
			this.gridTab.TabIndex = 1;
			this.gridTab.Text = "Grid";
			// 
			// categoryPanel
			// 
			this.categoryPanel.BottomBorderColor = System.Drawing.SystemColors.ActiveBorder;
			this.categoryPanel.BottomBorderSize = 0;
			this.categoryPanel.Controls.Add(this.generalBox);
			this.categoryPanel.Controls.Add(this.smileysBox);
			this.categoryPanel.Controls.Add(this.peopleBox);
			this.categoryPanel.Controls.Add(this.animalsBox);
			this.categoryPanel.Controls.Add(this.foodBox);
			this.categoryPanel.Controls.Add(this.travelBox);
			this.categoryPanel.Controls.Add(this.activitiesBox);
			this.categoryPanel.Controls.Add(this.objectsBox);
			this.categoryPanel.Controls.Add(this.symbolsBox);
			this.categoryPanel.Controls.Add(this.flagsBox);
			this.categoryPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.categoryPanel.Location = new System.Drawing.Point(0, 0);
			this.categoryPanel.Name = "categoryPanel";
			this.categoryPanel.Size = new System.Drawing.Size(200, 595);
			this.categoryPanel.TabIndex = 0;
			this.categoryPanel.ThemedBack = null;
			this.categoryPanel.ThemedFore = null;
			this.categoryPanel.TopBorderColor = System.Drawing.SystemColors.Control;
			this.categoryPanel.TopBorderSize = 0;
			// 
			// generalBox
			// 
			this.generalBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.generalBox.Checked = true;
			this.generalBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.generalBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.generalBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.generalBox.Location = new System.Drawing.Point(8, 10);
			this.generalBox.Name = "generalBox";
			this.generalBox.Size = new System.Drawing.Size(155, 25);
			this.generalBox.StylizeImage = false;
			this.generalBox.TabIndex = 0;
			this.generalBox.Text = "General";
			this.generalBox.ThemedBack = null;
			this.generalBox.ThemedFore = null;
			this.generalBox.UseVisualStyleBackColor = true;
			this.generalBox.CheckedChanged += new System.EventHandler(this.DoCategoryFilterChanged);
			// 
			// smileysBox
			// 
			this.smileysBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.smileysBox.Checked = true;
			this.smileysBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.smileysBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.smileysBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.smileysBox.Location = new System.Drawing.Point(8, 40);
			this.smileysBox.Name = "smileysBox";
			this.smileysBox.Size = new System.Drawing.Size(185, 25);
			this.smileysBox.StylizeImage = false;
			this.smileysBox.TabIndex = 1;
			this.smileysBox.Text = "Smileys && Emotion";
			this.smileysBox.ThemedBack = null;
			this.smileysBox.ThemedFore = null;
			this.smileysBox.UseVisualStyleBackColor = true;
			this.smileysBox.CheckedChanged += new System.EventHandler(this.DoCategoryFilterChanged);
			// 
			// peopleBox
			// 
			this.peopleBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.peopleBox.Checked = true;
			this.peopleBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.peopleBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.peopleBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.peopleBox.Location = new System.Drawing.Point(8, 70);
			this.peopleBox.Name = "peopleBox";
			this.peopleBox.Size = new System.Drawing.Size(155, 25);
			this.peopleBox.StylizeImage = false;
			this.peopleBox.TabIndex = 2;
			this.peopleBox.Text = "People && Body";
			this.peopleBox.ThemedBack = null;
			this.peopleBox.ThemedFore = null;
			this.peopleBox.UseVisualStyleBackColor = true;
			this.peopleBox.CheckedChanged += new System.EventHandler(this.DoCategoryFilterChanged);
			// 
			// animalsBox
			// 
			this.animalsBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.animalsBox.Checked = true;
			this.animalsBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.animalsBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.animalsBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.animalsBox.Location = new System.Drawing.Point(8, 100);
			this.animalsBox.Name = "animalsBox";
			this.animalsBox.Size = new System.Drawing.Size(174, 25);
			this.animalsBox.StylizeImage = false;
			this.animalsBox.TabIndex = 3;
			this.animalsBox.Text = "Animals && Nature";
			this.animalsBox.ThemedBack = null;
			this.animalsBox.ThemedFore = null;
			this.animalsBox.UseVisualStyleBackColor = true;
			this.animalsBox.CheckedChanged += new System.EventHandler(this.DoCategoryFilterChanged);
			// 
			// foodBox
			// 
			this.foodBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.foodBox.Checked = true;
			this.foodBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.foodBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.foodBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.foodBox.Location = new System.Drawing.Point(8, 130);
			this.foodBox.Name = "foodBox";
			this.foodBox.Size = new System.Drawing.Size(155, 25);
			this.foodBox.StylizeImage = false;
			this.foodBox.TabIndex = 4;
			this.foodBox.Text = "Food && Drink";
			this.foodBox.ThemedBack = null;
			this.foodBox.ThemedFore = null;
			this.foodBox.UseVisualStyleBackColor = true;
			this.foodBox.CheckedChanged += new System.EventHandler(this.DoCategoryFilterChanged);
			// 
			// travelBox
			// 
			this.travelBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.travelBox.Checked = true;
			this.travelBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.travelBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.travelBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.travelBox.Location = new System.Drawing.Point(8, 160);
			this.travelBox.Name = "travelBox";
			this.travelBox.Size = new System.Drawing.Size(162, 25);
			this.travelBox.StylizeImage = false;
			this.travelBox.TabIndex = 5;
			this.travelBox.Text = "Travel && Places";
			this.travelBox.ThemedBack = null;
			this.travelBox.ThemedFore = null;
			this.travelBox.UseVisualStyleBackColor = true;
			this.travelBox.CheckedChanged += new System.EventHandler(this.DoCategoryFilterChanged);
			// 
			// activitiesBox
			// 
			this.activitiesBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.activitiesBox.Checked = true;
			this.activitiesBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.activitiesBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.activitiesBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.activitiesBox.Location = new System.Drawing.Point(8, 190);
			this.activitiesBox.Name = "activitiesBox";
			this.activitiesBox.Size = new System.Drawing.Size(155, 25);
			this.activitiesBox.StylizeImage = false;
			this.activitiesBox.TabIndex = 6;
			this.activitiesBox.Text = "Activities";
			this.activitiesBox.ThemedBack = null;
			this.activitiesBox.ThemedFore = null;
			this.activitiesBox.UseVisualStyleBackColor = true;
			this.activitiesBox.CheckedChanged += new System.EventHandler(this.DoCategoryFilterChanged);
			// 
			// objectsBox
			// 
			this.objectsBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.objectsBox.Checked = true;
			this.objectsBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.objectsBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.objectsBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.objectsBox.Location = new System.Drawing.Point(8, 220);
			this.objectsBox.Name = "objectsBox";
			this.objectsBox.Size = new System.Drawing.Size(155, 25);
			this.objectsBox.StylizeImage = false;
			this.objectsBox.TabIndex = 7;
			this.objectsBox.Text = "Objects";
			this.objectsBox.ThemedBack = null;
			this.objectsBox.ThemedFore = null;
			this.objectsBox.UseVisualStyleBackColor = true;
			this.objectsBox.CheckedChanged += new System.EventHandler(this.DoCategoryFilterChanged);
			// 
			// symbolsBox
			// 
			this.symbolsBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.symbolsBox.Checked = true;
			this.symbolsBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.symbolsBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.symbolsBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.symbolsBox.Location = new System.Drawing.Point(8, 250);
			this.symbolsBox.Name = "symbolsBox";
			this.symbolsBox.Size = new System.Drawing.Size(155, 25);
			this.symbolsBox.StylizeImage = false;
			this.symbolsBox.TabIndex = 8;
			this.symbolsBox.Text = "Symbols";
			this.symbolsBox.ThemedBack = null;
			this.symbolsBox.ThemedFore = null;
			this.symbolsBox.UseVisualStyleBackColor = true;
			this.symbolsBox.CheckedChanged += new System.EventHandler(this.DoCategoryFilterChanged);
			// 
			// flagsBox
			// 
			this.flagsBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.flagsBox.Checked = true;
			this.flagsBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.flagsBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.flagsBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.flagsBox.Location = new System.Drawing.Point(8, 280);
			this.flagsBox.Name = "flagsBox";
			this.flagsBox.Size = new System.Drawing.Size(155, 25);
			this.flagsBox.StylizeImage = false;
			this.flagsBox.TabIndex = 9;
			this.flagsBox.Text = "Flags";
			this.flagsBox.ThemedBack = null;
			this.flagsBox.ThemedFore = null;
			this.flagsBox.UseVisualStyleBackColor = true;
			this.flagsBox.CheckedChanged += new System.EventHandler(this.DoCategoryFilterChanged);
			// 
			// gridBox
			// 
			this.gridBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridBox.HideSelection = false;
			this.gridBox.Location = new System.Drawing.Point(0, 0);
			this.gridBox.Name = "gridBox";
			this.gridBox.OwnerDraw = true;
			this.gridBox.Size = new System.Drawing.Size(364, 595);
			this.gridBox.TabIndex = 1;
			this.gridBox.UseCompatibleStateImageBehavior = false;
			this.gridBox.SelectedIndexChanged += new System.EventHandler(this.DoGridSelectionChanged);
			this.gridBox.DoubleClick += new System.EventHandler(this.DoubleClickItem);
			// 
			// introLabel
			// 
			this.introLabel.AutoSize = true;
			this.introLabel.Location = new System.Drawing.Point(14, 26);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(253, 20);
			this.introLabel.TabIndex = 4;
			this.introLabel.Text = "Select one or more emojis to insert";
			// 
			// EmojiDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(396, 752);
			this.Controls.Add(this.introLabel);
			this.Controls.Add(this.tabs);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(400, 600);
			this.Name = "EmojiDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Insert Emojis";
			this.tabs.ResumeLayout(false);
			this.listTab.ResumeLayout(false);
			this.gridTab.ResumeLayout(false);
			this.categoryPanel.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreButton cancelButton;
		private UI.MoreButton okButton;
		private UI.MoreTabControl tabs;
		private System.Windows.Forms.TabPage listTab;
		private UI.MoreListView emojiBox;
		private System.Windows.Forms.ColumnHeader nameColumn;
		private System.Windows.Forms.TabPage gridTab;
		private UI.MoreIconListView gridBox;
		private UI.MorePanel categoryPanel;
		private UI.MoreCheckBox generalBox;
		private UI.MoreCheckBox smileysBox;
		private UI.MoreCheckBox peopleBox;
		private UI.MoreCheckBox animalsBox;
		private UI.MoreCheckBox foodBox;
		private UI.MoreCheckBox travelBox;
		private UI.MoreCheckBox activitiesBox;
		private UI.MoreCheckBox objectsBox;
		private UI.MoreCheckBox symbolsBox;
		private UI.MoreCheckBox flagsBox;
		private System.Windows.Forms.Label introLabel;
	}
}
