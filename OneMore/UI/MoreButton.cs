//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S3881 // "IDisposable" should be implemented correctly

namespace River.OneMoreAddIn.UI
{
	using River.OneMoreAddIn.Commands;
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Windows.Forms;


	/// <summary>
	/// Extension of Button to allow forced system Hand cursor instead of
	/// default Forms Hand cursor.
	/// </summary>
	internal class MoreButton : Button, ILoadControl
	{
		private const int Radius = 4;
		private IntPtr hcursor;
		private Image image;
		private Image enabledImage;
		private Image grayImage;
		private readonly ThemeManager manager;
		private readonly Color backColor;
		private readonly Color foreColor;
		private readonly Color downColor;
		private readonly Color hoverColor;


		public MoreButton()
			: base()
		{
			//InitializeComponent();
			hcursor = IntPtr.Zero;

			SetStyle(
				ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
				ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.CacheText,
				true);

			manager = ThemeManager.Instance;

			// default value, maybe overriden by ILoadControl.Onload
			BackColor = backColor = manager.GetColor("ButtonFace");
			ForeColor = foreColor = manager.GetColor("ControlText");
			downColor = manager.GetColor("ButtonHighlight");
			hoverColor = manager.GetColor("ButtonHighlight");
		}


		protected override void Dispose(bool disposing)
		{
			image?.Dispose();
			ImageOver?.Dispose();
			enabledImage?.Dispose();
			grayImage?.Dispose();
		}


		/// <summary>
		/// Gets the state indicating normal, hover, or pressed.
		/// </summary>
		public MouseState MouseState { get; private set; }


		public string ThemedBack { get; set; }


		public string ThemedFore { get; set; }


		[Category("More"),
		Description("Specifies the image to show when the mouse is over the button")]
		public Image ImageOver
		{
			get;
			set;
		}


		[Category("More"),
		Description("Indicate that the Image should be set for dark mode and resized")]
		public bool StylizeImage { get; set; }


		/// <summary>
		/// Gets or sets a value indicating whether or not to draw the button border even
		/// when in normal state
		/// </summary>
		public bool ShowBorder { get; set; } = true;


		void ILoadControl.OnLoad()
		{
			BackColor = manager.GetColor("ButtonFace", ThemedBack);
			ForeColor = Enabled
				? manager.GetColor("ControlText", ThemedFore)
				: manager.GetColor("GrayText");

			if (StylizeImage)
			{
				var editor = new ImageEditor { Size = new Size(16, 16) };
				if (manager.DarkMode)
				{
					editor.Style = ImageEditor.Stylization.Invert;
				}

				using var img = Image;
				Image = editor.Apply(img);
			}
		}


		protected override void OnEnabledChanged(EventArgs e)
		{
			if (Image != null)
			{
				// this should work whether the button starts out enabled or disabled
				if (enabledImage == null || grayImage == null)
				{
					enabledImage = Image;
					grayImage = manager.GetGrayImage(Image);
				}

				Image = Enabled ? enabledImage : grayImage;
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
				if (ImageOver != null)
				{
					image = Image;
					Image = ImageOver;
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
			g.SmoothingMode = SmoothingMode.HighQuality;

			g.Clear(Parent.BackColor);

			var back = string.IsNullOrEmpty(ThemedBack)
				? backColor
				: manager.GetColor(ThemedBack);

			if (Enabled)
			{
				if (MouseState.HasFlag(MouseState.Hover))
					back = hoverColor;
				else if (MouseState.HasFlag(MouseState.Pushed))
					back = downColor;
			}

			var clip = pevent.ClipRectangle;

			using var brush = new SolidBrush(back);
			g.FillRoundedRectangle(brush, clip, Radius);

			if (BackgroundImage != null)
			{
				PaintBackgroundImage(g, clip);
			}

			if (Image != null && !string.IsNullOrWhiteSpace(Text))
			{
				var size = g.MeasureString(Text, Font);
				var x = (clip.Width - ((int)size.Width + Image.Width)) / 2;
				var y = (clip.Height - Image.Height) / 2;

				g.DrawImage(Image, x, y);
				PaintText(g, x + Image.Width, y);
			}
			else if (Image != null)
			{
				g.DrawImage(Image,
					(clip.Width - Image.Width) / 2,
					(clip.Height - Image.Height) / 2);
			}
			else if (!string.IsNullOrEmpty(Text))
			{
				var size = g.MeasureString(Text, Font);

				PaintText(g,
					(int)((clip.Width - size.Width) / 2),
					(int)((clip.Height - size.Height) / 2));
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
					color = manager.GetColor("HotTrack");
				}
				else if (MouseState.HasFlag(MouseState.Hover))
				{
					color = manager.ButtonHotBorder;
				}

				using var pen = new Pen(color);
				g.DrawRoundedRectangle(pen,
					new Rectangle(clip.X, clip.Y, clip.Width - 1, clip.Height - 1),
					Radius);
			}
		}


		private void PaintBackgroundImage(Graphics g, Rectangle clip)
		{
			static Image Scale(Image image, Rectangle clip)
			{
				var hscale = (float)clip.Height / image.Height;
				var wscale = (float)clip.Width / image.Width;
				var scale = Math.Min(hscale, wscale);
				var size = new SizeF(image.Width * scale, image.Height * scale).ToSize();
				return new ImageEditor { Size = size }.Apply(image);
			}

			if (BackgroundImageLayout == ImageLayout.Stretch &&
				(BackgroundImage.Width != clip.Width ||
				BackgroundImage.Height != clip.Height))
			{
				using var img = BackgroundImage;
				BackgroundImage = Scale(BackgroundImage, clip);
			}
			else
			{
				if (BackgroundImage.Width > clip.Width ||
					BackgroundImage.Height > clip.Height)
				{
					using var img = BackgroundImage;
					BackgroundImage = Scale(BackgroundImage, clip);
				}
			}

			g.DrawImage(BackgroundImage, clip);
		}


		private void PaintText(Graphics g, int x, int y)
		{
			using var brush = new SolidBrush(Enabled
				? string.IsNullOrWhiteSpace(ThemedFore)
					? foreColor
					: manager.GetColor(ThemedFore)
				: manager.GetColor("GrayText")
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
			(float dpiX, _) = UI.Scaling.GetDpiValues();
			if (Math.Floor(dpiX).EstEquals(96.0))
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
