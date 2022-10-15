
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
			this.textBox = new System.Windows.Forms.RichTextBox();
			this.okButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.iconBox)).BeginInit();
			this.SuspendLayout();
			// 
			// timer
			// 
			this.timer.Interval = 1000;
			this.timer.Tick += new System.EventHandler(this.Tick);
			// 
			// iconBox
			// 
			this.iconBox.BackgroundImage = global::River.OneMoreAddIn.Properties.Resources.Logo;
			this.iconBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.iconBox.Location = new System.Drawing.Point(23, 23);
			this.iconBox.Name = "iconBox";
			this.iconBox.Size = new System.Drawing.Size(64, 64);
			this.iconBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.iconBox.TabIndex = 1;
			this.iconBox.TabStop = false;
			// 
			// textBox
			// 
			this.textBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox.BackColor = System.Drawing.Color.Thistle;
			this.textBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textBox.Cursor = System.Windows.Forms.Cursors.Default;
			this.textBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBox.Location = new System.Drawing.Point(93, 23);
			this.textBox.Name = "textBox";
			this.textBox.ReadOnly = true;
			this.textBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.textBox.Size = new System.Drawing.Size(588, 91);
			this.textBox.TabIndex = 2;
			this.textBox.TabStop = false;
			this.textBox.Text = "This is the message";
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(566, 125);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(115, 36);
			this.okButton.TabIndex = 3;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.CloseWindow);
			// 
			// MoreBubbleWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Thistle;
			this.CancelButton = this.okButton;
			this.ClientSize = new System.Drawing.Size(704, 184);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.textBox);
			this.Controls.Add(this.iconBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MoreBubbleWindow";
			this.Opacity = 0.85D;
			this.Padding = new System.Windows.Forms.Padding(20);
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "OneNote Timer";
			this.Load += new System.EventHandler(this.DoLoad);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.StartWindowDrag);
			this.Move += new System.EventHandler(this.MoveWindow);
			((System.ComponentModel.ISupportInitialize)(this.iconBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.Timer timer;
		private System.Windows.Forms.PictureBox iconBox;
		private System.Windows.Forms.RichTextBox textBox;
		private System.Windows.Forms.Button okButton;
	}
}