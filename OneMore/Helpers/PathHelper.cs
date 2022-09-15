//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.IO;
	using System.Text.RegularExpressions;


	/// <summary>
	/// Helper methods complementory to System.IO.Path.
	/// </summary>
	/// <remarks>
	/// The Path class is static so does not allow extension methods, sadly.
	/// </remarks>
	internal static class PathHelper
	{
		// MAX_PATH in Windows should be 260 but OneNote.Export further restricts it to 239
		public const int MAX_PATH = 239;

		private const string LongKey = @"SYSTEM\CurrentControlSet\Control\FileSystem\LongPathsEnabled";

		private static char[] invalidFileChars;


		/// <summary>
		/// Clean a file name (typically a page title) to replace invalid file name
		/// characters with underscore characters.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static string CleanFileName(string name)
		{
			if (invalidFileChars == null)
				invalidFileChars = Path.GetInvalidFileNameChars();

			// OneNote sometimes add \r\n to URL names in HTML export files
			name = Regex.Replace(name, @"[\r\n]+", " ");

			return string.Join("_",
				name.Split(invalidFileChars, StringSplitOptions.RemoveEmptyEntries))
				.TrimEnd('.');
		}


		/// <summary>
		/// Ensures that the given string fits within the MAX_LENGTH characters allowed
		/// by Windows for a valid file path.
		/// </summary>
		/// <param name="path">A file path to test</param>
		/// <returns>Either path if it is valid or a modified path with shortened file name.</returns>
		public static string FitMaxPath(string path)
		{
			if (path.Length <= MAX_PATH)
			{
				return path;
			}

			/*
			 * Win 10 1607 and later is suposed to allow LongPathsEnabled Reg key in conjunction
			 * with UseLegacyPathHandling=false in appSettings but this doesn't seem to work
			 * correctly along with the .NET System.IO.Path methods
			 * 
			if (Environment.OSVersion.Version.Major > 10 ||
				Environment.OSVersion.Version.Build > 1607)
			{
				using (var key = Registry.ClassesRoot.OpenSubKey(LongKey, false))
				{
					if (key != null)
					{
						// has user opted to enable long paths, greater than 260?
						var allows = (int)key.GetValue(string.Empty);
						if (allows == 1)
						{
							return path;
						}
					}
				}
			}
			*/

			// use our own IO.Path methods to avoid PathTooLongException...

			var dir = GetLongDirectoryName(path);
			var nam = GetLongFileNameWithoutExtension(path);
			var ext = GetLongExtension(path);

			var dam = GetLongFileName(dir);
			if (dam == nam)
			{
				// special case with C:\somepath\filename\filename.ext
				// lets us trim both occurances of filename

				dir = GetLongDirectoryName(dir);

				var maxnam = (MAX_PATH - dir.Length - ext.Length - 1) / 2;
				if (maxnam > 0 && maxnam < nam.Length)
				{
					nam = nam.Substring(0, maxnam);
					return Path.Combine(dir, nam, nam + ext);
				}
			}
			else
			{
				var maxnam = MAX_PATH - dir.Length - ext.Length - 1;
				if (maxnam > 0 && maxnam < nam.Length)
				{
					nam = nam.Substring(0, maxnam);
					return Path.Combine(dir, nam + ext);
				}
			}

			// if maxnam < 0 then dir is too long and will cause PathTooLongException
			// eventually when caller tries to use it

			return path;
		}


		/// <summary>
		/// Gets a path to the OneMore data folder
		/// </summary>
		/// <returns></returns>
		public static string GetAppDataPath()
		{
			return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				River.OneMoreAddIn.AssemblyInfo.Product);
		}


		#region Get File Parts overrides
		private static string GetLongDirectoryName(string path)
		{
			var sep = path.Length - 1;
			while (sep > 0 &&
				path[sep] != Path.DirectorySeparatorChar &&
				path[sep] != Path.AltDirectorySeparatorChar)
			{
				sep--;
			}

			if (sep > 0)
			{
				return path.Substring(0, sep);
			}

			return string.Empty;
		}


		private static string GetLongExtension(string path)
		{
			var dot = path.Length - 1;
			while (dot >= 0 && path[dot] != '.')
			{
				dot--;
			}

			if (dot > -1)
			{
				return path.Substring(dot);
			}

			return String.Empty;
		}


		private static string GetLongFileName(string path)
		{
			var sep = path.Length - 1;
			while (sep >= 0 &&
				path[sep] != Path.DirectorySeparatorChar &&
				path[sep] != Path.AltDirectorySeparatorChar)
			{
				sep--;
			}

			if (sep > -1)
			{
				return path.Substring(sep + 1);
			}

			return String.Empty;
		}


		private static string GetLongFileNameWithoutExtension(string path)
		{
			var sep = path.Length - 1;
			while (sep >= 0 &&
				path[sep] != Path.DirectorySeparatorChar &&
				path[sep] != Path.AltDirectorySeparatorChar)
			{
				sep--;
			}

			if (sep > -1)
			{
				var dot = sep + 1;
				while (dot < path.Length && path[dot] != '.')
				{
					dot++;
				}

				if (dot < path.Length)
				{
					return path.Substring(sep + 1, dot - sep - 1);
				}
			}

			return String.Empty;
		}
		#endregion Get File Parts overrides


		/// <summary>
		/// Checks if the given paths exists and creates it if it is missing.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool EnsurePathExists(string path)
		{
			if (!Directory.Exists(path))
			{
				try
				{
					Directory.CreateDirectory(path);
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine("error creating " + path, exc);
					return false;
				}
			}

			return true;
		}


		/// <summary>
		/// Checks if the given path has a filename containing a wildcard character "*"
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool HasWildFileName(string path)
		{
			if (Path.HasExtension(path))
			{
				var slash = path.LastIndexOfAny(new char[]
				{
					Path.DirectorySeparatorChar,
					Path.AltDirectorySeparatorChar,
					Path.VolumeSeparatorChar
				});

				if (slash >= 0)
				{
					var wild = path.IndexOf('*', slash);
					if (wild > 0)
					{
						return true;
					}
				}
			}

			return false;
		}
	}
}
