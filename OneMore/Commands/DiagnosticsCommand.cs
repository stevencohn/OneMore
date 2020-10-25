//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Diagnostics;
	using System.Reflection;
	using System.Text;
	using One = Microsoft.Office.Interop.OneNote;


	internal class DiagnosticsCommand : Command
	{
		public DiagnosticsCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			var builder = new StringBuilder();
			builder.AppendLine("Diagnostics.Execute()");
			builder.AppendLine(new string('-', 80));

			builder.AppendLine($"ONENOTE...: {Process.GetProcessesByName("ONENOTE")[0].MainModule.FileName}");
			builder.AppendLine($"Addin path: {Assembly.GetExecutingAssembly().Location}");
			builder.AppendLine($"Data path.: {PathFactory.GetAppDataPath()}");
			builder.AppendLine($"Log path..: {logger.LogPath}");
			builder.AppendLine();

			using (var one = new OneNote())
			{
				var (backupFolder, defaultFolder, unfiledFolder) = one.GetFolders();
				builder.AppendLine($"Default path: {defaultFolder}");
				builder.AppendLine($"Backup  path: {backupFolder}");
				builder.AppendLine($"Unfiled path: {unfiledFolder}");
				builder.AppendLine();

				var (Name, Path, Link) = one.GetPageInfo();
				builder.AppendLine($"Page name: {Name}");
				builder.AppendLine($"Page path: {Path}");
				builder.AppendLine($"Page link: {Link}");
				builder.AppendLine();

				/*
				var app = one.Application;

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

				var e = app.Windows.GetEnumerator();
				while (e.MoveNext())
				{
					var window = e.Current as One.Window;

					var threadId = Native.GetWindowThreadProcessId(
						(IntPtr)window.WindowHandle,
						out var processId);

					builder.Append($"- window [processId:{processId}, threadId:{threadId}]");
					builder.Append($" handle:{window.WindowHandle:x} active:{window.Active}");

					if (window.WindowHandle == (ulong)one.WindowHandle)
					{
						builder.AppendLine(" (current)");
					}
				}
				*/

				builder.AppendLine();

				var page = one.GetPage();
				var pageColor = page.GetPageColor(out _, out _);
				var pageBrightness = pageColor.GetBrightness();

				builder.AppendLine($"Page background: {pageColor.ToRGBHtml()}");
				builder.AppendLine($"Page brightness: {pageBrightness}");
				builder.AppendLine($"Page is dark...: {pageBrightness < 0.5}");

				(float dpiX, float dpiY) = UIHelper.GetDpiValues();
				builder.AppendLine($"Screen DPI.....: horizontal/X:{dpiX} vertical/Y:{dpiY}");

				(float scalingX, float scalingY) = UIHelper.GetScalingFactors();
				builder.AppendLine($"Scaling factors: horizontal/X:{scalingX} vertical/Y:{scalingY}");

				builder.AppendLine(new string('-', 80));

				logger.WriteLine(builder.ToString());

				UIHelper.ShowInfo($"Diagnostics written to {logger.LogPath}");
			}
		}
	}
}
