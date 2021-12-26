//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using River.OneMoreAddIn;
	using System;
	using System.Linq;
	using System.Xml.Linq;

	internal class OneNoteProvider
	{
		public CalendarItems GetPages()
		{
			using (var one = new OneNote())
			{
				// find Personal notebook...

				var notebooks = one.GetNotebooks();
				var ns = notebooks.GetNamespaceOfPrefix(OneNote.Prefix);

				var personalID = notebooks.Elements(ns + "Notebook")
					.Where(e => e.Attribute("name").Value == "Personal")
					.Select(e => e.Attribute("ID").Value)
					.FirstOrDefault();


				var notebook = one.GetNotebook(personalID, OneNote.Scope.Pages);
				ns = notebook.GetNamespaceOfPrefix(OneNote.Prefix);

				// filter to selected month...

				var now = DateTime.Now;
				//var filter = "dateTime";
				var filter = "lastModifiedTime";

				var list = new CalendarItems();
				
				list.AddRange(notebook.Elements().Elements(ns + "Page")
					.Select(e => new { Page = e, Time = DateTime.Parse(e.Attribute(filter).Value) })
					.Where(e => e.Time.Year == now.Year && e.Time.Month == now.Month)
					.OrderBy(e => e.Time)
					.Select(e => new CalendarItem
					{
						Section = e.Page.Parent.Attribute("name").Value,
						Title = e.Page.Attribute("name").Value,
						Created = DateTime.Parse(e.Page.Attribute("dateTime").Value),
						Modified = DateTime.Parse(e.Page.Attribute("lastModifiedTime").Value),
						PageID = e.Page.Attribute("ID").Value
					}));

				return list;
			}
		}
	}
}
