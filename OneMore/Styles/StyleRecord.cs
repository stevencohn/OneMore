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
			if (element.ReadAttributeValue("name", out var name))
				Name = name;

			if (element.ReadAttributeValue("styleType", out StyleType styleType, StyleType.Character))
				StyleType = styleType;

			if (element.ReadAttributeValue("index", out int index, 0))
				Index = index;

			if (element.ReadAttributeValue("font", out var fontFamily))
				FontFamily = fontFamily;

			element.ReadAttributeValue("fontSize", out fontSize, 0.0);

			if (element.ReadAttributeValue("fontColor", out var fontColor))
				Color = fontColor;

			if (element.ReadAttributeValue("highlightColor", out var highlightColor))
				Highlight = highlightColor;

			if (element.ReadAttributeValue("applyColors", out bool applyColors, false))
				ApplyColors = applyColors;

			if (element.ReadAttributeValue("bold", out bool bold, false))
				IsBold = bold;

			if (element.ReadAttributeValue("italic", out bool italic, false))
				IsItalic = italic;

			if (element.ReadAttributeValue("underline", out bool underline, false))
				IsUnderline = underline;

			if (element.ReadAttributeValue("strikethrough", out bool strikethrough, false))
				IsStrikethrough = strikethrough;

			if (element.ReadAttributeValue("superscript", out bool superscript, false))
				IsSuperscript = superscript;

			if (element.ReadAttributeValue("subscript", out bool subscript, false))
				IsSubscript = subscript;

			element.ReadAttributeValue("spaceBefore", out spaceBefore, 0.0);
			element.ReadAttributeValue("spaceAfter", out spaceAfter, 0.0);
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
