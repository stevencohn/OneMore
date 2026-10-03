//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Pipeline
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Pipeline;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;


	/// <summary>
	/// The host is tested with fake stages and a simulated clock, so nothing here waits.
	/// </summary>
	[TestClass]
	public class PipelineServiceTests
	{
		private static readonly TimeSpan TwoMinutes = TimeSpan.FromMinutes(2);

		private DateTime now;
		private List<TimeSpan> delays;
		private List<string> ran;
		private CancellationTokenSource cancel;


		private sealed class FakeStage : IPipelineStage
		{
			private readonly List<string> ran;

			public FakeStage(string name, List<string> ran, int interval = 120000)
			{
				Name = name;
				Interval = interval;
				this.ran = ran;
			}

			public string Name { get; }
			public int Interval { get; set; }
			public bool Enabled { get; set; } = true;
			public bool Ready { get; set; } = true;
			public Func<PipelineContext, int, Task> Work { get; set; }
			public int Runs { get; private set; }
			public bool Initialized { get; private set; }
			public bool Disposed { get; private set; }

			public bool IsEnabled => Enabled;
			public void Initialize() => Initialized = true;
			public Task<bool> IsReady(CancellationToken token) => Task.FromResult(Ready);
			public void Dispose() => Disposed = true;

			public async Task Run(PipelineContext context, CancellationToken token)
			{
				Runs++;
				ran.Add(Name);

				if (Work is not null)
				{
					await Work(context, Runs);
				}
			}
		}


		[TestInitialize]
		public void Setup()
		{
			now = new DateTime(2026, 1, 1, 0, 0, 0);
			delays = new List<TimeSpan>();
			ran = new List<string>();
			cancel = new CancellationTokenSource();
		}


		private FakeStage Stage(string name, int interval = 120000)
		{
			return new FakeStage(name, ran, interval);
		}


		private PipelineService Create(params IPipelineStage[] stages)
		{
			// a delay moves the clock forward instead of waiting
			return new PipelineService(stages, () => now, (span, token) =>
			{
				token.ThrowIfCancellationRequested();
				delays.Add(span);
				now += span;
				return Task.CompletedTask;
			});
		}


		private Task<TimeSpan> Cycle(PipelineService service) => service.RunCycle(cancel.Token);


		[TestMethod]
		public async Task RunCycle_RunsEnabledStagesInOrder()
		{
			var a = Stage("a");
			var b = Stage("b");
			var c = Stage("c");
			b.Enabled = false;

			await Cycle(Create(a, b, c));

			CollectionAssert.AreEqual(new[] { "a", "c" }, ran);
		}


		[TestMethod]
		public async Task RunCycle_StageSeesWhatAnEarlierStagePublished_ButOnlyInTheSameCycle()
		{
			var seen = new List<string>();
			var a = Stage("a");
			var b = Stage("b");
			a.Work = (context, run) =>
			{
				if (run == 1)
				{
					context.Set("published by a");
				}

				return Task.CompletedTask;
			};

			b.Work = (context, run) =>
			{
				seen.Add(context.TryGet<string>(out var value) ? value : "nothing");
				return Task.CompletedTask;
			};

			var service = Create(a, b);
			await Cycle(service);

			now += TwoMinutes;
			await Cycle(service);

			CollectionAssert.AreEqual(new[] { "published by a", "nothing" }, seen);
		}


		[TestMethod]
		public async Task RunCycle_StageIsSkippedUntilItsIntervalHasElapsed()
		{
			var a = Stage("a", interval: 120000);
			var service = Create(a);

			await Cycle(service);
			now += TimeSpan.FromMinutes(1);
			await Cycle(service);
			Assert.AreEqual(1, a.Runs, "not due after one minute");

			now += TimeSpan.FromMinutes(1);
			await Cycle(service);
			Assert.AreEqual(2, a.Runs, "due after two");
		}


		[TestMethod]
		public async Task RunCycle_EveryStageKeepsItsOwnInterval()
		{
			var fast = Stage("fast", interval: 60000);
			var slow = Stage("slow", interval: 600000);
			var service = Create(fast, slow);

			// ten minutes, one cycle a minute
			for (var minute = 0; minute <= 10; minute++)
			{
				await Cycle(service);
				now += TimeSpan.FromMinutes(1);
			}

			Assert.AreEqual(11, fast.Runs);
			Assert.AreEqual(2, slow.Runs, "once at the start and once ten minutes later");
		}


		[TestMethod]
		public async Task RunCycle_NextDelayIsTheShortestIntervalOfTheEnabledStages()
		{
			var a = Stage("a", interval: 120000);
			var b = Stage("b", interval: 300000);
			var service = Create(a, b);

			Assert.AreEqual(TwoMinutes, await Cycle(service));

			a.Enabled = false;
			now += TimeSpan.FromMinutes(5);
			Assert.AreEqual(TimeSpan.FromMinutes(5), await Cycle(service));
		}


		[TestMethod]
		public async Task RunCycle_EveryStageDisabled_WaitsTheIdleDelay()
		{
			var a = Stage("a");
			a.Enabled = false;

			var next = await Cycle(Create(a));

			Assert.AreEqual(TimeSpan.FromMilliseconds(PipelineService.IdleDelay), next);
			Assert.AreEqual(0, a.Runs);
		}


		[TestMethod]
		public async Task RunCycle_NeverSpinsFasterThanTheMinimumInterval()
		{
			var a = Stage("a", interval: 0);
			var service = Create(a);

			var next = await Cycle(service);
			Assert.AreEqual(TimeSpan.FromMilliseconds(PipelineService.MinimumInterval), next);

			await Cycle(service);
			Assert.AreEqual(1, a.Runs, "not due again until the minimum has passed");
		}


		[TestMethod]
		public async Task RunCycle_StageThatIsNotReady_IsSkippedAndAskedAgainSoon_WithoutHoldingUpOthers()
		{
			var a = Stage("a");
			var b = Stage("b");
			a.Ready = false;
			var service = Create(a, b);

			var next = await Cycle(service);

			CollectionAssert.AreEqual(new[] { "b" }, ran);
			Assert.AreEqual(TimeSpan.FromMilliseconds(PipelineService.NotReadyDelay), next);

			// once ready, it runs at the next cycle, not after waiting out its whole interval
			a.Ready = true;
			now += next;
			await Cycle(service);
			Assert.AreEqual(1, a.Runs);
		}


		[TestMethod]
		public async Task RunCycle_StageDisabledThenEnabled_ResumesWithoutARestart()
		{
			var a = Stage("a");
			var service = Create(a);

			await Cycle(service);
			Assert.AreEqual(1, a.Runs);

			a.Enabled = false;
			now += TwoMinutes;
			await Cycle(service);
			Assert.AreEqual(1, a.Runs, "off");

			a.Enabled = true;
			now += TwoMinutes;
			await Cycle(service);
			Assert.AreEqual(2, a.Runs, "back on");
		}


		[TestMethod]
		public async Task RunCycle_StageThatThrows_DoesNotStopTheOthers()
		{
			var a = Stage("a");
			var b = Stage("b");
			a.Work = (c, run) => throw new InvalidOperationException("boom");

			await Cycle(Create(a, b));

			CollectionAssert.AreEqual(new[] { "a", "b" }, ran);
		}


		[TestMethod]
		public async Task RunCycle_FailedStage_IsTriedAgainAfterItsInterval_NotImmediately()
		{
			var a = Stage("a");
			a.Work = (c, run) => throw new InvalidOperationException("boom");
			var service = Create(a);

			await Cycle(service);
			await Cycle(service);
			Assert.AreEqual(1, a.Runs);

			now += TwoMinutes;
			await Cycle(service);
			Assert.AreEqual(2, a.Runs);
		}


		[TestMethod]
		public async Task RunCycle_FiveConsecutiveFailures_SwitchTheStageOffForTheSession()
		{
			var a = Stage("a");
			var b = Stage("b");
			a.Work = (c, run) => throw new InvalidOperationException("boom");
			var service = Create(a, b);

			for (var i = 0; i < 8; i++)
			{
				await Cycle(service);
				now += TwoMinutes;
			}

			Assert.AreEqual(PipelineService.MaxFailures, a.Runs, "stops after five");
			Assert.AreEqual(8, b.Runs, "the other stage is unaffected");
		}


		[TestMethod]
		public async Task RunCycle_ASuccessResetsTheFailureCount()
		{
			var a = Stage("a");

			// four failures, then a success, then four more failures: never five in a row
			a.Work = (c, run) => run == 5 ? Task.CompletedTask : throw new InvalidOperationException("boom");
			var service = Create(a);

			for (var i = 0; i < 9; i++)
			{
				await Cycle(service);
				now += TwoMinutes;
			}

			Assert.AreEqual(9, a.Runs, "never switched off");
		}


		[TestMethod]
		public async Task RunCycle_Cancellation_Propagates_AndIsNotCountedAsAFailure()
		{
			var a = Stage("a");
			a.Work = (c, run) => throw new OperationCanceledException();
			var service = Create(a);

			await Assert.ThrowsExceptionAsync<OperationCanceledException>(() => Cycle(service));

			// not a failure, so not on its way to being switched off
			now += TwoMinutes;
			await Assert.ThrowsExceptionAsync<OperationCanceledException>(() => Cycle(service));
			Assert.AreEqual(2, a.Runs);
		}


		[TestMethod]
		public void InitializeStages_InitializesEveryStage()
		{
			var a = Stage("a");
			var b = Stage("b");
			b.Enabled = false;

			Create(a, b).InitializeStages();

			Assert.IsTrue(a.Initialized);
			Assert.IsTrue(b.Initialized, "even a disabled stage, so it is ready when enabled");
		}


		[TestMethod]
		public async Task Run_WaitsToSettleBeforeTheFirstCycle()
		{
			var a = Stage("a");

			// the first delay is the settle delay; the next one cancels the loop
			var first = true;
			var withCancel = new PipelineService(new[] { a }, () => now, (span, token) =>
			{
				delays.Add(span);
				if (!first)
				{
					cancel.Cancel();
				}

				first = false;
				token.ThrowIfCancellationRequested();
				return Task.CompletedTask;
			});

			await withCancel.Run(cancel.Token);

			Assert.AreEqual(TimeSpan.FromMilliseconds(PipelineService.SettleDelay), delays[0]);
			Assert.AreEqual(1, a.Runs, "ran once before the cancel");
		}


		[TestMethod]
		public async Task Run_Continuous_StopsWhenCancelled_AndDisposesTheStages()
		{
			var a = Stage("a");
			var cycles = 0;
			a.Work = (c, run) =>
			{
				if (++cycles == 3)
				{
					cancel.Cancel();
				}

				return Task.CompletedTask;
			};

			var service = Create(a);
			await service.Run(cancel.Token);

			Assert.AreEqual(3, a.Runs);
			Assert.IsTrue(a.Disposed);
		}


		[TestMethod]
		public async Task Run_OneShot_RunsEveryStageOnce_ThenStops()
		{
			var a = Stage("a");
			var b = Stage("b");
			var service = Create(a, b);
			service.Mode = PipelineMode.OneShot;

			await service.Run(cancel.Token);

			CollectionAssert.AreEqual(new[] { "a", "b" }, ran);
			Assert.IsTrue(a.Disposed && b.Disposed);
		}


		[TestMethod]
		public async Task Run_OneShot_RetriesAFailingStage_ThenGivesUp_WhileOthersStillComplete()
		{
			var a = Stage("a");
			var b = Stage("b");
			a.Work = (c, run) => throw new InvalidOperationException("boom");
			var service = Create(a, b);
			service.Mode = PipelineMode.OneShot;

			await service.Run(cancel.Token);

			Assert.AreEqual(PipelineService.MaxFailures, a.Runs);
			Assert.AreEqual(1, b.Runs);
		}


		[TestMethod]
		public async Task Run_OneShot_AFailingStageThatThenSucceeds_IsDone()
		{
			var a = Stage("a");
			a.Work = (c, run) => run < 3 ? throw new InvalidOperationException("boom") : Task.CompletedTask;
			var service = Create(a);
			service.Mode = PipelineMode.OneShot;

			await service.Run(cancel.Token);

			Assert.AreEqual(3, a.Runs);
		}


		[TestMethod]
		public async Task Run_OneShot_ADisabledStageDoesNotKeepItWaiting()
		{
			var a = Stage("a");
			var b = Stage("b");
			a.Enabled = false;
			var service = Create(a, b);
			service.Mode = PipelineMode.OneShot;

			await service.Run(cancel.Token);

			Assert.AreEqual(0, a.Runs);
			Assert.AreEqual(1, b.Runs);
		}


		[TestMethod]
		public async Task Run_OneShot_AStageThatIsNotReadyIsPolledUntilItIs()
		{
			var a = Stage("a");
			a.Ready = false;
			var polls = 0;

			var service = new PipelineService(new[] { a }, () => now, (span, token) =>
			{
				delays.Add(span);
				now += span;
				if (++polls == 3)
				{
					a.Ready = true;
				}

				return Task.CompletedTask;
			});
			service.Mode = PipelineMode.OneShot;

			await service.Run(cancel.Token);

			Assert.AreEqual(1, a.Runs);
			Assert.IsTrue(delays.Skip(1).All(d => d == TimeSpan.FromMilliseconds(PipelineService.NotReadyDelay)),
				"polled quickly while waiting");
		}


		[TestMethod]
		public void Context_StoresOneValuePerType_AndReportsWhatIsMissing()
		{
			var context = new PipelineContext();

			Assert.IsFalse(context.TryGet<string>(out _));

			context.Set("first");
			context.Set("second");
			context.Set(new List<int> { 1 });

			Assert.IsTrue(context.TryGet<string>(out var text));
			Assert.AreEqual("second", text);
			Assert.IsTrue(context.TryGet<List<int>>(out var list));
			Assert.AreEqual(1, list.Count);
		}
	}
}
