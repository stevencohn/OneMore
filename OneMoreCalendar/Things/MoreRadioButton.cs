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
	internal class MoreRadioButton : RadioButton
	{
		private const int Radius = 4;


		/// <summary>
		/// Initialize a new instance.
		/// </summary>
		public MoreRadioButton()
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

			if (Appearance == Appearance.Button)
			{
				if (Enabled && (MouseState != MouseState.None || Checked))
				{
					var brush = MouseState.HasFlag(MouseState.Pushed) || Checked
						? AppColors.PressedBrush
						: AppColors.HoverBrush;

					g.FillRoundedRectangle(brush, pevent.ClipRectangle, Radius);

					var pen = MouseState.HasFlag(MouseState.Pushed) || Checked
						? AppColors.PressedPen
						: AppColors.HoverPen;

					g.DrawRoundedRectangle(pen, pevent.ClipRectangle, Radius);
				}

				g.DrawImageUnscaled(Image,
					(pevent.ClipRectangle.Width - Image.Width) / 2,
					(pevent.ClipRectangle.Height - Image.Height) / 2
					);
			}
			else
			{
				using (var pen = new Pen(AppColors.ControlColor))
				{
					g.DrawRectangle(pen, 0, 1, 14, 14);
				}

				if (Checked)
				{
					using (var brush = new SolidBrush(AppColors.ControlColor))
					{
						g.FillRectangle(brush, 2, 3, 11, 11);
					}
				}

				var size = g.MeasureString(Text, Font);
				using (var brush = new SolidBrush(ForeColor))
				{
					g.DrawString(Text, Font, brush,
						new Rectangle(16, // standard icon size
							(pevent.ClipRectangle.Height - (int)size.Height) / 2,
							pevent.ClipRectangle.Width - 16,
							(int)size.Height),
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
			if (Appearance == Appearance.Button)
			{
				if (Enabled)
				{
					Image.Dispose();
					Image = Properties.Resources.settings_32;
				}
				else
				{
					Image.Dispose();
					Image = Properties.Resources.settings_32.ConvertToGrayscale();
				}
			}

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
