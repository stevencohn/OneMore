//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Windows.Forms;


	/// <summary>
	/// Packet sent to hotkey event handlers
	/// </summary>
	internal class HotkeyEventArgs : EventArgs
	{

		/// <summary>
		/// The code of the primary key
		/// </summary>
		public Keys Key { get; private set; }


		/// <summary>
		/// Key modifiers: ctrl, shift, alt
		/// </summary>
		public Hotmods Modifiers { get; private set; }


		/// <summary>
		/// A bit mask combining Key and Modifiers where Modifiers is the low-orde word
		/// and Key is the high-order word
		/// </summary>
		public uint Value { get; private set; }


		public HotkeyEventArgs(IntPtr hotKeyParam)
		{
			Value = (uint)hotKeyParam.ToInt64();
			Key = (Keys)((Value & 0xffff0000) >> 16);
			Modifiers = (Hotmods)(Value & 0x0000ffff);
		}
	}
}
