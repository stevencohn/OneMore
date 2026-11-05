//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Tables
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands.Tables.Formulas;
	using River.OneMoreAddIn.Models;
	using System.Linq;


	[TestClass]
	public class FormulaShifterTests
	{
		[TestMethod]
		public void ZeroOffsetIsUnchanged()
		{
			Assert.AreEqual("average(b2:b9)",
				FormulaShifter.Shift("average(b2:b9)", 0, TableSelectionRange.Columns));
		}


		[TestMethod]
		public void ColumnsShiftLetters()
		{
			Assert.AreEqual("average(c2:c9)",
				FormulaShifter.Shift("average(b2:b9)", 1, TableSelectionRange.Columns));
		}


		[TestMethod]
		public void RowsShiftNumbers()
		{
			Assert.AreEqual("a3+b3",
				FormulaShifter.Shift("a2+b2", 1, TableSelectionRange.Rows));
		}


		[TestMethod]
		public void LongerReferencesStillShiftCorrectly()
		{
			Assert.AreEqual("sum(a10:a99)+a100",
				FormulaShifter.Shift("sum(a9:a98)+a99", 1, TableSelectionRange.Rows));
		}


		[TestMethod]
		public void TokenizerClassifiesParts()
		{
			var text = "average(a2:cell(0,-1))";
			var kinds = FormulaTokenizer.Tokenize(text)
				.Where(t => t.Kind != FormulaTokenKind.Punctuation)
				.Select(t => (text.Substring(t.Start, t.Length), t.Kind))
				.ToList();

			CollectionAssert.AreEqual(new[]
			{
				("average", FormulaTokenKind.Function),
				("a2", FormulaTokenKind.Cell),
				("cell", FormulaTokenKind.Function),
				("0", FormulaTokenKind.Number),
				("1", FormulaTokenKind.Number)
			}, kinds);
		}


		[TestMethod]
		public void TokenizerMarksCountifCriteria()
		{
			var text = "countif(a1:a8, < 123)";
			var tokens = FormulaTokenizer.Tokenize(text);
			Assert.IsTrue(tokens.Any(t =>
				t.Kind == FormulaTokenKind.Criteria && text.Substring(t.Start, t.Length) == "123"));
		}
	}
}
