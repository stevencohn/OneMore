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
			this.components = new System.ComponentModel.Container();
			this.pageLink = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.tooltip = new System.Windows.Forms.ToolTip(this.components);
			this.snippetsPanel = new System.Windows.Forms.FlowLayoutPanel();
			this.dateLabel = new System.Windows.Forms.Label();
			this.checkbox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.SuspendLayout();
			// 
			// pageLink
			// 
			this.pageLink.AutoSize = true;
			this.pageLink.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pageLink.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pageLink.HoverColor = System.Drawing.Color.MediumOrchid;
			this.pageLink.LinkColor = System.Drawing.Color.MediumOrchid;
			this.pageLink.Location = new System.Drawing.Point(41, 10);
			this.pageLink.Name = "pageLink";
			this.pageLink.Size = new System.Drawing.Size(502, 22);
			this.pageLink.TabIndex = 0;
			this.pageLink.TabStop = true;
			this.pageLink.Text = "/OneMore Wiki/Context Menus/Notebook Panel Context Menu";
			this.pageLink.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.pageLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.NavigateTo);
			// 
			// snippetsPanel
			// 
			this.snippetsPanel.AutoSize = true;
			this.snippetsPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.snippetsPanel.Location = new System.Drawing.Point(58, 38);
			this.snippetsPanel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.snippetsPanel.Name = "snippetsPanel";
			this.snippetsPanel.Size = new System.Drawing.Size(635, 41);
			this.snippetsPanel.TabIndex = 1;
			// 
			// dateLabel
			// 
			this.dateLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.dateLabel.AutoSize = true;
			this.dateLabel.ForeColor = System.Drawing.SystemColors.GrayText;
			this.dateLabel.Location = new System.Drawing.Point(642, 10);
			this.dateLabel.Name = "dateLabel";
			this.dateLabel.Size = new System.Drawing.Size(51, 20);
			this.dateLabel.TabIndex = 2;
			this.dateLabel.Text = "label1";
			this.dateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkbox
			// 
			this.checkbox.AutoSize = true;
			this.checkbox.Location = new System.Drawing.Point(13, 12);
			this.checkbox.Name = "checkbox";
			this.checkbox.Size = new System.Drawing.Size(22, 21);
			this.checkbox.TabIndex = 3;
			this.checkbox.UseVisualStyleBackColor = true;
			this.checkbox.CheckedChanged += new System.EventHandler(this.DoCheckChanged);
			// 
			// HashtagContextControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.checkbox);
			this.Controls.Add(this.dateLabel);
			this.Controls.Add(this.snippetsPanel);
			this.Controls.Add(this.pageLink);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
			this.Name = "HashtagContextControl";
			this.Padding = new System.Windows.Forms.Padding(10);
			this.Size = new System.Drawing.Size(706, 99);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreLinkLabel pageLink;
		private System.Windows.Forms.ToolTip tooltip;
		private System.Windows.Forms.FlowLayoutPanel snippetsPanel;
		private System.Windows.Forms.Label dateLabel;
		private UI.MoreCheckBox checkbox;
	}
}
