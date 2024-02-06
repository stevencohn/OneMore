//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System.Windows.Forms;


	internal interface ILoadControl
	{
		Control.ControlCollection Controls { get; }

		void OnLoad();
	}
}
