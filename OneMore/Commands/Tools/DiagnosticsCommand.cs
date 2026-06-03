//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Threading.Tasks;
	using Microsoft.Win32;
	using Newtonsoft.Json;
	using River.OneMoreAddIn.Cli;


	internal class DiagnosticsCommand : Command, ICliCommand
	{
		public DiagnosticsCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		#region CLI Implementation

		public string CommandName => "Diagnostics";


		public string Description => "Dump diagnostic information about OneNote and OneMore";


		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
				.AddBoolean("windows",
					"Return structured JSON describing all open OneNote windows",
					required: false, defaultValue: false);


		private sealed class CliLogger : Logger
		{
			private readonly StringBuilder buffer;

			public CliLogger(StringBuilder buffer)
			{
				this.buffer = buffer;
			}

			public override void Write(string message)
			{
				buffer.Append(message);
			}

			public override void WriteLine()
			{
				buffer.AppendLine();
			}

			public override void WriteLine(string message)
			{
				buffer.AppendLine(message);
			}
		}

		#endregion CLI Implementation


		public override async Task Execute(params object[] args)
		{
			var cliParams = args.Length > 0 ? args[0] as CliParameterSet : null;
			var windowsOnly = false;
			if (cliParams != null)
			{
				cliParams.TryGet("windows", out windowsOnly);
			}

			System.Text.StringBuilder cliBuffer = null;
			ILogger log;
			if (runningFromCli)
			{
				cliBuffer = new StringBuilder();
				log = new CliLogger(cliBuffer);
			}
			else
			{
				log = logger;
			}

			log.StartDiagnostic();

			if (runningFromCli)
			{
				log.WriteLine();
			}
			else
			{
				log.WriteLine("Diagnostics.Execute()");
			}

			log.WriteLine(new string('-', 80));

			var processes = Process.GetProcessesByName("ONENOTE");
			var moduledesc = "unknown";
			if (processes.Length > 0)
			{
				var module = processes[0].MainModule;
				var arc = RuntimeInformation.ProcessArchitecture.ToString();
				moduledesc = $"{module.FileName} ({module.FileVersionInfo.ProductVersion}) {arc}";
			}

			var ad = Assembly.GetExecutingAssembly();
			var adloc = ad.Location;
			var adarc = ad.GetName().ProcessorArchitecture;

			log.WriteLine($"Windows...: {GetWindowsProductName()}");
			log.WriteLine($"ONENOTE...: {moduledesc}");
			log.WriteLine($"Addin path: {adloc}, {adarc}");
			log.WriteLine($"Data path.: {PathHelper.GetAppDataPath()}");
			log.WriteLine($"Log path..: {log.LogPath}");
			log.WriteLine();

			await using var one = new OneNote();

			var (backupFolder, defaultFolder, unfiledFolder) = one.GetFolders();
			log.WriteLine($"Default path: {defaultFolder}");
			log.WriteLine($"Backup  path: {backupFolder}");
			log.WriteLine($"Unfiled path: {unfiledFolder}");
			log.WriteLine();

			var info = await one.GetPageInfo();
			if (info is not null)
			{
				log.WriteLine($"Page name: {info.Name}");
				log.WriteLine($"Page path: {info.Path}");
				log.WriteLine($"Page link: {info.Link}");
				log.WriteLine();
			}
			else
			{
				log.WriteLine($"runningFromCli: {runningFromCli}");
				log.WriteLine($"- PageInfo not found");
			}

			info = await one.GetSectionInfo();
			if (info is not null)
			{
				log.WriteLine($"Section name: {info.Name}");
				log.WriteLine($"Section path: {info.Path}");
				log.WriteLine($"Section link: {info.Link}");
				log.WriteLine();
			}
			else
			{
				log.WriteLine("- SectionInfo not found");
			}

			var notebook = await one.GetNotebook();
			if (notebook is not null)
			{
				var notebookId = one.CurrentNotebookId;
				log.WriteLine($"Notebook name: {notebook.Attribute("name")?.Value}");
				log.WriteLine($"Notebook link: {one.GetHyperlink(notebookId, null)}");
			}
			else
			{
				log.WriteLine("- NotebookInfo not found");
			}
			log.WriteLine();

			var windowJson = await one.CollectWindowDiagnostics();

			if (runningFromCli && windowsOnly)
			{
				CliOutput = windowJson;
				await Task.Yield();
				return;
			}

			WriteWindowDiagnostics(log, windowJson);

			log.WriteLine();

			var page = await one.GetPage();
			if (page != null)
			{
				var pageColor = page.GetPageColor(out _, out _);

				log.WriteLine($"Page background: {pageColor.ToRGBHtml()}");
				log.WriteLine($"Page brightness: {pageColor.GetBrightness()}");
				log.WriteLine($"Page bestText..: {page.GetBestTextColor().ToRGBHtml()}");
				log.WriteLine($"Page is dark...: {pageColor.IsDark()}");
			}

			(float dpiX, float dpiY) = UI.Scaling.GetDpiValues();
			log.WriteLine($"Screen DPI.....: horizontal/X:{dpiX} vertical/Y:{dpiY}");

			(float scalingX, float scalingY) = UI.Scaling.GetScalingFactors();
			log.WriteLine($"Scaling factors: horizontal/X:{scalingX} vertical/Y:{scalingY}");

			var magic = new UI.Scaling(100f, 100f);
			log.WriteLine($"Magic scaling..: ScalingX:{magic.ScalingX} ScalingY:{magic.ScalingY}");

			await RemindCommand.ReportDiagnostics(log);
			RemindScheduler.ReportDiagnostics(log);

			log.WriteLine(new string('-', 80));

			if (!runningFromCli)
			{
				using var dialog = new DiagnosticsDialog(logger.LogPath);
				dialog.ShowDialog(owner);
			}

			// turn headers back on
			log.End();

			if (runningFromCli)
			{
				CliOutput = cliBuffer.ToString();
			}

			await Task.Yield();
		}


		private static void WriteWindowDiagnostics(ILogger log, string json)
		{
			var windows = JsonConvert.DeserializeObject<List<Models.WindowInfo>>(json);
			if (windows is null || windows.Count == 0)
			{
				log.WriteLine("No windows");
				return;
			}

			var current = windows.FirstOrDefault(w => w.IsCurrent);
			if (current is null)
			{
				log.WriteLine("No current window");
			}
			else
			{
				log.WriteLine("Current Window");
				log.WriteLine($"- CurrentNotebookId: {current.CurrentNotebookId}");
				log.WriteLine($"- CurrentPageId....: {current.CurrentPageId}");
				log.WriteLine($"- CurrentSectionId.: {current.CurrentSectionId}");
				log.WriteLine($"- CurrentSecGrpId..: {current.CurrentSectionGroupId}");
				log.WriteLine($"- DockedLocation...: {current.DockedLocation}");
				log.WriteLine($"- IsFullPageView...: {current.IsFullPageView}");
				log.WriteLine($"- IsSideNote.......: {current.IsSideNote}");
				log.WriteLine($"- PID, TID, Handle.: {current.ProcessId}, {current.ThreadId}, {current.WindowHandle}");

				var b = current.Bounds;
				log.WriteLine($"- bounds...........: {b.Left},{b.Top},{b.Right},{b.Bottom}");
			}

			log.WriteLine();

			var others = windows.Where(w => !w.IsCurrent).ToList();
			if (others.Any())
			{
				log.WriteLine($"Other Windows ({others.Count})");
				foreach (var w in others)
				{
					log.Write(w.Active ? "*" : "-");
					log.Write($" window PID:{w.ProcessId}, TID:{w.ThreadId}");
					log.Write($" handle:{w.WindowHandle}");
					var wb = w.Bounds;
					log.WriteLine($" bounds:{wb.Left},{wb.Top},{wb.Right},{wb.Bottom}");
				}
			}
		}


		public static string GetWindowsEdition(Version version)
		{
			// on 32 bit Windows this will read the 32 bit hive instead
			using var hive = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
			using var key = hive.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion", false);
			return GetWindowsEdition(key, version);
		}


		private static string GetWindowsEdition(RegistryKey key, Version version)
		{

			// Windows 11 21H2 starts at Version >= 10.0.22000.120
			if (version.Major == 10 && version.Build >= 22000)
			{
				var editionID = key.GetValue("EditionID"); // e.g. "Professional"
				if (editionID is string edition && !string.IsNullOrWhiteSpace(edition))
				{
					return edition;
				}
			}
			else
			{
				// "Microsoft Windows XP"
				// "Windows 7 Ultimate"
				// "Windows 10 Pro"  (same string on Windows 11. Microsoft SUCKS!)
				return (string)key.GetValue("ProductName");
			}

			return string.Empty;
		}


		public static string GetWindowsProductName()
		{
			var name = new StringBuilder();

			try
			{
				// on 32 bit Windows this will read the 32 bit hive instead
				using var hive = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
				using var key = hive.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion", false);

				var os = Version.Parse(RuntimeInformation.OSDescription.Split(' ')[2]);
				var edition = GetWindowsEdition(key, os);

				// Windows 11 21H2 starts at Version >= 10.0.22000.120
				if (os.Major == 10 && os.Build >= 22000)
				{
					name.Append("Windows 11");

					if (!string.IsNullOrWhiteSpace(edition))
					{
						name.Append($" {edition}");
					}
				}
				else
				{
					name.Append(edition);
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

				name.Append($", Build {os.Build}");
				name.Append(Environment.Is64BitOperatingSystem ? ", x64" : ", x86");

				if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
				{
					name.Append(", (ARM64)");
				}
			}
			catch (Exception exc)
			{
				name.Append(exc.Message);
			}

			return name.ToString();
		}
	}
}
