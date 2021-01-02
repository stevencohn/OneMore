//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;
	using System.Windows.Forms;
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
				UIHelper.ShowMessage($"Could not load from {path}"); // translate
				return;
			}

			await SingleThreaded.Invoke(() =>
			{
				Win.Clipboard.Clear();
				Win.Clipboard.SetText(snippet, Win.TextDataFormat.Html);
			});

			SendKeys.SendWait("^(v)");
		}
	}
}
