//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Commands.Search;
	using System;
	using System.Collections.Generic;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class SearchCommand : Command
	{
		private bool copying;
		private List<string> selections;


		public SearchCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			// search for keywords and find page

			copying = false;

			using (var dialog = new SearchDialog())
			{
				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}

				copying = dialog.CopySelections;
				selections = dialog.SelectedPages;
			}

			logger.WriteLine($"selected {selections.Count} pages");

			// choose where to copy/move the selected pages
			// This needs to be done here, on this thread; I've tried to play threading tricks
			// to do this from the SearchDialog but could find a way to prevent hanging

			using (var one = new OneNote())
			{
				one.SelectLocation(
					Resx.SearchQF_Title, Resx.SearchQF_Description,
					OneNote.Scope.Sections, Callback);
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
			logger.Start($"..{action} {selections.Count} pages");

			try
			{
				using (var one = new OneNote())
				{
					var section = one.GetSection(sectionId);

					foreach (var pageId in selections)
					{
						if (one.GetParent(pageId) == sectionId)
						{
							continue;
						}

						if (copying)
						{
							one.CreatePage(sectionId, out var newPageId);

							// get the page to copy
							var page = one.GetPage(pageId);
							// set the page ID to the new page's ID
							page.Root.Attribute("ID").Value = newPageId;
							// remove all objectID values and let OneNote generate new IDs
							page.Root.Descendants().Attributes("objectID").Remove();
							one.Update(page);
						}
						else
						{
							//
						}
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
	}
}
