namespace River.OneMoreAddIn.Commands
{
	internal partial class CompleteHashtagDialog
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
			this.tagBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.SuspendLayout();
			//
			// tagBox
			//
			this.tagBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.tagBox.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.tagBox.Location = new System.Drawing.Point(0, 0);
			this.tagBox.Name = "tagBox";
			this.tagBox.ProcessEnterKey = false;
			this.tagBox.Size = new System.Drawing.Size(320, 23);
			this.tagBox.TabIndex = 0;
			this.tagBox.ThemedBack = null;
			this.tagBox.ThemedFore = null;
			//
			// CompleteHashtagDialog
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(320, 23);
			this.Controls.Add(this.tagBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CompleteHashtagDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoKeyDown);
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreTextBox tagBox;
	}
}
