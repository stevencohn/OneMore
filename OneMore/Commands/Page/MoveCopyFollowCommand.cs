//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Resx = Properties.Resources;


	/// <summary>
	/// Moves or copies the selected pages to a section chosen by the user and then
	/// navigates to the first page that was moved or copied.
	/// </summary>
	internal class MoveCopyFollowCommand : Command
	{
		private bool copy;
		private List<string> pageIds;


		public MoveCopyFollowCommand()
		{
		}


		/// <summary>
		/// Pass true as the first argument to copy pages; otherwise they are moved.
		/// </summary>
		public override async Task Execute(params object[] args)
		{
			copy = args.Length > 0 && args[0] is bool b && b;

			await using var one = new OneNote();

			var section = await one.GetSection();
			var ns = section.GetNamespaceOfPrefix(OneNote.Prefix);

			pageIds = section.Elements(ns + "Page")
				.Where(e =>
					// one or more pages selected
					e.Attribute("selected")?.Value == "all" ||
					// only the current page selected
					e.Attribute("isCurrentlyViewed")?.Value == "true")
				.Select(e => e.Attribute("ID").Value)
				.ToList();

			if (pageIds.Count == 0)
			{
				ShowError(Resx.MovePageCommand_noPages);
				return;
			}

			var description = copy
				? Resx.SearchQF_DescriptionCopy
				: Resx.SearchQF_DescriptionMove;

			one.SelectLocation(Resx.SearchQF_Title, description, OneNote.Scope.Sections, Callback);
		}


		private async Task Callback(string sectionId)
		{
			if (string.IsNullOrEmpty(sectionId))
			{
				// cancelled
				return;
			}

			using var indent = logger.Indent($"..{(copy ? "copy" : "move")} pages and follow");

			try
			{
				await using var one = new OneNote();
				var service = new SearchServices(one, sectionId);

				if (copy)
				{
					await service.CopyPages(pageIds, followFirst: true);
				}
				else
				{
					await service.MovePages(pageIds, followFirst: true);
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine(exc);
			}
		}
	}
}
