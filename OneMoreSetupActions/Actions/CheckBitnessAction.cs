//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreSetupActions
{
	using System;
	using System.Windows.Forms;


	/// <summary>
	/// Confirms that the installer bitness matches the OS bitness
	/// </summary>
	internal class CheckBitnessAction : CustomAction
	{
		private readonly bool x64;


		public CheckBitnessAction(Logger logger, Stepper stepper, bool x64)
			: base(logger, stepper)
		{
			this.x64 = x64;
		}


		public override int Install()
		{
			logger.WriteLine();
			logger.WriteLine("CheckBitnessAction.Install ---");

			var oarc = Environment.Is64BitOperatingSystem ? "x64" : "x86";
			var iarc = Environment.Is64BitProcess ? "x64" : "x86";
			var rarc = x64 ? "x64" : "x86";
			logger.WriteLine($"Installer architecture ({iarc}), OS architecture ({oarc}), requesting ({rarc})");

			if (Environment.Is64BitOperatingSystem != Environment.Is64BitProcess ||
				Environment.Is64BitOperatingSystem != x64)
			{
				var msg = $"Installer architecture ({iarc}) does not match OS ({oarc}) or request ({rarc})";
				logger.WriteLine(msg);

				MessageBox.Show(
					$"This is a {iarc} bit installer.\nYou must use the OneMore {oarc} bit installer.",
					"Incompatible Installer",
					MessageBoxButtons.OK, MessageBoxIcon.Error);

				return FAILURE;
			}

			return SUCCESS;
		}


		public override int Uninstall()
		{
			return SUCCESS;
		}
	}
}
