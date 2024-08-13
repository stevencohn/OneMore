//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Moves selected pages to either the top or bottom of the section page list.
	/// </summary>
	internal class MovePageCommand : Command
	{

		public MovePageCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote();

			var section = await one.GetSection();
			var ns = section.GetNamespaceOfPrefix(OneNote.Prefix);

			var pages = section.Elements(ns + "Page")
				.Where(e => e.Attributes().Any(a =>
					// one or more pages selected
					(a.Name == "selected" && a.Value == "all") ||
					// only the current page selected
					(a.Name == "isCurrentlyViewed" && a.Value == "true")))
				.ToList();

			if (!pages.Any())
			{
				ShowError(Resx.MovePageCommand_noPages);
				return;
			}

			if (args.Length > 0 && args[0] is bool top && top)
			{
				if (pages[0].PreviousNode is not null)
				{
					pages.Remove();
					section.AddFirst(pages);
					one.UpdateHierarchy(section);
				}
			}
			else
			{
				if (pages[pages.Count - 1].NextNode is not null)
				{
					pages.Remove();
					section.Add(pages);
					one.UpdateHierarchy(section);
				}
			}
		}
	}
}
