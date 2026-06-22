//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable IDE0042 // variable can be deconstructed

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	///
	/// </summary>
	/// <remarks>
	/// Disposables taken care of in OnClosed. Has two views, a curated List (named emojis,
	/// one image+name per row) and a Grid (every codepoint the installed Segoe UI Emoji
	/// font supports across the full Unicode range, images only, no names); both support
	/// multi-select, and selections made in either view are preserved across both.
	/// </remarks>

	internal partial class EmojiDialog : UI.MoreForm
	{
		private const int IconSize = 24;
		private const int GridIconSize = 48;

		private readonly Emojis emojis;
		private readonly List<Emoji> listSelections;
		private readonly List<Emoji> gridSelections;
		private readonly Dictionary<string, ListViewItem> categoryStartItems = new();
		private int quickWidth;
		private UnicodeEmojis unicodeEmojis;
		private UI.MoreLinkLabel activeCategoryLink;


		public EmojiDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.EmojiDialog_Text;

				Localize(new string[]
				{
					"introLabel",
					"okButton=word_OK",
					"cancelButton=word_Cancel",
					"listTab",
					"gridTab",
					"generalLink",
					"smileysLink",
					"peopleLink",
					"animalsLink",
					"foodLink",
					"travelLink",
					"activitiesLink",
					"objectsLink",
					"symbolsLink",
					"flagsLink"
				});
			}

			// must be defined before initializing the selected item
			listSelections = new();
			gridSelections = new();

			emojis = new Emojis();

			// the D3D11/Direct2D device must be created on this (UI) thread; touching the
			// singleton here, before the background scan below ever calls into the shared
			// IDWriteFactory from another thread, guarantees that's where it happens
			_ = ColorGlyphRenderer.Instance;

			// forces a tall enough row to comfortably fit the rendered icon; ListView has
			// no direct RowHeight property, so this is the standard WinForms workaround
			emojiBox.SmallImageList = new ImageList { ImageSize = new Size(1, IconSize + 2) };
			emojiBox.GetCellImage = GetListCellImage;

			var names = emojis.GetNames();
			emojiBox.BeginUpdate();
			for (var i = 0; i < names.Length; i++)
			{
				emojiBox.Items.Add(new ListViewItem(names[i]) { Tag = emojis[i] });
			}
			emojiBox.EndUpdate();

			emojiBox.SetColumnProportions(1f);

			if (emojiBox.Items.Count > 0)
			{
				emojiBox.Items[0].Selected = true;
				emojiBox.Items[0].Focused = true;
			}

			gridBox.LargeImageList = new ImageList { ImageSize = new Size(GridIconSize + 8, GridIconSize + 8) };
			gridBox.GetItemImage = GetGridItemImage;

			// generalLink.Active=true is set in the designer to match the grid's initial
			// scroll position (top, i.e. General); keep this field in sync with it
			activeCategoryLink = generalLink;

			// building the grid (cheap, but population of a few thousand items still costs
			// something) runs in the background so the dialog opens immediately on the
			// List tab; the Grid tab populates whenever it's ready, even if the user has
			// already switched to it
			_ = LoadGridAsync();
		}

		private void EmojiDialog_Shown(object sender, System.EventArgs e)
		{
			quickWidth = Width;
		}


		private async Task LoadGridAsync()
		{
			try
			{
				var loaded = await Task.Run(() => new UnicodeEmojis());

				if (IsDisposed || Disposing)
				{
					loaded.Dispose();
					return;
				}

				unicodeEmojis = loaded;

				// don't populate gridBox yet if the user hasn't switched to the Grid tab;
				// DoTabSelected populates it instead, once it's actually visible and sized
				// to its final dimensions - see the comment there for why that matters
				if (tabs.SelectedTab == gridTab)
				{
					PopulateGrid();
				}
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine("error loading emoji grid", exc);
			}
		}


		protected override void OnClosed(EventArgs e)
		{
			emojis.Dispose();
			unicodeEmojis?.Dispose();
		}

		private void OK(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}

		private void Cancel(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}


		/// <summary>
		/// Gets collection of user selected emojis, from either or both views
		/// </summary>
		/// <returns>A collection of IEmoji</returns>
		public IEnumerable<IEmoji> GetEmojis()
		{
			// pre-dispose images so caller doesn't have to
			emojis.Dispose();
			unicodeEmojis?.Dispose();

			foreach (var emoji in listSelections.Concat(gridSelections))
			{
				yield return emoji;
			}
		}


		// supplies MoreListView with the dynamically rendered, theme-aware icon for a row;
		// re-invoked on every paint so the icon's pre-filled background always matches
		// whatever MoreListView itself is about to fill behind it (selected or not)
		private Image GetListCellImage(ListViewItem item, int columnIndex)
		{
			if (DialogResult == DialogResult.OK || item.Tag is not Emoji emoji)
			{
				// double-click exit
				return null;
			}

			return RenderEmojiIcon(emoji, item.Selected, IconSize);
		}


		// supplies MoreIconListView with the dynamically rendered, theme-aware icon for a
		// grid cell; same rationale as GetListCellImage
		private Image GetGridItemImage(ListViewItem item)
		{
			if (DialogResult == DialogResult.OK || item.Tag is not Emoji emoji)
			{
				// double-click exit
				return null;
			}

			return RenderEmojiIcon(emoji, item.Selected, GridIconSize);
		}


		private Image RenderEmojiIcon(Emoji emoji, bool selected, int sizePx)
		{
			try
			{
				var manager = UI.ThemeManager.Instance;

				var background = manager.GetColor(selected ? "Highlight" : "ListView");
				var fallbackColor = emoji.Color is null
					? manager.GetColor(selected ? "HighlightText" : "ControlText")
					: ColorTranslator.FromHtml(emoji.Color);

				return emoji.GetImage(selected, sizePx, background, fallbackColor);
			}
			catch
			{
				// closing?
				return null;
			}
		}


		private void DoubleClickItem(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}


		// grows the dialog to comfortably fit the grid the first time it's shown; never
		// shrinks it back down when returning to the List tab
		private void DoTabSelected(object sender, EventArgs e)
		{
			if (tabs.SelectedTab != gridTab)
			{
				Left = (Left + (Width / 2)) - (quickWidth / 2);
				Width = quickWidth;
				return;
			}

			var width = Math.Max(Width, 1180);
			var height = Math.Max(Height, 900);

			if (width != Width || height != Height)
			{
				// recenter dialog horizontally
				Left = (Left + (Width / 2)) - (width / 2);

				Size = new Size(width, height);
			}

			// gridBox's native LargeIcon layout (hit-testing, scroll extents) gets baked
			// in based on the size and visibility it had when items were added. Adding
			// them earlier - while this tab wasn't yet selected and the dialog was still
			// at its small initial size - left clicks and scrolling silently out of sync
			// with what was actually drawn. Populating here instead, after the resize
			// above and only once this tab is actually showing, avoids that entirely.
			if (unicodeEmojis != null && gridBox.Items.Count == 0)
			{
				PopulateGrid();
			}
		}


		private void DoListSelectionChanged(object sender, EventArgs e)
		{
			UpdateSelections(emojiBox, listSelections);
		}


		private void DoGridSelectionChanged(object sender, EventArgs e)
		{
			UpdateSelections(gridBox, gridSelections);
		}


		// maintains a list of selections in the order in which they were selected... with
		// one caveat: doesn't know when a user drags across multiple items with the mouse
		// bottom-up
		private static void UpdateSelections(ListView box, List<Emoji> selections)
		{
			if (box.SelectedIndices.Count > 0)
			{
				var current = box.SelectedItems.Cast<ListViewItem>().Select(i => (Emoji)i.Tag).ToList();
				selections.AddRange(current.Except(selections));
				selections.RemoveRange(0, selections.Count - current.Count);
			}
		}


		// "General" covers any font-supported glyph that isn't part of one of Unicode's
		// named emoji groups (Emoji.Category is null for those); it's not a real key in
		// EmojiCategories.json, just the catch-all bucket this dialog presents
		private const string GeneralCategory = "General";

		// fixed display order for the category links in categoryPanel, and the order
		// their emoji are grouped into within gridBox
		private static readonly string[] CategoryOrder =
		{
			GeneralCategory,
			"Smileys & Emotion",
			"People & Body",
			"Animals & Nature",
			"Food & Drink",
			"Travel & Places",
			"Activities",
			"Objects",
			"Symbols",
			"Flags"
		};


		// scrolls gridBox so the clicked category's first item lands at the top of the
		// viewport; ListView.TopItem only supports Details/List view, so for a LargeIcon
		// view like gridBox, the scroll delta is computed directly from the item's
		// current bounds (already relative to the current scroll position) instead
		private void DoCategoryLinkClicked(object sender, EventArgs e)
		{
			if (sender is not UI.MoreLinkLabel link ||
				!categoryStartItems.TryGetValue((string)link.Tag, out var item))
			{
				return;
			}

			activeCategoryLink.Active = false;
			link.Active = true;
			activeCategoryLink = link;

			var dy = item.Bounds.Top;
			if (dy != 0)
			{
				Native.SendMessage(gridBox.Handle, Native.LVM_SCROLL, 0, dy);
			}
		}


		// groups every emoji in unicodeEmojis into gridBox by category - General first,
		// then the 9 Unicode groups in CategoryOrder - so each category link has one
		// contiguous section to scroll to; unicodeEmojis is already codepoint-sorted, so
		// a single bucketing pass preserves that order within each category. Runs exactly
		// once, only the first time the Grid tab is shown (see DoTabSelected and
		// LoadGridAsync), so there's never anything selected yet to preserve through
		// this build.
		private void PopulateGrid()
		{
			if (unicodeEmojis is null)
			{
				// background load not finished yet; the eventual LoadGridAsync completion
				// calls this again once it's ready
				return;
			}

			var buckets = CategoryOrder.ToDictionary(c => c, c => new List<Emoji>());
			for (var i = 0; i < unicodeEmojis.Count; i++)
			{
				var emoji = unicodeEmojis[i];
				buckets[emoji.Category ?? GeneralCategory].Add(emoji);
			}

			gridBox.BeginUpdate();
			foreach (var category in CategoryOrder)
			{
				var bucket = buckets[category];
				for (var i = 0; i < bucket.Count; i++)
				{
					var item = new ListViewItem(string.Empty) { Tag = bucket[i] };
					gridBox.Items.Add(item);

					if (i == 0)
					{
						categoryStartItems[category] = item;
					}
				}
			}
			gridBox.EndUpdate();
		}
	}
}
