//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Drawing;
	using System.Windows.Automation;
	using System.Windows.Forms;


	/// <summary>
	/// Attempts to locate the screen rectangle of the current text caret/selection within
	/// the OneNote editor surface, using UI Automation. OneNote's editor is a custom-rendered
	/// control so this is not guaranteed to succeed; callers should treat the mouse-cursor
	/// fallback as a normal outcome rather than an error case.
	/// </summary>
	internal static class CaretLocator
	{
		/// <summary>
		/// Gets the screen rectangle of the current text caret/selection within the given
		/// top-level OneNote window. If it cannot be determined via UI Automation, a
		/// zero-size rectangle at the current mouse cursor position is returned instead.
		/// </summary>
		/// <param name="oneNoteWindowHandle">
		/// A handle to (or descendant of) the OneNote window, e.g. OneNote.WindowHandle
		/// </param>
		/// <returns>A screen-coordinate rectangle to anchor a popup near</returns>
		public static Rectangle Locate(IntPtr oneNoteWindowHandle)
		{
			try
			{
				var rectangle = LocateCaretRectangle(oneNoteWindowHandle);
				if (rectangle.HasValue)
				{
					return rectangle.Value;
				}
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine(
					"CaretLocator: UI Automation failed to locate caret, using cursor position", exc);
			}

			var cursor = Cursor.Position;
			return new Rectangle(cursor.X, cursor.Y, 0, 0);
		}


		private static Rectangle? LocateCaretRectangle(IntPtr oneNoteWindowHandle)
		{
			var handle = GetTopLevelWindow(oneNoteWindowHandle);
			if (handle == IntPtr.Zero)
			{
				return null;
			}

			AutomationElement window;
			try
			{
				window = AutomationElement.FromHandle(handle);
			}
			catch (ElementNotAvailableException)
			{
				return null;
			}

			if (window is null)
			{
				return null;
			}

			// find the descendant that actually supports TextPattern; the top-level window
			// itself typically does not
			var editor = window.FindFirst(TreeScope.Descendants,
				new PropertyCondition(AutomationElement.IsTextPatternAvailableProperty, true));

			if (editor is null ||
				editor.GetCurrentPattern(TextPattern.Pattern) is not TextPattern pattern)
			{
				return null;
			}

			// a zero-length selection represents the caret
			var ranges = pattern.GetSelection();
			if (ranges is null || ranges.Length == 0)
			{
				return null;
			}

			var bounds = ranges[0].GetBoundingRectangles();
			if (bounds is null || bounds.Length == 0)
			{
				return null;
			}

			var rect = bounds[0];
			if (rect.IsEmpty || rect.Width <= 0 || rect.Height <= 0 ||
				double.IsInfinity(rect.Width) || double.IsInfinity(rect.Height))
			{
				return null;
			}

			return new Rectangle(
				(int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
		}


		// OneNote's COM Window.WindowHandle is not guaranteed to be the top-level frame -
		// it can be an inner pane - so walk up GetParent until there is no parent left,
		// mirroring the equivalent logic in OneNote.GetTopLevelWindow
		private static IntPtr GetTopLevelWindow(IntPtr handle)
		{
			var current = handle;
			while (current != IntPtr.Zero)
			{
				var parent = Native.GetParent(current);
				if (parent == IntPtr.Zero)
				{
					break;
				}

				current = parent;
			}

			return current;
		}
	}
}
