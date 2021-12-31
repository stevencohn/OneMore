//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using River.OneMoreAddIn;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	internal class OneNoteProvider
	{

		public CalendarItems GetPages(DateTime startDate, DateTime endDate)
		{
			using (var one = new OneNote())
			{
				//var notebooks = GetNotebooks();
				var notebooks = one.GetNotebooks(OneNote.Scope.Pages);
				var ns = notebooks.GetNamespaceOfPrefix(OneNote.Prefix);

				// filter to selected month...

				//var filter = "dateTime";
				var filter = "lastModifiedTime";

				var list = new CalendarItems();

				list.AddRange(notebooks.Descendants(ns + "Page")
					.Where(e => e.Attribute("isInRecycleBin") == null)
					.Select(e => new { Page = e, Date = DateTime.Parse(e.Attribute(filter).Value) })
					.Where(e => e.Date.CompareTo(startDate) >= 0 && e.Date.CompareTo(endDate) <= 0)
					.OrderBy(e => e.Date)
					.Select(e => new CalendarItem
					{
						Notebook = e.Page.Parent.Parent.Attribute("name").Value,
						Section = e.Page.Parent.Attribute("name").Value,
						Title = e.Page.Attribute("name").Value,
						Created = DateTime.Parse(e.Page.Attribute("dateTime").Value),
						Modified = DateTime.Parse(e.Page.Attribute("lastModifiedTime").Value),
						PageID = e.Page.Attribute("ID").Value
					}));

				return list;
			}
		}


		public IEnumerable<Notebook> GetNotebooks()
		{
			using (var one = new OneNote())
			{
				var notebooks = one.GetNotebooks();
				var ns = notebooks.GetNamespaceOfPrefix(OneNote.Prefix);

				return notebooks.Elements(ns + "Notebook")
					.Select(e => new Notebook(e));
			}
		}
	}
}
