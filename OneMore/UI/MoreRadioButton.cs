//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Windows.Forms;

	internal class MoreRadioButton : RadioButton
	{
		private const int Radius = 4;
		private const int Spacing = 4;
		private readonly ThemeManager manager;
		private readonly Color foreColor;
		private readonly IntPtr hcursor;
		private bool fitting;


		/// <summary>
		/// Initialize a new instance.
		/// </summary>
		public MoreRadioButton()
		{
			// force Paint event to fire, and reduce flickering
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);

			// force Hand cursor
			Cursor = Cursors.Hand;
			hcursor = Native.LoadCursor(IntPtr.Zero, Native.IDC_HAND);

			foreColor = ForeColor;
			manager = ThemeManager.Instance;
		}


		// computed from the control's current DPI so it tracks DPI changes
		private int BoxSize => MenuCheckSize.Width - 4;


		private Size MenuCheckSize => new(
			Scaling.GetSystemMetric(Native.SM_CXMENUCHECK, Scaling.GetDpi(this)),
			Scaling.GetSystemMetric(Native.SM_CYMENUCHECK, Scaling.GetDpi(this)));


		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (hcursor != IntPtr.Zero)
				{
					Native.DestroyCursor(hcursor);
				}
			}
			base.Dispose(disposing);
		}


		/// <summary>
		/// Gets the state indicating normal, hover, or pressed.
		/// </summary>
		public MouseState MouseState { get; private set; }


		protected override void OnPaint(PaintEventArgs pevent)
		{
			var g = pevent.Graphics;
			g.SmoothingMode = SmoothingMode.HighQuality;
			g.Clear(BackColor);

			if (Appearance == Appearance.Button)
			{
				PaintButton(pevent);
			}
			else
			{
				PaintNormal(pevent);
			}
		}


		private void PaintButton(PaintEventArgs pevent)
		{
			var g = pevent.Graphics;

			if (Enabled && (MouseState != MouseState.None || Checked))
			{
				using var brush = new SolidBrush(
					MouseState.HasFlag(MouseState.Pushed) || Checked
					? manager.ButtonHotBack
					: manager.ButtonBack);

				g.FillRoundedRectangle(brush, pevent.ClipRectangle, Radius);

				using var pen = new Pen(
					MouseState.HasFlag(MouseState.Pushed) || Checked
					? manager.ButtonPressBorder
					: manager.ButtonBorder);

				g.DrawRoundedRectangle(pen, pevent.ClipRectangle, Radius);
			}

			g.DrawImageUnscaled(Image,
				(pevent.ClipRectangle.Width - Image.Width) / 2,
				(pevent.ClipRectangle.Height - Image.Height) / 2
				);

		}


		private void PaintNormal(PaintEventArgs pevent)
		{
			var g = pevent.Graphics;

			var color = Enabled
				? manager.GetColor(foreColor)
				: manager.GetColor("GrayText");

			var radioColor = Enabled
				? manager.GetColor("Highlight")
				: color;

			var boxY = (Size.Height - BoxSize) / 2;

			using var radioPen = new Pen(radioColor);
			g.DrawEllipse(radioPen, 0, boxY, BoxSize, BoxSize);

			if (Checked)
			{
				using var fillBrush = new SolidBrush(radioColor);
				g.FillEllipse(fillBrush, 2, boxY + 2, BoxSize - 4, BoxSize - 4);
			}

			using var brush = new SolidBrush(color);

			var bounds = new Rectangle(BoxSize + Spacing,
					(pevent.ClipRectangle.Height - Size.Height) / 2 + 2,
					pevent.ClipRectangle.Width - (BoxSize + Spacing),
					Size.Height);

			using var format = CreateFormat();
			g.DrawString(Text, Font, brush, bounds, format);

			if (Focused)
			{
				using var focusPen = new Pen(manager.GetColor("GrayText"))
				{
					DashStyle = DashStyle.Dash
				};

				bounds.Offset(-1, -1);
				bounds.Inflate(-2, -2);
				g.DrawRectangle(focusPen, bounds);
			}
		}


		protected override void OnTextChanged(EventArgs e)
		{
			if (Appearance == Appearance.Normal)
			{
				var size = MenuCheckSize;
				if (string.IsNullOrWhiteSpace(Text) &&
					(Width < size.Width || Height < size.Height))
				{
					AutoSize = false;
					Width = size.Width;
					Height = size.Height;
					return;
				}

				EnsureFit();
			}

			base.OnTextChanged(e);
		}


		// the font is often inherited from the parent after Text is set, so re-fit then
		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			EnsureFit();
		}


		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);
			EnsureFit();
		}


		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			EnsureFit();
		}


		protected override void OnDpiChangedAfterParent(EventArgs e)
		{
			base.OnDpiChangedAfterParent(e);
			EnsureFit();
		}


		/// <summary>
		/// Grows the control, if needed, so the owner-drawn text is not clipped.
		/// </summary>
		private void EnsureFit()
		{
			if (Appearance != Appearance.Normal || string.IsNullOrWhiteSpace(Text))
			{
				return;
			}

			var fit = MeasureFit();
			fitting = true;

			if (Width != fit.Width || Height != fit.Height)
			{
				AutoSize = false;
				Width = fit.Width;
				Height = fit.Height;
			}
		}


		/// <summary>
		/// Gets the smallest size that shows the whole text.
		/// </summary>
		private Size MeasureFit()
		{
			// measure exactly as PaintNormal draws: same graphics type, font and format
			using var g = CreateGraphics();
			using var format = CreateFormat();
			var size = g.MeasureString(Text, Font, PointF.Empty, format);

			// +4 width for the trailing glyph overhang; +4 height because the text is
			// drawn 2px below the top and needs the same below
			return new Size(
				BoxSize + Spacing + (int)Math.Ceiling(size.Width) + 4,
				Math.Max((int)Math.Ceiling(size.Height) + 4, MenuCheckSize.Height));
		}


		/// <summary>
		/// Prevents the form's auto-scaling (which can run after the font has changed and
		/// resizes by rounded ratios) from shrinking the control below what the text needs.
		/// </summary>
		protected override void SetBoundsCore(
			int x, int y, int width, int height, BoundsSpecified specified)
		{
			if (fitting && !AutoSize && Appearance == Appearance.Normal &&
				!string.IsNullOrWhiteSpace(Text) && IsHandleCreated)
			{
				var fit = MeasureFit();
				width = Math.Max(width, fit.Width);
				height = Math.Max(height, fit.Height);
			}

			base.SetBoundsCore(x, y, width, height, specified);
		}


		private static StringFormat CreateFormat()
		{
			return new StringFormat
			{
				Trimming = StringTrimming.None,
				FormatFlags = StringFormatFlags.NoWrap
			};
		}


		protected override void OnMouseDown(MouseEventArgs mevent)
		{
			if (Enabled)
			{
				MouseState |= MouseState.Pushed;
				base.OnMouseDown(mevent);
			}
		}


		protected override void OnMouseUp(MouseEventArgs mevent)
		{
			if (Enabled)
			{
				MouseState &= ~MouseState.Pushed;
				base.OnMouseUp(mevent);
			}
		}


		protected override void OnMouseLeave(EventArgs eventargs)
		{
			if (Enabled)
			{
				MouseState &= ~MouseState.Hover;
				base.OnMouseLeave(eventargs);
			}
		}


		protected override void OnMouseEnter(EventArgs eventargs)
		{
			if (Enabled)
			{
				MouseState |= MouseState.Hover;
				base.OnMouseEnter(eventargs);
			}
		}


		protected override void WndProc(ref Message m)
		{
			if (m.Msg == Native.WM_SETCURSOR && hcursor != IntPtr.Zero)
			{
				Native.SetCursor(hcursor);
				m.Result = IntPtr.Zero; // indicate handled
				return;
			}

			base.WndProc(ref m);
		}
	}
}
