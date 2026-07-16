//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System.Linq;
	using System.Xml.Linq;


	internal class Paragraph : StylizedElement<Paragraph>
	{
		public Paragraph(string text)
			: this(PageNamespace.Value, text)
		{
		}


		public Paragraph(XNamespace ns, string text)
			: this(ns, new XElement(ns + "T", new XCData(text)))
		{
		}


		public Paragraph(XElement content)
			: this(PageNamespace.Value, content)
		{
		}


		public Paragraph(XNamespace ns, XElement content)
			: base(ns + "OE")
		{
			Add(content);
		}


		public Paragraph(params XObject[] nodes)
			: this(PageNamespace.Value, nodes)
		{
		}


		public Paragraph(XNamespace ns, params XObject[] nodes)
			: base(ns + "OE")
		{
			Add(nodes);
		}


		public Paragraph SetAlignment(string alignment)
		{
			SetAttributeValue("alignment", alignment);
			return this;
		}

		public Paragraph SetMeta(string name, string content)
		{
			var meta = new XElement(PageNamespace.Value + "Meta",
				new XAttribute("name", name),
				new XAttribute("content", content)
				);

			// Meta appears after Tag/MediaIndex per OE schema sequence
			var anchor = Elements(PageNamespace.Value + "Tag").LastOrDefault()
				?? Elements(PageNamespace.Value + "MediaIndex").LastOrDefault();
			if (anchor is null) AddFirst(meta);
			else anchor.AddAfterSelf(meta);

			return this;
		}


		public Paragraph SetQuickStyle(int index)
		{
			SetAttributeValue("quickStyleIndex", index);
			return this;
		}


		public Paragraph SetRTL(bool enable)
		{
			if (enable)
			{
				SetAttributeValue("alignment", "right");
				SetAttributeValue("RTL", "true");
			}
			else
			{
				SetAttributeValue("alignment", "left");
				SetAttributeValue("RTL", "false");
			}
			return this;
		}


		/// <summary>
		/// Applies the given css to this paragraph's own style attribute, merging it into any
		/// existing style or overwriting it entirely.
		/// </summary>
		/// <param name="css">A single "property:value;" pair, or a full style string</param>
		/// <param name="merge">
		/// True to merge css into the existing style, preserving other properties;
		/// false to overwrite the existing style entirely
		/// </param>
		/// <returns></returns>
		protected override Paragraph ApplyStyle(string css, bool merge)
		{
			SetAttributeValue("style",
				merge ? MergeCss(Attribute("style")?.Value, css) : css);

			return this;
		}
	}
}
