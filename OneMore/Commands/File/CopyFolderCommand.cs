//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class CopyFolderCommand : Command
	{

		public CopyFolderCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote())
			{
				one.SelectLocation(
					Resx.SearchQF_Title, Resx.SearchQF_DescriptionCopy, 
					OneNote.Scope.Sections, Callback);
			}

			logger.WriteLine("selected");
			await Task.Yield();
		}


		private async Task Callback(string targetId)
		{
			if (string.IsNullOrEmpty(targetId))
			{
				// cancelled
				return;
			}

			logger.Start($"target folder {targetId}");

			try
			{
				using (var one = new OneNote())
				{
					var target = one.GetSection(targetId);
					if (target == null)
					{
						logger.WriteLine("invalid target section");
						return;
					}

					var notebook = one.GetNotebook(OneNote.Scope.Pages);
					var ns = one.GetNamespace(notebook);

					// use current page to ascend back to closest folder to handle nesting...

					var element = notebook.Descendants(ns + "Page")
						.FirstOrDefault(e => e.Attribute("ID").Value == one.CurrentPageId);

					var folder = element.FirstAncestor(ns + "SectionGroup");
					if (folder == null)
					{
						logger.WriteLine("error finding ancestor folder");
						return;
					}

					// TODO: this needs to be a recursive search
					if (folder.Attribute("ID").Value == targetId)
					{
						logger.WriteLine("cannot copy a folder into itself");
						return;
					}

					logger.WriteLine(
						$"copying folder {folder.Attribute("name").Value} " +
						$"to {target.Attribute("name").Value}");

					target.Add(folder);

					await CopyPages(one, folder, ns);

					one.UpdateHierarchy(target);
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


		private async Task CopyPages(OneNote one, XElement root, XNamespace ns)
		{
			// remove the existing ID so OneNote can create its own for the new folder/section
			root.Attributes("ID").Remove();

			foreach (var element in root.Elements(ns + "Page"))
			{
				// get the page to copy
				var page = one.GetPage(element.Attribute("ID").Value);
				logger.WriteLine($"copy page {page.Title}");

				// create a new page to get a new ID
				one.CreatePage(root.Attribute("ID").Value, out var newPageId);

				// set the page ID to the new page's ID
				page.Root.Attribute("ID").Value = newPageId;
				// remove all objectID values and let OneNote generate new IDs
				page.Root.Descendants().Attributes("objectID").Remove();
				await one.Update(page);

				// recurse...

				foreach (var section in element.Elements(ns + "SectionGroup").Elements(ns + "Section"))
				{
					await CopyPages(one, section, ns);
				}

				foreach (var section in element.Elements(ns + "Section"))
				{
					await CopyPages(one, section, ns);
				}
			}
		}

		/*
		public async Task CopyPages(List<string> pageIds)
		{
			string lastId = null;

			using (var progress = new UI.ProgressDialog())
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
		}		 */
	}
}
