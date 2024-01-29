//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;


	internal class MoreCheckBox : CheckBox
	{
		private const int BoxSize = 20;
		private const int Radius = 4;
		private const int Spacing = 4;
		private readonly ThemeManager manager;
		private readonly Color foreColor;


		/// <summary>
		/// Initialize a new instance.
		/// </summary>
		public MoreCheckBox()
		{
			//// force Paint event to fire
			SetStyle(ControlStyles.UserPaint, true);
			//// reduce flickering
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);

			foreColor = ForeColor;
			manager = ThemeManager.Instance;
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

				var img = Image ?? BackgroundImage;
				if (img != null)
				{
					var r = pevent.ClipRectangle;
					r.Inflate(-3, -3);
					g.DrawImage(img, r);
				}

				if (!string.IsNullOrWhiteSpace(Text))
				{
					var color = Enabled
						? manager.GetThemedColor(foreColor)
						: manager.GetThemedColor("GrayText");

					var size = g.MeasureString(Text, Font);
					using var brush = new SolidBrush(color);

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
			else
			{
				// ensure we have something to measure relatively for box location
				var text = string.IsNullOrWhiteSpace(Text) ? "M" : Text;
				var size = g.MeasureString(text, Font);

				var color = Enabled
					? manager.GetThemedColor(foreColor)
					: manager.GetThemedColor("GrayText");

				using var pen = new Pen(color);
				var boxY = (int)((size.Height - BoxSize) / 2);

				g.SmoothingMode = SmoothingMode.HighQuality;

				if (Checked)
				{
					using var fillBrush = new SolidBrush(Enabled
						? manager.GetThemedColor("Highlight")
						: color);

					g.FillRoundedRectangle(fillBrush, new Rectangle(0, boxY, BoxSize, BoxSize), Radius);

					var path = new GraphicsPath();
					var x = BoxSize / 4;
					var y = boxY + (BoxSize / 2);
					path.AddLine(x, y, x + 3, y + 3);
					path.AddLine(x + 3, y + 3, x + 9, y - 3);

					using var checkPen = new Pen(BackColor, 2);
					g.DrawPath(checkPen, path);
				}
				else
				{
					g.DrawRoundedRectangle(pen, new Rectangle(0, boxY, BoxSize, BoxSize), Radius);
				}

				using var brush = new SolidBrush(color);

				pevent.Graphics.DrawString(Text, Font, brush,
					new Rectangle(BoxSize + Spacing,
						(pevent.ClipRectangle.Height - (int)size.Height) / 2,
						pevent.ClipRectangle.Width - (BoxSize + Spacing),
						(int)size.Height),
					new StringFormat
					{
						Trimming = StringTrimming.EllipsisCharacter,
						FormatFlags = StringFormatFlags.LineLimit | StringFormatFlags.NoWrap
					});
			}
		}


		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);

			// ensure we have something to measure and add fudge factor
			var text = string.IsNullOrWhiteSpace(Text) ? "M" : $"{Text}.";
			if (text.Contains(Environment.NewLine))
			{
				var parts = text.Split(
					new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

				var max = parts.Max(p => p.Length);
				text = $"{parts.First(p => p.Length == max)}.";
			}

			using var g = CreateGraphics();
			var size = g.MeasureString(text, Font);
			var w = (int)(size.Width + Spacing + BoxSize);
			if (Width != w)
			{
				AutoSize = false;
				Width = w;
			}
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
	}
}
