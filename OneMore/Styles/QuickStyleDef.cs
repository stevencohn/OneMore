//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Styles
{
	using System.Collections.Generic;
	using System.Xml.Linq;
	using System.Xml.Serialization;


	/// <summary>
	/// Models the QuickStyleDef element used on OneNote pages. The quick styles describe
	/// the hard-coded styles that come with OneNote. These cannot be changed and OneNote
	/// only create an element for a style if it is used on the page.
	/// </summary>
	[XmlRoot("QuickStyleDef")]
	public class QuickStyleDef : XmlStyleBase
	{

		public QuickStyleDef() : base()
		{
			Prefix = "one";
			Namespace = Properties.Resources.oneNamespace;

			// default quick style is for a simple paragraph
			Name = "P";
		}


		/// <summary>
		/// Initialize a new instance from the given element.
		/// </summary>
		/// <param name="element">An XElement from which properties are copied</param>
		public QuickStyleDef(XElement element) : base()
		{
			if (!element.HasAttributes) return;

			foreach (var attr in element.Attributes())
			{
				if (attr.Name == "index")
				{
					Index = int.Parse(attr.Value);
				}
				else if (attr.Name == "name")
				{
					Name = attr.Value;
				}
				else if (attr.Name == "font")
				{
					FontFamily = attr.Value;
				}
				else if (attr.Name == "fontSize")
				{
					FontSize = attr.Value;
				}
				else if (attr.Name == "fontColor")
				{
					Color = attr.Value;
				}
				else if (attr.Name == "highlightColor")
				{
					Highlight = attr.Value;
				}
				else if (attr.Name == "spaceBefore")
				{
					SpaceBefore = attr.Value;
				}
				else if (attr.Name == "spaceAfter")
				{
					SpaceAfter = attr.Value;
				}
			}
		}


		// one:QuickStyleDef possibles:
		// fontColor="automatic" highlightColor="automatic" font="Calibri" fontSize="11.0" spaceBefore="0.0" spaceAfter="0.0" />
		public static void CollectStyleProperties(
			XElement element, Dictionary<string, string> properties)
		{
			XAttribute a;

			a = element.Attribute("font");
			if (a != null && !properties.ContainsKey("font-family"))
				properties.Add("font-family", a.Value);

			a = element.Attribute("fontSize");
			if (a != null && !properties.ContainsKey("font-size"))
				properties.Add("font-size", a.Value);

			a = element.Attribute("fontColor");
			if (a != null && !a.Value.Equals(Automatic) && !properties.ContainsKey("color"))
				properties.Add("color", a.Value);

			a = element.Attribute("highlightColor");
			if (a != null && !a.Value.Equals(Automatic) && !properties.ContainsKey("background"))
				properties.Add("background", a.Value);

			a = element.Attribute("spaceBefore");
			if (a != null && !properties.ContainsKey("spaceBefore"))
				properties.Add("spaceBefore", a.Value);

			a = element.Attribute("spaceAfter");
			if (a != null && !properties.ContainsKey("spaceAfter"))
				properties.Add("spaceAfter", a.Value);
		}


		public override bool Equals(object obj)
		{
			if (!(obj is QuickStyleDef other))
			{
				return false;
			}

			if (FontFamily != other.FontFamily) return false;
			if (FontSize != other.FontSize) return false;
			if (!Color.Equals(other.color)) return false;
			if (!Highlight.Equals(other.FontFamily)) return false;
			if (SpaceBefore != other.SpaceBefore) return false;
			if (SpaceAfter != other.SpaceAfter) return false;

			return true;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}


		public XElement ToElement(XNamespace ns)
		{
			var element = new XElement(ns + "QuickStyleDef",
				new XAttribute("index", Index.ToString()),
				new XAttribute("name", Name),
				new XAttribute("font", FontFamily),
				new XAttribute("fontSize", FontSize),
				new XAttribute("fontColor", Color)
				);

			if (!string.IsNullOrEmpty(Highlight))
			{
				element.Add(new XAttribute("highlightColor", Highlight));
			}

			if (!string.IsNullOrEmpty(SpaceBefore))
			{
				element.Add(new XAttribute("spaceBefore", SpaceBefore));
			}

			if (!string.IsNullOrEmpty(SpaceAfter))
			{
				element.Add(new XAttribute("spaceAfter", SpaceAfter));
			}

			return element;
		}
	}
}
