//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

// uncomment to log hotkey arm/disarm and foreground window changes
//#define HOTKEY_TRACE

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
		private static bool suspended = false;


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
		/// Temporarily unregisters all hotkeys, e.g. while a modal dialog such as the
		/// Settings dialog is open and needs to capture key sequences of its own without
		/// triggering the global commands they're normally bound to.
		/// </summary>
		/// <remarks>
		/// Sets a sticky "suspended" flag so the WinEventProc's own foreground/minimize
		/// re-registration logic won't race with this and silently undo it; only Resume()
		/// clears that flag.
		/// </remarks>
		public static void Suspend()
		{
			resetEvent.WaitOne();

			mwindow.Invoke(new Action(() =>
			{
				if (registered && registeredKeys.Count > 0)
				{
					DisarmHotkeys("suspend");
				}

				suspended = true;
			}));
		}


		// Registers all hotkeys with the OS; must run on the message thread. Leaves
		// registered false if any registration fails so the reconcile timer retries.
		private static void ArmHotkeys(string reason)
		{
			const int ERROR_HOTKEY_ALREADY_REGISTERED = 1409;
			var ok = true;

			foreach (var k in registeredKeys)
			{
				if (!Native.RegisterHotKey(mhandle, k.Id, k.HotModifiers, k.Key))
				{
					var error = Marshal.GetLastWin32Error();
					if (error != ERROR_HOTKEY_ALREADY_REGISTERED)
					{
						ok = false;
						Logger.Current.Verbose(
							$"hotkey register failed id:{k.Id} key:{k.Key} error:{error} ({reason})");
					}
				}
			}

			registered = ok;
#if HOTKEY_TRACE
			Logger.Current.Verbose($"hotkey armed registered:{registered} ({reason})");
#endif
		}


		// Unregisters all hotkeys from the OS; must run on the message thread
		private static void DisarmHotkeys(string reason)
		{
			foreach (var k in registeredKeys)
			{
				Native.UnregisterHotKey(mhandle, k.Id);
			}

			registered = false;
#if HOTKEY_TRACE
			Logger.Current.Verbose($"hotkey disarmed ({reason})");
#endif
		}


		private static string DescribeWindow(IntPtr hwnd, uint pid)
		{
			var builder = new System.Text.StringBuilder(256);
			Native.GetClassName(hwnd, builder, builder.Capacity);
			return $"hwnd:{hwnd} pid:{pid} class:{builder}";
		}


		// True if the given window belongs to OneNote or to this add-in's own process
		private static bool IsOurs(IntPtr hwnd)
		{
			Native.GetWindowThreadProcessId(hwnd, out var pid);
			return pid == oneNotePID || pid == selfPID;
		}


		/// <summary>
		/// Reinstates hotkeys previously suspended by Suspend().
		/// </summary>
		public static void Resume()
		{
			resetEvent.WaitOne();

			mwindow.Invoke(new Action(() =>
			{
				suspended = false;

				// only re-arm if OneNote (or we) still have the foreground; otherwise the
				// WinEventProc/reconcile timer will arm when OneNote regains it
				if (!registered && registeredKeys.Count > 0 &&
					IsOurs(Native.GetForegroundWindow()))
				{
					ArmHotkeys("resume");
				}
			}));
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

				// Safety net: foreground events can be missed or misclassified, which leaves
				// hotkeys unregistered while OneNote is in front. Periodically reconcile the
				// registered state with the actual foreground window. Timer ticks are
				// delivered on this (message) thread so no extra locking is needed.
				reconcileTimer = new System.Windows.Forms.Timer { Interval = 1000 };
				reconcileTimer.Tick += Reconcile;
				reconcileTimer.Start();

				resetEvent.Set();
			}


			private readonly System.Windows.Forms.Timer reconcileTimer;


			private void Reconcile(object sender, EventArgs e)
			{
				if (suspended || registeredKeys.Count == 0)
				{
					return;
				}

				var foreground = Native.GetForegroundWindow();
				if (foreground == IntPtr.Zero)
				{
					// transient state during window switches
					return;
				}

				Native.GetWindowThreadProcessId(foreground, out var pid);
				var wanted = pid == oneNotePID || pid == selfPID;

				if (wanted && !registered)
				{
#if HOTKEY_TRACE
					Logger.Current.Verbose($"hotkey reconcile re-arming {DescribeWindow(foreground, pid)}");
#endif
					ArmHotkeys("reconcile");
				}
				else if (!wanted && registered)
				{
#if HOTKEY_TRACE
					Logger.Current.Verbose($"hotkey reconcile disarming {DescribeWindow(foreground, pid)}");
#endif
					DisarmHotkeys("reconcile");
				}
			}


			private void WinEventProc(
				IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
				int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
			{
				// ignore bogus/transient events; treating pid 0 as "another app" would
				// wrongly unregister hotkeys
				if (hwnd == IntPtr.Zero || !Native.IsWindow(hwnd))
				{
					return;
				}

				if (eventType == Native.EVENT_SYSTEM_MINIMIZESTART)
				{
					// hwnd is the window being minimized; unregister if it belongs to OneNote
					Native.GetWindowThreadProcessId(hwnd, out var hwndPid);
					if (hwndPid == oneNotePID && registered && registeredKeys.Count > 0)
					{
						Logger.Current.Verbose($"hotkey minimize {DescribeWindow(hwnd, hwndPid)}");
						DisarmHotkeys("minimize");
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
						if (!registered && !suspended && registeredKeys.Count > 0)
						{
#if HOTKEY_TRACE
							Logger.Current.Verbose(
								$"hotkey foreground(ours) {DescribeWindow(hwnd, pid)}");
#endif
							ArmHotkeys("foreground");
						}
					}
					else
					{
						if (registered && registeredKeys.Count > 0)
						{
#if HOTKEY_TRACE
							Logger.Current.Verbose(
								$"hotkey foreground(other) {DescribeWindow(hwnd, pid)}");
#endif
							DisarmHotkeys("foreground");
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
					else
					{
						Logger.Current.Verbose(
							$"hotkey dropped, foreground {DescribeWindow(Native.GetForegroundWindow(), pid)}");
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
