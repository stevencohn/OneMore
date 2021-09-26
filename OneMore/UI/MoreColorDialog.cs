//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  Yada yada...
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Windows.Forms;
	using System.Xml.Linq;


	/// <summary>
	/// Customized ColorDialog that provides a means to set its screen location.
	/// </summary>

	internal class MoreColorDialog : ColorDialog
	{
		#region Win32
		private const Int32 WM_INITDIALOG = 0x0110;
		private const uint SWP_NOSIZE = 0x0001;
		private const uint SWP_SHOWWINDOW = 0x0040;
		private const uint SWP_NOZORDER = 0x0004;
		private const uint UFLAGS = SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW;

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern bool SetWindowText(IntPtr hWnd, string text);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetWindowPos(
			IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

		[DllImport("kernel32.dll")]
		static extern uint GetLastError();
		#endregion Win32

		private bool once;
		private readonly string title;
		private readonly int x;
		private readonly int y;

		public MoreColorDialog(string title, int x, int y)
			: base()
		{
			FullOpen = false;

			LoadCustomColors();

			once = true;
			this.title = title;
			this.x = x;
			this.y = y;
		}


		protected override IntPtr HookProc(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
		{
			// must call base HookProc before chaning window pos or SetWindowPos won't work
			var hook = base.HookProc(hWnd, msg, wparam, lparam);

			if ((msg == WM_INITDIALOG) && once)
			{
				SetWindowText(hWnd, title);
				SetWindowPos(hWnd, IntPtr.Zero, x, y, 0, 0, UFLAGS);
				once = false;
			}

			return hook;
		}


		private void LoadCustomColors()
		{
			var path = Path.Combine(PathFactory.GetAppDataPath(), Properties.Resources.CustomColorsFilesname);
			if (File.Exists(path))
			{
				var doc = XElement.Load(path, LoadOptions.None);
				var ns = doc.GetDefaultNamespace();

				var colors = doc.Elements(ns + "color").Select(e => e.Value);

				if (colors?.Count() > 0)
				{
					var list = new List<int>();

					foreach (var color in colors)
					{
						list.Add(int.Parse(color[0] == '#' ? color.Substring(1) : color, NumberStyles.HexNumber));
					}

					CustomColors = list.ToArray();
				}
			}
		}


		protected override void Dispose(bool disposing)
		{
			if (CustomColors?.Length > 0)
			{
				var doc = new XElement("CustomColors");
				foreach (int color in CustomColors)
				{
					if (!BasicColors.IsKnown(color))
					{
						var hex = color.ToString("X6");
						if (hex.Length > 6) hex = hex.Substring(hex.Length - 6);

						doc.Add(new XElement("color", "#" + hex));
					}
				}

				var path = PathFactory.GetAppDataPath();
				if (PathFactory.EnsurePathExists(path))
				{
					path = Path.Combine(path, Properties.Resources.CustomColorsFilesname);
					doc.Save(path, SaveOptions.None);
				}
			}

			base.Dispose(disposing);
		}
	}
}
