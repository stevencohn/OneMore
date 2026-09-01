//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System.Windows.Forms;


	/// <summary>
	/// A plain Panel with double buffering enabled, for panels that are repositioned or
	/// resized directly by code (rather than through Dock/Anchor) and would otherwise show
	/// visible repaint artifacts on their children while that happens.
	/// </summary>
	internal class MoreBufferedPanel : Panel
	{
		public MoreBufferedPanel()
		{
			DoubleBuffered = true;
		}
	}
}
