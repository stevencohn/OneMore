
namespace River.OneMoreAddIn.Commands
{
	partial class TagControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TagControl));
			this.label = new System.Windows.Forms.Label();
			this.xButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label
			// 
			this.label.AutoSize = true;
			this.label.Location = new System.Drawing.Point(3, 3);
			this.label.Margin = new System.Windows.Forms.Padding(3);
			this.label.Name = "label";
			this.label.Size = new System.Drawing.Size(133, 20);
			this.label.TabIndex = 0;
			this.label.Text = "I\'m a tag, dummy!";
			// 
			// xButton
			// 
			this.xButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.xButton.Image = ((System.Drawing.Image)(resources.GetObject("xButton.Image")));
			this.xButton.Location = new System.Drawing.Point(162, -1);
			this.xButton.Margin = new System.Windows.Forms.Padding(0);
			this.xButton.Name = "xButton";
			this.xButton.Size = new System.Drawing.Size(31, 30);
			this.xButton.TabIndex = 1;
			this.xButton.UseVisualStyleBackColor = true;
			this.xButton.Click += new System.EventHandler(this.DeleteTag);
			// 
			// TagLabel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.Controls.Add(this.xButton);
			this.Controls.Add(this.label);
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "TagLabel";
			this.Size = new System.Drawing.Size(193, 29);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label;
		private System.Windows.Forms.Button xButton;
	}
}
