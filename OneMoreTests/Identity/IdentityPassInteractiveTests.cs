//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Identity
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Identity;
	using System.Data.SQLite;
	using System.Linq;
	using System.Threading.Tasks;


	/// <summary>
	/// A pass that someone is waiting for, such as a search, does not spend time filling in
	/// hyperlink GUIDs, but must still read the ones it needs in order to match a page.
	/// </summary>
	[TestClass]
	public class IdentityPassInteractiveTests
	{
		private const string G1 = "{11111111-1111-1111-1111-111111111111}";

		private SQLiteConnection connection;
		private PageIdentityProvider provider;


		[TestInitialize]
		public void Setup()
		{
			connection = new SQLiteConnection("Data Source=:memory:");
			connection.Open();
			provider = new PageIdentityProvider(connection);
		}


		[TestCleanup]
		public void Teardown()
		{
			provider?.Dispose();
			connection?.Dispose();
		}


		private static FakeHierarchy Hierarchy(string section, string pageID, string title, string created)
		{
			return new FakeHierarchy().Notebook("{nb}", "Notes", FakeHierarchy.Book(
				FakeHierarchy.Section("{" + section + "}", section,
					FakeHierarchy.Page(pageID, title, created))));
		}


		[TestMethod]
		public async Task WithoutBackfill_NoGuidIsRead_AndThePageStillGetsItsKey()
		{
			var source = Hierarchy("Inbox", "{p1}", "One", "2026-01-01T00:00:00.000Z");
			source.Guids["{p1}"] = G1;

			var snapshot = await new IdentityPass(provider, source).Run(fillGuids: false);

			Assert.AreEqual(0, source.GuidCalls);
			Assert.IsTrue(snapshot.Pages.Single().PageKey > 0);
			Assert.AreEqual(1, snapshot.GuidsPending);
			Assert.IsNull(provider.ReadAll().Single().PageGuid);
		}


		[TestMethod]
		public async Task WithBackfill_TheGuidIsRead_AsBefore()
		{
			var source = Hierarchy("Inbox", "{p1}", "One", "2026-01-01T00:00:00.000Z");
			source.Guids["{p1}"] = G1;

			var snapshot = await new IdentityPass(provider, source).Run(fillGuids: true);

			Assert.AreEqual(1, source.GuidCalls);
			Assert.AreEqual(0, snapshot.GuidsPending);
		}


		[TestMethod]
		public async Task WithoutBackfill_AGuidNeededToMatchAPage_IsStillRead()
		{
			var first = Hierarchy("Inbox", "{p1}", "Old title", "2026-01-01T00:00:00.000Z");
			first.Guids["{p1}"] = G1;
			var before = await new IdentityPass(provider, first).Run();

			// renamed, moved and given a new ID, then looked up by a search that is waiting
			var second = Hierarchy("Archive", "{p1-new}", "New title", "2026-09-09T00:00:00.000Z");
			second.Guids["{p1-new}"] = G1;
			var after = await new IdentityPass(provider, second).Run(fillGuids: false);

			Assert.AreEqual(before.Pages.Single().PageKey, after.Pages.Single().PageKey);
			Assert.AreEqual(1, second.GuidCalls);
		}
	}
}
