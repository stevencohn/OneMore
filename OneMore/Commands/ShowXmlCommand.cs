//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{

	internal class ShowXmlCommand : Command
	{
		public ShowXmlCommand ()
		{
		}


		public override void Execute(params object[] args)
		{
			using (var dialog = new Dialogs.XmlDialog())
			{
				dialog.ShowDialog(owner);
			}
		}
	}
}
