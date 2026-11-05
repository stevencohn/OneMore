//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.Formulas
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Windows.Forms;


	/// <summary>
	/// Owner-drawn list of reference entries. Each entry is two lines, the signature in a
	/// monospaced font followed by its description, with a small gap before the next entry.
	/// Entries that open a further detail view show a chevron on the right.
	/// </summary>
	internal class ReferenceListBox : ListBox
	{
		private const TextFormatFlags Flags = TextFormatFlags.NoPrefix
			| TextFormatFlags.EndEllipsis | TextFormatFlags.Left | TextFormatFlags.VerticalCenter
			| TextFormatFlags.SingleLine;

		private readonly ThemeManager manager;
		private Font signatureFont;
		private Font boldFont;


		public ReferenceListBox()
		{
			manager = ThemeManager.Instance;

			DrawMode = DrawMode.OwnerDrawFixed;
			BorderStyle = BorderStyle.None;
			IntegralHeight = false;
			DoubleBuffered = true;
			SetStyle(ControlStyles.ResizeRedraw, true);
		}


		public ReferenceEntry SelectedEntry => SelectedItem as ReferenceEntry;


		public void SetEntries(IEnumerable<ReferenceEntry> entries)
		{
			BeginUpdate();
			Items.Clear();
			foreach (var entry in entries)
			{
				Items.Add(entry);
			}
			EndUpdate();
		}


		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				signatureFont?.Dispose();
				boldFont?.Dispose();
			}

			base.Dispose(disposing);
		}


		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			BackColor = manager.GetColor("Window");

			// the native window can be recreated after the form applied its theme, for example
			// when the parent tab control changes its style, so dark scrollbars must be
			// re-applied every time the handle is created
			manager.InitializeTheme(this);
			UpdateMetrics();
		}


		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			UpdateMetrics();
		}


		private void UpdateMetrics()
		{
			signatureFont?.Dispose();
			boldFont?.Dispose();

			signatureFont = new Font("Consolas", Font.SizeInPoints + 1F);
			boldFont = new Font(signatureFont, FontStyle.Bold);

			// two lines plus padding above, between and below (the gap before the next entry)
			var pad = Math.Max(4, Font.Height / 3);
			ItemHeight = signatureFont.Height + Font.Height + (pad * 3);
		}


		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			if (e.Index < 0 || e.Index >= Items.Count || Items[e.Index] is not ReferenceEntry entry)
			{
				return;
			}

			var g = e.Graphics;
			var bounds = e.Bounds;
			var selected = (e.State & DrawItemState.Selected) != 0;
			var accent = manager.GetColor("HotTrack");
			var window = manager.GetColor("Window");

			using (var back = new SolidBrush(window))
			{
				g.FillRectangle(back, bounds);
			}

			if (selected)
			{
				using var tint = new SolidBrush(Color.FromArgb(40, accent));
				g.FillRectangle(tint, bounds);

				var barWidth = Math.Max(3, Font.Height / 5);
				using var bar = new SolidBrush(accent);
				g.FillRectangle(bar, bounds.X, bounds.Y, barWidth, bounds.Height);
			}

			var pad = Math.Max(4, Font.Height / 3);
			var left = bounds.X + (pad * 2);
			var right = bounds.Right - pad;

			if (entry.Drill)
			{
				using var chevronFont = new Font(Font.FontFamily, Font.SizeInPoints + 4F, FontStyle.Bold);
				var chevron = new Rectangle(
					right - (Font.Height * 2), bounds.Y, Font.Height * 2, bounds.Height);

				TextRenderer.DrawText(g, "›", chevronFont, chevron, accent,
					TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

				right = chevron.X;
			}

			var sigColor = entry.Custom || selected || entry.Drill
				? accent
				: manager.GetColor("ControlText");

			var sigRect = new Rectangle(left, bounds.Y + pad, right - left, signatureFont.Height);
			TextRenderer.DrawText(g, entry.Signature, selected ? boldFont : signatureFont,
				sigRect, sigColor, Flags);

			var descRect = new Rectangle(
				left, sigRect.Bottom + (pad / 2), right - left, Font.Height);

			TextRenderer.DrawText(g, entry.Description, Font, descRect,
				manager.GetColor("GrayText"), Flags);
		}
	}
}
