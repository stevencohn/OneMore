//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace OneMoreSetupActions
{
	using System;


	/// <summary>
	/// Whether the Defender ASR rule that blocks Office child processes is in effect.
	/// </summary>
	internal enum AsrState
	{
		/// <summary>Could not be determined</summary>
		Unknown,

		/// <summary>Rule is not enforced, so OneNote can start the protocol handler exe</summary>
		NotBlocking,

		/// <summary>Rule is set to block or warn, so OneNote cannot start the exe</summary>
		Blocking
	}


	/// <summary>
	/// Decides from Defender's reported settings whether onemore: links are affected by the
	/// ASR rule "Block all Office applications from creating child processes". Kept free of
	/// WMI so the decision table can be unit tested; see AsrDetector.
	/// </summary>
	internal static class AsrEvaluator
	{
		public const string OfficeChildProcessRule = "D4F940AB-401B-4EFC-AADC-AD5F3C50688A";

		// MSFT_MpPreference.AttackSurfaceReductionRules_Actions values; 0 disabled, 2 audit,
		// 5 not configured, none of which stop OneNote from creating a process
		private const byte ActionBlock = 1;
		private const byte ActionWarn = 6;


		/// <summary>
		/// Evaluates the effective Defender settings.
		/// </summary>
		/// <param name="runningMode">
		/// MSFT_MpComputerStatus.AMRunningMode, or null if it could not be read. ASR is only
		/// enforced when Defender Antivirus is the active engine, i.e. "Normal"; Passive, EDR
		/// block mode or a third-party antivirus do not enforce it.
		/// </param>
		/// <param name="ids">AttackSurfaceReductionRules_Ids, or null if none are configured</param>
		/// <param name="actions">AttackSurfaceReductionRules_Actions, parallel to ids</param>
		public static AsrState Evaluate(string runningMode, string[] ids, byte[] actions)
		{
			if (runningMode is null)
			{
				return AsrState.Unknown;
			}

			if (!string.Equals(runningMode, "Normal", StringComparison.OrdinalIgnoreCase))
			{
				return AsrState.NotBlocking;
			}

			if (ids is null || actions is null)
			{
				return AsrState.NotBlocking;
			}

			var count = Math.Min(ids.Length, actions.Length);
			for (var i = 0; i < count; i++)
			{
				if (string.Equals(ids[i]?.Trim('{', '}'), OfficeChildProcessRule,
					StringComparison.OrdinalIgnoreCase))
				{
					return actions[i] == ActionBlock || actions[i] == ActionWarn
						? AsrState.Blocking
						: AsrState.NotBlocking;
				}
			}

			return AsrState.NotBlocking;
		}
	}
}
