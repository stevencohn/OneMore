namespace River.OneMoreAddIn.Commands
{
	partial class DiagnosticsDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DiagnosticsDialog));
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.introLabel = new System.Windows.Forms.Label();
			this.linkLabel = new River.OneMoreAddIn.UI.MoreLinkLabel();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(575, 99);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(140, 42);
			this.okButton.TabIndex = 5;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.CloseWindow);
			// 
			// introLabel
			// 
			this.introLabel.AutoSize = true;
			this.introLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.introLabel.Location = new System.Drawing.Point(28, 25);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(161, 20);
			this.introLabel.TabIndex = 6;
			this.introLabel.Text = "Diagnostics written to";
			// 
			// linkLabel
			// 
			this.linkLabel.ActiveLinkColor = System.Drawing.Color.Orchid;
			this.linkLabel.AutoSize = true;
			this.linkLabel.Cursor = System.Windows.Forms.Cursors.Hand;
			this.linkLabel.HoverColor = System.Drawing.Color.Orchid;
			this.linkLabel.LinkColor = System.Drawing.Color.MediumOrchid;
			this.linkLabel.Location = new System.Drawing.Point(28, 57);
			this.linkLabel.Name = "linkLabel";
			this.linkLabel.Size = new System.Drawing.Size(80, 20);
			this.linkLabel.StrictColors = false;
			this.linkLabel.TabIndex = 7;
			this.linkLabel.TabStop = true;
			this.linkLabel.Text = "linkLabel1";
			this.linkLabel.ThemedBack = null;
			this.linkLabel.ThemedFore = null;
			this.linkLabel.VisitedLinkColor = System.Drawing.Color.MediumOrchid;
			this.linkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ShowLogOnLinkClicked);
			// 
			// timer
			// 
			this.timer.Interval = 1000;
			this.timer.Tick += new System.EventHandler(this.Tick);
			// 
			// DiagnosticsDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.okButton;
			this.ClientSize = new System.Drawing.Size(728, 154);
			this.Controls.Add(this.linkLabel);
			this.Controls.Add(this.introLabel);
			this.Controls.Add(this.okButton);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DiagnosticsDialog";
			this.Padding = new System.Windows.Forms.Padding(25, 25, 10, 10);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "OneMore Diagnostics";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreButton okButton;
		private System.Windows.Forms.Label introLabel;
		private UI.MoreLinkLabel linkLabel;
		private System.Windows.Forms.Timer timer;
	}
}