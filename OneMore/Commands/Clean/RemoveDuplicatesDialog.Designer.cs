namespace River.OneMoreAddIn.Commands
{
	partial class RemoveDuplicatesDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RemoveDuplicatesDialog));
			this.scopeGroupBox = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.scopeSelector = new River.OneMoreAddIn.UI.ScopeSelector();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.depthBox = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.includeTitlesBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.deepRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.basicRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.simpleRadio = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.scopeGroupBox.SuspendLayout();
			this.depthBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// scopeGroupBox
			// 
			this.scopeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.scopeGroupBox.Controls.Add(this.scopeSelector);
			this.scopeGroupBox.Location = new System.Drawing.Point(18, 219);
			this.scopeGroupBox.Margin = new System.Windows.Forms.Padding(3, 12, 3, 3);
			this.scopeGroupBox.Name = "scopeGroupBox";
			this.scopeGroupBox.Padding = new System.Windows.Forms.Padding(20, 15, 3, 3);
			this.scopeGroupBox.ShowOnlyTopEdge = true;
			this.scopeGroupBox.Size = new System.Drawing.Size(567, 341);
			this.scopeGroupBox.TabIndex = 0;
			this.scopeGroupBox.TabStop = false;
			this.scopeGroupBox.Text = "Scope";
			this.scopeGroupBox.ThemedBorder = null;
			this.scopeGroupBox.ThemedFore = null;
			// 
			// scopeSelector
			// 
			this.scopeSelector.Dock = System.Windows.Forms.DockStyle.Fill;
			this.scopeSelector.Location = new System.Drawing.Point(20, 34);
			this.scopeSelector.Name = "scopeSelector";
			this.scopeSelector.Scopes = ((River.OneMoreAddIn.UI.SelectorScope)((((River.OneMoreAddIn.UI.SelectorScope.Section | River.OneMoreAddIn.UI.SelectorScope.Notebook) 
            | River.OneMoreAddIn.UI.SelectorScope.Notebooks) 
            | River.OneMoreAddIn.UI.SelectorScope.SelectedNotebooks)));
			this.scopeSelector.Size = new System.Drawing.Size(544, 304);
			this.scopeSelector.TabIndex = 0;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(379, 577);
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
			this.cancelButton.Location = new System.Drawing.Point(485, 577);
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
			// depthBox
			// 
			this.depthBox.Controls.Add(this.includeTitlesBox);
			this.depthBox.Controls.Add(this.deepRadio);
			this.depthBox.Controls.Add(this.basicRadio);
			this.depthBox.Controls.Add(this.simpleRadio);
			this.depthBox.Location = new System.Drawing.Point(18, 13);
			this.depthBox.Name = "depthBox";
			this.depthBox.Padding = new System.Windows.Forms.Padding(3, 15, 3, 3);
			this.depthBox.ShowOnlyTopEdge = true;
			this.depthBox.Size = new System.Drawing.Size(567, 191);
			this.depthBox.TabIndex = 8;
			this.depthBox.TabStop = false;
			this.depthBox.Text = "Scanning Depth";
			this.depthBox.ThemedBorder = null;
			this.depthBox.ThemedFore = null;
			// 
			// includeTitlesBox
			// 
			this.includeTitlesBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.includeTitlesBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.includeTitlesBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.includeTitlesBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.includeTitlesBox.Location = new System.Drawing.Point(20, 148);
			this.includeTitlesBox.Name = "includeTitlesBox";
			this.includeTitlesBox.Size = new System.Drawing.Size(293, 25);
			this.includeTitlesBox.StylizeImage = false;
			this.includeTitlesBox.TabIndex = 3;
			this.includeTitlesBox.Text = "Include page titles when comparing";
			this.includeTitlesBox.ThemedBack = null;
			this.includeTitlesBox.ThemedFore = null;
			this.includeTitlesBox.UseVisualStyleBackColor = true;
			// 
			// deepRadio
			// 
			this.deepRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.deepRadio.Location = new System.Drawing.Point(20, 97);
			this.deepRadio.Name = "deepRadio";
			this.deepRadio.Size = new System.Drawing.Size(334, 25);
			this.deepRadio.TabIndex = 2;
			this.deepRadio.Text = "Deep XML with binary comparison (slow)";
			this.deepRadio.UseVisualStyleBackColor = true;
			// 
			// basicRadio
			// 
			this.basicRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.basicRadio.Location = new System.Drawing.Point(20, 67);
			this.basicRadio.Name = "basicRadio";
			this.basicRadio.Size = new System.Drawing.Size(183, 25);
			this.basicRadio.TabIndex = 1;
			this.basicRadio.Text = "Basic XML structure";
			this.basicRadio.UseVisualStyleBackColor = true;
			// 
			// simpleRadio
			// 
			this.simpleRadio.Checked = true;
			this.simpleRadio.Cursor = System.Windows.Forms.Cursors.Hand;
			this.simpleRadio.Location = new System.Drawing.Point(20, 37);
			this.simpleRadio.Name = "simpleRadio";
			this.simpleRadio.Size = new System.Drawing.Size(171, 25);
			this.simpleRadio.TabIndex = 0;
			this.simpleRadio.TabStop = true;
			this.simpleRadio.Text = "Simple text (quick)";
			this.simpleRadio.UseVisualStyleBackColor = true;
			// 
			// RemoveDuplicatesDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(598, 627);
			this.Controls.Add(this.depthBox);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.scopeGroupBox);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(500, 500);
			this.Name = "RemoveDuplicatesDialog";
			this.Padding = new System.Windows.Forms.Padding(15, 10, 10, 0);
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Remove Duplicate Pages";
			this.scopeGroupBox.ResumeLayout(false);
			this.depthBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreGroupBox scopeGroupBox;
		private UI.ScopeSelector scopeSelector;
		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private UI.MoreGroupBox depthBox;
		private UI.MoreRadioButton deepRadio;
		private UI.MoreRadioButton basicRadio;
		private UI.MoreRadioButton simpleRadio;
		private UI.MoreCheckBox includeTitlesBox;
	}
}