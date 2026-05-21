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


		/// <summary>
		/// Validates that the installer architecture is compatible with the OS and OneNote.
		/// Returns FAILURE with a user-facing MessageBox if OneNote is not found or the
		/// wrong installer bitness was used; sets OneNoteArchitecture for downstream actions.
		/// </summary>
		public override int Install()
		{
			logger.WriteLine();
			logger.WriteLine("CheckBitnessAction.Install ---");

			var inarc = RuntimeInformation.ProcessArchitecture; // F4: was Environment.Is64BitProcess
			var osarc = RuntimeInformation.OSArchitecture;
			var onarc = GetOneNoteArchitecture(); // F2: returns Architecture?
			var urarc = architecture;

			// F2: distinguish "OneNote not found" from a genuine bitness mismatch
			if (onarc is null)
			{
				logger.WriteLine($"... Install process architecture:{inarc}, OS:{osarc}, OneNote.exe:not detected, requesting:{urarc}");
				logger.WriteLine("error: OneNote Desktop installation was not detected");

				MessageBox.Show(
					"OneNote Desktop was not detected on this system.\n" +
					"OneMore requires OneNote Desktop, not the Microsoft Store app.\n" +
					"Please install OneNote Desktop and try again.",
					"OneNote Not Found",
					MessageBoxButtons.OK, MessageBoxIcon.Error);

				return FAILURE;
			}

			logger.WriteLine($"... Install process architecture:{inarc}, OS:{osarc}, OneNote.exe:{onarc}, requesting:{urarc}");

			/*
			 * On Windows x64 with OneNote x64, must run OneMore x64 installer.
			 * On Windows x64 with OneNote x86, must run OneMore x86 installer.
			 * On Windows x86 must run OneMore x86 installer.
			 * On Windows ARM64, OneMore installer must match OneNote, either ARM64 or x64.
			 * ARM64EC OneNote (Office on ARM64) has Machine.Amd64 in its PE header but is
			 * detected heuristically as Arm64; both ARM64 and x64 installers are accepted.
			 */

			bool ok;
			if (osarc == Architecture.Arm64)
			{
				ok =
					(onarc == Architecture.Arm64 && (urarc == Architecture.Arm64 || urarc == Architecture.X64)) ||
					(onarc == Architecture.X64 && urarc == Architecture.X64)
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

			OneNoteArchitecture = onarc.Value;

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


		/// <summary>
		/// Reads ONENOTE.EXE's PE header to determine its architecture. Returns null if
		/// OneNote Desktop is not installed or the header cannot be read. Applies the
		/// ARM64EC heuristic: Machine.Amd64 on an ARM64 OS is treated as Arm64.
		/// </summary>
		private Architecture? GetOneNoteArchitecture()
		{
			var onepath = GetOneNotePath();

			if (string.IsNullOrWhiteSpace(onepath))
			{
				logger.Indented = false; // F5
				logger.WriteLine("error finding OneNote.exe path");
				return null;
			}

			if (!File.Exists(onepath))
			{
				logger.Indented = false; // F5
				logger.WriteLine($"error OneNote.exe not found at {onepath}");
				return null;
			}

			Architecture? onearc = null;

			try
			{
				using var stream = new FileStream(onepath, FileMode.Open, FileAccess.Read);
				using var reader = new PEReader(stream);
				var machine = reader.PEHeaders.CoffHeader.Machine;

				onearc = machine switch
				{
					Machine.I386 => Architecture.X86,
					Machine.Arm64 => Architecture.Arm64,
					// F1: ARM64EC (Office on ARM64) reports Machine.Amd64 but runs natively on ARM64.
					// Treat as Arm64 so both the ARM64 and x64 OneMore installers are accepted.
					Machine.Amd64 when RuntimeInformation.OSArchitecture == Architecture.Arm64
						=> Architecture.Arm64,
					_ => Architecture.X64
				};
			}
			catch (Exception exc)
			{
				logger.WriteLine("error reading OneNote.exe header");
				logger.WriteLine(exc);
			}

			logger.Indented = false;
			return onearc;
		}


		/// <summary>
		/// Locates ONENOTE.EXE by trying multiple registry paths in order of preference,
		/// covering all supported Office configurations (ARM64, x64, x86, Click-to-Run,
		/// MSI-based) on all supported Windows architectures.
		/// </summary>
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

			var sub = @"Microsoft\Windows\CurrentVersion\App Paths\OneNote.exe";

			// F3: removed WOWAA64Node lookups — that registry node does not exist on any shipping
			// Windows version (confirmed in issue #1981 by ARM64 hardware users).
			//
			// Lookup order covers all supported configurations:
			// ARM64 Win: SOFTWARE\ = native ARM64 or ARM64EC (x64-emulated); WOW6432Node\ = x86
			// x64 Win:   SOFTWARE\ = native x64;                             WOW6432Node\ = x86
			// x86 Win:   SOFTWARE\ = native x86

			return ReadDefaultValue(@$"SOFTWARE\{sub}")
				?? ReadDefaultValue(@$"SOFTWARE\WOW6432Node\{sub}")
				?? ReadAppPathValue(@"SOFTWARE\Microsoft\Office", "OneNote")
				?? ReadAppPathValue(@"SOFTWARE\WOW6432Node\Microsoft\Office", "OneNote")
				?? ReadAppPathValue(@"SOFTWARE\Microsoft\Office", "Common")
				?? ReadAppPathValue(@"SOFTWARE\WOW6432Node\Microsoft\Office", "Common")
				;
		}


		public override int Uninstall()
		{
			return SUCCESS;
		}
	}
}
