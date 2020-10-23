//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;
	using System.Linq;


	internal static class IEnumerableExtensions
	{

		public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
		{
			return collection == null || !collection.Any();
		}


		public static decimal Median(this IEnumerable<decimal> values)
		{
			int count = values.Count();
			if (count % 2 == 0)
			{
				return values.OrderBy(n => n).Skip((count / 2) - 1).Take(2).Average();
			}

			return values.OrderBy(n => n).ElementAt(count / 2);
		}


		public static decimal Mode(this IEnumerable<decimal> values)
		{
			return values
				.GroupBy(n => n)
				.OrderByDescending(g => g.Count())
				.Select(g => g.Key).FirstOrDefault();
		}


		public static decimal StandardDeviation(this IEnumerable<decimal> values)
		{
			var variance = Variance(values);

			if (variance == 0.0M)
				return 0.0M;

			return (decimal)Math.Sqrt((double)variance);
		}


		public static decimal Variance(this IEnumerable<decimal> values)
		{
			var mean = 0.0M;
			var sum = 0.0M;
			var variance = 0.0M;
			var n = 0;
			foreach (var value in values)
			{
				n++;
				var delta = value - mean;
				mean += delta / n;
				sum += delta * (value - mean);
			}

			if (n > 1)
			{
				// if (Population)
				variance = sum / (n - 1);

				// else if (Sample)
				//variance = sum / n;
			}

			return variance;
		}
	}
}
