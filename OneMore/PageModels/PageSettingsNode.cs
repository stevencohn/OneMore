//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.PageModels
{
	using System.Globalization;
	using System.Linq;
	using System.Xml.Linq;


	/// <summary>
	/// Wraps the one:PageSettings element, which controls page layout: background color,
	/// RTL direction, page size/orientation/dimensions/margins, and rule lines.
	/// </summary>
	internal sealed class PageSettingsNode : OneNoteNode
	{
		internal PageSettingsNode(XElement el) : base(el) { }


		/// <summary>Creates a new PageSettings element with default (automatic) values.</summary>
		public static PageSettingsNode Create()
			=> new PageSettingsNode(E("PageSettings",
				new XAttribute("RTL", "false"),
				new XAttribute("color", "automatic"),
				E("PageSize", E("Automatic"))));


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Page-level attributes

		/// <summary>Right-to-left page layout.</summary>
		public bool RTL
		{
			get => AttrBool("RTL") == true;
			set => AttrBool("RTL", value);
		}


		/// <summary>
		/// Page background color (#RRGGBB). Null when absent or "automatic" (default white).
		/// Write null to restore automatic.
		/// </summary>
		public string Color
		{
			get { var v = Attr("color"); return v == "automatic" ? null : v; }
			set => Attr("color", value ?? "automatic");
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Page size

		/// <summary>True when the page uses automatic (content-driven) sizing.</summary>
		public bool IsAutoSize
			=> el.Element(NS + "PageSize")?.Element(NS + "Automatic") is not null;


		/// <summary>True when the page is in landscape orientation.</summary>
		public bool IsLandscape
		{
			get
			{
				var orient = el.Element(NS + "PageSize")?.Element(NS + "Orientation");
				return orient is not null && (orient.Attribute("landscape")?.Value == "true");
			}
			set
			{
				EnsureManualPageSize();
				SetOrCreate(el.Element(NS + "PageSize"), "Orientation",
					new XAttribute("landscape", value ? "true" : "false"));
			}
		}


		/// <summary>Page width in points. NaN when auto-sized.</summary>
		public double PageWidth
		{
			get => GetDimension("width");
			set { EnsureManualPageSize(); SetDimension("width", value); }
		}


		/// <summary>Page height in points. NaN when auto-sized.</summary>
		public double PageHeight
		{
			get => GetDimension("height");
			set { EnsureManualPageSize(); SetDimension("height", value); }
		}


		public double MarginTop
		{
			get => GetMargin("top");
			set { EnsureManualPageSize(); SetMargin("top", value); }
		}


		public double MarginBottom
		{
			get => GetMargin("bottom");
			set { EnsureManualPageSize(); SetMargin("bottom", value); }
		}


		public double MarginLeft
		{
			get => GetMargin("left");
			set { EnsureManualPageSize(); SetMargin("left", value); }
		}


		public double MarginRight
		{
			get => GetMargin("right");
			set { EnsureManualPageSize(); SetMargin("right", value); }
		}


		/// <summary>Switches the page to automatic (content-driven) sizing.</summary>
		public void SetAutoSize()
		{
			var pageSize = el.Element(NS + "PageSize");
			if (pageSize is null)
			{
				el.Add(E("PageSize", E("Automatic")));
			}
			else
			{
				pageSize.RemoveNodes();
				pageSize.Add(E("Automatic"));
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Rule lines

		/// <summary>Whether ruled lines are drawn on the page canvas.</summary>
		public bool RuleLinesVisible
		{
			get => el.Element(NS + "RuleLines")?.Attribute("visible")?.Value == "true";
			set
			{
				var rl = EnsureRuleLines();
				rl.SetAttributeValue("visible", value ? "true" : "false");
			}
		}


		/// <summary>Ruled line color (#RRGGBB). Null when absent or "automatic".</summary>
		public string RuleLineColor
		{
			get
			{
				var v = el.Element(NS + "RuleLines")
					?.Element(NS + "RuleLineSettings")
					?.Attribute("color")?.Value;
				return v == "automatic" ? null : v;
			}
			set
			{
				EnsureRuleLineSettings().SetAttributeValue("color", value ?? "automatic");
			}
		}


		/// <summary>Spacing between ruled lines in points.</summary>
		public double? RuleLineSpacing
		{
			get
			{
				var v = el.Element(NS + "RuleLines")
					?.Element(NS + "RuleLineSettings")
					?.Attribute("spacing")?.Value;
				return double.TryParse(v, NumberStyles.Any,
					CultureInfo.InvariantCulture, out var d) ? d : (double?)null;
			}
			set
			{
				var attr = value?.ToString("F1", CultureInfo.InvariantCulture);
				EnsureRuleLineSettings().SetAttributeValue("spacing", attr);
			}
		}


		/// <summary>Margin indicator line color (#RRGGBB). Null when absent or "automatic".</summary>
		public string RuleMarginColor
		{
			get
			{
				var v = el.Element(NS + "RuleLines")
					?.Element(NS + "RuleMarginSettings")
					?.Attribute("color")?.Value;
				return v == "automatic" ? null : v;
			}
			set
			{
				EnsureRuleMarginSettings().SetAttributeValue("color", value ?? "automatic");
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Helpers

		private void EnsureManualPageSize()
		{
			var pageSize = el.Element(NS + "PageSize");
			if (pageSize is null)
			{
				pageSize = E("PageSize");
				el.Add(pageSize);
			}
			// remove Automatic child if present
			pageSize.Element(NS + "Automatic")?.Remove();
		}


		private double GetDimension(string attr)
		{
			var v = el.Element(NS + "PageSize")
				?.Element(NS + "Dimensions")
				?.Attribute(attr)?.Value;
			return double.TryParse(v, NumberStyles.Any,
				CultureInfo.InvariantCulture, out var d) ? d : double.NaN;
		}


		private void SetDimension(string attr, double value)
		{
			var pageSize = el.Element(NS + "PageSize")!;
			var dims = pageSize.Element(NS + "Dimensions");
			if (dims is null)
			{
				dims = E("Dimensions");
				pageSize.Add(dims);
			}
			dims.SetAttributeValue(attr, value.ToString("F2", CultureInfo.InvariantCulture));
		}


		private double GetMargin(string side)
		{
			var v = el.Element(NS + "PageSize")
				?.Element(NS + "Margins")
				?.Attribute(side)?.Value;
			return double.TryParse(v, NumberStyles.Any,
				CultureInfo.InvariantCulture, out var d) ? d : double.NaN;
		}


		private void SetMargin(string side, double value)
		{
			var pageSize = el.Element(NS + "PageSize")!;
			var margins = pageSize.Element(NS + "Margins");
			if (margins is null)
			{
				margins = E("Margins");
				pageSize.Add(margins);
			}
			margins.SetAttributeValue(side, value.ToString("F2", CultureInfo.InvariantCulture));
		}


		private XElement EnsureRuleLines()
		{
			var rl = el.Element(NS + "RuleLines");
			if (rl is null)
			{
				rl = E("RuleLines", new XAttribute("visible", "false"));
				el.Add(rl);
			}
			return rl;
		}


		private XElement EnsureRuleLineSettings()
		{
			var rl = EnsureRuleLines();
			var rls = rl.Element(NS + "RuleLineSettings");
			if (rls is null)
			{
				rls = E("RuleLineSettings", new XAttribute("color", "automatic"));
				rl.Add(rls);
			}
			return rls;
		}


		private XElement EnsureRuleMarginSettings()
		{
			var rl = EnsureRuleLines();
			var rms = rl.Element(NS + "RuleMarginSettings");
			if (rms is null)
			{
				rms = E("RuleMarginSettings", new XAttribute("color", "automatic"));
				rl.Add(rms);
			}
			return rms;
		}


		private static void SetOrCreate(XElement parent, string childName, params object[] attrs)
		{
			var child = parent?.Element(NS + childName);
			if (child is null)
				parent?.Add(new XElement(NS + childName, attrs));
			else
				foreach (XAttribute a in attrs)
					child.SetAttributeValue(a.Name, a.Value);
		}

		private static XNamespace NS => OneNoteNode.NS;
	}
}
