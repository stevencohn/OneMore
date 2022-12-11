//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Text;
	using Resx = Properties.Resources;


	internal interface IEmoji
	{
		public string Symbol { get; }
		public string Name { get; }
		public string Color { get; }

	}


	internal sealed class Emoji : IEmoji, IDisposable
	{
		public Emoji(string symbol, string name, Image image, string color = null)
		{
			Symbol = symbol;
			Name = name;
			Image = image;
			Color = color;
		}

		#region Lifecycle
		private bool disposedValue;

		private void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					Image?.Dispose();
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
		#endregion Lifecycle

		public string Symbol { get; set; }
		public string Name { get; set; }
		public Image Image { get; set; }
		public string Color { get; set; }
	}


	internal class Emojis : IDisposable
	{

		private readonly Emoji[] map =
		{
			new Emoji("🚩", Resx.Emoji_Important, Resx.Flag),					// 1F6A9 Triangular Flag On Post
			new Emoji("📐", Resx.Emoji_Architecture, Resx.Architecture),		// 1F4D0 Triangular Ruler
			new Emoji("🚗", Resx.Emoji_Automobile, Resx.Automobile),			// 1F697 Automobile
			new Emoji("●", Resx.Emoji_Bullet, Resx.Bullet, "#0070C0"),			// 25CF  Black Circle
			new Emoji("📆", Resx.Emoji_Calendar, Resx.Calendar),				// 1F4C6 Tear-Off Calendar
			new Emoji("✓", Resx.Emoji_CheckMark, Resx.CheckMark, "#70AD47"),	// 2713  Check Mark
			new Emoji("⭕", Resx.Emoji_Circle, Resx.Circle),						// 2B55  Heavy Large Circle
			new Emoji("☁", Resx.Emoji_Cloud, Resx.Cloud),						// 2601  Cloud
			new Emoji("©", Resx.Emoji_Copyright, Resx.Copyright),				// 00A9  Copyright
			new Emoji("❌", Resx.Emoji_CrossMark, Resx.Cross),					// 274C  Cross Mark
			new Emoji("🚴", Resx.Emoji_Cycling, Resx.Cyclist),					// 1F684 Bicyclist
			new Emoji("✉", Resx.Emoji_Email, Resx.Envelope),					// 2709  Envelope
			new Emoji("👀", Resx.Emoji_Eyes, Resx.Eyes),						// 1F440 Eyes
			new Emoji("👪", Resx.Emoji_Family, Resx.Family),					// 1F46A Family
			new Emoji("💲", Resx.Emoji_Financial, Resx.Financial),				// 1F4B2 Heavy Dollar Sign
			new Emoji("🔨", Resx.Emoji_Hammer, Resx.Hammer),					// 1F528 Hammer
			new Emoji("📷", Resx.Emoji_Images, Resx.Camera),					// 1F4F7 Camera
			new Emoji("📓", Resx.Emoji_Journal, Resx.Journal),					// 1F4D3 Notebook
			new Emoji("←", Resx.Emoji_LeftwardsArrow, Resx.LeftwardsArrow, "#0070C0"),		// 2190  Leftwards Arrow
			new Emoji("📝", Resx.Emoji_Memo, Resx.Memo),						// 1F4DD Memo
			new Emoji("🔑", Resx.Emoji_Passwords, Resx.Passwords),				// 1F511 Key
			new Emoji("✏", Resx.Emoji_Pencil, Resx.Pencil),						// 270F  Pencil
			new Emoji("📌", Resx.Emoji_Pushpin, Resx.Pushpin),					// 1F4CC Pushpin
			new Emoji("→", Resx.Emoji_RightwardsArrow, Resx.RightwardsArrow, "#0070C0"),	// 2192  Righwards Arrow
			new Emoji("§", Resx.Emoji_Section, Resx.Section),					// 00A7  Section
			new Emoji("⚡", Resx.Emoji_Shazam, Resx.Shazam),					// 26A1  Lightning
			new Emoji("🙂", Resx.Emoji_Smiley, Resx.Smiley),					// 1F642 Smiley
			new Emoji("💾", Resx.Emoji_Software, Resx.Software),				// 1F4BE Floppy Disk
			new Emoji("⚾", Resx.Emoji_Sports, Resx.Sports),					// 26BE  Baseball
			new Emoji("⭐", Resx.Emoji_Star, Resx.Star),							// 2B50  White Medium Star
			new Emoji("∑", Resx.Emoji_Summary, Resx.Summary),					// 2211  N-Ary Summation
			new Emoji("☑", Resx.Emoji_Tasks, Resx.Tasks),						// 2611  Ballot Box
			new Emoji("🛩", Resx.Emoji_Travel, Resx.Travel),					// 1F6E9 Airplane
			new Emoji("⌚", Resx.Emoji_Watch, Resx.Watch),						// 231A  Watch
			new Emoji("✗", Resx.Emoji_XMark, Resx.XMark, "#E84C22")				// 2717  X Mark
		};


		#region Lifecycle
		private bool disposedValue;


		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					foreach (var entry in map)
					{
						entry.Dispose();
					}
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
		#endregion Lifecycle


		/// <summary>
		/// Gets an array of emoji names
		/// </summary>
		/// <returns>An array of strings</returns>
		public string[] GetNames() => map.Select(e => e.Name).ToArray();


		/// <summary>
		/// Parses a comma-delimeted string of emoji symbols and returns a collection of the
		/// corresponding Emoji
		/// </summary>
		/// <param name="symbols">A string of emoji symbols delimeted by commas</param>
		/// <returns>A collection of IEmoji</returns>
		public IEnumerable<IEmoji> ParseSymbols(string symbols)
		{
			var parts = symbols.Split(',');
			return map.Where(e => parts.Contains(e.Symbol)).Select(e => e);
		}


		/// <summary>
		/// Gets the indexed emoji
		/// </summary>
		/// <param name="index">The index of the emoji</param>
		/// <returns>An Emoji instance describing the emoji</returns>
		public Emoji this[int index] => map[index];


		/// <summary>
		/// Removes emojis from the given string. Used by various commands to "clean" their
		/// page titles before further modifications.
		/// </summary>
		/// <param name="value">A string to modify</param>
		/// <returns>A new string without emojis</returns>
		public static string RemoveEmojis(string value)
		{
			// these lists are aut-generated from my own emojis.linq...

			// single-char emojis
			var singles = new List<char> {
				(char)0x00A7, // § section
				(char)0x00A9, // © copyright
				(char)0x2190, // ← left arrow
				(char)0x2192, // → right arrow
				(char)0x2211, // ∑ summarize
				(char)0x231A, // ⌚ watch
				(char)0x25CF, // ● bullet
				(char)0x2601, // ☁ cloud
				(char)0x2611, // ☑ ballot check
				(char)0x26A1, // ⚡shazam
				(char)0x26BE, // ⚾ sports
				(char)0x2709, // ✉ camera
				(char)0x270F, // ✏ pencil
				(char)0x2713, // ✓ checkmark
				(char)0x2717, // ✗ xmark
				(char)0x274C, // ❌ cross
				(char)0x2B50, // ⭐ star
				(char)0x2B55  // ⭕ circle
			};

			// double-char emojis, all have first char 0xD83D
			var doubleMarker = (char)0xD83D;

			var doubles = new List<char> {
				(char)0xDC40, (char)0xDC6A, (char)0xDCB2, (char)0xDCBE, (char)0xDCC6, (char)0xDCCC,
				(char)0xDCD0, (char)0xDCD3, (char)0xDCDD, (char)0xDCF7, (char)0xDD11, (char)0xDD28,
				(char)0xDE42, (char)0xDE97, (char)0xDEA9, (char)0xDEB4, (char)0xDEE9
			};

			var builder = new StringBuilder(value);
			int i;

			foreach (var c in singles)
			{
				if ((i = builder.IndexOf(c)) >= 0)
				{
					builder.Remove(i, 1);
				}
			}

			while (((i = builder.IndexOf(doubleMarker)) >= 0) && (i < builder.Length - 1))
			{
				if (doubles.Contains(builder[i + 1]))
				{
					builder.Remove(i, 2);
				}
			}

			return builder.ToString();
		}
	}
}
