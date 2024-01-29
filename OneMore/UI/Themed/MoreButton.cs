//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using River.OneMoreAddIn.Commands;
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Windows.Forms;


	/// <summary>
	/// Extension of Button to allow forced system Hand cursor instead of default Forms Hand cursor.
	/// </summary>
	internal class MoreButton : Button, IThemedControl
	{
		private const int Radius = 4;
		private IntPtr hcursor;
		private Image image;
		private Image imageOver;
		private Image enabledImage;
		private readonly ThemeManager manager;


		public MoreButton()
			: base()
		{
			//InitializeComponent();
			hcursor = IntPtr.Zero;

			// force Paint event to fire
			SetStyle(ControlStyles.UserPaint, true);
			// reduce flickering
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);

			manager = ThemeManager.Instance;
		}


		/// <summary>
		/// Gets the state indicating normal, hover, or pressed.
		/// </summary>
		public MouseState MouseState { get; private set; }


		/// <summary>
		/// Gets or sets the preferred background color
		/// </summary>
		public string PreferredBack { get; set; }


		/// <summary>
		/// Gets or sets the preferred foreground color
		/// </summary>
		public string PreferredFore { get; set; }


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
		public bool ShowBorder { get; set; } = true;


		/*
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
		*/


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
			}

			base.OnMouseDown(mevent);
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
			}

			base.OnMouseEnter(e);
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
			}

			base.OnMouseLeave(e);
		}


		protected override void OnMouseUp(MouseEventArgs mevent)
		{
			if (Enabled)
			{
				MouseState &= ~MouseState.Pushed;
			}

			base.OnMouseUp(mevent);
		}


		protected override void OnPaint(PaintEventArgs pevent)
		{
			var g = pevent.Graphics;
			g.Clear(string.IsNullOrEmpty(PreferredBack)
				? manager.ButtonBack
				: manager.GetThemedColor(PreferredBack));

			if (Enabled && MouseState != MouseState.None)
			{
				using var brush = new SolidBrush(manager.ButtonHotBack);
				g.FillRoundedRectangle(brush, pevent.ClipRectangle, Radius);
			}

			if (BackgroundImage != null)
			{
				g.DrawImageUnscaled(BackgroundImage,
					(pevent.ClipRectangle.Width - BackgroundImage.Width) / 2,
					(pevent.ClipRectangle.Height - BackgroundImage.Height) / 2);
				//g.DrawImage(BackgroundImage, 0, 0,
				//	pevent.ClipRectangle.Width, pevent.ClipRectangle.Height);
			}

			if (Image != null && !string.IsNullOrWhiteSpace(Text))
			{
				var size = g.MeasureString(Text, Font);
				var x = (pevent.ClipRectangle.Width - ((int)size.Width + Image.Width)) / 2;
				var y = (pevent.ClipRectangle.Height - Image.Height) / 2;

				g.DrawImageUnscaled(Image, x, y);
				PaintText(g, x + Image.Width, y);
			}
			else if (Image != null)
			{
				g.DrawImageUnscaled(Image,
					(pevent.ClipRectangle.Width - Image.Width) / 2,
					(pevent.ClipRectangle.Height - Image.Height) / 2);
			}
			else if (!string.IsNullOrEmpty(Text))
			{
				var size = g.MeasureString(Text, Font);

				PaintText(g,
					(int)((pevent.ClipRectangle.Width - size.Width) / 2),
					(int)((pevent.ClipRectangle.Height - size.Height) / 2));
			}

			if (ShowBorder || (Enabled && MouseState != MouseState.None))
			{
				var color = manager.ButtonBorder;
				if (MouseState.HasFlag(MouseState.Pushed))
				{
					color = manager.ButtonPressBorder;
				}
				else if (Focused || IsDefault) // || this == FindForm().AcceptButton)
				{
					color = manager.GetThemedColor("HotTrack");
				}
				else if (MouseState.HasFlag(MouseState.Hover))
				{
					color = manager.ButtonHotBorder;
				}

				using var pen = new Pen(color, 2);
				g.DrawRoundedRectangle(pen, pevent.ClipRectangle, Radius);
			}
		}


		private void PaintText(Graphics g, int x, int y)
		{
			using var brush = new SolidBrush(Enabled
				? string.IsNullOrWhiteSpace(PreferredFore)
					? manager.GetThemedColor("ControlText")
					: manager.GetThemedColor(PreferredFore)
				: manager.GetThemedColor("GrayText")
				);

			g.DrawString(Text, Font, brush, x, y,
				new StringFormat
				{
					Trimming = StringTrimming.EllipsisCharacter,
					FormatFlags = StringFormatFlags.LineLimit | StringFormatFlags.NoWrap
				});
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

					var editor = new ImageEditor { Size = new Size(16, 16) };
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
