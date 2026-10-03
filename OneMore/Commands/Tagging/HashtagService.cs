//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable IDE0039 // Use local function

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;


	/// <summary>
	/// Background service to collect ##hashtags within content.
	/// This is a polling mechanism with specified throttling limits.
	/// </summary>
	/// <remarks>
	/// What is specific to hashtags lives in <see cref="HashtagStage"/>; this class owns the
	/// thread, cancellation, re-entrancy guard, and the loop and its error policy.
	/// </remarks>
	internal class HashtagService : Loggable
	{
		private readonly bool disabled;

		protected HashtagStage stage;
		protected int scanInterval;
		protected ThreadPriority threadPriority;
		protected CancellationTokenSource serviceToken;

		private int running; // re-entrancy guard, Interlocked access only


		/// <summary>
		/// Initialize a new instance for use by the OneMore add-in
		/// </summary>
		public HashtagService()
		{
			stage = new HashtagStage();
			stage.OnHashtagScanned += (sender, e) => OnHashtagScanned?.Invoke(this, e);

			scanInterval = stage.Interval;
			disabled = !stage.IsEnabled;

			threadPriority = ThreadPriority.Lowest;
		}


		/// <summary>
		/// Fired upon the completion of each full scan
		/// </summary>
		public event HashtagStage.HashtagScannedHandler OnHashtagScanned;


		/// <summary>
		/// Raise the OnHashtagScanned even.
		/// Available for inheritors, who are not allowed to invoke events directly.
		/// </summary>
		/// <param name="args"></param>
		protected void HashtagScanned(HashtagScannedEventArgs args)
		{
			OnHashtagScanned?.Invoke(this, args);
		}


		/// <summary>
		/// Start the service
		/// </summary>
		public void Startup()
		{
			if (disabled)
			{
				logger.WriteLine("Startup: hashtag service is disabled");
				return;
			}

			stage.Initialize();

			serviceToken = new CancellationTokenSource();
			Application.ApplicationExit += OnApplicationExit;

			// new thread to provide a bit of isolation
			var thread = new Thread(async () =>
			{
				await StartupLoop();
			})
			{
				Name = $"{nameof(HashtagService)}Thread"
			};

			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground = true;
			thread.Priority = threadPriority;
			thread.Start();
		}


		private void OnApplicationExit(object sender, EventArgs e)
		{
			logger.WriteLine("Shutdown: cancelling HashtagService on ApplicationExit");
			serviceToken?.Cancel();
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// This base method is executed in the context of the OneMore add-in running in OneNote.
		// Compare against the OneMoreTray override.

		protected virtual async Task StartupLoop()
		{
			logger.Debug("StartupLoop() WaitForReady()");
			if (!await stage.WaitForReady(serviceToken.Token))
			{
				CleanupToken();
				return;
			}

			// execute forever or until there are five consecutive errors...

			var errors = 0;
			while (errors < 5 && !serviceToken.IsCancellationRequested)
			{
				if (!stage.IsEnabled)
				{
					logger.WriteLine("hashtag service disabled by user, stopping");
					break;
				}

				try
				{
					await Scan(serviceToken.Token);
					errors = 0;
				}
				catch (OperationCanceledException)
				{
					break;
				}
				catch (Exception exc)
				{
					logger.WriteLine($"hashtag service exception {errors}", exc);
					errors++;
				}

				if (errors < 5 && !serviceToken.IsCancellationRequested)
				{
					try
					{
						await Task.Delay(scanInterval, serviceToken.Token);
					}
					catch (OperationCanceledException)
					{
						break;
					}
				}
			}

			CleanupToken();
			logger.WriteLine("Shutdown: hashtag service has stopped");
		}


		private void CleanupToken()
		{
			Application.ApplicationExit -= OnApplicationExit;
			serviceToken?.Dispose();
			serviceToken = null;

			stage.Release();
		}


		protected async Task Scan(CancellationToken token = default)
		{
			// guard against overlapping scans if the interval is shorter than a scan's duration
			if (Interlocked.CompareExchange(ref running, 1, 0) != 0)
			{
				logger.WriteLine("hashtag service: skipping scan, previous scan still in progress");
				return;
			}

			try
			{
				await stage.Run(token);
			}
			finally
			{
				Interlocked.Exchange(ref running, 0);
			}
		}
	}
}
