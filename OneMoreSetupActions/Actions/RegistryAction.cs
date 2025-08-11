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
		private Architecture architecture;

		public RegistryAction(Logger logger, Stepper stepper)
			: base(logger, stepper)
		{
		}


		//========================================================================================

		/// <summary>
		/// </summary>
		/// <param name="oArchitecture">The bit architecture of the installed OneNote.exe</param>
		/// <returns></returns>
		public int Install(Architecture onArchitecture)
		{
			architecture = onArchitecture;
			return Install();
		}


		/// <summary>
		/// </summary>
		/// <returns></returns>
		public override int Install()
		{
			logger.WriteLine();
			logger.WriteLine("RegistryAction.Install ---");

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


		private string GetRegistryConfig()
		{
			var env = architecture == Architecture.X86 ? "ProgramFiles" : "ProgramFiles(x86)";

			var config = Properties.Resource.Registry
				.Replace("{OneMoreID}", RegistryHelper.OneMoreID)
				.Replace("{ProgramFiles}", Environment.GetEnvironmentVariable(env))
				.Replace("{Version}", AssemblyInfo.Version);

			return config;
		}


		private RegistryKey OpenOrCreateKey(string line)
		{
			var raw = line.Trim('[', ']');

			// extract hive name, ending up with something like "HKEY_CLASSES_ROOT"
			var hiveName = raw.Substring(0, raw.IndexOf('\\'));

			// we only care about these two!
			var hive = hiveName.EndsWith("ROOT") ? Registry.ClassesRoot : Registry.CurrentUser;

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

		public override int Uninstall()
		{
			logger.WriteLine();
			logger.WriteLine("RegistryAction.Uninstall ---");

			//var config = GetRegistryConfig();

			logger.WriteLine("RegistryAction.Uninstall N/A ---");
			return SUCCESS;
		}
	}
}
