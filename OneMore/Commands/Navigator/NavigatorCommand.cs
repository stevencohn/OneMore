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


		public NavigatorCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var guard = EnterOnce();
			if (guard is null) { return; }

			var settings = new SettingsProvider().GetCollection(nameof(NavigatorSheet));
			if (settings.Get("disabled", false))
			{
				// the tracking service is off but the reading list is independent of it,
				// so still let the user get to their pinned pages rather than blocking
				// the whole window
				using var provider = new NavigationProvider();
				var pinned = await provider.ReadPinned();
				if (pinned.Count == 0)
				{
					ShowInfo(Resx.NavigatorWindow_disabled);
					return;
				}
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


		private static void CloseNavigatorWindow(object sender, FormClosedEventArgs e)
		{
			window.Dispose();
			window = null;
		}

	}
}
