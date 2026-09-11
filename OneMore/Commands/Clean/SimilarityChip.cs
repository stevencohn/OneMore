//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Commands.Compare;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Globalization;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// A small rounded-pill badge used by RemoveDuplicatesNavigator's results view, reusing
	/// Compare Hierarchy's similarity-score coloring and SimilarityPopup rubric breakdown.
	/// An exact-hash match shows a fixed, non-interactive "100% - identical" pill (hashing
	/// already proved identity, nothing to break down); a near-duplicate match shows a
	/// "NN% similar" pill that shows SimilarityPopup on hover, anchored near the chip, and
	/// closes it again once the mouse leaves both the chip and the popup.
	/// </summary>
	internal class SimilarityChip : Panel
	{
		private const int CornerRadius = 11;

		// how often, while a hover popup is open, to re-check whether the cursor is still
		// over the chip or the popup - polled rather than driven off MouseEnter/MouseLeave,
		// since those fire per child-control boundary within the popup's own many labels/
		// panels, not just at the popup window's outer edge
		private const int HoverPollIntervalMs = 200;

		private string text = string.Empty;
		private bool clickable;
		private Color fillColor;
		private Color textColor;
		private Color? borderColor;
		private SimilarityResult result;
		private string leftName;
		private string rightName;
		private SimilarityPopup activePopup;
		private Timer hoverTimer;


		/// <summary>
		/// The dialog this chip lives in. Needed only to re-elevate that dialog above OneNote
		/// after the popup closes - MoreForm.OnFormClosed unconditionally hands foreground
		/// focus back to OneNote on close (correct for a popup invoked directly from OneNote),
		/// which would otherwise submerge this still-open host dialog behind OneNote instead
		/// (same fix CompareDialog applies to its own "Compare contents..." popup).
		/// </summary>
		public MoreForm HostForm { get; set; }


		public SimilarityChip()
		{
			SetStyle(
				ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
				ControlStyles.OptimizedDoubleBuffer, true);

			Height = 22;
			Cursor = Cursors.Default;
		}


		/// <summary>
		/// Shows the fixed, non-clickable "100% - identical" pill for an exact hash match.
		/// </summary>
		public void SetExact()
		{
			var manager = ThemeManager.Instance;

			text = Resx.RemoveDuplicatesNavigator_identicalChip;
			clickable = false;
			result = null;
			Cursor = Cursors.Default;

			// same "100% = SuccessFill" color HierarchyDiffView.DrawScoreGlyph/DrawEqualsGlyph
			// uses for a perfect/definite match, for the same fixed "100% · identical" case
			textColor = manager.GetColor("SuccessFill");
			// blended against this chip's own (caller-assigned) BackColor, not a fixed theme
			// key, so the tint stays consistent with whatever surface actually hosts it
			fillColor = Blend(BackColor, textColor, 0.15f);
			borderColor = null;

			Invalidate();
		}


		/// <summary>
		/// Shows a clickable "NN% similar" pill, colored using the exact same tiers/limits and
		/// theme keys as Compare Hierarchy's well for a measured score (see
		/// HierarchyDiffView.DrawScoreGlyph: 100=SuccessFill, &gt;95=HintText, &gt;80=
		/// CompareWarningFill, else CompareErrorFill). A scored pair *can* round to 100% here
		/// even though it's not a hash-proven exact match - e.g. text-identical pages that
		/// differ only in something the checked metrics don't look at - so that tier is real,
		/// not just theoretical.
		/// </summary>
		public void SetSimilar(SimilarityResult similarityResult, string comparedLeftName, string comparedRightName)
		{
			var manager = ThemeManager.Instance;

			result = similarityResult;
			leftName = comparedLeftName;
			rightName = comparedRightName;

			var percent = (int)Math.Round(Math.Max(0.0, Math.Min(1.0, result.Overall)) * 100);
			text = string.Format(
				CultureInfo.InvariantCulture, Resx.RemoveDuplicatesNavigator_similarChipFormat, percent);
			clickable = true;
			Cursor = Cursors.Hand;

			textColor = percent == 100 ? manager.GetColor("SuccessFill")
				: percent > 95 ? manager.GetColor("HintText")
				: percent > 80 ? manager.GetColor("CompareWarningFill")
				: manager.GetColor("CompareErrorFill");
			// blended against this chip's own (caller-assigned) BackColor, not a fixed theme
			// key, so the tint stays consistent with whatever surface actually hosts it
			fillColor = Blend(BackColor, textColor, 0.15f);
			borderColor = textColor;

			Invalidate();
		}


		private static Color Blend(Color from, Color to, float amount)
		{
			return Color.FromArgb(
				(int)(from.R + ((to.R - from.R) * amount)),
				(int)(from.G + ((to.G - from.G) * amount)),
				(int)(from.B + ((to.B - from.B) * amount)));
		}


		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			if (string.IsNullOrEmpty(text))
			{
				return;
			}

			var g = e.Graphics;
			g.SmoothingMode = SmoothingMode.AntiAlias;

			var bounds = new Rectangle(0, 0, Width - 1, Height - 1);

			using var fillBrush = new SolidBrush(fillColor);
			g.FillRoundedRectangle(fillBrush, bounds, CornerRadius);

			if (borderColor.HasValue)
			{
				using var pen = new Pen(borderColor.Value, 1.25f);
				g.DrawRoundedRectangle(pen, bounds, CornerRadius);
			}

			TextRenderer.DrawText(g, text, Font, ClientRectangle, textColor,
				TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
				TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
		}


		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			if (clickable && result != null && activePopup == null)
			{
				ShowPopup();
			}
		}


		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				StopHoverTimer();
				activePopup?.Close();
			}

			base.Dispose(disposing);
		}


		private void ShowPopup()
		{
			var popup = new SimilarityPopup(leftName, rightName, result);
			activePopup = popup;

			popup.RunModeless(GetPopupLocation(popup), (s, ev) =>
			{
				StopHoverTimer();
				if (activePopup == popup)
				{
					activePopup = null;
				}

				popup.Dispose();

				// see HostForm's own comment: without this, closing the popup (by hovering
				// away, by Esc, or by clicking anywhere else, including back on this same
				// host dialog) leaves the host dialog submerged behind OneNote instead of on
				// top of it. keepTop:false matters here - Elevate()'s default (true) would
				// leave HostForm permanently TopMost, which would then sit above every *later*
				// popup too (each shown non-topmost via Elevate(false) below), even though
				// each of those correctly re-elevates itself once
				HostForm?.Elevate(false);
			});

			hoverTimer = new Timer { Interval = HoverPollIntervalMs };
			hoverTimer.Tick += (s, e) => CheckStillHovering(popup);
			hoverTimer.Start();

			// MoreForm.OnActivated normally does this same Elevate(false) call, but only once
			// Windows actually grants the new window activation - a plain mouse hover (unlike
			// a click) doesn't reliably carry that right from a background-process (dllhost.exe)
			// window, so OnActivated can simply never fire here; force it unconditionally
			// instead of relying on that
			popup.Elevate(false);
		}


		/// <summary>
		/// Closes the popup once the cursor is over neither the chip nor the popup itself -
		/// polled instead of MouseEnter/MouseLeave (see HoverPollIntervalMs) and in screen
		/// coordinates, so it stays correct if resultsPanel has scrolled since the popup opened.
		/// </summary>
		private void CheckStillHovering(SimilarityPopup popup)
		{
			if (popup.IsDisposed)
			{
				StopHoverTimer();
				return;
			}

			if (!IsHandleCreated || Parent == null)
			{
				// this row no longer exists (e.g. Rebuild ran after a delete/Keep Newest
				// elsewhere) - nothing left to hover, so don't leave the popup stranded open
				StopHoverTimer();
				popup.Close();
				return;
			}

			// inflated a few pixels past each control's real bounds: GetPopupLocation leaves a
			// small gap between the chip's top edge and the popup below it, and without some
			// tolerance there, a poll landing mid-transit across that gap would see the cursor
			// over neither rectangle and close the popup before the user finishes moving to it
			var cursor = Cursor.Position;

			var chipRect = RectangleToScreen(ClientRectangle);
			chipRect.Inflate(8, 8);

			var popupRect = popup.Bounds;
			popupRect.Inflate(8, 8);

			if (!chipRect.Contains(cursor) && !popupRect.Contains(cursor))
			{
				StopHoverTimer();
				popup.Close();
			}
		}


		private void StopHoverTimer()
		{
			if (hoverTimer != null)
			{
				hoverTimer.Stop();
				hoverTimer.Dispose();
				hoverTimer = null;
			}
		}


		private Point GetPopupLocation(SimilarityPopup popup)
		{
			var anchor = PointToScreen(Point.Empty);
			var size = popup.PreferredSize;

			var x = anchor.X;
			var y = anchor.Y - size.Height - 6;

			var working = Screen.FromControl(this).WorkingArea;
			x = Math.Min(x, working.Right - size.Width);
			x = Math.Max(x, working.Left);
			y = Math.Max(y, working.Top);

			return new Point(x, y);
		}
	}
}
