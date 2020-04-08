//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Text;
	using One = Microsoft.Office.Interop.OneNote;


	internal class DiagnosticsCommand : Command
	{
		public DiagnosticsCommand()
		{
		}


		public void Execute()
		{
			var builder = new StringBuilder();
			builder.AppendLine("Diagnostics.Execute()");
			builder.AppendLine(new string('-', 80));

			builder.AppendLine($"Logger path: {(logger as Logger).LogPath}");
			builder.AppendLine();

			using (var manager = new ApplicationManager())
			{
				var (backupPath, defaultPath, unfiledPath) = manager.GetLocations();
				builder.AppendLine($"Backup path.: {backupPath}");
				builder.AppendLine($"Default path: {defaultPath}");
				builder.AppendLine($"Unfiles path: {unfiledPath}");
				builder.AppendLine();

				var (pageName, pagePath, pageLink) = manager.GetCurrentPageInfo();
				builder.AppendLine($"Page name: {pageName}");
				builder.AppendLine($"Page path: {pagePath}");
				builder.AppendLine($"Page link: {pageLink}");
				builder.AppendLine();

				var app = manager.Application;

				var win = app.Windows.CurrentWindow;

				builder.AppendLine($"CurrentNotebookId: {win.CurrentNotebookId}");
				builder.AppendLine($"CurrentPageId....: {win.CurrentPageId}");
				builder.AppendLine($"CurrentSectionId.: {win.CurrentSectionId}");
				builder.AppendLine($"CurrentSecGrpId..: {win.CurrentSectionGroupId}");
				builder.AppendLine($"DockedLocation...: {win.DockedLocation}");
				builder.AppendLine($"IsFullPageView...: {win.FullPageView}");
				builder.AppendLine($"IsSideNote.......: {win.SideNote}");
				builder.AppendLine();

				builder.AppendLine($"Windows ({app.Windows.Count})");
				var currentHandle = manager.Application.Windows.CurrentWindow.WindowHandle;

				var e = app.Windows.GetEnumerator();
				while (e.MoveNext())
				{
					var window = e.Current as One.Window;

					var threadId = Native.GetWindowThreadProcessId(
						(IntPtr)window.WindowHandle,
						out var processId);

					builder.Append($"- window [processId:{processId}, threadId:{threadId}]");
					builder.Append($" handle:{window.WindowHandle:x} active:{window.Active}");

					if (window.WindowHandle == currentHandle)
					{
						builder.AppendLine(" (current)");
					}
				}

				builder.AppendLine(new string('-', 80));

				logger.WriteLine(builder.ToString());
			}
		}
	}
}
