//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CA1810 // Initialize reference type static fields inline

namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using System.Windows.Forms;


	// based on
	// https://stackoverflow.com/questions/3654787/global-hotkey-in-console-application

	/// <summary>
	/// 
	/// </summary>
	internal static class HotkeyManager
	{
		private delegate void RegisterHotKeyDelegate(IntPtr hwnd, int id, uint modifiers, uint key);
		private delegate void UnRegisterHotKeyDelegate(IntPtr hwnd, int id);

		private static volatile MessageWindow window;
		private static volatile IntPtr handle;
		private static readonly uint threadId;
		private static bool registered = false;
		private static int counter = 0xE000;
		private static readonly List<Hotkey> registeredKeys = new List<Hotkey>();
		private static readonly ManualResetEvent resetEvent = new ManualResetEvent(false);

		static HotkeyManager()
		{
			using (var mgr = new ApplicationManager())
			{
				threadId = Native.GetWindowThreadProcessId(mgr.WindowHandle, out _);
			}

			var messageLoop = new Thread(delegate () { Application.Run(new MessageWindow()); })
			{
				Name = $"{nameof(HotkeyManager)}Thread",
				IsBackground = true
			};

			messageLoop.Start();
		}


		/// <summary>
		/// 
		/// </summary>
		public static event EventHandler<HotkeyEventArgs> HotKeyPressed;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="modifiers"></param>
		public static void RegisterHotKey(Keys key, Hotmods modifiers = 0)
		{
			resetEvent.WaitOne();
			int id = Interlocked.Increment(ref counter);
			modifiers |= Hotmods.NoRepeat;

			window.Invoke(
				new RegisterHotKeyDelegate(Register),
				handle, id, (uint)modifiers, (uint)key);

			registeredKeys.Add(new Hotkey
			{
				Id = id,
				Key = (uint)key,
				Modifiers = (uint)modifiers
			});

			registered = true;
		}


		private static void Register(IntPtr hwnd, int id, uint modifiers, uint key)
		{
			Native.RegisterHotKey(hwnd, id, modifiers, key);
		}


		/// <summary>
		/// 
		/// </summary>
		public static void Unregister()
		{
			registeredKeys.ForEach(k =>
				window.Invoke(new UnRegisterHotKeyDelegate(Unregister), handle, k.Id));
		}


		private static void Unregister(IntPtr hwnd, int id)
		{
			Native.UnregisterHotKey(handle, id);
		}


		private static void OnHotKeyPressed(HotkeyEventArgs e)
		{
			HotKeyPressed?.Invoke(null, e);
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		private class MessageWindow : Form
		{
			private readonly uint winThreadId;

			public MessageWindow()
			{
				window = this;
				handle = Handle;

				winThreadId = Native.GetWindowThreadProcessId(handle, out _);

				_ = Native.SetWinEventHook(
					Native.EVENT_SYSTEM_FOREGROUND,
					Native.EVENT_SYSTEM_MINIMIZEEND,
					IntPtr.Zero,
					new Native.WinEventDelegate(WinEventProc),
					0, 0, Native.WINEVENT_OUTOFCONTEXT);

				resetEvent.Set();
			}

			private void WinEventProc(
				IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
				int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
			{
				if (eventType == Native.EVENT_SYSTEM_FOREGROUND ||
					eventType == Native.EVENT_SYSTEM_MINIMIZESTART ||
					eventType == Native.EVENT_SYSTEM_MINIMIZEEND)
				{
					if ((dwEventThread == threadId) || (dwEventThread == winThreadId))
					{
						if (!registered && registeredKeys.Count > 0)
						{
							//Logger.Current.WriteLine("hotkey re-registering");
							registeredKeys.ForEach(k =>
								Native.RegisterHotKey(handle, k.Id, k.Modifiers, k.Key));

							registered = true;
						}
					}
					else
					{
						if (registered && registeredKeys.Count > 0)
						{
							//Logger.Current.WriteLine("hotkey uregistering");
							registeredKeys.ForEach(k =>
								Native.UnregisterHotKey(handle, k.Id));

							registered = false;
						}
					}
				}
			}

			protected override void WndProc(ref Message m)
			{
				if (m.Msg == Native.WM_HOTKEY)
				{
					var tid = Native.GetWindowThreadProcessId(Native.GetForegroundWindow(), out _);
					if (tid == threadId)
					{
						OnHotKeyPressed(new HotkeyEventArgs(m.LParam));
					}
				}

				base.WndProc(ref m);
			}

			protected override void SetVisibleCore(bool value)
			{
				// ensure the window never becomes visible
				base.SetVisibleCore(false);
			}
		}
	}
}
