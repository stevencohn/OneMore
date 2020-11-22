//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class TaggingCommand : Command
	{

		public TaggingCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			using (var dialog = new TaggingDialog())
			{
				dialog.ShowDialog(owner);
			}
		}
	}
}
