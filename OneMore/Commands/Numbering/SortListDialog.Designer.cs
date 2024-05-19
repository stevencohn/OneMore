
namespace River.OneMoreAddIn.Commands
{
	partial class SortListDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SortListDialog));
			this.thisListButton = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.allListsButton = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.typeBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.deepBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.duplicatesBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.introLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// thisListButton
			// 
			this.thisListButton.Checked = true;
			this.thisListButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.thisListButton.Location = new System.Drawing.Point(41, 71);
			this.thisListButton.Name = "thisListButton";
			this.thisListButton.Size = new System.Drawing.Size(208, 25);
			this.thisListButton.TabIndex = 0;
			this.thisListButton.TabStop = true;
			this.thisListButton.Text = "Sort only the current list";
			this.thisListButton.UseVisualStyleBackColor = true;
			// 
			// allListsButton
			// 
			this.allListsButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.allListsButton.Location = new System.Drawing.Point(41, 101);
			this.allListsButton.Name = "allListsButton";
			this.allListsButton.Size = new System.Drawing.Size(278, 25);
			this.allListsButton.TabIndex = 1;
			this.allListsButton.Text = "Sort all top-level lists on the page";
			this.allListsButton.UseVisualStyleBackColor = true;
			// 
			// typeBox
			// 
			this.typeBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.typeBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.typeBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.typeBox.Location = new System.Drawing.Point(41, 177);
			this.typeBox.Name = "typeBox";
			this.typeBox.Size = new System.Drawing.Size(290, 25);
			this.typeBox.StylizeImage = false;
			this.typeBox.TabIndex = 3;
			this.typeBox.Text = "Both bullet lists and numbered lists";
			this.typeBox.ThemedBack = null;
			this.typeBox.ThemedFore = null;
			this.typeBox.UseVisualStyleBackColor = true;
			// 
			// deepBox
			// 
			this.deepBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.deepBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.deepBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.deepBox.Location = new System.Drawing.Point(41, 147);
			this.deepBox.Name = "deepBox";
			this.deepBox.Size = new System.Drawing.Size(154, 25);
			this.deepBox.StylizeImage = false;
			this.deepBox.TabIndex = 2;
			this.deepBox.Text = "Include sub-lists";
			this.deepBox.ThemedBack = null;
			this.deepBox.ThemedFore = null;
			this.deepBox.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(391, 267);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(100, 38);
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
			this.cancelButton.Location = new System.Drawing.Point(497, 267);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// duplicatesBox
			// 
			this.duplicatesBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.duplicatesBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.duplicatesBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.duplicatesBox.Location = new System.Drawing.Point(41, 207);
			this.duplicatesBox.Name = "duplicatesBox";
			this.duplicatesBox.Size = new System.Drawing.Size(176, 25);
			this.duplicatesBox.StylizeImage = false;
			this.duplicatesBox.TabIndex = 4;
			this.duplicatesBox.Text = "Remove duplicates";
			this.duplicatesBox.ThemedBack = null;
			this.duplicatesBox.ThemedFore = null;
			this.duplicatesBox.UseVisualStyleBackColor = true;
			// 
			// introLabel
			// 
			this.introLabel.AutoSize = true;
			this.introLabel.Location = new System.Drawing.Point(23, 20);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(414, 20);
			this.introLabel.TabIndex = 9;
			this.introLabel.Text = "Default is to sort the current list or only top level bullet lists";
			// 
			// SortListDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(620, 328);
			this.Controls.Add(this.duplicatesBox);
			this.Controls.Add(this.introLabel);
			this.Controls.Add(this.thisListButton);
			this.Controls.Add(this.allListsButton);
			this.Controls.Add(this.typeBox);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.deepBox);
			this.Controls.Add(this.cancelButton);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SortListDialog";
			this.Padding = new System.Windows.Forms.Padding(20);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Sort Lists";
			this.TopMost = true;
			this.Load += new System.EventHandler(this.LoadForm);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreRadioButton thisListButton;
		private UI.MoreRadioButton allListsButton;
		private UI.MoreCheckBox typeBox;
		private UI.MoreCheckBox deepBox;
		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private System.Windows.Forms.Label introLabel;
		private UI.MoreCheckBox duplicatesBox;
	}
}