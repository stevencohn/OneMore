
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LinkDialog));
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.notebooksRadio = new System.Windows.Forms.RadioButton();
			this.notebookRadio = new System.Windows.Forms.RadioButton();
			this.sectionRadio = new System.Windows.Forms.RadioButton();
			this.groupBox = new System.Windows.Forms.GroupBox();
			this.introLabel = new System.Windows.Forms.Label();
			this.synopsisBox = new System.Windows.Forms.CheckBox();
			this.unindexedBox = new System.Windows.Forms.CheckBox();
			this.tooltip = new System.Windows.Forms.ToolTip(this.components);
			this.groupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(311, 335);
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
			this.cancelButton.Location = new System.Drawing.Point(437, 335);
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
			// 
			// groupBox
			// 
			this.groupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox.Controls.Add(this.notebookRadio);
			this.groupBox.Controls.Add(this.sectionRadio);
			this.groupBox.Controls.Add(this.notebooksRadio);
			this.groupBox.Location = new System.Drawing.Point(23, 70);
			this.groupBox.Name = "groupBox";
			this.groupBox.Padding = new System.Windows.Forms.Padding(20);
			this.groupBox.Size = new System.Drawing.Size(534, 165);
			this.groupBox.TabIndex = 5;
			this.groupBox.TabStop = false;
			this.groupBox.Text = "Pages in";
			// 
			// introLabel
			// 
			this.introLabel.AutoSize = true;
			this.introLabel.Location = new System.Drawing.Point(23, 20);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(343, 20);
			this.introLabel.TabIndex = 6;
			this.introLabel.Text = "Create linked references to the title of this page";
			// 
			// synopsisBox
			// 
			this.synopsisBox.AutoSize = true;
			this.synopsisBox.Location = new System.Drawing.Point(46, 256);
			this.synopsisBox.Name = "synopsisBox";
			this.synopsisBox.Size = new System.Drawing.Size(262, 24);
			this.synopsisBox.TabIndex = 7;
			this.synopsisBox.Text = "Include a synopsis of each page";
			this.synopsisBox.UseVisualStyleBackColor = true;
			// 
			// unindexedBox
			// 
			this.unindexedBox.AutoSize = true;
			this.unindexedBox.Location = new System.Drawing.Point(46, 286);
			this.unindexedBox.Name = "unindexedBox";
			this.unindexedBox.Size = new System.Drawing.Size(271, 24);
			this.unindexedBox.TabIndex = 8;
			this.unindexedBox.Text = "Include unindexed pages in query";
			this.tooltip.SetToolTip(this.unindexedBox, "Enable to search newer text that has not yet been indexed by OneNote. This may be" +
        " slower.");
			this.unindexedBox.UseVisualStyleBackColor = true;
			// 
			// LinkDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(580, 386);
			this.Controls.Add(this.unindexedBox);
			this.Controls.Add(this.synopsisBox);
			this.Controls.Add(this.introLabel);
			this.Controls.Add(this.groupBox);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LinkDialog";
			this.Padding = new System.Windows.Forms.Padding(20, 20, 20, 10);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Link References";
			this.groupBox.ResumeLayout(false);
			this.groupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.RadioButton notebooksRadio;
		private System.Windows.Forms.RadioButton notebookRadio;
		private System.Windows.Forms.RadioButton sectionRadio;
		private System.Windows.Forms.GroupBox groupBox;
		private System.Windows.Forms.Label introLabel;
		private System.Windows.Forms.CheckBox synopsisBox;
		private System.Windows.Forms.CheckBox unindexedBox;
		private System.Windows.Forms.ToolTip tooltip;
	}
}