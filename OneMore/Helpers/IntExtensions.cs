//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{

    // copied verbatim unashamedly from
    // http://csharphelper.com/blog/2016/04/convert-to-and-from-roman-numerals-in-c/

    internal static class IntExtensions
	{
        // Map digits to letters.
        private static string[] ThouLetters = { "", "M", "MM", "MMM" };

        private static string[] HundLetters =
            { "", "C", "CC", "CCC", "CD", "D", "DC", "DCC", "DCCC", "CM" };

        private static string[] TensLetters =
            { "", "X", "XX", "XXX", "XL", "L", "LX", "LXX", "LXXX", "XC" };

        private static string[] OnesLetters =
            { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" };


        // convert int to alphabet string
        public static string ToAlphabetic(this int value)
		{
            string result = string.Empty;
            while (--value >= 0)
            {
                result = (char)('A' + value % 26) + result;
                value /= 26;
            }

            return result.ToLower();
        }


        // convert int to roman numerals
        public static string ToRoman(this int value)
        {
            if (value >= 4000)
            {
                // use parentheses
                int thou = value / 1000;
                value %= 1000;
                return "(" + ToRoman(thou) + ")" + ToRoman(value);
            }

            // otherwise process the letters
            string result = "";

            // pull out thousands
            int num;
            num = value / 1000;
            result += ThouLetters[num];
            value %= 1000;

            // handle hundreds
            num = value / 100;
            result += HundLetters[num];
            value %= 100;

            // handle tens
            num = value / 10;
            result += TensLetters[num];
            value %= 10;

            // handle ones
            result += OnesLetters[value];

            return result;
        }
    }
}
