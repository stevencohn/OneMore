//************************************************************************************************
// Copyright © 2022 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;


	/// <summary>
	/// Custom Button
	/// </summary>
	internal class MoreButton : Button
	{
		private const int Radius = 4;
		private Image enabledImage;


		/// <summary>
		/// Initialize a new instance.
		/// </summary>
		public MoreButton()
		{
			// force Paint event to fire
			SetStyle(ControlStyles.UserPaint, true);
			// reduce flickering
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
		}


		/// <summary>
		/// Gets the state indicating normal, hover, or pressed.
		/// </summary>
		public MouseState MouseState { get; private set; }


		protected override void OnPaint(PaintEventArgs pevent)
		{
			var g = pevent.Graphics;
			g.Clear(BackColor);

			if (Enabled && MouseState != MouseState.None)
			{
				var color = MouseState.HasFlag(MouseState.Pushed)
					? AppColors.PressedColor
					: AppColors.HoverColor;

				using (var brush = new SolidBrush(color))
				{
					g.FillRoundedRectangle(brush, pevent.ClipRectangle, Radius);
				}

				color = MouseState.HasFlag(MouseState.Pushed)
					? AppColors.PressedBorder
					: AppColors.HoverBorder;

				using (var pen = new Pen(color))
				{
					g.DrawRoundedRectangle(pen, pevent.ClipRectangle, Radius);
				}
			}

			if (Image != null)
			{
				g.DrawImageUnscaled(Image,
					(pevent.ClipRectangle.Width - Image.Width) / 2,
					(pevent.ClipRectangle.Height - Image.Height) / 2
					);
			}

			if (!string.IsNullOrEmpty(Text))
			{
				var size = g.MeasureString(Text, Font);
				using (var brush = new SolidBrush(Enabled ? ForeColor : Color.Gray))
				{
					g.DrawString(Text, Font, brush,
						(pevent.ClipRectangle.Width - size.Width) / 2,
						(pevent.ClipRectangle.Height - size.Height) / 2,
						new StringFormat
						{
							Trimming = StringTrimming.EllipsisCharacter,
							FormatFlags = StringFormatFlags.LineLimit | StringFormatFlags.NoWrap
						});
				}
			}
		}


		protected override void OnEnabledChanged(EventArgs e)
		{
			if (Image != null)
			{
				if (Enabled)
				{
					Image.Dispose();
					Image = enabledImage;
				}
				else
				{
					enabledImage = Image;
					Image = ((Bitmap)Image).ConvertToGrayscale();
				}
			}

			MouseState = MouseState.None;

			base.OnEnabledChanged(e);
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
