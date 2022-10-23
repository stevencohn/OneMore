
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
			this.components = new System.ComponentModel.Container();
			this.introBox = new System.Windows.Forms.TextBox();
			this.layoutPanel = new System.Windows.Forms.Panel();
			this.imageViewerLabel = new System.Windows.Forms.Label();
			this.imageViewerButton = new System.Windows.Forms.Button();
			this.imageViewerBox = new System.Windows.Forms.TextBox();
			this.langBox = new System.Windows.Forms.ComboBox();
			this.langLabel = new System.Windows.Forms.Label();
			this.checkUpdatesBox = new System.Windows.Forms.CheckBox();
			this.enablersBox = new System.Windows.Forms.CheckBox();
			this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
			this.layoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
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
			this.layoutPanel.Controls.Add(this.imageViewerLabel);
			this.layoutPanel.Controls.Add(this.imageViewerButton);
			this.layoutPanel.Controls.Add(this.imageViewerBox);
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
			// imageViewerLabel
			// 
			this.imageViewerLabel.AutoSize = true;
			this.imageViewerLabel.Location = new System.Drawing.Point(3, 218);
			this.imageViewerLabel.Margin = new System.Windows.Forms.Padding(3, 10, 3, 0);
			this.imageViewerLabel.Name = "imageViewerLabel";
			this.imageViewerLabel.Size = new System.Drawing.Size(359, 20);
			this.imageViewerLabel.TabIndex = 6;
			this.imageViewerLabel.Text = "External image viewer (default if empty is mspaint)";
			// 
			// imageViewerButton
			// 
			this.imageViewerButton.Location = new System.Drawing.Point(591, 240);
			this.imageViewerButton.Name = "imageViewerButton";
			this.imageViewerButton.Size = new System.Drawing.Size(36, 31);
			this.imageViewerButton.TabIndex = 5;
			this.imageViewerButton.Text = "...";
			this.imageViewerButton.UseVisualStyleBackColor = true;
			this.imageViewerButton.Click += new System.EventHandler(this.BrowseImageViewer);
			// 
			// imageViewerBox
			// 
			this.imageViewerBox.Location = new System.Drawing.Point(7, 241);
			this.imageViewerBox.Name = "imageViewerBox";
			this.imageViewerBox.Size = new System.Drawing.Size(578, 26);
			this.imageViewerBox.TabIndex = 4;
			this.imageViewerBox.Text = "mspaint";
			this.imageViewerBox.TextChanged += new System.EventHandler(this.ValidateImageViewer);
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
			// langLabel
			// 
			this.langLabel.AutoSize = true;
			this.langLabel.Location = new System.Drawing.Point(3, 131);
			this.langLabel.Name = "langLabel";
			this.langLabel.Size = new System.Drawing.Size(297, 20);
			this.langLabel.TabIndex = 2;
			this.langLabel.Text = "Display language (must restart OneNote)";
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
			// errorProvider1
			// 
			this.errorProvider1.ContainerControl = this;
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
			((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
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
		private System.Windows.Forms.Label imageViewerLabel;
		private System.Windows.Forms.Button imageViewerButton;
		private System.Windows.Forms.TextBox imageViewerBox;
		private System.Windows.Forms.ErrorProvider errorProvider1;
	}
}
