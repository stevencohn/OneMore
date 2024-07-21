//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	#region Wrapper
	internal class PreviousUnreadPageCommand : NextUnreadPageCommand
	{
		public PreviousUnreadPageCommand() : base() { }
		public override Task Execute(params object[] args)
		{
			return base.Execute(false);
		}
	}
	#endregion Wrapper


	/// <summary>
	/// Navigate to the next or previous unread page if any.
	/// </summary>
	internal class NextUnreadPageCommand : Command
	{
		public NextUnreadPageCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var forward = args.Length == 0 || (bool)args[0];

			await using var one = new OneNote();
			var notebooks = await one.GetNotebooks();
			var ns = notebooks.GetNamespaceOfPrefix(OneNote.Prefix);

			var books = notebooks.Elements(ns + "Notebook");
			if (!forward)
			{
				books = books.Reverse();
			}

			// load and scan one notebook at a time in case any are huge...

			XElement next = null;
			foreach (var book in books)
			{
				var notebook = await one.GetNotebook(
					book.Attribute("ID").Value, OneNote.Scope.Pages);

				// create a new root with a flat list of pages so ElementsAfter/BeforeSelf
				// can be used to easily scan...

				var pages = new XElement(ns + "pages",
					notebook.Descendants(ns + "Page").Where(e =>
						e.Attribute("isCurrentlyViewed") is not null ||
						e.Attribute("isUnread") is not null)
					);

				if (!pages.HasElements)
				{
					// must not be current notebook and has no unread pages
					continue;
				}

				if (notebook.Attribute("isCurrentlyViewed") is not null)
				{
					// find the anchor point
					var current = pages.Elements()
						.First(e => e.Attribute("isCurrentlyViewed") is not null);

					// find next unread in desired direction
					next = forward
						? current.ElementsAfterSelf()
							.FirstOrDefault(e => e.Attribute("isUnread") is not null)
						: current.ElementsBeforeSelf()
							.FirstOrDefault(e => e.Attribute("isUnread") is not null);
				}
				else
				{
					// in other notebook, no current page, so blindly grab the first unread found
					next = forward
						? pages.Elements().First()
						: pages.Elements().Last();
				}

				if (next is not null)
				{
					break;
				}
			}

			if (next is null)
			{
				ShowInfo(Resx.NextUnreadPageCommand_nomore);
				return;
			}

			await one.NavigateTo(next.Attribute("ID").Value);
		}
	}
}
