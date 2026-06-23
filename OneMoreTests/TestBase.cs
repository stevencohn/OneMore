//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using System.Xml.Linq;


	/// <summary>
	/// Base class for all OneMore unit tests. Sets up MockApplication before each test
	/// and tears it down after, so each test starts with a clean in-memory OneNote state.
	/// </summary>
	[TestClass]
	public abstract class TestBase
	{
		protected MockApplication Mock { get; private set; }


		[AssemblyInitialize]
		public static void AssemblyInit(TestContext context)
		{
			Logger.SetApplication("OneMore-UnitTests");
		}


		[TestInitialize]
		public void Setup()
		{
			Mock = new MockApplication();
			ApplicationFactory.RegisterApplication(Mock);
		}


		[TestCleanup]
		public void Teardown()
		{
			ApplicationFactory.RegisterApplication((Microsoft.Office.Interop.OneNote.IApplication)null);
		}


		/// <summary>
		/// Stores a page XML string in the mock under the given ID and sets it as the current page.
		/// </summary>
		protected void SetupPage(string pageId, string xml)
		{
			Mock.SetPage(pageId, xml);
			Mock.CurrentPageId = pageId;
		}


		/// <summary>
		/// Retrieves and parses the page XML that was last written by UpdatePageContent.
		/// Returns null if the page was never updated.
		/// </summary>
		protected XElement GetUpdatedPage(string pageId)
		{
			var xml = Mock.GetPage(pageId);
			return xml is null ? null : XElement.Parse(xml);
		}
	}
}
