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
			g.SmoothingMode = SmoothingMode.HighQuality;
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
			var clip = pevent.ClipRectangle;

			if (Enabled && (MouseState != MouseState.None || Checked))
			{
				using var brush = new SolidBrush(
					Checked
						? manager.GetThemedColor("ButtonDown")
						: MouseState.HasFlag(MouseState.Hover) ? hoverColor : backColor);

				g.FillRoundedRectangle(brush, clip, Radius);

				using var pen = new Pen(
					MouseState.HasFlag(MouseState.Pushed) || Checked
					? manager.ButtonPressBorder
					: manager.ButtonBorder);

				g.DrawRoundedRectangle(pen, clip, Radius);
			}

			var img = Image ?? BackgroundImage;
			if (img != null)
			{
				var r = clip;
				r.Inflate(-3, -3);
				g.DrawImage(img, r);
			}

			if (!string.IsNullOrWhiteSpace(Text))
			{
				var size = g.MeasureString(Text, Font);

				using var brush = new SolidBrush(
					Enabled ? foreColor : manager.GetThemedColor("GrayText"));

				pevent.Graphics.DrawString(Text, Font, brush,
					(clip.Width - (int)size.Width) / 2f,
					(clip.Height - (int)size.Height) / 2f,
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
			g.DrawRoundedRectangle(bpen,
				new Rectangle(clip.X, clip.Y, clip.Width - 1, clip.Height - 1),
				Radius);
		}


		private void PaintNormal(PaintEventArgs pevent)
		{
			var g = pevent.Graphics;

			var color = Enabled
				? manager.GetThemedColor(foreColor)
				: manager.GetThemedColor("GrayText");

			var boxColor = Enabled ? manager.GetThemedColor("Highlight") : color;

			var boxY = (Size.Height - boxSize) / 2;

			using var boxPen = new Pen(boxColor);
			g.DrawRoundedRectangle(boxPen, new Rectangle(0, boxY, boxSize, boxSize), Radius);

			if (Checked)
			{
				using var fillBrush = new SolidBrush(boxColor);
				g.FillRoundedRectangle(fillBrush,
					new Rectangle(2, boxY + 2, boxSize - 4, boxSize - 4), Radius);
			}

			using var brush = new SolidBrush(color);
			var textsize = g.MeasureString(Text, Font);

			pevent.Graphics.DrawString(Text, Font, brush,
				new Rectangle(boxSize + Spacing,
					(int)((pevent.ClipRectangle.Height - textsize.Height) / 2),
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
