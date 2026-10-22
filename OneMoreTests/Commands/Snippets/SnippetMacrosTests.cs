//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Snippets
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using System;

	/*
	 * Test Protocol - SnippetMacros
	 * Macros in snippets are expanded when a snippet is inserted.
	 *
	 *   1. Insert a snippet containing every macro; each is replaced with its value.
	 *   2. Macros are not case sensitive; unrecognized macros are left as-is.
	 *   3. {=SELECTION}, {=BODY} and {=SNIPPET_BODY} behave like {SNIPPET_BODY}.
	 */

	[TestClass]
	public class SnippetMacrosTests : TestBase
	{
		private static SnippetMacros Create(int year = 2026, int month = 10, int day = 22)
		{
			return new SnippetMacros(new DateTime(year, month, day, 13, 45, 0));
		}


		[TestMethod]
		public void Quarter()
		{
			Assert.AreEqual("Q4", Create().Expand("{=quarter}"));
			Assert.AreEqual("Q1", Create(2026, 3, 31).Expand("{=QUARTER}"));
		}


		[TestMethod]
		public void DateTimeWithParentheses()
		{
			Assert.AreEqual("2026-10-22 (Q4)",
				Create().Expand("{=DATETIME(yyyy-MM-dd)} ({=QUARTER})"));
		}


		[TestMethod]
		public void TwoDateTimes()
		{
			Assert.AreEqual("2026/10",
				Create().Expand("{=DATETIME(yyyy)}/{=DATETIME(MM)}"));
		}


		[TestMethod]
		public void IsoWeekNumbers()
		{
			Assert.AreEqual(1, SnippetMacros.IsoWeekNumber(new DateTime(2026, 1, 1)));
			Assert.AreEqual(1, SnippetMacros.IsoWeekNumber(new DateTime(2024, 12, 30)));
			Assert.AreEqual(53, SnippetMacros.IsoWeekNumber(new DateTime(2021, 1, 3)));
			Assert.AreEqual(43, SnippetMacros.IsoWeekNumber(new DateTime(2026, 10, 22)));
			Assert.AreEqual("43", Create().Expand("{=WeekNumber}"));
		}


		[TestMethod]
		public void DaysUntil()
		{
			Assert.AreEqual("70", Create().Expand("{=DAYSUNTIL(2026-12-31)}"));
			Assert.AreEqual("-1", Create().Expand("{=DAYSUNTIL(2026-10-21)}"));
			Assert.AreEqual("0", Create().Expand("{=DAYSUNTIL(2026-10-22)}"));
		}


		[TestMethod]
		public void InvalidDaysUntilLeftAsIs()
		{
			Assert.AreEqual("{=DAYSUNTIL(nope)}", Create().Expand("{=DAYSUNTIL(nope)}"));
			Assert.AreEqual("{=DAYSUNTIL}", Create().Expand("{=DAYSUNTIL}"));
		}


		[TestMethod]
		public void UnrecognizedLeftAsIs()
		{
			Assert.AreEqual("{=BOGUS} {=QUARTER(1)}", Create().Expand("{=BOGUS} {=QUARTER(1)}"));
		}


		[TestMethod]
		public void PageValuesAreEncoded()
		{
			var macros = Create();
			macros.PageTitle = "R&D <plan>";
			macros.Section = "Work";
			macros.Notebook = "Main";
			macros.PageLink = "onenote:https://x/y?a=1&b=2";

			Assert.AreEqual("R&amp;D &lt;plan&gt; Work Main",
				macros.Expand("{=PageTitle} {=section} {=NOTEBOOK}"));

			Assert.AreEqual(
				"<a href=\"onenote:https://x/y?a=1&amp;b=2\">R&amp;D &lt;plan&gt;</a>",
				macros.Expand("{=PAGELINK}"));
		}


		[TestMethod]
		public void PromptAndCounter()
		{
			var macros = Create();
			macros.Prompts["Who"] = "Ann";
			macros.Counters["Meeting"] = 6;
			macros.Counters["Other"] = 0;

			Assert.AreEqual("Ann Ann #7 #8 #1",
				macros.Expand("{=PROMPT(Who)} {=prompt(who)} #{=COUNTER(meeting)} " +
					"#{=COUNTER(MEETING)} #{=COUNTER(other)}"));

			Assert.AreEqual("{=PROMPT(Other)}", macros.Expand("{=PROMPT(Other)}"));
		}


		[TestMethod]
		public void BodyAliases()
		{
			Assert.AreEqual("{SNIPPET_BODY}", Create().Expand("{=SELECTION}"));
			Assert.AreEqual("{SNIPPET_BODY}", Create().Expand("{=body}"));
			Assert.AreEqual("{SNIPPET_BODY}", Create().Expand("{=SNIPPET_BODY}"));
		}
	}
}
