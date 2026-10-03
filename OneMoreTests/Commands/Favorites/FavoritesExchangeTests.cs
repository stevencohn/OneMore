//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Favorites
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using River.OneMoreAddIn.Commands.Favorites;
	using River.OneMoreAddIn.Commands.Workspaces;
	using River.OneMoreAddIn.Identity;
	using River.OneMoreAddIn.Tests.Commands.Workspaces;
	using System.Data.SQLite;
	using System.Linq;


	[TestClass]
	public class FavoritesExchangeTests
	{
		private const string Created = "2026-01-01T00:00:00.000Z";


		private static string Link(string id) => "link:" + id;


		private static IdentityRow Row(long key, string title = "Page", string section = "/Inbox")
		{
			return new IdentityRow
			{
				PageKey = key,
				Title = title,
				Created = Created,
				NotebookKey = "https://x/notes",
				SectionKey = section
			};
		}


		private static Favorite PageFavorite(long? key = null)
		{
			return new Favorite
			{
				ID = 5,
				Name = "Page",
				Location = "/Notes/Inbox/Page",
				Uri = "onenote:old",
				NotebookID = "{nb-foreign}",
				SectionID = "{sec-foreign}",
				PageID = "{page-foreign}",
				PageKey = key
			};
		}


		private static TargetFingerprint PageFingerprint(string notebookKey = "notes", string sectionKey = "/Inbox")
		{
			return new TargetFingerprint
			{
				Title = "Page",
				Created = "2026-01-01T00:00:00.000Z",
				NotebookKey = notebookKey,
				SectionKey = sectionKey
			};
		}


		#region Export
		[TestMethod]
		public void APageFavorite_GetsItsFingerprintFromTheIdentityCatalog()
		{
			var print = FavoritesExchange.FingerprintOf(PageFavorite(42), key => Row(key));

			Assert.AreEqual("Page", print.Title);
			Assert.AreEqual(Created, print.Created);
			Assert.AreEqual("https://x/notes", print.NotebookKey);
			Assert.AreEqual("/Inbox", print.SectionKey);
			Assert.IsTrue(print.IdentifiesAPage);
		}


		[TestMethod]
		public void APageFavoriteNotYetFound_HasNoFingerprint()
		{
			Assert.IsNull(FavoritesExchange.FingerprintOf(PageFavorite(null), key => Row(key)));
			Assert.IsNull(FavoritesExchange.FingerprintOf(PageFavorite(42), key => null));
		}


		[TestMethod]
		public void AContainer_CarriesOnlyItsKeys()
		{
			var section = new Favorite { NotebookKey = "https://x/notes", SectionKey = "/Inbox", SectionID = "{s}" };
			var notebook = new Favorite
			{
				NotebookKey = "https://x/notes",
				SectionID = "{nb}",
				Kind = Favorite.KindNotebook
			};

			var a = FavoritesExchange.FingerprintOf(section, key => null);
			var b = FavoritesExchange.FingerprintOf(notebook, key => null);

			Assert.AreEqual("/Inbox", a.SectionKey);
			Assert.IsNull(a.Title);
			Assert.IsNull(b.SectionKey);
			Assert.IsFalse(a.IdentifiesAPage);
		}


		[TestMethod]
		public void AContainerNotYetFound_HasNoFingerprint()
		{
			var legacy = new Favorite { SectionID = "{s}", Location = "/Notes/Inbox" };

			Assert.IsNull(FavoritesExchange.FingerprintOf(legacy, key => null));
		}


		[TestMethod]
		public void AddFingerprints_ReachesEveryFavorite_InFoldersAndInTheRoot()
		{
			var collection = new FavoritesCollection();
			var inFolder = PageFavorite(1);
			var folder = new FavoritesFolder { Name = "F" };
			folder.Items.Add(inFolder);
			collection.Folders.Add(folder);

			var inRoot = PageFavorite(2);
			collection.Items.Add(inRoot);

			FavoritesExchange.AddFingerprints(collection, key => Row(key));

			Assert.IsNotNull(inFolder.Fingerprint);
			Assert.IsNotNull(inRoot.Fingerprint);
		}
		#endregion Export


		#region The file
		[TestMethod]
		public void TheFile_CarriesTheFingerprint_AndNeverAPageKeyOrNotebookKeyOfItsOwn()
		{
			var collection = new FavoritesCollection();
			var favorite = PageFavorite(42);
			favorite.NotebookKey = "https://x/notes";
			favorite.SectionKey = "/Inbox";
			collection.Items.Add(favorite);

			FavoritesExchange.AddFingerprints(collection, key => Row(key));

			var json = JsonConvert.SerializeObject(collection, Formatting.Indented);
			var item = JObject.Parse(json)["Items"][0];

			Assert.IsNull(item["PageKey"], "a page key belongs to one database");
			Assert.IsNull(item["NotebookKey"]);
			Assert.IsNull(item["SectionKey"]);
			Assert.AreEqual("Page", (string)item["Fingerprint"]["Title"]);
			Assert.AreEqual("https://x/notes", (string)item["Fingerprint"]["NotebookKey"]);
			Assert.IsFalse(json.Contains("\"PageKey\""));
		}


		[TestMethod]
		public void TheFingerprintInTheFile_HoldsOnlyWhatIdentifiesTheTarget()
		{
			var collection = new FavoritesCollection();
			var page = PageFavorite(42);
			var section = new Favorite
			{
				NotebookKey = "https://x/notes",
				SectionKey = "/Inbox",
				SectionID = "{s}",
				Location = "/Notes/Inbox"
			};

			collection.Items.Add(page);
			collection.Items.Add(section);
			FavoritesExchange.AddFingerprints(collection, key => Row(key));

			var items = JObject.Parse(JsonConvert.SerializeObject(collection))["Items"];
			var pagePrint = (JObject)items[0]["Fingerprint"];
			var sectionPrint = (JObject)items[1]["Fingerprint"];

			CollectionAssert.AreEquivalent(
				new[] { "Title", "Created", "NotebookKey", "SectionKey" },
				pagePrint.Properties().Select(p => p.Name).ToList());

			// a section has no title or creation time, and a helper property must not leak in
			CollectionAssert.AreEquivalent(
				new[] { "NotebookKey", "SectionKey" },
				sectionPrint.Properties().Select(p => p.Name).ToList());
		}


		[TestMethod]
		public void WithoutAFingerprint_TheFileHasNoFingerprintProperty()
		{
			var collection = new FavoritesCollection();
			collection.Items.Add(PageFavorite(null));

			var json = JsonConvert.SerializeObject(collection);

			Assert.IsFalse(json.Contains("Fingerprint"));
		}


		[TestMethod]
		public void AFileInTheOldFormat_StillReads_WithoutKeysOrAFingerprint()
		{
			// as written before favorites had keys or fingerprints
			const string old = @"{
				""SchemaVersion"": 1,
				""Folders"": [ { ""FolderID"": 1, ""Name"": ""Cars"", ""Items"": [
					{ ""ID"": 3, ""FolderID"": 1, ""Name"": ""VINs"", ""Alias"": null,
					  ""Location"": ""/Personal/Automobiles/VINs"", ""Uri"": ""onenote:x#VINs&section-id={AAAAAAAA-0000-0000-0000-000000000000}&page-id={BBBBBBBB-0000-0000-0000-000000000000}&end"",
					  ""NotebookID"": ""{nb}"", ""SectionID"": ""{sec}"", ""PageID"": ""{page}"", ""Kind"": null, ""SortOrder"": 2 } ] } ],
				""Items"": [
					{ ""ID"": 4, ""FolderID"": 0, ""Name"": ""Notes"", ""Alias"": ""Mine"", ""Location"": ""/Notes"",
					  ""Uri"": ""{nb}"", ""NotebookID"": ""{nb}"", ""SectionID"": ""{nb}"", ""PageID"": null,
					  ""Kind"": ""notebook"", ""SortOrder"": 0 } ] }";

			var collection = JsonConvert.DeserializeObject<FavoritesCollection>(old);

			Assert.AreEqual(1, collection.Folders.Count);
			Assert.AreEqual("VINs", collection.Folders[0].Items[0].Name);
			Assert.AreEqual("Mine", collection.Items[0].Alias);

			var all = collection.Folders.SelectMany(f => f.Items).Concat(collection.Items).ToList();
			Assert.IsTrue(all.All(f => f.PageKey is null && f.NotebookKey is null && f.SectionKey is null));
			Assert.IsTrue(all.All(f => f.Fingerprint is null));
		}


		[TestMethod]
		public void AStrayKeyInAFile_IsIgnored()
		{
			const string stray = @"{ ""Items"": [ { ""Name"": ""X"", ""Location"": ""/N/S/X"", ""Uri"": ""u"",
				""NotebookID"": ""{nb}"", ""SectionID"": ""{s}"", ""PageID"": ""{p}"",
				""PageKey"": 99, ""NotebookKey"": ""evil"", ""SectionKey"": ""/evil"" } ] }";

			var favorite = JsonConvert.DeserializeObject<FavoritesCollection>(stray).Items.Single();

			Assert.IsNull(favorite.PageKey);
			Assert.IsNull(favorite.NotebookKey);
			Assert.IsNull(favorite.SectionKey);
		}


		[TestMethod]
		public void UnknownPropertiesInAFile_AreIgnored()
		{
			const string future = @"{ ""FormatVersion"": 9, ""SomethingNew"": [1, 2],
				""Items"": [ { ""Name"": ""X"", ""Location"": ""/N/S/X"", ""Uri"": ""u"",
				""NotebookID"": ""{nb}"", ""SectionID"": ""{s}"", ""PageID"": ""{p}"", ""Color"": ""red"" } ] }";

			var collection = JsonConvert.DeserializeObject<FavoritesCollection>(future);

			Assert.AreEqual("X", collection.Items.Single().Name);
		}


		[TestMethod]
		public void TheFingerprint_RoundTripsThroughAFile()
		{
			var collection = new FavoritesCollection();
			var favorite = PageFavorite(42);
			collection.Items.Add(favorite);
			FavoritesExchange.AddFingerprints(collection, key => Row(key));

			var read = JsonConvert.DeserializeObject<FavoritesCollection>(
				JsonConvert.SerializeObject(collection)).Items.Single();

			Assert.AreEqual("Page", read.Fingerprint.Title);
			Assert.AreEqual(Created, read.Fingerprint.Created);
			Assert.IsNull(read.PageKey);
		}
		#endregion The file


		#region Import
		[TestMethod]
		public void APageFromAnotherMachine_IsFoundByItsFingerprint_AndStoredWithWhatIsTrueHere()
		{
			var book = Snap.Book("{nb-here}", "Notes", "https://x/notes");
			Snap.Page(book, 7, "{page-here}", "Page", "Inbox", sectionID: "{sec-here}");

			var favorite = PageFavorite();
			favorite.Fingerprint = PageFingerprint("https://x/notes", "/Inbox");

			var found = FavoritesExchange.Prepare(favorite, new TargetResolver(Snap.Of(0, book)), Link);

			Assert.AreEqual(ResolveMethod.Fingerprint, found.Method);
			Assert.AreEqual(7L, favorite.PageKey);
			Assert.AreEqual("{page-here}", favorite.PageID);
			Assert.AreEqual("{sec-here}", favorite.SectionID);
			Assert.AreEqual("{nb-here}", favorite.NotebookID);
			Assert.AreEqual("link:{page-here}", favorite.Uri);
			Assert.AreEqual("Page", favorite.Name, "the name is the user's");
		}


		[TestMethod]
		public void APageInACloudNotebook_IsFoundByTheGuidInItsLink_EvenWithoutAFingerprint()
		{
			var book = Snap.Book("{nb-here}", "Notes");
			Snap.Page(book, 7, "{page-here}", "Page", guid: Snap.G1);

			var favorite = PageFavorite();
			favorite.Uri = "onenote:x#Page&section-id={AAAAAAAA-0000-0000-0000-000000000000}&page-id=" + Snap.G1 + "&end";

			var found = FavoritesExchange.Prepare(favorite, new TargetResolver(Snap.Of(0, book)), Link);

			Assert.AreEqual(ResolveMethod.Guid, found.Method);
			Assert.AreEqual(7L, favorite.PageKey);
		}


		[TestMethod]
		public void AKeyCarriedByTheFile_IsDiscarded_EvenIfNothingIsFound()
		{
			var favorite = PageFavorite(99);

			FavoritesExchange.Prepare(favorite, new TargetResolver(Snap.Of(0, Snap.Book("{nb}", "Other"))), Link);

			Assert.IsNull(favorite.PageKey);
		}


		[TestMethod]
		public void APageThatIsNotFound_IsStoredAsItCame_WithNoKeys()
		{
			var favorite = PageFavorite();
			favorite.Fingerprint = PageFingerprint("https://x/notes", "/Inbox");

			FavoritesExchange.Prepare(favorite, new TargetResolver(Snap.Of(0, Snap.Book("{nb}", "Other"))), Link);

			// a page with notebook and section keys but no page key would be taken for a section
			Assert.IsNull(favorite.NotebookKey);
			Assert.IsNull(favorite.SectionKey);
			Assert.AreEqual("{page-foreign}", favorite.PageID);
		}


		[TestMethod]
		public void ASectionFromAnotherMachine_IsFoundByTheKeysInItsFingerprint()
		{
			var book = Snap.Book("{nb-here}", "Notes", "https://x/notes");
			Snap.Section(book, "{sec-here}", "Inbox");

			var favorite = new Favorite
			{
				Name = "Inbox",
				Location = "/Notes/Inbox",
				Uri = "onenote:foreign",
				NotebookID = "{nb-foreign}",
				SectionID = "{sec-foreign}",
				Fingerprint = new TargetFingerprint { NotebookKey = "https://x/notes", SectionKey = "/Inbox" }
			};

			var found = FavoritesExchange.Prepare(favorite, new TargetResolver(Snap.Of(0, book)), Link);

			Assert.AreEqual(ResolveMethod.Key, found.Method);
			Assert.AreEqual("{sec-here}", favorite.SectionID);
			Assert.AreEqual("https://x/notes", favorite.NotebookKey);
		}


		[TestMethod]
		public void ANotebookFromAnotherMachine_IsFoundByItsKey_AndOpenedByItsIDHere()
		{
			var book = Snap.Book("{nb-here}", "Notes", "https://x/notes");

			var favorite = new Favorite
			{
				Name = "Notes",
				Location = "/Notes",
				Uri = "{nb-foreign}",
				NotebookID = "{nb-foreign}",
				SectionID = "{nb-foreign}",
				Kind = Favorite.KindNotebook,
				Fingerprint = new TargetFingerprint { NotebookKey = "https://x/notes" }
			};

			FavoritesExchange.Prepare(favorite, new TargetResolver(Snap.Of(0, book)), Link);

			Assert.AreEqual("{nb-here}", favorite.SectionID);
			Assert.AreEqual("{nb-here}", favorite.Uri);
		}


		[TestMethod]
		public void AContainerWhoseNotebookIsClosed_KeepsTheFilesKeys_SoItCanBeFoundWhenItOpens()
		{
			var favorite = new Favorite
			{
				Name = "Inbox",
				Location = "/Archive/Inbox",
				Uri = "onenote:foreign",
				NotebookID = "{nb-foreign}",
				SectionID = "{sec-foreign}",
				Fingerprint = new TargetFingerprint { NotebookKey = "https://x/archive", SectionKey = "/Inbox" }
			};

			var other = Snap.Book("{nb}", "Notes", "https://x/notes");
			FavoritesExchange.Prepare(favorite, new TargetResolver(Snap.Of(0, other)), Link);

			Assert.AreEqual("https://x/archive", favorite.NotebookKey);
			Assert.AreEqual("/Inbox", favorite.SectionKey);
		}


		[TestMethod]
		public void WhenOneNoteCannotBeRead_TheFavoriteIsStoredAsItCame_ContainersKeepTheirKeys()
		{
			var page = PageFavorite();
			page.Fingerprint = PageFingerprint();

			var section = new Favorite
			{
				Location = "/Notes/Inbox",
				SectionID = "{s}",
				Fingerprint = new TargetFingerprint { NotebookKey = "notes", SectionKey = "/Inbox" }
			};

			Assert.IsNull(FavoritesExchange.Prepare(page, null, Link));
			Assert.IsNull(FavoritesExchange.Prepare(section, null, Link));

			Assert.IsNull(page.NotebookKey);
			Assert.AreEqual("notes", section.NotebookKey);
		}
		#endregion Import


		#region Duplicates on import
		[TestMethod]
		public void ImportingAPageAlreadyHere_IsRejected_EvenThoughItsIDsAreDifferent()
		{
			var connection = new SQLiteConnection("Data Source=:memory:");
			connection.Open();
			using var provider = new FavoritesProvider(connection);

			var book = Snap.Book("{nb}", "Notes", "https://x/notes");
			Snap.Page(book, 7, "{page-here}", "Page", "Inbox", sectionID: "{sec-here}");
			var resolver = new TargetResolver(Snap.Of(0, book));

			// already a favorite here, keyed
			var mine = PageFavorite();
			mine.Fingerprint = PageFingerprint("https://x/notes", "/Inbox");
			FavoritesExchange.Prepare(mine, resolver, Link);
			Assert.IsTrue(provider.WriteFavorite(mine));

			// the same page again, from a file made before OneNote changed its IDs
			var again = PageFavorite();
			again.PageID = "{an-older-page-id}";
			again.SectionID = "{an-older-section-id}";
			again.Fingerprint = PageFingerprint("https://x/notes", "/Inbox");
			FavoritesExchange.Prepare(again, resolver, Link);

			Assert.IsFalse(provider.WriteFavorite(again, out var duplicate));
			Assert.IsTrue(duplicate);
		}


		[TestMethod]
		public void AnUnresolvedPage_DoesNotCollideWithASectionFavoriteOfItsSection()
		{
			var connection = new SQLiteConnection("Data Source=:memory:");
			connection.Open();
			using var provider = new FavoritesProvider(connection);

			var book = Snap.Book("{nb}", "Notes", "https://x/notes");
			Snap.Section(book, "{sec}", "Inbox");
			var resolver = new TargetResolver(Snap.Of(0, book));

			var section = new Favorite
			{
				Name = "Inbox",
				Location = "/Notes/Inbox",
				Uri = "u",
				NotebookID = "{nb}",
				SectionID = "{sec}",
				NotebookKey = "https://x/notes",
				SectionKey = "/Inbox"
			};

			Assert.IsTrue(provider.WriteFavorite(section));

			// a page of that section that is not found: it must not be taken for the section
			var page = PageFavorite();
			page.Fingerprint = PageFingerprint("https://x/notes", "/Inbox");
			FavoritesExchange.Prepare(page, resolver, Link);

			Assert.IsTrue(provider.WriteFavorite(page, out var duplicate));
			Assert.IsFalse(duplicate);
		}
		#endregion Duplicates on import
	}


	[TestClass]
	public class TargetResolverFingerprintTests
	{
		private static TargetQuery Query(string title = "Page", string created = "2026-01-01T00:00:00.000Z",
			string notebookKey = "notes", string sectionKey = "/Inbox")
		{
			return new TargetQuery
			{
				PageID = "{foreign}",
				Uri = "onenote:foreign",
				Location = "/Elsewhere/Path/Name",
				Title = title,
				Created = created,
				NotebookKey = notebookKey,
				SectionKey = sectionKey
			};
		}


		[TestMethod]
		public void ByTitleCreatedNotebookAndSection_Confident()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Page");
			Snap.Page(book, 8, "{q}", "Another");

			var result = new TargetResolver(Snap.Of(0, book)).Resolve(Query());

			Assert.AreEqual(ResolveMethod.Fingerprint, result.Method);
			Assert.AreEqual(7L, result.PageKey);
			Assert.IsTrue(result.IsConfident);
		}


		[TestMethod]
		public void ADifferentCreationTime_IsNotTheSamePage()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Page");

			var result = new TargetResolver(Snap.Of(0, book)).Resolve(Query(created: "2020-05-05T00:00:00.000Z"));

			// the page is not taken for the exported one; nothing else in the query names it either
			Assert.IsFalse(result.IsResolved);
			Assert.AreNotEqual(ResolveMethod.Fingerprint, result.Method);
		}


		[TestMethod]
		public void TwoIdenticalPages_AreAmbiguous()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{a}", "Page");
			Snap.Page(book, 8, "{b}", "Page");

			var result = new TargetResolver(Snap.Of(0, book)).Resolve(Query());

			Assert.AreEqual(ResolveOutcome.Ambiguous, result.Outcome);
		}


		[TestMethod]
		public void APartialFingerprint_IsNotUsedToFindAPage()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Page");

			var result = new TargetResolver(Snap.Of(0, book)).Resolve(Query(created: null));

			Assert.AreNotEqual(ResolveMethod.Fingerprint, result.Method);
		}


		[TestMethod]
		public void TheGuidStillComesFirst_AndAnUnreadGuidStillMeansPending()
		{
			var book = Snap.Book("{nb}", "Notes");
			Snap.Page(book, 7, "{p}", "Page", guid: Snap.G1);
			Snap.Page(book, 8, "{q}", "Other");

			var byGuid = Query();
			byGuid.Uri = "onenote:x#a&section-id={AAAAAAAA-0000-0000-0000-000000000000}&page-id=" + Snap.G1 + "&end";
			Assert.AreEqual(ResolveMethod.Guid, new TargetResolver(Snap.Of(0, book)).Resolve(byGuid).Method);

			var pending = new TargetQuery
			{
				PageID = "{old}",
				Uri = "onenote:x#a&section-id={AAAAAAAA-0000-0000-0000-000000000000}&page-id=" + Snap.G3 + "&end",
				Location = "/Notes/Inbox/Other"
			};

			Assert.AreEqual(ResolveOutcome.Pending, new TargetResolver(Snap.Of(5, book)).Resolve(pending).Outcome);
		}
	}
}
