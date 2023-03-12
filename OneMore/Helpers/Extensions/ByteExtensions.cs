//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	internal static class ByteExtensions
	{
		/// <summary>
		/// OneMore Extension >> Determines if the current buffer ends with the given byte pattern
		/// </summary>
		/// <param name="buffer">A byte array</param>
		/// <param name="pattern">A pattern of bytes to compare against the last bytes of buffer</param>
		/// <returns>True if buffer ends with pattern, otherwise false</returns>
		public static bool EndsWith(this byte[] buffer, byte[] pattern)
		{
			return With(buffer, pattern, buffer.Length - pattern.Length);
		}


		/// <summary>
		/// OneMore Extension >> Determines if the current buffer starts with the given byte pattern.
		/// </summary>
		/// <param name="buffer">A byte array.</param>
		/// <param name="pattern">A pattern of bytes to compare against the beginning bytes of buffer</param>
		/// <returns>True if buffer starts with pattern, otherwise false</returns>
		public static bool StartsWith(this byte[] buffer, byte[] pattern)
		{
			return With(buffer, pattern, 0);
		}


		private static bool With(byte[] buffer, byte[] pattern, int start)
		{
			if ((buffer.Length < pattern.Length) || (buffer.Length <= start))
			{
				return false;
			}

			for (int b = start, p = 0; p < pattern.Length; p++, b++)
			{
				if (buffer[b] != pattern[p])
				{
					return false;
				}
			}

			return true;
		}
	}
}
