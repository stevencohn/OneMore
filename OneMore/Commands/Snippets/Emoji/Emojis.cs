//************************************************************************************************
// Copyright © 2016 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Newtonsoft.Json;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Text;
	using Resx = Properties.Resources;


	internal interface IEmoji : IDisposable
	{
		public string Glyph { get; }
		public string Color { get; }

	}


	internal sealed class Emoji : IEmoji
	{
		/// <summary>
		/// Constructs an Emoji, either a curated entry (with a resID identifying its
		/// localized name) or a bare glyph scanned from the full Unicode range (resID
		/// null, no Name).
		/// </summary>
		public Emoji(string glyph, string resID, string color = null)
		{
			Glyph = glyph;
			ResID = resID;
			Color = color;

			if (resID is not null)
			{
				// strip e_ prefix
				var key = ResID.Substring(2);

				Name = Resx.ResourceManager.GetString($"Emoji_{key}", AddIn.Culture);
				if (string.IsNullOrWhiteSpace(Name))
				{
					Name = $"RESX...Emoji_{resID}";
				}
			}

			// glyph must be exactly one codepoint's worth of UTF-16 units (either a
			// single char or a single surrogate pair) for ConvertToUtf32 to be valid;
			// multi-codepoint sequences (none exist in the curated list today, but
			// don't assume that always holds) are left uncategorized
			Category = glyph.Length == 1 || (glyph.Length == 2 && char.IsSurrogatePair(glyph, 0))
				? EmojiCategories.GetCategory(char.ConvertToUtf32(glyph, 0))
				: null;
		}

		#region Lifecycle
		private bool disposedValue;

		private void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					selectedImage?.Dispose();
					unselectedImage?.Dispose();
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

		public string Glyph { get; set; }
		public string ResID { get; set; }
		public string Name { get; set; }
		public string Color { get; set; }

		/// <summary>
		/// Gets the Unicode top-level emoji group name (e.g. "Animals &amp; Nature"),
		/// or null if this glyph isn't part of Unicode's emoji category data.
		/// </summary>
		public string Category { get; }

		private Bitmap selectedImage;
		private Bitmap unselectedImage;


		/// <summary>
		/// Gets a lazily rendered, cached color bitmap of this emoji's glyph for the given
		/// selection state, rendered via DirectWrite/Direct2D since GDI+ cannot render the
		/// color layers of a color font.
		/// </summary>
		/// <param name="selected">True to render against the selected-row background</param>
		/// <param name="sizePx">The width and height in pixels of the icon</param>
		/// <param name="background">The background color behind the icon for this state</param>
		/// <param name="fallbackColor">The color to use if the glyph has no color layer</param>
		/// <returns>A cached Bitmap; do not dispose, owned by this Emoji</returns>

		public Bitmap GetImage(bool selected, int sizePx, Color background, Color fallbackColor)
		{
			if (selected)
			{
				return selectedImage ??=
					ColorGlyphRenderer.Instance.RenderGlyph(Glyph, sizePx, background, fallbackColor);
			}

			return unselectedImage ??=
				ColorGlyphRenderer.Instance.RenderGlyph(Glyph, sizePx, background, fallbackColor);
		}
	}


	/// <summary>
	/// Looks up the Unicode top-level emoji group name for a single codepoint, derived
	/// from the Unicode Consortium's emoji-test.txt (https://unicode.org/Public/emoji/latest/emoji-test.txt),
	/// since neither the Segoe UI Emoji font nor any Windows API exposes this.
	/// </summary>
	internal static class EmojiCategories
	{
		private static IReadOnlyDictionary<int, string> map;


		/// <summary>
		/// Gets the category name for the given codepoint, or null if it isn't part of
		/// the Unicode emoji category data.
		/// </summary>
		/// <param name="codepoint">The Unicode codepoint to look up</param>
		/// <returns>A category name, or null</returns>
		public static string GetCategory(int codepoint)
		{
			return (map ??= Load()).TryGetValue(codepoint, out var name) ? name : null;
		}


		private static IReadOnlyDictionary<int, string> Load()
		{
			var groups = JsonConvert.DeserializeObject<Dictionary<string, int[]>>(Resx.EmojiCategories);

			var result = new Dictionary<int, string>();
			foreach (var group in groups)
			{
				foreach (var codepoint in group.Value)
				{
					result[codepoint] = group.Key;
				}
			}

			return result;
		}
	}


	internal class Emojis : Loggable, IDisposable
	{
		private readonly List<Emoji> map;


		public Emojis()
		{
			// Emojis.json must be stored as UTF-8 Text in Resources.resx
			map = JsonConvert.DeserializeObject<List<Emoji>>(Resx.Emojis)
				.OrderBy(e => e.Name, StringComparer.Create(AddIn.Culture, true))
				.ToList();
		}


		#region Lifecycle
		private bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					foreach (var item in map)
					{
						item.Dispose();
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
			return map.Where(e => parts.Contains(e.Glyph)).Select(e => e);
		}


		/// <summary>
		/// Gets the indexed emoji
		/// </summary>
		/// <param name="index">The index of the emoji</param>
		/// <returns>An Emoji instance describing the emoji</returns>
		public Emoji this[int index] => map[index];


		/// <summary>
		/// Removes emojis from the given string.
		/// Used by various commands to "clean" their page titles before further modifications.
		/// </summary>
		/// <param name="value">A string to modify</param>
		/// <returns>A new string without emojis</returns>
		public string RemoveEmojis(string value)
		{
			var builder = new StringBuilder(value);

			foreach (var glyph in map.Select(m => m.Glyph))
			{
				var index = 0;
				do
				{
					// glyphs are either one char or two char in length

					index = builder.IndexOf(glyph[0], index);
					if (index >= 0 && glyph.Length == 1)
					{
						// found one char glyph, remove it
						builder.Remove(index, 1);
					}
					else if (index >= 0 && index <= builder.Length - 2)
					{
						// found first of two chars, test second
						if (builder[index + 1] == glyph[1])
						{
							builder.Remove(index, 2);
						}
						else
						{
							index++;
						}
					}
				} while (index >= 0 && index < builder.Length);
			}

			return builder.ToString();
		}
	}


	/// <summary>
	/// An ordinally ordered collection of every Unicode codepoint in the Segoe UI Emoji
	/// font's range that the installed font actually defines a glyph for, used to back
	/// the EmojiDialog's "all emoji" grid view alongside the curated Emojis list.
	/// </summary>
	internal sealed class UnicodeEmojis : IDisposable
	{
		private const int FirstCodepoint = 0x21;
		private const int LastCodepoint = 0x1FAF8;

		private readonly List<Emoji> map;


		public UnicodeEmojis()
		{
			map = ColorGlyphRenderer.Instance.GetSupportedCodepoints(FirstCodepoint, LastCodepoint)
				.Select(cp => new Emoji(char.ConvertFromUtf32(cp), resID: null))
				.ToList();
		}


		#region Lifecycle
		private bool disposedValue;

		private void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					foreach (var item in map)
					{
						item.Dispose();
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
		/// Gets the number of supported codepoints
		/// </summary>
		public int Count => map.Count;


		/// <summary>
		/// Gets the indexed emoji
		/// </summary>
		/// <param name="index">The index of the emoji</param>
		/// <returns>An Emoji instance describing the emoji</returns>
		public Emoji this[int index] => map[index];
	}
}
