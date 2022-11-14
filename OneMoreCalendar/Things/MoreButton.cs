//************************************************************************************************
// Copyright © 2022 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using River.OneMoreAddIn;
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Windows.Forms;


	/// <summary>
	/// Custom Button
	/// </summary>
	/// <remarks>
	/// ArrowDirections
	/// "⏵" // \u23F5
	/// "⏶" // \u23F6
	/// "⏷" // \u23F7
	/// "⏴" // \u23F4
	/// </remarks>
	internal class MoreButton : Button
	{
		private const int Radius = 4;
		private MouseEventArgs downArgs = null;
		private Image enabledImage;
		private Timer timer;


		/// <summary>
		/// Initialize a new instance.
		/// </summary>
		public MoreButton()
			: base()
		{
			InitializeComponent();

			InitialDelay = 400;
			RepeatInterval = 62;

			// force Paint event to fire
			SetStyle(ControlStyles.UserPaint, true);
			// reduce flickering
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
		}

		private void InitializeComponent()
		{
			var components = new Container();
			timer = new Timer(components);
			SuspendLayout();
			timer.Tick += new EventHandler(Tick);
			ResumeLayout(false);
		}


		/// <summary>
		/// Pause before starting repeat
		/// </summary>
		[DefaultValue(400)]
		public int InitialDelay { set; get; }


		/// <summary>
		/// Gets the state indicating normal, hover, or pressed.
		/// </summary>
		public MouseState MouseState { get; private set; }


		/// <summary>
		/// Gets or sets the preferred background color
		/// </summary>
		public Color PreferredBack { get; set; } = Color.Empty;


		/// <summary>
		/// Gets or sets the preferred foreground color
		/// </summary>
		public Color PreferredFore { get; set; } = Color.Empty;


		/// <summary>
		/// Interval between repeat firings
		/// </summary>
		[DefaultValue(62)]
		public int RepeatInterval { set; get; }


		/// <summary>
		/// Gets or sets a value indicating whether or not to draw the button border even
		/// when in normal state
		/// </summary>
		public bool ShowBorder { get; set; }


		private ThemeProvider Theme => ThemeProvider.Instance;



		protected override void OnPaint(PaintEventArgs pevent)
		{
			var g = pevent.Graphics;
			g.Clear(PreferredBack.IsEmpty ? Theme.BackColor : PreferredBack);

			if (Enabled && MouseState != MouseState.None)
			{
				using var brush = new SolidBrush(Theme.ButtonHotBack);
				g.FillRoundedRectangle(brush, pevent.ClipRectangle, Radius);
			}

			if (ShowBorder || (Enabled && MouseState != MouseState.None))
			{
				using var pen = new Pen(
					MouseState.HasFlag(MouseState.Pushed) ? Theme.ButtonPressBorder : 
					MouseState.HasFlag(MouseState.Hover) ? Theme.ButtonHotBorder : Theme.ButtonBorder);

				g.DrawRoundedRectangle(pen, pevent.ClipRectangle, Radius);
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
				using var brush = new SolidBrush(Enabled 
					? PreferredFore.IsEmpty ? Theme.ButtonFore : PreferredFore
					: Theme.ButtonDisabled);

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
				timer.Enabled = false;
				downArgs = mevent;
				Tick(null, EventArgs.Empty);
			}
		}


		private void Tick(object sender, EventArgs e)
		{
			base.OnMouseDown(downArgs);
			timer.Interval = timer.Enabled ? RepeatInterval : InitialDelay;
			timer.Enabled = true;
		}


		protected override void OnMouseUp(MouseEventArgs mevent)
		{
			if (Enabled)
			{
				MouseState &= ~MouseState.Pushed;
				timer.Enabled = false;
				base.OnMouseUp(mevent);
			}
		}


		protected override void OnMouseLeave(EventArgs e)
		{
			if (Enabled)
			{
				MouseState &= ~MouseState.Hover;
				base.OnMouseLeave(e);
			}
		}


		protected override void OnMouseEnter(EventArgs e)
		{
			if (Enabled)
			{
				MouseState |= MouseState.Hover;
				base.OnMouseEnter(e);
			}
		}
	}
}
