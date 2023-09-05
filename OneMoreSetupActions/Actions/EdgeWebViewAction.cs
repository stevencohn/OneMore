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
	using System.Net.NetworkInformation;
	using System.Text.RegularExpressions;
	using System.Threading;


	/// <summary>
	/// Install or uninstall the Edge WebView2 control, primarily for Windows 10. Edge is
	/// the default browser in Windows 11 and includes WebView2 which cannot be uninstalled.
	/// </summary>
	internal class EdgeWebViewAction : CustomAction
	{
		// WebView2 distribution:
		// > https://docs.microsoft.com/en-us/microsoft-edge/webview2/concepts/distribution

		// this is the Evergreen Bootstrapper download link from this download page:
		// > https://developer.microsoft.com/en-us/microsoft-edge/webview2/#download-section
		private const string DownloadUrl = "https://go.microsoft.com/fwlink/p/?LinkId=2124703";

		private const string ClientKey = @"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients";
		private const string RuntimeId = "{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}";


		public EdgeWebViewAction(Logger logger, Stepper stepper)
			: base(logger, stepper)
		{
		}


		public override int Install()
		{
			logger.WriteLine();
			logger.WriteLine("EdgeWebViewAction.Install ---");

			var key = Registry.LocalMachine.OpenSubKey($"{ClientKey}\\{RuntimeId}");
			if (key != null)
			{
				logger.WriteLine("WebView2 Runtime is already installed");
				return SUCCESS;
			}

			var bootstrap = Path.Combine(
			Path.GetTempPath(),
			Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".exe");

			if (!IsNetworkAvailable())
			{
				logger.WriteLine("no internet connection; skipping WebView2 installation");
				return SUCCESS;
			}

			if (!DownloadBootstrap(bootstrap))
			{
				logger.WriteLine("unable to download WebView2 bootstrap installer");
				return FAILURE;
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

			return SUCCESS;
		}


		private static bool IsNetworkAvailable()
		{
			// only recognizes changes related to Internet adapters
			if (NetworkInterface.GetIsNetworkAvailable())
			{
				// however, this will include all adapters
				var interfaces = NetworkInterface.GetAllNetworkInterfaces();
				foreach (var face in interfaces)
				{
					// filter so we see only Internet adapters
					if (face.OperationalStatus == OperationalStatus.Up)
					{
						if ((face.NetworkInterfaceType != NetworkInterfaceType.Tunnel) &&
							(face.NetworkInterfaceType != NetworkInterfaceType.Loopback))
						{
							var statistics = face.GetIPv4Statistics();

							// all testing seems to prove that once an interface comes online
							// it has already accrued statistics for both received and sent...

							if ((statistics.BytesReceived > 0) &&
								(statistics.BytesSent > 0))
							{
								return true;
							}
						}
					}
				}
			}

			return false;
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


		public override int Uninstall()
		{
			logger.WriteLine();
			logger.WriteLine("EdgeWebViewAction.Uninstall ---");

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
						return SUCCESS;
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

			return FAILURE;
		}
	}
}
