//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	/// <summary>
	/// Sort the items in one or more bulleted or numbered lists
	/// </summary>
	internal class SortListCommand : Command
	{
		private sealed class ListItem
		{
			public XElement Item;
			public string Text;
			public List<XElement> Spaces;
		}


		private XNamespace ns;


		public SortListCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out ns))
			{
				var cursor = page.GetTextCursor();
				if (cursor == null)
				{
					UIHelper.ShowMessage(Resx.SortListCommand_BadContext);
					return;
				}

				if (!(cursor.Parent.FirstNode is XElement first) ||
					first.Name.LocalName != "List")
				{
					UIHelper.ShowMessage(Resx.SortListCommand_BadContext);
					return;
				}

				bool includeAllLists;
				bool includeChildLists;
				bool includeNumberedLists;

				using (var dialog = new SortListDialog())
				{
					var result = dialog.ShowDialog(owner);
					if (result == DialogResult.Cancel)
					{
						return;
					}

					includeAllLists = dialog.IncludeAllLists;
					includeChildLists = dialog.IncludeChildLists;
					includeNumberedLists = dialog.IncludeNumberedLists;
				}

				if (includeAllLists)
				{
					var lists = page.Root.Descendants(ns + "OEChildren")
						.Elements(ns + "OE")
						.Elements(ns + "List")
						.Select(e => e.Parent.Parent)
						.Distinct();

					if (!includeChildLists)
					{
						// whittle it down to only top level lists
						lists = lists.Where(
							e => e.Parent.Elements().First().Name.LocalName != "List");
					}

					if (!includeNumberedLists)
					{
						// whittle it down to only bulleted lists
						lists = lists.Where(
							e => !e.Elements(ns + "OE")
								.Elements(ns + "List").Elements(ns + "Number").Any());
					}

					foreach (var list in lists)
					{
						OrderList(list, false);
					}
				}
				else
				{
					// root is the list's containing OEChildren
					var root = cursor.Parent.Parent;
					OrderList(root, includeChildLists);
				}

				await one.Update(page);
			}
		}


		private void OrderList(XElement root, bool includeChildLists)
		{
			// keep empty items with their preceding item, e.g. if an item with content is
			// followed by two empty items then those two empty items are kept with the first

			// list items look like OE/List,T or OE/List,Image, ...
			// so this prefers to look for Ts so it can use its text to alphabetize and will
			// order images and other sibling elements, usually putting them first

			// find all non-empty items
			var items = root.Elements(ns + "OE")
				.Select(e => new { Element = e, Text = e.Element(ns + "T")?.GetCData().Value })
				.Where(item => item.Text == null || item.Text.Length > 0)
				.Select(item => new ListItem
				{
					Item = item.Element,
					Text = item.Text,
					Spaces = new List<XElement>()
				})
				.ToList();

			if (!items.Any())
			{
				// list contains only empty items!
				return;
			}

			// for each item, drag along any immediately following empty items
			foreach (var item in items)
			{
				var element = item.Item;
				while (element.NextNode is XElement next &&
					next.Element(ns + "T")?.GetCData().Value.Length == 0)
				{
					item.Spaces.Add(next);
					element = next;
				}
			}

			// sort and repopulate
			root.RemoveAll();
			foreach (var item in items.OrderBy(i => i.Text))
			{
				root.Add(item.Item);
				root.Add(item.Spaces);
			}

			if (includeChildLists)
			{
				// DIVE! DIVE! DIVE!
				foreach (var item in items)
				{
					if (item.Item.LastNode is XElement last &&
						last.Name.LocalName == "OEChildren")
					{
						OrderList(last, includeChildLists);
					}
				}
			}
		}
	}
}
