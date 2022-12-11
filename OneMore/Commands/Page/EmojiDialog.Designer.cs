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
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.iconBox = new System.Windows.Forms.ListBox();
			this.introLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(268, 478);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(99, 38);
			this.cancelButton.TabIndex = 2;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.Cancel);
			// 
			// okButton
			// 
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(164, 478);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(99, 38);
			this.okButton.TabIndex = 1;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OK);
			// 
			// iconBox
			// 
			this.iconBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this.iconBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.iconBox.FormattingEnabled = true;
			this.iconBox.Location = new System.Drawing.Point(16, 62);
			this.iconBox.Name = "iconBox";
			this.iconBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.iconBox.Size = new System.Drawing.Size(349, 394);
			this.iconBox.TabIndex = 0;
			this.iconBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.DrawIconItem);
			this.iconBox.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.MeasureIconItemSIze);
			this.iconBox.DoubleClick += new System.EventHandler(this.DoubleClickItem);
			// 
			// introLabel
			// 
			this.introLabel.AutoSize = true;
			this.introLabel.Location = new System.Drawing.Point(14, 26);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(328, 20);
			this.introLabel.TabIndex = 4;
			this.introLabel.Text = "Select one or more emojis to add to page title";
			// 
			// EmojiDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(387, 534);
			this.Controls.Add(this.introLabel);
			this.Controls.Add(this.iconBox);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "EmojiDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Page Title Emojis";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.ListBox iconBox;
		private System.Windows.Forms.Label introLabel;
	}
}