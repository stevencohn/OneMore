namespace River.OneMoreAddIn.Dialogs
{
	partial class ReorderDialog
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
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.upButton = new System.Windows.Forms.Button();
			this.downButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(87, 424);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(115, 38);
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(208, 424);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(115, 38);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// listBox1
			// 
			this.listBox1.FormattingEnabled = true;
			this.listBox1.ItemHeight = 20;
			this.listBox1.Location = new System.Drawing.Point(23, 23);
			this.listBox1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
			this.listBox1.Name = "listBox1";
			this.listBox1.Size = new System.Drawing.Size(300, 384);
			this.listBox1.TabIndex = 2;
			// 
			// upButton
			// 
			this.upButton.Image = global::River.OneMoreAddIn.Properties.Resources.Architecture;
			this.upButton.Location = new System.Drawing.Point(330, 105);
			this.upButton.Name = "upButton";
			this.upButton.Size = new System.Drawing.Size(42, 42);
			this.upButton.TabIndex = 3;
			this.upButton.UseVisualStyleBackColor = true;
			// 
			// downButton
			// 
			this.downButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.downButton.Image = global::River.OneMoreAddIn.Properties.Resources.Star;
			this.downButton.Location = new System.Drawing.Point(329, 153);
			this.downButton.Name = "downButton";
			this.downButton.Size = new System.Drawing.Size(42, 42);
			this.downButton.TabIndex = 4;
			this.downButton.UseVisualStyleBackColor = true;
			// 
			// ReorderDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(395, 485);
			this.Controls.Add(this.downButton);
			this.Controls.Add(this.upButton);
			this.Controls.Add(this.listBox1);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "ReorderDialog";
			this.Padding = new System.Windows.Forms.Padding(20, 20, 10, 20);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Reorder Custom Styles";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.ListBox listBox1;
		private System.Windows.Forms.Button upButton;
		private System.Windows.Forms.Button downButton;
	}
}