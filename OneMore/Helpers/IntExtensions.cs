//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers
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

        // Convert Roman numerals to an integer.
        private static string ToRoman(this int arabic)
        {
            if (arabic >= 4000)
            {
                // use parentheses
                int thou = arabic / 1000;
                arabic %= 1000;
                return "(" + ToRoman(thou) + ")" + ToRoman(arabic);
            }

            // otherwise process the letters
            string result = "";

            // pull out thousands
            int num;
            num = arabic / 1000;
            result += ThouLetters[num];
            arabic %= 1000;

            // handle hundreds
            num = arabic / 100;
            result += HundLetters[num];
            arabic %= 100;

            // handle tens
            num = arabic / 10;
            result += TensLetters[num];
            arabic %= 10;

            // handle ones
            result += OnesLetters[arabic];

            return result;
        }
    }
}
