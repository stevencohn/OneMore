//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System.Linq;
	using System.Xml.Linq;


	/// <summary>
	/// Base class for OneNote page elements that can be stylized with bold, italic, or
	/// arbitrary CSS. Shares the fluent SetBold/SetItalic/SetStyle API and CSS merging logic
	/// between <see cref="TextRun"/> and <see cref="Paragraph"/>, while letting each subclass
	/// decide where its "style" attribute actually lives.
	/// </summary>
	/// <typeparam name="T">The concrete derived type, enabling fluent methods to return it</typeparam>
	internal abstract class StylizedElement<T> : XElement where T : StylizedElement<T>
	{
		protected StylizedElement(XName name)
			: base(name)
		{
		}


		protected StylizedElement(XName name, params object[] content)
			: base(name, content)
		{
		}


		/// <summary>
		/// Sets bold by merging font-weight:bold; into the element's existing style.
		/// </summary>
		/// <returns></returns>
		public T SetBold()
		{
			return ApplyStyle("font-weight:bold;", merge: true);
		}


		/// <summary>
		/// Sets italic by merging font-style:italic; into the element's existing style.
		/// </summary>
		/// <returns></returns>
		public T SetItalic()
		{
			return ApplyStyle("font-style:italic;", merge: true);
		}


		/// <summary>
		/// Sets the element's style by overwriting it with the given css. This will override
		/// any existing style.
		/// </summary>
		/// <param name="css"></param>
		/// <returns></returns>
		public T SetStyle(string css)
		{
			return ApplyStyle(css, merge: false);
		}


		/// <summary>
		/// Applies the given css to this element's style, merging it into any existing style
		/// or overwriting it entirely. Subclasses determine which element actually carries
		/// the "style" attribute.
		/// </summary>
		/// <param name="css">A single "property:value;" pair, or a full style string</param>
		/// <param name="merge">
		/// True to merge css into the existing style, preserving other properties;
		/// false to overwrite the existing style entirely
		/// </param>
		/// <returns></returns>
		protected abstract T ApplyStyle(string css, bool merge);


		/// <summary>
		/// Adds or updates the given css property within an existing style string,
		/// leaving all other properties untouched.
		/// </summary>
		/// <param name="existing">The existing style string, possibly null or empty</param>
		/// <param name="property">A single "property:value;" pair to merge in</param>
		/// <returns>The merged style string</returns>
		protected static string MergeCss(string existing, string property)
		{
			var key = property.Split(':')[0].Trim();

			var properties = (existing ?? string.Empty)
				.Split(';')
				.Select(p => p.Trim())
				.Where(p => p.Length > 0 && !p.StartsWith($"{key}:"))
				.ToList();

			properties.Add(property.TrimEnd(';'));

			return $"{string.Join(";", properties)};";
		}
	}
}
