//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System.Windows.Forms;


	/// <summary>
	/// Reduce flickering
	/// </summary>
	internal class MoreFlowLayoutPanel : FlowLayoutPanel
	{
		public MoreFlowLayoutPanel()
		{
			DoubleBuffered = true;
		}
	}
}
