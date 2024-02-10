namespace River.OneMoreAddIn.Commands
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
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.listBox = new System.Windows.Forms.ListBox();
			this.upButton = new River.OneMoreAddIn.UI.MoreButton();
			this.downButton = new River.OneMoreAddIn.UI.MoreButton();
			this.label = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(120, 526);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(116, 38);
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
			this.cancelButton.Location = new System.Drawing.Point(242, 526);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(116, 38);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// listBox
			// 
			this.listBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.listBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.listBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this.listBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.listBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.listBox.FormattingEnabled = true;
			this.listBox.ItemHeight = 22;
			this.listBox.Location = new System.Drawing.Point(30, 88);
			this.listBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
			this.listBox.Name = "listBox";
			this.listBox.Size = new System.Drawing.Size(325, 400);
			this.listBox.TabIndex = 2;
			this.listBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.DrawItem);
			this.listBox.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.MeasureItem);
			this.listBox.SelectedIndexChanged += new System.EventHandler(this.ChangeSelection);
			// 
			// upButton
			// 
			this.upButton.AutoSize = true;
			this.upButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.upButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.upButton.Font = new System.Drawing.Font("Segoe UI Symbol", 10F);
			this.upButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.upButton.ImageOver = null;
			this.upButton.Location = new System.Drawing.Point(363, 169);
			this.upButton.Name = "upButton";
			this.upButton.ShowBorder = true;
			this.upButton.Size = new System.Drawing.Size(39, 38);
			this.upButton.TabIndex = 3;
			this.upButton.Text = "▲";
			this.upButton.ThemedBack = null;
			this.upButton.ThemedFore = null;
			this.upButton.UseVisualStyleBackColor = false;
			this.upButton.Click += new System.EventHandler(this.MoveUp);
			// 
			// downButton
			// 
			this.downButton.AutoSize = true;
			this.downButton.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.downButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.downButton.Font = new System.Drawing.Font("Segoe UI Symbol", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.downButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.downButton.ImageOver = null;
			this.downButton.Location = new System.Drawing.Point(363, 217);
			this.downButton.Name = "downButton";
			this.downButton.ShowBorder = true;
			this.downButton.Size = new System.Drawing.Size(39, 38);
			this.downButton.TabIndex = 4;
			this.downButton.Text = "▼";
			this.downButton.ThemedBack = null;
			this.downButton.ThemedFore = null;
			this.downButton.UseVisualStyleBackColor = false;
			this.downButton.Click += new System.EventHandler(this.MoveDown);
			// 
			// label
			// 
			this.label.Location = new System.Drawing.Point(26, 26);
			this.label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label.Name = "label";
			this.label.Size = new System.Drawing.Size(376, 52);
			this.label.TabIndex = 5;
			this.label.Text = "Reorder how styles appear in the gallery. Also changes the order of headings in a" +
    " TOC";
			// 
			// ReorderDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(417, 586);
			this.Controls.Add(this.label);
			this.Controls.Add(this.downButton);
			this.Controls.Add(this.upButton);
			this.Controls.Add(this.listBox);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "ReorderDialog";
			this.Padding = new System.Windows.Forms.Padding(20, 20, 10, 20);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Reorder Custom Styles";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private System.Windows.Forms.ListBox listBox;
		private UI.MoreButton upButton;
		private UI.MoreButton downButton;
		private System.Windows.Forms.Label label;
	}
}