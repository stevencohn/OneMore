namespace River.OneMoreAddIn.Commands.Workspaces
{
	partial class ManageWorkspaceDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ManageWorkspaceDialog));
			this.tabs = new River.OneMoreAddIn.UI.MoreTabControl();
			this.favoritesTab = new System.Windows.Forms.TabPage();
			this.managerControl = new River.OneMoreAddIn.Commands.Favorites.MangeFavoritesControl();
			this.layoutsTab = new System.Windows.Forms.TabPage();
			this.layoutsControl = new River.OneMoreAddIn.Commands.Layouts.ManageLayoutsControl();
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.noteLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.tabs.SuspendLayout();
			this.favoritesTab.SuspendLayout();
			this.layoutsTab.SuspendLayout();
			this.buttonPanel.SuspendLayout();
			this.SuspendLayout();
			//
			// tabs
			//
			this.tabs.Controls.Add(this.favoritesTab);
			this.tabs.Controls.Add(this.layoutsTab);
			this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabs.Location = new System.Drawing.Point(0, 0);
			this.tabs.Margin = new System.Windows.Forms.Padding(0);
			this.tabs.Name = "tabs";
			this.tabs.Padding = new System.Drawing.Point(0, 0);
			this.tabs.SelectedIndex = 0;
			this.tabs.Size = new System.Drawing.Size(928, 684);
			this.tabs.TabIndex = 0;
			//
			// favoritesTab
			//
			this.favoritesTab.Controls.Add(this.managerControl);
			this.favoritesTab.Location = new System.Drawing.Point(4, 29);
			this.favoritesTab.Name = "favoritesTab";
			this.favoritesTab.Size = new System.Drawing.Size(920, 651);
			this.favoritesTab.TabIndex = 0;
			this.favoritesTab.Text = "Favorites";
			//
			// managerControl
			//
			this.managerControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.managerControl.Location = new System.Drawing.Point(0, 0);
			this.managerControl.Name = "managerControl";
			this.managerControl.Size = new System.Drawing.Size(920, 651);
			this.managerControl.TabIndex = 0;
			//
			// layoutsTab
			//
			this.layoutsTab.Controls.Add(this.layoutsControl);
			this.layoutsTab.Location = new System.Drawing.Point(4, 29);
			this.layoutsTab.Name = "layoutsTab";
			this.layoutsTab.Size = new System.Drawing.Size(920, 651);
			this.layoutsTab.TabIndex = 1;
			this.layoutsTab.Text = "Layouts";
			//
			// layoutsControl
			//
			this.layoutsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.layoutsControl.Location = new System.Drawing.Point(0, 0);
			this.layoutsControl.Name = "layoutsControl";
			this.layoutsControl.Size = new System.Drawing.Size(920, 651);
			this.layoutsControl.TabIndex = 0;
			//
			// buttonPanel
			//
			this.buttonPanel.BackColor = System.Drawing.SystemColors.ControlLight;
			this.buttonPanel.Controls.Add(this.noteLabel);
			this.buttonPanel.Controls.Add(this.okButton);
			this.buttonPanel.Controls.Add(this.cancelButton);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.buttonPanel.Location = new System.Drawing.Point(0, 684);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Padding = new System.Windows.Forms.Padding(8);
			this.buttonPanel.Size = new System.Drawing.Size(928, 60);
			this.buttonPanel.TabIndex = 1;
			//
			// noteLabel
			//
			this.noteLabel.AutoSize = true;
			this.noteLabel.Location = new System.Drawing.Point(33, 20);
			this.noteLabel.Name = "noteLabel";
			this.noteLabel.Size = new System.Drawing.Size(132, 20);
			this.noteLabel.TabIndex = 2;
			this.noteLabel.Text = "Check completed";
			this.noteLabel.ThemedBack = null;
			this.noteLabel.ThemedFore = null;
			this.noteLabel.Visible = false;
			//
			// okButton
			//
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(668, 11);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(120, 38);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OK);
			//
			// cancelButton
			//
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(794, 11);
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
			// ManageWorkspaceDialog
			//
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(928, 744);
			this.Controls.Add(this.tabs);
			this.Controls.Add(this.buttonPanel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(600, 400);
			this.Name = "ManageWorkspaceDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Workspace";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConfirmClosing);
			this.Load += new System.EventHandler(this.BindOnLoad);
			this.tabs.ResumeLayout(false);
			this.favoritesTab.ResumeLayout(false);
			this.layoutsTab.ResumeLayout(false);
			this.buttonPanel.ResumeLayout(false);
			this.buttonPanel.PerformLayout();
			this.ResumeLayout(false);

		}
		#endregion

		private River.OneMoreAddIn.UI.MoreTabControl tabs;
		private System.Windows.Forms.TabPage favoritesTab;
		private River.OneMoreAddIn.Commands.Favorites.MangeFavoritesControl managerControl;
		private System.Windows.Forms.TabPage layoutsTab;
		private River.OneMoreAddIn.Commands.Layouts.ManageLayoutsControl layoutsControl;
		private System.Windows.Forms.Panel buttonPanel;
		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private UI.MoreLabel noteLabel;
	}
}
