//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S1075 // URIs should not be hardcoded

namespace OneMoreSetupActions
{
	using Microsoft.Win32;
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Net.Http;
	using System.Text.RegularExpressions;
	using System.Threading;

	internal class EdgeWebViewDeployment : Deployment
	{
		private const string DownloadUrl = "https://go.microsoft.com/fwlink/p/?LinkId=2124703";
		private const string ClientKey = @"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients";
		private const string RuntimeId = "{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}";


		public EdgeWebViewDeployment(Logger logger, Stepper stepper)
			: base(logger, stepper)
		{
		}


		public override bool Install()
		{
			logger.WriteLine();
			logger.WriteLine("EdgeWebViewDeployment.Install ---");

			var key = Registry.LocalMachine.OpenSubKey($"{ClientKey}\\{RuntimeId}");
			if (key != null)
			{
				logger.WriteLine("WebView2 Runtime is already installed");
				CleanupChromium();
				return true;
			}

			var bootstrap = Path.Combine(
			Path.GetTempPath(),
			Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".exe");

			if (!DownloadBootstrap(bootstrap))
			{
				return false;
			}

			logger.WriteLine("running bootstrap");
			Process.Start(bootstrap).WaitForExit();

			while (true)
			{
				if (!Process.GetProcesses()
					.Any(p => p.ProcessName.StartsWith("MicrosoftEdgeUpdate")))
				{
					logger.WriteLine("maybe done");
					break;
				}

				logger.WriteLine("waiting for MicrosoftEdgeUpdate process to exit...");
				Thread.Sleep(1000);
			}

			try
			{
				logger.WriteLine("cleaning up bootstrap file");
				File.Delete(bootstrap);
			}
			catch (Exception exc)
			{
				logger.WriteLine("error deleting bootstrap");
				logger.WriteLine(exc);
			}

			// deprecate
			CleanupChromium();

			return true;
		}


		private bool DownloadBootstrap(string bootstrap)
		{
			using (var client = new HttpClient())
			{
				using (var response = client.GetAsync(new Uri(DownloadUrl, UriKind.Absolute)).Result)
				{
					if (response.IsSuccessStatusCode)
					{
						using (var stream = new FileStream(bootstrap, FileMode.CreateNew))
						{
							response.Content.CopyToAsync(stream).Wait();
							logger.WriteLine($"downloaded {bootstrap}");
							return true;
						}
					}
					else
					{
						logger.WriteLine($"download status code[{response.StatusCode}]");
					}
				}
			}

			return false;
		}


		public override bool Uninstall()
		{
			logger.WriteLine();
			logger.WriteLine("EdgeWebViewDeployment.Uninstall ---");

			var key = Registry.LocalMachine.OpenSubKey($"{ClientKey}\\{RuntimeId}");
			if (key != null)
			{
				// "C:\Program Files (x86)\Microsoft\EdgeWebView\Application\96.0.1054.34\Installer\setup.exe"
				//    --force-uninstall --uninstall --msedgewebview --system-level --verbose-logging

				var command = key.GetValue("SilentUninstall") as string;
				if (!string.IsNullOrEmpty(command))
				{
					var match = Regex.Match(command, "\"([^\"]*)\" (.*)");
					if (match.Success)
					{
						logger.WriteLine($"command: {command}");

						Process.Start(match.Groups[1].Value, match.Groups[2].Value).WaitForExit();
						return true;
					}
				}
				else
				{
					logger.WriteLine("SilentUninstall key not found in Registry");
				}
			}
			else
			{
				logger.WriteLine("WebView client key not found in Registry");
			}

			// deprecate
			CleanupChromium();

			return false;
		}


		/// <summary>
		/// Temporary action to clean up the chromium folder under AppData\Roaming\OneMore.
		/// This method can be removed after a few release cycles.
		/// </summary>
		private void CleanupChromium()
		{
			var path = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), "OneMore");
			if (!Directory.Exists(path))
			{
				return;
			}

			var chrome = Directory.GetFiles(path, "chrome.exe", SearchOption.AllDirectories).FirstOrDefault();
			if (chrome == null)
			{
				return;
			}

			try
			{
				var parent = Path.GetDirectoryName(Path.GetDirectoryName(chrome));
				logger.WriteLine($"cleaning up chromium {parent}");

				Directory.Delete(parent, true);
			}
			catch (Exception exc)
			{
				logger.WriteLine("error cleaning up chromium");
				logger.WriteLine(exc);
			}
		}
	}
}
