namespace River.OneMoreAddIn.Commands
{
	partial class CommandPaletteDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CommandPaletteDialog));
			this.cmdBox = new System.Windows.Forms.TextBox();
			this.okButton = new System.Windows.Forms.Button();
			this.bodyPanel = new System.Windows.Forms.Panel();
			this.bodyPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// cmdBox
			// 
			this.cmdBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cmdBox.Location = new System.Drawing.Point(43, 23);
			this.cmdBox.Name = "cmdBox";
			this.cmdBox.Size = new System.Drawing.Size(551, 28);
			this.cmdBox.TabIndex = 0;
			this.cmdBox.TextChanged += new System.EventHandler(this.ValidateCommand);
			this.cmdBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoKeyDown);
			// 
			// okButton
			// 
			this.okButton.Enabled = false;
			this.okButton.Location = new System.Drawing.Point(600, 24);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 28);
			this.okButton.TabIndex = 1;
			this.okButton.Text = "Go";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.InvokeCommand);
			// 
			// bodyPanel
			// 
			this.bodyPanel.Controls.Add(this.cmdBox);
			this.bodyPanel.Controls.Add(this.okButton);
			this.bodyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.bodyPanel.Location = new System.Drawing.Point(0, 0);
			this.bodyPanel.Name = "bodyPanel";
			this.bodyPanel.Padding = new System.Windows.Forms.Padding(40, 20, 40, 20);
			this.bodyPanel.Size = new System.Drawing.Size(718, 115);
			this.bodyPanel.TabIndex = 3;
			// 
			// CommandPaletteDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(718, 115);
			this.Controls.Add(this.bodyPanel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CommandPaletteDialog";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "OneMore Command Palette";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoKeyDown);
			this.bodyPanel.ResumeLayout(false);
			this.bodyPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.TextBox cmdBox;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Panel bodyPanel;
	}
}