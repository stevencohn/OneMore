
namespace River.OneMoreAddIn.Settings
{
	partial class ImagesSheet
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
			this.introBox = new River.OneMoreAddIn.UI.MoreMultilineLabel();
			this.layoutPanel = new System.Windows.Forms.Panel();
			this.resizeGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.widthBox = new System.Windows.Forms.NumericUpDown();
			this.widthLabel = new System.Windows.Forms.Label();
			this.generalGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.imageViewerLabel = new System.Windows.Forms.Label();
			this.imageViewerBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.imageViewerButton = new River.OneMoreAddIn.UI.MoreButton();
			this.plantGroup = new River.OneMoreAddIn.UI.MoreGroupBox();
			this.plantUriLabel = new System.Windows.Forms.Label();
			this.plantUriBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.plantRemoveBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.plantCollapseBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.plantAfterBox = new River.OneMoreAddIn.UI.MoreCheckBox();
			this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
			this.layoutPanel.SuspendLayout();
			this.resizeGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.widthBox)).BeginInit();
			this.generalGroup.SuspendLayout();
			this.plantGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
			this.SuspendLayout();
			// 
			// introBox
			// 
			this.introBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.introBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.introBox.Location = new System.Drawing.Point(13, 8);
			this.introBox.Name = "introBox";
			this.introBox.Padding = new System.Windows.Forms.Padding(0, 0, 0, 36);
			this.introBox.Size = new System.Drawing.Size(772, 52);
			this.introBox.TabIndex = 3;
			this.introBox.Text = "Customize the defaults for Image commands";
			this.introBox.ThemedBack = "ControlLightLight";
			this.introBox.ThemedFore = "ControlText";
			// 
			// layoutPanel
			// 
			this.layoutPanel.Controls.Add(this.resizeGroup);
			this.layoutPanel.Controls.Add(this.generalGroup);
			this.layoutPanel.Controls.Add(this.plantGroup);
			this.layoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.layoutPanel.Location = new System.Drawing.Point(13, 60);
			this.layoutPanel.Margin = new System.Windows.Forms.Padding(0);
			this.layoutPanel.Name = "layoutPanel";
			this.layoutPanel.Size = new System.Drawing.Size(772, 430);
			this.layoutPanel.TabIndex = 4;
			// 
			// resizeGroup
			// 
			this.resizeGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.resizeGroup.Controls.Add(this.widthBox);
			this.resizeGroup.Controls.Add(this.widthLabel);
			this.resizeGroup.Location = new System.Drawing.Point(7, 121);
			this.resizeGroup.Name = "resizeGroup";
			this.resizeGroup.ShowOnlyTopEdge = true;
			this.resizeGroup.Size = new System.Drawing.Size(762, 90);
			this.resizeGroup.TabIndex = 11;
			this.resizeGroup.TabStop = false;
			this.resizeGroup.Text = "Resize Options";
			this.resizeGroup.ThemedBorder = null;
			this.resizeGroup.ThemedFore = null;
			// 
			// widthBox
			// 
			this.widthBox.Location = new System.Drawing.Point(254, 33);
			this.widthBox.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
			this.widthBox.Name = "widthBox";
			this.widthBox.Size = new System.Drawing.Size(120, 26);
			this.widthBox.TabIndex = 1;
			this.widthBox.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
			// 
			// widthLabel
			// 
			this.widthLabel.AutoSize = true;
			this.widthLabel.Location = new System.Drawing.Point(28, 35);
			this.widthLabel.Name = "widthLabel";
			this.widthLabel.Size = new System.Drawing.Size(162, 20);
			this.widthLabel.TabIndex = 0;
			this.widthLabel.Text = "Preferred resize width";
			// 
			// generalGroup
			// 
			this.generalGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.generalGroup.Controls.Add(this.imageViewerLabel);
			this.generalGroup.Controls.Add(this.imageViewerBox);
			this.generalGroup.Controls.Add(this.imageViewerButton);
			this.generalGroup.Location = new System.Drawing.Point(7, 6);
			this.generalGroup.Name = "generalGroup";
			this.generalGroup.ShowOnlyTopEdge = true;
			this.generalGroup.Size = new System.Drawing.Size(762, 109);
			this.generalGroup.TabIndex = 10;
			this.generalGroup.TabStop = false;
			this.generalGroup.Text = "General";
			this.generalGroup.ThemedBorder = null;
			this.generalGroup.ThemedFore = null;
			// 
			// imageViewerLabel
			// 
			this.imageViewerLabel.AutoSize = true;
			this.imageViewerLabel.Location = new System.Drawing.Point(28, 32);
			this.imageViewerLabel.Margin = new System.Windows.Forms.Padding(3, 10, 3, 0);
			this.imageViewerLabel.Name = "imageViewerLabel";
			this.imageViewerLabel.Size = new System.Drawing.Size(359, 20);
			this.imageViewerLabel.TabIndex = 9;
			this.imageViewerLabel.Text = "External image viewer (default if empty is mspaint)";
			// 
			// imageViewerBox
			// 
			this.imageViewerBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.imageViewerBox.Location = new System.Drawing.Point(32, 55);
			this.imageViewerBox.Name = "imageViewerBox";
			this.imageViewerBox.ProcessEnterKey = false;
			this.imageViewerBox.Size = new System.Drawing.Size(578, 26);
			this.imageViewerBox.TabIndex = 7;
			this.imageViewerBox.Text = "mspaint";
			this.imageViewerBox.ThemedBack = null;
			this.imageViewerBox.ThemedFore = null;
			this.imageViewerBox.TextChanged += new System.EventHandler(this.ValidateImageViewer);
			// 
			// imageViewerButton
			// 
			this.imageViewerButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.imageViewerButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.imageViewerButton.ImageOver = null;
			this.imageViewerButton.Location = new System.Drawing.Point(616, 54);
			this.imageViewerButton.Name = "imageViewerButton";
			this.imageViewerButton.ShowBorder = true;
			this.imageViewerButton.Size = new System.Drawing.Size(36, 31);
			this.imageViewerButton.StylizeImage = false;
			this.imageViewerButton.TabIndex = 8;
			this.imageViewerButton.Text = "...";
			this.imageViewerButton.ThemedBack = null;
			this.imageViewerButton.ThemedFore = null;
			this.imageViewerButton.UseVisualStyleBackColor = true;
			this.imageViewerButton.Click += new System.EventHandler(this.BrowseImageViewer);
			// 
			// plantGroup
			// 
			this.plantGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.plantGroup.Controls.Add(this.plantUriLabel);
			this.plantGroup.Controls.Add(this.plantUriBox);
			this.plantGroup.Controls.Add(this.plantRemoveBox);
			this.plantGroup.Controls.Add(this.plantCollapseBox);
			this.plantGroup.Controls.Add(this.plantAfterBox);
			this.plantGroup.Location = new System.Drawing.Point(7, 217);
			this.plantGroup.Name = "plantGroup";
			this.plantGroup.ShowOnlyTopEdge = true;
			this.plantGroup.Size = new System.Drawing.Size(762, 200);
			this.plantGroup.TabIndex = 0;
			this.plantGroup.TabStop = false;
			this.plantGroup.Text = "PlantUML Options";
			this.plantGroup.ThemedBorder = null;
			this.plantGroup.ThemedFore = null;
			// 
			// plantUriLabel
			// 
			this.plantUriLabel.AutoSize = true;
			this.plantUriLabel.Location = new System.Drawing.Point(28, 136);
			this.plantUriLabel.Margin = new System.Windows.Forms.Padding(3, 10, 3, 0);
			this.plantUriLabel.Name = "plantUriLabel";
			this.plantUriLabel.Size = new System.Drawing.Size(386, 20);
			this.plantUriLabel.TabIndex = 11;
			this.plantUriLabel.Text = "PlantUML Server Address (leave blank to use default)";
			// 
			// plantUriBox
			// 
			this.plantUriBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.plantUriBox.Location = new System.Drawing.Point(32, 159);
			this.plantUriBox.Name = "plantUriBox";
			this.plantUriBox.ProcessEnterKey = false;
			this.plantUriBox.Size = new System.Drawing.Size(578, 26);
			this.plantUriBox.TabIndex = 10;
			this.plantUriBox.ThemedBack = null;
			this.plantUriBox.ThemedFore = null;
			// 
			// plantRemoveBox
			// 
			this.plantRemoveBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.plantRemoveBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.plantRemoveBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.plantRemoveBox.Location = new System.Drawing.Point(32, 97);
			this.plantRemoveBox.Name = "plantRemoveBox";
			this.plantRemoveBox.Size = new System.Drawing.Size(398, 25);
			this.plantRemoveBox.StylizeImage = false;
			this.plantRemoveBox.TabIndex = 2;
			this.plantRemoveBox.Text = "Remove PlantUML text and embed it in the image";
			this.plantRemoveBox.ThemedBack = null;
			this.plantRemoveBox.ThemedFore = null;
			this.plantRemoveBox.UseVisualStyleBackColor = true;
			this.plantRemoveBox.Click += new System.EventHandler(this.ToggleOnClick);
			// 
			// plantCollapseBox
			// 
			this.plantCollapseBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.plantCollapseBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.plantCollapseBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.plantCollapseBox.Location = new System.Drawing.Point(32, 67);
			this.plantCollapseBox.Name = "plantCollapseBox";
			this.plantCollapseBox.Size = new System.Drawing.Size(207, 25);
			this.plantCollapseBox.StylizeImage = false;
			this.plantCollapseBox.TabIndex = 1;
			this.plantCollapseBox.Text = "Collapse PlantUML text";
			this.plantCollapseBox.ThemedBack = null;
			this.plantCollapseBox.ThemedFore = null;
			this.plantCollapseBox.UseVisualStyleBackColor = true;
			this.plantCollapseBox.Click += new System.EventHandler(this.ToggleOnClick);
			// 
			// plantAfterBox
			// 
			this.plantAfterBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.plantAfterBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.plantAfterBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.plantAfterBox.Location = new System.Drawing.Point(32, 37);
			this.plantAfterBox.Name = "plantAfterBox";
			this.plantAfterBox.Size = new System.Drawing.Size(419, 25);
			this.plantAfterBox.StylizeImage = false;
			this.plantAfterBox.TabIndex = 0;
			this.plantAfterBox.Text = "Insert drawing after PlantUML text rather than before";
			this.plantAfterBox.ThemedBack = null;
			this.plantAfterBox.ThemedFore = null;
			this.plantAfterBox.UseVisualStyleBackColor = true;
			this.plantAfterBox.Click += new System.EventHandler(this.ToggleOnClick);
			// 
			// errorProvider
			// 
			this.errorProvider.ContainerControl = this;
			// 
			// ImagesSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Controls.Add(this.layoutPanel);
			this.Controls.Add(this.introBox);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Name = "ImagesSheet";
			this.Padding = new System.Windows.Forms.Padding(13, 8, 15, 10);
			this.Size = new System.Drawing.Size(800, 500);
			this.layoutPanel.ResumeLayout(false);
			this.resizeGroup.ResumeLayout(false);
			this.resizeGroup.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.widthBox)).EndInit();
			this.generalGroup.ResumeLayout(false);
			this.generalGroup.PerformLayout();
			this.plantGroup.ResumeLayout(false);
			this.plantGroup.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private UI.MoreMultilineLabel introBox;
		private System.Windows.Forms.Panel layoutPanel;
		private UI.MoreGroupBox plantGroup;
		private UI.MoreCheckBox plantCollapseBox;
		private UI.MoreCheckBox plantAfterBox;
		private UI.MoreGroupBox generalGroup;
		private System.Windows.Forms.Label imageViewerLabel;
		private UI.MoreTextBox imageViewerBox;
		private UI.MoreButton imageViewerButton;
		private UI.MoreGroupBox resizeGroup;
		private System.Windows.Forms.NumericUpDown widthBox;
		private System.Windows.Forms.Label widthLabel;
		private System.Windows.Forms.ErrorProvider errorProvider;
		private UI.MoreCheckBox plantRemoveBox;
		private System.Windows.Forms.Label plantUriLabel;
		private UI.MoreTextBox plantUriBox;
	}
}
