//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.FillCellModels
{
	using River.OneMoreAddIn.Models;
	using System.Text.RegularExpressions;


	internal class AlphaNumericFiller : Filler
	{
		private readonly string key;
		private int value;


		public AlphaNumericFiller(TableCell cell)
			: base(cell)
		{
			var text = cell.GetText(true);

			// this matches the YearMonthPattern, MMMM yyyy, so FillCellsCommand should
			// prioritize DateFiller over AlphaNumericFiller
			var match = Regex.Match(text, "^([^\\d]+)([\\d]+)$");
			if (match.Success)
			{
				key = match.Groups[1].Value;
				value = int.Parse(match.Groups[2].Value);
			}
		}


		public override FillType Type => FillType.AlphaNumeric;


		public int Value => value;


		public static bool CanParse(string text)
		{
			return Regex.IsMatch(text, "^[^\\d]+[\\d]+$");
		}


		public override string Increment(int increment)
		{
			value += increment;
			return $"{key}{value}";
		}


		public override int Subtract(IFiller other)
		{
			if (other is AlphaNumericFiller o)
				return value - o.Value;

			return 0;
		}
	}
}
