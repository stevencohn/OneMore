//************************************************************************************************
// Copyright © 2026 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Globalization;
	using System.Text;


	/// <summary>
	/// Draws page titles that may contain emoji, switching to the "Segoe UI Emoji" font for the
	/// emoji runs since GDI+ cannot render them using the default UI font. Titles with no emoji
	/// are drawn exactly as before, via the normal Graphics.DrawString path.
	/// </summary>
	internal static class TitleRenderer
	{
		private const string EmojiFamily = "Segoe UI Emoji";
		private const int UnconstrainedWidth = 100000;

		private static readonly Dictionary<(float Size, FontStyle Style), Font> emojiFontCache = new();

		// runs are measured and drawn using this same format so accumulated x-offsets match the
		// actual painted glyph positions; the default format adds extra bearing around each
		// string that would otherwise show up as visible gaps between adjacent runs
		private static readonly StringFormat typographicFormat = new(StringFormat.GenericTypographic)
		{
			FormatFlags = StringFormatFlags.NoWrap,
			Trimming = StringTrimming.None
		};

		private struct RunLayout
		{
			public string Text;
			public Font Font;
			public bool IsEmoji;
			public float X;
			public float Width;
		}


		/// <summary>
		/// Draws a page title, using the "Segoe UI Emoji" font for any emoji it contains and
		/// truncating with an ellipsis if it doesn't fit within the given bounds.
		/// </summary>
		/// <param name="g">The graphics surface to draw on</param>
		/// <param name="title">The title to draw</param>
		/// <param name="font">The font to use for non-emoji text</param>
		/// <param name="brush">The brush to draw with</param>
		/// <param name="bounds">The bounding rectangle to draw within</param>
		/// <param name="format">The format to use when the title contains no emoji</param>
		/// <returns>The actual size of the rendered title</returns>
		internal static Size DrawTitle(
			Graphics g, string title, Font font, Brush brush, Rectangle bounds, StringFormat format)
		{
			var runs = EmojiRuns.Split(title);
			if (runs.Count == 1 && !runs[0].IsEmoji)
			{
				g.DrawString(title, font, brush, bounds, format);
				return g.MeasureString(title, font, bounds.Width, format).ToSize();
			}

			var layout = ComputeLayout(g, runs, font, bounds.Width);

			foreach (var run in layout)
			{
				g.DrawString(run.Text, run.Font, brush,
					new RectangleF(bounds.X + run.X, bounds.Y, run.Width, bounds.Height),
					typographicFormat);
			}

			var last = layout[layout.Count - 1];
			var width = last.X + last.Width;
			return new Size((int)Math.Ceiling(width), font.Height);
		}


		private static Font GetEmojiFont(Font baseFont)
		{
			var key = (baseFont.SizeInPoints, baseFont.Style);
			if (!emojiFontCache.TryGetValue(key, out var font))
			{
				font = new Font(EmojiFamily, key.SizeInPoints, key.Style, GraphicsUnit.Point);
				emojiFontCache[key] = font;
			}

			return font;
		}


		private static List<RunLayout> ComputeLayout(
			Graphics g, IReadOnlyList<(string Text, bool IsEmoji)> runs, Font font, int maxWidth)
		{
			var layout = new List<RunLayout>(runs.Count);
			var x = 0f;

			foreach (var (text, isEmoji) in runs)
			{
				var runFont = isEmoji ? GetEmojiFont(font) : font;
				var width = g.MeasureString(text, runFont, UnconstrainedWidth, typographicFormat).Width;
				layout.Add(new RunLayout { Text = text, Font = runFont, IsEmoji = isEmoji, X = x, Width = width });
				x += width;
			}

			return x <= maxWidth ? layout : Truncate(g, layout, font, maxWidth);
		}


		private static List<RunLayout> Truncate(Graphics g, List<RunLayout> runs, Font font, int maxWidth)
		{
			var ellipsisWidth = g.MeasureString("…", font, UnconstrainedWidth, typographicFormat).Width;
			var budget = Math.Max(0, maxWidth - ellipsisWidth);

			var truncated = new List<RunLayout>();
			var used = 0f;

			foreach (var run in runs)
			{
				if (used + run.Width <= budget)
				{
					truncated.Add(new RunLayout
					{
						Text = run.Text, Font = run.Font, IsEmoji = run.IsEmoji, X = used, Width = run.Width
					});

					used += run.Width;
					continue;
				}

				// never slice inside an emoji run (would break a ZWJ/flag sequence); only a
				// plain text run gets trimmed to fit the remaining space
				var remaining = budget - used;
				if (remaining > 0 && !run.IsEmoji)
				{
					var text = TrimToWidth(g, run.Text, run.Font, remaining);
					if (!string.IsNullOrEmpty(text))
					{
						var width = g.MeasureString(text, run.Font, UnconstrainedWidth, typographicFormat).Width;
						truncated.Add(new RunLayout { Text = text, Font = run.Font, X = used, Width = width });
						used += width;
					}
				}

				break;
			}

			truncated.Add(new RunLayout { Text = "…", Font = font, X = used, Width = ellipsisWidth });
			return truncated;
		}


		private static string TrimToWidth(Graphics g, string text, Font font, float maxWidth)
		{
			var elements = StringInfo.GetTextElementEnumerator(text);
			var builder = new StringBuilder();

			while (elements.MoveNext())
			{
				var element = (string)elements.Current;
				var width = g.MeasureString(builder + element, font, UnconstrainedWidth, typographicFormat).Width;
				if (width > maxWidth)
				{
					break;
				}

				builder.Append(element);
			}

			return builder.ToString();
		}
	}
}
