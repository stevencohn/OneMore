//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

using System;

namespace River.OneMoreAddIn
{

	/// <summary>
	/// Details a registered hotkey.
	/// </summary>
	internal class Hotkey
	{
		public int Id;
		public uint Key;
		public uint Modifiers;
		public Action Action;
	}
}
