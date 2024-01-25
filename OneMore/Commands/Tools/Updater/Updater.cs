﻿//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S1075 // URIs should not be hardcoded
#define xDebugUpdater

namespace River.OneMoreAddIn.Commands.Tools.Updater
{
	using Microsoft.Win32;
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Web.Script.Serialization;


	internal class Updater : IUpdateReport
	{
		private const string LatestUrl = "https://api.github.com/repos/stevencohn/onemore/releases/latest";
		private const string TagUrl = "https://github.com/stevencohn/OneMore/releases/tag";

		private GitRelease release;
		private readonly string productCode;


		public bool IsUpToDate { get; private set; }
		public string InstalledDate { get; private set; }
		public string InstalledUrl { get; private set; }
		public string InstalledVersion { get; private set; }

		public string UpdateDate => release.published_at;
		public string UpdateDescription => release.name;
		public string UpdateUrl => release.html_url;
		public string UpdateVersion => release.tag_name;


		public Updater()
		{
			// get current installed info...

			//var path = Environment.Is64BitProcess
			//	? @"Software\Microsoft\Windows\CurrentVersion\Uninstall"
			//	: @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";

			// since we only deploy a 64-bit installer now...
			var path = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";

			using var hive = RegistryKey
				.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

			using var root = hive.OpenSubKey(path);
			if (root != null)
			{
				foreach (var subName in root.GetSubKeyNames())
				{
					using var key = root.OpenSubKey(subName);

					if (key?.GetValue("DisplayName") is string name &&
						name == "OneMoreAddIn")
					{
						if (key.GetValue("UninstallString") is string cmd &&
							!string.IsNullOrEmpty(cmd))
						{
							productCode = cmd.Substring(cmd.IndexOf('{'));
						}

						if (key.GetValue("InstallDate") is string indate)
						{
							InstalledDate = indate;
						}

						// found the OneMore key so our job is done here
						break;
					}
				}
			}
			else
			{
				Logger.Current.WriteLine($"updater: Registry key not found HKLM::{path}");
			}

			InstalledVersion = AssemblyInfo.Version;
			InstalledUrl = $"{TagUrl}/{InstalledVersion}";
		}


		public async Task<bool> FetchLatestRelease()
		{
			var client = HttpClientFactory.Create();

			try
			{
				using var response = await client.GetAsync(LatestUrl);
				var body = await response.Content.ReadAsStringAsync();

				// use the .NET Framework serializer;
				// it's not great but I didn't want to pull in a nuget if I didn't need to
				var serializer = new JavaScriptSerializer();
				release = serializer.Deserialize<GitRelease>(body);
			}
			catch (AggregateException exc)
			{
				Logger.Current.WriteLine("aggregate exception...");

				exc.Handle(e =>
				{
					// called for each exception in AggregateException...

					Logger.Current.WriteLine("error(s) fetching latest release", e);
					return true; // true=handled, don't rethrow
				});

				return false;
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine($"error fetching latest release {exc.Message}", exc);
				return false;
			}

			// allow semantic version e.g. "v1.2.3" or suffixed e.g. "v1.2.3-beta"
			var plainver = release.tag_name;
			var match = Regex.Match(plainver, @"\d+\.\d+(?:\.\d+)?");
			if (match.Success && match.Captures[0].Value.Length < plainver.Length)
			{
				plainver = match.Captures[0].Value;
			}

			var currentVersion = new Version(InstalledVersion);
			var releaseVersion = new Version(plainver);
			IsUpToDate = currentVersion >= releaseVersion;

			return true;
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		public async Task<bool> Update()
		{
			if (string.IsNullOrEmpty(productCode))
			{
				Logger.Current.WriteLine("missing product code");
				return false;
			}

			// presume the msi has one of these two keywords in its name
			// NOTE that only the x64 installer is released as of Dec 2021 so this will
			// still fail if the user's computer is 32-bit. But seriously, who still has one?!
			var key = Environment.Is64BitOperatingSystem ? "x64" : "x86";

			var asset = release.assets.Find(a => a.browser_download_url.Contains(key));
			if (asset == null)
			{
				Logger.Current.WriteLine($"did not find installer asset for {key}");
				return false;
			}

			var msi = Path.Combine(Path.GetTempPath(), asset.name);

			try
			{
				Logger.Current.WriteLine($"downloading MSI from {asset.browser_download_url}");

				var client = HttpClientFactory.Create();
				using var response = await client.GetAsync(asset.browser_download_url);
				using var stream = await response.Content.ReadAsStreamAsync();
				using var file = File.OpenWrite(msi);
				stream.CopyTo(file);
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine($"error downloading {asset.browser_download_url}", exc);
				return false;
			}

			// windows installer command line options
			// https://docs.microsoft.com/en-us/windows/win32/msi/command-line-options?redirectedfrom=MSDN

			// make installer script, which runs as a separate process so we have a chance
			// to terminate onenote before the msi runs

			var path = Path.Combine(Path.GetTempPath(), "OneMoreInstaller.cmd");
			Logger.Current.WriteLine($"creating install script {path}");

			var action = Path.Combine(
				Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
				"OneMoreSetupActions.exe");

			var shutdown = $"start /b \"\" \"{action}\" --uninstall-shutdown";

			using var writer = new StreamWriter(path, false);
			writer.WriteLine(shutdown);
			writer.WriteLine(msi);
#if DebugUpdater
			writer.WriteLine("set /p \"continue: \""); // for debugging
#endif
			writer.Flush();
			writer.Close();

			// run installer script

			Logger.Current.WriteLine($"starting installation process");
			Process.Start(new ProcessStartInfo
			{
				FileName = Environment.ExpandEnvironmentVariables("%ComSpec%"),
				Arguments = $"/c {path}",
				UseShellExecute = true,
#if DebugUpdater
				WindowStyle = ProcessWindowStyle.Normal
#else
				WindowStyle = ProcessWindowStyle.Hidden
#endif
			});

			return true;
		}
	}
}
