namespace River.OneMoreAddIn.Commands
{
	partial class ToggleDttmDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ToggleDttmDialog));
			this.hideRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.showRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.toggleGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.scopeGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.scopeSelector = new River.OneMoreAddIn.UI.ScopeSelector();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.toggleGroup.SuspendLayout();
			this.scopeGroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// hideRadio
			// 
			this.hideRadio.Checked = true;
			this.hideRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.hideRadio.Location = new System.Drawing.Point(18, 37);
			this.hideRadio.Name = "hideRadio";
			this.hideRadio.Size = new System.Drawing.Size(173, 25);
			this.hideRadio.TabIndex = 0;
			this.hideRadio.TabStop = true;
			this.hideRadio.Text = "Hide date and time";
			this.hideRadio.UseVisualStyleBackColor = true;
			// 
			// showRadio
			// 
			this.showRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.showRadio.Location = new System.Drawing.Point(18, 67);
			this.showRadio.Name = "showRadio";
			this.showRadio.Size = new System.Drawing.Size(181, 25);
			this.showRadio.TabIndex = 1;
			this.showRadio.Text = "Show date and time";
			this.showRadio.UseVisualStyleBackColor = true;
			// 
			// toggleGroup
			// 
			this.toggleGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.toggleGroup.Controls.Add(this.hideRadio);
			this.toggleGroup.Controls.Add(this.showRadio);
			this.toggleGroup.ForeColor = System.Drawing.SystemColors.ControlText;
			this.toggleGroup.Location = new System.Drawing.Point(18, 18);
			this.toggleGroup.Name = "toggleGroup";
			this.toggleGroup.Padding = new System.Windows.Forms.Padding(15);
			this.toggleGroup.ShowOnlyTopEdge = true;
			this.toggleGroup.Size = new System.Drawing.Size(424, 119);
			this.toggleGroup.TabIndex = 2;
			this.toggleGroup.TabStop = false;
			this.toggleGroup.Text = "Toggle";
			this.toggleGroup.ThemedBorder = null;
			this.toggleGroup.ThemedFore = null;
			// 
			// scopeGroup
			// 
			this.scopeGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.scopeGroup.Controls.Add(this.scopeSelector);
			this.scopeGroup.ForeColor = System.Drawing.SystemColors.ControlText;
			this.scopeGroup.Location = new System.Drawing.Point(18, 159);
			this.scopeGroup.Name = "scopeGroup";
			this.scopeGroup.Padding = new System.Windows.Forms.Padding(15);
			this.scopeGroup.ShowOnlyTopEdge = true;
			this.scopeGroup.Size = new System.Drawing.Size(424, 124);
			this.scopeGroup.TabIndex = 3;
			this.scopeGroup.TabStop = false;
			this.scopeGroup.Text = "Scope";
			this.scopeGroup.ThemedBorder = null;
			this.scopeGroup.ThemedFore = null;
			// 
			// scopeSelector
			// 
			this.scopeSelector.Location = new System.Drawing.Point(18, 34);
			this.scopeSelector.Margin = new System.Windows.Forms.Padding(0);
			this.scopeSelector.Name = "scopeSelector";
			this.scopeSelector.Scopes = ((River.OneMoreAddIn.UI.SelectorScope)((River.OneMoreAddIn.UI.SelectorScope.Page | River.OneMoreAddIn.UI.SelectorScope.Section)));
			this.scopeSelector.Size = new System.Drawing.Size(345, 75);
			this.scopeSelector.TabIndex = 0;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(342, 305);
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
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(236, 305);
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
			// ToggleDttmDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(460, 361);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.scopeGroup);
			this.Controls.Add(this.toggleGroup);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ToggleDttmDialog";
			this.Padding = new System.Windows.Forms.Padding(15);
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Show/Hide Date and Time";
			this.toggleGroup.ResumeLayout(false);
			this.scopeGroup.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreRadioButton hideRadio;
		private UI.MoreRadioButton showRadio;
		private UI.MoreGroupBox toggleGroup;
		private UI.MoreGroupBox scopeGroup;
		private UI.MoreButton cancelButton;
		private UI.MoreButton okButton;
		private UI.ScopeSelector scopeSelector;
	}
}