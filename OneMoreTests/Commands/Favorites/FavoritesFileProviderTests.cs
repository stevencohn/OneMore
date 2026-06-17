//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Favorites
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands.Favorites;
	using System;
	using System.IO;


	[TestClass]
	public class FavoritesFileProviderTests
	{
		private string path;


		[TestInitialize]
		public void Setup()
		{
			path = Path.Combine(Path.GetTempPath(), $"FavoritesFileProviderTests-{Guid.NewGuid():N}.xml");
		}


		[TestCleanup]
		public void Teardown()
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}


		[TestMethod]
		public void LoadFavorites_FileMissing_ReturnsEmptyList()
		{
			var provider = new FavoritesFileProvider(path);
			var result = provider.LoadFavorites();

			Assert.AreEqual(0, result.Count);
		}


		[TestMethod]
		public void LoadFavorites_ParsesGotoFavoriteButtons_InDocumentOrder()
		{
			File.WriteAllText(path,
@"<menu xmlns=""http://schemas.microsoft.com/office/2009/07/customui"">
  <button label=""First"" screentip=""Notebook/Section1"" tag=""onenote:#Section1""
    onAction=""GotoFavoriteCmd"" notebookID=""nb-1"" objectID=""sec-1"" />
  <button label=""Second"" screentip=""Notebook/Section2"" tag=""onenote:#Section2""
    onAction=""GotoFavoriteCmd"" notebookID=""nb-1"" objectID=""sec-2"" />
</menu>");

			var provider = new FavoritesFileProvider(path);
			var result = provider.LoadFavorites();

			Assert.AreEqual(2, result.Count);

			Assert.AreEqual(0, result[0].Index);
			Assert.AreEqual("First", result[0].Name);
			Assert.AreEqual("Notebook/Section1", result[0].Location);
			Assert.AreEqual("onenote:#Section1", result[0].Uri);
			Assert.AreEqual("nb-1", result[0].NotebookID);
			Assert.AreEqual("sec-1", result[0].ObjectID);

			Assert.AreEqual(1, result[1].Index);
			Assert.AreEqual("Second", result[1].Name);
		}


		[TestMethod]
		public void LoadFavorites_IgnoresButtonsWithOtherActions()
		{
			File.WriteAllText(path,
@"<menu xmlns=""http://schemas.microsoft.com/office/2009/07/customui"">
  <button label=""Add"" onAction=""AddFavoritePageCmd"" />
  <button label=""Keep"" screentip=""Notebook/Section"" tag=""onenote:#Section""
    onAction=""GotoFavoriteCmd"" />
</menu>");

			var provider = new FavoritesFileProvider(path);
			var result = provider.LoadFavorites();

			Assert.AreEqual(1, result.Count);
			Assert.AreEqual("Keep", result[0].Name);
		}


		[TestMethod]
		public void LoadFavorites_MalformedXml_ReturnsEmptyListWithoutThrowing()
		{
			File.WriteAllText(path, "<menu><button label=\"Unterminated\"");

			var provider = new FavoritesFileProvider(path);
			var result = provider.LoadFavorites();

			Assert.AreEqual(0, result.Count);
		}
	}
}
