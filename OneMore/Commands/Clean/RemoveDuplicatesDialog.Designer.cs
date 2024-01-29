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
			this.includeTitlesBox = new System.Windows.Forms.CheckBox();
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
			this.scopeGroupBox.Location = new System.Drawing.Point(18, 204);
			this.scopeGroupBox.Margin = new System.Windows.Forms.Padding(3, 12, 3, 3);
			this.scopeGroupBox.Name = "scopeGroupBox";
			this.scopeGroupBox.Padding = new System.Windows.Forms.Padding(20, 3, 3, 3);
			this.scopeGroupBox.Size = new System.Drawing.Size(567, 313);
			this.scopeGroupBox.TabIndex = 0;
			this.scopeGroupBox.TabStop = false;
			this.scopeGroupBox.Text = "Scope";
			// 
			// scopeSelector
			// 
			this.scopeSelector.Dock = System.Windows.Forms.DockStyle.Fill;
			this.scopeSelector.Location = new System.Drawing.Point(20, 22);
			this.scopeSelector.Name = "scopeSelector";
			this.scopeSelector.Scopes = ((River.OneMoreAddIn.UI.SelectorScope)((((River.OneMoreAddIn.UI.SelectorScope.Section | River.OneMoreAddIn.UI.SelectorScope.Notebook) 
            | River.OneMoreAddIn.UI.SelectorScope.Notebooks) 
            | River.OneMoreAddIn.UI.SelectorScope.SelectedNotebooks)));
			this.scopeSelector.Size = new System.Drawing.Size(544, 288);
			this.scopeSelector.TabIndex = 0;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(379, 534);
			this.okButton.Name = "okButton";
			this.okButton.PreferredBack = null;
			this.okButton.PreferredFore = null;
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(485, 534);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.PreferredBack = null;
			this.cancelButton.PreferredFore = null;
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
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
			this.depthBox.Size = new System.Drawing.Size(567, 166);
			this.depthBox.TabIndex = 8;
			this.depthBox.TabStop = false;
			this.depthBox.Text = "Scanning Depth";
			// 
			// includeTitlesBox
			// 
			this.includeTitlesBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.includeTitlesBox.AutoSize = true;
			this.includeTitlesBox.Location = new System.Drawing.Point(20, 130);
			this.includeTitlesBox.Name = "includeTitlesBox";
			this.includeTitlesBox.Size = new System.Drawing.Size(284, 24);
			this.includeTitlesBox.TabIndex = 3;
			this.includeTitlesBox.Text = "Include page titles when comparing";
			this.includeTitlesBox.UseVisualStyleBackColor = true;
			// 
			// deepRadio
			// 
			this.deepRadio.Location = new System.Drawing.Point(20, 85);
			this.deepRadio.Name = "deepRadio";
			this.deepRadio.Size = new System.Drawing.Size(330, 24);
			this.deepRadio.TabIndex = 2;
			this.deepRadio.Text = "Deep XML with binary comparison (slow)";
			this.deepRadio.UseVisualStyleBackColor = true;
			// 
			// basicRadio
			// 
			this.basicRadio.Location = new System.Drawing.Point(20, 55);
			this.basicRadio.Name = "basicRadio";
			this.basicRadio.Size = new System.Drawing.Size(178, 24);
			this.basicRadio.TabIndex = 1;
			this.basicRadio.Text = "Basic XML structure";
			this.basicRadio.UseVisualStyleBackColor = true;
			// 
			// simpleRadio
			// 
			this.simpleRadio.Checked = true;
			this.simpleRadio.Location = new System.Drawing.Point(20, 25);
			this.simpleRadio.Name = "simpleRadio";
			this.simpleRadio.Size = new System.Drawing.Size(166, 24);
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
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(598, 584);
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
			this.depthBox.PerformLayout();
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
		private System.Windows.Forms.CheckBox includeTitlesBox;
	}
}