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


		private async Task Callback(string sectionId)
		{
			if (string.IsNullOrEmpty(sectionId))
			{
				// cancelled
				return;
			}

			logger.Start($"..copying folder ..");

			try
			{
				using (var one = new OneNote())
				{
					var section = one.GetSection(sectionId);
					if (section == null)
					{
						logger.WriteLine("invalid target section");
						return;
					}

					var notebook = one.GetNotebook(OneNote.Scope.Pages);
					var ns = one.GetNamespace(notebook);

					var element = notebook.Descendants(ns + "Page")
						.FirstOrDefault(e => e.Attribute("ID").Value == one.CurrentPageId);

					var folder = element.FirstAncestor(ns + "SectionGroup");
					if (folder == null)
					{
						logger.WriteLine("error finding ancestor folder");
						return;
					}

					logger.WriteLine($"copying folder {folder.Attribute("name").Value}");

					section.Add(folder);

					await CopyPages(folder, ns);

					one.UpdateHierarchy(section);
				}

				//await service.CopyPages(pageIds);
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


		private async Task CopyPages(XElement root, XNamespace ns)
		{
			// remove the existing ID so OneNote can create its own for the new folder/section
			root.Attributes("ID").Remove();

			foreach (var element in root.Elements())
			{
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
