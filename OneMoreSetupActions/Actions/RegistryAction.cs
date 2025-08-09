//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace OneMoreSetupActions
{
	using Microsoft.Win32;
	using System;
	using System.Text.RegularExpressions;


	/// <summary>
	/// Ensures the OneMore Registry settings are defined per the config file.
	/// </summary>
	internal class RegistryAction : CustomAction
	{

		public RegistryAction(Logger logger, Stepper stepper)
			: base(logger, stepper)
		{
		}


		//========================================================================================

		/// <summary>
		/// Note this is invoked as a single call, not as part of Program:Install
		/// </summary>
		/// <returns></returns>
		public override int Install()
		{
			logger.WriteLine();
			logger.WriteLine("RegistryAction.Install ---");

			var config = GetRegistryConfig();
			RegistryKey key = null;

			foreach (var line in config.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
			{
				if (line[0] == '[')
				{
					key?.Dispose();

					var raw = line.Trim('[', ']');

					// extract hive name, ending up with something like "CLASSESROOT"
					var hiveName = raw.Substring(0, raw.IndexOf('\\'));

					// we only care about these two!
					var hive = hiveName.EndsWith("ROOT") ? Registry.ClassesRoot : Registry.CurrentUser;

					// extract key name, ending up with something like "Software\OneMore"
					var keyName = raw.Substring(raw.IndexOf('\\') + 1);

					key = hive.CreateSubKey(keyName, RegistryKeyPermissionCheck.ReadWriteSubTree);
				}
				else if (key is not null && (line[0] == '@' || line[0] == '"'))
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
			}

			key?.Dispose();

			return SUCCESS;
		}


		private static string GetRegistryConfig()
		{
			var config = Properties.Resource.Registry
				.Replace("{OneNoteID}", RegistryHelper.OneMoreID)
				.Replace("{ProgramFiles}", Environment.GetEnvironmentVariable("ProgramFiles"))
				.Replace("{Version}", AssemblyInfo.Version);

			return config;
		}


		//========================================================================================

		public override int Uninstall()
		{
			logger.WriteLine();
			logger.WriteLine("RegistryAction.Uninstall ---");

			var config = GetRegistryConfig();

			logger.WriteLine("RegistryAction.Uninstall done ---");
			return SUCCESS;
		}
	}
}
