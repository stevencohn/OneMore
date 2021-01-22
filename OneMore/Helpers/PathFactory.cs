//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.IO;


	/// <summary>
	/// Helper methods complementory to System.IO.Path.
	/// </summary>
	/// <remarks>
	/// The Path class is static so does not allow extension methods, sadly.
	/// </remarks>
	internal static class PathFactory
	{
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

			return string.Join("_",
				name.Split(invalidFileChars, StringSplitOptions.RemoveEmptyEntries))
				.TrimEnd('.');
		}


		/// <summary>
		/// Gets a path to the OneMore data folder
		/// </summary>
		/// <returns></returns>
		public static string GetAppDataPath ()
		{
			return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				River.OneMoreAddIn.AssemblyInfo.Product);
		}


		/// <summary>
		/// Checks if the given paths exists and creates it if it is missing.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool EnsurePathExists (string path)
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
	}
}
