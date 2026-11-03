//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Tables
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands.Tables.Formulas;


	[TestClass]
	public class FormulaFixerTests
	{
		// formula anchored at C2, shifted by (dr, dc)
		private static string Shift(string expression, int dr, int dc, out bool review)
		{
			return FormulaFixer.Shift(expression, 2, 3, dr, dc, out review);
		}


		[TestMethod]
		public void RowInsertedAbove_ShiftsSameRowRef()
		{
			Assert.AreEqual("B3+11", Shift("B2+11", 1, 0, out var review));
			Assert.IsFalse(review);
		}


		[TestMethod]
		public void ColumnInsertedLeft_ShiftsSameRowRef()
		{
			Assert.AreEqual("C2+11", Shift("B2+11", 0, 1, out var review));
			Assert.IsFalse(review);
		}


		[TestMethod]
		public void RowDeleted_ShiftsUp()
		{
			Assert.AreEqual("B2*2", Shift("B3*2", -1, 0, out _));
		}


		[TestMethod]
		public void Range_ShiftsAsUnit()
		{
			// formula in C2 summing C5:C9 (same column) after one row inserted above
			Assert.AreEqual("sum(C6:C10)", Shift("sum(C5:C9)", 1, 0, out var review));
			Assert.IsFalse(review);
		}


		[TestMethod]
		public void UnrelatedRef_IsLeftAloneAndFlagged()
		{
			Assert.AreEqual("A9+B3", Shift("A9+B2", 1, 0, out var review));
			Assert.IsTrue(review);
		}


		[TestMethod]
		public void FunctionNames_AreNotReferences()
		{
			Assert.AreEqual("log10(B3)", Shift("log10(B2)", 1, 0, out _));
		}


		[TestMethod]
		public void OutOfBounds_IsLeftAloneAndFlagged()
		{
			Assert.AreEqual("B2", Shift("B2", -5, 0, out var review));
			Assert.IsTrue(review);
		}
	}
}
