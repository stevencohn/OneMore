//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Globalization;
	using System.Xml.Linq;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.Styles;


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

			var settings = new SettingsProvider().GetCollection(nameof(HighlightsSheet));
			var extended = settings.Get<bool>("extended");

			var schemeName = settings.Get<string>("theme") switch
			{
				"Faded" => "Faded",
				"Deep" => "Deep",
				_ => "Bright"
			};

			var scheme = new HighlightColorSchemes().GetScheme(schemeName);
			var max = extended ? scheme.Colors.Count + scheme.Extended.Count : scheme.Colors.Count;

			var meta = page.GetMetaContent(MetaNames.HighlightIndex);
			if (meta != null)
			{
				if (int.TryParse(meta, out index) && advance)
				{
					index = index < max - 1 ? index + 1 : 0;
				}
			}

			return (index, GetColor(scheme, index, extended));
		}


		private static string GetColor(HighlightColorScheme scheme, int index, bool extended)
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
					return new XElement("span", new XAttribute("style", $"background:{color}"), text);
				}

				var span = (XElement)s;
				span.GetAttributeValue("style", out var css, string.Empty);

				if (string.IsNullOrEmpty(css))
				{
					span.SetAttributeValue("style", $"background:{color}");
				}
				else
				{
					// need to parse so we filter out mso-highlight attribute
					// setDefaults to false so it doesn't default font family and size
					var style = new Style(css, setDefaults: false)
					{
						// enable ApplyColors so it emits background attribute
						ApplyColors = true,
						Highlight = color
					};

					span.SetAttributeValue("style", style.ToCss(false));
				}

				return span;
			});
		}
	}
}
