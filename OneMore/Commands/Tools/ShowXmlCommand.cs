//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;


	/// <summary>
	/// Display the XML representation of the current page, section, and notebook.
	/// </summary>
	internal class ShowXmlCommand : Command
	{
		public ShowXmlCommand ()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			using (var dialog = new ShowXmlDialog())
			{
				dialog.ShowDialog();
			}

			await Task.Yield();
		}
	}
}
