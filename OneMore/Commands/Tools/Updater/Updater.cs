//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S1075 // URIs should not be hardcoded

// uncomment this to enable interactive interception of the installer
//#define Interactive

namespace River.OneMoreAddIn.Commands.Tools.Updater
{
	using Microsoft.Win32;
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Reflection;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Web.Script.Serialization;


	internal class Updater : Loggable, IUpdateReport
	{
		private const string LatestUrl = "https://api.github.com/repos/stevencohn/onemore/releases";
		private const string Latest = "/latest";
		private const string LatestN = "?per_page=5";
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
			if (root is not null)
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
				logger.WriteLine($"updater: Registry key not found HKLM::{path}");
			}

			InstalledVersion = AssemblyInfo.Version;
			InstalledUrl = $"{TagUrl}/{InstalledVersion}";
		}


		public async Task<bool> FetchLatestRelease()
		{
			var client = HttpClientFactory.Create();
			var uri = $"{LatestUrl}{LatestN}";
			GitRelease[] releases;

			try
			{
				using var response = await client.GetAsync(uri);
				var body = await response.Content.ReadAsStringAsync();

				// use the .NET Framework serializer;
				// it's not great but I didn't want to pull in a nuget if I didn't need to
				var serializer = new JavaScriptSerializer();
				releases = serializer.Deserialize<GitRelease[]>(body);
			}
			catch (AggregateException exc)
			{
				logger.WriteLine("aggregate exception...");

				exc.Handle(e =>
				{
					// called for each exception in AggregateException...

					logger.WriteLine("error(s) fetching latest release", e);
					return true; // true=handled, don't rethrow
				});

				return false;
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error fetching latest release {exc.Message}", exc);
				return false;
			}

			// find latest released-release, allowing prereleases to be published
			release = null;
			foreach (var r in releases)
			{
				// dump them out for debugging
				var name = r.prerelease ? $"{r.name} PRERELEASE" : r.name;
				logger.WriteLine($"fetched {r.tag_name}, {r.published_at} - \"{name}\"");
				if (!r.prerelease && release is null)
				{
					release = r;
				}
			}

			// either no releases fetched or only prerelease versions!
			if (release is null)
			{
				logger.Write($"no official release found; ");
				if (releases.Length == 0)
				{
					logger.WriteLine("empty release list returned");
				}
				else
				{
					logger.WriteLine($"latest release is [{releases[0].tag_name}]");
				}

				return false;
			}

			// allow semantic version e.g. "v1.2.3" or suffixed e.g. "v1.2.3-beta"
			var plainver = release.tag_name;
			if (plainver is not null)
			{
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

			logger.WriteLine("updated fetched empty version, release object dump:");
			logger.Dump(release);

			return false;
		}


		// = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =

		public async Task<bool> Update()
		{
			if (string.IsNullOrEmpty(productCode))
			{
				logger.WriteLine("missing product code");
				return false;
			}

			// presume the msi has one of these two keywords in its name
			// NOTE that only the x64 installer is released as of Dec 2021 so this will
			// still fail if the user's computer is 32-bit. But seriously, who still has one?!
			var key = Environment.Is64BitOperatingSystem ? "x64" : "x86";

			var asset = release.assets.Find(a => a.browser_download_url.Contains(key));
			if (asset is null)
			{
				logger.WriteLine($"did not find installer asset for {key}");
				return false;
			}

			var msi = Path.Combine(Path.GetTempPath(), asset.name);

			try
			{
				logger.WriteLine($"downloading MSI from {asset.browser_download_url}");

				var client = HttpClientFactory.Create();
				using var response = await client.GetAsync(asset.browser_download_url);
				using var stream = await response.Content.ReadAsStreamAsync();
				using var file = File.OpenWrite(msi);
				await stream.CopyToAsync(file);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error downloading {asset.browser_download_url}", exc);
				return false;
			}

			// windows installer command line options
			// https://docs.microsoft.com/en-us/windows/win32/msi/command-line-options?redirectedfrom=MSDN

			// make installer script, which runs as a separate process so we have a chance
			// to terminate onenote before the msi runs

			var path = Path.Combine(Path.GetTempPath(), "OneMoreInstaller.cmd");
			logger.WriteLine($"creating install script {path}");

			var action = Path.Combine(
				Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
				"OneMoreSetupActions.exe");

			var shutdown = $"start /b \"\" \"{action}\" --uninstall-shutdown";

			using var writer = new StreamWriter(path, false);
			await writer.WriteLineAsync(shutdown);
			await writer.WriteLineAsync(msi);
#if Interactive
			writer.WriteLine("set /p \"continue: \""); // for debugging
#endif
			await writer.FlushAsync();
			writer.Close();

			// run installer script

			logger.WriteLine($"starting installation process");
			Process.Start(new ProcessStartInfo
			{
				FileName = Environment.ExpandEnvironmentVariables("%ComSpec%"),
				Arguments = $"/c {path}",
				UseShellExecute = true,
#if Interactive
				WindowStyle = ProcessWindowStyle.Normal
#else
				WindowStyle = ProcessWindowStyle.Hidden
#endif
			});

			return true;
		}
	}
}
