//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.UI
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.UI;
	using System;


	[TestClass]
	public class DatePartMathTests
	{
		private static readonly DateTime Min = new DateTime(1753, 1, 1);
		private static readonly DateTime Max = new DateTime(9998, 12, 31);


		[TestMethod]
		public void Wrap_WrapsBothDirections()
		{
			Assert.AreEqual(1, DatePartMath.Wrap(12, 1, 1, 12));
			Assert.AreEqual(12, DatePartMath.Wrap(1, -1, 1, 12));
			Assert.AreEqual(3, DatePartMath.Wrap(3, 24, 1, 12));
		}


		[TestMethod]
		public void Adjust_Month_WrapsWithinYearAndClampsDay()
		{
			var date = new DateTime(2026, 1, 31, 8, 0, 0);

			var feb = DatePartMath.Adjust(date, DatePart.Month, 1, Min, Max);
			Assert.AreEqual(new DateTime(2026, 2, 28, 8, 0, 0), feb);

			var dec = DatePartMath.Adjust(date, DatePart.Month, -1, Min, Max);
			Assert.AreEqual(new DateTime(2026, 12, 31, 8, 0, 0), dec);
		}


		[TestMethod]
		public void Adjust_Year_LeapDayClamps()
		{
			var date = new DateTime(2024, 2, 29);
			var next = DatePartMath.Adjust(date, DatePart.Year, 1, Min, Max);

			Assert.AreEqual(new DateTime(2025, 2, 28), next);
		}


		[TestMethod]
		public void Adjust_Day_WrapsWithinMonth()
		{
			var date = new DateTime(2026, 4, 30);

			Assert.AreEqual(1, DatePartMath.Adjust(date, DatePart.Day, 1, Min, Max).Day);
			Assert.AreEqual(30,
				DatePartMath.Adjust(new DateTime(2026, 4, 1), DatePart.Day, -1, Min, Max).Day);
		}


		[TestMethod]
		public void Adjust_Hour12_StaysInHalfDay()
		{
			var pm = new DateTime(2026, 1, 1, 23, 0, 0);
			var result = DatePartMath.Adjust(pm, DatePart.Hour12, 1, Min, Max);

			// 11 PM + 1 hour wraps to 12 PM (noon), not 12 AM, as with the stock control
			Assert.AreEqual(12, result.Hour);
			Assert.AreEqual(1, result.Day);
		}


		[TestMethod]
		public void Adjust_AmPm_Toggles()
		{
			var date = new DateTime(2026, 1, 1, 9, 30, 0);
			var result = DatePartMath.Adjust(date, DatePart.AmPm, 1, Min, Max);

			Assert.AreEqual(21, result.Hour);
			Assert.AreEqual(30, result.Minute);
		}


		[TestMethod]
		public void Adjust_OutOfRange_IsRejectedNotClamped()
		{
			var date = new DateTime(2026, 6, 15);
			var min = new DateTime(2026, 6, 10);
			var max = new DateTime(2026, 6, 20);

			Assert.AreEqual(date,
				DatePartMath.Adjust(date, DatePart.DayOfWeek, 30, min, max));
		}


		[TestMethod]
		public void Adjust_Year_PastMax_DoesNotChangeDay()
		{
			// search dialog: MaxDate is tomorrow, user previously picked an earlier day
			var max = new DateTime(2026, 10, 12);
			var date = new DateTime(2026, 10, 5);

			Assert.AreEqual(date, DatePartMath.Adjust(date, DatePart.Year, 1, Min, max));
		}


		[TestMethod]
		public void Adjust_Month_PastMax_SkipsToNextInRangeMonth()
		{
			var max = new DateTime(2026, 10, 12);
			var date = new DateTime(2026, 10, 5);

			// Nov and Dec 2026 are out of range; wraps to January of the same year
			var result = DatePartMath.Adjust(date, DatePart.Month, 1, Min, max);

			Assert.AreEqual(new DateTime(2026, 1, 5), result);
		}


		[TestMethod]
		public void Adjust_Month_NoValidMonth_Unchanged()
		{
			var min = new DateTime(2026, 10, 5);
			var max = new DateTime(2026, 10, 12);
			var date = new DateTime(2026, 10, 7);

			Assert.AreEqual(date, DatePartMath.Adjust(date, DatePart.Month, 1, min, max));
			Assert.AreEqual(date, DatePartMath.Adjust(date, DatePart.Month, -1, min, max));
		}


		[TestMethod]
		public void Adjust_PreservesKindAndFraction()
		{
			var date = new DateTime(2026, 5, 5, 1, 2, 3, DateTimeKind.Utc).AddTicks(1234);
			var result = DatePartMath.Adjust(date, DatePart.Minute, 1, Min, Max);

			Assert.AreEqual(DateTimeKind.Utc, result.Kind);
			Assert.AreEqual(date.Ticks % TimeSpan.TicksPerSecond,
				result.Ticks % TimeSpan.TicksPerSecond);
		}


		[TestMethod]
		public void SetPart_Hour12_KeepsAmPm()
		{
			var pm = new DateTime(2026, 1, 1, 15, 0, 0);

			Assert.AreEqual(23, DatePartMath.SetPart(pm, DatePart.Hour12, 11)?.Hour);
			Assert.AreEqual(12, DatePartMath.SetPart(pm, DatePart.Hour12, 12)?.Hour);

			var am = new DateTime(2026, 1, 1, 3, 0, 0);
			Assert.AreEqual(0, DatePartMath.SetPart(am, DatePart.Hour12, 12)?.Hour);
		}


		[TestMethod]
		public void SetPart_InvalidReturnsNull()
		{
			var date = new DateTime(2026, 2, 1);

			Assert.IsNull(DatePartMath.SetPart(date, DatePart.Month, 13));
			Assert.IsNull(DatePartMath.SetPart(date, DatePart.Day, 30));
			Assert.IsNull(DatePartMath.SetPart(date, DatePart.Minute, 60));
		}


		[TestMethod]
		public void SetPart_Month_ClampsDay()
		{
			var date = new DateTime(2026, 1, 31);
			var result = DatePartMath.SetPart(date, DatePart.Month, 2);

			Assert.AreEqual(new DateTime(2026, 2, 28), result);
		}


		[TestMethod]
		public void TryEnter_Month_AdvancesWhenNoMoreDigitsPossible()
		{
			var date = new DateTime(2026, 1, 1);

			Assert.IsTrue(DatePartMath.TryEnter(
				"", '1', DatePart.Month, date, 2, out var buffer, out var complete));
			Assert.AreEqual("1", buffer);
			Assert.IsFalse(complete);   // could become 10, 11, 12

			Assert.IsTrue(DatePartMath.TryEnter(
				buffer, '2', DatePart.Month, date, 2, out buffer, out complete));
			Assert.AreEqual("12", buffer);
			Assert.IsTrue(complete);

			Assert.IsTrue(DatePartMath.TryEnter(
				"", '5', DatePart.Month, date, 2, out buffer, out complete));
			Assert.AreEqual("5", buffer);
			Assert.IsTrue(complete);    // 50+ is not a month
		}


		[TestMethod]
		public void TryEnter_Overflow_RestartsBuffer()
		{
			var date = new DateTime(2026, 1, 1);

			// "1" then "3" would be month 13: restart with "3"
			DatePartMath.TryEnter("1", '3', DatePart.Month, date, 2, out var buffer, out var complete);

			Assert.AreEqual("3", buffer);
			Assert.IsTrue(complete);
		}


		[TestMethod]
		public void TryEnter_DoubleZero_IsRejected()
		{
			var date = new DateTime(2026, 1, 1);

			DatePartMath.TryEnter("0", '0', DatePart.Day, date, 2, out var buffer, out var complete);

			Assert.AreEqual("0", buffer);
			Assert.IsFalse(complete);
		}


		[TestMethod]
		public void TryEnter_Day_UsesLengthOfMonth()
		{
			var feb = new DateTime(2025, 2, 1);

			// 3 can start 30/31 in most months but not in February: no second digit possible
			DatePartMath.TryEnter("", '3', DatePart.Day, feb, 2, out var buffer, out var complete);

			Assert.AreEqual("3", buffer);
			Assert.IsTrue(complete);
		}


		[TestMethod]
		public void TryEnter_NonDigit_IsRejected()
		{
			var ok = DatePartMath.TryEnter(
				"1", 'x', DatePart.Month, DateTime.Today, 2, out var buffer, out var complete);

			Assert.IsFalse(ok);
			Assert.AreEqual("1", buffer);
			Assert.IsFalse(complete);
		}


		[TestMethod]
		public void TryEnter_FourDigitYear()
		{
			var date = new DateTime(2026, 1, 1);
			var buffer = "";
			var complete = false;

			foreach (var digit in "2031")
			{
				DatePartMath.TryEnter(buffer, digit, DatePart.Year, date, 4, out buffer, out complete);
			}

			Assert.AreEqual("2031", buffer);
			Assert.IsTrue(complete);
		}
	}
}
