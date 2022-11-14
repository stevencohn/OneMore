//************************************************************************************************
// Copyright © 2022 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.ComponentModel;
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


		[Description("Round radio if true")]
		public bool Round { get; set; }


		private ThemeProvider Theme => ThemeProvider.Instance;


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
						? Theme.ButtonHotBack
						: Theme.ButtonBack);

					g.FillRoundedRectangle(brush, pevent.ClipRectangle, Radius);

					using var pen = new Pen(
						MouseState.HasFlag(MouseState.Pushed) || Checked
						? Theme.ButtonPressBorder
						: Theme.Border);

					g.DrawRoundedRectangle(pen, pevent.ClipRectangle, Radius);
				}

				g.DrawImageUnscaled(Image,
					(pevent.ClipRectangle.Width - Image.Width) / 2,
					(pevent.ClipRectangle.Height - Image.Height) / 2
					);
			}
			else
			{
				using (var pen = new Pen(Theme.Control))
				{
					if (Round)
					{
						g.DrawArc(pen, 0, 1, 14, 14, 0, 360);
					}
					else
					{
						g.DrawRectangle(pen, 0, 1, 14, 14);
					}
				}

				if (Checked)
				{
					using var brush = new SolidBrush(Theme.Control);
					if (Round)
					{
						g.FillEllipse(brush, new Rectangle(2, 3, 10, 10));
					}
					else
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
