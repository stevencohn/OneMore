//************************************************************************************************
// Copyright © 2024 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;


	internal static class DoubleExtensions
	{
		/// <summary>
		/// OneMore Extension >> Compares two Doubles with a specified epsilon window.
		/// </summary>
		/// <param name="a">This Double</param>
		/// <param name="b">That Double</param>
		/// <param name="epsilon">
		/// The fudge factor for comparison. This defaults to 0.5 which is good for
		/// font size comparisons, our most common use case in OneMore
		/// </param>
		/// <returns>True if the two values are within the specified epsilon difference</returns>
		public static bool EstEquals(this double a, double b, double epsilon = 0.5)
		{
			return Math.Abs(a - b) < epsilon;
		}


		/// <summary>
		/// OneMore Extension >> Compares two Floats with a specified epsilon window.
		/// </summary>
		/// <param name="a">This Float</param>
		/// <param name="b">That Float</param>
		/// <param name="epsilon">
		/// The fudge factor for comparison. This defaults to Float.Epsilon, the smallest
		/// viable vaiance window for comparison.
		/// </param>
		/// <returns>True if the two values are within the specified epsilon difference</returns>
		public static bool EstEquals(this float a, float b, float epsilon = float.Epsilon)
		{
			return Math.Abs(a - b) < epsilon;
		}
	}
}
