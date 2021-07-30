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
				// Find first selected cell as anchor point to locate table; by filtering on
				// selected=all, we avoid including the parent table of a selected nested table.

				var anchor = page.Root.Descendants(ns + "Cell")
					// first dive down to find the selected T
					.Elements(ns + "OEChildren").Elements(ns + "OE")
					.Elements(ns + "T")
					.Where(e => e.Attribute("selected")?.Value == "all")
					// now move back up to the Cell
					.Select(e => e.Parent.Parent.Parent)
					.FirstOrDefault();

				if (anchor == null)
				{
					UIHelper.ShowInfo(one.Window, Resx.PasteCellsCommand_SelectCell);
					return;
				}

				var otable = anchor.Ancestors(ns + "Table").FirstOrDefault();
				if (otable == null)
				{
					UIHelper.ShowInfo(one.Window, "error finding <one:Table>; this shouldn't happen!");
					return;
				}

				var targetTable = new Table(otable);
				logger.WriteLine($"target {targetTable.RowCount} rows, {targetTable.ColumnCount} columns");

				// get the content to paste
				var sourceTable = await GetSourceTable();
				if (sourceTable == null)
				{
					UIHelper.ShowInfo(one.Window, Resx.PasteCellsCommand_NoContent);
					return;
				}

				logger.WriteLine($"source {sourceTable.RowCount} rows, {sourceTable.ColumnCount} columns");



				/*
				await one.Update(page);
				*/
			}
		}


		private async Task<Table> GetSourceTable()
		{
			var content = await SingleThreaded.Invoke(() =>
			{
				return Win.Clipboard.ContainsText(Win.TextDataFormat.Html)
					? Win.Clipboard.GetText(Win.TextDataFormat.Html)
					: null;
			});

			if (string.IsNullOrEmpty(content))
			{
				UIHelper.ShowInfo(one.Window, Resx.PasteCellsCommand_NoContent);
				return null;
			}

			if (!ValidateClipboardContent(content))
			{
				UIHelper.ShowInfo(one.Window, Resx.PasteCellsCommand_NoContent);
				return null;
			}

			var page = await CreateTemplatePage();

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

		/*
		private void ShiftDown(Table table, List<TableCell> cells, int shiftCount)
		{
			// RowNum and ColNum are 1-based so must shift them to be 0-based
			var minCol = cells.Min(c => c.ColNum) - 1;
			var maxCol = cells.Max(c => c.ColNum) - 1;
			var minRow = cells.Min(c => c.RowNum) - 1;
			var maxRow = cells.Max(c => c.RowNum) - 1;

			// if selected one row then move all visible content in desired direction
			// if rectangular then only move rectangle...

			if (minRow == maxRow)
			{
				// find last row that contains visible text
				var found = false;
				maxRow = table.Rows.Count() - 1;
				while (maxRow > minRow && !found)
				{
					for (var c = minCol; c <= maxCol; c++)
					{
						if (!string.IsNullOrEmpty(table[maxRow][c].GetText()))
						{
							found = true;
							break;
						}
					}

					if (!found)
					{
						maxRow--;
					}
				}
			}

			// add rows to make room for all visible text
			int neededRows = (maxRow + 1) + shiftCount;
			if (neededRows > table.Rows.Count())
			{
				for (var i = table.Rows.Count(); i < neededRows; i++) table.AddRow();
			}

			// iterate target coordinates
			for (var r = maxRow + shiftCount; r >= minRow + shiftCount; r--)
			{
				for (var c = minCol; c <= maxCol; c++)
				{
					var source = table[r - shiftCount][c];

					table[r][c].SetContent(source.GetContent());
					source.SetContent(string.Empty);
				}
			}
		}


		private void ShiftRight(Table table, List<TableCell> cells, int shiftCount)
		{
			// RowNum and ColNum are 1-based so must shift them to be 0-based
			var minCol = cells.Min(c => c.ColNum) - 1;
			var maxCol = cells.Max(c => c.ColNum) - 1;
			var minRow = cells.Min(c => c.RowNum) - 1;
			var maxRow = cells.Max(c => c.RowNum) - 1;

			var cols = table[0].Cells.Count();

			// if selected one col then move all visible content in desired direction
			// if rectangular then only move rectangle...

			if (minCol == maxCol)
			{
				// find last col that contains visible text
				var found = false;
				maxCol = cols - 1;
				while (maxCol > minCol && !found)
				{
					for (var r = minRow; r <= maxRow; r++)
					{
						if (!string.IsNullOrEmpty(table[r][maxCol].GetText()))
						{
							found = true;
							break;
						}
					}

					if (!found)
					{
						maxCol--;
					}
				}
			}

			// add cols to make room for all visible text
			int overflow = (maxCol + 1) + shiftCount - cols;
			if (overflow > 0)
			{
				for (var i = 0; i < overflow; i++) table.AddColumn(80f);
			}

			int offset = shiftCount;

			// iterate target coordinates
			for (var c = maxCol + offset; c >= minCol + offset; c--)
			{
				for (var r = minRow; r <= maxRow; r++)
				{
					var source = table[r][c - offset];

					table[r][c].SetContent(source.GetContent());
					source.SetContent(string.Empty);
				}
			}
		}
		*/
	}
}
