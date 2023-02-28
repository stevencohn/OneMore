namespace River.OneMoreAddIn.Commands
{
	partial class NavigatorWindow
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

				provider.Dispose();
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NavigatorWindow));
			this.controlPanel = new System.Windows.Forms.Panel();
			this.closeButton = new System.Windows.Forms.Button();
			this.mainContainer = new System.Windows.Forms.SplitContainer();
			this.pageBox = new System.Windows.Forms.ListBox();
			this.head1Panel = new System.Windows.Forms.Panel();
			this.head1Label = new System.Windows.Forms.Label();
			this.subContainer = new System.Windows.Forms.SplitContainer();
			this.pinnedBox = new System.Windows.Forms.ListBox();
			this.head2Panel = new System.Windows.Forms.Panel();
			this.unpinButton = new System.Windows.Forms.Button();
			this.head2Label = new System.Windows.Forms.Label();
			this.historyBox = new River.OneMoreAddIn.UI.MoreListView();
			this.head3Panel = new System.Windows.Forms.Panel();
			this.pinButton = new System.Windows.Forms.Button();
			this.head3Label = new System.Windows.Forms.Label();
			this.controlPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainContainer)).BeginInit();
			this.mainContainer.Panel1.SuspendLayout();
			this.mainContainer.Panel2.SuspendLayout();
			this.mainContainer.SuspendLayout();
			this.head1Panel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.subContainer)).BeginInit();
			this.subContainer.Panel1.SuspendLayout();
			this.subContainer.Panel2.SuspendLayout();
			this.subContainer.SuspendLayout();
			this.head2Panel.SuspendLayout();
			this.head3Panel.SuspendLayout();
			this.SuspendLayout();
			// 
			// controlPanel
			// 
			this.controlPanel.Controls.Add(this.closeButton);
			this.controlPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.controlPanel.Location = new System.Drawing.Point(0, 880);
			this.controlPanel.Name = "controlPanel";
			this.controlPanel.Padding = new System.Windows.Forms.Padding(0, 5, 10, 10);
			this.controlPanel.Size = new System.Drawing.Size(528, 64);
			this.controlPanel.TabIndex = 0;
			// 
			// closeButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = new System.Drawing.Point(407, 13);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(108, 38);
			this.closeButton.TabIndex = 2;
			this.closeButton.Text = "OK";
			this.closeButton.UseVisualStyleBackColor = true;
			this.closeButton.Click += new System.EventHandler(this.CloseOnClick);
			// 
			// mainContainer
			// 
			this.mainContainer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(143)))), ((int)(((byte)(32)))), ((int)(((byte)(199)))));
			this.mainContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainContainer.Location = new System.Drawing.Point(0, 0);
			this.mainContainer.Name = "mainContainer";
			this.mainContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// mainContainer.Panel1
			// 
			this.mainContainer.Panel1.BackColor = System.Drawing.SystemColors.Window;
			this.mainContainer.Panel1.Controls.Add(this.pageBox);
			this.mainContainer.Panel1.Controls.Add(this.head1Panel);
			this.mainContainer.Panel1MinSize = 80;
			// 
			// mainContainer.Panel2
			// 
			this.mainContainer.Panel2.Controls.Add(this.subContainer);
			this.mainContainer.Panel2MinSize = 100;
			this.mainContainer.Size = new System.Drawing.Size(528, 880);
			this.mainContainer.SplitterDistance = 291;
			this.mainContainer.TabIndex = 1;
			// 
			// pageBox
			// 
			this.pageBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pageBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pageBox.FormattingEnabled = true;
			this.pageBox.ItemHeight = 22;
			this.pageBox.Location = new System.Drawing.Point(0, 41);
			this.pageBox.Name = "pageBox";
			this.pageBox.Size = new System.Drawing.Size(528, 250);
			this.pageBox.TabIndex = 4;
			// 
			// head1Panel
			// 
			this.head1Panel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(179)))), ((int)(((byte)(218)))));
			this.head1Panel.Controls.Add(this.head1Label);
			this.head1Panel.Dock = System.Windows.Forms.DockStyle.Top;
			this.head1Panel.Location = new System.Drawing.Point(0, 0);
			this.head1Panel.Margin = new System.Windows.Forms.Padding(0);
			this.head1Panel.Name = "head1Panel";
			this.head1Panel.Size = new System.Drawing.Size(528, 41);
			this.head1Panel.TabIndex = 0;
			// 
			// head1Label
			// 
			this.head1Label.AutoSize = true;
			this.head1Label.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.head1Label.Location = new System.Drawing.Point(12, 9);
			this.head1Label.Name = "head1Label";
			this.head1Label.Size = new System.Drawing.Size(216, 22);
			this.head1Label.TabIndex = 0;
			this.head1Label.Text = "Current page title here";
			// 
			// subContainer
			// 
			this.subContainer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(143)))), ((int)(((byte)(32)))), ((int)(((byte)(199)))));
			this.subContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.subContainer.Location = new System.Drawing.Point(0, 0);
			this.subContainer.Name = "subContainer";
			this.subContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// subContainer.Panel1
			// 
			this.subContainer.Panel1.BackColor = System.Drawing.SystemColors.Window;
			this.subContainer.Panel1.Controls.Add(this.pinnedBox);
			this.subContainer.Panel1.Controls.Add(this.head2Panel);
			this.subContainer.Panel1MinSize = 80;
			// 
			// subContainer.Panel2
			// 
			this.subContainer.Panel2.BackColor = System.Drawing.SystemColors.Window;
			this.subContainer.Panel2.Controls.Add(this.historyBox);
			this.subContainer.Panel2.Controls.Add(this.head3Panel);
			this.subContainer.Panel2MinSize = 80;
			this.subContainer.Size = new System.Drawing.Size(528, 585);
			this.subContainer.SplitterDistance = 253;
			this.subContainer.TabIndex = 0;
			// 
			// pinnedBox
			// 
			this.pinnedBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pinnedBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pinnedBox.FormattingEnabled = true;
			this.pinnedBox.ItemHeight = 22;
			this.pinnedBox.Location = new System.Drawing.Point(0, 41);
			this.pinnedBox.Name = "pinnedBox";
			this.pinnedBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.pinnedBox.Size = new System.Drawing.Size(528, 212);
			this.pinnedBox.TabIndex = 3;
			// 
			// head2Panel
			// 
			this.head2Panel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(179)))), ((int)(((byte)(218)))));
			this.head2Panel.Controls.Add(this.unpinButton);
			this.head2Panel.Controls.Add(this.head2Label);
			this.head2Panel.Dock = System.Windows.Forms.DockStyle.Top;
			this.head2Panel.Location = new System.Drawing.Point(0, 0);
			this.head2Panel.Margin = new System.Windows.Forms.Padding(0);
			this.head2Panel.Name = "head2Panel";
			this.head2Panel.Padding = new System.Windows.Forms.Padding(0, 2, 5, 2);
			this.head2Panel.Size = new System.Drawing.Size(528, 41);
			this.head2Panel.TabIndex = 1;
			// 
			// unpinButton
			// 
			this.unpinButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.unpinButton.FlatAppearance.BorderSize = 0;
			this.unpinButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.unpinButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.unpinButton.Location = new System.Drawing.Point(481, 3);
			this.unpinButton.Name = "unpinButton";
			this.unpinButton.Size = new System.Drawing.Size(35, 37);
			this.unpinButton.TabIndex = 3;
			this.unpinButton.Text = "🗑";
			this.unpinButton.UseVisualStyleBackColor = true;
			// 
			// head2Label
			// 
			this.head2Label.AutoSize = true;
			this.head2Label.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.head2Label.Location = new System.Drawing.Point(12, 12);
			this.head2Label.Name = "head2Label";
			this.head2Label.Size = new System.Drawing.Size(73, 22);
			this.head2Label.TabIndex = 1;
			this.head2Label.Text = "Pinned";
			// 
			// historyBox
			// 
			this.historyBox.ControlPadding = 2;
			this.historyBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.historyBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.historyBox.FullRowSelect = true;
			this.historyBox.HideSelection = false;
			this.historyBox.HighlightBackground = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(213)))), ((int)(((byte)(228)))));
			this.historyBox.HighlightForeground = System.Drawing.SystemColors.HighlightText;
			this.historyBox.Location = new System.Drawing.Point(0, 41);
			this.historyBox.Name = "historyBox";
			this.historyBox.RowHeight = 29;
			this.historyBox.Size = new System.Drawing.Size(528, 287);
			this.historyBox.SortedBackground = System.Drawing.SystemColors.Window;
			this.historyBox.TabIndex = 2;
			this.historyBox.UseCompatibleStateImageBehavior = false;
			this.historyBox.View = System.Windows.Forms.View.Details;
			// 
			// head3Panel
			// 
			this.head3Panel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(205)))), ((int)(((byte)(179)))), ((int)(((byte)(218)))));
			this.head3Panel.Controls.Add(this.pinButton);
			this.head3Panel.Controls.Add(this.head3Label);
			this.head3Panel.Dock = System.Windows.Forms.DockStyle.Top;
			this.head3Panel.Location = new System.Drawing.Point(0, 0);
			this.head3Panel.Margin = new System.Windows.Forms.Padding(0);
			this.head3Panel.Name = "head3Panel";
			this.head3Panel.Padding = new System.Windows.Forms.Padding(5, 2, 5, 2);
			this.head3Panel.Size = new System.Drawing.Size(528, 41);
			this.head3Panel.TabIndex = 1;
			// 
			// pinButton
			// 
			this.pinButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.pinButton.FlatAppearance.BorderSize = 0;
			this.pinButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.pinButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pinButton.Location = new System.Drawing.Point(481, 2);
			this.pinButton.Name = "pinButton";
			this.pinButton.Size = new System.Drawing.Size(35, 35);
			this.pinButton.TabIndex = 2;
			this.pinButton.Text = "📌";
			this.pinButton.UseVisualStyleBackColor = true;
			// 
			// head3Label
			// 
			this.head3Label.AutoSize = true;
			this.head3Label.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.head3Label.Location = new System.Drawing.Point(17, 12);
			this.head3Label.Name = "head3Label";
			this.head3Label.Size = new System.Drawing.Size(74, 22);
			this.head3Label.TabIndex = 1;
			this.head3Label.Text = "History";
			// 
			// NavigatorWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.closeButton;
			this.ClientSize = new System.Drawing.Size(528, 944);
			this.Controls.Add(this.mainContainer);
			this.Controls.Add(this.controlPanel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(1000, 1500);
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(400, 300);
			this.Name = "NavigatorWindow";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "OneMore Navigator";
			this.Activated += new System.EventHandler(this.TopOnShown);
			this.Load += new System.EventHandler(this.PositionOnLoad);
			this.Shown += new System.EventHandler(this.TopOnShown);
			this.SizeChanged += new System.EventHandler(this.SetLimitsOnSizeChanged);
			this.Move += new System.EventHandler(this.RestrictOnMove);
			this.controlPanel.ResumeLayout(false);
			this.mainContainer.Panel1.ResumeLayout(false);
			this.mainContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainContainer)).EndInit();
			this.mainContainer.ResumeLayout(false);
			this.head1Panel.ResumeLayout(false);
			this.head1Panel.PerformLayout();
			this.subContainer.Panel1.ResumeLayout(false);
			this.subContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.subContainer)).EndInit();
			this.subContainer.ResumeLayout(false);
			this.head2Panel.ResumeLayout(false);
			this.head2Panel.PerformLayout();
			this.head3Panel.ResumeLayout(false);
			this.head3Panel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel controlPanel;
		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.SplitContainer mainContainer;
		private System.Windows.Forms.Panel head1Panel;
		private System.Windows.Forms.SplitContainer subContainer;
		private System.Windows.Forms.Panel head2Panel;
		private System.Windows.Forms.Panel head3Panel;
		private System.Windows.Forms.Label head1Label;
		private System.Windows.Forms.Label head2Label;
		private System.Windows.Forms.Label head3Label;
		private System.Windows.Forms.ListBox pinnedBox;
		private River.OneMoreAddIn.UI.MoreListView historyBox;
		private System.Windows.Forms.ListBox pageBox;
		private System.Windows.Forms.Button pinButton;
		private System.Windows.Forms.Button unpinButton;
	}
}