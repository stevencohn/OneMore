//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Drawing;


	internal class TableThemePainter
	{
		private const int MaxRows = 5;
		private const int MaxCols = 5;

		private readonly Image image;
		private readonly Rectangle bounds;
		private readonly Color background;


		public TableThemePainter(Image image, Rectangle bounds, Color background)
		{
			this.image = image;
			this.bounds = bounds;
			this.background = background;
		}


		public void Paint(TableTheme theme)
		{
			using var g = Graphics.FromImage(image);
			DrawGrid(g);
			FillTable(g, theme);
			HighlightTable(g, theme);
		}

		private void DrawGrid(Graphics g)
		{
			g.Clear(background);
			g.DrawRectangle(Pens.Gray, bounds);

			var fudge = 5;

			var columnWidth = bounds.Width / MaxCols;
			for (int x = bounds.X + columnWidth; x < bounds.Right - fudge; x += columnWidth)
			{
				g.DrawLine(Pens.Gray, x, bounds.Top, x, bounds.Bottom);
			}

			var rowHeight = bounds.Height / MaxRows;
			for (int y = bounds.Y + rowHeight; y < bounds.Bottom - fudge; y += rowHeight)
			{
				g.DrawLine(Pens.Gray, bounds.Left, y, bounds.Right, y);
			}
		}


		private void FillTable(Graphics g, TableTheme theme)
		{
			if (theme.WholeTable == TableTheme.Rainbow)
			{
				if (theme.FirstColumn == TableTheme.Rainbow)
				{
					for (var r = 0; r < MaxRows; r++)
					{
						for (var c = 0; c < MaxCols; c++)
						{
							using var brush = new SolidBrush(ColorTranslator.FromHtml(
								TableTheme.LightColorNames[r % TableTheme.LightColorNames.Length]));
							FillCell(g, c, r, brush);
						}
					}

					return;
				}

				if (theme.HeaderRow == TableTheme.Rainbow)
				{
					for (var c = 0; c < MaxCols; c++)
					{
						for (var r = 0; r < MaxRows; r++)
						{
							using var brush = new SolidBrush(ColorTranslator.FromHtml(
								TableTheme.LightColorNames[c % TableTheme.LightColorNames.Length]));
							FillCell(g, c, r, brush);
						}
					}

					return;
				}
			}

			Color c0 = Color.Empty; // even
			Color c1 = Color.Empty; // odd
			bool rows = true;

			if (!theme.FirstRowStripe.IsEmpty && !theme.SecondRowStripe.IsEmpty)
			{
				c0 = theme.FirstRowStripe;
				c1 = theme.SecondRowStripe;
			}
			else if (!theme.FirstColumnStripe.IsEmpty && !theme.SecondColumnStripe.IsEmpty)
			{
				c0 = theme.FirstColumnStripe;
				c1 = theme.SecondColumnStripe;
				rows = false;
			}
			else if (!theme.WholeTable.IsEmpty)
			{
				c0 = c1 = theme.WholeTable;
			}

			if (!c0.IsEmpty)
			{
				using var b0 = new SolidBrush(c0);
				using var b1 = new SolidBrush(c1);
				for (var c = 0; c < MaxCols; c++)
				{
					for (var r = 0; r < MaxRows; r++)
					{
						FillCell(g, c, r, (rows ? r : c) % 2 == 0 ? b0 : b1);
					}
				}
			}
		}


		private void HighlightTable(Graphics g, TableTheme theme)
		{
			if (theme.FirstColumn == TableTheme.Rainbow)
			{
				for (int r = 0; r < MaxRows; r++)
				{
					using var b = new SolidBrush(ColorTranslator.FromHtml(
						TableTheme.MediumColorNames[r % TableTheme.MediumColorNames.Length]));
					FillCell(g, 0, r, b);
				}
			}
			else if (!theme.FirstColumn.IsEmpty)
			{
				using var b = new SolidBrush(theme.FirstColumn);
				for (int r = 0; r < MaxRows; r++)
				{
					FillCell(g, 0, r, b);
				}
			}

			if (!theme.LastColumn.IsEmpty)
			{
				using var b = new SolidBrush(theme.LastColumn);
				for (int r = 0; r < MaxRows; r++)
				{
					FillCell(g, MaxCols - 1, r, b);
				}
			}

			if (theme.HeaderRow == TableTheme.Rainbow)
			{
				for (int c = 0; c < MaxCols; c++)
				{
					using var b = new SolidBrush(
						ColorTranslator.FromHtml(TableTheme.MediumColorNames[c % 6]));
					FillCell(g, c, 0, b);
				}
			}
			else if (!theme.HeaderRow.IsEmpty)
			{
				using var b = new SolidBrush(theme.HeaderRow);
				for (int c = 0; c < MaxCols; c++)
				{
					FillCell(g, c, 0, b);
				}
			}

			if (!theme.TotalRow.IsEmpty)
			{
				using var b = new SolidBrush(theme.TotalRow);
				for (int c = 0; c < MaxCols; c++)
				{
					FillCell(g, c, MaxRows - 1, b);
				}
			}

			if (!theme.HeaderFirstCell.IsEmpty)
			{
				using var b = new SolidBrush(theme.HeaderFirstCell);
				FillCell(g, 0, 0, b);
			}

			if (!theme.HeaderLastCell.IsEmpty)
			{
				using var b = new SolidBrush(theme.HeaderLastCell);
				FillCell(g, MaxCols - 1, 0, b);
			}

			if (!theme.TotalFirstCell.IsEmpty)
			{
				using var b = new SolidBrush(theme.TotalFirstCell);
				FillCell(g, 0, MaxRows - 1, b);
			}

			if (!theme.TotalLastCell.IsEmpty)
			{
				using var b = new SolidBrush(theme.TotalLastCell);
				FillCell(g, MaxCols - 1, MaxRows - 1, b);
			}
		}


		private void FillCell(Graphics g, int c, int r, Brush brush)
		{
			var columnWidth = bounds.Width / MaxCols;
			var rowHeight = bounds.Height / MaxRows;

			var x = bounds.Left + (c * columnWidth) + 1;
			var y = bounds.Top + (r * rowHeight) + 1;

			g.FillRectangle(brush, x, y, columnWidth - 1, rowHeight - 1);

			x += (int)(columnWidth * 0.30);
			y += (rowHeight - 1) / 2;
			g.DrawLine(Pens.Black, x, y, x + (int)(columnWidth * 0.40), y);
		}
	}
}
