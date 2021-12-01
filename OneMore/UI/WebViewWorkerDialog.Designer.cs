namespace River.OneMoreAddIn.UI
{
	partial class WebViewWorkerDialog
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
				webView.Dispose();
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
			this.webView = new Microsoft.Web.WebView2.WinForms.WebView2();
			((System.ComponentModel.ISupportInitialize)(this.webView)).BeginInit();
			this.SuspendLayout();
			// 
			// webView
			// 
			this.webView.CreationProperties = null;
			this.webView.DefaultBackgroundColor = System.Drawing.Color.White;
			this.webView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.webView.Location = new System.Drawing.Point(10, 10);
			this.webView.Name = "webView";
			this.webView.Size = new System.Drawing.Size(580, 278);
			this.webView.TabIndex = 0;
			this.webView.ZoomFactor = 1D;
			this.webView.NavigationCompleted += new System.EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs>(this.WorkNavComplete);
			// 
			// WebViewWorkerDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(600, 298);
			this.Controls.Add(this.webView);
			this.Name = "WebViewWorkerDialog";
			this.Opacity = 0D;
			this.Padding = new System.Windows.Forms.Padding(10);
			this.Text = "WebViewWorkerDialog";
			this.Load += new System.EventHandler(this.StartWorkLoaded);
			((System.ComponentModel.ISupportInitialize)(this.webView)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Microsoft.Web.WebView2.WinForms.WebView2 webView;
	}
}