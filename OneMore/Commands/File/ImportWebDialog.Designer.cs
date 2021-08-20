
namespace River.OneMoreAddIn.Commands
{
	partial class ImportWebDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportWebDialog));
			this.addressLabel = new System.Windows.Forms.Label();
			this.addressBox = new System.Windows.Forms.TextBox();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.appendButton = new System.Windows.Forms.RadioButton();
			this.newPageButton = new System.Windows.Forms.RadioButton();
			this.newChildButton = new System.Windows.Forms.RadioButton();
			this.imagesBox = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// addressLabel
			// 
			this.addressLabel.AutoSize = true;
			this.addressLabel.Location = new System.Drawing.Point(18, 30);
			this.addressLabel.Name = "addressLabel";
			this.addressLabel.Size = new System.Drawing.Size(68, 20);
			this.addressLabel.TabIndex = 0;
			this.addressLabel.Text = "Address";
			// 
			// addressBox
			// 
			this.addressBox.Location = new System.Drawing.Point(92, 27);
			this.addressBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 15);
			this.addressBox.Name = "addressBox";
			this.addressBox.Size = new System.Drawing.Size(548, 26);
			this.addressBox.TabIndex = 1;
			this.addressBox.TextChanged += new System.EventHandler(this.addressBox_TextChanged);
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Enabled = false;
			this.okButton.Location = new System.Drawing.Point(434, 219);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.TabIndex = 9;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(540, 219);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.TabIndex = 8;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// appendButton
			// 
			this.appendButton.AutoSize = true;
			this.appendButton.Location = new System.Drawing.Point(92, 171);
			this.appendButton.Name = "appendButton";
			this.appendButton.Size = new System.Drawing.Size(202, 24);
			this.appendButton.TabIndex = 10;
			this.appendButton.Text = "Append to current page";
			this.appendButton.UseVisualStyleBackColor = true;
			// 
			// newPageButton
			// 
			this.newPageButton.AutoSize = true;
			this.newPageButton.Checked = true;
			this.newPageButton.Location = new System.Drawing.Point(92, 111);
			this.newPageButton.Name = "newPageButton";
			this.newPageButton.Size = new System.Drawing.Size(176, 24);
			this.newPageButton.TabIndex = 11;
			this.newPageButton.TabStop = true;
			this.newPageButton.Text = "Create as new page";
			this.newPageButton.UseVisualStyleBackColor = true;
			// 
			// newChildButton
			// 
			this.newChildButton.AutoSize = true;
			this.newChildButton.Location = new System.Drawing.Point(92, 141);
			this.newChildButton.Name = "newChildButton";
			this.newChildButton.Size = new System.Drawing.Size(284, 24);
			this.newChildButton.TabIndex = 12;
			this.newChildButton.Text = "Create as new child of current page";
			this.newChildButton.UseVisualStyleBackColor = true;
			// 
			// imagesBox
			// 
			this.imagesBox.AutoSize = true;
			this.imagesBox.Location = new System.Drawing.Point(92, 71);
			this.imagesBox.Name = "imagesBox";
			this.imagesBox.Size = new System.Drawing.Size(199, 24);
			this.imagesBox.TabIndex = 13;
			this.imagesBox.Text = "Import as static images";
			this.imagesBox.UseVisualStyleBackColor = true;
			// 
			// ImportWebDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(658, 275);
			this.Controls.Add(this.imagesBox);
			this.Controls.Add(this.newChildButton);
			this.Controls.Add(this.newPageButton);
			this.Controls.Add(this.appendButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.addressBox);
			this.Controls.Add(this.addressLabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ImportWebDialog";
			this.Padding = new System.Windows.Forms.Padding(15);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Import Web Page";
			this.Load += new System.EventHandler(this.ImportWebDialog_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label addressLabel;
		private System.Windows.Forms.TextBox addressBox;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.RadioButton appendButton;
		private System.Windows.Forms.RadioButton newPageButton;
		private System.Windows.Forms.RadioButton newChildButton;
		private System.Windows.Forms.CheckBox imagesBox;
	}
}