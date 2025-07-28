//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace OneMoreSetupActions
{
	using Microsoft.Win32;
	using System;
	using System.IO;
	using System.Reflection.PortableExecutable;
	using System.Runtime.InteropServices;
	using System.Windows.Forms;


	/// <summary>
	/// Confirms that the installer bitness matches the OS bitness
	/// </summary>
	internal class CheckBitnessAction : CustomAction
	{
		private readonly Architecture architecture;


		public CheckBitnessAction(Logger logger, Stepper stepper, Architecture architecture)
			: base(logger, stepper)
		{
			this.architecture = architecture;
		}


		public override int Install()
		{
			logger.WriteLine();
			logger.WriteLine("CheckBitnessAction.Install ---");

			var inarc = Environment.Is64BitProcess ? Architecture.X64 : Architecture.X86;
			var oparc = RuntimeInformation.OSArchitecture;
			var onarc = GetOneNoteArchitecture();
			var urarc = architecture;
			logger.WriteLine($"Install process architecture: {inarc}, OS:{oparc}, OneNote.exe:{onarc}, requesting:{urarc}");

			/*
			 * Either x86 or x64 OneMore can run with x64 OneNote.
			 */

			var ok =
				// 32-bit installer can run on X86 OS
				(inarc == Architecture.X86 && oparc == Architecture.X86) ||
				// 64-bit installer can run on x64 or Arm64 OS
				(inarc == Architecture.X64 && oparc != Architecture.X86) ||
				// Requesting x86 install for x86 OneNote
				(urarc == Architecture.X86 && onarc == Architecture.X86) ||
				// Requesting x64 install for x64 or Arm64 OneNote
				(urarc == Architecture.X64 && onarc != Architecture.X86)
				;

			if (!ok)
			{
				var msg = $"Installer architecture ({inarc}) does not match OS ({oparc}) or request ({urarc})";
				logger.WriteLine(msg);

				MessageBox.Show(
					$"This is a {inarc} bit installer.\nYou must use the OneMore {oparc} bit installer.",
					"Incompatible Installer",
					MessageBoxButtons.OK, MessageBoxIcon.Error);

				return FAILURE;
			}

			logger.WriteLine("OK - bitness check passed");
			return SUCCESS;
		}


		private Architecture GetOneNoteArchitecture()
		{
			logger.WriteLine(nameof(GetOneNoteArchitecture) + "()");
			logger.Indented = true;

			// [HKEY_CLASSES_ROOT\OneNote.Application]
			// @= "Application Class"
			var path = @"OneNote.Application\CLSID";
			var key = Registry.ClassesRoot.OpenSubKey(path, false);

			if (key == null)
			{
				logger.WriteLine($"error finding HKCR:\\{path}");
				return Architecture.Arm;
			}

			var clsid = key.GetValue(null, string.Empty) as string;
			key.Dispose();

			path = @$"CLSID\{clsid}\LocalServer32";
			key = Registry.ClassesRoot.OpenSubKey(path, false);
			if (key == null)
			{
				logger.WriteLine($"error finding HKCR:\\{path}");
				return Architecture.Arm;
			}

			var onepath = key.GetValue(null, string.Empty) as string;
			key.Dispose();

			if (onepath == null)
			{
				logger.WriteLine($"error reading {path} value");
				return Architecture.Arm;
			}

			if (!File.Exists(onepath))
			{
				logger.WriteLine($"error finding executable at {onepath}");
				return Architecture.Arm;
			}

			using var stream = new FileStream(onepath, FileMode.Open, FileAccess.Read);
			using var reader = new PEReader(stream);
			var onearc = reader.PEHeaders.CoffHeader.Machine switch
			{
				Machine.I386 => Architecture.X86,
				Machine.Arm64 => Architecture.Arm64,
				_ => Architecture.X64
			};

			logger.Indented = false;
			return onearc;
		}


		public override int Uninstall()
		{
			return SUCCESS;
		}
	}
}
