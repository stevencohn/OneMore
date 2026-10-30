//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Xml.Linq;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.Styles;
	using River.OneMoreAddIn.UI;


	/// <summary>
	/// Shared logic for the rotating highlighter; picks the next color from the configured
	/// scheme, remembers the rotation index on the page, and applies a color to selected text.
	/// </summary>
	internal static class HighlightRotator
	{
		/// <summary>
		/// Gets the color for the current, or next, position in the rotation.
		/// </summary>
		/// <param name="page">The page whose meta holds the rotation index</param>
		/// <param name="advance">True to move to the next color, false to repeat the last</param>
		/// <returns>The rotation index and its color</returns>
		public static (int Index, string Color) GetColor(Page page, bool advance)
		{
			var index = 0;

			var scheme = LoadScheme(page, out var extended, out var dark);
			var max = extended ? scheme.Colors.Count + scheme.Extended.Count : scheme.Colors.Count;

			var meta = page.GetMetaContent(MetaNames.HighlightIndex);
			if (meta != null)
			{
				if (int.TryParse(meta, out index) && advance)
				{
					index = index < max - 1 ? index + 1 : 0;
				}
			}

			return (index, GetColor(scheme, index, extended, dark));
		}


		/// <summary>
		/// Gets every color in the configured scheme, in rotation order, so a user can choose
		/// one explicitly. The list position is the rotation index of that color.
		/// </summary>
		/// <param name="page">The page whose background determines the light or dark set</param>
		/// <returns>
		/// Display is the color as the user will see it; Stored is the value to write to the page
		/// </returns>
		public static List<(string Display, string Stored)> GetPalette(Page page)
		{
			var scheme = LoadScheme(page, out var extended, out var dark);
			var count = extended ? scheme.Colors.Count + scheme.Extended.Count : scheme.Colors.Count;

			var palette = new List<(string Display, string Stored)>(count);
			for (var index = 0; index < count; index++)
			{
				palette.Add((PickColor(scheme, index, extended), GetColor(scheme, index, extended, dark)));
			}

			return palette;
		}


		private static HighlightColorSet LoadScheme(Page page, out bool extended, out bool dark)
		{
			var settings = new SettingsProvider().GetCollection(nameof(HighlightsSheet));
			extended = settings.Get<bool>("extended");

			var schemeName = settings.Get<string>("theme") switch
			{
				"Faded" => "Faded",
				"Deep" => "Deep",
				_ => "Bright"
			};

			dark = MoreColorPicker.IsDarkPage(page.GetPageColor(out _, out _));
			return new HighlightColorSchemes().GetScheme(schemeName).GetSet(dark);
		}


		private static string GetColor(
			HighlightColorSet scheme, int index, bool extended, bool dark)
		{
			var color = PickColor(scheme, index, extended);
			if (!dark || !ColorMath.TryParseHex(color, out var rendered))
			{
				return color;
			}

			// dark colors are what we want to see, so store what OneNote will invert into them
			return ColorMath.ToHex(ColorPalette.ToLight(rendered));
		}


		private static string PickColor(HighlightColorSet scheme, int index, bool extended)
		{
			if (extended && index >= scheme.Colors.Count)
			{
				return scheme.Extended[(index - scheme.Colors.Count) % scheme.Extended.Count];
			}

			// clamp into Colors range even if index is a stale/leftover value from
			// when extended was enabled (e.g. via the "highlight last" repeat command)
			return scheme.Colors[index % scheme.Colors.Count];
		}


		/// <summary>
		/// Persists the rotation index so the next call continues from here.
		/// </summary>
		public static void SaveIndex(Page page, int index)
		{
			page.SetMeta(MetaNames.HighlightIndex, index.ToString(CultureInfo.InvariantCulture));
		}


		/// <summary>
		/// Applies the background color to all selected text on the page.
		/// </summary>
		/// <returns>True if any text was updated</returns>
		public static bool Apply(Page page, string color)
		{
			return new PageEditor(page).EditSelected((s) =>
			{
				if (s is XText text)
				{
					return Wrap(text, color);
				}

				var span = (XElement)s;
				SetBackground(span, color);
				return span;
			});
		}


		/// <summary>
		/// Wraps the text in a span with the given background color.
		/// </summary>
		public static XElement Wrap(XText text, string color)
		{
			return new XElement("span", new XAttribute("style", $"background:{color}"), text);
		}


		/// <summary>
		/// Sets the background color of the span, replacing any existing background and
		/// preserving all other style attributes such as bold, italic, color, and font.
		/// </summary>
		public static void SetBackground(XElement span, string color)
		{
			span.GetAttributeValue("style", out var css, string.Empty);

			// edit the declarations directly rather than round-tripping through Style, which
			// would drop properties it does not emit such as font family and size; also drop
			// the mso-highlight declaration, which would otherwise override the background
			var declarations = new List<string>();
			foreach (var part in css.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
			{
				var declaration = part.Trim();
				if (declaration.Length == 0)
				{
					continue;
				}

				var name = declaration.Split(':')[0].Trim().ToLowerInvariant();
				if (name != "background" && name != "background-color" && name != "mso-highlight")
				{
					declarations.Add(declaration);
				}
			}

			declarations.Add($"background:{color}");
			span.SetAttributeValue("style", string.Join(";", declarations));
		}
	}
}
