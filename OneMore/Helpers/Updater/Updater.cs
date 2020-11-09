//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S1075 // URIs should not be hardcoded

namespace River.OneMoreAddIn.Helpers.Updater
{
	using Microsoft.Win32;
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Threading.Tasks;
	using System.Web.Script.Serialization;


	internal class Updater
	{
		private const string LatestUrl = "https://api.github.com/repos/stevencohn/onemore/releases/latest";

		private static HttpClient client;
		private GitRelease release;


		public string Tag => release.tag_name;

		public string Name => release.name;


		public async Task<bool> FetchLatestRelease()
		{
			if (client == null)
			{
				ServicePointManager.SecurityProtocol =
					SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

				client = new HttpClient();
				client.DefaultRequestHeaders.Add("User-Agent", "OneMore");
			}

			try
			{
				using (var response = await client.GetAsync(LatestUrl))
				{
					var body = await response.Content.ReadAsStringAsync();

					// use the .NET Framework serializer;
					// it's not great but I didn't want to pull in a nuget if I didn't need to
					var serializer = new JavaScriptSerializer();
					release = serializer.Deserialize<GitRelease>(body);
				}
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine("Error fetching latest release", exc);
				return false;
			}

			return true;
		}


		public bool IsUpToDate(string assemblyVersion)
		{
			var currentVersion = new Version(assemblyVersion);
			var releaseVersion = new Version(Tag);
			return currentVersion >= releaseVersion;
		}


		public async Task<bool> Update()
		{
			// presume the msi has one of these two keywords in its name
			var key = Environment.Is64BitProcess ? "x64" : "x86";

			var asset = release.assets.FirstOrDefault(a => a.browser_download_url.Contains(key));

			if (asset == null)
			{
				Logger.Current.WriteLine($"Did not find installer asset for {key}");
				return false;
			}

			var msi = Path.Combine(Path.GetTempPath(), asset.name);

			try
			{
				using (var response = await client.GetAsync(asset.browser_download_url))
				{
					using (var stream = await response.Content.ReadAsStreamAsync())
					{
						using (var file = File.OpenWrite(msi))
						{
							stream.CopyTo(file);
						}
					}
				}
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine(
					$"Error downloading latest installer from {asset.browser_download_url}", exc);

				return false;
			}

			// windows installer command line options
			// https://docs.microsoft.com/en-us/windows/win32/msi/command-line-options?redirectedfrom=MSDN

			// make installer script, which runs as a separate process so we have a chance
			// to terminate onenote before the msi runs

			var script = Path.Combine(Path.GetTempPath(), "OneMoreInstaller.bat");
			using (var writer = new StreamWriter(script, false))
			{
				writer.WriteLine("taskkill /im ONENOTE.exe");

				var code = GetProductCode();
				if (!string.IsNullOrEmpty(code))
				{
					writer.WriteLine("start /wait msiexec /x" + GetProductCode());
				}

				writer.WriteLine(msi);
			}

			// run installer script

			Process.Start(new ProcessStartInfo
			{
				FileName = script,
				UseShellExecute = true,
				WindowStyle = ProcessWindowStyle.Hidden
			});

			return true;
		}


		private static string GetProductCode()
		{
			string code = null;

			var path = Environment.Is64BitProcess
				? @"Software\Microsoft\Windows\CurrentVersion\Uninstall"
				: @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";

			var root = Registry.LocalMachine.OpenSubKey(path);
			if (root != null)
			{
				foreach (var subName in root.GetSubKeyNames())
				{
					var key = root.OpenSubKey(subName);
					if (key != null)
					{
						var name = key.GetValue("DisplayName") as string;
						if (name == "OneMoreAddIn")
						{
							var cmd = key.GetValue("UninstallString") as string;
							if (!string.IsNullOrEmpty(cmd))
							{
								code = cmd.Substring(cmd.IndexOf('{'));
								break;
							}
						}
					}
				}
			}

			return code;
		}
	}
}
