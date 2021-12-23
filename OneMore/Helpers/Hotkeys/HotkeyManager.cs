//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CA1810 // Initialize reference type static fields inline
#pragma warning disable S3010 // Static fields should not be updated in constructors

namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Threading;
	using System.Windows.Forms;


	// based on
	// https://stackoverflow.com/questions/3654787/global-hotkey-in-console-application


	/// <summary>
	/// Maintains a set of global hotkeys that remain active only while OneNote is the
	/// active application.
	/// </summary>
	/// <remarks>
	/// Switching global hotkeys on/off based on which application is active is accomplished
	/// using a Windows event hook.
	/// <para>
	/// This approach is needed because OneMore runs as a .NET interop module and is limited
	/// in what it can do to attach to, intercept messages and events, or inject handlers into
	/// OneNote's unmanaged context.
	/// </para>
	/// </remarks>
	internal static class HotkeyManager
	{
		private delegate void RegisterHotkeyDelegate(IntPtr hwnd, int id, uint modifiers, uint key);
		private delegate void UnRegisterHotkeyDelegate(IntPtr hwnd, int id);

		private static readonly List<Hotkey> registeredKeys = new List<Hotkey>();
		private static readonly ManualResetEvent resetEvent = new ManualResetEvent(false);
		private static volatile MessageWindow window;
		private static volatile IntPtr handle;
		private static uint threadId;
		private static bool registered = false;
		private static int counter = 0xE000;
		private static GCHandle gch;


		/// <summary>
		/// An event handler for consumers
		/// </summary>
		public static event EventHandler<HotkeyEventArgs> HotKeyPressed;


		/// <summary>
		/// Initializes the background message pump used to filter our own registered key sequences
		/// </summary>
		public static void Initialize()
		{
			using (var one = new OneNote())
			{
				threadId = Native.GetWindowThreadProcessId(one.WindowHandle, out _);
			}

			var mthread = new Thread(delegate () { Application.Run(new MessageWindow()); })
			{
				Name = $"{nameof(HotkeyManager)}Thread",
				IsBackground = true
			};

			mthread.Start();
		}


		/// <summary>
		/// Registers a new global hotkey
		/// </summary>
		/// <param name="key">The primary key code</param>
		/// <param name="modifiers">The key modifiers such as Ctrl, Shift, and Alt</param>
		public static void RegisterHotKey(Keys key, Hotmods modifiers = 0)
		{
			resetEvent.WaitOne();

			int keyId = Interlocked.Increment(ref counter);
			modifiers |= Hotmods.NoRepeat;

			window.Invoke(
				new RegisterHotkeyDelegate(Register),
				handle, keyId, (uint)modifiers, (uint)key);

			registeredKeys.Add(new Hotkey
			{
				Id = keyId,
				Key = (uint)key,
				Modifiers = (uint)modifiers
			});

			registered = true;
		}


		/// <summary>
		/// Registers a new global hotkey bound to the given action.
		/// </summary>
		/// <param name="action">The action to invoke when the hotkey is pressed</param>
		/// <param name="key">They key identifier</param>
		/// <param name="modifiers">The key modifiers, if any</param>
		public static void RegisterHotKey(Action action, Keys key, Hotmods modifiers = 0)
		{
			resetEvent.WaitOne();

			int keyId = Interlocked.Increment(ref counter);
			modifiers |= Hotmods.NoRepeat;

			window.Invoke(
				new RegisterHotkeyDelegate(Register),
				handle, keyId, (uint)modifiers, (uint)key);

			registeredKeys.Add(new Hotkey
			{
				Id = keyId,
				Key = (uint)key,
				Modifiers = (uint)modifiers,
				Action = action
			});

			registered = true;
		}


		// runs as a delegated routine within the context of MessageWindow
		private static void Register(IntPtr hwnd, int id, uint modifiers, uint key)
		{
			Native.RegisterHotKey(hwnd, id, modifiers, key);
		}


		/// <summary>
		/// Unregisters all hotkeys; used for OneNote shutdown
		/// </summary>
		public static void Unregister()
		{
			registeredKeys.ForEach(k =>
				window.Invoke(new UnRegisterHotkeyDelegate(Unregister), handle, k.Id));

			// may not be allocated if the add-in startup has failed
			if (gch.IsAllocated)
			{
				gch.Free();
			}
		}


		// runs as a delegated routine within the context of MessageWindow
		private static void Unregister(IntPtr hwnd, int id)
		{
			Native.UnregisterHotKey(handle, id);
		}


		// Invoked from MessageWindow to propagate event to consumer's handler
		private static void OnHotKeyPressed(HotkeyEventArgs e)
		{
			//Logger.Current.WriteLine($"keypress key:{e.Key} mods:{e.Modifiers}");

			var key = registeredKeys
				.FirstOrDefault(k => 
					k.Key == (uint)e.Key && 
					k.Modifiers == (uint)(e.Modifiers|Hotmods.NoRepeat));

			if (key != null)
			{
				if (key.Action != null)
				{
					key.Action();
				}
				else
				{
					HotKeyPressed?.Invoke(null, e);
				}
			}
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
		// Private message interceptor

		private sealed class MessageWindow : Form
		{
			private readonly uint msgThreadId;

			public MessageWindow()
			{
				window = this;
				handle = Handle;

				// thread of MessageWindow would be a separate dllhost.exe
				// process started by the OneNote process (with SysWOW64 in its command line)
				msgThreadId = Native.GetWindowThreadProcessId(handle, out _);

				// maintain a ref so GC doesn't remove it and cause exceptions
				var evDelegate = new Native.WinEventDelegate(WinEventProc);
				gch = GCHandle.Alloc(evDelegate);

				// set up event hook to monitor switching application
				Native.SetWinEventHook(
					Native.EVENT_SYSTEM_FOREGROUND,
					Native.EVENT_SYSTEM_MINIMIZEEND,
					IntPtr.Zero,
					evDelegate,
					0, 0, Native.WINEVENT_OUTOFCONTEXT | Native.WINEVENT_SKIPOWNTHREAD);

				resetEvent.Set();
			}


			private void WinEventProc(
				IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
				int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
			{
				//Logger.Current.WriteLine($"hotkey event:{eventType} thread:{dwEventThread}");

				if (eventType == Native.EVENT_SYSTEM_FOREGROUND ||
					eventType == Native.EVENT_SYSTEM_MINIMIZESTART ||
					eventType == Native.EVENT_SYSTEM_MINIMIZEEND)
				{
					// threadId is the OneNote.exe process main UI thread
					// msgThreadId is the dllhost.exe process thread hosting this MessageWindow
					// Both are needed because threadId will be current when switching back to
					// OneNote.exe from another app; while msgThreadId will be current when
					// opening a OneMore dialog such as "Search and Replace"

					if ((dwEventThread == threadId) || (dwEventThread == msgThreadId))
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
					// check if this is the main OneNote.exe thread and not a dllhost.exe thread
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
