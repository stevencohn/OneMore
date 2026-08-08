//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Drawing;
	using System.Runtime.InteropServices;
	using System.Windows.Automation;


	/// <summary>
	/// Attempts to locate the screen rectangle of the current text caret/selection within
	/// the OneNote editor surface. OneNote's editor is a custom-rendered control so neither
	/// strategy here is guaranteed to succeed; callers should treat the window-center
	/// fallback as a normal outcome rather than an error case. Enable verbose logging to
	/// see which strategy (if any) located the caret for a given invocation.
	/// </summary>
	internal static class CaretLocator
	{
		/// <summary>
		/// Gets the screen rectangle of the current text caret/selection within the given
		/// OneNote window. If it cannot be determined, a zero-size rectangle at the center
		/// of that window is returned instead.
		/// </summary>
		/// <param name="oneNoteWindowHandle">The OneNote window handle, e.g. OneNote.WindowHandle</param>
		/// <returns>A screen-coordinate rectangle to anchor a popup near</returns>
		public static Rectangle Locate(IntPtr oneNoteWindowHandle)
		{
			// use the raw handle throughout, matching MoreForm.RunModeless and HotkeyManager,
			// both of which operate on OneNote.WindowHandle directly with no GetParent walk
			var rectangle =
				TryLocate("automation", () => LocateCaretViaAutomation(oneNoteWindowHandle)) ??
				TryLocate("guithread", () => LocateCaretViaGuiThread(oneNoteWindowHandle));

			if (rectangle.HasValue)
			{
				return rectangle.Value;
			}

			var center = GetWindowCenter(oneNoteWindowHandle);
			Logger.Current.Verbose(
				$"CaretLocator: no strategy located the caret, using window center {center}");

			return center;
		}


		private static Rectangle? TryLocate(string strategy, Func<Rectangle?> locate)
		{
			try
			{
				var rectangle = locate();

				Logger.Current.Verbose(rectangle.HasValue
					? $"CaretLocator: [{strategy}] found caret at {rectangle.Value}"
					: $"CaretLocator: [{strategy}] did not find the caret");

				return rectangle;
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine($"CaretLocator: [{strategy}] threw", exc);
				return null;
			}
		}


		// Strategy 1: UI Automation TextPattern. OneNote's page canvas is custom-rendered and
		// may not expose this at all; this only succeeds if some descendant advertises support.
		private static Rectangle? LocateCaretViaAutomation(IntPtr handle)
		{
			if (handle == IntPtr.Zero)
			{
				Logger.Current.Verbose("CaretLocator.automation: window handle is zero");
				return null;
			}

			AutomationElement window;
			try
			{
				window = AutomationElement.FromHandle(handle);
			}
			catch (ElementNotAvailableException)
			{
				Logger.Current.Verbose("CaretLocator.automation: window element not available");
				return null;
			}

			if (window is null)
			{
				Logger.Current.Verbose("CaretLocator.automation: FromHandle returned null");
				return null;
			}

			// find the descendant that actually supports TextPattern; the top-level window
			// itself typically does not
			var editor = window.FindFirst(TreeScope.Descendants,
				new PropertyCondition(AutomationElement.IsTextPatternAvailableProperty, true));

			if (editor is null)
			{
				Logger.Current.Verbose(
					"CaretLocator.automation: no descendant advertises TextPattern support");
				return null;
			}

			if (editor.GetCurrentPattern(TextPattern.Pattern) is not TextPattern pattern)
			{
				Logger.Current.Verbose(
					"CaretLocator.automation: GetCurrentPattern(TextPattern) failed");
				return null;
			}

			// a zero-length selection represents the caret
			var ranges = pattern.GetSelection();
			if (ranges is null || ranges.Length == 0)
			{
				Logger.Current.Verbose("CaretLocator.automation: GetSelection() returned no ranges");
				return null;
			}

			var bounds = ranges[0].GetBoundingRectangles();
			if (bounds is null || bounds.Length == 0)
			{
				Logger.Current.Verbose(
					"CaretLocator.automation: GetBoundingRectangles() returned nothing");
				return null;
			}

			var rect = bounds[0];
			if (rect.IsEmpty || rect.Width <= 0 || rect.Height <= 0 ||
				double.IsInfinity(rect.Width) || double.IsInfinity(rect.Height))
			{
				Logger.Current.Verbose($"CaretLocator.automation: degenerate bounding rect {rect}");
				return null;
			}

			return new Rectangle(
				(int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
		}


		// Strategy 2: the classic Win32 caret, via GetGUIThreadInfo. Some custom-rendered
		// controls (including, possibly, OneNote's) still call CreateCaret/SetCaretPos for
		// IME and accessibility purposes even though they don't support UI Automation's
		// TextPattern, so this can succeed where the automation strategy fails.
		private static Rectangle? LocateCaretViaGuiThread(IntPtr handle)
		{
			if (handle == IntPtr.Zero)
			{
				Logger.Current.Verbose("CaretLocator.guithread: window handle is zero");
				return null;
			}

			var threadId = Native.GetWindowThreadProcessId(handle, out _);
			if (threadId == 0)
			{
				Logger.Current.Verbose(
					"CaretLocator.guithread: GetWindowThreadProcessId returned 0");
				return null;
			}

			var info = new Native.GUITHREADINFO
			{
				cbSize = Marshal.SizeOf<Native.GUITHREADINFO>()
			};

			if (!Native.GetGUIThreadInfo(threadId, ref info))
			{
				Logger.Current.Verbose("CaretLocator.guithread: GetGUIThreadInfo failed");
				return null;
			}

			if (info.hwndCaret == IntPtr.Zero)
			{
				Logger.Current.Verbose(
					"CaretLocator.guithread: no hwndCaret reported for this thread");
				return null;
			}

			var topLeft = new Native.Point
			{
				X = info.rcCaret.Left,
				Y = info.rcCaret.Top
			};

			if (!Native.ClientToScreen(info.hwndCaret, ref topLeft))
			{
				Logger.Current.Verbose("CaretLocator.guithread: ClientToScreen failed");
				return null;
			}

			var width = info.rcCaret.Right - info.rcCaret.Left;
			var height = info.rcCaret.Bottom - info.rcCaret.Top;
			if (height <= 0)
			{
				Logger.Current.Verbose(
					$"CaretLocator.guithread: degenerate caret rect, height={height}");
				return null;
			}

			return new Rectangle(topLeft.X, topLeft.Y, Math.Max(width, 1), height);
		}


		private static Rectangle GetWindowCenter(IntPtr handle)
		{
			var rect = new Native.Rectangle();
			if (handle != IntPtr.Zero)
			{
				Native.GetWindowRect(handle, ref rect);
			}

			return new Rectangle(
				rect.Left + ((rect.Right - rect.Left) / 2),
				rect.Top + ((rect.Bottom - rect.Top) / 2),
				0, 0);
		}
	}
}
