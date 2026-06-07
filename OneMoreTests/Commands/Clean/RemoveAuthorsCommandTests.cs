//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Clean
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Tests.Builders;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;

	/*
	 * Test Protocol - RemoveAuthorCommand
	 * Create a page with at least two paragraphs. The first paragraph should have the default
	 * user author attributes. For the second paragraph, set the author and lastModifiedBy
	 * attributes to "User Two", and authorInitials and lastModifiedByInitials attributes to "UT",
	 * ensuring those are different than the default user.
	 *
	 *   1. Invoke Clean/Remove Author Information
	 *   2. Confirm author attributes for the "User Two" paragraph have been reset to the
	 *      default user and the author bar is no longer visually obvious
	 */

	[TestClass]
	public class RemoveAuthorsCommandTests : TestBase
	{
		private const string PageId = "page-1";
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";

		private static readonly string[] AuthorAttributes =
		{
			"author", "authorInitials", "authorResolutionID",
			"lastModifiedBy", "lastModifiedByInitials", "lastModifiedByResolutionID"
		};


		private static Task ExecuteCommand()
		{
			var cmd = new RemoveAuthorsCommand();
			cmd.SetLogger(Logger.Current);
			return cmd.Execute();
		}


		[TestMethod]
		public async Task RemoveAuthors_MixedAuthorAttributes_RemovesAllAuthorAttributes()
		{
			// Arrange: two paragraphs — first with default-user authorship, second authored by "User Two"
			var oeDefault = new XElement(Ns + "OE",
				new XAttribute("author", "Default User"),
				new XAttribute("authorInitials", "DU"),
				new XAttribute("lastModifiedBy", "Default User"),
				new XAttribute("lastModifiedByInitials", "DU"),
				new XElement(Ns + "T", new XCData("paragraph one")));

			var oeUserTwo = new XElement(Ns + "OE",
				new XAttribute("author", "User Two"),
				new XAttribute("authorInitials", "UT"),
				new XAttribute("lastModifiedBy", "User Two"),
				new XAttribute("lastModifiedByInitials", "UT"),
				new XElement(Ns + "T", new XCData("paragraph two")));

			var xml = new PageBuilder(PageId, "Remove Author Test")
				.WithElement(oeDefault)
				.WithElement(oeUserTwo)
				.Build();

			SetupPage(PageId, xml);

			// Act
			await ExecuteCommand();

			// Assert: all author-related attributes removed from every body OE
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var bodyOes = updated
				.Descendants(Ns + "OE")
				.Where(oe => !oe.Ancestors(Ns + "Title").Any())
				.ToList();

			Assert.AreEqual(2, bodyOes.Count, "Expected two body OE elements");

			foreach (var oe in bodyOes)
			{
				foreach (var attr in AuthorAttributes)
				{
					Assert.IsNull(oe.Attribute(attr),
						$"Expected attribute '{attr}' to be removed from OE but it is still present");
				}
			}
		}


		[TestMethod]
		public async Task RemoveAuthors_NoAuthorAttributes_DoesNotCallUpdate()
		{
			// Arrange: page with plain paragraphs and no author attributes
			var xml = new PageBuilder(PageId, "No Author Attributes Test")
				.WithParagraph("paragraph one")
				.WithParagraph("paragraph two")
				.Build();

			SetupPage(PageId, xml);
			var originalXml = Mock.GetPage(PageId);

			// Act
			await ExecuteCommand();

			// Assert: page XML is unchanged because UpdatePageContent was never called
			var storedXml = Mock.GetPage(PageId);
			Assert.AreEqual(originalXml, storedXml,
				"Page should not have been updated when no author attributes exist");
		}
	}
}
