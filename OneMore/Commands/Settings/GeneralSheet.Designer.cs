
namespace River.OneMoreAddIn.Settings
{
	partial class GeneralSheet
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
			this.introBox = new System.Windows.Forms.TextBox();
			this.layoutPanel = new System.Windows.Forms.Panel();
			this.checkUpdatesBox = new System.Windows.Forms.CheckBox();
			this.enablersBox = new System.Windows.Forms.CheckBox();
			this.langLabel = new System.Windows.Forms.Label();
			this.langBox = new System.Windows.Forms.ComboBox();
			this.layoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// introBox
			// 
			this.introBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.introBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.introBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.introBox.Location = new System.Drawing.Point(13, 8);
			this.introBox.Multiline = true;
			this.introBox.Name = "introBox";
			this.introBox.ReadOnly = true;
			this.introBox.Size = new System.Drawing.Size(772, 66);
			this.introBox.TabIndex = 3;
			this.introBox.Text = "Customize the overall behavior of OneMore";
			// 
			// layoutPanel
			// 
			this.layoutPanel.Controls.Add(this.langBox);
			this.layoutPanel.Controls.Add(this.langLabel);
			this.layoutPanel.Controls.Add(this.checkUpdatesBox);
			this.layoutPanel.Controls.Add(this.enablersBox);
			this.layoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.layoutPanel.Location = new System.Drawing.Point(13, 74);
			this.layoutPanel.Margin = new System.Windows.Forms.Padding(0);
			this.layoutPanel.Name = "layoutPanel";
			this.layoutPanel.Size = new System.Drawing.Size(772, 416);
			this.layoutPanel.TabIndex = 4;
			// 
			// checkUpdatesBox
			// 
			this.checkUpdatesBox.Checked = true;
			this.checkUpdatesBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkUpdatesBox.Location = new System.Drawing.Point(0, 65);
			this.checkUpdatesBox.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
			this.checkUpdatesBox.Name = "checkUpdatesBox";
			this.checkUpdatesBox.Size = new System.Drawing.Size(772, 52);
			this.checkUpdatesBox.TabIndex = 1;
			this.checkUpdatesBox.Text = "Check for new versions of OneMore when OneNote starts";
			this.checkUpdatesBox.UseVisualStyleBackColor = true;
			// 
			// enablersBox
			// 
			this.enablersBox.Checked = true;
			this.enablersBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.enablersBox.Location = new System.Drawing.Point(0, 3);
			this.enablersBox.Margin = new System.Windows.Forms.Padding(0);
			this.enablersBox.Name = "enablersBox";
			this.enablersBox.Size = new System.Drawing.Size(772, 52);
			this.enablersBox.TabIndex = 0;
			this.enablersBox.Text = "Enable dynamic menu context settings. Disable if you\'ve added OneMore commands to" +
    " the Quick Access toolbar";
			this.enablersBox.UseVisualStyleBackColor = true;
			// 
			// langLabel
			// 
			this.langLabel.AutoSize = true;
			this.langLabel.Location = new System.Drawing.Point(3, 131);
			this.langLabel.Name = "langLabel";
			this.langLabel.Size = new System.Drawing.Size(297, 20);
			this.langLabel.TabIndex = 2;
			this.langLabel.Text = "Display language (must restart OneNote)";
			// 
			// langBox
			// 
			this.langBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.langBox.FormattingEnabled = true;
			this.langBox.Location = new System.Drawing.Point(7, 154);
			this.langBox.Name = "langBox";
			this.langBox.Size = new System.Drawing.Size(345, 28);
			this.langBox.TabIndex = 3;
			// 
			// GeneralSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.layoutPanel);
			this.Controls.Add(this.introBox);
			this.Name = "GeneralSheet";
			this.Padding = new System.Windows.Forms.Padding(13, 8, 15, 10);
			this.Size = new System.Drawing.Size(800, 500);
			this.layoutPanel.ResumeLayout(false);
			this.layoutPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox introBox;
		private System.Windows.Forms.Panel layoutPanel;
		private System.Windows.Forms.CheckBox enablersBox;
		private System.Windows.Forms.CheckBox checkUpdatesBox;
		private System.Windows.Forms.ComboBox langBox;
		private System.Windows.Forms.Label langLabel;
	}
}
