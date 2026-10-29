//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.TextIndex
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Identity;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Pipeline;
	using River.OneMoreAddIn.TextIndex;
	using System;
	using System.Collections.Generic;
	using System.Data.SQLite;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	[TestClass]
	public class IndexConsumerTests
	{
		private const string Ns = "http://schemas.microsoft.com/office/onenote/2013/onenote";
		private const string Modified = "2026-02-01T00:00:00.000Z";

		private SQLiteConnection connection;
		private IndexProvider provider;
		private bool indexAll;
		private IReadOnlyCollection<long> knownKeys;


		[TestInitialize]
		public void Setup()
		{
			if (!IndexProvider.IsAvailable)
			{
				Assert.Inconclusive("FTS5 with the trigram tokenizer cannot be loaded here");
			}

			connection = new SQLiteConnection("Data Source=:memory:");
			connection.Open();
			provider = new IndexProvider(connection);
			indexAll = false;
			knownKeys = null;
		}


		[TestCleanup]
		public void Teardown()
		{
			provider?.Dispose();
			connection?.Dispose();
		}


		private IndexConsumer MakeConsumer(TimeSpan? budget = null)
		{
			// a generous budget unless a test is about the budget, so timing never limits a test
			return new IndexConsumer(
				() => provider, () => indexAll, () => knownKeys, ownsProvider: false,
				passBudget: budget ?? TimeSpan.FromMinutes(10));
		}


		private static IdentityPage MakePage(
			long key, string pageID = null, string modified = Modified,
			ResolutionKind kind = ResolutionKind.Known, bool tagIndex = false)
		{
			var page = new IdentityPage(
				new PageRef(pageID ?? $"id{key}", "nb", "/sec", $"title{key}", "c", modified),
				"nbID", "secID", "/Notebook/Section", tagIndex);

			page.Resolution = new PageResolution(page.Ref, null, kind) { PageKey = key };
			return page;
		}


		private static IdentityNotebook MakeNotebook(string key, params IdentityPage[] pages)
		{
			var notebook = new IdentityNotebook($"id-{key}", key, key) { Listed = true };
			notebook.Pages.AddRange(pages);
			return notebook;
		}


		private static IdentitySnapshot Snapshot(
			IEnumerable<IdentityNotebook> notebooks, params long[] purged)
		{
			return new IdentitySnapshot(
				notebooks.ToList(), Array.Empty<string>(), purged, TimeSpan.Zero);
		}


		private static Page MakeContent(params string[] texts)
		{
			var oes = string.Concat(texts.Select((t, i) =>
				$"<one:OE objectID=\"{{o{i}}}\"><one:T><![CDATA[{t}]]></one:T></one:OE>"));

			return new Page(XElement.Parse(
				$"<one:Page xmlns:one=\"{Ns}\" ID=\"{{p}}\" name=\"t\">" +
				$"<one:Outline objectID=\"{{outline}}\"><one:OEChildren>{oes}</one:OEChildren></one:Outline>" +
				"</one:Page>"));
		}


		private static readonly IndexStamp Stamp = new IndexStamp { PageID = "id1", Modified = Modified };


		#region NeedsIndexing
		[TestMethod]
		public void NeedsIndexing_NeverIndexed_IsTrue()
		{
			Assert.IsTrue(IndexConsumer.NeedsIndexing(null, "id1", Modified, ResolutionKind.New));
		}


		[TestMethod]
		public void NeedsIndexing_Unchanged_IsFalse()
		{
			Assert.IsFalse(IndexConsumer.NeedsIndexing(Stamp, "id1", Modified, ResolutionKind.Known));
		}


		[TestMethod]
		public void NeedsIndexing_Edited_IsTrue()
		{
			Assert.IsTrue(IndexConsumer.NeedsIndexing(
				Stamp, "id1", "2026-03-01T00:00:00.000Z", ResolutionKind.Known));
		}


		[TestMethod]
		public void NeedsIndexing_RestoredToAnOlderVersion_IsTrue()
		{
			Assert.IsTrue(IndexConsumer.NeedsIndexing(
				Stamp, "id1", "2025-01-01T00:00:00.000Z", ResolutionKind.Known));
		}


		[TestMethod]
		public void NeedsIndexing_PageIDChanged_IsTrue_EvenIfTheIdentityPassAlreadyAbsorbedIt()
		{
			// after a notebook reopen the pass reports Known on its next run, because it records
			// the new ID; the paragraph object IDs in the index are stale all the same
			Assert.IsTrue(IndexConsumer.NeedsIndexing(Stamp, "id1-new", Modified, ResolutionKind.Known));
			Assert.IsTrue(IndexConsumer.NeedsIndexing(Stamp, "id1-new", Modified, ResolutionKind.SameLocation));
		}


		[TestMethod]
		public void NeedsIndexing_WeakMatch_IsTrue()
		{
			Assert.IsTrue(IndexConsumer.NeedsIndexing(Stamp, "id1", Modified, ResolutionKind.SameTitle));
		}


		[TestMethod]
		public void NeedsIndexing_MovedButOtherwiseUnchangedAndSameID_IsFalse()
		{
			// a move keeps the page ID, and the paragraphs, so there is nothing to read again
			Assert.IsFalse(IndexConsumer.NeedsIndexing(Stamp, "id1", Modified, ResolutionKind.MovedSection));
		}
		#endregion NeedsIndexing


		#region Pass
		private async Task<(List<string> Log, PageContentPass Pass)> RunPass(
			IndexConsumer consumer, IdentitySnapshot snapshot, Func<string, string[]> contents)
		{
			var loaded = new List<string>();
			var pass = new PageContentPass(
				new IPageConsumer[] { consumer }, 0, null,
				(one, identity) =>
				{
					loaded.Add(identity.Ref.PageID);
					return Task.FromResult(MakeContent(contents(identity.Ref.PageID)));
				});

			await pass.Run(snapshot);
			return (loaded, pass);
		}


		[TestMethod]
		public async Task Notebook_NotChosen_IsNotIndexed()
		{
			using var consumer = MakeConsumer();
			var snapshot = Snapshot(new[] { MakeNotebook("nb", MakePage(1)) });

			var (loaded, _) = await RunPass(consumer, snapshot, id => new[] { "hello world" });

			Assert.AreEqual(0, loaded.Count);
			Assert.AreEqual(0, provider.ReadStatistics().Pages);
		}


		[TestMethod]
		public async Task Notebook_ExplicitlyIncluded_IsIndexed()
		{
			provider.WriteNotebook("nb", true);

			using var consumer = MakeConsumer();
			var snapshot = Snapshot(new[] { MakeNotebook("nb", MakePage(1)) });

			await RunPass(consumer, snapshot, id => new[] { "hello world" });

			Assert.AreEqual(1, provider.Find("\"hello\"").Count);
			Assert.AreEqual(1, consumer.Indexed);
			Assert.IsNotNull(provider.ReadNotebooks().Single().LastIndexed);
		}


		[TestMethod]
		public async Task IndexAll_IndexesEveryNotebook_EvenOneLeftUncheckedBefore_AndWritesNoChoices()
		{
			indexAll = true;

			// the user once chose notebooks and left this one unchecked; "all" still means all
			provider.WriteNotebook("skip", false);

			using var consumer = MakeConsumer();
			var snapshot = Snapshot(new[]
			{
				MakeNotebook("nb", MakePage(1)),
				MakeNotebook("skip", MakePage(2))
			});

			await RunPass(consumer, snapshot, id => new[] { "hello world" });

			Assert.AreEqual(2, provider.ReadStatistics().Pages);
			Assert.IsTrue(provider.ReadStamps().ContainsKey(1));
			Assert.IsTrue(provider.ReadStamps().ContainsKey(2));

			// indexing everything must not turn each notebook into an explicit choice, and the
			// earlier choice is remembered for when the user goes back to choosing
			var notebooks = provider.ReadNotebooks();
			Assert.AreEqual(1, notebooks.Count);
			Assert.IsFalse(notebooks[0].Included);
		}


		private void CreateIdentity(params (long Key, string Notebook)[] pages)
		{
			using var cmd = connection.CreateCommand();
			cmd.CommandText =
				"CREATE TABLE IF NOT EXISTS identity_page (pageKey INTEGER PRIMARY KEY, notebookKey TEXT)";
			cmd.ExecuteNonQuery();

			foreach (var (key, notebook) in pages)
			{
				cmd.CommandText = $"INSERT INTO identity_page (pageKey, notebookKey) VALUES ({key}, '{notebook}')";
				cmd.ExecuteNonQuery();
			}
		}


		private static List<ParagraphText> Text(string text)
		{
			return new List<ParagraphText>
			{
				new ParagraphText { ObjectID = "{o}", Ordinal = 0, Text = text }
			};
		}


		[TestMethod]
		public void ReadStatistics_TotalPages_CountsKnownPagesOfChosenNotebooks()
		{
			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText =
					"CREATE TABLE identity_page (pageKey INTEGER PRIMARY KEY, notebookKey TEXT, missingSince TEXT);" +
					"INSERT INTO identity_page VALUES (1, 'keep', NULL);" +
					"INSERT INTO identity_page VALUES (2, 'keep', NULL);" +
					"INSERT INTO identity_page VALUES (3, 'keep', '2026-01-01');" +
					"INSERT INTO identity_page VALUES (4, 'other', NULL);";
				cmd.ExecuteNonQuery();
			}

			provider.ReplacePage(1, "id1", Modified, Text("kept text"));

			var stats = provider.ReadStatistics(key => key == "keep");

			Assert.AreEqual(1, stats.Pages);
			Assert.AreEqual(2, stats.TotalPages);
		}


		[TestMethod]
		public async Task NotebookNoLongerChosen_IsRemovedFromTheIndex()
		{
			// both notebooks were indexed, say when every notebook was chosen
			CreateIdentity((1, "keep"), (2, "drop"));
			provider.ReplacePage(1, "id1", Modified, Text("kept text"));
			provider.ReplacePage(2, "id2", Modified, Text("dropped text"));
			provider.WriteNotebook("keep", true);

			using var consumer = MakeConsumer();
			var snapshot = Snapshot(new[] { MakeNotebook("keep", MakePage(1)) });

			await RunPass(consumer, snapshot, id => new[] { "kept text" });

			var stamps = provider.ReadStamps();
			Assert.IsTrue(stamps.ContainsKey(1));
			Assert.IsFalse(stamps.ContainsKey(2));
			Assert.AreEqual(0, provider.Find("\"dropped\"").Count);
			Assert.AreEqual(1, provider.Find("\"kept\"").Count);
		}


		[TestMethod]
		public async Task Prune_IncludesNotebooksThatAreClosed()
		{
			// "closed" is simply a notebook the snapshot does not list; the catalog still knows it
			CreateIdentity((1, "keep"), (2, "closed"));
			provider.ReplacePage(1, "id1", Modified, Text("kept text"));
			provider.ReplacePage(2, "id2", Modified, Text("closed text"));
			provider.WriteNotebook("keep", true);

			using var consumer = MakeConsumer();
			await RunPass(consumer, Snapshot(new[] { MakeNotebook("keep", MakePage(1)) }),
				id => new[] { "kept text" });

			Assert.IsFalse(provider.ReadStamps().ContainsKey(2));
		}


		[TestMethod]
		public async Task Prune_RunsWhenTheChoiceChanges_NotEveryPass()
		{
			CreateIdentity((1, "keep"), (2, "other"));
			provider.WriteNotebook("keep", true);

			using var consumer = MakeConsumer();
			var snapshot = Snapshot(new[] { MakeNotebook("keep", MakePage(1)) });

			await RunPass(consumer, snapshot, id => new[] { "kept text" });

			// something puts the other notebook's page back; the choice has not changed, so the
			// next pass leaves it, as pruning is not repeated for the same choice
			provider.ReplacePage(2, "id2", Modified, Text("other text"));
			await RunPass(consumer, snapshot, id => new[] { "kept text" });
			Assert.IsTrue(provider.ReadStamps().ContainsKey(2));

			// the choice changes, so it is pruned again
			provider.WriteNotebook("elsewhere", true);
			await RunPass(consumer, snapshot, id => new[] { "kept text" });
			Assert.IsFalse(provider.ReadStamps().ContainsKey(2));
		}


		[TestMethod]
		public async Task Prune_IsNotDoneWhileABuildIsRunning()
		{
			CreateIdentity((1, "keep"), (2, "other"));
			provider.WriteNotebook("keep", true);
			provider.ReplacePage(2, "id2", Modified, Text("other text"));

			using var consumer = MakeConsumer();

			using (IndexConsumer.SuspendBackground())
			{
				await RunPass(consumer, Snapshot(new[] { MakeNotebook("keep", MakePage(1)) }),
					id => new[] { "kept text" });

				// the build may be indexing a notebook the user has not saved as chosen yet
				Assert.IsTrue(provider.ReadStamps().ContainsKey(2));
			}

			await RunPass(consumer, Snapshot(new[] { MakeNotebook("keep", MakePage(1)) }),
				id => new[] { "kept text" });

			Assert.IsFalse(provider.ReadStamps().ContainsKey(2));
		}


		[TestMethod]
		public async Task IndexAll_PrunesNothing()
		{
			indexAll = true;
			CreateIdentity((1, "a"), (2, "b"));
			provider.ReplacePage(1, "id1", Modified, Text("text a"));
			provider.ReplacePage(2, "id2", Modified, Text("text b"));

			using var consumer = MakeConsumer();
			await RunPass(consumer, Snapshot(new IdentityNotebook[0]), id => new string[0]);

			Assert.AreEqual(2, provider.ReadStamps().Count);
		}


		[TestMethod]
		public async Task SecondPass_ReadsOnlyChangedPages()
		{
			indexAll = true;
			var snapshot1 = Snapshot(new[] { MakeNotebook("nb", MakePage(1), MakePage(2)) });

			using var consumer = MakeConsumer();
			var (first, _) = await RunPass(consumer, snapshot1, id => new[] { $"text of {id}" });
			Assert.AreEqual(2, first.Count);

			var snapshot2 = Snapshot(new[]
			{
				MakeNotebook("nb", MakePage(1), MakePage(2, modified: "2026-04-01T00:00:00.000Z"))
			});

			var (second, _) = await RunPass(consumer, snapshot2, id => new[] { $"edited {id}" });

			CollectionAssert.AreEqual(new[] { "id2" }, second);
			Assert.AreEqual(1, provider.Find("\"edited\"").Count);
			Assert.AreEqual(1, provider.Find("\"text of id1\"").Count);
		}


		[TestMethod]
		public async Task ReopenedNotebook_PageIsIndexedAgainWithItsNewObjectIDs()
		{
			indexAll = true;
			using var consumer = MakeConsumer();

			await RunPass(consumer,
				Snapshot(new[] { MakeNotebook("nb", MakePage(1)) }), id => new[] { "stable text" });

			Assert.AreEqual("id1", provider.ReadStamps()[1].PageID);

			// same page key and content, but OneNote regenerated the page ID
			var (loaded, _) = await RunPass(consumer,
				Snapshot(new[] { MakeNotebook("nb", MakePage(1, pageID: "id1-reopened")) }),
				id => new[] { "stable text" });

			CollectionAssert.AreEqual(new[] { "id1-reopened" }, loaded);
			Assert.AreEqual("id1-reopened", provider.ReadStamps()[1].PageID);
		}


		[TestMethod]
		public async Task Budget_SpreadsTheBacklogOverPasses_OnePageAtLeastEachTime()
		{
			indexAll = true;

			// a budget that is always spent, so each pass is allowed only the one guaranteed page
			using var consumer = MakeConsumer(TimeSpan.Zero);
			var snapshot = Snapshot(new[]
			{
				MakeNotebook("nb", MakePage(1), MakePage(2), MakePage(3))
			});

			var (first, _) = await RunPass(consumer, snapshot, id => new[] { $"text {id}" });
			CollectionAssert.AreEqual(new[] { "id1" }, first);
			Assert.AreEqual(1, provider.ReadStatistics().Pages);

			var (second, _) = await RunPass(consumer, snapshot, id => new[] { $"text {id}" });
			CollectionAssert.AreEqual(new[] { "id2" }, second);

			var (third, _) = await RunPass(consumer, snapshot, id => new[] { $"text {id}" });
			CollectionAssert.AreEqual(new[] { "id3" }, third);

			// nothing left; the budget is not what stops a pass that has no work
			var (fourth, _) = await RunPass(consumer, snapshot, id => new[] { $"text {id}" });
			Assert.AreEqual(0, fourth.Count);
			Assert.AreEqual(3, provider.ReadStatistics().Pages);
		}


		[TestMethod]
		public async Task Budget_DoesNotCountPagesThatAreUpToDate()
		{
			indexAll = true;
			using var consumer = MakeConsumer(TimeSpan.Zero);
			var snapshot = Snapshot(new[]
			{
				MakeNotebook("nb", MakePage(1), MakePage(2), MakePage(3))
			});

			for (var i = 0; i < 3; i++)
			{
				await RunPass(consumer, snapshot, id => new[] { "text" });
			}

			// page 2 changes; it is read at once even though the budget is always spent, because
			// the pages ahead of it need nothing and cost nothing
			var edited = Snapshot(new[]
			{
				MakeNotebook("nb", MakePage(1),
					MakePage(2, modified: "2026-06-01T00:00:00.000Z"), MakePage(3))
			});

			var (loaded, _) = await RunPass(consumer, edited, id => new[] { "edited text" });
			CollectionAssert.AreEqual(new[] { "id2" }, loaded);
		}


		[TestMethod]
		public async Task Build_ReportsProgressAgainstWhatItExpects()
		{
			indexAll = true;
			var reports = new List<(int Done, int Expected)>();

			// page 3 is already up to date so it is not part of the work
			provider.ReplacePage(3, "id3", Modified, new List<ParagraphText>());

			using var consumer = new IndexConsumer(
				() => provider, () => indexAll, () => knownKeys, ownsProvider: false,
				passBudget: TimeSpan.MaxValue, isBuild: true,
				progress: (done, expected) => reports.Add((done, expected)));

			var snapshot = Snapshot(new[]
			{
				MakeNotebook("nb", MakePage(1), MakePage(2), MakePage(3))
			});

			await RunPass(consumer, snapshot, id => new[] { "text" });

			CollectionAssert.AreEqual(new[] { (1, 2), (2, 2) }, reports);
		}


		[TestMethod]
		public async Task Build_IsNotCappedByTheBudget()
		{
			indexAll = true;

			using var consumer = new IndexConsumer(
				() => provider, () => indexAll, () => knownKeys, ownsProvider: false,
				passBudget: TimeSpan.MaxValue, isBuild: true);

			var snapshot = Snapshot(new[]
			{
				MakeNotebook("nb", MakePage(1), MakePage(2), MakePage(3), MakePage(4))
			});

			var (loaded, _) = await RunPass(consumer, snapshot, id => new[] { "text" });

			Assert.AreEqual(4, loaded.Count);
		}


		[TestMethod]
		public async Task SuspendBackground_HoldsBackTheBackgroundConsumerButNotABuild()
		{
			indexAll = true;
			var snapshot = Snapshot(new[] { MakeNotebook("nb", MakePage(1)) });

			using var background = MakeConsumer();
			using var build = new IndexConsumer(
				() => provider, () => indexAll, () => knownKeys, ownsProvider: false,
				passBudget: TimeSpan.MaxValue, isBuild: true);

			using (IndexConsumer.SuspendBackground())
			{
				var (held, _) = await RunPass(background, snapshot, id => new[] { "text" });
				Assert.AreEqual(0, held.Count);

				var (built, _) = await RunPass(build, snapshot, id => new[] { "text" });
				Assert.AreEqual(1, built.Count);
			}

			// resumed: the page is up to date now, so there is simply nothing left to do, but the
			// consumer takes part again
			provider.DeletePages(new long[] { 1 });

			var (resumed, _) = await RunPass(background, snapshot, id => new[] { "text" });
			Assert.AreEqual(1, resumed.Count);
		}


		[TestMethod]
		public void MinimumThrottle_IsTheReadGap()
		{
			using var consumer = MakeConsumer();
			Assert.AreEqual(IndexConsumer.ReadGap, consumer.MinimumThrottle);
		}


		[TestMethod]
		public async Task TagIndexPage_IsNotIndexed()
		{
			indexAll = true;
			using var consumer = MakeConsumer();
			var snapshot = Snapshot(new[] { MakeNotebook("nb", MakePage(1, tagIndex: true)) });

			var (loaded, _) = await RunPass(consumer, snapshot, id => new[] { "#tag" });

			Assert.AreEqual(0, loaded.Count);
		}


		[TestMethod]
		public async Task PurgedPages_AreRemoved()
		{
			indexAll = true;
			using var consumer = MakeConsumer();

			await RunPass(consumer,
				Snapshot(new[] { MakeNotebook("nb", MakePage(1), MakePage(2)) }),
				id => new[] { "some text" });

			await RunPass(consumer,
				Snapshot(new[] { MakeNotebook("nb", MakePage(1)) }, purged: new long[] { 2 }),
				id => new[] { "some text" });

			Assert.IsTrue(provider.ReadStamps().ContainsKey(1));
			Assert.IsFalse(provider.ReadStamps().ContainsKey(2));
		}


		[TestMethod]
		public async Task OrphanedPages_AreSweptOncePerSession()
		{
			// pages 1 and 2 were indexed earlier; the catalog now knows only page 1
			provider.ReplacePage(1, "id1", Modified, new List<ParagraphText>());
			provider.ReplacePage(2, "id2", Modified, new List<ParagraphText>());
			knownKeys = new long[] { 1 };

			using var consumer = MakeConsumer();
			await RunPass(consumer, Snapshot(new IdentityNotebook[0]), id => new string[0]);

			Assert.IsTrue(provider.ReadStamps().ContainsKey(1));
			Assert.IsFalse(provider.ReadStamps().ContainsKey(2));
		}


		[TestMethod]
		public async Task OrphanSweep_WhenCatalogCannotBeRead_RemovesNothing()
		{
			provider.ReplacePage(1, "id1", Modified, new List<ParagraphText>());
			knownKeys = null;

			using var consumer = MakeConsumer();
			await RunPass(consumer, Snapshot(new IdentityNotebook[0]), id => new string[0]);

			Assert.IsTrue(provider.ReadStamps().ContainsKey(1));
		}
		#endregion Pass
	}
}
