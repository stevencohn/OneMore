//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Generates colors appropriate for each value within a collection
	/// suitable as background fill in table cells that enumerate the values.
	/// This is similar to the Conditional Formatting of cells in Excel.
	/// </summary>
	internal class Heatmap
	{
		public enum Level { Empty, Low, Medium, High }

		private const int Lightness = 204;              // 85% of 240
		private const int Saturation = 180;             // 75% of 240 
		private const decimal Scale = (decimal)0.666;   // scales 0°..359° to 0..240

		private decimal minValue;
		private decimal maxValue;
		private decimal margin;


		/// <summary>
		/// Initialize a new instance based on the given collection of values
		/// </summary>
		/// <param name="values">A collection of decimal values</param>
		/// <exception cref="ArgumentException">The collection cannot be empty</exception>
		public Heatmap(IEnumerable<decimal> values)
		{
			if (!values.Any()) throw new ArgumentException("values cannot be empty");
			minValue = decimal.MaxValue;
			maxValue = decimal.MinValue;
			AddValues(values);
		}


		public void AddValues(IEnumerable<decimal> values)
		{
			if (!values.Any()) throw new ArgumentException("values cannot be empty");
			var min = values.Min();
			var max = values.Max();
			if (min < minValue) minValue = min;
			if (max > maxValue) maxValue = max;
			var range = maxValue - minValue;
			margin = range / values.Count() switch { 1 => 1, 2 => 2, 3 => 3, _ => 3 };
		}


		public int MapToRGB(decimal value)
		{
			return MapToRGB(value, out _);
		}


		public int MapToRGB(decimal value, out Level level)
		{
			if (value == 0 || margin == 0)
			{
				level = Level.Empty;
				return 0xffffff; // white background fill
			}

			decimal h;

			// The HSL color space is cylindrical, 0°..359°, which must be scaled to
			// the Windows API 8-bit range 0..240. The low/med/high ranges roughly
			// map to green/yellow/red. The 'h' calculations below approximates
			// the interval (range) containing those three colors.

			// h = intervalMax - (interval * %value)
			// where %value = (value - intervalMin) / (intervalMax - intervalMin)

			if (value >= maxValue - margin)
			{
				// red (0..29° = 29 ≈ 0..17Hue)
				h = 29 - 29 * ((value - (maxValue - margin)) / (maxValue - (maxValue - margin)));
				if (h > 360) h = 360 - h;
				level = Level.High;
			}
			else if (value <= minValue + margin)
			{
				// green (35..75° = 40 ≈ 50..81Hue)
				h = 75 - 51 * ((value - (minValue + margin)) / ((maxValue + margin) - minValue));
				level = Level.Low;
			}
			else
			{
				// yellow (83..135° ≈ 21..45Hue)
				h = 60 - 30 * ((value - (minValue + margin)) / ((maxValue - margin) - (minValue + margin)));
				level = Level.Medium;
			}

			// increase the lightness slightly to mellow the color for background fill
			var bgr = Native.ColorHLSToRGB((int)(h * Scale), Lightness, Saturation);

			// flip the BGR value to RGB
			var rgb = ((bgr & 0xFF) << 16) + (bgr & 0xFF00) + ((bgr & 0xFF0000) >> 16);

			return rgb;
		}
	}
}
