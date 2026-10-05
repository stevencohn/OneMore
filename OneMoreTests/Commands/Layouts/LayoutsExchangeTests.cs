//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Layouts
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using River.OneMoreAddIn.Commands.Layouts;
	using River.OneMoreAddIn.Commands.Workspaces;
	using River.OneMoreAddIn.Identity;
	using River.OneMoreAddIn.Tests.Commands.Workspaces;
	using System.Data.SQLite;
	using System.Linq;


	[TestClass]
	public class LayoutsExchangeTests
	{
		private const string Created = "2026-01-01T00:00:00.000Z";


		private static string Link(string id) => "link:" + id;


		private static IdentityRow Row(long key)
		{
			return new IdentityRow
			{
				PageKey = key,
				Title = "Page",
				Created = Created,
				NotebookKey = "https://x/notes",
				SectionKey = "/Inbox"
			};
		}


		private static LayoutWindow Window(long? key = null, int layoutID = 1)
		{
			return new LayoutWindow
			{
				ID = 5,
				LayoutID = layoutID,
				Name = "Page",
				Location = "/Notes/Inbox/Page",
				Uri = "onenote:old",
				NotebookID = "{nb-foreign}",
				SectionID = "{sec-foreign}",
				PageID = "{page-foreign}",
				ZOrder = 1,
				PageKey = key
			};
		}


		private static TargetFingerprint Print()
		{
			return new TargetFingerprint
			{
				Title = "Page",
				Created = Created,
				NotebookKey = "https://x/notes",
				SectionKey = "/Inbox"
			};
		}


		#region Export
		[TestMethod]
		public void AWindow_GetsItsFingerprintFromTheIdentityCatalog()
		{
			var print = LayoutsExchange.FingerprintOf(Window(42), Row);

			Assert.AreEqual("Page", print.Title);
			Assert.AreEqual(Created, print.Created);
			Assert.AreEqual("https://x/notes", print.NotebookKey);
			Assert.AreEqual("/Inbox", print.SectionKey);
			Assert.IsTrue(print.IdentifiesAPage);
		}


		[TestMethod]
		public void AWindowNotYetFound_HasNoFingerprint()
		{
			Assert.IsNull(LayoutsExchange.FingerprintOf(Window(null), Row));
			Assert.IsNull(LayoutsExchange.FingerprintOf(Window(42), key => null));
		}


		[TestMethod]
		public void AddFingerprints_ReachesTheWindowsOfEveryLayout()
		{
			var collection = new LayoutsCollection();
			var one = new Layout { LayoutID = 1, Name = "One" };
			var two = new Layout { LayoutID = 2, Name = "Two" };
			one.Windows.Add(Window(1));
			two.Windows.Add(Window(2, 2));
			collection.Layouts.Add(one);
			collection.Layouts.Add(two);

			LayoutsExchange.AddFingerprints(collection, Row);

			Assert.IsTrue(collection.Layouts.SelectMany(l => l.Windows).All(w => w.Fingerprint is not null));
		}
		#endregion Export


		#region The file
		[TestMethod]
		public void TheFile_CarriesTheFingerprint_AndNeverAPageKey()
		{
			var collection = new LayoutsCollection();
			var layout = new Layout { LayoutID = 1, Name = "Work" };
			layout.Windows.Add(Window(42));
			collection.Layouts.Add(layout);

			LayoutsExchange.AddFingerprints(collection, Row);

			var json = JsonConvert.SerializeObject(collection, Formatting.Indented);
			var window = JObject.Parse(json)["Layouts"][0]["Windows"][0];

			Assert.IsNull(window["PageKey"], "a page key belongs to one database");
			Assert.IsFalse(json.Contains("\"PageKey\""));

			CollectionAssert.AreEquivalent(
				new[] { "Title", "Created", "NotebookKey", "SectionKey" },
				((JObject)window["Fingerprint"]).Properties().Select(p => p.Name).ToList());
		}


		[TestMethod]
		public void WithoutAFingerprint_TheFileHasNoFingerprintProperty()
		{
			var collection = new LayoutsCollection();
			var layout = new Layout { LayoutID = 1, Name = "Work" };
			layout.Windows.Add(Window(null));
			collection.Layouts.Add(layout);

			Assert.IsFalse(JsonConvert.SerializeObject(collection).Contains("Fingerprint"));
		}


		[TestMethod]
		public void AFileInTheOldFormat_StillReads_WithoutAKeyOrAFingerprint()
		{
			// as written before windows had keys or fingerprints
			const string old = @"{
				""SchemaVersion"": 1,
				""Layouts"": [ { ""LayoutID"": 1, ""Name"": ""Work"", ""Windows"": [
					{ ""ID"": 3, ""LayoutID"": 1, ""Name"": ""Plan"", ""Alias"": ""Main"",
					  ""Location"": ""/Notes/Inbox/Plan"", ""Uri"": ""onenote:x#Plan"",
					  ""NotebookID"": ""{nb}"", ""SectionID"": ""{sec}"", ""PageID"": ""{page}"", ""ZOrder"": 2,
					  ""Device"": ""\\\\.\\DISPLAY1"", ""WinLeft"": 10, ""WinTop"": 20, ""WinRight"": 800, ""WinBottom"": 600 } ] } ] }";

			var window = JsonConvert.DeserializeObject<LayoutsCollection>(old).Layouts.Single().Windows.Single();

			Assert.AreEqual("Main", window.Alias);
			Assert.AreEqual(2, window.ZOrder);
			Assert.AreEqual(800, window.WinRight);
			Assert.IsNull(window.PageKey);
			Assert.IsNull(window.Fingerprint);
		}


		[TestMethod]
		public void AStrayKeyInAFile_IsIgnored()
		{
			const string stray = @"{ ""Layouts"": [ { ""Name"": ""Work"", ""Windows"": [
				{ ""Name"": ""X"", ""Location"": ""/N/S/X"", ""Uri"": ""u"", ""NotebookID"": ""{nb}"",
				  ""SectionID"": ""{s}"", ""PageID"": ""{p}"", ""PageKey"": 99 } ] } ] }";

			var window = JsonConvert.DeserializeObject<LayoutsCollection>(stray).Layouts.Single().Windows.Single();

			Assert.IsNull(window.PageKey);
		}


		[TestMethod]
		public void UnknownPropertiesInAFile_AreIgnored()
		{
			const string future = @"{ ""FormatVersion"": 9, ""Colors"": [1],
				""Layouts"": [ { ""Name"": ""Work"", ""Extra"": true, ""Windows"": [
				{ ""Name"": ""X"", ""Location"": ""/N/S/X"", ""Uri"": ""u"", ""NotebookID"": ""{nb}"",
				  ""SectionID"": ""{s}"", ""PageID"": ""{p}"", ""Opacity"": 0.5 } ] } ] }";

			var collection = JsonConvert.DeserializeObject<LayoutsCollection>(future);

			Assert.AreEqual("X", collection.Layouts.Single().Windows.Single().Name);
		}
		#endregion The file


		#region Import
		[TestMethod]
		public void APageFromAnotherMachine_IsFoundByItsFingerprint_AndStoredWithWhatIsTrueHere()
		{
			var book = Snap.Book("{nb-here}", "Notes", "https://x/notes");
			Snap.Page(book, 7, "{page-here}", "Page", "Inbox", sectionID: "{sec-here}");

			var window = Window();
			window.Alias = "Mine";
			window.Fingerprint = new TargetFingerprint
			{
				Title = "Page", Created = Created, NotebookKey = "https://x/notes", SectionKey = "/Inbox"
			};

			var found = LayoutsExchange.Prepare(window, new TargetResolver(Snap.Of(0, book)), Link);

			Assert.AreEqual(ResolveMethod.Fingerprint, found.Method);
			Assert.AreEqual(7L, window.PageKey);
			Assert.AreEqual("{page-here}", window.PageID);
			Assert.AreEqual("{sec-here}", window.SectionID);
			Assert.AreEqual("{nb-here}", window.NotebookID);
			Assert.AreEqual("link:{page-here}", window.Uri);
			Assert.AreEqual("Page", window.Name);
			Assert.AreEqual("Mine", window.Alias);
			Assert.AreEqual(1, window.ZOrder);
		}


		[TestMethod]
		public void APageInACloudNotebook_IsFoundByTheGuidInItsLink_EvenWithoutAFingerprint()
		{
			var book = Snap.Book("{nb-here}", "Notes");
			Snap.Page(book, 7, "{page-here}", "Page", guid: Snap.G1);

			var window = Window();
			window.Uri = "onenote:x#Page&section-id={AAAAAAAA-0000-0000-0000-000000000000}&page-id=" + Snap.G1 + "&end";

			var found = LayoutsExchange.Prepare(window, new TargetResolver(Snap.Of(0, book)), Link);

			Assert.AreEqual(ResolveMethod.Guid, found.Method);
			Assert.AreEqual(7L, window.PageKey);
		}


		[TestMethod]
		public void AKeyCarriedByTheFile_IsDiscarded_EvenIfNothingIsFound()
		{
			var window = Window(99);

			LayoutsExchange.Prepare(window, new TargetResolver(Snap.Of(0, Snap.Book("{nb}", "Other"))), Link);

			Assert.IsNull(window.PageKey);
		}


		[TestMethod]
		public void APageThatIsNotFound_IsStoredAsItCame()
		{
			var window = Window();
			window.Fingerprint = Print();

			LayoutsExchange.Prepare(window, new TargetResolver(Snap.Of(0, Snap.Book("{nb}", "Other"))), Link);

			Assert.AreEqual("{page-foreign}", window.PageID);
			Assert.AreEqual("onenote:old", window.Uri);
			Assert.IsNull(window.PageKey);
		}


		[TestMethod]
		public void WhenOneNoteCannotBeRead_TheWindowIsStoredAsItCame()
		{
			var window = Window(99);
			window.Fingerprint = Print();

			Assert.IsNull(LayoutsExchange.Prepare(window, null, Link));
			Assert.IsNull(window.PageKey);
			Assert.AreEqual("{page-foreign}", window.PageID);
		}
		#endregion Import


		#region Duplicates on import
		[TestMethod]
		public void ImportingAPageAlreadyInTheLayout_IsRejected_EvenThoughItsIDsAreDifferent()
		{
			var connection = new SQLiteConnection("Data Source=:memory:");
			connection.Open();
			using var provider = new LayoutsProvider(connection);
			var layoutID = provider.CreateLayout("Work");

			var book = Snap.Book("{nb}", "Notes", "https://x/notes");
			Snap.Page(book, 7, "{page-here}", "Page", "Inbox", sectionID: "{sec-here}");
			var resolver = new TargetResolver(Snap.Of(0, book));

			var mine = Window(layoutID: layoutID);
			mine.Fingerprint = Print();
			LayoutsExchange.Prepare(mine, resolver, Link);
			Assert.IsTrue(provider.WriteWindow(mine));

			// the same page again, from a file made before OneNote changed its IDs
			var again = Window(layoutID: layoutID);
			again.PageID = "{an-older-page-id}";
			again.Fingerprint = Print();
			LayoutsExchange.Prepare(again, resolver, Link);

			Assert.IsFalse(provider.WriteWindow(again, out var duplicate));
			Assert.IsTrue(duplicate);
		}


		[TestMethod]
		public void ImportingThePageIntoAnotherLayout_IsAllowed()
		{
			var connection = new SQLiteConnection("Data Source=:memory:");
			connection.Open();
			using var provider = new LayoutsProvider(connection);
			var work = provider.CreateLayout("Work");
			var home = provider.CreateLayout("Home");

			var book = Snap.Book("{nb}", "Notes", "https://x/notes");
			Snap.Page(book, 7, "{page-here}", "Page", "Inbox", sectionID: "{sec-here}");
			var resolver = new TargetResolver(Snap.Of(0, book));

			var inWork = Window(layoutID: work);
			inWork.Fingerprint = Print();
			LayoutsExchange.Prepare(inWork, resolver, Link);
			Assert.IsTrue(provider.WriteWindow(inWork));

			var inHome = Window(layoutID: home);
			inHome.Fingerprint = Print();
			LayoutsExchange.Prepare(inHome, resolver, Link);

			Assert.IsTrue(provider.WriteWindow(inHome, out var duplicate));
			Assert.IsFalse(duplicate);
		}
		#endregion Duplicates on import
	}
}
