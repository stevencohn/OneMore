//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;
	using System.Windows.Forms;


	internal class ChooseColorizerCommand : Command
	{
		private ColorizeDialog dialog;
		private string key;


		public ChooseColorizerCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			if (dialog != null)
			{
				// single instance
				dialog.Elevate();
				return;
			}

			dialog = new ColorizeDialog();
			dialog.FormClosed += Dialog_FormClosed;

			if (dialog.ShowDialog(owner) == DialogResult.OK)
			{
				if (key != null)
				{
					await factory.Run<ColorizeCommand>(key);
				}
			}
		}


		private void Dialog_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (dialog != null)
			{
				key = dialog.LanguageKey;
				dialog.FormClosed -= Dialog_FormClosed;
				dialog.Dispose();
				dialog = null;
			}
		}
	}
}
