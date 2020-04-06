//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;
	using System.Threading;
	using System.Windows.Forms;


	// based on
	// https://stackoverflow.com/questions/3654787/global-hotkey-in-console-application

	[Flags]
	internal enum KeyModifiers
	{
		Alt = 1,
		Control = 2,
		Shift = 4,
		Windows = 8,
		NoRepeat = 0x4000
	}

	internal class HotkeyEventArgs : EventArgs
	{
		public readonly Keys Key;
		public readonly KeyModifiers Modifiers;
		public readonly uint Value;

		public HotkeyEventArgs(Keys key, KeyModifiers modifiers)
		{
			this.Key = key;
			this.Modifiers = modifiers;
		}

		public HotkeyEventArgs(IntPtr hotKeyParam)
		{
			Value = (uint)hotKeyParam.ToInt64();
			Key = (Keys)((Value & 0xffff0000) >> 16);
			Modifiers = (KeyModifiers)(Value & 0x0000ffff);
		}
	}

	internal static class HotkeyManager
	{
		public static event EventHandler<HotkeyEventArgs> HotKeyPressed;

		delegate void RegisterHotKeyDelegate(IntPtr hwnd, int id, uint modifiers, uint key);
		delegate void UnRegisterHotKeyDelegate(IntPtr hwnd, int id);

		private class MessageWindow : Form
		{
			private const int WM_HOTKEY = 0x312;

			public MessageWindow()
			{
				window = this;
				handle = Handle;
				resetEvent.Set();
			}

			protected override void WndProc(ref Message m)
			{
				if (m.Msg == WM_HOTKEY)
				{
					var fg = GetForegroundWindow();
					_ = GetWindowThreadProcessId(fg, out var fpid);
					if (fpid == processId)
					{
						OnHotKeyPressed(new HotkeyEventArgs(m.LParam));
					}
					else
					{
						SendMessage(fg, m.Msg, (int)m.WParam, m.LParam);
					}
				}

				base.WndProc(ref m);
			}

			protected override void SetVisibleCore(bool value)
			{
				// Ensure the window never becomes visible
				base.SetVisibleCore(false);
			}
		}

		private static volatile MessageWindow window;
		private static volatile IntPtr handle;
		private static int processId;
		private static int counter = 0xE000;
		private static readonly List<int> registeredKeys = new List<int>();
		private static readonly ManualResetEvent resetEvent = new ManualResetEvent(false);

		static HotkeyManager()
		{
			using (var mgr = new ApplicationManager())
			{
				_ = GetWindowThreadProcessId(mgr.WindowHandle, out var pid);
				processId = (int)pid;
			}

			var messageLoop = new Thread(delegate () { Application.Run(new MessageWindow()); })
			{
				Name = $"{nameof(HotkeyManager)}Thread",
				IsBackground = true
			};

			messageLoop.Start();
		}

		public static ApplicationManager AppManager { get; set; }

		public static void RegisterHotKey(Keys key, KeyModifiers modifiers)
		{
			resetEvent.WaitOne();
			int id = Interlocked.Increment(ref counter);
			modifiers |= KeyModifiers.NoRepeat;
			window.Invoke(new RegisterHotKeyDelegate(RegisterHotKeyInternal), handle, id, (uint)modifiers, (uint)key);
			registeredKeys.Add(id);
		}

		public static void Unregister()
		{
			foreach (var id in registeredKeys)
			{
				window.Invoke(new UnRegisterHotKeyDelegate(UnRegisterHotKeyInternal), handle, id);
			}
		}

		private static void RegisterHotKeyInternal(IntPtr hwnd, int id, uint modifiers, uint key)
		{
			RegisterHotKey(hwnd, id, modifiers, key);
		}

		private static void UnRegisterHotKeyInternal(IntPtr hwnd, int id)
		{
			UnregisterHotKey(handle, id);
		}

		private static void OnHotKeyPressed(HotkeyEventArgs e)
		{
			HotKeyPressed?.Invoke(null, e);
		}

		[DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll", SetLastError = true)]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);

		[DllImport("user32", SetLastError = true)]
		private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

		[DllImport("user32", SetLastError = true)]
		private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
	}
}
