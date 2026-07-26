
namespace River.OneMoreAddIn.Commands
{
	partial class AlignContainersDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AlignContainersDialog));
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.scopePanel = new System.Windows.Forms.Panel();
			this.allLabel = new System.Windows.Forms.Label();
			this.selectedButton = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.allButton = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.alignGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.leftButton = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.rightButton = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.topButton = new River.OneMoreAddIn.UI.MoreRadioButton();
			this.optionsPanel = new River.OneMoreAddIn.UI.MorePanel();
			this.overlapCheckBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.scopePanel.SuspendLayout();
			this.alignGroup.SuspendLayout();
			this.optionsPanel.SuspendLayout();
			this.SuspendLayout();
			//
			// okButton
			//
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(285, 305);
			this.okButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(112, 35);
			this.okButton.TabIndex = 6;
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
			this.cancelButton.Location = new System.Drawing.Point(406, 305);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(112, 35);
			this.cancelButton.TabIndex = 7;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			//
			// scopePanel
			//
			this.scopePanel.BackColor = System.Drawing.SystemColors.ControlLight;
			this.scopePanel.Controls.Add(this.allLabel);
			this.scopePanel.Controls.Add(this.selectedButton);
			this.scopePanel.Controls.Add(this.allButton);
			this.scopePanel.Location = new System.Drawing.Point(23, 15);
			this.scopePanel.Name = "scopePanel";
			this.scopePanel.Size = new System.Drawing.Size(485, 70);
			this.scopePanel.TabIndex = 0;
			//
			// allLabel
			//
			this.allLabel.AutoSize = true;
			this.allLabel.Location = new System.Drawing.Point(10, 8);
			this.allLabel.Name = "allLabel";
			this.allLabel.Size = new System.Drawing.Size(220, 20);
			this.allLabel.TabIndex = 0;
			this.allLabel.Text = "Applies to all containers";
			//
			// selectedButton
			//
			this.selectedButton.Checked = true;
			this.selectedButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.selectedButton.Location = new System.Drawing.Point(10, 5);
			this.selectedButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.selectedButton.Name = "selectedButton";
			this.selectedButton.Size = new System.Drawing.Size(440, 25);
			this.selectedButton.TabIndex = 0;
			this.selectedButton.TabStop = true;
			this.selectedButton.Text = "Applies to {0} selected containers";
			this.selectedButton.UseVisualStyleBackColor = true;
			//
			// allButton
			//
			this.allButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.allButton.Location = new System.Drawing.Point(10, 35);
			this.allButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.allButton.Name = "allButton";
			this.allButton.Size = new System.Drawing.Size(440, 25);
			this.allButton.TabIndex = 1;
			this.allButton.Text = "Applies to all containers";
			this.allButton.UseVisualStyleBackColor = true;
			//
			// alignGroup
			//
			this.alignGroup.Controls.Add(this.leftButton);
			this.alignGroup.Controls.Add(this.rightButton);
			this.alignGroup.Controls.Add(this.topButton);
			this.alignGroup.Location = new System.Drawing.Point(23, 95);
			this.alignGroup.Name = "alignGroup";
			this.alignGroup.Padding = new System.Windows.Forms.Padding(3, 15, 3, 3);
			this.alignGroup.ShowOnlyTopEdge = true;
			this.alignGroup.Size = new System.Drawing.Size(485, 130);
			this.alignGroup.TabIndex = 1;
			this.alignGroup.TabStop = false;
			this.alignGroup.Text = "Alignment";
			this.alignGroup.ThemedBorder = null;
			this.alignGroup.ThemedFore = null;
			//
			// leftButton
			//
			this.leftButton.Checked = true;
			this.leftButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.leftButton.Location = new System.Drawing.Point(20, 37);
			this.leftButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.leftButton.Name = "leftButton";
			this.leftButton.Size = new System.Drawing.Size(200, 25);
			this.leftButton.TabIndex = 0;
			this.leftButton.TabStop = true;
			this.leftButton.Text = "Left";
			this.leftButton.UseVisualStyleBackColor = true;
			//
			// rightButton
			//
			this.rightButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.rightButton.Location = new System.Drawing.Point(20, 67);
			this.rightButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.rightButton.Name = "rightButton";
			this.rightButton.Size = new System.Drawing.Size(200, 25);
			this.rightButton.TabIndex = 1;
			this.rightButton.Text = "Right";
			this.rightButton.UseVisualStyleBackColor = true;
			//
			// topButton
			//
			this.topButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.topButton.Location = new System.Drawing.Point(20, 97);
			this.topButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.topButton.Name = "topButton";
			this.topButton.Size = new System.Drawing.Size(200, 25);
			this.topButton.TabIndex = 2;
			this.topButton.Text = "Top";
			this.topButton.UseVisualStyleBackColor = true;
			//
			// optionsPanel
			//
			this.optionsPanel.BottomBorderColor = System.Drawing.SystemColors.Control;
			this.optionsPanel.BottomBorderSize = 0;
			this.optionsPanel.Controls.Add(this.overlapCheckBox);
			this.optionsPanel.Location = new System.Drawing.Point(23, 235);
			this.optionsPanel.Name = "optionsPanel";
			this.optionsPanel.Size = new System.Drawing.Size(485, 60);
			this.optionsPanel.TabIndex = 2;
			this.optionsPanel.ThemedBack = "ControlLight";
			this.optionsPanel.ThemedFore = null;
			this.optionsPanel.TopBorderColor = System.Drawing.SystemColors.ActiveBorder;
			this.optionsPanel.TopBorderSize = 1;
			//
			// overlapCheckBox
			//
			this.overlapCheckBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.overlapCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.overlapCheckBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.overlapCheckBox.Location = new System.Drawing.Point(10, 17);
			this.overlapCheckBox.Name = "overlapCheckBox";
			this.overlapCheckBox.Size = new System.Drawing.Size(160, 25);
			this.overlapCheckBox.TabIndex = 0;
			this.overlapCheckBox.Text = "Prevent overlaps";
			this.overlapCheckBox.UseVisualStyleBackColor = true;
			//
			// AlignContainersDialog
			//
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(537, 360);
			this.Controls.Add(this.optionsPanel);
			this.Controls.Add(this.alignGroup);
			this.Controls.Add(this.scopePanel);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AlignContainersDialog";
			this.Padding = new System.Windows.Forms.Padding(20, 20, 15, 15);
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Align Containers";
			this.scopePanel.ResumeLayout(false);
			this.scopePanel.PerformLayout();
			this.alignGroup.ResumeLayout(false);
			this.optionsPanel.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreButton okButton;
		private UI.MoreButton cancelButton;
		private System.Windows.Forms.Panel scopePanel;
		private System.Windows.Forms.Label allLabel;
		private UI.MoreRadioButton selectedButton;
		private UI.MoreRadioButton allButton;
		private UI.MoreGroupBox alignGroup;
		private UI.MoreRadioButton leftButton;
		private UI.MoreRadioButton rightButton;
		private UI.MoreRadioButton topButton;
		private UI.MorePanel optionsPanel;
		private UI.MoreCheckBox overlapCheckBox;
	}
}
