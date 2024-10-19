namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;

	partial class BiLinkDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BiLinkDialog));
			this.topPanel = new System.Windows.Forms.Panel();
			this.hideBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.messageBox = new River.OneMoreAddIn.UI.MoreRichLabel();
			this.iconBox = new System.Windows.Forms.PictureBox();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.buttonPanel = new System.Windows.Forms.Panel();
			this.topPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.iconBox)).BeginInit();
			this.buttonPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// topPanel
			// 
			this.topPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.topPanel.Controls.Add(this.hideBox);
			this.topPanel.Controls.Add(this.messageBox);
			this.topPanel.Controls.Add(this.iconBox);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.topPanel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.topPanel.Location = new System.Drawing.Point(0, 0);
			this.topPanel.Margin = new System.Windows.Forms.Padding(0);
			this.topPanel.Name = "topPanel";
			this.topPanel.Padding = new System.Windows.Forms.Padding(20);
			this.topPanel.Size = new System.Drawing.Size(816, 158);
			this.topPanel.TabIndex = 0;
			// 
			// hideBox
			// 
			this.hideBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.hideBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.hideBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.hideBox.Location = new System.Drawing.Point(94, 113);
			this.hideBox.Name = "hideBox";
			this.hideBox.Size = new System.Drawing.Size(273, 25);
			this.hideBox.StylizeImage = false;
			this.hideBox.TabIndex = 2;
			this.hideBox.Text = "Do not show this message again";
			this.hideBox.ThemedBack = null;
			this.hideBox.ThemedFore = null;
			this.hideBox.UseVisualStyleBackColor = false;
			// 
			// messageBox
			// 
			this.messageBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.messageBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.messageBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.messageBox.Cursor = System.Windows.Forms.Cursors.Default;
			this.messageBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.messageBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.messageBox.Location = new System.Drawing.Point(94, 23);
			this.messageBox.Name = "messageBox";
			this.messageBox.ReadOnly = true;
			this.messageBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.messageBox.Size = new System.Drawing.Size(677, 84);
			this.messageBox.TabIndex = 1;
			this.messageBox.TabStop = false;
			this.messageBox.Text = "Marked the anchor. Now select the target text and finish the bi-directional link." +
    "";
			this.messageBox.SelectionChanged += new System.EventHandler(this.HideSelection);
			// 
			// iconBox
			// 
			this.iconBox.Location = new System.Drawing.Point(23, 23);
			this.iconBox.Name = "iconBox";
			this.iconBox.Size = new System.Drawing.Size(48, 48);
			this.iconBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.iconBox.TabIndex = 0;
			this.iconBox.TabStop = false;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(689, 13);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(115, 36);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.DoClick);
			this.okButton.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoKeyDown);
			// 
			// buttonPanel
			// 
			this.buttonPanel.BackColor = System.Drawing.SystemColors.Control;
			this.buttonPanel.Controls.Add(this.okButton);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.buttonPanel.Location = new System.Drawing.Point(0, 158);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Size = new System.Drawing.Size(816, 61);
			this.buttonPanel.TabIndex = 3;
			// 
			// BiLinkDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(816, 219);
			this.Controls.Add(this.topPanel);
			this.Controls.Add(this.buttonPanel);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "BiLinkDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "OneMore";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoKeyDown);
			this.topPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.iconBox)).EndInit();
			this.buttonPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel topPanel;
		private UI.MoreButton okButton;
		private System.Windows.Forms.Panel buttonPanel;
		private System.Windows.Forms.PictureBox iconBox;
		private River.OneMoreAddIn.UI.MoreRichLabel messageBox;
		private MoreCheckBox hideBox;
	}
}