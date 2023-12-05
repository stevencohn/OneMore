namespace River.OneMoreAddIn.Commands
{
	partial class HashtagContextControl
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
			this.pageLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.contextLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.SuspendLayout();
			// 
			// pageLink
			// 
			this.pageLink.AutoSize = true;
			this.pageLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pageLink.HoverColor = System.Drawing.Color.MediumOrchid;
			this.pageLink.LinkColor = System.Drawing.Color.MediumOrchid;
			this.pageLink.Location = new System.Drawing.Point(13, 10);
			this.pageLink.Name = "pageLink";
			this.pageLink.Size = new System.Drawing.Size(443, 20);
			this.pageLink.TabIndex = 0;
			this.pageLink.TabStop = true;
			this.pageLink.Text = "/OneMore Wiki/Context Menus/Notebook Panel Context Menu";
			this.pageLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			// 
			// contextLink
			// 
			this.contextLink.ActiveLinkColor = System.Drawing.SystemColors.GrayText;
			this.contextLink.AutoSize = true;
			this.contextLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.contextLink.ForeColor = System.Drawing.SystemColors.GrayText;
			this.contextLink.HoverColor = System.Drawing.Color.MediumOrchid;
			this.contextLink.LinkColor = System.Drawing.SystemColors.GrayText;
			this.contextLink.Location = new System.Drawing.Point(30, 40);
			this.contextLink.Margin = new System.Windows.Forms.Padding(20, 10, 10, 10);
			this.contextLink.Name = "contextLink";
			this.contextLink.Size = new System.Drawing.Size(530, 20);
			this.contextLink.TabIndex = 1;
			this.contextLink.TabStop = true;
			this.contextLink.Text = "... to substring search for a complete hashtag name of the form \"##name,\"";
			this.contextLink.VisitedLinkColor = System.Drawing.SystemColors.GrayText;
			// 
			// HashtagContextControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.Controls.Add(this.contextLink);
			this.Controls.Add(this.pageLink);
			this.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
			this.Name = "HashtagContextControl";
			this.Padding = new System.Windows.Forms.Padding(10);
			this.Size = new System.Drawing.Size(706, 88);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreLinkLabel pageLink;
		private UI.MoreLinkLabel contextLink;
	}
}
