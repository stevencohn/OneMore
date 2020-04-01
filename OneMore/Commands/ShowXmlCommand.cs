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


		public void Execute ()
		{
			logger.WriteLine("ShowXmlCommand.Execute()");

			using (var dialog = new XmlDialog())
			{
				dialog.ShowDialog(owner);
			}
		}
	}
}
