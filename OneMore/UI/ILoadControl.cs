//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System.Windows.Forms;


	/// <summary>
	/// Provides an entry point to an OnLoad method for simple controls that do not support
	/// OnLoad out of the box, such as Button and Label. This enables top level containers,
	/// MoreForm and SheetBase to ask controls to OnLoad themselves: maintained encapsulation.
	/// </summary>
	internal interface ILoadControl
	{
		/// <summary>
		/// All controls have a Controls collection. This just lets the container logic
		/// recursively navigate the control hierarchy.
		/// </summary>
		Control.ControlCollection Controls { get; }


		/// <summary>
		/// Custom OnLoad method, can be declared exactly as:
		/// 
		///		void ILoadControl.OnLoad() { }
		///	
		/// making it easy to spot and differentiate from the built-in OnLoad handler
		/// </summary>
		void OnLoad();
	}
}
