namespace River.OneMoreAddIn.UI
{
	partial class MoreMessageBox
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MoreMessageBox));
			this.topPanel = new System.Windows.Forms.Panel();
			this.textBox = new System.Windows.Forms.RichTextBox();
			this.iconBox = new System.Windows.Forms.PictureBox();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.topPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.iconBox)).BeginInit();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// topPanel
			// 
			this.topPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.topPanel.Controls.Add(this.textBox);
			this.topPanel.Controls.Add(this.iconBox);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.topPanel.Location = new System.Drawing.Point(0, 0);
			this.topPanel.Margin = new System.Windows.Forms.Padding(0);
			this.topPanel.Name = "topPanel";
			this.topPanel.Padding = new System.Windows.Forms.Padding(20);
			this.topPanel.Size = new System.Drawing.Size(526, 92);
			this.topPanel.TabIndex = 0;
			// 
			// textBox
			// 
			this.textBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.textBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textBox.Cursor = System.Windows.Forms.Cursors.Default;
			this.textBox.Location = new System.Drawing.Point(94, 23);
			this.textBox.Name = "textBox";
			this.textBox.ReadOnly = true;
			this.textBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
			this.textBox.Size = new System.Drawing.Size(387, 46);
			this.textBox.TabIndex = 1;
			this.textBox.TabStop = false;
			this.textBox.Text = "This is the message";
			this.textBox.SelectionChanged += new System.EventHandler(this.HideSelection);
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
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(399, 13);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(115, 36);
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoKeyDown);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(278, 13);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(115, 36);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoKeyDown);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.cancelButton);
			this.panel1.Controls.Add(this.okButton);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 92);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(526, 61);
			this.panel1.TabIndex = 3;
			// 
			// MoreMessageBox
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(526, 153);
			this.Controls.Add(this.topPanel);
			this.Controls.Add(this.panel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MoreMessageBox";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "OneMore";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoKeyDown);
			this.topPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.iconBox)).EndInit();
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel topPanel;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.PictureBox iconBox;
		private System.Windows.Forms.RichTextBox textBox;
	}
}