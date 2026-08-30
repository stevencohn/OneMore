//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Workspaces
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Linq;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Lists every open OneNote window - its full page path, on-screen location, and
	/// front-to-back Z-order - and lets the user pick one to activate. Windows sharing the
	/// same page are colored with a matching swatch, cycling through a small fixed palette,
	/// so duplicates are easy to spot at a glance.
	/// </summary>
	internal partial class ShowWindowsDialog : MoreForm
	{
		/// <summary>
		/// A single row: the window it represents, its Z-order rank (1 = frontmost), and
		/// the swatch color it shares with other windows on the same page, if any.
		/// </summary>
		private sealed class Row
		{
			public WindowInfo Window;
			public int ZRank;
			public Color? Swatch;
		}


		// fixed categorical palette rather than ThemeManager colors: needs to stay visually
		// distinct across 5-6 cycling entries and legible against both the light and dark
		// ListView background, independent of whichever theme colors happen to be active
		private static readonly Color[] SwatchPalette =
		{
			Color.FromArgb(0xE6, 0x19, 0x4B), // red
			Color.FromArgb(0x3C, 0xB4, 0x4B), // green
			Color.FromArgb(0x43, 0x63, 0xD8), // blue
			Color.FromArgb(0xF5, 0x82, 0x31), // orange
			Color.FromArgb(0x91, 0x1E, 0xB4), // purple
			Color.FromArgb(0xF0, 0x32, 0xE6), // magenta
		};

		private const int SwatchSize = 12;

		private readonly Dictionary<Color, Bitmap> swatchCache = new();
		private List<Row> allRows;


		public ShowWindowsDialog()
		{
			InitializeComponent();
			RememberSize = true;

			if (NeedsLocalizing())
			{
				Text = Resx.ShowWindowsDialog_Text;

				Localize(new string[]
				{
					"goButton=word_Go",
					"cancelButton=word_Cancel",
					"searchLabel=word_Search"
				});

				pageColumn.Text = Resx.word_Page;
				locationColumn.Text = Resx.ShowWindowsDialog_locationColumn_HeaderText;
				zColumn.Text = Resx.ShowWindowsDialog_zColumn_HeaderText;
			}

			listView.SetColumnProportions(0.55f, 0.30f, 0.15f);
			listView.GetCellImage = GetSwatchImage;
			DefaultControl = searchBox;

			FormClosed += (s, e) =>
			{
				foreach (var bitmap in swatchCache.Values)
				{
					bitmap.Dispose();
				}
			};
		}


		private void FocusOnActivated(object sender, EventArgs e)
		{
			searchBox.Focus();
		}


		/// <summary>
		/// The Win32 handle of the window the user chose, or IntPtr.Zero if the dialog was
		/// cancelled. Only meaningful when ShowDialog returns DialogResult.OK.
		/// </summary>
		public IntPtr SelectedHandle { get; private set; }


		/// <summary>
		/// Builds the row set, grouping duplicate-page windows by a shared swatch color, and
		/// renders the unfiltered list. Must be called before ShowDialog.
		/// </summary>
		/// <param name="windows">Every currently open OneNote window</param>
		public void Populate(List<WindowInfo> windows)
		{
			var ranked = SaveLayoutCommand.OrderByZOrder(windows)
				.Select((w, i) => new Row { Window = w, ZRank = i + 1 })
				.ToList();

			var counts = ranked
				.GroupBy(r => r.Window.CurrentPage)
				.ToDictionary(g => g.Key, g => g.Count());

			var colorByPath = new Dictionary<string, Color>();
			var next = 0;
			foreach (var path in ranked.Select(r => r.Window.CurrentPage).Distinct())
			{
				if (counts[path] > 1)
				{
					colorByPath[path] = SwatchPalette[next++ % SwatchPalette.Length];
				}
			}

			foreach (var row in ranked)
			{
				if (colorByPath.TryGetValue(row.Window.CurrentPage, out var color))
				{
					row.Swatch = color;
				}
			}

			allRows = ranked;
			Filter(string.Empty);
		}


		/// <summary>
		/// Rebuilds the list from the full row set, showing only windows whose page path
		/// contains the given filter text (or every window when the filter is empty), and
		/// preserves the selected window across the rebuild where possible.
		/// </summary>
		private void Filter(string filterText)
		{
			var text = filterText.Trim();
			var filtering = text.Length > 0;

			var selectedHandle = listView.SelectedItems.Count > 0 &&
				listView.SelectedItems[0].Tag is Row selected
					? selected.Window.WindowHandle
					: null;

			var rows = filtering
				? allRows.Where(r => r.Window.CurrentPage.ContainsICIC(text))
				: allRows.AsEnumerable();

			listView.BeginUpdate();
			listView.Items.Clear();

			foreach (var row in rows
				.OrderBy(r => r.Window.CurrentPage, StringComparer.CurrentCultureIgnoreCase)
				.ThenBy(r => r.ZRank))
			{
				var item = new ListViewItem(row.Window.CurrentPage) { Tag = row };
				item.SubItems.Add(FormatLocation(row.Window));
				item.SubItems.Add(row.ZRank.ToString());
				listView.Items.Add(item);
			}

			listView.EndUpdate();

			var index = selectedHandle is null ? -1 : IndexOfHandle(selectedHandle);
			if (index < 0 && listView.Items.Count > 0)
			{
				index = 0;
			}

			if (index >= 0)
			{
				listView.Items[index].Selected = true;
				listView.Items[index].EnsureVisible();
			}
		}


		private int IndexOfHandle(string handle)
		{
			for (var i = 0; i < listView.Items.Count; i++)
			{
				if (listView.Items[i].Tag is Row row && row.Window.WindowHandle == handle)
				{
					return i;
				}
			}

			return -1;
		}


		private void MoveSelection(int delta)
		{
			var count = listView.Items.Count;
			if (count == 0)
			{
				return;
			}

			var index = listView.SelectedItems.Count > 0
				? listView.SelectedItems[0].Index + delta
				: 0;

			index = Math.Max(0, Math.Min(count - 1, index));

			listView.Items[index].Selected = true;
			listView.Items[index].EnsureVisible();
		}


		private void FilterRowOnKeyUp(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Down:
					MoveSelection(1);
					e.Handled = true;
					return;

				case Keys.Up:
					MoveSelection(-1);
					e.Handled = true;
					return;
			}

			if (char.IsControl((char)e.KeyValue) &&
				e.KeyCode != Keys.Delete && e.KeyCode != Keys.Back)
			{
				e.Handled = true;
				return;
			}

			Filter(searchBox.Text);
			e.Handled = true;
		}


		private static string FormatLocation(WindowInfo window)
		{
			var device = SaveLayoutCommand.GetDeviceName(window.WindowHandle);
			var label = device?.Replace(@"\\.\DISPLAY", "Display") ?? string.Empty;
			return $"{label} {window.Bounds.Left},{window.Bounds.Top}".Trim();
		}


		private Image GetSwatchImage(ListViewItem item, int columnIndex)
		{
			if (columnIndex != 0 || item.Tag is not Row { Swatch: { } color })
			{
				return null;
			}

			if (!swatchCache.TryGetValue(color, out var bitmap))
			{
				bitmap = BuildSwatch(color);
				swatchCache[color] = bitmap;
			}

			return bitmap;
		}


		private static Bitmap BuildSwatch(Color color)
		{
			var bitmap = new Bitmap(SwatchSize, SwatchSize);
			using var g = Graphics.FromImage(bitmap);
			g.SmoothingMode = SmoothingMode.AntiAlias;

			using var brush = new SolidBrush(color);
			g.FillRoundedRectangle(brush, new Rectangle(0, 0, SwatchSize - 1, SwatchSize - 1), 3);

			return bitmap;
		}


		private void ChooseByClick(object sender, EventArgs e)
		{
			Choose();
		}


		private void ChooseByDoubleClick(object sender, EventArgs e)
		{
			Choose();
		}


		private void ChooseByKeyboard(object sender, KeyEventArgs e)
		{
			if (e.KeyCode != Keys.Enter)
			{
				return;
			}

			e.Handled = true;
			Choose();
		}


		private void Choose()
		{
			if (listView.SelectedItems.Count == 0 ||
				listView.SelectedItems[0].Tag is not Row row)
			{
				return;
			}

			SelectedHandle = new IntPtr(Convert.ToInt64(row.Window.WindowHandle, 16));
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
