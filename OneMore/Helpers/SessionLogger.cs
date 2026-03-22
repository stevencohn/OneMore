//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers
{
	using River.OneMoreAddIn.Commands;
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Management;
	using System.Reflection;
	using System.Reflection.PortableExecutable;
	using System.Runtime.InteropServices;
	using System.Threading;

	internal static class SessionLogger
	{
		private const uint ReasonableClockSpeed = 1800;


		public static void WriteSessionHeader()
		{
			var logger = Logger.Current;

			var (cpu, ram) = GetMachineProps();
			var uram = ram > ulong.MaxValue ? ">GB" : ((ulong)ram).ToBytes();

			var process = Process.GetCurrentProcess();
			var thread = Thread.CurrentThread;

			var codebase = Assembly.GetExecutingAssembly().CodeBase;
			var arc = GetAssemblyArchitecture(new Uri(codebase).LocalPath);

			logger.WriteLine();
			logger.Start(
				$"Starting {process.ProcessName} {process.Id}, {cpu} Mhz, {uram}, " +
				$"{thread.CurrentCulture.Name}/{thread.CurrentUICulture.Name}, " +
				$"v{AssemblyInfo.Version} {arc}, " +
				$"{DateTime.Now.ToString("yyyy-MM-dd HH:mm")} " +
				$"[{TelemetryClient.Template.SessionId}]");

			logger.WriteLine(DescribeProducts());
		}


		private static (uint, double) GetMachineProps()
		{
			// using this as a means of short-circuiting the Ensure methods for slower machines
			// to speed up the display of the menus. CurrentClockSpeed will vary depending on
			// battery capacity and other factors, whereas MaxClockSpeed is a constant

			uint speed = ReasonableClockSpeed;

			try
			{
				using var searcher =
					new ManagementObjectSearcher("select CurrentClockSpeed from Win32_Processor");

				foreach (var item in searcher.Get())
				{
					speed = Convert.ToUInt32(item["CurrentClockSpeed"]);
					item.Dispose();
				}
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine(
					"error reading CurrentClockSpeed from Win32_Processor", exc);
			}

			if (speed == 0) speed = ReasonableClockSpeed;

			// returns total RAM across all physical slots; as KB so convert to bytes
			//var memory = Query<ulong>("MaxCapacityEx", "Win32_PhysicalMemoryArray") * 1024;
			//var memory = Query<ulong>("Capacity", "Win32_PhysicalMemory") * 1024;

			//var memory = Math.Ceiling(Query<double>(
			//	"*", "Win32_OperatingSystem", "TotalVisibleMemorySize") * 1024);

			double memory = 0;

			try
			{
				using var searcher =
					new ManagementObjectSearcher("select * from Win32_OperatingSystem");

				foreach (var item in searcher.Get())
				{
					memory = Convert.ToDouble(item["TotalVisibleMemorySize"]);
					item.Dispose();
				}
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine(
					"error reading TotalVisibleMemorySize from Win32_OperatingSystem", exc);
			}

			return (speed, memory);
		}


		public static string GetAssemblyArchitecture(string path)
		{
			try
			{
				using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
				using var reader = new PEReader(stream);
				return reader.PEHeaders.CoffHeader.Machine switch
				{
					Machine.I386 => "x86",
					Machine.Arm64 => "ARM64",
					_ => "x64"
				};
			}
			catch (Exception exc)
			{
				return $"error reading header: {exc.Message}";
			}
		}


		private static string DescribeProducts()
		{
			var hostproc = Process.GetProcessesByName("ONENOTE");
			if (hostproc.Length == 0)
			{
				return "could not read ONENOTE.EXE process information";
			}

			try
			{
				var module = hostproc[0].MainModule;
				var arc = GetAssemblyArchitecture(module.FileName);
				var win = Commands.DiagnosticsCommand.GetWindowsProductName();

				return $"OneNote {Office.Office.GetOneNoteVersion()} " + 
					$"({module.FileVersionInfo.ProductVersion} {arc}), " +
					$"Office {Office.Office.GetOfficeVersion()} | {win}";
			}
			catch (Exception exc)
			{
				return $"error reading OneNote.exe header: {exc.Message}";
			}
		}


		/// <summary>
		/// Collects diagnostic properties related to the current application, OneNote
		/// process, and system environment. Used by TelemetryClient to read and cache
		/// once during the session.
		/// </summary>
		/// <remarks>
		/// <returns>
		/// The dictionary is empty if the OneNote process is not found or if an
		/// error occurs while retrieving process information.
		/// </returns>
		public static TelemetryClient.TelemetryEvent MakeTelemetryTemplate()
		{
			var hostproc = Process.GetProcessesByName("ONENOTE");
			if (hostproc.Length == 0) { return null; }
			string oneArc;
			string oneVer;
			try
			{
				var module = hostproc[0].MainModule;
				oneArc = GetAssemblyArchitecture(module.FileName);
				oneVer = module.FileVersionInfo.ProductVersion;
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine("error reading OneNote.exe header", exc);
				return null;
			}

			// collect...

			var winver = Version.Parse(RuntimeInformation.OSDescription.Split(' ')[2]);

			var winarc = RuntimeInformation.OSArchitecture == Architecture.Arm64
				? "ARM64" : (Environment.Is64BitOperatingSystem ? ", x64" : ", x86");

			var codebase = Assembly.GetExecutingAssembly().CodeBase;

			return new TelemetryClient.TelemetryEvent
			{
				Version = AssemblyInfo.Version,
				SessionId = Guid.NewGuid().ToString("N"),
				Client = new TelemetryClient.ClientInfo
				{
					OneVersion = oneVer,
					OneArc = oneArc,
					MoreArc = GetAssemblyArchitecture(new Uri(codebase).LocalPath),
					OsMajor = winver.Major,
					OsMinor = winver.Minor,
					OsBuild = winver.Build,
					OsEdition = DiagnosticsCommand.GetWindowsEdition(winver),
					OsArc = winarc,

					Culture = Thread.CurrentThread.CurrentCulture.Name
				}
			};
		}
	}
}