//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Pipeline
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Identity;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Pipeline;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// The pass is tested with fake consumers and a fake page loader, so nothing opens OneNote.
	/// </summary>
	[TestClass]
	public class PageContentPassTests
	{
		private sealed class FakeConsumer : IPageConsumer
		{
			private readonly List<string> log;
			private readonly Func<IdentityPage, bool> needs;

			public FakeConsumer(string name, List<string> log, Func<IdentityPage, bool> needs = null)
			{
				Name = name;
				this.log = log;
				this.needs = needs ?? (_ => true);
			}

			public string Name { get; }
			public int MinimumThrottle { get; set; }
			public bool AcceptNotebooks { get; set; } = true;
			public bool Dirty { get; set; }
			public Action<IdentityPage> OnConsume { get; set; }
			public List<string> Consumed { get; } = new();
			public List<(string Notebook, int Dirty)> Ended { get; } = new();
			public bool? Cancelled { get; private set; }
			public int Begins { get; private set; }

			public void Begin(IdentitySnapshot snapshot)
			{
				Begins++;
				log.Add($"{Name}:begin");
			}

			public bool BeginNotebook(IdentityNotebook notebook)
			{
				log.Add($"{Name}:begin-notebook {notebook.Name}");
				return AcceptNotebooks;
			}

			public bool NeedsPage(IdentityPage page) => needs(page);

			public Task<bool> Consume(OneNote one, Page page, IdentityPage identity)
			{
				Consumed.Add(identity.Ref.Title);
				log.Add($"{Name}:consume {identity.Ref.Title}");
				OnConsume?.Invoke(identity);
				return Task.FromResult(Dirty);
			}

			public void EndNotebook(IdentityNotebook notebook, int dirtyPages)
			{
				Ended.Add((notebook.Name, dirtyPages));
				log.Add($"{Name}:end-notebook {notebook.Name}");
			}

			public void End(IdentitySnapshot snapshot, bool cancelled)
			{
				Cancelled = cancelled;
				log.Add($"{Name}:end");
			}
		}


		private readonly List<string> log = new();
		private readonly List<string> loaded = new();
		private int loadCalls;


		private static IdentityPage MakePage(string title, long key = 0)
		{
			var page = new IdentityPage(
				new PageRef($"id-{title}", "nb", "/sec", title, "c", "m"),
				"nbID", "secID", "/Notebook/Section", isTagIndex: false);

			page.Resolution = new PageResolution(page.Ref, null, ResolutionKind.New)
			{
				PageKey = key
			};

			return page;
		}


		private static IdentityNotebook MakeNotebook(string name, bool listed, params string[] titles)
		{
			var notebook = new IdentityNotebook($"id-{name}", name, name.ToLowerInvariant())
			{
				Listed = listed
			};

			var key = 1;
			foreach (var title in titles)
			{
				notebook.Pages.Add(MakePage(title, key++));
			}

			return notebook;
		}


		private static IdentitySnapshot Snapshot(params IdentityNotebook[] notebooks)
		{
			return new IdentitySnapshot(
				notebooks, Array.Empty<string>(), Array.Empty<long>(), TimeSpan.Zero);
		}


		private PageContentPass MakePass(params IPageConsumer[] consumers)
		{
			return new PageContentPass(consumers, throttle: 0, isPaused: null, (one, identity) =>
			{
				loadCalls++;
				loaded.Add(identity.Ref.Title);
				return Task.FromResult(new Page(new XElement("Page")));
			});
		}


		[TestMethod]
		public async Task PageWantedByTwoConsumers_IsLoadedOnce()
		{
			var a = new FakeConsumer("a", log);
			var b = new FakeConsumer("b", log);
			var pass = MakePass(a, b);

			await pass.Run(Snapshot(MakeNotebook("N", true, "p1", "p2")));

			Assert.AreEqual(2, loadCalls);
			CollectionAssert.AreEqual(new[] { "p1", "p2" }, a.Consumed);
			CollectionAssert.AreEqual(new[] { "p1", "p2" }, b.Consumed);
			Assert.AreEqual(2, pass.Stats.PagesRead);
		}


		[TestMethod]
		public async Task PageWantedByNoConsumer_IsNotLoaded()
		{
			var a = new FakeConsumer("a", log, _ => false);
			var pass = MakePass(a);

			await pass.Run(Snapshot(MakeNotebook("N", true, "p1", "p2")));

			Assert.AreEqual(0, loadCalls);
			Assert.AreEqual(0, a.Consumed.Count);
			Assert.AreEqual(0, pass.Stats.PagesRead);
		}


		[TestMethod]
		public async Task Consumer_IsOnlyGivenPagesItNeeds()
		{
			var a = new FakeConsumer("a", log, p => p.Ref.Title == "p1");
			var b = new FakeConsumer("b", log, p => p.Ref.Title == "p2");
			var pass = MakePass(a, b);

			await pass.Run(Snapshot(MakeNotebook("N", true, "p1", "p2", "p3")));

			// p3 is wanted by nobody so it is never read
			CollectionAssert.AreEqual(new[] { "p1", "p2" }, loaded);
			CollectionAssert.AreEqual(new[] { "p1" }, a.Consumed);
			CollectionAssert.AreEqual(new[] { "p2" }, b.Consumed);
		}


		[TestMethod]
		public async Task NotebookDeclinedByConsumer_SkipsItsPagesAndEnd()
		{
			var a = new FakeConsumer("a", log) { AcceptNotebooks = false };
			var b = new FakeConsumer("b", log);
			var pass = MakePass(a, b);

			await pass.Run(Snapshot(MakeNotebook("N", true, "p1")));

			Assert.AreEqual(0, a.Consumed.Count);
			Assert.AreEqual(0, a.Ended.Count);
			CollectionAssert.AreEqual(new[] { "p1" }, b.Consumed);
			Assert.AreEqual(1, b.Ended.Count);
		}


		[TestMethod]
		public async Task NotebookDeclinedByEveryConsumer_LoadsNothing()
		{
			var a = new FakeConsumer("a", log) { AcceptNotebooks = false };
			var pass = MakePass(a);

			await pass.Run(Snapshot(MakeNotebook("N", true, "p1")));

			Assert.AreEqual(0, loadCalls);
			Assert.AreEqual(false, a.Cancelled);
		}


		[TestMethod]
		public async Task DirtyPages_AreCountedPerConsumer()
		{
			var a = new FakeConsumer("a", log) { Dirty = true };
			var b = new FakeConsumer("b", log) { Dirty = false };
			var pass = MakePass(a, b);

			await pass.Run(Snapshot(MakeNotebook("N", true, "p1", "p2", "p3")));

			Assert.AreEqual(3, a.Ended.Single().Dirty);
			Assert.AreEqual(0, b.Ended.Single().Dirty);
		}


		[TestMethod]
		public async Task UnreadablePage_IsNotConsumedAndNotDirty()
		{
			var a = new FakeConsumer("a", log) { Dirty = true };
			var pass = new PageContentPass(new IPageConsumer[] { a }, 0, null,
				(one, identity) =>
				{
					loaded.Add(identity.Ref.Title);
					return Task.FromResult<Page>(identity.Ref.Title == "locked"
						? null
						: new Page(new XElement("Page")));
				});

			await pass.Run(Snapshot(MakeNotebook("N", true, "p1", "locked", "p2")));

			CollectionAssert.AreEqual(new[] { "p1", "locked", "p2" }, loaded);
			CollectionAssert.AreEqual(new[] { "p1", "p2" }, a.Consumed);
			Assert.AreEqual(2, a.Ended.Single().Dirty);
		}


		[TestMethod]
		public async Task UnlistedNotebook_VisitsNoPagesButIsEnded()
		{
			var a = new FakeConsumer("a", log);
			var pass = MakePass(a);

			await pass.Run(Snapshot(MakeNotebook("N", listed: false, "p1")));

			Assert.AreEqual(0, loadCalls);
			Assert.AreEqual(("N", 0), a.Ended.Single());
		}


		[TestMethod]
		public async Task Callbacks_ArriveInOrder()
		{
			var a = new FakeConsumer("a", log);
			var pass = MakePass(a);

			await pass.Run(Snapshot(
				MakeNotebook("N1", true, "p1"), MakeNotebook("N2", true, "p2")));

			CollectionAssert.AreEqual(
				new[]
				{
					"a:begin",
					"a:begin-notebook N1", "a:consume p1", "a:end-notebook N1",
					"a:begin-notebook N2", "a:consume p2", "a:end-notebook N2",
					"a:end"
				},
				log);

			Assert.AreEqual(1, a.Begins);
			Assert.AreEqual(false, a.Cancelled);
		}


		[TestMethod]
		public async Task Cancellation_StopsAndTellsConsumers()
		{
			using var cancel = new CancellationTokenSource();
			var a = new FakeConsumer("a", log) { OnConsume = _ => cancel.Cancel() };
			var pass = MakePass(a);

			await pass.Run(
				Snapshot(MakeNotebook("N1", true, "p1", "p2"), MakeNotebook("N2", true, "p3")),
				cancel.Token);

			// the page in hand is finished, then nothing else is read or begun
			CollectionAssert.AreEqual(new[] { "p1" }, a.Consumed);
			Assert.AreEqual(true, a.Cancelled);
			Assert.IsFalse(log.Contains("a:begin-notebook N2"));
		}


		[TestMethod]
		public async Task Throttle_IsSpentOncePerPageRead_NotPerConsumer()
		{
			var a = new FakeConsumer("a", log);
			var b = new FakeConsumer("b", log);
			var pass = new PageContentPass(new IPageConsumer[] { a, b }, throttle: 15, isPaused: null,
				(one, identity) => Task.FromResult(new Page(new XElement("Page"))));

			await pass.Run(Snapshot(MakeNotebook("N", true, "p1", "p2")));

			// two pages read, two waits of 15ms; two consumers must not double it
			Assert.AreEqual(2, pass.Stats.PagesRead);
			Assert.IsTrue(pass.Stats.ThrottleTime >= 25, $"throttle {pass.Stats.ThrottleTime}");
			Assert.IsTrue(pass.Stats.ThrottleTime < 500, $"throttle {pass.Stats.ThrottleTime}");
		}


		[TestMethod]
		public async Task ConsumerMinimumThrottle_IsAFloorEvenWhenTheThrottleIsOff()
		{
			var a = new FakeConsumer("a", log) { MinimumThrottle = 20 };
			var pass = new PageContentPass(new IPageConsumer[] { a }, throttle: 0, isPaused: null,
				(one, identity) => Task.FromResult(new Page(new XElement("Page"))));

			await pass.Run(Snapshot(MakeNotebook("N", true, "p1", "p2")));

			Assert.IsTrue(pass.Stats.ThrottleTime >= 30, $"throttle {pass.Stats.ThrottleTime}");
		}


		[TestMethod]
		public async Task ConsumerMinimumThrottle_IsOnlyAppliedToPagesItWanted()
		{
			var slow = new FakeConsumer("slow", log, p => p.Ref.Title == "p1") { MinimumThrottle = 100 };
			var fast = new FakeConsumer("fast", log, p => p.Ref.Title == "p2");
			var pass = new PageContentPass(new IPageConsumer[] { slow, fast }, throttle: 0, isPaused: null,
				(one, identity) => Task.FromResult(new Page(new XElement("Page"))));

			await pass.Run(Snapshot(MakeNotebook("N", true, "p1", "p2")));

			// only p1 was wanted by the consumer that asked for a floor
			Assert.IsTrue(pass.Stats.ThrottleTime >= 90, $"throttle {pass.Stats.ThrottleTime}");
			Assert.IsTrue(pass.Stats.ThrottleTime < 190, $"throttle {pass.Stats.ThrottleTime}");
		}


		[TestMethod]
		public async Task Paused_UsesTheExtendedThrottle()
		{
			var a = new FakeConsumer("a", log);
			var pass = new PageContentPass(new IPageConsumer[] { a }, throttle: 1, isPaused: () => true,
				(one, identity) => Task.FromResult(new Page(new XElement("Page"))));

			await pass.Run(Snapshot(MakeNotebook("N", true, "p1")));

			Assert.IsTrue(pass.Stats.ThrottleTime >= PageContentPass.PausedThrottle - 50,
				$"throttle {pass.Stats.ThrottleTime}");
		}
	}
}
