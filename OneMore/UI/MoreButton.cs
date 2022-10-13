//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Management.Instrumentation;
	using System.Windows.Forms;


	/// <summary>
	/// Extension of Button to allow forced system Hand cursor instead of default Forms Hand cursor.
	/// </summary>
	internal class MoreButton : Button
	{
		private IntPtr hcursor;
		private Image image;
		private Image imageOver;


		public MoreButton()
			: base()
		{
			hcursor = IntPtr.Zero;
		}


		[Description("Specifies the image to show when the mouse is over the button")]
		public Image ImageOver
		{
			get;
			set;
		}


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


		protected override void OnMouseEnter(EventArgs e)
		{
			if (imageOver != null)
			{
				Image = imageOver;
			}

			base.OnMouseEnter(e);
		}


		protected override void OnMouseLeave(EventArgs e)
		{
			if (image != null)
			{
				Image = image;
			}

			base.OnMouseLeave(e);
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
