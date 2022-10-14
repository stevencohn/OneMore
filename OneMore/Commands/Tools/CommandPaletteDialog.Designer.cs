namespace River.OneMoreAddIn.Commands
{
	partial class CommandPaletteDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CommandPaletteDialog));
			this.cmdBox = new System.Windows.Forms.TextBox();
			this.bodyPanel = new System.Windows.Forms.Panel();
			this.clearLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.introPanel = new System.Windows.Forms.Panel();
			this.introLabel = new System.Windows.Forms.Label();
			this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
			this.bodyPanel.SuspendLayout();
			this.introPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
			this.SuspendLayout();
			// 
			// cmdBox
			// 
			this.cmdBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cmdBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cmdBox.Location = new System.Drawing.Point(27, 46);
			this.cmdBox.Name = "cmdBox";
			this.cmdBox.Size = new System.Drawing.Size(613, 28);
			this.cmdBox.TabIndex = 0;
			this.cmdBox.TextChanged += new System.EventHandler(this.ValidateCommand);
			this.cmdBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoKeyDown);
			this.cmdBox.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.DoPreviewKeyDown);
			// 
			// bodyPanel
			// 
			this.bodyPanel.Controls.Add(this.clearLink);
			this.bodyPanel.Controls.Add(this.cmdBox);
			this.bodyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.bodyPanel.Location = new System.Drawing.Point(0, 64);
			this.bodyPanel.Margin = new System.Windows.Forms.Padding(0);
			this.bodyPanel.Name = "bodyPanel";
			this.bodyPanel.Padding = new System.Windows.Forms.Padding(20, 20, 50, 20);
			this.bodyPanel.Size = new System.Drawing.Size(693, 121);
			this.bodyPanel.TabIndex = 3;
			// 
			// clearLink
			// 
			this.clearLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.clearLink.AutoSize = true;
			this.clearLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.clearLink.HoverColor = System.Drawing.Color.MediumOrchid;
			this.clearLink.Location = new System.Drawing.Point(463, 20);
			this.clearLink.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.clearLink.Name = "clearLink";
			this.clearLink.Size = new System.Drawing.Size(177, 20);
			this.clearLink.TabIndex = 2;
			this.clearLink.TabStop = true;
			this.clearLink.Text = "Clear recent commands";
			this.clearLink.TextAlign = System.Drawing.ContentAlignment.TopRight;
			this.clearLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ClearRecentCommands);
			// 
			// introPanel
			// 
			this.introPanel.BackColor = System.Drawing.SystemColors.Window;
			this.introPanel.Controls.Add(this.introLabel);
			this.introPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.introPanel.Location = new System.Drawing.Point(0, 0);
			this.introPanel.Name = "introPanel";
			this.introPanel.Padding = new System.Windows.Forms.Padding(20);
			this.introPanel.Size = new System.Drawing.Size(693, 64);
			this.introPanel.TabIndex = 4;
			// 
			// introLabel
			// 
			this.introLabel.AutoSize = true;
			this.introLabel.Location = new System.Drawing.Point(23, 20);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(441, 20);
			this.introLabel.TabIndex = 0;
			this.introLabel.Text = "Type a command and press Enter or click an item from the list";
			// 
			// errorProvider
			// 
			this.errorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
			this.errorProvider.ContainerControl = this;
			// 
			// CommandPaletteDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(693, 185);
			this.Controls.Add(this.bodyPanel);
			this.Controls.Add(this.introPanel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CommandPaletteDialog";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "OneMore Command Palette";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoKeyDown);
			this.bodyPanel.ResumeLayout(false);
			this.bodyPanel.PerformLayout();
			this.introPanel.ResumeLayout(false);
			this.introPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.TextBox cmdBox;
		private System.Windows.Forms.Panel bodyPanel;
		private UI.MoreLinkLabel clearLink;
		private System.Windows.Forms.Panel introPanel;
		private System.Windows.Forms.Label introLabel;
		private System.Windows.Forms.ErrorProvider errorProvider;
	}
}