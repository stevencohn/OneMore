//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System.Linq;
	using System.Xml.Linq;


	/// <summary>
	/// Helpers for the Bullet or Number marker of a list item, i.e. OE/List/(Bullet|Number)
	/// </summary>
	internal static class ListMarker
	{
		// style attributes only; structural attributes such as bullet, numberFormat,
		// numberSequence, text and startAt define the list itself and must be preserved
		private static readonly string[] StyleAttributes =
		{
			"font", "fontSize", "fontColor", "bold", "italic", "underline",
			"strikethrough", "superscript", "subscript", "highlightColor"
		};


		/// <summary>
		/// Finds the Bullet or Number marker of the given list item.
		/// </summary>
		/// <param name="oe">An OE element</param>
		/// <returns>The Bullet or Number element, or null if the OE is not a list item</returns>
		public static XElement Find(XElement oe)
		{
			return oe.Elements(oe.Name.Namespace + "List").Elements()
				.FirstOrDefault(e =>
					e.Name.LocalName == "Bullet" ||
					e.Name.LocalName == "Number");
		}


		/// <summary>
		/// Removes all style attributes from the marker of the given list item, leaving
		/// only its structural attributes, then optionally applies a font and size.
		/// </summary>
		/// <param name="oe">An OE element</param>
		/// <param name="font">
		/// The font family to apply to a Number marker; null leaves the font unspecified.
		/// A Bullet marker has no font attribute so this is ignored for bullets.
		/// </param>
		/// <param name="fontSize">
		/// The font size to apply to the marker; null leaves the size unspecified.
		/// </param>
		/// <returns>True if the marker was changed; false if not a list item or unchanged</returns>
		public static bool Reset(XElement oe, string font = null, string fontSize = null)
		{
			var marker = Find(oe);
			if (marker is null)
			{
				return false;
			}

			var changed = false;

			foreach (var name in StyleAttributes)
			{
				var desired = name switch
				{
					"font" => marker.Name.LocalName == "Number" ? font : null,
					"fontSize" => fontSize,
					_ => null
				};

				if (marker.Attribute(name)?.Value != desired)
				{
					marker.SetAttributeValue(name, desired);
					changed = true;
				}
			}

			return changed;
		}
	}
}
