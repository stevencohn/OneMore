//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class SearchServices
	{
		private readonly OneNote one;
		private readonly string sectionId;


		public SearchServices(OneNote one, string sectionId)
		{
			this.one = one;
			this.sectionId = sectionId;
		}


		/// <summary>
		/// Copies the given pages into the target section.
		/// </summary>
		/// <param name="pageIds">The IDs of the pages to copy</param>
		/// <param name="followFirst">
		/// True to navigate to the first copied page, otherwise navigate to the last
		/// </param>
		public async Task CopyPages(IEnumerable<string> pageIds, bool followFirst = false)
		{
			string firstId = null;
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
					var page = await one.GetPage(pageId);
					progress.SetMessage(page.Title);

					// create a new page to get a new ID
					one.CreatePage(sectionId, out var newPageId);

					// set the page ID to the new page's ID
					page.Root.Attribute("ID").Value = newPageId;
					// remove all objectID values and let OneNote generate new IDs
					page.Root.Descendants().Attributes("objectID").Remove();
					await one.Update(page);

					firstId ??= newPageId;
					lastId = newPageId;

					progress.Increment();
				}
			}

			// navigate after progress dialog is closed otherwise it will hang!
			var targetId = followFirst ? firstId : lastId;
			if (targetId is not null)
			{
				await one.NavigateTo(targetId);
			}
		}


		/// <summary>
		/// Moves the given pages into the target section.
		/// </summary>
		/// <param name="pageIds">The IDs of the pages to move</param>
		/// <param name="followFirst">
		/// True to navigate to the first moved page, otherwise navigate to the section
		/// </param>
		public async Task MovePages(IEnumerable<string> pageIds, bool followFirst = false)
		{
			string firstId = null;
			string firstName = null;
			var sections = new Dictionary<string, XElement>();
			var section = await one.GetSection(sectionId);
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
						parent = await one.GetSection(parentId);
						sections.Add(parentId, parent);
					}

					// get the Page reference within the owing section
					var element = parent.Elements(ns + "Page")
						.FirstOrDefault(e => e.Attribute("ID").Value == pageId);

					if (element != null)
					{
						progress.SetMessage(element.Attribute("name").Value);

						if (firstId is null)
						{
							firstId = pageId;
							firstName = element.Attribute("name").Value;
						}

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
				if (followFirst && firstId is not null)
				{
					await one.NavigateTo(await FindMovedPageId(firstId, firstName));
				}
				else
				{
					await one.NavigateTo(sectionId);
				}
			}
		}


		// OneNote may reassign the ID of a moved page; find it by ID, then by name,
		// and finally fall back to the target section itself
		private async Task<string> FindMovedPageId(string pageId, string pageName)
		{
			var target = await one.GetSection(sectionId);
			var ns = one.GetNamespace(target);
			var pages = target.Elements(ns + "Page").ToList();

			var found = pages.FirstOrDefault(e => e.Attribute("ID").Value == pageId)
				?? pages.LastOrDefault(e => e.Attribute("name")?.Value == pageName);

			return found?.Attribute("ID").Value ?? sectionId;
		}
	}
}
