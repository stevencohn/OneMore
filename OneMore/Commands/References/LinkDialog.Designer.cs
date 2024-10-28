
namespace River.OneMoreAddIn.Commands
{
	partial class LinkDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LinkDialog));
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.notebooksRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.notebookRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.sectionRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.groupBox = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.introLabel = new System.Windows.Forms.Label();
			this.synopsisBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.titleLabel = new System.Windows.Forms.Label();
			this.groupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(311, 335);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(120, 38);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(437, 335);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(120, 38);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// notebooksRadio
			// 
			this.notebooksRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.notebooksRadio.Location = new System.Drawing.Point(23, 42);
			this.notebooksRadio.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.notebooksRadio.Name = "notebooksRadio";
			this.notebooksRadio.Size = new System.Drawing.Size(134, 25);
			this.notebooksRadio.TabIndex = 2;
			this.notebooksRadio.Text = "All notebooks";
			this.notebooksRadio.UseVisualStyleBackColor = true;
			// 
			// notebookRadio
			// 
			this.notebookRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.notebookRadio.Location = new System.Drawing.Point(23, 79);
			this.notebookRadio.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.notebookRadio.Name = "notebookRadio";
			this.notebookRadio.Size = new System.Drawing.Size(291, 25);
			this.notebookRadio.TabIndex = 3;
			this.notebookRadio.Text = "All sections in the current notebook";
			this.notebookRadio.UseVisualStyleBackColor = true;
			// 
			// sectionRadio
			// 
			this.sectionRadio.Checked = true;
			this.sectionRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.sectionRadio.Location = new System.Drawing.Point(23, 116);
			this.sectionRadio.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.sectionRadio.Name = "sectionRadio";
			this.sectionRadio.Size = new System.Drawing.Size(176, 25);
			this.sectionRadio.TabIndex = 4;
			this.sectionRadio.TabStop = true;
			this.sectionRadio.Text = "The current section";
			this.sectionRadio.UseVisualStyleBackColor = true;
			// 
			// groupBox
			// 
			this.groupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox.Controls.Add(this.notebookRadio);
			this.groupBox.Controls.Add(this.sectionRadio);
			this.groupBox.Controls.Add(this.notebooksRadio);
			this.groupBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.groupBox.Location = new System.Drawing.Point(23, 91);
			this.groupBox.Name = "groupBox";
			this.groupBox.Padding = new System.Windows.Forms.Padding(20);
			this.groupBox.Size = new System.Drawing.Size(534, 165);
			this.groupBox.TabIndex = 5;
			this.groupBox.TabStop = false;
			this.groupBox.Text = "Pages in";
			this.groupBox.ThemedBorder = null;
			this.groupBox.ThemedFore = null;
			// 
			// introLabel
			// 
			this.introLabel.AutoSize = true;
			this.introLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.introLabel.Location = new System.Drawing.Point(23, 20);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(343, 20);
			this.introLabel.TabIndex = 6;
			this.introLabel.Text = "Create linked references to the title of this page";
			// 
			// synopsisBox
			// 
			this.synopsisBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.synopsisBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.synopsisBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.synopsisBox.Location = new System.Drawing.Point(46, 277);
			this.synopsisBox.Name = "synopsisBox";
			this.synopsisBox.Size = new System.Drawing.Size(270, 25);
			this.synopsisBox.StylizeImage = false;
			this.synopsisBox.TabIndex = 7;
			this.synopsisBox.Text = "Include a synopsis of each page";
			this.synopsisBox.ThemedBack = null;
			this.synopsisBox.ThemedFore = null;
			this.synopsisBox.UseVisualStyleBackColor = true;
			// 
			// titleLabel
			// 
			this.titleLabel.AutoSize = true;
			this.titleLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.titleLabel.Location = new System.Drawing.Point(23, 49);
			this.titleLabel.Name = "titleLabel";
			this.titleLabel.Size = new System.Drawing.Size(34, 20);
			this.titleLabel.TabIndex = 8;
			this.titleLabel.Text = "title";
			// 
			// LinkDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(580, 386);
			this.Controls.Add(this.titleLabel);
			this.Controls.Add(this.synopsisBox);
			this.Controls.Add(this.introLabel);
			this.Controls.Add(this.groupBox);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LinkDialog";
			this.Padding = new System.Windows.Forms.Padding(20, 20, 20, 10);
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Link References";
			this.groupBox.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private UI.MoreRadioButton notebooksRadio;
		private UI.MoreRadioButton notebookRadio;
		private UI.MoreRadioButton sectionRadio;
		private UI.MoreGroupBox groupBox;
		private System.Windows.Forms.Label introLabel;
		private UI.MoreCheckBox synopsisBox;
		private System.Windows.Forms.Label titleLabel;
	}
}