//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands.Tables.FillCellModels
{
	using System.Text.RegularExpressions;


	internal class AlphaNumericFiller : IFiller
	{
		private readonly string key;
		private int value;


		public AlphaNumericFiller(string text)
		{
			// this matches the YearMonthPattern, MMMM yyyy, so FillCellsCommand should
			// prioritize DateFiller over AlphaNumericFiller

			var match = Regex.Match(text, "^([^\\d]+)([\\d]+)$");
			if (match.Success)
			{
				key = match.Groups[1].Value;
				value = int.Parse(match.Groups[2].Value);
			}
		}


		public FillType Type => FillType.AlphaNumeric;


		public int Value => value;


		public static bool CanParse(string text)
		{
			return Regex.IsMatch(text, "^[^\\d]+[\\d]+$");
		}


		public string Increment(int increment)
		{
			value += increment;
			return $"{key}{value}";
		}
	}
}
