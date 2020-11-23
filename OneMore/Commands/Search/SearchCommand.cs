//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Commands.Search;
	using River.OneMoreAddIn.Dialogs;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class SearchCommand : Command
	{
		private bool copying;
		private List<string> pageIds;


		public SearchCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			// search for keywords and find page

			copying = false;

			using (var one = new OneNote())
			{
				// start background navigator in base thread
				one.StartDispatcher();

				using (var dialog = new SearchDialog(one))
				{
					if (dialog.ShowDialog(owner) != DialogResult.OK)
					{
						return;
					}

					copying = dialog.CopySelections;
					pageIds = dialog.SelectedPages;
				}

				// choose where to copy/move the selected pages
				// This needs to be done here, on this thread; I've tried to play threading tricks
				// to do this from the SearchDialog but could find a way to prevent hanging

				var desc = copying
					? Resx.SearchQF_DescriptionCopy
					: Resx.SearchQF_DescriptionMove;

				one.SelectLocation(Resx.SearchQF_Title, desc, OneNote.Scope.Sections, Callback);
			}
		}


		private void Callback(string sectionId)
		{
			if (string.IsNullOrEmpty(sectionId))
			{
				// cancelled
				return;
			}

			var action = copying ? "copying" : "moving";
			logger.Start($"..{action} {pageIds.Count} pages");

			try
			{
				using (var one = new OneNote())
				{
					if (copying)
					{
						CopyPages(sectionId, one);
					}
					else
					{
						MovePages(sectionId, one);
					}

					one.NavigateTo(sectionId);
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine(exc);
			}
			finally
			{
				logger.End();
			}
		}


		private void CopyPages(string sectionId, OneNote one)
		{
			using (var progress = new ProgressDialog())
			{
				progress.SetMaximum(pageIds.Count);
				progress.Show(owner);

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
					one.Update(page);

					progress.Increment();
				}
			}
		}


		private void MovePages(string sectionId, OneNote one)
		{
			var sections = new Dictionary<string, XElement>();
			var section = one.GetSection(sectionId);
			var ns = one.GetNamespace(section);

			var updated = false;

			using (var progress = new ProgressDialog())
			{
				progress.SetMaximum(pageIds.Count);
				progress.Show(owner);

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
			}
		}
	}
}
