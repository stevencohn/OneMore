//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// A borderless, tooltip-like popup showing a 7-column gallery of table-theme thumbnails
	/// (built-in themes first, then user-defined ones) that the user can click to duplicate,
	/// replicating the ribbon's own Table Theme gallery. Dismissed by Esc or by clicking
	/// anywhere outside it, same as SimilarityPopup.
	/// </summary>
	internal partial class ThemeGalleryPopup : MoreForm
	{
		private const int Columns = 7;
		private const int TileSize = 80;
		private const int MaxGridHeight = TileSize * 6;

		private readonly Color borderColor;
		private readonly ToolTip tooltip;


		public ThemeGalleryPopup(IEnumerable<TableTheme> systemThemes, IEnumerable<TableTheme> userThemes)
		{
			InitializeComponent();

			// parented to components (created in InitializeComponent) so the designer's
			// standard Dispose(bool) - which disposes components when non-null - cleans
			// this up along with the rest of the form
			tooltip = new ToolTip(components);

			var manager = ThemeManager.Instance;
			borderColor = manager.GetColor("ButtonBorder");
			BackColor = manager.GetColor("Control");

			BuildContent(systemThemes.ToList(), userThemes.ToList());
		}


		/// <summary>
		/// Raised when the user clicks a tile; the popup closes itself immediately after.
		/// </summary>
		public event EventHandler<TableTheme> ThemeSelected;


		private void BuildContent(List<TableTheme> systemThemes, List<TableTheme> userThemes)
		{
			var manager = ThemeManager.Instance;
			var gridWidth = Columns * TileSize;
			var hostWidth = gridWidth + SystemInformation.VerticalScrollBarWidth;

			var root = new TableLayoutPanel
			{
				AutoSize = true,
				AutoSizeMode = AutoSizeMode.GrowAndShrink,
				ColumnCount = 1,
				Padding = new Padding(14, 12, 14, 12)
			};
			root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, hostWidth));

			AddRow(root, BuildHeaderLabel(Resx.EditTableThemesDialog_duplicateFrom, bold: true));

			// tiles are positioned manually (explicit Location, no FlowLayoutPanel) - this
			// mirrors BoxTypesPanel's own card layout, which hit the same problem: a
			// FlowLayoutPanel's AutoSize/AutoSizeMode.GrowAndShrink fights WrapContents and
			// can collapse everything back toward a single row instead of actually wrapping
			var host = new Panel
			{
				AutoScroll = true,
				BackColor = manager.GetColor("Control"),
				Width = hostWidth,
				Margin = new Padding(0, 8, 0, 0)
			};

			var y = 0;
			y = AddThemeSection(host, Resx.word_Builtin, systemThemes, y);
			y = AddThemeSection(host, Resx.EditTableThemesDialog_yourThemes, userThemes, y);

			host.Height = Math.Min(y, MaxGridHeight);
			host.MaximumSize = new Size(hostWidth, MaxGridHeight);
			host.AutoScrollMinSize = new Size(0, y);

			AddRow(root, host);

			root.Paint += (s, e) =>
			{
				using var pen = new Pen(borderColor);
				e.Graphics.DrawRectangle(pen, 0, 0, root.Width - 1, root.Height - 1);
			};

			Controls.Add(root);
		}


		/// <summary>
		/// Adds one labeled section (a header followed by its theme tiles, wrapped every
		/// Columns tiles) to host, starting at vertical offset y, and returns the y position
		/// immediately below everything just added.
		/// </summary>
		private int AddThemeSection(Panel host, string title, List<TableTheme> themes, int y)
		{
			if (themes.Count == 0)
			{
				return y;
			}

			if (y > 0)
			{
				y += 10;
			}

			var header = BuildHeaderLabel(title, bold: false);
			header.Location = new Point(2, y);
			host.Controls.Add(header);
			y += header.PreferredSize.Height + 4;

			var col = 0;
			foreach (var theme in themes)
			{
				var tile = new ThemeGalleryTile(theme);
				tooltip.SetToolTip(tile, theme.Name);
				tile.Location = new Point(col * TileSize, y);
				tile.Click += (s, e) =>
				{
					ThemeSelected?.Invoke(this, tile.Theme);
					Close();
				};
				host.Controls.Add(tile);

				col++;
				if (col >= Columns)
				{
					col = 0;
					y += TileSize;
				}
			}

			if (col != 0)
			{
				// a partial trailing row still needs its own height counted
				y += TileSize;
			}

			return y;
		}


		private static MoreLabel BuildHeaderLabel(string text, bool bold)
		{
			var label = new MoreLabel
			{
				AutoSize = true,
				Text = text
			};

			if (bold)
			{
				label.Font = new Font(label.Font, FontStyle.Bold);
			}
			else
			{
				label.ThemedFore = "GrayText";
			}

			return label;
		}


		private static void AddRow(TableLayoutPanel root, Control control)
		{
			root.RowCount++;
			root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			root.Controls.Add(control, 0, root.RowCount - 1);
		}


		// click-away dismissal: matches SimilarityPopup - this popup has no true owner-modal
		// relationship to key off of, so losing activation to anything else (including the
		// EditTableThemesDialog it was invoked from) is the signal to close
		protected override void OnDeactivate(EventArgs e)
		{
			base.OnDeactivate(e);
			if (!IsDisposed)
			{
				Close();
			}
		}


		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Escape)
			{
				Close();
				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}
	}
}
