//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class EditTableThemesCommand : Command
	{

		public EditTableThemesCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var dialog = new EditTableThemesDialog();
			dialog.ShowDialog(owner);

			await Task.Yield();
		}
	}
}
