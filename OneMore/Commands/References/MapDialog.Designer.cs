
namespace River.OneMoreAddIn.Commands
{
	partial class MapDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapDialog));
			this.okButton = new UI.MoreButton();
			this.cancelButton = new UI.MoreButton();
			this.notebooksRadio = new UI.MoreRadioButton();
			this.notebookRadio = new UI.MoreRadioButton();
			this.sectionRadio = new UI.MoreRadioButton();
			this.groupBox = new UI.MoreGroupBox();
			this.catalogBox = new UI.MoreCheckBox();
			this.groupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(311, 267);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(120, 38);
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(437, 267);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(120, 38);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// notebooksRadio
			// 
			this.notebooksRadio.AutoSize = true;
			this.notebooksRadio.Location = new System.Drawing.Point(23, 42);
			this.notebooksRadio.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.notebooksRadio.Name = "notebooksRadio";
			this.notebooksRadio.Size = new System.Drawing.Size(130, 24);
			this.notebooksRadio.TabIndex = 2;
			this.notebooksRadio.Text = "All notebooks";
			this.notebooksRadio.UseVisualStyleBackColor = true;
			this.notebooksRadio.Click += new System.EventHandler(this.ChangeScope);
			// 
			// notebookRadio
			// 
			this.notebookRadio.AutoSize = true;
			this.notebookRadio.Location = new System.Drawing.Point(23, 79);
			this.notebookRadio.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.notebookRadio.Name = "notebookRadio";
			this.notebookRadio.Size = new System.Drawing.Size(282, 24);
			this.notebookRadio.TabIndex = 3;
			this.notebookRadio.Text = "All sections in the current notebook";
			this.notebookRadio.UseVisualStyleBackColor = true;
			this.notebookRadio.Click += new System.EventHandler(this.ChangeScope);
			// 
			// sectionRadio
			// 
			this.sectionRadio.AutoSize = true;
			this.sectionRadio.Checked = true;
			this.sectionRadio.Location = new System.Drawing.Point(23, 116);
			this.sectionRadio.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.sectionRadio.Name = "sectionRadio";
			this.sectionRadio.Size = new System.Drawing.Size(170, 24);
			this.sectionRadio.TabIndex = 4;
			this.sectionRadio.TabStop = true;
			this.sectionRadio.Text = "The current section";
			this.sectionRadio.UseVisualStyleBackColor = true;
			this.sectionRadio.Click += new System.EventHandler(this.ChangeScope);
			// 
			// groupBox
			// 
			this.groupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox.Controls.Add(this.catalogBox);
			this.groupBox.Controls.Add(this.notebookRadio);
			this.groupBox.Controls.Add(this.sectionRadio);
			this.groupBox.Controls.Add(this.notebooksRadio);
			this.groupBox.Location = new System.Drawing.Point(23, 23);
			this.groupBox.Name = "groupBox";
			this.groupBox.Padding = new System.Windows.Forms.Padding(20);
			this.groupBox.Size = new System.Drawing.Size(534, 223);
			this.groupBox.TabIndex = 5;
			this.groupBox.TabStop = false;
			this.groupBox.Text = "Pages in";
			// 
			// catalogBox
			// 
			this.catalogBox.AutoSize = true;
			this.catalogBox.Location = new System.Drawing.Point(23, 176);
			this.catalogBox.Name = "catalogBox";
			this.catalogBox.Size = new System.Drawing.Size(281, 24);
			this.catalogBox.TabIndex = 5;
			this.catalogBox.Text = "Include cross-notebook references";
			this.catalogBox.UseVisualStyleBackColor = true;
			// 
			// MapDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(580, 318);
			this.Controls.Add(this.groupBox);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MapDialog";
			this.Padding = new System.Windows.Forms.Padding(20, 20, 20, 10);
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Create Page Map";
			this.groupBox.ResumeLayout(false);
			this.groupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private UI.MoreRadioButton notebooksRadio;
		private UI.MoreRadioButton notebookRadio;
		private UI.MoreRadioButton sectionRadio;
		private UI.MoreGroupBox groupBox;
		private UI.MoreCheckBox catalogBox;
	}
}