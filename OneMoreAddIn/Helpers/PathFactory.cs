//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.IO;


	internal static class PathFactory
	{
		public static string GetAppDataPath ()
		{
			return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				AssemblyInfo.Product);
		}


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
					Logger.Current.WriteLine("Error creating " + path);
					Logger.Current.WriteLine(exc);
					return false;
				}
			}

			return true;
		}
	}
}
