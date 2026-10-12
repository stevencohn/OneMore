//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.UI
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.UI;
	using System;


	[TestClass]
	public class CalendarLayoutTests
	{
		[TestMethod]
		public void FirstVisibleDate_IsOnFirstDayOfWeek()
		{
			var month = new DateTime(2026, 10, 15);

			foreach (DayOfWeek first in Enum.GetValues(typeof(DayOfWeek)))
			{
				var start = CalendarLayout.FirstVisibleDate(month, first);

				Assert.AreEqual(first, start.DayOfWeek);
				Assert.IsTrue(start < new DateTime(2026, 10, 1), "shows trailing days");
				Assert.IsTrue((new DateTime(2026, 10, 1) - start).Days <= 7);
			}
		}


		[TestMethod]
		public void FirstVisibleDate_WhenFirstIsFirstDay_ShowsFullPreviousWeek()
		{
			// 2026-03-01 is a Sunday
			var start = CalendarLayout.FirstVisibleDate(new DateTime(2026, 3, 1), DayOfWeek.Sunday);

			Assert.AreEqual(new DateTime(2026, 2, 22), start);
		}


		[TestMethod]
		public void GridCoversWholeMonth()
		{
			for (var month = 1; month <= 12; month++)
			{
				var date = new DateTime(2026, month, 1);
				var start = CalendarLayout.FirstVisibleDate(date, DayOfWeek.Monday);
				var last = CalendarLayout.CellDate(
					start, CalendarLayout.Rows - 1, CalendarLayout.Columns - 1);

				Assert.IsTrue(last >= date.AddMonths(1).AddDays(-1), $"month {month}");
			}
		}


		[TestMethod]
		public void TryGetCell_RoundTrips()
		{
			var start = CalendarLayout.FirstVisibleDate(new DateTime(2026, 7, 1), DayOfWeek.Sunday);
			var date = new DateTime(2026, 7, 17);

			Assert.IsTrue(CalendarLayout.TryGetCell(start, date, out var row, out var column));
			Assert.AreEqual(date, CalendarLayout.CellDate(start, row, column));

			Assert.IsFalse(CalendarLayout.TryGetCell(start, new DateTime(2027, 1, 1), out _, out _));
		}


		[TestMethod]
		public void FirstVisibleDate_NearMinDate_DoesNotThrow()
		{
			var start = CalendarLayout.FirstVisibleDate(new DateTime(1, 1, 1), DayOfWeek.Sunday);
			Assert.AreEqual(new DateTime(1, 1, 1), start);
		}


		[TestMethod]
		public void CellDate_NearMaxDate_Clamps()
		{
			var start = CalendarLayout.FirstVisibleDate(new DateTime(9999, 12, 1), DayOfWeek.Sunday);
			var last = CalendarLayout.CellDate(start, 5, 6);

			Assert.AreEqual(DateTime.MaxValue.Date, last);
		}


		[TestMethod]
		public void IsInRange_BoundariesAreInclusive()
		{
			var min = new DateTime(1753, 1, 1);
			var max = new DateTime(9998, 12, 31);

			Assert.IsTrue(CalendarLayout.IsInRange(min, min, max));
			Assert.IsTrue(CalendarLayout.IsInRange(max.AddHours(5), min, max));
			Assert.IsFalse(CalendarLayout.IsInRange(min.AddDays(-1), min, max));
			Assert.IsFalse(CalendarLayout.IsInRange(max.AddDays(1), min, max));
		}


		[TestMethod]
		public void IsMonthInRange_PartialMonthsCount()
		{
			var min = new DateTime(2026, 5, 20);
			var max = new DateTime(2026, 8, 10);

			Assert.IsFalse(CalendarLayout.IsMonthInRange(2026, 4, min, max));
			Assert.IsTrue(CalendarLayout.IsMonthInRange(2026, 5, min, max));
			Assert.IsTrue(CalendarLayout.IsMonthInRange(2026, 8, min, max));
			Assert.IsFalse(CalendarLayout.IsMonthInRange(2026, 9, min, max));
			Assert.IsFalse(CalendarLayout.IsMonthInRange(0, 1, min, max));
		}


		[TestMethod]
		public void IsDecadeInRange_Boundaries()
		{
			var min = new DateTime(1753, 1, 1);
			var max = new DateTime(9998, 12, 31);

			Assert.IsFalse(CalendarLayout.IsDecadeInRange(1740, min, max));
			Assert.IsTrue(CalendarLayout.IsDecadeInRange(1750, min, max));
			Assert.IsTrue(CalendarLayout.IsDecadeInRange(9990, min, max));
			Assert.IsFalse(CalendarLayout.IsDecadeInRange(10000, min, max));
		}


		[TestMethod]
		public void PageStarts()
		{
			// 2026 sits in the 2020s; page starts one year before the decade
			Assert.AreEqual(2019, CalendarLayout.YearPageStart(2026));
			Assert.AreEqual(2019, CalendarLayout.YearPageStart(2020));
			Assert.AreEqual(2009, CalendarLayout.YearPageStart(2010));

			// and one decade before the century
			Assert.AreEqual(1990, CalendarLayout.DecadePageStart(2026));
			Assert.AreEqual(1990, CalendarLayout.DecadePageStart(2000));
		}


		[TestMethod]
		public void WeekNumber_UsesCultureRule()
		{
			var culture = new System.Globalization.CultureInfo("en-US");
			var week = CalendarLayout.WeekNumber(new DateTime(2026, 1, 1), culture, DayOfWeek.Sunday);

			Assert.AreEqual(1, week);
		}
	}
}
