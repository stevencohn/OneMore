//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using River.OneMoreAddIn.Styles;
	using System.Xml.Linq;


	/// <summary>
	/// Used to copy and merge quick style definitions from one page to another, tracking
	/// index changes from page to page.
	/// </summary>
	public class QuickStyleMapping
	{
		/// <summary>
		/// Gets or sets the QuickStyleDef element. The setter is used to deep-clone the
		/// original so the source page is not affected when merging styles
		/// </summary>
		public XElement Element { get; set; }


		/// <summary>
		/// Gets the style definition for this quick style. Consumers that use this to copy
		/// or merge styles to a target page can update the style index for the target page
		/// </summary>
		public QuickStyleDef Style { get; private set; }


		/// <summary>
		/// Gets the index value of this quick style on the source page before it was
		/// mapped to the target page
		/// </summary>
		public string OriginalIndex { get; private set; }


		/// <summary>
		/// Initializes a new instance from the given QuickStyleDef element
		/// </summary>
		/// <param name="element"></param>
		public QuickStyleMapping(XElement element)
		{
			Element = element;
			Style = new QuickStyleDef(element);
			OriginalIndex = Style.Index.ToString();
		}
	}
}
