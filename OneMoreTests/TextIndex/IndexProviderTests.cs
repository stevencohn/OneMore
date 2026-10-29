//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.TextIndex
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Properties;
	using River.OneMoreAddIn.TextIndex;
	using System;
	using System.Collections.Generic;
	using System.Data.SQLite;
	using System.Linq;
	using System.Text.RegularExpressions;


	[TestClass]
	public class IndexProviderTests
	{
		private const string Modified = "2026-02-01T00:00:00.000Z";

		private SQLiteConnection connection;
		private IndexProvider provider;


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
		}


		[TestCleanup]
		public void Teardown()
		{
			provider?.Dispose();
			connection?.Dispose();
		}


		private static List<ParagraphText> Paras(params string[] texts)
		{
			return texts
				.Select((t, i) => new ParagraphText { ObjectID = $"{{o{i}}}", Ordinal = i, Text = t })
				.ToList();
		}


		private object Scalar(string sql)
		{
			using var cmd = connection.CreateCommand();
			cmd.CommandText = sql;
			return cmd.ExecuteScalar();
		}


		private void Execute(string sql)
		{
			using var cmd = connection.CreateCommand();
			cmd.CommandText = sql;
			cmd.ExecuteNonQuery();
		}


		[TestMethod]
		public void Constructor_CreatesTheSchema()
		{
			foreach (var name in new[]
				{ "index_schema", "index_page", "index_text", "index_fts", "index_notebook" })
			{
				Assert.AreEqual(1L,
					Scalar($"SELECT COUNT(1) FROM sqlite_master WHERE name = '{name}'"), name);
			}

			Assert.AreEqual(3L,
				Scalar("SELECT COUNT(1) FROM sqlite_master WHERE type = 'trigger'"));
		}


		[TestMethod]
		public void ReplacePage_Find_ReturnsParagraphWithObjectIDAndOrdinal()
		{
			provider.ReplacePage(7, "id7", Modified, Paras("The quick brown fox", "jumps over the lazy dog"));

			var hits = provider.Find("\"lazy\"");

			Assert.AreEqual(1, hits.Count);
			Assert.AreEqual(7L, hits[0].PageKey);
			Assert.AreEqual("{o1}", hits[0].ObjectID);
			Assert.AreEqual(1, hits[0].Ordinal);
			Assert.AreEqual("jumps over the lazy dog", hits[0].Text);
		}


		[TestMethod]
		public void Find_ImageText_IsKindImage_AndExcludedWhenNotRequested()
		{
			provider.ReplacePage(1, "id1", Modified, new List<ParagraphText>
			{
				new ParagraphText { ObjectID = "{t}", Ordinal = 0, Text = "typed zebra" },
				new ParagraphText
				{
					ObjectID = "{i}", Ordinal = 1, Text = "pictured zebra", Kind = ParagraphKind.Image
				}
			});

			var all = provider.Find("\"zebra\"");
			Assert.AreEqual(2, all.Count);
			Assert.AreEqual(ParagraphKind.Text, all[0].Kind);
			Assert.AreEqual(ParagraphKind.Image, all[1].Kind);

			var typed = provider.Find("\"zebra\"", includeOcr: false);
			Assert.AreEqual(1, typed.Count);
			Assert.AreEqual("{t}", typed[0].ObjectID);

			Assert.AreEqual(1, provider.Query("\"zebra\"", includeOcr: false).Count());
			Assert.AreEqual(1, provider.Query(null, includeOcr: false).Count());
			Assert.AreEqual(2, provider.Query(null).Count());
		}


		[TestMethod]
		public void Find_MatchesInsideWords_LikeTheLiveSearch()
		{
			provider.ReplacePage(1, "id1", Modified, Paras("my notebook is here", "unrelated"));

			// TextMatchBuilder has no word boundaries, so "book" matches inside "notebook"
			Assert.AreEqual(1, provider.Find("\"book\"").Count);
			Assert.AreEqual(1, provider.Find("\"ebo\"").Count);
		}


		[TestMethod]
		public void Find_IgnoresCase()
		{
			provider.ReplacePage(1, "id1", Modified, Paras("Hello WORLD"));

			Assert.AreEqual(1, provider.Find("\"hello\"").Count);
			Assert.AreEqual(1, provider.Find("\"world\"").Count);
		}


		[TestMethod]
		public void Find_Phrase_AndBooleans()
		{
			provider.ReplacePage(1, "id1", Modified, Paras(
				"error in the parser", "warning in the parser", "error and warning together"));

			Assert.AreEqual(2, provider.Find("\"error\"").Count);
			Assert.AreEqual(1, provider.Find("\"error\" AND \"warning\"").Count);
			Assert.AreEqual(3, provider.Find("\"error\" OR \"warning\"").Count);
			Assert.AreEqual(1, provider.Find("\"error\" NOT \"warning\"").Count);
			Assert.AreEqual(1, provider.Find("\"in the parser\" AND \"warning\"").Count);
		}


		[TestMethod]
		public void Find_TermShorterThanTrigram_FindsNothing()
		{
			provider.ReplacePage(1, "id1", Modified, Paras("an ab cd ef"));

			// a trigram index cannot answer a 2-character term; callers must not rely on it
			Assert.AreEqual(IndexProvider.MinTermLength, 3);
			Assert.AreEqual(0, provider.Find("\"ab\"").Count);
		}


		[TestMethod]
		public void ReplacePage_ReplacesPreviousRowsAndIndexEntries()
		{
			provider.ReplacePage(1, "id1", Modified, Paras("first version alpha"));
			provider.ReplacePage(1, "id1", "2026-03-01T00:00:00.000Z", Paras("second version beta"));

			Assert.AreEqual(0, provider.Find("\"alpha\"").Count);
			Assert.AreEqual(1, provider.Find("\"beta\"").Count);
			Assert.AreEqual(1L, Scalar("SELECT COUNT(1) FROM index_text WHERE pageKey = 1"));
			Assert.AreEqual(1L, Scalar("SELECT COUNT(1) FROM index_page"));
			Assert.AreEqual("2026-03-01T00:00:00.000Z", provider.ReadStamps()[1].Modified);
		}


		[TestMethod]
		public void ReplacePage_LeavesOtherPagesAlone()
		{
			provider.ReplacePage(1, "id1", Modified, Paras("shared phrase one"));
			provider.ReplacePage(2, "id2", Modified, Paras("shared phrase two"));

			provider.ReplacePage(1, "id1", Modified, Paras("something else"));

			var hits = provider.Find("\"shared\"");
			Assert.AreEqual(1, hits.Count);
			Assert.AreEqual(2L, hits[0].PageKey);
		}


		[TestMethod]
		public void ReplacePage_PageWithNoText_RecordsStampAndNoRows()
		{
			provider.ReplacePage(5, "id5", Modified, Paras("some text here"));
			provider.ReplacePage(5, "id5", Modified, new List<ParagraphText>());

			Assert.AreEqual(0, provider.Find("\"some\"").Count);
			Assert.IsTrue(provider.ReadStamps().ContainsKey(5));

			var stats = provider.ReadStatistics();
			Assert.AreEqual(1, stats.Pages);
			Assert.AreEqual(0, stats.Paragraphs);
		}


		[TestMethod]
		public void ReadStamps_AbsentPage_HasNeverBeenIndexed()
		{
			provider.ReplacePage(1, "id1", Modified, Paras("text"));

			var stamps = provider.ReadStamps();
			Assert.IsTrue(stamps.ContainsKey(1));
			Assert.IsFalse(stamps.ContainsKey(2));
		}


		[TestMethod]
		public void DeletePages_RemovesTextIndexEntriesAndStamps()
		{
			provider.ReplacePage(1, "id1", Modified, Paras("keep this one"));
			provider.ReplacePage(2, "id2", Modified, Paras("drop this one"));
			provider.ReplacePage(3, "id3", Modified, Paras("drop this too"));

			var count = provider.DeletePages(new long[] { 2, 3, 99 });

			Assert.AreEqual(2, count);
			Assert.AreEqual(1, provider.Find("\"this\"").Count);
			Assert.AreEqual(1, provider.ReadStamps().Count);
			Assert.AreEqual(1, provider.ReadStatistics().Paragraphs);
		}


		[TestMethod]
		public void DeletePages_NoneGiven_DoesNothing()
		{
			provider.ReplacePage(1, "id1", Modified, Paras("text here"));

			Assert.AreEqual(0, provider.DeletePages(Array.Empty<long>()));
			Assert.AreEqual(0, provider.DeletePages(null));
			Assert.AreEqual(1, provider.ReadStamps().Count);
		}


		[TestMethod]
		public void ReplacePage_Failure_RollsBackAndKeepsThePreviousVersion()
		{
			provider.ReplacePage(1, "id1", Modified, Paras("original text"));

			var bad = new List<ParagraphText>
			{
				new ParagraphText { ObjectID = "{a}", Ordinal = 0, Text = "fine" },
				new ParagraphText { ObjectID = "{b}", Ordinal = 1, Text = null }
			};

			Assert.ThrowsException<SQLiteException>(() => provider.ReplacePage(1, "id1", "later", bad));

			Assert.AreEqual(1, provider.Find("\"original\"").Count);
			Assert.AreEqual(Modified, provider.ReadStamps()[1].Modified);

			// and the connection is usable afterwards, so the transaction was really ended
			provider.ReplacePage(1, "id1", "again", Paras("recovered text"));
			Assert.AreEqual(1, provider.Find("\"recovered\"").Count);
		}


		[TestMethod]
		public void Rebuild_IndexesRowsInsertedWithoutTriggers()
		{
			Execute("DROP TRIGGER index_text_ai");
			Execute("INSERT INTO index_text (pageKey, objectID, ordinal, text) " +
				"VALUES (1, '{o}', 0, 'bulk loaded paragraph')");

			Assert.AreEqual(0, provider.Find("\"bulk\"").Count);

			provider.Rebuild();

			Assert.AreEqual(1, provider.Find("\"bulk\"").Count);
		}


		private void CreateIdentity(params (long Key, string Notebook)[] pages)
		{
			Execute("CREATE TABLE IF NOT EXISTS identity_page (pageKey INTEGER PRIMARY KEY, notebookKey TEXT, missingSince TEXT)");
			foreach (var (key, notebook) in pages)
			{
				Execute($"INSERT INTO identity_page (pageKey, notebookKey) VALUES ({key}, '{notebook}')");
			}
		}


		[TestMethod]
		public void Clear_RemovesAllIndexedText_KeepsChoicesAndWorksAgain()
		{
			provider.ReplacePage(1, "id1", Modified, Paras("alpha one", "alpha two"));
			provider.ReplacePage(2, "id2", Modified, Paras("bravo three"));
			provider.WriteNotebook("a", true, "2026-02-01T00:00:00.000Z");
			provider.WriteNotebook("b", false);

			var pages = provider.Clear();

			Assert.AreEqual(2, pages);
			Assert.AreEqual(0, provider.ReadStamps().Count);
			Assert.AreEqual(0L, Scalar("SELECT COUNT(1) FROM index_text"));
			Assert.AreEqual(0, provider.Find("\"alpha\"").Count);

			// the user's choices stay; when the notebooks were indexed no longer is true
			var notebooks = provider.ReadNotebooks();
			Assert.AreEqual(2, notebooks.Count);
			Assert.IsTrue(notebooks.Single(n => n.NotebookKey == "a").Included);
			Assert.IsNull(notebooks.Single(n => n.NotebookKey == "a").LastIndexed);
			Assert.IsFalse(notebooks.Single(n => n.NotebookKey == "b").Included);

			// and the index is as good as new
			provider.ReplacePage(3, "id3", Modified, Paras("charlie four"));
			Assert.AreEqual(1, provider.Find("\"charlie\"").Count);
		}


		[TestMethod]
		public void Clear_OnAnEmptyIndex_RemovesNothing()
		{
			Assert.AreEqual(0, provider.Clear());
		}


		[TestMethod]
		public void PruneNotebooks_RemovesOnlyUnchosenNotebooks_FromTextAndIndex()
		{
			CreateIdentity((1, "a"), (2, "a"), (3, "b"), (4, "c"));
			provider.ReplacePage(1, "id1", Modified, Paras("alpha one"));
			provider.ReplacePage(2, "id2", Modified, Paras("alpha two"));
			provider.ReplacePage(3, "id3", Modified, Paras("bravo three"));
			provider.ReplacePage(4, "id4", Modified, Paras("charlie four"));

			var removed = provider.PruneNotebooks(key => key == "b");

			// notebooks "a" (2 pages) and "c" (1 page) are not chosen
			Assert.AreEqual(3, removed);
			CollectionAssert.AreEqual(new long[] { 3 }, provider.ReadStamps().Keys.ToArray());
			Assert.AreEqual(0, provider.Find("\"alpha\"").Count);
			Assert.AreEqual(0, provider.Find("\"charlie\"").Count);
			Assert.AreEqual(1, provider.Find("\"bravo\"").Count);
			Assert.AreEqual(1, provider.ReadStatistics().Paragraphs);
		}


		[TestMethod]
		public void PruneNotebooks_EverythingChosen_DoesNothing()
		{
			CreateIdentity((1, "a"), (2, "b"));
			provider.ReplacePage(1, "id1", Modified, Paras("alpha"));
			provider.ReplacePage(2, "id2", Modified, Paras("bravo"));

			Assert.AreEqual(0, provider.PruneNotebooks(_ => true));
			Assert.AreEqual(2, provider.ReadStamps().Count);
		}


		[TestMethod]
		public void PruneNotebooks_PageUnknownToTheCatalog_IsLeftAlone()
		{
			// no way to say which notebook it belongs to; the orphan sweep deals with these
			CreateIdentity((1, "a"));
			provider.ReplacePage(1, "id1", Modified, Paras("alpha"));
			provider.ReplacePage(9, "id9", Modified, Paras("mystery"));

			provider.PruneNotebooks(_ => false);

			CollectionAssert.AreEqual(new long[] { 9 }, provider.ReadStamps().Keys.ToArray());
		}


		[TestMethod]
		public void PruneNotebooks_WithoutAnIdentityCatalog_DoesNothing()
		{
			provider.ReplacePage(1, "id1", Modified, Paras("alpha"));

			Assert.AreEqual(0, provider.PruneNotebooks(_ => false));
			Assert.AreEqual(1, provider.ReadStamps().Count);
		}


		[TestMethod]
		public void ReadStatistics_Chosen_CountsOnlyChosenNotebooks()
		{
			CreateIdentity((1, "a"), (2, "b"), (3, "b"));
			provider.ReplacePage(1, "id1", Modified, Paras("one", "two"));
			provider.ReplacePage(2, "id2", Modified, Paras("three"));
			provider.ReplacePage(3, "id3", Modified, Paras("four", "five", "six"));

			var all = provider.ReadStatistics();
			Assert.AreEqual(3, all.Pages);
			Assert.AreEqual(6, all.Paragraphs);

			var b = provider.ReadStatistics(key => key == "b");
			Assert.AreEqual(2, b.Pages);
			Assert.AreEqual(4, b.Paragraphs);

			var none = provider.ReadStatistics(_ => false);
			Assert.AreEqual(0, none.Pages);
		}


		[TestMethod]
		public void WriteNotebook_Upserts_AndKeepsLastIndexedWhenNotGiven()
		{
			provider.WriteNotebook("nb1", true, "2026-02-01T00:00:00.000Z");
			provider.WriteNotebook("nb2", false);
			provider.WriteNotebook("nb1", false);

			var notebooks = provider.ReadNotebooks();
			Assert.AreEqual(2, notebooks.Count);

			var nb1 = notebooks.Single(n => n.NotebookKey == "nb1");
			Assert.IsFalse(nb1.Included);
			Assert.AreEqual("2026-02-01T00:00:00.000Z", nb1.LastIndexed);

			var nb2 = notebooks.Single(n => n.NotebookKey == "nb2");
			Assert.IsFalse(nb2.Included);
			Assert.IsNull(nb2.LastIndexed);
		}


	}


	[TestClass]
	public class TextIndexSchemaTests
	{
		private static List<(string Type, string Name)> Entities()
		{
			return Regex.Split(Resources.TextIndexDB, @"\r\n|\n\r|\n")
				.Select(d => DatabaseProvider.EntityPattern.Match(d))
				.Where(m => m.Success)
				.Select(m => (m.Groups[1].Value, m.Groups[2].Value))
				.ToList();
		}


		[TestMethod]
		public void EntityPattern_RecognizesVirtualTablesAsTablesAndTriggers()
		{
			var entities = Entities();

			CollectionAssert.Contains(entities, ("TABLE", "index_fts"));
			CollectionAssert.Contains(entities, ("TABLE", "index_text"));
			CollectionAssert.Contains(entities, ("INDEX", "IDX_index_text_pageKey"));
			CollectionAssert.Contains(entities, ("TRIGGER", "index_text_ai"));
			Assert.AreEqual(3, entities.Count(e => e.Type == "TRIGGER"));
		}


		[TestMethod]
		public void EntityPattern_StillRecognizesUniqueIndexes()
		{
			var m = DatabaseProvider.EntityPattern.Match(
				"CREATE UNIQUE INDEX IF NOT EXISTS IDX_x ON t (a);");

			Assert.IsTrue(m.Success);
			Assert.AreEqual("INDEX", m.Groups[1].Value);
			Assert.AreEqual("IDX_x", m.Groups[2].Value);
		}


		[TestMethod]
		public void DroppingTheEntities_InDropCatalogOrder_RemovesEverything()
		{
			if (!IndexProvider.IsAvailable)
			{
				Assert.Inconclusive("FTS5 with the trigram tokenizer cannot be loaded here");
			}

			using var connection = new SQLiteConnection("Data Source=:memory:");
			connection.Open();
			using var provider = new IndexProvider(connection);

			// DropCatalog drops triggers, then views, then indexes, then tables
			var order = new[] { "TRIGGER", "VIEW", "INDEX", "TABLE" };
			foreach (var type in order)
			{
				foreach (var entity in Entities().Where(e => e.Type == type))
				{
					using var cmd = connection.CreateCommand();
					cmd.CommandText = $"DROP {type} IF EXISTS {entity.Name}";
					cmd.ExecuteNonQuery();
				}
			}

			using var check = connection.CreateCommand();
			check.CommandText =
				"SELECT COUNT(1) FROM sqlite_master WHERE name LIKE 'index\\_%' ESCAPE '\\' " +
				"OR name LIKE 'IDX\\_index%' ESCAPE '\\'";

			Assert.AreEqual(0L, check.ExecuteScalar());
		}
	}
}
