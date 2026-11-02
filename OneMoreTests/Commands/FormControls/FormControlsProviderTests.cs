//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.FormControls
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using System.Data.SQLite;
	using System.Linq;


	[TestClass]
	public class FormControlsProviderTests
	{
		private SQLiteConnection connection;


		[TestInitialize]
		public void Setup()
		{
			connection = new SQLiteConnection("Data Source=:memory:");
			connection.Open();
		}


		[TestCleanup]
		public void Teardown()
		{
			connection.Dispose();
		}


		[TestMethod]
		public void WriteLists_ReadLists_RoundTrip_ListsAlphabeticItemsInOrder()
		{
			using var provider = new FormControlsProvider(connection);

			var fruit = new FormList("Fruit");
			fruit.Items.AddRange(new[] { "Pear", "Apple", "Mango" });
			var colors = new FormList("Colors");
			colors.Items.Add("Red");

			Assert.IsTrue(provider.WriteLists(new[] { fruit, colors }));

			var lists = provider.ReadLists();
			Assert.AreEqual(2, lists.Count);
			Assert.AreEqual("Colors", lists[0].Name);
			CollectionAssert.AreEqual(new[] { "Red" }, lists[0].Items);
			Assert.AreEqual("Fruit", lists[1].Name);
			CollectionAssert.AreEqual(new[] { "Pear", "Apple", "Mango" }, lists[1].Items);
		}


		[TestMethod]
		public void WriteLists_ReplacesPreviousLists()
		{
			using var provider = new FormControlsProvider(connection);

			var first = new FormList("One");
			first.Items.Add("a");
			provider.WriteLists(new[] { first });

			var second = new FormList("Two");
			provider.WriteLists(new[] { second });

			var lists = provider.ReadLists();
			Assert.AreEqual(1, lists.Count);
			Assert.AreEqual("Two", lists[0].Name);
			Assert.AreEqual(0, lists[0].Items.Count);
		}


		[TestMethod]
		public void WriteLists_DuplicateNames_RollsBackAndKeepsExisting()
		{
			using var provider = new FormControlsProvider(connection);

			var original = new FormList("Keep");
			original.Items.Add("x");
			provider.WriteLists(new[] { original });

			var written = provider.WriteLists(new[] { new FormList("Dup"), new FormList("dup") });
			Assert.IsFalse(written);

			var lists = provider.ReadLists();
			Assert.AreEqual(1, lists.Count);
			Assert.AreEqual("Keep", lists[0].Name);
			CollectionAssert.AreEqual(new[] { "x" }, lists[0].Items);
		}


		[TestMethod]
		public void ReadItems_IsCaseInsensitiveAndEmptyWhenMissing()
		{
			using var provider = new FormControlsProvider(connection);

			var fruit = new FormList("Fruit");
			fruit.Items.AddRange(new[] { "Pear", "Apple" });
			provider.WriteLists(new[] { fruit });

			CollectionAssert.AreEqual(new[] { "Pear", "Apple" }, provider.ReadItems("fRUIT"));
			Assert.AreEqual(0, provider.ReadItems("Missing").Count);
		}


		[TestMethod]
		public void Setting_DefaultsThenRoundTrips()
		{
			using var provider = new FormControlsProvider(connection);

			Assert.AreEqual(
				FormControlsProvider.DefaultDateFormat,
				provider.ReadSetting(
					FormControlsProvider.DateFormatKey, FormControlsProvider.DefaultDateFormat));

			Assert.IsTrue(provider.WriteSetting(FormControlsProvider.DateFormatKey, "yyyy-MM-dd"));
			Assert.IsTrue(provider.WriteSetting(FormControlsProvider.DateFormatKey, "MM/dd/yyyy"));

			Assert.AreEqual(
				"MM/dd/yyyy",
				provider.ReadSetting(FormControlsProvider.DateFormatKey, "x"));
		}
	}
}
