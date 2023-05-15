//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// An abstraction of the OneNote class, provides a bit of decoupling between
	/// the calendar app and the OneNote addin library
	/// </summary>
	internal class OneNoteProvider
	{

		/// <summary>
		/// Export an EMF representation of the specified page to the TEMP folder
		/// </summary>
		/// <param name="pageID"></param>
		/// <returns>The path of the file generated</returns>
		public string Export(string pageID)
		{
			var path = Path.Combine(
				Path.GetTempPath(),
				Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".emf");

			using var one = new OneNote();
			one.Export(pageID, path, OneNote.ExportFormat.EMF);

			return path;
		}


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
			using var one = new OneNote();

			var notebooks = await GetNotebooks(notebookIDs);
			var ns = notebooks.GetNamespaceOfPrefix(OneNote.Prefix);

			// filter to selected month...

			var pages = new CalendarPages();

			pages.AddRange(notebooks.Descendants(ns + "Page")
				.Where(e => deleted || e.Attribute("isInRecycleBin") == null)
				// collect all pages
				.Select(e => new
				{
					Page = e,
					Created = DateTime.Parse(e.Attribute("dateTime").Value),
					Modified = DateTime.Parse(e.Attribute("lastModifiedTime").Value),
					IsDeleted = e.Attribute("isInRecycleBin") != null
				})
				// filter by one or both filters
				.Where(a =>
					(created && a.Created.InRange(startDate, endDate)) ||
					(modified && a.Modified.InRange(startDate, endDate)))
				// prefer creation time
				.OrderBy(a => created ? a.Created : a.Modified)
				// pretty it up
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
					IsDeleted = a.IsDeleted,
					HasReminders = a.Page.Elements(ns + "Meta")
						.Any(e => e.Attribute("name").Value == MetaNames.Reminder)
				}));

			pages.ForEach(page =>
			{
				var DeletedPages = "OneNote_RecycleBin > Deleted Pages";
				if (page.Path.EndsWith(DeletedPages))
				{
					page.Path = page.Path.Substring(
						0, page.Path.Length - DeletedPages.Length) + "Recycle Bin";
				}
			});

			return pages;
		}


		private async Task<XElement> GetNotebooks(IEnumerable<string> ids)
		{
			// attempt optimal ways to load...

			using var one = new OneNote();

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

			// filter out unknown notebookIDs to avoid uncatchable exception!
			var nids = notebooks.Elements(ns + "Notebook").Select(e => e.Attribute("ID").Value);
			var knownIDs = ids.Where(i => nids.Contains(i));

			var books = new XElement(ns + "Notebooks",
				new XAttribute(XNamespace.Xmlns + OneNote.Prefix, ns)
				);

			foreach (var id in knownIDs)
			{
				var book = await one.GetNotebook(id, OneNote.Scope.Pages);
				books.Add(book);
			}

			// return filtered list; otherwise return all notebooks
			return books.Elements().Any() ? books : notebooks;
		}


		/// <summary>
		/// Get a collection of all available notebooks
		/// </summary>
		/// <returns></returns>
		public async Task<IEnumerable<Notebook>> GetNotebooks()
		{
			using var one = new OneNote();
			var notebooks = await one.GetNotebooks();
			var ns = notebooks.GetNamespaceOfPrefix(OneNote.Prefix);

			return notebooks.Elements(ns + "Notebook")
				.Select(e => new Notebook(e));
		}


		/// <summary>
		/// Get a collection of all unique years in the given notebooks
		/// </summary>
		/// <param name="notebookIDs"></param>
		/// <returns></returns>
		public async Task<IEnumerable<int>> GetYears(IEnumerable<string> notebookIDs)
		{
			using var one = new OneNote();
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


		/// <summary>
		/// Open OneNote and navigate to the specified page
		/// </summary>
		/// <param name="pageID"></param>
		/// <returns></returns>
		public async Task NavigateTo(string pageID)
		{
			using var one = new OneNote();
			var url = one.GetHyperlink(pageID, string.Empty);
			if (!string.IsNullOrEmpty(url))
			{
				await one.NavigateTo(url);
			}
		}
	}
}
