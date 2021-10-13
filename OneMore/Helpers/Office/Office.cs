﻿//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Office
{
	using Microsoft.Win32;
	using System;
	using System.Collections.Generic;


	/// <summary>
	/// Provides basic information about the local installation of Microsoft Office
	/// </summary>
	internal static class Office
	{

		/// <summary>
		/// Get a collection of enabled Office editing and proofing languages
		/// </summary>
		/// <returns>An array of strings</returns>
		public static string[] GetEditingLanguages()
		{
			/*
			 * Enabled editing languages are stored here:
			 * HKCU\SOFTWARE\Microsoft\Office\16.0\Common\LanguageResources\EnabledEditingLanguages
			 * However, must also consider value of each entry where(I think)
			 * 1 == preferred proofing language
			 * 3 == secondary
			 * 4 == installed but not enabled for proofing
			 */

			var version = GetOfficeVersion();
			var path = $@"SOFTWARE\Microsoft\Office\{version}\Common\LanguageResources\EnabledEditingLanguages";

			using (var key = Registry.CurrentUser.OpenSubKey(path, false))
			{
				if (key != null)
				{
					var list = new List<string>();
					var names = key.GetValueNames();
					foreach (var name in names)
					{
						if (key.GetValue(name) is int value && ((value & 1) == 1))
						{
							list.Add(name);
						}
					}
					return list.ToArray();
				}
			}

			return new string[0];
		}


		/// <summary>
		/// Get installed version of Office, which may differ from version of OneNote
		/// </summary>
		/// <returns></returns>
		public static Version GetOfficeVersion()
		{
			return GetVersion("Excel", 16);
		}


		public static Version GetOneNoteVersion()
		{
			return GetVersion("OneNote", 15);
		}


		private static Version GetVersion(string name, int latest)
		{
			using (var key = Registry.ClassesRoot.OpenSubKey($@"\{name}.Application\CurVer", false))
			{
				if (key != null)
				{
					// get default value string
					var value = (string)key.GetValue(string.Empty);
					// extract version number
					var version = new Version(value.Substring(value.LastIndexOf('.') + 1) + ".0");
					return version;
				}
			}

			// presume latest
			return new Version(latest, 0);
		}


		/// <summary>
		/// Determines if Word is installed
		/// </summary>
		/// <returns></returns>
		public static bool IsWordInstalled()
		{
			var type = Type.GetTypeFromProgID("Word.Application");
			return type != null;
		}


		/// <summary>
		/// Determines if PowerPoint is installed
		/// </summary>
		/// <returns></returns>
		public static bool IsPowerPointInstalled()
		{
			var type = Type.GetTypeFromProgID("Powerpoint.Application");
			return type != null;
		}


		/// <summary>
		/// Determines if Office is set to Black color theme.
		/// </summary>
		/// <returns>True if Black theme is set; otherwise false</returns>
		public static bool IsBlackThemeEnabled()
		{
			var version = GetOfficeVersion();

			using (var key = Registry.CurrentUser.OpenSubKey(
				$@"Software\Microsoft\Office\{version.Major}.{version.Minor}\Common"))
			{
				if (key != null)
				{
					var theme = key.GetValue("UI Theme") as Int32?;
					if (theme == null)
					{
						theme = key.GetValue("Theme") as Int32?;
					}

					if (theme != null)
					{
						/*
						Colorful   0
						Dark Gray  3
						Black      4
						White      5
						*/

						return theme == 4;
					}
				}
			}

			return false;
		}
	}
}
