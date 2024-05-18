//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Microsoft.Office.Core;
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Diagnostics;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal class SettingsCommand : Command
	{
		public SettingsCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			using var dialog = new SettingsDialog(args[0] as IRibbonUI);
			dialog.ShowDialog(owner);

			if (!dialog.RestartNeeded)
			{
				return;
			}

			var one = new OneNote();
			if (one.WindowCount > 1)
			{
				ShowInfo(Resx.SettingsDialog_closeWindows);
				return;
			}
			else if (UI.MoreMessageBox.ShowQuestion(owner,
				Resx.SettingsDialog_Restart) != DialogResult.Yes)
			{
				return;
			}

			var processes = Process.GetProcessesByName("ONENOTE");
			if (processes.Length == 0)
			{
				logger.WriteLine("cannot find ONENOTE process to restart");
				return;
			}

			var path = processes[0].MainModule.FileName;

			// the hidden cmd window will remain until OneNote is subsequently closed
			// not sure how to make the cmd.exe host process close immediately...

			var Info = new ProcessStartInfo
			{
				Arguments = $"/C taskkill /fi \"pid gt 0\" /im ONENOTE.exe && ping 127.0.0.1 -n 3 && \"{path}\"",
				WindowStyle = ProcessWindowStyle.Hidden,
				CreateNoWindow = true,
				FileName = "cmd.exe"
			};

			Process.Start(Info);

			await Task.Yield();
		}
	}
}
