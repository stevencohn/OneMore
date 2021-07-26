//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Xml;
	using System.Xml.Linq;
    using System.Xml.Serialization;


	/// <summary>
	/// Serializable style used to store custom styles.
	/// </summary>
	[XmlRoot("Style")]
	public class StyleRecord : XmlStyleBase, IXmlSerializable
	{

		/// <summary>
		/// Initialize a new instance with defaults.
		/// </summary>
		public StyleRecord() : base()
		{
			Namespace = null;
			ApplyColors = true;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="style"></param>
		public StyleRecord(StyleBase style) : base(style)
		{
		}


		public StyleRecord(XElement element) : this()
		{
			if (element.GetAttributeValue("name", out var name))
				Name = name;

			if (element.GetAttributeValue("styleType", out StyleType styleType, StyleType.Character))
				StyleType = styleType;

			if (element.GetAttributeValue("index", out int index, 0))
				Index = index;

			if (element.GetAttributeValue("font", out var fontFamily))
				FontFamily = fontFamily;

			element.GetAttributeValue("fontSize", out fontSize, 0.0);

			if (element.GetAttributeValue("fontColor", out var fontColor))
				Color = fontColor;

			if (element.GetAttributeValue("highlightColor", out var highlightColor))
				Highlight = highlightColor;

			if (element.GetAttributeValue("applyColors", out bool applyColors, false))
				ApplyColors = applyColors;

			if (element.GetAttributeValue("bold", out bool bold, false))
				IsBold = bold;

			if (element.GetAttributeValue("italic", out bool italic, false))
				IsItalic = italic;

			if (element.GetAttributeValue("underline", out bool underline, false))
				IsUnderline = underline;

			if (element.GetAttributeValue("strikethrough", out bool strikethrough, false))
				IsStrikethrough = strikethrough;

			if (element.GetAttributeValue("superscript", out bool superscript, false))
				IsSuperscript = superscript;

			if (element.GetAttributeValue("subscript", out bool subscript, false))
				IsSubscript = subscript;

			element.GetAttributeValue("spaceBefore", out spaceBefore, 0.0);
			element.GetAttributeValue("spaceAfter", out spaceAfter, 0.0);
			element.GetAttributeValue("spacing", out spacing, 0.0);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public XElement ToXElement()
		{
			// TODO: this could be rethought and made more efficient!
			return XElement.Parse(ToXml());
		}


		/// <summary>
		/// Extends the base WriteXml with additional properties managed by this class.
		/// </summary>
		/// <param name="writer">The XmlWriter used by the serializer.</param>
		public override void WriteXml(XmlWriter writer)
		{
			base.WriteXml(writer);

			writer.WriteAttributeString("applyColors", ApplyColors.ToString().ToLower());
			writer.WriteAttributeString("styleType", StyleType.ToString());
		}
	}
}
