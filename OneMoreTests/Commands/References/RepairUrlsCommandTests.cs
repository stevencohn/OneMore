//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.References
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;


	/*
	 * Test Protocol
	 * Commands/References/RepairUrlsCommand
	 *
	 * These unit tests exercise IsStale, which decides whether a link differs from the link
	 * OneNote makes today for the same page. A link made inside its own notebook names only the
	 * section file and carries the notebook folder in base-path; it must compare equal to the
	 * absolute form that names the whole path (#2724).
	 */
	[TestClass]
	public class RepairUrlsCommandTests
	{
		private const string Section = "{28B2D6E5-D7B5-4C7C-A20B-703AB6ADC05D}";
		private const string Page = "{1F32691A-483B-4F0C-AF18-20795735EC11}";
		private const string Object = "{EDEA9CC6-D8A0-0269-1C66-4BE881F2898C}";
		private const string Base = "https://d.docs.live.net/9bd3f986a71fcc33/OneNote/DAP";


		[TestMethod]
		public void RelativeLinkEqualsAbsoluteLink()
		{
			var href =
				$"onenote:Active%20GR%27s.one#Title&section-id={Section}&page-id={Page}" +
				$"&object-id={Object}&11&base-path={Base}";

			var fresh =
				$"onenote:{Base}/Active%20GR%27s.one#Title&section-id={Section}&page-id={Page}" +
				$"&object-id={Object}&11";

			Assert.IsFalse(RepairUrlsCommand.IsStale(href, fresh));
		}


		[TestMethod]
		public void AbsoluteLinksAreEqual()
		{
			var href = $"onenote:{Base}/Active.one#T&section-id={Section}&page-id={Page}&end";
			Assert.IsFalse(RepairUrlsCommand.IsStale(href, href));
		}


		[TestMethod]
		public void MovedSectionIsStale()
		{
			var href =
				$"onenote:Active.one#T&section-id={Section}&page-id={Page}&end&base-path={Base}";

			var fresh =
				$"onenote:{Base}/Other.one#T&section-id={{11111111-2222-3333-4444-555555555555}}" +
				$"&page-id={Page}&end";

			Assert.IsTrue(RepairUrlsCommand.IsStale(href, fresh));
		}


		[TestMethod]
		public void DifferentNotebookIsStale()
		{
			var href =
				$"onenote:Active.one#T&section-id={Section}&page-id={Page}&end&base-path={Base}";

			var fresh =
				$"onenote:https://d.docs.live.net/9bd3f986a71fcc33/OneNote/Elsewhere/Active.one" +
				$"#T&section-id={Section}&page-id={Page}&end";

			Assert.IsTrue(RepairUrlsCommand.IsStale(href, fresh));
		}


		[TestMethod]
		public void LocalDrivePathIsNotRebased()
		{
			var href =
				$"onenote:C:\\Notes\\Active.one#T&section-id={Section}&page-id={Page}&end";

			var fresh =
				$"onenote:C:\\Notes\\Active.one#T&section-id={Section}&page-id={Page}&end";

			Assert.IsFalse(RepairUrlsCommand.IsStale(href, fresh));
		}
	}
}
