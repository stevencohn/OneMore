//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using River.OneMoreAddIn;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class OneNoteProvider
	{
		private OneNote one;


		public CalendarItems GetPages(
			DateTime startDate, DateTime endDate,
			IEnumerable<string> notebookIDs,
			bool created, bool modified, bool deleted)
		{
			using (one = new OneNote())
			{
				var notebooks = GetNotebooks(notebookIDs);
				var ns = notebooks.GetNamespaceOfPrefix(OneNote.Prefix);

				// filter to selected month...

				var list = new CalendarItems();

				list.AddRange(notebooks.Descendants(ns + "Page")
					.Where(e => deleted || e.Attribute("isInRecycleBin") == null)
					.Select(e => new
					{
						Page = e,
						Created = DateTime.Parse(e.Attribute("dateTime").Value),
						Modified = DateTime.Parse(e.Attribute("lastModifiedTime").Value),
						Deleted = e.Attribute("isInRecycleBin") != null
					})
					.Where(a =>
						(!created || (created && a.Created.InRange(startDate, endDate))) &&
						(!modified || (modified && a.Modified.InRange(startDate, endDate))))
					.OrderBy(a => created ? a.Created : a.Modified)
					.Select(a => new CalendarItem
					{
						PageID = a.Page.Attribute("ID").Value,
						Path = a.Page.Ancestors()
							.Where(n => n.Attribute("name") != null)
							.Select(n => n.Attribute("name").Value)
							.Aggregate((name1, name2) => $"{name2}/{name1}"),
						Title = a.Page.Attribute("name").Value,
						Created = a.Created,
						Modified = a.Modified
					}));

				return list;
			}
		}


		private XElement GetNotebooks(IEnumerable<string> ids)
		{
			if (!ids.Any())
			{
				return one.GetNotebooks(OneNote.Scope.Pages);
			}

			var notebooks = one.GetNotebooks();
			var ns = notebooks.GetNamespaceOfPrefix(OneNote.Prefix);
			if (ids.Count() == notebooks.Elements(ns + "Notebook").Count())
			{
				var found = ids.Count(i => notebooks
					.Elements(ns + "Notebook")
					.Any(e => e.Attribute("ID").Value == i));

				if (found == ids.Count())
				{
					return one.GetNotebooks(OneNote.Scope.Pages);
				}
			}

			notebooks.Elements(ns + "Notebook").Remove();
			foreach (var id in ids)
			{
				var book = one.GetNotebook(id, OneNote.Scope.Pages);
				notebooks.Add(book);
			}

			return notebooks;
		}


		public IEnumerable<Notebook> GetNotebooks()
		{
			using (one = new OneNote())
			{
				var notebooks = one.GetNotebooks();
				var ns = notebooks.GetNamespaceOfPrefix(OneNote.Prefix);

				return notebooks.Elements(ns + "Notebook")
					.Select(e => new Notebook(e));
			}
		}



		public async Task NavigateTo(string pageID)
		{
			using (one = new OneNote())
			{
				var url = one.GetHyperlink(pageID, string.Empty);
				if (url != null)
				{
					await one.NavigateTo(url);
				}
			}

		}
	}
}
