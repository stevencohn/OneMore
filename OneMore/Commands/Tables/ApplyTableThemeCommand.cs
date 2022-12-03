//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Applies a table theme to the current table including cell shading and font styles.
	/// </summary>
	internal class ApplyTableThemeCommand : Command
	{
		public ApplyTableThemeCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var selectedIndex = (int)args[0];

			using var one = new OneNote(out var page, out var ns);
			if (!page.ConfirmBodyContext())
			{
				UIHelper.ShowInfo(one.Window, "Set cursor in a table"); // Resx.ApplyTableTheme_SelectTable);
				return;
			}

			// Find first selected cell as anchor point to locate table to stylize; by filtering
			// on selected=all, we avoid including the parent table of a selected nested table

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
				UIHelper.ShowInfo(one.Window, "Set cursor in a table"); // Resx.ApplyTableTheme_SelectTable);
				return;
			}

			TableTheme theme;
			var clear = selectedIndex == int.MaxValue;
			if (clear)
			{
				// this will clear formatting in the table
				theme = new TableTheme();
			}
			else
			{
				var provider = new TableThemeProvider();
				if (selectedIndex < 0 || selectedIndex >= provider.Count)
				{
					UIHelper.ShowInfo(one.Window, "Invalid theme index"); // Resx.ApplyTableTheme_SelectTable);
					return;
				}

				theme = provider.GetTheme(selectedIndex);
			}

			var table = new Table(anchor.FirstAncestor(ns + "Table"));

			FillTable(table, theme);
			HighlightTable(table, theme);

			if (clear)
			{
				ClearFonts(table);
			}
			else
			{
				ApplyFonts(table, theme);
			}

			await one.Update(page);
		}


		private void FillTable(Table table, TableTheme theme)
		{
			if (theme.WholeTable == TableTheme.Rainbow)
			{
				if (theme.FirstColumn == TableTheme.Rainbow)
				{
					for (var r = 0; r < table.RowCount; r++)
					{
						for (var c = 0; c < table.ColumnCount; c++)
						{
							table[r][c].ShadingColor =
								TableTheme.LightColorNames[r % TableTheme.LightColorNames.Length];
						}
					}

					return;
				}

				if (theme.HeaderRow == TableTheme.Rainbow)
				{
					for (var c = 0; c < table.ColumnCount; c++)
					{
						for (var r = 0; r < table.RowCount; r++)
						{
							table[r][c].ShadingColor =
								TableTheme.LightColorNames[c % TableTheme.LightColorNames.Length];
						}
					}

					return;
				}
			}

			string c0; // even
			string c1; // odd
			bool rows = true;

			if (!theme.FirstRowStripe.IsEmpty && !theme.SecondRowStripe.IsEmpty)
			{
				c0 = theme.FirstRowStripe.ToRGBHtml();
				c1 = theme.SecondRowStripe.ToRGBHtml();
			}
			else if (!theme.FirstColumnStripe.IsEmpty && !theme.SecondColumnStripe.IsEmpty)
			{
				c0 = theme.FirstColumnStripe.ToRGBHtml();
				c1 = theme.SecondColumnStripe.ToRGBHtml();
				rows = false;
			}
			else if (!theme.WholeTable.IsEmpty)
			{
				c0 = c1 = theme.WholeTable.ToRGBHtml();
			}
			else
			{
				c0 = c1 = StyleBase.Automatic;
			}

			if (!string.IsNullOrEmpty(c0))
			{
				for (var c = 0; c < table.ColumnCount; c++)
				{
					for (var r = 0; r < table.RowCount; r++)
					{
						table[r][c].ShadingColor = (rows ? r : c) % 2 == 0 ? c0 : c1;
					}
				}
			}
		}


		private void HighlightTable(Table table, TableTheme theme)
		{
			if (theme.FirstColumn == TableTheme.Rainbow)
			{
				for (int r = 0; r < table.RowCount; r++)
				{
					table[r][0].ShadingColor =
						TableTheme.MediumColorNames[r % TableTheme.MediumColorNames.Length];
				}
			}
			else if (!theme.FirstColumn.IsEmpty)
			{
				var color = theme.FirstColumn.ToRGBHtml();
				for (int r = 0; r < table.RowCount; r++)
				{
					table[r][0].ShadingColor = color;
				}
			}

			if (!theme.LastColumn.IsEmpty)
			{
				var color = theme.LastColumn.ToRGBHtml();
				for (int r = 0; r < table.RowCount; r++)
				{
					table[r][table.ColumnCount - 1].ShadingColor = color;
				}
			}

			if (theme.HeaderRow == TableTheme.Rainbow)
			{
				for (int c = 0; c < table.ColumnCount; c++)
				{
					table[0][c].ShadingColor =
						TableTheme.MediumColorNames[c % TableTheme.MediumColorNames.Length];
				}
			}
			else if (!theme.HeaderRow.IsEmpty)
			{
				var color = theme.HeaderRow.ToRGBHtml();
				for (int c = 0; c < table.ColumnCount; c++)
				{
					table[0][c].ShadingColor = color;
				}
			}

			if (!theme.TotalRow.IsEmpty)
			{
				var color = theme.TotalRow.ToRGBHtml();
				for (int c = 0; c < table.ColumnCount; c++)
				{
					table[table.RowCount - 1][c].ShadingColor = color;
				}
			}

			if (!theme.HeaderFirstCell.IsEmpty)
			{
				table[0][0].ShadingColor = theme.HeaderFirstCell.ToRGBHtml();
			}

			if (!theme.HeaderLastCell.IsEmpty)
			{
				table[0][table.ColumnCount - 1].ShadingColor = theme.HeaderLastCell.ToRGBHtml();
			}

			if (!theme.TotalFirstCell.IsEmpty)
			{
				table[table.RowCount - 1][0].ShadingColor = theme.TotalFirstCell.ToRGBHtml();
			}

			if (!theme.TotalLastCell.IsEmpty)
			{
				table[table.RowCount - 1][table.ColumnCount - 1].ShadingColor =
					theme.TotalLastCell.ToRGBHtml();
			}
		}


		private void ClearFonts(Table table)
		{
			var stylizer = new Stylizer(new Style());
			for (int r = 0; r < table.RowCount; r++)
			{
				for (int c = 0; c < table.ColumnCount; c++)
				{
					stylizer.Clear(table[r][c].Root, Stylizer.Clearing.All);
				}
			}
		}


		private void ApplyFonts(Table table, TableTheme theme)
		{
			var minrow = theme.HeaderFont == null ? 0 : 1;
			var maxrow = table.RowCount - (theme.TotalFont == null ? 0 : 1);
			var mincol = theme.FirstColumnFont == null ? 0 : 1;
			var maxcol = table.ColumnCount - (theme.LastColumnFont == null ? 0 : 1);

			if (theme.HeaderFont != null)
			{
				var stylizer = MakeStylizer(theme.HeaderFont);
				for (int c = mincol; c < maxcol; c++)
				{
					stylizer.ApplyStyle(table[0][c].Root);
				}
			}

			if (theme.TotalFont != null)
			{
				var stylizer = MakeStylizer(theme.TotalFont);
				for (int c = mincol; c < maxcol; c++)
				{
					stylizer.ApplyStyle(table[table.RowCount - 1][c].Root);
				}
			}

			if (theme.FirstColumnFont != null)
			{
				var stylizer = MakeStylizer(theme.FirstColumnFont);
				for (int r = minrow; r < maxrow; r++)
				{
					stylizer.ApplyStyle(table[r][0].Root);
				}
			}

			if (theme.LastColumnFont != null)
			{
				var stylizer = MakeStylizer(theme.LastColumnFont);
				for (int r = minrow; r < maxrow; r++)
				{
					stylizer.ApplyStyle(table[r][table.ColumnCount - 1].Root);
				}
			}

			if (theme.DefaultFont != null)
			{
				var stylizer = MakeStylizer(theme.DefaultFont);
				for (int r = minrow; r < maxrow; r++)
				{
					for (int c = mincol; c < maxcol; c++)
					{
						stylizer.ApplyStyle(table[r][c].Root);
					}
				}
			}
		}


		private Stylizer MakeStylizer(TableTheme.ColorFont font)
		{
			return new Stylizer(new Style
			{
				FontFamily = font.Font.FontFamily.Name,
				FontSize = font.Font.Size.ToString("0.#"),
				IsBold = font.Font.Bold,
				IsItalic = font.Font.Italic,
				IsUnderline = font.Font.Underline,
				Color = font.Foreground.IsEmpty ? StyleBase.Automatic : font.Foreground.ToRGBHtml()
			});
		}
	}
}
