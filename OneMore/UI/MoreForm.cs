//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Diagnostics;
	using System.Drawing;
	using System.Threading.Tasks;
	using System.Windows.Automation;
	using System.Windows.Forms;


	public interface IOneMoreWindow : IDisposable
	{
	}


	internal class MoreForm : Form, IOneMoreWindow
	{
		public event EventHandler ModelessClosed;

		protected readonly ThemeManager manager;
		protected readonly ILogger logger;

		private ApplicationContext appContext;
		private bool modeless = false;
		private IntPtr oneNoteHandle = IntPtr.Zero;

		private bool elevatedWithOneNote;
		private int processId;
		private int trackedId;


		public MoreForm()
		{
			Properties.Resources.Culture = AddIn.Culture;
			manager = ThemeManager.Instance;
			logger = Logger.Current;
		}


		/// <summary>
		/// Gets or sets the control that should be focused by default when the form is loaded.
		/// </summary>
		protected Control DefaultControl { get; set; }


		/// <summary>
		/// Gets or sets whether this form tracks the elevation of any ONENOTE window and,
		/// based on user preference, will elevate this form as well.
		/// </summary>
		public bool ElevatedWithOneNote
		{
			get => elevatedWithOneNote;

			set
			{
				elevatedWithOneNote = value;
				processId = Process.GetCurrentProcess().Id;
				trackedId = processId;
			}
		}


		/// <summary>
		/// Gets or sets whether the location has been set by the caller and should NOT be
		/// overriden by the OnLoad method below...
		/// </summary>
		public bool ManualLocation { get; set; } = false;


		/// <summary>
		/// Opt-in: when true, this form's Size is saved when it closes and restored
		/// when it next loads, keyed by the derived class's type name. Only meant for
		/// forms with a sizable FormBorderStyle (Sizable / SizableToolWindow).
		/// </summary>
		protected bool RememberSize { get; set; } = false;


		/// <summary>
		/// Lets inheritors disable theming for specialized cases like TimerWindow
		/// </summary>
		protected bool ThemeEnabled { get; set; } = true;


		/// <summary>
		/// Sets the absolute vertical offset in pixels from "centered" that you want to
		/// position this window upon load. This can be either a positive or negative value.
		/// </summary>
		public int VerticalOffset { private get; set; }


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		/// <summary>
		/// Determines if the main OneNote thread culture differs from our default design-time
		/// language, English. If true, then the Localize method should be called.
		/// </summary>
		/// <returns></returns>
		protected static bool NeedsLocalizing()
		{
			return Translator.NeedsLocalizing();
		}


		/// <summary>
		/// Traslate the text of specified controls on this form
		/// </summary>
		/// <param name="keys">
		/// A list of control identifiers as described by TranslationHelper
		/// </param>
		/// <seealso cref="River.OneMoreAddIn.UI.Translator"/>
		protected void Localize(string[] keys)
		{
			Translator.Localize(this, keys);
		}


		/// <summary>
		/// In order for a dialog to interact with OneNote, it must run modeless so it doesn't
		/// block the OneNote main UI thread. This method runs the current form as a modeless
		/// window and invokes the specified callbacks upon OK and Cancel.
		/// </summary>
		/// <param name="closedAction">
		/// An event handler to run when the modeless dialog is closed
		/// </param>
		/// <param name="topDelta">
		/// Optionally percentage of the dialog height to subtract from the top coordinate, 0-100
		/// </param>
		public void RunModeless(EventHandler closedAction = null, int topDelta = 0)
		{
			StartPosition = FormStartPosition.Manual;
			modeless = true;

			// must happen before centering below, which reads Width/Height, and before
			// OnLoad's own RestoreSize call, which runs too late here because OnLoad
			// skips re-centering entirely for modeless forms (see OnLoad)
			if (RememberSize)
			{
				RestoreSize();
			}

			var rect = new Native.Rectangle();
			using (var one = new OneNote())
			{
				Native.GetWindowRect(one.WindowHandle, ref rect);
				oneNoteHandle = one.WindowHandle;
			}

			var yoffset = (int)(Height * topDelta / 100.0);

			Location = new Point(
				(rect.Left + ((rect.Right - rect.Left) / 2)) - (Width / 2),
				(rect.Top + ((rect.Bottom - rect.Top) / 2)) - (Height / 2) - yoffset
				);

			RunModelessCore(closedAction);
		}


		/// <summary>
		/// Runs the current form as a modeless window positioned at an explicit screen
		/// location rather than centered over the OneNote window, e.g. for a popup that
		/// must be anchored near a specific point on the page.
		/// </summary>
		/// <param name="location">The explicit screen location for the form</param>
		/// <param name="closedAction">
		/// An event handler to run when the modeless dialog is closed
		/// </param>
		public void RunModeless(Point location, EventHandler closedAction = null)
		{
			StartPosition = FormStartPosition.Manual;
			modeless = true;
			ManualLocation = true;
			Location = location;

			using (var one = new OneNote())
			{
				oneNoteHandle = one.WindowHandle;
			}

			RunModelessCore(closedAction);
		}


		private void RunModelessCore(EventHandler closedAction)
		{
			if (closedAction != null)
			{
				ModelessClosed += (sender, e) => { closedAction(sender, e); };
			}

			if (Application.MessageLoop)
			{
				// starting a second message loop on a single thread is not a valid operation
				// so just display the form if we already have a message loop
				Show();
				return;
			}

			appContext = new ApplicationContext(this);
			Application.Run(appContext);
		}


		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			if (RememberSize && WindowState == FormWindowState.Normal)
			{
				SaveSize();
			}

			base.OnFormClosed(e);
			appContext?.Dispose();

			if (modeless && oneNoteHandle != IntPtr.Zero)
			{
				// OneMore runs in dllhost.exe (COM surrogate), not ONENOTE.EXE, so closing
				// this modeless dialog does not automatically hand foreground focus back to
				// ONENOTE.EXE's window - it can be left on this (now-closing) dllhost window
				// or nowhere in particular. Until the user manually reactivates OneNote (a
				// click, or typing into it), HotkeyManager's WndProc gate - which only
				// dispatches WM_HOTKEY when GetForegroundWindow() belongs to oneNotePID -
				// silently swallows every hotkey press. This call is allowed to succeed
				// without the AttachThreadInput dance that Elevate() needs, because this
				// window is itself still the foreground window and just received the input
				// (e.g. Escape) that's closing it - one of the documented exceptions to the
				// SetForegroundWindow restriction.
				Native.SetForegroundWindow(oneNoteHandle);
			}

			if (ElevatedWithOneNote)
			{
				// undo the AddAutomationFocusChangedEventHandler from OnShown; otherwise this
				// (now disposed) form keeps receiving process-wide focus-change callbacks and
				// Elevate() throws ObjectDisposedException on every one of them, silently, for
				// as long as the process lives - this compounds quickly for dialogs that are
				// shown and closed repeatedly, like CompleteHashtagDialog.
				//
				// This must NOT run inline here: Automation.RemoveAutomationFocusChangedEventHandler
				// synchronizes with the UI Automation provider infrastructure and can block for
				// several seconds. Since ModelessClosed (below) is what releases the owning
				// command's re-entry guard and clears its static dialog reference, blocking here
				// blocks that cleanup too - every hotkey/ribbon invocation in the meantime sees
				// a stale, already-disposed dialog and silently no-ops via Elevate()'s IsDisposed
				// check, making the dialog appear unable to reopen for however long this call
				// happens to take. Run it fire-and-forget instead; OnFocusChanged's own IsDisposed
				// guard already makes any straggling callback in the meantime harmless.
				Task.Run(() =>
				{
					try
					{
						Automation.RemoveAutomationFocusChangedEventHandler(OnFocusChanged);
					}
					catch
					{
						// best-effort cleanup; a failure here just leaves a harmless
						// (IsDisposed-guarded) stale handler registered
					}
				});
			}

			ModelessClosed?.Invoke(this, e);
		}


		/*========================================================================================
		//

		Event Handlers run in this sequence: OnActivated... OnLoad... OnShown

		Through a bit of voodoo and dark incantations, we attempt to handle all cases that
		need to elevate or surface new or existing dialogs. Dialogs are invoked from and through
		the OneNote Interop COM API, making ownership and parent/child relationships muddy.

		There are two distinct invocation scenarios:

			- Invoking a command from the OneNote ribbon UI and the OneMore menus. In this case,
			  there is a disconnect between the OneNote native thread and the OneMore dllhost
			  managed thread, making it impossible to Activate the dialog and capture input

			- Invoking a command as a hotkey. In this case, OneNote somehow hands input capture
			  over to the dialog. (Is there a property somewhere that indicates a difference?)
		
		Here are a couple of examples:

			- The palette uses System.Window.Forms.Form.ShowDialog() to create a modal dialog
			- Mardown preview is a wrapper of WebViewDialog which needs an STA context
			- Hashtag uses MoreForm.RunModeless() to create a modeless dialog

		ShowDialog pretty much takes care of itself.
		RunModeless is isolated into its own ApplicationContext.

		Microsoft actively does whatever it can to prevent an app from selecting a random window
		to SetActive or capture keyboard input. Security experts tend to think this is a threat.

		- - - - > >

		So the conclusion is that the code below is a careful balance that eliminates the
		OneNote window from flickering when CommandPalette is opened, but also allows proper
		elevation for all other windows, with the concession that we don't really allow
		persistent TopMost for any OneMore dialog.

		//
		//======================================================================================*/

		protected override void OnActivated(EventArgs e)
		{
			//logger.WriteLine($"MoreForm.OnActivated [{Text}]");

			base.OnActivated(e);

			if (modeless)
			{
				// will get called twice, but it's needed to ensure RunModeless dialogs like
				// FindHashtagDialog is elevated properly
				Elevate(false);
			}
		}


		protected override async void OnLoad(EventArgs e)
		{
			#region LoadControls() for themes
			static void LoadControls(Control.ControlCollection controls)
			{
				foreach (Control child in controls)
				{
					if (child is ILoadControl loader)
					{
						loader.OnLoad();
					}

					if (child.Controls.Count > 0)
					{
						LoadControls(child.Controls);
					}
				}
			}
			#endregion

			if (ThemeEnabled)
			{
				manager.InitializeTheme(this);
			}

			LoadControls(Controls);

			//logger.WriteLine($"MoreForm.OnLoad try focus");
			TryFocus();

			if (RememberSize && !DesignMode)
			{
				RestoreSize();
			}

			// RunModeless has already set location so don't repeat that here and only set
			// location if inheritor hasn't declined by setting it to zero. Also, we're doing
			// this in OnLoad so it doesn't visually "jump" as it would if done in OnShown
			if (DesignMode || modeless)
			{
				//logger.WriteLine($"MoreForm.OnLoad calling base due to modeless...");
				base.OnLoad(e);
				//logger.WriteLine($"MoreForm.OnLoad returning due to modeless");
				return;
			}

			if (!ManualLocation && StartPosition == FormStartPosition.Manual)
			{
				//logger.WriteLine($"MoreForm.OnLoad manual manual?");

				/***** ********************************************************** *****/
				/***** ********************************************************** *****/
				/*****                                                            *****/
				/*****  DO NOT SET A BREAKPOINT PRIOR TO THIS POINT IN THE CODE   *****/
				/*****  otherwise the call to new OneNote() will hang!            *****/
				/*****                                                            *****/
				/*****  If a breakpoint IS set prior to this, you MUST attach     *****/
				/*****  the debugger or Debugger.IsAttached will be false and     *****/
				/*****  the call to new OneNote() will hang!                      *****/
				/*****                                                            *****/
				/***** ********************************************************** *****/
				/***** ********************************************************** *****/

				if (!Debugger.IsAttached)
				{
					// find the center point of the active OneNote window
					await using var one = new OneNote();
					var bounds = one.GetCurrentMainWindowBounds();
					var center = new Point(
						bounds.Left + (bounds.Right - bounds.Left) / 2,
						bounds.Top + (bounds.Bottom - bounds.Top) / 2);

					Location = new Point(center.X - (Width / 2), center.Y - (Height / 2));

					//logger.WriteLine($"MoreForm.OnLoad center point {Location.X}x{Location.Y}");
				}
			}

			if (VerticalOffset != 0)
			{
				StartPosition = FormStartPosition.Manual;
				var x = Location.X < 0 ? 0 : Location.X;
				var y = Location.Y + VerticalOffset;

				Location = new Point(x, y < 0 ? 0 : y);

				//logger.WriteLine($"MoreForm.OnLoad vertical offset {Location.X}x{Location.Y}");
			}

			//logger.WriteLine($"MoreForm.OnLoad calling base...");
			base.OnLoad(e);
			//logger.WriteLine($"MoreForm.OnLoad after base");
		}


		protected override void OnShown(EventArgs e)
		{
			//logger.WriteLine($"showing [{Text}]");
			base.OnShown(e);
			TryFocus();

			if (ElevatedWithOneNote)
			{
				// Must not run inline here: Automation.AddAutomationFocusChangedEventHandler
				// synchronizes with the UI Automation provider infrastructure and can block for
				// several seconds - the same cost proven out on the RemoveAutomationFocus-
				// ChangedEventHandler side in OnFormClosed below. Since OnShown runs before the
				// message loop gets back around to painting this form's child controls, blocking
				// here shows up as a multi-second delay between the window appearing and its
				// controls actually painting. This is especially likely to contend with a
				// still-in-flight background Remove from a just-closed dialog of the same kind
				// (e.g. reopening this dialog in quick succession), since both sides now run
				// off-thread instead of being serialized by one blocking the other.
				Task.Run(() =>
				{
					try
					{
						Automation.AddAutomationFocusChangedEventHandler(OnFocusChanged);
					}
					catch
					{
						// best-effort; a failure here just means this form won't elevate
						// automatically when ONENOTE regains focus
					}
				});
			}
		}


		private void TryFocus()
		{
			if (DesignMode)
			{
				return;
			}

			if (DefaultControl is not null)
			{
				//logger.WriteLine("focusing on default control");
				DefaultControl.FindForm()?.Activate();
				DefaultControl.Select();
				DefaultControl.Focus();
			}
		}


		/// <summary>
		/// Uses Windows Automation to track when a main ONENOTE window is focused or elevated
		/// on top of other windows, and elevates this form. Typically used for NavigatorWindow.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFocusChanged(object sender, AutomationFocusChangedEventArgs e)
		{
			// defensive guard against any stale handler still registered on a disposed form,
			// e.g. from a session predating the OnFormClosed cleanup added alongside this
			if (IsDisposed)
			{
				return;
			}

			if (sender is AutomationElement element)
			{
				var pid = element.Current.ProcessId;
				var process = Process.GetProcessById(pid);
				var name = process.ProcessName;

				// elevates this form notop of ONENOTE when ONENOTE is elevated, but also allows
				// ONENOTE to be on top of this window when switching immediately from this
				// window to ONENOTE

				if (name == "ONENOTE" && trackedId != pid && trackedId != processId)
				{
					//logger.WriteLine($"focused tracking elevating");
					Elevate();
				}
				else if (name != "ONENOTE" && pid != processId && TopMost)
				{
					// some other application took focus away from ONENOTE; let this
					// window submerge along with ONENOTE instead of staying stuck on top
					TopMost = false;
				}

				trackedId = pid;
			}
		}


		/// <summary>
		/// Modeless dialogs would appear behind the OneNote window by default
		/// so this forces the dialog to the foreground
		/// </summary>
		/// <param name="keepTop">True to maintain this form as a TopMost form</param>
		public void Elevate(bool keepTop = true)
		{
			if (DesignMode || IsDisposed)
			{
				return;
			}

			//logger.WriteLine($"elevating [{Text}]");

			if (modeless)
			{
				BringToFront();
			}

			// Temporarily share the input queue with the foreground thread so that
			// SetForegroundWindow succeeds regardless of which process has foreground rights.
			// This is needed because OneMore runs in dllhost.exe (COM surrogate), not ONENOTE.EXE,
			// and by the time dialogs are shown the original COM call / WM_HOTKEY rights are gone.
			try
			{
				var foreground = Native.GetForegroundWindow();
				if (!IsDisposed && IsHandleCreated && foreground != IntPtr.Zero && foreground != Handle)
				{
					uint foregroundThread = Native.GetWindowThreadProcessId(foreground, out _);
					uint currentThread = Native.GetCurrentThreadId();

					bool attached = foregroundThread != currentThread &&
						Native.AttachThreadInput(foregroundThread, currentThread, true);

					Native.SetForegroundWindow(Handle);
					Native.BringWindowToTop(Handle);

					if (attached)
					{
						Native.AttachThreadInput(foregroundThread, currentThread, false);
					}
				}
			}
			catch (ObjectDisposedException)
			{
				// the dialog can close and dispose itself between the IsDisposed/IsHandleCreated
				// checks above and the Handle access, racing with this call on another thread
				return;
			}

			// TopMost toggle ensures the window appears above OneNote in z-order
			TopMost = false;
			TopMost = true;
			TopMost = keepTop;

			if (!IsDisposed)
			{
				try
				{
					Select();
					Focus();
				}
				catch
				{
					// swallow disposed exception
				}
			}
		}


		public virtual void OnThemeChange()
		{
		}


		private void RestoreSize()
		{
			var settings = new SettingsProvider().GetCollection(GetType().Name);
			if (!settings.Contains("width") || !settings.Contains("height"))
			{
				return;
			}

			var screen = Screen.FromControl(this);
			var width = Math.Min(settings.Get("width", Width), screen.WorkingArea.Width);
			var height = Math.Min(settings.Get("height", Height), screen.WorkingArea.Height);

			if (MinimumSize.Width > 0)
			{
				width = Math.Max(width, MinimumSize.Width);
			}

			if (MinimumSize.Height > 0)
			{
				height = Math.Max(height, MinimumSize.Height);
			}

			Size = new Size(width, height);
		}


		private void SaveSize()
		{
			var provider = new SettingsProvider();
			var settings = provider.GetCollection(GetType().Name);
			settings.Add("width", Width);
			settings.Add("height", Height);
			provider.SetCollection(settings);
			provider.Save();
		}
	}
}
