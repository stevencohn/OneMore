//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System.Xml.Linq;


	/// <summary>
	/// Represents a Meta element
	/// </summary>
	internal class Meta
	{
		private readonly XElement root;


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
		{
			root = new XElement(ns + "Meta",
				new XAttribute("name", name),
				new XAttribute("content", content)
				);
		}


		/// <summary>
		/// Initialize a new meta by wrapping the given root XML
		/// </summary>
		/// <param name="root">An XElement describing a Meta element</param>
		public Meta(XElement root)
		{
			this.root = root;
		}


		/// <summary>
		/// Gets the name of this meta element
		/// </summary>
		public string Name => root.Attribute("name").Value;


		/// <summary>
		/// Gets the value of this meta element
		/// </summary>
		public string Content => root.Attribute("content").Value;


		/// <summary>
		/// Gets the XElement of this meta element
		/// </summary>
		public XElement Root => root;


		/// <summary>
		/// Sets the value of this meta element
		/// </summary>
		/// <param name="content"></param>
		public void SetContent(string content)
		{
			root.SetAttributeValue("content", content);
		}
	}
}
