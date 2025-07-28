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
			var osarc = RuntimeInformation.OSArchitecture;
			var onarc = GetOneNoteArchitecture();
			var urarc = architecture;
			logger.WriteLine($"Install process architecture: {inarc}, OS:{osarc}, OneNote.exe:{onarc}, requesting:{urarc}");

			/*
			 * On Windows x64 with OneNote x64, must run OneMore x64 installer.
			 * On Windows x64 with OneNote x86, must run OneMore x86 installer.
			 */

			bool ok;
			if (osarc == Architecture.Arm64)
			{
				ok =
					(urarc == Architecture.Arm64) &&
					(onarc == Architecture.X64 || onarc == Architecture.Arm64)
					;
			}
			else if (osarc == Architecture.X64)
			{
				ok =
					(onarc == Architecture.X64 && urarc == Architecture.X64) ||
					(onarc == Architecture.X86 && urarc == Architecture.X86)
					;
			}
			else
			{
				ok =
					urarc == Architecture.X86 &&
					onarc == Architecture.X86
					;
			}

			if (!ok)
			{
				var msg = $"error: installer architecture ({urarc}) does not match OS ({osarc}) or OneNote ({onarc})";
				logger.WriteLine(msg);

				MessageBox.Show(
					$"This is a {urarc} installer.\nYou must use the OneMore {osarc} installer.",
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
