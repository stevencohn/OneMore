//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Favorites
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands.Favorites;
	using River.OneMoreAddIn.Tests.Builders;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	[TestClass]
	public class FavoritesCheckerTests : TestBase
	{
		[TestMethod]
		public async Task InvalidFavorites_ValidIds_NameUnchanged_MarksKnownNoUpdate()
		{
			Mock.SetHierarchyXml(new HierarchyBuilder()
				.WithNotebook("nb-1", "Notebook1")
				.WithSection("sec-1", "Section1", "nb-1")
				.WithPage("page-1", "Page1", "sec-1")
				.Build());

			var favorite = new Favorite
			{
				NotebookID = "nb-1", SectionID = "sec-1", PageID = "page-1",
				Name = "Page1", Location = "Notebook1/Section1/Page1"
			};

			await using var checker = new FavoritesChecker(Logger.Current);
			var updated = await checker.InvalidFavorites(new List<Favorite> { favorite });

			Assert.IsFalse(updated);
			Assert.AreEqual(FavoriteStatus.Known, favorite.Status);
			Assert.AreEqual("Page1", favorite.Name);
		}


		[TestMethod]
		public async Task InvalidFavorites_ValidIds_TargetRenamed_UpdatesNameReturnsTrue()
		{
			Mock.SetHierarchyXml(new HierarchyBuilder()
				.WithNotebook("nb-1", "Notebook1")
				.WithSection("sec-1", "Section1", "nb-1")
				.WithPage("page-1", "NewPageName", "sec-1")
				.Build());

			var favorite = new Favorite
			{
				NotebookID = "nb-1", SectionID = "sec-1", PageID = "page-1",
				Name = "OldPageName", Location = "Notebook1/Section1/OldPageName"
			};

			await using var checker = new FavoritesChecker(Logger.Current);
			var updated = await checker.InvalidFavorites(new List<Favorite> { favorite });

			Assert.IsTrue(updated);
			Assert.AreEqual(FavoriteStatus.Known, favorite.Status);
			Assert.AreEqual("NewPageName", favorite.Name);
		}


		[TestMethod]
		public async Task InvalidFavorites_StaleIds_RepairsByLocation_PatchesIdsReturnsTrue()
		{
			// ConfirmByID's lookup of the stale notebook ID comes back empty (simulates a
			// notebook/section that's been deleted), forcing the fall-through to ConfirmByLocation.
			Mock.SetHierarchyXml("stale-nb", string.Empty);

			// GetNotebooks(): a flat list used to resolve the first path segment to a notebook ID.
			var notebooksList = new HierarchyBuilder()
				.WithNotebook("nb-1", "Notebook1")
				.Build();
			Mock.SetHierarchyXml(string.Empty, notebooksList);

			// GetNotebook("nb-1"): the resolved notebook's own subtree, used to walk the
			// remaining path segments. Section name case differs to exercise the
			// case-insensitive segment matching.
			var notebookSubtree = XElement.Parse(new HierarchyBuilder()
				.WithNotebook("nb-1", "Notebook1")
				.WithSection("sec-1", "Section1", "nb-1")
				.Build()).Elements().First().ToString();
			Mock.SetHierarchyXml("nb-1", notebookSubtree);

			var favorite = new Favorite
			{
				NotebookID = "stale-nb", SectionID = "stale-sec", PageID = null,
				Name = "Section1", Location = "Notebook1/section1"
			};

			await using var checker = new FavoritesChecker(Logger.Current);
			var updated = await checker.InvalidFavorites(new List<Favorite> { favorite });

			Assert.IsTrue(updated);
			Assert.AreEqual(FavoriteStatus.Known, favorite.Status);
			Assert.AreEqual("nb-1", favorite.NotebookID);
			Assert.AreEqual("sec-1", favorite.SectionID);
			Assert.IsNull(favorite.PageID);
		}


		[TestMethod]
		public async Task InvalidFavorites_StaleIdsAndUnmatchedLocation_MarksSuspectReturnsFalse()
		{
			Mock.SetHierarchyXml("stale-nb", string.Empty);

			Mock.SetHierarchyXml(string.Empty, new HierarchyBuilder()
				.WithNotebook("nb-1", "Notebook1")
				.Build());

			var favorite = new Favorite
			{
				NotebookID = "stale-nb", SectionID = "stale-sec", PageID = null,
				Name = "Section1", Location = "NoSuchNotebook/Section1"
			};

			await using var checker = new FavoritesChecker(Logger.Current);
			var updated = await checker.InvalidFavorites(new List<Favorite> { favorite });

			Assert.IsFalse(updated);
			Assert.AreEqual(FavoriteStatus.Suspect, favorite.Status);
		}


		[TestMethod]
		public async Task InvalidFavorites_StaleIdsAndMalformedLocation_MarksSuspectWithoutHierarchyLookup()
		{
			Mock.SetHierarchyXml("stale-nb", string.Empty);

			var favorite = new Favorite
			{
				NotebookID = "stale-nb", SectionID = "stale-sec", PageID = null,
				Name = "Section1", Location = "JustOneSegment"
			};

			await using var checker = new FavoritesChecker(Logger.Current);
			var updated = await checker.InvalidFavorites(new List<Favorite> { favorite });

			Assert.IsFalse(updated);
			Assert.AreEqual(FavoriteStatus.Suspect, favorite.Status);
		}
	}
}
