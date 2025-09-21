//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Windows.Forms;


	internal class SearchCloseEventArgs : EventArgs
	{

		public DialogResult DialogResult { get; }


		public SearchCloseEventArgs(DialogResult result)
		{
			DialogResult = result;
		}
	}
}
