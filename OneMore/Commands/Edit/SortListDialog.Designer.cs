
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
			this.thisListButton = new System.Windows.Forms.RadioButton();
			this.allListsButton = new System.Windows.Forms.RadioButton();
			this.typeBox = new System.Windows.Forms.CheckBox();
			this.deepBox = new System.Windows.Forms.CheckBox();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.optionsBox = new System.Windows.Forms.GroupBox();
			this.introLabel = new System.Windows.Forms.Label();
			this.optionsBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// thisListButton
			// 
			this.thisListButton.AutoSize = true;
			this.thisListButton.Checked = true;
			this.thisListButton.Location = new System.Drawing.Point(23, 42);
			this.thisListButton.Name = "thisListButton";
			this.thisListButton.Size = new System.Drawing.Size(200, 24);
			this.thisListButton.TabIndex = 0;
			this.thisListButton.TabStop = true;
			this.thisListButton.Text = "Sort only the current list";
			this.thisListButton.UseVisualStyleBackColor = true;
			// 
			// allListsButton
			// 
			this.allListsButton.AutoSize = true;
			this.allListsButton.Location = new System.Drawing.Point(23, 72);
			this.allListsButton.Name = "allListsButton";
			this.allListsButton.Size = new System.Drawing.Size(266, 24);
			this.allListsButton.TabIndex = 1;
			this.allListsButton.Text = "Sort all top-level lists on the page";
			this.allListsButton.UseVisualStyleBackColor = true;
			// 
			// typeBox
			// 
			this.typeBox.AutoSize = true;
			this.typeBox.Location = new System.Drawing.Point(23, 148);
			this.typeBox.Name = "typeBox";
			this.typeBox.Size = new System.Drawing.Size(280, 24);
			this.typeBox.TabIndex = 2;
			this.typeBox.Text = "Both bullet lists and numbered lists";
			this.typeBox.UseVisualStyleBackColor = true;
			// 
			// deepBox
			// 
			this.deepBox.AutoSize = true;
			this.deepBox.Location = new System.Drawing.Point(23, 118);
			this.deepBox.Name = "deepBox";
			this.deepBox.Size = new System.Drawing.Size(149, 24);
			this.deepBox.TabIndex = 3;
			this.deepBox.Text = "Include sub-lists";
			this.deepBox.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(391, 300);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.TabIndex = 7;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(497, 300);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.TabIndex = 6;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// optionsBox
			// 
			this.optionsBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.optionsBox.Controls.Add(this.thisListButton);
			this.optionsBox.Controls.Add(this.allListsButton);
			this.optionsBox.Controls.Add(this.typeBox);
			this.optionsBox.Controls.Add(this.deepBox);
			this.optionsBox.Location = new System.Drawing.Point(23, 67);
			this.optionsBox.Name = "optionsBox";
			this.optionsBox.Padding = new System.Windows.Forms.Padding(20);
			this.optionsBox.Size = new System.Drawing.Size(574, 209);
			this.optionsBox.TabIndex = 8;
			this.optionsBox.TabStop = false;
			this.optionsBox.Text = "Options";
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
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(620, 361);
			this.Controls.Add(this.introLabel);
			this.Controls.Add(this.optionsBox);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SortListDialog";
			this.Padding = new System.Windows.Forms.Padding(20);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Sort Lists";
			this.optionsBox.ResumeLayout(false);
			this.optionsBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.RadioButton thisListButton;
		private System.Windows.Forms.RadioButton allListsButton;
		private System.Windows.Forms.CheckBox typeBox;
		private System.Windows.Forms.CheckBox deepBox;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.GroupBox optionsBox;
		private System.Windows.Forms.Label introLabel;
	}
}