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
			using var one = new OneNote(out var page, out ns);
			var cursor = page.GetTextCursor();
			if (cursor == null)
			{
				UIHelper.ShowMessage(Resx.SortListCommand_BadContext);
				return;
			}

			if (cursor.Parent.FirstNode is not XElement first ||
				first.Name.LocalName != "List")
			{
				UIHelper.ShowMessage(Resx.SortListCommand_BadContext);
				return;
			}

			using var dialog = new SortListDialog();
			var result = dialog.ShowDialog();
			if (result == DialogResult.Cancel)
			{
				return;
			}

			if (dialog.IncludeAllLists)
			{
				var lists = page.Root.Descendants(ns + "OEChildren")
					.Elements(ns + "OE")
					.Elements(ns + "List")
					.Select(e => e.Parent.Parent)
					.Distinct();

				if (!dialog.IncludeChildLists)
				{
					// whittle it down to only top level lists
					lists = lists.Where(
						e => e.Parent.Elements().First().Name.LocalName != "List");
				}

				if (!dialog.IncludeNumberedLists)
				{
					// whittle it down to only bulleted lists
					lists = lists.Where(
						e => !e.Elements(ns + "OE")
							.Elements(ns + "List").Elements(ns + "Number").Any());
				}

				foreach (var list in lists)
				{
					OrderList(list, false, dialog.RemoveDuplicates);
				}
			}
			else
			{
				// root is the list's containing OEChildren
				var list = cursor.Parent.Parent;
				OrderList(list, dialog.IncludeChildLists, dialog.RemoveDuplicates);
			}

			await one.Update(page);
		}


		private void OrderList(XElement list, bool includeChildLists, bool removeDuplicates)
		{
			// keep empty items with their preceding item, e.g. if an item with content is
			// followed by two empty items then those two empty items are kept with the first

			// list items look like OE/List,T or OE/List,Image, ...
			// so this prefers to look for Ts so it can use its text to alphabetize and will
			// order images and other sibling elements, usually putting them first

			// find all non-empty items
			var items = list.Elements(ns + "OE")
				.Select(e => new 
				{ 
					Element = e,
					Text = e.Elements(ns + "T")?

						// TODO: using Aggregate here because GetCData alone will return only the
						// first T which may be the select=all empty run which will end up
						// deleting the item! (Issue #742)
						// TODO: investigate whether GetCData is correct
						// TODO: investigate whether we can optimize TextValue using Aggregate

						.Aggregate(string.Empty, (s, t) => $"{s}{t.GetCData().Value}", s => s)
				})
				.Where(item => item.Text == null || item.Text.Length > 0)
				.Select(item => new ListItem
				{
					Item = item.Element,
					Text = item.Text?.Trim(),
					Spaces = new List<XElement>()
				})
				.ToList();

			if (!items.Any())
			{
				// list contains only empty items!
				return;
			}

			// for each item, unwrap hyperlinks to get to the display text
			// and drag along any immediately following empty items
			foreach (var item in items)
			{
				// unwrap hyperlinks
				if (item.Text.Contains("<a"))
				{
					item.Text = item.Text.ToXmlWrapper().Value;
				}

				// drag along empty followers
				var element = item.Item;
				while (element.NextNode is XElement next &&
					next.Elements(ns + "T")?
						.Aggregate(string.Empty, (s, t) => $"{s}{t.GetCData().Value}", s => s).Length == 0)
				{
					item.Spaces.Add(next);
					element = next;
				}
			}

			// sort and repopulate
			list.RemoveAll();

			if (removeDuplicates)
			{
				items = items.GroupBy(i => i.Text).Select(g => g.First()).ToList();
			}

			var ordered = items.OrderBy(i => i.Text);

			foreach (var item in ordered)
			{
				list.Add(item.Item);

				if (item.Spaces.Any())
				{
					list.Add(item.Spaces);
				}
			}

			if (includeChildLists)
			{
				// DIVE! DIVE! DIVE!
				foreach (var item in items)
				{
					if (item.Item.LastNode is XElement last &&
						last.Name.LocalName == "OEChildren")
					{
						OrderList(last, includeChildLists, removeDuplicates);
					}
				}
			}
		}
	}
}
