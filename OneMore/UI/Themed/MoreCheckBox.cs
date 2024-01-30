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


	internal class MoreCheckBox : CheckBox
	{
		private const int Radius = 4;
		private const int Spacing = 4;
		private readonly ThemeManager manager;
		private readonly int boxSize;
		private readonly IntPtr hcursor;
		private readonly Color backColor;
		private readonly Color foreColor;
		private readonly Color hoverColor;


		/// <summary>
		/// Initialize a new instance.
		/// </summary>
		public MoreCheckBox()
		{
			// force Paint event to fire, and reduce flickering
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);

			// force Hand cursor
			Cursor = Cursors.Hand;
			hcursor = Native.LoadCursor(IntPtr.Zero, Native.IDC_HAND);

			boxSize = SystemInformation.MenuCheckSize.Width - 3;

			manager = ThemeManager.Instance;
			BackColor = backColor = manager.GetThemedColor("ButtonFace");
			ForeColor = foreColor = manager.GetThemedColor("ControlText");
			hoverColor = manager.GetThemedColor("ButtonHighlight");
		}


		/// <summary>
		/// Gets the state indicating normal, hover, or pressed.
		/// </summary>
		public MouseState MouseState { get; private set; }


		protected override void OnPaint(PaintEventArgs pevent)
		{
			var g = pevent.Graphics;
			g.Clear(Parent.BackColor);

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
					Checked
						? manager.GetThemedColor("ButtonDown")
						: MouseState.HasFlag(MouseState.Hover) ? hoverColor : backColor);

				g.FillRoundedRectangle(brush, pevent.ClipRectangle, Radius);

				using var pen = new Pen(
					MouseState.HasFlag(MouseState.Pushed) || Checked
					? manager.ButtonPressBorder
					: manager.ButtonBorder);

				g.DrawRoundedRectangle(pen, pevent.ClipRectangle, Radius);
			}

			var img = Image ?? BackgroundImage;
			if (img != null)
			{
				var r = pevent.ClipRectangle;
				r.Inflate(-3, -3);
				g.DrawImage(img, r);
			}

			if (!string.IsNullOrWhiteSpace(Text))
			{
				var size = g.MeasureString(Text, Font);

				using var brush = new SolidBrush(
					Enabled ? foreColor : manager.GetThemedColor("GrayText"));

				pevent.Graphics.DrawString(Text, Font, brush,
					(pevent.ClipRectangle.Width - (int)size.Width) / 2f,
					(pevent.ClipRectangle.Height - (int)size.Height) / 2f,
					new StringFormat
					{
						Trimming = StringTrimming.EllipsisCharacter,
						FormatFlags = StringFormatFlags.LineLimit | StringFormatFlags.NoWrap
					});
			}

			Color border;
			if (MouseState.HasFlag(MouseState.Pushed))
			{
				border = manager.ButtonPressBorder;
			}
			else if (Focused || IsDefault) // || this == FindForm().AcceptButton)
			{
				border = manager.GetThemedColor("HotTrack");
			}
			else if (MouseState.HasFlag(MouseState.Hover))
			{
				border = manager.ButtonHotBorder;
			}
			else
			{
				border = manager.ButtonBorder;
			}

			using var bpen = new Pen(border, 2);
			g.DrawRoundedRectangle(bpen, pevent.ClipRectangle, Radius);
		}


		private void PaintNormal(PaintEventArgs pevent)
		{
			// ensure we have something to measure relatively for box location

			var color = Enabled
				? manager.GetThemedColor(foreColor)
				: manager.GetThemedColor("GrayText");

			using var pen = new Pen(color);
			var boxY = (Size.Height - boxSize) / 2;

			var g = pevent.Graphics;
			g.SmoothingMode = SmoothingMode.HighQuality;

			if (Checked)
			{
				using var fillBrush = new SolidBrush(Enabled
					? manager.GetThemedColor("Highlight")
					: color);

				g.FillRoundedRectangle(fillBrush, new Rectangle(0, boxY, boxSize, boxSize), Radius);

				var path = new GraphicsPath();
				var x = boxSize / 4;
				var y = boxY + (boxSize / 2);
				path.AddLine(x, y, x + 3, y + 3);
				path.AddLine(x + 3, y + 3, x + 9, y - 3);

				using var checkPen = new Pen(BackColor, 2);
				g.DrawPath(checkPen, path);
			}
			else
			{
				g.DrawRoundedRectangle(pen, new Rectangle(0, boxY, boxSize, boxSize), Radius);
			}

			using var brush = new SolidBrush(color);

			pevent.Graphics.DrawString(Text, Font, brush,
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
