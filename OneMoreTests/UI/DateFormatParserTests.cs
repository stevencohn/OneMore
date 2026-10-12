//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.UI
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Globalization;
	using System.Text;
	using System.Windows.Forms;


	[TestClass]
	public class DateFormatParserTests
	{
		private static readonly CultureInfo US = new CultureInfo("en-US");
		private static readonly CultureInfo DE = new CultureInfo("de-DE");
		private static readonly DateTime Sample = new DateTime(2026, 3, 9, 14, 5, 7);


		private static string Render(string pattern, CultureInfo culture)
		{
			var builder = new StringBuilder();
			foreach (var segment in DateFormatParser.Parse(pattern, culture))
			{
				builder.Append(segment.Format(Sample, culture));
			}

			return builder.ToString();
		}


		[TestMethod]
		public void LongDatePattern_RendersLikeDateTime()
		{
			var pattern = US.DateTimeFormat.LongDatePattern;
			Assert.AreEqual(Sample.ToString(pattern, US), Render(pattern, US));
		}


		[TestMethod]
		public void CustomPattern_RendersLikeDateTime()
		{
			const string pattern = "dddd, MMMM d, yyyy h:mm tt";
			Assert.AreEqual(Sample.ToString(pattern, US), Render(pattern, US));
		}


		[TestMethod]
		public void Separators_UseCultureSeparators()
		{
			// de-DE uses '.' for dates and ':' for time
			Assert.AreEqual("09.03.2026", Render("dd/MM/yyyy", DE));
			Assert.AreEqual("14:05:07", Render("HH:mm:ss", DE));
		}


		[TestMethod]
		public void QuotedAndEscaped_AreLiterals()
		{
			var segments = DateFormatParser.Parse("'Day' d \\M", US);

			Assert.IsTrue(segments[0].IsLiteral);
			Assert.AreEqual("Day ", segments[0].Literal);
			Assert.AreEqual("Day 9 M", Render("'Day' d \\M", US));
		}


		[TestMethod]
		public void UnterminatedQuote_RunsToEnd()
		{
			var segments = DateFormatParser.Parse("yyyy 'abc", US);

			Assert.AreEqual(2, segments.Count);
			Assert.AreEqual(DatePart.Year, segments[0].Part);
			Assert.AreEqual(" abc".Length, segments[1].Literal.Length);
		}


		[TestMethod]
		public void PercentSpecifier_IsSingleLetterField()
		{
			var segments = DateFormatParser.Parse("%d", US);

			Assert.AreEqual(1, segments.Count);
			Assert.AreEqual(DatePart.Day, segments[0].Part);
			Assert.AreEqual("9", Render("%d", US));
		}


		[TestMethod]
		public void WeekdayNames_AreNotEditable()
		{
			var segments = DateFormatParser.Parse("dddd d", US);

			Assert.AreEqual(DatePart.DayOfWeek, segments[0].Part);
			Assert.IsFalse(segments[0].IsEditable);
			Assert.IsTrue(segments[2].IsEditable);
		}


		[TestMethod]
		public void GetPattern_MapsKinds()
		{
			var info = US.DateTimeFormat;

			Assert.AreEqual(info.LongDatePattern,
				DateFormatParser.GetPattern(DateTimePickerFormat.Long, null, US));

			Assert.AreEqual(info.ShortDatePattern,
				DateFormatParser.GetPattern(DateTimePickerFormat.Short, null, US));

			Assert.AreEqual(info.LongTimePattern,
				DateFormatParser.GetPattern(DateTimePickerFormat.Time, null, US));

			Assert.AreEqual("yyyy",
				DateFormatParser.GetPattern(DateTimePickerFormat.Custom, "yyyy", US));

			// empty custom format behaves like Long
			Assert.AreEqual(info.LongDatePattern,
				DateFormatParser.GetPattern(DateTimePickerFormat.Custom, "", US));
		}


		[TestMethod]
		public void MaxDigits_MatchesPart()
		{
			Assert.AreEqual(4, DateFormatParser.Parse("yyyy", US)[0].MaxDigits);
			Assert.AreEqual(2, DateFormatParser.Parse("yy", US)[0].MaxDigits);
			Assert.AreEqual(2, DateFormatParser.Parse("MM", US)[0].MaxDigits);
			Assert.AreEqual(0, DateFormatParser.Parse("MMM", US)[0].MaxDigits);
			Assert.AreEqual(0, DateFormatParser.Parse("tt", US)[0].MaxDigits);
		}
	}
}
