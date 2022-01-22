//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Windows.Forms;


	/// <summary>
	/// Details a registered hotkey.
	/// </summary>
	internal class Hotkey
	{
		public int Id;
		public uint Key;		// Keys
		public uint Modifiers;  // Hotmods
		public Action Action;


		public Hotkey()
		{
		}


		public Hotkey(Keys keys)
			: this(keys & Keys.KeyCode, keys & Keys.Modifiers)
		{
		}


		public Hotkey(Keys key, Hotmods modifiers)
		{
			Key = (uint)key;
			Modifiers = (uint)modifiers;
		}


		public Hotkey(Keys key, Keys modifiers)
		{
			Key = (uint)key;
			Modifiers = 0;
			
			if (modifiers.HasFlag(Keys.Control) ||
				modifiers.HasFlag(Keys.LControlKey) ||
				modifiers.HasFlag(Keys.RControlKey))
			{
				Modifiers += (uint)Hotmods.Control;
			}

			if (modifiers.HasFlag(Keys.Shift) ||
				modifiers.HasFlag(Keys.LShiftKey) ||
				modifiers.HasFlag(Keys.RShiftKey))
			{
				Modifiers += (uint)Hotmods.Shift;
			}

			if (modifiers.HasFlag(Keys.Alt) ||
				modifiers.HasFlag(Keys.RMenu) ||
				modifiers.HasFlag(Keys.LMenu))
			{
				Modifiers += (uint)Hotmods.Alt;
			}

			if (modifiers.HasFlag(Keys.LWin) ||
				modifiers.HasFlag(Keys.RWin))
			{
				Modifiers += (uint)Hotmods.Windows;
			}
		}


		public Keys Keys => (Keys)Key;


		public Keys KeyMods
		{
			get
			{
				var keys = Keys.None;

				if ((Modifiers & (uint)Hotmods.Control) > 0)
				{
					keys |= Keys.Control;
				}

				if ((Modifiers & (uint)Hotmods.Shift) > 0)
				{
					keys |= Keys.Shift;
				}

				if ((Modifiers & (uint)Hotmods.Alt) > 0)
				{
					keys |= Keys.Alt;
				}

				if ((Modifiers & (uint)Hotmods.Windows) > 0)
				{
					keys |= Keys.LWin;
				}

				return keys;
			}
		}


		public override string ToString()
		{
			var sequence = string.Empty;

			if ((Keys)Key != Keys.Back)
			{
				if ((Modifiers & (uint)Hotmods.Control) > 0) sequence = $"{sequence}Ctrl+";
				if ((Modifiers & (uint)Hotmods.Shift) > 0) sequence = $"{sequence}Shift+";
				if ((Modifiers & (uint)Hotmods.Alt) > 0) sequence = $"{sequence}Alt+";
				sequence = $"{sequence}{(Keys)Key}";
			}

			return sequence;
		}
	}
}
