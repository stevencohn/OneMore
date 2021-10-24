//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System.Xml.Linq;


	/// <summary>
	/// Represents a Tag element
	/// </summary>
	internal class Tag : XElement
	{

		/// <summary>
		/// Instantiates a new meta with the given content and predefined namespace
		/// </summary>
		/// <param name="index">The tag index</param>
		/// <param name="completed">True if the tag should be marked completed</param>
		/// <remarks>
		/// PageNamespace.Value must be set prior to using this constructor
		/// </remarks>
		public Tag(string index, bool completed)
			: this(PageNamespace.Value, index, completed)
		{
		}


		/// <summary>
		/// Initializes a new Tag with the given content and namespace
		/// </summary>
		/// <param name="index">The tag index</param>
		/// <param name="completed">True if the tag should be marked completed</param>
		public Tag(XNamespace ns, string index, bool completed)
			: base(ns + "Tag")
		{
			Add(new XAttribute("index", index));
			Add(new XAttribute("completed", completed ? "true" : "false"));
		}


		/// <summary>
		/// Gets the index of this Tag element
		/// </summary>
		public string TagIndex => Attribute("index").Value;


		/// <summary>
		/// Gets the completed state of this Tag element.
		/// </summary>
		public bool Completed => Attribute("completed").Value == "true";


		public XElement SetEnabled(bool enabled)
		{
			SetAttributeValue("disabled", !enabled);
			return this;
		}
	}
}
