//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using OneMoreSetupActions;


	/*
	 * Test Protocol
	 * Verifies the installer's decision table for choosing the COM protocol handler when
	 * Defender's ASR rule "Block all Office applications from creating child processes" applies
	 */

	[TestClass]
	public class AsrEvaluatorTests
	{
		private const string Rule = AsrEvaluator.OfficeChildProcessRule;
		private const string Other = "3B576869-A4EC-4529-8536-B80A7769E899";


		private static AsrState Evaluate(string mode, string[] ids, byte[] actions)
		{
			return AsrEvaluator.Evaluate(mode, ids, actions);
		}


		[TestMethod]
		public void BlockAndWarnAreBlocking()
		{
			Assert.AreEqual(AsrState.Blocking, Evaluate("Normal", new[] { Rule }, new byte[] { 1 }));
			Assert.AreEqual(AsrState.Blocking, Evaluate("Normal", new[] { Rule }, new byte[] { 6 }));
		}


		[TestMethod]
		public void AuditDisabledAndNotConfiguredAreNotBlocking()
		{
			Assert.AreEqual(AsrState.NotBlocking, Evaluate("Normal", new[] { Rule }, new byte[] { 2 }));
			Assert.AreEqual(AsrState.NotBlocking, Evaluate("Normal", new[] { Rule }, new byte[] { 0 }));
			Assert.AreEqual(AsrState.NotBlocking, Evaluate("Normal", new[] { Rule }, new byte[] { 5 }));
		}


		[TestMethod]
		public void RuleAbsentOrNoRulesIsNotBlocking()
		{
			Assert.AreEqual(AsrState.NotBlocking, Evaluate("Normal", new[] { Other }, new byte[] { 1 }));
			Assert.AreEqual(AsrState.NotBlocking, Evaluate("Normal", new string[0], new byte[0]));
			Assert.AreEqual(AsrState.NotBlocking, Evaluate("Normal", null, null));
		}


		[TestMethod]
		public void FindsRuleAmongOthers()
		{
			var ids = new[] { Other, Rule.ToLowerInvariant(), "00000000-0000-0000-0000-000000000000" };

			Assert.AreEqual(AsrState.Blocking, Evaluate("Normal", ids, new byte[] { 2, 1, 0 }));
			Assert.AreEqual(AsrState.NotBlocking, Evaluate("Normal", ids, new byte[] { 1, 2, 1 }));
		}


		[TestMethod]
		public void GuidMatchIgnoresCaseAndBraces()
		{
			Assert.AreEqual(AsrState.Blocking,
				Evaluate("Normal", new[] { "{" + Rule.ToLowerInvariant() + "}" }, new byte[] { 1 }));
		}


		[TestMethod]
		public void DefenderNotActiveEngineIsNotBlocking()
		{
			// ASR is only enforced when Defender Antivirus is the active engine
			foreach (var mode in new[] { "Passive", "SxS Passive Mode", "EDR Block Mode", "Not running" })
			{
				Assert.AreEqual(AsrState.NotBlocking,
					Evaluate(mode, new[] { Rule }, new byte[] { 1 }), mode);
			}
		}


		[TestMethod]
		public void UnreadableRunningModeIsUnknown()
		{
			Assert.AreEqual(AsrState.Unknown, Evaluate(null, new[] { Rule }, new byte[] { 1 }));
		}


		[TestMethod]
		public void MismatchedArraysAreTolerated()
		{
			// the rule has no parallel action, so it can't be treated as blocking
			Assert.AreEqual(AsrState.NotBlocking,
				Evaluate("Normal", new[] { Other, Rule }, new byte[] { 1 }));
		}
	}
}
