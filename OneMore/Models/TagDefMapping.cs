//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System.Xml.Linq;


	/// <summary>
	/// Used to copy and merge TagDefs from one page to another, tracking index changes
	/// from page to page so they can be applied to content when pages are merges
	/// </summary>
	internal class TagDefMapping
	{
		/// <summary>
		/// Gets or sets the TagDef element. The setter is used to deep-clone the
		/// original so the source page is not affected when merging styles
		/// </summary>
		public XElement Element { get; set; }


		/// <summary>
		/// Gets or sets a smart copy of the original TagDef element
		/// </summary>
		public TagDef TagDef { get; set; }


		/// <summary>
		/// Gets the index value of this quick style on the source page before it was
		/// mapped to the target page
		/// </summary>
		public string OriginalIndex { get; private set; }


		/// <summary>
		/// Initializes a new instance from the given TagDef element
		/// </summary>
		/// <param name="element"></param>
		public TagDefMapping(XElement element)
		{
			Element = element;
			TagDef = new TagDef(element);
			OriginalIndex = TagDef.Index;
		}
	}
}
