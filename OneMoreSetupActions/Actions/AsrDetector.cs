//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace OneMoreSetupActions
{
	using System;
	using System.Management;


	/// <summary>
	/// Reads Defender's effective ASR configuration through WMI, the same source as the
	/// Get-MpPreference cmdlet, which merges local, Group Policy and Intune settings.
	/// </summary>
	internal static class AsrDetector
	{
		private const string Scope = @"root\Microsoft\Windows\Defender";


		/// <summary>
		/// Determines whether the ASR rule that blocks Office child processes is in effect.
		/// Never throws; any failure, such as Defender not being installed, is logged and
		/// reported as Unknown.
		/// </summary>
		public static AsrState Detect(Logger logger)
		{
			try
			{
				string runningMode = null;
				using (var searcher = new ManagementObjectSearcher(
					Scope, "SELECT AMRunningMode FROM MSFT_MpComputerStatus"))
				using (var results = searcher.Get())
				{
					foreach (var item in results)
					{
						runningMode = item["AMRunningMode"] as string;
						item.Dispose();
					}
				}

				string[] ids = null;
				byte[] actions = null;
				using (var searcher = new ManagementObjectSearcher(Scope,
					"SELECT AttackSurfaceReductionRules_Ids, AttackSurfaceReductionRules_Actions " +
					"FROM MSFT_MpPreference"))
				using (var results = searcher.Get())
				{
					foreach (var item in results)
					{
						ids = item["AttackSurfaceReductionRules_Ids"] as string[];
						actions = ToBytes(item["AttackSurfaceReductionRules_Actions"]);
						item.Dispose();
					}
				}

				var state = AsrEvaluator.Evaluate(runningMode, ids, actions);
				logger.WriteLine(
					$"ASR: AMRunningMode={runningMode}, configured rules={ids?.Length ?? 0}, " +
					$"office child process rule => {state}");

				return state;
			}
			catch (Exception exc)
			{
				logger.WriteLine("could not read Defender ASR settings; treating as unknown");
				logger.WriteLine(exc);
				return AsrState.Unknown;
			}
		}


		private static byte[] ToBytes(object value)
		{
			if (value is byte[] bytes)
			{
				return bytes;
			}

			if (value is Array array)
			{
				var result = new byte[array.Length];
				for (var i = 0; i < array.Length; i++)
				{
					result[i] = Convert.ToByte(array.GetValue(i));
				}

				return result;
			}

			return Array.Empty<byte>();
		}
	}
}
