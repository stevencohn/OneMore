namespace River.OneMoreAddIn.Commands
{
	partial class RemoveDuplicatesNavigator
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RemoveDuplicatesNavigator));
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.view = new System.Windows.Forms.ListView();
			this.pageColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.textRatingColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.xmlRatingColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.distanceColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.trashColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(595, 372);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(100, 38);
			this.okButton.TabIndex = 9;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(701, 372);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(100, 38);
			this.cancelButton.TabIndex = 8;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// view
			// 
			this.view.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.view.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.pageColumn,
            this.textRatingColumn,
            this.xmlRatingColumn,
            this.distanceColumn,
            this.trashColumn});
			this.view.HideSelection = false;
			this.view.Location = new System.Drawing.Point(12, 35);
			this.view.MultiSelect = false;
			this.view.Name = "view";
			this.view.Size = new System.Drawing.Size(789, 331);
			this.view.TabIndex = 10;
			this.view.UseCompatibleStateImageBehavior = false;
			this.view.View = System.Windows.Forms.View.Details;
			// 
			// pageColumn
			// 
			this.pageColumn.Text = "Page";
			this.pageColumn.Width = 400;
			// 
			// textRatingColumn
			// 
			this.textRatingColumn.Text = "Text Rating";
			this.textRatingColumn.Width = 80;
			// 
			// xmlRatingColumn
			// 
			this.xmlRatingColumn.Text = "XML Rating";
			this.xmlRatingColumn.Width = 80;
			// 
			// distanceColumn
			// 
			this.distanceColumn.Text = "Distance";
			this.distanceColumn.Width = 80;
			// 
			// trashColumn
			// 
			this.trashColumn.Text = "";
			this.trashColumn.Width = 40;
			// 
			// RemoveDuplicatesNavigator
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(813, 422);
			this.Controls.Add(this.view);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "RemoveDuplicatesNavigator";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Remove Duplicate Pages";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.ListView view;
		private System.Windows.Forms.ColumnHeader pageColumn;
		private System.Windows.Forms.ColumnHeader textRatingColumn;
		private System.Windows.Forms.ColumnHeader xmlRatingColumn;
		private System.Windows.Forms.ColumnHeader distanceColumn;
		private System.Windows.Forms.ColumnHeader trashColumn;
	}
}