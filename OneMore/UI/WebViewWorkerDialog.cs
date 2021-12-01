
#pragma warning disable CS3001 // Argument type is not CLS-compliant

namespace River.OneMoreAddIn.UI
{
	using Microsoft.Web.WebView2.Core;
	using Microsoft.Web.WebView2.WinForms;
	using System;
	using System.IO;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


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
	public partial class WebViewWorkerDialog : Form
	{
		private readonly WebViewWorker startup;
		private readonly WebViewWorker work;


		/// <summary>
		/// Not called directly
		/// </summary>
		public WebViewWorkerDialog()
		{
			InitializeComponent();
		}


		/// <summary>
		/// Initializes a new dialog with the given startup and worker routines
		/// </summary>
		/// <param name="startup">The consumer provider startup operation</param>
		/// <param name="work">The consumer provided worker operation</param>
		public WebViewWorkerDialog(WebViewWorker startup, WebViewWorker work)
			: this()
		{
			this.startup = startup;
			this.work = work;
		}


		// Form.Loaded
		private async void StartWorkLoaded(object sender, EventArgs e)
		{
			Logger.Current.WriteLine("start, creating env");

			var env = await CoreWebView2Environment.CreateAsync(null,
				Path.Combine(PathFactory.GetAppDataPath(), Resx.ProgramName));

			Logger.Current.WriteLine($"start, userdata = {env.UserDataFolder}");

			try
			{
				await webView.EnsureCoreWebView2Async(env);
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine(exc.Message, exc);
			}

			Logger.Current.WriteLine("start, running startup");
			await startup(webView);

			Logger.Current.WriteLine("start, done");
		}


		// WebView2.NavigationComplete
		private async void WorkNavComplete(object sender, CoreWebView2NavigationCompletedEventArgs e)
		{
			Logger.Current.WriteLine("navcompleted, working...");
			await work(webView);

			Logger.Current.WriteLine("navcompleted, work done, closing");
			Close();
		}
	}
}
