﻿//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class SortListCommand : Command
	{

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
					UIHelper.ShowMessage("Place the cursor on one list item");
					return;
				}

				if (!(cursor.Parent.FirstNode is XElement first) ||
					first.Name.LocalName != "List")
				{
					UIHelper.ShowMessage("Place the cursor on one list item");
				}

				bool includeAllLists;
				bool includeChildLists;
				bool includeNumberedLists;

				using (var dialog = new SortListDialog())
				{
					var result = dialog.ShowDialog(one.Window);
					if (result == DialogResult.Cancel)
					{
						return;
					}

					includeAllLists = dialog.IncludeAllLists;
					includeChildLists = dialog.IncludeChildLists;
					includeNumberedLists = dialog.IncludeNumberedLists;
				}

				// root is the list's containing OEChildren
				var root = cursor.Parent.Parent;

				OrderList(root, includeChildLists);

				await one.Update(page);
			}

			await Task.Yield();
		}


		private void OrderList(XElement root, bool includeChildLists)
		{
			var items = root.Elements(ns + "OE")
				.OrderBy(e => e.Element(ns + "T").GetCData().GetWrapper().Value)
				.ToList();

			root.ReplaceAll(items);
		}
	}
}
