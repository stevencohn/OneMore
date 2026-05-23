//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.PageModels
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;


	/// <summary>
	/// Parses, builds, and merges the CSS-like style attribute used on OneNote XML elements.
	/// Format: semicolon-separated key:value pairs, e.g.
	///   "font-family:Calibri;font-size:11.0pt;color:#1F4E79;font-style:italic"
	///
	/// Quirks handled:
	///  - font-size always written as "11.0pt" (one decimal, pt suffix)
	///  - color:automatic and color:none are normalised to null on read
	///  - mso-highlight kept alongside background for Office compatibility
	///  - trailing semicolons and extra whitespace stripped on parse
	/// </summary>
	internal sealed class StyleString
	{
		private const string Automatic = "automatic";
		private const string None = "none";

		private readonly Dictionary<string, string> props;


		private StyleString()
		{
			props = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		}


		private StyleString(Dictionary<string, string> initial)
		{
			props = initial;
		}


		public static StyleString Empty => new StyleString();


		/// <summary>Parses a CSS-like style string. Returns an empty instance for null/empty input.</summary>
		public static StyleString Parse(string css)
		{
			var props = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			if (string.IsNullOrWhiteSpace(css)) return new StyleString(props);

			foreach (var part in css.Split(';'))
			{
				var colon = part.IndexOf(':');
				if (colon < 1) continue;

				var key = part.Substring(0, colon).Trim();
				var val = part.Substring(colon + 1).Trim().Trim('\'');

				if (!string.IsNullOrEmpty(key) && !props.ContainsKey(key))
					props[key] = val;
			}

			return new StyleString(props);
		}


		/// <summary>
		/// Merges multiple style layers (cascade). Later layers override earlier ones.
		/// Equivalent to CSS cascade: QuickStyleDef < OE style < T style.
		/// </summary>
		public static StyleString Merge(params StyleString[] layers)
		{
			var props = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			foreach (var layer in layers)
				foreach (var kv in layer.props)
					props[kv.Key] = kv.Value;
			return new StyleString(props);
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Typed accessors

		public string FontFamily
		{
			get => Get("font-family");
			set => SetOrRemove("font-family", value);
		}


		/// <summary>Font size in points. Written as "11.0pt" on serialization.</summary>
		public double? FontSizePt
		{
			get
			{
				var v = Get("font-size");
				if (v is null) return null;
				v = v.Replace("pt", string.Empty).Trim();
				return double.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : (double?)null;
			}
			set
			{
				if (value is null)
					Remove("font-size");
				else
					Set("font-size", $"{value.Value:F1}pt");
			}
		}


		/// <summary>
		/// Foreground color as #RRGGBB. Returns null when not set, or when value is
		/// "automatic" or "none" (OneNote sentinel values).
		/// </summary>
		public string Color
		{
			get
			{
				var v = Get("color");
				if (v is null
					|| v.Equals(Automatic, StringComparison.OrdinalIgnoreCase)
					|| v.Equals(None, StringComparison.OrdinalIgnoreCase))
					return null;
				return v;
			}
			set => SetOrRemove("color", value);
		}


		/// <summary>
		/// Highlight/background color. Sets both "background" and "mso-highlight" for
		/// Office compatibility. Returns null when not set or sentinel.
		/// </summary>
		public string Background
		{
			get
			{
				var v = Get("background") ?? Get("mso-highlight");
				if (v is null
					|| v.Equals(Automatic, StringComparison.OrdinalIgnoreCase)
					|| v.Equals(None, StringComparison.OrdinalIgnoreCase))
					return null;
				return v;
			}
			set
			{
				SetOrRemove("background", value);
				// keep mso-highlight in sync for Office compatibility
				if (value is not null)
					Set("mso-highlight", value);
				else
					Remove("mso-highlight");
			}
		}


		/// <summary>Bold text. Reads font-weight:bold; writes "bold" or removes the property.</summary>
		public bool? Bold
		{
			get
			{
				var v = Get("font-weight");
				if (v is null) return null;
				return v.Equals("bold", StringComparison.OrdinalIgnoreCase);
			}
			set
			{
				if (value is null) Remove("font-weight");
				else Set("font-weight", value.Value ? "bold" : "normal");
			}
		}


		public bool? Italic
		{
			get
			{
				var v = Get("font-style");
				if (v is null) return null;
				return v.Equals("italic", StringComparison.OrdinalIgnoreCase);
			}
			set
			{
				if (value is null) Remove("font-style");
				else Set("font-style", value.Value ? "italic" : "normal");
			}
		}


		public bool? Underline
		{
			get => GetTextDecoration("underline");
			set => SetTextDecoration("underline", value);
		}


		public bool? Strikethrough
		{
			get => GetTextDecoration("line-through");
			set => SetTextDecoration("line-through", value);
		}


		public bool? Superscript
		{
			get => GetVerticalAlign("super");
			set { if (value == true) Set("vertical-align", "super"); else if (value == false) Remove("vertical-align"); }
		}


		public bool? Subscript
		{
			get => GetVerticalAlign("sub");
			set { if (value == true) Set("vertical-align", "sub"); else if (value == false) Remove("vertical-align"); }
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Raw access

		public string Get(string cssProperty)
			=> props.TryGetValue(cssProperty, out var v) ? v : null;


		public void Set(string cssProperty, string value)
			=> props[cssProperty] = value;


		public void Remove(string cssProperty)
			=> props.Remove(cssProperty);


		public bool IsEmpty => props.Count == 0;


		public override string ToString()
		{
			if (props.Count == 0) return string.Empty;
			var sb = new StringBuilder();
			foreach (var kv in props)
			{
				if (sb.Length > 0) sb.Append(';');
				sb.Append(kv.Key).Append(':').Append(kv.Value);
			}
			return sb.ToString();
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Helpers

		private void SetOrRemove(string key, string value)
		{
			if (value is null) Remove(key);
			else Set(key, value);
		}


		private bool? GetTextDecoration(string decoration)
		{
			var v = Get("text-decoration");
			if (v is null) return null;
			return v.IndexOf(decoration, StringComparison.OrdinalIgnoreCase) >= 0;
		}


		private void SetTextDecoration(string decoration, bool? value)
		{
			// text-decoration can be "underline line-through" (both set simultaneously)
			var current = Get("text-decoration") ?? string.Empty;
			var parts = current.Split(' ')
				.Select(p => p.Trim())
				.Where(p => p.Length > 0 && !p.Equals(decoration, StringComparison.OrdinalIgnoreCase))
				.ToList();

			if (value == true) parts.Add(decoration);
			var result = string.Join(" ", parts);

			if (result.Length == 0) Remove("text-decoration");
			else Set("text-decoration", result);
		}


		private bool? GetVerticalAlign(string alignment)
		{
			var v = Get("vertical-align");
			if (v is null) return null;
			return v.Equals(alignment, StringComparison.OrdinalIgnoreCase);
		}
	}
}
