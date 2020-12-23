//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  Yada yada...
//************************************************************************************************

#pragma warning disable IDE1006 // Naming Styles

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.ComponentModel;
	using System.Threading;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	/// <summary>
	/// Displays a dialog with a progress bar and, optionally, a cancel button.
	/// </summary>
	/// <remarks>
	/// Can run in one of two modes. For simple cases, call the default constructor and the
	/// progress bar is shown with a message area; the consumer is responsible for setting the maximum
	/// increment value, incrementing it during its own processing, and then closing the dialog when
	/// it's work is complete.
	/// 
	/// It can also run with a timer. call the constructor that accepts a CancellationTokenSource and
	/// call StartTimer when ready. A cancel button is displayed. The dialog closes when a cancellation
	/// is pending and returns DialogResult.OK. It also closes when the cancel button is pressed and returns
	/// DialogResult.Cancel. If the timer reaches Maximum seconds then the dialog is closed and returns
	/// DialogResult.Abort.
	/// </remarks>
	internal partial class ProgressDialog : LocalizableForm
	{
		private readonly CancellationTokenSource source;
		private Action<ProgressDialog, CancellationToken> execute;


		/// <summary>
		/// Initializes a UI with message area, progress bar, and cancel button. When the cancel
		/// button is clicked, the source is marked as IsCancellationPending
		/// </summary>
		/// <param name="source">A cancellation source that can be used to cancel the active work</param>
		public ProgressDialog(CancellationTokenSource source = null)
		{
			InitializeComponent();
			Localize();

			if (source == null)
			{
				cancelButton.Visible = false;
				Height = 112;
			}
			else
			{
				Height = 144;
				this.source = source;
				timer.Tick += Timer_Tick;
			}

			(_, float factorY) = UIHelper.GetScalingFactors();
			Height = (int)Math.Round(Height * factorY);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="execute"></param>
		public ProgressDialog(Action<ProgressDialog, CancellationToken> execute)
		{
			InitializeComponent();
			TopMost = true;
			ShowInTaskbar = true;

			Localize();

			(_, float factorY) = UIHelper.GetScalingFactors();
			Height = (int)Math.Round(144 * factorY);

			source = new CancellationTokenSource();

			this.execute = execute;
		}


		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			var worker = new BackgroundWorker();
			worker.DoWork += Worker_DoWork;
			worker.RunWorkerAsync();
		}

		private void Worker_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				execute(this, source.Token);
				DialogResult = DialogResult.OK;
			}
			catch (Exception exc)
			{
				logger.WriteLine("error executing work", exc);
				DialogResult = DialogResult.Cancel;
			}

			Close();
		}

		private void Localize()
		{
			if (NeedsLocalizing())
			{
				Text = Resx.ProgressDialog_Text;

				Localize(new string[]
				{
					"cancelButton"
				});
			}
		}


		/// <summary>
		/// Increments the progress bar by one step.
		/// </summary>
		/// <param name="step"></param>
		public void Increment(int step = 1)
		{
			progressBar.Value += step;
		}


		/// <summary>
		/// Sets the maximum increment value
		/// </summary>
		public void SetMaximum(int value)
		{
			progressBar.Maximum = value;
			progressBar.Value = 0;
		}


		/// <summary>
		/// Sets the message displayed
		/// </summary>
		public void SetMessage(string value)
		{
			messageLabel.Text = value;
			messageLabel.Refresh();
		}


		/// <summary>
		/// Starts the second countdown timer, up to Maximum seconds. If Maximum seconds ellapse then
		/// the dialog is closed and DialogResult is set to Abort.
		/// </summary>
		public void StartTimer()
		{
			timer.Start();
		}


		private void Timer_Tick(object sender, EventArgs e)
		{
			if (source.Token.IsCancellationRequested)
			{
				timer.Stop();
				DialogResult = DialogResult.OK;
				Close();
				return;
			}

			if (InvokeRequired)
			{
				Invoke(new Action(() => Timer_Tick(sender, e)));
			}

			Increment();

			if (progressBar.Value >= progressBar.Maximum)
			{
				timer.Stop();
				source.Cancel();
				DialogResult = DialogResult.Abort;
			}
		}


		private void cancelButton_Click(object sender, EventArgs e)
		{
			if (timer.Enabled)
			{
				timer.Stop();
			}

			source.Cancel();

			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
