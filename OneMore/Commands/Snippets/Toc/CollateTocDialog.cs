//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal partial class CollateTocDialog : MoreForm
	{
		private readonly MoreCheckListPanel notebookPanel;
		private readonly MoreAutoCompleteList palette;
		private readonly Dictionary<(bool Checked, string Color), Image> swatchGlyphCache = new();


		public CollateTocDialog(IEnumerable<XElement> notebooks)
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.CollateTocDialog_Title;

				Localize(new string[]
				{
					"introLabel=CollateTocDialog_introLabel",
					"hashtagLabel=CollateTocDialog_hashtagLabel",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			Disposed += (s, e) =>
			{
				foreach (var image in swatchGlyphCache.Values)
				{
					image.Dispose();
				}
			};

			notebookPanel = new MoreCheckListPanel { Dock = DockStyle.Fill };

			var list = notebookPanel.List;
			list.Columns.Add(string.Empty);
			list.SetColumnProportions(1f);
			list.SelectedBackColorKey = "LinkHighlight";
			list.SelectedForeColorKey = "ControlText";

			var baseGlyph = list.GetCellImage;
			list.GetCellImage = (item, column) =>
			{
				if (column != 0)
				{
					return null;
				}

				var (_, _, color) = ((string Id, string Name, string Color))item.Tag;
				var key = (item.Checked, color);
				if (!swatchGlyphCache.TryGetValue(key, out var glyph))
				{
					glyph = ComposeSwatchGlyph(baseGlyph?.Invoke(item, column), ColorHelper.FromHtml(color));
					swatchGlyphCache[key] = glyph;
				}

				return glyph;
			};

			list.BeginUpdate();
			foreach (var notebook in notebooks)
			{
				var id = notebook.Attribute("ID").Value;
				var name = notebook.Attribute("name").Value;
				var color = notebook.Attribute("color")?.Value ?? "automatic";

				list.Items.Add(new ListViewItem(name)
				{
					Tag = (id, name, color),
					Checked = true
				});
			}
			list.EndUpdate();

			notebooksHostPanel.Controls.Add(notebookPanel);

			palette = new MoreAutoCompleteList
			{
				FreeText = true,
				WordChars = new[] { '#' }
			};
			palette.SetAutoCompleteList(tagBox);

			// Tab (or any other real focus change) away from tagBox must close the popup;
			// this is independent of ToolStripDropDown's own key routing, which appears to
			// swallow Tab/Escape before this dialog's KeyDown handling ever sees them
			tagBox.Leave += (s, e) =>
			{
				if (palette.IsPopupVisible)
				{
					palette.HidePopup(s, e);
				}
			};

			DefaultControl = tagBox;
		}


		/// <summary>
		/// A second, independent interception point for Escape/Tab, tried because the
		/// KeyPreview + KeyDown path in DoKeyDown does not reliably see these keys while
		/// the autocomplete popup (a ToolStripDropDown) is showing.
		/// </summary>
		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (palette.IsPopupVisible)
			{
				if (keyData == Keys.Escape)
				{
					palette.HidePopup(this, EventArgs.Empty);
					return true;
				}

				if (keyData == Keys.Tab)
				{
					palette.HidePopup(this, EventArgs.Empty);
					// fall through so Tab still advances focus normally
				}
			}

			return base.ProcessDialogKey(keyData);
		}


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			using var provider = new HashtagProvider();
			var names = provider.ReadTagNames();
			var recent = provider.ReadLatestTagNames();
			palette.LoadCommands(names.ToArray(), recent.ToArray());
		}


		public List<string> SelectedNotebookIds =>
			notebookPanel.List.Items.Cast<ListViewItem>()
				.Where(i => i.Checked)
				.Select(i => ((string Id, string Name, string Color))i.Tag)
				.Select(t => t.Id)
				.ToList();


		public List<string> Hashtags =>
			tagBox.Text
				.Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
				.Distinct()
				.ToList();


		private void DoKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				if (palette.IsPopupVisible)
				{
					// first Escape dismisses the popup only
					palette.HidePopup(sender, e);
					e.Handled = true;
				}
				else
				{
					// second Escape (popup already closed) cancels the dialog
					e.Handled = true;
					DialogResult = DialogResult.Cancel;
					Close();
				}
			}
			else if (e.KeyCode == Keys.Tab && palette.IsPopupVisible)
			{
				// let Tab continue to advance focus normally, but don't leave the
				// popup floating over the OK/Cancel row once focus has moved on
				palette.HidePopup(sender, e);
			}
		}


		private void Accept(object sender, EventArgs e)
		{
			if (palette.IsPopupVisible)
			{
				palette.HidePopup(sender, e);
			}

			if (!SelectedNotebookIds.Any())
			{
				MoreMessageBox.ShowError(this, Resx.CollateTocDialog_noNotebooks);
				return;
			}

			if (!Hashtags.Any())
			{
				MoreMessageBox.ShowError(this, Resx.CollateTocDialog_noHashtags);
				return;
			}

			DialogResult = DialogResult.OK;
			Close();
		}


		private static Image ComposeSwatchGlyph(Image checkGlyph, Color swatchColor)
		{
			const int SwatchSize = 10;
			const int Gap = 6;

			var width = (checkGlyph?.Width ?? 0) + Gap + SwatchSize;
			var height = Math.Max(checkGlyph?.Height ?? 0, SwatchSize);

			var bitmap = new Bitmap(width, height);
			using var g = Graphics.FromImage(bitmap);
			g.SmoothingMode = SmoothingMode.AntiAlias;

			var x = 0;
			if (checkGlyph != null)
			{
				g.DrawImage(checkGlyph, 0, (height - checkGlyph.Height) / 2);
				x = checkGlyph.Width + Gap;
			}

			using var brush = new SolidBrush(swatchColor);
			g.FillRoundedRectangle(brush, new Rectangle(x, (height - SwatchSize) / 2, SwatchSize, SwatchSize), 2);

			return bitmap;
		}
	}
}
