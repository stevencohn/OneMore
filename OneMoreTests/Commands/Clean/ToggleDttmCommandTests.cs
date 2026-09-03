//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Clean
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Cli;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Tests.Builders;
	using System.Threading.Tasks;
	using System.Xml.Linq;

	/*
	 * Test Protocol
	 * Toggles the page date and time stamps under the title on the current page or all pages
	 * in the current section. Internally, this adds an attribute to the pageTitle like
	 * showTime="false"
	 *
	 *     1. More/Clean/Show-Hide Page Date and Time Stamps
	 *     2. Choose Hide and press OK
	 *     3. Confirm the date stamp disappears from the page
	 *     4. Repeat command, choose Show and press OK
	 *     5. Confirm the date stamp appears
	 */

	[TestClass]
	public class ToggleDttmCommandTests : TestBase
	{
		private const string PageId = "page-1";
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


		// ToggleDttmCommand's dialog-driven path (Toggle) cannot be exercised headlessly since
		// it reads dialog.PageOnly/dialog.ShowTimestamps from a ToggleDttmDialog instance. The
		// command also implements ICliPageCommand, whose Execute branch accepts a
		// CliParameterSet carrying "pageId" and "visibility" and drives the same
		// SetTimestampVisibility logic without any UI, so these tests exercise it that way.
		private static Task ExecuteCommand(string pageId, string visibility)
		{
			var cliParams = new CliParameterSet();
			cliParams.Set("pageId", pageId);
			cliParams.Set("visibility", visibility);

			return new ToggleDttmCommand().Execute(cliParams);
		}


		private static XElement BuildTitledPage(string pageId, string title,
			string showDate = null, string showTime = null)
		{
			var page = new PageBuilder(pageId, title)
				.WithParagraph("body text")
				.BuildElement();

			var titleElement = page.Element(Ns + "Title");
			if (showDate != null)
			{
				titleElement.SetAttributeValue("showDate", showDate);
			}
			if (showTime != null)
			{
				titleElement.SetAttributeValue("showTime", showTime);
			}

			return page;
		}


		[TestMethod]
		public async Task Hide_VisibleTimestamps_SetsShowDateAndShowTimeFalse()
		{
			// Arrange: title with no showDate/showTime attributes, meaning stamps are visible
			var page = BuildTitledPage(PageId, "Hide Test");
			SetupPage(PageId, page.ToString(SaveOptions.OmitDuplicateNamespaces));

			// Act
			await ExecuteCommand(PageId, "hide");

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var title = updated.Element(Ns + "Title");
			Assert.AreEqual("false", (string)title.Attribute("showDate"),
				"Expected showDate to be set to false");
			Assert.AreEqual("false", (string)title.Attribute("showTime"),
				"Expected showTime to be set to false");
		}


		[TestMethod]
		public async Task Show_HiddenTimestamps_RemovesShowDateAndShowTimeAttributes()
		{
			// Arrange: title with showDate/showTime explicitly hidden
			var page = BuildTitledPage(PageId, "Show Test", showDate: "false", showTime: "false");
			SetupPage(PageId, page.ToString(SaveOptions.OmitDuplicateNamespaces));

			// Act
			await ExecuteCommand(PageId, "show");

			// Assert
			var updated = GetUpdatedPage(PageId);
			Assert.IsNotNull(updated, "UpdatePageContent was never called");

			var title = updated.Element(Ns + "Title");
			Assert.IsNull(title.Attribute("showDate"), "Expected showDate attribute to be removed");
			Assert.IsNull(title.Attribute("showTime"), "Expected showTime attribute to be removed");
		}


		[TestMethod]
		public async Task Hide_AlreadyHidden_DoesNotCallUpdate()
		{
			// Arrange: title already hidden
			var page = BuildTitledPage(PageId, "Already Hidden Test", showDate: "false", showTime: "false");
			var xml = page.ToString(SaveOptions.OmitDuplicateNamespaces);
			SetupPage(PageId, xml);

			// Act
			await ExecuteCommand(PageId, "hide");

			// Assert: page XML is unchanged because UpdatePageContent was never called
			var storedXml = Mock.GetPage(PageId);
			Assert.AreEqual(xml, storedXml,
				"Page should not have been updated when timestamps are already hidden");
		}


		[TestMethod]
		public async Task Show_AlreadyVisible_DoesNotCallUpdate()
		{
			// Arrange: title with no showDate/showTime attributes, meaning stamps are already visible
			var page = BuildTitledPage(PageId, "Already Visible Test");
			var xml = page.ToString(SaveOptions.OmitDuplicateNamespaces);
			SetupPage(PageId, xml);

			// Act
			await ExecuteCommand(PageId, "show");

			// Assert: page XML is unchanged because UpdatePageContent was never called
			var storedXml = Mock.GetPage(PageId);
			Assert.AreEqual(xml, storedXml,
				"Page should not have been updated when timestamps are already visible");
		}
	}
}
