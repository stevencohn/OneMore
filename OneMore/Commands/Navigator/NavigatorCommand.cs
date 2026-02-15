//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Opens the Navigator window
	/// </summary>
	internal class NavigatorCommand : Command
	{
		private static NavigatorWindow window;
		private static bool commandIsActive = false;


		public NavigatorCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			if (commandIsActive) { return; }
			commandIsActive = true;

			try
			{
				var settings = new SettingsProvider().GetCollection(nameof(NavigatorSheet));
				if (settings.Get("disabled", false))
				{
					ShowInfo(Resx.NavigatorWindow_disabled);
					return;
				}

				if (window == null)
				{
					window = new NavigatorWindow();
					window.FormClosed += CloseNavigatorWindow;
					window.RunModeless();
					return;
				}

				if (window.IsDisposed)
				{
					return;
				}

				if (window.WindowState == FormWindowState.Minimized)
				{
					window.WindowState = FormWindowState.Normal;
				}

				await window.RefreshPageHeadings();
				window.Elevate(false);

				await Task.Yield();
			}
			finally
			{
				commandIsActive = false;
			}
		}


		private static void CloseNavigatorWindow(object sender, FormClosedEventArgs e)
		{
			window.Dispose();
			window = null;
		}

	}
}
