//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Xml.Linq;
	using System.Xml.Serialization;


	/// <summary>
	/// Models the QuickStyleDef element used on OneNote pages. The quick styles describe
	/// the hard-coded styles that come with OneNote. These cannot be changed and OneNote
	/// only create an element for a style if it is used on the page.
	/// </summary>
	[XmlRoot("QuickStyleDef")]
	public class QuickStyleDef : XmlStyleBase, IXmlSerializable
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
				else if (attr.Name == "fontCOlor")
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
	}
}
