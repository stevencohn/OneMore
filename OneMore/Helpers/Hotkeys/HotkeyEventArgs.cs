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
		public Keys Key { get; private set; }

		public Hotmods Modifiers { get; private set; }

		public uint Value { get; private set; }


		public HotkeyEventArgs(Keys key, Hotmods modifiers)
		{
			Key = key;
			Modifiers = modifiers;
		}


		public HotkeyEventArgs(IntPtr hotKeyParam)
		{
			Value = (uint)hotKeyParam.ToInt64();
			Key = (Keys)((Value & 0xffff0000) >> 16);
			Modifiers = (Hotmods)(Value & 0x0000ffff);
		}
	}
}
