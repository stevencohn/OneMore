//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Interop.OneNote;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class SortCommand : Command
	{
		private class PageNode
		{
			public XElement Page;
			public List<XElement> Children;
			public PageNode(XElement page) { this.Page = page; this.Children = new List<XElement>(); }
		};


		private HierarchyScope scope;
		private SortDialog.Sortings sorting;
		private SortDialog.Directions direction;


		public SortCommand() : base()
		{
		}


		public void Execute()
		{
			logger.WriteLine("SortCommand.Execute()");

			DialogResult result = DialogResult.None;

			using (var dialog = new SortDialog())
			{
				result = dialog.ShowDialog(owner);
				dialog.Focus();

				result = dialog.DialogResult;
				scope = dialog.Scope;
				sorting = dialog.Soring;
				direction = dialog.Direction;
			}

			if (result == DialogResult.OK)
			{
				logger.WriteLine($"- sort scope [{scope}]");
				logger.WriteLine($"- sort sorting[{sorting}]");
				logger.WriteLine($"- sort direction [{direction}]");

				using (var manager = new ApplicationManager())
				{
					XElement root;

					if (scope == HierarchyScope.hsPages)
					{
						// get just the current section with all pages as child elements
						root = manager.CurrentSection();
						root = SortPages(root, sorting, direction);
					}
					else
					{
						root = manager.GetHierarchy(scope);

						if (scope == HierarchyScope.hsNotebooks)
						{
							root = SortNotebooks(root, sorting, direction);
						}
						else
						{
							// sections will include all sections for the current notebook
						}
					}

					manager.UpdateHierarchy(root);
				}
			}
		}


		private XElement SortPages(
			XElement root, SortDialog.Sortings sorting, SortDialog.Directions direction)
		{
			/*
			 * <one:Page ID="{B49..." 
			 *		name="Notes"
			 *		dateTime="2018-01-24T03:23:12.000Z"
			 *		lastModifiedTime="2019-10-26T12:56:53.000Z"
			 *		pageLevel="1" />
			 */

			string keyName;

			if (sorting == SortDialog.Sortings.ByCreated) keyName = "dateTime";
			else if (sorting == SortDialog.Sortings.ByModified) keyName = "lastModifiedTime";
			else keyName = "name";

			var ns = root.GetNamespaceOfPrefix("one");

			/*
			var pages =
				from p in root.Elements(ns + "Page")
				// keep only printable characters (don't sort on title emoticons!)
				let key = Regex.Replace(p.Attribute(keyName).Value, @"[^ -~]", "")
				orderby key
				select p;
			*/

			var pages = new List<PageNode>();

			foreach (var child in root.Elements(ns + "Page"))
			{
				if (child.Attribute("pageLevel").Value == "1")
				{
					pages.Add(new PageNode(child));
				}
				else
				{
					pages[pages.Count - 1].Children.Add(child);
				}
			}

			pages =
				(from p in pages
				 let key = Regex.Replace(p.Page.Attribute(keyName).Value, @"[^ -~]", "")
				 orderby key
				 select p).ToList();

			if (direction == SortDialog.Directions.Descending)
			{
				pages.Reverse();
			}

			root.RemoveNodes();

			foreach (var page in pages)
			{
				root.Add(page.Page);

				foreach (var child in page.Children)
				{
					root.Add(child);
				}
			}

			//root.ReplaceNodes(pages);

			return root;
		}


		private XElement SortNotebooks(
			XElement root, SortDialog.Sortings sorting, SortDialog.Directions direction)
		{
			/*
			 * <one:Notebook
			 *		name="Personal"
			 *		nickname="Personal"
			 *		ID="{CAE56365-6026-4E6C-A313-667D6FEBE5D8}{1}{B0}"
			 *		path="https://d.docs.live.net/6925d0374517d4b4/Documents/Personal/"
			 *		lastModifiedTime="2020-03-04T00:00:32.000Z"
			 *		color="#FFD869" />
			 */

			// nickname is display name whereas name is the folder name
			string keyName = sorting == SortDialog.Sortings.ByModified 
				? "lastModifiedTime" 
				: "nickname";

			var ns = root.GetNamespaceOfPrefix("one");

			var books =
				from p in root.Elements(ns + "Notebook")
				// keep only printable characters (don't sort on title emoticons!)
				let key = Regex.Replace(p.Attribute(keyName).Value, @"[^ -~]", "")
				orderby key
				select p;

			if (direction == SortDialog.Directions.Descending)
			{
				books = books.Reverse();
			}

			root.ReplaceNodes(books);

			return root;
		}
	}
}
