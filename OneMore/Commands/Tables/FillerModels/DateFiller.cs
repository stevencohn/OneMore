//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.FillCellModels
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Globalization;
	using System.Linq;


	internal class DateFiller : Filler
	{
		private static readonly string[] formats = RegisterFormattingPatterns();

		private DateTime value;
		private readonly string format;


		public DateFiller(TableCell cell)
			: base(cell)
		{
			var text = cell.GetText(true);
			Parse(text, out value, out format);
		}


		public override FillType Type => FillType.Date;


		public DateTime Value => value;


		public static bool CanParse(string text)
		{
			return Parse(text, out _, out _);
		}


		private static string[] RegisterFormattingPatterns()
		{
			// DateTimeFormats with days but without times include
			//  M/d/yyyy
			//  MMM d, yyyy
			//  M/d/yy
			//  dddd, MMMM d, yyyy
			//  MMMM d, yyyy
			//  MMMM d

			var list = CultureInfo.CurrentCulture.DateTimeFormat
				.GetAllDateTimePatterns()
				.Where(p => p.Contains('d') && !p.Contains('m')).Distinct()
				.ToList();

			// add our own custom formats
			if (!list.Contains("MMM d, yyyy")) list.Add("MMM d, yyyy");
			if (!list.Contains("d-MMM-yyyy")) list.Add("d-MMM-yyyy");
			if (!list.Contains("MMM d")) list.Add("MMM d");

			return list.ToArray();
		}


		private static bool Parse(string text, out DateTime value, out string format)
		{
			var culture = CultureInfo.CurrentCulture;

			var i = 0;
			while (i < formats.Length)
			{
				format = formats[i];
				if (DateTime.TryParseExact(text, format, culture, DateTimeStyles.None, out value))
				{
					return true;
				}

				i++;
			}

			value = DateTime.MinValue;
			format = null;
			return false;
		}


		public override string Increment(int increment)
		{
			value = value.AddDays(increment);
			return value.ToString(format);
		}


		public override int Subtract(IFiller other)
		{
			if (other is DateFiller o)
				return value.Subtract(o.Value).Days;

			return 0;
		}
	}
}
