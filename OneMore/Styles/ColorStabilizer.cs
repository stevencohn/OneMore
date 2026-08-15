//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Styles
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Xml.Linq;


	/// <summary>
	/// Scans a page's paragraphs just prior to save and pins an explicit color onto any
	/// local style attribute that is missing one but resolves, via its QuickStyleDef, to a
	/// color that is low-contrast against the page background. Guards against a OneNote COM
	/// defect where UpdatePageContent's own save-time normalization strips a local style
	/// attribute that appears redundant with its QuickStyleDef, silently falling back to the
	/// QuickStyleDef's fontColor -- which can be an invisible leftover value.
	/// </summary>
	internal static class ColorStabilizer
	{
		/// <summary>
		/// Scans all OE and T elements under the given root and injects an explicit
		/// contrasting color into any local style attribute that lacks one and would
		/// otherwise resolve to a low-contrast QuickStyleDef color.
		/// </summary>
		/// <param name="root">The page root element</param>
		/// <param name="ns">The page namespace</param>
		/// <param name="background">The page's effective background color</param>
		/// <param name="contrast">A color known to contrast against the background</param>
		/// <returns>The number of style attributes that were patched</returns>
		public static int Stabilize(XElement root, XNamespace ns, Color background, Color contrast)
		{
			var quickStyles = IndexQuickStyles(root, ns);
			if (quickStyles.Count == 0)
			{
				return 0;
			}

			var contrastCss = contrast.ToRGBHtml();
			var patched = 0;

			foreach (var oe in root.Descendants(ns + "OE"))
			{
				if (StabilizeElement(oe, quickStyles, background, contrastCss))
				{
					patched++;
				}

				foreach (var t in oe.Elements(ns + "T"))
				{
					if (StabilizeElement(t, quickStyles, background, contrastCss))
					{
						patched++;
					}
				}
			}

			return patched;
		}


		private static Dictionary<int, QuickStyleDef> IndexQuickStyles(XElement root, XNamespace ns)
		{
			var map = new Dictionary<int, QuickStyleDef>();
			foreach (var e in root.Elements(ns + "QuickStyleDef"))
			{
				var quick = new QuickStyleDef(e);
				if (!map.ContainsKey(quick.Index))
				{
					map.Add(quick.Index, quick);
				}
			}

			return map;
		}


		private static bool StabilizeElement(
			XElement element, Dictionary<int, QuickStyleDef> quickStyles,
			Color background, string contrastCss)
		{
			var attr = element.Attribute("style");
			if (attr is null || string.IsNullOrWhiteSpace(attr.Value))
			{
				// no local style at all; out of scope, fully relies on QuickStyleDef
				return false;
			}

			try
			{
				var local = new Style(attr.Value, setDefaults: false);
				if (!string.IsNullOrEmpty(local.Color))
				{
					// already has an explicit color; already pinned
					return false;
				}

				var index = ResolveQuickStyleIndex(element);
				if (index is null || !quickStyles.TryGetValue(index.Value, out var quick))
				{
					return false;
				}

				if (string.IsNullOrEmpty(quick.Color) || quick.Color.Equals(StyleBase.Automatic))
				{
					// automatic color; no invisibility risk
					return false;
				}

				var quickColor = ColorHelper.FromHtml(quick.Color);
				if (!quickColor.LowContrast(background))
				{
					return false;
				}

				// Style.ToCss() gates color emission behind ApplyColors, which defaults to
				// false when parsed with setDefaults:false; without this, the injected
				// color would be silently dropped when re-serialized below
				local.ApplyColors = true;
				local.Color = contrastCss;
				attr.Value = local.ToCss(all: true);
				return true;
			}
			catch (Exception exc)
			{
				// never let a malformed color/style on one paragraph abort the save
				Logger.Current.WriteLine($"error stabilizing color on {element.Name.LocalName}", exc);
				return false;
			}
		}


		private static int? ResolveQuickStyleIndex(XElement element)
		{
			// quickStyleIndex cascades down from an ancestor (OE, OEChildren, Outline);
			// GetAttributeValue does not ascend on its own so walk up explicitly
			var e = element;
			while (e is not null)
			{
				if (e.GetAttributeValue("quickStyleIndex", out int index, -1))
				{
					return index;
				}

				e = e.Parent;
			}

			return null;
		}
	}
}
