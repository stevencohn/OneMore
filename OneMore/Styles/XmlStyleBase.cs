//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable CA1012 // abstract should not have constructor

namespace River.OneMoreAddIn.Styles
{
	using System;
	using System.IO;
	using System.Xml;
	using System.Xml.Schema;
	using System.Xml.Serialization;

	/*
	  <one:QuickStyleDef index="1" name="p"
	    fontColor="automatic" highlightColor="automatic"
		font="Calibri" fontSize="12.0"
		spaceBefore="0.0" spaceAfter="0.0" />

	  <xsd:complexType name="QuickStyleDef">
		<xsd:attribute name="index" type="xsd:nonNegativeInteger" use="required"/>
		<xsd:attribute name="name" type="xsd:string" use="required"/>
		<xsd:attribute name="fontColor" type="COLOR" default="automatic"/>
		<xsd:attribute name="highlightColor" type="COLOR" default="automatic"/>
		<xsd:attribute name="font" type="xsd:string" use="required"/>
		<xsd:attribute name="fontSize" type="FontSize" use="required"/>
		<xsd:attribute name="bold" type="xsd:boolean" default="false"/>
		<xsd:attribute name="italic" type="xsd:boolean" default="false"/>
		<xsd:attribute name="underline" type="xsd:boolean" default="false"/>
		<xsd:attribute name="strikethrough" type="xsd:boolean" default="false"/>
		<xsd:attribute name="superscript" type="xsd:boolean" default="false"/>
		<xsd:attribute name="subscript" type="xsd:boolean" default="false"/>
		<xsd:attribute name="spaceBefore" type="xsd:float" default="0"/>
		<xsd:attribute name="spaceAfter" type="xsd:float" default="0"/>
	  </xsd:complexType>
	*/

	/// <summary>
	/// Models the QuickStyleDef element used on OneNote pages.
	/// </summary>
	public abstract class XmlStyleBase : StyleBase, IXmlSerializable
	{

		// XmlRootAttribute has a Namespace property but not a Prefix, so these two fields
		// emulate that so we can override it in inherited classes to customize XML output
		protected string Prefix;
		protected string Namespace;


		protected XmlStyleBase() : base()
		{
		}


		protected XmlStyleBase(StyleBase style) : base(style)
		{
		}


		public string ToXml()
		{
			var serializer = new XmlSerializer(this.GetType());
			string xml = null;

			using (var writer = new StringWriter())
			{
				serializer.Serialize(writer, this);
				xml = writer.ToString();
			}

			return xml;
		}


		//========================================================================================
		// IXmlSerializable
		//
		//	Customize serialization so only "set" attributes are serialized,
		//	leaving out unset optional attributes.
		//

		public XmlSchema GetSchema()
		{
			return null;
		}

		public virtual void ReadXml(XmlReader reader)
		{
			if (reader.MoveToAttribute("name") && reader.ReadAttributeValue())
				Name = reader.Value;

			if (reader.MoveToAttribute("styleType") && reader.ReadAttributeValue())
				StyleType = (StyleType)Enum.Parse(typeof(StyleType), reader.Value);

			if (reader.MoveToAttribute("index") && reader.ReadAttributeValue())
				Index = Convert.ToInt32(reader.Value);

			if (reader.MoveToAttribute("fontFamily") && reader.ReadAttributeValue())
				FontSize = reader.Value;

			if (reader.MoveToAttribute("fontSize") && reader.ReadAttributeValue())
				FontSize = reader.Value;

			if (reader.MoveToAttribute("fontColor") && reader.ReadAttributeValue())
				Color = reader.Value;

			if (reader.MoveToAttribute("highlightColor") && reader.ReadAttributeValue())
				Highlight = reader.Value;

			if (reader.MoveToAttribute("bold") && reader.ReadAttributeValue())
				IsBold = Convert.ToBoolean(reader.Value);

			if (reader.MoveToAttribute("italic") && reader.ReadAttributeValue())
				IsItalic = Convert.ToBoolean(reader.Value);

			if (reader.MoveToAttribute("underline") && reader.ReadAttributeValue())
				IsUnderline = Convert.ToBoolean(reader.Value);

			if (reader.MoveToAttribute("strikethrough") && reader.ReadAttributeValue())
				IsStrikethrough = Convert.ToBoolean(reader.Value);

			if (reader.MoveToAttribute("superscript") && reader.ReadAttributeValue())
				IsSuperscript = Convert.ToBoolean(reader.Value);

			if (reader.MoveToAttribute("subscript") && reader.ReadAttributeValue())
				IsSubscript = Convert.ToBoolean(reader.Value);

			if (reader.MoveToAttribute("spaceBefore") && reader.ReadAttributeValue())
				SpaceBefore = reader.Value;

			if (reader.MoveToAttribute("spaceAfter") && reader.ReadAttributeValue())
				SpaceAfter = reader.Value;

			if (reader.MoveToAttribute("spacing") && reader.ReadAttributeValue())
				Spacing = reader.Value;
		}

		public virtual void WriteXml(XmlWriter writer)
		{
			if (!string.IsNullOrEmpty(Namespace))
			{
				var name = string.IsNullOrEmpty(Prefix) ? "xmlns" : $"xmlns:{Prefix}";
				writer.WriteAttributeString(name, Namespace);
			}

			writer.WriteAttributeString("index", Index.ToString());
			writer.WriteAttributeString("name", Name);
			writer.WriteAttributeString("font", FontFamily);

			if (!Color.Equals(Transparent) && !Color.Equals(Automatic))
				writer.WriteAttributeString("fontColor", Color);

			if (!Highlight.Equals(Transparent) && !Highlight.Equals(Automatic))
				writer.WriteAttributeString("highlightColor", Highlight);

			writer.WriteAttributeString("fontSize", FontSize);

			if (IsBold)
				writer.WriteAttributeString("bold", IsBold.ToString().ToLower());

			if (IsItalic)
				writer.WriteAttributeString("italic", IsItalic.ToString().ToLower());

			if (IsUnderline)
				writer.WriteAttributeString("underline", IsUnderline.ToString().ToLower());

			if (IsStrikethrough)
				writer.WriteAttributeString("strikethrough", IsStrikethrough.ToString().ToLower());

			if (IsSuperscript)
				writer.WriteAttributeString("superscript", IsSuperscript.ToString().ToLower());

			if (IsSubscript)
				writer.WriteAttributeString("subscript", IsSubscript.ToString().ToLower());

			writer.WriteAttributeString("spaceBefore", SpaceBefore);
			writer.WriteAttributeString("spaceAfter", SpaceAfter);
			writer.WriteAttributeString("spacing", Spacing);
		}
	}
}
