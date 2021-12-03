//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Diagnostics;
	using System.Management;


	internal static class ProcessExtensions
	{

		/// <summary>
		/// Returns the command line for the given process.
		/// </summary>
		/// <param name="process">A Process for which to retrieve the command line</param>
		/// <returns>A string specifying the full command line or null if </returns>
		public static string GetCommandLine(this Process process)
		{
			using (var searcher = new ManagementObjectSearcher(
			  $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {process.Id}"))
			{
				// since we look up the process using a unique PID, the query must
				// return at most one match
				using (var e = searcher.Get().GetEnumerator())
				{
					if (e.MoveNext()) // move to first item
					{
						return e.Current["CommandLine"]?.ToString();
					}
				}
			}

			// not having found a command line implies either:
			// - an access denied exception due to lack of privileges
			// - cannot process request because process has exited
			return null;
		}
	}
}
