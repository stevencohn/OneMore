//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Starts and opens a visual timer. This small window will first appear in the upper right
	/// corner of the display and immediately starts counting seconds, minutes, and hours.
	/// </summary>
	/// <remarks>
	/// There are three controls on the window: copy the current value to the clipboard, 
	/// restart the timer from zero, and close the timer. The window can be moved but is limited 
	/// to the current desktop window.
	/// The current value shown in the timer window can easily be inserted into the body of the
	/// open page by using the Insert Timer command.
	/// </remarks>
	internal class TimerWindowCommand : Command
	{

		private static TimerWindow window;


		public TimerWindowCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			if (args.Length == 0)
			{
				if (window == null)
				{
					window = new TimerWindow();
					window.FormClosed += CloseTimerWindow;
					await window.RunModeless();
				}

				return;
			}

			if (window == null || window.IsDisposed)
			{
				return;
			}

			using var one = new OneNote(out var page, out var ns);
			var run = page.Root.Descendants(ns + "T")
				.FirstOrDefault(e => e.Attribute("selected")?.Value == "all");

			if (run != null)
			{
				var stamp = TimeSpan.FromSeconds(window.Seconds).ToString("c");
				run.GetCData().Value = stamp;
				run.Attribute("selected").Remove();

				run.AddAfterSelf(new XElement(ns + "T",
					run.Attributes(),
					new XAttribute("selected", "all"),
					new XCData(string.Empty))
					);

				await one.Update(page);
			}
		}


		private void CloseTimerWindow(object sender, System.Windows.Forms.FormClosedEventArgs e)
		{
			window.Dispose();
			window = null;
		}
	}
}
