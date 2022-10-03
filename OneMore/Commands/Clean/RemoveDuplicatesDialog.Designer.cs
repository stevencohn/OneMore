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
			this.scopeGroupBox = new System.Windows.Forms.GroupBox();
			this.scopeSelector = new River.OneMoreAddIn.UI.ScopeSelector();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.depthBox = new System.Windows.Forms.GroupBox();
			this.deepRadio = new System.Windows.Forms.RadioButton();
			this.basicRadio = new System.Windows.Forms.RadioButton();
			this.simpleRadio = new System.Windows.Forms.RadioButton();
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
			this.scopeGroupBox.Location = new System.Drawing.Point(12, 151);
			this.scopeGroupBox.Name = "scopeGroupBox";
			this.scopeGroupBox.Padding = new System.Windows.Forms.Padding(20, 3, 3, 3);
			this.scopeGroupBox.Size = new System.Drawing.Size(574, 366);
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
			this.scopeSelector.Size = new System.Drawing.Size(551, 341);
			this.scopeSelector.TabIndex = 0;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(380, 534);
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
			this.cancelButton.Location = new System.Drawing.Point(486, 534);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.TabIndex = 6;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// depthBox
			// 
			this.depthBox.Controls.Add(this.deepRadio);
			this.depthBox.Controls.Add(this.basicRadio);
			this.depthBox.Controls.Add(this.simpleRadio);
			this.depthBox.Location = new System.Drawing.Point(12, 12);
			this.depthBox.Name = "depthBox";
			this.depthBox.Size = new System.Drawing.Size(592, 133);
			this.depthBox.TabIndex = 8;
			this.depthBox.TabStop = false;
			this.depthBox.Text = "Scanning Depth";
			// 
			// deepRadio
			// 
			this.deepRadio.AutoSize = true;
			this.deepRadio.Location = new System.Drawing.Point(20, 85);
			this.deepRadio.Name = "deepRadio";
			this.deepRadio.Size = new System.Drawing.Size(319, 24);
			this.deepRadio.TabIndex = 2;
			this.deepRadio.Text = "Deep XML with binary comparison (slow)";
			this.deepRadio.UseVisualStyleBackColor = true;
			// 
			// basicRadio
			// 
			this.basicRadio.AutoSize = true;
			this.basicRadio.Location = new System.Drawing.Point(20, 55);
			this.basicRadio.Name = "basicRadio";
			this.basicRadio.Size = new System.Drawing.Size(177, 24);
			this.basicRadio.TabIndex = 1;
			this.basicRadio.Text = "Basic XML structure";
			this.basicRadio.UseVisualStyleBackColor = true;
			// 
			// simpleRadio
			// 
			this.simpleRadio.AutoSize = true;
			this.simpleRadio.Checked = true;
			this.simpleRadio.Location = new System.Drawing.Point(20, 25);
			this.simpleRadio.Name = "simpleRadio";
			this.simpleRadio.Size = new System.Drawing.Size(163, 24);
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
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(598, 584);
			this.Controls.Add(this.depthBox);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.scopeGroupBox);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(500, 500);
			this.Name = "RemoveDuplicatesDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Remove Duplicate Pages";
			this.scopeGroupBox.ResumeLayout(false);
			this.depthBox.ResumeLayout(false);
			this.depthBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox scopeGroupBox;
		private UI.ScopeSelector scopeSelector;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.GroupBox depthBox;
		private System.Windows.Forms.RadioButton deepRadio;
		private System.Windows.Forms.RadioButton basicRadio;
		private System.Windows.Forms.RadioButton simpleRadio;
	}
}