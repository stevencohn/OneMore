//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Layouts
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands.Layouts;
	using System;
	using System.Data.SQLite;
	using System.Linq;


	[TestClass]
	public class LayoutsProviderTests
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


		private static LayoutWindow Window(int layoutID, string name, string pageID, int zOrder = 0,
			long? key = null, string alias = null)
		{
			return new LayoutWindow
			{
				LayoutID = layoutID,
				Name = name,
				Alias = alias,
				Location = "/Notes/Inbox/" + name,
				Uri = "onenote:#" + name,
				NotebookID = "{nb}",
				SectionID = "{sec}",
				PageID = pageID,
				ZOrder = zOrder,
				PageKey = key
			};
		}


		// a catalog as version 1 left it: no layouts_schema table, and no page key
		private void MakeVersion1()
		{
			Execute("CREATE TABLE layout (layoutID INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT NOT NULL, UNIQUE (name))");
			Execute("CREATE TABLE layout_window (windowID INTEGER PRIMARY KEY AUTOINCREMENT, layoutID INTEGER NOT NULL REFERENCES layout (layoutID) ON DELETE CASCADE, name TEXT NOT NULL, alias TEXT, location TEXT, uri TEXT NOT NULL, notebookID TEXT NOT NULL, sectionID TEXT NOT NULL, pageID TEXT NOT NULL, zOrder INTEGER NOT NULL DEFAULT 0, device TEXT, winLeft INTEGER, winTop INTEGER, winRight INTEGER, winBottom  INTEGER)");
			Execute("CREATE INDEX idx_layouts_layout ON layout (layoutID)");
			Execute("CREATE UNIQUE INDEX idx_layouts_alias_per_window ON layout_window (layoutID, alias) WHERE alias IS NOT NULL");
			Execute("CREATE UNIQUE INDEX idx_layouts_target_page ON layout_window (layoutID, pageID)");
			Execute("INSERT INTO layout (name) VALUES ('Work')");
			Execute("INSERT INTO layout_window (layoutID, name, alias, location, uri, notebookID, sectionID, pageID, zOrder, device, winLeft, winTop, winRight, winBottom) " +
				"VALUES (1, 'Plan', 'Main', '/Notes/Inbox/Plan', 'onenote:#plan', '{nb}', '{sec}', '{page}', 2, '\\\\.\\DISPLAY1', 10, 20, 800, 600)");
		}


		#region Schema
		[TestMethod]
		public void FreshCatalog_IsVersion2_WithThePageKey()
		{
			using var provider = new LayoutsProvider(connection);

			Assert.AreEqual(2L, Convert.ToInt64(Scalar("SELECT version FROM layouts_schema")));
			Assert.AreEqual(1L, Convert.ToInt64(Scalar(
				"SELECT count(*) FROM pragma_table_info('layout_window') WHERE name = 'pageKey'")));
		}


		[TestMethod]
		public void Version1Catalog_IsUpgraded_KeepingEverything()
		{
			MakeVersion1();

			using var provider = new LayoutsProvider(connection);

			Assert.AreEqual(2L, Convert.ToInt64(Scalar("SELECT version FROM layouts_schema")));

			var window = provider.ReadLayouts().Layouts.Single().Windows.Single();
			Assert.AreEqual("Main", window.Alias);
			Assert.AreEqual("{page}", window.PageID);
			Assert.AreEqual(2, window.ZOrder);
			Assert.AreEqual("\\\\.\\DISPLAY1", window.Device);
			Assert.AreEqual(10, window.WinLeft);
			Assert.AreEqual(600, window.WinBottom);
			Assert.IsNull(window.PageKey);
		}


		[TestMethod]
		public void Version1Catalog_AfterUpgrade_StillRejectsTheSameHandleID()
		{
			MakeVersion1();
			using var provider = new LayoutsProvider(connection);

			Assert.IsFalse(provider.WriteWindow(Window(1, "Another", "{page}"), out var duplicate));
			Assert.IsTrue(duplicate);
		}


		[TestMethod]
		public void UpgradedCatalog_CanBeOpenedAgain()
		{
			MakeVersion1();

			// the provider closes the connection it is given when it is disposed, so the first one
			// is left open, as another process's would be, while the second opens the same catalog
			var first = new LayoutsProvider(connection);
			Assert.IsNotNull(first);

			using var again = new LayoutsProvider(connection);

			Assert.AreEqual(2L, Convert.ToInt64(Scalar("SELECT version FROM layouts_schema")));
			Assert.AreEqual(1, again.ReadLayouts().Layouts.Count);
		}


		[TestMethod]
		public void CatalogNewerThanKnown_IsLeftAlone()
		{
			MakeVersion1();
			Execute("CREATE TABLE layouts_schema (schemaID INTEGER PRIMARY KEY UNIQUE NOT NULL, version NUMERIC (12) UNIQUE NOT NULL)");
			Execute("INSERT INTO layouts_schema (schemaID, version) VALUES (0, 9)");

			using var provider = new LayoutsProvider(connection);

			Assert.AreEqual(9L, Convert.ToInt64(Scalar("SELECT version FROM layouts_schema")));
			Assert.AreEqual(0L, Convert.ToInt64(Scalar(
				"SELECT count(*) FROM pragma_table_info('layout_window') WHERE name = 'pageKey'")));
		}
		#endregion Schema


		#region Layouts
		[TestMethod]
		public void CreateLayout_ReturnsAnID_AndNamesAreUnique()
		{
			using var provider = new LayoutsProvider(connection);

			Assert.IsTrue(provider.CreateLayout("Work") > 0);
			Assert.AreEqual(0, provider.CreateLayout("Work"));
		}


		[TestMethod]
		public void RenameLayout_ChangesTheName()
		{
			using var provider = new LayoutsProvider(connection);
			var id = provider.CreateLayout("Work");

			Assert.IsTrue(provider.RenameLayout(id, "Home"));
			Assert.AreEqual("Home", provider.ReadLayouts().Layouts.Single().Name);
		}


		[TestMethod]
		public void ReadLayouts_IncludesAnEmptyLayout()
		{
			using var provider = new LayoutsProvider(connection);
			provider.CreateLayout("Empty");

			var layout = provider.ReadLayouts().Layouts.Single();

			Assert.AreEqual("Empty", layout.Name);
			Assert.AreEqual(0, layout.Windows.Count);
		}


		[TestMethod]
		public void ReadLayouts_AreOrderedByName_AndWindowsByZOrder()
		{
			using var provider = new LayoutsProvider(connection);
			var b = provider.CreateLayout("Beta");
			var a = provider.CreateLayout("Alpha");

			provider.WriteWindow(Window(b, "Two", "{2}", zOrder: 1));
			provider.WriteWindow(Window(b, "One", "{1}", zOrder: 0));
			provider.WriteWindow(Window(a, "Only", "{3}"));

			var layouts = provider.ReadLayouts().Layouts;

			CollectionAssert.AreEqual(new[] { "Alpha", "Beta" }, layouts.Select(l => l.Name).ToList());
			CollectionAssert.AreEqual(new[] { "One", "Two" }, layouts[1].Windows.Select(w => w.Name).ToList());
		}


		[TestMethod]
		public void DeleteLayout_RemovesItsWindows_AndNoOthers()
		{
			using var provider = new LayoutsProvider(connection);
			var a = provider.CreateLayout("A");
			var b = provider.CreateLayout("B");
			provider.WriteWindow(Window(a, "One", "{1}"));
			provider.WriteWindow(Window(b, "Two", "{2}"));

			Assert.IsTrue(provider.DeleteLayout(a));

			var left = provider.ReadLayouts().Layouts.Single();
			Assert.AreEqual("B", left.Name);
			Assert.AreEqual(1, left.Windows.Count);
		}
		#endregion Layouts


		#region Windows
		[TestMethod]
		public void WriteWindow_ReadLayouts_RoundTrip_IncludingGeometry()
		{
			using var provider = new LayoutsProvider(connection);
			var id = provider.CreateLayout("Work");

			var window = Window(id, "Plan", "{p}", zOrder: 3, alias: "Main");
			window.Device = "\\\\.\\DISPLAY2";
			window.WinLeft = -1920; window.WinTop = 40; window.WinRight = -100; window.WinBottom = 900;

			Assert.IsTrue(provider.WriteWindow(window));

			var read = provider.ReadLayouts().Layouts.Single().Windows.Single();
			Assert.AreEqual("Plan", read.Name);
			Assert.AreEqual("Main", read.Alias);
			Assert.AreEqual("/Notes/Inbox/Plan", read.Location);
			Assert.AreEqual("onenote:#Plan", read.Uri);
			Assert.AreEqual("{nb}", read.NotebookID);
			Assert.AreEqual("{sec}", read.SectionID);
			Assert.AreEqual("{p}", read.PageID);
			Assert.AreEqual(3, read.ZOrder);
			Assert.AreEqual("\\\\.\\DISPLAY2", read.Device);
			Assert.AreEqual(-1920, read.WinLeft);
			Assert.AreEqual(900, read.WinBottom);
		}


		[TestMethod]
		public void WindowWithoutGeometry_ReadsBackWithNulls()
		{
			using var provider = new LayoutsProvider(connection);
			var id = provider.CreateLayout("Work");
			provider.WriteWindow(Window(id, "Plan", "{p}"));

			var read = provider.ReadLayouts().Layouts.Single().Windows.Single();

			Assert.IsNull(read.Alias);
			Assert.IsNull(read.Device);
			Assert.IsNull(read.WinLeft);
			Assert.IsNull(read.WinTop);
			Assert.IsNull(read.WinRight);
			Assert.IsNull(read.WinBottom);
		}


		[TestMethod]
		public void ThePageKey_RoundTrips_AndStaysNullWhenUnset()
		{
			using var provider = new LayoutsProvider(connection);
			var id = provider.CreateLayout("Work");
			provider.WriteWindow(Window(id, "Keyed", "{1}", key: 42));
			provider.WriteWindow(Window(id, "Legacy", "{2}", zOrder: 1));

			var windows = provider.ReadLayouts().Layouts.Single().Windows;

			Assert.AreEqual(42L, windows[0].PageKey);
			Assert.IsNull(windows[1].PageKey);
		}


		[TestMethod]
		public void SamePageID_InTheSameLayout_IsADuplicate_ButNotInAnother()
		{
			using var provider = new LayoutsProvider(connection);
			var a = provider.CreateLayout("A");
			var b = provider.CreateLayout("B");

			Assert.IsTrue(provider.WriteWindow(Window(a, "One", "{p}")));
			Assert.IsFalse(provider.WriteWindow(Window(a, "Again", "{p}"), out var duplicate));
			Assert.IsTrue(duplicate);

			Assert.IsTrue(provider.WriteWindow(Window(b, "One", "{p}")), "another layout may show the same page");
		}


		[TestMethod]
		public void SamePageKey_InTheSameLayout_IsADuplicate_EvenWithDifferentIDs()
		{
			using var provider = new LayoutsProvider(connection);
			var a = provider.CreateLayout("A");
			var b = provider.CreateLayout("B");

			Assert.IsTrue(provider.WriteWindow(Window(a, "One", "{old-id}", key: 42)));

			// the page was added again after a reopen gave it a new ID
			Assert.IsFalse(provider.WriteWindow(Window(a, "Again", "{new-id}", key: 42), out var duplicate));
			Assert.IsTrue(duplicate);

			Assert.IsTrue(provider.WriteWindow(Window(b, "One", "{new-id}", key: 42)));
		}


		[TestMethod]
		public void TheSameAlias_InOneLayout_IsRejected()
		{
			using var provider = new LayoutsProvider(connection);
			var a = provider.CreateLayout("A");

			Assert.IsTrue(provider.WriteWindow(Window(a, "One", "{1}", alias: "Main")));
			Assert.IsFalse(provider.WriteWindow(Window(a, "Two", "{2}", alias: "Main")));
		}


		[TestMethod]
		public void UpdateWindow_ChangesNameAliasLocationLayoutAndOrder_NotTheTarget()
		{
			using var provider = new LayoutsProvider(connection);
			var a = provider.CreateLayout("A");
			var b = provider.CreateLayout("B");
			provider.WriteWindow(Window(a, "One", "{1}"));

			var window = provider.ReadLayouts().Layouts.Single(l => l.Name == "A").Windows.Single();
			window.Name = "Renamed";
			window.Alias = "Mine";
			window.Location = "/Elsewhere";
			window.LayoutID = b;
			window.ZOrder = 7;
			window.PageID = "{ignored}";
			window.Uri = "ignored";

			Assert.IsTrue(provider.UpdateWindow(window));

			var read = provider.ReadLayouts().Layouts.Single(l => l.Name == "B").Windows.Single();
			Assert.AreEqual("Renamed", read.Name);
			Assert.AreEqual("Mine", read.Alias);
			Assert.AreEqual("/Elsewhere", read.Location);
			Assert.AreEqual(7, read.ZOrder);
			Assert.AreEqual("{1}", read.PageID);
			Assert.AreEqual("onenote:#One", read.Uri);
		}


		[TestMethod]
		public void DeleteWindow_RemovesJustThatWindow()
		{
			using var provider = new LayoutsProvider(connection);
			var a = provider.CreateLayout("A");
			provider.WriteWindow(Window(a, "One", "{1}"));
			provider.WriteWindow(Window(a, "Two", "{2}", zOrder: 1));

			var first = provider.ReadLayouts().Layouts.Single().Windows[0];

			Assert.IsTrue(provider.DeleteWindow(first.ID));
			Assert.AreEqual("Two", provider.ReadLayouts().Layouts.Single().Windows.Single().Name);
		}
		#endregion Windows


		#region UpdateTarget
		[TestMethod]
		public void UpdateTarget_SavesWhatWasFound_AndLeavesTheUsersChoicesAlone()
		{
			using var provider = new LayoutsProvider(connection);
			var a = provider.CreateLayout("A");

			var legacy = Window(a, "Old name", "{old-page}", zOrder: 4, alias: "Mine");
			legacy.Device = "\\\\.\\DISPLAY1";
			legacy.WinLeft = 5; legacy.WinTop = 6; legacy.WinRight = 700; legacy.WinBottom = 500;
			provider.WriteWindow(legacy);

			var stored = provider.ReadLayouts().Layouts.Single().Windows.Single();

			stored.Location = "/Notes/Archive/New name";
			stored.Uri = "onenote:#new";
			stored.NotebookID = "{nb-new}";
			stored.SectionID = "{sec-new}";
			stored.PageID = "{page-new}";
			stored.PageKey = 42;

			// the user's name, alias and order must not be written even if they changed in memory
			stored.Name = "A name changed in memory";
			stored.Alias = "ignored";
			stored.ZOrder = 99;

			Assert.IsTrue(provider.UpdateTarget(stored, out var duplicate));
			Assert.IsFalse(duplicate);

			var read = provider.ReadLayouts().Layouts.Single().Windows.Single();
			Assert.AreEqual("/Notes/Archive/New name", read.Location);
			Assert.AreEqual("onenote:#new", read.Uri);
			Assert.AreEqual("{nb-new}", read.NotebookID);
			Assert.AreEqual("{sec-new}", read.SectionID);
			Assert.AreEqual("{page-new}", read.PageID);
			Assert.AreEqual(42L, read.PageKey);

			Assert.AreEqual("Old name", read.Name);
			Assert.AreEqual("Mine", read.Alias);
			Assert.AreEqual(4, read.ZOrder);
			Assert.AreEqual("\\\\.\\DISPLAY1", read.Device);
			Assert.AreEqual(700, read.WinRight);
		}


		[TestMethod]
		public void UpdateTarget_TwoWindowsTurnOutToBeTheSamePage_ReportsTheDuplicate()
		{
			using var provider = new LayoutsProvider(connection);
			var a = provider.CreateLayout("A");
			provider.WriteWindow(Window(a, "First", "{a}", key: 42));
			provider.WriteWindow(Window(a, "Second", "{b}", zOrder: 1));

			var second = provider.ReadLayouts().Layouts.Single().Windows.Single(w => w.Name == "Second");
			second.PageKey = 42;

			Assert.IsFalse(provider.UpdateTarget(second, out var duplicate));
			Assert.IsTrue(duplicate);

			// left as it was, still unresolved
			Assert.IsNull(provider.ReadLayouts().Layouts.Single().Windows.Single(w => w.Name == "Second").PageKey);
		}


		[TestMethod]
		public void UpdateTarget_UnknownWindow_ChangesNothing()
		{
			using var provider = new LayoutsProvider(connection);
			var a = provider.CreateLayout("A");
			provider.WriteWindow(Window(a, "One", "{1}", key: 1));

			var ghost = Window(a, "Ghost", "{g}", key: 5);
			ghost.ID = 9999;

			Assert.IsTrue(provider.UpdateTarget(ghost, out _));
			Assert.AreEqual(1, provider.ReadLayouts().Layouts.Single().Windows.Count);
		}
		#endregion UpdateTarget
	}
}
