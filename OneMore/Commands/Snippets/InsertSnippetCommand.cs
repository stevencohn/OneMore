//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;
	using Win = System.Windows;


	internal class InsertSnippetCommand : Command
	{

		public InsertSnippetCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var path = args[0] as string;

			var snippet = await new SnippetsProvider().Load(path);

			if (snippet == null)
			{
				UIHelper.ShowMessage(string.Format(Resx.InsertSnippets_CouldNotLoad, path));
				return;
			}

			await SingleThreaded.Invoke(() =>
			{
				Win.Clipboard.SetText(snippet, Win.TextDataFormat.Html);
			});

			// both SetText and SendWait are very unpredictable so wait a little
			await Task.Delay(200);

			SendKeys.SendWait("^(v)");
		}
	}
}
