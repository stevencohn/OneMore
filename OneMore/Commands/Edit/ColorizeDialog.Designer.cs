namespace River.OneMoreAddIn.Commands
{
	partial class ColorizeDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ColorizeDialog));
			this.controlPanel = new System.Windows.Forms.Panel();
			this.okButton = new River.OneMoreAddIn.UI.MoreButton();
			this.mainPanel = new System.Windows.Forms.Panel();
			this.view = new System.Windows.Forms.ListView();
			this.nameColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.controlPanel.SuspendLayout();
			this.mainPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// controlPanel
			// 
			this.controlPanel.BackColor = System.Drawing.SystemColors.ControlLight;
			this.controlPanel.Controls.Add(this.okButton);
			this.controlPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.controlPanel.Location = new System.Drawing.Point(0, 516);
			this.controlPanel.Name = "controlPanel";
			this.controlPanel.Size = new System.Drawing.Size(373, 62);
			this.controlPanel.TabIndex = 0;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.okButton.ImageOver = null;
			this.okButton.Location = new System.Drawing.Point(253, 13);
			this.okButton.Name = "okButton";
			this.okButton.ShowBorder = true;
			this.okButton.Size = new System.Drawing.Size(108, 38);
			this.okButton.StylizeImage = false;
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.ThemedBack = null;
			this.okButton.ThemedFore = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.KeyUp += new System.Windows.Forms.KeyEventHandler(this.CancelOnKeyUp);
			// 
			// mainPanel
			// 
			this.mainPanel.BackColor = System.Drawing.SystemColors.ControlLight;
			this.mainPanel.Controls.Add(this.view);
			this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainPanel.Location = new System.Drawing.Point(0, 0);
			this.mainPanel.Name = "mainPanel";
			this.mainPanel.Padding = new System.Windows.Forms.Padding(10);
			this.mainPanel.Size = new System.Drawing.Size(373, 516);
			this.mainPanel.TabIndex = 1;
			// 
			// view
			// 
			this.view.BackColor = System.Drawing.SystemColors.Window;
			this.view.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameColumn});
			this.view.Dock = System.Windows.Forms.DockStyle.Fill;
			this.view.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.view.FullRowSelect = true;
			this.view.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.view.HideSelection = false;
			this.view.Location = new System.Drawing.Point(10, 10);
			this.view.Name = "view";
			this.view.Size = new System.Drawing.Size(353, 496);
			this.view.TabIndex = 0;
			this.view.UseCompatibleStateImageBehavior = false;
			this.view.View = System.Windows.Forms.View.Details;
			this.view.DoubleClick += new System.EventHandler(this.SelectOnDoubleClick);
			this.view.KeyUp += new System.Windows.Forms.KeyEventHandler(this.CancelOnKeyUp);
			// 
			// nameColumn
			// 
			this.nameColumn.Width = 300;
			// 
			// ColorizeDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(373, 578);
			this.Controls.Add(this.mainPanel);
			this.Controls.Add(this.controlPanel);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "ColorizeDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Select Language";
			this.Activated += new System.EventHandler(this.FocusOnActivated);
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.CancelOnKeyUp);
			this.controlPanel.ResumeLayout(false);
			this.mainPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel controlPanel;
		private UI.MoreButton okButton;
		private System.Windows.Forms.Panel mainPanel;
		private System.Windows.Forms.ListView view;
		private System.Windows.Forms.ColumnHeader nameColumn;
	}
}