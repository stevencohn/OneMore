//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Tagging
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using System.Collections.Generic;
	using System.Linq;


	[TestClass]
	public class HashtagFreshenerTests
	{
		private static Hashtag Tag(string moreID, string pageID, string tag = "#t")
		{
			return new Hashtag { Tag = tag, MoreID = moreID, PageID = pageID, ObjectID = "{o}" };
		}


		private static bool Always(long key) => true;


		[TestMethod]
		public void APageThatOneNoteStillKnows_IsNotStale()
		{
			var tags = new[] { Tag("42", "{page}") };

			Assert.AreEqual(0, HashtagFreshener.FindStale(tags, id => true, Always).Count);
		}


		[TestMethod]
		public void APageWhoseIDNoLongerExists_IsStale()
		{
			var tags = new[] { Tag("42", "{old-id}") };

			var stale = HashtagFreshener.FindStale(tags, id => false, Always);

			Assert.AreEqual(1, stale.Count);
			Assert.AreEqual("42", stale[0].MoreID);
		}


		[TestMethod]
		public void ManyTagsOnOnePage_ReportThePageOnce()
		{
			var tags = new[]
			{
				Tag("42", "{old-id}", "#one"),
				Tag("42", "{old-id}", "#two"),
				Tag("42", "{old-id}", "#three")
			};

			Assert.AreEqual(1, HashtagFreshener.FindStale(tags, id => false, Always).Count);
		}


		[TestMethod]
		public void OnlyTheStalePagesAreReported()
		{
			var tags = new[] { Tag("1", "{fine}"), Tag("2", "{gone}"), Tag("3", "{fine-too}") };

			var stale = HashtagFreshener.FindStale(tags, id => id != "{gone}", Always);

			CollectionAssert.AreEqual(new[] { "2" }, stale.Select(t => t.MoreID).ToList());
		}


		[TestMethod]
		public void APageInAClosedNotebook_IsSkipped()
		{
			// its ID does not exist because the notebook is closed, and the identity catalog has
			// it marked missing; refreshing it would only cost a pass over the hierarchy
			var tags = new[] { Tag("42", "{offline}") };

			Assert.AreEqual(0, HashtagFreshener.FindStale(tags, id => false, key => false).Count);
		}


		[TestMethod]
		public void AStampThatIsNotAPageKey_IsSkipped()
		{
			var tags = new[]
			{
				Tag("6f037c477bea4d8fa0a0366e9b5e3a89", "{old}"),
				Tag("", "{none}"),
				Tag(null, "{null}"),
				Tag("-5", "{negative}")
			};

			Assert.AreEqual(0, HashtagFreshener.FindStale(tags, id => false, Always).Count);
		}


		[TestMethod]
		public void TheIdentityCatalogIsOnlyAskedAboutPagesThatAreMissingFromOneNote()
		{
			var asked = new List<long>();
			var tags = new[] { Tag("1", "{fine}"), Tag("2", "{gone}") };

			HashtagFreshener.FindStale(tags, id => id == "{fine}", key => { asked.Add(key); return true; });

			CollectionAssert.AreEqual(new long[] { 2 }, asked);
		}


		[TestMethod]
		public void NoResults_NothingIsStale()
		{
			Assert.AreEqual(0, HashtagFreshener.FindStale(new Hashtag[0], id => false, Always).Count);
		}
	}
}
