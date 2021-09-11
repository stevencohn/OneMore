//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.FillCellModels
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Globalization;


	internal class DateFiller : Filler
	{
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


		private static bool Parse(string text, out DateTime value, out string format)
		{
			var culture = CultureInfo.CurrentCulture;
			var policy = culture.DateTimeFormat;

			format = policy.ShortDatePattern; // M/d/YYYY
			if (DateTime.TryParseExact(text, format, culture, DateTimeStyles.None, out value))
				return true;

			format = policy.MonthDayPattern; // MMMM d
			if (DateTime.TryParseExact(text, format, culture, DateTimeStyles.None, out value))
				return true;

			format = policy.LongDatePattern; // dddd, MMMM d, yyyy
			if (DateTime.TryParseExact(text, format, culture, DateTimeStyles.None, out value))
				return true;

			format = policy.YearMonthPattern; // MMMM yyyy
			if (DateTime.TryParseExact(text, format, culture, DateTimeStyles.None, out value))
				return true;

			format = "d-MMM-yyyy";
			if (DateTime.TryParseExact(text, format, culture, DateTimeStyles.None, out value))
				return true;

			format = "MMM d, yyyy";
			if (DateTime.TryParseExact(text, format, culture, DateTimeStyles.None, out value))
				return true;

			format = "MMM d";
			if (DateTime.TryParseExact(text, format, culture, DateTimeStyles.None, out value))
				return true;

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
