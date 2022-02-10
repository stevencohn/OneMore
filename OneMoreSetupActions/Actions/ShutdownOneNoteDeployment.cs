//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreSetupActions
{
	using System.Diagnostics;
	using System.Linq;

	internal class ShutdownOneNoteDeployment : Deployment
	{
		private const string OneNoteName = "ONENOTE";


		public ShutdownOneNoteDeployment(Logger logger, Stepper stepper)
			: base(logger, stepper)
		{
		}


		public override int Install()
		{
			logger.WriteLine();
			logger.WriteLine("ShutdownOneNoteDeployment.Install ---");
			return StopProcess();
		}


		public override int Uninstall()
		{
			logger.WriteLine();
			logger.WriteLine("ShutdownOneNoteDeployment.Uninstall ---");

			var status = FAILURE;
			var tries = 0;

			while (status == FAILURE && tries < 2)
			{
				status = StopProcess();
				tries++;
			}

			return status;
		}


		private int StopProcess()
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
						logger.WriteLine("process termination confirmed");
					}
					else
					{
						logger.WriteLine("process still running after 3 seconds");
						status = FAILURE;
					}
				}
			}

			return status;
		}
	}
}
