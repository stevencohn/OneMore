//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreSetupActions
{
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


		private int StopHost()
		{
			var status = SUCCESS;
			var killed = false;

			var sql = 
				"SELECT ProcessID, CommandLine FROM Win32_Process " +
				$"WHERE CommandLine LIKE '%{RegistryHelper.OneNoteID}%'";

			using (var searcher = new ManagementObjectSearcher(sql))
			{
				using (var collection = searcher.Get())
				{
					if (collection.Count > 0)
					{
						foreach (var item in collection)
						{
							var processID = (int)(uint)item["ProcessID"];
							item.Dispose();

							using (var process = Process.GetProcessById(processID))
							{
								if (process == null)
								{
									logger.WriteLine($"{DllHostName} process not found");
								}
								else
								{
									logger.WriteLine($"stopping process {DllHostName}, pid {process.Id}");
									process.Kill();
									killed = true;

									if (!process.WaitForExit(3000))
									{
										status = FAILURE;
									}
								}
							}
						}
					}
					else
					{
						logger.WriteLine($"{DllHostName} process not found");
					}
				}
			}

			if (killed && status == SUCCESS)
			{
				using (var searcher = new ManagementObjectSearcher(sql))
				{
					using (var collection = searcher.Get())
					{
						if (collection.Count == 0)
						{
							logger.WriteLine($"{DllHostName} process termination confirmed");
						}
						else
						{
							logger.WriteLine($"{DllHostName} process still running after 3 seconds");
							status = FAILURE;
						}
					}
				}
			}

			return status;
		}


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
