﻿//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;
	using System.Windows.Forms;


	internal class ChooseColorizerCommand : Command
	{
		private ColorizeDialog dialog;


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
				var key = dialog.LanguageKey;
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
				dialog.FormClosed -= Dialog_FormClosed;
				dialog.Dispose();
				dialog = null;
			}
		}
	}
}
