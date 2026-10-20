//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Drawing.Text;
	using System.Runtime.InteropServices;
	using System.Windows.Forms;


	/// <summary>
	/// A themed ComboBox that integrates with ThemeManager and supports an optional image
	/// prefix on each item. Items may be plain strings or <see cref="ComboItem"/> instances.
	/// </summary>
	internal class MoreComboBox : ComboBox, ILoadControl
	{
		private const int ImagePad = 4;

		protected StringFormat stringFormat;

		private const int DividerShift = 3;

		private const int WM_ERASEBKGND = 0x0014;
		private const int WM_SIZE = 0x0005;
		private const int WM_SETFONT = 0x0030;
		private const int CB_SETITEMHEIGHT = 0x0153;
		private const int CB_SETDROPPEDWIDTH = 0x0160;

		private bool hot;


		/// <summary>
		/// An item carrying optional image prefix alongside display text.
		/// </summary>
		public sealed class ComboItem
		{
			public ComboItem(string text, Image image = null)
			{
				Text = text ?? string.Empty;
				Image = image;
			}

			public string Text { get; }

			public Image Image { get; }

			// Required: ComboBox calls ToString() to render the edit-area text
			public override string ToString() => Text;
		}


		// Lifecycle

		public MoreComboBox()
		{
			DrawMode = DrawMode.OwnerDrawFixed;
			MakeStringFormat();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				stringFormat?.Dispose();
				stringFormat = null;
			}
			base.Dispose(disposing);
		}


		// designer-hidden property overrides

		[Browsable(false),
		 DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
		 EditorBrowsable(EditorBrowsableState.Never)]
		public new DrawMode DrawMode
		{
			get => base.DrawMode;
			set => base.DrawMode = value;
		}


		// theming

		public string ThemedBack { get; set; }

		public string ThemedFore { get; set; }

		void ILoadControl.OnLoad()
		{
			var manager = ThemeManager.Instance;
			BackColor = manager.GetColor("Window", ThemedBack);
			ForeColor = Enabled
				? manager.GetColor("WindowText", ThemedFore)
				: manager.GetColor("GrayText");

			if (manager.DarkMode)
			{
				FlatStyle = FlatStyle.Popup;
			}
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			((ILoadControl)this).OnLoad();
		}


		// owner-draw - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private int GetReservedImageWidth()
		{
			int max = 0;
			foreach (var raw in Items)
			{
				if (raw is ComboItem ci && ci.Image != null)
				{
					int w = ci.Image.Width + ImagePad * 2;
					if (w > max) max = w;
				}
			}
			return max;
		}

		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			base.OnDrawItem(e);

			if (e.Index < 0 || e.Index >= Items.Count)
				return;

			var manager = ThemeManager.Instance;
			bool isEdit = (e.State & DrawItemState.ComboBoxEdit) != 0;
			bool isSel  = !isEdit && (e.State & DrawItemState.Selected) != 0;

			Color back, fore;
			if (!Enabled)
			{
				back = manager.GetColor("InactiveWindow");
				fore = manager.GetColor("GrayText");
			}
			else if (isSel)
			{
				back = manager.GetColor("Highlight");
				fore = manager.GetColor("HighlightText");
			}
			else
			{
				back = manager.GetColor("Window", ThemedBack);
				fore = manager.GetColor("WindowText", ThemedFore);
			}

			using var backBrush = new SolidBrush(back);
			e.Graphics.FillRectangle(backBrush, e.Bounds);

			int reservedWidth = GetReservedImageWidth();
			var raw = Items[e.Index];
			int textX = e.Bounds.X + 2;

			if (reservedWidth > 0)
			{
				textX = e.Bounds.X + reservedWidth;
				if (raw is ComboItem ci && ci.Image != null)
				{
					int imgY = e.Bounds.Y + (e.Bounds.Height - ci.Image.Height) / 2;
					e.Graphics.DrawImage(ci.Image, e.Bounds.X + ImagePad, imgY);
				}
			}

			var textBounds = new Rectangle(
				textX, e.Bounds.Y, e.Bounds.Right - textX - 2, e.Bounds.Height);

			// A draw message can still arrive while the dialog is closing and its controls, fonts
			// and formats are being disposed, or after the combo has been collapsed to nothing;
			// GDI+ answers either with "Parameter is not valid", which is not worth reporting.
			if (IsDisposed || Disposing || textBounds.Width <= 0 || textBounds.Height <= 0)
			{
				return;
			}

			using var foreBrush = new SolidBrush(fore);

			try
			{
				e.Graphics.DrawString(
					// use GetItemText here to pick up DisplayMember when there is a DataSource
					GetItemText(raw),
					GetItemFont(raw), foreBrush, textBounds, stringFormat);
			}
			catch (ArgumentException)
			{
				// the font or format was disposed while the form was closing
				return;
			}

			if (!isEdit && (e.State & DrawItemState.Focus) != 0)
				e.DrawFocusRectangle();
		}


		/// <summary>
		/// Gets the font to use when drawing the given item's text. Overridable so subclasses
		/// like FontComboBox can render each item in its own font family.
		/// </summary>
		protected virtual Font GetItemFont(object item) => Font;


		protected virtual void MakeStringFormat()
		{
			stringFormat?.Dispose();
			stringFormat = new StringFormat(StringFormatFlags.NoWrap)
			{
				Trimming      = StringTrimming.EllipsisCharacter,
				HotkeyPrefix  = HotkeyPrefix.None,
				Alignment     = StringAlignment.Near,
				LineAlignment = StringAlignment.Center
			};
		}


		// dark-mode dropdown button theming - - - - - - - - - - - - - - - - - - - - - - - - - - -

		protected override void WndProc(ref Message m)
		{
			// Overlaying our border on top of the native/WinForms drawing always flashes the
			// heavy native border for a moment, so for drop-down lists paint everything
			// ourselves into a buffer and never let the native control draw.
			if (IsHandleCreated && DropDownStyle != ComboBoxStyle.Simple)
			{
				if (m.Msg == WM_ERASEBKGND)
				{
					m.Result = (IntPtr)1;
					return;
				}

				if (m.Msg == Native.WM_PAINT)
				{
					PaintOwned();
					m.Result = IntPtr.Zero;
					return;
				}
			}

			base.WndProc(ref m);

			switch (m.Msg)
			{
				case Native.WM_PAINT:
					PaintOverlay();
					break;

				// native layout of the edit and button areas changes on these without
				// necessarily repainting, and we've suppressed the native painting
				case WM_SIZE:
				case WM_SETFONT:
				case CB_SETITEMHEIGHT:
				case CB_SETDROPPEDWIDTH:
					Invalidate();
					break;
			}
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);

			// the first paint can precede the final native layout
			BeginInvoke(new Action(Invalidate));
		}


		[StructLayout(LayoutKind.Sequential)]
		private struct PAINTSTRUCT
		{
			public IntPtr hdc;
			public int fErase;
			public Native.RECT rcPaint;
			public int fRestore;
			public int fIncUpdate;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
			public byte[] rgbReserved;
		}

		[DllImport("user32.dll")]
		private static extern IntPtr BeginPaint(IntPtr hWnd, out PAINTSTRUCT ps);

		[DllImport("gdi32.dll")]
		private static extern int ExcludeClipRect(IntPtr hdc, int left, int top, int right, int bottom);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT ps);


		private void PaintOwned()
		{
			var hdc = BeginPaint(Handle, out var ps);
			try
			{
				if (IsDisposed || Width < 4 || Height < 4)
				{
					return;
				}

				var manager = ThemeManager.Instance;
				var bounds = new Rectangle(0, 0, Width, Height);

				var context = BufferedGraphicsManager.Current;
				using var buffered = context.Allocate(hdc, bounds);
				var g = buffered.Graphics;

				using (var backBrush = new SolidBrush(
					Enabled ? BackColor : manager.GetColor("InactiveWindow")))
				{
					g.FillRectangle(backBrush, bounds);
				}

				Rectangle? editRect = null;
				var info = new Native.COMBOBOXINFO { cbSize = Marshal.SizeOf<Native.COMBOBOXINFO>() };
				if (Native.GetComboBoxInfo(Handle, ref info))
				{
					var item = Rectangle.FromLTRB(
						info.rcItem.Left, info.rcItem.Top, info.rcItem.Right, info.rcItem.Bottom);

					if (!item.IsEmpty)
					{
						if (DropDownStyle == ComboBoxStyle.DropDownList)
						{
							if (SelectedIndex >= 0)
							{
								g.SetClip(item);
								OnDrawItem(new DrawItemEventArgs(
									g, Font, item, SelectedIndex, DrawItemState.ComboBoxEdit,
									ForeColor, BackColor));
								g.ResetClip();
							}
						}
						else
						{
							// an editable combo has a child edit control that paints itself;
							// keep it out of the buffer so its pixels aren't overwritten
							// never exclude the button area, even if the layout is momentarily stale
						var buttonLeft = info.rcButton.Left - DividerShift;
						if (buttonLeft > item.Left && item.Right > buttonLeft)
						{
							item.Width = buttonLeft - item.Left;
						}

						editRect = item;
						}
					}
				}

				PaintDarkButton(g, manager);
				PaintBorder(g, manager);

				if (editRect is Rectangle edit)
				{
					ExcludeClipRect(hdc, edit.Left, edit.Top, edit.Right, edit.Bottom);
				}

				buffered.Render();
			}
			finally
			{
				EndPaint(Handle, ref ps);
			}
		}


		private void PaintOverlay()
		{
			if (IsDisposed || !IsHandleCreated || Width < 4 || Height < 4)
			{
				return;
			}

			var manager = ThemeManager.Instance;

			using var g = Graphics.FromHwnd(Handle);

			if (manager.DarkMode)
			{
				PaintDarkButton(g, manager);
			}

			PaintBorder(g, manager);
		}


		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			hot = true;
			Invalidate();
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			hot = false;
			Invalidate();
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			Invalidate();
		}

		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
			Invalidate();
		}

		protected override void OnSelectedIndexChanged(EventArgs e)
		{
			base.OnSelectedIndexChanged(e);
			Invalidate();
		}

		protected override void OnDropDownClosed(EventArgs e)
		{
			base.OnDropDownClosed(e);
			Invalidate();
		}


		/// <summary>
		/// Replaces the native border, which is heavier at rest than on hover, with a single
		/// 1px outline consistent with MoreTextBox and MoreDateTimePicker.
		/// </summary>
		private void PaintBorder(Graphics g, ThemeManager manager)
		{
			var border = manager.ButtonBorder;
			if (Enabled && ContainsFocus)
			{
				border = manager.GetColor("HotTrack");
			}
			else if (Enabled && hot)
			{
				border = manager.ButtonHotBorder;
			}

			// cover the inner border drawn by FlatStyle.Popup
			using var innerPen = new Pen(BackColor);
			g.DrawRectangle(innerPen, 1, 1, Width - 3, Height - 3);

			using var pen = new Pen(border);
			g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
		}


		private void PaintDarkButton(Graphics g, ThemeManager manager)
		{
			var info = new Native.COMBOBOXINFO { cbSize = Marshal.SizeOf<Native.COMBOBOXINFO>() };
			if (!Native.GetComboBoxInfo(Handle, ref info))
			{
				return;
			}

			var btnRect = Rectangle.FromLTRB(
				info.rcButton.Left, info.rcButton.Top, info.rcButton.Right, info.rcButton.Bottom);

			if (btnRect.IsEmpty)
			{
				return;
			}

			bool pressed = info.stateButton == 2;

			var backColor = pressed ? manager.ButtonPressBorder : manager.ButtonBack;
			using var backBrush = new SolidBrush(backColor);

			// erase the divider that FlatStyle.Popup draws just left of the button so only
			// our 1px divider remains; it can sit a pixel or two left of the button
			using var clearBrush = new SolidBrush(BackColor);
			g.FillRectangle(clearBrush, btnRect.Left - 4, btnRect.Top, 4, btnRect.Height);

			// extend the button 3px left to place the divider there
			btnRect = Rectangle.FromLTRB(
				btnRect.Left - DividerShift, btnRect.Top, btnRect.Right, btnRect.Bottom);

			g.FillRectangle(backBrush, btnRect);

			using var borderPen = new Pen(manager.ButtonBorder);
			g.DrawLine(borderPen, btnRect.Left, btnRect.Top, btnRect.Left, btnRect.Bottom - 1);

			var arrowColor = Enabled ? ForeColor : manager.GetColor("GrayText");
			using var arrowBrush = new SolidBrush(arrowColor);
			const int aw = 8, ah = 4;
			int ax = btnRect.Left + (btnRect.Width - aw) / 2;
			int ay = btnRect.Top  + (btnRect.Height - ah) / 2;
			g.FillPolygon(arrowBrush, new[]
			{
				new Point(ax,          ay),
				new Point(ax + aw,     ay),
				new Point(ax + aw / 2, ay + ah)
			});
		}
	}
}
