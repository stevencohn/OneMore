//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CA1810 // Initialize reference type static fields inline
#pragma warning disable S3010 // Static fields should not be updated in constructors

namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Threading;
	using System.Threading.Tasks;
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

		private static readonly List<Hotkey> registeredKeys = new();
		private static readonly ManualResetEvent resetEvent = new(false);

		private static volatile MessageWindow mwindow;  // message window
		private static volatile IntPtr mhandle;         // message window handle
		private static GCHandle mroot;                  // rooted handle to message window

		private static uint oneNotePID;                 // onenote process ID
		private static uint selfPID;                    // this add-in's own (dllhost) process ID

		private static bool registered = false;


		/// <summary>
		/// An event handler for consumers
		/// </summary>
		public static event EventHandler<HotkeyEventArgs> HotKeyPressed;


		/// <summary>
		/// Initializes the background message pump used to filter our own registered key sequences
		/// </summary>
		public static async Task Initialize()
		{
			await using var one = new OneNote();
			Native.GetWindowThreadProcessId(one.WindowHandle, out oneNotePID);
			selfPID = (uint)Process.GetCurrentProcess().Id;

			var mthread = new Thread(delegate () { Application.Run(new MessageWindow()); })
			{
				Name = $"{nameof(HotkeyManager)}Thread",
				IsBackground = true
			};

			mthread.Start();
		}


		/// <summary>
		/// Marshals the given action onto this manager's own persistent message-pump
		/// thread, which keeps an Application.Run loop alive for the lifetime of the
		/// process. Commands normally run inside CommandFactory.RunCore's Task.Run, i.e.
		/// on a throwaway threadpool thread with no message loop of its own, which forces
		/// MoreForm.RunModeless into a comparatively slow/blocking nested Application.Run
		/// every single time it shows a form. Invoking the show step through here instead
		/// lets RunModeless see an existing, already-running message loop and take its
		/// lightweight, non-blocking Show() path.
		/// </summary>
		/// <param name="action">The action to run on the message-pump thread</param>
		public static void InvokeOnMessageThread(Action action)
		{
			resetEvent.WaitOne();
			mwindow.Invoke(action);
		}


		/// <summary>
		/// Registers a new global hotkey bound to the given action.
		/// </summary>
		/// <param name="action">The action to invoke when the hotkey is pressed</param>
		/// <param name="hotkey">The Hotkey specifying the Key and Modifiers</param>
		public static void RegisterHotKey(Action action, Hotkey hotkey)
		{
			resetEvent.WaitOne();

			var modifiers = hotkey.HotModifiers | (uint)HotModifier.NoRepeat;

			mwindow.Invoke(
				new RegisterHotkeyDelegate(Register),
				mhandle, hotkey.Id, modifiers, hotkey.Key);

			hotkey.Action = action;
			hotkey.HotModifiers = modifiers;

			registeredKeys.Add(hotkey);

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
				mwindow.Invoke(new UnRegisterHotkeyDelegate(Unregister), mhandle, k.Id));

			// may not be allocated if the add-in startup has failed
			if (mroot.IsAllocated)
			{
				mroot.Free();
			}
		}


		// runs as a delegated routine within the context of MessageWindow
		private static void Unregister(IntPtr hwnd, int id)
		{
			Native.UnregisterHotKey(mhandle, id);
		}


		// Invoked from MessageWindow to propagate event to consumer's handler
		private static void OnHotKeyPressed(HotkeyEventArgs e)
		{
			//Logger.Current.WriteLine($"keypress key:{e.Key} mods:{e.Modifiers}");

			var key = registeredKeys
				.FirstOrDefault(k =>
					k.Key == (uint)e.Key &&
					k.HotModifiers == (uint)(e.HotModifiers | HotModifier.NoRepeat));

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
			public MessageWindow()
			{
				mwindow = this;
				mhandle = Handle;

				// maintain a ref so GC doesn't remove it and cause exceptions
				var evDelegate = new Native.WinEventDelegate(WinEventProc);
				mroot = GCHandle.Alloc(evDelegate);

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
				//Logger.Current.WriteLine($"hotkey event:{eventType} hwnd:{hwnd} thread:{dwEventThread}");

				if (eventType == Native.EVENT_SYSTEM_MINIMIZESTART)
				{
					// hwnd is the window being minimized; unregister if it belongs to OneNote
					Native.GetWindowThreadProcessId(hwnd, out var hwndPid);
					if (hwndPid == oneNotePID && registered && registeredKeys.Count > 0)
					{
						//Logger.Current.WriteLine("hotkey unregistering (minimize)");
						registeredKeys.ForEach(k => Native.UnregisterHotKey(mhandle, k.Id));
						registered = false;
					}

					return;
				}

				if (eventType == Native.EVENT_SYSTEM_FOREGROUND ||
					eventType == Native.EVENT_SYSTEM_MINIMIZEEND)
				{
					// Use hwnd rather than GetForegroundWindow() to avoid a TOCTOU race:
					// WINEVENT_OUTOFCONTEXT posts the callback asynchronously, so by the time
					// we run, GetForegroundWindow() may no longer match the window that fired
					// the event. hwnd carries the state at event-generation time.
					// - FOREGROUND: hwnd is the window gaining focus
					// - MINIMIZEEND: hwnd is the window being restored (OneNote)
					Native.GetWindowThreadProcessId(hwnd, out var pid);

					// OneMore's own modeless popups (e.g. CompleteHashtagDialog) run in this
					// same dllhost process and can legitimately become the foreground window;
					// treat that the same as OneNote itself so hotkeys stay registered while
					// they're shown, rather than unregistering and racing to re-register them
					// only after the popup closes - a race that made rapid close/reopen cycles
					// (e.g. repeated Alt+G) intermittently do nothing
					if (pid == oneNotePID || pid == selfPID)
					{
						if (!registered && registeredKeys.Count > 0)
						{
							//Logger.Current.WriteLine("hotkey re-registering");
							registeredKeys.ForEach(k =>
								Native.RegisterHotKey(mhandle, k.Id, k.HotModifiers, k.Key));

							registered = true;
						}
					}
					else
					{
						if (registered && registeredKeys.Count > 0)
						{
							//Logger.Current.WriteLine("hotkey unregistering");
							registeredKeys.ForEach(k =>
								Native.UnregisterHotKey(mhandle, k.Id));

							registered = false;
						}
					}
				}
			}


			protected override void WndProc(ref Message m)
			{
				if (m.Msg == Native.WM_HOTKEY)
				{
					// accept the keypress if the foreground window belongs to OneNote or to
					// this add-in's own dllhost.exe process; mirrors the same pid check in
					// WinEventProc above, which keeps hotkeys registered while one of OneMore's
					// own modeless popups (Navigator, Search, Command Palette, etc.) has focus -
					// without this, those windows stayed "registered" but every keypress while
					// they were focused was silently dropped right here
					Native.GetWindowThreadProcessId(Native.GetForegroundWindow(), out var pid);
					if (pid == oneNotePID || pid == selfPID)
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
