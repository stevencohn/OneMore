//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Tables
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands.Tables.Formulas;


	[TestClass]
	public class CalculatorTests
	{
		[TestMethod]
		public void SimplePower()
		{
			Assert.AreEqual(16.0, new Calculator().Compute("4 ^ 2"));
		}


		[TestMethod]
		public void LargePowerUsesExponentNotation()
		{
			// result.ToString() emits "3.09485009821345E+26", which must parse back
			var result = new Calculator().Compute("4 ^ 44");
			Assert.AreEqual(System.Math.Pow(4, 44), result, System.Math.Pow(4, 44) * 1e-12);
		}


		[TestMethod]
		public void LargeIntermediateResults()
		{
			Assert.AreEqual(4.0, new Calculator().Compute("2^100 / 2^98"), 1e-9);
			// results pass through string tokens with 15 significant digits; use relative tolerance
			var expected = System.Math.Pow(4, 30) + 1;
			Assert.AreEqual(expected, new Calculator().Compute("(4^30)+1"), expected * 1e-12);
		}


		[TestMethod]
		public void SmallResultUsesExponentNotation()
		{
			Assert.AreEqual(0.00001, new Calculator().Compute("1/100000"), 1e-12);
		}


		[TestMethod]
		public void OutOfRangeThrows()
		{
			var ex = Assert.ThrowsException<CalculatorException>(
				() => new Calculator().Compute("10 ^ 400"));

			StringAssert.DoesNotMatch(ex.Message, new System.Text.RegularExpressions.Regex("Undefined"));
		}
	}
}
