//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace OneMoreSetupActions
{
	using Microsoft.Win32;
	using System;
	using System.Runtime.InteropServices;
	using System.Security.AccessControl;
	using System.Text.RegularExpressions;


	/// <summary>
	/// Ensures the OneMore Registry settings are defined per the config file.
	/// </summary>
	internal class RegistryAction : CustomAction
	{
		private readonly Architecture architecture;

		public RegistryAction(Logger logger, Stepper stepper, Architecture onArchitecture)
			: base(logger, stepper)
		{
			architecture = onArchitecture;
		}


		//========================================================================================

		/// <summary>
		/// Applies the OneMore registry configuration by processing the embedded Registry.reg
		/// template, substituting the architecture-appropriate ProgramFiles path, OneMore CLSID,
		/// and current version, then writing each key and value.
		/// </summary>
		public override int Install()
		{
			logger.WriteLine();
			logger.WriteLine($"RegistryAction.Install --- {architecture}");

			var config = GetRegistryConfig();
			RegistryKey key = null;

			try
			{
				foreach (var line in config.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
				{
					if (line.Length == 0 || line[0] == ';')
					{
						continue; // skip empty lines and comments
					}

					if (line[0] == '[')
					{
						key?.Dispose();
						key = OpenOrCreateKey(line);
					}
					else if (key is not null && (line[0] == '@' || line[0] == '"'))
					{
						SetValue(key, line);
					}
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("RegistryAction.Install failed:");
				logger.WriteLine(exc);
				return FAILURE;
			}
			finally
			{
				key?.Dispose();
			}

			SetLaunchPermission();

			return SUCCESS;
		}


		/// <summary>
		/// Sets the DCOM LaunchPermission on the OneMore AppID so that non-admin users can launch
		/// the COM surrogate on ARM64 Windows.
		/// <para>
		/// On ARM64 Windows the machine-wide DCOM default launch security is more restrictive than
		/// on x64: it does not include BUILTIN\Users in the local-launch grant. Without an explicit
		/// LaunchPermission on the AppID, COM silently refuses to start dllhost.exe for non-admin
		/// sessions and the add-in never loads. This override grants Authenticated Users local
		/// launch and local activate rights, matching the permissive default that x64 Windows
		/// provides out of the box. The fix is harmless on x64 — it simply makes the existing
		/// implicit grant explicit.
		/// </para>
		/// <para>
		/// COM access-right bits used in the DACL (0x0b = 11):
		///   COM_RIGHTS_EXECUTE        0x1 — connect
		///   COM_RIGHTS_EXECUTE_LOCAL  0x2 — local launch
		///   COM_RIGHTS_ACTIVATE_LOCAL 0x8 — local activate
		/// </para>
		/// </summary>
		private void SetLaunchPermission()
		{
			logger.WriteLine($"step {stepper.Step()}: setting AppID LaunchPermission");

			try
			{
				// O:BA  owner = Administrators
				// G:BA  primary group = Administrators
				// D:    DACL — three ACEs:
				//   AU = Authenticated Users (all valid accounts, covers non-admin users)
				//   SY = SYSTEM
				//   BA = Built-in Administrators
				const string sddl = "O:BAG:BAD:(A;;0x0b;;;AU)(A;;0x0b;;;SY)(A;;0x0b;;;BA)";

				var sd = new RawSecurityDescriptor(sddl);
				var bytes = new byte[sd.BinaryLength];
				sd.GetBinaryForm(bytes, 0);

				using var baseKey = RegistryKey.OpenBaseKey(
					RegistryHive.LocalMachine, RegistryView.Registry64);

				using var key = baseKey.OpenSubKey(
					@"SOFTWARE\Classes\AppID\" + RegistryHelper.OneMoreID,
					RegistryKeyPermissionCheck.ReadWriteSubTree);

				if (key is not null)
				{
					key.SetValue("LaunchPermission", bytes, RegistryValueKind.Binary);
					logger.WriteLine("LaunchPermission set (Authenticated Users: local launch + activate)");
				}
				else
				{
					logger.WriteLine("AppID key not found; LaunchPermission not set");
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error setting LaunchPermission");
				logger.WriteLine(exc);
			}
		}


		/// <summary>
		/// Loads the Registry.reg embedded resource and substitutes the OneMore CLSID,
		/// ProgramFiles path, and version placeholders.
		/// </summary>
		private string GetRegistryConfig()
		{
			var env = architecture == Architecture.X86 ? "ProgramFiles(x86)" : "ProgramFiles";

			var config = Properties.Resource.Registry
				.Replace("{OneMoreID}", RegistryHelper.OneMoreID)
				.Replace("{ProgramFiles}", Environment.GetEnvironmentVariable(env))
				.Replace("{Version}", AssemblyInfo.Version);

			return config;
		}


		/// <summary>
		/// Returns Registry64 on a 64-bit OS so that registry writes bypass WOW64
		/// redirection when this process runs as 32-bit (x86-package MSI custom actions
		/// always run as 32-bit on 64-bit Windows). HKCU has no 32/64 split.
		/// </summary>
		private static RegistryView ViewFor(RegistryHive hive) =>
			hive != RegistryHive.CurrentUser && Environment.Is64BitOperatingSystem
				? RegistryView.Registry64
				: RegistryView.Default;


		/// <summary>
		/// Resolves a .reg-format hive name to a (RegistryHive, keyPath) pair.
		/// HKEY_CLASSES_ROOT is mapped to HKLM\SOFTWARE\Classes so writes land in the
		/// machine-wide hive rather than the per-user merged view.
		/// </summary>
		private static (RegistryHive hive, string keyPath) ResolveHiveAndPath(
			string hiveName, string keyName)
		{
			if (hiveName.EndsWith("ROOT"))
				return (RegistryHive.LocalMachine, @"SOFTWARE\Classes\" + keyName);

			if (hiveName.EndsWith("MACHINE"))
				return (RegistryHive.LocalMachine, keyName);

			return (RegistryHive.CurrentUser, keyName);
		}


		/// <summary>
		/// Opens or creates the registry key described by a .reg-format section header line
		/// such as [HKEY_LOCAL_MACHINE\Software\...].
		/// </summary>
		private RegistryKey OpenOrCreateKey(string line)
		{
			var raw = line.Trim('[', ']');
			var sep = raw.IndexOf('\\');
			var hiveName = raw.Substring(0, sep);
			var (hive, keyPath) = ResolveHiveAndPath(hiveName, raw.Substring(sep + 1));

			using var baseKey = RegistryKey.OpenBaseKey(hive, ViewFor(hive));

			var key = baseKey.OpenSubKey(keyPath, RegistryKeyPermissionCheck.ReadWriteSubTree);
			if (key is null)
			{
				logger.WriteLine($"creating key: {hiveName}\\{keyPath}");
				key = baseKey.CreateSubKey(keyPath, RegistryKeyPermissionCheck.ReadWriteSubTree);
			}
			else
			{
				logger.WriteLine($"opened key: {hiveName}\\{keyPath}");
			}

			return key;
		}


		/// <summary>
		/// Parses a .reg-format value line (e.g. "Name"=dword:0000001) and writes the
		/// typed value to the given key. Skips the write if the value is already correct.
		/// </summary>
		private void SetValue(RegistryKey key, string line)
		{
			var matches = Regex.Match(line, @"^(?<name>@|""\w+"")=(?<type>[^""]\w+:)?(?<value>.+)$");
			if (matches.Success)
			{
				var name = matches.Groups["name"].Value.Trim('"');
				var type = matches.Groups["type"].Value.TrimEnd(':');
				var value = matches.Groups["value"].Value.Trim('"');

				if (name == "@")
				{
					name = string.Empty; // use empty string for default value
				}

				if (type.Length == 0)
				{
					type = "string"; // default type is string
				}

				var v = key.GetValue(name);
				if (v is not null && v.ToString() == value)
				{
					logger.WriteLine($"value already set: {name} = {value} ({type})");
					return; // no change needed
				}

				logger.WriteLine($"setting value: {name} = {value} ({type})");

				if (type == "dword")
				{
					var dword = int.Parse(value, System.Globalization.NumberStyles.HexNumber);
					key.SetValue(name, dword, RegistryValueKind.DWord);
				}
				else
				{
					key.SetValue(name, value, RegistryValueKind.String);
				}
			}
		}

		//========================================================================================

		/// <summary>
		/// Deletes all registry subtrees defined in the config. Tracks the last deleted root
		/// so child keys of an already-deleted tree are skipped rather than causing errors.
		/// </summary>
		public override int Uninstall()
		{
			logger.WriteLine();
			logger.WriteLine($"RegistryAction.Uninstall --- {architecture}");

			var config = GetRegistryConfig();

			string marker = null;

			try
			{
				foreach (var line in config.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
				{
					if (line.Length > 0 && line[0] == '[')
					{
						var raw = line.Trim('[', ']');
						if (marker is null || !raw.StartsWith(marker))
						{
							var sep = raw.IndexOf('\\');
							var hiveName = raw.Substring(0, sep);
							var (hive, keyPath) = ResolveHiveAndPath(hiveName, raw.Substring(sep + 1));

							logger.WriteLine($"deleting tree: {raw}");
							using var baseKey = RegistryKey.OpenBaseKey(hive, ViewFor(hive));
							baseKey.DeleteSubKeyTree(keyPath, false);

							// remember for next pass; only need to delete root of subtree
							marker = raw;
						}
					}
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("RegistryAction.Uninstall failed:");
				logger.WriteLine(exc);
				return FAILURE;
			}

			return SUCCESS;
		}
	}
}
