namespace River.OneMoreAddIn.Commands
{
	partial class HashtagErrorControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.messageBox = new System.Windows.Forms.TextBox();
			this.notesBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// messageBox
			// 
			this.messageBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.messageBox.ForeColor = System.Drawing.Color.Brown;
			this.messageBox.Location = new System.Drawing.Point(15, 15);
			this.messageBox.Multiline = true;
			this.messageBox.Name = "messageBox";
			this.messageBox.Size = new System.Drawing.Size(670, 52);
			this.messageBox.TabIndex = 0;
			this.messageBox.Text = "messageBox";
			// 
			// notesBox
			// 
			this.notesBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.notesBox.Font = new System.Drawing.Font("Lucida Console", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.notesBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.notesBox.Location = new System.Drawing.Point(15, 73);
			this.notesBox.Multiline = true;
			this.notesBox.Name = "notesBox";
			this.notesBox.Size = new System.Drawing.Size(670, 52);
			this.notesBox.TabIndex = 1;
			this.notesBox.Text = "notesBox";
			// 
			// HashtagErrorControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.Controls.Add(this.notesBox);
			this.Controls.Add(this.messageBox);
			this.Name = "HashtagErrorControl";
			this.Padding = new System.Windows.Forms.Padding(12);
			this.Size = new System.Drawing.Size(700, 153);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox messageBox;
		private System.Windows.Forms.TextBox notesBox;
	}
}
