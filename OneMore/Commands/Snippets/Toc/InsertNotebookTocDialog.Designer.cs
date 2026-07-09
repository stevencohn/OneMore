namespace River.OneMoreAddIn.Commands
{
	partial class InsertNotebookTocDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InsertNotebookTocDialog));
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.pagesBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.previewBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.timeBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(222, 147);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(120, 38);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 3;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.CollectParametersOnOK);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(350, 147);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(120, 38);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 4;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// pagesBox
			// 
			this.pagesBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.pagesBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pagesBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.pagesBox.Location = new System.Drawing.Point(23, 20);
			this.pagesBox.Name = "pagesBox";
			this.pagesBox.Size = new System.Drawing.Size(252, 25);
			this.pagesBox.StylizeImage = false;
			this.pagesBox.TabIndex = 0;
			this.pagesBox.Text = "Include pages in each section";
			this.pagesBox.ThemedBack = null;
			this.pagesBox.ThemedFore = null;
			this.pagesBox.UseVisualStyleBackColor = true;
			this.pagesBox.CheckedChanged += new System.EventHandler(this.TogglePagesBox);
			// 
			// previewBox
			// 
			this.previewBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.previewBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.previewBox.Enabled = false;
			this.previewBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.previewBox.Location = new System.Drawing.Point(23, 51);
			this.previewBox.Name = "previewBox";
			this.previewBox.Size = new System.Drawing.Size(280, 25);
			this.previewBox.StylizeImage = false;
			this.previewBox.TabIndex = 1;
			this.previewBox.Text = "Include text preview of each page";
			this.previewBox.ThemedBack = null;
			this.previewBox.ThemedFore = null;
			this.previewBox.UseVisualStyleBackColor = true;
			// 
			// timeBox
			// 
			this.timeBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.timeBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.timeBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.timeBox.Location = new System.Drawing.Point(23, 82);
			this.timeBox.Name = "timeBox";
			this.timeBox.Size = new System.Drawing.Size(233, 25);
			this.timeBox.StylizeImage = false;
			this.timeBox.TabIndex = 2;
			this.timeBox.Text = "Update page date and time";
			this.timeBox.ThemedBack = null;
			this.timeBox.ThemedFore = null;
			this.timeBox.UseVisualStyleBackColor = true;
			// 
			// InsertNotebookTocDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(482, 210);
			this.Controls.Add(this.timeBox);
			this.Controls.Add(this.previewBox);
			this.Controls.Add(this.pagesBox);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "InsertNotebookTocDialog";
			this.Padding = new System.Windows.Forms.Padding(20, 25, 0, 0);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Table of Sections in Notebook";
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private UI.MoreCheckBox pagesBox;
		private UI.MoreCheckBox previewBox;
		private UI.MoreCheckBox timeBox;
	}
}
