//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Threading.Tasks;


	/// <summary>
	/// Display the XML representation of the current page, section, and notebook.
	/// </summary>
	internal class ShowXmlCommand : Command
	{
		private static bool commandIsActive = false;


		public ShowXmlCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			if (commandIsActive) { return; }
			commandIsActive = true;

			try
			{
				using var dialog = new ShowXmlDialog();
				dialog.ShowDialog(owner);

				await Task.Yield();
			}
			finally
			{
				commandIsActive = false;
			}
		}
	}
}
