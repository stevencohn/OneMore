//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;
	using System.Windows.Forms;


	/// <summary>
	/// Converts a DateTimePicker format into a list of segments: fields that can be edited
	/// and literals that separate them. Mirrors the .NET custom date/time format syntax.
	/// </summary>
	internal static class DateFormatParser
	{
		/// <summary>
		/// Resolves the pattern string for the given format kind.
		/// </summary>
		public static string GetPattern(
			DateTimePickerFormat format, string customFormat, CultureInfo culture)
		{
			var info = culture.DateTimeFormat;

			switch (format)
			{
				case DateTimePickerFormat.Short:
					return info.ShortDatePattern;

				case DateTimePickerFormat.Time:
					return info.LongTimePattern;

				case DateTimePickerFormat.Custom:
					return string.IsNullOrEmpty(customFormat)
						? info.LongDatePattern
						: customFormat;

				default:
					return info.LongDatePattern;
			}
		}


		/// <summary>
		/// Parses the pattern into segments. Unterminated quotes are treated as running to
		/// the end of the pattern.
		/// </summary>
		public static List<DateSegment> Parse(string pattern, CultureInfo culture)
		{
			var segments = new List<DateSegment>();
			if (string.IsNullOrEmpty(pattern))
			{
				return segments;
			}

			var info = culture.DateTimeFormat;
			var literal = new StringBuilder();

			void FlushLiteral()
			{
				if (literal.Length > 0)
				{
					segments.Add(DateSegment.MakeLiteral(literal.ToString()));
					literal.Clear();
				}
			}

			var i = 0;
			while (i < pattern.Length)
			{
				var c = pattern[i];

				if (c == '\'' || c == '"')
				{
					// quoted literal
					i++;
					while (i < pattern.Length && pattern[i] != c)
					{
						literal.Append(pattern[i]);
						i++;
					}

					i++; // skip closing quote
				}
				else if (c == '\\')
				{
					// escaped character
					if (i + 1 < pattern.Length)
					{
						literal.Append(pattern[i + 1]);
					}

					i += 2;
				}
				else if (c == '%')
				{
					// single custom specifier; next letter is a one-character field
					if (i + 1 < pattern.Length && IsFieldLetter(pattern[i + 1]))
					{
						FlushLiteral();
						segments.Add(MakeField(pattern[i + 1], 1));
					}

					i += 2;
				}
				else if (c == '/')
				{
					literal.Append(info.DateSeparator);
					i++;
				}
				else if (c == ':')
				{
					literal.Append(info.TimeSeparator);
					i++;
				}
				else if (IsFieldLetter(c))
				{
					var count = 1;
					while (i + count < pattern.Length && pattern[i + count] == c)
					{
						count++;
					}

					FlushLiteral();
					segments.Add(MakeField(c, count));
					i += count;
				}
				else
				{
					literal.Append(c);
					i++;
				}
			}

			FlushLiteral();
			return segments;
		}


		private static bool IsFieldLetter(char c)
		{
			switch (c)
			{
				case 'd':
				case 'M':
				case 'y':
				case 'h':
				case 'H':
				case 'm':
				case 's':
				case 't':
				case 'g':
				case 'f':
				case 'F':
				case 'z':
				case 'K':
					return true;

				default:
					return false;
			}
		}


		private static DateSegment MakeField(char letter, int count)
		{
			DatePart part;
			switch (letter)
			{
				case 'd':
					part = count <= 2 ? DatePart.Day : DatePart.DayOfWeek;
					break;

				case 'M':
					part = DatePart.Month;
					break;

				case 'y':
					part = DatePart.Year;
					break;

				case 'h':
					part = DatePart.Hour12;
					break;

				case 'H':
					part = DatePart.Hour24;
					break;

				case 'm':
					part = DatePart.Minute;
					break;

				case 's':
					part = DatePart.Second;
					break;

				case 't':
					part = DatePart.AmPm;
					break;

				default:
					part = DatePart.Other;
					break;
			}

			return DateSegment.MakeField(part, letter, count);
		}
	}
}
