//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Windows.Forms;


	/// <summary>
	/// A TrackBar with the same API and behavior as the standard control, but whose channel
	/// and thumb are painted with theme colors. The thumb uses the theme Highlight color, the
	/// channel is one pixel thinner than the native channel and is the same color as buttons
	/// (ButtonFace). The mouse cursor is a hand while over the thumb. Tick marks are left
	/// to the native control.
	/// </summary>
	/// <remarks>
	/// Painting is done by answering the control's NM_CUSTOMDRAW notifications, which keeps
	/// all native input, keyboard, scrolling and accessibility behavior intact.
	/// </remarks>
	internal class MoreSlider : TrackBar, ILoadControl
	{
		private readonly IntPtr handCursor = Native.LoadCursor(IntPtr.Zero, Native.IDC_HAND);


		// themed containers such as MorePanel assign their own background after their children
		// have been colorized, so the slider is told to adopt its parent's final color on load
		void ILoadControl.OnLoad()
		{
			MatchParentBackColor();
		}


		protected override void OnParentBackColorChanged(EventArgs e)
		{
			base.OnParentBackColorChanged(e);
			MatchParentBackColor();
		}


		private void MatchParentBackColor()
		{
			if (Parent is not null && BackColor != Parent.BackColor)
			{
				BackColor = Parent.BackColor;
			}
		}


		protected override void WndProc(ref Message m)
		{
			if (m.Msg == Native.WM_SETCURSOR && handCursor != IntPtr.Zero && ShowHandCursor())
			{
				Native.SetCursor(handCursor);
				m.Result = (IntPtr)1; // indicate handled
				return;
			}

			// NM_CUSTOMDRAW is sent to the parent as WM_NOTIFY, which WinForms reflects back
			// to this control as WM_REFLECT + WM_NOTIFY
			if (m.Msg == Native.WM_REFLECT + Native.WM_NOTIFY && CustomDraw(ref m))
			{
				return;
			}

			base.WndProc(ref m);
		}


		// over the thumb, or anywhere while dragging it since the mouse is captured then
		private bool ShowHandCursor()
		{
			if (!Enabled)
			{
				return false;
			}

			return Capture || GetThumbRect().Contains(PointToClient(Cursor.Position));
		}


		private bool CustomDraw(ref Message m)
		{
			var hdr = (Native.NMHDR)m.GetLParam(typeof(Native.NMHDR));
			if (hdr.code != Native.NM_CUSTOMDRAW || hdr.hwndFrom != Handle)
			{
				return false;
			}

			var nmcd = (Native.NMCUSTOMDRAW)m.GetLParam(typeof(Native.NMCUSTOMDRAW));

			if (nmcd.dwDrawStage == Native.CDDS_PREPAINT)
			{
				m.Result = (IntPtr)Native.CDRF_NOTIFYITEMDRAW;
				return true;
			}

			if (nmcd.dwDrawStage == Native.CDDS_ITEMPREPAINT)
			{
				var part = nmcd.dwItemSpec.ToInt32();
				if (part == Native.TBCD_CHANNEL)
				{
					PaintChannel(nmcd);
					m.Result = (IntPtr)Native.CDRF_SKIPDEFAULT;
					return true;
				}

				if (part == Native.TBCD_THUMB)
				{
					PaintThumb(nmcd);
					m.Result = (IntPtr)Native.CDRF_SKIPDEFAULT;
					return true;
				}
			}

			// ticks and everything else are painted natively
			m.Result = (IntPtr)Native.CDRF_DODEFAULT;
			return true;
		}


		private void PaintChannel(Native.NMCUSTOMDRAW nmcd)
		{
			var manager = ThemeManager.Instance;
			var horizontal = Orientation == Orientation.Horizontal;

			var channel = Rectangle.FromLTRB(
				nmcd.rc.Left, nmcd.rc.Top, nmcd.rc.Right, nmcd.rc.Bottom);

			// one pixel thinner than the native channel, kept centered within it
			Rectangle bar;
			if (horizontal)
			{
				var h = Math.Max(1, channel.Height - 1);
				bar = new Rectangle(channel.X, channel.Y + ((channel.Height - h) / 2), channel.Width, h);
			}
			else
			{
				var w = Math.Max(1, channel.Width - 1);
				bar = new Rectangle(channel.X + ((channel.Width - w) / 2), channel.Y, w, channel.Height);
			}

			var color = manager.GetColor("ButtonFace");

			using var g = Graphics.FromHdc(nmcd.hdc);
			g.SmoothingMode = SmoothingMode.AntiAlias;

			var thickness = horizontal ? bar.Height : bar.Width;
			using var path = RoundedPath(bar, thickness / 2f);

			using (var brush = new SolidBrush(color))
			{
				g.FillPath(brush, path);
			}

			// outline keeps the bar visible against the page in themes where it matches
			using var pen = new Pen(manager.GetColor("ControlDark"));
			g.DrawPath(pen, path);
		}


		private void PaintThumb(Native.NMCUSTOMDRAW nmcd)
		{
			var manager = ThemeManager.Instance;

			var rect = Rectangle.FromLTRB(
				nmcd.rc.Left, nmcd.rc.Top, nmcd.rc.Right, nmcd.rc.Bottom);

			var fill = Enabled
				? manager.GetColor("Highlight")
				: manager.GetColor("GrayText");

			using var g = Graphics.FromHdc(nmcd.hdc);
			g.SmoothingMode = SmoothingMode.AntiAlias;

			// keep the 1px stroke inside the thumb's rectangle
			rect.Width--;
			rect.Height--;

			using var path = ThumbPath(rect);
			using var brush = new SolidBrush(fill);
			g.FillPath(brush, path);

			using var pen = new Pen(Blend(fill, manager.GetColor("ControlDarkDark"), 0.25f));
			g.DrawPath(pen, path);
		}


		private Rectangle GetThumbRect()
		{
			var rect = new Native.RECT();
			Native.SendMessage(Handle, Native.TBM_GETTHUMBRECT, IntPtr.Zero, ref rect);
			return Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}


		// the native thumb points toward the ticks when they are on only one side
		private GraphicsPath ThumbPath(Rectangle r)
		{
			var path = new GraphicsPath();
			var horizontal = Orientation == Orientation.Horizontal;
			var point = TickStyle switch
			{
				TickStyle.BottomRight => 1,
				TickStyle.TopLeft => -1,
				_ => 0
			};

			if (point == 0)
			{
				path.AddRectangle(r);
			}
			else if (horizontal)
			{
				var tip = r.Width / 2;
				if (point > 0)
				{
					// pointing down
					path.AddPolygon(new[]
					{
						new Point(r.Left, r.Top), new Point(r.Right, r.Top),
						new Point(r.Right, r.Bottom - tip), new Point(r.Left + tip, r.Bottom),
						new Point(r.Left, r.Bottom - tip)
					});
				}
				else
				{
					// pointing up
					path.AddPolygon(new[]
					{
						new Point(r.Left, r.Bottom), new Point(r.Right, r.Bottom),
						new Point(r.Right, r.Top + tip), new Point(r.Left + tip, r.Top),
						new Point(r.Left, r.Top + tip)
					});
				}
			}
			else
			{
				var tip = r.Height / 2;
				if (point > 0)
				{
					// pointing right
					path.AddPolygon(new[]
					{
						new Point(r.Left, r.Top), new Point(r.Right - tip, r.Top),
						new Point(r.Right, r.Top + tip), new Point(r.Right - tip, r.Bottom),
						new Point(r.Left, r.Bottom)
					});
				}
				else
				{
					// pointing left
					path.AddPolygon(new[]
					{
						new Point(r.Right, r.Top), new Point(r.Left + tip, r.Top),
						new Point(r.Left, r.Top + tip), new Point(r.Left + tip, r.Bottom),
						new Point(r.Right, r.Bottom)
					});
				}
			}

			return path;
		}


		private static GraphicsPath RoundedPath(Rectangle r, float radius)
		{
			var path = new GraphicsPath();
			var d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
			if (d < 1)
			{
				path.AddRectangle(r);
				return path;
			}

			// outline is drawn on the pixel grid so shrink by one to stay inside the bounds
			var b = new RectangleF(r.X, r.Y, r.Width - 1, r.Height - 1);
			path.AddArc(b.Left, b.Top, d, d, 180, 90);
			path.AddArc(b.Right - d, b.Top, d, d, 270, 90);
			path.AddArc(b.Right - d, b.Bottom - d, d, d, 0, 90);
			path.AddArc(b.Left, b.Bottom - d, d, d, 90, 90);
			path.CloseFigure();
			return path;
		}


		private static Color Blend(Color color, Color toward, float amount)
		{
			return Color.FromArgb(
				(int)(color.R + ((toward.R - color.R) * amount)),
				(int)(color.G + ((toward.G - color.G) * amount)),
				(int)(color.B + ((toward.B - color.B) * amount)));
		}
	}
}
