//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Identity
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Identity;


	[TestClass]
	public class GuidRetryLedgerTests
	{
		[TestMethod]
		public void UnknownPage_IsNotSkipped()
		{
			var ledger = new GuidRetryLedger();
			ledger.NextPass();

			Assert.IsFalse(ledger.ShouldSkip(1, "{a}"));
		}


		[TestMethod]
		public void FailedPage_IsSkippedForAWhile_ThenTriedAgain()
		{
			var ledger = new GuidRetryLedger();
			ledger.NextPass();
			ledger.Failed(1, "{a}");

			ledger.NextPass();
			Assert.IsTrue(ledger.ShouldSkip(1, "{a}"), "skipped on the next pass");

			ledger.NextPass();
			Assert.IsFalse(ledger.ShouldSkip(1, "{a}"), "tried again after the first wait");
		}


		[TestMethod]
		public void PageThatKeepsFailing_IsGivenUp()
		{
			var ledger = new GuidRetryLedger();

			for (var i = 0; i < GuidRetryLedger.MaxAttempts; i++)
			{
				ledger.NextPass();
				ledger.Failed(1, "{a}");
			}

			for (var i = 0; i < 200; i++)
			{
				ledger.NextPass();
				Assert.IsTrue(ledger.ShouldSkip(1, "{a}"));
			}
		}


		[TestMethod]
		public void NewPageID_GetsAFreshTry()
		{
			var ledger = new GuidRetryLedger();
			ledger.NextPass();
			ledger.Failed(1, "{a}");

			ledger.NextPass();
			Assert.IsFalse(ledger.ShouldSkip(1, "{b}"));
			Assert.AreEqual(0, ledger.Count);
		}


		[TestMethod]
		public void SuccessForgetsThePage()
		{
			var ledger = new GuidRetryLedger();
			ledger.NextPass();
			ledger.Failed(1, "{a}");
			ledger.Succeeded(1);

			Assert.AreEqual(0, ledger.Count);
		}
	}
}
