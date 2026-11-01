//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;


	/// <summary>
	/// Runs a background polling loop on a pool thread that survives errors. Consecutive
	/// failures back off up to a ceiling rather than ending the loop, and the loop stops
	/// when the application exits or when <see cref="StopAll"/> is called at shutdown.
	/// </summary>
	internal static class ServiceLoop
	{
		private const int MaxBackoff = 60000;

		private static readonly object gate = new();
		private static readonly List<CancellationTokenSource> sources = new();


		/// <summary>
		/// Cancels every running loop. ApplicationExit is not reliably raised when the add-in
		/// is hosted in the dllhost surrogate, so a loop holding a machine-wide resource such
		/// as the command pipe would otherwise outlive OneNote and block the next instance.
		/// </summary>
		public static void StopAll()
		{
			CancellationTokenSource[] stopping;
			lock (gate)
			{
				stopping = sources.ToArray();
				sources.Clear();
			}

			foreach (var source in stopping)
			{
				source.Cancel();
			}
		}


		/// <summary>
		/// Start the loop. The body runs, then waits for the interval; if the body throws,
		/// the wait doubles for each consecutive failure up to a ceiling, and resets to the
		/// normal interval after the next success.
		/// </summary>
		/// <param name="name">The service name, for logging</param>
		/// <param name="logger">The logger of the owning service</param>
		/// <param name="interval">The delay in ms between iterations</param>
		/// <param name="body">The work for one iteration</param>
		public static void Start(
			string name, ILogger logger, Func<int> interval, Func<CancellationToken, Task> body)
		{
			var cts = new CancellationTokenSource();

			lock (gate)
			{
				sources.Add(cts);
			}

			void OnExit(object sender, EventArgs e)
			{
				Application.ApplicationExit -= OnExit;
				lock (gate)
				{
					sources.Remove(cts);
				}

				cts.Cancel();
			}

			Application.ApplicationExit += OnExit;

			_ = Task.Run(async () =>
			{
				var errors = 0;
				var token = cts.Token;

				try
				{
					while (!token.IsCancellationRequested)
					{
						var wait = interval();

						try
						{
							await body(token);
							errors = 0;
						}
						catch (OperationCanceledException) when (token.IsCancellationRequested)
						{
							break;
						}
						catch (Exception exc)
						{
							errors++;
							logger.WriteLine($"{name} exception {errors}", exc);

							// back off exponentially, capped
							var factor = 1 << Math.Min(errors, 6);
							wait = (int)Math.Min((long)Math.Max(wait, 500) * factor, MaxBackoff);
						}

						await Task.Delay(wait, token);
					}
				}
				catch (OperationCanceledException)
				{
					// normal shutdown
				}
				catch (Exception exc)
				{
					logger.WriteLine($"{name} loop terminated unexpectedly", exc);
				}

				logger.WriteLine($"{name} has stopped");
			});
		}
	}
}
