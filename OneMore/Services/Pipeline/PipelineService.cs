//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Pipeline
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;


	/// <summary>
	/// How a pipeline runs its stages.
	/// </summary>
	internal enum PipelineMode
	{
		/// <summary>Keep cycling until cancelled, as the add-in does.</summary>
		Continuous,

		/// <summary>Run each stage until it has succeeded once, then stop, as the tray does.</summary>
		OneShot
	}


	/// <summary>
	/// A background host that runs a list of stages in order, over and over. It owns the
	/// thread, cancellation, and the error policy, and knows nothing about what the stages do.
	/// </summary>
	/// <remarks>
	/// Each cycle visits the stages in order and runs the ones that are enabled, ready, and
	/// due. Every stage has its own interval, and the host wakes at the shortest of them, so a
	/// slow stage never slows a fast one. A stage that throws does not stop the others; one
	/// that fails several cycles in a row is switched off for the rest of the session.
	/// </remarks>
	internal sealed class PipelineService : Loggable
	{
		// let OneMore settle before the first cycle
		internal const int SettleDelay = 3000;

		// how soon to ask again when a stage reports that it is not ready
		internal const int NotReadyDelay = 10000;

		// how often to look around when every stage is disabled
		internal const int IdleDelay = 120000;

		// no stage may spin faster than this
		internal const int MinimumInterval = 1000;

		internal const int MaxFailures = 5;

		private sealed class StageState
		{
			public DateTime? LastRun;
			public int Failures;
			public bool Off;
			public bool Completed;
			public bool? Enabled;
		}

		private readonly IReadOnlyList<IPipelineStage> stages;
		private readonly Dictionary<IPipelineStage, StageState> states = new();
		private readonly Func<DateTime> clock;
		private readonly Func<TimeSpan, CancellationToken, Task> delay;
		private CancellationTokenSource serviceToken;
		private int running; // re-entrancy guard, Interlocked access only


		/// <summary>
		/// Initialize a new instance that runs the stages in the order given.
		/// </summary>
		public PipelineService(IEnumerable<IPipelineStage> stages)
			: this(stages, () => DateTime.Now, Task.Delay)
		{
		}


		/// <summary>
		/// Initialize a new instance with its own clock and delay, so that time can be
		/// simulated.
		/// </summary>
		internal PipelineService(
			IEnumerable<IPipelineStage> stages,
			Func<DateTime> clock,
			Func<TimeSpan, CancellationToken, Task> delay)
		{
			this.stages = stages.ToList();
			this.clock = clock;
			this.delay = delay;

			foreach (var stage in this.stages)
			{
				states[stage] = new StageState();
			}
		}


		/// <summary>
		/// Gets or sets how the stages are run. The default is continuous.
		/// </summary>
		public PipelineMode Mode { get; set; } = PipelineMode.Continuous;


		/// <summary>
		/// Gets or sets the priority of the background thread.
		/// </summary>
		public ThreadPriority ThreadPriority { get; set; } = ThreadPriority.Lowest;


		/// <summary>
		/// Start the service on its own background thread.
		/// </summary>
		public void Startup()
		{
			InitializeStages();

			serviceToken = new CancellationTokenSource();
			Application.ApplicationExit += OnApplicationExit;

			// new thread to provide a bit of isolation
			var thread = new Thread(async () =>
			{
				await Run(serviceToken.Token);
			})
			{
				Name = $"{nameof(PipelineService)}Thread"
			};

			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground = true;
			thread.Priority = ThreadPriority;
			thread.Start();
		}


		/// <summary>
		/// Gives every stage its one chance to prepare before the first cycle.
		/// </summary>
		internal void InitializeStages()
		{
			foreach (var stage in stages)
			{
				stage.Initialize();
			}
		}


		private void OnApplicationExit(object sender, EventArgs e)
		{
			logger.WriteLine("Shutdown: cancelling PipelineService on ApplicationExit");
			serviceToken?.Cancel();
		}


		/// <summary>
		/// Runs cycles until cancelled or, in one-shot mode, until every stage is done.
		/// </summary>
		internal async Task Run(CancellationToken token)
		{
			try
			{
				// wait at least once to let OneMore settle before we start
				await delay(TimeSpan.FromMilliseconds(SettleDelay), token);

				while (!token.IsCancellationRequested)
				{
					var next = await RunCycle(token);

					if (Mode == PipelineMode.OneShot && AllDone())
					{
						break;
					}

					await delay(next, token);
				}
			}
			catch (OperationCanceledException)
			{
				logger.Verbose("PipelineService canceled");
			}
			finally
			{
				Cleanup();
			}

			logger.WriteLine("Shutdown: pipeline service has stopped");
		}


		private void Cleanup()
		{
			Application.ApplicationExit -= OnApplicationExit;
			serviceToken?.Dispose();
			serviceToken = null;

			foreach (var stage in stages)
			{
				stage.Dispose();
			}
		}


		/// <summary>
		/// Visits every stage once, running those that are enabled, ready, and due.
		/// </summary>
		/// <returns>How long to wait before the next cycle</returns>
		internal async Task<TimeSpan> RunCycle(CancellationToken token)
		{
			// guard against overlapping cycles
			if (Interlocked.CompareExchange(ref running, 1, 0) != 0)
			{
				logger.WriteLine("pipeline service: skipping cycle, previous cycle still in progress");
				return NextDelay(waiting: false);
			}

			try
			{
				var context = new PipelineContext();
				var waiting = false;

				foreach (var stage in stages)
				{
					var state = states[stage];

					if (!NoteEnabled(stage, state) || state.Off)
					{
						continue;
					}

					if (Mode == PipelineMode.OneShot && state.Completed)
					{
						continue;
					}

					if (!IsDue(stage, state))
					{
						continue;
					}

					if (!await RunStage(stage, state, context, token))
					{
						waiting = true;
					}
				}

				return NextDelay(waiting);
			}
			finally
			{
				Interlocked.Exchange(ref running, 0);
			}
		}


		// records a change in whether the user has a stage enabled, and says whether it is
		private bool NoteEnabled(IPipelineStage stage, StageState state)
		{
			var enabled = stage.IsEnabled;

			if (state.Enabled != enabled)
			{
				// a stage that starts enabled is not news
				if (state.Enabled is not null || !enabled)
				{
					logger.WriteLine(
						$"pipeline stage '{stage.Name}' {(enabled ? "enabled" : "disabled")}");
				}

				state.Enabled = enabled;
			}

			return enabled;
		}


		private bool IsDue(IPipelineStage stage, StageState state)
		{
			return state.LastRun is null ||
				clock() - state.LastRun.Value >= TimeSpan.FromMilliseconds(Interval(stage));
		}


		private static int Interval(IPipelineStage stage)
		{
			return Math.Max(stage.Interval, MinimumInterval);
		}


		// returns false if the stage is not ready
		private async Task<bool> RunStage(
			IPipelineStage stage, StageState state, PipelineContext context, CancellationToken token)
		{
			try
			{
				if (!await stage.IsReady(token))
				{
					return false;
				}

				var watch = Stopwatch.StartNew();
				await stage.Run(context, token);

				state.Failures = 0;
				state.Completed = true;
				logger.Verbose($"pipeline stage '{stage.Name}' ran in {watch.ElapsedMilliseconds}ms");
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception exc)
			{
				state.Failures++;
				logger.WriteLine($"pipeline stage '{stage.Name}' exception {state.Failures}", exc);

				if (state.Failures >= MaxFailures)
				{
					state.Off = true;
					logger.WriteLine(
						$"pipeline stage '{stage.Name}' has stopped after {MaxFailures} " +
						"consecutive errors; check for exceptions above");
				}
			}

			// a failed stage is tried again after its interval, not immediately
			state.LastRun = clock();
			return true;
		}


		// the host wakes at the shortest interval among the stages that can run, but sooner
		// than that when a stage is waiting to become ready
		private TimeSpan NextDelay(bool waiting)
		{
			var intervals = stages
				.Where(s => !states[s].Off && states[s].Enabled == true)
				.Select(Interval)
				.ToList();

			var next = intervals.Count > 0 ? intervals.Min() : IdleDelay;
			if (waiting)
			{
				next = Math.Min(next, NotReadyDelay);
			}

			return TimeSpan.FromMilliseconds(next);
		}


		// in one-shot mode: nothing is left to do for any stage
		private bool AllDone()
		{
			return stages.All(s =>
				states[s].Off || states[s].Completed || states[s].Enabled == false);
		}
	}
}
