//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable CS3001 // Argument type is not CLS-compliant

namespace River.OneMoreAddIn.UI
{
	using Microsoft.Web.WebView2.Core;
	using Microsoft.Web.WebView2.WinForms;
	using System;
	using System.IO;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Describes the signature of the startup and worker delegates passed to the
	/// WebViewWorkerDialog constructor
	/// </summary>
	/// <param name="webview">The WebView2 passed into the worker</param>
	/// <returns>A Task of bool</returns>
	public delegate Task<bool> WebViewWorker(WebView2 webview);


	/// <summary>
	/// When run from SingleThreaded.Invoke, this provides an STA thread with a message
	/// pump from which to run operations on the hosted WebView2. This dialog has an
	/// opacity set to 0% so it is hidden from the user and works in the background.
	/// </summary>
	internal partial class WebViewDialog : MoreForm
	{
		private readonly WebViewWorker startup;
		private readonly WebViewWorker work;


		/// <summary>
		/// Not called directly
		/// </summary>
		public WebViewDialog()
		{
			InitializeComponent();
		}


		/// <summary>
		/// Create a 100% opaque dialog for displaying the given Uri
		/// </summary>
		/// <param name="uri"></param>
		public WebViewDialog(Uri uri)
			: this()
		{
			Opacity = 1.0;
			ManualLocation = true;
			StartPosition = FormStartPosition.Manual;

			startup = new WebViewWorker(async (webview) =>
			{
				//logger.WriteLine($"starting up webview with {uri}");
				webview.Source = uri;
				await Task.Yield();
				return true;
			});
		}


		/// <summary>
		/// Initializes a new dialog with the given startup and worker routines
		/// </summary>
		/// <param name="startup">The consumer provided startup operation</param>
		/// <param name="work">The consumer provided worker operation</param>
		public WebViewDialog(WebViewWorker startup, WebViewWorker work)
			: this()
		{
			this.startup = startup;
			this.work = work;
		}


		// Form.Loaded
		private async void StartWorkLoaded(object sender, EventArgs e)
		{
			try
			{
				var env = await CoreWebView2Environment.CreateAsync(null,
					Path.Combine(PathHelper.GetAppDataPath(), Resx.ProgramName));

				await webView.EnsureCoreWebView2Async(env);

				if (startup is not null)
				{
					await startup(webView);
				}
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine(exc.Message, exc);
			}
		}


		// WebView2.NavigationComplete
		private async void WorkNavComplete(
			object sender, CoreWebView2NavigationCompletedEventArgs e)
		{
			if (e.IsSuccess)
			{
				if (work is not null)
				{
					await work(webView);
					Close();
				}
			}
			else
			{
				Logger.Current.WriteLine(
					$"WebNavComplete error HttpStatusCode:{e.HttpStatusCode} " +
					$"WebErrorStatus:{e.WebErrorStatus}");
			}
		}
	}
}
