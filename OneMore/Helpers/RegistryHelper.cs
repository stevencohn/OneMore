//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers
{
	using Microsoft.Win32;
	using System;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;


	internal class FileAssociation
	{
		public string ProgID { get; set; }
		public string Application { get; set; }
		public string DefaultExt { get; set; }
		public string IconFilename { get; set; }
		public int IconIndex { get; set; }
	}


	internal static class RegistryHelper
	{
		/// <summary>
		/// Load the registered application details for a given file extension.
		/// </summary>
		/// <param name="ext">A file extension string similar to ".txt"</param>
		/// <returns>A FileAssociation instance or null if not found</returns>
		public static FileAssociation GetAssociation(string ext)
		{
			var application = GetApplication(ext);
			if (application == null)
			{
				return null;
			}

			var association = LookupAssociation(ext, application);
			if (association != null)
			{
				return association;
			}

			var progID = GetUserChoiceProgID(ext);
			if (progID != null)
			{
				association = new FileAssociation
				{
					Application = application,
					ProgID = progID,
					DefaultExt = ext,
					IconFilename = application,
					IconIndex = 0
				};
			}

			return association;
		}


		private static string GetApplication(string ext)
		{
			string application = null;

			// fetch length of output buffer
			uint length = 0;
			uint ret = Native.AssocQueryString(
				Native.AssocF.None, Native.AssocStr.Executable, ext, null, null, ref length);

			if (ret == 1) // expect S_FALSE
			{
				// fill buffer with executable path
				var buffer = new StringBuilder((int)length); // long enough for zero-term
				ret = Native.AssocQueryString(
					Native.AssocF.None, Native.AssocStr.Executable, ext, null, buffer, ref length);

				if (ret == 0) // expect S_OK
				{
					application = buffer.ToString();
				}
			}

			return application;
		}


		private static FileAssociation LookupAssociation(string ext, string application)
		{
			using (var key = Registry.ClassesRoot.OpenSubKey("CLSID"))
			{
				foreach (var classNames in key.GetSubKeyNames())
				{
					using (var classKey = key.OpenSubKey(classNames))
					{
						var association = GetDetails(classKey, ext, application);
						if (association != null)
						{
							return association;
						}
					}
				}
			}

			return null;
		}


		private static FileAssociation GetDetails(RegistryKey key, string ext, string application)
		{
			var names = key.GetSubKeyNames();
			if (names.Contains("VersionIndependentProgID") &&
				names.Contains("DefaultExtension") &&
				names.Contains("DefaultIcon") &&
				(names.Contains("InprocServer32") || names.Contains("LocalServer32"))
				)
			{
				var association = new FileAssociation
				{
					ProgID = (string)key.GetValue(string.Empty)
				};

				if (!string.IsNullOrEmpty(association.ProgID))
				{
					association.DefaultExt = GetDefaultValue(key, "DefaultExtension");

					if (string.IsNullOrEmpty(association.DefaultExt) ||
						!Regex.IsMatch(association.DefaultExt, $@"\{ext}\b?"))
					{
						return null;
					}

					association.Application = GetDefaultValue(key, "InprocServer32");
					if (string.IsNullOrEmpty(association.Application))
					{
						association.Application = GetDefaultValue(key, "LocalServer32");
					}

					if (!string.IsNullOrEmpty(association.Application) &&
						association.Application.Equals(application, StringComparison.InvariantCultureIgnoreCase))
					{
						var defaultIcon = GetDefaultValue(key, "DefaultIcon");
						var matches = Regex.Matches(defaultIcon, @"([^,]+),(\d+)");
						if (matches.Count > 0)
						{
							association.IconFilename = matches[0].Groups[1].Value;
							if (int.TryParse(matches[0].Groups[2].Value, out var iconIndex))
							{
								association.IconIndex = iconIndex;
							}
						}
						else
						{
							association.IconFilename = association.Application;
							association.IconIndex = 0;
						}

						return association;
					}
				}
			}

			return null;
		}


		private static string GetDefaultValue(RegistryKey key, string name)
		{
			using (var subkey = key.OpenSubKey(name))
			{
				if (subkey != null)
				{
					return (string)subkey.GetValue(string.Empty);
				}
			}

			return null;
		}


		private static string GetUserChoiceProgID(string ext)
		{
			string progID = null;
			using (var key = Registry.CurrentUser.OpenSubKey(
				$@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\{ext}\UserChoice"))
			{
				if (key != null)
				{
					progID = (string)key.GetValue("ProgID");
				}
			}

			return progID;
		}

	}
}
