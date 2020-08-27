namespace River.OneMoreAddIn.Dialogs
{
	partial class ChangePageColorDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChangePageColorDialog));
			this.lightButton = new System.Windows.Forms.RadioButton();
			this.darkButton = new System.Windows.Forms.RadioButton();
			this.customButton = new System.Windows.Forms.RadioButton();
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.introLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lightButton
			// 
			this.lightButton.BackColor = System.Drawing.SystemColors.Window;
			this.lightButton.Font = new System.Drawing.Font("Calibri", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lightButton.Location = new System.Drawing.Point(22, 110);
			this.lightButton.Name = "lightButton";
			this.lightButton.Padding = new System.Windows.Forms.Padding(10, 5, 0, 5);
			this.lightButton.Size = new System.Drawing.Size(400, 41);
			this.lightButton.TabIndex = 0;
			this.lightButton.TabStop = true;
			this.lightButton.Text = "Light background";
			this.lightButton.UseVisualStyleBackColor = false;
			this.lightButton.Click += new System.EventHandler(this.SetLightColor);
			// 
			// darkButton
			// 
			this.darkButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.darkButton.Font = new System.Drawing.Font("Calibri", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.darkButton.ForeColor = System.Drawing.Color.White;
			this.darkButton.Location = new System.Drawing.Point(22, 177);
			this.darkButton.Name = "darkButton";
			this.darkButton.Padding = new System.Windows.Forms.Padding(10, 5, 0, 5);
			this.darkButton.Size = new System.Drawing.Size(400, 41);
			this.darkButton.TabIndex = 1;
			this.darkButton.TabStop = true;
			this.darkButton.Text = "Dark background";
			this.darkButton.UseVisualStyleBackColor = false;
			this.darkButton.Click += new System.EventHandler(this.SetDarkColor);
			// 
			// customButton
			// 
			this.customButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
			this.customButton.Font = new System.Drawing.Font("Calibri", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.customButton.ForeColor = System.Drawing.Color.Black;
			this.customButton.Location = new System.Drawing.Point(22, 245);
			this.customButton.Name = "customButton";
			this.customButton.Padding = new System.Windows.Forms.Padding(10, 5, 0, 5);
			this.customButton.Size = new System.Drawing.Size(400, 41);
			this.customButton.TabIndex = 3;
			this.customButton.TabStop = true;
			this.customButton.Text = "Custom background...";
			this.customButton.UseVisualStyleBackColor = false;
			this.customButton.Click += new System.EventHandler(this.ChooseCustomColor);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(382, 323);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.TabIndex = 4;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(276, 323);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.TabIndex = 5;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// introLabel
			// 
			this.introLabel.Location = new System.Drawing.Point(13, 10);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(469, 63);
			this.introLabel.TabIndex = 6;
			this.introLabel.Text = "After choosing a page color, load one of the predefined styles or customize your " +
    "own styles so all content has enough contrast to be visible.\r\n\r\n";
			// 
			// ChangePageColorDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(495, 374);
			this.Controls.Add(this.introLabel);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.customButton);
			this.Controls.Add(this.darkButton);
			this.Controls.Add(this.lightButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ChangePageColorDialog";
			this.Padding = new System.Windows.Forms.Padding(10);
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Change Page Color";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RadioButton lightButton;
		private System.Windows.Forms.RadioButton darkButton;
		private System.Windows.Forms.RadioButton customButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Label introLabel;
	}
}