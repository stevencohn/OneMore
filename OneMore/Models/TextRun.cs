//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System.Linq;
	using System.Xml.Linq;


	/// <summary>
	/// Wraps the one:T element in a OneNote page, which contains all or some of the text
	/// of a paragraph. Use this to specify raw CDATA text and apply styles to it, such as bold or 
	/// italic.
	/// </summary>
	/// <remarks>
	/// Use this to construct a Paragraph with multiple runs of text, each with its own style. For 
	/// example, you can create a paragraph that contains a run of bold text followed by a run of 
	/// italic text, and then append them together with a soft line break in between.
	/// 
	/// This preserved raw CDATA text, whereas Paragraph(text) escapes any HTML characters in the 
	/// text. If you want to include HTML tags in your text (e.g., <span>), use this instead.
	/// </remarks>
	internal class TextRun : StylizedElement<TextRun>
	{
		public TextRun(string text)
			: this(PageNamespace.Value, text)
		{
		}


		public TextRun(XNamespace ns, string text)
			: this(ns, new XCData(text))
		{
		}


		public TextRun(XNamespace ns, XCData data)
			: base(ns + "T", data)
		{
		}


		/// <summary>
		/// Appends this text run with another text run, inserting a soft line break between them.
		/// </summary>
		/// <param name="run"></param>
		/// <returns></returns>
		public TextRun AppendWithSoftBreak(TextRun run)
		{
			var cdata = this.GetCData();
			var other = run.GetCData();
			cdata.Value = $"{cdata.Value}<br/>{other.Value}";
			return this;
		}


		/// <summary>
		/// Applies the given css property to the run's CDATA span, merging it into any
		/// existing style or overwriting it entirely.
		/// </summary>
		/// <param name="css">A single "property:value;" pair, or a full style string</param>
		/// <param name="merge">
		/// True to merge css into the existing style, preserving other properties;
		/// false to overwrite the existing style entirely
		/// </param>
		/// <returns></returns>
		protected override TextRun ApplyStyle(string css, bool merge)
		{
			var cdata = this.GetCData();
			if (cdata is null)
			{
				return this;
			}

			if (cdata.IsEmpty())
			{
				// OneNote strips span styling from an empty CDATA, so target its
				// parent one:T element instead
				var target = cdata.Parent;
				target.SetAttributeValue("style",
					merge ? MergeCss(target.Attribute("style")?.Value, css) : css);

				return this;
			}

			var wrapper = cdata.GetWrapper();
			var span = GetOrCreateSpan(wrapper);

			span.SetAttributeValue("style",
				merge ? MergeCss(span.Attribute("style")?.Value, css) : css);

			cdata.Value = wrapper.GetInnerXml();
			return this;
		}


		/// <summary>
		/// Finds the single <span> wrapping all of the wrapper's content, creating one
		/// around the existing content if it doesn't already exist.
		/// </summary>
		/// <param name="wrapper">The parsed CDATA wrapper element</param>
		/// <returns>The existing or newly created span element</returns>
		private static XElement GetOrCreateSpan(XElement wrapper)
		{
			var nodes = wrapper.Nodes().ToList();
			if (nodes.Count == 1 && nodes[0] is XElement existing &&
				existing.Name.LocalName == "span")
			{
				return existing;
			}

			foreach (var node in nodes)
			{
				node.Remove();
			}

			var span = new XElement("span");
			span.Add(nodes);
			wrapper.Add(span);
			return span;
		}
	}
}
