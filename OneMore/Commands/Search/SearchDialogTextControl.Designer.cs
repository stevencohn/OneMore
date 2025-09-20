namespace River.OneMoreAddIn.Commands
{
	partial class SearchDialogTextControl
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
			this.introLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.cancelButton = new River.OneMoreAddIn.UI.MoreButton();
			this.textLabel = new River.OneMoreAddIn.UI.MoreLabel();
			this.findBox = new River.OneMoreAddIn.UI.MoreTextBox();
			this.searchButton = new River.OneMoreAddIn.UI.MoreButton();
			this.resultsView = new River.OneMoreAddIn.UI.MoreListView();
			this.SuspendLayout();
			// 
			// introLabel
			// 
			this.introLabel.AutoSize = true;
			this.introLabel.Location = new System.Drawing.Point(19, 20);
			this.introLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 15);
			this.introLabel.Name = "introLabel";
			this.introLabel.Size = new System.Drawing.Size(579, 20);
			this.introLabel.TabIndex = 9;
			this.introLabel.Text = "Enter one or more keywords, \"quoted phrases\", and use uppercase AND and OR";
			this.introLabel.ThemedBack = null;
			this.introLabel.ThemedFore = null;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cancelButton.ImageOver = null;
			this.cancelButton.Location = new System.Drawing.Point(681, 576);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 15);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShowBorder = true;
			this.cancelButton.Size = new System.Drawing.Size(112, 35);
			this.cancelButton.StylizeImage = false;
			this.cancelButton.TabIndex = 10;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ThemedBack = null;
			this.cancelButton.ThemedFore = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// textLabel
			// 
			this.textLabel.AutoSize = true;
			this.textLabel.Location = new System.Drawing.Point(19, 72);
			this.textLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.textLabel.Name = "textLabel";
			this.textLabel.Size = new System.Drawing.Size(43, 20);
			this.textLabel.TabIndex = 11;
			this.textLabel.Text = "Text:";
			this.textLabel.ThemedBack = null;
			this.textLabel.ThemedFore = null;
			// 
			// findBox
			// 
			this.findBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.findBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.findBox.Location = new System.Drawing.Point(70, 70);
			this.findBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.findBox.Name = "findBox";
			this.findBox.ProcessEnterKey = false;
			this.findBox.Size = new System.Drawing.Size(554, 26);
			this.findBox.TabIndex = 12;
			this.findBox.ThemedBack = null;
			this.findBox.ThemedFore = null;
			// 
			// searchButton
			// 
			this.searchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.searchButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
			this.searchButton.Enabled = false;
			this.searchButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.searchButton.Image = global::River.OneMoreAddIn.Properties.Resources.m_Search;
			this.searchButton.ImageOver = null;
			this.searchButton.Location = new System.Drawing.Point(632, 66);
			this.searchButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 15);
			this.searchButton.Name = "searchButton";
			this.searchButton.ShowBorder = true;
			this.searchButton.Size = new System.Drawing.Size(68, 32);
			this.searchButton.StylizeImage = true;
			this.searchButton.TabIndex = 13;
			this.searchButton.ThemedBack = null;
			this.searchButton.ThemedFore = null;
			this.searchButton.UseVisualStyleBackColor = true;
			// 
			// resultsView
			// 
			this.resultsView.AllowItemReorder = false;
			this.resultsView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.resultsView.ControlPadding = 2;
			this.resultsView.FullRowSelect = true;
			this.resultsView.HideSelection = false;
			this.resultsView.HighlightBackground = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(193)))), ((int)(((byte)(255)))));
			this.resultsView.HighlightForeground = System.Drawing.SystemColors.HighlightText;
			this.resultsView.Location = new System.Drawing.Point(23, 120);
			this.resultsView.Name = "resultsView";
			this.resultsView.RowHeight = 24;
			this.resultsView.Size = new System.Drawing.Size(770, 448);
			this.resultsView.SortedBackground = System.Drawing.SystemColors.Window;
			this.resultsView.TabIndex = 14;
			this.resultsView.UseCompatibleStateImageBehavior = false;
			this.resultsView.View = System.Windows.Forms.View.Details;
			// 
			// SearchDialogTextControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.resultsView);
			this.Controls.Add(this.searchButton);
			this.Controls.Add(this.findBox);
			this.Controls.Add(this.textLabel);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.introLabel);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "SearchDialogTextControl";
			this.Padding = new System.Windows.Forms.Padding(15, 15, 15, 10);
			this.Size = new System.Drawing.Size(812, 636);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.MoreLabel introLabel;
		private UI.MoreButton cancelButton;
		private UI.MoreLabel textLabel;
		private UI.MoreTextBox findBox;
		private UI.MoreButton searchButton;
		private UI.MoreListView resultsView;
	}
}
