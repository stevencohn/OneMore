//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace OneMoreCli
{
	using Microsoft.Win32;
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;


	/// <summary>
	/// Locates ONENOTE.EXE and ensures it is running as a full UI process, not just as a
	/// headless COM server. Some OneNote APIs — notably Publish — require services that
	/// only initialize when OneNote is launched interactively, so commands that depend on
	/// those APIs must call <see cref="EnsureRunning"/> before any COM activation.
	/// </summary>
	internal static class OneNoteLauncher
	{
		/// <summary>
		/// Returns true if an interactive ONENOTE.EXE is already running, otherwise locates
		/// the executable via the registry, launches it, and waits up to 60s for its main
		/// window to appear plus a small buffer for service init.
		/// </summary>
		public static async Task<bool> EnsureRunning()
		{
			if (HasInteractiveInstance())
			{
				return true;
			}

			var path = FindOneNoteExe();
			if (string.IsNullOrEmpty(path) || !File.Exists(path))
			{
				CliConsole.WriteError(
					$"Cannot locate ONENOTE.EXE (registry lookup returned '{path ?? "null"}')");
				return false;
			}

			CliConsole.WriteInfo($"Launching ONENOTE.EXE: {path}");
			try
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = path,
					UseShellExecute = true
				});
			}
			catch (Exception exc)
			{
				CliConsole.WriteError($"Error launching ONENOTE.EXE: {exc.Message}");
				return false;
			}

			var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(60);
			while (DateTime.UtcNow < deadline)
			{
				await Task.Delay(500);
				if (HasInteractiveInstance())
				{
					// extra buffer for OneNote to finish wiring up its internal services
					await Task.Delay(3000);
					return true;
				}
			}

			return false;
		}


		// "Interactive" = a ONENOTE.EXE process whose main UI window has been created.
		// A process started purely for COM activation (e.g. via `new Application()`) has
		// no MainWindowHandle and does not satisfy this check.
		private static bool HasInteractiveInstance()
		{
			foreach (var proc in Process.GetProcessesByName("ONENOTE"))
			{
				try
				{
					proc.Refresh();
					if (proc.MainWindowHandle != IntPtr.Zero)
					{
						return true;
					}
				}
				catch
				{
					// process may exit between enumerate and inspect — skip it
				}
			}
			return false;
		}


		// Mirrors the registry lookup order used by OneMoreSetupActions.CheckBitnessAction
		// so we find ONENOTE.EXE on every supported Office configuration (C2R, MSI, x86,
		// x64, ARM64 native and ARM64EC emulated).
		private static string FindOneNoteExe()
		{
			const string appPathSub = @"Microsoft\Windows\CurrentVersion\App Paths\OneNote.exe";

			return ReadDefaultValue(@$"SOFTWARE\{appPathSub}")
				?? ReadDefaultValue(@$"SOFTWARE\WOW6432Node\{appPathSub}")
				?? ReadInstallRootValue(@"SOFTWARE\Microsoft\Office", "OneNote")
				?? ReadInstallRootValue(@"SOFTWARE\WOW6432Node\Microsoft\Office", "OneNote")
				?? ReadInstallRootValue(@"SOFTWARE\Microsoft\Office", "Common")
				?? ReadInstallRootValue(@"SOFTWARE\WOW6432Node\Microsoft\Office", "Common");
		}


		private static string ReadDefaultValue(string keyPath)
		{
			using var key = Registry.LocalMachine.OpenSubKey(keyPath, false);
			var value = key?.GetValue(null, string.Empty) as string;
			return string.IsNullOrWhiteSpace(value) ? null : value;
		}


		private static string ReadInstallRootValue(string keyPath, string subname)
		{
			using var key = Registry.LocalMachine.OpenSubKey(keyPath, false);
			if (key == null) return null;

			foreach (var name in key.GetSubKeyNames()
				.Where(n => Regex.IsMatch(n, @"^\d+\.\d+$")))
			{
				using var subkey = key.OpenSubKey($@"{name}\{subname}\InstallRoot", false);
				var value = subkey?.GetValue("Path") as string;
				if (!string.IsNullOrWhiteSpace(value))
				{
					return Path.Combine(value, "ONENOTE.EXE");
				}
			}

			return null;
		}
	}
}
