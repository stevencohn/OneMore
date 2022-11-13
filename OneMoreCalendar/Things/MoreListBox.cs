//************************************************************************************************
// Copyright © 2022 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

#pragma warning disable S1144 // Unused private types or members should be removed
#pragma warning disable S4487 // Unread "private" fields should be removed
#pragma warning disable S3459 // Unassigned members should be removed

namespace OneMoreCalendar
{
	using River.OneMoreAddIn;
	using System;
	using System.ComponentModel;
	using System.Runtime.InteropServices;
	using System.Windows.Forms;


	internal class MoreListBox : ListBox
	{
		private const int WM_VSCROLL = 0x115;
		private const int SB_THUMBPOSITION = 4;
		private const int SB_ENDSCROLL = 8;
		private const int SB_VERT = 0x1;
		private const int SIF_TRACKPOS = 0x10;
		private const int SIF_RANGE = 0x1;
		private const int SIF_POS = 0x4;
		private const int SIF_PAGE = 0x2;
		private const int SIF_ALL = SIF_RANGE | SIF_PAGE | SIF_POS | SIF_TRACKPOS;

		private struct ScrollInfo
		{
			public int Size;
			public int Mask;
			public int Min;
			public int Max;
			public int Page;            // unused but do not remove, S1144
			public int Position;
			public int TrackPosition;   // unused but do not remove, S1144
		}

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetScrollInfo(IntPtr hwnd, int fnBar, ref ScrollInfo lpsi);

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);

		[Category("Action")]
		public event ScrollEventHandler Scrolled = null;


		public int GetScrollOffset()
		{
			var offset = 0;
			for (int i = 0; i < TopIndex; i++)
			{
				offset += GetItemHeight(i);
			}

			return offset;
		}

		public bool ScrollDown()
		{
			return Scroll(1);
		}

		public bool ScrollUp()
		{
			return Scroll(-1);
		}

		private bool Scroll(int delta)
		{
			var info = GetScrollInfo();
			var pos = info.Position + delta;
			if (pos >= info.Min && pos <= info.Max)
			{
				var param = ((uint)pos << 16) | (SB_THUMBPOSITION & 0xffff);
				SendMessage(Handle, WM_VSCROLL, param, 0);

				return true;
			}

			return false;
		}


		private ScrollInfo GetScrollInfo()
		{
			var info = new ScrollInfo
			{
				Mask = SIF_ALL
			};

			info.Size = Marshal.SizeOf(info);

			GetScrollInfo(Handle, SB_VERT, ref info);
			return info;
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			//var info = GetScrollInfo();
			//Logger.Current.WriteLine($"wheel delta:{e.Delta} pos:{info.Position} min:{info.Min} max:{info.Max}");
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WM_VSCROLL)
			{
				if (Scrolled != null)
				{
					var info = GetScrollInfo();
					//Logger.Current.WriteLine($"wndproc pos:{info.Position} min:{info.Min} max:{info.Max}");

					if (m.WParam.ToInt32() == SB_ENDSCROLL)
					{
						Scrolled(this, new ScrollEventArgs(
							ScrollEventType.EndScroll,
							info.Position,
							ScrollOrientation.VerticalScroll));
					}
				}
			}

			base.WndProc(ref m);
		}
	}
}
