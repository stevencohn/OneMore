//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Identity
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Identity;
	using River.OneMoreAddIn.Pipeline;
	using System;
	using System.Collections.Generic;
	using System.Data.SQLite;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// A hierarchy that tests control: notebooks by ID, each with its listing and its pages.
	/// </summary>
	internal sealed class FakeHierarchy : IHierarchySource
	{
		private static readonly XNamespace ns = "http://schemas.microsoft.com/office/onenote/2013/onenote";

		private readonly Dictionary<string, XElement> hierarchies = new Dictionary<string, XElement>();
		private readonly List<XElement> listings = new List<XElement>();

		public bool CannotListNotebooks { get; set; }

		/// <summary>The hyperlink page GUID of each page by its page ID; a page not listed has none.</summary>
		public Dictionary<string, string> Guids { get; } = new Dictionary<string, string>();

		/// <summary>How many times a GUID was asked for.</summary>
		public int GuidCalls { get; private set; }


		public static XNamespace Namespace => ns;


		public FakeHierarchy Notebook(string id, string name, XElement content, string path = null)
		{
			listings.Add(new XElement(ns + "Notebook",
				new XAttribute("ID", id), new XAttribute("name", name),
				path is null ? null : new XAttribute("path", path)));

			hierarchies[id] = content;
			return this;
		}


		public Task<XElement> GetNotebooks()
		{
			return Task.FromResult(CannotListNotebooks
				? null
				: new XElement(ns + "Notebooks", listings));
		}


		public Task<XElement> GetNotebookPages(string notebookID)
		{
			return Task.FromResult(hierarchies.TryGetValue(notebookID, out var e) ? e : null);
		}


		public Task<string> GetPageGuid(string pageID)
		{
			GuidCalls++;
			return Task.FromResult(Guids.TryGetValue(pageID, out var g) ? g : null);
		}


		public static XElement Book(params object[] children)
		{
			return new XElement(ns + "Notebook", children);
		}


		public static XElement Group(string name, params object[] children)
		{
			return new XElement(ns + "SectionGroup", new XAttribute("name", name), children);
		}


		public static XElement Section(string id, string name, params object[] children)
		{
			return new XElement(ns + "Section",
				new XAttribute("ID", id), new XAttribute("name", name), children);
		}


		public static XElement Page(string id, string title, string created = "2026-01-01T00:00:00.000Z")
		{
			return new XElement(ns + "Page",
				new XAttribute("ID", id), new XAttribute("name", title),
				new XAttribute("dateTime", created),
				new XAttribute("lastModifiedTime", "2026-02-01T00:00:00.000Z"));
		}
	}


	[TestClass]
	public class IdentityGathererTests
	{
		private static IdentityNotebook Gather(XElement hierarchy, string path = null, List<string> skipped = null)
		{
			var listing = new XElement(FakeHierarchy.Namespace + "Notebook",
				new XAttribute("ID", "{nb}"), new XAttribute("name", "Notes"),
				path is null ? null : new XAttribute("path", path));

			return IdentityGatherer.Gather(listing, hierarchy, skipped ?? new List<string>());
		}


		[TestMethod]
		public void PagesInSectionsAndGroups_GetSectionKeysAndPaths()
		{
			var book = FakeHierarchy.Book(
				FakeHierarchy.Section("{s1}", "Top", FakeHierarchy.Page("{p1}", "One")),
				FakeHierarchy.Group("Outer",
					FakeHierarchy.Group("Inner",
						FakeHierarchy.Section("{s2}", "Deep", FakeHierarchy.Page("{p2}", "Two")))));

			var notebook = Gather(book);

			Assert.IsTrue(notebook.Listed);
			Assert.AreEqual(2, notebook.Pages.Count);

			Assert.AreEqual(PageIdentityKeys.SectionKey(null, "Top"), notebook.Pages[0].Ref.SectionKey);
			Assert.AreEqual("/Notes/Top", notebook.Pages[0].SectionPath);
			Assert.AreEqual("{s1}", notebook.Pages[0].SectionID);
			Assert.AreEqual("{nb}", notebook.Pages[0].NotebookID);

			Assert.AreEqual(
				PageIdentityKeys.SectionKey(new[] { "Outer", "Inner" }, "Deep"),
				notebook.Pages[1].Ref.SectionKey);
			Assert.AreEqual("/Notes/Outer/Inner/Deep", notebook.Pages[1].SectionPath);
		}


		[TestMethod]
		public void NotebookKey_UsesPathWhenPresent_NameOtherwise()
		{
			Assert.AreEqual(PageIdentityKeys.NotebookKey("https://x/y", "Notes"), Gather(FakeHierarchy.Book(), "https://x/y").Key);
			Assert.AreEqual(PageIdentityKeys.NotebookKey(null, "Notes"), Gather(FakeHierarchy.Book()).Key);
		}


		[TestMethod]
		public void RecycleBin_GroupSectionAndPages_AreSkipped()
		{
			var bin = FakeHierarchy.Group("OneNote_RecycleBin",
				FakeHierarchy.Section("{r}", "Deleted Pages", FakeHierarchy.Page("{rp}", "Gone")));
			bin.Add(new XAttribute("isRecycleBin", "true"));

			var deleted = FakeHierarchy.Section("{d}", "Deleted", FakeHierarchy.Page("{dp}", "Gone"));
			deleted.Add(new XAttribute("isDeletedPages", "true"));

			var binned = FakeHierarchy.Page("{bp}", "Binned");
			binned.Add(new XAttribute("isInRecycleBin", "true"));

			var book = FakeHierarchy.Book(
				bin, deleted,
				FakeHierarchy.Section("{s}", "Real", FakeHierarchy.Page("{p}", "Kept"), binned));

			var notebook = Gather(book);

			Assert.AreEqual(1, notebook.Pages.Count);
			Assert.AreEqual("{p}", notebook.Pages[0].Ref.PageID);
		}


		[TestMethod]
		public void LockedSection_IsReportedAsSkipped_NotAsEmpty()
		{
			var locked = FakeHierarchy.Section("{l}", "Secret");
			locked.Add(new XAttribute("locked", "true"));

			var skipped = new List<string>();
			var notebook = Gather(FakeHierarchy.Book(locked), skipped: skipped);

			Assert.AreEqual(0, notebook.Pages.Count);
			Assert.AreEqual(1, skipped.Count);
			Assert.AreEqual(
				PageIdentityKeys.SectionScope(notebook.Key, PageIdentityKeys.SectionKey(null, "Secret")),
				skipped[0]);
		}


		[TestMethod]
		public void TagIndexPage_IsFlagged()
		{
			var ns = FakeHierarchy.Namespace;
			var index = FakeHierarchy.Page("{i}", "#Tags");
			index.Add(new XElement(ns + "Meta",
				new XAttribute("name", River.OneMoreAddIn.Models.MetaNames.TagIndex),
				new XAttribute("content", "true")));

			var notebook = Gather(FakeHierarchy.Book(
				FakeHierarchy.Section("{s}", "S", index, FakeHierarchy.Page("{p}", "Plain"))));

			Assert.IsTrue(notebook.Pages[0].IsTagIndex);
			Assert.IsFalse(notebook.Pages[1].IsTagIndex);
		}


		[TestMethod]
		public void UnreadableNotebook_IsNotListed()
		{
			var notebook = Gather(null);

			Assert.IsFalse(notebook.Listed);
			Assert.AreEqual(0, notebook.Pages.Count);
		}
	}


	[TestClass]
	public class IdentityPassTests
	{
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


		private static FakeHierarchy Notebooks(string idSuffix = "", params string[] titles)
		{
			// one notebook "Notes" with one section "Inbox" holding the titled pages
			var pages = titles.Select((t, n) => FakeHierarchy.Page($"{{p{n}{idSuffix}}}", t)).Cast<object>().ToArray();
			return new FakeHierarchy().Notebook($"{{nb{idSuffix}}}", "Notes",
				FakeHierarchy.Book(FakeHierarchy.Section($"{{s{idSuffix}}}", "Inbox", pages)));
		}


		[TestMethod]
		public async Task FirstRun_GivesEveryPageItsOwnKey()
		{
			var snapshot = await new IdentityPass(provider, Notebooks("", "A", "B", "C")).Run();

			var keys = snapshot.Pages.Select(p => p.PageKey).ToList();
			Assert.AreEqual(3, keys.Count);
			Assert.IsTrue(keys.All(k => k > 0));
			Assert.AreEqual(3, keys.Distinct().Count());
			Assert.IsTrue(snapshot.Pages.All(p => p.Resolution.Kind == ResolutionKind.New));
		}


		[TestMethod]
		public async Task NotebookReopened_AllIDsChange_KeysSurvive()
		{
			var before = await new IdentityPass(provider, Notebooks("", "A", "B")).Run();
			var after = await new IdentityPass(provider, Notebooks("-new", "A", "B")).Run();

			CollectionAssert.AreEqual(
				before.Pages.Select(p => p.PageKey).ToList(),
				after.Pages.Select(p => p.PageKey).ToList());

			Assert.IsTrue(after.Pages.All(p => p.Resolution.Kind == ResolutionKind.SameLocation));
			Assert.IsTrue(after.Pages.All(p => p.Ref.PageID.Contains("-new")));
		}


		[TestMethod]
		public async Task UnreadableNotebook_KeepsItsPages_NotMarkedMissing()
		{
			var first = await new IdentityPass(provider, Notebooks("", "A", "B")).Run();

			// the notebook is listed but its hierarchy cannot be read right now
			var broken = new FakeHierarchy().Notebook("{nb}", "Notes", null);
			var second = await new IdentityPass(provider, broken).Run();

			Assert.IsFalse(second.Notebooks[0].Listed);
			Assert.IsTrue(provider.ReadAll().All(r => !r.IsMissing));

			// and when it can be read again the pages are still the same pages
			var third = await new IdentityPass(provider, Notebooks("", "A", "B")).Run();
			CollectionAssert.AreEqual(
				first.Pages.Select(p => p.PageKey).ToList(),
				third.Pages.Select(p => p.PageKey).ToList());
		}


		[TestMethod]
		public async Task NotebookGone_PagesAreMarkedMissing_ThenFoundWhenItReturns()
		{
			var first = await new IdentityPass(provider, Notebooks("", "A", "B")).Run();

			// the notebook is listed and readable but has no pages: they really are gone
			var empty = new FakeHierarchy().Notebook("{nb}", "Notes",
				FakeHierarchy.Book(FakeHierarchy.Section("{s}", "Inbox")));
			await new IdentityPass(provider, empty).Run();
			Assert.IsTrue(provider.ReadAll().All(r => r.IsMissing));

			var back = await new IdentityPass(provider, Notebooks("", "A", "B")).Run();
			CollectionAssert.AreEqual(
				first.Pages.Select(p => p.PageKey).ToList(),
				back.Pages.Select(p => p.PageKey).ToList());
			Assert.IsTrue(provider.ReadAll().All(r => !r.IsMissing));
		}


		[TestMethod]
		public async Task LockedSection_KeepsItsStoredPages()
		{
			var open = new FakeHierarchy().Notebook("{nb}", "Notes", FakeHierarchy.Book(
				FakeHierarchy.Section("{s}", "Vault", FakeHierarchy.Page("{p}", "Secret page"))));
			var first = await new IdentityPass(provider, open).Run();

			var lockedSection = FakeHierarchy.Section("{s}", "Vault");
			lockedSection.Add(new XAttribute("locked", "true"));
			var locked = new FakeHierarchy().Notebook("{nb}", "Notes", FakeHierarchy.Book(lockedSection));
			var second = await new IdentityPass(provider, locked).Run();

			Assert.AreEqual(1, second.SkippedSections.Count);
			Assert.IsTrue(provider.ReadAll().All(r => !r.IsMissing));
			Assert.AreEqual(first.Pages.Single().PageKey, provider.ReadAll().Single().PageKey);
		}


		[TestMethod]
		public async Task PageMovedToAnotherSection_KeepsItsKey()
		{
			var before = new FakeHierarchy().Notebook("{nb}", "Notes", FakeHierarchy.Book(
				FakeHierarchy.Section("{s1}", "One", FakeHierarchy.Page("{p}", "Mover")),
				FakeHierarchy.Section("{s2}", "Two")));
			var first = await new IdentityPass(provider, before).Run();

			var after = new FakeHierarchy().Notebook("{nb}", "Notes", FakeHierarchy.Book(
				FakeHierarchy.Section("{s1}", "One"),
				FakeHierarchy.Section("{s2}", "Two", FakeHierarchy.Page("{p-moved}", "Mover"))));
			var second = await new IdentityPass(provider, after).Run();

			Assert.AreEqual(first.Pages.Single().PageKey, second.Pages.Single().PageKey);
			Assert.AreEqual(ResolutionKind.MovedSection, second.Pages.Single().Resolution.Kind);
		}


		[TestMethod]
		public async Task CannotListNotebooks_ReturnsNull_AndChangesNothing()
		{
			await new IdentityPass(provider, Notebooks("", "A")).Run();

			var source = Notebooks("", "A");
			source.CannotListNotebooks = true;

			Assert.IsNull(await new IdentityPass(provider, source).Run());
			Assert.AreEqual(1, provider.ReadAll().Count);
			Assert.IsTrue(provider.ReadAll().All(r => !r.IsMissing));
		}


		[TestMethod]
		public async Task Cancelled_Throws()
		{
			using var cts = new CancellationTokenSource();
			cts.Cancel();

			await Assert.ThrowsExceptionAsync<OperationCanceledException>(
				() => new IdentityPass(provider, Notebooks("", "A")).Run(cts.Token));
		}
	}


	[TestClass]
	public class IdentityStageTests
	{
		[TestMethod]
		public async Task Run_PublishesSnapshot_IntoContext()
		{
			using var connection = new SQLiteConnection("Data Source=:memory:");
			connection.Open();
			using var provider = new PageIdentityProvider(connection);

			using var stage = new IdentityStage(provider,
				() => new FakeHierarchy().Notebook("{nb}", "Notes", FakeHierarchy.Book(
					FakeHierarchy.Section("{s}", "Inbox", FakeHierarchy.Page("{p}", "A")))));

			var context = new PipelineContext();
			await stage.Run(context, CancellationToken.None);

			Assert.IsTrue(context.TryGet<IdentitySnapshot>(out var snapshot));
			Assert.AreEqual(1, snapshot.Pages.Count());
		}


		[TestMethod]
		public async Task Run_WhenOneNoteCannotBeRead_PublishesNothing()
		{
			using var connection = new SQLiteConnection("Data Source=:memory:");
			connection.Open();
			using var provider = new PageIdentityProvider(connection);

			using var stage = new IdentityStage(provider,
				() => new FakeHierarchy { CannotListNotebooks = true });

			var context = new PipelineContext();
			await stage.Run(context, CancellationToken.None);

			Assert.IsFalse(context.TryGet<IdentitySnapshot>(out _));
		}


		[TestMethod]
		public async Task Stage_IsAlwaysEnabledAndReady_OnAFixedInterval()
		{
			using var stage = new IdentityStage();

			Assert.AreEqual("identity", stage.Name);
			Assert.IsTrue(stage.IsEnabled);
			Assert.AreEqual(IdentityStage.PassInterval, stage.Interval);
			Assert.IsTrue(await stage.IsReady(CancellationToken.None));
		}
	}
}
