//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;

	internal class ShowKeyboardShortcutsCommand : Command
	{
		public ShowKeyboardShortcutsCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			using (var one = new OneNote())
			{
				var results = one.SearchMeta(string.Empty, "omKeyboardShortcuts");
				var ns = one.GetNamespace(results);

				var pageId = results?.Descendants(ns + "Meta")
					.Where(e => e.Attribute("name").Value == "omKeyboardShortcuts")
					.Select(e => e.Parent.Attribute("ID").Value)
					.FirstOrDefault();

				if (pageId != null)
				{
					one.NavigateTo(pageId);
				}
			}
		}
	}
}