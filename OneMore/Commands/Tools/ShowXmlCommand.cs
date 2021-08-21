//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;


	internal class ShowXmlCommand : Command
	{
		public ShowXmlCommand ()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var dialog = new ShowXmlDialog())
			{
				dialog.ShowDialog(owner);
			}

			await Task.Yield();
		}
	}
}
