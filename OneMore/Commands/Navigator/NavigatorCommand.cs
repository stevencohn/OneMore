//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;
	using System.Windows.Forms;


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
			if (window == null)
			{
				window = new NavigatorWindow();
				window.FormClosed += CloseNavigatorWindow;
				await window.RunModeless();
			}

			if (window == null || window.IsDisposed)
			{
				return;
			}

			window.ForceTopMost();
		}


		private void CloseNavigatorWindow(object sender, FormClosedEventArgs e)
		{
			window.Dispose();
			window = null;
		}

	}
}
