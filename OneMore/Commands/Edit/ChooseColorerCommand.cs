//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;
	using System.Windows.Forms;


	internal class ChooseColorizerCommand : Command
	{
		public ChooseColorizerCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var dialog = new ColorizeDialog();
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				var key = dialog.LanguageKey;
				if (key != null)
				{
					await new ColorizeCommand().Execute(key);
				}
			}
		}
	}
}
