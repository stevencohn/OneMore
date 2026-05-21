//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace OneMoreSetupActions
{
	using Microsoft.Win32;
	using System;
	using System.Runtime.InteropServices;
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

			return SUCCESS;
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
		/// Maps a .reg-format hive name (e.g. HKEY_LOCAL_MACHINE) to the corresponding
		/// RegistryKey, defaulting to CurrentUser for anything unrecognised.
		/// </summary>
		private static RegistryKey ResolveHive(string hiveName)
		{
			if (hiveName.EndsWith("ROOT")) return Registry.ClassesRoot;
			if (hiveName.EndsWith("MACHINE")) return Registry.LocalMachine;
			return Registry.CurrentUser;
		}


		/// <summary>
		/// Opens or creates the registry key described by a .reg-format section header line
		/// such as [HKEY_LOCAL_MACHINE\Software\...].
		/// </summary>
		private RegistryKey OpenOrCreateKey(string line)
		{
			var raw = line.Trim('[', ']');

			// extract hive name, ending up with something like "HKEY_CLASSES_ROOT"
			var hiveName = raw.Substring(0, raw.IndexOf('\\'));
			var hive = ResolveHive(hiveName);

			// extract key path, ending up with something like "Software\OneMore"
			var keyName = raw.Substring(raw.IndexOf('\\') + 1);

			var key = hive.OpenSubKey(keyName, RegistryKeyPermissionCheck.ReadWriteSubTree);
			if (key is null)
			{
				logger.WriteLine($"creating key: {hive.Name}\\{keyName}");
				key = hive.CreateSubKey(keyName, RegistryKeyPermissionCheck.ReadWriteSubTree);
			}
			else
			{
				logger.WriteLine($"opened key: {hive.Name}\\{keyName}");
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
							var hiveName = raw.Substring(0, raw.IndexOf('\\'));
							var hive = ResolveHive(hiveName);
							var keyName = raw.Substring(raw.IndexOf('\\') + 1);

							logger.WriteLine($"deleting tree: {raw}");
							hive.DeleteSubKeyTree(keyName, false);

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
