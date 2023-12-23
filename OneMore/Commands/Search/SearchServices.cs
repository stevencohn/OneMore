﻿//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class SearchServices
	{
		private readonly IWin32Window owner;
		private readonly OneNote one;
		private readonly string sectionId;


		public SearchServices(IWin32Window owner, OneNote one, string sectionId)
		{
			this.owner = owner;
			this.one = one;
			this.sectionId = sectionId;
		}


		public async Task CopyPages(IEnumerable<string> pageIds)
		{
			string lastId = null;

			using (var progress = new UI.ProgressDialog())
			{
				progress.SetMaximum(pageIds.Count());
				progress.Show();

				foreach (var pageId in pageIds)
				{
					if (one.GetParent(pageId) == sectionId)
					{
						continue;
					}

					// get the page to copy
					var page = one.GetPage(pageId);
					progress.SetMessage(page.Title);

					// create a new page to get a new ID
					one.CreatePage(sectionId, out var newPageId);

					// set the page ID to the new page's ID
					page.Root.Attribute("ID").Value = newPageId;
					// remove all objectID values and let OneNote generate new IDs
					page.Root.Descendants().Attributes("objectID").Remove();
					await one.Update(page);

					lastId = newPageId;

					progress.Increment();
				}
			}

			// navigate after progress dialog is closed otherwise it will hang!
			if (lastId != null)
			{
				await one.NavigateTo(lastId);
			}
		}


		public async Task IndexPages(IEnumerable<string> pageIds)
		{
			string indexId = null;

			using (var progress = new UI.ProgressDialog())
			{
				progress.SetMaximum(pageIds.Count());
				progress.Show();

				// create a new page to get a new ID
				one.CreatePage(sectionId, out indexId);
				var indexPage = one.GetPage(indexId);

				indexPage.Title = "Page Index";

				var container = indexPage.EnsureContentContainer();

				foreach (var pageId in pageIds)
				{
					// get the page to copy
					var page = one.GetPage(pageId);
					var ns = page.Namespace;

					progress.SetMessage(page.Title);
					progress.Increment();

					var link = one.GetHyperlink(page.PageId, string.Empty);

					container.Add(new XElement(ns + "OE",
						new XElement(ns + "T",
							new XCData($"<a href=\"{link}\">{page.Title}</a>"))
						));
				}

				await one.Update(indexPage);
			}

			// navigate after progress dialog is closed otherwise it will hang!
			if (indexId != null)
			{
				await one.NavigateTo(indexId);
			}
		}


		public async Task MovePages(IEnumerable<string> pageIds)
		{
			var sections = new Dictionary<string, XElement>();
			var section = one.GetSection(sectionId);
			var ns = one.GetNamespace(section);

			var updated = false;

			using (var progress = new UI.ProgressDialog())
			{
				progress.SetMaximum(pageIds.Count());
				progress.Show();

				foreach (var pageId in pageIds)
				{
					// find the section that currently owns the page
					var parentId = one.GetParent(pageId);
					if (parentId == sectionId)
					{
						continue;
					}

					// load the owning section
					XElement parent;
					if (sections.ContainsKey(parentId))
					{
						parent = sections[parentId];
					}
					else
					{
						parent = one.GetSection(parentId);
						sections.Add(parentId, parent);
					}

					// get the Page reference within the owing section
					var element = parent.Elements(ns + "Page")
						.FirstOrDefault(e => e.Attribute("ID").Value == pageId);

					if (element != null)
					{
						progress.SetMessage(element.Attribute("name").Value);

						// remove page from current owner
						element.Remove();

						// remove misc attributes; OneNote will recreate them
						element.Attributes()
							.Where(a => a.Name != "ID" && a.Name != "name")
							.Remove();

						// add page to target section
						section.Add(element);

						updated = true;
					}

					progress.Increment();
				}
			}

			// updated at least one
			if (updated)
			{
				// update each source section
				foreach (var s in sections.Values)
				{
					one.UpdateHierarchy(s);
				}

				sections.Clear();

				// update target section
				one.UpdateHierarchy(section);

				// navigate after progress dialog is closed otherwise it will hang!
				await one.NavigateTo(sectionId);
			}
		}
	}
}
