//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers
{
	using System;
	using System.Diagnostics;
	using System.Management;
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

			logger.WriteLine();
			logger.Start(
				$"Starting {process.ProcessName} {process.Id}, {cpu} Mhz, {uram}, " +
				$"{thread.CurrentCulture.Name}/{thread.CurrentUICulture.Name}, " +
				$"v{AssemblyInfo.Version}, OneNote {Office.Office.GetOneNoteVersion()}, " +
				$"Office {Office.Office.GetOfficeVersion()}, " +
				DateTime.Now.ToString("yyyy-MM-dd HH:mm"));

			logger.WriteLine(Commands.DiagnosticsCommand.GetWindowsProductName());

			var hostproc = Process.GetProcessesByName("ONENOTE");
			if (hostproc.Length > 0)
			{
				var module = hostproc[0].MainModule;
				logger.WriteLine($"{module.FileName} ({module.FileVersionInfo.ProductVersion})");
			}
		}


		private static (uint, double) GetMachineProps()
		{
			// using this as a means of short-circuiting the Ensure methods for slower machines
			// to speed up the display of the menus. CurrentClockSpeed will vary depending on
			// battery capacity and other factors, whereas MaxClockSpeed is a constant

			uint speed = ReasonableClockSpeed;
			using (var searcher =
				new ManagementObjectSearcher("select CurrentClockSpeed from Win32_Processor"))
			{
				foreach (var item in searcher.Get())
				{
					speed = Convert.ToUInt32(item["CurrentClockSpeed"]);
					item.Dispose();
				}
			}

			if (speed == 0) speed = ReasonableClockSpeed;

			// returns total RAM across all physical slots; as KB so convert to bytes
			//var memory = Query<ulong>("MaxCapacityEx", "Win32_PhysicalMemoryArray") * 1024;
			//var memory = Query<ulong>("Capacity", "Win32_PhysicalMemory") * 1024;

			//var memory = Math.Ceiling(Query<double>(
			//	"*", "Win32_OperatingSystem", "TotalVisibleMemorySize") * 1024);

			double memory = 0;
			using (var searcher =
				new ManagementObjectSearcher("select * from Win32_OperatingSystem"))
			{
				foreach (var item in searcher.Get())
				{
					memory = Convert.ToDouble(item["TotalVisibleMemorySize"]);
					item.Dispose();
				}
			}

			return (speed, memory);
		}
	}
}
