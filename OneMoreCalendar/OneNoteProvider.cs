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


	/// <summary>
	/// 
	/// </summary>
	internal class OneNoteProvider
	{
		private OneNote one;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <param name="notebookIDs"></param>
		/// <param name="created"></param>
		/// <param name="modified"></param>
		/// <param name="deleted"></param>
		/// <returns></returns>
		public async Task<CalendarPages> GetPages(
			DateTime startDate, DateTime endDate,
			IEnumerable<string> notebookIDs,
			bool created, bool modified, bool deleted)
		{
			using (one = new OneNote())
			{
				var notebooks = await GetNotebooks(notebookIDs);
				var ns = notebooks.GetNamespaceOfPrefix(OneNote.Prefix);

				// filter to selected month...

				var pages = new CalendarPages();

				pages.AddRange(notebooks.Descendants(ns + "Page")
					.Where(e => deleted || e.Attribute("isInRecycleBin") == null)
					.Select(e => new
					{
						Page = e,
						Created = DateTime.Parse(e.Attribute("dateTime").Value),
						Modified = DateTime.Parse(e.Attribute("lastModifiedTime").Value),
						IsDeleted = e.Attribute("isInRecycleBin") != null
					})
					.Where(a =>
						(created && a.Created.InRange(startDate, endDate)) ||
						(modified && a.Modified.InRange(startDate, endDate)))
					.OrderBy(a => created ? a.Created : a.Modified)
					.Select(a => new CalendarPage
					{
						PageID = a.Page.Attribute("ID").Value,
						Path = a.Page.Ancestors()
							.Where(n => n.Attribute("name") != null)
							.Select(n => n.Attribute("name").Value)
							.Aggregate((name1, name2) => $"{name2} > {name1}"),
						Title = a.Page.Attribute("name").Value,
						Created = a.Created,
						Modified = a.Modified,
						IsDeleted = a.IsDeleted
					}));

				return pages;
			}
		}


		private async Task<XElement> GetNotebooks(IEnumerable<string> ids)
		{
			// attempt optimal ways to load...

			if (!ids.Any())
			{
				return await one.GetNotebooks(OneNote.Scope.Pages);
			}

			var notebooks = await one.GetNotebooks();
			var ns = notebooks.GetNamespaceOfPrefix(OneNote.Prefix);
			if (ids.Count() == notebooks.Elements(ns + "Notebook").Count())
			{
				var found = ids.Count(i => notebooks
					.Elements(ns + "Notebook")
					.Any(e => e.Attribute("ID").Value == i));

				if (found == ids.Count())
				{
					return await one.GetNotebooks(OneNote.Scope.Pages);
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


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public async Task<IEnumerable<Notebook>> GetNotebooks()
		{
			using (one = new OneNote())
			{
				var notebooks = await one.GetNotebooks();
				var ns = notebooks.GetNamespaceOfPrefix(OneNote.Prefix);

				return notebooks.Elements(ns + "Notebook")
					.Select(e => new Notebook(e));
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="notebookIDs"></param>
		/// <returns></returns>
		public async Task<IEnumerable<int>> GetYears(IEnumerable<string> notebookIDs)
		{
			using (one = new OneNote())
			{
				var notebooks = await GetNotebooks(notebookIDs);
				var ns = notebooks.GetNamespaceOfPrefix(OneNote.Prefix);

				var pages = notebooks.Descendants(ns + "Page");

				var years = pages
					.Select(p => DateTime.Parse(p.Attribute("dateTime").Value).Year)
					.Union(pages.Select(p => DateTime.Parse(p.Attribute("lastModifiedTime").Value).Year))
					.Distinct()
					.OrderByDescending(y => y);

				return years;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="pageID"></param>
		/// <returns></returns>
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
