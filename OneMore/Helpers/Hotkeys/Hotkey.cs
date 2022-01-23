//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Threading;
	using System.Windows.Forms;


	/// <summary>
	/// Details a registered hotkey.
	/// </summary>
	internal class Hotkey
	{
		private static int counter = 0xE000;


		/// <summary>
		/// Used during hot key registration, translate a Keys longword with both subject key
		/// and modifiers to a new Hotkey
		/// </summary>
		/// <param name="keys">A Forms.Keys with both lower-order bits specifying the subject key
		/// and high-order bits specifying the modifier keys</param>
		public Hotkey(Keys keys)
		{
			Id = Interlocked.Increment(ref counter);
			SetKeys(keys & Keys.KeyCode, keys & Keys.Modifiers);
		}


		/// <summary>
		/// Initialize a new instance by copying the given instance
		/// </summary>
		/// <param name="original">A Hotkey to copy</param>
		public Hotkey(Hotkey original)
		{
			Key = original.Key;
			HotModifiers = original.HotModifiers;
		}


		/// <summary>
		/// Gets or sets the action for this hot key
		/// </summary>
		public Action Action { get; set; }


		/// <summary>
		/// Gets or sets the hot modifiers bits of this hot key
		/// </summary>
		public uint HotModifiers { get; set; }


		/// <summary>
		/// Gets the unique ID of this hot key
		/// </summary>
		public int Id { get; private set; }


		/// <summary>
		/// Gets or sets the subject key of this hot key
		/// Equivalent to Forms.Keys
		/// </summary>
		public uint Key { get; set; }


		/// <summary>
		/// Gets the subject keys as a Forms.Keys
		/// </summary>
		public Keys Keys => (Keys)Key;


		/// <summary>
		/// Gets the modifiers as a Forms.Keys
		/// </summary>
		public Keys Modifiers
		{
			get
			{
				if (HotModifiers == (uint)HotModifier.None)
				{
					return Keys.None;
				}

				var keys = Keys.None;

				if ((HotModifiers & (uint)HotModifier.Control) > 0) keys |= Keys.Control;
				if ((HotModifiers & (uint)HotModifier.Shift) > 0) keys |= Keys.Shift;
				if ((HotModifiers & (uint)HotModifier.Alt) > 0) keys |= Keys.Alt;
				if ((HotModifiers & (uint)HotModifier.Windows) > 0) keys |= Keys.LWin;

				return keys;
			}
		}


		/// <summary>
		/// Compares this instance with a given instance
		/// </summary>
		/// <param name="obj">Another Hotkey to compare</param>
		/// <returns>True if both the subject and modifiers are the same as this instance</returns>
		public override bool Equals(object obj)
		{
			if (obj is Hotkey other)
			{
				return other.Key == Key && other.HotModifiers == HotModifiers;
			}

			return false;
		}


		/// <summary>
		/// Gets the unique hash of this instance
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}


		/// <summary>
		/// Set the key bindings for this instance
		/// </summary>
		/// <param name="keys">The subject key</param>
		/// <param name="modifiers">The modifiers</param>
		public void SetKeys(Keys keys, Keys modifiers)
		{
			Key = (uint)keys;
			HotModifiers = 0;

			if ((modifiers & (Keys.Control | Keys.LControlKey | Keys.RControlKey)) > 0)
			{
				HotModifiers += (uint)HotModifier.Control;
			}

			if ((modifiers & (Keys.Shift | Keys.LShiftKey | Keys.RShiftKey)) > 0)
			{
				HotModifiers += (uint)HotModifier.Shift;
			}

			if ((modifiers & (Keys.Alt | Keys.RMenu | Keys.LMenu)) > 0)
			{
				HotModifiers += (uint)HotModifier.Alt;
			}

			if ((modifiers & (Keys.LWin | Keys.RWin)) > 0)
			{
				HotModifiers += (uint)HotModifier.Windows;
			}
		}


		/// <summary>
		/// Makes a pretty user-facing description of the full key binding
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			if (Keys == Keys.None || Keys == Keys.Back)
			{
				return string.Empty;
			}

			var sequence = string.Empty;
			var mods = (HotModifier)HotModifiers;
			if (mods.HasFlag(HotModifier.Control)) sequence = $"{sequence}Ctrl+";
			if (mods.HasFlag(HotModifier.Shift)) sequence = $"{sequence}Shift+";
			if (mods.HasFlag(HotModifier.Alt)) sequence = $"{sequence}Alt+";
			return $"{sequence}{(Keys)Key}";
		}
	}
}
