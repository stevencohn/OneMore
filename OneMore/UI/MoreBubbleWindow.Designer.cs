
namespace River.OneMoreAddIn.UI
{
	partial class MoreBubbleWindow
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MoreBubbleWindow));
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.iconBox = new System.Windows.Forms.PictureBox();
			this.messageBox = new River.OneMoreAddIn.UI.MoreRichLabel();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.panel1 = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.iconBox)).BeginInit();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// timer
			// 
			this.timer.Interval = 20;
			this.timer.Tick += new System.EventHandler(this.Tick);
			// 
			// iconBox
			// 
			this.iconBox.BackgroundImage = global::River.OneMoreAddIn.Properties.Resources.Logo;
			this.iconBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.iconBox.Location = new System.Drawing.Point(27, 29);
			this.iconBox.Name = "iconBox";
			this.iconBox.Size = new System.Drawing.Size(48, 48);
			this.iconBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.iconBox.TabIndex = 1;
			this.iconBox.TabStop = false;
			this.iconBox.MouseEnter += new System.EventHandler(this.PauseTimer);
			// 
			// messageBox
			// 
			this.messageBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.messageBox.BackColor = System.Drawing.Color.Thistle;
			this.messageBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.messageBox.Cursor = System.Windows.Forms.Cursors.Default;
			this.messageBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.messageBox.ForeColor = System.Drawing.Color.Black;
			this.messageBox.Location = new System.Drawing.Point(94, 29);
			this.messageBox.Name = "messageBox";
			this.messageBox.ReadOnly = true;
			this.messageBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.messageBox.Size = new System.Drawing.Size(680, 71);
			this.messageBox.TabIndex = 2;
			this.messageBox.TabStop = false;
			this.messageBox.Text = "This is the message";
			this.messageBox.Click += new System.EventHandler(this.Unclick);
			this.messageBox.MouseEnter += new System.EventHandler(this.PauseTimer);
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(659, 137);
			this.okButton.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.okButton.Name = "okButton";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(115, 36);
			this.okButton.TabIndex = 3;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.CloseWindow);
			this.okButton.MouseEnter += new System.EventHandler(this.PauseTimer);
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.Thistle;
			this.panel1.Controls.Add(this.iconBox);
			this.panel1.Controls.Add(this.messageBox);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Margin = new System.Windows.Forms.Padding(0);
			this.panel1.Name = "panel1";
			this.panel1.Padding = new System.Windows.Forms.Padding(8);
			this.panel1.Size = new System.Drawing.Size(796, 119);
			this.panel1.TabIndex = 4;
			// 
			// MoreBubbleWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.CancelButton = this.okButton;
			this.ClientSize = new System.Drawing.Size(796, 192);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.okButton);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MoreBubbleWindow";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "OneNote";
			((System.ComponentModel.ISupportInitialize)(this.iconBox)).EndInit();
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.Timer timer;
		private System.Windows.Forms.PictureBox iconBox;
		private River.OneMoreAddIn.UI.MoreRichLabel messageBox;
		private UI.MoreButton okButton;
		private System.Windows.Forms.Panel panel1;
	}
}