//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Diagnostics;
	using System.Reflection;
	using System.Threading.Tasks;


	internal class DiagnosticsCommand : Command
	{
		public DiagnosticsCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			logger.StartDiagnostic();
			logger.WriteLine("Diagnostics.Execute()");
			logger.WriteLine(new string('-', 80));

			var processes = Process.GetProcessesByName("ONENOTE");
			var module = processes.Length > 0 ? processes[0].MainModule.FileName : "unknown";

			logger.WriteLine($"ONENOTE...: {module}");
			logger.WriteLine($"Addin path: {Assembly.GetExecutingAssembly().Location}");
			logger.WriteLine($"Data path.: {PathFactory.GetAppDataPath()}");
			logger.WriteLine($"Log path..: {logger.LogPath}");
			logger.WriteLine();

			using (var one = new OneNote())
			{
				var (backupFolder, defaultFolder, unfiledFolder) = one.GetFolders();
				logger.WriteLine($"Default path: {defaultFolder}");
				logger.WriteLine($"Backup  path: {backupFolder}");
				logger.WriteLine($"Unfiled path: {unfiledFolder}");
				logger.WriteLine();

				var info = one.GetPageInfo();
				logger.WriteLine($"Page name: {info.Name}");
				logger.WriteLine($"Page path: {info.Path}");
				logger.WriteLine($"Page link: {info.Link}");
				logger.WriteLine();

				info = one.GetSectionInfo();
				logger.WriteLine($"Section name: {info.Name}");
				logger.WriteLine($"Section path: {info.Path}");
				logger.WriteLine($"Section link: {info.Link}");
				logger.WriteLine();

				var notebook = one.GetNotebook();
				var notebookId = one.CurrentNotebookId;
				logger.WriteLine($"Notebook name: {notebook.Attribute("name").Value}");
				logger.WriteLine($"Notebook link: {one.GetHyperlink(notebookId, null)}");
				logger.WriteLine();

				one.ReportWindowDiagnostics(logger);

				logger.WriteLine();

				var page = one.GetPage();
				var pageColor = page.GetPageColor(out _, out _);
				var pageBrightness = pageColor.GetBrightness();

				logger.WriteLine($"Page background: {pageColor.ToRGBHtml()}");
				logger.WriteLine($"Page brightness: {pageBrightness}");
				logger.WriteLine($"Page is dark...: {pageBrightness < 0.5}");

				(float dpiX, float dpiY) = UIHelper.GetDpiValues();
				logger.WriteLine($"Screen DPI.....: horizontal/X:{dpiX} vertical/Y:{dpiY}");

				(float scalingX, float scalingY) = UIHelper.GetScalingFactors();
				logger.WriteLine($"Scaling factors: horizontal/X:{scalingX} vertical/Y:{scalingY}");

				RemindCommand.ReportDiagnostics(logger);
				RemindScheduler.ReportDiagnostics(logger);

				logger.WriteLine(new string('-', 80));

				using (var dialog = new DiagnosticsDialog(logger.LogPath))
				{
					dialog.ShowDialog(owner);
				}
			}

			// turn headers back on
			logger.End();

			await Task.Yield();
		}
	}
}
