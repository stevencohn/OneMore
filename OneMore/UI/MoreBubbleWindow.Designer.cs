
namespace River.OneMoreAddIn.Commands
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
			this.okButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.iconBox)).BeginInit();
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
			this.iconBox.Location = new System.Drawing.Point(23, 23);
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
			this.messageBox.Location = new System.Drawing.Point(93, 23);
			this.messageBox.Name = "messageBox";
			this.messageBox.ReadOnly = true;
			this.messageBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.messageBox.Size = new System.Drawing.Size(510, 71);
			this.messageBox.TabIndex = 2;
			this.messageBox.TabStop = false;
			this.messageBox.Text = "This is the message";
			this.messageBox.Click += new System.EventHandler(this.Unclick);
			this.messageBox.MouseEnter += new System.EventHandler(this.PauseTimer);
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(488, 105);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(115, 36);
			this.okButton.TabIndex = 3;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.CloseWindow);
			this.okButton.MouseEnter += new System.EventHandler(this.PauseTimer);
			// 
			// MoreBubbleWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Thistle;
			this.CancelButton = this.okButton;
			this.ClientSize = new System.Drawing.Size(626, 164);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.messageBox);
			this.Controls.Add(this.iconBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MoreBubbleWindow";
			this.Padding = new System.Windows.Forms.Padding(20);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "OneNote";
			((System.ComponentModel.ISupportInitialize)(this.iconBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.Timer timer;
		private System.Windows.Forms.PictureBox iconBox;
		private River.OneMoreAddIn.UI.MoreRichLabel messageBox;
		private System.Windows.Forms.Button okButton;
	}
}