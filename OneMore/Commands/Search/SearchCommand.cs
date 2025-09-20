//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal class SearchCommand : Command
	{
		private bool copying;
		private List<string> pageIds;


		public SearchCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			copying = false;

			var dialog = new SearchDialog();
			dialog.RunModeless(async (sender, e) =>
			{
				if (sender is SearchDialog d && d.DialogResult == DialogResult.OK)
				{
					copying = d.CopySelections;
					pageIds = d.SelectedPages;

					var desc = copying
						? Resx.SearchQF_DescriptionCopy
						: Resx.SearchQF_DescriptionMove;

					await using var one = new OneNote();
					one.SelectLocation(Resx.SearchQF_Title, desc, OneNote.Scope.Sections, Callback);
				}
			},
			20);

			await Task.Yield();
		}


		private async Task Callback(string sectionId)
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
				await using var one = new OneNote();
				var service = new SearchServices(one, sectionId);

				if (copying)
				{
					await service.CopyPages(pageIds);
				}
				else
				{
					await service.MovePages(pageIds);
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

		/*
		public override async Task Execute(params object[] args)
		{
			// pattern to remove SPAN|A elements and &#nn; escaped characters
			var regex = new Regex(
				@"(?:<\s*(?:span|a)[^>]*?>)|(?:</(?:span|a)>)|(?:&#\d+;)",
				RegexOptions.Compiled);

			await using var one = new OneNote(out var page, out var ns);

			var paragraphs = page.BodyOutlines
				.Descendants(ns + "OE")
				.Where(e => e.Elements(ns + "T").Any());

			if (paragraphs.Any())
			{
				foreach (var paragraph in paragraphs)
				{
					var text = string.Empty;
					paragraph.Elements(ns + "T").ForEach(e =>
					{
						var line = e.TextValue(true).Trim();
						if (line.Length > 0)
						{
							text = $"{text}{line} ";
						}
					});

					text = regex.Replace(text.Trim(), string.Empty);
					if (text.Length > 0)
					{
						logger.WriteLine($"paragraph [{text}]");
					}
				}
			}
		}
		*/
	}
}
