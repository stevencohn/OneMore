//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S3267 // ignore loop converstion to LINQ

namespace OneMoreSetupActions
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Management;


	/// <summary>
	/// Prep step in the installer to force shutdown of OneNote.
	/// </summary>
	internal class ShutdownOneNoteAction : CustomAction
	{
		private const string OneNoteName = "ONENOTE";
		private const string DllHostName = "dllhost";


		public ShutdownOneNoteAction(Logger logger, Stepper stepper)
			: base(logger, stepper)
		{
		}


		/// <summary>
		/// Stops the C2R service, shuts down the COM surrogate host and OneNote.
		/// </summary>
		public override int Install()
		{
			logger.WriteLine();
			logger.WriteLine("ShutdownOneNoteAction.Install ---");

			var status = StopHost();
			if (status == SUCCESS)
			{
				status = StopOneNote();
			}

			return status;
		}


		/// <summary>
		/// Stops the C2R service, shuts down the COM surrogate host and OneNote
		/// (retried once on failure).
		/// </summary>
		public override int Uninstall()
		{
			logger.WriteLine();
			logger.WriteLine("ShutdownOneNoteAction.Uninstall ---");

			var status = FAILURE;
			var tries = 0;

			while (status == FAILURE && tries < 2)
			{
				status = StopHost();
				if (status == SUCCESS)
				{
					status = StopOneNote();
				}

				tries++;
			}

			return status;
		}


		/// <summary>
		/// Finds and kills all dllhost instances running the OneMore COM surrogate,
		/// then verifies they are gone via a second WMI query.
		/// </summary>
		private int StopHost()
		{
			var sql =
				"SELECT ProcessID, CommandLine FROM Win32_Process " +
				$"WHERE CommandLine LIKE '%{RegistryHelper.OneMoreID}%'";

			var pids = GetHostProcessIds(sql);
			if (pids.Count == 0)
			{
				logger.WriteLine($"{DllHostName} process not found");
				return SUCCESS;
			}

			var status = SUCCESS;
			foreach (var pid in pids)
			{
				if (!KillHostProcess(pid))
				{
					status = FAILURE;
				}
			}

			if (status == SUCCESS)
			{
				status = VerifyHostStopped(sql);
			}

			return status;
		}


		/// <summary>
		/// Returns the PIDs of dllhost processes whose command line contains the OneMore CLSID.
		/// Uses WMI rather than Process.GetProcessesByName because there can be many dllhost
		/// instances on a system; the command line is the only reliable discriminator.
		/// </summary>
		private List<int> GetHostProcessIds(string sql)
		{
			var pids = new List<int>();
			try
			{
				using var searcher = new ManagementObjectSearcher(sql);
				using var collection = searcher.Get();
				foreach (var item in collection)
				{
					pids.Add((int)(uint)item["ProcessID"]);
					item.Dispose();
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("failed to query host processes");
				logger.WriteLine(exc);
			}
			return pids;
		}


		/// <summary>
		/// Attempts a graceful close of the given process, falling back to a hard kill after
		/// 5 seconds. Returns true if the process is gone, including if it had already exited
		/// (ArgumentException from GetProcessById is treated as success, not an error).
		/// </summary>
		private bool KillHostProcess(int pid)
		{
			try
			{
				using var process = Process.GetProcessById(pid);
				logger.WriteLine($"stopping process {DllHostName}, pid {pid}");

				process.CloseMainWindow();
				if (!process.WaitForExit(5000))
				{
					logger.WriteLine($"{DllHostName} did not close gracefully, killing");
					process.Kill();
				}

				if (process.WaitForExit(3000))
				{
					return true;
				}

				logger.WriteLine($"failed to stop {DllHostName} pid {pid}");
				return false;
			}
			catch (ArgumentException)
			{
				logger.WriteLine($"{DllHostName} pid {pid} already exited");
				return true;
			}
			catch (Exception exc)
			{
				logger.WriteLine($"failed to stop {DllHostName} pid {pid}");
				logger.WriteLine(exc);
				return false;
			}
		}


		/// <summary>
		/// Re-runs the WMI query to confirm no matching dllhost processes remain after the kill.
		/// </summary>
		private int VerifyHostStopped(string sql)
		{
			try
			{
				using var searcher = new ManagementObjectSearcher(sql);
				using var collection = searcher.Get();

				if (collection.Count == 0)
				{
					logger.WriteLine($"{DllHostName} process termination confirmed");
					return SUCCESS;
				}

				logger.WriteLine($"{DllHostName} still running after termination");
				return FAILURE;
			}
			catch (Exception exc)
			{
				logger.WriteLine("failed to verify host process termination");
				logger.WriteLine(exc);
				return FAILURE;
			}
		}


		/// <summary>
		/// Kills the ONENOTE.EXE process and verifies it has exited.
		/// </summary>
		private int StopOneNote()
		{
			var status = SUCCESS;
			var killed = false;

			using (var process = Process.GetProcessesByName(OneNoteName).FirstOrDefault())
			{
				if (process == null)
				{
					logger.WriteLine($"{OneNoteName} process not found");
				}
				else
				{
					logger.WriteLine($"stopping process {OneNoteName}, pid {process.Id}");
					process.Kill();
					killed = true;

					if (!process.WaitForExit(3000))
					{
						status = FAILURE;
					}
				}
			}

			if (killed && status == SUCCESS)
			{
				using (var process = Process.GetProcessesByName(OneNoteName).FirstOrDefault())
				{
					if (process == null)
					{
						logger.WriteLine($"{OneNoteName} process termination confirmed");
					}
					else
					{
						logger.WriteLine($"{OneNoteName} process still running after 3 seconds");
						status = FAILURE;
					}
				}
			}

			return status;
		}
	}
}
