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
		public Emoji(string glyph, string resID, string color = null)
		{
			Glyph = glyph;
			ResID = resID;
			Color = color;

			// strip e_ prefix
			var key = ResID.Substring(2);

			Name = Resx.ResourceManager.GetString($"Emoji_{key}", AddIn.Culture);
			if (string.IsNullOrWhiteSpace(Name))
			{
				Name = $"RESX...Emoji_{resID}";
			}
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

		public string Glyph { get; set; }
		public string ResID { get; set; }
		public string Name { get; set; }
		public string Color { get; set; }
		public Image Image { get; set; }
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
		/// Load each emoji image from resources
		/// </summary>
		public void LoadImages()
		{
			foreach (var emoji in map)
			{
				emoji.Image = ((Bitmap)Resx.ResourceManager.GetObject(emoji.ResID));
			}
		}


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
}
