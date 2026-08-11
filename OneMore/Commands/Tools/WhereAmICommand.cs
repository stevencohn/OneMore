//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;
	using System.Windows.Forms;


	/// <summary>
	/// Displays a popup showing the hierarchy breadcrumb path of the current page. Each segment
	/// is a dropdown listing its siblings so the user can quickly jump to any ancestor notebook,
	/// section group, section, or page.
	/// </summary>
	internal class WhereAmICommand : Command
	{
		private static WhereAmIWindow window;
		private static bool commandIsActive = false;


		public WhereAmICommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			if (commandIsActive) { return; }
			commandIsActive = true;

			try
			{
				if (window is not null)
				{
					window.Elevate();
					return;
				}

				await using var one = new OneNote();

				var segments = await one.GetPageBreadcrumb();
				if (segments is null)
				{
					return;
				}

				var maxWidth = Screen.FromHandle(one.WindowHandle).WorkingArea.Width;

				window = new WhereAmIWindow(segments, maxWidth);
				window.FormClosed += WindowClosed;
				window.RunModeless();
			}
			finally
			{
				commandIsActive = false;
			}
		}


		private static void WindowClosed(object sender, FormClosedEventArgs e)
		{
			window.FormClosed -= WindowClosed;
			window.Dispose();
			window = null;
		}
	}
}
