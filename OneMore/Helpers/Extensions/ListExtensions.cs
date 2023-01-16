//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Extensions
{
	using System.Collections.Generic;


	/// <summary>
	/// Add Write and WriteLine extension methods to List`string so it can be used
	/// similar to TextWriter. Entries in the list are treated like "lines" in text.
	/// </summary>
	internal static class ListExtensions
	{
		private static bool newline = true;


		/// <summary>
		/// Appends a string to the last line, or adds a new line if the list is empty
		/// </summary>
		/// <param name="list">This list</param>
		/// <param name="text">The string to append</param>
		public static void Write(this List<string> list, string text)
		{
			if (list.Count == 0 || newline)
			{
				list.Add(text);
			}
			else
			{
				list[list.Count - 1] += text;
			}

			newline = false;
		}


		/// <summary>
		/// Ends the current line. Subsequent writes will add a new line.
		/// </summary>
		/// <param name="list">This list</param>
		public static void WriteLine(this List<string> list)
		{
			newline = true;
		}


		/// <summary>
		/// Appends a string to the last line and ends the line.
		/// Subsequent writes will add a new line.
		/// </summary>
		/// <param name="list">This list</param>
		/// <param name="text">The string to append</param>
		public static void WriteLine(this List<string> list, string text)
		{
			Write(list, text);
			newline = true;
		}
	}
}
