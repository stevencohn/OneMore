//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;


	internal class DumpDiagnosticsCommand : Command
	{
		public DumpDiagnosticsCommand()
		{
		}


		public void Execute()
		{
			logger.WriteLine("DumpDiagnostics.Execute()");

			using (var manager = new ApplicationManager())
			{
				var currentHandle = manager.Application.Windows.CurrentWindow.WindowHandle;

				var e = manager.Application.Windows.GetEnumerator();
				while (e.MoveNext())
				{
					var window = e.Current as Microsoft.Office.Interop.OneNote.Window;

					var threadId = Native.GetWindowThreadProcessId(
						(IntPtr)window.WindowHandle,
						out var processId);

					if (window.WindowHandle == currentHandle)
					{
						logger.WriteLine($"window [{processId}:{threadId}] {window.WindowHandle:x} active:{window.Active} (current)");
					}
					else
					{
						logger.WriteLine($"window [{processId}:{threadId}] {window.WindowHandle:x} active:{window.Active}");
					}
				}
			}
		}
	}
}
