//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Microsoft.Win32;
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Reflection;
	using System.Text;
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
			var moduledesc = "unknown";
			if (processes.Length > 0)
			{
				var module = processes[0].MainModule;
				moduledesc = $"{module.FileName} ({module.FileVersionInfo.ProductVersion})";
			}

			logger.WriteLine($"Windows...: {GetWindowsProductName()}");
			logger.WriteLine($"ONENOTE...: {moduledesc}");
			logger.WriteLine($"Addin path: {Assembly.GetExecutingAssembly().Location}");
			logger.WriteLine($"Data path.: {PathHelper.GetAppDataPath()}");
			logger.WriteLine($"Log path..: {logger.LogPath}");
			logger.WriteLine();

			using var one = new OneNote();

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

			var notebook = await one.GetNotebook();
			var notebookId = one.CurrentNotebookId;
			logger.WriteLine($"Notebook name: {notebook.Attribute("name").Value}");
			logger.WriteLine($"Notebook link: {one.GetHyperlink(notebookId, null)}");
			logger.WriteLine();

			one.ReportWindowDiagnostics(logger);

			logger.WriteLine();

			var page = one.GetPage();
			var pageColor = page.GetPageColor(out _, out _);

			logger.WriteLine($"Page background: {pageColor.ToRGBHtml()}");
			logger.WriteLine($"Page brightness: {pageColor.GetBrightness()}");
			logger.WriteLine($"Page bestText..: {page.GetBestTextColor().ToRGBHtml()}");
			logger.WriteLine($"Page is dark...: {pageColor.IsDark()}");

			(float dpiX, float dpiY) = UIHelper.GetDpiValues();
			logger.WriteLine($"Screen DPI.....: horizontal/X:{dpiX} vertical/Y:{dpiY}");

			(float scalingX, float scalingY) = UIHelper.GetScalingFactors();
			logger.WriteLine($"Scaling factors: horizontal/X:{scalingX} vertical/Y:{scalingY}");

			var magic = new MagicScaling(100f, 100f);
			logger.WriteLine($"Magic scaling..: ScalingX:{magic.ScalingX} ScalingY:{magic.ScalingY}");

			RemindCommand.ReportDiagnostics(logger);
			RemindScheduler.ReportDiagnostics(logger);

			logger.WriteLine(new string('-', 80));

			using var dialog = new DiagnosticsDialog(logger.LogPath);
			dialog.ShowDialog(owner);

			// turn headers back on
			logger.End();

			await Task.Yield();
		}


		public static string GetWindowsProductName()
		{
			var name = new StringBuilder();

			try
			{
				// on 32 bit Windows this will read the 32 bit hive instead
				using var hive = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
				using var key = hive.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion", false);

				var kernel = FileVersionInfo.GetVersionInfo(
					Path.Combine(Environment.SystemDirectory, "Kernel32.dll"));

				// Kernel32.dll on Windows 11 has Product Version >= 10.0.22000.120
				if (kernel.ProductMajorPart == 10 && kernel.ProductBuildPart >= 22000)
				{
					name.Append("Windows 11");

					var editionID = key.GetValue("EditionID"); // e.g. "Professional"
					if (editionID is string edition && !string.IsNullOrWhiteSpace(edition))
					{
						name.Append($" {edition}");
					}
				}
				else
				{
					// "Microsoft Windows XP"
					// "Windows 7 Ultimate"
					// "Windows 10 Pro"  (same string on Windows 11. Microsoft SUCKS!)
					name.Append((string)key.GetValue("ProductName"));
				}

				// see: https://en.wikipedia.org/wiki/Windows_10_version_history

				// Windows 10 latest release --> "21H1"
				// Windows 11 first  release --> "21H2"
				var displayVersion = key.GetValue("DisplayVersion");
				if (displayVersion is string display && !string.IsNullOrWhiteSpace(display))
				{
					name.Append($", Version {display}");
				}
				else
				{
					// Windows 10 older releases --> "2009" (invalid if DisplayVersion exists)
					var releaseID = key.GetValue("ReleaseId");
					if (releaseID is string release && !string.IsNullOrWhiteSpace(release))
					{
						name.Append($", Version {release}");
					}
				}

				name.Append($", Build {kernel.ProductBuildPart}");
				name.Append(Environment.Is64BitOperatingSystem ? ", 64 bit" : ", 32 bit");

			}
			catch (Exception exc)
			{
				name.Append(exc.Message);
			}

			return name.ToString();
		}
	}
}
