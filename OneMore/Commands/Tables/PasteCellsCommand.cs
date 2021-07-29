﻿//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
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
			var clipboard = await SingleThreaded.Invoke(() =>
			{
				return Win.Clipboard.ContainsText(Win.TextDataFormat.Html)
					? Win.Clipboard.GetText(Win.TextDataFormat.Html)
					: null;
			});

			if (string.IsNullOrEmpty(clipboard))
			{
				UIHelper.ShowInfo("No content to paste");
				return;
			}

			using (one = new OneNote())
			{
				var template = await CreateTemplatePage(clipboard);
				logger.WriteLine(template.Root);

				/*
				// Find first selected cell as anchor point to locate table ; By filtering on
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
					UIHelper.ShowInfo(one.Window, Resx.InsertCellsCommand_NoSelection);
					return;
				}

				var table = new Table(anchor.FirstAncestor(ns + "Table"));
				var cells = table.GetSelectedCells(out var range).ToList();

				var shiftDown = true;
				var shiftCount = 1;

				using (var dialog = new InsertCellsDialog())
				{
					if (dialog.ShowDialog(owner) != DialogResult.OK)
					{
						return;
					}

					shiftDown = dialog.ShiftDown;
					shiftCount = dialog.ShiftCount;
				}

				if (shiftDown)
				{
					ShiftDown(table, cells, shiftCount);
				}
				else
				{
					ShiftRight(table, cells, shiftCount);
				}

				await one.Update(page);
				*/
			}
		}

		private async Task<Page> CreateTemplatePage(string content)
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
