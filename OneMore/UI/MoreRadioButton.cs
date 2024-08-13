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

			var boxY = (Size.Height - boxSize) / 2;

			using var radioPen = new Pen(radioColor);
			g.DrawEllipse(radioPen, 0, boxY, boxSize, boxSize);

			if (Checked)
			{
				using var fillBrush = new SolidBrush(radioColor);
				g.FillEllipse(fillBrush, 2, boxY + 2, boxSize - 4, boxSize - 4);
			}

			using var brush = new SolidBrush(color);

			g.DrawString(Text, Font, brush,
				new Rectangle(boxSize + Spacing,
					(pevent.ClipRectangle.Height - Size.Height) / 2,
					pevent.ClipRectangle.Width - (boxSize + Spacing),
					Size.Height),
				new StringFormat
				{
					Trimming = StringTrimming.None,
					FormatFlags = StringFormatFlags.NoWrap
				});
		}


		protected override void OnTextChanged(EventArgs e)
		{
			if (Appearance == Appearance.Normal)
			{
				var size = SystemInformation.MenuCheckSize;
				if (string.IsNullOrWhiteSpace(Text) &&
					(Width < size.Width || Height < size.Height))
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
				var h = Math.Max(size.Height, SystemInformation.MenuCheckSize.Height);
				if (Width < w || Height < h)
				{
					AutoSize = false;
					Width = w;
					Height = h;
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
