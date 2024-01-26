//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using River.OneMoreAddIn.Commands;
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Windows.Forms;


	[Flags]
	internal enum MouseState
	{
		None = 0,
		Hover = 1,
		Pushed = 2
	}


	/// <summary>
	/// Extension of Button to allow forced system Hand cursor instead of default Forms Hand cursor.
	/// </summary>
	internal class MoreButton : Button
	{
		private const int Radius = 4;
		private IntPtr hcursor;
		private Image image;
		private Image imageOver;
		private Image enabledImage;
		private MouseEventArgs downArgs = null;


		public MoreButton()
			: base()
		{
			//InitializeComponent();
			hcursor = IntPtr.Zero;

			// force Paint event to fire
			SetStyle(ControlStyles.UserPaint, true);
			// reduce flickering
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
		}


		//private void InitializeComponent()
		//{
		//	var components = new Container();
		//	timer = new Timer(components);
		//	SuspendLayout();
		//	timer.Tick += new EventHandler(Tick);
		//	ResumeLayout(false);
		//}


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


		[Description("Specifies the image to show when the mouse is over the button")]
		public Image ImageOver
		{
			get;
			set;
		}


		/// <summary>
		/// Gets or sets a value indicating whether or not to draw the button border even
		/// when in normal state
		/// </summary>
		public bool ShowBorder { get; set; }


		private ThemeManager Theme => ThemeManager.Instance;


		protected override void OnClientSizeChanged(EventArgs e)
		{
			base.OnClientSizeChanged(e);

			if (Image == null && ImageOver == null)
			{
				return;
			}

			var size = new Size(
				ClientSize.Width - Padding.Left - Padding.Right - 5,
				ClientSize.Height - Padding.Top - Padding.Bottom - 5);

			if (Image != null)
			{
				Image = image = new Bitmap(Image, size);
			}

			if (ImageOver != null)
			{
				imageOver = new Bitmap(ImageOver, size);
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
					var editor = new ImageEditor { Style = ImageEditor.Stylization.GrayScale };
					Image = editor.Apply(Image);
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
				downArgs = mevent;
			}
		}


		protected override void OnMouseEnter(EventArgs e)
		{
			if (Enabled)
			{
				if (imageOver != null)
				{
					Image = imageOver;
				}

				MouseState |= MouseState.Hover;
				base.OnMouseEnter(e);
			}
		}


		protected override void OnMouseLeave(EventArgs e)
		{
			if (Enabled)
			{
				if (image != null)
				{
					Image = image;
				}

				MouseState &= ~MouseState.Hover;
				base.OnMouseLeave(e);
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


		public void Rescale()
		{
			// special-case handling for 96 DPI monitors
			(float dpiX, _) = UIHelper.GetDpiValues();
			if (Math.Floor(dpiX) == 96)
			{
				if (BackgroundImage != null)
				{
					using var img = BackgroundImage;

					var editor = new Commands.ImageEditor { Size = new Size(16, 16) };
					BackgroundImage = editor.Apply(img);
				}
			}
		}


		public void SetHandCursor()
		{
			Cursor = Cursors.Hand;
			hcursor = Native.LoadCursor(IntPtr.Zero, Native.IDC_HAND);
		}


		protected override void WndProc(ref Message m)
		{
			if (hcursor != IntPtr.Zero &&
				m.Msg == Native.WM_SETCURSOR && hcursor != IntPtr.Zero)
			{
				Native.SetCursor(hcursor);
				m.Result = IntPtr.Zero; // indicate handled
				return;
			}

			base.WndProc(ref m);
		}
	}
}
