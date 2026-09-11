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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RemoveDuplicatesDialog));
			this.scopeGroupBox = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.scopeSelector = new River.OneMoreAddIn.UI.ScopeSelector();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.metricsBox = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.tooltip = new System.Windows.Forms.ToolTip(this.components);
			this.scopeGroupBox.SuspendLayout();
			this.metricsBox.SuspendLayout();
			this.SuspendLayout();
			//
			// scopeGroupBox
			//
			this.scopeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.scopeGroupBox.Controls.Add(this.scopeSelector);
			this.scopeGroupBox.Location = new System.Drawing.Point(18, 209);
			this.scopeGroupBox.Margin = new System.Windows.Forms.Padding(3, 12, 3, 3);
			this.scopeGroupBox.Name = "scopeGroupBox";
			this.scopeGroupBox.Padding = new System.Windows.Forms.Padding(20, 15, 3, 3);
			this.scopeGroupBox.ShowOnlyTopEdge = true;
			this.scopeGroupBox.Size = new System.Drawing.Size(567, 341);
			this.scopeGroupBox.TabIndex = 2;
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
			this.okButton.Location = new System.Drawing.Point(379, 566);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 3;
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
			this.cancelButton.Location = new System.Drawing.Point(485, 566);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 4;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			//
			// metricsBox
			//
			// three MoreCheckBoxes - Content similarity / Presentation similarity (each a
			// two-line title+subtitle collapsing three SimilarityEngine.Rubric flags apiece)
			// plus "Show only exact duplicates" - are added dynamically in
			// RemoveDuplicatesDialog's constructor rather than declared here, since their
			// label text is table-driven/resx-composed and their spacing is computed together
			// - see CreateMetricBox/CreateExactOnlyBox
			this.metricsBox.Location = new System.Drawing.Point(18, 13);
			this.metricsBox.Name = "metricsBox";
			this.metricsBox.Padding = new System.Windows.Forms.Padding(3, 15, 3, 3);
			this.metricsBox.ShowOnlyTopEdge = true;
			this.metricsBox.Size = new System.Drawing.Size(567, 182);
			this.metricsBox.TabIndex = 0;
			this.metricsBox.TabStop = false;
			this.metricsBox.Text = "Similarity metrics";
			this.metricsBox.ThemedBorder = null;
			this.metricsBox.ThemedFore = null;
			//
			// RemoveDuplicatesDialog
			//
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(598, 620);
			this.Controls.Add(this.metricsBox);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.scopeGroupBox);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(500, 535);
			this.Name = "RemoveDuplicatesDialog";
			this.Padding = new System.Windows.Forms.Padding(15, 10, 10, 0);
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Remove Duplicate Pages";
			this.scopeGroupBox.ResumeLayout(false);
			this.metricsBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreGroupBox scopeGroupBox;
		private UI.ScopeSelector scopeSelector;
		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private UI.MoreGroupBox metricsBox;
		private System.Windows.Forms.ToolTip tooltip;
	}
}
