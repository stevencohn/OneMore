//************************************************************************************************
// Copyright © 2024 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;


	internal static class ArrayExtensions
	{

		public static int IndexOf<T>(this T[] array, Func<T, bool> predicate)
		{
			int num = 0;
			foreach (T arg in array)
			{
				if (predicate(arg))
				{
					return num;
				}

				num++;
			}

			return -1;
		}
	}
}
