//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.TextIndex
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.TextIndex;
	using System.Collections.Generic;
	using System.Data.SQLite;
	using System.Linq;
	using System.Threading;


	[TestClass]
	public class IndexedHitsTests
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


		// the corpus the parity test runs over
		private static readonly string[] Corpus =
		{
			"The quick brown fox",
			"Notebook and bookmark",
			"error: disk full",
			"warning: disk almost full",
			"Error and failure together",
			"a b c",
			"page identity layer",
			"identity of the page",
			"HASHTAG scanner",
			"tom & jerry",
			"naïve café"
		};


		private IndexedHits Load(string query, bool caseSensitive = false, bool asRegex = false)
		{
			var builder = new TextMatchBuilder(asRegex, caseSensitive);
			var finder = builder.BuildRegex(query);

			return IndexedHits.Load(
				provider, builder.BuildIndexQuery(query), finder, CancellationToken.None);
		}


		[TestMethod]
		public void Load_FindsParagraphsOfCurrentPages()
		{
			provider.ReplacePage(1, "id1", Modified, Paras("alpha one", "beta"));
			provider.ReplacePage(2, "id2", Modified, Paras("alpha two"));

			var result = Load("alpha");

			Assert.IsTrue(result.TryGet("id1", Modified, out var hits1));
			Assert.AreEqual(1, hits1.Count);
			Assert.AreEqual("alpha one", hits1[0].PlainText);
			Assert.AreEqual("{o0}", hits1[0].ObjectId);

			Assert.IsTrue(result.TryGet("id2", Modified, out var hits2));
			Assert.AreEqual(1, hits2.Count);
			Assert.AreEqual(2, result.Paragraphs);
		}


		[TestMethod]
		public void Load_ImageText_OnlyWhenRequested_AndMarked()
		{
			provider.ReplacePage(1, "id1", Modified, new List<ParagraphText>
			{
				new ParagraphText { ObjectID = "{t}", Ordinal = 0, Text = "alpha typed" },
				new ParagraphText
				{
					ObjectID = "{i}", Ordinal = 1, Text = "alpha &amp; picture", Kind = ParagraphKind.Image
				}
			});

			var builder = new TextMatchBuilder(false, false);
			var finder = builder.BuildRegex("alpha");
			var match = builder.BuildIndexQuery("alpha");

			var without = IndexedHits.Load(provider, match, finder, CancellationToken.None);
			Assert.IsTrue(without.TryGet("id1", Modified, out var typed));
			Assert.AreEqual(1, typed.Count);
			Assert.IsFalse(typed[0].IsImageText);

			var with = IndexedHits.Load(provider, match, finder, CancellationToken.None, includeOcr: true);
			Assert.IsTrue(with.TryGet("id1", Modified, out var both));
			Assert.AreEqual(2, both.Count);
			Assert.IsTrue(both[1].IsImageText);

			// image text is plain text, so it is not HTML decoded for display
			Assert.AreEqual("alpha &amp; picture", both[1].PlainText);
		}


		[TestMethod]
		public void TryGet_PageWithoutMatches_IsCurrentAndEmpty()
		{
			provider.ReplacePage(1, "id1", Modified, Paras("alpha"));
			provider.ReplacePage(2, "id2", Modified, Paras("bravo"));

			var result = Load("alpha");

			// the index knows page 2 and found nothing; it need not be read
			Assert.IsTrue(result.TryGet("id2", Modified, out var hits));
			Assert.AreEqual(0, hits.Count);
		}


		[TestMethod]
		public void TryGet_PageChangedSinceItWasIndexed_IsNotAnswered()
		{
			provider.ReplacePage(1, "id1", Modified, Paras("alpha"));

			var result = Load("alpha");

			Assert.IsFalse(result.TryGet("id1", "2026-03-01T00:00:00.000Z", out _));
		}


		[TestMethod]
		public void TryGet_PageGivenANewID_IsNotAnswered()
		{
			// a reopened notebook; the page's paragraphs have new object IDs too
			provider.ReplacePage(1, "id1", Modified, Paras("alpha"));

			var result = Load("alpha");

			Assert.IsFalse(result.TryGet("id1-reopened", Modified, out _));
		}


		[TestMethod]
		public void TryGet_PageNeverIndexed_IsNotAnswered()
		{
			provider.ReplacePage(1, "id1", Modified, Paras("alpha"));

			var result = Load("alpha");

			Assert.IsFalse(result.TryGet("other", Modified, out _));
			Assert.IsFalse(result.TryGet(null, Modified, out _));
			Assert.IsFalse(result.TryGet("id1", null, out _));
		}


		[TestMethod]
		public void Load_CaseSensitive_IsNarrowedByTheRegex()
		{
			provider.ReplacePage(1, "id1", Modified, Paras("Error here", "error there"));

			var insensitive = Load("error");
			insensitive.TryGet("id1", Modified, out var both);
			Assert.AreEqual(2, both.Count);

			var sensitive = Load("error", caseSensitive: true);
			sensitive.TryGet("id1", Modified, out var one);
			Assert.AreEqual(1, one.Count);
			Assert.AreEqual("error there", one[0].PlainText);
		}


		[TestMethod]
		public void Load_ShortTerm_ScansEveryParagraph()
		{
			provider.ReplacePage(1, "id1", Modified, Paras("a b c", "abc"));

			// two characters cannot be looked up by the trigram index, so every paragraph is tested
			var result = Load("bc");

			result.TryGet("id1", Modified, out var hits);
			Assert.AreEqual(1, hits.Count);
			Assert.AreEqual("abc", hits[0].PlainText);
		}


		[TestMethod]
		public void Load_DecodesHitsForDisplayButMatchesTheRawText()
		{
			provider.ReplacePage(1, "id1", Modified, Paras("tom &amp; jerry"));

			var result = Load("amp");

			result.TryGet("id1", Modified, out var hits);
			Assert.AreEqual(1, hits.Count);
			Assert.AreEqual("tom & jerry", hits[0].PlainText);
		}


		[TestMethod]
		public void Load_Cancelled_Throws()
		{
			provider.ReplacePage(1, "id1", Modified, Paras("alpha"));

			using var source = new CancellationTokenSource();
			source.Cancel();

			var builder = new TextMatchBuilder(false, false);
			var finder = builder.BuildRegex("alpha");

			Assert.ThrowsException<System.OperationCanceledException>(() =>
				IndexedHits.Load(provider, builder.BuildIndexQuery("alpha"), finder, source.Token));
		}


		// The index must give exactly what scanning the same paragraphs with the regex gives.
		[TestMethod]
		public void Load_GivesWhatTheRegexGivesOverTheCorpus()
		{
			provider.ReplacePage(1, "id1", Modified, Paras(Corpus));

			var queries = new[]
			{
				"error", "book", "*ookma*", "error OR warning", "disk AND NOT warning",
				"disk NOT warning", "\"page identity\"", "identity page", "hash*", "ab", "a b",
				"NOT disk", "fail* OR quick", "tom &amp", "café", "naïve", "full NOT almost",
				"(error OR warning) AND disk"
			};

			foreach (var query in queries)
			{
				foreach (var caseSensitive in new[] { false, true })
				{
					var finder = new TextMatchBuilder(false, caseSensitive).BuildRegex(query);
					var expected = Corpus.Where(p => finder.IsMatch(p)).ToList();

					var result = Load(query, caseSensitive);
					result.TryGet("id1", Modified, out var hits);

					CollectionAssert.AreEqual(expected,
						hits.Select(h => h.PlainText).ToList(),
						$"query [{query}] caseSensitive={caseSensitive}");
				}
			}
		}


		[TestMethod]
		public void Load_RegularExpression_ScansTheText()
		{
			provider.ReplacePage(1, "id1", Modified, Paras(Corpus));

			var finder = new TextMatchBuilder(true, false).BuildRegex("^error.*full$");
			var result = Load("^error.*full$", asRegex: true);

			result.TryGet("id1", Modified, out var hits);
			CollectionAssert.AreEqual(
				Corpus.Where(p => finder.IsMatch(p)).ToList(),
				hits.Select(h => h.PlainText).ToList());
			Assert.IsTrue(hits.Count > 0);
		}
	}
}
