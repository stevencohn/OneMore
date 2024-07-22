//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using River.OneMoreAddIn.Commands;
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Linq;
	using System.Windows.Forms;


	internal class MoreCheckBox : CheckBox, ILoadControl
	{
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
			SetStyle(
				ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
				ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.CacheText,
				true);

			// force Hand cursor
			Cursor = Cursors.Hand;
			hcursor = Native.LoadCursor(IntPtr.Zero, Native.IDC_HAND);

			boxSize = SystemInformation.MenuCheckSize.Width - 3;

			manager = ThemeManager.Instance;
			BackColor = backColor = manager.GetColor("ButtonFace");
			ForeColor = foreColor = manager.GetColor("ControlText");
			hoverColor = manager.GetColor("ButtonHighlight");
		}


		/// <summary>
		/// Gets the state indicating normal, hover, or pressed.
		/// </summary>
		public MouseState MouseState { get; private set; }


		[Category("More"),
		 Description("Indicate that the Image should be set for dark mode and resized")]
		public bool StylizeImage { get; set; }


		public string ThemedBack { get; set; }


		public string ThemedFore { get; set; }



		void ILoadControl.OnLoad()
		{
			BackColor = manager.GetColor("ButtonFace", ThemedBack);
			ForeColor = Enabled
				? manager.GetColor("ControlText", ThemedFore)
				: manager.GetColor("GrayText");

			if (StylizeImage)
			{
				if (Image is not null)
				{
					var editor = new ImageEditor { Size = new Size(16, 16) };
					if (manager.DarkMode)
					{
						editor.Style = ImageEditor.Stylization.Invert;
					}

					using var img = Image;
					Image = editor.Apply(img);
				}

				if (BackgroundImage is not null)
				{
					var editor = new ImageEditor { Size = new Size(16, 16) };
					if (manager.DarkMode)
					{
						editor.Style = ImageEditor.Stylization.Invert;
					}

					using var img = BackgroundImage;
					BackgroundImage = editor.Apply(img);
				}
			}
		}


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
			var radius = g.DpiX == 96f ? 2 : 4;

			if (Enabled && (MouseState != MouseState.None || Checked))
			{
				using var brush = new SolidBrush(
					Checked
						? manager.GetColor("ButtonDown")
						: MouseState.HasFlag(MouseState.Hover) ? hoverColor : backColor);

				g.FillRoundedRectangle(brush, clip, radius);
			}

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
				var x = (clip.Width - Image.Width) / 2;
				var y = (clip.Height - Image.Height) / 2;
				g.DrawImage(Image, x, y);
			}
			else if (!string.IsNullOrEmpty(Text))
			{
				var size = g.MeasureString(Text, Font);

				PaintText(g,
					(int)((clip.Width - size.Width) / 2),
					(int)((clip.Height - size.Height) / 2));
			}

			var color = manager.ButtonBorder;
			if (MouseState.HasFlag(MouseState.Pushed))
			{
				color = manager.ButtonPressBorder;
			}
			else if (MouseState.HasFlag(MouseState.Hover))
			{
				color = manager.ButtonHotBorder;
			}
			else if (Checked)
			{
				color = manager.GetColor("MenuItemBorder");
			}

			using var pen = new Pen(color);
			g.DrawRoundedRectangle(pen,
				new Rectangle(clip.X, clip.Y, clip.Width - 1, clip.Height - 1),
				radius);
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

			if (!Padding.Equals(Padding.Empty))
			{
				clip = new Rectangle(
					clip.X + Padding.Left,
					clip.Y + Padding.Right,
					clip.Width - (Padding.Left + Padding.Right),
					clip.Height - (Padding.Bottom + Padding.Top));
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


		private void PaintNormal(PaintEventArgs pevent)
		{
			var g = pevent.Graphics;

			var color = Enabled
				? manager.GetColor(foreColor)
				: manager.GetColor("GrayText");

			var boxColor = Enabled ? manager.GetColor("Highlight") : color;

			var boxY = (Size.Height - boxSize) / 2;

			using var boxPen = new Pen(boxColor);
			var radius = g.DpiX == 96 ? 2 : 4;
			g.DrawRoundedRectangle(boxPen, new Rectangle(0, boxY, boxSize, boxSize), radius);

			if (Checked)
			{
				using var fillBrush = new SolidBrush(boxColor);
				g.FillRoundedRectangle(fillBrush,
					new Rectangle(2, boxY + 2, boxSize - 4, boxSize - 4), radius);
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
					Trimming = StringTrimming.None,
					FormatFlags = StringFormatFlags.NoWrap
				});
		}


		protected override void OnTextChanged(EventArgs e)
		{
			if (Appearance == Appearance.Normal)
			{
				var size = SystemInformation.MenuCheckSize;
				if (string.IsNullOrWhiteSpace(Text) &&
					(Width < size.Width || Height < size.Height))
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
				var h = Math.Max(size.Height, SystemInformation.MenuCheckSize.Height);
				if (Width < w || Height < h)
				{
					AutoSize = false;
					Width = w;
					Height = h;
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
