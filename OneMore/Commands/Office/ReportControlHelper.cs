//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Xml.Linq;


	/// <summary>
	/// Shared building blocks for the "control row" used atop generated contact
	/// reports: a 1x3 table with an empty leading/trailing cell and a status/refresh
	/// line in the middle, plus the lookup used to find that row's table again when
	/// refreshing a report in place.
	/// </summary>
	internal static class ReportControlHelper
	{
		/// <summary>
		/// Builds the standard 1x3 control-row table: an empty spacer cell, the given
		/// content in the middle cell, and a trailing empty spacer cell.
		/// </summary>
		/// <param name="ns">The page namespace</param>
		/// <param name="shading">The row's background shading color</param>
		/// <param name="col0Width">Width of the leading spacer column</param>
		/// <param name="col1Width">Width of the content column</param>
		/// <param name="col2Width">Width of the trailing spacer column</param>
		/// <param name="content">The paragraph to place in the middle cell</param>
		/// <returns>The control-row Table</returns>
		public static Table BuildControlTable(
			XNamespace ns, string shading, int col0Width, int col1Width, int col2Width,
			Paragraph content)
		{
			var table = new Table(ns, 1, 3)
			{
				HasHeaderRow = true
			};

			table.SetColumnWidth(0, col0Width);
			table.SetColumnWidth(1, col1Width);
			table.SetColumnWidth(2, col2Width);

			var row = table[0];
			row.SetShading(shading);
			row[0].SetContent(new Paragraph(string.Empty));
			row[1].SetContent(content);
			row[2].SetContent(new Paragraph(string.Empty));

			return table;
		}


		/// <summary>
		/// Finds the Table element previously stamped with the given meta name and
		/// guid (via Paragraph.SetMeta), as used when a report's data table is later
		/// refreshed in place.
		/// </summary>
		/// <param name="page">The page to search</param>
		/// <param name="ns">The page namespace</param>
		/// <param name="metaName">The meta name the table was stamped with</param>
		/// <param name="guid">The unique content value the table was stamped with</param>
		/// <returns>The matching Table element, or null if not found</returns>
		public static XElement FindReportTable(Page page, XNamespace ns, string metaName, string guid) =>
			page.Root.Descendants(ns + "Meta")
				.FirstOrDefault(e =>
					e.Attribute("name").Value == metaName &&
					e.Attribute("content").Value == guid &&
					e.Parent.Elements(ns + "Table").Any())
				?.Parent.Elements(ns + "Table").First();
	}
}
