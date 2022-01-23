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
		public HotModifier HotModifiers { get; private set; }


		/// <summary>
		/// A bit mask combining Key and Modifiers where Modifiers is the low-orde word
		/// and Key is the high-order word
		/// </summary>
		public uint Value { get; private set; }


		/// <summary>
		/// Initialize a new event from the given WM_HOTKEY msg.LParam value; this value is
		/// a longword where the low-order word contains the modifiers and the high-order
		/// word contains the Keys. The modifiers bitmask are not equivalent to the same
		/// Keys modifiers - Control, Shift, Alt - so must be remapped if needed
		/// </summary>
		/// <param name="hotKeyParam"></param>
		public HotkeyEventArgs(IntPtr hotKeyParam)
		{
			Value = (uint)hotKeyParam.ToInt64();
			Key = (Keys)((Value & 0xffff0000) >> 16);
			HotModifiers = (HotModifier)(Value & 0x0000ffff);
		}
	}
}
