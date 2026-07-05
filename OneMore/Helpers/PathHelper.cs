//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;
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
		// unique qualifier appended to filename: " (123)"
		public const int COUNTER_WIDTH = 6;

		// in Windows this should be 260 but OneNote.Export further restricts it to 239
		public const int MAX_PATH = 239 - COUNTER_WIDTH;

		// shortest filename, allowing 3 chars plus counter
		public const int MIN_NAME = 3 + COUNTER_WIDTH;

		//private const string LongKey = @"SYSTEM\CurrentControlSet\Control\FileSystem\LongPathsEnabled";

		private static char[] invalidFileChars;


		/// <summary>
		/// Clean a file name (typically a page title) to replace invalid file name
		/// characters with underscore characters.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static string CleanFileName(string name)
		{
			invalidFileChars ??= Path.GetInvalidFileNameChars();

			// OneNote sometimes adds \r\n to URL names in HTML export files
			name = Regex.Replace(name, @"[\r\n]+", " ");

			return string.Join("_",
				name.Split(invalidFileChars, StringSplitOptions.RemoveEmptyEntries))
				.TrimEnd('.');
		}


		/// <summary>
		/// Builds the folder path for a section based on its position in the notebook
		/// hierarchy: each ancestor section group becomes a "[name]" folder and the section
		/// itself becomes a "(name)" folder, nested beneath the given root.
		/// </summary>
		/// <param name="root">The root export folder</param>
		/// <param name="sectionGroups">Ancestor section group names, outermost first</param>
		/// <param name="sectionName">The name of the section</param>
		/// <returns>The full nested folder path</returns>
		public static string BuildSectionFolderPath(
			string root, IEnumerable<string> sectionGroups, string sectionName)
		{
			var path = root;
			foreach (var group in sectionGroups)
			{
				path = Path.Combine(path, $"[{CleanFileName(group)}]");
			}

			return Path.Combine(path, $"({CleanFileName(sectionName)})");
		}


		/// <summary>
		/// Gets a path to the OneMore data folder
		/// </summary>
		/// <returns></returns>
		public static string GetAppDataPath()
		{
			return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				AssemblyInfo.Product);
		}


		/// <summary>
		/// In its simplest form, appends a counter to the filename if a similarly named file
		/// already exists to ensure the name is unique, .e.g, "name (1).txt". But this also
		/// ensures that the full path fits within MAX_PATH for both the filename and a presumed
		/// parent folder of the same name, e.g. c:\foo\name\name.txt. This parent folder is
		/// used for attachments associated with the file.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="name"></param>
		/// <param name="ext"></param>
		/// <returns></returns>
		public static string GetUniqueQualifiedFileName(string path, string name, string ext)
		{
			// max length of FileNameWithoutExt is half the width of MAX_PATH minus the length
			// of the root path (+1 for path separator). Half because the name is used again
			// to store attachments in a similarlly named subfolder; also subtract 12 to make
			// room for an optional counter like " (123)"
			var max = (PathHelper.MAX_PATH - path.Length - ext.Length - 1) / 2 - 6;

			if (max <= MIN_NAME)
			{
				// root path is too long
				return null;
			}

			if (name.Length > max)
			{
				name = name.Substring(0, max).Trim();
			}

			name = CleanFileName(name);

			var full = Path.Combine(path, name + ext);
			if (!File.Exists(full))
			{
				return full;
			}

			var counter = 1;
			var nameCounter = $"{name} ({counter})";
			full = Path.Combine(path, $"{nameCounter}{ext}");

			while (File.Exists(full))
			{
				counter++;
				nameCounter = $"{name} ({counter})";
				full = Path.Combine(path, $"{nameCounter}{ext}");
			}

			return full;
		}


		/// <summary>
		/// Checks that the given path is syntactically valid, i.e. contains no illegal
		/// characters and can be resolved by Path.GetFullPath.
		/// </summary>
		/// <param name="path">The path to validate</param>
		/// <param name="errorMessage">
		/// On failure, a human-readable description of the problem; otherwise null
		/// </param>
		/// <returns>True if the path is valid</returns>
		public static bool IsValidPath(string path, out string errorMessage)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				errorMessage = "Path must not be empty.";
				return false;
			}

			var invalid = path.IndexOfAny(Path.GetInvalidPathChars());
			if (invalid >= 0)
			{
				errorMessage =
					$"'{path}' is not a valid path; it contains an illegal '{path[invalid]}' " +
					"character. If this path was quoted on the command line, make sure it " +
					"doesn't end with a trailing backslash before the closing quote " +
					"(use \"C:\\folder\" rather than \"C:\\folder\\\").";
				return false;
			}

			try
			{
				Path.GetFullPath(path);
			}
			catch (Exception exc)
			{
				errorMessage = $"'{path}' is not a valid path: {exc.Message}";
				return false;
			}

			errorMessage = null;
			return true;
		}


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
