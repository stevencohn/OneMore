//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace OneMoreSetupActions
{
	using Microsoft.Win32;
	using System;
	using System.IO;
	using System.Linq;
	using System.Reflection.PortableExecutable;
	using System.Runtime.InteropServices;
	using System.Text.RegularExpressions;
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


		public Architecture OneNoteArchitecture { get; private set; }


		public override int Install()
		{
			logger.WriteLine();
			logger.WriteLine("CheckBitnessAction.Install ---");

			var inarc = Environment.Is64BitProcess ? Architecture.X64 : Architecture.X86;
			var osarc = RuntimeInformation.OSArchitecture;
			var onarc = GetOneNoteArchitecture();
			var urarc = architecture;
			logger.WriteLine($"... Install process architecture:{inarc}, OS:{osarc}, OneNote.exe:{onarc}, requesting:{urarc}");

			/*
			 * On Windows x64 with OneNote x64, must run OneMore x64 installer.
			 * On Windows x64 with OneNote x86, must run OneMore x86 installer.
			 * On Windows x86 must run OneMore x86 installer.
			 * On Windows ARM64, OneMore installer must match OneNote, either ARM64 or x64.
			 */

			bool ok;
			if (osarc == Architecture.Arm64)
			{
				ok =
					(onarc == Architecture.X64 && urarc == Architecture.X64) ||
					(onarc == Architecture.Arm64 && urarc == Architecture.Arm64)
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

			OneNoteArchitecture = onarc;

			if (!ok)
			{
				var msg = $"error: installer architecture ({urarc}) does not match OS ({osarc}) or OneNote ({onarc})";
				logger.WriteLine(msg);

				MessageBox.Show(
					$"This is a {urarc} installer.\nYou must use the OneMore {onarc} installer.",
					"Incompatible Installer",
					MessageBoxButtons.OK, MessageBoxIcon.Error);

				return FAILURE;
			}

			logger.WriteLine("... OK - bitness check passed");
			return SUCCESS;
		}


		private Architecture GetOneNoteArchitecture()
		{
			var onepath = GetOneNotePath();

			if (string.IsNullOrWhiteSpace(onepath))
			{
				logger.WriteLine($"error finding OneNote.exe path");
				return Architecture.Arm;
			}

			if (!File.Exists(onepath))
			{
				logger.WriteLine($"error OneNote.exe not found at {onepath}");
				return Architecture.Arm;
			}

			var onearc = Architecture.Arm; // Arm is actually unused

			try
			{
				using var stream = new FileStream(onepath, FileMode.Open, FileAccess.Read);
				using var reader = new PEReader(stream);
				onearc = reader.PEHeaders.CoffHeader.Machine switch
				{
					Machine.I386 => Architecture.X86,
					Machine.Arm64 => Architecture.Arm64,
					_ => Architecture.X64
				};
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error reading OneNote.exe header");
				logger.WriteLine(exc);
			}

			logger.Indented = false;
			return onearc;
		}


		private string GetOneNotePath()
		{
			string ReadDefaultValue(string path)
			{
				using var key = Registry.LocalMachine.OpenSubKey(path, false);
				if (key == null)
				{
					logger.WriteLine($"warn finding HKLM:\\{path}");
					return null;
				}

				var value = key.GetValue(null, string.Empty) as string;
				if (string.IsNullOrWhiteSpace(value))
				{
					logger.WriteLine($"warn finding value at HKLM:\\{path}");
					return null;
				}

				return value;
			}

			string ReadAppPathValue(string path, string subname)
			{
				using var key = Registry.LocalMachine.OpenSubKey(path, false);
				if (key == null)
				{
					logger.WriteLine($"warn finding HKLM:\\{path}");
					return null;
				}

				foreach (var name in key.GetSubKeyNames()
					.Where(n => Regex.IsMatch(n, @"^\d+\.\d+$")))
				{
					using var subkey = key.OpenSubKey($@"{name}\{subname}\InstallRoot", false);
					if (subkey != null)
					{
						var value = subkey.GetValue("Path") as string;
						if (!string.IsNullOrWhiteSpace(value))
						{
							logger.WriteLine($@"found Path at HKLM:\{path}\{subname}\InstallRoot");
							logger.WriteLine($@"Path is {value}\ONENOTE.EXE");
							return Path.Combine(value, "ONENOTE.EXE");
						}
					}

					logger.WriteLine($@"warn finding value at HKLM:\{path}\{subname}\InstallRoot");
				}
				return null;
			}

			logger.WriteLine(nameof(GetOneNoteArchitecture) + "()");
			logger.Indented = true;

			var onepath = ReadDefaultValue(
				@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\OneNote.exe");

			if (string.IsNullOrWhiteSpace(onepath))
			{
				onepath = ReadDefaultValue(
					@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\App Paths\OneNote.exe");

				if (string.IsNullOrWhiteSpace(onepath))
				{
					onepath = ReadAppPathValue(@"SOFTWARE\Microsoft\Office", "OneNote");
					if (string.IsNullOrWhiteSpace(onepath))
					{
						onepath = ReadAppPathValue(@"SOFTWARE\WOW6432Node\Microsoft\Office", "OneNote");
						if (string.IsNullOrWhiteSpace(onepath))
						{
							onepath = ReadAppPathValue(@"SOFTWARE\Microsoft\Office", "Common");
							if (string.IsNullOrWhiteSpace(onepath))
							{
								onepath = ReadAppPathValue(@"SOFTWARE\WOW6432Node\Microsoft\Office", "Common");
							}
						}
					}
				}
			}

			return onepath;
		}


		public override int Uninstall()
		{
			return SUCCESS;
		}
	}
}
