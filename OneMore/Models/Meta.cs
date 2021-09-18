//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System.Xml.Linq;


	/// <summary>
	/// Represents a Meta element
	/// </summary>
	internal class Meta : XElement
	{

		/// <summary>
		/// Instantiates a new meta with the given content and predefined namespace
		/// </summary>
		/// <remarks>
		/// PageNamespace.Value must be set prior to using this constructor
		/// </remarks>
		public Meta(string name, string content)
			: this(PageNamespace.Value, name, content)
		{
		}


		/// <summary>
		/// Initializes a new meta with the given content and namespace
		/// </summary>
		/// <param name="ns">A namespace</param>
		public Meta(XNamespace ns, string name, string content)
			: base(ns + "Meta")
		{
			Add(new XAttribute("name", name));
			Add(new XAttribute("content", content));
		}


		/// <summary>
		/// Gets the name of this meta element
		/// </summary>
		public string MetaName => Attribute("name").Value;


		/// <summary>
		/// Gets the value of this meta element
		/// </summary>
		public string Content => Attribute("content").Value;


		/// <summary>
		/// Sets the value of this meta element
		/// </summary>
		/// <param name="content"></param>
		public XElement SetContent(string content)
		{
			SetAttributeValue("content", content);
			return this;
		}
	}
}
