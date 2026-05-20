//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;


	/// <summary>
	/// Virtualized scrollable panel that owner-paints search result cards — one card per page.
	/// Each card shows the section path / page title and its matching paragraph snippets.
	/// Painting cost is proportional to the number of visible cards, not total card count,
	/// so the control scales to thousands of results without allocating a child Control per card.
	/// </summary>
	internal class SearchResultsCardView : MoreUserControl
	{
		// visual constants
		private const int CardMarginH = 8;   // horizontal gap between control edge and card
		private const int SwatchWidth = 5;   // section-color stripe on the left edge of card
		private const int CardPadX = 8;      // horizontal padding between swatch and text
		private const int CardPadV = 5;      // vertical padding inside card (top and bottom)
		private const int TitleRowH = 26;    // height of the page-title row
		private const int HitRowH = 22;      // height of each snippet row
		private const int HitIndent = 20;     // additional indent for snippet rows
		private const int CardGap = 6;       // gap between consecutive cards
		private const int TopPad = 6;        // gap above first card
		private const int CornerRadius = 6;
		private const int MaxHits = 10_000;
		private const int CheckBoxSize = 16; // width/height of the checkbox glyph in the title row
		private const int CheckBoxGap = 4;   // gap between checkbox right edge and title text

		private readonly List<CardModel> cards = new();
		private bool layoutDirty;
		private int totalHits;
		private bool hitCap;

		// selection
		private int selectedCard = -1;
		private int selectedHit = -1;

		// hover
		private int hoverCard = -1;
		private int hoverHit = -1;

		// themed resources
		private SolidBrush cardBackBrush;
		private Pen borderPen;
		private SolidBrush selectionBackBrush;
		private Color titleFore;
		private Color hitFore;
		private Color selectionFore;
		private Color hoverFore;
		private Font titleFont;
		private Font hitFont;
		private Pen checkboxPen;
		private SolidBrush checkboxFillBrush;


		public SearchResultsCardView()
		{
			SetStyle(
				ControlStyles.OptimizedDoubleBuffer |
				ControlStyles.UserPaint |
				ControlStyles.AllPaintingInWmPaint, true);

			AutoScroll = true;
		}


		public event EventHandler<NavigateCardEventArgs> CardActivated;
		public event EventHandler<NavigateHitEventArgs> HitActivated;
		public event EventHandler CheckedChanged;


		public bool HasHits => cards.Any(c => c.Hits.Count > 0);


		public int CheckedCount => cards.Count(c => c.IsChecked && c.PageId != null);


		public IReadOnlyList<string> GetCheckedPageIds() =>
			cards.Where(c => c.IsChecked && c.PageId != null).Select(c => c.PageId).ToList();


		public void CheckAll()
		{
			foreach (var card in cards.Where(c => c.Title != null && c.PageId != null))
			{
				card.IsChecked = true;
			}
			Invalidate();
			CheckedChanged?.Invoke(this, EventArgs.Empty);
		}


		public void ClearChecked()
		{
			foreach (var card in cards.Where(c => c.IsChecked))
			{
				card.IsChecked = false;
			}
			Invalidate();
			CheckedChanged?.Invoke(this, EventArgs.Empty);
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Theming

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);  // calls ThemeManager.InitializeTheme → OnThemeChange in dark mode
			if (cardBackBrush == null) OnThemeChange();  // ensure light mode resources too
		}


		public override void OnThemeChange()
		{
			base.OnThemeChange();
			DisposeThemed();

			cardBackBrush    = new SolidBrush(manager.GetColor("ControlLightLight"));
			borderPen        = new Pen(manager.GetColor("ControlDark"));
			selectionBackBrush = new SolidBrush(manager.GetColor("Highlight"));
			titleFore        = manager.GetColor("ControlText");
			hitFore          = manager.GetColor("HintText");
			selectionFore    = manager.GetColor("HighlightText");
			hoverFore        = manager.GetColor("HoverColor");
			BackColor         = manager.GetColor("AppWorkspace");
			checkboxPen       = new Pen(manager.GetColor("Highlight"));
			checkboxFillBrush = new SolidBrush(manager.GetColor("Highlight"));

			titleFont?.Dispose();
			hitFont?.Dispose();
			titleFont = new Font("Segoe UI", 8.5f, FontStyle.Bold, GraphicsUnit.Point);
			hitFont   = new Font("Segoe UI", 8.5f, FontStyle.Regular, GraphicsUnit.Point);

			Invalidate();
		}


		private void DisposeThemed()
		{
			cardBackBrush?.Dispose();    cardBackBrush = null;
			borderPen?.Dispose();        borderPen = null;
			selectionBackBrush?.Dispose();  selectionBackBrush = null;
			checkboxPen?.Dispose();         checkboxPen = null;
			checkboxFillBrush?.Dispose();   checkboxFillBrush = null;
		}


		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				DisposeThemed();
				titleFont?.Dispose();
				hitFont?.Dispose();
			}
			base.Dispose(disposing);
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Data model

		public void Clear()
		{
			cards.Clear();
			totalHits = 0;
			hitCap = false;
			layoutDirty = true;
			selectedCard = selectedHit = -1;
			hoverCard = hoverHit = -1;
			AutoScrollMinSize = Size.Empty;
			Invalidate();
		}


		public void AppendCard(CardModel card)
		{
			if (hitCap) return;

			cards.Add(card);
			totalHits += card.Hits.Count;

			if (totalHits >= MaxHits)
			{
				hitCap = true;
				cards.Add(new CardModel
				{
					Title = $"Showing first {MaxHits:N0} hits — refine your search to see more.",
					SectionColor = Color.Empty
				});
			}

			layoutDirty = true;
			EnsureLayout();

			// Invalidate from the new card down so it appears immediately.
			// This is the paint-priming trick that prevents the search loop's
			// SynchronizationContext continuations from starving WM_PAINT.
			var newCard = hitCap ? cards[cards.Count - 2] : cards[cards.Count - 1];
			var screenY = newCard.Y + AutoScrollPosition.Y;
			if (screenY < ClientSize.Height)
			{
				var h = ClientSize.Height - Math.Max(0, screenY);
				Invalidate(new Rectangle(0, Math.Max(0, screenY), ClientSize.Width, h));
			}
			Update();
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Layout

		private void EnsureLayout()
		{
			if (!layoutDirty) return;

			int y = TopPad;
			foreach (var card in cards)
			{
				card.Y = y;
				card.Height = CardPadV
					+ (card.Title != null ? TitleRowH : 0)
					+ card.Hits.Count * HitRowH
					+ CardPadV;

				// minimum height so an empty/anonymous card is still visible
				if (card.Height < CardPadV * 2 + HitRowH)
					card.Height = CardPadV * 2 + HitRowH;

				y += card.Height + CardGap;
			}

			AutoScrollMinSize = new Size(0, Math.Max(0, y - CardGap));
			layoutDirty = false;
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Painting

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			if (cards.Count == 0) return;

			EnsureLayout();

			// TextRenderer.DrawText uses GDI and ignores Graphics.TranslateTransform, so we
			// must compute screen coordinates directly rather than relying on the world transform.
			var scrollY = -AutoScrollPosition.Y;  // pixels scrolled down (positive)
			e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

			// convert clip rect to virtual coordinates for the binary search
			var clipTop    = e.ClipRectangle.Top    + scrollY;
			var clipBottom = e.ClipRectangle.Bottom + scrollY;

			var start = BinarySearchFirst(clipTop);
			for (int i = start; i < cards.Count && cards[i].Y < clipBottom; i++)
			{
				DrawCard(e.Graphics, cards[i], i, scrollY);
			}
		}


		private void DrawCard(Graphics g, CardModel card, int cardIndex, int scrollY)
		{
			var cx        = CardMarginH;
			var cw        = ClientSize.Width - CardMarginH * 2;
			var screenTop = card.Y - scrollY;  // virtual → screen coordinate
			var cardRect  = new Rectangle(cx, screenTop, cw, card.Height);

			// Card body
			g.FillRoundedRectangle(cardBackBrush, cardRect, CornerRadius);

			// Section-color swatch — inset from corners to stay within the rounded region
			if (card.SectionColor != Color.Empty)
			{
				var swatchRect = new Rectangle(
					cx + 2, screenTop + CornerRadius,
					SwatchWidth, card.Height - CornerRadius * 2);

				using var swatchBrush = new SolidBrush(card.SectionColor);
				g.FillRectangle(swatchBrush, swatchRect);
			}

			const TextFormatFlags flags =
				TextFormatFlags.VerticalCenter |
				TextFormatFlags.EndEllipsis |
				TextFormatFlags.NoPrefix |
				TextFormatFlags.SingleLine;

			int contentX = cx + SwatchWidth + CardPadX;
			int contentW = cw - SwatchWidth - CardPadX * 2;
			int y        = screenTop + CardPadV;

			// Title row
			if (card.Title != null)
			{
				var isHovered = hoverCard == cardIndex && hoverHit == -1;
				var fore      = isHovered ? hoverFore : titleFore;

				// Checkbox glyph: visible when the card is hovered or already checked.
				// Reserve horizontal space for the checkbox in all titled cards so titles
				// don't reflow as the mouse moves between cards.
				var checkboxX  = contentX;
				var titleTextX = contentX + CheckBoxSize + CheckBoxGap;
				var titleW     = contentW - CheckBoxSize - CheckBoxGap;

				if (card.PageId != null && (isHovered || card.IsChecked))
				{
					var checkY = y + (TitleRowH - CheckBoxSize) / 2;
					var cbRect = new Rectangle(checkboxX, checkY, CheckBoxSize, CheckBoxSize);
					g.DrawRoundedRectangle(checkboxPen, cbRect, 2);
					if (card.IsChecked)
					{
						g.FillRoundedRectangle(checkboxFillBrush,
							new Rectangle(checkboxX + 2, checkY + 2, CheckBoxSize - 4, CheckBoxSize - 4), 2);
					}
				}

				var titleRect = new Rectangle(titleTextX, y, titleW, TitleRowH);
				TextRenderer.DrawText(g, card.Title, titleFont ?? Font, titleRect, fore, flags);
				y += TitleRowH;
			}

			// Hit rows
			for (int i = 0; i < card.Hits.Count; i++)
			{
				var isSelected = selectedCard == cardIndex && selectedHit == i;
				var isHovered  = hoverCard == cardIndex && hoverHit == i;

				if (isSelected)
				{
					var selRect = new Rectangle(cx + SwatchWidth + 4, y, cw - SwatchWidth - 5, HitRowH);
					g.FillRectangle(selectionBackBrush, selRect);
				}

				var fore    = isSelected ? selectionFore : isHovered ? hoverFore : hitFore;
				var hitRect = new Rectangle(contentX + HitIndent, y, contentW - HitIndent, HitRowH);

				TextRenderer.DrawText(g, card.Hits[i].PlainText, hitFont ?? Font, hitRect, fore, flags);
				y += HitRowH;
			}

			// Border drawn last so it sits on top of the swatch
			g.DrawRoundedRectangle(borderPen, cardRect, CornerRadius);
		}


		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			// Row heights are fixed, so layout doesn't change on resize — just repaint
			Invalidate();
		}


		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);

			// ScrollableControl.ScrollWindow BitBlts existing bits and only invalidates the
			// newly exposed strip. After base processes the scroll, force a full repaint so
			// OnPaint redraws all visible cards from the updated AutoScrollPosition.
			const int WM_VSCROLL    = 0x0115;
			const int WM_MOUSEWHEEL = 0x020A;

			if (m.Msg == WM_VSCROLL || m.Msg == WM_MOUSEWHEEL)
			{
				// Invalidate + Update (Refresh) forces a synchronous repaint immediately after
				// ScrollWindow's BitBlt so stale pixels don't linger on screen.
				Refresh();
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Hit testing

		private struct HitInfo
		{
			public int CardIndex;
			public int HitIndex;  // -1 = title
		}


		private HitInfo? HitTest(Point screenPt)
		{
			if (cards.Count == 0 || layoutDirty) return null;

			var vy = screenPt.Y - AutoScrollPosition.Y;  // virtual Y

			var idx = BinarySearchFirst(vy);
			if (idx >= cards.Count) return null;

			var card = cards[idx];
			if (vy < card.Y || vy >= card.Y + card.Height) return null;

			int y = card.Y + CardPadV;

			if (card.Title != null)
			{
				if (vy < y + TitleRowH)
					return new HitInfo { CardIndex = idx, HitIndex = -1 };

				y += TitleRowH;
			}

			for (int i = 0; i < card.Hits.Count; i++)
			{
				if (vy < y + HitRowH)
					return new HitInfo { CardIndex = idx, HitIndex = i };

				y += HitRowH;
			}

			return null;
		}


		private Rectangle GetCheckboxScreenRect(int cardIndex)
		{
			var card  = cards[cardIndex];
			var cx    = CardMarginH;
			var checkX = cx + SwatchWidth + CardPadX;
			var checkY = card.Y + CardPadV + (TitleRowH - CheckBoxSize) / 2 + AutoScrollPosition.Y;
			return new Rectangle(checkX, checkY, CheckBoxSize, CheckBoxSize);
		}


		protected override void OnMouseClick(MouseEventArgs e)
		{
			base.OnMouseClick(e);
			var hit = HitTest(e.Location);
			if (hit == null) return;

			var card = cards[hit.Value.CardIndex];

			// Checkbox click: toggle selection when clicking the glyph area on a titled card
			if (hit.Value.HitIndex == -1 && card.PageId != null && card.Title != null)
			{
				var cbRect = GetCheckboxScreenRect(hit.Value.CardIndex);
				if (cbRect.Contains(e.Location))
				{
					card.IsChecked = !card.IsChecked;
					InvalidateHitRegion(hit.Value.CardIndex, -1);
					CheckedChanged?.Invoke(this, EventArgs.Empty);
					return;
				}
			}

			if (hit.Value.HitIndex == -1 && card.PageId != null)
			{
				if (card.Hits.Count > 0)
				{
					SelectHit(hit.Value.CardIndex, 0);
				}
				CardActivated?.Invoke(this, new NavigateCardEventArgs(card.PageId));
			}
			else if (hit.Value.HitIndex >= 0)
			{
				SelectHit(hit.Value.CardIndex, hit.Value.HitIndex);
				var h = card.Hits[hit.Value.HitIndex];
				HitActivated?.Invoke(this, new NavigateHitEventArgs(h.PageId, h.ObjectId));
			}
		}


		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			var hit = HitTest(e.Location);

			int newCard = hit?.CardIndex ?? -1;
			int newHit  = hit?.HitIndex  ?? -1;

			Cursor = hit != null ? Cursors.Hand : Cursors.Default;

			if (newCard != hoverCard || newHit != hoverHit)
			{
				InvalidateHitRegion(hoverCard, hoverHit);
				hoverCard = newCard;
				hoverHit  = newHit;
				InvalidateHitRegion(hoverCard, hoverHit);
			}
		}


		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			Cursor = Cursors.Default;
			if (hoverCard >= 0)
			{
				InvalidateHitRegion(hoverCard, hoverHit);
				hoverCard = hoverHit = -1;
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Keyboard navigation

		public void MoveSelection(int delta)
		{
			if (cards.Count == 0) return;

			if (selectedCard < 0)
			{
				if (delta > 0) SelectFirst();
				else SelectLast();
				return;
			}

			int c = selectedCard;
			int h = selectedHit;

			while (delta > 0)
			{
				h++;
				if (h >= cards[c].Hits.Count)
				{
					c++;
					while (c < cards.Count && cards[c].Hits.Count == 0) c++;
					if (c >= cards.Count) return;
					h = 0;
				}
				delta--;
			}

			while (delta < 0)
			{
				h--;
				if (h < 0)
				{
					c--;
					while (c >= 0 && cards[c].Hits.Count == 0) c--;
					if (c < 0) return;
					h = cards[c].Hits.Count - 1;
				}
				delta++;
			}

			SelectHit(c, h);
		}


		private void SelectFirst()
		{
			for (int c = 0; c < cards.Count; c++)
			{
				if (cards[c].Hits.Count > 0) { SelectHit(c, 0); return; }
			}
		}


		private void SelectLast()
		{
			for (int c = cards.Count - 1; c >= 0; c--)
			{
				if (cards[c].Hits.Count > 0) { SelectHit(c, cards[c].Hits.Count - 1); return; }
			}
		}


		private void SelectHit(int cardIndex, int hitIndex)
		{
			if (cardIndex == selectedCard && hitIndex == selectedHit) return;

			InvalidateHitRegion(selectedCard, selectedHit);
			selectedCard = cardIndex;
			selectedHit  = hitIndex;
			InvalidateHitRegion(selectedCard, selectedHit);

			EnsureHitVisible(selectedCard, selectedHit);
		}


		public (string pageId, string objectId)? GetSelectedTarget()
		{
			if (selectedCard < 0 || selectedHit < 0) return null;
			var h = cards[selectedCard].Hits[selectedHit];
			return (h.PageId, h.ObjectId);
		}


		private void EnsureHitVisible(int cardIndex, int hitIndex)
		{
			if (cardIndex < 0 || hitIndex < 0) return;
			EnsureLayout();

			var card   = cards[cardIndex];
			var hitTop = card.Y + CardPadV
				+ (card.Title != null ? TitleRowH : 0)
				+ hitIndex * HitRowH;
			var hitBottom = hitTop + HitRowH;

			var scrollY = -AutoScrollPosition.Y;
			var clientH = ClientSize.Height;

			if (hitTop < scrollY)
				AutoScrollPosition = new Point(0, hitTop);
			else if (hitBottom > scrollY + clientH)
				AutoScrollPosition = new Point(0, hitBottom - clientH);
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Helpers

		/// <summary>
		/// Binary search for the first card whose bottom edge is below virtualY.
		/// Returns cards.Count if all cards are above virtualY.
		/// </summary>
		private int BinarySearchFirst(int virtualY)
		{
			if (cards.Count == 0) return 0;

			int lo = 0, hi = cards.Count - 1, result = cards.Count;

			while (lo <= hi)
			{
				int mid = (lo + hi) / 2;
				if (cards[mid].Y + cards[mid].Height > virtualY)
				{
					result = mid;
					hi = mid - 1;
				}
				else
				{
					lo = mid + 1;
				}
			}

			return result;
		}


		private void InvalidateHitRegion(int cardIndex, int hitIndex)
		{
			if (cardIndex < 0 || cardIndex >= cards.Count) return;
			EnsureLayout();

			var card = cards[cardIndex];
			Rectangle rect;

			if (hitIndex == -1)
			{
				var screenY = card.Y + CardPadV + AutoScrollPosition.Y;
				rect = new Rectangle(0, screenY, ClientSize.Width, TitleRowH);
			}
			else if (hitIndex >= 0 && hitIndex < card.Hits.Count)
			{
				var hitTop  = card.Y + CardPadV
					+ (card.Title != null ? TitleRowH : 0)
					+ hitIndex * HitRowH;
				var screenY = hitTop + AutoScrollPosition.Y;
				rect = new Rectangle(0, screenY, ClientSize.Width, HitRowH);
			}
			else return;

			if (rect.Bottom > 0 && rect.Top < ClientSize.Height)
				Invalidate(rect);
		}
	}


	internal sealed class NavigateCardEventArgs : EventArgs
	{
		public NavigateCardEventArgs(string pageId) { PageId = pageId; }
		public string PageId { get; }
	}


	internal sealed class NavigateHitEventArgs : EventArgs
	{
		public NavigateHitEventArgs(string pageId, string objectId)
		{
			PageId   = pageId;
			ObjectId = objectId;
		}
		public string PageId   { get; }
		public string ObjectId { get; }
	}
}
