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
			this.generalLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.smileysLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.peopleLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.animalsLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.foodLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.travelLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.activitiesLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.objectsLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.symbolsLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.flagsLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
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
			this.listTab.Text = "Quick Pick";
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
			this.gridTab.Text = "Palette";
			// 
			// categoryPanel
			// 
			this.categoryPanel.BottomBorderColor = System.Drawing.SystemColors.ActiveBorder;
			this.categoryPanel.BottomBorderSize = 0;
			this.categoryPanel.Controls.Add(this.generalLink);
			this.categoryPanel.Controls.Add(this.smileysLink);
			this.categoryPanel.Controls.Add(this.peopleLink);
			this.categoryPanel.Controls.Add(this.animalsLink);
			this.categoryPanel.Controls.Add(this.foodLink);
			this.categoryPanel.Controls.Add(this.travelLink);
			this.categoryPanel.Controls.Add(this.activitiesLink);
			this.categoryPanel.Controls.Add(this.objectsLink);
			this.categoryPanel.Controls.Add(this.symbolsLink);
			this.categoryPanel.Controls.Add(this.flagsLink);
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
			// generalLink
			//
			this.generalLink.Active = true;
			this.generalLink.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.generalLink.AutoSize = false;
			this.generalLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.generalLink.Location = new System.Drawing.Point(8, 10);
			this.generalLink.Name = "generalLink";
			this.generalLink.NavMode = true;
			this.generalLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.generalLink.Size = new System.Drawing.Size(184, 20);
			this.generalLink.TabIndex = 0;
			this.generalLink.TabStop = true;
			this.generalLink.Tag = "General";
			this.generalLink.Text = "General";
			this.generalLink.ThemedBack = null;
			this.generalLink.ThemedFore = null;
			this.generalLink.Click += new System.EventHandler(this.DoCategoryLinkClicked);
			//
			// smileysLink
			//
			this.smileysLink.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.smileysLink.AutoSize = false;
			this.smileysLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.smileysLink.Location = new System.Drawing.Point(8, 40);
			this.smileysLink.Name = "smileysLink";
			this.smileysLink.NavMode = true;
			this.smileysLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.smileysLink.Size = new System.Drawing.Size(184, 20);
			this.smileysLink.TabIndex = 1;
			this.smileysLink.TabStop = true;
			this.smileysLink.Tag = "Smileys & Emotion";
			this.smileysLink.Text = "Smileys && Emotion";
			this.smileysLink.ThemedBack = null;
			this.smileysLink.ThemedFore = null;
			this.smileysLink.Click += new System.EventHandler(this.DoCategoryLinkClicked);
			//
			// peopleLink
			//
			this.peopleLink.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.peopleLink.AutoSize = false;
			this.peopleLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.peopleLink.Location = new System.Drawing.Point(8, 70);
			this.peopleLink.Name = "peopleLink";
			this.peopleLink.NavMode = true;
			this.peopleLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.peopleLink.Size = new System.Drawing.Size(184, 20);
			this.peopleLink.TabIndex = 2;
			this.peopleLink.TabStop = true;
			this.peopleLink.Tag = "People & Body";
			this.peopleLink.Text = "People && Body";
			this.peopleLink.ThemedBack = null;
			this.peopleLink.ThemedFore = null;
			this.peopleLink.Click += new System.EventHandler(this.DoCategoryLinkClicked);
			//
			// animalsLink
			//
			this.animalsLink.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.animalsLink.AutoSize = false;
			this.animalsLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.animalsLink.Location = new System.Drawing.Point(8, 100);
			this.animalsLink.Name = "animalsLink";
			this.animalsLink.NavMode = true;
			this.animalsLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.animalsLink.Size = new System.Drawing.Size(184, 20);
			this.animalsLink.TabIndex = 3;
			this.animalsLink.TabStop = true;
			this.animalsLink.Tag = "Animals & Nature";
			this.animalsLink.Text = "Animals && Nature";
			this.animalsLink.ThemedBack = null;
			this.animalsLink.ThemedFore = null;
			this.animalsLink.Click += new System.EventHandler(this.DoCategoryLinkClicked);
			//
			// foodLink
			//
			this.foodLink.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.foodLink.AutoSize = false;
			this.foodLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.foodLink.Location = new System.Drawing.Point(8, 130);
			this.foodLink.Name = "foodLink";
			this.foodLink.NavMode = true;
			this.foodLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.foodLink.Size = new System.Drawing.Size(184, 20);
			this.foodLink.TabIndex = 4;
			this.foodLink.TabStop = true;
			this.foodLink.Tag = "Food & Drink";
			this.foodLink.Text = "Food && Drink";
			this.foodLink.ThemedBack = null;
			this.foodLink.ThemedFore = null;
			this.foodLink.Click += new System.EventHandler(this.DoCategoryLinkClicked);
			//
			// travelLink
			//
			this.travelLink.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.travelLink.AutoSize = false;
			this.travelLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.travelLink.Location = new System.Drawing.Point(8, 160);
			this.travelLink.Name = "travelLink";
			this.travelLink.NavMode = true;
			this.travelLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.travelLink.Size = new System.Drawing.Size(184, 20);
			this.travelLink.TabIndex = 5;
			this.travelLink.TabStop = true;
			this.travelLink.Tag = "Travel & Places";
			this.travelLink.Text = "Travel && Places";
			this.travelLink.ThemedBack = null;
			this.travelLink.ThemedFore = null;
			this.travelLink.Click += new System.EventHandler(this.DoCategoryLinkClicked);
			//
			// activitiesLink
			//
			this.activitiesLink.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.activitiesLink.AutoSize = false;
			this.activitiesLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.activitiesLink.Location = new System.Drawing.Point(8, 190);
			this.activitiesLink.Name = "activitiesLink";
			this.activitiesLink.NavMode = true;
			this.activitiesLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.activitiesLink.Size = new System.Drawing.Size(184, 20);
			this.activitiesLink.TabIndex = 6;
			this.activitiesLink.TabStop = true;
			this.activitiesLink.Tag = "Activities";
			this.activitiesLink.Text = "Activities";
			this.activitiesLink.ThemedBack = null;
			this.activitiesLink.ThemedFore = null;
			this.activitiesLink.Click += new System.EventHandler(this.DoCategoryLinkClicked);
			//
			// objectsLink
			//
			this.objectsLink.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.objectsLink.AutoSize = false;
			this.objectsLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.objectsLink.Location = new System.Drawing.Point(8, 220);
			this.objectsLink.Name = "objectsLink";
			this.objectsLink.NavMode = true;
			this.objectsLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.objectsLink.Size = new System.Drawing.Size(184, 20);
			this.objectsLink.TabIndex = 7;
			this.objectsLink.TabStop = true;
			this.objectsLink.Tag = "Objects";
			this.objectsLink.Text = "Objects";
			this.objectsLink.ThemedBack = null;
			this.objectsLink.ThemedFore = null;
			this.objectsLink.Click += new System.EventHandler(this.DoCategoryLinkClicked);
			//
			// symbolsLink
			//
			this.symbolsLink.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.symbolsLink.AutoSize = false;
			this.symbolsLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.symbolsLink.Location = new System.Drawing.Point(8, 250);
			this.symbolsLink.Name = "symbolsLink";
			this.symbolsLink.NavMode = true;
			this.symbolsLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.symbolsLink.Size = new System.Drawing.Size(184, 20);
			this.symbolsLink.TabIndex = 8;
			this.symbolsLink.TabStop = true;
			this.symbolsLink.Tag = "Symbols";
			this.symbolsLink.Text = "Symbols";
			this.symbolsLink.ThemedBack = null;
			this.symbolsLink.ThemedFore = null;
			this.symbolsLink.Click += new System.EventHandler(this.DoCategoryLinkClicked);
			//
			// flagsLink
			//
			this.flagsLink.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.flagsLink.AutoSize = false;
			this.flagsLink.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.flagsLink.Location = new System.Drawing.Point(8, 280);
			this.flagsLink.Name = "flagsLink";
			this.flagsLink.NavMode = true;
			this.flagsLink.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.flagsLink.Size = new System.Drawing.Size(184, 20);
			this.flagsLink.TabIndex = 9;
			this.flagsLink.TabStop = true;
			this.flagsLink.Tag = "Flags";
			this.flagsLink.Text = "Flags";
			this.flagsLink.ThemedBack = null;
			this.flagsLink.ThemedFore = null;
			this.flagsLink.Click += new System.EventHandler(this.DoCategoryLinkClicked);
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
			this.Shown += EmojiDialog_Shown;
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
		private UI.MoreLinkLabel generalLink;
		private UI.MoreLinkLabel smileysLink;
		private UI.MoreLinkLabel peopleLink;
		private UI.MoreLinkLabel animalsLink;
		private UI.MoreLinkLabel foodLink;
		private UI.MoreLinkLabel travelLink;
		private UI.MoreLinkLabel activitiesLink;
		private UI.MoreLinkLabel objectsLink;
		private UI.MoreLinkLabel symbolsLink;
		private UI.MoreLinkLabel flagsLink;
		private System.Windows.Forms.Label introLabel;
	}
}
