//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Linq;
	using System.Windows.Forms;


	internal class MoreRadioButton : RadioButton
	{
		private const int Radius = 4;
		private const int Spacing = 4;
		private readonly ThemeManager manager;
		private readonly Color foreColor;
		private readonly int boxSize;
		private readonly IntPtr hcursor;


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
			boxSize = SystemInformation.MenuCheckSize.Width - 3;
		}


		/// <summary>
		/// Gets the state indicating normal, hover, or pressed.
		/// </summary>
		public MouseState MouseState { get; private set; }


		protected override void OnPaint(PaintEventArgs pevent)
		{
			var g = pevent.Graphics;
			g.Clear(BackColor);

			if (Appearance == Appearance.Button)
			{
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
			else
			{
				var color = Enabled
					? manager.GetThemedColor(foreColor)
					: manager.GetThemedColor("GrayText");

				g.SmoothingMode = SmoothingMode.HighQuality;

				var boxY = (Size.Height - boxSize) / 2;

				if (Checked)
				{
					var c = Enabled
						? manager.GetThemedColor("Highlight")
						: color;

					using var pen = new Pen(c);
					g.DrawEllipse(pen, 0, boxY, boxSize, boxSize);

					using var fillBrush = new SolidBrush(c);
					g.FillEllipse(fillBrush, 0, boxY, boxSize, boxSize);

					using var dotbrush = new SolidBrush(BackColor);
					g.FillEllipse(dotbrush, 4, boxY + 4, boxSize - 8, boxSize - 8);
				}
				else
				{
					using var pen = new Pen(color);
					g.DrawEllipse(pen, 0, boxY, boxSize, boxSize);
				}

				using var brush = new SolidBrush(color);

				g.DrawString(Text, Font, brush,
					new Rectangle(boxSize + Spacing,
						(pevent.ClipRectangle.Height - Size.Height) / 2,
						pevent.ClipRectangle.Width - (boxSize + Spacing),
						Size.Height),
					new StringFormat
					{
						Trimming = StringTrimming.EllipsisCharacter,
						FormatFlags = StringFormatFlags.LineLimit | StringFormatFlags.NoWrap
					});
			}
		}


		protected override void OnTextChanged(EventArgs e)
		{
			if (Appearance == Appearance.Normal)
			{
				var size = SystemInformation.MenuCheckSize;
				if (string.IsNullOrWhiteSpace(Text) && Width != size.Width)
				{
					AutoSize = false;
					Width = size.Width;
					Height = size.Height;
					return;
				}

				// add fudge factor
				var text = $"{Text}.";
				if (Text.Contains(Environment.NewLine))
				{
					var parts = Text.Split(
						new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

					var max = parts.Max(p => p.Length);
					text = $"{parts.First(p => p.Length == max)}.";
				}

				using var g = CreateGraphics();
				size = g.MeasureString(text, Font).ToSize();
				var w = boxSize + Spacing + size.Width;
				if (Width != w)
				{
					AutoSize = false;
					Width = w;
					Height = Math.Max(size.Height, SystemInformation.MenuCheckSize.Height);
				}
			}

			base.OnTextChanged(e);
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
