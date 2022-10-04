namespace River.OneMoreAddIn.UI
{
	partial class ScopeSelector
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.pageButton = new System.Windows.Forms.RadioButton();
			this.sectionButton = new System.Windows.Forms.RadioButton();
			this.notebookButton = new System.Windows.Forms.RadioButton();
			this.notebooksButton = new System.Windows.Forms.RadioButton();
			this.selectedButton = new System.Windows.Forms.RadioButton();
			this.choiceBox = new System.Windows.Forms.Panel();
			this.selectionPanel = new System.Windows.Forms.Panel();
			this.listBox = new System.Windows.Forms.CheckedListBox();
			this.choiceBox.SuspendLayout();
			this.selectionPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// pageButton
			// 
			this.pageButton.AutoSize = true;
			this.pageButton.Location = new System.Drawing.Point(3, 3);
			this.pageButton.Name = "pageButton";
			this.pageButton.Size = new System.Drawing.Size(103, 24);
			this.pageButton.TabIndex = 0;
			this.pageButton.Text = "This page";
			this.pageButton.UseVisualStyleBackColor = true;
			this.pageButton.CheckedChanged += new System.EventHandler(this.ScopeChanged);
			// 
			// sectionButton
			// 
			this.sectionButton.AutoSize = true;
			this.sectionButton.Location = new System.Drawing.Point(3, 33);
			this.sectionButton.Name = "sectionButton";
			this.sectionButton.Size = new System.Drawing.Size(170, 24);
			this.sectionButton.TabIndex = 1;
			this.sectionButton.Text = "The current section";
			this.sectionButton.UseVisualStyleBackColor = true;
			this.sectionButton.CheckedChanged += new System.EventHandler(this.ScopeChanged);
			// 
			// notebookButton
			// 
			this.notebookButton.AutoSize = true;
			this.notebookButton.Location = new System.Drawing.Point(3, 63);
			this.notebookButton.Name = "notebookButton";
			this.notebookButton.Size = new System.Drawing.Size(282, 24);
			this.notebookButton.TabIndex = 2;
			this.notebookButton.Text = "All sections in the current notebook";
			this.notebookButton.UseVisualStyleBackColor = true;
			this.notebookButton.CheckedChanged += new System.EventHandler(this.ScopeChanged);
			// 
			// notebooksButton
			// 
			this.notebooksButton.AutoSize = true;
			this.notebooksButton.Location = new System.Drawing.Point(3, 93);
			this.notebooksButton.Name = "notebooksButton";
			this.notebooksButton.Size = new System.Drawing.Size(130, 24);
			this.notebooksButton.TabIndex = 3;
			this.notebooksButton.Text = "All notebooks";
			this.notebooksButton.UseVisualStyleBackColor = true;
			this.notebooksButton.CheckedChanged += new System.EventHandler(this.ScopeChanged);
			// 
			// selectedButton
			// 
			this.selectedButton.AutoSize = true;
			this.selectedButton.Location = new System.Drawing.Point(3, 123);
			this.selectedButton.Name = "selectedButton";
			this.selectedButton.Size = new System.Drawing.Size(188, 24);
			this.selectedButton.TabIndex = 4;
			this.selectedButton.Text = "Selected notebooks...";
			this.selectedButton.UseVisualStyleBackColor = true;
			this.selectedButton.CheckedChanged += new System.EventHandler(this.ScopeChanged);
			// 
			// choiceBox
			// 
			this.choiceBox.Controls.Add(this.selectedButton);
			this.choiceBox.Controls.Add(this.pageButton);
			this.choiceBox.Controls.Add(this.notebooksButton);
			this.choiceBox.Controls.Add(this.sectionButton);
			this.choiceBox.Controls.Add(this.notebookButton);
			this.choiceBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.choiceBox.Location = new System.Drawing.Point(0, 0);
			this.choiceBox.Name = "choiceBox";
			this.choiceBox.Size = new System.Drawing.Size(485, 158);
			this.choiceBox.TabIndex = 5;
			// 
			// selectionPanel
			// 
			this.selectionPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.selectionPanel.Controls.Add(this.listBox);
			this.selectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.selectionPanel.Location = new System.Drawing.Point(0, 158);
			this.selectionPanel.Name = "selectionPanel";
			this.selectionPanel.Padding = new System.Windows.Forms.Padding(20, 10, 0, 0);
			this.selectionPanel.Size = new System.Drawing.Size(485, 329);
			this.selectionPanel.TabIndex = 6;
			// 
			// listBox
			// 
			this.listBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listBox.CheckOnClick = true;
			this.listBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBox.FormattingEnabled = true;
			this.listBox.Location = new System.Drawing.Point(20, 10);
			this.listBox.Name = "listBox";
			this.listBox.Size = new System.Drawing.Size(465, 319);
			this.listBox.TabIndex = 0;
			// 
			// ScopeSelector
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.selectionPanel);
			this.Controls.Add(this.choiceBox);
			this.Name = "ScopeSelector";
			this.Size = new System.Drawing.Size(485, 487);
			this.choiceBox.ResumeLayout(false);
			this.choiceBox.PerformLayout();
			this.selectionPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RadioButton pageButton;
		private System.Windows.Forms.RadioButton sectionButton;
		private System.Windows.Forms.RadioButton notebookButton;
		private System.Windows.Forms.RadioButton notebooksButton;
		private System.Windows.Forms.RadioButton selectedButton;
		private System.Windows.Forms.Panel choiceBox;
		private System.Windows.Forms.Panel selectionPanel;
		private System.Windows.Forms.CheckedListBox listBox;
	}
}
