//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  Yada yada...
//************************************************************************************************

#pragma warning disable IDE1006 // Naming Styles

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	/// <summary>
	/// Displays a dialog with a progress bar and, optionally, a cancel button.
	/// </summary>
	/// <remarks>
	/// Can run in one of three modes. For simple cases, call the default constructor and the
	/// progress bar is shown with a message area; the consumer is responsible for setting the
	/// maximum increment value, incrementing it during its own processing, and then closing the
	/// dialog when it's work is complete.
	/// 
	/// It can also run with a timer; call the constructor with a max seconds timeout value, set
	/// the initial message, and call ShowTimedDialog along with a callback. A cancel button is
	/// displayed. The dialog closes when the callback completes and returns DialogResult.Cancel.
	/// It also closes when the cancel button is pressed and returns DialogResult.Cancel. If the
	/// timer reaches Maximum seconds then the dialog is closed and returns DialogResult.Abort.
	/// 
	/// The third mode accepts an execution action that is run in the background by the dialog.
	/// The consumer is responsible for increment the progress and updating the status message.
	/// A cancel button is displayed that, when pressed, sets the cancelltion token and returns
	/// DialogResult.Cancel. If the execute action completes without cancellation OK is returned.
	/// </remarks>
	internal partial class ProgressDialog : LocalizableForm
	{
		private const int SimpleHeight = 112;
		private const int CancelHeight = 144;

		private readonly CancellationTokenSource source;
		// Func<p1, p2, Task> is the async equivalent of Action<p1, p2>
		private readonly Func<ProgressDialog, CancellationToken, Task> execute;


		/// <summary>
		/// Initializes a new dialog with message area and a progress bar; no cancellation is
		/// allowed. The consumer is responsible for setting the maximum, incrementing, and
		/// setting messages as events occur.
		/// </summary>
		public ProgressDialog()
		{
			Initialize(SimpleHeight);

			cancelButton.Visible = false;
		}


		/// <summary>
		/// Initializes a new dialog with message area, progress bar, and cacel button
		/// that is used as a time-boxed controller and intended to be combined with the
		/// ShowTimedDialog method. The given action is responsible for changing the message.
		/// No need to invoke SetMaximum as this will be done by ShowTimedDialog.
		/// </summary>
		/// <param name="maxSeconds">
		/// The maximum allotted seconds before the dialog times out and DialogResult.Cancel
		/// is returned
		/// </param>
		public ProgressDialog(int maxSeconds)
		{
			Initialize(CancelHeight);
			source = new CancellationTokenSource();
			SetMaximum(maxSeconds);
		}


		/// <summary>
		/// Initializes a new dialog with message area, progress bar, and a cancel button.
		/// Dialog should be started using RunModeless
		/// </summary>
		/// <param name="action">
		/// A callback invoked by the dialog and that contains the logic to execute. This action
		/// is passed a reference to the dialog so it can update its message and increment. The
		/// action should also periodically check the cancellation token and abort when true.
		/// </param>
		public ProgressDialog(Func<ProgressDialog, CancellationToken, Task> action)
		{
			Initialize(CancelHeight);

			source = new CancellationTokenSource();
			execute = action;
		}


		private void Initialize(int height)
		{
			InitializeComponent();

			(_, float factorY) = UIHelper.GetScalingFactors();
			Height = (int)Math.Round(height * factorY);

			if (NeedsLocalizing())
			{
				Text = Resx.ProgramName;

				Localize(new string[]
				{
					"cancelButton=word_Cancel"
				});
			}
		}


		/// <summary>
		/// Shows the progress dialog with a timed progression and a callback action to
		/// invoke on a secondary thread.
		/// </summary>
		/// <param name="owner">The owner window used to center this dialog</param>
		/// <param name="action">The callback method to invoke</param>
		/// <returns></returns>
		public DialogResult ShowTimedDialog(
			IWin32Window owner, Func<ProgressDialog, CancellationToken, Task<bool>> action)
		{
			timer.Tick += Tick;
			StartTimer();

			DialogResult result = DialogResult.Cancel;

			try
			{
				// process should run in an STA thread otherwise it will conflict with
				// the OneNote MTA thread environment
				var thread = new Thread(async () =>
				{
					logger.WriteLine("running action...");
					var ok = await action(this, source.Token);
					logger.WriteLine($"completed action ({ok})");

					DialogResult = source.IsCancellationRequested 
						? DialogResult.Abort
						: ok ? DialogResult.OK : DialogResult.Cancel;

					timer.Stop();
					Close();
				});

				thread.SetApartmentState(ApartmentState.STA);
				thread.IsBackground = true;
				thread.Start();

				result = ShowDialog(owner);

				if (result == DialogResult.Cancel)
				{
					logger.WriteLine("clicked cancel");
					source.Cancel();
					thread.Abort();
					return result;
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error importing", exc);
			}

			return result;
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		/// <summary>
		/// Called after Show()
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (execute == null)
			{
				StartPosition = FormStartPosition.Manual;
				TopMost = true;

				var rect = new Native.Rectangle();
				using (var one = new OneNote())
				{
					Native.GetWindowRect(one.WindowHandle, ref rect);
				}

				var yoffset = (int)(Height * 20 / 100.0);

				Location = new System.Drawing.Point(
					(rect.Left + ((rect.Right - rect.Left) / 2)) - (Width / 2),
					(rect.Top + ((rect.Bottom - rect.Top) / 2)) - (Height / 2) - yoffset
					);
			}
		}


		/// <summary>
		/// Called after RunModeless()
		/// </summary>
		/// <param name="e"></param>
		protected override async void OnShown(EventArgs e)
		{
			base.OnShown(e);

			if (execute != null)
			{
				await Task.Factory.StartNew(async () =>
				{
					try
					{
						await execute(this, source.Token);
						DialogResult = DialogResult.OK;
						logger.WriteLine("progress dialog completed");
					}
					catch (Exception exc)
					{
						DialogResult = DialogResult.Cancel;
						//logger.WriteLine("error executing work", exc);
						logger.WriteLine(exc.Message, exc);
					}

					Close();
				});
			}
		}


		private void Tick(object sender, EventArgs e)
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
				Invoke(new Action(() => Tick(sender, e)));
			}

			Increment();

			if (progressBar.Value >= progressBar.Maximum)
			{
				timer.Stop();
				source.Cancel();
				DialogResult = DialogResult.Abort;
			}
		}


		private void Cancel(object sender, EventArgs e)
		{
			if (timer.Enabled)
			{
				timer.Stop();
			}

			source.Cancel();

			DialogResult = DialogResult.Cancel;
			Close();
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		/// <summary>
		/// Increments the progress bar
		/// </summary>
		public void Increment()
		{
			if (progressBar.Value < progressBar.Maximum)
			{
				progressBar.Value++;
			}
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
		/// Starts the second countdown timer, up to Maximum seconds.
		/// If Maximum seconds ellapse then the dialog is closed and DialogResult is set to Abort.
		/// </summary>
		public void StartTimer()
		{
			timer.Start();
		}
	}
}
