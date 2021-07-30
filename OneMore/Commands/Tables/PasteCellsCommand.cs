//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Hap = HtmlAgilityPack;
	using Resx = River.OneMoreAddIn.Properties.Resources;
	using Win = System.Windows;


	/// <summary>
	/// Paste the copied cells into the target table, overlaying cells rather than inserting
	/// a nested table. The target table is expanded with extra rows or columns as needed.
	/// </summary>
	internal class PasteCellsCommand : Command
	{
		private OneNote one;


		public PasteCellsCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (one = new OneNote(out var page, out var ns))
			{
				// make sure cursor is positioned in a target table...

				var targetRoot = GetTargetTableRoot(page, ns);
				if (targetRoot == null)
				{
					return;
				}

				var table = new Table(targetRoot);

				// anchor is the upper-left cell into which pasting will begin
				var anchor = table.GetSelectedCells(out _).FirstOrDefault();

				if (anchor == null)
				{
					logger.WriteLine("could not find anchor cell");
					UIHelper.ShowInfo(one.Window, "could not find anchor cell; this shouldn't happen!");
					return;
				}

				// get the content to paste...

				var spage = await GetSourcePage();
				if (spage == null)
				{
					UIHelper.ShowInfo(one.Window, Resx.PasteCellsCommand_NoContent);
					return;
				}

				var source = GetSourceTable(spage);
				if (source == null)
				{
					UIHelper.ShowInfo(one.Window, Resx.PasteCellsCommand_NoContent);
					return;
				}

				var mapping = page.MergeQuickStyles(spage);

				EnsureRoom(table, anchor, source);

				// paste...

				for (int r = 0, row = anchor.RowNum - 1; r < source.RowCount; r++, row++)
				{
					for (int c = 0, col = anchor.ColNum - 1; c < source.ColumnCount; c++, col++)
					{
						table[row][col].SetContent(source[r][c].GetContent());
						page.ApplyStyleMapping(mapping, table[row][col].Root);

						var shading = source[r][c].ShadingColor;
						if (shading != null)
						{
							table[row][col].ShadingColor = shading;
						}
					}
				}

				await one.Update(page);
			}
		}


		private XElement GetTargetTableRoot(Page page, XNamespace ns)
		{
			// Find first selected cell as anchor point to locate table; by filtering on
			// selected=all, we avoid including the parent table of a selected nested table.

			var cell = page.Root.Descendants(ns + "Cell")
				// first dive down to find the selected T
				.Elements(ns + "OEChildren").Elements(ns + "OE")
				.Elements(ns + "T")
				.Where(e => e.Attribute("selected")?.Value == "all")
				// now move back up to the Cell
				.Select(e => e.Parent.Parent.Parent)
				.FirstOrDefault();

			if (cell == null)
			{
				UIHelper.ShowInfo(one.Window, Resx.PasteCellsCommand_SelectCell);
				return null;
			}

			var root = cell.Ancestors(ns + "Table").FirstOrDefault();
			if (root == null)
			{
				logger.WriteLine("error finding <one:Table>");
				UIHelper.ShowInfo(one.Window, "error finding <one:Table>; this shouldn't happen!");
				return null;
			}

			return root;
		}


		private async Task<Page> GetSourcePage()
		{
			// the Clipboard will contain HTML of the copied cells wrapped in a <table>
			var content = await SingleThreaded.Invoke(() =>
			{
				return Win.Clipboard.ContainsText(Win.TextDataFormat.Html)
					? Win.Clipboard.GetText(Win.TextDataFormat.Html)
					: null;
			});

			if (string.IsNullOrEmpty(content))
			{
				return null;
			}

			if (!ValidateClipboardContent(content))
			{
				return null;
			}

			return await CreateTemplatePage();
		}


		private Table GetSourceTable(Page page)
		{
			var element = page.Root.Descendants(page.Namespace + "Table").FirstOrDefault();
			if (element == null)
			{
				return null;
			}

			return new Table(element);
		}


		private bool ValidateClipboardContent(string content)
		{
			var index = content.IndexOf("<html");
			if (index < 0)
			{
				return false;
			}

			try
			{
				var html = content.Substring(index);
				var doc = new Hap.HtmlDocument();
				doc.LoadHtml(html);

				var table = doc.DocumentNode.SelectSingleNode("//body//table");
				if (table == null)
				{
					logger.WriteLine("no <table> found in content");
					return false;
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error parsing clipboard content", exc);
				return false;
			}

			return true;
		}


		private async Task<Page> CreateTemplatePage()
		{
			// use a temporary scratch page to convert the HTML into OneNote XML by pasting the
			// HTML contents of the clipboard and let OneNote do its magic...

			var currentPageId = one.CurrentPageId;

			one.CreatePage(one.CurrentSectionId, out var pageId);
			await one.NavigateTo(pageId);

			// since the Hotkey message loop is watching all input, explicitly setting
			// focus on the OneNote main window provides a direct path for SendKeys
			Native.SetForegroundWindow(one.WindowHandle);
			SendKeys.SendWait("^(v)");

			var page = one.GetPage(pageId);
			one.DeleteHierarchy(pageId);

			await one.NavigateTo(currentPageId);

			return page;
		}


		private void EnsureRoom(Table table, TableCell anchor, Table source)
		{
			var room = table.RowCount - anchor.RowNum + 1;
			while (room < source.RowCount)
			{
				table.AddRow();
				room++;
			}

			room = table.ColumnCount - anchor.ColNum + 1;
			while (room < source.ColumnCount)
			{
				table.AddColumn(40);
				room++;
			}
		}
	}
}
