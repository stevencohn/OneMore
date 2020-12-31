//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Xml.Linq;


	internal class SaveSnippetCommand : Command
	{

		public SaveSnippetCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote())
			{
				// since the Hotkey message loop is watching all input, explicitly setting
				// focus on the OneNote main window provides a direct path for SendKeys
				Native.SetForegroundWindow(one.WindowHandle);
				System.Windows.Forms.SendKeys.SendWait("^(c)");
			}

			var html = await SingleThreaded.Invoke(() =>
			{
				return Clipboard.GetText(TextDataFormat.Html);
			});

			logger.WriteLine(html);
		}
	}
}
