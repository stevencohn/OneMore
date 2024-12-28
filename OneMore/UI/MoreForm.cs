//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Diagnostics;
	using System.Drawing;
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


		public MoreForm()
		{
			Properties.Resources.Culture = AddIn.Culture;
			manager = ThemeManager.Instance;
			logger = Logger.Current;
		}


		/// <summary>
		/// 
		/// </summary>
		protected Control DefaultControl { get; set; }


		/// <summary>
		/// Gets or sets whether the location has been set by the caller and should NOT be
		/// overriden by the OnLoad method below...
		/// </summary>
		public bool ManualLocation { get; set; } = false;


		/// <summary>
		/// Lets inheritors disable theming for specialized cases like TimerWindow
		/// </summary>
		protected bool ThemeEnabled { get; set; } = true;


		/// <summary>
		/// Sets the absolute vertical offset in pixels from "centered" that you want to
		/// position this window upon load. This can be either a positive or negative value.
		/// </summary>
		public int VerticalOffset { private get; set; }


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

			var rect = new Native.Rectangle();
			using (var one = new OneNote())
			{
				Native.GetWindowRect(one.WindowHandle, ref rect);
			}

			var yoffset = (int)(Height * topDelta / 100.0);

			Location = new Point(
				(rect.Left + ((rect.Right - rect.Left) / 2)) - (Width / 2),
				(rect.Top + ((rect.Bottom - rect.Top) / 2)) - (Height / 2) - yoffset
				);

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
			base.OnFormClosed(e);
			appContext?.Dispose();
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
		/// Modeless dialogs would appear behind the OneNote window by default
		/// so this forces the dialog to the foreground
		/// </summary>
		/// <param name="keepTop">True to maintain this form as a TopMost form</param>
		public void Elevate(bool keepTop = true)
		{
			if (DesignMode)
			{
				return;
			}

			//logger.WriteLine($"elevating [{Text}]");

			// a bunch of hocus-pocus to force the form to the foreground...

			//IntPtr HWND_TOPMOST = new(-1);
			//Native.SetWindowPos(Handle, HWND_TOPMOST, 0, 0, 0, 0,
			//	Native.SWP_NOMOVE | Native.SWP_NOSIZE);

			//var location = Location;

			//Native.SetForegroundWindow(Handle);
			if (modeless)
			{
				BringToFront();
			}

			//Location = location;

			// this is the trick needed to elevate a dialog to TopMost
			TopMost = false;
			TopMost = true;
			TopMost = keepTop;

			Select();
			Focus();
		}


		public virtual void OnThemeChange()
		{
		}
	}
}
